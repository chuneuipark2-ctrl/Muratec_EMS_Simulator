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
    public partial class PANEL_DIO : Form
    {

        private Control[] ledControls;
        private Control[] outputLeds; // 출력용 (신규)
        public int TargetIndex = 0; // 창을 띄울 때 0, 1, 2... 지정


        public PANEL_DIO()
        {
            InitializeComponent();
            InitLedArrays(); // 컨트롤 배열 초기화 함수 호출

   


            // 2. Rail_DIO 이벤트 구독
            Rail_DIO.Instance.OnInputUpdated += UpdateLedDisplay;
            Rail_DIO.Instance.OnOutputUpdated += UpdateOutputLedDisplay;

            // 폼이 닫힐 때 이벤트 해제 필수
            this.FormClosed += (s, e) => Rail_DIO.Instance.OnInputUpdated -= UpdateLedDisplay;
            this.FormClosed += (s, e) => Rail_DIO.Instance.OnOutputUpdated -= UpdateOutputLedDisplay;
            }



        private void InitLedArrays()
        {
            ledControls = new Control[] {
            DI1, DI2, DI3, DI4, DI5, DI6, DI7, DI8,
            DI9, DI10, DI11, DI12, DI13, DI14, DI15, DI16
            };

            outputLeds = new Control[] {
            DO1, DO2, DO3, DO4, DO5,
            DO6, DO7, DO8, DO9, DO10,
            DO11, DO12, DO13, DO14, DO15, DO16 };

        }





        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form SR50_DIO1 = new SR50_DIO1();
            SR50_DIO1.ShowDialog();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
  
            this.Hide();
           

            Rail_DIO.Instance.DISCONNECT();


            // 2. 현재 열려있는 폼들 중에서 메인 폼(이름이 Main인 경우)을 찾아 색상 변경
            foreach (Form openForm in Application.OpenForms)
            {
              
                if (openForm is Main mainForm)
                {
                    mainForm.UpdateConnectionStatus(false);
                    break;
                }
            }

            this.Close();
        }

        private void Panel_DioFormClosing(object sender, FormClosingEventArgs e)
        {

            Rail_DIO.Instance.DISCONNECT();
            // 2. 현재 열려있는 폼들 중에서 메인 폼(이름이 Main인 경우)을 찾아 색상 변경
            foreach (Form openForm in Application.OpenForms)
            {
          
                if (openForm is Main mainForm)
                {
                    mainForm.UpdateConnectionStatus(false);
                    break;
                }
            }


        }



        private void PANEL_DIO_FormClosed(object sender, FormClosedEventArgs e)
        {
            Rail_DIO.Instance.OnInputUpdated -= UpdateLedDisplay;
            Rail_DIO.Instance.OnOutputUpdated -= UpdateOutputLedDisplay;
        }




        private async void button3_Click(object sender, EventArgs e)
        {

            Rail_DIO.Instance.io_test_flag = true;
            await Task.Run(() => Rail_DIO.Instance.bit_step_test());
         

                //            Rail_DIO.Instance.bit_step_test();//8BIT STEP TEST 진행
        }

        private void DI1_Paint(object sender, PaintEventArgs e)
        {

        }

        // 3. 실시간 색상 변경 함수
        private void UpdateLedDisplay(int index, uint[] status)
        {
            // 중요: 스레드가 다르므로 Invoke를 사용해 UI 스레드에서 실행
            // 창이 이미 닫혔거나 파괴되었다면 함수를 즉시 종료
            if (this.IsDisposed || this.Disposing) return;
            // 내가 담당한 장비 번호가 아니면 무시!
            if (this.TargetIndex != index) return;


            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new Action(() => UpdateLedDisplay(index, status)));
                }
                catch (ObjectDisposedException) { /* 창이 닫히는 찰나에 호출될 경우 예외 처리 */ }
                return;
            }

            // ... 나머지 색상 변경 로직
            for (int i = 0; i < 16; i++)
            {
                if (status[i] != 0) // 비트값이 0이 아니면 ON
                    ledControls[i].BackColor = Color.Lime;
                else
                    ledControls[i].BackColor = Color.Gray;
            }
        }


        private void UpdateOutputLedDisplay(int index, uint status)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            // 내가 담당한 장비 번호가 아니면 무시!
            if (this.TargetIndex != index) return;


            if (this.InvokeRequired)
            {
                try { this.Invoke(new Action(() => UpdateOutputLedDisplay(index, status))); }
                catch (ObjectDisposedException) {
                  
                }
                return;
            }


            // 2. 현재 선택된 장비의 index와 맞는지 확인 (예: index 0번 장비 표시 중일 때)
            // if (index != currentSelectedIndex) return;

            for (int i = 0; i < 16; i++)
            {
                bool isOn = (status & (0x00010000u << i)) != 0;
                outputLeds[i].BackColor = isOn ? Color.Red : Color.DimGray;
            }
        }




    }
}
