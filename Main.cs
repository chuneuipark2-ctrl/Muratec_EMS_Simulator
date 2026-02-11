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
using System.IO;


using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using System.Security.Cryptography.X509Certificates;


namespace EMS_TEST_SIMULATOR
{
    public partial class Main : Form
    {

        public static Main Instance  = new Main();

        // [추가] 엔코더 설정을 전담할 매니저 생성
        public Encoder_Setting_Manager _encManager = new Encoder_Setting_Manager();
        private EMS_TCP_UDP_Connect SKY_RAV_CONNECT_FORM;
        public IDeviceComm GlobalComm; // 통신 객체를 담아둘 공용 보관함








        public Main()
        {
            InitializeComponent();
        }


        public class ErrorItem
        {
            public string Code { get; set; }
            public string Name { get; set; }

            public ErrorItem(string code, string name)
            {
                Code = code;
                Name = name;
            }
        }


        public class ResponseCodeItems
        {
            public string Code { get; set; }
            public string Name { get; set; }

            public ResponseCodeItems(string code, string name)
            {
                Code = code;
                Name = name;
            }
        }

        public class target_operating_code_itmes
        {
            public string Code { get; set; }

            public string Name { get; set; }

            public target_operating_code_itmes(string code, string name)
            {
                Code = code;
                Name = name;
            }

        }

        public class Operating_Code_Items
        {
            public string Code { get; set; }
            public string Name { get; set; }

            public Operating_Code_Items(string code, string name)
            {
                Code = code;
                Name = name;
            }
        }





