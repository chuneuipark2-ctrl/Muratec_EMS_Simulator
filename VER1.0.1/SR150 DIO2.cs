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
    public partial class SR150_DIO2 : Form
    {
        public SR150_DIO2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            Rail_DIO.Instance.DISCONNECT();

            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form SR150_DIO1 = new SR150_DIO1();
            SR150_DIO1.ShowDialog();
            this.Close();
        }

        private void SR150dio2_form_closing(object sender, FormClosingEventArgs e)
        {

            Rail_DIO.Instance.DISCONNECT();
            foreach (Form openForm in Application.OpenForms)
            {

                if (openForm is Main mainForm)
                {
                    mainForm.UpdateConnectionStatus(false);
                    break;
                }
            }
        }
    }
}
