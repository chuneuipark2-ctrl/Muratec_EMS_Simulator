using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EMS_TEST_SIMULATOR
{
    public class EMS_Mode_Sequence
    {
        private System.Threading.CancellationTokenSource _blinkToken;

        // [추가] 반송 데이터 No를 관리하기 위한 전역 카운터 (명령 순번)
        // 프로그램 실행 시 1부터 시작하며, 명령이 나갈 때마다 증가합니다.
        private static int _globalCommandCount = 1;

        public void ProcessMode(int mode, Command_Form form, Main mainForm)
        {
            switch (mode)
            {
                case 1: Manual_Sequence(); break;
                case 2: _ = SemiAuto_Sequence(form, mainForm); break;
            }
        }

        public async Task<bool> SemiAuto_Sequence(Command_Form form, Main mainForm)
        {
            var data = form.currentData;
            var proto = form._emsProto;

            // 1. 선행 상태 체크
            var status = proto.Parser.CurrentStatus;
            if (status.MachineMode != "2" || status.CommandAcceptStatus != "1")
            {
                MessageBox.Show($"조건 미충족 (모드:{status.MachineMode}, 접수:{status.CommandAcceptStatus})");
                return false;
            }

            _ = StartBlinking(2);

            try
            {
                for (int i = 0; i < data.loop_command; i++)
                {
                    // STEP 1: 전진 (하강상태 - 엔코더 5000)
                    if (!await RunStep(form, data.End_count.ToString(), "하강")) throw new Exception("전진 실패");

                    // STEP 2: 복귀 (정상상태 - 엔코더 0000)
                    if (!await RunStep(form, data.Start_count.ToString(), "정상")) throw new Exception("복귀 실패");
                }
                MessageBox.Show("시퀀스 완료");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                _blinkToken?.Cancel();
            }
        }

        private async Task<bool> RunStep(Command_Form form, string targetDest, string stateName)
        {
            var proto = form._emsProto;
            var comm = form._comm;

            // 1. 넘버링 규칙 적용: 현재 카운트를 4자리 문자열로 변환 (예: 1 -> "0001")
            string currentDataNo = _globalCommandCount.ToString().PadLeft(4, '0');

            // 2. 다음 명령을 위해 카운트 증가 (9999 초과 시 1로 리셋)
            _globalCommandCount = (_globalCommandCount >= 9999) ? 1 : _globalCommandCount + 1;

            // 하강상태(5000), 정상상태(0000) 구분
            string encoderValue = (stateName == "하강") ? "5000" : "0000";

            var steps = new List<ReturnStepData> {
                new ReturnStepData {
                    SectionNo = targetDest,
                    ActionMode = "0", // 0: 이동
                    EncoderValue = encoderValue
                }
            };

            // [H2] 상세 반송 지시 (자동 생성된 명령 순번 currentDataNo 사용)
            byte[] h2 = proto.EMS_Return_Instruction(currentDataNo, steps);
            if (h2 != null) await comm.SendData(Encoding.ASCII.GetString(h2));

            await Task.Delay(500); // UDP 패킷 간 처리 여유 시간

            // [H4] 작업 지시 (주행 시작)
            byte[] h4 = proto.EMS_Item_order("1");
            if (h4 != null) await comm.SendData(Encoding.ASCII.GetString(h4));

            // 도착 모니터링
            int timeout = 0;
            while (timeout < 600)
            {
                if (int.TryParse(proto.Parser.CurrentStatus.CurrentSectionCount, out int current) &&
                    int.TryParse(targetDest, out int target))
                {
                    // 장비가 보고하는 현재 위치가 목적지에 도달했는지 확인
                    if (current == target) return true;
                }
                await Task.Delay(200);
                timeout++;
            }
            return false;
        }

        public async Task StartBlinking(int boardNo)
        {
            _blinkToken = new System.Threading.CancellationTokenSource();
            try
            {
                while (!_blinkToken.Token.IsCancellationRequested)
                {
                    FASTECH.EziMOTIONPlusELib.FAS_SetOutput(boardNo, 0x4AAA0000, 0xB555FFFF);
                    await Task.Delay(500);
                    FASTECH.EziMOTIONPlusELib.FAS_SetOutput(boardNo, 0x00000000, 0xFFFFFFFF);
                    await Task.Delay(500);
                }
            }
            catch { }
        }

        public void Manual_Sequence() { FASTECH.EziMOTIONPlusELib.FAS_SetOutput(2, 0x10000000, 0xEFFFFFFF); }
        public void Error_Sequence() { FASTECH.EziMOTIONPlusELib.FAS_SetOutput(2, 0x80000000, 0x7FFFFFFF); }
    }
}