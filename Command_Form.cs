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

                // 3. 만약 여전히 null이라면, 정말로 연결을 한 번도 안 한 것입니다.
                if (this._comm == null)
                {
                    MessageBox.Show("통신 객체가 비어있습니다.\n[Connect] 창에서 연결을 먼저 완료해주세요.");
                }
            }
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

        private async void button1_Click(object sender, EventArgs e)
        {
            // [추가] 통신 객체가 제대로 넘어왔는지 확인하는 안전장치
            if (_comm == null)
            {
                MessageBox.Show("통신 연결 객체(_comm)가 없습니다. 메인폼에서 연결을 먼저 확인하세요.");
                return;
            }

            DialogResult result = MessageBox.Show("동작을 시작하시겠습니까?", "확인",
                                                  MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                currentData.rail_data = comboBox4.Text;
                currentData.EMS_NO = comboBox3.Text;
                int.TryParse(comboBox5.Text, out currentData.Start_count);
                int.TryParse(comboBox2.Text, out currentData.End_count);
                int.TryParse(comboBox1.Text, out currentData.command_alloc);
                int.TryParse(textBox1.Text, out currentData.loop_command);

                EMS_Mode_Sequence seq = new EMS_Mode_Sequence();

                if (this.Owner is Main mainForm)
                {
                    bool isSuccess = await seq.SemiAuto_Sequence(this, mainForm);

                    if (isSuccess)
                    {
                        MessageBox.Show($"{currentData.rail_data} {currentData.EMS_NO} 동작을 완료했습니다.");
                    }
                }
            }
        }

        // 아래 빈 이벤트들은 에러 방지를 위해 그대로 둡니다.
        private void button2_Click(object sender, EventArgs e) { }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e) { }
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e) { }
        public void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}