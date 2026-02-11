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
    public partial class SR50_DIO2 : Form
    {
        public SR50_DIO2()
        {
            InitializeComponent();
        }

        private void SR50_DIO2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form SR150_DIO1 = new SR150_DIO1();
            SR150_DIO1.ShowDialog();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form SR50_DIO1 = new SR50_DIO1();
            SR50_DIO1.ShowDialog();
            this.Close();
        }

        private void SR50DIO2_Form_closing(object sender, FormClosingEventArgs e)
        {
            Rail_DIO init_status = new Rail_DIO();
            init_status.DISCONNECT();
        }
    }
}
