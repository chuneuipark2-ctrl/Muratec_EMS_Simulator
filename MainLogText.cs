using System;

namespace EMS_TEST_SIMULATOR
{
    /// <summary>주요로그·통신로그용 짧은 해석 코드.</summary>
    public static class MainLogText
    {
        private const int SummaryMaxLen = 72;

        public static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            return text.Replace("\r\n", "\n").Replace('\r', '\n').Replace('\n', ' ').Trim();
        }

        /// <summary>프로그램 알람 → 해석 열 짧은 코드 (예: EMO(불가), H2(타임아웃)).</summary>
        public static string ToInterpretCode(string category, string message)
        {
            string m = Normalize(message);
            string cat = (category ?? "").Trim();

            if (m.Contains("EMO") && (m.Contains("활성") || m.Contains("눌려")))
                return "EMO(불가)";
            if (m.Contains("I.O 체크") || m.Contains("I/O 체크"))
                return "I/O(미완)";
            if (cat.Contains("EMS연결") || (m.Contains("연결") && (m.Contains("먼저") || m.Contains("비어") || m.Contains("완료") || m.Contains("연결되지"))))
                return "연결(불가)";
            if (m.Contains("저장된 호기") || m.Contains("Line_Setup") || m.Contains("저장된 정보"))
                return "Setup(미완)";
            if (m.Contains("엔코더"))
                return "엔코더(미설정)";
            if (m.Contains("명령할당"))
                return "명령(미선택)";
            if (m.Contains("시작 카운트") || m.Contains("시작위치와 목적"))
                return "위치(오류)";
            if (m.Contains("H4"))
                return m.Contains("타임아웃") || m.Contains("초과") || m.Contains("미변화") ? "H4(타임아웃)" : "H4(불가)";
            if (m.Contains("H2") || cat == "탑재" || cat == "이재")
                return m.Contains("타임아웃") || m.Contains("초과") || m.Contains("지연") ? "H2(타임아웃)" : "H2(불가)";
            if (m.Contains("원점") && m.Contains("초과"))
                return "원점(타임아웃)";
            if (m.Contains("사이클 정지"))
                return "AUTO(정지실패)";
            if (cat == "AUTO" && m.Contains("취소"))
                return "AUTO(중단)";
            if (cat == "레일")
                return "레일(연결실패)";
            if (cat == "AUTO" || cat == "반자동")
                return cat + "(불가)";

            if (string.IsNullOrEmpty(m))
                return string.IsNullOrEmpty(cat) ? "알람" : cat + "(알람)";
            return Truncate(cat + "(" + ShortKeyword(m) + ")", 24);
        }

        /// <summary>앱 알람 행 방향: 패킷 송신 관련이면 PC→EMS, 아니면 PC(앱).</summary>
        public static string ResolveAppDirection(string category, string message)
        {
            string m = Normalize(message);
            if (m.Contains("H4") || m.Contains("H2") || m.Contains("전송") || m.Contains("패킷"))
                return "PC→EMS";
            return "PC(앱)";
        }

        public static string Truncate(string text, int maxLen)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLen) return text;
            return text.Substring(0, maxLen - 1) + "…";
        }

        private static string ShortKeyword(string m)
        {
            if (m.Length <= 8) return m;
            if (m.Contains("실패")) return "실패";
            if (m.Contains("불가")) return "불가";
            if (m.Contains("오류")) return "오류";
            return "알람";
        }
    }
}
