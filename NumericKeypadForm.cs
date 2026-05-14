using System;
using System.Drawing;
using System.Windows.Forms;

namespace EMS_TEST_SIMULATOR
{
    /// <summary>
    /// 터치/HMI용 숫자만 입력하는 작은 모달 키패드.
    /// </summary>
    public class NumericKeypadForm : Form
    {
        private readonly TextBox _display;
        private readonly int _maxDigits;

        public string ResultText => _display.Text;

        public NumericKeypadForm(string initialValue, int maxDigits = 12)
        {
            _maxDigits = maxDigits;
            Text = "숫자 입력";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(45, 45, 48);
            ForeColor = Color.White;
            ClientSize = new Size(280, 320);
            Font = new Font("맑은 고딕", 11F);

            _display = new TextBox
            {
                Location = new Point(12, 12),
                Size = new Size(256, 32),
                ReadOnly = true,
                TabStop = false,
                Text = initialValue ?? "",
                Font = new Font("맑은 고딕", 14F, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(_display);

            var table = new TableLayoutPanel
            {
                Location = new Point(12, 52),
                Size = new Size(256, 220),
                ColumnCount = 3,
                RowCount = 4,
                BackColor = Color.FromArgb(45, 45, 48)
            };
            for (int c = 0; c < 3; c++)
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            for (int r = 0; r < 4; r++)
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
            Controls.Add(table);

            string[,] keys =
            {
                { "7", "8", "9" },
                { "4", "5", "6" },
                { "1", "2", "3" },
                { "←", "0", "C" }
            };

            for (int r = 0; r < 4; r++)
            for (int c = 0; c < 3; c++)
            {
                var key = keys[r, c];
                var btn = new Button
                {
                    Text = key,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(4),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(62, 62, 66),
                    ForeColor = Color.White,
                    Font = new Font("맑은 고딕", 12F, FontStyle.Bold),
                    TabStop = false
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
                btn.Click += (_, __) => OnKey(key);
                table.Controls.Add(btn, c, r);
            }

            var flow = new FlowLayoutPanel
            {
                Location = new Point(12, 278),
                Size = new Size(256, 36),
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.FromArgb(45, 45, 48)
            };
            Controls.Add(flow);

            var btnOk = new Button
            {
                Text = "확인",
                Size = new Size(88, 32),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TabStop = true
            };
            btnOk.FlatAppearance.BorderColor = Color.FromArgb(0, 102, 170);

            var btnCancel = new Button
            {
                Text = "취소",
                Size = new Size(88, 32),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TabStop = true
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            flow.Controls.Add(btnOk);
            flow.Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private void OnKey(string key)
        {
            if (key == "←")
            {
                if (_display.TextLength > 0)
                    _display.Text = _display.Text.Substring(0, _display.TextLength - 1);
                return;
            }
            if (key == "C")
            {
                _display.Text = "";
                return;
            }
            if (key.Length == 1 && char.IsDigit(key[0]) && _display.TextLength < _maxDigits)
                _display.Text += key;
        }

        /// <summary>부모 창 앞에 모달로 띄우고, 확인 시 true와 결과 문자열을 반환합니다.</summary>
        public static bool TryShow(IWin32Window owner, string initial, out string result, int maxDigits = 12)
        {
            using (var f = new NumericKeypadForm(initial, maxDigits))
            {
                if (f.ShowDialog(owner) == DialogResult.OK)
                {
                    result = f.ResultText;
                    return true;
                }
                result = null;
                return false;
            }
        }
    }
}
