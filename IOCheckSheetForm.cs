using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EMS_TEST_SIMULATOR
{
    /// <summary>DIO I.O 체크 성적서 폼. 마스터 CSV 기반 동적 I/O 표시.</summary>
    public partial class IOCheckSheetForm : Form
    {
        private DataTable _allIoTable = new DataTable();

        // 내장 INPUT/OUTPUT 데이터 (파일 로드 대신 하드코딩)
        // 구조: 어드레스, BIT, 기본신호, 기본논리, CHUCK, CHUCK논리, CAGE, CAGE논리, 컨베이어, 컨베이어논리,
        //       충돌방지, 충돌방지논리, ROP, ROP논리, 8bit, 8bit논리, SS무선, SS무선논리, 직선, 직선논리, 분기, 분기논리, 옵션, 옵션논리
        private static readonly List<string[]> EmbeddedInputData = new List<string[]>
        {
            // === 800010 (I/O기판 IN) ===
            new[] { "800010", "0", "승강대신호1", "", "승강대적재1", "b", "승강대적재1", "b", "승강대 좌탑재", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "1", "승강대신호2", "", "", "", "", "", "승강대 우탑재", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "2", "승강대신호3", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "3", "승강대신호4", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "4", "승강대신호5", "", "CHUCK 열림단", "a", "우측 돌출감지", "b", "우측 돌출감지", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "5", "승강대신호6", "", "CHUCK 닫힘단", "a", "좌측 돌출감지", "b", "좌측 돌출감지", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "6", "승강대신호7", "", "", "", "", "", "승강정지우", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "7", "승강대신호8", "", "", "", "", "", "승강정지좌", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "8", "승강부원점", "a", "승강부원점", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "9", "느슨해짐 검출", "b", "느슨해짐 검출", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "10", "승강 엔코더", "a", "승강 엔코더", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "11", "인버터 이상", "b", "인버터 이상", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "12", "인버터 운전중", "a", "인버터 운전중", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "13", "인버터 주파수 검출", "a", "인버터 주파수 검출", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "14", "트롤리제어(인터록)", "a", "트롤리제어(인터록)", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "15", "충돌검출(유지)", "b", "", "", "", "", "", "", "충돌검출(유지)", "b", "", "", "", "", "", "", "", "", "", "", "", "" },
            // === 800012 (I/O기판 IN) ===
            new[] { "800012", "0", "극한검출(릴레이접점)", "b", "", "", "", "", "", "", "극한검출(릴레이접점)", "b", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "1", "구동금지", "a", "구동금지", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "2", "주행모터 브레이크 개방 스위치", "a", "주행모터 브레이크 개방 스위치", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "3", "이재모터1 브레이크 개방 스위치", "a", "이재모터1 브레이크 개방 스위치", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "4", "주행모터 써멀 트립", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "5", "이재모터 전원 이상", "b", "", "", "이재모터 전원 이상", "b", "", "", "이재모터 전원 이상", "b", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "6", "이재모터1 과부하", "a", "", "", "이재모터1 과부하", "a", "", "", "이재모터1 과부하", "a", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "7", "예비(미사용)", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "8", "예비(미사용)", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "9", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "10", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "11", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "12", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "13", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "14", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "15", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            // === 800014 (I/O기판 IN) ===
            new[] { "800014", "0", "이재인터록1", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "1", "이재인터록2", "a", "", "", "화물탑재 가능", "a", "상승가", "a", "화물탑재 가능", "a", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "2", "이재인터록3", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "3", "이재인터록4", "a", "", "", "화물이재 가능", "a", "하강가", "a", "화물이재 가능", "a", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "4", "이재인터록5", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "5", "이재인터록6", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "6", "이재인터록7", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "7", "이재인터록8", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "8", "정지1", "a", "정지1", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "9", "정지2", "a", "정지2", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "10", "정지패리티", "a", "정지패리티", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "11", "충돌방지전 감속", "b", "", "", "", "", "", "", "", "", "충돌방지전 감속", "b", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "12", "충돌방지전 정지", "b", "", "", "", "", "", "", "", "", "충돌방지전 정지", "b", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "13", "충돌방지후 감속", "b", "", "", "", "", "", "", "", "", "충돌방지후 감속", "b", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "14", "충돌방지후 정지", "b", "", "", "", "", "", "", "", "", "충돌방지후 정지", "b", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "15", "충돌검출(센서입력)", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "충돌검출(센서입력)", "b" },
            // === 800016 (I/O기판 IN) ===
            new[] { "800016", "0", "교신점확인", "a", "교신점확인", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800016", "1", "인터록확인", "b", "인터록확인", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800016", "2", "종단확인", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "종단확인", "b", "", "", "", "" },
            new[] { "800016", "3", "극한검출(센서입력)", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "극한검출(센서입력)", "b" },
            new[] { "800016", "4", "분기확인", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "분기확인", "a", "", "" },
            new[] { "800016", "5", "분기대차발진가", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "분기대차발진가", "a", "", "" },
            new[] { "800016", "6", "예비1 (분기-8bit)", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800016", "7", "예비2 (분기-8bit)", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800016", "8", "행선1", "a", "", "", "", "", "", "", "", "", "", "", "", "", "행선1", "a", "현재치확인1", "b", "", "", "", "" },
            new[] { "800016", "9", "행선2", "a", "", "", "", "", "", "", "", "", "", "", "", "", "행선2", "a", "현재치확인2", "b", "", "", "", "" },
            new[] { "800016", "10", "행선3", "a", "", "", "", "", "", "", "", "", "", "", "", "", "행선3", "a", "현재치확인3", "b", "", "", "", "" },
            new[] { "800016", "11", "행선4", "a", "", "", "", "", "", "", "", "", "", "", "", "", "행선4", "a", "현재치확인4", "b", "", "", "", "" },
            new[] { "800016", "12", "행선5", "a", "", "", "", "", "", "", "", "", "", "", "", "", "행선5", "a", "현재치확인5", "b", "", "", "", "" },
            new[] { "800016", "13", "행선패리티", "a", "", "", "", "", "", "", "", "", "", "", "", "", "행선패리티", "a", "현재치확인6", "b", "", "", "", "" },
            new[] { "800016", "14", "행선스트로브", "a", "", "", "", "", "", "", "", "", "", "", "", "", "행선스트로브", "a", "현재치확인7", "b", "", "", "", "" },
            new[] { "800016", "15", "예비1 (ROP)", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
        };

        private static readonly List<string[]> EmbeddedOutputData = new List<string[]>
        {
            // OUTPUT 구조: 어드레스, BIT, 기본신호, 기본논리, CHUCK, CHUCK논리, CAGE, CAGE논리, 컨베이어, 컨베이어논리, ROP, ROP논리, 분기, 분기논리, 옵션, 옵션논리
            // === 800010 (I/O기판 OUT) ===
            new[] { "800010", "0", "인버터 정전", "", "인버터 정전", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "1", "인버터 역전", "", "인버터 역전", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "2", "인버터 출력정지", "", "인버터 출력정지", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "3", "인버터 제2기능선택", "", "인버터 제2기능선택", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "4", "인버터 리셋", "", "인버터 리셋", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "5", "예비1 (인버터)", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "6", "주행 구동", "", "주행 구동", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "7", "주행 브레이크 해제", "", "주행 브레이크 해제", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "8", "승강 구동", "", "승강 구동", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "9", "승강 브레이크 해제", "", "승강 브레이크 해제", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "10", "이재모터1 정전", "", "", "", "이재모터1 정전", "", "", "", "이재모터1 정전", "", "", "", "", "" },
            new[] { "800010", "11", "이재모터1 역전", "", "", "", "이재모터1 역전", "", "", "", "이재모터1 역전", "", "", "", "", "" },
            new[] { "800010", "12", "이재모터1 브레이크해제", "", "", "", "이재모터1 브레이크해제", "", "", "", "이재모터1 브레이크해제", "", "", "", "", "" },
            new[] { "800010", "13", "강제 해제", "", "강제 해제", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "14", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "15", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            // === 800014 (I/O기판 OUT) ===
            new[] { "800014", "0", "이재 인터록 1", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "1", "이재 인터록 2", "", "", "", "구동불가", "", "구동가", "", "구동불가", "", "", "", "", "" },
            new[] { "800014", "2", "이재 인터록 3", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "3", "이재 인터록 4", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "4", "이재 인터록 5", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "5", "이재 인터록 6", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "6", "이재 인터록 7", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "7", "이재 인터록 8", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800014", "8", "데이터 요구", "", "", "", "", "", "", "", "데이터 요구", "", "", "", "", "" },
            new[] { "800014", "9", "수신완료", "", "", "", "", "", "", "", "수신완료", "", "", "", "", "" },
            new[] { "800014", "10", "전송정지1", "", "", "", "전송정지1", "", "전송정지1", "", "전송정지1", "", "전송정지1", "", "", "" },
            new[] { "800014", "11", "분기 인터록1", "", "", "", "", "", "", "", "", "", "분기 인터록1", "", "", "" },
            new[] { "800014", "12", "분기 인터록2", "", "", "", "", "", "", "", "", "", "분기 인터록2", "", "", "" },
            new[] { "800014", "13", "분기 인터록3", "", "", "", "", "", "", "", "", "", "분기 인터록3", "", "", "" },
            new[] { "800014", "14", "분기 인터록4", "", "", "", "", "", "", "", "", "", "분기 인터록4", "", "", "" },
            new[] { "800014", "15", "전송정지2", "", "", "", "", "", "", "", "", "", "", "", "전송정지2", "" },
        };

        // 일본어(카타카나) → 한국어 번역 매핑
        private static readonly Dictionary<string, string> JapaneseToKorean = new Dictionary<string, string>
        {
            { "デジスイッチ", "디지스위치" },
            { "イニシャルスイッチ", "이니셜스위치" },
            { "リセット", "리셋" },
            { "リモコン", "리모컨" },
            { "スイッチ", "스위치" },
            { "センサー", "센서" },
            { "モーター", "모터" },
            { "エンコーダ", "엔코더" },
            { "インバーター", "인버터" },
            { "ブレーキ", "브레이크" },
            { "コンベア", "컨베이어" },
            { "ホイスト", "호이스트" },
            { "チャック", "척" },
            { "ケージ", "케이지" },
            { "リレー", "릴레이" },
            { "パリティ", "패리티" },
            { "ストローブ", "스트로브" },
            { "データ", "데이터" },
            { "カウンタ", "카운터" },
            { "カウンター", "카운터" },
            { "ドライバ", "드라이버" },
            { "ドライバー", "드라이버" },
            { "セグメント", "세그먼트" },
            { "ドット", "도트" },
            { "ブザー", "부저" },
            { "表示", "표시" },
            { "電源", "전원" },
            { "運転", "운전" },
            { "異常", "이상" },
            { "手動", "수동" },
            { "自動", "자동" },
            { "停止", "정지" },
            { "駆動", "구동" },
            { "禁止", "금지" },
            { "解除", "해제" },
            { "正転", "정전" },
            { "逆転", "역전" },
            { "上昇", "상승" },
            { "下降", "하강" },
            { "前進", "전진" },
            { "後進", "후진" },
            { "開", "열림" },
            { "閉", "닫힘" },
            { "入力", "입력" },
            { "出力", "출력" }
        };

        public string TesterName => textBoxTesterName?.Text?.Trim() ?? "";
        public string TestDate => textBoxTestDate?.Text?.Trim() ?? "";

        public IOCheckSheetForm()
        {
            InitializeComponent();
        }

        private void IOCheckSheetForm_Load(object sender, EventArgs e)
        {
            textBoxTestDate.Text = DateTime.Now.ToString("yyyy-MM-dd");

            // 드롭다운 초기값 설정 (기본 상태)
            // 승강대: CAGE (인덱스 1)
            if (comboHoistType.Items.Count > 1) comboHoistType.SelectedIndex = 1;
            // 통신방식: SS무선 (인덱스 3: 없음=0, ROP=1, 8bit=2, SS무선=3)
            if (comboCommType.Items.Count > 3) comboCommType.SelectedIndex = 3;
            // 충돌방지센서: 1개소 전측 (인덱스 1: 없음=0, 1개소 전측=1)
            if (comboCollision.Items.Count > 1) comboCollision.SelectedIndex = 1;
            // 레이아웃: 직선형 (인덱스 0)
            if (comboLayout.Items.Count > 0) comboLayout.SelectedIndex = 0;
            // 화물돌출센서: 좌/우 (인덱스 3: 없음=0, 우측=1, 좌측=2, 좌/우=3)
            if (comboCargoProtrusion.Items.Count > 3) comboCargoProtrusion.SelectedIndex = 3;
            // 승강정지센서: 없음 (인덱스 0)
            if (comboLiftStop.Items.Count > 0) comboLiftStop.SelectedIndex = 0;
            // 추가옵션: 없음 (인덱스 0)
            if (comboOption.Items.Count > 0) comboOption.SelectedIndex = 0;

            // 내장 데이터로 테이블 생성 (파일 로드 없이 하드코딩된 데이터 사용)
            BuildTableFromEmbeddedData();

            FillAddressFilterCombo();
            ApplyFilter();
            SetGridColumnWidths();
            dataGridViewIO.CellBeginEdit += dataGridViewIO_CellBeginEdit;
            dataGridViewIO.CellFormatting += dataGridViewIO_CellFormatting;
        }

        /// <summary>내장 데이터에서 I/O 테이블 생성 (파일 로드 없이)</summary>
        private void BuildTableFromEmbeddedData()
        {
            _allIoTable.Clear();
            _allIoTable.Columns.Clear();
            _allIoTable.Columns.Add("어드레스", typeof(string));
            _allIoTable.Columns.Add("IO구분", typeof(string));
            _allIoTable.Columns.Add("BIT", typeof(string));
            _allIoTable.Columns.Add("I.O기판/커넥터No.", typeof(string));
            _allIoTable.Columns.Add("IO기판/PIN-NO", typeof(string));
            _allIoTable.Columns.Add("중계기판/기판명칭", typeof(string));
            _allIoTable.Columns.Add("중계기판/커넥터NO", typeof(string));
            _allIoTable.Columns.Add("중계기판 PIN-NO.", typeof(string));
            _allIoTable.Columns.Add("신호명칭", typeof(string));
            _allIoTable.Columns.Add("논리", typeof(string));
            _allIoTable.Columns.Add("I.O 체크", typeof(bool));
            _allIoTable.Columns.Add("비고", typeof(string));

            // 드롭다운에서 선택된 값 가져오기
            string hoistType = comboHoistType.SelectedItem?.ToString() ?? "CHUCK";
            string commType = comboCommType.SelectedItem?.ToString() ?? "없음";
            string collision = comboCollision.SelectedItem?.ToString() ?? "없음";
            string layout = comboLayout.SelectedItem?.ToString() ?? "직선형";
            string cargoProtrusion = comboCargoProtrusion.SelectedItem?.ToString() ?? "없음";
            string liftStop = comboLiftStop.SelectedItem?.ToString() ?? "없음";
            string extraOpt = comboOption.SelectedItem?.ToString() ?? "없음";

            // INPUT 처리 (내장 데이터 사용)
            ProcessEmbeddedInputData(hoistType, commType, collision, layout, cargoProtrusion, liftStop, extraOpt);
            // OUTPUT 처리 (내장 데이터 사용)
            ProcessEmbeddedOutputData(hoistType, commType, layout, extraOpt);
        }

        // VLOOKUP 함수가 없는 BIT (제외 목록)
        private static readonly HashSet<string> ExcludedInputBits = new HashSet<string>
        {
            // 800012: BIT 4, 7-15
            "800012_4", "800012_7", "800012_8", "800012_9", "800012_10",
            "800012_11", "800012_12", "800012_13", "800012_14", "800012_15",
            // 800014: BIT 4-7
            "800014_4", "800014_5", "800014_6", "800014_7",
            // 800016: BIT 6-7, 12-15
            "800016_6", "800016_7", "800016_12", "800016_13", "800016_14", "800016_15"
        };

        private static readonly HashSet<string> ExcludedOutputBits = new HashSet<string>
        {
            // 800010: BIT 5, 14-15
            "800010_5", "800010_14", "800010_15",
            // 800014: BIT 8-9
            "800014_8", "800014_9"
        };

        /// <summary>내장 INPUT 데이터 처리</summary>
        private void ProcessEmbeddedInputData(string hoistType, string commType, string collision, string layout, string cargoProtrusion, string liftStop, string extraOpt)
        {
            // INPUT 데이터 열 인덱스:
            // 0:어드레스, 1:BIT, 2:기본신호, 3:기본논리
            // 4:CHUCK, 5:CHUCK논리, 6:CAGE, 7:CAGE논리, 8:컨베이어, 9:컨베이어논리
            // 10:충돌방지, 11:충돌방지논리, 12:ROP, 13:ROP논리, 14:8bit, 15:8bit논리, 16:SS무선, 17:SS무선논리
            // 18:직선, 19:직선논리, 20:분기, 21:분기논리, 22:옵션, 23:옵션논리

            foreach (var row in EmbeddedInputData)
            {
                string addr = row[0];
                string bit = row[1];
                string baseSignal = row[2];
                string baseLogic = row[3];

                // VLOOKUP 함수가 없는 BIT는 건너뛰기
                string bitKey = $"{addr}_{bit}";
                if (ExcludedInputBits.Contains(bitKey))
                    continue;

                string finalSignal = "";
                string finalLogic = "";
                int bitNum = int.TryParse(bit, out var b) ? b : -1;

                // 1. 승강대 타입별 신호 (CHUCK/CAGE/컨베이어)
                if (hoistType == "CHUCK" && row.Length > 5 && !string.IsNullOrEmpty(row[4]))
                {
                    finalSignal = row[4];
                    finalLogic = row[5];
                }
                else if (hoistType == "CAGE" && row.Length > 7 && !string.IsNullOrEmpty(row[6]))
                {
                    finalSignal = row[6];
                    finalLogic = row[7];
                }
                else if (hoistType == "컨베이어" && row.Length > 9 && !string.IsNullOrEmpty(row[8]))
                {
                    finalSignal = row[8];
                    finalLogic = row[9];
                }

                // 2. 화물돌출센서 (800010 BIT 4, 5)
                // 없음(1), 우측(2), 좌측(3), 좌/우(4)
                if (addr == "800010" && (bitNum == 4 || bitNum == 5))
                {
                    bool showRight = (cargoProtrusion == "우측" || cargoProtrusion == "좌/우");
                    bool showLeft = (cargoProtrusion == "좌측" || cargoProtrusion == "좌/우");

                    if (bitNum == 4 && showRight)
                    {
                        finalSignal = "우측 돌출감지";
                        finalLogic = "b";
                    }
                    else if (bitNum == 5 && showLeft)
                    {
                        finalSignal = "좌측 돌출감지";
                        finalLogic = "b";
                    }
                    else
                    {
                        // 조건에 맞지 않으면 이 BIT는 건너뛰기
                        continue;
                    }
                }

                // 3. 승강정지센서 (800010 BIT 6, 7)
                // 없음(1), 우측(2), 좌측(3), 좌/우(4)
                if (addr == "800010" && (bitNum == 6 || bitNum == 7))
                {
                    bool showRight = (liftStop == "우측" || liftStop == "좌/우");
                    bool showLeft = (liftStop == "좌측" || liftStop == "좌/우");

                    if (bitNum == 6 && showRight)
                    {
                        finalSignal = "승강정지우";
                        finalLogic = "a";
                    }
                    else if (bitNum == 7 && showLeft)
                    {
                        finalSignal = "승강정지좌";
                        finalLogic = "a";
                    }
                    else
                    {
                        // 조건에 맞지 않으면 이 BIT는 건너뛰기
                        continue;
                    }
                }

                // 4. 충돌방지센서 신호 (세분화된 옵션)
                // 없음(1), 1개소 전측(2), 1개소 후측(3), 2개소(4)
                // 800014 BIT 11-14: 충돌방지전/후 감속/정지 신호
                bool showFront = (collision == "1개소 전측" || collision == "2개소");
                bool showRear = (collision == "1개소 후측" || collision == "2개소");

                // 800014 충돌방지 BIT (11-14)는 옵션에 따라 표시 여부 결정
                if (addr == "800014" && (bitNum == 11 || bitNum == 12 || bitNum == 13 || bitNum == 14))
                {
                    bool isFrontSignal = (bitNum == 11 || bitNum == 12);
                    bool isRearSignal = (bitNum == 13 || bitNum == 14);

                    // 전측 신호: 1개소 전측 또는 2개소일 때만 표시
                    if (isFrontSignal && showFront && row.Length > 13 && !string.IsNullOrEmpty(row[12]))
                    {
                        finalSignal = row[12];
                        finalLogic = row[13];
                    }
                    // 후측 신호: 1개소 후측 또는 2개소일 때만 표시
                    else if (isRearSignal && showRear && row.Length > 13 && !string.IsNullOrEmpty(row[12]))
                    {
                        finalSignal = row[12];
                        finalLogic = row[13];
                    }
                    else
                    {
                        // 조건에 맞지 않으면 이 BIT는 건너뛰기
                        continue;
                    }
                }
                // 다른 충돌방지 관련 신호 (800010 BIT 15, 800012 BIT 0)
                else if (collision != "없음" && string.IsNullOrEmpty(finalSignal) && row.Length > 11 && !string.IsNullOrEmpty(row[10]))
                {
                    finalSignal = row[10];
                    finalLogic = row[11];
                }

                // 5. 통신방식 (ROP/8bit/SS무선)
                if (string.IsNullOrEmpty(finalSignal))
                {
                    if (commType == "ROP" && row.Length > 13 && !string.IsNullOrEmpty(row[12]))
                    {
                        finalSignal = row[12];
                        finalLogic = row[13];
                    }
                    else if (commType == "8bit" && row.Length > 15 && !string.IsNullOrEmpty(row[14]))
                    {
                        finalSignal = row[14];
                        finalLogic = row[15];
                    }
                    else if (commType == "SS무선" && row.Length > 17 && !string.IsNullOrEmpty(row[16]))
                    {
                        finalSignal = row[16];
                        finalLogic = row[17];
                    }
                }

                // 6. 레이아웃 (직선형/분기장치)
                if (string.IsNullOrEmpty(finalSignal))
                {
                    if (layout == "직선형" && row.Length > 19 && !string.IsNullOrEmpty(row[18]))
                    {
                        finalSignal = row[18];
                        finalLogic = row[19];
                    }
                    else if (layout == "분기장치" && row.Length > 21 && !string.IsNullOrEmpty(row[20]))
                    {
                        finalSignal = row[20];
                        finalLogic = row[21];
                    }
                }

                // 7. 추가옵션
                if (extraOpt == "있음" && string.IsNullOrEmpty(finalSignal) && row.Length > 23 && !string.IsNullOrEmpty(row[22]))
                {
                    finalSignal = row[22];
                    finalLogic = row[23];
                }

                // 8. 기본 신호
                if (string.IsNullOrEmpty(finalSignal))
                {
                    finalSignal = baseSignal;
                    finalLogic = baseLogic;
                }

                // 논리(a, b)가 비어있으면 사용하지 않는 신호이므로 신호명도 비움
                if (string.IsNullOrEmpty(finalLogic))
                {
                    finalSignal = "";
                }

                _allIoTable.Rows.Add(addr, "INPUT", bit, "I/O기판", "", "", "", "", finalSignal, finalLogic, false, "");
            }
        }

        /// <summary>내장 OUTPUT 데이터 처리</summary>
        private void ProcessEmbeddedOutputData(string hoistType, string commType, string layout, string extraOpt)
        {
            // OUTPUT 데이터 열 인덱스:
            // 0:어드레스, 1:BIT, 2:기본신호, 3:기본논리
            // 4:CHUCK, 5:CHUCK논리, 6:CAGE, 7:CAGE논리, 8:컨베이어, 9:컨베이어논리
            // 10:ROP, 11:ROP논리, 12:분기, 13:분기논리, 14:옵션, 15:옵션논리

            foreach (var row in EmbeddedOutputData)
            {
                string addr = row[0];
                string bit = row[1];
                string baseSignal = row[2];
                string baseLogic = row[3];

                // VLOOKUP 함수가 없는 BIT는 건너뛰기
                string bitKey = $"{addr}_{bit}";
                if (ExcludedOutputBits.Contains(bitKey))
                    continue;

                string finalSignal = "";
                string finalLogic = "";

                // 1. 승강대 타입별 신호 (CHUCK/CAGE/컨베이어)
                if (hoistType == "CHUCK" && row.Length > 5 && !string.IsNullOrEmpty(row[4]))
                {
                    finalSignal = row[4];
                    finalLogic = row[5];
                }
                else if (hoistType == "CAGE" && row.Length > 7 && !string.IsNullOrEmpty(row[6]))
                {
                    finalSignal = row[6];
                    finalLogic = row[7];
                }
                else if (hoistType == "컨베이어" && row.Length > 9 && !string.IsNullOrEmpty(row[8]))
                {
                    finalSignal = row[8];
                    finalLogic = row[9];
                }

                // 2. 통신방식 (ROP만 - OUTPUT에는 8bit/SS무선 없음)
                if (string.IsNullOrEmpty(finalSignal) && commType == "ROP" && row.Length > 11 && !string.IsNullOrEmpty(row[10]))
                {
                    finalSignal = row[10];
                    finalLogic = row[11];
                }

                // 3. 분기장치
                if (string.IsNullOrEmpty(finalSignal) && layout == "분기장치" && row.Length > 13 && !string.IsNullOrEmpty(row[12]))
                {
                    finalSignal = row[12];
                    finalLogic = row[13];
                }

                // 4. 추가옵션
                if (extraOpt == "있음" && string.IsNullOrEmpty(finalSignal) && row.Length > 15 && !string.IsNullOrEmpty(row[14]))
                {
                    finalSignal = row[14];
                    finalLogic = row[15];
                }

                // 5. 기본 신호
                if (string.IsNullOrEmpty(finalSignal))
                {
                    finalSignal = baseSignal;
                    finalLogic = baseLogic;
                }

                // OUTPUT은 논리 값이 없어도 신호명이 있으면 표시 (INPUT과 다름)
                _allIoTable.Rows.Add(addr, "OUTPUT", bit, "I/O기판", "", "", "", "", finalSignal, finalLogic, false, "");
            }
        }

        /// <summary>일본어(카타카나/한자)를 한국어로 번역</summary>
        private static string TranslateJapanese(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string result = text;
            foreach (var kv in JapaneseToKorean)
            {
                if (result.Contains(kv.Key))
                    result = result.Replace(kv.Key, kv.Value);
            }
            return result;
        }

        /// <summary>I/O 표 적용 버튼 클릭</summary>
        private void buttonApplyOptions_Click(object sender, EventArgs e)
        {
            BuildTableFromEmbeddedData();
            FillAddressFilterCombo();
            ApplyFilter();
            SetGridColumnWidths();

            // INPUT/OUTPUT 개수 계산
            int inputCount = 0, outputCount = 0;
            foreach (DataRow row in _allIoTable.Rows)
            {
                string ioType = row["IO구분"]?.ToString() ?? "";
                if (ioType == "INPUT") inputCount++;
                else if (ioType == "OUTPUT") outputCount++;
            }

            string msg = $"I/O 표가 적용되었습니다.\n\n" +
                         $"승강대: {comboHoistType.SelectedItem}\n" +
                         $"통신방식: {comboCommType.SelectedItem}\n" +
                         $"충돌방지센서: {comboCollision.SelectedItem}\n" +
                         $"레이아웃: {comboLayout.SelectedItem}\n" +
                         $"화물돌출센서: {comboCargoProtrusion.SelectedItem}\n" +
                         $"승강정지센서: {comboLiftStop.SelectedItem}\n" +
                         $"추가옵션: {comboOption.SelectedItem}\n\n" +
                         $"INPUT: {inputCount}개, OUTPUT: {outputCount}개\n" +
                         $"총 {_allIoTable.Rows.Count}개 I/O 로드됨";
            MessageBox.Show(msg, "적용 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>미사용 신호(신호명칭 없음) 행의 I.O 체크 셀은 편집 불가(비활성화).</summary>
        private void dataGridViewIO_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (dataGridViewIO.Columns[e.ColumnIndex].Name != "I.O 체크") return;
            var dt = dataGridViewIO.DataSource as DataTable;
            if (dt == null || e.RowIndex < 0 || e.RowIndex >= dt.Rows.Count) return;
            string sig = dt.Rows[e.RowIndex]["신호명칭"]?.ToString()?.Trim() ?? "";
            if (string.IsNullOrEmpty(sig))
                e.Cancel = true;
        }

        /// <summary>INPUT=빨강, OUTPUT=초록 글씨 / 미사용 신호 행의 I.O 체크 셀은 회색.</summary>
        private void dataGridViewIO_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var dt = dataGridViewIO.DataSource as DataTable;
            if (dt == null || e.RowIndex < 0 || e.RowIndex >= dt.Rows.Count) return;
            string ioType = dt.Rows[e.RowIndex]["IO구분"]?.ToString() ?? "";
            if (ioType == "INPUT")
                e.CellStyle.ForeColor = Color.FromArgb(255, 100, 100);
            else if (ioType == "OUTPUT")
                e.CellStyle.ForeColor = Color.FromArgb(100, 220, 100);
            if (dataGridViewIO.Columns[e.ColumnIndex].Name == "I.O 체크")
            {
                string sig = dt.Rows[e.RowIndex]["신호명칭"]?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(sig))
                {
                    e.CellStyle.BackColor = Color.FromArgb(52, 52, 56);
                    e.CellStyle.ForeColor = Color.Gray;
                }
            }
        }

        /// <summary>열 최소 너비 설정 - 줄바꿈 없이 한 줄로 보이도록, 좌우 스크롤로 나머지 확인</summary>
        /// <summary>열 너비를 넉넉히 설정해 헤더·내용이 잘리지 않도록 함.</summary>
        private void SetGridColumnWidths()
        {
            if (dataGridViewIO.Columns.Count == 0) return;
            dataGridViewIO.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            // 어드레스, IO구분(숨김), BIT, I.O기판/커넥터No., ... 비고
            int[] widths = { 95, 0, 50, 165, 115, 155, 140, 135, 270, 52, 95, 105 };
            for (int i = 0; i < dataGridViewIO.Columns.Count && i < widths.Length; i++)
            {
                dataGridViewIO.Columns[i].MinimumWidth = widths[i] > 0 ? widths[i] : 30;
                dataGridViewIO.Columns[i].Width = widths[i] > 0 ? widths[i] : 0;
                if (dataGridViewIO.Columns[i].Name == "IO구분")
                    dataGridViewIO.Columns[i].Visible = false;
            }
        }

        private void FillAddressFilterCombo()
        {
            comboFilter.Items.Clear();
            comboFilter.Items.Add("전체");
            if (_allIoTable != null && _allIoTable.Rows.Count > 0)
            {
                var addrs = new HashSet<string>();
                foreach (DataRow row in _allIoTable.Rows)
                {
                    var a = row["어드레스"]?.ToString();
                    if (!string.IsNullOrEmpty(a)) addrs.Add(a);
                }
                foreach (var a in addrs.OrderBy(x => x))
                    comboFilter.Items.Add(a);
            }
            comboFilter.SelectedIndex = 0;
        }

        private void comboFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void checkBoxUsedSignalFilter_CheckedChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_allIoTable == null || _allIoTable.Columns.Count == 0) return;
            string sel = comboFilter.SelectedItem?.ToString();
            bool usedSignalOnly = checkBoxUsedSignalFilter?.Checked ?? false;

            DataTable source = _allIoTable;
            if (!string.IsNullOrEmpty(sel) && sel != "전체")
            {
                var byAddr = _allIoTable.Clone();
                foreach (DataRow row in _allIoTable.Rows)
                {
                    if (row["어드레스"]?.ToString() == sel)
                        AddRowByColumn(byAddr, row);
                }
                source = byAddr;
            }

            if (!usedSignalOnly)
            {
                dataGridViewIO.DataSource = source;
                return;
            }
            var filtered = source.Clone();
            foreach (DataRow row in source.Rows)
            {
                string sig = row["신호명칭"]?.ToString()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(sig))
                    AddRowByColumn(filtered, row);
            }
            dataGridViewIO.DataSource = filtered;
        }

        /// <summary>열 순서/이름으로 복사해 행 추가 (ItemArray 열 매핑 이슈 방지).</summary>
        private static void AddRowByColumn(DataTable target, DataRow sourceRow)
        {
            DataRow newRow = target.NewRow();
            foreach (DataColumn col in target.Columns)
            {
                if (sourceRow.Table.Columns.Contains(col.ColumnName))
                    newRow[col.ColumnName] = sourceRow[col.ColumnName];
            }
            target.Rows.Add(newRow);
        }

        private void buttonSavePdf_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF 파일|*.pdf|모든 파일|*.*";
                sfd.DefaultExt = "pdf";
                sfd.FileName = "DIO_I.O_체크성적서_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
                if (sfd.ShowDialog() != DialogResult.OK) return;
                try
                {
                    SaveToPdf(sfd.FileName);
                    MessageBox.Show("저장했습니다.\r\n" + sfd.FileName, "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("PDF 저장 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SaveToPdf(string path)
        {
            var bmp = DrawSheetToBitmap();
            if (bmp == null) throw new InvalidOperationException("이미지 생성 실패");
            try
            {
                if (path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    string pngPath = Path.ChangeExtension(path, ".png");
                    bmp.Save(pngPath, System.Drawing.Imaging.ImageFormat.Png);
                    MessageBox.Show("PNG로 저장했습니다.\r\n" + pngPath + "\r\n\r\n인쇄 시 'PDF로 저장'을 선택하면 PDF로 출력할 수 있습니다.", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }
            finally
            {
                bmp.Dispose();
            }
        }

        private Bitmap DrawSheetToBitmap()
        {
            int w = 1100;
            int h = 800;
            var bmp = new Bitmap(w, h);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                var fontTitle = new Font("맑은 고딕", 14, FontStyle.Bold);
                var fontHead = new Font("맑은 고딕", 9, FontStyle.Bold);
                var fontBody = new Font("맑은 고딕", 8);
                int y = 10;
                g.DrawString("SKY-RAV DIO SHEET  I.O 체크 성적서", fontTitle, Brushes.Black, 20, y);
                y += 28;
                g.DrawString("테스트 실시자: " + TesterName + "    테스트 일자: " + TestDate, fontBody, Brushes.Black, 20, y);
                y += 24;
                g.DrawString("현장코드 F0455  |  현장명칭 한국타이어 금산신공장  |  도번 F0455-L-G103  |  TYPE HSR-050-HWAL  |  승강대 HOIST", fontBody, Brushes.Black, 20, y);
                y += 22;
                var brushInput = new SolidBrush(Color.FromArgb(220, 60, 60));
                var brushOutput = new SolidBrush(Color.FromArgb(60, 180, 60));
                g.DrawString("범례:", fontBody, Brushes.Black, w - 180, 10);
                g.DrawString("INPUT", fontBody, brushInput, w - 130, 10);
                g.DrawString("OUTPUT", fontBody, brushOutput, w - 75, 10);
                int[] widths = { 82, 44, 118, 92, 118, 108, 108, 200, 44, 72, 88 };
                string[] headers = { "어드레스", "BIT", "I.O기판/커넥터No.", "IO기판/PIN-NO", "중계기판/기판명칭", "중계기판/커넥터NO", "중계기판 PIN-NO.", "신호명칭", "논리", "I.O 체크", "비고" };
                for (int c = 0; c < headers.Length; c++)
                    g.DrawString(headers[c], fontHead, Brushes.Black, 20 + widths.Take(c).Sum(), y);
                y += 20;
                g.DrawLine(Pens.Black, 20, y, w - 20, y);
                y += 4;
                var dt = dataGridViewIO.DataSource as DataTable ?? _allIoTable;
                if (dt != null && dt.Columns.Count >= widths.Length)
                {
                    string[] colNames = { "어드레스", "BIT", "I.O기판/커넥터No.", "IO기판/PIN-NO", "중계기판/기판명칭", "중계기판/커넥터NO", "중계기판 PIN-NO.", "신호명칭", "논리", "I.O 체크", "비고" };
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        DataRow row = dt.Rows[r];
                        string ioType = row["IO구분"]?.ToString() ?? "";
                        Brush rowBrush = ioType == "INPUT" ? brushInput : (ioType == "OUTPUT" ? brushOutput : Brushes.Black);
                        int x = 22;
                        for (int c = 0; c < widths.Length; c++)
                        {
                            string val = "";
                            if (c < colNames.Length && dt.Columns.Contains(colNames[c]))
                            {
                                object cell = row[colNames[c]];
                                val = cell is bool b ? (b ? "✓" : "") : (cell?.ToString() ?? "");
                            }
                            if (val.Length > 18) val = val.Substring(0, 15) + "...";
                            g.DrawString(val, fontBody, rowBrush, x, y);
                            x += widths[c];
                        }
                        y += 18;
                        if (y > h - 40) break;
                    }
                }
                brushInput.Dispose();
                brushOutput.Dispose();
            }
            return bmp;
        }

        private void labelTitle_Click(object sender, EventArgs e)
        {

        }
    }
}
