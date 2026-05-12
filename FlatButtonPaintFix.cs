using System.Windows.Forms;

namespace EMS_TEST_SIMULATOR
{
    /// <summary>
    /// FlatStyle.Flat 버튼은 눌림/호버 시 기본 배경이 달라지며 텍스트 위치가 미세하게 바뀌는 경우가 있음.
    /// MouseDown/MouseOver 배경을 현재 BackColor와 동일하게 맞춤.
    /// </summary>
    internal static class FlatButtonPaintFix
    {
        public static void ApplyToButton(Button btn)
        {
            if (btn == null || btn.FlatStyle != FlatStyle.Flat) return;
            var c = btn.BackColor;
            btn.FlatAppearance.MouseDownBackColor = c;
            btn.FlatAppearance.MouseOverBackColor = c;
        }

        public static void ApplyToTree(Control root)
        {
            if (root == null) return;
            foreach (Control c in root.Controls)
            {
                if (c is Button btn && btn.FlatStyle == FlatStyle.Flat)
                    ApplyToButton(btn);
                if (c.HasChildren)
                    ApplyToTree(c);
            }
        }
    }
}
