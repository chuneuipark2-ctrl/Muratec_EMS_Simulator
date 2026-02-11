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



  
        public PANEL_DIO()
        {
            InitializeComponent();
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
            this.Close();
        }

        private void Panel_DioFormClosing(object sender, FormClosingEventArgs e)
        {

            Rail_DIO.Instance.DISCONNECT();

           

        }
    }
}
