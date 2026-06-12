using System;
using System.Windows.Forms;

namespace EMS_TEST_SIMULATOR
{
    /// <summary>프로그램(시퀀스·폼)에서 발생한 에러를 Main 주요로그로 전달.</summary>
    public static class AppErrorLog
    {
        public static event Action<string, string> OnError;

        public static void Raise(string category, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            OnError?.Invoke(category ?? "에러", message);
        }

        public static void RaiseAndShow(string category, string message, string title = null,
            MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Warning)
        {
            Raise(category, message);
            MessageBox.Show(message, title ?? category, buttons, icon);
        }
    }
}