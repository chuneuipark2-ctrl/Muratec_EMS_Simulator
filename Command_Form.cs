using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EMS_TEST_SIMULATOR
{
    public partial class Command_Form : Form
    {
        public command_Data currentData = new command_Data();
        public EMS_Protocol _emsProto = new EMS_Protocol();

        // Main의 _comm과 타입을 똑같이 맞춰야 합니다. 
        // 만약 Main에서 EMS_TCP_UDP_Connect로 선언했다면 이대로 두시면 됩니다.
        public IDeviceComm _comm;

        /// <summary>true이면 START 시 반자동 대신 AUTO 시퀀스 시작 (Main button7 경로)</summary>
        public bool LaunchAsAuto { get; set; }

        public Command_Form()
        {
            InitializeComponent();
        }

        // [합치기] 중복되었던 Load 이벤트를 하나로 통합했습니다.
        private void Command_Form_Load(object sender, EventArgs e)
        {

            // 1. 이 폼을 연 주인이 메인 폼(Main)인지 확인합니다.
            if (this.Owner is Main mainForm)
            {
                // 2. [핵심] 이제 창을 찾지 않고, 메인 폼이 보관 중인 GlobalComm을 바로 가져옵니다.
                this._comm = mainForm.GlobalComm;
                if (mainForm.GlobalEmsProto != null)
                    this._emsProto = mainForm.GlobalEmsProto;

                if (this._comm == null)
                {
                    AppErrorLog.RaiseAndShow("반자동", "통신 객체가 비어있습니다.\n[Connect] 창에서 연결을 먼저 완료해주세요.", "반자동");
                }

                if (LaunchAsAuto)
                {
                    if (!string.IsNullOrEmpty(Line_Setup.SavedLineName))
                        comboBox4.Text = Line_Setup.SavedLineName;
                    if (!string.IsNullOrEmpty(Line_Setup.SavedVehicleNo))
                        comboBox3.Text = Line_Setup.SavedVehicleNo;
                }
            }
            FlatButtonPaintFix.ApplyToTree(this);
        }

        public class command_Data
        {
            public string rail_data = "SR50 레일";
            public string EMS_NO = "0호기";
            public int Start_count = 0;
            public int End_count = 0;
            public int command_alloc = 0;
            public int loop_command = 0;
        }

        
        /// <summary>
        /// 반자동 명령을 동작시키기 위한 조건들
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
        
        private async void button1_Click(object sender, EventArgs e)
        {
            // [추가] 통신 객체가 제대로 넘어왔는지 확인하는 안전장치
            if (_comm == null)
            {
                AppErrorLog.RaiseAndShow("반자동", "통신 연결 객체(_comm)가 없습니다. 메인폼에서 연결을 먼저 확인하세요.", "반자동");
                return;
            }

            // Line_Setup에서 저장된 호기·라인과 일치하는지 검사 (저장 안 했거나 다르면 동작 불가)
            if (string.IsNullOrEmpty(Line_Setup.SavedVehicleNo) || string.IsNullOrEmpty(Line_Setup.SavedLineName))
            {
                AppErrorLog.RaiseAndShow("반자동", "저장된 정보가 없습니다.\r\nLine_Setup에서 라인·호기를 선택한 뒤 [상태저장]을 실행해 주세요.", "저장된 정보 없음");
                return;
            }
            string currentRail = (comboBox4?.Text ?? "").Trim();
            string currentNo = (comboBox3?.Text ?? "").Trim();
            if (currentNo != Line_Setup.SavedVehicleNo || currentRail != Line_Setup.SavedLineName)
            {
                AppErrorLog.RaiseAndShow("반자동", "저장된 정보가 없습니다.\r\n현재 선택한 레일·호기가 Line_Setup에 저장된 값과 일치하지 않습니다.", "저장된 정보 없음");
                return;
            }

            if (this.Owner is Main mainForEnc)
            {
                int v101 = mainForEnc._EncManager.GetStoredValue(currentNo, "101");
                int v110 = mainForEnc._EncManager.GetStoredValue(currentNo, "110");
                int v113 = mainForEnc._EncManager.GetStoredValue(currentNo, "113");
                if (v101 < 0 || v101 > 500 || v110 < 0 || v110 > 500 || v113 < 0 || v113 > 500)
                {
                    AppErrorLog.RaiseAndShow("반자동",
                        "엔코더 설정에 선택 호기(" + currentNo + ")의 101·110·113 값(0~500)이 저장되어 있어야 합니다.",
                        "반자동 명령 불가");
                    return;
                }
            }

            DialogResult result = MessageBox.Show("동작을 시작하시겠습니까?", "확인",
                                                  MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                currentData.rail_data = comboBox4.Text;
                currentData.EMS_NO = comboBox3.Text;
                int.TryParse(comboBox5.Text, out currentData.Start_count); //시작위치
                int.TryParse(comboBox2.Text, out currentData.End_count); //앤드위치
                currentData.command_alloc = ParseCommandAlloc(comboBox1.Text);
                if (!LaunchAsAuto && (currentData.command_alloc < 1 || currentData.command_alloc > 3))
                {
                    AppErrorLog.RaiseAndShow("반자동", "명령할당(이동/탑재/이재)을 선택해 주세요.", "반자동");
                    return;
                }
                if (currentData.Start_count <= 0 || currentData.End_count <= 0)
                {
                    AppErrorLog.RaiseAndShow(LaunchAsAuto ? "AUTO" : "반자동",
                        "시작 카운트와 목적 카운트를 선택해 주세요.", LaunchAsAuto ? "AUTO" : "반자동");
                    return;
                }
               // int.TryParse(textBox1.Text, out currentData.loop_command);

                EMS_Mode_Sequence seq = new EMS_Mode_Sequence();

                if (this.Owner is Main mainForm)
                {
                    if (LaunchAsAuto)
                    {
                        mainForm.BeginAutoSequenceFromCommandForm(this);
                        Close();
                        return;
                    }

                    bool isSuccess = await seq.SemiAuto_Sequence(this, mainForm);

                    if (isSuccess)
                    {
                        MessageBox.Show($"{currentData.rail_data} {currentData.EMS_NO} 동작을 완료했습니다.");
                    }
                }
            }
        }

        /// <summary>명령할당 콤보(이동/탑재/이재) → 1/2/3</summary>
        private static int ParseCommandAlloc(string text)
        {
            switch ((text ?? "").Trim())
            {
                case "이동": return 1;
                case "탑재": return 2;
                case "이재": return 3;
                default:
                    return int.TryParse(text, out int n) ? n : 0;
            }
        }

        // 아래 빈 이벤트들은 에러 방지를 위해 그대로 둡니다.
        private void button2_Click(object sender, EventArgs e) { }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e) { }
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e) { }
        public void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}