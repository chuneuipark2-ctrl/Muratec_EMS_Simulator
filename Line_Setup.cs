using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EMS_TEST_SIMULATOR
{
    public partial class Line_Setup : Form
    {
        public class Save_status
        {
            public string Save_LineName { get; set; }
            public string Save_NoSelBox { get; set; }

            public string Save_TypeSelBox { get; set; }
        
        }

        public class Load_status
        {
            public string Load_LineName {  get; set; }
            public string Load_NoSelBox { get; set; }

            public string Load_TypeSelBox { get; set; }
        }

        Save_status savestatus = new Save_status();//클래스 인스턴스 생성
        Load_status loadstatus = new Load_status();//클래스 인스턴스 생성

        /// <summary>Line_Setup 호기선택 박스에서 [상태저장] 시 저장된 호기 (반자동 명령 호기 일치 검사용)</summary>
        public static string SavedVehicleNo { get; set; } = "";

        public Line_Setup()
        {
            InitializeComponent();
        }



        private void button1_Click(object sender, EventArgs e)// 상태저장
        {
            savestatus.Save_LineName = line_sel_box.SelectedItem?.ToString() ?? "";
            savestatus.Save_NoSelBox = no_sel_box.SelectedItem?.ToString() ?? "";
            savestatus.Save_TypeSelBox = type_sel_box.SelectedItem?.ToString() ?? "";
            SavedVehicleNo = savestatus.Save_NoSelBox;
            //Console.WriteLine(savestatus.Save_LineName);
        }

        private void button2_Click(object sender, EventArgs e)// 상태갱신
        {
            // 1. 저장된 값 불러오기 (savestatus -> loadstatus)
            loadstatus.Load_LineName = savestatus.Save_LineName;
            loadstatus.Load_NoSelBox = savestatus.Save_NoSelBox;
            loadstatus.Load_TypeSelBox = savestatus.Save_TypeSelBox;

            // 2. 왼쪽 콤보박스 갱신
            line_sel_box.Text = loadstatus.Load_LineName;
            no_sel_box.Text = loadstatus.Load_NoSelBox;
            type_sel_box.Text = loadstatus.Load_TypeSelBox;

            // 3. 모든 체크박스 초기화
            for (int i = 1; i <= 16; i++)
            {
                // 이름으로 컨트롤을 찾습니다 (예: "checkBox1", "checkBox2"...)
                Control[] found = this.Controls.Find("checkBox" + i, true);
                if (found.Length > 0 && found[0] is CheckBox cb)
                {
                    cb.Checked = false;
                }
            }

            // 4. 선택된 호기에 맞는 체크박스만 체크
            // 예: loadstatus.Load_NoSelBox가 "8호기"라면 숫자 "8"만 추출
            string numOnly = System.Text.RegularExpressions.Regex.Replace(loadstatus.Load_NoSelBox, @"\D", "");

            if (!string.IsNullOrEmpty(numOnly))
            {
                Control[] found = this.Controls.Find("checkBox" + numOnly, true);
                if (found.Length > 0 && found[0] is CheckBox cb)
                {
                    cb.Checked = true;
                }
            }

        }

        // 모든 체크박스를 해제하는 메서드
        private void ClearAllCheckBoxes()
        {
            // 만약 체크박스들이 주황색 판넬 안에 있다면 'this' 대신 '판넬이름'을 넣으세요.
            foreach (Control c in this.Controls)
            {
                if (c is CheckBox cb) cb.Checked = false;
            }
        }


        private void SetCheckBoxByUnitName(string unitName)
        {
            foreach (Control c in this.Controls)
            {
                // 콤보박스에서 선택한 "8호기"라는 글자가 
                // 화면에 배치된 특정 라벨이나 체크박스의 이름/텍스트와 일치하는지 확인
                if (c is CheckBox cb)
                {
                    // 체크박스 자체의 텍스트가 "8호기"인 경우
                    if (cb.Text.Trim() == unitName.Trim())
                    {
                        cb.Checked = true;
                        break; // 찾았으면 중단
                    }
                }
            }
        }

    }
}