        private void Form1_Load(object sender, EventArgs e)
        {
         
            
            ResponseCodeList();//응답코드 리스트뷰
            taget_operating_code_list(); // 타겟동작 코드리스트
            Operating_Code_list(); // 동작 코드리스트
            UploadCodeList();//에러 리스트뷰
            _encManager.Initialize(this);


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

        private void  Rail_io_Click(object sender, EventArgs e)
        {
            // 1. 연결 시도
            bool isAllConnected = Rail_DIO.Instance.FASTECH_CONNECT();

            // 2. 텍스트박스 초기화
            tBox1.Clear();

            if (isAllConnected)
            {
                UpdateRailButtonStatus(true);
                tBox1.AppendText("모든 장비 연결 성공 (192.168.0.20 ~ 24)" + Environment.NewLine);

                PANEL_DIO panel = new PANEL_DIO();
                panel.Show();
            }
            else
            {
                UpdateRailButtonStatus(false);
                Rail_io.BackColor = Color.Orange;


                PANEL_DIO panel = new PANEL_DIO(); //임시사용
                panel.Show();

                // 3. 에러가 난 IP들만 골라서 텍스트박스에 표기
                tBox1.AppendText("--- 연결 실패 목록 ---" + Environment.NewLine);
                foreach (string errorIp in Rail_DIO.Instance.ErrorIPs)
                {
                    tBox1.AppendText($"[실패] {errorIp}" + Environment.NewLine);
                }

                MessageBox.Show("일부 장비 연결에 실패했습니다. 텍스트박스를 확인하세요.");
            }

        }

        public void UpdateConnectionStatus(bool connected)
        {
            if (connected)
            {
                Rail_io.BackColor = Color.Lime;
            }
            else
            {
                Rail_io.BackColor = Color.DarkGray; // 기본 색상
            }
        }


        public void UpdateRailButtonStatus(bool isConnected)
        {
            // UI 스레드 안전성 검사 (다른 스레드에서 호출할 경우를 대비)
            if (Rail_io.InvokeRequired)
            {
                Rail_io.Invoke(new Action(() => UpdateRailButtonStatus(isConnected)));
                return;
            }

            if (isConnected)
            {
                // 연결 성공 시: 녹색
                Rail_io.BackColor = Color.Lime;
                Rail_io.Text = "Rail I.O (Connected)";
            }
            else
            {
                // 연결 실패/해제 시: 기본색(회색)
                Rail_io.BackColor = Color.FromKnownColor(KnownColor.Control);
                Rail_io.Text = "Rail I.O (Disconnected)";
            }
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void elementHost1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void lbl_response_code_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_operating_code_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_error_code_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_transfer_data_no_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_vehicle_mode_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_load_status_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_transfer_command_Received_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_target_mode_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_current_point_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_target_point_text_Click(object sender, EventArgs e)
        {

        }

        private void lbl_error_point_text_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //에러코드 추가

        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //에러코드 불러들이는 부분
         }


        private void UploadCodeList()
        {
            // 1. 리스트뷰 설정 초기화 (디자인 타임에서 안 했을 경우를 대비)
            listView2.View = View.Details;
            listView2.GridLines = true;
            listView2.FullRowSelect = true;

            // 2. 99개의 데이터를 담을 리스트 (예시 데이터 3개만 적었지만 99개까지 추가 가능)
            List<ErrorItem> errorList = new List<ErrorItem>();
            errorList.Add(new ErrorItem("01", "주행 인버터 이상(주행중)"));
            errorList.Add(new ErrorItem("02", "팬모터 이상"));
            errorList.Add(new ErrorItem("03", "인버터 운전중 신호이상(주행중)"));
            errorList.Add(new ErrorItem("04", "인버터 운전중 신호출력이상(주행중)"));
            errorList.Add(new ErrorItem("05", "-"));
            errorList.Add(new ErrorItem("06", "이재 모터 과부하(주행중)"));
            errorList.Add(new ErrorItem("07", "주행 모터 과부하"));
            errorList.Add(new ErrorItem("08", "인버터 주파수검출 ON 대기이상/승강타이밍벨트 이상(주행중)"));
            errorList.Add(new ErrorItem("09", "인버터 주파수검출 OFF 대기이상/센서네트 경보(주행중) "));
            errorList.Add(new ErrorItem("10", "인버터 이상(승강중)"));
            errorList.Add(new ErrorItem("11", "하강 FINAL 이상"));
            errorList.Add(new ErrorItem("12", "하강 원점 확인 이상"));
            errorList.Add(new ErrorItem("13", "인버터 운전중 신호 이상(승강중)"));
            errorList.Add(new ErrorItem("14", "상승 원점 확인 이상"));
            errorList.Add(new ErrorItem("15", "엔코더 이상"));
            errorList.Add(new ErrorItem("16", "CHUCK 모터 과부하(승강중)"));
            errorList.Add(new ErrorItem("17", "이재 모터 과부하(승강중)"));
            errorList.Add(new ErrorItem("18", "승강 타이밍 벨트 이상(승강중)"));
            errorList.Add(new ErrorItem("19", "SENSOR NET 이상(승강중)"));
            errorList.Add(new ErrorItem("20", "승강 원점 이상(주행중)"));
            errorList.Add(new ErrorItem("21", "카운트 미스"));
            errorList.Add(new ErrorItem("22", "CHUCK 열림 이상(주행중)"));
            errorList.Add(new ErrorItem("23", "CHUCK 닫힘 이상(주행중)"));
            errorList.Add(new ErrorItem("24", "CHUCK 화물 있을 때 열림 이상(주행중)"));
            errorList.Add(new ErrorItem("25", "CHUCK 화물 있을 떄 닫힘 이상(주행중)"));
            errorList.Add(new ErrorItem("26", "화물 적재 이상(주행중)"));

            errorList.Add(new ErrorItem("27", "화물 돌출 감지 이상(주행중)"));
            errorList.Add(new ErrorItem("28", "교신점 확인 이상"));
            errorList.Add(new ErrorItem("29", "CHUCK 개폐 리미트 스위치 이상"));
            errorList.Add(new ErrorItem("30", "승강대 원점 이상(승강 에러 복구시)"));
            errorList.Add(new ErrorItem("31", "CHUCK 열림 이상(승강중)"));
            errorList.Add(new ErrorItem("32", "CHUCK 닫힘 이상(승강중)"));
            errorList.Add(new ErrorItem("33", "화물 CATCH 이상"));
            errorList.Add(new ErrorItem("34", "화물 UNCATCH 이상"));
            errorList.Add(new ErrorItem("35", "화물 있음 이상"));
            errorList.Add(new ErrorItem("36", "화물 없음 이상"));
            errorList.Add(new ErrorItem("37", "화물 돌출 감지 이상(승강중)"));
            errorList.Add(new ErrorItem("38", "승강대 적재 이상(승강중)"));
            errorList.Add(new ErrorItem("39", "선입품 이상"));
            errorList.Add(new ErrorItem("40", "패리티 이상"));
            errorList.Add(new ErrorItem("41", "정지 미스"));
            errorList.Add(new ErrorItem("42", "주행중 전원 차단 이상"));
            errorList.Add(new ErrorItem("43", "수동 절환"));
            errorList.Add(new ErrorItem("44", "하강 정지위치 결정 센서 에러"));
            errorList.Add(new ErrorItem("45", "승강중 정지 미스"));
            errorList.Add(new ErrorItem("46", "승강중 전원 차단 이상"));
            errorList.Add(new ErrorItem("47", "승강 위치 결정 이상"));
            errorList.Add(new ErrorItem("48", "승강대 화물 걸림 이상"));
            errorList.Add(new ErrorItem("49", "선입품 AREA 센서 이상"));
            errorList.Add(new ErrorItem("50", "정위치 확인 입력 없음 이상"));
            errorList.Add(new ErrorItem("51", "정위치 확인 복수 입력 이상"));
            errorList.Add(new ErrorItem("52", "정위치 확인 데이터 없음 이상"));
            errorList.Add(new ErrorItem("53", "주행중 수신 감시 이상"));

            errorList.Add(new ErrorItem("54", "주행중 이상정지 지시"));
            errorList.Add(new ErrorItem("55", "승강중 수신 감시 이상"));
            errorList.Add(new ErrorItem("56", "이재중 이상정지 지시"));
            errorList.Add(new ErrorItem("57", "화물 탑재 가능 시간 초과 보고"));
            errorList.Add(new ErrorItem("58", "화물 이재 가능 시간 초과 보고"));
            errorList.Add(new ErrorItem("59", "선입품 이상 보고"));
            errorList.Add(new ErrorItem("60", "화물 탑재 주행 시작시 CHUCK 상태 이상"));
            errorList.Add(new ErrorItem("61", "화물 이재 주행 시작시 CHUCK 상태 이상"));
            errorList.Add(new ErrorItem("62", "-"));
            errorList.Add(new ErrorItem("63", "-"));
            errorList.Add(new ErrorItem("64", "화물 탑재 주행 시작시 적재상태 이상"));
            errorList.Add(new ErrorItem("65", "화물 이재 주행 시작시 적재상태 이상"));
            errorList.Add(new ErrorItem("66", "인터록 미검출 이상"));
            errorList.Add(new ErrorItem("67", "승강중 SET 가능 신호 이상"));
            errorList.Add(new ErrorItem("68", "반송지령 패리티 이상"));
            errorList.Add(new ErrorItem("69", "반송지령 데이터 이상"));
            errorList.Add(new ErrorItem("70", "승강 인터록 시간 초과"));
            errorList.Add(new ErrorItem("71", "컨베이어 이재 시간 초과"));
            errorList.Add(new ErrorItem("72", "주행 시간 초과"));
            errorList.Add(new ErrorItem("73", "CHUCK 동작 시간 초과"));
            errorList.Add(new ErrorItem("74", "교신 스트로브 OFF 시간 초과"));
            errorList.Add(new ErrorItem("75", "자동 주행 운전 가능 시간 초과"));
            errorList.Add(new ErrorItem("76", "이재 시간 초과"));
            errorList.Add(new ErrorItem("77", "상승 간으 시간 초과"));
            errorList.Add(new ErrorItem("78", "초기 주행 발진 가능 시간 초과"));

            errorList.Add(new ErrorItem("79", "반송개시 시간 초과"));
            errorList.Add(new ErrorItem("80", "-"));
            errorList.Add(new ErrorItem("81", "섹션 카운트 에러"));
            errorList.Add(new ErrorItem("82", "동작 모드 에러"));
            errorList.Add(new ErrorItem("83", "반송지령 수신 불가 이상"));
            errorList.Add(new ErrorItem("84", "이재 에러"));
            errorList.Add(new ErrorItem("85", "데이터 번호 중복 에러"));
            errorList.Add(new ErrorItem("86", "전문 ID 에러"));
            errorList.Add(new ErrorItem("87", "전문 길이 에러"));
            errorList.Add(new ErrorItem("88", "하드웨어 에러"));
            errorList.Add(new ErrorItem("89", "BCC 에러"));
            errorList.Add(new ErrorItem("90", "-"));
            errorList.Add(new ErrorItem("91", "프로그램 섬 체크 이상"));
            errorList.Add(new ErrorItem("92", "플래시 메모리 에러"));
            errorList.Add(new ErrorItem("93", "시스템 파라미터 섬 체크 에러"));
            errorList.Add(new ErrorItem("94", "워치독 이상"));
            errorList.Add(new ErrorItem("95", "순시 정전 이상"));
            errorList.Add(new ErrorItem("96", "예외 처리 이상"));
            errorList.Add(new ErrorItem("97", "주행 구동 금지 이상"));
            errorList.Add(new ErrorItem("98", "구동 금지 승강 이상"));
            errorList.Add(new ErrorItem("99", "시퀀스 번호 이상"));



            // ... 나머지 99개까지 .Add 하시면 됩니다.

            // 3. ListView 업데이트 속도 향상을 위해 BeginUpdate 사용
            listView2.BeginUpdate();
            listView2.Items.Clear(); // 기존 데이터 삭제

            // 4. 반복문(foreach)을 돌며 ListView에 아이템 업로드
            foreach (var item in errorList)
            {
                // 첫 번째 컬럼(코드) 생성
                ListViewItem lvi = new ListViewItem(item.Code);
                // 두 번째 컬럼(명칭) 추가
                lvi.SubItems.Add(item.Name);

                // 최종 업로드
                listView2.Items.Add(lvi);
            }

            listView2.EndUpdate(); // 업데이트 종료 및 화면 갱신

        }

        private void listView4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void ResponseCodeList()
        {
            listView1.View = View.Details; // 표형태의 보기를 의미
            listView1.GridLines = true; // 그리드 입력
            listView1.FullRowSelect = true; //사용자가 행 선택가능

            listView1.Items.Clear();

            //메모리내 데이터 구조화 작업//
            // ResponseCodeItem이라는 사용자 정의 클래스(또는 구조체)를 객체화 하여 리스트에 담는다.
            // UI에 직접 넣기 전에 메모리(list<T>)에서 먼저 데이터를 관리하는 것이 유지보수에 유리하다.

            List<ResponseCodeItems> responseCodeItems = new List<ResponseCodeItems>();
            responseCodeItems.Add(new ResponseCodeItems("00","정상 수신 완료"));
            responseCodeItems.Add(new ResponseCodeItems("01", "Header 미검출"));
            responseCodeItems.Add(new ResponseCodeItems("02", "-"));
            responseCodeItems.Add(new ResponseCodeItems("03", "수신 문자 초과"));
            responseCodeItems.Add(new ResponseCodeItems("04", "체크섬 에러"));
            responseCodeItems.Add(new ResponseCodeItems("05", "하드 에러"));
            responseCodeItems.Add(new ResponseCodeItems("06", "-"));
            responseCodeItems.Add(new ResponseCodeItems("07", "-"));
            responseCodeItems.Add(new ResponseCodeItems("08", "수신제 에러"));
            responseCodeItems.Add(new ResponseCodeItems("09", "시퀀스 순서 에러"));

            responseCodeItems.Add(new ResponseCodeItems("10", "-"));
            responseCodeItems.Add(new ResponseCodeItems("11", "-"));
            responseCodeItems.Add(new ResponseCodeItems("12", "-"));
            responseCodeItems.Add(new ResponseCodeItems("13", "-"));
            responseCodeItems.Add(new ResponseCodeItems("14", "-"));
            responseCodeItems.Add(new ResponseCodeItems("15", "-"));
            responseCodeItems.Add(new ResponseCodeItems("16", "-"));
            responseCodeItems.Add(new ResponseCodeItems("17", "-"));
            responseCodeItems.Add(new ResponseCodeItems("18", "-"));
            responseCodeItems.Add(new ResponseCodeItems("19", "-"));

            responseCodeItems.Add(new ResponseCodeItems("20", "-"));
            responseCodeItems.Add(new ResponseCodeItems("21", "-"));
            responseCodeItems.Add(new ResponseCodeItems("22", "-"));
            responseCodeItems.Add(new ResponseCodeItems("23", "-"));
            responseCodeItems.Add(new ResponseCodeItems("24", "-"));
            responseCodeItems.Add(new ResponseCodeItems("25", "-"));
            responseCodeItems.Add(new ResponseCodeItems("26", "-"));
            responseCodeItems.Add(new ResponseCodeItems("27", "-"));
            responseCodeItems.Add(new ResponseCodeItems("28", "-"));
            responseCodeItems.Add(new ResponseCodeItems("29", "-"));

            responseCodeItems.Add(new ResponseCodeItems("30", "-"));
            responseCodeItems.Add(new ResponseCodeItems("31", "-"));
            responseCodeItems.Add(new ResponseCodeItems("32", "-"));
            responseCodeItems.Add(new ResponseCodeItems("33", "-"));
            responseCodeItems.Add(new ResponseCodeItems("34", "-"));
            responseCodeItems.Add(new ResponseCodeItems("35", "-"));
            responseCodeItems.Add(new ResponseCodeItems("36", "-"));
            responseCodeItems.Add(new ResponseCodeItems("37", "-"));
            responseCodeItems.Add(new ResponseCodeItems("38", "-"));
            responseCodeItems.Add(new ResponseCodeItems("39", "-"));

            responseCodeItems.Add(new ResponseCodeItems("40", "ID 에러"));
            responseCodeItems.Add(new ResponseCodeItems("41", "전문 길이 에러"));
            responseCodeItems.Add(new ResponseCodeItems("42", "포인트 No. 에러"));
            responseCodeItems.Add(new ResponseCodeItems("43", "-"));
            responseCodeItems.Add(new ResponseCodeItems("44", "현재 동작중 에러"));
            responseCodeItems.Add(new ResponseCodeItems("45", "이재 포인트 에러"));
            responseCodeItems.Add(new ResponseCodeItems("46", "지시모드 에러"));
            responseCodeItems.Add(new ResponseCodeItems("47", "데이터 취소 이상"));
            responseCodeItems.Add(new ResponseCodeItems("48", "초기주행중 이상"));
            responseCodeItems.Add(new ResponseCodeItems("49", "수동중 이상"));

            responseCodeItems.Add(new ResponseCodeItems("50", "현재 분기중 이상"));
            responseCodeItems.Add(new ResponseCodeItems("51", "-"));
            responseCodeItems.Add(new ResponseCodeItems("52", "-"));
            responseCodeItems.Add(new ResponseCodeItems("53", "-"));
            responseCodeItems.Add(new ResponseCodeItems("54", "-"));
            responseCodeItems.Add(new ResponseCodeItems("55", "-"));
            responseCodeItems.Add(new ResponseCodeItems("56", "-"));
            responseCodeItems.Add(new ResponseCodeItems("57", "-"));
            responseCodeItems.Add(new ResponseCodeItems("58", "-"));
            responseCodeItems.Add(new ResponseCodeItems("59", "-"));

            responseCodeItems.Add(new ResponseCodeItems("60", "-"));
            responseCodeItems.Add(new ResponseCodeItems("61", "-"));
            responseCodeItems.Add(new ResponseCodeItems("62", "-"));
            responseCodeItems.Add(new ResponseCodeItems("63", "-"));
            responseCodeItems.Add(new ResponseCodeItems("64", "-"));
            responseCodeItems.Add(new ResponseCodeItems("65", "-"));
            responseCodeItems.Add(new ResponseCodeItems("66", "-"));
            responseCodeItems.Add(new ResponseCodeItems("67", "-"));
            responseCodeItems.Add(new ResponseCodeItems("68", "-"));
            responseCodeItems.Add(new ResponseCodeItems("69", "-"));

            responseCodeItems.Add(new ResponseCodeItems("70", "-"));
            responseCodeItems.Add(new ResponseCodeItems("71", "-"));
            responseCodeItems.Add(new ResponseCodeItems("72", "-"));
            responseCodeItems.Add(new ResponseCodeItems("73", "-"));
            responseCodeItems.Add(new ResponseCodeItems("74", "-"));
            responseCodeItems.Add(new ResponseCodeItems("75", "-"));
            responseCodeItems.Add(new ResponseCodeItems("76", "-"));
            responseCodeItems.Add(new ResponseCodeItems("77", "-"));
            responseCodeItems.Add(new ResponseCodeItems("78", "-"));
            responseCodeItems.Add(new ResponseCodeItems("79", "-"));

            responseCodeItems.Add(new ResponseCodeItems("80", "-"));
            responseCodeItems.Add(new ResponseCodeItems("81", "-"));
            responseCodeItems.Add(new ResponseCodeItems("82", "-"));
            responseCodeItems.Add(new ResponseCodeItems("83", "-"));
            responseCodeItems.Add(new ResponseCodeItems("84", "-"));
            responseCodeItems.Add(new ResponseCodeItems("85", "-"));
            responseCodeItems.Add(new ResponseCodeItems("86", "-"));
            responseCodeItems.Add(new ResponseCodeItems("87", "-"));
            responseCodeItems.Add(new ResponseCodeItems("88", "-"));
            responseCodeItems.Add(new ResponseCodeItems("89", "-"));

            responseCodeItems.Add(new ResponseCodeItems("90", "-"));
            responseCodeItems.Add(new ResponseCodeItems("91", "-"));
            responseCodeItems.Add(new ResponseCodeItems("92", "-"));
            responseCodeItems.Add(new ResponseCodeItems("93", "-"));
            responseCodeItems.Add(new ResponseCodeItems("94", "-"));
            responseCodeItems.Add(new ResponseCodeItems("95", "-"));
            responseCodeItems.Add(new ResponseCodeItems("96", "-"));
            responseCodeItems.Add(new ResponseCodeItems("97", "-"));
            responseCodeItems.Add(new ResponseCodeItems("98", "-"));
            responseCodeItems.Add(new ResponseCodeItems("99", "-"));

           listView1.BeginUpdate(); // 그리기 이벤트 일시중지, 아이템을 99번 추가할때마다 화면을 새로고침하면 깜빡임이 발생하고, 속도가 매우느려짐
                                   // 이를 방지하기 위한 코드라고 보면된다.


            foreach(var item in responseCodeItems)
            {
                ListViewItem lvi = new ListViewItem(item.Code); 
                //ListViewItem은 행(Row) 하나를 의미한다.
                //생성자 인자로 들어가는 item.Code는 첫 번째 열의 텍스트가된다.
                lvi.SubItems.Add(item.Name);
                //SubItems는 두번째 열부터 순서대로 들어가는 상세 데이터이다.
                //여기서는 item.Name이 두 번째 열(에러명칭)에 배치된다.
                listView1.Items.Add(lvi);
            }
            listView1.EndUpdate();
        }


        private void taget_operating_code_list()
            {
            listView3.View = View.Details;
            listView3.GridLines = true;
            listView3.FullRowSelect = true;
        
            listView3.Items.Clear();

            List<target_operating_code_itmes> taget_Operating_Code_Items = new List<target_operating_code_itmes>();

            taget_Operating_Code_Items.Add(new target_operating_code_itmes("0","이동"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("1","화물 적재동작"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("2","화물 이재동작"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("3", "승강"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("4", "-"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("5", "-"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("6", "-"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("7", "-"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("8", "-"));
            taget_Operating_Code_Items.Add(new target_operating_code_itmes("9", "-"));

            listView3.BeginUpdate();

            foreach(var item in taget_Operating_Code_Items)
            {
                ListViewItem lvi = new ListViewItem(item.Code);
                lvi.SubItems.Add(item.Name);

                listView3.Items.Add(lvi);
            }

            listView3.EndUpdate();



        }

        private void Operating_Code_list()
        {
            listView4.View = View.Details;
            listView4.GridLines = true;
            listView4.FullRowSelect = true;
            listView4.Items.Clear();

            List<Operating_Code_Items> operating_Code_Items = new List<Operating_Code_Items>();

            operating_Code_Items.Add(new Operating_Code_Items("0","수동운전모드"));
            operating_Code_Items.Add(new Operating_Code_Items("1", "초기주행 전진"));
            operating_Code_Items.Add(new Operating_Code_Items("2", "초기주행 후진"));
            operating_Code_Items.Add(new Operating_Code_Items("3", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("4", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("5", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("6", "충돌방지 감속"));
            operating_Code_Items.Add(new Operating_Code_Items("7", "인터록 정지"));
            operating_Code_Items.Add(new Operating_Code_Items("8", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("9", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("10", "주행 전진"));
            operating_Code_Items.Add(new Operating_Code_Items("11", "주행 후진"));
            operating_Code_Items.Add(new Operating_Code_Items("12", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("13", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("14", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("15", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("16", "충돌방지 감속"));
            operating_Code_Items.Add(new Operating_Code_Items("17", "충돌방지 정지"));
            operating_Code_Items.Add(new Operating_Code_Items("18", "인터록 정지"));
            operating_Code_Items.Add(new Operating_Code_Items("19", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("20", "교신 준비"));
            operating_Code_Items.Add(new Operating_Code_Items("21", "교신 요구 송신"));
            operating_Code_Items.Add(new Operating_Code_Items("22", "교신 지령 수신"));
            operating_Code_Items.Add(new Operating_Code_Items("23", "교신 수신 완료 송신"));
            operating_Code_Items.Add(new Operating_Code_Items("24", "교신 반송 개시 수신"));
            operating_Code_Items.Add(new Operating_Code_Items("25", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("26", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("27", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("28", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("29", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("30", "목적지 변경"));
            operating_Code_Items.Add(new Operating_Code_Items("31", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("32", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("33", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("34", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("35", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("36", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("37", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("38", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("39", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("40", "분기전 감속"));
            operating_Code_Items.Add(new Operating_Code_Items("41", "분기전 카운트 정지"));
            operating_Code_Items.Add(new Operating_Code_Items("42", "분기 탑승 정지"));
            operating_Code_Items.Add(new Operating_Code_Items("43", "분기 출력"));
            operating_Code_Items.Add(new Operating_Code_Items("44", "분기 발진"));
            operating_Code_Items.Add(new Operating_Code_Items("45", "분기 탑승 출력"));
            operating_Code_Items.Add(new Operating_Code_Items("46", "충돌방지 감속"));
            operating_Code_Items.Add(new Operating_Code_Items("47", "충돌방지 정지"));
            operating_Code_Items.Add(new Operating_Code_Items("48", "인터록 정지"));
            operating_Code_Items.Add(new Operating_Code_Items("49", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("50", "IDLE 정지중"));
            operating_Code_Items.Add(new Operating_Code_Items("51", "선입품 에러 발생 지시대기"));
            operating_Code_Items.Add(new Operating_Code_Items("52", "SET가능 에러 지시대기"));
            operating_Code_Items.Add(new Operating_Code_Items("53", "이동 주행 에러 지시대기"));
            operating_Code_Items.Add(new Operating_Code_Items("54", "화물탑재전 주행 에러 지시대기"));
            operating_Code_Items.Add(new Operating_Code_Items("55", "리트라이 지시대기"));
            operating_Code_Items.Add(new Operating_Code_Items("56", "화물반출전 주행 에러 지시대기"));
            operating_Code_Items.Add(new Operating_Code_Items("57", "화물탑재 이상 지시대기"));
            operating_Code_Items.Add(new Operating_Code_Items("58", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("59", "일시정지중"));

            operating_Code_Items.Add(new Operating_Code_Items("60", "하강 준비"));
            operating_Code_Items.Add(new Operating_Code_Items("61", "하강 고속"));
            operating_Code_Items.Add(new Operating_Code_Items("62", "하강 저속"));
            operating_Code_Items.Add(new Operating_Code_Items("63", "하강 카운트 타이머 대기"));
            operating_Code_Items.Add(new Operating_Code_Items("64", "하강 정지1"));
            operating_Code_Items.Add(new Operating_Code_Items("65", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("66", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("67", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("68", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("69", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("70", "화물 탑재"));
            operating_Code_Items.Add(new Operating_Code_Items("71", "화물 반출"));
            operating_Code_Items.Add(new Operating_Code_Items("72", "케이지 상승가능 대기"));
            operating_Code_Items.Add(new Operating_Code_Items("73", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("74", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("75", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("76", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("77", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("78", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("79", "하강 정지2"));

            operating_Code_Items.Add(new Operating_Code_Items("80", "상승 준비"));
            operating_Code_Items.Add(new Operating_Code_Items("81", "상승 저속"));
            operating_Code_Items.Add(new Operating_Code_Items("82", "상승 고속"));
            operating_Code_Items.Add(new Operating_Code_Items("83", "상승 원점 전 감속"));
            operating_Code_Items.Add(new Operating_Code_Items("84", "상승정지"));
            operating_Code_Items.Add(new Operating_Code_Items("85", "상승종료"));
            operating_Code_Items.Add(new Operating_Code_Items("86", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("87", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("88", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("89", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("90", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("91", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("92", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("93", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("94", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("95", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("96", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("97", "-"));
            operating_Code_Items.Add(new Operating_Code_Items("98", "-"));

            operating_Code_Items.Add(new Operating_Code_Items("99", "-"));

            listView4.BeginUpdate();

            foreach(var item in operating_Code_Items)
            {
                ListViewItem lvi = new ListViewItem(item.Code);
                lvi.SubItems.Add(item.Name);
                listView4.Items.Add(lvi);

            }
            listView4.EndUpdate();  


        }





        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void listView3_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void listView4_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void event_log_listview_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Line_Setup line_Setup_func = new Line_Setup();

            line_Setup_func.ShowDialog();

        }

        private void btn_Command_Click(object sender, EventArgs e)
        {
           Command_Form command_Form = new Command_Form();

            // [이것이 Owner 설정!] 
            // cmdForm의 주인은 나(this = Main)라고 알려주는 겁니다.
            command_Form.Owner = this;

            command_Form.ShowDialog();

        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (SKY_RAV_CONNECT_FORM == null || SKY_RAV_CONNECT_FORM.IsDisposed)
            {
                SKY_RAV_CONNECT_FORM = new EMS_TCP_UDP_Connect();
                SKY_RAV_CONNECT_FORM.Owner = this;

                // [수정 포인트] VisibleChanged 이벤트에서 색상 변경 조건을 확인합니다.
                SKY_RAV_CONNECT_FORM.VisibleChanged += (s, args) =>
                {
                    // Connect 창이 연결 성공 후 Close() 되거나 Hide() 되었을 때
                    if (!SKY_RAV_CONNECT_FORM.Visible && SKY_RAV_CONNECT_FORM.DialogResult == DialogResult.OK)
                    {
                        // 통신 객체 안전하게 복사
                        if (this.GlobalComm == null && SKY_RAV_CONNECT_FORM._comm != null)
                        {
                            this.GlobalComm = SKY_RAV_CONNECT_FORM._comm;
                        }

                        // 버튼 색상을 연두색으로 변경
                        button2.BackColor = Color.Lime; // 또는 Color.GreenYellow
                        button2.ForeColor = Color.Black; // 검은색 글씨가 연두색에서 잘 보입니다.
                        button2.Text = "연결 완료";
                    }
                };
            }

            SKY_RAV_CONNECT_FORM.Show();
            SKY_RAV_CONNECT_FORM.BringToFront();
        }



        private void button7_Click(object sender, EventArgs e)
        {
            // 메세지 박스 띄우기(문구, 제목, 버튼 종류, 아이콘)
            DialogResult result = MessageBox.Show("자동모드를 실행하시겠습니까?","상태확인",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
           



            if (result == DialogResult.Yes)
            {
                //반자동 완료 FLAG 확인, 안전하게 사용하기 위해서 반자동모드가 완료되고, 플래그 비트가 1이되어야만 시작할 수 있는 조건을 건다.

                EMS_Mode_Sequence emsSeq = new EMS_Mode_Sequence();

                MessageBox.Show("레일 주변에 장애물이 없는지 확인한후 진행하시기 바랍니다.");

                //emsSeq.ProcessMode(3);//자동모드 실행

                

            }
            else
            {
                MessageBox.Show("취소하였습니다.");
            }

        }

        private void button8_Click(object sender, EventArgs e)//비상정지 버튼을 눌렀을때, 평상시 모드전환
        {
            EMS_Mode_Sequence emsProcMode = new EMS_Mode_Sequence();
            //emsProcMode.ProcessMode(0);//에러시 모드로 전환
            
       

        }

        private void button5_Click(object sender, EventArgs e)
        {
            // 콤보박스(101~113)와 입력 텍스트박스의 값을 매니저에게 전달
            // 매니저 내부에서 0~530 체크 후 lift_enc_datas에 저장하고 라벨을 바꿉니다.
            // comboBox1: 호기번호, comboBox2: 포지션설정, textBox1: 엔코더입력
            _encManager.ExecuteSave(comboBox1.Text, comboBox2.Text, textBox1.Text);
        }

        // [현재값 갱신] 버튼 클릭 이벤트 (버튼 이름을 btnEncoderRefresh라고 가정)
        private void btnEncoderRefresh_Click(object sender, EventArgs e)
        {
            string vID = comboBox1.Text;
            string pos = comboBox2.Text;

            int val = _encManager.GetStoredValue(vID, pos);
            textBox1.Text = val.ToString();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
{
  
}

        private void button6_Click(object sender, EventArgs e)
        {
            string selectedVehicle = comboBox1.Text;

            // 1. 선택된 호기의 모든 데이터를 우측 라벨 표(101~113)에 표시
            _encManager.DisplayVehicleData(selectedVehicle);

            // 2. 현재 선택된 포지션의 값만 텍스트박스(입력창)에도 표시
            int currentVal = _encManager.GetStoredValue(selectedVehicle, comboBox2.Text);
            textBox1.Text = currentVal.ToString();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("정말로 모든 데이터를 초기화하시겠습니까?", "확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _encManager.ClearAllData();
                textBox1.Clear();
            }
        }


        public void ResetConnectButton()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(ResetConnectButton));
                return;
            }

            button2.BackColor = SystemColors.Control; // 기본 회색으로 복구
            button2.ForeColor = Color.Black;
            button2.Text = "Connect"; // 원래 텍스트로 복구
        }


    }
}
