using System;
using System.Drawing;
using System.Windows.Forms;

namespace EMS_TEST_SIMULATOR
{
    /// <summary>터치 PC용 날짜 선택 팝업. 테스트 일자 클릭 시 표시.</summary>
    public class CalendarPopupForm : Form
    {
        private readonly TextBox _target;
        private MonthCalendar _calendar;
        private Button _btnOk;

        public CalendarPopupForm(TextBox target)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));

            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(45, 45, 48);
            Text = "달력";
            Size = new Size(900, 1000);
            MinimumSize = new Size(800, 900);
            KeyPreview = true;

            _calendar = new MonthCalendar
            {
                Location = new Point(12, 12),
                Font = new Font("맑은 고딕", 9F),
                BackColor = Color.White,
                ForeColor = Color.Black,
                MaxSelectionCount = 1,
                CalendarDimensions = new Size(4, 3)
            };
            int year = DateTime.Now.Year;
            if (!string.IsNullOrEmpty(_target.Text) && DateTime.TryParse(_target.Text, out var dt))
            {
                _calendar.SelectionStart = _calendar.SelectionEnd = dt;
                year = dt.Year;
            }
            _calendar.SetDate(new DateTime(year, 1, 1));

            _btnOk = new Button
            {
                Text = "적용",
                Size = new Size(100, 44),
                Location = new Point(Size.Width - 300, Size.Height - 200),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 11F)
            };
            _btnOk.FlatAppearance.BorderSize = 0;
            _btnOk.Click += BtnOk_Click;

            _calendar.DateSelected += (s, e) => { };

            Controls.Add(_calendar);
            Controls.Add(_btnOk);
            AcceptButton = _btnOk;

            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (_target.IsDisposed) return;
            _target.Text = _calendar.SelectionStart.ToString("yyyy-MM-dd");
            DialogResult = DialogResult.OK;
            Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // CalendarPopupForm
            // 
            this.ClientSize = new System.Drawing.Size(278, 244);
            this.Name = "CalendarPopupForm";
            this.Load += new System.EventHandler(this.CalendarPopupForm_Load);
            this.ResumeLayout(false);

        }

        private void CalendarPopupForm_Load(object sender, EventArgs e)
        {

        }
    }
}
