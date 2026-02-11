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
    public partial class SR150_DIO1 : Form
    {
        public SR150_DIO1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form SR150_DIO2 = new SR150_DIO2();
            SR150_DIO2.ShowDialog();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form SR50_DIO2 = new SR50_DIO2();
            SR50_DIO2.ShowDialog();
            this.Close();
        }

        private void SR150DIO1_Form_closing(object sender, FormClosingEventArgs e)
        {
            Rail_DIO init_status = new Rail_DIO();
            init_status.DISCONNECT();
        }
    }
}
