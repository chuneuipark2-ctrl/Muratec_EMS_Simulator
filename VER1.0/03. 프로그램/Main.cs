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
            Rail_DIO RAIL_PART_RIO = new Rail_DIO();
            RAIL_PART_RIO.FASTECH_CONNECT();

            Form PANEL_DIO = new PANEL_DIO();
            PANEL_DIO.Show();

            /*
            Rail_DIO thread_start = new Rail_DIO();// 멀티쓰래드 구현??? 맞는지 잘모르겠음, 객체선언
            Thread Rail_DIO_thread = new Thread(thread_start.connect_rail_dio);
            Rail_DIO_thread.IsBackground = true;
            Rail_DIO_thread.Start();
            */

        }
    }
}
