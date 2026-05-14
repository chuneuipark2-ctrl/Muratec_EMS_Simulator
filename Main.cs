using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using System.Security.Cryptography.X509Certificates;


namespace EMS_TEST_SIMULATOR
{
    public partial class Main : Form
    {

        public static Main Instance  = new Main();

        // [추가] 엔코더 설정을 전담할 매니저 생성
        public Encoder_Setting_Manager _encManager = new Encoder_Setting_Manager();
        public Encoder_Setting_Manager _EncManager => _encManager;

        private EMS_TCP_UDP_Connect SKY_RAV_CONNECT_FORM;
        public IDeviceComm GlobalComm; // 통신 객체를 담아둘 공용 보관함
        public EMS_Protocol GlobalEmsProto => (SKY_RAV_CONNECT_FORM != null && !SKY_RAV_CONNECT_FORM.IsDisposed) ? SKY_RAV_CONNECT_FORM.EmsProto : null;

        private System.Threading.CancellationTokenSource _autoCts;
        private bool _autoRunning = false;
        private bool _cycleStopRequestedByUser = false;
        /// <summary>EMO로 인해 AUTO가 즉시 중단된 경우, 완료 시 '사이클 정지 실패' 경고 표시용</summary>
        private bool _autoStoppedByEmo = false;

        /// <summary>EMS 상태 패널 실시간 갱신용 타이머 (UI 스레드에서만 라벨 갱신 → 스레드 안전)</summary>
        private System.Windows.Forms.Timer _emsStatusUpdateTimer;

        /// <summary>AUTO·EMO가 아닐 때 레일 5대 연결 상태 변화를 타워램프에 반영 (출력 스레드는 DO만 갱신)</summary>
        private System.Windows.Forms.Timer _towerIdleRefreshTimer;

        /// <summary>통신 로그: 최대 행 수 제한(초과 시 오래된 행 삭제), 뻑남 방지</summary>
        private const int _commLogMaxRows = 3000;
        private bool _commLogPaused = false;
        /// <summary>주요로그 에러 행 적시용: 직전 송신 데이터(내 프로그램)</summary>
        private string _lastSentHex = "";
        private string _lastSentDesc = "";

        /// <summary>EMO 버튼: 한번째 터치(초록띄+1번 시퀀스) / 두번째 터치(띄 제거+2번 시퀀스) 상태</summary>
        private bool _emoGreenBorderOn = false;

        /// <summary>AUTO 버튼: 클릭 시 초록 띄 토글만 (한번 누르면 띄, 두번째 누르면 띄 제거, 무한 반복)</summary>
        private bool _autoGreenBorderOn = false;

        /// <summary>제목(SKY-RAV…) 비밀번호 1 통과 시: AUTO에서 레일 8비트 인터록 생략, 1초 대기 후 H4 진행.</summary>
        public bool BypassRailInterlockForAuto { get; set; }

        /// <summary>EMO 눌려 있는 동안 기체 이상정지 H4(05) 주기 전송용 타이머. 해제 시 중지 후 H4(06) 1회 전송.</summary>
        private System.Windows.Forms.Timer _emoH4AbnormalStopTimer;

        /// <summary>pictureBox1 이미지 전환: 목적 동작 코드가 바뀔 때만 갱신해 GIF 재시작 방지. 초기값은 센티넬(처음 null 호출 시 정지 이미지 적용되도록).</summary>
        private string _lastTargetActionModeForPictureBox = "__INIT__";








        public Main()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyDarkTheme();
            FlatButtonPaintFix.ApplyToTree(this);
            // 기본은 EMS 정지. 목적 동작 코드 0/1/2 수신 시에만 이동/승강/하강 GIF 표시.
            if (pictureBox1 != null)
                pictureBox1.Image = Properties.Resources.EMS_정지;
        }

        public class ErrorItem
        {
            public string Code { get; set; }
            public string Name { get; set; }

            public ErrorItem(string code, string name)
            {
                Code = code;
                Name = name;
            }
        }


        public class ResponseCodeItems
        {
            public string Code { get; set; }
            public string Name { get; set; }

            public ResponseCodeItems(string code, string name)
            {
                Code = code;
                Name = name;
            }
        }

        public class target_operating_code_itmes
        {
            public string Code { get; set; }

            public string Name { get; set; }

            public target_operating_code_itmes(string code, string name)
            {
                Code = code;
                Name = name;
            }

        }

        public class Operating_Code_Items
        {
            public string Code { get; set; }
            public string Name { get; set; }

            public Operating_Code_Items(string code, string name)
            {
                Code = code;
                Name = name;
            }
        }





        // 테이블레이아웃 패널과 동일한 어두운 색으로 폼 본배경·탭·패널 전부 통일
        private static readonly Color _darkPanel = Color.FromArgb(62, 62, 66);

        private void Form1_Load(object sender, EventArgs e)
        {
            ResponseCodeList();//응답코드 리스트뷰
            taget_operating_code_list(); // 타겟동작 코드리스트
            Operating_Code_list(); // 동작 코드리스트
            UploadCodeList();//에러 리스트뷰
            _encManager.Initialize(this);

            ApplyDarkTheme();

            // 다크 테마: 엔코더 패널 라벨 흰색 Bold
            foreach (Control c in tableLayoutPanel3.Controls)
            {
                if (c is Label lb) { lb.ForeColor = Color.White; lb.Font = new Font("맑은 고딕", 9F, FontStyle.Bold); }
            }
            // 코드 리스트 ListView 다크 스타일
            foreach (System.Windows.Forms.ListView lv in new System.Windows.Forms.ListView[] { listView4, listView3, listView1, listView2 })
            {
                if (lv != null) { lv.BackColor = _darkPanel; lv.ForeColor = Color.White; lv.Font = new Font("맑은 고딕", 9F, FontStyle.Bold); }
            }

            // EMS 상태 패널 실시간 갱신
            _emsStatusUpdateTimer = new System.Windows.Forms.Timer();
            _emsStatusUpdateTimer.Interval = 300;
            _emsStatusUpdateTimer.Tick += (s, ev) => RefreshEmsStatusPanel();
            _emsStatusUpdateTimer.Start();

            TowerLamp.SetMode(TowerLampVisualMode.IdleRedSteady);
            _towerIdleRefreshTimer = new System.Windows.Forms.Timer();
            _towerIdleRefreshTimer.Interval = 1000;
            _towerIdleRefreshTimer.Tick += (s, ev) => RefreshTowerLampAfterAutoOrRail();
            _towerIdleRefreshTimer.Start();

            // ★ 통신 로그: 그리드 준비 + 정적 이벤트 구독 (Connect 폼 참조 없이 로그 수신)
            InitializeCommLogGrid();
            InitializeMainLog();
            CommLogBridge.OnLog += (dir, data, desc) => this.AddCommLog(dir, data, desc);
            FlatButtonPaintFix.ApplyToTree(this);
        }

        /// <summary>EMO 버튼 테두리: true = 초록색 띄, false = 테두리 제거.</summary>
        private void ApplyEmoButtonGreenBorder(bool green)
        {
            if (button8 == null) return;
            if (green)
            {
                button8.FlatAppearance.BorderSize = 4;
                button8.FlatAppearance.BorderColor = Color.Lime;
            }
            else
            {
                button8.FlatAppearance.BorderSize = 0;
            }
            button8.Invalidate();
        }

        /// <summary>AUTO 버튼 테두리: true = 초록색 띄, false = 테두리 제거. (기존 로직 무관)</summary>
        private void ApplyAutoButtonGreenBorder(bool green)
        {
            if (button7 == null) return;
            if (green)
            {
                button7.FlatAppearance.BorderSize = 4;
                button7.FlatAppearance.BorderColor = Color.Lime;
            }
            else
            {
                button7.FlatAppearance.BorderSize = 0;
            }
            button7.Invalidate();
        }

        private const int _mainLogMaxItems = 1000;

        /// <summary>주요로그(ListView): 에러 시에만 기록. EMS 상태보고 응답코드 != 00일 때 직전 송신+수신 데이터·해석 적시.</summary>
        private void InitializeMainLog()
        {
            if (event_log_listview == null) return;
            event_log_listview.View = View.Details;
            event_log_listview.FullRowSelect = true;
            if (event_log_listview.Columns.Count == 0)
            {
                event_log_listview.Columns.Add("시간", 90);
                event_log_listview.Columns.Add("직전 송신(내 프로그램)", 220);
                event_log_listview.Columns.Add("수신 데이터(EMS)", 220);
                event_log_listview.Columns.Add("해석", 200);
            }
            event_log_listview.OwnerDraw = true;
            event_log_listview.DrawColumnHeader += Event_log_listview_DrawColumnHeader;
            event_log_listview.DrawSubItem += Event_log_listview_DrawSubItem;
            event_log_listview.DrawItem += Event_log_listview_DrawItem;
        }

