using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Windows.Forms;
using System.Collections.Generic;

namespace EMS_TEST_SIMULATOR
{
    public class Rail_DIO
    {
        // 1. 통신 대상 IP 주소 배열 (5대의 장비)
        private string[] ipAddresses = {
            "192.168.0.20", "192.168.0.21", "192.168.0.22", "192.168.0.23", "192.168.0.24"
        };

        // 2. 파스텍 라이브러리에서 사용할 Slave ID (각 IP당 고정 ID 할당)
        private byte[] slaveIds = { 2, 3, 4, 5, 6 };

        // 3. 상태 관리 및 스레드 배열
        private bool[] fastech_connects = new bool[5];  // 각 장비의 연결 성공 여부 플래그
        private Thread[] inputThreads = new Thread[5];   // 입력 감시 스레드 배열
        private Thread[] outputThreads = new Thread[5];  // 출력 감시 스레드 배열

        public static Rail_DIO Instance = new Rail_DIO(); // 싱글톤 인스턴스
        public List<string> ErrorIPs = new List<string>(); // 초기 연결 실패한 IP 기록용

        public bool io_test_flag = false; // 테스트 모드 활성화 플래그 (평상시 제어 차단)

        public bool AreAllFiveSlavesConnected()
        {
            for (int i = 0; i < 5; i++)
            {
                if (!fastech_connects[i]) return false;
            }
            return true;
        }

        /// <summary>5대 중 하나라도 연결된 상태인지(부분 연결 판별용)</summary>
        public bool IsAnySlaveConnected()
        {
            for (int i = 0; i < 5; i++)
            {
                if (fastech_connects[i]) return true;
            }
            return false;
        }

        // 4. 이벤트 핸들러 (UI 갱신을 위해 외부 윈폼에서 구독)
        public delegate void InputUpdateHandler(int index, uint[] status);
        public event InputUpdateHandler OnInputUpdated;
        public delegate void OutputUpdateHandler(int index, uint status);
        public event OutputUpdateHandler OnOutputUpdated;

        private bool isWatchdogRunning = false; // 워치독 중복 실행 방지 변수

        public Rail_DIO() { }

        // 최대 10개의 독립적인 센서 시퀀스를 처리하기 위해 크기를 10으로 확장
        private bool[] isTimerActive = new bool[10];
        private DateTime[] moveSignalStartTimes = new DateTime[10];



        /// <summary>
        /// 모든 장비에 초기 TCP 접속을 시도하는 함수
        /// </summary>
        public bool FASTECH_CONNECT()
        {
            bool successAll = true;
            ErrorIPs.Clear();

            for (int i = 0; i < 5; i++)
            {
                if (fastech_connects[i]) continue; // 이미 연결된 장비는 건너뜀

                string currentIp = ipAddresses[i];
                byte currentId = slaveIds[i];

                // TCP 연결 시도
                bool isConnected = FASTECH.EziMOTIONPlusELib.FAS_ConnectTCP(IPAddress.Parse(currentIp), currentId);

                if (!isConnected)
                {
                    ErrorIPs.Add(currentIp);
                    successAll = false;
                    continue;
                }

                Thread.Sleep(200); // 통신 안정화 대기

                // 보드가 실제 네트워크상에 존재하는지 최종 확인
                if (FASTECH.EziMOTIONPlusELib.FAS_IsSlaveExist(currentId) == 0)
                {
                    FASTECH.EziMOTIONPlusELib.FAS_Close(currentId);
                    fastech_connects[i] = false;
                    ErrorIPs.Add($"{currentIp} (ID:{currentId} 없음)");
                    successAll = false;
                    continue;
                }

                fastech_connects[i] = true;
                StartDeviceThreads(i); // 장비별 데이터 수집 스레드 시작
            }

            StartConnectionWatchdog(); // 재연결 감시 워치독 가동
            return successAll;
        }

