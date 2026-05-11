using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms; // Timer 사용을 위해 필요

namespace EMS_TEST_SIMULATOR
{
    // 1. 반송 지시(H2)용 동작 데이터 모델
    public class ReturnStepData
    {
        public string SectionNo { get; set; }      // 반송 섹션 (4바이트)
        public string ActionMode { get; set; }    // 동작 모드 (1바이트)
        public string EncoderValue { get; set; }  // 승강 엔코더값 (4바이트)
    }

    public class OpratingOrder
    {
       public string OrderMode { get; set; }     // 지시모드 (1바이트)
                                                 // 1: 자동
                                                 // 2: 00
                                                 // 3: 00
                                                 // 4: 00
                                                 // 5: 기체 이상 정지
                                                 // 6: 기체 이상 리셋
    }


    // 2. 상태 보고 데이터 모델 (29바이트 규격)
    public class SKY_RAV_Status
    {
        public string ResponseCode { get; set; }        // 1. 응답 코드 (2바이트)
        public string TransferDataNo { get; set; }      // 2. 반송 데이터 No (4바이트)
        public string CurrentSectionCount { get; set; } // 3. 현재 섹션 카운트 (4바이트)
        public string TargetSectionCount { get; set; }  // 4. 목적 섹션 카운트 (4바이트)
        public string TargetActionMode { get; set; }    // 5. 목적 동작 모드 (1바이트)
        public string ErrorSectionCount { get; set; }   // 6. 에러 발생 섹션 (4바이트)
        public string ActionCode { get; set; }          // 7. 동작 코드 (2바이트)
        public string ErrorCode { get; set; }           // 8. 에러 코드 (2바이트)
        public string MachineMode { get; set; }         // 9. 기체 모드 상태 (1:수동/2:자동)
        public string CargoStatus { get; set; }         // 10. 화물 탑재 상태 (0:없음/1:있음)
        public string CommandAcceptStatus { get; set; } // 11. 반송 지령 접수 상태 (0:불가/1:가능)
    }

    public class EMS_Protocol
    {
        private const string Host_Header = "RXT240"; // 시스템 헤더
        private const string Host_StationID = "01";   // 장비 ID
        private const string Host_Tail = "\r\n";     // 종료 문자

        private RAV_Packet_Parser _parser = new RAV_Packet_Parser();
        private int msg_seq = 0;

        private System.Windows.Forms.Timer _statusTimer;
        public IDeviceComm _comm; // 인터페이스 IDeviceComm 사용

        public RAV_Packet_Parser Parser => _parser;

        // [자동 상태 문의 시작]
        public void StartStatusPolling(IDeviceComm comm)
        {
            this._comm = comm;
            if (_statusTimer == null)
            {
                _statusTimer = new System.Windows.Forms.Timer();
                _statusTimer.Interval = 300;
                // [수정] 오타 수정: _statusTimer로 통일
                _statusTimer.Tick += async (s, e) =>
                {
                    if (_comm != null)
                    {
                        // [수정] H1 패킷 생성 후 문자열로 변환하여 전송
                        byte[] h1packet = EMS_Link_check_and_Inquire();
                        await _comm.SendData(Encoding.ASCII.GetString(h1packet));
                    }
                };
            }
            _statusTimer.Start();
        }

        // [자동 상태 문의 중지]
        public void StopStatusPolling()
        {
            _statusTimer?.Stop();
        }

        // [F1] 데이터 링크 확립 요구
        public byte[] EMS_Link_Connect()
        {
            return Host_Packet(GetNextSeq(), "F1", "", false);
        }

        // [H1] 상태 문의
        public byte[] EMS_Link_check_and_Inquire()
        {
            return Host_Packet(GetNextSeq(), "H1", "0", true);
        }

        // [H2] 상세 반송 지시
        public byte[] EMS_Return_Instruction(string dataNo, List<ReturnStepData> steps)
        {
            StringBuilder content = new StringBuilder();
            content.Append(dataNo.PadLeft(4, '0'));

            foreach (var step in steps)
            {
                content.Append(step.SectionNo.PadLeft(4, '0'));
                content.Append(step.ActionMode.PadLeft(1, '0'));
                content.Append("0");
                content.Append(step.EncoderValue.PadLeft(4, '0'));
                content.Append("0000");
            }
            return Host_Packet(GetNextSeq(), "H2", content.ToString(), true);
        }

        // [H3] 반송데이터 클리어 지시
        public byte[] EMS_TransferDataClear()
        {
            return Host_Packet(GetNextSeq(), "H3", "", true);
        }

        // [H4] 작업 지시
        public byte[] EMS_Item_order(string instructionMode)
        {
            return Host_Packet(GetNextSeq(), "H4", instructionMode, true);//작업번호, 작업명령(h4면 동작지시임), 지시모드, 체크섬 넘겨줌
        }

        // 메시지 순번 생성 (000~999)
        private string GetNextSeq() 
        {
            string seq = (msg_seq % 1000).ToString("D3");
            msg_seq++;
            return seq;
        }

        // 패킷 조립 및 체크섬 계산
        public byte[] Host_Packet(string Host_sequence, string Host_msgId, string Host_content, bool useChecksum)
        {
            string dataBody = $"{Host_Header}{Host_StationID}{Host_sequence}{Host_msgId}{Host_content}";
            string fullPacket = useChecksum ? $"{dataBody}{CalculateSumCheck(dataBody)}{Host_Tail}" : $"{dataBody}{Host_Tail}";
            return Encoding.ASCII.GetBytes(fullPacket);
        }

        private string CalculateSumCheck(string data)
        {
            int sum = 0;
            foreach (char c in data) sum += (int)c;
            return (sum % 256).ToString("X2");
        }

        public void ReceiveFromDevice(byte[] data) => _parser.ParseResponse(data);

        // [내장 파서 클래스]
        public class RAV_Packet_Parser
        {
            public SKY_RAV_Status CurrentStatus { get; private set; } = new SKY_RAV_Status();
            public bool IsLinkEstablished { get; private set; } = false;

            public void ParseResponse(byte[] receivedData)
            {
                try
                {
                    string res = Encoding.ASCII.GetString(receivedData);
                    if (res.Contains("@TXT240"))
                    {
                        int startIndex = res.IndexOf("@TXT240") + 12;
                        string body = res.Substring(startIndex);

                        if (body.Length >= 26)
                        {
                            CurrentStatus.ResponseCode = body.Substring(0, 2);
                            CurrentStatus.TransferDataNo = body.Substring(2, 4);
                            CurrentStatus.CurrentSectionCount = body.Substring(6, 4);
                            CurrentStatus.TargetSectionCount = body.Substring(10, 4);
                            CurrentStatus.TargetActionMode = body.Substring(14, 1);
                            CurrentStatus.ErrorSectionCount = body.Substring(15, 4);
                            CurrentStatus.ActionCode = body.Substring(19, 2);
                            CurrentStatus.ErrorCode = body.Substring(21, 2);
                            CurrentStatus.MachineMode = body.Substring(23, 1);
                            CurrentStatus.CargoStatus = body.Substring(24, 1);
                            CurrentStatus.CommandAcceptStatus = body.Substring(25, 1);
                            

                            if (CurrentStatus.ResponseCode == "00") IsLinkEstablished = true;
                        }
                    }
                }
                catch { }
            }
        }
    }
}