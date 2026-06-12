using System;
using System.Collections.Generic;
using System.Text;

namespace EMS_TEST_SIMULATOR
{
    public struct EmsStatusSnapshot
    {
        public string ResponseCode;
        public string ErrorCode;
        public string CommandAcceptStatus;
        public string MachineMode;
        public string CurrentSectionCount;
    }

    /// <summary>EMS @TXT240 상태보고 파싱 및 응답/에러 코드 해석.</summary>
    public static class EmsLogHelper
    {
        private static readonly Dictionary<string, string> ResponseNames = BuildResponseNames();
        private static readonly Dictionary<string, string> ErrorNames = BuildErrorNames();

        public static bool TryParseStatus(byte[] data, out EmsStatusSnapshot snapshot)
        {
            snapshot = default;
            if (data == null || data.Length < 38) return false;
            try
            {
                string res = Encoding.ASCII.GetString(data);
                int idx = res.IndexOf("@TXT240");
                if (idx < 0) return false;
                string body = res.Substring(idx + 12);
                if (body.Length < 26) return false;

                snapshot.ResponseCode = body.Substring(0, 2);
                snapshot.CurrentSectionCount = body.Substring(6, 4);
                snapshot.ErrorCode = body.Substring(21, 2);
                snapshot.MachineMode = body.Substring(23, 1);
                snapshot.CommandAcceptStatus = body.Length > 25 ? body.Substring(25, 1) : "";
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsErrorCodeActive(string errorCode)
        {
            string ec = errorCode?.Trim();
            return !string.IsNullOrEmpty(ec) && ec != "00";
        }

        public static bool HasReportedFault(EmsStatusSnapshot s)
        {
            return (s.ResponseCode ?? "00") != "00" || IsErrorCodeActive(s.ErrorCode);
        }

        public static string FormatResponseError(string responseCode)
        {
            string code = (responseCode ?? "??").Trim();
            if (ResponseNames.TryGetValue(code, out string name) && name != "-")
                return $"응답코드 {code}: {name}";
            return $"응답코드 {code} 에러";
        }

        public static string FormatErrorCode(string errorCode)
        {
            string code = (errorCode ?? "??").Trim();
            if (ErrorNames.TryGetValue(code, out string name) && name != "-")
                return $"에러코드 {code}: {name}";
            return $"에러코드 {code}";
        }

        public static string BuildStatusErrorDescription(EmsStatusSnapshot s)
        {
            var parts = new List<string>();
            if ((s.ResponseCode ?? "00") != "00")
                parts.Add(FormatResponseError(s.ResponseCode));
            if (IsErrorCodeActive(s.ErrorCode))
                parts.Add(FormatErrorCode(s.ErrorCode));
            if (parts.Count == 0)
                parts.Add("EMS 상태 이상");
            if (!string.IsNullOrEmpty(s.CurrentSectionCount))
                parts.Add($"현재위치 {s.CurrentSectionCount}");
            return string.Join("; ", parts);
        }

        private static Dictionary<string, string> BuildResponseNames()
        {
            var d = new Dictionary<string, string>(StringComparer.Ordinal);
            void A(string c, string n) => d[c] = n;
            A("00", "정상 수신 완료");
            A("01", "Header 미검출");
            A("03", "수신 문자 초과");
            A("04", "체크섬 에러");
            A("05", "하드 에러");
            A("08", "수신제 에러");
            A("09", "시퀀스 순서 에러");
            A("40", "ID 에러");
            A("41", "전문 길이 에러");
            A("42", "포인트 No. 에러");
            A("44", "현재 동작중 에러");
            A("45", "이재 포인트 에러");
            A("46", "지시모드 에러");
            A("47", "데이터 취소 이상");
            return d;
        }

        private static Dictionary<string, string> BuildErrorNames()
        {
            var d = new Dictionary<string, string>(StringComparer.Ordinal);
            void A(string c, string n) => d[c] = n;
            A("01", "주행 인버터 이상(주행중)");
            A("02", "팬모터 이상");
            A("03", "인버터 운전중 신호이상(주행중)");
            A("04", "인버터 운전중 신호출력이상(주행중)");
            A("06", "이재 모터 과부하(주행중)");
            A("07", "주행 모터 과부하");
            A("08", "인버터 주파수검출 ON 대기이상/승강타이밍벨트 이상(주행중)");
            A("09", "인버터 주파수검출 OFF 대기이상/센서네트 경보(주행중)");
            A("10", "인버터 이상(승강중)");
            A("11", "하강 FINAL 이상");
            A("12", "하강 원점 확인 이상");
            A("13", "인버터 운전중 신호 이상(승강중)");
            A("14", "상승 원점 확인 이상");
            A("15", "엔코더 이상");
            A("16", "CHUCK 모터 과부하(승강중)");
            A("17", "이재 모터 과부하(승강중)");
            A("18", "승강 타이밍 벨트 이상(승강중)");
            A("19", "SENSOR NET 이상(승강중)");
            A("20", "승강 원점 이상(주행중)");
            A("21", "카운트 미스");
            A("66", "인터록 미검출 이상");
            A("72", "주행 시간 초과");
            A("78", "초기 주행 발진 가능 시간 초과");
            A("79", "반송개시 시간 초과");
            A("81", "섹션 카운트 에러");
            A("82", "동작 모드 에러");
            A("83", "반송지령 수신 불가 이상");
            A("88", "하드웨어 에러");
            A("89", "BCC 에러");
            A("91", "프로그램 섬 체크 이상");
            A("99", "시퀀스 번호 이상");
            return d;
        }
    }
}