        /// <summary>
        /// 개별 장비의 입출력 데이터를 주기적으로 읽을 스레드를 생성하고 시작
        /// </summary>
        private void StartDeviceThreads(int index)
        {
            // 람다 식을 사용하여 현재 루프의 index를 스레드에 고정 전달
            inputThreads[index] = new Thread(() => Func_input_status(index)) { IsBackground = true };
            outputThreads[index] = new Thread(() => Func_output_status(index)) { IsBackground = true };

            inputThreads[index].Start();
            outputThreads[index].Start();
        }

        /// <summary>
        /// 입력(DI) 상태를 무한 루프 돌며 읽어오는 함수
        /// </summary>
        private void Func_input_status(int index)
        {
            uint In_get = 0, In_latch = 0;
            byte targetId = slaveIds[index];
            uint[] DI_STATUS = new uint[16];

            // 중요: 케이블이 빠져서 fastech_connects가 false가 되면 루프 자동 종료
            while (fastech_connects[index])
            {
                // 라이브러리를 통해 입력값 읽기 시도
                if (FASTECH.EziMOTIONPlusELib.FAS_GetInput(targetId, ref In_get, ref In_latch) == FASTECH.EziMOTIONPlusELib.FMM_OK)
                {
                    // 32비트 데이터를 16개 비트 배열로 변환 (0 또는 1)
                    for (int i = 0; i < 16; i++)
                        DI_STATUS[i] = (uint)((In_get & (0x01 << i)) != 0 ? 1 : 0);

                    // 구독중인 UI에 이벤트 발생
                    OnInputUpdated?.Invoke(index, DI_STATUS);
                }
                else
                {
                    // [물리적 단절 감지] 읽기 실패 시 즉시 자원 해제 및 플래그 OFF
                    System.Diagnostics.Debug.WriteLine($"[ERR] ID {targetId} 통신 단절! 스레드 종료.");
                    fastech_connects[index] = false; // 이 플래그가 꺼져야 워치독이 재연결을 시작함
                    FASTECH.EziMOTIONPlusELib.FAS_Close(targetId); // 죽은 소켓 핸들 제거
                    break;
                }
                Thread.Sleep(50); // CPU 과점유 방지
            }
        }

        /// <summary>
        /// 출력(DO) 상태를 무한 루프 돌며 읽어오는 함수
        /// </summary>
        private void Func_output_status(int index)
        {
            byte targetId = slaveIds[index];
            uint Out_get = 0, Out_latch = 0;

            while (fastech_connects[index])
            {
                // 현재 장비의 출력 레지스터 읽기
                if (FASTECH.EziMOTIONPlusELib.FAS_GetOutput(targetId, ref Out_get, ref Out_latch) == FASTECH.EziMOTIONPlusELib.FMM_OK)
                {
                    // 파스텍 구조상 상위 16비트(16~31)가 출력이므로 UI 업데이트 시 비트 계산 주의
                    OnOutputUpdated?.Invoke(index, Out_get);
                }
                else
                {
                    // 통신 단절 감지
                    fastech_connects[index] = false;
                    FASTECH.EziMOTIONPlusELib.FAS_Close(targetId);
                    break;
                }

                // 테스트 모드가 아닐 때의 기본 출력 제어 로직. SR50=주소 3·4번만, SR150=5·6번만
                if (!io_test_flag)
                {
                    bool sr50 = IsSr50Rail();
                    switch (targetId)
                    {
                        case 2:
                            gcp_dio(); // 2번은 레일 타입 무관 (기존 동작)
                            break;

                        case 3:
                            if (sr50)
                            {
                                bit_interlock(index, 1, 0);
                                bit_interlock(index, 2, 8);
                            }
                            break;

                        case 4:
                            if (sr50) bit_interlock(index, 3, 0); // SR50일 때만 4번
                            break;

                        case 5:
                            if (!sr50)
                            {
                                bit_interlock(index, 4, 0);
                                bit_interlock(index, 5, 8);
                            }
                            break;

                        case 6:
                            if (!sr50) bit_interlock(index, 6, 0); // SR150일 때만 6번
                            break;
                    }
                }
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 모든 연결을 종료하고 자원을 반납
        /// </summary>
        public void DISCONNECT()
        {
            isWatchdogRunning = false; // 워치독 먼저 정지
            for (int i = 0; i < 5; i++)
            {
                if (!fastech_connects[i]) continue;
                fastech_connects[i] = false;

                byte targetId = slaveIds[i];
                FASTECH.EziMOTIONPlusELib.FAS_SetOutput(targetId, 0, 0xFFFFFFFF); // 출력 모두 끔
                FASTECH.EziMOTIONPlusELib.FAS_Close(targetId);
            }
            TowerLamp.SetMode(TowerLampVisualMode.IdleRedSteady);
        }

        /// <summary>
        /// 순차적으로 비트를 하나씩 켜보는 테스트 함수 (16번 비트~31번 비트)
        /// </summary>
        public void bit_step_test()
        {
            uint temp_bit = 0x00010000; // 파스텍 출력 시작 비트 (16번)

            for (int j = 0; j < 17; j++)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (fastech_connects[i])
                    {
                        // 해당 비트만 켜고 나머지는 반전시켜서 끔
                        FASTECH.EziMOTIONPlusELib.FAS_SetOutput(slaveIds[i], temp_bit, ~temp_bit);
                    }
                }
                Thread.Sleep(100); // 확인 속도 조정
                if (j == 16)
                {
                    for (int i = 0; i < 5; i++) FASTECH.EziMOTIONPlusELib.FAS_SetOutput(slaveIds[i], 0, 0);
                    break;
                }
                temp_bit <<= 1; // 다음 비트로 이동
            }
            io_test_flag = false;

        }

