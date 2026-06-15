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

        private static void LogSeqError(string category, string message, string title = null, MessageBoxIcon icon = MessageBoxIcon.Warning)
        {
            AppErrorLog.Raise(category, message);
            MessageBox.Show(message, title ?? category, MessageBoxButtons.OK, icon);
        }

        /// <summary>반자동·AUTO 가동 중 여부 (EMS 알람 경고창 연동용)</summary>
        public static bool IsOperationActive { get; private set; }

        private bool _legAlarmShownInCurrentLeg;

        private static string DescribeEmsStatusForAlarm(SKY_RAV_Status status)
        {
            if (status == null) return "EMS 상태 응답 없음";
            var parts = new List<string>();
            if ((status.ResponseCode ?? "00") != "00")
                parts.Add(EmsLogHelper.FormatResponseError(status.ResponseCode));
            if (!IsEmsErrorCodeClear(status))
                parts.Add(EmsLogHelper.FormatErrorCode(status.ErrorCode));
            if (status.CommandAcceptStatus != "1")
                parts.Add($"반송지령 접수: {status.CommandAcceptStatus ?? "?"} (1=가능)");
            if (status.MachineMode != "2")
                parts.Add($"기체모드: {status.MachineMode ?? "?"} (2=자동·작업가능)");
            if (!string.IsNullOrEmpty(status.CurrentSectionCount))
                parts.Add($"현재위치: {status.CurrentSectionCount.Trim()}");
            return parts.Count > 0 ? string.Join("\n", parts) : "EMS 상태 이상";
        }

        private static string DescribeH2Action(string actionMode)
        {
            switch (actionMode)
            {
                case "0": return "이동";
                case "1": return "탑재";
                case "2": return "이재";
                default: return actionMode ?? "?";
            }
        }

        private void MarkLegAlarmShown() => _legAlarmShownInCurrentLeg = true;

        private static void InvokeLogSeqError(Main mainForm, string category, string message, string title = null, MessageBoxIcon icon = MessageBoxIcon.Warning)
        {
            if (mainForm == null || mainForm.IsDisposed) return;
            mainForm.Invoke(new Action(() => LogSeqError(category, message, title, icon)));
        }

        private void InvokeLegAlarm(Main mainForm, string category, string message, string title = null, MessageBoxIcon icon = MessageBoxIcon.Warning)
        {
            MarkLegAlarmShown();
            InvokeLogSeqError(mainForm, category, message, title ?? $"{category} 알람", icon);
        }

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

        private string GetEncoderValueForSection(Main mainForm, Command_Form form, string section4Digit)
        {
            string vehicleId = form.currentData.EMS_NO ?? "";
            string posKey = int.TryParse(section4Digit, out int sec) ? sec.ToString() : "";
            if (string.IsNullOrEmpty(posKey)) return null;
            var encMgr = mainForm?._EncManager ?? _encManager;
            int v = encMgr.GetStoredValue(vehicleId, posKey);
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

        /// <summary>AUTO 등: 응답 00·자동·접수가능</summary>
        private static bool IsEmsRunStatusOk(SKY_RAV_Status status)
        {
            return status != null
                && status.ResponseCode == "00"
                && status.MachineMode == "2"
                && status.CommandAcceptStatus == "1";
        }

        /// <summary>반자동·AUTO H4 전: 응답 00·접수가능·ErrorCode 무에러 (기체 자동은 H2 직전에 확인)</summary>
        private static bool IsEmsPreConditionForH4(SKY_RAV_Status status)
        {
            return status != null
                && status.ResponseCode == "00"
                && status.CommandAcceptStatus == "1"
                && IsEmsErrorCodeClear(status);
        }

        /// <summary>반자동 H2 전 / 원점 잡힘: H4 전제 + 기체 자동</summary>
        private static bool IsEmsReadyForH2(SKY_RAV_Status status)
        {
            return IsEmsPreConditionForH4(status)
                && status.MachineMode == "2";
        }

        private static bool IsEmsErrorCodeClear(SKY_RAV_Status status)
        {
            if (status == null) return false;
            string ec = status.ErrorCode?.Trim();
            return string.IsNullOrEmpty(ec) || ec == "00";
        }

        private static bool SectionMatches(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
            if (string.Equals(a, b, StringComparison.Ordinal)) return true;
            if (int.TryParse(a, out int ia) && int.TryParse(b, out int ib)) return ia == ib;
            return false;
        }

        private const int SemiPositionChangePollCount = 10; // 200ms × 10 = 2초
        private static readonly int[] SemiStationDogCounts = { 101, 110, 113 };

        private static bool IsSemiStationSection(string section4)
        {
            foreach (int dog in SemiStationDogCounts)
            {
                if (SectionMatches(section4, DogCountToSectionNumber(dog)))
                    return true;
            }
            return false;
        }

        /// <summary>탑재/이재 leg: 시작·목적·스테이션(101/110/113)에서는 2초 위치변화 감시 생략</summary>
        private static bool IsPositionChangeMonitorExempt(string targetSection, string actionMode, string startPos, string endPos)
        {
            if (actionMode != "1" && actionMode != "2") return false;
            return SectionMatches(targetSection, startPos)
                || SectionMatches(targetSection, endPos)
                || IsSemiStationSection(targetSection);
        }

        private async Task<bool> WaitForPositionChangeWithin2SecAsync(EMS_Protocol proto, string baselineSection, CancellationToken cancelToken)
        {
            for (int i = 0; i < SemiPositionChangePollCount; i++)
            {
                if (cancelToken.IsCancellationRequested) return false;
                await Task.Delay(EmsPollIntervalMs, cancelToken);
                string current = GetCurrentSection4(proto);
                if (!string.IsNullOrEmpty(current) && !SectionMatches(current, baselineSection))
                    return true;
            }
            return false;
        }

        private async Task<bool> WaitForWorkReadyAsync(EMS_Protocol proto, CancellationToken cancelToken)
        {
            for (int i = 0; i < EmsTimeoutPollCount; i++)
            {
                if (cancelToken.IsCancellationRequested) return false;
                if (IsEmsReadyForH2(proto?.Parser?.CurrentStatus))
                    return true;
                await Task.Delay(EmsPollIntervalMs, cancelToken);
            }
            return false;
        }

        /// <summary>반자동·AUTO 공통: H4(1) 1회 → 2초 내 위치변화 → 작업가능(원점) 대기. 101 정렬 없음.</summary>
        private async Task<bool> RunSemiAutoHomingAsync(Command_Form form, EMS_Protocol proto, Main mainForm,
            CancellationToken cancelToken, string dialogTitle = "반자동")
        {
            if (form?._comm == null)
            {
                if (!mainForm.IsDisposed)
                    InvokeLogSeqError(mainForm, dialogTitle, "H4 전송 불가: 통신 연결이 없습니다.", $"{dialogTitle} 알람");
                return false;
            }

            if (!IsEmsPreConditionForH4(proto?.Parser?.CurrentStatus))
            {
                var s = proto?.Parser?.CurrentStatus;
                if (!mainForm.IsDisposed)
                    InvokeLogSeqError(mainForm, dialogTitle,
                        $"H4 전 EMS 조건 미충족\n{DescribeEmsStatusForAlarm(s)}",
                        $"{dialogTitle} 알람", MessageBoxIcon.Warning);
                return false;
            }

            string baselineSection = GetCurrentSection4(proto);
            byte[] h4 = EMS_Order.EMS_Item_order("1");
            if (h4 == null)
            {
                if (!mainForm.IsDisposed)
                    InvokeLogSeqError(mainForm, dialogTitle, "H4(작업지시) 패킷 생성에 실패했습니다.", $"{dialogTitle} 알람");
                return false;
            }
            await form._comm.SendData(Encoding.ASCII.GetString(h4));

            if (!await WaitForPositionChangeWithin2SecAsync(proto, baselineSection, cancelToken))
            {
                await SendH3ClearAsync(form);
                if (!mainForm.IsDisposed)
                    InvokeLogSeqError(mainForm, dialogTitle,
                        "H4 후 2초 안에 현재 위치(CurrentSectionCount)가 변하지 않았습니다.\n원점 잡힘·H4 접수를 확인하세요.",
                        $"{dialogTitle} 알람", MessageBoxIcon.Error);
                return false;
            }

            if (!await WaitForWorkReadyAsync(proto, cancelToken))
            {
                await SendH3ClearAsync(form);
                if (!mainForm.IsDisposed)
                    InvokeLogSeqError(mainForm, dialogTitle,
                        $"원점(작업가능) 대기 시간 초과 ({EmsTimeoutSeconds}초).\nMachineMode=2·접수1·에러없음 조건 미충족.\n{DescribeEmsStatusForAlarm(proto?.Parser?.CurrentStatus)}",
                        $"{dialogTitle} 알람", MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private async Task<(bool success, string failReason)> WaitSectionArrivalWithReasonAsync(
            EMS_Protocol proto, string section4, CancellationToken cancelToken)
        {
            for (int i = 0; i < EmsTimeoutPollCount; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return (false, "동작이 취소되었습니다.");
                var status = proto?.Parser?.CurrentStatus;
                if (!IsEmsReadyForH2(status))
                    return (false, $"EMS 작업가능 상태 상실\n{DescribeEmsStatusForAlarm(status)}");
                if (SectionMatches(GetCurrentSection4(proto), section4))
                    return (true, null);
                await Task.Delay(EmsPollIntervalMs, cancelToken);
            }
            return (false, $"목적 위치({section4}) 도착 대기 시간 초과 ({EmsTimeoutSeconds}초)\n현재위치: {GetCurrentSection4(proto)}");
        }

        /// <summary>반자동·AUTO H2 1회 전송 및 leg 완료 대기</summary>
        private async Task<bool> SendSemiAutoH2LegAsync(Command_Form form, EMS_Protocol proto, Main mainForm,
            string section4, string actionMode, string encoderValue, string startPos, string endPos,
            CancellationToken cancelToken, string dialogTitle = "반자동")
        {
            _legAlarmShownInCurrentLeg = false;
            string actionLabel = DescribeH2Action(actionMode);

            if (form?._comm == null)
            {
                if (!mainForm.IsDisposed)
                    InvokeLegAlarm(mainForm, dialogTitle, $"H2 전송 불가: 통신 연결이 없습니다.\n목적:{section4} ({actionLabel})", $"{dialogTitle} 알람");
                return false;
            }

            if (!IsEmsReadyForH2(proto?.Parser?.CurrentStatus))
            {
                if (!mainForm.IsDisposed)
                    InvokeLegAlarm(mainForm, dialogTitle,
                        $"H2 전송 전 EMS 작업가능 상태가 아닙니다. (목적:{section4}, {actionLabel})\n{DescribeEmsStatusForAlarm(proto?.Parser?.CurrentStatus)}",
                        $"{dialogTitle} 알람", MessageBoxIcon.Warning);
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
            if (h2 == null)
            {
                if (!mainForm.IsDisposed)
                    InvokeLegAlarm(mainForm, dialogTitle, $"H2 패킷 생성 실패. (목적:{section4}, {actionLabel})", $"{dialogTitle} 알람");
                return false;
            }

            string baselineAtSend = GetCurrentSection4(proto);
            bool monitorPositionChange = !IsPositionChangeMonitorExempt(section4, actionMode, startPos, endPos);

            await form._comm.SendData(Encoding.ASCII.GetString(h2));
            await Task.Delay(500, cancelToken);

            if (monitorPositionChange)
            {
                if (!await WaitForPositionChangeWithin2SecAsync(proto, baselineAtSend, cancelToken))
                {
                    await SendH3ClearAsync(form);
                    if (!mainForm.IsDisposed)
                        InvokeLegAlarm(mainForm, dialogTitle,
                            $"H2 후 2초 안에 위치가 변하지 않았습니다.\n목적:{section4} ({actionLabel})\n전송 전 위치:{baselineAtSend}",
                            $"{dialogTitle} 알람", MessageBoxIcon.Error);
                    return false;
                }
            }

            if (SectionMatches(GetCurrentSection4(proto), section4))
            {
                await SendH3ClearAsync(form);
                return true;
            }

            var (arrived, failReason) = await WaitSectionArrivalWithReasonAsync(proto, section4, cancelToken);
            if (arrived)
            {
                await SendH3ClearAsync(form);
                return true;
            }

            await SendH3ClearAsync(form);
            if (!mainForm.IsDisposed)
                InvokeLegAlarm(mainForm, dialogTitle,
                    $"H2 leg 실패 (목적:{section4}, {actionLabel})\n{failReason}",
                    $"{dialogTitle} 알람", MessageBoxIcon.Warning);
            return false;
        }

        private async Task<bool> AutoSendH2StepAsync(Command_Form form, EMS_Protocol proto, Main mainForm,
            string section4, string actionMode, string encoder, CancellationToken cancelToken)
        {
            bool ok = await SendSemiAutoH2LegAsync(form, proto, mainForm, section4, actionMode, encoder,
                Start_position, End_position, cancelToken, "AUTO");
            if (!ok)
                AutoH2StepFailed(mainForm, section4, actionMode, cancelToken);
            return ok;
        }

        private static bool ValidateSemiAutoCargoForMode(int mode, SKY_RAV_Status status, out string message)
        {
            message = null;
            if (mode == 2 && status?.CargoStatus == "1")
            {
                message = "이미 화물이 탑재되어 있습니다.";
                return false;
            }
            if (mode == 3 && status?.CargoStatus == "0")
            {
                message = "화물이 없습니다. 이재 명령을 취소합니다.";
                return false;
            }
            return true;
        }

        private static bool ValidateSemiAutoFormSetup(Command_Form form, out string message)
        {
            message = null;
            if (string.IsNullOrEmpty(Line_Setup.SavedVehicleNo) || string.IsNullOrEmpty(Line_Setup.SavedLineName))
            {
                message = "저장된 정보가 없습니다. Line_Setup에서 [상태저장]을 실행해 주세요.";
                return false;
            }
            if (form.currentData.EMS_NO != Line_Setup.SavedVehicleNo)
            {
                message = "Line_Setup에 저장된 호기와 반자동 명령의 호기가 일치하지 않습니다.";
                return false;
            }
            if (form.currentData.rail_data != Line_Setup.SavedLineName)
            {
                message = "Line_Setup에 저장된 레일과 반자동 명령의 레일선택이 일치하지 않습니다.";
                return false;
            }
            return true;
        }

        /// <summary>원점 잡힌 뒤 시작카운트≠EMS현재면 시작→목적, 같으면 목적만</summary>
        private async Task<bool> RunSemiAutoLegsAfterHomingAsync(Command_Form form, EMS_Protocol proto, Main mainForm,
            string finalActionMode, string finalEncoder, CancellationToken cancelToken)
        {
            string emsPos = GetCurrentSection4(proto);

            if (!SectionMatches(Start_position, emsPos))
            {
                if (!await SendSemiAutoH2LegAsync(form, proto, mainForm, Start_position, "0", "0000",
                        Start_position, End_position, cancelToken))
                    return false;
            }

            if (string.Equals(finalActionMode, "0", StringComparison.Ordinal))
            {
                if (SectionMatches(GetCurrentSection4(proto), End_position))
                    return true;
            }

            return await SendSemiAutoH2LegAsync(form, proto, mainForm, End_position, finalActionMode, finalEncoder,
                Start_position, End_position, cancelToken);
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


        /// <summary>반자동: H4 → 원점 → H2 leg(시작/목적). 탑재/이재는 화물·엔코더 조건 추가.</summary>
        public async Task<bool> SemiAuto_Sequence(Command_Form form, Main mainForm)
        {
            BindSemiAutoCommAndProto(form, mainForm);
            var proto = form._emsProto;
            int mode = form.currentData.command_alloc;

            if (form._comm == null)
            {
                LogSeqError("반자동", "통신이 연결되지 않았습니다.", "반자동", MessageBoxIcon.Warning);
                return false;
            }

            var status = proto?.Parser?.CurrentStatus;
            if (status == null || status.ResponseCode == null)
            {
                LogSeqError("반자동", "EMS로부터 응답이 없습니다.", "반자동", MessageBoxIcon.Warning);
                return false;
            }

            if (!ValidateSemiAutoFormSetup(form, out string setupMsg))
            {
                LogSeqError("반자동", setupMsg, "반자동", MessageBoxIcon.Warning);
                return false;
            }

            if (!ValidateSemiAutoCargoForMode(mode, status, out string cargoMsg))
            {
                LogSeqError("반자동", cargoMsg, "반자동", MessageBoxIcon.Warning);
                return false;
            }

            _ = StartBlinking(2);
            var cancelToken = CancellationToken.None;
            IsOperationActive = true;

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
                        LogSeqError("반자동", "지원하지 않는 반자동 명령입니다.", "반자동", MessageBoxIcon.Warning);
                        return false;
                }
            }
            catch (Exception ex)
            {
                LogSeqError("반자동", ex.Message, "반자동", MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                IsOperationActive = false;
                _blinkToken?.Cancel();
            }
        }

        private async Task<bool> SemiAuto_RunMoveAsync(Command_Form form, EMS_Protocol proto, Main mainForm, CancellationToken cancelToken)
        {
            Start_position = DogCountToSectionNumber(form.currentData.Start_count);
            End_position = DogCountToSectionNumber(form.currentData.End_count);

            if (Start_position == End_position)
            {
                LogSeqError("반자동", "시작위치와 목적위치가 같습니다.", "반자동", MessageBoxIcon.Warning);
                return false;
            }

            if (!await RunSemiAutoHomingAsync(form, proto, mainForm, cancelToken))
                return false;

            return await RunSemiAutoLegsAfterHomingAsync(form, proto, mainForm, "0", "0000", cancelToken);
        }

        private async Task<bool> SemiAuto_RunLoadAsync(Command_Form form, EMS_Protocol proto, Main mainForm, CancellationToken cancelToken)
        {
            Start_position = DogCountToSectionNumber(form.currentData.Start_count);
            End_position = DogCountToSectionNumber(form.currentData.End_count);

            string encEnd = GetEncoderValueForSection(mainForm, form, End_position);
            if (encEnd == null)
            {
                LogSeqError("엔코더", "엔코더 설정에 해당 목적위치 값이 존재하지 않습니다.", "엔코더 값 없음", MessageBoxIcon.Warning);
                return false;
            }

            if (Start_position == End_position)
            {
                LogSeqError("반자동", "시작위치와 목적위치가 같습니다.", "반자동", MessageBoxIcon.Warning);
                return false;
            }

            if (!await RunSemiAutoHomingAsync(form, proto, mainForm, cancelToken))
                return false;

            if (!ValidateSemiAutoCargoForMode(2, proto.Parser.CurrentStatus, out string cargoMsg))
            {
                LogSeqError("탑재", cargoMsg, "탑재", MessageBoxIcon.Warning);
                return false;
            }

            return await RunSemiAutoLegsAfterHomingAsync(form, proto, mainForm, "1", encEnd, cancelToken);
        }

        private async Task<bool> SemiAuto_RunUnloadAsync(Command_Form form, EMS_Protocol proto, Main mainForm, CancellationToken cancelToken)
        {
            Start_position = DogCountToSectionNumber(form.currentData.Start_count);
            End_position = DogCountToSectionNumber(form.currentData.End_count);

            string encEnd = GetEncoderValueForSection(mainForm, form, End_position);
            if (encEnd == null)
            {
                LogSeqError("엔코더", "엔코더 설정에 해당 목적위치 값이 존재하지 않습니다.", "엔코더 값 없음", MessageBoxIcon.Warning);
                return false;
            }

            if (Start_position == End_position)
            {
                LogSeqError("반자동", "시작위치와 목적위치가 같습니다.", "반자동", MessageBoxIcon.Warning);
                return false;
            }

            if (!await RunSemiAutoHomingAsync(form, proto, mainForm, cancelToken))
                return false;

            if (!ValidateSemiAutoCargoForMode(3, proto.Parser.CurrentStatus, out string cargoMsg))
            {
                LogSeqError("이재", cargoMsg, "이재", MessageBoxIcon.Warning);
                return false;
            }

            return await RunSemiAutoLegsAfterHomingAsync(form, proto, mainForm, "2", encEnd, cancelToken);
        }

        private const string AUTO_SECTION_101 = "0101";
        private const string AUTO_SECTION_110 = "0110";
        private const string AUTO_SECTION_113 = "0113";

        /// <summary>true로 설정 시 현재 사이클 끝난 뒤 101번으로 복귀하여 타이어 이재 후 정지</summary>
        public static bool CycleStopRequested { get; set; }

        /// <summary>AUTO 사이클 H2 스텝 (반자동과 동일 leg 로직)</summary>
        private async Task<bool> ReturnTo101AndUnload(Command_Form form, Main mainForm, string enc101, CancellationToken cancelToken)
        {
            return await AutoSendH2StepAsync(form, form._emsProto, mainForm, AUTO_SECTION_101, "2", enc101, cancelToken);
        }

        private void AutoH2StepFailed(Main mainForm, string section4, string actionMode, CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested || mainForm.IsDisposed) return;
            string actionLabel = DescribeH2Action(actionMode);
            mainForm.Invoke(new Action(() =>
            {
                if (!_legAlarmShownInCurrentLeg)
                {
                    LogSeqError("AUTO",
                        $"AUTO H2 스텝 실패 (목적:{section4}, {actionLabel})\n원인을 확인할 수 없습니다. 통신·EMS 상태·엔코더를 점검하세요.",
                        "AUTO 알람", MessageBoxIcon.Error);
                }
                TowerLamp.SetMode(TowerLampVisualMode.EmsFaultRedSteady);
            }));
        }

        private static void LogCycleStopFail(Main mainForm)
        {
            InvokeLogSeqError(mainForm, "AUTO",
                "사이클 정지 실패.\n101번 위치 복귀·타이어 이재(H2)가 완료되지 않았습니다.\n원점 상태를 확인하세요.",
                "AUTO 알람", MessageBoxIcon.Warning);
        }

        /// <summary>모든 사이클 종료 시 H4 수동(0) 전송</summary>
        private async Task SendH4ManualAsync(Command_Form form)
        {
            if (form?._comm == null) return;
            byte[] h4 = EMS_Order.EMS_Item_order("0"); // 0: 수동
            if (h4 != null) await form._comm.SendData(Encoding.ASCII.GetString(h4));
        }

        /// <summary>H3 반송데이터 클리어 (H2 leg 성공·실패/타임아웃 후 EMS 반송 버퍼 정리)</summary>
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
            IsOperationActive = true;
            try
            {
                TowerLamp.SetMode(TowerLampVisualMode.AutoAfterConfirmBlueBlink);
                var proto = form._emsProto;
                string enc101 = GetEncoderValueForSection(mainForm, form, AUTO_SECTION_101) ?? "0000";
                string enc110 = GetEncoderValueForSection(mainForm, form, AUTO_SECTION_110) ?? "0000";
                string enc113 = GetEncoderValueForSection(mainForm, form, AUTO_SECTION_113) ?? "0000";

                Start_position = DogCountToSectionNumber(form.currentData.Start_count);
                End_position = DogCountToSectionNumber(form.currentData.End_count);

                // AUTO 레일 8비트: 기본은 미검사. Main 개발 모드(BypassRailInterlockForAuto=false)일 때만 검사.
                Rail_DIO.Instance.io_test_flag = false;
                if (mainForm != null && !mainForm.BypassRailInterlockForAuto)
                {
                    while (!Rail_DIO.Instance.Is8BitSensorInterlockOkForAuto())
                    {
                        await Task.Delay(EmsPollIntervalMs, cancelToken);
                        if (cancelToken.IsCancellationRequested) return;
                    }
                }

                if (!await RunSemiAutoHomingAsync(form, proto, mainForm, cancelToken, "AUTO"))
                {
                    if (!cancelToken.IsCancellationRequested)
                        TowerLamp.SetMode(TowerLampVisualMode.EmsFaultRedSteady);
                    return;
                }

                TowerLamp.SetMode(TowerLampVisualMode.AutoH4WaitGreenBlink);

                // Command_Form 시작/목적 leg (반자동과 동일)
                if (form.currentData.Start_count > 0 && form.currentData.End_count > 0
                    && !SectionMatches(Start_position, End_position))
                {
                    if (!await RunSemiAutoLegsAfterHomingAsync(form, proto, mainForm, "0", "0000", cancelToken))
                    {
                        if (!cancelToken.IsCancellationRequested)
                            TowerLamp.SetMode(TowerLampVisualMode.EmsFaultRedSteady);
                        return;
                    }
                }

                TowerLamp.SetMode(TowerLampVisualMode.AutoH2RunGreenBlink);
                bool firstLoop = true;
                while (!cancelToken.IsCancellationRequested)
                {
                    if (CycleStopRequested)
                    {
                        TowerLamp.SetMode(TowerLampVisualMode.AutoCycleStopYellowBlink);
                        bool arrived = await ReturnTo101AndUnload(form, mainForm, enc101, cancelToken);
                        CycleStopRequested = false;
                        if (!arrived)
                        {
                            await SendH3ClearAsync(form);
                            if (!mainForm.IsDisposed)
                                LogCycleStopFail(mainForm);
                        }
                        await SendH4ManualAsync(form);
                        return;
                    }
                    if (firstLoop)
                    {
                        if (!await AutoSendH2StepAsync(form, proto, mainForm, AUTO_SECTION_101, "1", enc101, cancelToken)) break;
                        if (CycleStopRequested) { TowerLamp.SetMode(TowerLampVisualMode.AutoCycleStopYellowBlink); bool a = await ReturnTo101AndUnload(form, mainForm, enc101, cancelToken); CycleStopRequested = false; if (!a && !mainForm.IsDisposed) LogCycleStopFail(mainForm); await SendH4ManualAsync(form); return; }
                        if (!await AutoSendH2StepAsync(form, proto, mainForm, AUTO_SECTION_110, "2", enc110, cancelToken)) break;
                        if (CycleStopRequested) { TowerLamp.SetMode(TowerLampVisualMode.AutoCycleStopYellowBlink); bool a = await ReturnTo101AndUnload(form, mainForm, enc101, cancelToken); CycleStopRequested = false; if (!a && !mainForm.IsDisposed) LogCycleStopFail(mainForm); await SendH4ManualAsync(form); return; }
                        if (!await AutoSendH2StepAsync(form, proto, mainForm, AUTO_SECTION_110, "1", enc110, cancelToken)) break;
                        if (CycleStopRequested) { TowerLamp.SetMode(TowerLampVisualMode.AutoCycleStopYellowBlink); bool a = await ReturnTo101AndUnload(form, mainForm, enc101, cancelToken); CycleStopRequested = false; if (!a && !mainForm.IsDisposed) LogCycleStopFail(mainForm); await SendH4ManualAsync(form); return; }
                        if (!await AutoSendH2StepAsync(form, proto, mainForm, AUTO_SECTION_113, "2", enc113, cancelToken)) break;
                        firstLoop = false;
                    }
                    else
                    {
                        if (!await AutoSendH2StepAsync(form, proto, mainForm, AUTO_SECTION_113, "1", enc113, cancelToken)) break;
                        if (CycleStopRequested) { TowerLamp.SetMode(TowerLampVisualMode.AutoCycleStopYellowBlink); bool a = await ReturnTo101AndUnload(form, mainForm, enc101, cancelToken); CycleStopRequested = false; if (!a && !mainForm.IsDisposed) LogCycleStopFail(mainForm); await SendH4ManualAsync(form); return; }
                        if (!await AutoSendH2StepAsync(form, proto, mainForm, AUTO_SECTION_101, "2", enc101, cancelToken)) break;
                        if (CycleStopRequested) { TowerLamp.SetMode(TowerLampVisualMode.AutoCycleStopYellowBlink); bool a = await ReturnTo101AndUnload(form, mainForm, enc101, cancelToken); CycleStopRequested = false; if (!a && !mainForm.IsDisposed) LogCycleStopFail(mainForm); await SendH4ManualAsync(form); return; }
                        if (!await AutoSendH2StepAsync(form, proto, mainForm, AUTO_SECTION_101, "1", enc101, cancelToken)) break;
                        if (CycleStopRequested) { TowerLamp.SetMode(TowerLampVisualMode.AutoCycleStopYellowBlink); bool a = await ReturnTo101AndUnload(form, mainForm, enc101, cancelToken); CycleStopRequested = false; if (!a && !mainForm.IsDisposed) LogCycleStopFail(mainForm); await SendH4ManualAsync(form); return; }
                        if (!await AutoSendH2StepAsync(form, proto, mainForm, AUTO_SECTION_113, "2", enc113, cancelToken)) break;
                    }
                }
                await SendH4ManualAsync(form);
            }
            catch (OperationCanceledException)
            {
                await SendH4ManualAsync(form);
                if (!cancelToken.IsCancellationRequested && !mainForm.IsDisposed)
                    InvokeLogSeqError(mainForm, "AUTO", "AUTO 동작이 취소되었습니다.\n(EMO 또는 사용자 중단)", "AUTO 알람", MessageBoxIcon.Warning);
            }
            finally
            {
                IsOperationActive = false;
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
                LogSeqError("엔코더", "엔코더값 에러 (미설정이거나 범위 초과)", "엔코더", MessageBoxIcon.Warning);
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
                {
                    await SendH3ClearAsync(form);
                    return true;
                }
                await Task.Delay(EmsPollIntervalMs);
            }
            await SendH3ClearAsync(form);
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