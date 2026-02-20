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
        /// <summary>Line_Setup 라인선택에서 [상태저장] 시 저장된 라인명 (반자동 명령 라인 일치 검사용)</summary>
        public static string SavedLineName { get; set; } = "";
        /// <summary>Line_Setup 타입선택에서 [상태저장] 시 저장된 레일 타입 (SR50 레일 / SR150 레일). RIO 주소 2·3번 vs 4·5번 구분용.</summary>
        public static string SavedRailType { get; set; } = "";

        public Line_Setup()
        {
            InitializeComponent();
            line_sel_box.SelectedIndexChanged += Line_sel_box_SelectedIndexChanged;
            this.Load += Line_Setup_Load;
        }

        private void Line_Setup_Load(object sender, EventArgs e)
        {
            RefreshTypeListByLine();
        }

        private void Line_sel_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshTypeListByLine();
        }

        /// <summary>라인선택에 따라 타입선택 콤보를 SR50/SR50-H 또는 SR150/SR150-H만 보이도록 갱신</summary>
        private void RefreshTypeListByLine()
        {
            string line = line_sel_box.SelectedItem?.ToString() ?? "";
            object prevType = type_sel_box.SelectedItem;

            type_sel_box.Items.Clear();
            if (line.Contains("SR50") && !line.Contains("SR150"))
            {
                type_sel_box.Items.Add("SR50");
                type_sel_box.Items.Add("SR50-H");
            }
            else if (line.Contains("SR150"))
            {
                type_sel_box.Items.Add("SR150");
                type_sel_box.Items.Add("SR150-H");
            }

            if (type_sel_box.Items.Count > 0)
            {
                bool valid = false;
                if (prevType != null)
                {
                    string s = prevType.ToString();
                    valid = (line.Contains("SR50") && !line.Contains("SR150") && (s == "SR50" || s == "SR50-H"))
                         || (line.Contains("SR150") && (s == "SR150" || s == "SR150-H" || s == "S150"));
                }
                type_sel_box.SelectedIndex = valid ? type_sel_box.Items.IndexOf(prevType) : 0;
            }
        }

        /// <summary>현재 라인과 타입 조합이 허용된지 검사 (SR50 레일 → SR50/SR50-H, SR150 레일 → SR150/SR150-H)</summary>
        private bool IsLineTypeCombinationValid()
        {
            string line = line_sel_box.SelectedItem?.ToString() ?? "";
            string type = type_sel_box.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(type)) return true;
            if (line.Contains("SR50") && !line.Contains("SR150"))
                return type == "SR50" || type == "SR50-H";
            if (line.Contains("SR150"))
                return type == "SR150" || type == "SR150-H" || type == "S150";
            return true;
        }



        private void button1_Click(object sender, EventArgs e)// 상태저장
        {
            if (!IsLineTypeCombinationValid())
            {
                MessageBox.Show("라인과 타입이 맞지 않습니다.\r\nSR50 레일은 SR50, SR50-H만 / SR150 레일은 SR150, SR150-H만 선택 가능합니다.", "라인·타입 불일치", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            savestatus.Save_LineName = line_sel_box.SelectedItem?.ToString() ?? "";
            savestatus.Save_NoSelBox = no_sel_box.SelectedItem?.ToString() ?? "";
            savestatus.Save_TypeSelBox = type_sel_box.SelectedItem?.ToString() ?? "";
            SavedVehicleNo = savestatus.Save_NoSelBox;
            SavedLineName = savestatus.Save_LineName;
            SavedRailType = savestatus.Save_TypeSelBox;
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