        /// <summary>
        /// 연결이 끊긴 장비를 찾아 3초마다 재접속을 시도하는 워치독
        /// </summary>
        public void StartConnectionWatchdog()
        {
            if (isWatchdogRunning) return;
            isWatchdogRunning = true;

            Thread watchdog = new Thread(() =>
            {
                while (isWatchdogRunning)
                {
                    Thread.Sleep(3000); // 3초 간격 모니터링

                    for (int i = 0; i < 5; i++)
                    {
                        // 감시 스레드에 의해 끊김(false)으로 판정된 장비 발견 시
                        if (!fastech_connects[i])
                        {
                            System.Diagnostics.Debug.WriteLine($"[Watchdog] {ipAddresses[i]} 재연결 시도 중...");
                            TRY_CLEAN_RECONNECT(i);
                        }
                    }
                }
            })
            { IsBackground = true, Name = "DIO_Watchdog" };

            watchdog.Start();
        }

        /// <summary>
        /// [물리 복구 핵심] 죽은 소켓을 완전히 닫고 처음부터 다시 Connect 시도
        /// </summary>
        private void TRY_CLEAN_RECONNECT(int i)
        {
            byte targetId = slaveIds[i];
            string ip = ipAddresses[i];

            // 1. 기존 핸들이 라이브러리에 남아있으면 재연결이 절대 안 됨. 강제 Close.
            FASTECH.EziMOTIONPlusELib.FAS_Close(targetId);
            Thread.Sleep(300); // OS 소켓 자원 회수 시간 대기

            // 2. 새로운 TCP 연결 시도 (FAS_Reconnect보다 FAS_ConnectTCP가 물리 단절 복구에 훨씬 확실함)
            if (FASTECH.EziMOTIONPlusELib.FAS_ConnectTCP(IPAddress.Parse(ip), targetId))
            {
                // 3. 실제 보드와 응답을 주고받을 수 있는지 최종 검사
                if (FASTECH.EziMOTIONPlusELib.FAS_IsSlaveExist(targetId) != 0)
                {
                    fastech_connects[i] = true; // 플래그를 true로 바꾸어 루프 다시 가동
                    StartDeviceThreads(i);      // 중단되었던 통신 스레드 재생성 및 시작
                    System.Diagnostics.Debug.WriteLine($"[Watchdog] {ip} 물리적 재연결 성공!");
                }
                else
                {
                    FASTECH.EziMOTIONPlusELib.FAS_Close(targetId); // 응답 없으면 다시 해제
                }
            }
        }

        /// <summary>Line_Setup 선택 레일에 따라 RIO 주소: SR50=3·4번만, SR150=5·6번만 사용</summary>
        private static bool IsSr50Rail()
        {
            string t = Line_Setup.SavedRailType ?? "";
            return t.Contains("SR50") || string.IsNullOrEmpty(t);
        }

