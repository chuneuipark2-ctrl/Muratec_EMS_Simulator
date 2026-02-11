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

namespace EMS_TEST_SIMULATOR
{
    public partial class Main : Form
    {

        public static Main Instance  = new Main();






        public Main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

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

        private void button3_Click(object sender, EventArgs e)
        {
            //Rail_DIO.Instance.FASTECH_CONNECT();
            //Form PANEL_DIO = new PANEL_DIO();
            //PANEL_DIO.Show();

            // 1. 연결 시도 및 결과 확인
            bool isConnected = Rail_DIO.Instance.FASTECH_CONNECT();

            if (isConnected)
            {
                // 2. 성공 시 버튼 색상을 녹색(Lime)으로 변경
                Rail_io.BackColor = Color.Lime;
                Rail_io.ForeColor = Color.Black; // 글자색도 잘 보이게 조정
                Rail_io.Text = "Rail I.O (Connected)";

                // 3. 다음 화면 띄우기
                PANEL_DIO panel = new PANEL_DIO();
                panel.Show();
                // this.Hide(); // 만약 메인 버튼 상태를 계속 봐야 한다면 Hide하지 마세요.
            }
            else
            {
                // 실패 시 시각적 알림
                Rail_io.BackColor = Color.Red;
                MessageBox.Show("통신 연결에 실패했습니다. 장비 전원과 IP를 확인하세요.");
            }


            /*
            Rail_DIO thread_start = new Rail_DIO();// 멀티쓰래드 구현??? 맞는지 잘모르겠음, 객체선언
            Thread Rail_DIO_thread = new Thread(thread_start.connect_rail_dio);
            Rail_DIO_thread.IsBackground = true;
            Rail_DIO_thread.Start();
            */


        }

        public void UpdateConnectionStatus(bool connected)
        {
            if (connected)
            {
                Rail_io.BackColor = Color.Lime;
            }
            else
            {
                Rail_io.BackColor = Color.FromKnownColor(KnownColor.Control); // 기본 색상
            }
        }




        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }
    }
}
