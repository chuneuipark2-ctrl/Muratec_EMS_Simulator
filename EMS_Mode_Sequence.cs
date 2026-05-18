using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Text;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EMS_TEST_SIMULATOR
{
    public class EMS_Mode_Sequence : Main
    {
        private System.Threading.CancellationTokenSource _blinkToken;

        // 반송 데이터 No 관리 전역 카운터 (0~9999 무한 반복, 데이터로그용 내부 메모리)
        private static int _globalCommandCount = 0;

        // 엔코더 설정 매니저 객체 (부모 Main._encManager 숨김)
        public new Encoder_Setting_Manager _encManager = new Encoder_Setting_Manager();

        //시작, 종료위치 저장변수
        private string Start_position;
        private string End_position;
        private string Current_position;

        public EMS_Protocol EMS_Current_status = new EMS_Protocol(); //현재 위치값 확인용
        public EMS_Protocol EMS_Order =new EMS_Protocol(); // 명령주는용
        public string Order_no; // 반송데이터 작업번호

        private const int EmsPollIntervalMs = 200;
        private const int EmsTimeoutPollCount = 50;
        private const int EmsTimeoutSeconds = 10;
        private const string SemiOriginSection101 = "0101";

        /// <summary>
        /// 도그 카운트(101~113 등)를 프로토콜용 4자리 문자열로 변환.
        /// 앞 2자리 = 섹션번호(101→01), 뒤 2자리 = 도그 순번(101→01, 102→02, ..., 113→13).
        /// </summary>
        public static string DogCountToSectionNumber(int dogCount)
        {
            int section = dogCount / 100;   // 101→1, 113→1, 201→2
            int seq = dogCount % 100;       // 101→1, 102→2, 113→13
            return section.ToString("D2") + seq.ToString("D2");  // "0101", "0102", ..., "0113"
        }

        private static string GetVehicleIDFromForm(Command_Form form)
        {
            string emsNo = form.currentData.EMS_NO ?? "";
            string numPart = System.Text.RegularExpressions.Regex.Match(emsNo, @"\d+").Value;
            return int.TryParse(numPart, out int n) ? n.ToString("D2") : "01";
        }

        private string GetEncoderValueForSection(Command_Form form, string section4Digit)
        {
            string vehicleID = GetVehicleIDFromForm(form);
            string posKey = int.TryParse(section4Digit, out int sec) ? sec.ToString() : "";
            if (string.IsNullOrEmpty(posKey)) return null;
            int v = _encManager.GetStoredValue(vehicleID, posKey);
            if (v >= 0 && v <= 500) return v.ToString().PadLeft(4, '0');
            return null;
        }

        private static void BindSemiAutoCommAndProto(Command_Form form, Main mainForm)
        {
            if (mainForm?.GlobalComm != null)
                form._comm = mainForm.GlobalComm;
            if (mainForm?.GlobalEmsProto != null)
                form._emsProto = mainForm.GlobalEmsProto;
        }

        private static string GetCurrentSection4(EMS_Protocol proto)
        {
            return proto?.Parser?.CurrentStatus?.CurrentSectionCount?.Trim() ?? "";
        }

        private static bool IsEmsRunStatusOk(SKY_RAV_Status status)
        {
            return status != null
                && status.ResponseCode == "00"
                && status.MachineMode == "2"
                && status.CommandAcceptStatus == "1";
        }

        private static bool SectionMatches(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
            if (string.Equals(a, b, StringComparison.Ordinal)) return true;
            if (int.TryParse(a, out int ia) && int.TryParse(b, out int ib)) return ia == ib;
            return false;
        }

        private async Task<bool> SendH4AutoAndWaitAcceptAsync(Command_Form form, EMS_Protocol proto, CancellationToken cancelToken)
        {
            if (form?._comm == null) return false;
            if (!IsEmsRunStatusOk(proto?.Parser?.CurrentStatus))
                return false;

            byte[] h4 = EMS_Order.EMS_Item_order("1");
            if (h4 == null) return false;
            await form._comm.SendData(Encoding.ASCII.GetString(h4));
            return await WaitForTransferCommandAcceptAfterH4Async(proto, cancelToken);
        }

        /// <summary>H4 후 CurrentSectionCount 갱신 = 원점 잡힘. EMS는 원점 전 H2 불가.</summary>
        private async Task<bool> WaitForOriginHomingAsync(Command_Form form, EMS_Protocol proto, Main mainForm, CancellationToken cancelToken)
        {
            string initialSection = GetCurrentSection4(proto);
            for (int i = 0; i < EmsTimeoutPollCount; i++)
            {
                if (cancelToken.IsCancellationRequested) return false;
                await Task.Delay(EmsPollIntervalMs, cancelToken);

                var status = proto?.Parser?.CurrentStatus;
                if (!IsEmsRunStatusOk(status))
                {
                    await SendH3ClearAsync(form);
                    if (!mainForm.IsDisposed)
                        mainForm.Invoke(new Action(() =>
                            MessageBox.Show($"EMS 상태 이상 (응답:{status?.ResponseCode}, 모드:{status?.MachineMode}, 접수:{status?.CommandAcceptStatus})",
                                "반자동", MessageBoxButtons.OK, MessageBoxIcon.Warning)));
                    return false;
                }

                string current = GetCurrentSection4(proto);
                if (!string.IsNullOrEmpty(current) && current != initialSection)
                    return true;
            }

            await SendH3ClearAsync(form);
            if (!mainForm.IsDisposed)
                mainForm.Invoke(new Action(() =>
                    MessageBox.Show($"원점(현재위치 갱신) 대기 시간 초과 ({EmsTimeoutSeconds}초).", "반자동",
                        MessageBoxButtons.OK, MessageBoxIcon.Error)));
            return false;
        }

        private async Task<bool> WaitSectionArrivalAsync(EMS_Protocol proto, string section4, CancellationToken cancelToken)
        {
            for (int i = 0; i < EmsTimeoutPollCount; i++)
            {
                if (cancelToken.IsCancellationRequested) return false;
                var status = proto?.Parser?.CurrentStatus;
                if (!IsEmsRunStatusOk(status)) return false;
                if (SectionMatches(GetCurrentSection4(proto), section4))
                    return true;
                await Task.Delay(EmsPollIntervalMs, cancelToken);
            }
            return false;
        }

        /// <summary>H4·원점 확보 후 H2 1회 전송 및 도착 대기(이동/탑재/이재 공통).</summary>
        private async Task<bool> SendH2LegAsync(Command_Form form, EMS_Protocol proto, Main mainForm, string section4,
            string actionMode, string encoderValue, CancellationToken cancelToken)
        {
            if (form?._comm == null) return false;
            if (!IsEmsRunStatusOk(proto?.Parser?.CurrentStatus))
            {
                if (!mainForm.IsDisposed)
                    mainForm.Invoke(new Action(() =>
                        MessageBox.Show("H2 전송 전 EMS 상태가 정상(00·자동·접수가능)이 아닙니다.", "반자동",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)));
                return false;
            }

            Order_no = _globalCommandCount.ToString("D4");
            _globalCommandCount = (_globalCommandCount + 1) % 10000;
            byte[] h2 = EMS_Order.EMS_Return_Instruction(Order_no, new List<ReturnStepData>
            {
                new ReturnStepData
                {
                    SectionNo = section4,
                    ActionMode = actionMode,
                    EncoderValue = encoderValue ?? "0000"
                }
            });
            if (h2 == null) return false;
            await form._comm.SendData(Encoding.ASCII.GetString(h2));
            await Task.Delay(500, cancelToken);

            if (await WaitSectionArrivalAsync(proto, section4, cancelToken))
                return true;

            if (!mainForm.IsDisposed)
                mainForm.Invoke(new Action(() =>
                    MessageBox.Show($"목적 위치({section4}) 도착 대기 시간 초과 ({EmsTimeoutSeconds}초).", "반자동",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning)));
            return false;
        }

        /// <summary>반자동 공통: H4 → 원점(현재위치 갱신) → H2로 101 정렬.</summary>
        private async Task<bool> RunSemiAutoHomingAndAlign101Async(Command_Form form, EMS_Protocol proto, Main mainForm,
            CancellationToken cancelToken)
        {
            if (!await SendH4AutoAndWaitAcceptAsync(form, proto, cancelToken))
            {
                await SendH3ClearAsync(form);
                if (!mainForm.IsDisposed)
                    mainForm.Invoke(new Action(() =>
                        MessageBox.Show($"H4 후 반송지령 접수(1) 대기 시간 초과 ({EmsTimeoutSeconds}초).", "반자동",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)));
                return false;
            }

            if (!await WaitForOriginHomingAsync(form, proto, mainForm, cancelToken))
                return false;

            if (SectionMatches(GetCurrentSection4(proto), SemiOriginSection101))
                return true;

            return await SendH2LegAsync(form, proto, mainForm, SemiOriginSection101, "0", "0000", cancelToken);
        }

        public void ProcessMode(int mode, Command_Form form, Main mainForm)
        {
            mode = form.currentData.command_alloc;


            switch (mode)
            {
                case 1: Manual_Sequence(); break;
                case 2: _ = SemiAuto_Sequence(form, mainForm); break;
                case 3: _ = Auto_sequence(form, mainForm, System.Threading.CancellationToken.None); break;
            }
        }


        /// <summary>반자동: H4 → 원점(현재위치 갱신) → H2(101) → Command Form 경로. 탑재/이재는 화물·엔코더 조건 추가.</summary>
        public async Task<bool> SemiAuto_Sequence(Command_Form form, Main mainForm)
        {
            BindSemiAutoCommAndProto(form, mainForm);
            var proto = form._emsProto;
            int mode = form.currentData.command_alloc;

            if (form._comm == null)
            {
                MessageBox.Show("통신이 연결되지 않았습니다.", "반자동", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var status = proto?.Parser?.CurrentStatus;
            if (status == null || status.ResponseCode == null || status.MachineMode == null)
            {
                MessageBox.Show("EMS로부터 응답이 없습니다.", "반자동", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!IsEmsRunStatusOk(status))
            {
                MessageBox.Show($"조건 미충족 (응답:{status.ResponseCode}, 모드:{status.MachineMode}, 접수:{status.CommandAcceptStatus})",
                    "반자동", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(Line_Setup.SavedVehicleNo))
            {
                MessageBox.Show("저장된 호기가 없습니다. Line_Setup에서 호기를 선택한 뒤 [상태저장]을 눌러 주세요.");
                return false;
            }
            if (form.currentData.EMS_NO != Line_Setup.SavedVehicleNo)
            {
                MessageBox.Show("Line_Setup에 저장된 호기와 반자동 명령의 호기가 일치하지 않습니다.");
                return false;
            }

            _ = StartBlinking(2);
            var cancelToken = CancellationToken.None;

            try
            {
                bool ok;
                switch (mode)
                {
                    case 1:
                        ok = await SemiAuto_RunMoveAsync(form, proto, mainForm, cancelToken);
                        if (ok) MessageBox.Show("이동 시퀀스 완료", "반자동", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return ok;
                    case 2:
                        ok = await SemiAuto_RunLoadAsync(form, proto, mainForm, cancelToken);
                        if (ok) MessageBox.Show("탑재 시퀀스 완료", "반자동", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return ok;
                    case 3:
                        ok = await SemiAuto_RunUnloadAsync(form, proto, mainForm, cancelToken);
                        if (ok) MessageBox.Show("이재 시퀀스 완료", "반자동", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return ok;
                    default:
                        MessageBox.Show("지원하지 않는 반자동 명령입니다.", "반자동", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "반자동", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                _blinkToken?.Cancel();
            }
        }

        private async Task<bool> SemiAuto_RunMoveAsync(Command_Form form, EMS_Protocol proto, Main mainForm, CancellationToken cancelToken)
        {
            Start_position = DogCountToSectionNumber(form.currentData.Start_count);
            End_position = DogCountToSectionNumber(form.currentData.End_count);
            Current_position = GetCurrentSection4(proto);

            if (Start_position == End_position)
            {
                MessageBox.Show("시작위치와 목적위치가 같습니다.", "반자동", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (SectionMatches(Current_position, End_position))
            {
                MessageBox.Show("현재 위치가 목적위치와 같습니다. 이동할 수 없습니다.", "반자동", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!await RunSemiAutoHomingAndAlign101Async(form, proto, mainForm, cancelToken))
                return false;

            if (!SectionMatches(Start_position, SemiOriginSection101))
            {
                if (!await SendH2LegAsync(form, proto, mainForm, Start_position, "0", "0000", cancelToken))
                    return false;
            }

            if (!SectionMatches(GetCurrentSection4(proto), End_position))
            {
                if (!await SendH2LegAsync(form, proto, mainForm, End_position, "0", "0000", cancelToken))
                    return false;
            }

            return true;
        }

        private async Task<bool> SemiAuto_RunLoadAsync(Command_Form form, EMS_Protocol proto, Main mainForm, CancellationToken cancelToken)
        {
            Start_position = DogCountToSectionNumber(form.currentData.Start_count);
            End_position = DogCountToSectionNumber(form.currentData.End_count);
            Current_position = GetCurrentSection4(proto);

            string encEnd = GetEncoderValueForSection(form, End_position);
            if (encEnd == null)
            {
                MessageBox.Show("엔코더 설정에 해당 목적위치 값이 존재하지 않습니다.", "엔코더 값 없음", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (Start_position == End_position)
            {
                MessageBox.Show("시작위치와 목적위치가 같습니다.", "반자동", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (SectionMatches(Current_position, End_position))
            {
                MessageBox.Show("현재 위치가 목적위치와 같습니다.", "반자동", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var status = proto.Parser.CurrentStatus;
            if (status.CargoStatus == "1")
            {
                MessageBox.Show("이미 화물이 탑재되어 있습니다.", "탑재", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!await RunSemiAutoHomingAndAlign101Async(form, proto, mainForm, cancelToken))
                return false;

            if (!SectionMatches(Start_position, SemiOriginSection101))
            {
                if (!await SendH2LegAsync(form, proto, mainForm, Start_position, "0", "0000", cancelToken))
                    return false;
            }

            status = proto.Parser.CurrentStatus;
            if (status.CargoStatus == "1")
            {
                MessageBox.Show("이미 화물이 탑재되어 있습니다.", "탑재", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return await SendH2LegAsync(form, proto, mainForm, End_position, "1", encEnd, cancelToken);
        }

        private async Task<bool> SemiAuto_RunUnloadAsync(Command_Form form, EMS_Protocol proto, Main mainForm, CancellationToken cancelToken)
        {
            Start_position = DogCountToSectionNumber(form.currentData.Start_count);
            End_position = DogCountToSectionNumber(form.currentData.End_count);
            Current_position = GetCurrentSection4(proto);

            string encEnd = GetEncoderValueForSection(form, End_position);
            if (encEnd == null)
            {
                MessageBox.Show("엔코더 설정에 해당 목적위치 값이 존재하지 않습니다.", "엔코더 값 없음", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (Start_position == End_position)
            {
                MessageBox.Show("시작위치와 목적위치가 같습니다.", "반자동", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (SectionMatches(Current_position, End_position))
            {
                MessageBox.Show("현재 위치가 목적위치와 같습니다.", "반자동", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var status = proto.Parser.CurrentStatus;
            if (status.CargoStatus == "0")
            {
                MessageBox.Show("화물이 없습니다. 이재 명령을 취소합니다.", "이재", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!await RunSemiAutoHomingAndAlign101Async(form, proto, mainForm, cancelToken))
                return false;

            if (!SectionMatches(Start_position, SemiOriginSection101))
            {
                if (!await SendH2LegAsync(form, proto, mainForm, Start_position, "0", "0000", cancelToken))
                    return false;
            }

            status = proto.Parser.CurrentStatus;
            if (status.CargoStatus == "0")
            {
                MessageBox.Show("화물이 없습니다. 이재 명령을 취소합니다.", "이재", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return await SendH2LegAsync(form, proto, mainForm, End_position, "2", encEnd, cancelToken);
        }

        private const string AUTO_SECTION_101 = "0101";
        private const string AUTO_SECTION_110 = "0110";
        private const string AUTO_SECTION_113 = "0113";

        /// <summary>true로 설정 시 현재 사이클 끝난 뒤 101번으로 복귀하여 타이어 이재 후 정지</summary>
        public static bool CycleStopRequested { get; set; }

        /// <summary>AUTO 시퀀스: 스텝마다 H2만 전송 (H4는 시퀀스 시작 시 1회만)</summary>
        private async Task<bool> SendAndWaitArrival(Command_Form form, string section4, string actionMode, string encoder, System.Threading.CancellationToken cancelToken)
        {
            var proto = form._emsProto;
            var comm = form._comm;
            Order_no = _globalCommandCount.ToString("D4");
            _globalCommandCount = (_globalCommandCount + 1) % 10000;
            byte[] h2 = EMS_Order.EMS_Return_Instruction(Order_no, new List<ReturnStepData> {
                new ReturnStepData { SectionNo = section4, ActionMode = actionMode, EncoderValue = encoder }
            });
            if (h2 != null) await comm.SendData(Encoding.ASCII.GetString(h2));
            await Task.Delay(500, cancelToken);
            int targetVal = int.TryParse(section4, out int t) ? t : 0;
            for (int i = 0; i < EmsTimeoutPollCount; i++)
            {
                if (cancelToken.IsCancellationRequested) return false;
                if (!IsEmsRunStatusOk(proto?.Parser?.CurrentStatus)) return false;
                if (int.TryParse(proto.Parser.CurrentStatus.CurrentSectionCount, out int current) && current == targetVal)
                    return true;
                await Task.Delay(EmsPollIntervalMs, cancelToken);
            }
            return false;
        }

        private async Task<bool> ReturnTo101AndUnload(Command_Form form, string enc101, System.Threading.CancellationToken cancelToken)
        {
            return await SendAndWaitArrival(form, AUTO_SECTION_101, "2", enc101, cancelToken);
        }

        /// <summary>모든 사이클 종료 시 H4 수동(0) 전송</summary>
        private async Task SendH4ManualAsync(Command_Form form)
        {
            if (form?._comm == null) return;
            byte[] h4 = EMS_Order.EMS_Item_order("0"); // 0: 수동
            if (h4 != null) await form._comm.SendData(Encoding.ASCII.GetString(h4));
        }

        /// <summary>H3 반송데이터 클리어 지시 전송 (2초 위치 미변화/타임아웃 시 호출)</summary>
        private async Task SendH3ClearAsync(Command_Form form)
        {
            if (form?._comm == null) return;
            byte[] h3 = EMS_Order.EMS_TransferDataClear();
            if (h3 != null) await form._comm.SendData(Encoding.ASCII.GetString(h3));
        }

        /// <summary>
        /// H4 전송 후 H1 상태 문의로 갱신된 보고에서 반송지령 접수상태가 1(및 응답 00)일 때까지 대기한다. 타이머 고정 지연 대신 사용.
        /// </summary>
        private async Task<bool> WaitForTransferCommandAcceptAfterH4Async(EMS_Protocol proto, CancellationToken cancelToken)
        {
            for (int i = 0; i < EmsTimeoutPollCount; i++)
            {
                if (cancelToken.IsCancellationRequested) return false;
                if (IsEmsRunStatusOk(proto?.Parser?.CurrentStatus))
                    return true;
                await Task.Delay(EmsPollIntervalMs, cancelToken);
            }
            return false;
        }

        private async Task Auto_sequence(Command_Form form, Main mainForm, System.Threading.CancellationToken cancelToken)
        {
            try
            {
                TowerLamp.SetMode(TowerLampVisualMode.AutoAfterConfirmBlueBlink);
                var proto = form._emsProto;
                var comm = form._comm;
                string enc101 = GetEncoderValueForSection(form, AUTO_SECTION_101) ?? "0000";
                string enc110 = GetEncoderValueForSection(form, AUTO_SECTION_110) ?? "0000";
                string enc113 = GetEncoderValueForSection(form, AUTO_SECTION_113) ?? "0000";

                // AUTO 시작 직후 2초 안에 (유효한) 현재 위치값이 변하지 않으면 EMS 동작이상 에러
                // null과 "" 구분 시 null != "" 로 바로 break되는 것 방지: 유효한 값이 들어와서 달라질 때만 break
                string initialSection = GetCurrentSection4(proto);
                for (int i = 0; i < EmsTimeoutPollCount; i++)
                {
                    await Task.Delay(EmsPollIntervalMs, cancelToken);
                    if (cancelToken.IsCancellationRequested) return;
                    if (!IsEmsRunStatusOk(proto?.Parser?.CurrentStatus))
                    {
                        await SendH3ClearAsync(form);
                        if (!mainForm.IsDisposed)
                            mainForm.Invoke(new Action(() =>
                            {
                                TowerLamp.SetMode(TowerLampVisualMode.EmsFaultRedSteady);
                                MessageBox.Show("EMS 상태 이상", "AUTO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }));
                        return;
                    }
                    string current = GetCurrentSection4(proto);
                    if (!string.IsNullOrEmpty(current) && current != initialSection)
                        break;
                    if (i == EmsTimeoutPollCount - 1)
                    {
                        await SendH3ClearAsync(form);
                        if (!mainForm.IsDisposed)
                            mainForm.Invoke(new Action(() =>
                            {
                                TowerLamp.SetMode(TowerLampVisualMode.EmsFaultRedSteady);
                                MessageBox.Show($"EMS 동작이상/원점 대기 시간 초과 ({EmsTimeoutSeconds}초).", "AUTO",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }));
                        return;
                    }
                }

                // AUTO 시 레일 8비트 센서 인터록: 기본은 충족까지 대기. Main 제목 비밀(1) 통과 시 생략 후 1초만 대기.
                Rail_DIO.Instance.io_test_flag = false;
                if (mainForm != null && mainForm.BypassRailInterlockForAuto)
                {
                    await Task.Delay(1000, cancelToken);
                    if (cancelToken.IsCancellationRequested) return;
                }
                else
                {
                    while (!Rail_DIO.Instance.Is8BitSensorInterlockOkForAuto())
                    {
                        await Task.Delay(200, cancelToken);
                        if (cancelToken.IsCancellationRequested) return;
                    }
                }

                // AUTO 최초 1회만 H4 자동(1) 전송, 이후 스텝은 H2만 전송
                byte[] h4Auto = EMS_Order.EMS_Item_order("1");
                if (h4Auto != null) await comm.SendData(Encoding.ASCII.GetString(h4Auto));
                TowerLamp.SetMode(TowerLampVisualMode.AutoH4WaitGreenBlink);
                if (!await WaitForTransferCommandAcceptAfterH4Async(proto, cancelToken))
                {
                    await SendH3ClearAsync(form);
                    if (!mainForm.IsDisposed)
                        mainForm.Invoke(new Action(() =>
                        {
                            TowerLamp.SetMode(TowerLampVisualMode.EmsFaultRedSteady);
                            MessageBox.Show("H4 후 반송지령 접수(1) 대기 시간이 초과되었습니다.", "AUTO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }));
                    return;
                }

                TowerLamp.SetMode(TowerLampVisualMode.AutoH2RunGreenBlink);
                bool firstLoop = true;
                while (!cancelToken.IsCancellationRequested)
                {
                    if (CycleStopRequested)
                    {
                        TowerLamp.SetMode(TowerLampVisualMode.AutoCycleStopYellowBlink);
                        bool arrived = await ReturnTo101AndUnload(form, enc101, cancelToken);
                        CycleStopRequested = false;
                        if (!arrived)
                        {
                            await SendH3ClearAsync(form);
                            if (!mainForm.IsDisposed)
                                mainForm.Invoke(new Action(() => MessageBox.Show("사이클 정지 실패. 원점으로 이동하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning)));
                        }
                        await SendH4ManualAsync(form);
                        return;
                    }
                    if (firstLoop)
                    {
                        if (await SendAndWaitArrival(form, AUTO_SECTION_101, "1", enc101, cancelToken) == false) { await SendH3ClearAsync(form); if (!cancelToken.IsCancellationRequested) TowerLamp.SetMode(TowerLampVisualMode.EmsFaultRedSteady); break; }
                        if (CycleStopRequested) { TowerLamp.SetMode(TowerLampVisualMode.AutoCycleStopYellowBlink); bool a = await ReturnTo101AndUnload(form, enc101, cancelToken); CycleStopRequested = false; if (!a && !mainForm.IsDisposed) mainForm.Invoke(new Action(() => MessageBox.Show("사이클 정지 실패. 원점으로 이동하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning))); await SendH4ManualAsync(form); return; }
                        if (await SendAndWaitArrival(form, AUTO_SECTION_110, "2", enc110, cancelToken) == false) { await SendH3ClearAsync(form); if (!cancelToken.IsCancellationRequested) TowerLamp.SetMode(TowerLampVisualMode.EmsFaultRedSteady); break; }
                        if (CycleStopRequested) { TowerLamp.SetMode(TowerLampVisualMode.AutoCycleStopYellowBlink); bool a = await ReturnTo101AndUnload(form, enc101, cancelToken); CycleStopRequested = false; if (!a && !mainForm.IsDisposed) mainForm.Invoke(new Action(() => MessageBox.Show("사이클 정지 실패. 원점으로 이동하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning))); await SendH4ManualAsync(form); return; }
                        if (await SendAndWaitArrival(form, AUTO_SECTION_110, "1", enc110, cancelToken) == false) { await SendH3ClearAsync(form); if (!cancelToken.IsCancellationRequested) TowerLamp.SetMode(TowerLampVisualMode.EmsFaultRedSteady); break; }
                        if (CycleStopRequested) { TowerLamp.SetMode(TowerLampVisualMode.AutoCycleStopYellowBlink); bool a = await ReturnTo101AndUnload(form, enc101, cancelToken); CycleStopRequested = false; if (!a && !mainForm.IsDisposed) mainForm.Invoke(new Action(() => MessageBox.Show("사이클 정지 실패. 원점으로 이동하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning))); await SendH4ManualAsync(form); return; }
                        if (await SendAndWaitArrival(form, AUTO_SECTION_113, "2", enc113, cancelToken) == false) { await SendH3ClearAsync(form); if (!cancelToken.IsCancellationRequested) TowerLamp.SetMode(TowerLampVisualMode.EmsFaultRedSteady); break; }
                        firstLoop = false;
                    }
                    else
                    {
                        if (await SendAndWaitArrival(form, AUTO_SECTION_113, "1", enc113, cancelToken) == false) { await SendH3ClearAsync(form); if (!cancelToken.IsCancellationRequested) TowerLamp.SetMode(TowerLampVisualMode.EmsFaultRedSteady); break; }
                        if (CycleStopRequested) { TowerLamp.SetMode(TowerLampVisualMode.AutoCycleStopYellowBlink); bool a = await ReturnTo101AndUnload(form, enc101, cancelToken); CycleStopRequested = false; if (!a && !mainForm.IsDisposed) mainForm.Invoke(new Action(() => MessageBox.Show("사이클 정지 실패. 원점으로 이동하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning))); await SendH4ManualAsync(form); return; }
                        if (await SendAndWaitArrival(form, AUTO_SECTION_101, "2", enc101, cancelToken) == false) { await SendH3ClearAsync(form); if (!cancelToken.IsCancellationRequested) TowerLamp.SetMode(TowerLampVisualMode.EmsFaultRedSteady); break; }
                        if (CycleStopRequested) { TowerLamp.SetMode(TowerLampVisualMode.AutoCycleStopYellowBlink); bool a = await ReturnTo101AndUnload(form, enc101, cancelToken); CycleStopRequested = false; if (!a && !mainForm.IsDisposed) mainForm.Invoke(new Action(() => MessageBox.Show("사이클 정지 실패. 원점으로 이동하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning))); await SendH4ManualAsync(form); return; }
                        if (await SendAndWaitArrival(form, AUTO_SECTION_101, "1", enc101, cancelToken) == false) { await SendH3ClearAsync(form); if (!cancelToken.IsCancellationRequested) TowerLamp.SetMode(TowerLampVisualMode.EmsFaultRedSteady); break; }
                        if (CycleStopRequested) { TowerLamp.SetMode(TowerLampVisualMode.AutoCycleStopYellowBlink); bool a = await ReturnTo101AndUnload(form, enc101, cancelToken); CycleStopRequested = false; if (!a && !mainForm.IsDisposed) mainForm.Invoke(new Action(() => MessageBox.Show("사이클 정지 실패. 원점으로 이동하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning))); await SendH4ManualAsync(form); return; }
                        if (await SendAndWaitArrival(form, AUTO_SECTION_113, "2", enc113, cancelToken) == false) { await SendH3ClearAsync(form); if (!cancelToken.IsCancellationRequested) TowerLamp.SetMode(TowerLampVisualMode.EmsFaultRedSteady); break; }
                    }
                }
                await SendH4ManualAsync(form);
            }
            catch (OperationCanceledException)
            {
                await SendH4ManualAsync(form);
            }
        }

        public async System.Threading.Tasks.Task RunAutoSequenceAsync(Command_Form form, Main mainForm, System.Threading.CancellationToken cancelToken)
        {
            await Auto_sequence(form, mainForm, cancelToken);
        }






        private async Task<bool> RunStep(Command_Form form, string targetDest, string stateName)
        {
            var proto = form._emsProto;
            var comm = form._comm;
            string vehicleID = "01"; // TODO: 실제 호기 번호로 연결 필요

            // [핵심] 1. 매니저에서 저장된 엔코더 값 가져오기
            int storedValue = _encManager.GetStoredValue(vehicleID, targetDest);

            // [핵심] 2. 패킷에 넣을 4자리 문자열 변환 로직 (미설정=-1, 설정값=0~500)
            string encoderValue;
            if (storedValue >= 0 && storedValue <= 500)
            {
                encoderValue = storedValue.ToString().PadLeft(4, '0');
            }
            else
            {
                MessageBox.Show("엔코더값 에러 (미설정이거나 범위 초과)");
                return false;
            }

            // 3. 메시지 순번(Data No) 생성
            string currentDataNo = _globalCommandCount.ToString("D4");
            _globalCommandCount = (_globalCommandCount + 1) % 10000;

            // 4. 전송용 스텝 리스트 구성
            var steps = new List<ReturnStepData> {
                new ReturnStepData {
                    SectionNo = targetDest,
                    ActionMode = "0", // 0: 이동
                    EncoderValue = encoderValue // 최종 결정된 엔코더 값 할당
                }
            }; 

            // [H4] 작업 지시
            byte[] h4 = proto.EMS_Item_order("1");
            if (h4 != null) await comm.SendData(Encoding.ASCII.GetString(h4));
            if (!await WaitForTransferCommandAcceptAfterH4Async(proto, CancellationToken.None))
                return false;

            // [H2] 상세 반송 지시
            byte[] h2 = proto.EMS_Return_Instruction(currentDataNo, steps);
            if (h2 != null) await comm.SendData(Encoding.ASCII.GetString(h2));
            await Task.Delay(500);

            for (int i = 0; i < EmsTimeoutPollCount; i++)
            {
                if (int.TryParse(proto.Parser.CurrentStatus.CurrentSectionCount, out int current) &&
                    int.TryParse(targetDest, out int target) && current == target)
                    return true;
                await Task.Delay(EmsPollIntervalMs);
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