        /// <summary>AUTO 시퀀스 시 레일 8비트 센서 인터록 충족 여부. SR50=주소 3·4번만, SR150=5·6번만 검사</summary>
        public bool Is8BitSensorInterlockOkForAuto()
        {
            if (io_test_flag) return false;
            uint input_raw = 0, latch = 0;
            bool sr50 = IsSr50Rail();
            if (sr50)
            {
                for (int i = 1; i <= 2; i++)
                {
                    if (!fastech_connects[i]) return false;
                    if (FASTECH.EziMOTIONPlusELib.FAS_GetInput(slaveIds[i], ref input_raw, ref latch) != FASTECH.EziMOTIONPlusELib.FMM_OK) return false;
                }
            }
            else
            {
                for (int i = 3; i <= 4; i++)
                {
                    if (!fastech_connects[i]) return false;
                    if (FASTECH.EziMOTIONPlusELib.FAS_GetInput(slaveIds[i], ref input_raw, ref latch) != FASTECH.EziMOTIONPlusELib.FMM_OK) return false;
                }
            }
            return true;
        }

        public void bit_interlock(int index, int seqIndex, int offset)
        {
            uint input_raw = 0, output_raw = 0, latch = 0;
            byte slave_id = slaveIds[index];

            // [수정] 비트 번호를 직접 지정하여 계산 (offset 적용)
            // Input: 포트 1번(Move) -> offset 0이면 1번, offset 8이면 9번
            uint move_bit_no = (uint)(1 + offset);
            uint move_signal = (uint)0x01 << (int)move_bit_no;

            // Output: 포트 1번(Up), 3번(Down) -> 파스텍은 16번부터 출력 시작
            uint up_bit_no = (uint)(16 + 1 + offset);
            uint down_bit_no = (uint)(16 + 3 + offset);

            uint up_signal = (uint)0x01 << (int)up_bit_no;
            uint down_signal = (uint)0x01 << (int)down_bit_no;

            if (FASTECH.EziMOTIONPlusELib.FAS_GetInput(slave_id, ref input_raw, ref latch) != FASTECH.EziMOTIONPlusELib.FMM_OK) return;

            // --- 시퀀스 제어 ---
            if ((input_raw & move_signal) != 0) // 구동 신호(Move) ON
            {
                if (!isTimerActive[seqIndex])
                {
                    FASTECH.EziMOTIONPlusELib.FAS_GetOutput(slave_id, ref output_raw, ref latch);
                    if ((output_raw & up_signal) == 0) // 상승 신호가 꺼져 있을 때만 타이머 시작
                    {
                        moveSignalStartTimes[seqIndex] = DateTime.Now;
                        isTimerActive[seqIndex] = true;
                    }
                }

                if (isTimerActive[seqIndex])
                {
                    if ((DateTime.Now - moveSignalStartTimes[seqIndex]).TotalSeconds >= 3.0)
                    {
                        // 3초 경과: 상승 ON, 하강 OFF (정확하게 해당 비트만 지정)
                        FASTECH.EziMOTIONPlusELib.FAS_SetOutput(slave_id, up_signal, down_signal);
                        isTimerActive[seqIndex] = false;
                    }
                    else
                    {
                        // 3초 대기 중: 하강 ON, 상승 OFF
                        FASTECH.EziMOTIONPlusELib.FAS_SetOutput(slave_id, down_signal, up_signal);
                    }
                }
                else
                {
                    // 타이머 완료 후 유지: 상승 ON, 하강 OFF
                    FASTECH.EziMOTIONPlusELib.FAS_SetOutput(slave_id, up_signal, down_signal);
                }
            }
            else // 구동 신호(Move) OFF
            {
                // 기본 상태: 하강 ON, 상승 OFF
                FASTECH.EziMOTIONPlusELib.FAS_SetOutput(slave_id, down_signal, up_signal);
                isTimerActive[seqIndex] = false;
            }
        }



        public void gcp_dio()
        {
            TowerLamp.ApplyHardware();
        }




    }
}