        private void Event_log_listview_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawBackground();
            using (var sf = new StringFormat { Alignment = StringAlignment.Near })
            using (var br = new SolidBrush(Color.White))
                e.Graphics.DrawString(e.Header.Text, e.Font, br, e.Bounds, sf);
        }

        private void Event_log_listview_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            bool isError = e.Item.Tag is bool b && b;
            Color fore = isError ? Color.Red : Color.White;
            e.DrawBackground();
            using (var br = new SolidBrush(fore))
                e.Graphics.DrawString(e.Item?.Text ?? "", e.Item.Font ?? event_log_listview.Font, br, e.Bounds, StringFormat.GenericDefault);
        }

        private void Event_log_listview_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            bool isError = e.Item.Tag is bool b && b;
            Color fore = isError ? Color.Red : Color.White;
            e.DrawBackground();
            using (var br = new SolidBrush(fore))
                e.Graphics.DrawString(e.SubItem?.Text ?? "", e.Item.Font ?? event_log_listview.Font, br, e.Bounds, StringFormat.GenericDefault);
        }

        /// <summary>EMS 상태 응답 형식인지 (@TXT240 + body 26자 이상).</summary>
        private static bool IsEmsStatusResponse(byte[] data)
        {
            if (data == null || data.Length < 38) return false;
            try
            {
                string res = Encoding.ASCII.GetString(data);
                int idx = res.IndexOf("@TXT240");
                if (idx < 0) return false;
                string body = res.Substring(idx + 12);
                return body.Length >= 26;
            }
            catch { return false; }
        }

        /// <summary>EMS 상태 응답에서 응답코드(ResponseCode)가 00이 아니면 에러.</summary>
        private static bool IsEmsStatusResponseError(byte[] data)
        {
            if (data == null || data.Length < 38) return false;
            try
            {
                string res = Encoding.ASCII.GetString(data);
                int idx = res.IndexOf("@TXT240");
                if (idx < 0) return false;
                string body = res.Substring(idx + 12);
                if (body.Length < 2) return false;
                string responseCode = body.Substring(0, 2);
                return responseCode != "00";
            }
            catch { return false; }
        }

        /// <summary>EMS 수신 데이터에서 응답코드 추출 후 "응답코드 XX 에러" 문자열 반환.</summary>
        private static string GetEmsResponseCodeErrorDesc(byte[] data)
        {
            if (data == null || data.Length < 38) return "응답코드 ??: 에러";
            try
            {
                string res = Encoding.ASCII.GetString(data);
                int idx = res.IndexOf("@TXT240");
                if (idx < 0) return "응답코드 ??: 에러";
                string body = res.Substring(idx + 12);
                if (body.Length < 2) return "응답코드 ??: 에러";
                string responseCode = body.Substring(0, 2);
                return "응답코드 " + responseCode + " 에러";
            }
            catch { return "응답코드 ??: 에러"; }
        }

        /// <summary>통신 로그 그리드 초기화 (Form1_Load에서 호출). 컬럼 구성·버튼 연결·표시 설정.</summary>
        private void InitializeCommLogGrid()
        {
            if (dgvCommLog == null) return;
            if (dgvCommLog.Columns.Count == 0)
            {
                dgvCommLog.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTime", HeaderText = "시간", Width = 90, ReadOnly = true });
                dgvCommLog.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDirection", HeaderText = "방향", Width = 60, ReadOnly = true });
                dgvCommLog.Columns.Add(new DataGridViewTextBoxColumn { Name = "colData", HeaderText = "데이터(HEX)", Width = 220, ReadOnly = true });
                dgvCommLog.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDesc", HeaderText = "해석", Width = 200, ReadOnly = true });
            }
            dgvCommLog.Visible = true;
            dgvCommLog.BringToFront();
            btn_log_stop.Click += Btn_log_stop_Click;
            btn_log_save.Click += Btn_log_save_Click;
            if (Communication_log != null)
            {
                Communication_log.SelectedIndexChanged += (s, ev) => { if (dgvCommLog != null && !dgvCommLog.IsDisposed) dgvCommLog.BringToFront(); };
                if (Communication_log.TabCount > 1) Communication_log.SelectedIndex = 1;
            }
        }

        private void Btn_log_stop_Click(object sender, EventArgs e)
        {
            _commLogPaused = !_commLogPaused;
            btn_log_stop.Text = _commLogPaused ? "Resume" : "Stop";
        }

        private void Btn_log_save_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "CSV 파일|*.csv|텍스트 파일|*.txt|모든 파일|*.*";
                dlg.DefaultExt = "csv";
                dlg.FileName = $"CommLog_{DateTime.Now:yyyyMMdd_HHmmss}";
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("시간\t방향\t데이터(HEX)\t해석");
                    if (dgvCommLog.Rows != null)
                        foreach (DataGridViewRow row in dgvCommLog.Rows)
                        {
                            if (row.IsNewRow) continue;
                            var c = row.Cells;
                            sb.AppendLine($"{(c.Count > 0 ? c[0].Value : "")}\t{(c.Count > 1 ? c[1].Value : "")}\t{(c.Count > 2 ? c[2].Value : "")}\t{(c.Count > 3 ? c[3].Value : "")}");
                        }
                    System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("저장 완료: " + dlg.FileName);
                }
                catch (Exception ex) { MessageBox.Show("저장 실패: " + ex.Message); }
            }
        }

        /// <summary>통신 로그 추가. 통신 스레드에서 호출 가능(Invoke로 UI 스레드에서 갱신). 최대 행 수 초과 시 오래된 행 삭제.</summary>
        public void AddCommLog(string direction, byte[] data, string description)
        {
            if (data == null) data = Array.Empty<byte>();
            string time = DateTime.Now.ToString("HH:mm:ss.fff");
            string hex = BitConverter.ToString(data).Replace("-", " ");
            if (hex.Length > 200) hex = hex.Substring(0, 200) + "...";
            string desc = description ?? "";
            void DoAdd()
            {
                if (_commLogPaused) return;
                if (dgvCommLog == null || dgvCommLog.IsDisposed) return;
                if (dgvCommLog.Columns.Count < 4)
                {
                    try
                    {
                        dgvCommLog.Columns.Clear();
                        dgvCommLog.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTime", HeaderText = "시간", Width = 90, ReadOnly = true });
                        dgvCommLog.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDirection", HeaderText = "방향", Width = 60, ReadOnly = true });
                        dgvCommLog.Columns.Add(new DataGridViewTextBoxColumn { Name = "colData", HeaderText = "데이터(HEX)", Width = 220, ReadOnly = true });
                        dgvCommLog.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDesc", HeaderText = "해석", Width = 200, ReadOnly = true });
                    }
                    catch { return; }
                }
                try
                {
                    dgvCommLog.Rows.Add(time, direction, hex, desc);
                    while (dgvCommLog.Rows.Count > _commLogMaxRows && dgvCommLog.Rows.Count > 0)
                        dgvCommLog.Rows.RemoveAt(0);
                }
                catch { }
                if (direction == "송신")
                {
                    _lastSentHex = hex;
                    _lastSentDesc = desc;
                }
                bool isEmsError = direction == "수신" && IsEmsStatusResponse(data) && IsEmsStatusResponseError(data);
                if (isEmsError && event_log_listview != null && !event_log_listview.IsDisposed)
                {
                    try
                    {
                        string errorDesc = GetEmsResponseCodeErrorDesc(data);
                        var li = new ListViewItem(time);
                        li.SubItems.Add(string.IsNullOrEmpty(_lastSentHex) ? "-" : _lastSentHex);
                        li.SubItems.Add(hex);
                        li.SubItems.Add(errorDesc);
                        li.Tag = true;
                        event_log_listview.Items.Add(li);
                        while (event_log_listview.Items.Count > _mainLogMaxItems)
                            event_log_listview.Items.RemoveAt(0);
                    }
                    catch { }
                }
            }
            try
            {
                if (InvokeRequired)
                    BeginInvoke(new Action(DoAdd));
                else
                    DoAdd();
            }
            catch { }
        }

        /// <summary>EMS 수신 상태를 상태 패널 라벨에 반영. 타이머 Tick에서만 호출되므로 UI 스레드에서 실행됨.</summary>
        private void RefreshEmsStatusPanel()
        {
            var status = GlobalEmsProto?.Parser?.CurrentStatus;
            string na = "-";
            if (status == null)
            {
                lbl_response_code_text.Text = na;
                lbl_operating_code_text.Text = na;
                lbl_error_code_text.Text = na;
                lbl_transfer_data_no_text.Text = na;
                lbl_target_mode_text.Text = na;
                lbl_vehicle_mode_text.Text = na;
                lbl_current_point_text.Text = na;
                lbl_load_status_text.Text = na;
                lbl_transfer_command_Received_text.Text = na;
                lbl_target_point_text.Text = na;
                lbl_error_point_text.Text = na;
                SetPictureBox1ByTargetActionMode(null);
                return;
            }
            lbl_response_code_text.Text = string.IsNullOrEmpty(status.ResponseCode) ? na : status.ResponseCode;
            lbl_operating_code_text.Text = string.IsNullOrEmpty(status.ActionCode) ? na : status.ActionCode;
            lbl_error_code_text.Text = string.IsNullOrEmpty(status.ErrorCode) ? na : status.ErrorCode;
            lbl_transfer_data_no_text.Text = string.IsNullOrEmpty(status.TransferDataNo) ? na : status.TransferDataNo;
            lbl_target_mode_text.Text = string.IsNullOrEmpty(status.TargetActionMode) ? na : status.TargetActionMode;
            lbl_vehicle_mode_text.Text = FormatMachineMode(status.MachineMode);
            lbl_current_point_text.Text = string.IsNullOrEmpty(status.CurrentSectionCount) ? na : status.CurrentSectionCount;
            lbl_load_status_text.Text = FormatCargoStatus(status.CargoStatus);
            lbl_transfer_command_Received_text.Text = FormatCommandAcceptStatus(status.CommandAcceptStatus);
            lbl_target_point_text.Text = string.IsNullOrEmpty(status.TargetSectionCount) ? na : status.TargetSectionCount;
            lbl_error_point_text.Text = string.IsNullOrEmpty(status.ErrorSectionCount) ? na : status.ErrorSectionCount;
            SetPictureBox1ByTargetActionMode(status.TargetActionMode);
        }

        /// <summary>목적 동작 모드에 따라 pictureBox1(EMS 현재상태) 이미지 전환. 0=이동/주행, 1=01.승강 EMS, 2=02.하강 EMS. 코드가 바뀔 때만 갱신해 GIF가 매 틱마다 처음부터 재생되지 않도록 함.</summary>
        private void SetPictureBox1ByTargetActionMode(string targetActionMode)
        {
            if (pictureBox1 == null) return;
            if (targetActionMode == _lastTargetActionModeForPictureBox) return;
            _lastTargetActionModeForPictureBox = targetActionMode;

            Image img;
            switch (targetActionMode)
            {
                case "0": img = Properties.Resources._02__레이아웃; break;   // 이동(주행)
                case "1": img = Properties.Resources._01__승강_EMS; break;   // 01. 승강 EMS (탑재)
                case "2": img = Properties.Resources._02__하강_EMS; break;   // 02. 하강 EMS (이재)
                default: img = Properties.Resources.EMS_정지; break;          // 이동/탑재/이재 아님 → EMS 정지
            }
            pictureBox1.Image = img;
        }

        private static string FormatMachineMode(string value)
        {
            if (string.IsNullOrEmpty(value)) return "-";
            if (value == "1") return "수동";
            if (value == "2") return "자동";
            return value;
        }

        private static string FormatCargoStatus(string value)
        {
            if (string.IsNullOrEmpty(value)) return "-";
            if (value == "0") return "없음";
            if (value == "1") return "있음";
            return value;
        }

        private static string FormatCommandAcceptStatus(string value)
        {
            if (string.IsNullOrEmpty(value)) return "-";
            if (value == "0") return "불가";
            if (value == "1") return "가능";
            return value;
        }

        /// <summary>테이블레이아웃 패널과 같은 어두운 색(62,62,66)을 폼 본배경·탭·패널 전부에 적용</summary>
        private void ApplyDarkTheme()
        {
            this.BackColor = _darkPanel;
            if (tableLayoutPanel2 != null) tableLayoutPanel2.BackColor = _darkPanel;
            if (tabControl1 != null) { tabControl1.BackColor = _darkPanel; tabControl1.ForeColor = Color.White; }
            if (tabControl2 != null) { tabControl2.BackColor = _darkPanel; tabControl2.ForeColor = Color.White; }
            if (tabControl3 != null) { tabControl3.BackColor = _darkPanel; tabControl3.ForeColor = Color.White; }
            if (Communication_log != null) Communication_log.BackColor = _darkPanel;
            var darkPages = new TabPage[] { 통신로그, 코드리스트, 엔코더설정, tabPage1, tabPage2, tabPage3, tabPage4, 응답코드, 목적동작코드, 동작코드, 에러코드 };
            foreach (var p in darkPages)
                if (p != null) { p.BackColor = _darkPanel; p.UseVisualStyleBackColor = false; }
            if (tableLayoutPanel1 != null) tableLayoutPanel1.BackColor = _darkPanel;
            if (tableLayoutPanel3 != null) tableLayoutPanel3.BackColor = _darkPanel;
            if (tBox1 != null) { tBox1.BackColor = _darkPanel; tBox1.ForeColor = Color.White; }
            if (dgvCommLog != null) { dgvCommLog.BackgroundColor = _darkPanel; dgvCommLog.DefaultCellStyle = new DataGridViewCellStyle { BackColor = _darkPanel, ForeColor = Color.White }; dgvCommLog.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(45, 45, 48), ForeColor = Color.White }; }
            if (event_log_listview != null) { event_log_listview.BackColor = _darkPanel; event_log_listview.ForeColor = Color.White; }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void  Rail_io_Click(object sender, EventArgs e)
        {
            // 1. 연결 시도
            bool isAllConnected = Rail_DIO.Instance.FASTECH_CONNECT();

            // 2. 텍스트박스 초기화
            tBox1.Clear();

            if (isAllConnected)
            {
                UpdateRailButtonStatus(true);
                tBox1.AppendText("모든 장비 연결 성공 (192.168.0.20 ~ 24)" + Environment.NewLine);

                if (!_autoRunning && !_emoGreenBorderOn)
                    TowerLamp.SetMode(TowerLampVisualMode.RailAllOkBlueSteady);

                PANEL_DIO panel = new PANEL_DIO();
                panel.Show();
            }
            else
            {
                UpdateRailButtonStatus(false);
                Rail_io.BackColor = Color.Orange;
                FlatButtonPaintFix.ApplyToButton(Rail_io);

                PANEL_DIO panel = new PANEL_DIO(); //임시사용
                panel.Show();

                // 3. 에러가 난 IP들만 골라서 텍스트박스에 표기
                tBox1.AppendText("--- 연결 실패 목록 ---" + Environment.NewLine);
                foreach (string errorIp in Rail_DIO.Instance.ErrorIPs)
                {
                    tBox1.AppendText($"[실패] {errorIp}" + Environment.NewLine);
                }

                if (!_autoRunning && !_emoGreenBorderOn)
                {
                    if (Rail_DIO.Instance.IsAnySlaveConnected())
                        TowerLamp.SetMode(TowerLampVisualMode.RailErrorRedBlink);
                    else
                        TowerLamp.SetMode(TowerLampVisualMode.IdleRedSteady);
                }

                MessageBox.Show("일부 장비 연결에 실패했습니다. 텍스트박스를 확인하세요.");
            }

        }

        /// <summary>
        /// AUTO 가동 중이 아닐 때 레일·EMO 상태에 맞춰 타워램프 논리 모드 갱신.
        /// EMS 동작이상 등으로 적색 상시(EmsFaultRedSteady)인 경우 덮어쓰지 않음(해제: AUTO 재시도 등 SetMode).
        /// </summary>
        public void RefreshTowerLampAfterAutoOrRail()
        {
            if (_autoRunning) return;
            if (TowerLamp.GetMode() == TowerLampVisualMode.EmsFaultRedSteady) return;
            if (_emoGreenBorderOn)
            {
                TowerLamp.SetMode(TowerLampVisualMode.EmoRedBlink);
                return;
            }
            if (Rail_DIO.Instance.AreAllFiveSlavesConnected())
                TowerLamp.SetMode(TowerLampVisualMode.RailAllOkBlueSteady);
            else if (Rail_DIO.Instance.IsAnySlaveConnected())
                TowerLamp.SetMode(TowerLampVisualMode.RailErrorRedBlink);
            else
                TowerLamp.SetMode(TowerLampVisualMode.IdleRedSteady);
        }

        public void UpdateConnectionStatus(bool connected)
        {
            if (connected)
            {
                Rail_io.BackColor = Color.Lime;
                FlatButtonPaintFix.ApplyToButton(Rail_io);
            }
            else
            {
                Rail_io.BackColor = Color.FromArgb(37, 99, 235); // 끊김 시 파란색(기본 버튼색) 복귀
                Rail_io.ForeColor = Color.White;
                FlatButtonPaintFix.ApplyToButton(Rail_io);
            }
        }


        public void UpdateRailButtonStatus(bool isConnected)
        {
            // UI 스레드 안전성 검사 (다른 스레드에서 호출할 경우를 대비)
            if (Rail_io.InvokeRequired)
            {
                Rail_io.Invoke(new Action(() => UpdateRailButtonStatus(isConnected)));
                return;
            }

            if (isConnected)
            {
                // 연결 성공 시: 녹색
                Rail_io.BackColor = Color.Lime;
                FlatButtonPaintFix.ApplyToButton(Rail_io);
            }
            else
            {
                // 연결 실패/해제 시: 파란색(기본 버튼색) 복귀
                Rail_io.BackColor = Color.FromArgb(37, 99, 235);
                Rail_io.ForeColor = Color.White;
                FlatButtonPaintFix.ApplyToButton(Rail_io);
            }
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void elementHost1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void lbl_response_code_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_operating_code_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_error_code_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_transfer_data_no_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_vehicle_mode_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_load_status_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_transfer_command_Received_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_target_mode_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_current_point_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_target_point_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_error_point_text_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //에러코드 추가

        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //에러코드 불러들이는 부분
         }


        private void UploadCodeList()
        {
            // 1. 리스트뷰 설정 초기화 (디자인 타임에서 안 했을 경우를 대비)
            listView2.View = View.Details;
            listView2.GridLines = true;
            listView2.FullRowSelect = true;

            // 2. 99개의 데이터를 담을 리스트 (예시 데이터 3개만 적었지만 99개까지 추가 가능)
            List<ErrorItem> errorList = new List<ErrorItem>();
            errorList.Add(new ErrorItem("01", "주행 인버터 이상(주행중)"));
            errorList.Add(new ErrorItem("02", "팬모터 이상"));
            errorList.Add(new ErrorItem("03", "인버터 운전중 신호이상(주행중)"));
            errorList.Add(new ErrorItem("04", "인버터 운전중 신호출력이상(주행중)"));
            errorList.Add(new ErrorItem("05", "-"));
            errorList.Add(new ErrorItem("06", "이재 모터 과부하(주행중)"));
            errorList.Add(new ErrorItem("07", "주행 모터 과부하"));
            errorList.Add(new ErrorItem("08", "인버터 주파수검출 ON 대기이상/승강타이밍벨트 이상(주행중)"));
            errorList.Add(new ErrorItem("09", "인버터 주파수검출 OFF 대기이상/센서네트 경보(주행중) "));
            errorList.Add(new ErrorItem("10", "인버터 이상(승강중)"));
            errorList.Add(new ErrorItem("11", "하강 FINAL 이상"));
            errorList.Add(new ErrorItem("12", "하강 원점 확인 이상"));
            errorList.Add(new ErrorItem("13", "인버터 운전중 신호 이상(승강중)"));
            errorList.Add(new ErrorItem("14", "상승 원점 확인 이상"));
            errorList.Add(new ErrorItem("15", "엔코더 이상"));
            errorList.Add(new ErrorItem("16", "CHUCK 모터 과부하(승강중)"));
            errorList.Add(new ErrorItem("17", "이재 모터 과부하(승강중)"));
            errorList.Add(new ErrorItem("18", "승강 타이밍 벨트 이상(승강중)"));
            errorList.Add(new ErrorItem("19", "SENSOR NET 이상(승강중)"));
            errorList.Add(new ErrorItem("20", "승강 원점 이상(주행중)"));
            errorList.Add(new ErrorItem("21", "카운트 미스"));
            errorList.Add(new ErrorItem("22", "CHUCK 열림 이상(주행중)"));
            errorList.Add(new ErrorItem("23", "CHUCK 닫힘 이상(주행중)"));
            errorList.Add(new ErrorItem("24", "CHUCK 화물 있을 때 열림 이상(주행중)"));
            errorList.Add(new ErrorItem("25", "CHUCK 화물 있을 떄 닫힘 이상(주행중)"));
            errorList.Add(new ErrorItem("26", "화물 적재 이상(주행중)"));

            errorList.Add(new ErrorItem("27", "화물 돌출 감지 이상(주행중)"));
            errorList.Add(new ErrorItem("28", "교신점 확인 이상"));
            errorList.Add(new ErrorItem("29", "CHUCK 개폐 리미트 스위치 이상"));
            errorList.Add(new ErrorItem("30", "승강대 원점 이상(승강 에러 복구시)"));
            errorList.Add(new ErrorItem("31", "CHUCK 열림 이상(승강중)"));
            errorList.Add(new ErrorItem("32", "CHUCK 닫힘 이상(승강중)"));
            errorList.Add(new ErrorItem("33", "화물 CATCH 이상"));
            errorList.Add(new ErrorItem("34", "화물 UNCATCH 이상"));
            errorList.Add(new ErrorItem("35", "화물 있음 이상"));
            errorList.Add(new ErrorItem("36", "화물 없음 이상"));
            errorList.Add(new ErrorItem("37", "화물 돌출 감지 이상(승강중)"));
            errorList.Add(new ErrorItem("38", "승강대 적재 이상(승강중)"));
            errorList.Add(new ErrorItem("39", "선입품 이상"));
            errorList.Add(new ErrorItem("40", "패리티 이상"));
            errorList.Add(new ErrorItem("41", "정지 미스"));
            errorList.Add(new ErrorItem("42", "주행중 전원 차단 이상"));
            errorList.Add(new ErrorItem("43", "수동 절환"));
            errorList.Add(new ErrorItem("44", "하강 정지위치 결정 센서 에러"));
            errorList.Add(new ErrorItem("45", "승강중 정지 미스"));
            errorList.Add(new ErrorItem("46", "승강중 전원 차단 이상"));
            errorList.Add(new ErrorItem("47", "승강 위치 결정 이상"));
            errorList.Add(new ErrorItem("48", "승강대 화물 걸림 이상"));
            errorList.Add(new ErrorItem("49", "선입품 AREA 센서 이상"));
            errorList.Add(new ErrorItem("50", "정위치 확인 입력 없음 이상"));
            errorList.Add(new ErrorItem("51", "정위치 확인 복수 입력 이상"));
            errorList.Add(new ErrorItem("52", "정위치 확인 데이터 없음 이상"));
            errorList.Add(new ErrorItem("53", "주행중 수신 감시 이상"));

            errorList.Add(new ErrorItem("54", "주행중 이상정지 지시"));
            errorList.Add(new ErrorItem("55", "승강중 수신 감시 이상"));
            errorList.Add(new ErrorItem("56", "이재중 이상정지 지시"));
            errorList.Add(new ErrorItem("57", "화물 탑재 가능 시간 초과 보고"));
            errorList.Add(new ErrorItem("58", "화물 이재 가능 시간 초과 보고"));
            errorList.Add(new ErrorItem("59", "선입품 이상 보고"));
            errorList.Add(new ErrorItem("60", "화물 탑재 주행 시작시 CHUCK 상태 이상"));
            errorList.Add(new ErrorItem("61", "화물 이재 주행 시작시 CHUCK 상태 이상"));
            errorList.Add(new ErrorItem("62", "-"));
            errorList.Add(new ErrorItem("63", "-"));
            errorList.Add(new ErrorItem("64", "화물 탑재 주행 시작시 적재상태 이상"));
            errorList.Add(new ErrorItem("65", "화물 이재 주행 시작시 적재상태 이상"));
            errorList.Add(new ErrorItem("66", "인터록 미검출 이상"));
            errorList.Add(new ErrorItem("67", "승강중 SET 가능 신호 이상"));
            errorList.Add(new ErrorItem("68", "반송지령 패리티 이상"));
            errorList.Add(new ErrorItem("69", "반송지령 데이터 이상"));
            errorList.Add(new ErrorItem("70", "승강 인터록 시간 초과"));
            errorList.Add(new ErrorItem("71", "컨베이어 이재 시간 초과"));
            errorList.Add(new ErrorItem("72", "주행 시간 초과"));
            errorList.Add(new ErrorItem("73", "CHUCK 동작 시간 초과"));
            errorList.Add(new ErrorItem("74", "교신 스트로브 OFF 시간 초과"));
            errorList.Add(new ErrorItem("75", "자동 주행 운전 가능 시간 초과"));
            errorList.Add(new ErrorItem("76", "이재 시간 초과"));
            errorList.Add(new ErrorItem("77", "상승 간으 시간 초과"));
            errorList.Add(new ErrorItem("78", "초기 주행 발진 가능 시간 초과"));

            errorList.Add(new ErrorItem("79", "반송개시 시간 초과"));
            errorList.Add(new ErrorItem("80", "-"));
            errorList.Add(new ErrorItem("81", "섹션 카운트 에러"));
            errorList.Add(new ErrorItem("82", "동작 모드 에러"));
            errorList.Add(new ErrorItem("83", "반송지령 수신 불가 이상"));
            errorList.Add(new ErrorItem("84", "이재 에러"));
            errorList.Add(new ErrorItem("85", "데이터 번호 중복 에러"));
            errorList.Add(new ErrorItem("86", "전문 ID 에러"));
            errorList.Add(new ErrorItem("87", "전문 길이 에러"));
            errorList.Add(new ErrorItem("88", "하드웨어 에러"));
            errorList.Add(new ErrorItem("89", "BCC 에러"));
            errorList.Add(new ErrorItem("90", "-"));
            errorList.Add(new ErrorItem("91", "프로그램 섬 체크 이상"));
            errorList.Add(new ErrorItem("92", "플래시 메모리 에러"));
            errorList.Add(new ErrorItem("93", "시스템 파라미터 섬 체크 에러"));
            errorList.Add(new ErrorItem("94", "워치독 이상"));
            errorList.Add(new ErrorItem("95", "순시 정전 이상"));
            errorList.Add(new ErrorItem("96", "예외 처리 이상"));
            errorList.Add(new ErrorItem("97", "주행 구동 금지 이상"));
            errorList.Add(new ErrorItem("98", "구동 금지 승강 이상"));
            errorList.Add(new ErrorItem("99", "시퀀스 번호 이상"));



            // ... 나머지 99개까지 .Add 하시면 됩니다.

            // 3. ListView 업데이트 속도 향상을 위해 BeginUpdate 사용
            listView2.BeginUpdate();
            listView2.Items.Clear(); // 기존 데이터 삭제

            // 4. 반복문(foreach)을 돌며 ListView에 아이템 업로드
            foreach (var item in errorList)
            {
                // 첫 번째 컬럼(코드) 생성
                ListViewItem lvi = new ListViewItem(item.Code);
                // 두 번째 컬럼(명칭) 추가
                lvi.SubItems.Add(item.Name);

                // 최종 업로드
                listView2.Items.Add(lvi);
            }

            listView2.EndUpdate(); // 업데이트 종료 및 화면 갱신

        }

        private void listView4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void ResponseCodeList()
        {
            listView1.View = View.Details; // 표형태의 보기를 의미
            listView1.GridLines = true; // 그리드 입력
            listView1.FullRowSelect = true; //사용자가 행 선택가능

            listView1.Items.Clear();

            //메모리내 데이터 구조화 작업//
            // ResponseCodeItem이라는 사용자 정의 클래스(또는 구조체)를 객체화 하여 리스트에 담는다.
            // UI에 직접 넣기 전에 메모리(list<T>)에서 먼저 데이터를 관리하는 것이 유지보수에 유리하다.

            List<ResponseCodeItems> responseCodeItems = new List<ResponseCodeItems>();
            responseCodeItems.Add(new ResponseCodeItems("00","정상 수신 완료"));
            responseCodeItems.Add(new ResponseCodeItems("01", "Header 미검출"));
            responseCodeItems.Add(new ResponseCodeItems("02", "-"));
            responseCodeItems.Add(new ResponseCodeItems("03", "수신 문자 초과"));
            responseCodeItems.Add(new ResponseCodeItems("04", "체크섬 에러"));
            responseCodeItems.Add(new ResponseCodeItems("05", "하드 에러"));
            responseCodeItems.Add(new ResponseCodeItems("06", "-"));
            responseCodeItems.Add(new ResponseCodeItems("07", "-"));
            responseCodeItems.Add(new ResponseCodeItems("08", "수신제 에러"));
            responseCodeItems.Add(new ResponseCodeItems("09", "시퀀스 순서 에러"));

            responseCodeItems.Add(new ResponseCodeItems("10", "-"));
            responseCodeItems.Add(new ResponseCodeItems("11", "-"));
            responseCodeItems.Add(new ResponseCodeItems("12", "-"));
            responseCodeItems.Add(new ResponseCodeItems("13", "-"));
            responseCodeItems.Add(new ResponseCodeItems("14", "-"));
            responseCodeItems.Add(new ResponseCodeItems("15", "-"));
            responseCodeItems.Add(new ResponseCodeItems("16", "-"));
            responseCodeItems.Add(new ResponseCodeItems("17", "-"));
            responseCodeItems.Add(new ResponseCodeItems("18", "-"));
            responseCodeItems.Add(new ResponseCodeItems("19", "-"));

            responseCodeItems.Add(new ResponseCodeItems("20", "-"));
            responseCodeItems.Add(new ResponseCodeItems("21", "-"));
            responseCodeItems.Add(new ResponseCodeItems("22", "-"));
            responseCodeItems.Add(new ResponseCodeItems("23", "-"));
            responseCodeItems.Add(new ResponseCodeItems("24", "-"));
            responseCodeItems.Add(new ResponseCodeItems("25", "-"));
            responseCodeItems.Add(new ResponseCodeItems("26", "-"));
            responseCodeItems.Add(new ResponseCodeItems("27", "-"));
            responseCodeItems.Add(new ResponseCodeItems("28", "-"));
            responseCodeItems.Add(new ResponseCodeItems("29", "-"));

            responseCodeItems.Add(new ResponseCodeItems("30", "-"));
            responseCodeItems.Add(new ResponseCodeItems("31", "-"));
            responseCodeItems.Add(new ResponseCodeItems("32", "-"));
            responseCodeItems.Add(new ResponseCodeItems("33", "-"));
            responseCodeItems.Add(new ResponseCodeItems("34", "-"));
            responseCodeItems.Add(new ResponseCodeItems("35", "-"));
            responseCodeItems.Add(new ResponseCodeItems("36", "-"));
            responseCodeItems.Add(new ResponseCodeItems("37", "-"));
            responseCodeItems.Add(new ResponseCodeItems("38", "-"));
            responseCodeItems.Add(new ResponseCodeItems("39", "-"));

            responseCodeItems.Add(new ResponseCodeItems("40", "ID 에러"));
            responseCodeItems.Add(new ResponseCodeItems("41", "전문 길이 에러"));
            responseCodeItems.Add(new ResponseCodeItems("42", "포인트 No. 에러"));
            responseCodeItems.Add(new ResponseCodeItems("43", "-"));
            responseCodeItems.Add(new ResponseCodeItems("44", "현재 동작중 에러"));
            responseCodeItems.Add(new ResponseCodeItems("45", "이재 포인트 에러"));
            responseCodeItems.Add(new ResponseCodeItems("46", "지시모드 에러"));
            responseCodeItems.Add(new ResponseCodeItems("47", "데이터 취소 이상"));
            responseCodeItems.Add(new ResponseCodeItems("48", "초기주행중 이상"));
            responseCodeItems.Add(new ResponseCodeItems("49", "수동중 이상"));

            responseCodeItems.Add(new ResponseCodeItems("50", "현재 분기중 이상"));
            responseCodeItems.Add(new ResponseCodeItems("51", "-"));
            responseCodeItems.Add(new ResponseCodeItems("52", "-"));
            responseCodeItems.Add(new ResponseCodeItems("53", "-"));
            responseCodeItems.Add(new ResponseCodeItems("54", "-"));
            responseCodeItems.Add(new ResponseCodeItems("55", "-"));
            responseCodeItems.Add(new ResponseCodeItems("56", "-"));
            responseCodeItems.Add(new ResponseCodeItems("57", "-"));
            responseCodeItems.Add(new ResponseCodeItems("58", "-"));
            responseCodeItems.Add(new ResponseCodeItems("59", "-"));

            responseCodeItems.Add(new ResponseCodeItems("60", "-"));
            responseCodeItems.Add(new ResponseCodeItems("61", "-"));
            responseCodeItems.Add(new ResponseCodeItems("62", "-"));
            responseCodeItems.Add(new ResponseCodeItems("63", "-"));
            responseCodeItems.Add(new ResponseCodeItems("64", "-"));
            responseCodeItems.Add(new ResponseCodeItems("65", "-"));
            responseCodeItems.Add(new ResponseCodeItems("66", "-"));
            responseCodeItems.Add(new ResponseCodeItems("67", "-"));
            responseCodeItems.Add(new ResponseCodeItems("68", "-"));
            responseCodeItems.Add(new ResponseCodeItems("69", "-"));

            responseCodeItems.Add(new ResponseCodeItems("70", "-"));
            responseCodeItems.Add(new ResponseCodeItems("71", "-"));
            responseCodeItems.Add(new ResponseCodeItems("72", "-"));
            responseCodeItems.Add(new ResponseCodeItems("73", "-"));
            responseCodeItems.Add(new ResponseCodeItems("74", "-"));
            responseCodeItems.Add(new ResponseCodeItems("75", "-"));
            responseCodeItems.Add(new ResponseCodeItems("76", "-"));
            responseCodeItems.Add(new ResponseCodeItems("77", "-"));
            responseCodeItems.Add(new ResponseCodeItems("78", "-"));
            responseCodeItems.Add(new ResponseCodeItems("79", "-"));

            responseCodeItems.Add(new ResponseCodeItems("80", "-"));
            responseCodeItems.Add(new ResponseCodeItems("81", "-"));
            responseCodeItems.Add(new ResponseCodeItems("82", "-"));
            responseCodeItems.Add(new ResponseCodeItems("83", "-"));
            responseCodeItems.Add(new ResponseCodeItems("84", "-"));
            responseCodeItems.Add(new ResponseCodeItems("85", "-"));
            responseCodeItems.Add(new ResponseCodeItems("86", "-"));
            responseCodeItems.Add(new ResponseCodeItems("87", "-"));
            responseCodeItems.Add(new ResponseCodeItems("88", "-"));
            responseCodeItems.Add(new ResponseCodeItems("89", "-"));

            responseCodeItems.Add(new ResponseCodeItems("90", "-"));
            responseCodeItems.Add(new ResponseCodeItems("91", "-"));
            responseCodeItems.Add(new ResponseCodeItems("92", "-"));
            responseCodeItems.Add(new ResponseCodeItems("93", "-"));
            responseCodeItems.Add(new ResponseCodeItems("94", "-"));
            responseCodeItems.Add(new ResponseCodeItems("95", "-"));
            responseCodeItems.Add(new ResponseCodeItems("96", "-"));
            responseCodeItems.Add(new ResponseCodeItems("97", "-"));
            responseCodeItems.Add(new ResponseCodeItems("98", "-"));
            responseCodeItems.Add(new ResponseCodeItems("99", "-"));

           listView1.BeginUpdate(); // 그리기 이벤트 일시중지, 아이템을 99번 추가할때마다 화면을 새로고침하면 깜빡임이 발생하고, 속도가 매우느려짐
                                   // 이를 방지하기 위한 코드라고 보면된다.


            foreach(var item in responseCodeItems)
            {
                ListViewItem lvi = new ListViewItem(item.Code); 
                //ListViewItem은 행(Row) 하나를 의미한다.
                //생성자 인자로 들어가는 item.Code는 첫 번째 열의 텍스트가된다.
                lvi.SubItems.Add(item.Name);
                //SubItems는 두번째 열부터 순서대로 들어가는 상세 데이터이다.
                //여기서는 item.Name이 두 번째 열(에러명칭)에 배치된다.
                listView1.Items.Add(lvi);
            }
            listView1.EndUpdate();
        }


        private void taget_operating_code_list()
            {
            listView3.View = View.Details;
            listView3.GridLines = true;
            listView3.FullRowSelect = true;
        
            listView3.Items.Clear();

            List<target_operating_code_itmes> taget_Operating_Code_Items = new List<target_operating_code_itmes>();

            taget_Operating_Code_Items.Add(new target_operating_code_itmes("0","이동"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("1","화물 적재동작"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("2","화물 이재동작"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("3", "승강"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("4", "-"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("5", "-"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("6", "-"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("7", "-"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("8", "-"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("9", "-"));

            listView3.BeginUpdate();

            foreach(var item in taget_Operating_Code_Items)
            {
                ListViewItem lvi = new ListViewItem(item.Code);
                lvi.SubItems.Add(item.Name);

                listView3.Items.Add(lvi);
            }

            listView3.EndUpdate();



        }

        private void Operating_Code_list()
        {
            listView4.View = View.Details;
            listView4.GridLines = true;
            listView4.FullRowSelect = true;
            listView4.Items.Clear();

            List<Operating_Code_Items> operating_Code_Items = new List<Operating_Code_Items>();

            operating_Code_Items.Add(new Operating_Code_Items("0","수동 운전 모드"));
            operating_Code_Items.Add(new Operating_Code_Items("1", "초기주행 전진"));
            operating_Code_Items.Add(new Operating_Code_Items("2", "초기주행 후진"));
            operating_Code_Items.Add(new Operating_Code_Items("3", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("4", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("5", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("6", "충돌방지 감속"));
            operating_Code_Items.Add(new Operating_Code_Items("7", "인터록 정지"));
            operating_Code_Items.Add(new Operating_Code_Items("8", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("9", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("10", "주행 전진"));
            operating_Code_Items.Add(new Operating_Code_Items("11", "주행 후진"));
            operating_Code_Items.Add(new Operating_Code_Items("12", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("13", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("14", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("15", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("16", "충돌방지 감속"));
            operating_Code_Items.Add(new Operating_Code_Items("17", "충돌방지 정지"));
            operating_Code_Items.Add(new Operating_Code_Items("18", "인터록 정지"));
            operating_Code_Items.Add(new Operating_Code_Items("19", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("20", "교신 준비"));
            operating_Code_Items.Add(new Operating_Code_Items("21", "교신 요구 송신"));
            operating_Code_Items.Add(new Operating_Code_Items("22", "교신 지령 수신"));
            operating_Code_Items.Add(new Operating_Code_Items("23", "교신 수신 완료 송신"));
            operating_Code_Items.Add(new Operating_Code_Items("24", "교신 반송 개시 수신"));
            operating_Code_Items.Add(new Operating_Code_Items("25", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("26", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("27", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("28", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("29", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("30", "목적지 변경"));
            operating_Code_Items.Add(new Operating_Code_Items("31", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("32", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("33", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("34", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("35", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("36", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("37", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("38", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("39", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("40", "분기전 감속"));
            operating_Code_Items.Add(new Operating_Code_Items("41", "분기전 카운트 정지"));
            operating_Code_Items.Add(new Operating_Code_Items("42", "분기 탑승 정지"));
            operating_Code_Items.Add(new Operating_Code_Items("43", "분기 출력"));
            operating_Code_Items.Add(new Operating_Code_Items("44", "분기 발진"));
            operating_Code_Items.Add(new Operating_Code_Items("45", "분기 탑승 출력"));
            operating_Code_Items.Add(new Operating_Code_Items("46", "충돌방지 감속"));
            operating_Code_Items.Add(new Operating_Code_Items("47", "충돌방지 정지"));
            operating_Code_Items.Add(new Operating_Code_Items("48", "인터록 정지"));
            operating_Code_Items.Add(new Operating_Code_Items("49", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("50", "IDLE 정지중"));
            operating_Code_Items.Add(new Operating_Code_Items("51", "선입품 에러 발생 지시대기"));
            operating_Code_Items.Add(new Operating_Code_Items("52", "SET가능 에러 지시대기"));
            operating_Code_Items.Add(new Operating_Code_Items("53", "이동 주행 에러 지시대기"));
            operating_Code_Items.Add(new Operating_Code_Items("54", "화물탑재전 주행 에러 지시대기"));
            operating_Code_Items.Add(new Operating_Code_Items("55", "리트라이 지시대기"));
            operating_Code_Items.Add(new Operating_Code_Items("56", "화물반출전 주행 에러 지시대기"));
            operating_Code_Items.Add(new Operating_Code_Items("57", "화물탑재 이상 지시대기"));
            operating_Code_Items.Add(new Operating_Code_Items("58", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("59", "일시정지중"));

            operating_Code_Items.Add(new Operating_Code_Items("60", "하강 준비"));
            operating_Code_Items.Add(new Operating_Code_Items("61", "하강 고속"));
            operating_Code_Items.Add(new Operating_Code_Items("62", "하강 저속"));
            operating_Code_Items.Add(new Operating_Code_Items("63", "하강 카운트 타이머 대기"));
            operating_Code_Items.Add(new Operating_Code_Items("64", "하강 정지1"));
            operating_Code_Items.Add(new Operating_Code_Items("65", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("66", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("67", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("68", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("69", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("70", "화물 탑재"));
            operating_Code_Items.Add(new Operating_Code_Items("71", "화물 반출"));
            operating_Code_Items.Add(new Operating_Code_Items("72", "케이지 상승가능 대기"));
            operating_Code_Items.Add(new Operating_Code_Items("73", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("74", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("75", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("76", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("77", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("78", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("79", "하강 정지2"));

            operating_Code_Items.Add(new Operating_Code_Items("80", "상승 준비"));
            operating_Code_Items.Add(new Operating_Code_Items("81", "상승 저속"));
            operating_Code_Items.Add(new Operating_Code_Items("82", "상승 고속"));
            operating_Code_Items.Add(new Operating_Code_Items("83", "상승 원점 전 감속"));
            operating_Code_Items.Add(new Operating_Code_Items("84", "상승정지"));
            operating_Code_Items.Add(new Operating_Code_Items("85", "상승종료"));
            operating_Code_Items.Add(new Operating_Code_Items("86", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("87", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("88", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("89", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("90", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("91", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("92", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("93", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("94", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("95", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("96", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("97", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("98", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("99", "-"));

            listView4.BeginUpdate();

            foreach(var item in operating_Code_Items)
            {
                ListViewItem lvi = new ListViewItem(item.Code);
                lvi.SubItems.Add(item.Name);
                listView4.Items.Add(lvi);

            }
            listView4.EndUpdate();  


        }





        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void listView3_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void listView4_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void event_log_listview_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {
            using (var dlg = new Form())
            {
                dlg.Text = "패스워드";
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MinimizeBox = false;
                dlg.MaximizeBox = false;
                dlg.ShowInTaskbar = false;
                dlg.ClientSize = new Size(288, 118);
                dlg.BackColor = Color.FromArgb(62, 62, 66);
                dlg.ForeColor = Color.White;

                var lbl = new Label { Text = "비밀번호 입력", Location = new Point(12, 12), AutoSize = true, ForeColor = Color.White };
                var tb = new System.Windows.Forms.TextBox { PasswordChar = '*', Location = new Point(12, 36), Width = 256 };
                var btnOk = new System.Windows.Forms.Button { Text = "확인", Location = new Point(96, 72), DialogResult = DialogResult.OK, Size = new Size(78, 28) };
                var btnCancel = new System.Windows.Forms.Button { Text = "취소", Location = new Point(184, 72), DialogResult = DialogResult.Cancel, Size = new Size(78, 28) };
                dlg.Controls.Add(lbl);
                dlg.Controls.Add(tb);
                dlg.Controls.Add(btnOk);
                dlg.Controls.Add(btnCancel);
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
                if (tb.Text == "1")
                {
                    BypassRailInterlockForAuto = true;
                    MessageBox.Show(this,
                        "AUTO 실행 시 레일 8비트 인터록을 건너뜁니다. H4 전송 전 1초만 대기합니다.\r\n※ 해제: 프로그램 재시작",
                        "개발 모드",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show(this, "비밀번호가 맞지 않습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Line_Setup line_Setup_func = new Line_Setup();

            line_Setup_func.ShowDialog();

        }

        private void btn_Command_Click(object sender, EventArgs e)
        {
            // 1) EMO 해제
            if (_emoGreenBorderOn)
            {
                MessageBox.Show("EMO가 눌려 있습니다. 해제 후 반자동 명령을 실행하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 2) I.O 체크 완료
            if (!IOCheckSheetForm.IoCheckCompleted)
            {
                MessageBox.Show("I.O 체크 성적서에서 옵션에 맞는 I.O 체크를 완료한 뒤 [상태저장]을 눌러 주세요.", "반자동 명령 불가", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 3) 통신 연결
            if (GlobalComm == null)
            {
                MessageBox.Show("먼저 연결을 완료해 주세요.", "반자동 명령 불가", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 4) Line_Setup 호기 저장
            if (string.IsNullOrEmpty(Line_Setup.SavedVehicleNo))
            {
                MessageBox.Show("저장된 호기가 없습니다. Line_Setup에서 호기를 선택한 뒤 [상태저장]을 눌러 주세요.", "반자동 명령 불가", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 5) 엔코더 101·110·113
            string vehicleID = Line_Setup.SavedVehicleNo;
            int v101 = _encManager.GetStoredValue(vehicleID, "101");
            int v110 = _encManager.GetStoredValue(vehicleID, "110");
            int v113 = _encManager.GetStoredValue(vehicleID, "113");
            if (v101 < 0 || v101 > 500 || v110 < 0 || v110 > 500 || v113 < 0 || v113 > 500)
            {
                MessageBox.Show("Line_Setup에서 저장한 호기(" + vehicleID + ")에 대한 엔코더값(101, 110, 113)이 엔코더설정에 저장되어 있어야 합니다. 해당 호기를 선택한 뒤 101·110·113 포지션을 0~500 범위로 저장하세요.", "반자동 명령 불가", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Command_Form command_Form = new Command_Form();

            // [이것이 Owner 설정!] 
            // cmdForm의 주인은 나(this = Main)라고 알려주는 겁니다.
            command_Form.Owner = this;

            command_Form.ShowDialog();

        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (SKY_RAV_CONNECT_FORM == null || SKY_RAV_CONNECT_FORM.IsDisposed)
            {
                SKY_RAV_CONNECT_FORM = new EMS_TCP_UDP_Connect();
                SKY_RAV_CONNECT_FORM.Owner = this;
                SKY_RAV_CONNECT_FORM.SetMainFormForLog(this);

                // [수정 포인트] VisibleChanged 이벤트에서 색상 변경 조건을 확인합니다.
                SKY_RAV_CONNECT_FORM.VisibleChanged += (s, args) =>
                {
                    // Connect 창이 연결 성공 후 Close() 되거나 Hide() 되었을 때
                    if (!SKY_RAV_CONNECT_FORM.Visible && SKY_RAV_CONNECT_FORM.DialogResult == DialogResult.OK)
                    {
                        // 통신 객체 안전하게 복사
                        if (this.GlobalComm == null && SKY_RAV_CONNECT_FORM._comm != null)
                        {
                            this.GlobalComm = SKY_RAV_CONNECT_FORM._comm;
                        }

                        // 버튼 색상을 연두색으로 변경
                        button2.BackColor = Color.Lime; // 또는 Color.GreenYellow
                        button2.ForeColor = Color.Black; // 검은색 글씨가 연두색에서 잘 보입니다.
                        button2.Text = "연결 완료";
                        FlatButtonPaintFix.ApplyToButton(button2);
                    }
                };
            }

            SKY_RAV_CONNECT_FORM.Show();
            SKY_RAV_CONNECT_FORM.BringToFront();
        }



        private void button7_Click(object sender, EventArgs e)
        {
            // 1) EMO 해제
            if (_emoGreenBorderOn)
            {
                MessageBox.Show("EMO가 눌려 있습니다. 해제 후 자동모드를 실행하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 2) I.O 체크 완료
            if (!IOCheckSheetForm.IoCheckCompleted)
            {
                MessageBox.Show("I.O 체크 성적서에서 옵션에 맞는 I.O 체크를 완료한 뒤 [상태저장]을 눌러 주세요.", "AUTO 실행 불가", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 3) 통신 연결
            if (GlobalComm == null)
            {
                MessageBox.Show("먼저 연결을 완료해 주세요.");
                return;
            }
            // 4) Line_Setup 호기 저장
            if (string.IsNullOrEmpty(Line_Setup.SavedVehicleNo))
            {
                MessageBox.Show("저장된 호기가 없습니다. Line_Setup에서 호기를 선택한 뒤 [상태저장]을 눌러 주세요.", "AUTO 실행 불가", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 5) 엔코더 101·110·113
            string vehicleID = Line_Setup.SavedVehicleNo;
            int v101 = _encManager.GetStoredValue(vehicleID, "101");
            int v110 = _encManager.GetStoredValue(vehicleID, "110");
            int v113 = _encManager.GetStoredValue(vehicleID, "113");
            if (v101 < 0 || v101 > 500 || v110 < 0 || v110 > 500 || v113 < 0 || v113 > 500)
            {
                MessageBox.Show("Line_Setup에서 저장한 호기(" + vehicleID + ")에 대한 엔코더값(101, 110, 113)이 엔코더설정에 저장되어 있어야 합니다. 해당 호기를 선택한 뒤 101·110·113 포지션을 0~500 범위로 저장하세요.", "AUTO 실행 불가", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 6) 자동모드 미실행 (실행 중이면 정지 요청 처리)
            if (_autoRunning)
            {
                _autoGreenBorderOn = false;
                ApplyAutoButtonGreenBorder(false);
                _cycleStopRequestedByUser = true;
                EMS_Mode_Sequence.CycleStopRequested = true;
                TowerLamp.SetMode(TowerLampVisualMode.AutoCycleStopYellowBlink);
                MessageBox.Show("사이클 정지 요청되었습니다. 현재 동작을 마친 뒤 101번 위치로 복귀하여 타이어를 내려놓은 후 정지합니다.");
                return;
            }
            // 7) 실행 확인 (대화상자 전: 녹색 점멸)
            TowerLamp.SetMode(TowerLampVisualMode.AutoConfirmGreenBlink);
            DialogResult result = MessageBox.Show("자동모드를 실행하시겠습니까?", "상태확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                MessageBox.Show("취소하였습니다.");
                RefreshTowerLampAfterAutoOrRail();
                return;
            }

            TowerLamp.SetMode(TowerLampVisualMode.AutoAfterConfirmBlueBlink);
            MessageBox.Show("레일 주변에 장애물이 없는지 확인한 후 진행하시기 바랍니다.");

            _autoGreenBorderOn = true;
            ApplyAutoButtonGreenBorder(true);

            Command_Form command_Form = new Command_Form();
            command_Form.Owner = this;
            command_Form._comm = GlobalComm;
            command_Form._emsProto = GlobalEmsProto ?? new EMS_Protocol();
            command_Form.currentData.command_alloc = 3;
            command_Form.currentData.EMS_NO = Line_Setup.SavedVehicleNo ?? "1호기";

            _autoCts = new System.Threading.CancellationTokenSource();
            _autoRunning = true;

            EMS_Mode_Sequence emsSeq = new EMS_Mode_Sequence();
            var autoTask = emsSeq.RunAutoSequenceAsync(command_Form, this, _autoCts.Token);
            autoTask.ContinueWith(_ =>
            {
                void Done()
                {
                    if (this.IsDisposed) return;
                    _autoRunning = false;
                    _autoGreenBorderOn = false;
                    ApplyAutoButtonGreenBorder(false);
                    button7.Text = "";
                    if (_autoStoppedByEmo)
                    {
                        _autoStoppedByEmo = false;
                        MessageBox.Show("사이클 정지 실패. 원점으로 이동하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (_cycleStopRequestedByUser)
                    {
                        _cycleStopRequestedByUser = false;
                        IOCheckSheetForm.AutoCycleCompleted = true;  // 사이클 정지 완료 시점 → I.O 자동 체크 대상 반영용
                        MessageBox.Show("자동모드를 정지하였습니다. (101번 위치, 타이어 하차 완료)");
                    }
                    RefreshTowerLampAfterAutoOrRail();
                }
                if (!this.IsDisposed)
                {
                    if (InvokeRequired) Invoke(new Action(Done));
                    else Done();
                }
            }, System.Threading.Tasks.TaskScheduler.Default);
        }

        private void button8_Click(object sender, EventArgs e)//EMO: 한번째 터치 → 초록띄 + 1번 시퀀스, 두번째 터치 → 띄 제거 + 2번 시퀀스
        {
            if (!_emoGreenBorderOn)
            {
                if (_autoRunning)
                {
                    _autoStoppedByEmo = true;
                    _autoCts?.Cancel();
                }
                _autoGreenBorderOn = false;
                ApplyAutoButtonGreenBorder(false); // EMO 눌림 = AUTO 꺼짐 → 초록 띄 즉시 제거
                ApplyEmoButtonGreenBorder(true);
                _emoGreenBorderOn = true;
                TowerLamp.SetMode(TowerLampVisualMode.EmoRedBlink);
                EmoFirstTapSequence();
            }
            else
            {
                ApplyEmoButtonGreenBorder(false);
                _emoGreenBorderOn = false;
                EmoSecondTapSequence();
                RefreshTowerLampAfterAutoOrRail();
            }
        }

        /// <summary>EMO 한번째 터치 시 실행할 시퀀스: EMO 해제 전까지 기체 이상정지 H4(05) 주기 전송.</summary>
        private void EmoFirstTapSequence()
        {
            if (_emoH4AbnormalStopTimer == null)
            {
                _emoH4AbnormalStopTimer = new System.Windows.Forms.Timer();
                _emoH4AbnormalStopTimer.Interval = 500; // 500ms 주기
                _emoH4AbnormalStopTimer.Tick += (s, ev) =>
                {
                    if (GlobalComm == null || GlobalEmsProto == null) return;
                    byte[] h4 = GlobalEmsProto.EMS_Item_order("5"); // 05: 기체 이상정지
                    if (h4 != null)
                        _ = GlobalComm.SendData(Encoding.ASCII.GetString(h4));
                };
            }
            _emoH4AbnormalStopTimer.Start();
        }

        /// <summary>EMO 두번째 터치 시 실행할 시퀀스: 주기 전송 중지 후 기체 이상리셋 H4(06) 1회 전송.</summary>
        private void EmoSecondTapSequence()
        {
            _emoH4AbnormalStopTimer?.Stop();
            if (GlobalComm != null && GlobalEmsProto != null)
            {
                byte[] h4 = GlobalEmsProto.EMS_Item_order("6"); // 06: 기체 이상리셋
                if (h4 != null)
                    _ = GlobalComm.SendData(Encoding.ASCII.GetString(h4));
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // 콤보박스(101~113)와 입력 텍스트박스의 값을 매니저에게 전달
            // 매니저 내부에서 0~530 체크 후 lift_enc_datas에 저장하고 라벨을 바꿉니다.
            // comboBox1: 호기번호, comboBox2: 포지션설정, textBox1: 엔코더입력
            _encManager.ExecuteSave(comboBox1.Text, comboBox2.Text, textBox1.Text);
        }

        // [현재값 갱신] 버튼 클릭 이벤트 (버튼 이름을 btnEncoderRefresh라고 가정)
        private void btnEncoderRefresh_Click(object sender, EventArgs e)
        {
            string vID = comboBox1.Text;
            string pos = comboBox2.Text;

            int val = _encManager.GetStoredValue(vID, pos);
            textBox1.Text = val < 0 ? "" : val.ToString(); // 미설정(-1)이면 빈칸, 0 포함 설정값이면 숫자
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
{
  
}

        private void button6_Click(object sender, EventArgs e)
        {
            string selectedVehicle = comboBox1.Text;

            // 1. 선택된 호기의 모든 데이터를 우측 라벨 표(101~113)에 표시
            _encManager.DisplayVehicleData(selectedVehicle);

            // 2. 현재 선택된 포지션의 값만 텍스트박스(입력창)에도 표시
            int currentVal = _encManager.GetStoredValue(selectedVehicle, comboBox2.Text);
            textBox1.Text = currentVal < 0 ? "" : currentVal.ToString(); // 미설정(-1)이면 빈칸
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("정말로 모든 데이터를 초기화하시겠습니까?", "확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _encManager.ClearAllData();
                textBox1.Clear();
            }
        }


        public void ResetConnectButton()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(ResetConnectButton));
                return;
            }

            button2.BackColor = Color.FromArgb(37, 99, 235); // 끊김 시 파란색(기본 버튼색) 복귀
            button2.ForeColor = Color.White;
            button2.Text = "Connect"; // 원래 텍스트로 복구
            FlatButtonPaintFix.ApplyToButton(button2);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            using (var form = new IOCheckSheetForm())
            {
                form.ShowDialog(this);
            }
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void 통신로그_Click(object sender, EventArgs e)
        {

        }

        private void 엔코더설정_Click(object sender, EventArgs e)
        {

        }
    }
}
