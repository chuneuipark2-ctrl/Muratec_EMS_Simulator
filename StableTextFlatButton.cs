using System.Drawing;
using System.Windows.Forms;

namespace EMS_TEST_SIMULATOR
{
    /// <summary>
    /// 기본 Flat 버튼은 눌렀다 뗀 뒤에도 GDI 텍스트 사각형이 달라져 글자가 위로 붙어 보이는 경우가 있음(특히 맑은 고딕).
    /// 배경과 텍스트를 TextRenderer로 고정 규칙으로만 그려 세로 위치를 유지함.
    /// </summary>
    public class StableTextFlatButton : Button
    {
        public StableTextFlatButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Color back = Enabled ? BackColor : SystemColors.Control;
            using (var br = new SolidBrush(back))
                g.FillRectangle(br, ClientRectangle);

            Color fore = Enabled ? ForeColor : SystemColors.GrayText;
            TextFormatFlags flags =
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine |
                TextFormatFlags.NoPadding |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.EndEllipsis;

            Rectangle textRect = ClientRectangle;
            if (Padding != Padding.Empty)
                textRect = new Rectangle(
                    ClientRectangle.Left + Padding.Left,
                    ClientRectangle.Top + Padding.Top,
                    ClientRectangle.Width - Padding.Horizontal,
                    ClientRectangle.Height - Padding.Vertical);

            TextRenderer.DrawText(g, Text ?? string.Empty, Font, textRect, fore, flags);

            if (Focused && ShowFocusCues)
                ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(ClientRectangle, -3, -3));
        }
    }
}
