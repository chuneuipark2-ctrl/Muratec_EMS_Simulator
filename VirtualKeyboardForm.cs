using System;
using System.Drawing;
using System.Windows.Forms;

namespace EMS_TEST_SIMULATOR
{
    /// <summary>터치 PC용 가상 키보드. 수행원 등 텍스트 입력 시 표시. 영문·숫자·특수문자.</summary>
    public class VirtualKeyboardForm : Form
    {
        private readonly TextBox _target;
        private const string NumRow = "1234567890";
        private const string SymRow = "!@#$%^&*()";
        private const string EngRow2 = "QWERTYUIOP";
        private const string EngRow3 = "ASDFGHJKL";
        private const string EngRow4 = "ZXCVBNM";
        private const int KeyWidth = 44;
        private const int KeyHeight = 44;

        public VirtualKeyboardForm(TextBox target)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            if (_target.IsDisposed) return;

            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(45, 45, 48);
            Font = new Font("맑은 고딕", 12F);
            Text = "가상 키보드";
            MinimumSize = new Size(520, 440);
            Size = new Size(560, 460);
            KeyPreview = true;

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 8, 8, 8),
                ColumnCount = 1,
                RowCount = 6
            };
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, KeyHeight + 8));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, KeyHeight + 8));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 40f));

            panel.Controls.Add(CreateNumberRow(), 0, 0);
            panel.Controls.Add(CreateSymbolRow(), 0, 1);
            panel.Controls.Add(CreateLetterRow(EngRow2), 0, 2);
            panel.Controls.Add(CreateLetterRow(EngRow3), 0, 3);
            panel.Controls.Add(CreateLetterRow(EngRow4), 0, 4);
            panel.Controls.Add(CreateBottomRow(), 0, 5);

            Controls.Add(panel);

            FlatButtonPaintFix.ApplyToTree(this);

            // 화면 작업 영역 안에 전부 보이게 위치 (하단 중앙, 잘리지 않도록)
            var screen = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 800, 600);
            int x = screen.Left + Math.Max(0, (screen.Width - Width) / 2);
            int y = screen.Bottom - Height - 16;
            if (y < screen.Top) y = screen.Top + 16;
            Location = new Point(x, y);
        }

        private FlowLayoutPanel CreateNumberRow()
        {
            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(3, 3, 3, 3)
            };
            for (int i = 0; i < NumRow.Length; i++)
            {
                var btn = CreateKeyButton(NumRow[i].ToString(), KeyWidth);
                btn.Click += (s, e) => AppendText(((Button)s).Text);
                row.Controls.Add(btn);
            }
            return row;
        }

        private FlowLayoutPanel CreateSymbolRow()
        {
            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(3, 3, 3, 3)
            };
            foreach (char c in SymRow)
            {
                var btn = CreateKeyButton(c.ToString(), KeyWidth);
                btn.Click += (s, e) => AppendText(((Button)s).Text);
                row.Controls.Add(btn);
            }
            return row;
        }

        private FlowLayoutPanel CreateLetterRow(string keys)
        {
            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(3, 3, 3, 3)
            };
            foreach (char c in keys)
            {
                var btn = CreateKeyButton(c.ToString(), KeyWidth);
                btn.Click += (s, e) => AppendText(((Button)s).Text);
                row.Controls.Add(btn);
            }
            return row;
        }

        private FlowLayoutPanel CreateBottomRow()
        {
            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(3, 3, 3, 3)
            };
            var spaceBtn = CreateKeyButton("  공백  ", 120);
            spaceBtn.Click += (s, e) => AppendText(" ");
            row.Controls.Add(spaceBtn);

            var backBtn = CreateKeyButton("⌫ 삭제", 120);
            backBtn.Click += (s, e) =>
            {
                if (_target.IsDisposed) return;
                if (_target.Text.Length > 0)
                    _target.Text = _target.Text.Substring(0, _target.Text.Length - 1);
            };
            row.Controls.Add(backBtn);

            var clearBtn = CreateKeyButton("지우기", 100);
            clearBtn.Click += (s, e) => { if (!_target.IsDisposed) _target.Clear(); };
            row.Controls.Add(clearBtn);

            var closeBtn = CreateKeyButton("닫기", 100);
            closeBtn.Click += (s, e) => Close();
            row.Controls.Add(closeBtn);

            return row;
        }

        private Button CreateKeyButton(string text, int width)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(width, KeyHeight),
                MinimumSize = new Size(width, KeyHeight),
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 11F),
                Margin = new Padding(3, 3, 3, 3),
                Padding = new Padding(2, 2, 2, 2)
            };
            btn.FlatAppearance.BorderColor = Color.Gray;
            return btn;
        }

        private void AppendText(string s)
        {
            if (_target.IsDisposed) return;
            _target.AppendText(s);
        }
    }
}
