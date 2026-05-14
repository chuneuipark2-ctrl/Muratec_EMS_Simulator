using System;

namespace EMS_TEST_SIMULATOR
{
    /// <summary>통신 로그를 Connect 폼 참조 없이 Main으로 전달하는 정적 브릿지. Main이 Form_Load에서 구독.</summary>
    public static class CommLogBridge
    {
        public static event Action<string, byte[], string> OnLog;

        public static void Raise(string direction, byte[] data, string description) => OnLog?.Invoke(direction, data, description);
    }
}
