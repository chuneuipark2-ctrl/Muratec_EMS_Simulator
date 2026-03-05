using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ClosedXML.Excel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace EMS_TEST_SIMULATOR
{
    /// <summary>DIO I.O 체크 성적서 폼. 마스터 CSV 기반 동적 I/O 표시.</summary>
    public partial class IOCheckSheetForm : Form
    {
        /// <summary>I.O 체크 상태저장 시 설정. 이 플래그가 true일 때만 메인폼의 command/auto 컨트롤 사용 가능.</summary>
        public static bool IoCheckCompleted { get; private set; }
        /// <summary>AUTO 두 번째 누름(사이클 정지)이 완료된 시점에 Main에서 true로 설정. I.O 체크 시트 열 때 구분=자동 행들을 자동 체크한 뒤 false로 리셋.</summary>
        public static bool AutoCycleCompleted { get; set; }

        private DataTable _allIoTable = new DataTable();
        /// <summary>옵션 필터 없이 모든 시그널. 사용신호 필터링 체크 해제 시 이 테이블 표시.</summary>
        private DataTable _allIoTableFull = new DataTable();
        /// <summary>터치 PC용 가상 키보드 (중복 오픈 방지)</summary>
        private VirtualKeyboardForm _virtualKeyboardForm;
        /// <summary>가상 키보드 표시 중 재진입 방지 (클릭/포커스 동시 발생 시 멈춤 방지)</summary>
        private bool _showingVirtualKeyboard;
        /// <summary>DEVICE LIST 셀 전체 데이터 툴팁 (잘린 데이터 확인용)</summary>
        private ToolTip _deviceListToolTip;
        /// <summary>I.O LIST에서 사용자가 직접 체크하는 행만의 키 집합. 키 형식: "IO구분_어드레스_BIT" (예: INPUT_800010_0). 비어 있으면 CSV 미로드 시 기존 동작(신호명칭 있는 행만 편집/검사).</summary>
        private HashSet<string> _userCheckableIoKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 내장 INPUT/OUTPUT 데이터 (파일 로드 대신 하드코딩)
        // INPUT 구조: 어드레스, BIT, 기본신호, 기본논리, ... 옵션, 옵션논리(0~23), [24:IO커넥터No, 25:IO PIN-NO, 26:IF 기판명칭, 27:IF 커넥터NO, 28:IF PIN-NO]
        private static readonly List<string[]> EmbeddedInputData = new List<string[]>
        {
            // === 800010 (I/O기판 IN) ===
            new[] { "800010", "0", "승강대신호1", "", "승강대적재1", "b", "승강대적재1", "b", "승강대 좌탑재", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "1", "DW", "CNH1", "10" },
            new[] { "800010", "1", "승강대신호2", "", "", "", "", "", "승강대 우탑재", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "2", "DW", "CNH1", "11" },
            new[] { "800010", "2", "승강대신호3", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "3", "DW", "CNH1", "12" },
            new[] { "800010", "3", "승강대신호4", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "4", "DW", "CNH1", "13" },
            new[] { "800010", "4", "승강대신호5", "", "CHUCK 열림단", "a", "우측 돌출감지", "b", "우측 돌출감지", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "5", "DW", "CNH2", "10" },
            new[] { "800010", "5", "승강대신호6", "", "CHUCK 닫힘단", "a", "좌측 돌출감지", "b", "좌측 돌출감지", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "6", "DW", "CNH2", "11" },
            new[] { "800010", "6", "승강대신호7", "", "", "", "", "", "승강정지우", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "7", "DW", "CNH2", "12" },
            new[] { "800010", "7", "승강대신호8", "", "", "", "", "", "승강정지좌", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "8", "DW", "CNH2", "13" },
            new[] { "800010", "8", "승강부원점", "a", "승강부원점", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "9", "DW", "CNO1A,B", "2" },
            new[] { "800010", "9", "느슨해짐 검출", "b", "느슨해짐 검출", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "10", "DW", "CNL1~4", "2" },
            new[] { "800010", "10", "승강 엔코더", "a", "승강 엔코더", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "11", "DW", "CN02", "2" },
            new[] { "800010", "11", "인버터 이상", "b", "인버터 이상", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "12", "DW", "CNINV", "1" },
            new[] { "800010", "12", "인버터 운전중", "a", "인버터 운전중", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "13", "DW", "CNINV", "2" },
            new[] { "800010", "13", "인버터 주파수 검출", "a", "인버터 주파수 검출", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "14", "DW", "CNINV", "3" },
            new[] { "800010", "14", "트롤리제어(인터록)", "a", "트롤리제어(인터록)", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "15", "RLY", "CN1", "8" },
            new[] { "800010", "15", "충돌검출(유지)", "b", "", "", "", "", "", "", "충돌검출(유지)", "b", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "16", "RLY", "CN1", "9" },
            // === 800012 (I/O기판 IN) ===
            new[] { "800012", "0", "극한검출(릴레이접점)", "b", "", "", "", "", "", "", "극한검출(릴레이접점)", "b", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "17", "RLY", "CN1", "10" },
            new[] { "800012", "1", "구동금지", "a", "구동금지", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "18", "RLY", "CN1", "11" },
            new[] { "800012", "2", "주행모터 브레이크 개방 스위치", "a", "주행모터 브레이크 개방 스위치", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "19", "RLY", "CN1", "12" },
            new[] { "800012", "3", "승강모터 브레이크 개방 스위치", "a", "승강모터 브레이크 개방 스위치", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "20", "RLY", "CN1", "13" },
            new[] { "800012", "4", "주행모터 써멀 트립", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "21", "RLY", "CN1", "14" },
            new[] { "800012", "5", "이재모터 전원 이상", "b", "", "", "이재모터 전원 이상", "b", "", "", "이재모터 전원 이상", "b", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "22", "RLY", "CN1", "15" },
            new[] { "800012", "6", "이재모터1 과부하", "a", "", "", "이재모터1 과부하", "a", "", "", "이재모터1 과부하", "a", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "23", "RLY", "CN1", "16" },
            new[] { "800012", "7", "예비(미사용)", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "8", "예비(미사용)", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "9", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "10", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "11", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "12", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "13", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "14", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800012", "15", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            // === 800014 (I/O기판 IN) ===
            new[] { "800014", "0", "이재인터록1", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "1", "UW", "CNS1", "1" },
            new[] { "800014", "1", "이재인터록2", "a", "", "", "화물탑재 가능", "a", "상승가", "a", "화물탑재 가능", "a", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "2", "UW", "CNS1", "2" },
            new[] { "800014", "2", "이재인터록3", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "3", "UW", "CNS1", "3" },
            new[] { "800014", "3", "이재인터록4", "a", "", "", "화물이재 가능", "a", "하강가", "a", "화물이재 가능", "a", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "4", "UW", "CNS1", "4" },
            new[] { "800014", "4", "이재인터록5", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "5", "UW", "CNS2", "10" },
            new[] { "800014", "5", "이재인터록6", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "6", "UW", "CNS2", "11" },
            new[] { "800014", "6", "이재인터록7", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "7", "UW", "CNS2", "12" },
            new[] { "800014", "7", "이재인터록8", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "8", "UW", "CNS2", "13" },
            new[] { "800014", "8", "정지1", "a", "정지1", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "9", "UW", "CN01", "2" },
            new[] { "800014", "9", "정지2", "a", "정지2", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "10", "UW", "CN02", "2" },
            new[] { "800014", "10", "정지패리티", "a", "정지패리티", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "11", "UW", "CN03", "2" },
            new[] { "800014", "11", "충돌방지전 감속", "b", "", "", "", "", "", "", "", "", "충돌방지전 감속", "b", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "12", "UW", "CN04", "2" },
            new[] { "800014", "12", "충돌방지전 정지", "b", "", "", "", "", "", "", "", "", "충돌방지전 정지", "b", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "13", "UW", "CN05", "2" },
            new[] { "800014", "13", "충돌방지후 감속", "b", "", "", "", "", "", "", "", "", "충돌방지후 감속", "b", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "14", "UW", "CN06", "2" },
            new[] { "800014", "14", "충돌방지후 정지", "b", "", "", "", "", "", "", "", "", "충돌방지후 정지", "b", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "15", "UW", "CN07", "2" },
            new[] { "800014", "15", "충돌검출(센서입력)", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "16", "UW", "CN08", "2" },
            // === 800016 (I/O기판 IN) ===
            new[] { "800016", "0", "교신점확인", "a", "교신점확인", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "17", "UW", "CN09", "2" },
            new[] { "800016", "1", "인터록확인", "b", "인터록확인", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "18", "UW", "CN10", "2" },
            new[] { "800016", "2", "종단확인", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "종단확인", "b", "", "", "", "", "I/O CN41", "19", "UW", "CN11", "2" },
            new[] { "800016", "3", "극한검출(센서입력)", "b", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "20", "UW", "CN12", "2" },
            new[] { "800016", "4", "분기확인", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "분기확인", "a", "", "", "I/O CN41", "21", "UW", "CN13", "2" },
            new[] { "800016", "5", "분기대차발진가", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "분기대차발진가", "a", "", "", "I/O CN41", "22", "UW", "CNB1", "10" },
            new[] { "800016", "6", "예비1 (분기-8bit)", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "23", "UW", "CNB1", "11" },
            new[] { "800016", "7", "예비2 (분기-8bit)", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "24", "UW", "CNB1", "12" },
            new[] { "800016", "8", "행선1", "a", "", "", "", "", "", "", "", "", "", "", "", "", "행선1", "a", "현재치확인1", "b", "", "", "", "", "I/O CN41", "25", "UW", "CND1", "10" },
            new[] { "800016", "9", "행선2", "a", "", "", "", "", "", "", "", "", "", "", "", "", "행선2", "a", "현재치확인2", "b", "", "", "", "", "I/O CN41", "26", "UW", "CND1", "11" },
            new[] { "800016", "10", "행선3", "a", "", "", "", "", "", "", "", "", "", "", "", "", "행선3", "a", "현재치확인3", "b", "", "", "", "", "I/O CN41", "27", "UW", "CND1", "12" },
            new[] { "800016", "11", "행선4", "a", "", "", "", "", "", "", "", "", "", "", "", "", "행선4", "a", "현재치확인4", "b", "", "", "", "", "I/O CN41", "28", "UW", "CND1", "13" },
            new[] { "800016", "12", "행선5", "a", "", "", "", "", "", "", "", "", "", "", "", "", "행선5", "a", "현재치확인5", "b", "", "", "", "", "I/O CN41", "29", "UW", "CND2", "10" },
            new[] { "800016", "13", "행선패리티", "a", "", "", "", "", "", "", "", "", "", "", "", "", "행선패리티", "a", "현재치확인6", "b", "", "", "", "", "I/O CN41", "30", "UW", "CND2", "11" },
            new[] { "800016", "14", "행선스트로브", "a", "", "", "", "", "", "", "", "", "", "", "", "", "행선스트로브", "a", "현재치확인7", "b", "", "", "", "", "I/O CN41", "31", "UW", "CND2", "12" },
            new[] { "800016", "15", "예비1 (ROP)", "a", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "32", "UW", "CND2", "13" },
        };

        private static readonly List<string[]> EmbeddedOutputData = new List<string[]>
        {
            // OUTPUT 구조: 어드레스, BIT, 기본신호, 기본논리, ... 옵션, 옵션논리(0~15), [16:IO커넥터No, 17:IO PIN-NO, 18:IF 기판명칭, 19:IF 커넥터NO, 20:IF PIN-NO]
            // === 800010 (I/O기판 OUT) ===
            new[] { "800010", "0", "인버터 정전", "", "인버터 정전", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "33", "DW", "CNINV", "4" },
            new[] { "800010", "1", "인버터 역전", "", "인버터 역전", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "34", "DW", "CNINV", "5" },
            new[] { "800010", "2", "인버터 출력정지", "", "인버터 출력정지", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "35", "DW", "CNINV", "6" },
            new[] { "800010", "3", "인버터 제2기능선택", "", "인버터 제2기능선택", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "36", "DW", "CNINV", "7" },
            new[] { "800010", "4", "인버터 리셋", "", "인버터 리셋", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "37", "DW", "CNINV", "8" },
            new[] { "800010", "5", "예비1 (인버터)", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "38", "DW", "CNINV", "9" },
            new[] { "800010", "6", "주행 구동", "", "주행 구동", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "39", "RLY", "CN1", "17" },
            new[] { "800010", "7", "주행 브레이크 해제", "", "주행 브레이크 해제", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "40", "RLY", "CN1", "18" },
            new[] { "800010", "8", "승강 구동", "", "승강 구동", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "41", "RLY", "CN1", "19" },
            new[] { "800010", "9", "승강 브레이크 해제", "", "승강 브레이크 해제", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "42", "RLY", "CN1", "20" },
            new[] { "800010", "10", "이재모터1 정전", "", "", "", "이재모터1 정전", "", "", "", "이재모터1 정전", "", "", "", "", "", "I/O CN31", "43", "RLY", "CN1", "21" },
            new[] { "800010", "11", "이재모터1 역전", "", "", "", "이재모터1 역전", "", "", "", "이재모터1 역전", "", "", "", "", "", "I/O CN31", "44", "RLY", "CN1", "22" },
            new[] { "800010", "12", "이재모터1 브레이크해제", "", "", "", "이재모터1 브레이크해제", "", "", "", "이재모터1 브레이크해제", "", "", "", "", "", "I/O CN31", "45", "RLY", "CN1", "23" },
            new[] { "800010", "13", "강제 해제", "", "강제 해제", "", "", "", "", "", "", "", "", "", "", "", "I/O CN31", "46", "RLY", "CN1", "24" },
            new[] { "800010", "14", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            new[] { "800010", "15", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" },
            // === 800014 (I/O기판 OUT) ===
            new[] { "800014", "0", "이재 인터록 1", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "33", "UW", "CNS1", "3" },
            new[] { "800014", "1", "이재 인터록 2", "", "", "", "구동불가", "", "구동가", "", "구동불가", "", "", "", "", "", "I/O CN41", "34", "UW", "CNS1", "4" },
            new[] { "800014", "2", "이재 인터록 3", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "35", "UW", "CNS1", "5" },
            new[] { "800014", "3", "이재 인터록 4", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "36", "UW", "CNS1", "6" },
            new[] { "800014", "4", "이재 인터록 5", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "37", "UW", "CNS2", "3" },
            new[] { "800014", "5", "이재 인터록 6", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "38", "UW", "CNS2", "4" },
            new[] { "800014", "6", "이재 인터록 7", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "39", "UW", "CNS2", "5" },
            new[] { "800014", "7", "이재 인터록 8", "", "", "", "", "", "", "", "", "", "", "", "", "", "I/O CN41", "40", "UW", "CNS2", "6" },
            new[] { "800014", "8", "데이터 요구", "", "", "", "", "", "", "", "데이터 요구", "", "", "", "", "", "I/O CN41", "41", "UW", "CND1", "3" },
            new[] { "800014", "9", "수신완료", "", "", "", "", "", "", "", "수신완료", "", "", "", "", "", "I/O CN41", "42", "UW", "CND1", "4" },
            new[] { "800014", "10", "전송정지1", "", "", "", "전송정지1", "", "전송정지1", "", "전송정지1", "", "전송정지1", "", "", "", "I/O CN41", "43", "UW", "CNS1", "14" },
            new[] { "800014", "11", "분기 인터록1", "", "", "", "", "", "", "", "", "", "분기 인터록1", "", "", "", "I/O CN41", "44", "UW", "CNB1", "3" },
            new[] { "800014", "12", "분기 인터록2", "", "", "", "", "", "", "", "", "", "분기 인터록2", "", "", "", "I/O CN41", "45", "UW", "CNB1", "4" },
            new[] { "800014", "13", "분기 인터록3", "", "", "", "", "", "", "", "", "", "분기 인터록3", "", "", "", "I/O CN41", "46", "UW", "CNB1", "5" },
            new[] { "800014", "14", "분기 인터록4", "", "", "", "", "", "", "", "", "", "분기 인터록4", "", "", "", "I/O CN41", "47", "UW", "CNB1", "6" },
            new[] { "800014", "15", "전송정지2", "", "", "", "", "", "", "", "", "", "", "", "전송정지2", "", "I/O CN41", "48", "UW", "CNB1", "14" },
            // === 800018 (이재모터2, 이재모터 수=1개일 때만 표시). 마지막 5열(16~20)=IO커넥터No, IO PIN-NO, IF 기판명칭, IF 커넥터NO, IF PIN-NO
            new[] { "800018", "8", "이재모터2 정전", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "증설 I/O-1 CN1", "41", "RLY", "CN2", "3" },
            new[] { "800018", "9", "이재모터2 역전", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "증설 I/O-1 CN1", "42", "RLY", "CN2", "4" },
            new[] { "800018", "10", "이재모터2 브레이크 해제", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "증설 I/O-1 CN1", "43", "RLY", "CN2", "5" },
            new[] { "800018", "11", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "증설 I/O-1 CN1", "44", "RLY", "CN2", "6" },
            new[] { "800018", "12", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "증설 I/O-1 CN1", "45", "RLY", "CN2", "7" },
            new[] { "800018", "13", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "증설 I/O-1 CN1", "46", "RLY", "CN2", "8" },
            new[] { "800018", "14", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "증설 I/O-1 CN1", "47", "RLY", "CN2", "9" },
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

            // 터치 PC: 가상 키보드는 텍스트박스를 클릭했을 때만 표시 (I/O 적용 등 버튼 클릭 시 포커스 이동으로 자동 뜨지 않도록 Enter는 사용하지 않음).
            if (label1 != null)
                label1.Click += (s, ev) => { textBoxTesterName?.Focus(); ShowVirtualKeyboard(); };
            // textBoxTesterName.Click 은 Designer에서 연결 → 클릭 시에만 키보드 표시
            textBoxTestDate.Click += (s, ev) => ShowCalendar();

            if (button1 != null)
                button1.Click += IOCheckSheetForm_StateSaveClick;

            // 드롭다운 초기값 설정 (기본 상태 = 이미지 기준)
            // LAYOUT SPEC: 형태 루프, 분기장치 없음
            if (IO_CBOX1?.Items.Count > 1) IO_CBOX1.SelectedIndex = 1;   // 루프
            if (IO_CBOX2?.Items.Count > 0) IO_CBOX2.SelectedIndex = 0;   // 없음
            // 본체 SPEC: 승강대 CAGE, 교신방법 SS무선, 충돌방지 1개소 전측, 분기신호 1점
            if (comboHoistType.Items.Count > 1) comboHoistType.SelectedIndex = 1;   // CAGE
            if (comboCommType.Items.Count > 2) comboCommType.SelectedIndex = 2;   // SS무선
            if (comboCollision.Items.Count > 1) comboCollision.SelectedIndex = 1;   // 1개소 전측
            if (comboLayout.Items.Count > 0) comboLayout.SelectedIndex = 0;         // 1점
            // 이재인터록 있음(UI=comboCargoProtrusion), 화물감지 1개소, 화물돌출 좌/우
            if (comboCargoProtrusion?.Items.Count > 1) comboCargoProtrusion.SelectedIndex = 1; // 이재인터록 있음
            if (comboBox3?.Items.Count > 0) comboBox3.SelectedIndex = 0;   // 극한검출 없음(UI=comboBox3)
            if (comboLiftStop.Items.Count > 1) comboLiftStop.SelectedIndex = 1;     // 1개소 (화물감지센서)
            if (comboOption.Items.Count > 3) comboOption.SelectedIndex = 3;         // 좌/우 (화물돌출센서)
            // 옵션설정: 충돌검출/극한검출/이재모터 수 = 없음(0), 8bit전송정지 = 1점(0)
            if (comboCollisionDetect?.Items.Count > 0) comboCollisionDetect.SelectedIndex = 0;
            if (comboLimitDetect?.Items.Count > 0) comboLimitDetect.SelectedIndex = 0;
            if (comboTransferMotor?.Items.Count > 0) comboTransferMotor.SelectedIndex = 0;
            if (combo8bitStop?.Items.Count > 0) combo8bitStop.SelectedIndex = 0;

            // I.O LIST 사용자 체크 가능 목록 로드 (IO_List_All_Signals.csv 구분=사용자)
            LoadIoUserCheckableCsv();

            // 내장 데이터로 테이블 생성 (파일 로드 없이 하드코딩된 데이터 사용)
            BuildTableFromEmbeddedData();

            FillAddressFilterCombo();
            ApplyFilter();
            SetGridColumnWidths();
            dataGridViewIO.CellBeginEdit += dataGridViewIO_CellBeginEdit;
            dataGridViewIO.CellFormatting += dataGridViewIO_CellFormatting;

            SetupDeviceListGrid();

            // AUTO 사이클 정지 완료 플래그가 켜져 있으면 구분=자동 행들 I.O 체크 true 반영 후 플래그 리셋
            if (AutoCycleCompleted)
            {
                ApplyAutoCompletedCheck();
                AutoCycleCompleted = false;
                ApplyFilter();
            }
        }

        /// <summary>AUTO 사이클 정지 완료 시점 플래그 반영: _allIoTable, _allIoTableFull에서 구분=사용자 아닌 행의 I.O 체크를 true로 설정.</summary>
        private void ApplyAutoCompletedCheck()
        {
            if (_userCheckableIoKeys.Count == 0) return;
            int colIo = _allIoTable?.Columns.IndexOf("IO구분") ?? -1;
            int colAddr = _allIoTable?.Columns.IndexOf("어드레스") ?? -1;
            int colBit = _allIoTable?.Columns.IndexOf("BIT") ?? -1;
            int colIoCheck = _allIoTable?.Columns.IndexOf("I.O 체크") ?? -1;
            if (colIo < 0 || colAddr < 0 || colBit < 0 || colIoCheck < 0) return;
            foreach (DataTable dt in new[] { _allIoTable, _allIoTableFull })
            {
                if (dt == null || dt.Columns.Count <= colIoCheck) continue;
                foreach (DataRow row in dt.Rows)
                {
                    string io = row[colIo]?.ToString()?.Trim() ?? "";
                    string addr = row[colAddr]?.ToString()?.Trim() ?? "";
                    string bit = row[colBit]?.ToString()?.Trim() ?? "";
                    if (!_userCheckableIoKeys.Contains($"{io}_{addr}_{bit}"))
                        row[colIoCheck] = true;
                }
            }
        }

        /// <summary>IO_List_All_Signals.csv에서 구분=사용자 행만 읽어 _userCheckableIoKeys에 채움. 키: IO구분_어드레스_BIT.</summary>
        private void LoadIoUserCheckableCsv()
        {
            _userCheckableIoKeys.Clear();
            string path = Path.Combine(Application.StartupPath, "IO_List_All_Signals.csv");
            if (!File.Exists(path)) return;
            try
            {
                var lines = File.ReadAllLines(path, Encoding.UTF8);
                if (lines.Length < 2) return;
                int colIo = -1, colAddr = -1, colBit = -1, colGubun = -1;
                var headers = ParseCsvLine(lines[0]);
                for (int i = 0; i < headers.Length; i++)
                {
                    string h = headers[i].Trim();
                    if (h == "IO구분") colIo = i;
                    else if (h == "어드레스") colAddr = i;
                    else if (h == "BIT") colBit = i;
                    else if (h == "구분") colGubun = i;
                }
                if (colIo < 0 || colAddr < 0 || colBit < 0 || colGubun < 0) return;
                for (int i = 1; i < lines.Length; i++)
                {
                    var cols = ParseCsvLine(lines[i]);
                    if (cols.Length <= Math.Max(colIo, Math.Max(colAddr, Math.Max(colBit, colGubun)))) continue;
                    string gubun = cols[colGubun].Trim();
                    if (!gubun.Equals("사용자", StringComparison.OrdinalIgnoreCase)) continue;
                    string io = cols[colIo].Trim(), addr = cols[colAddr].Trim(), bit = cols[colBit].Trim();
                    if (string.IsNullOrEmpty(io) || string.IsNullOrEmpty(addr) || string.IsNullOrEmpty(bit)) continue;
                    _userCheckableIoKeys.Add($"{io}_{addr}_{bit}");
                }
            }
            catch { /* 무시 */ }
        }

        private static string[] ParseCsvLine(string line)
        {
            var list = new List<string>();
            var sb = new StringBuilder();
            bool inQuoted = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"') inQuoted = !inQuoted;
                else if ((c == ',' && !inQuoted) || c == '\r' || c == '\n') { list.Add(sb.ToString()); sb.Clear(); if (c != ',') break; }
                else sb.Append(c);
            }
            list.Add(sb.ToString());
            return list.ToArray();
        }

        /// <summary>DEVICE LIST에서 분류 행 여부. [SENSOR PART]처럼 대괄호로 된 분류에는 체크박스 없음.</summary>
        private static bool IsDeviceListCategoryRow(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return false;
            var d = description.Trim();
            return d.StartsWith("[", StringComparison.Ordinal) && d.IndexOf("]", StringComparison.Ordinal) >= 0;
        }

        /// <summary>DEVICE LIST ListView 열 구성: PDM No, DESCRIPTION, SPECIFICATION, MANUFACTURE, 확인(체크박스)</summary>
        private void SetupDeviceListGrid()
        {
            listViewDeviceList.Columns.Clear();
            listViewDeviceList.Columns.Add("PDM No", 100);
            listViewDeviceList.Columns.Add("DESCRIPTION", 280);
            listViewDeviceList.Columns.Add("SPECIFICATION", 280);
            listViewDeviceList.Columns.Add("MANUFACTURE", 140);
            listViewDeviceList.Columns.Add("확인", 70);
            listViewDeviceList.OwnerDraw = true;
            listViewDeviceList.DrawColumnHeader += listViewDeviceList_DrawColumnHeader;
            listViewDeviceList.DrawItem += listViewDeviceList_DrawItem;
            listViewDeviceList.DrawSubItem += listViewDeviceList_DrawSubItem;
            listViewDeviceList.Resize += listViewDeviceList_Resize;
            listViewDeviceList.Layout += listViewDeviceList_Resize;
            listViewDeviceList.MouseDown += listViewDeviceList_MouseDown;
            listViewDeviceList.MouseMove += listViewDeviceList_MouseMove;
            if (_deviceListToolTip == null)
            {
                _deviceListToolTip = new ToolTip { AutoPopDelay = 15000, InitialDelay = 400, ReshowDelay = 200 };
                _deviceListToolTip.SetToolTip(listViewDeviceList, "");
            }
            ApplyDeviceListColumnWidths();
            panelDeviceList.AllowDrop = true;
            panelDeviceList.DragEnter += listViewDeviceList_DragEnter;
            panelDeviceList.DragDrop += listViewDeviceList_DragDrop;
        }

        /// <summary>ListView 클라이언트 너비에 맞춰 열 너비 배분 (글자 잘림 방지용 최소 너비 적용)</summary>
        private void ApplyDeviceListColumnWidths()
        {
            if (listViewDeviceList?.Columns == null || listViewDeviceList.Columns.Count < 5) return;
            int total = listViewDeviceList.ClientSize.Width;
            if (total <= 0) return;
            int sb = 0;
            if (listViewDeviceList.Items.Count > 0)
            {
                try
                {
                    var r = listViewDeviceList.GetItemRect(0);
                    if (r.Height * listViewDeviceList.Items.Count > listViewDeviceList.ClientSize.Height)
                        sb = SystemInformation.VerticalScrollBarWidth;
                }
                catch { /* 레이아웃 전 등에서 GetItemRect(0) 예외 시 스크롤바 없음으로 처리 */ }
            }
            int available = total - sb;
            const int pdmW = 135, manuW = 175, confirmW = 70;
            const int minDescW = 250, minSpecW = 250;
            int flex = available - pdmW - manuW - confirmW;
            if (flex < minDescW + minSpecW) flex = minDescW + minSpecW;
            int descW = Math.Max(minDescW, flex / 2);
            int specW = Math.Max(minSpecW, flex - descW);
            if (descW + specW > flex) { specW = flex - descW; if (specW < minSpecW) { specW = minSpecW; descW = flex - minSpecW; } }
            listViewDeviceList.Columns[0].Width = pdmW;
            listViewDeviceList.Columns[1].Width = descW;
            listViewDeviceList.Columns[2].Width = specW;
            listViewDeviceList.Columns[3].Width = manuW;
            listViewDeviceList.Columns[4].Width = confirmW;
        }

        private void listViewDeviceList_Resize(object sender, EventArgs e)
        {
            ApplyDeviceListColumnWidths();
        }

        private void listViewDeviceList_MouseDown(object sender, MouseEventArgs e)
        {
            var info = listViewDeviceList.HitTest(e.Location);
            if (info.Item == null || info.SubItem == null) return;
            if (info.Item.Tag as string == "Category") return;
            int subIdx = info.Item.SubItems.IndexOf(info.SubItem);
            if (subIdx != 4) return;  // 4 = "확인" 열(체크박스)
            info.Item.Checked = !info.Item.Checked;
            listViewDeviceList.Invalidate(info.Item.Bounds);
        }

        /// <summary>마우스가 올라간 셀의 전체 데이터를 툴팁으로 표시 (잘려 보이는 데이터 확인용)</summary>
        private void listViewDeviceList_MouseMove(object sender, MouseEventArgs e)
        {
            if (_deviceListToolTip == null) return;
            var info = listViewDeviceList.HitTest(e.Location);
            string cellText = "";
            if (info.Item != null)
            {
                if (info.SubItem != null)
                {
                    int subIdx = info.Item.SubItems.IndexOf(info.SubItem);
                    if (subIdx == 4)
                        cellText = info.Item.Tag as string == "Category" ? "분류 행" : (info.Item.Checked ? "확인됨" : "미확인");
                    else
                        cellText = info.SubItem.Text ?? "";
                }
                else
                    cellText = info.Item.Text ?? "";
            }
            _deviceListToolTip.SetToolTip(listViewDeviceList, cellText);
        }

        private static readonly Color _deviceListHeaderBack = Color.FromArgb(37, 99, 235);
        private static readonly Color _deviceListItemBack = Color.FromArgb(62, 62, 66);
        private static readonly Color _deviceListFore = Color.White;

        private void listViewDeviceList_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(_deviceListHeaderBack), e.Bounds);
            const int pad = 6;
            var textRect = new Rectangle(e.Bounds.X + pad, e.Bounds.Y, Math.Max(0, e.Bounds.Width - pad * 2), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.Header?.Text ?? "", e.Font ?? listViewDeviceList.Font, textRect, _deviceListFore, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix);
        }

        private void listViewDeviceList_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // 첫 번째 열 포함 모든 열은 DrawSubItem에서 그리므로 여기서는 미그림
        }

        private void listViewDeviceList_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ItemIndex < 0) return;
            var back = e.Item.Selected ? SystemColors.Highlight : _deviceListItemBack;
            e.Graphics.FillRectangle(new SolidBrush(back), e.Bounds);
            var fore = e.Item.Selected ? SystemColors.HighlightText : _deviceListFore;
            // 오른쪽 끝 "확인" 열: 분류 행([SENSOR PART] 등)은 체크박스 없음, 일반 행만 체크박스 그리기
            if (e.ColumnIndex == 4)
            {
                if (e.Item?.Tag as string != "Category")
                {
                    int boxSize = Math.Min(16, Math.Min(e.Bounds.Width, e.Bounds.Height) - 4);
                    if (boxSize > 0)
                    {
                        int x = e.Bounds.X + (e.Bounds.Width - boxSize) / 2;
                        int y = e.Bounds.Y + (e.Bounds.Height - boxSize) / 2;
                        var boxRect = new Rectangle(x, y, boxSize, boxSize);
                        var state = e.Item.Checked ? System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal : System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;
                        System.Windows.Forms.CheckBoxRenderer.DrawCheckBox(e.Graphics, boxRect.Location, state);
                    }
                }
                return;
            }
            var text = e.ColumnIndex == 0 ? e.Item.Text : (e.SubItem?.Text ?? "");
            const int pad = 6;
            var textRect = new Rectangle(e.Bounds.X + pad, e.Bounds.Y, Math.Max(0, e.Bounds.Width - pad * 2), e.Bounds.Height);
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis;
            TextRenderer.DrawText(e.Graphics, text, e.SubItem?.Font ?? listViewDeviceList.Font, textRect, fore, flags);
        }

        /// <summary>가상 키보드 표시. 클릭할 때마다 확인·표시. 이미 떠 있으면 아무 동작 안 함(캘린더처럼).</summary>
        private void ShowVirtualKeyboard()
        {
            if (_showingVirtualKeyboard)
                return;
            if (_virtualKeyboardForm != null && !_virtualKeyboardForm.IsDisposed)
            {
                if (_virtualKeyboardForm.Visible)
                    return;
                _virtualKeyboardForm.Show(this);
                _virtualKeyboardForm.BringToFront();
                return;
            }
            _showingVirtualKeyboard = true;
            try
            {
                _virtualKeyboardForm = new VirtualKeyboardForm(textBoxTesterName);
                _virtualKeyboardForm.FormClosed += (s, ev) =>
                {
                    _virtualKeyboardForm = null;
                    _showingVirtualKeyboard = false;
                };
                _virtualKeyboardForm.Show(this);
            }
            catch
            {
                _showingVirtualKeyboard = false;
                throw;
            }
        }

        private void ShowCalendar()
        {
            using (var cal = new CalendarPopupForm(textBoxTestDate))
                cal.ShowDialog(this);
        }

        /// <summary>내장 데이터에서 I/O 테이블 생성 (파일 로드 없이)</summary>
        private void BuildTableFromEmbeddedData()
        {
            _allIoTable.Clear();
            _allIoTable.Columns.Clear();
            _allIoTable.Columns.Add("어드레스", typeof(string));
            _allIoTable.Columns.Add("IO구분", typeof(string));
            _allIoTable.Columns.Add("BIT", typeof(string));
            _allIoTable.Columns.Add("IO 커넥터No.", typeof(string));
            _allIoTable.Columns.Add("IO PIN-NO", typeof(string));
            _allIoTable.Columns.Add("IF 기판명칭", typeof(string));
            _allIoTable.Columns.Add("IF 커넥터NO", typeof(string));
            _allIoTable.Columns.Add("IF PIN-NO.", typeof(string));
            _allIoTable.Columns.Add("신호명칭", typeof(string));
            _allIoTable.Columns.Add("논리", typeof(string));
            _allIoTable.Columns.Add("I.O 체크", typeof(bool));
            _allIoTable.Columns.Add("비고", typeof(string));

            // 드롭다운에서 선택된 값 가져오기
            string hoistType = comboHoistType.SelectedItem?.ToString() ?? "CHUCK";
            string commType = comboCommType.SelectedItem?.ToString() ?? "없음";
            string collision = comboCollision.SelectedItem?.ToString() ?? "없음";
            string formType = IO_CBOX1?.SelectedItem?.ToString() ?? "루프";  // 형태: 직선형 / 루프
            string branchDevice = IO_CBOX2?.SelectedItem?.ToString() ?? "없음";  // 분기장치: 없음 / 있음
            string layout = comboLayout.SelectedItem?.ToString() ?? "직선형";
            // 화물돌출센서(우측/좌측/좌/우) = comboOption, 화물감지 = comboLiftStop
            string cargoProtrusion = comboOption.SelectedItem?.ToString() ?? "없음";
            string liftStop = comboLiftStop.SelectedItem?.ToString() ?? "없음";
            string extraOpt = comboCargoProtrusion.SelectedItem?.ToString() ?? "없음";
            // UI 라벨 "이재인터록" = comboCargoProtrusion (있음/없음)
            string unloadingInterlock = comboCargoProtrusion?.SelectedItem?.ToString() ?? "없음";

            // 옵션설정 인덱스 (1=없음/1점, 2=있음/2점) - VLOOKUP 열 오프셋용, ComboBox SelectedIndex+1
            // UI 라벨 기준: 충돌검출=comboLimitDetect, 극한검출=comboBox3
            int optCollisionDetect = GetOptionSettingIndex(comboLimitDetect);
            int optLimitDetect = GetOptionSettingIndex(comboBox3);
            int optTransferMotor = GetOptionSettingIndex(comboTransferMotor);
            int opt8bitStop = GetOptionSettingIndex(combo8bitStop);

            // INPUT 처리 (내장 데이터 사용)
            ProcessEmbeddedInputData(hoistType, commType, collision, formType, branchDevice, layout, cargoProtrusion, liftStop, extraOpt, optCollisionDetect, optLimitDetect, optTransferMotor);
            // OUTPUT 처리 (내장 데이터 사용)
            ProcessEmbeddedOutputData(hoistType, commType, branchDevice, layout, extraOpt, optTransferMotor, opt8bitStop, unloadingInterlock);

            // 전체 시그널 테이블 (옵션 필터 없음) — 사용신호 필터링 체크 해제 시 표시용
            _allIoTableFull.Clear();
            _allIoTableFull.Columns.Clear();
            foreach (DataColumn col in _allIoTable.Columns)
                _allIoTableFull.Columns.Add(col.ColumnName, col.DataType);
            ProcessEmbeddedInputDataFull(_allIoTableFull);
            ProcessEmbeddedOutputDataFull(_allIoTableFull);
        }

        /// <summary>옵션설정 인덱스 반환 (1 또는 2) - ComboBox SelectedIndex+1</summary>
        private static int GetOptionSettingIndex(ComboBox combo)
        {
            if (combo == null) return 1;
            int idx = combo.SelectedIndex;
            if (idx < 0) return 1;
            return idx + 1;
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
        private void ProcessEmbeddedInputData(string hoistType, string commType, string collision, string formType, string branchDevice, string layout, string cargoProtrusion, string liftStop, string extraOpt, int optCollisionDetect, int optLimitDetect, int optTransferMotor)
        {
            // formType: 형태(직선형/루프) — 800016 BIT 2 종단확인 표시 여부
            // branchDevice: 분기장치(없음/있음) — 800016 BIT 4·5 분기확인/분기대차발진가 표시 여부
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

                // 옵션설정: 충돌검출=없음(1)이면 충돌검출 신호 BIT 표시 안 함
                if (optCollisionDetect == 1 && ((addr == "800010" && bit == "15") || (addr == "800014" && bit == "15")))
                    continue;
                // 옵션설정: 극한검출=없음(1)이면 극한검출 신호 BIT 표시 안 함
                if (optLimitDetect == 1 && ((addr == "800012" && bit == "0") || (addr == "800016" && bit == "3")))
                    continue;
                // 옵션설정: 이재모터 수=없음(1)이면 이재모터 관련 INPUT 표시 안 함
                if (optTransferMotor == 1 && addr == "800012" && (bit == "5" || bit == "6"))
                    continue;
                // 표준(고정신호): 800012 BIT 3 승강모터 브레이크 개방 스위치 — 옵션 무관 항상 표시 (제외 안 함)

                // 형태(직선형/루프): 800016 BIT 2 — 직선형일 때만 종단확인(b), 루프일 때는 행 제외
                if (addr == "800016" && bit == "2")
                {
                    if (formType != "직선형")
                        continue;
                }
                // 분기장치(없음/있음): 800016 BIT 4·5 — 있음일 때만 분기확인(a), 분기대차발진가(a), 없음이면 행 제외
                if (addr == "800016" && (bit == "4" || bit == "5"))
                {
                    if (branchDevice != "있음")
                        continue;
                }
                // 승강대 CAGE: 화물돌출 없을 때만 800010 INPUT BIT 4·5 제외 (화물돌출 선택 시에는 표시)
                if (hoistType == "CAGE" && addr == "800010" && (bit == "4" || bit == "5") && cargoProtrusion == "없음")
                    continue;
                // 교신방법 ROP: 800016 INPUT BIT 8·9·10·11 표시 안 함
                if (commType == "ROP" && addr == "800016" && (bit == "8" || bit == "9" || bit == "10" || bit == "11"))
                    continue;
                // 충돌방지센서 없음: 800014 INPUT BIT 11·12·13·14 그리드에서 제외
                if (collision == "없음" && addr == "800014" && (bit == "11" || bit == "12" || bit == "13" || bit == "14"))
                    continue;
                // 화물감지센서 없음: 800010 INPUT BIT 0 그리드에서 제외 (1~4개소일 때만 승강대적재1 b 표시)
                if (liftStop == "없음" && addr == "800010" && bit == "0")
                    continue;
                // 화물돌출센서 없음: 800010 INPUT BIT 4·5 그리드에서 제외
                if (cargoProtrusion == "없음" && addr == "800010" && (bit == "4" || bit == "5"))
                    continue;
                // 표준(고정신호): 800010 INPUT BIT 8 승강부원점 — 옵션 무관 항상 표시 (제외 안 함)

                string finalSignal = "";
                string finalLogic = "";

                // Embedded 행에서 IO/IF 5열 직접 읽기 (인덱스 24~28: IO커넥터No, IO PIN-NO, IF 기판명칭, IF 커넥터NO, IF PIN-NO)
                string ioconnactorno = (row.Length > 24) ? (row[24] ?? "") : "";
                string iopinno = (row.Length > 25) ? (row[25] ?? "") : "";
                string ifboardname = (row.Length > 26) ? (row[26] ?? "") : "";
                string ifconnactorno = (row.Length > 27) ? (row[27] ?? "") : "";
                string ifpinno = (row.Length > 28) ? (row[28] ?? "") : "";

                int bitNum = int.TryParse(bit, out var b) ? b : -1;

                // 1. 승강대 타입별 신호 (CHUCK/CAGE/컨베이어). 해당 옵션 선택 시 그 옵션 신호가 우선되도록 예외 처리
                bool skipByOption = (addr == "800010" && (bit == "4" || bit == "5") && cargoProtrusion != "없음")
                    || (addr == "800016" && bit == "2" && formType == "직선형")
                    || (addr == "800016" && (bit == "4" || bit == "5") && branchDevice == "있음")
                    || (addr == "800016" && (bit == "8" || bit == "9" || bit == "10" || bit == "11") && (commType == "8bit 센서" || commType == "SS무선"));
                if (hoistType == "CHUCK" && row.Length > 5 && !string.IsNullOrEmpty(row[4]) && !skipByOption)
                {
                    finalSignal = row[4];
                    finalLogic = row[5];
                }
                else if (hoistType == "CAGE" && row.Length > 7 && !string.IsNullOrEmpty(row[6]) && !skipByOption)
                {
                    finalSignal = row[6];
                    finalLogic = row[7];
                }
                else if (hoistType == "컨베이어" && row.Length > 9 && !string.IsNullOrEmpty(row[8]) && !skipByOption)
                {
                    finalSignal = row[8];
                    finalLogic = row[9];
                }
                // 1-1. 승강대별 800014 INPUT BIT 1·3 고정 (CHUCK: 화물탑재/이재 가능, CAGE: 상승가/하강가)
                if (hoistType == "CHUCK" && addr == "800014" && (bit == "1" || bit == "3"))
                {
                    finalSignal = bit == "1" ? "화물탑재 가능" : "화물이재 가능";
                    finalLogic = "a";
                }
                else if (hoistType == "CAGE" && addr == "800014" && (bit == "1" || bit == "3"))
                {
                    finalSignal = bit == "1" ? "상승가" : "하강가";
                    finalLogic = "a";
                }
                // 1-2. 화물감지센서 1~4개소: 800010 INPUT BIT 0 → 승강대적재1(b) (없음이면 이미 상단에서 제외)
                if (addr == "800010" && bit == "0" && (liftStop == "1개소" || liftStop == "2개소" || liftStop == "3개소" || liftStop == "4개소"))
                {
                    finalSignal = "승강대적재1";
                    finalLogic = "b";
                }
                // 1-3. 승강정지센서 그외(optCollisionDetect>=2): 800010 INPUT BIT 8 → 승강부원점(a)
                if (addr == "800010" && bit == "8" && optCollisionDetect >= 2)
                {
                    finalSignal = "승강부원점";
                    finalLogic = "a";
                }
                // 1-4. 옵션설정 충돌검출=있음(optCollisionDetect>=2): 800010 BIT 15 충돌검출(유지) b, 800014 BIT 15 충돌검출(센서입력) b
                if (optCollisionDetect >= 2 && addr == "800010" && bit == "15")
                {
                    finalSignal = "충돌검출(유지)";
                    finalLogic = "b";
                }
                else if (optCollisionDetect >= 2 && addr == "800014" && bit == "15")
                {
                    finalSignal = "충돌검출(센서입력)";
                    finalLogic = "b";
                }
                // 1-5. 옵션설정 극한검출=있음(optLimitDetect>=2): 800012 BIT 0 극한검출(릴레이접점) b, 800016 BIT 3 극한검출(센서입력) b
                if (optLimitDetect >= 2 && addr == "800012" && bit == "0")
                {
                    finalSignal = "극한검출(릴레이접점)";
                    finalLogic = "b";
                }
                else if (optLimitDetect >= 2 && addr == "800016" && bit == "3")
                {
                    finalSignal = "극한검출(센서입력)";
                    finalLogic = "b";
                }
                // 1-6. 옵션설정 이재모터 수=1개(optTransferMotor==2): 800012 INPUT BIT 3 승강모터 브레이크 개방스위치 a
                if (optTransferMotor == 2 && addr == "800012" && bit == "3")
                {
                    finalSignal = "승강모터 브레이크 개방스위치";
                    finalLogic = "a";
                }

                // 2. 화물돌출센서 (800010 BIT 4, 5) — 없음이면 상단에서 제외. 우측=4번만, 좌측=5번만, 좌/우=둘 다. 화물돌출 선택 시 승강대 무관 표시.
                if (addr == "800010" && (bitNum == 4 || bitNum == 5) && cargoProtrusion != "없음")
                {
                    bool showRight = (cargoProtrusion == "우측" || cargoProtrusion == "좌/우");
                    bool showLeft = (cargoProtrusion == "좌측" || cargoProtrusion == "좌/우");

                    if (bitNum == 4 && showRight)
                    {
                        finalSignal = "우측돌출감지";
                        finalLogic = "b";
                    }
                    else if (bitNum == 5 && showLeft)
                    {
                        finalSignal = "좌측돌출감지";
                        finalLogic = "b";
                    }
                    else
                        continue;
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

                // 4. 충돌방지센서 신호 (800014 BIT 11-14) — 없음이면 이미 상단에서 제외
                // 1개소 전측: 11·12만 표시(충돌방지전 감속/정지 b), 13·14 제외. 1개소 후측: 13·14만 표시(충돌방지후 감속/정지 b), 11·12 제외. 2개소: 11~14 모두 표시
                bool showFront = (collision == "1개소 전측" || collision == "2개소");
                bool showRear = (collision == "1개소 후측" || collision == "2개소");

                if (addr == "800014" && (bitNum == 11 || bitNum == 12 || bitNum == 13 || bitNum == 14))
                {
                    bool isFrontSignal = (bitNum == 11 || bitNum == 12);
                    bool isRearSignal = (bitNum == 13 || bitNum == 14);

                    if (isFrontSignal && showFront && row.Length > 3 && !string.IsNullOrEmpty(row[2]))
                    {
                        finalSignal = row[2];
                        finalLogic = row[3];
                    }
                    else if (isRearSignal && showRear && row.Length > 3 && !string.IsNullOrEmpty(row[2]))
                    {
                        finalSignal = row[2];
                        finalLogic = row[3];
                    }
                    else
                        continue;
                }
                // 다른 충돌방지 관련 신호 (800010 BIT 15, 800012 BIT 0)
                else if (collision != "없음" && string.IsNullOrEmpty(finalSignal) && row.Length > 11 && !string.IsNullOrEmpty(row[10]))
                {
                    finalSignal = row[10];
                    finalLogic = row[11];
                }

                // 5. 통신방식 (ROP/8bit/SS무선) — 800016 BIT 8~11은 교신방법별 고정 신호명
                if (string.IsNullOrEmpty(finalSignal) && addr == "800016" && (bit == "8" || bit == "9" || bit == "10" || bit == "11"))
                {
                    if (commType == "8bit 센서")
                    {
                        string[] names = { "8BIT INPUT1", "8BIT INPUT2", "8BIT INPUT3", "8BIT INPUT4" };
                        int idx = int.Parse(bit) - 8;
                        finalSignal = names[idx];
                        finalLogic = "a";
                    }
                    else if (commType == "SS무선")
                    {
                        string[] names = { "현재위치확인1", "현재위치 확인2", "현재위치 확인3", "현재위치 확인4" };
                        int idx = int.Parse(bit) - 8;
                        finalSignal = names[idx];
                        finalLogic = "b";
                    }
                }
                if (string.IsNullOrEmpty(finalSignal))
                {
                    if (commType == "ROP" && row.Length > 13 && !string.IsNullOrEmpty(row[12]))
                    {
                        finalSignal = row[12];
                        finalLogic = row[13];
                    }
                    else if (commType == "8bit 센서" && row.Length > 15 && !string.IsNullOrEmpty(row[14]))
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

                // 6. 레이아웃 (직선형/분기장치) — 800016 BIT 2는 형태(formType), BIT 4·5는 분기장치(branchDevice)
                if (string.IsNullOrEmpty(finalSignal))
                {
                    if (addr == "800016" && bit == "2" && formType == "직선형" && row.Length > 19 && !string.IsNullOrEmpty(row[18]))
                    {
                        finalSignal = row[18];
                        finalLogic = row[19];
                    }
                    else if (addr == "800016" && (bit == "4" || bit == "5") && branchDevice == "있음" && row.Length > 21 && !string.IsNullOrEmpty(row[20]))
                    {
                        finalSignal = row[20];
                        finalLogic = row[21];
                    }
                    else if (layout == "직선형" && row.Length > 19 && !string.IsNullOrEmpty(row[18]))
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

                _allIoTable.Rows.Add(addr, "INPUT", bit, ioconnactorno, iopinno, ifboardname , ifconnactorno , ifpinno , finalSignal, finalLogic, false, "");
            }
        }

        /// <summary>내장 OUTPUT 데이터 처리</summary>
        private void ProcessEmbeddedOutputData(string hoistType, string commType, string branchDevice, string layout, string extraOpt, int optTransferMotor, int opt8bitStop, string unloadingInterlock)
        {
            // unloadingInterlock(이재인터록): 없음 → 800014 OUTPUT BIT 1 제외, 있음 → 구동가
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

                // 800018은 이재모터 수=1개(2)일 때만 표시
                if (addr == "800018" && optTransferMotor != 2)
                    continue;

                // Embedded 행에서 IO/IF 5열 직접 읽기 (인덱스 16~20: IO커넥터No, IO PIN-NO, IF 기판명칭, IF 커넥터NO, IF PIN-NO)
                string ioconnactorno = (row.Length > 16) ? (row[16] ?? "") : "";
                string iopinno = (row.Length > 17) ? (row[17] ?? "") : "";
                string ifboardname = (row.Length > 18) ? (row[18] ?? "") : "";
                string ifconnactorno = (row.Length > 19) ? (row[19] ?? "") : "";
                string ifpinno = (row.Length > 20) ? (row[20] ?? "") : "";

                // VLOOKUP 함수가 없는 BIT는 건너뛰기
                string bitKey = $"{addr}_{bit}";
                if (ExcludedOutputBits.Contains(bitKey))
                    continue;

                // 옵션설정: 이재모터 수=없음(1)이면 이재모터1 정전/역전/브레이크해제 표시 안 함
                if (optTransferMotor == 1 && addr == "800010" && (bit == "10" || bit == "11" || bit == "12"))
                    continue;
                // 옵션설정: 8bit전송정지=2점(2)일 때만 800014 OUTPUT BIT 10 전송정지1, BIT 15 전송정지2 표시, 그 외(1점 등)는 그리드에서 제외
                if (opt8bitStop != 2 && addr == "800014" && (bit == "10" || bit == "15"))
                    continue;
                // 교신방법 ROP/SS무선: 800014 OUTPUT BIT 8·9 표시 안 함
                if ((commType == "ROP" || commType == "SS무선") && addr == "800014" && (bit == "8" || bit == "9"))
                    continue;
                // 이재인터록 없음: 800014 OUTPUT BIT 1 그리드에서 제외
                if (unloadingInterlock == "없음" && addr == "800014" && bit == "1")
                    continue;
                // 분기장치 없음: 800014 OUTPUT BIT 11~14(분기인터록1~4) 전부 그리드에서 제외
                if (branchDevice == "없음" && addr == "800014" && (bit == "11" || bit == "12" || bit == "13" || bit == "14"))
                    continue;
                // 분기장치 있음: 분기신호 N점이면 분기인터록 1~N까지 표시 — 1점→11만, 2점→11·12, 3점→11·12·13, 4점→11·12·13·14
                if (branchDevice == "있음" && addr == "800014" && (bit == "11" || bit == "12" || bit == "13" || bit == "14") && (
                    (layout == "1점" && (bit == "12" || bit == "13" || bit == "14")) ||
                    (layout == "2점" && (bit == "13" || bit == "14")) ||
                    (layout == "3점" && bit == "14")))
                    continue;

                string finalSignal = "";
                string finalLogic = "";

                // 1. 승강대 타입별 신호 (CHUCK/CAGE/컨베이어). 해당 옵션 선택 시 그 옵션 신호가 우선되도록 예외 처리
                bool skipByOptionOut = (addr == "800014" && bit == "1" && unloadingInterlock == "있음")
                    || (addr == "800014" && (bit == "8" || bit == "9") && commType == "8bit 센서")
                    || (addr == "800014" && (bit == "11" || bit == "12" || bit == "13" || bit == "14") && branchDevice == "있음")
                    || (addr == "800014" && (bit == "10" || bit == "15") && opt8bitStop == 2)
                    || (extraOpt == "있음" && row.Length > 15 && !string.IsNullOrEmpty(row[14]));
                if (hoistType == "CHUCK" && row.Length > 5 && !string.IsNullOrEmpty(row[4]) && !skipByOptionOut)
                {
                    finalSignal = row[4];
                    finalLogic = row[5];
                }
                else if (hoistType == "CAGE" && row.Length > 7 && !string.IsNullOrEmpty(row[6]) && !skipByOptionOut)
                {
                    finalSignal = row[6];
                    finalLogic = row[7];
                }
                else if (hoistType == "컨베이어" && row.Length > 9 && !string.IsNullOrEmpty(row[8]) && !skipByOptionOut)
                {
                    finalSignal = row[8];
                    finalLogic = row[9];
                }
                // 1-1. 이재인터록 있음: 800014 OUTPUT BIT 1 → 구동가
                if (unloadingInterlock == "있음" && addr == "800014" && bit == "1")
                {
                    finalSignal = "구동가";
                    finalLogic = "";
                }

                // 2. 통신방식 — 800014 OUTPUT 8·9: 8bit 센서일 때만 데이터 요구/수신 완료
                if (string.IsNullOrEmpty(finalSignal) && addr == "800014" && (bit == "8" || bit == "9") && commType == "8bit 센서")
                {
                    finalSignal = bit == "8" ? "데이터 요구" : "수신 완료";
                }
                else if (string.IsNullOrEmpty(finalSignal) && commType == "ROP" && row.Length > 11 && !string.IsNullOrEmpty(row[10]))
                {
                    finalSignal = row[10];
                    finalLogic = row[11];
                }

                // 3. 분기장치(있음) — 분기인터록1~4: 분기신호(1~4점)에 따라 해당 BIT만 표시되고, 신호명은 row[12]/[13]
                if (string.IsNullOrEmpty(finalSignal) && branchDevice == "있음" && addr == "800014" && (bit == "11" || bit == "12" || bit == "13" || bit == "14") && row.Length > 13 && !string.IsNullOrEmpty(row[12]))
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
                _allIoTable.Rows.Add(addr, "OUTPUT", bit, ioconnactorno, iopinno, ifboardname, ifconnactorno, ifpinno, finalSignal, finalLogic, false, "");
            }

            // 800018 행은 EmbeddedOutputData에 포함되어 루프에서 처리됨 (optTransferMotor==2일 때만 continue 안 함)
        }

        /// <summary>전체 INPUT 시그널 추가 (옵션 필터 없음). 각 어드레스마다 BIT 0~15 전부 표시.</summary>
        private static void ProcessEmbeddedInputDataFull(DataTable target)
        {
            foreach (var row in EmbeddedInputData)
            {
                string addr = row[0];
                string bit = row[1];
                string baseSignal = row.Length > 2 ? (row[2] ?? "") : "";
                string baseLogic = row.Length > 3 ? (row[3] ?? "") : "";
                // 전체 테이블에서는 제외 비트 없이 0~15 전부 표시
                string ioconnactorno = (row.Length > 24) ? (row[24] ?? "") : "";
                string iopinno = (row.Length > 25) ? (row[25] ?? "") : "";
                string ifboardname = (row.Length > 26) ? (row[26] ?? "") : "";
                string ifconnactorno = (row.Length > 27) ? (row[27] ?? "") : "";
                string ifpinno = (row.Length > 28) ? (row[28] ?? "") : "";
                target.Rows.Add(addr, "INPUT", bit, ioconnactorno, iopinno, ifboardname, ifconnactorno, ifpinno, baseSignal, baseLogic, false, "");
            }
        }

        /// <summary>전체 OUTPUT 시그널 추가 (옵션 필터 없음). 각 어드레스마다 BIT 0~15 전부 표시.</summary>
        private static void ProcessEmbeddedOutputDataFull(DataTable target)
        {
            foreach (var row in EmbeddedOutputData)
            {
                string addr = row[0];
                string bit = row[1];
                string baseSignal = row.Length > 2 ? (row[2] ?? "") : "";
                string baseLogic = row.Length > 3 ? (row[3] ?? "") : "";
                // 전체 테이블에서는 제외 비트 없이 0~15 전부 표시
                string ioconnactorno = (row.Length > 16) ? (row[16] ?? "") : "";
                string iopinno = (row.Length > 17) ? (row[17] ?? "") : "";
                string ifboardname = (row.Length > 18) ? (row[18] ?? "") : "";
                string ifconnactorno = (row.Length > 19) ? (row[19] ?? "") : "";
                string ifpinno = (row.Length > 20) ? (row[20] ?? "") : "";
                target.Rows.Add(addr, "OUTPUT", bit, ioconnactorno, iopinno, ifboardname, ifconnactorno, ifpinno, baseSignal, baseLogic, false, "");
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
                         $"화물돌출센서: {comboOption.SelectedItem}\n" +
                         $"화물감지센서: {comboLiftStop.SelectedItem}\n" +
                         $"추가옵션(이재인터록 등): {comboCargoProtrusion.SelectedItem}\n\n" +
                         $"INPUT: {inputCount}개, OUTPUT: {outputCount}개\n" +
                         $"총 {_allIoTable.Rows.Count}개 I/O 로드됨";
            MessageBox.Show(msg, "적용 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>I.O 체크 셀: CSV 구분=사용자 행만 편집 가능. 그 외(AUTO 플래그 대상)는 편집 불가. CSV 미로드 시 신호명칭 있는 행만 편집 가능.</summary>
        private void dataGridViewIO_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (dataGridViewIO.Columns[e.ColumnIndex].Name != "I.O 체크") return;
            var dt = dataGridViewIO.DataSource as DataTable;
            if (dt == null || e.RowIndex < 0 || e.RowIndex >= dt.Rows.Count) return;
            if (_userCheckableIoKeys.Count > 0)
            {
                DataRow row = dt.Rows[e.RowIndex];
                string io = row["IO구분"]?.ToString()?.Trim() ?? "";
                string addr = row["어드레스"]?.ToString()?.Trim() ?? "";
                string bit = row["BIT"]?.ToString()?.Trim() ?? "";
                if (!_userCheckableIoKeys.Contains($"{io}_{addr}_{bit}"))
                    e.Cancel = true;
                return;
            }
            string sig = dt.Rows[e.RowIndex]["신호명칭"]?.ToString()?.Trim() ?? "";
            if (string.IsNullOrEmpty(sig))
                e.Cancel = true;
        }

        /// <summary>INPUT=빨강, OUTPUT=초록 글씨 / I.O 체크 셀: 구분=사용자 아닌 행(자동 체크 대상)은 회색 비활성 표시.</summary>
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
                bool grayOut = false;
                if (_userCheckableIoKeys.Count > 0)
                {
                    DataRow row = dt.Rows[e.RowIndex];
                    string io = row["IO구분"]?.ToString()?.Trim() ?? "";
                    string addr = row["어드레스"]?.ToString()?.Trim() ?? "";
                    string bit = row["BIT"]?.ToString()?.Trim() ?? "";
                    grayOut = !_userCheckableIoKeys.Contains($"{io}_{addr}_{bit}");
                }
                else
                {
                    string sig = dt.Rows[e.RowIndex]["신호명칭"]?.ToString()?.Trim() ?? "";
                    grayOut = string.IsNullOrEmpty(sig);
                }
                if (grayOut)
                {
                    e.CellStyle.BackColor = Color.FromArgb(52, 52, 56);
                    e.CellStyle.ForeColor = Color.Gray;
                }
            }
        }

        /// <summary>열 너비 설정 (머리글 줄바꿈 없음, 정렬은 Designer에서).</summary>
        private void SetGridColumnWidths()
        {
            if (dataGridViewIO.Columns.Count == 0) return;
            dataGridViewIO.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            // 어드레스, IO구분(숨김), BIT, I.O기판/커넥터No., IO기판/PIN-NO, 중계기판/기판명칭, 중계기판/커넥터NO, 중계기판 PIN-NO., 신호명칭, 논리, I.O 체크, 비고
            int[] widths = { 115, 0, 50, 185, 130, 170, 160, 155, 320, 72, 95, 120 };
            for (int i = 0; i < dataGridViewIO.Columns.Count && i < widths.Length; i++)
            {
                var col = dataGridViewIO.Columns[i];
                col.MinimumWidth = widths[i] > 0 ? widths[i] : 30;
                col.Width = widths[i] > 0 ? widths[i] : 0;
                if (col.Name == "IO구분")
                    col.Visible = false;
            }
        }

        private void FillAddressFilterCombo()
        {
            comboFilter.Items.Clear();
            comboFilter.Items.Add("전체");
            // 전체 시그널 테이블 기준으로 어드레스 목록 채움 (사용신호 필터 해제 시 모든 시그널 보이므로)
            DataTable refTable = (_allIoTableFull != null && _allIoTableFull.Rows.Count > 0) ? _allIoTableFull : _allIoTable;
            if (refTable != null && refTable.Rows.Count > 0)
            {
                var addrs = new HashSet<string>();
                foreach (DataRow row in refTable.Rows)
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

            // 사용신호 필터링 체크 해제 시 = 옵션과 관계없이 모든 시그널 표시 (_allIoTableFull)
            DataTable baseTable = (usedSignalOnly ? _allIoTable : _allIoTableFull) ?? _allIoTable;
            if (baseTable.Rows.Count == 0 && baseTable != _allIoTable)
                baseTable = _allIoTable;

            DataTable source = baseTable;
            if (!string.IsNullOrEmpty(sel) && sel != "전체")
            {
                var byAddr = baseTable.Clone();
                foreach (DataRow row in baseTable.Rows)
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
            // 사용신호 필터링 체크 시 = 신호명이 있는 행만 표시
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
                sfd.Filter = "엑셀 파일|*.xlsx|PDF/이미지|*.pdf;*.png|모든 파일|*.*";
                sfd.DefaultExt = "xlsx";
                sfd.FileName = "DIO_I.O_체크성적서_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                if (sfd.ShowDialog() != DialogResult.OK) return;
                string path = sfd.FileName;
                try
                {
                    if (path.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    {
                        string templatePath = ResolveExcelTemplatePath();
                        if (string.IsNullOrEmpty(templatePath)) return;
                        SaveToExcel(templatePath, path);
                        MessageBox.Show("저장했습니다.\r\n" + path, "체크시트 출력 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        SaveToPdf(path);
                        MessageBox.Show("저장했습니다.\r\n" + path, "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("저장 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>지정된 엑셀 템플릿 파일명. 프로그램 폴더(StartupPath)에서만 사용.</summary>
        private const string ExcelTemplateFileName = "[Quality Inspection Report] EMS_Electricity_Rev1.0_260206.xlsx";

        /// <summary>엑셀 템플릿 경로: 프로그램 폴더 내 지정된 파일만 사용. 사용자 선택 대화상자 없음.</summary>
        private static string ResolveExcelTemplatePath()
        {
            string path = Path.Combine(Application.StartupPath ?? "", ExcelTemplateFileName);
            if (File.Exists(path)) return path;
            MessageBox.Show("템플릿 파일을 찾을 수 없습니다.\r\n프로그램 폴더에 아래 파일을 넣어 주세요.\r\n\r\n" + ExcelTemplateFileName, "템플릿 없음", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        /// <summary>엑셀 템플릿을 열어 시트1에 검사일/검사자/유형·I.O만 채우고, 시트2에 DEVICE LIST 전용(양식 동일)으로 채운 뒤 저장.</summary>
        private void SaveToExcel(string templatePath, string outputPath)
        {
            if (!File.Exists(templatePath))
                throw new FileNotFoundException("템플릿 파일을 찾을 수 없습니다.", templatePath);

            using (var workbook = new XLWorkbook(templatePath))
            {
                var ws = workbook.Worksheet(1);
                if (ws == null) throw new InvalidOperationException("시트가 없습니다.");

                ReplaceExcelPlaceholdersClosedXML(ws);
                FillIoTableClosedXML(ws, 0);

                var wsDevice = workbook.Worksheets.Count >= 2 ? workbook.Worksheet(2) : workbook.AddWorksheet("DEVICE LIST");
                if (wsDevice != null)
                    FillDeviceListSheetClosedXML(wsDevice);

                workbook.SaveAs(outputPath);
            }
        }

        /// <summary>별도 시트에 DEVICE LIST만 동일 양식(PDM No, DESCRIPTION, SPECIFICATION, MANUFACTURE, 확인)으로 채움. 시트 선삭제로 중복·빈행 방지, 테두리·열너비·헤더 음영·줄바꿈·행높이 적용.</summary>
        private void FillDeviceListSheetClosedXML(IXLWorksheet ws)
        {
            if (listViewDeviceList?.Items == null) return;
            int n = listViewDeviceList.Items.Count;
            const int lastCol = 5;

            var used = ws.RangeUsed();
            if (used != null)
                ws.Range(used.FirstRow().RowNumber(), 1, used.LastRow().RowNumber(), Math.Max(lastCol, used.LastColumn().ColumnNumber())).Clear();
            else
                ws.Range(1, 1, 500, lastCol).Clear();

            ws.Cell(1, 1).Value = "Device List";
            ws.Range(1, 1, 1, lastCol).Merge();
            var titleCell = ws.Cell(1, 1);
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontName = "Malgun Gothic";
            titleCell.Style.Font.FontSize = 24;
            titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(2, 1).Value = "PDM No";
            ws.Cell(2, 2).Value = "DESCRIPTION";
            ws.Cell(2, 3).Value = "SPECIFICATION";
            ws.Cell(2, 4).Value = "MANUFACTURE";
            ws.Cell(2, 5).Value = "확인";

            ws.Column(1).Width = 14;
            ws.Column(2).Width = 38;
            ws.Column(3).Width = 38;
            ws.Column(4).Width = 28;
            ws.Column(5).Width = 8;

            for (int c = 1; c <= lastCol; c++)
            {
                var cell = ws.Cell(2, c);
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0xD9, 0xD9, 0xD9);
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.FromArgb(0xE5, 0x39, 0x35);
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
            ws.Row(2).Height = 31.5;

            const double dataRowHeight = 24.0;
            int excelRow = 3;
            for (int r = 0; r < n; r++)
            {
                var item = listViewDeviceList.Items[r];
                bool isCategory = (item.Tag as string) == "Category";
                if (isCategory)
                {
                    string catText = item.Text ?? "";
                    if (string.IsNullOrWhiteSpace(catText) && item.SubItems.Count > 0)
                        catText = item.SubItems[0]?.Text ?? "";
                    if (string.IsNullOrWhiteSpace(catText))
                        continue;
                    ws.Cell(excelRow, 1).Value = catText;
                    ws.Cell(excelRow, 2).Value = "";
                    ws.Cell(excelRow, 3).Value = "";
                    ws.Cell(excelRow, 4).Value = "";
                    ws.Cell(excelRow, 5).Value = "";
                }
                else
                {
                    // ListViewItem: Text=1열(PDM No), SubItems[0]=1열과 동일, SubItems[1]=DESCRIPTION, [2]=SPEC, [3]=MANUFACTURE
                    string c1 = item.Text ?? "";
                    string c2 = item.SubItems.Count > 1 ? (item.SubItems[1]?.Text ?? "") : "";
                    string c3 = item.SubItems.Count > 2 ? (item.SubItems[2]?.Text ?? "") : "";
                    string c4 = item.SubItems.Count > 3 ? (item.SubItems[3]?.Text ?? "") : "";
                    if (string.IsNullOrWhiteSpace(c1) && string.IsNullOrWhiteSpace(c2) && string.IsNullOrWhiteSpace(c3) && string.IsNullOrWhiteSpace(c4))
                        continue;
                    ws.Cell(excelRow, 1).Value = c1;
                    ws.Cell(excelRow, 2).Value = c2;
                    ws.Cell(excelRow, 3).Value = c3;
                    ws.Cell(excelRow, 4).Value = c4;
                    ws.Cell(excelRow, 5).Value = item.Checked ? "O" : "";
                }
                ws.Cell(excelRow, 2).Style.Alignment.WrapText = true;
                ws.Cell(excelRow, 3).Style.Alignment.WrapText = true;
                ws.Cell(excelRow, 4).Style.Alignment.WrapText = true;
                ws.Row(excelRow).Height = dataRowHeight;
                excelRow++;
            }

            int lastRow = excelRow - 1;
            if (lastRow < 2) lastRow = 2;
            var tableRange = ws.Range(1, 1, lastRow, lastCol);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        private void ReplaceExcelPlaceholdersClosedXML(IXLWorksheet ws)
        {
            string testDate = TestDate ?? "";
            string testerName = TesterName ?? "";
            string typeVal = Line_Setup.SavedRailType ?? "";

            var used = ws.RangeUsed();
            if (used == null) return;
            int lastRow = Math.Min(used.LastRow().RowNumber(), 60);
            int lastCol = Math.Min(used.LastColumn().ColumnNumber(), 30);

            for (int r = 1; r <= lastRow; r++)
            {
                for (int c = 1; c <= lastCol; c++)
                {
                    var cell = ws.Cell(r, c);
                    string s = cell.GetString();
                    if (string.IsNullOrEmpty(s)) continue;
                    if (s.IndexOf("textBoxTestDate", StringComparison.OrdinalIgnoreCase) >= 0)
                        cell.Value = testDate;
                    else if (s.IndexOf("textBoxTesterName", StringComparison.OrdinalIgnoreCase) >= 0)
                        cell.Value = testerName;
                    else if (s.IndexOf("type_sel_box", StringComparison.OrdinalIgnoreCase) >= 0)
                        cell.Value = typeVal;
                }
            }
        }

        /// <summary>(B) 정적 ROW 한 번만 찾기. PDM No/DESCRIPTION 등 2개 이상 있는 첫 행 번호 반환, 없으면 0.</summary>
        private static int FindDeviceListStaticHeaderRow(IXLWorksheet ws)
        {
            var used = ws.RangeUsed();
            if (used == null) return 0;
            int maxRow = Math.Min(used.LastRow().RowNumber(), 80);
            int maxCol = used.LastColumn().ColumnNumber();
            for (int r = 1; r <= maxRow; r++)
            {
                int pdm = -1, desc = -1, spec = -1, manu = -1, confirm = -1;
                for (int i = 1; i <= maxCol; i++)
                {
                    string h = ws.Cell(r, i).GetString();
                    if (string.IsNullOrEmpty(h)) continue;
                    if (FindColContains(h, new[] { "PDM No", "PDM No.", "PDM번호", "PDM NO" })) pdm = i - 1;
                    if (FindColContains(h, new[] { "DESCRIPTION", "설명", "품목", "품목명", "Description", "품명" })) desc = i - 1;
                    if (FindColContains(h, new[] { "SPECIFICATION", "규격", "사양", "Spec", "Specification", "세부품명", "세부사양" })) spec = i - 1;
                    if (FindColContains(h, new[] { "MANUFACTURE", "Manufacture", "제조사", "제조사명", "MAKER" })) manu = i - 1;
                    if (FindColContains(h, new[] { "확인", "Hwagin", "체크" })) confirm = i - 1;
                }
                int headerLikeCount = (pdm >= 0 ? 1 : 0) + (desc >= 0 ? 1 : 0) + (spec >= 0 ? 1 : 0) + (manu >= 0 ? 1 : 0);
                if (headerLikeCount >= 2 && (pdm >= 0 || desc >= 0) && !RowContainsPdmNumber(ws, r, maxCol))
                    return r;
            }
            return 0;
        }

        /// <summary>ClosedXML 시트에서 "PDM No"/"DESCRIPTION" 헤더로 DEVICE LIST 테이블 찾아 listViewDeviceList 데이터로 채움. staticHeaderRow&gt;0이면 해당 행을 정적 헤더로 사용(검색 생략).</summary>
        private void FillDeviceListClosedXML(IXLWorksheet ws, int staticHeaderRow = 0)
        {
            if (listViewDeviceList?.Items == null) return;
            int programRowCount = listViewDeviceList.Items.Count;
            if (programRowCount <= 0) return;

            int headerRow;
            int colPdm, colDesc, colSpec, colManu, colConfirm;
            if (staticHeaderRow > 0)
            {
                headerRow = staticHeaderRow;
                if (!GetDeviceListColumnsFromRow(ws, headerRow, out colPdm, out colDesc, out colSpec, out colManu, out colConfirm))
                    return;
            }
            else if (!FindDeviceListHeaderClosedXML(ws, 0, out headerRow, out colPdm, out colDesc, out colSpec, out colManu, out colConfirm))
                return;
            if (colPdm < 0 && colDesc < 0) return;

            int dataStartRow = headerRow + 1;
            int templateDataRowCount = CountDeviceListTemplateDataRowsClosedXML(ws, dataStartRow);
            if (templateDataRowCount < 0) return;

            int insertAfterRow;
            if (templateDataRowCount <= 0)
            {
                insertAfterRow = headerRow;
                int insertCount = programRowCount;
                if (insertCount > 0)
                {
                    double sourceRowHeight = ws.Row(insertAfterRow).Height > 0 ? ws.Row(insertAfterRow).Height : 15;
                    ws.Row(insertAfterRow).InsertRowsBelow(insertCount);
                    for (int i = 1; i <= insertCount; i++)
                        ws.Row(insertAfterRow + i).Height = sourceRowHeight;
                }
            }
            else if (programRowCount > templateDataRowCount)
            {
                int insertCount = programRowCount - templateDataRowCount;
                insertAfterRow = dataStartRow + templateDataRowCount - 1;
                double sourceRowHeight = ws.Row(insertAfterRow).Height;
                ws.Row(insertAfterRow).InsertRowsBelow(insertCount);
                for (int i = 1; i <= insertCount; i++)
                    ws.Row(insertAfterRow + i).Height = sourceRowHeight;
            }

            for (int r = 0; r < programRowCount; r++)
            {
                int excelRow = dataStartRow + r;
                var item = listViewDeviceList.Items[r];
                bool isCategory = (item.Tag as string) == "Category";
                if (isCategory)
                {
                    string catText = item.Text ?? "";
                    if (string.IsNullOrWhiteSpace(catText) && item.SubItems.Count > 0)
                        catText = item.SubItems[0]?.Text ?? "";
                    if (colPdm >= 0) ws.Cell(excelRow, colPdm + 1).Value = catText;
                    if (colDesc >= 0) ws.Cell(excelRow, colDesc + 1).Value = "";
                    if (colSpec >= 0) ws.Cell(excelRow, colSpec + 1).Value = "";
                    if (colManu >= 0) ws.Cell(excelRow, colManu + 1).Value = "";
                    if (colConfirm >= 0) ws.Cell(excelRow, colConfirm + 1).Value = "";
                }
                else
                {
                    if (colPdm >= 0) ws.Cell(excelRow, colPdm + 1).Value = item.Text ?? "";
                    if (colDesc >= 0) ws.Cell(excelRow, colDesc + 1).Value = item.SubItems.Count > 0 ? (item.SubItems[0]?.Text ?? "") : "";
                    if (colSpec >= 0) ws.Cell(excelRow, colSpec + 1).Value = item.SubItems.Count > 1 ? (item.SubItems[1]?.Text ?? "") : "";
                    if (colManu >= 0) ws.Cell(excelRow, colManu + 1).Value = item.SubItems.Count > 2 ? (item.SubItems[2]?.Text ?? "") : "";
                    if (colConfirm >= 0) ws.Cell(excelRow, colConfirm + 1).Value = item.Checked ? "O" : "";
                }
            }

            if (programRowCount < templateDataRowCount)
            {
                for (int r = programRowCount; r < templateDataRowCount; r++)
                {
                    int excelRow = dataStartRow + r;
                    foreach (int col in new[] { colPdm, colDesc, colSpec, colManu, colConfirm })
                        if (col >= 0) ws.Cell(excelRow, col + 1).Value = "";
                }
            }

            int sectionStartRow = FindDeviceListSectionStartFromRow(ws, dataStartRow + programRowCount, 100);
            if (sectionStartRow > dataStartRow + programRowCount)
            {
                int gapFirstRow = dataStartRow + programRowCount;
                int gapLastRow = sectionStartRow - 1;
                for (int r = gapFirstRow; r <= gapLastRow; r++)
                    foreach (int col in new[] { colPdm, colDesc, colSpec, colManu, colConfirm })
                        if (col >= 0) ws.Cell(r, col + 1).Value = "";
                int gapColCount = 30;
                var usedRange = ws.RangeUsed();
                if (usedRange != null) gapColCount = Math.Max(gapColCount, Math.Min(usedRange.LastColumn().ColumnNumber(), 50));
                var gapRange = ws.Range(gapFirstRow, 1, gapLastRow, gapColCount);
                gapRange.Clear();
            }
        }

        /// <summary>지정 행에서 DEVICE LIST 열 인덱스만 추출. (B) 정적 ROW 사용 시.</summary>
        private static bool GetDeviceListColumnsFromRow(IXLWorksheet ws, int row, out int colPdm, out int colDesc, out int colSpec, out int colManu, out int colConfirm)
        {
            colPdm = colDesc = colSpec = colManu = colConfirm = -1;
            var used = ws.RangeUsed();
            if (used == null || row < 1) return false;
            int maxCol = used.LastColumn().ColumnNumber();
            for (int i = 1; i <= maxCol; i++)
            {
                string h = ws.Cell(row, i).GetString();
                if (string.IsNullOrEmpty(h)) continue;
                if (FindColContains(h, new[] { "PDM No", "PDM No.", "PDM번호", "PDM NO" })) colPdm = i - 1;
                if (FindColContains(h, new[] { "DESCRIPTION", "설명", "품목", "품목명", "Description", "품명" })) colDesc = i - 1;
                if (FindColContains(h, new[] { "SPECIFICATION", "규격", "사양", "Spec", "Specification", "세부품명", "세부사양" })) colSpec = i - 1;
                if (FindColContains(h, new[] { "MANUFACTURE", "Manufacture", "제조사", "제조사명", "MAKER" })) colManu = i - 1;
                if (FindColContains(h, new[] { "확인", "Hwagin", "체크" })) colConfirm = i - 1;
            }
            return colPdm >= 0 || colDesc >= 0;
        }

        private static bool FindDeviceListHeaderClosedXML(IXLWorksheet ws, int startFromRow, out int headerRow, out int colPdm, out int colDesc, out int colSpec, out int colManu, out int colConfirm)
        {
            colPdm = colDesc = colSpec = colManu = colConfirm = -1;
            headerRow = 0;
            var used = ws.RangeUsed();
            if (used == null) return false;
            int maxRow = Math.Min(used.LastRow().RowNumber(), 80);
            int maxCol = used.LastColumn().ColumnNumber();
            int rStart = (startFromRow > 0) ? startFromRow : 1;

            for (int r = rStart; r <= maxRow; r++)
            {
                int pdm = -1, desc = -1, spec = -1, manu = -1, confirm = -1;
                for (int i = 1; i <= maxCol; i++)
                {
                    string h = ws.Cell(r, i).GetString();
                    if (string.IsNullOrEmpty(h)) continue;
                    if (FindColContains(h, new[] { "PDM No", "PDM No.", "PDM번호", "PDM NO" })) pdm = i - 1;
                    if (FindColContains(h, new[] { "DESCRIPTION", "설명", "품목", "품목명", "Description", "품명" })) desc = i - 1;
                    if (FindColContains(h, new[] { "SPECIFICATION", "규격", "사양", "Spec", "Specification", "세부품명", "세부사양" })) spec = i - 1;
                    if (FindColContains(h, new[] { "MANUFACTURE", "Manufacture", "제조사", "제조사명", "MAKER" })) manu = i - 1;
                    if (FindColContains(h, new[] { "확인", "Hwagin", "체크" })) confirm = i - 1;
                }
                int headerLikeCount = (pdm >= 0 ? 1 : 0) + (desc >= 0 ? 1 : 0) + (spec >= 0 ? 1 : 0) + (manu >= 0 ? 1 : 0);
                if (headerLikeCount >= 2 && (pdm >= 0 || desc >= 0))
                {
                    if (RowContainsPdmNumber(ws, r, maxCol)) continue;
                    headerRow = r;
                    colPdm = pdm;
                    colDesc = desc;
                    colSpec = spec;
                    colManu = manu;
                    colConfirm = confirm;
                    return true;
                }
            }
            return false;
        }

        private static bool RowContainsPdmNumber(IXLWorksheet ws, int row, int maxCol)
        {
            for (int c = 1; c <= maxCol; c++)
            {
                string s = ws.Cell(row, c).GetString();
                if (string.IsNullOrEmpty(s)) continue;
                string trimmed = s.Trim().Replace(" ", "");
                if (trimmed.Length >= 6 && trimmed.Length <= 12 && Regex.IsMatch(trimmed, @"^\d+$")) return true;
            }
            return false;
        }

        private static int CountDeviceListTemplateDataRowsClosedXML(IXLWorksheet ws, int dataStartRow)
        {
            for (int r = dataStartRow; r <= dataStartRow + 500; r++)
            {
                if (IsDeviceListSectionStartRow(ws, r))
                    return r - dataStartRow;
            }
            return 20;
        }

        /// <summary>해당 행이 다음 섹션 시작(어드레스/I.O 또는 1.2 제작 검사)인지 판별. 데이터 셀의 "1.2"(형번 등)는 제외.</summary>
        private static bool IsDeviceListSectionStartRow(IXLWorksheet ws, int row)
        {
            var used = ws.RangeUsed();
            if (used == null) return false;
            int maxCol = Math.Min(used.LastColumn().ColumnNumber(), 20);
            for (int c = 1; c <= maxCol; c++)
            {
                string s = ws.Cell(row, c).GetString();
                if (string.IsNullOrEmpty(s)) continue;
                if (s.IndexOf("어드레스", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (s.IndexOf("제작 검사", StringComparison.OrdinalIgnoreCase) >= 0 || s.IndexOf("제작검사", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (s.IndexOf("1.2.", StringComparison.OrdinalIgnoreCase) >= 0 || s.IndexOf("1-2.", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (Regex.IsMatch(s, @"(^|\s)1[-.]2\s", RegexOptions.IgnoreCase)) return true;
            }
            return false;
        }

        private static int FindDeviceListSectionStartFromRow(IXLWorksheet ws, int startRow, int maxSearchRows)
        {
            for (int r = startRow; r < startRow + maxSearchRows; r++)
            {
                if (IsDeviceListSectionStartRow(ws, r)) return r;
            }
            return startRow + maxSearchRows;
        }

        /// <summary>ClosedXML 시트에서 "어드레스" 헤더로 I.O 테이블 찾아 프로그램 데이터로 채움. staticHeaderRow&gt;0이면 해당 행 아래에서만 I.O 사용(B). 쓰기 시 정적 ROW는 건너뜀(C).</summary>
        private int FillIoTableClosedXML(IXLWorksheet ws, int staticHeaderRow = 0)
        {
            DataTable dt = dataGridViewIO?.DataSource as DataTable ?? _allIoTable;
            if (dt == null || !dt.Columns.Contains("어드레스") || !dt.Columns.Contains("신호명칭")) return 0;

            int headerRow;
            int colNo, colAddr, colBit, colSig, colLogic, colIoCheck, colRemarks;
            if (!FindIoTableHeaderClosedXML(ws, staticHeaderRow, out headerRow, out colNo, out colAddr, out colBit, out colSig, out colLogic, out colIoCheck, out colRemarks))
                return 0;

            int dataStartRow = headerRow + 1;
            int templateDataRowCount = CountTemplateDataRowsClosedXML(ws, dataStartRow, colAddr, colSig);
            int programRowCount = dt.Rows.Count;
            if (programRowCount <= 0) return 0;

            int idxAddr = dt.Columns.IndexOf("어드레스");
            int idxBit = dt.Columns.IndexOf("BIT");
            int idxSig = dt.Columns.IndexOf("신호명칭");
            int idxLogic = dt.Columns.IndexOf("논리");
            int idxIoCheck = dt.Columns.IndexOf("I.O 체크");
            int idxRemarks = dt.Columns.IndexOf("비고");
            if (idxAddr < 0 || idxSig < 0) return 0;

            if (programRowCount > templateDataRowCount)
            {
                int insertCount = programRowCount - templateDataRowCount;
                int insertAfterRow = dataStartRow + templateDataRowCount - 1;
                double sourceRowHeight = ws.Row(insertAfterRow).Height;
                ws.Row(insertAfterRow).InsertRowsBelow(insertCount);
                for (int i = 1; i <= insertCount; i++)
                    ws.Row(insertAfterRow + i).Height = sourceRowHeight;
            }

            for (int r = 0; r < programRowCount; r++)
            {
                int excelRow = dataStartRow + r;
                if (staticHeaderRow > 0 && excelRow == staticHeaderRow) continue;
                DataRow dataRow = dt.Rows[r];
                if (colNo >= 0) ws.Cell(excelRow, colNo + 1).Value = (r + 1).ToString();
                ws.Cell(excelRow, colAddr + 1).Value = dataRow[idxAddr]?.ToString() ?? "";
                if (colBit >= 0 && idxBit >= 0) ws.Cell(excelRow, colBit + 1).Value = dataRow[idxBit]?.ToString() ?? "";
                ws.Cell(excelRow, colSig + 1).Value = dataRow[idxSig]?.ToString() ?? "";
                if (colLogic >= 0 && idxLogic >= 0) ws.Cell(excelRow, colLogic + 1).Value = dataRow[idxLogic]?.ToString() ?? "";
                if (colIoCheck >= 0 && idxIoCheck >= 0)
                    ws.Cell(excelRow, colIoCheck + 1).Value = (dataRow[idxIoCheck] is bool b && b) ? "O" : "";
                if (colRemarks >= 0 && idxRemarks >= 0) ws.Cell(excelRow, colRemarks + 1).Value = dataRow[idxRemarks]?.ToString() ?? "";
            }

            // I.O 테이블 데이터 행 범위 안에서만: 프로그램 행 수보다 많은 부분은 이 테이블 7개 열만 비움. (C) 정적 ROW 제외.
            if (programRowCount < templateDataRowCount)
            {
                for (int r = programRowCount; r < templateDataRowCount; r++)
                {
                    int excelRow = dataStartRow + r;
                    if (staticHeaderRow > 0 && excelRow == staticHeaderRow) continue;
                    foreach (int col in new[] { colNo, colAddr, colBit, colSig, colLogic, colIoCheck, colRemarks })
                        if (col >= 0) ws.Cell(excelRow, col + 1).Value = "";
                }
            }

            // InsertRowsBelow로 밀려난 템플릿 행(No. 21, 22만 있는 행) 제거: "1-2. 제작 검사" 시작 행 직전까지 I.O 7열만 비우고, 그 구간은 줄(테두리) 없음으로.
            int sectionStartRow = FindRowContainingText(ws, dataStartRow + programRowCount, dataStartRow + programRowCount + 30, new[] { "1-2", "제작 검사", "제작검사" });
            if (sectionStartRow > dataStartRow + programRowCount)
            {
                int gapFirstRow = dataStartRow + programRowCount;
                int gapLastRow = sectionStartRow - 1;
                for (int r = gapFirstRow; r <= gapLastRow; r++)
                {
                    if (staticHeaderRow > 0 && r == staticHeaderRow) continue;
                    foreach (int col in new[] { colNo, colAddr, colBit, colSig, colLogic, colIoCheck, colRemarks })
                        if (col >= 0) ws.Cell(r, col + 1).Value = "";
                }

                // 원인: 템플릿의 해당 구간에 검은 테두리가 있어 줄로 보임. (C) 정적 ROW는 Clear 제외.
                int gapColCount = 30;
                var usedRange = ws.RangeUsed();
                if (usedRange != null) gapColCount = Math.Max(gapColCount, Math.Min(usedRange.LastColumn().ColumnNumber(), 50));
                for (int r = gapFirstRow; r <= gapLastRow; r++)
                {
                    if (staticHeaderRow > 0 && r == staticHeaderRow) continue;
                    ws.Range(r, 1, r, gapColCount).Clear();
                }

                // 빈칸 최소화: 한 칸만 띄우고 나머지 갭 행 삭제
                int gapRowCount = gapLastRow - gapFirstRow + 1;
                if (gapRowCount > 1 && staticHeaderRow <= 0)
                {
                    ws.Rows(gapFirstRow + 1, gapLastRow).Delete();
                }
            }
            return dataStartRow + programRowCount - 1;
        }

        /// <summary>지정 행 구간에서 주어진 키워드 중 하나라도 포함된 첫 행 번호 반환. 없으면 endRow+1.</summary>
        private static int FindRowContainingText(IXLWorksheet ws, int startRow, int endRow, string[] keywords)
        {
            var used = ws.RangeUsed();
            if (used == null) return endRow + 1;
            int maxCol = Math.Min(used.LastColumn().ColumnNumber(), 20);
            for (int r = startRow; r <= endRow; r++)
            {
                for (int c = 1; c <= maxCol; c++)
                {
                    string s = ws.Cell(r, c).GetString();
                    if (string.IsNullOrEmpty(s)) continue;
                    foreach (var kw in keywords)
                        if (s.IndexOf(kw.Trim(), StringComparison.OrdinalIgnoreCase) >= 0)
                            return r;
                }
            }
            return endRow + 1;
        }

        private static bool FindIoTableHeaderClosedXML(IXLWorksheet ws, int staticHeaderRow, out int headerRow, out int colNo, out int colAddr, out int colBit, out int colSig, out int colLogic, out int colIoCheck, out int colRemarks)
        {
            colNo = colAddr = colBit = colSig = colLogic = colIoCheck = colRemarks = -1;
            headerRow = 0;
            var used = ws.RangeUsed();
            if (used == null) return false;
            int maxRow = Math.Min(used.LastRow().RowNumber(), 80);
            int maxCol = used.LastColumn().ColumnNumber();

            for (int r = 1; r <= maxRow; r++)
            {
                for (int c = 1; c <= maxCol; c++)
                {
                    string s = ws.Cell(r, c).GetString();
                    if (string.IsNullOrEmpty(s)) continue;
                    if (s.IndexOf("어드레스", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (staticHeaderRow > 0 && (r + 1) <= staticHeaderRow)
                            continue;
                        if (RowContainsDeviceListHeader(ws, r + 1, maxCol))
                            continue;
                        headerRow = r;
                        for (int i = 1; i <= maxCol; i++)
                        {
                            string h = ws.Cell(r, i).GetString();
                            if (string.IsNullOrEmpty(h)) continue;
                            if (FindColContains(h, new[] { "No.", "No", "번호" })) colNo = i - 1;
                            if (FindColContains(h, new[] { "어드레스", "Address" })) colAddr = i - 1;
                            if (FindColContains(h, new[] { "BIT", "Bit" })) colBit = i - 1;
                            if (FindColContains(h, new[] { "신호명칭", "Signal" })) colSig = i - 1;
                            if (FindColContains(h, new[] { "논리", "Logic" })) colLogic = i - 1;
                            if (FindColContains(h, new[] { "I.O 체크", "I.O체크", "체크" })) colIoCheck = i - 1;
                            if (FindColContains(h, new[] { "비고", "Remarks" })) colRemarks = i - 1;
                        }
                        if (colAddr < 0) colAddr = c - 1;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>해당 행에 DEVICE LIST 정적 헤더(PDM No, DESCRIPTION 등)가 있으면 true. I.O 채울 때 이 행 덮어쓰지 않기 위해 사용.</summary>
        private static bool RowContainsDeviceListHeader(IXLWorksheet ws, int row, int maxCol)
        {
            if (row < 1) return false;
            for (int i = 1; i <= maxCol; i++)
            {
                string h = ws.Cell(row, i).GetString();
                if (string.IsNullOrEmpty(h)) continue;
                if (FindColContains(h, new[] { "PDM No", "PDM No.", "PDM번호", "PDM NO" })) return true;
                if (FindColContains(h, new[] { "DESCRIPTION", "설명", "품목", "품목명", "Description", "품명" })) return true;
            }
            return false;
        }

        private static bool FindColContains(string header, string[] names)
        {
            if (string.IsNullOrEmpty(header)) return false;
            foreach (var n in names)
                if (header.IndexOf(n.Trim(), StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private static int CountTemplateDataRowsClosedXML(IXLWorksheet ws, int dataStartRow, int colAddr, int colSig)
        {
            int count = 0;
            for (int r = dataStartRow; r <= dataStartRow + 500; r++)
            {
                string a = colAddr >= 0 ? ws.Cell(r, colAddr + 1).GetString() : "";
                string s = colSig >= 0 ? ws.Cell(r, colSig + 1).GetString() : "";
                if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(s)) break;
                count++;
            }
            return count > 0 ? count : 20;
        }

        private static bool FindIoTableHeader(ISheet sheet, out int headerRowIndex, out int colNo, out int colAddr, out int colBit, out int colSig, out int colLogic, out int colIoCheck, out int colRemarks)
        {
            colNo = colAddr = colBit = colSig = colLogic = colIoCheck = colRemarks = -1;
            headerRowIndex = 0;
            for (int r = 0; r <= Math.Min(60, sheet.LastRowNum); r++)
            {
                IRow row = sheet.GetRow(r);
                if (row == null) continue;
                for (int c = 0; c < row.LastCellNum; c++)
                {
                    string s = GetCellString(row.GetCell(c));
                    if (string.IsNullOrEmpty(s)) continue;
                    if (s.IndexOf("어드레스", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        headerRowIndex = r;
                        string[] headers = new string[row.LastCellNum];
                        for (int i = 0; i < row.LastCellNum; i++)
                            headers[i] = GetCellString(row.GetCell(i));
                        colNo = FindColumnIndexContains(headers, new[] { "No.", "No", "번호" });
                        colAddr = FindColumnIndexContains(headers, new[] { "어드레스", "Address" });
                        colBit = FindColumnIndexContains(headers, new[] { "BIT", "Bit" });
                        colSig = FindColumnIndexContains(headers, new[] { "신호명칭", "Signal" });
                        colLogic = FindColumnIndexContains(headers, new[] { "논리", "Logic" });
                        colIoCheck = FindColumnIndexContains(headers, new[] { "I.O 체크", "I.O체크", "체크" });
                        colRemarks = FindColumnIndexContains(headers, new[] { "비고", "Remarks" });
                        if (colAddr < 0) colAddr = c;
                        return true;
                    }
                }
            }
            return false;
        }

        private static int CountTemplateDataRows(ISheet sheet, int dataStartRow, int colAddr, int colSig)
        {
            int count = 0;
            for (int r = dataStartRow; r <= Math.Min(dataStartRow + 500, sheet.LastRowNum); r++)
            {
                IRow row = sheet.GetRow(r);
                if (row == null) break;
                string a = GetCellString(colAddr >= 0 ? row.GetCell(colAddr) : null);
                string s = GetCellString(colSig >= 0 ? row.GetCell(colSig) : null);
                if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(s)) break;
                count++;
            }
            return count > 0 ? count : 20;
        }

        private static void CopyRowStyle(IWorkbook workbook, IRow sourceRow, IRow targetRow, int[] columnIndexes)
        {
            if (sourceRow == null) return;
            foreach (int c in columnIndexes)
            {
                if (c < 0) continue;
                ICell srcCell = sourceRow.GetCell(c);
                ICell tgtCell = GetOrCreateCell(targetRow, c);
                if (srcCell != null && srcCell.CellStyle != null)
                    tgtCell.CellStyle = srcCell.CellStyle;
            }
        }

        private static ICell GetOrCreateCell(IRow row, int columnIndex)
        {
            ICell cell = row.GetCell(columnIndex);
            if (cell == null) cell = row.CreateCell(columnIndex);
            return cell;
        }

        /// <summary>지정된 행 구간의 수식 셀을 캐시된 값으로 치환. 저장 시 "cell reference after sheet name" 등 수식 파싱 오류 방지.</summary>
        private static void ClearFormulaCellsInRowRange(ISheet sheet, int rowStart, int rowEnd)
        {
            if (sheet == null || rowEnd < rowStart) return;
            for (int r = rowStart; r <= rowEnd; r++)
            {
                IRow row = sheet.GetRow(r);
                if (row == null) continue;
                for (int c = 0; c < row.LastCellNum; c++)
                {
                    ICell cell = row.GetCell(c);
                    if (cell == null || cell.CellType != CellType.Formula) continue;
                    try
                    {
                        switch (cell.CachedFormulaResultType)
                        {
                            case CellType.Numeric:
                                cell.SetCellValue(cell.NumericCellValue);
                                cell.SetCellType(CellType.Numeric);
                                break;
                            case CellType.String:
                                cell.SetCellValue(cell.StringCellValue ?? "");
                                cell.SetCellType(CellType.String);
                                break;
                            case CellType.Boolean:
                                cell.SetCellValue(cell.BooleanCellValue);
                                cell.SetCellType(CellType.Boolean);
                                break;
                            default:
                                cell.SetCellValue("");
                                cell.SetCellType(CellType.String);
                                break;
                        }
                    }
                    catch { cell.SetCellValue(""); cell.SetCellType(CellType.String); }
                }
            }
        }

        /// <summary>지정된 행 구간과 겹치는 시트의 병합 영역을 제거. ShiftRows 후 겹침 오류 방지용. 인덱스 시프트 방지를 위해 역순 제거.</summary>
        private static void RemoveMergedRegionsInRowRange(ISheet sheet, int rowStart, int rowEnd)
        {
            if (sheet == null || rowEnd < rowStart) return;
            for (int i = sheet.NumMergedRegions - 1; i >= 0; i--)
            {
                CellRangeAddress merged = sheet.GetMergedRegion(i);
                if (merged == null) continue;
                if (merged.LastRow < rowStart || merged.FirstRow > rowEnd) continue;
                sheet.RemoveMergedRegion(i);
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

        private void dataGridViewIO_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        /// <summary>[상태저장] 클릭: (1) DEVICE LIST 확인 체크 전부 완료 (2) I.O LIST 사용신호 필터링 시 표시된 행의 I.O 체크 전부 완료 → 둘 다 만족 시에만 저장.</summary>
        private void IOCheckSheetForm_StateSaveClick(object sender, EventArgs e)
        {
            // 1) DEVICE LIST: 확인 열 체크박스가 모두 체크되어 있어야 함 (분류 행 [본체], [SENSOR PART] 등은 체크 대상 제외)
            if (listViewDeviceList != null)
            {
                foreach (ListViewItem item in listViewDeviceList.Items)
                {
                    if (item.Tag as string == "Category") continue;
                    if (!item.Checked)
                    {
                        MessageBox.Show("DEVICE LIST 탭에서 정의된 모든 항목의 [확인] 체크박스를 체크한 뒤 [상태저장] 해 주세요.", "DEVICE LIST 미완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }

            // 2) I.O LIST: CSV에 "사용자"로 표기된 행만 체크되어 있어야 함. CSV 미로드 시 기존처럼 사용신호 필터링 켜진 경우 신호명칭 있는 행 전부 체크 필요.
            var dt = dataGridViewIO?.DataSource as DataTable;
            if (dt != null && dt.Columns.Contains("I.O 체크"))
            {
                int colIoCheck = dt.Columns.IndexOf("I.O 체크");
                int colIo = dt.Columns.IndexOf("IO구분");
                int colAddr = dt.Columns.IndexOf("어드레스");
                int colBit = dt.Columns.IndexOf("BIT");
                int colSig = dt.Columns.IndexOf("신호명칭");
                if (colIoCheck < 0) { }
                else if (_userCheckableIoKeys.Count > 0 && colIo >= 0 && colAddr >= 0 && colBit >= 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string io = row[colIo]?.ToString()?.Trim() ?? "";
                        string addr = row[colAddr]?.ToString()?.Trim() ?? "";
                        string bit = row[colBit]?.ToString()?.Trim() ?? "";
                        string key = $"{io}_{addr}_{bit}";
                        if (!_userCheckableIoKeys.Contains(key)) continue;
                        object v = row[colIoCheck];
                        bool chk = v is bool b && b || (v != null && (v is DBNull ? false : Convert.ToBoolean(v)));
                        if (!chk)
                        {
                            MessageBox.Show("I.O LIST에서 사용자가 체크하는 항목(구분=사용자)을 모두 체크한 뒤 [상태저장] 해 주세요.", "I.O LIST 미완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                }
                else
                {
                    bool usedOnly = checkBoxUsedSignalFilter?.Checked ?? false;
                    if (usedOnly && colSig >= 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            string sig = row[colSig]?.ToString()?.Trim() ?? "";
                            if (string.IsNullOrEmpty(sig)) continue;
                            object v = row[colIoCheck];
                            bool chk = v is bool b && b || (v != null && (v is DBNull ? false : Convert.ToBoolean(v)));
                            if (!chk)
                            {
                                MessageBox.Show("I.O LIST에서 신호명칭이 있는 모든 항목에 I.O 체크를 한 뒤 [상태저장] 해 주세요.", "I.O LIST 미완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }
                    }
                }
            }

            IoCheckCompleted = true;
            MessageBox.Show("I.O 체크 상태가 저장되었습니다. 이제 메인 화면에서 반자동 명령·자동모드를 사용할 수 있습니다.", "상태저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void textBoxTesterName_Click(object sender, EventArgs e)
        {
            ShowVirtualKeyboard();
        }

        private void textBoxTesterName_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonLoadExcel_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Excel 파일 (*.xls; *.xlsx)|*.xls;*.xlsx|모든 파일 (*.*)|*.*";
                dlg.Title = "DEVICE LIST 엑셀 선택";
                dlg.DefaultExt = "xlsx";
                if (dlg.ShowDialog() != DialogResult.OK) return;
                LoadDeviceListFromFile(dlg.FileName);
            }
        }

        private void listViewDeviceList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void listViewDeviceList_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data?.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;
            var path = files[0];
            if (string.IsNullOrEmpty(path)) return;
            var ext = Path.GetExtension(path ?? "");
            if (!ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) && !ext.Equals(".xls", StringComparison.OrdinalIgnoreCase))
                return;
            LoadDeviceListFromFile(path);
        }

        /// <summary>엑셀(.xls, .xlsx) 파일을 NPOI로 그리드에 로드. 별도 드라이버 설치 불필요.</summary>
        private void LoadDeviceListFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("파일을 찾을 수 없습니다.", "파일 로드", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var ext = Path.GetExtension(filePath ?? "").ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
            {
                MessageBox.Show("엑셀 파일(.xls, .xlsx)만 지원합니다.", "파일 로드", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var book = WorkbookFactory.Create(fs))
                {
                    var sheet = book.GetSheetAt(0);
                    if (sheet == null)
                    {
                        MessageBox.Show("시트가 없습니다.", "엑셀 로드", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    int headerRowIndex;
                    var headerArr = GetExcelHeaderRow(sheet, out headerRowIndex);
                    if (headerArr == null || headerArr.Length == 0)
                    {
                        MessageBox.Show("헤더 행을 찾을 수 없습니다. 첫 시트 상단에 PDM/품명/규격 등 열 이름이 있어야 합니다.", "엑셀 로드", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    int idxPdm = FindColumnIndex(headerArr, new[] { "PDM No", "PDM No.", "PDM번호", "PDM NO", "PDM", "NO" });
                    if (idxPdm < 0) idxPdm = FindColumnIndexContains(headerArr, new[] { "PDM", "번호" });
                    int idxDesc = FindColumnIndex(headerArr, new[] { "DESCRIPTION", "설명", "품목", "품목명", "Description", "품명" });
                    if (idxDesc < 0) idxDesc = FindColumnIndexContains(headerArr, new[] { "DESCRIPTION", "품명", "설명", "품목" });
                    int idxSpec = FindColumnIndex(headerArr, new[] { "SPECIFICATION", "규격", "사양", "Spec", "Specification", "세부품명", "세부사양" });
                    if (idxSpec < 0) idxSpec = FindColumnIndexContains(headerArr, new[] { "SPECIFICATION", "규격", "사양", "세부" });
                    int idxManufacture = FindColumnIndex(headerArr, new[] { "MANUFACTURE", "Manufacture", "제조사", "제조사명", "MAKER" });
                    if (idxManufacture < 0) idxManufacture = FindColumnIndexContains(headerArr, new[] { "MANUFACTURE", "제조사", "MAKER" });
                    if (idxPdm < 0 || idxDesc < 0 || idxSpec < 0)
                    {
                        MessageBox.Show("필수 열을 찾을 수 없습니다. (PDM No, DESCRIPTION, SPECIFICATION 또는 규격/사양 등)\r\n\r\n엑셀 첫 시트 상단에 위 열 이름이 있는지 확인해 주세요.", "엑셀 로드", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    listViewDeviceList.BeginUpdate();
                    try
                    {
                        listViewDeviceList.Items.Clear();
                        for (int r = headerRowIndex + 1; r <= sheet.LastRowNum; r++)
                        {
                            var row = sheet.GetRow(r);
                            if (row == null) continue;
                            string pdm = GetCellString(row.GetCell(idxPdm));
                            string desc = GetCellString(row.GetCell(idxDesc));
                            string spec = SanitizeSpec(GetCellString(row.GetCell(idxSpec)));
                            string manufacture = idxManufacture >= 0 ? GetManufactureOnly(GetCellString(row.GetCell(idxManufacture))) : "";
                            manufacture = SanitizeManufacture(manufacture);
                            if (string.IsNullOrWhiteSpace(pdm) && string.IsNullOrWhiteSpace(desc) && string.IsNullOrWhiteSpace(spec))
                                continue;
                            var item = new ListViewItem(pdm);
                            item.SubItems.Add(desc);
                            item.SubItems.Add(spec);
                            item.SubItems.Add(manufacture);
                            item.SubItems.Add("");
                            item.Checked = false;
                            if (IsDeviceListCategoryRow(desc)) item.Tag = "Category";
                            listViewDeviceList.Items.Add(item);
                        }
                    }
                    finally
                    {
                        listViewDeviceList.EndUpdate();
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message ?? "";
                if (msg.IndexOf("ACE", StringComparison.OrdinalIgnoreCase) >= 0 || msg.IndexOf("OLEDB", StringComparison.OrdinalIgnoreCase) >= 0)
                    msg = "이 오류는 수정된 버전이 아닌 이전 실행 파일이 동작 중일 때 나옵니다.\r\n\r\n[할 일]\r\n1. Visual Studio에서 솔루션 우클릭 → NuGet 패키지 복원\r\n2. 메뉴에서 빌드 → 솔루션 다시 빌드\r\n3. 빌드 후 다시 실행한 뒤 파일 불러오기\r\n\r\n원래 오류: " + msg;
                MessageBox.Show("파일을 읽는 중 오류가 발생했습니다.\r\n" + msg, "파일 로드", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string GetCellString(ICell cell)
        {
            if (cell == null) return "";
            try
            {
                switch (cell.CellType)
                {
                    case CellType.String: return (cell.StringCellValue ?? "").Trim();
                    case CellType.Numeric:
                        if (DateUtil.IsCellDateFormatted(cell))
                            return string.Format("{0:yyyy-MM-dd}", cell.DateCellValue);
                        var n = cell.NumericCellValue;
                        return (n == (long)n) ? ((long)n).ToString() : n.ToString();
                    case CellType.Boolean: return cell.BooleanCellValue ? "1" : "0";
                    case CellType.Formula:
                        try { return (cell.ToString() ?? "").Trim(); }
                        catch { return ""; }
                    default: return (cell.ToString() ?? "").Trim();
                }
            }
            catch { return ""; }
        }

        /// <summary>PDF에서 X좌표 기반으로 PDM No / DESCRIPTION / SPECIFICATION / TOTAL 열을 인식해, 조건 맞는 행만 그리드에 로드.</summary>
        private void LoadDeviceListFromPdf(string filePath)
        {
            try
            {
                var allRows = new List<string[]>();
                using (var document = PdfDocument.Open(filePath))
                {
                    foreach (var page in document.GetPages())
                    {
                        List<List<PdfWord>> lines = null;
                        var words = page.GetWords()?.ToList() ?? new List<Word>();
                        if (words.Count > 0)
                        {
                            try
                            {
                                const double lineTolerance = 3.0;
                                lines = words
                                    .GroupBy(w => Math.Round(w.BoundingBox.Bottom / lineTolerance) * lineTolerance)
                                    .OrderByDescending(g => g.Key)
                                    .Select(g => g.OrderBy(w => w.BoundingBox.Left)
                                        .Where(w => !string.IsNullOrWhiteSpace(GetWordText(w)))
                                        .Select(w => new PdfWord { Text = GetWordText(w), Left = w.BoundingBox.Left, Right = w.BoundingBox.Right })
                                        .ToList())
                                    .Where(list => list.Count > 0).ToList();
                            }
                            catch { lines = null; }
                        }
                        if (lines == null || lines.Count == 0)
                            lines = BuildLinesFromLetters(page);
                        if (lines == null || lines.Count == 0) continue;
                        double[] colBounds = DetectPdfColumnBounds(lines);
                        double xSpecStart = DetectSpecColumnStart(lines, colBounds);
                        foreach (var line in lines)
                        {
                            var cells = GetCellsByDescriptionSpecSplit(line, colBounds, xSpecStart);
                            if (!string.IsNullOrWhiteSpace(cells[0]))
                            {
                                allRows.Add(new[] { cells[0], cells[1], cells[2], cells[3] });
                                continue;
                            }
                            cells = GetCellsByColumnBounds(line, colBounds);
                            if (cells.Count >= 4)
                                allRows.Add(new[] { cells[0], cells[1], cells[2], cells[3] });
                            else if (cells.Count == 3)
                                allRows.Add(new[] { cells[0], cells[1], cells[2], "" });
                            else if (cells.Count == 2)
                                allRows.Add(new[] { cells[0], cells[1], "", "" });
                            else if (cells.Count == 1 && !string.IsNullOrWhiteSpace(cells[0]))
                            {
                                var raw = cells[0].Trim();
                                var parts = Regex.Split(raw, @"\s{2,}").Select(p => p.Trim()).Where(p => p.Length > 0).ToArray();
                                if (parts.Length >= 3 && Regex.IsMatch(parts[0], @"^\d{5,12}$"))
                                    allRows.Add(new[] { parts[0], parts[1], string.Join(" ", parts.Skip(2)), "" });
                                else if (parts.Length == 2 && Regex.IsMatch(parts[0], @"^\d{5,12}$"))
                                    allRows.Add(new[] { parts[0], parts[1], "", "" });
                            }
                            else if (cells.Count == 0 && line.Count >= 2)
                            {
                                var w0 = (line[0].Text ?? "").Trim();
                                if (Regex.IsMatch(w0, @"^\d{5,12}$") && !w0.StartsWith("E1339", StringComparison.OrdinalIgnoreCase))
                                {
                                    var w1 = line.Count > 1 ? (line[1].Text ?? "").Trim() : "";
                                    var rest = line.Count > 2 ? string.Join(" ", line.Skip(2).Select(w => (w.Text ?? "").Trim())).Trim() : "";
                                    allRows.Add(new[] { w0, w1, rest, "" });
                                }
                            }
                        }
                    }
                }
                NormalizeMergedFirstColumn(allRows);
                for (int i = 0; i < allRows.Count; i++)
                {
                    var row = allRows[i];
                    if (row != null && row.Length >= 3)
                        MoveLeadingNonSpecFromSpecToDesc(row);
                }
                for (int i = 0; i < allRows.Count; i++)
                {
                    var row = allRows[i];
                    if (row != null && row.Length >= 4)
                        MoveSpecBrandToManufacture(row);
                }
                for (int i = 0; i < allRows.Count; i++)
                {
                    var row = allRows[i];
                    if (row != null && row.Length >= 3)
                        SplitDescriptionAtDatePattern(row);
                }
                for (int i = 0; i < allRows.Count; i++)
                {
                    var row = allRows[i];
                    if (row != null && row.Length >= 4)
                        ExtractManufacturerFromDescription(row);
                }
                var dataRows = allRows
                    .Where(r => !IsHeaderRow(r))
                    .Where(r => ShouldAddDeviceRow(r[0], r.Length > 1 ? r[1] : "", r.Length > 2 ? r[2] : "", r.Length > 3 ? r[3] : ""))
                    .ToList();
                if (dataRows.Count == 0 && allRows.Count > 0)
                {
                    dataRows = allRows.Where(r => !IsHeaderRow(r)).Where(r =>
                    {
                        var p = (r[0] ?? "").Trim();
                        var d = (r.Length > 1 ? r[1] : "").Trim();
                        var s = (r.Length > 2 ? r[2] : "").Trim();
                        if (string.IsNullOrEmpty(p)) return false;
                        if (!Regex.IsMatch(p, @"^\d{5,12}$")) return false;
                        if (p.StartsWith("[") || p.StartsWith("공사명") || p.StartsWith("기존교체") || p.StartsWith("E1339", StringComparison.OrdinalIgnoreCase) || p.StartsWith("송사명")) return false;
                        return !string.IsNullOrEmpty(d) || !string.IsNullOrEmpty(s);
                    }).ToList();
                }
                listViewDeviceList.BeginUpdate();
                try
                {
                    listViewDeviceList.Items.Clear();
                    foreach (var row in dataRows)
                    {
                        var pdm = (row[0] ?? "").Trim();
                        var desc = RemoveDuplicateFirstWord((row.Length > 1 ? row[1] : "").Trim());
                        desc = RemoveSpecSuffixFromDescription(desc);
                        var specCell = (row.Length > 2 ? row[2] : "").Trim();
                        var spec = SanitizeSpec(GetSpecOnly(specCell));
                        var manufacture = GetManufactureOnly(row.Length > 3 ? row[3] : "");
                        if (string.IsNullOrEmpty(manufacture)) manufacture = GetManufactureFromSpecString(specCell);
                        manufacture = SanitizeManufacture(manufacture);
                        var item = new ListViewItem(pdm);
                        item.SubItems.Add(desc);
                        item.SubItems.Add(spec);
                        item.SubItems.Add(manufacture);
                        item.SubItems.Add("");
                        item.Checked = false;
                        if (IsDeviceListCategoryRow(desc)) item.Tag = "Category";
                        listViewDeviceList.Items.Add(item);
                    }
                }
                finally
                {
                    listViewDeviceList.EndUpdate();
                }
                if (dataRows.Count == 0)
                {
                    MessageBox.Show(
                        "PDF에서 추출된 데이터가 없습니다.\r\n\r\n" +
                        "• 텍스트가 선택 가능한 PDF인지 확인해 주세요.\r\n" +
                        "• 스캔본(이미지) PDF는 인식되지 않습니다.\r\n" +
                        "• PDM No / 품명 / 세부품명이 있는 표가 포함된 PDF를 사용해 주세요.",
                        "파일 불러오기",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (TypeLoadException)
            {
                MessageBox.Show("PDF 로드를 위해 NuGet 패키지 복원 후 다시 빌드해 주세요.\r\n(솔루션 우클릭 → NuGet 패키지 복원)", "PDF 로드", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("PDF를 읽는 중 오류가 발생했습니다.\r\n" + ex.Message, "PDF 로드", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string GetWordText(Word w)
        {
            if (w == null) return "";
            try
            {
                var t = w.Text;
                return (t ?? "").Trim();
            }
            catch { return ""; }
        }

        /// <summary>GetWords()가 비어 있을 때 페이지 Letters로 라인(단어 리스트) 구성.</summary>
        private static List<List<PdfWord>> BuildLinesFromLetters(Page page)
        {
            if (page?.Letters == null || page.Letters.Count == 0) return new List<List<PdfWord>>();
            const double lineTolerance = 3.0;
            const double wordGap = 10.0;
            try
            {
                var letters = page.Letters.OrderBy(l => l.GlyphRectangle.Bottom).ThenBy(l => l.GlyphRectangle.Left).ToList();
                var lineGroups = letters
                    .GroupBy(l => Math.Round(l.GlyphRectangle.Bottom / lineTolerance) * lineTolerance)
                    .OrderByDescending(g => g.Key)
                    .Select(g => g.OrderBy(l => l.GlyphRectangle.Left).ToList())
                    .ToList();
                var result = new List<List<PdfWord>>();
                foreach (var lineLetters in lineGroups)
                {
                    var words = new List<PdfWord>();
                    double lastRight = -1000;
                    var wordLetters = new List<Letter>();
                    foreach (var l in lineLetters)
                    {
                        var left = l.GlyphRectangle.Left;
                        if (wordLetters.Count > 0 && left - lastRight > wordGap)
                        {
                            if (wordLetters.Count > 0)
                            {
                                var text = string.Join("", wordLetters.Select(x => x.Value ?? "")).Trim();
                                if (text.Length > 0)
                                    words.Add(new PdfWord { Text = text, Left = wordLetters[0].GlyphRectangle.Left, Right = wordLetters[wordLetters.Count - 1].GlyphRectangle.Right });
                            }
                            wordLetters.Clear();
                        }
                        wordLetters.Add(l);
                        lastRight = l.GlyphRectangle.Right;
                    }
                    if (wordLetters.Count > 0)
                    {
                        var text = string.Join("", wordLetters.Select(x => x.Value ?? "")).Trim();
                        if (text.Length > 0)
                            words.Add(new PdfWord { Text = text, Left = wordLetters[0].GlyphRectangle.Left, Right = wordLetters[wordLetters.Count - 1].GlyphRectangle.Right });
                    }
                    if (words.Count > 0) result.Add(words);
                }
                return result;
            }
            catch { return new List<List<PdfWord>>(); }
        }

        /// <summary>SPECIFICATION 열에 오는 문자열 패턴(제품코드형: 하이픈+숫자 또는 영문+숫자). Q-Lite·Breaker 등 브랜드/품명 단어는 제외.</summary>
        private static bool LooksLikeSpecification(string word)
        {
            if (string.IsNullOrWhiteSpace(word) || word.Length < 4) return false;
            var w = word.Trim();
            if (w.Contains("-") && Regex.IsMatch(w, @"[0-9]")) return true;
            if (Regex.IsMatch(w, @"^[A-Z0-9]{6,}$", RegexOptions.IgnoreCase) && Regex.IsMatch(w, @"[0-9]")) return true;
            if (Regex.IsMatch(w, @"^[A-Z]{2,}[0-9]", RegexOptions.IgnoreCase)) return true;
            return false;
        }

        /// <summary>하이픈 있으나 숫자 없음 → 브랜드/품명형(Q-Lite 등). SPEC에 있으면 MANUFACTURE로 넘길 후보.</summary>
        private static bool LooksLikeBrandNotSpec(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return false;
            var w = word.Trim();
            return w.Length >= 2 && w.Contains("-") && !Regex.IsMatch(w, @"[0-9]");
        }

        /// <summary>PDF 표의 열 경계(X좌표) 감지. PDF에 그려진 선(테이블 경계선)은 라이브러리에서 추출 불가하므로, 텍스트의 Left X좌표로 열 경계를 추정함.</summary>
        private static double[] DetectPdfColumnBounds(List<List<PdfWord>> lines)
        {
            const double gapThreshold = 8.0;
            int headerIdx = -1;
            int dataLineIdx = -1;
            for (int i = 0; i < Math.Min(40, lines.Count); i++)
            {
                if (lines[i].Count == 0) continue;
                var first = (lines[i][0].Text ?? "").Trim();
                if (Regex.IsMatch(first, @"^\d{5,12}$") && dataLineIdx < 0)
                    dataLineIdx = i;
                var text = string.Join(" ", lines[i].Select(x => x.Text)).ToUpperInvariant();
                if ((text.Contains("PDM") || text.Contains("DESCRIPTION") || text.Contains("SPECIFICATION") || text.Contains("TOTAL")) &&
                    (text.Contains("DESCRIPTION") || text.Contains("품명") || text.Contains("SPECIFICATION") || text.Contains("세부") || text.Contains("TOTAL")))
                {
                    headerIdx = i;
                    if (dataLineIdx >= 0) break;
                }
            }
            var bounds = new List<double>();
            // 1) 첫 데이터 행으로 PDM / DESCRIPTION / SPECIFICATION 열 경계 설정
            if (dataLineIdx >= 0 && dataLineIdx < lines.Count)
            {
                var row = lines[dataLineIdx];
                if (row.Count >= 2)
                {
                    bounds.Add(row[0].Left);
                    bounds.Add(row[1].Left);
                    int specIdx = -1;
                    for (int j = 1; j < row.Count; j++)
                    {
                        if (LooksLikeSpecification(row[j].Text ?? ""))
                        {
                            specIdx = j;
                            break;
                        }
                    }
                    if (specIdx >= 1)
                        bounds.Add(row[specIdx].Left);
                    else if (row.Count >= 3)
                        bounds.Add(row[row.Count - 1].Left);
                }
            }
            // 2) 부족하면 헤더 행에서 열 경계 추가
            if (bounds.Count < 3 && headerIdx >= 0 && headerIdx < lines.Count)
            {
                var header = lines[headerIdx];
                double lastRight = -1;
                for (int j = 0; j < header.Count && bounds.Count < 4; j++)
                {
                    double left = header[j].Left;
                    if (j == 0 || left - lastRight > gapThreshold)
                    {
                        if (bounds.Count < 4 && !bounds.Any(b => Math.Abs(b - left) < 5))
                            bounds.Add(left);
                    }
                    lastRight = Math.Max(lastRight, header[j].Right);
                }
            }
            bounds = bounds.OrderBy(x => x).Distinct().ToList();
            double pageWidth = 550;
            if (lines.Count > 0 && lines[0].Count > 0)
                pageWidth = Math.Max(400, lines[0].Max(x => x.Right) + 30);
            if (bounds.Count < 3 && dataLineIdx >= 0 && dataLineIdx < lines.Count && lines[dataLineIdx].Count >= 3)
            {
                var row = lines[dataLineIdx];
                bounds.Clear();
                bounds.Add(row[0].Left);
                bounds.Add(row[1].Left);
                bounds.Add(row[row.Count - 1].Left);
            }
            if (bounds.Count == 2)
            {
                bounds.Add(bounds[1] + Math.Max(40, (pageWidth - bounds[1]) * 0.4));
            }
            if (bounds.Count < 3)
            {
                bounds.Clear();
                bounds.Add(0);
                bounds.Add(pageWidth * 0.12);
                bounds.Add(pageWidth * 0.45);
                bounds.Add(pageWidth * 0.78);
            }
            return bounds.OrderBy(x => x).Distinct().Take(4).ToArray();
        }

        private static List<string> GetCellsByColumnBounds(List<PdfWord> line, double[] colBounds)
        {
            var cells = new List<string>();
            if (colBounds == null || colBounds.Length == 0) return cells;
            for (int c = 0; c < colBounds.Length; c++)
            {
                double xStart = colBounds[c];
                double xEnd = c + 1 < colBounds.Length ? colBounds[c + 1] : xStart + 9999;
                var part = line.Where(w => w.Left >= xStart - 2 && w.Left < xEnd + 2).Select(w => w.Text).ToList();
                cells.Add(string.Join(" ", part).Trim());
            }
            return cells;
        }

        /// <summary>페이지 내 형번(SPECIFICATION) 열의 왼쪽 X 경계. 형번 패턴(숫자 포함/하이픈) 단어들의 Left 수집 후 중앙값 사용해 단일 행 오인식 완화.</summary>
        private static double DetectSpecColumnStart(List<List<PdfWord>> lines, double[] colBounds)
        {
            if (lines == null || lines.Count == 0 || colBounds == null || colBounds.Length < 2) return -1;
            double x1 = colBounds[1];
            var candidateX = new List<double>();
            foreach (var line in lines)
            {
                if (line == null) continue;
                foreach (var w in line)
                {
                    if (w.Left < x1 - 2) continue;
                    var t = (w.Text ?? "").Trim();
                    if (t.Length > 0 && LooksLikeSpecification(t))
                        candidateX.Add(w.Left);
                }
            }
            if (candidateX.Count == 0) return colBounds.Length >= 3 ? colBounds[2] : -1;
            candidateX.Sort();
            double xSpec = candidateX.Count % 2 == 1
                ? candidateX[candidateX.Count / 2]
                : (candidateX[candidateX.Count / 2 - 1] + candidateX[candidateX.Count / 2]) / 2.0;
            return xSpec;
        }

        /// <summary>SPEC 셀 앞에 붙은 비형번 단어(Breaker, Protector 등)를 DESCRIPTION으로 넘김. SPEC이 비면 row[3]을 SPEC으로 올림.</summary>
        private static void MoveLeadingNonSpecFromSpecToDesc(string[] row)
        {
            if (row == null || row.Length < 3) return;
            var specCell = (row[2] ?? "").Trim();
            if (string.IsNullOrEmpty(specCell))
            {
                if (row.Length > 3 && !string.IsNullOrWhiteSpace(row[3]))
                {
                    row[2] = (row[3] ?? "").Trim();
                    row[3] = "";
                }
                return;
            }
            var parts = specCell.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int moveCount = 0;
            foreach (var p in parts)
            {
                if (string.IsNullOrEmpty(p) || LooksLikeSpecification(p)) break;
                moveCount++;
            }
            if (moveCount == 0) return;
            var toDesc = string.Join(" ", parts.Take(moveCount));
            var newSpec = string.Join(" ", parts.Skip(moveCount));
            row[1] = ((row.Length > 1 ? row[1] : "").Trim() + " " + toDesc).Trim();
            row[2] = newSpec;
            if (string.IsNullOrWhiteSpace(row[2]) && row.Length > 3 && !string.IsNullOrWhiteSpace(row[3]))
            {
                row[2] = (row[3] ?? "").Trim();
                row[3] = "";
            }
        }

        /// <summary>SPEC이 브랜드형(Q-Lite 등)이고 MANUFACTURE가 숫자만 있으면 SPEC을 MANUFACTURE로 넘기고 SPEC 비움.</summary>
        private static void MoveSpecBrandToManufacture(string[] row)
        {
            if (row == null || row.Length < 4) return;
            var spec = (row[2] ?? "").Trim();
            var manu = (row[3] ?? "").Trim();
            if (string.IsNullOrEmpty(spec) || string.IsNullOrEmpty(manu)) return;
            var specParts = spec.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (specParts.Length != 1) return;
            if (!LooksLikeBrandNotSpec(specParts[0])) return;
            if (!Regex.IsMatch(manu, @"^\d+$")) return;
            row[3] = specParts[0];
            row[2] = "";
        }

        /// <summary>DESCRIPTION이 길고 날짜 패턴(nn.nn.nn)을 포함하면, 그 이전까지를 품명으로 자르고 나머지는 SPEC으로.</summary>
        private static void SplitDescriptionAtDatePattern(string[] row)
        {
            if (row == null || row.Length < 3) return;
            var desc = (row[1] ?? "").Trim();
            var spec = (row[2] ?? "").Trim();
            if (string.IsNullOrEmpty(desc) || desc.Length < 20) return;
            var dateMatch = Regex.Match(desc, @"(\d{2}\.\d{2}\.\d{2})");
            if (!dateMatch.Success) return;
            int idx = dateMatch.Index;
            var beforeDate = desc.Substring(0, idx).Trim();
            var fromDate = desc.Substring(idx).Trim();
            if (string.IsNullOrEmpty(beforeDate)) return;
            row[1] = beforeDate;
            if (string.IsNullOrEmpty(spec))
                row[2] = fromDate;
            else
                row[2] = (fromDate + " " + spec).Trim();
        }

        /// <summary>DESCRIPTION 끝의 제조사명(한성제어기, 창성제어 등)·날짜·수량을 MANUFACTURE로 옮기고 DESCRIPTION에서 제거.</summary>
        private static void ExtractManufacturerFromDescription(string[] row)
        {
            if (row == null || row.Length < 4) return;
            var manu = (row[3] ?? "").Trim();
            if (!string.IsNullOrEmpty(manu) && !Regex.IsMatch(manu, @"^\d+$") && !Regex.IsMatch(manu, @"^\d{2}\.\d{2}\.\d{2}$"))
                return;
            var desc = (row[1] ?? "").Trim();
            if (string.IsNullOrEmpty(desc)) return;
            var words = desc.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (words.Count == 0) return;
            int i = words.Count - 1;
            while (i >= 0 && (Regex.IsMatch(words[i], @"^\d+$") || Regex.IsMatch(words[i], @"^\d{2}\.\d{2}\.\d{2}$")))
                i--;
            string extractedManu = null;
            int keepCount = i + 1;
            if (i >= 0 && (words[i].EndsWith("제어기", StringComparison.Ordinal) || words[i].EndsWith("제어", StringComparison.Ordinal)))
            {
                extractedManu = words[i];
                keepCount = i;
            }
            if (keepCount >= words.Count && extractedManu == null) return;
            row[1] = keepCount > 0 ? string.Join(" ", words.Take(keepCount)).Trim() : "";
            if (extractedManu != null)
            {
                row[3] = extractedManu;
                row[1] = RemoveTrailingNumbersFromDescription(row[1]);
            }
        }

        /// <summary>DESCRIPTION 끝의 수량(순수 숫자 토큰) 제거.</summary>
        private static string RemoveTrailingNumbersFromDescription(string desc)
        {
            if (string.IsNullOrWhiteSpace(desc)) return desc ?? "";
            var words = desc.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            while (words.Count > 0 && Regex.IsMatch(words[words.Count - 1], @"^\d+$"))
                words.RemoveAt(words.Count - 1);
            return string.Join(" ", words).Trim();
        }

        /// <summary>형번 열 시작 X(xSpecStart)로 DESCRIPTION/SPEC 구분. 띄어쓰기 많아도 X 왼쪽은 전부 품명.</summary>
        private static List<string> GetCellsByDescriptionSpecSplit(List<PdfWord> line, double[] colBounds, double xSpecStart)
        {
            var result = new List<string>(4) { "", "", "", "" };
            if (line == null || line.Count == 0) return result;
            var byLeft = line.OrderBy(w => w.Left).ToList();
            List<PdfWord> rest;
            if (colBounds != null && colBounds.Length >= 2)
            {
                double x0 = colBounds[0], x1 = colBounds[1];
                var pdmWords = line.Where(w => w.Left >= x0 - 2 && w.Left < x1 + 2).Select(w => (w.Text ?? "").Trim()).Where(s => s.Length > 0).ToList();
                result[0] = string.Join(" ", pdmWords).Trim();
                rest = line.Where(w => w.Left >= x1 - 2).OrderBy(w => w.Left).ToList();
                if (string.IsNullOrEmpty(result[0]) && byLeft.Count > 0)
                {
                    var firstText = (byLeft[0].Text ?? "").Trim();
                    if (Regex.IsMatch(firstText, @"^\d{5,12}$") && !firstText.StartsWith("E1339", StringComparison.OrdinalIgnoreCase))
                    {
                        result[0] = firstText;
                        rest = byLeft.Skip(1).ToList();
                    }
                }
            }
            else
            {
                if (byLeft.Count == 0) return result;
                var firstText = (byLeft[0].Text ?? "").Trim();
                if (Regex.IsMatch(firstText, @"^\d{5,12}$") && !firstText.StartsWith("E1339", StringComparison.OrdinalIgnoreCase))
                {
                    result[0] = firstText;
                    rest = byLeft.Skip(1).ToList();
                }
                else
                    rest = byLeft;
            }
            if (rest.Count == 0) return result;
            if (xSpecStart > 0)
            {
                var descWords = rest.Where(w => w.Left < xSpecStart - 2).Select(w => (w.Text ?? "").Trim()).Where(s => s.Length > 0);
                var specWords = rest.Where(w => w.Left >= xSpecStart - 2).Select(w => (w.Text ?? "").Trim()).Where(s => s.Length > 0);
                result[1] = string.Join(" ", descWords).Trim();
                result[2] = string.Join(" ", specWords).Trim();
                return result;
            }
            int specIdx = -1;
            for (int i = 0; i < rest.Count; i++)
            {
                var t = (rest[i].Text ?? "").Trim();
                if (t.Length > 0 && LooksLikeSpecification(t)) { specIdx = i; break; }
            }
            if (specIdx < 0)
            {
                result[1] = string.Join(" ", rest.Select(w => (w.Text ?? "").Trim())).Trim();
                return result;
            }
            var descWordsFallback = rest.Take(specIdx).Select(w => (w.Text ?? "").Trim()).Where(s => s.Length > 0);
            var specWordsFallback = rest.Skip(specIdx).Select(w => (w.Text ?? "").Trim()).Where(s => s.Length > 0);
            result[1] = string.Join(" ", descWordsFallback).Trim();
            result[2] = string.Join(" ", specWordsFallback).Trim();
            return result;
        }

        /// <summary>수량·날짜·금액 제외, 제조사명만 반환. 예: "HELCO 1 3 상성세어 22.08.30 589" → "HELCO"</summary>
        private static string GetManufactureOnly(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            var first = raw.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return first ?? "";
        }

        /// <summary>MANUFACTURE 칼럼에 어울리지 않는 값(숫자만, 날짜 패턴, 수량)이면 빈 문자열로 지움.</summary>
        private static string SanitizeManufacture(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            var v = value.Trim();
            if (Regex.IsMatch(v, @"^\d+$")) return "";
            if (Regex.IsMatch(v, @"^\d{2}\.\d{2}\.\d{2}$")) return "";
            return v;
        }

        /// <summary>SPECIFICATION 칼럼에 어울리지 않는 값(날짜만, 순수 숫자만)이면 빈 문자열로 지움.</summary>
        private static string SanitizeSpec(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            var v = value.Trim();
            if (Regex.IsMatch(v, @"^\d{2}\.\d{2}\.\d{2}$")) return "";
            if (Regex.IsMatch(v, @"^\d+$")) return "";
            return v;
        }

        /// <summary>SPECIFICATION 열용: 제품코드(첫 번째 spec형 토큰)만 반환. 예: "MVL-MPJ-CPU-204 HELCO 1 3 창성제어..." → "MVL-MPJ-CPU-204"</summary>
        private static string GetSpecOnly(string specString)
        {
            if (string.IsNullOrWhiteSpace(specString)) return "";
            var parts = specString.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
                if (LooksLikeSpecification(p)) return p;
            return parts.Length > 0 ? parts[0] : "";
        }

        /// <summary>SPEC 셀에 제품코드+제조사+나머지가 붙어 있을 때, 제조사(제품코드 다음 한 단어)만 반환. 수량·날짜 등은 표시 안 함.</summary>
        private static string GetManufactureFromSpecString(string specString)
        {
            if (string.IsNullOrWhiteSpace(specString)) return "";
            var parts = specString.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
                if (LooksLikeSpecification(parts[i]) && i + 1 < parts.Length)
                    return parts[i + 1];
            return "";
        }

        /// <summary>Description 끝에 붙은 세부품명(제품코드형)을 제거. 예: "PC BOARD MVL-MJ-CPU 204" → "PC BOARD"</summary>
        private static string RemoveSpecSuffixFromDescription(string desc)
        {
            if (string.IsNullOrWhiteSpace(desc)) return desc ?? "";
            var words = desc.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return desc.Trim();
            while (words.Length > 1)
            {
                var last = words[words.Length - 1];
                if (LooksLikeSpecification(last)) { words = words.Take(words.Length - 1).ToArray(); continue; }
                if (Regex.IsMatch(last, @"^\d{2,}$")) { words = words.Take(words.Length - 1).ToArray(); continue; }
                break;
            }
            return string.Join(" ", words).Trim();
        }

        /// <summary>앞에 같은 단어가 두 번 나오면 한 번만 남김. 예: "PC PC BOARD" → "PC BOARD"</summary>
        private static string RemoveDuplicateFirstWord(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s ?? "";
            var parts = s.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && string.Equals(parts[0], parts[1], StringComparison.Ordinal))
                return string.Join(" ", parts.Skip(1));
            return s.Trim();
        }

        /// <summary>첫 셀에 "727002021 PC BOARD"처럼 PDM+품명이 합쳐진 경우 PDM만 떼어 내고 품명에 합침.</summary>
        private static void NormalizeMergedFirstColumn(List<string[]> allRows)
        {
            for (int i = 0; i < allRows.Count; i++)
            {
                var row = allRows[i];
                if (row == null || row.Length < 2) continue;
                var first = (row[0] ?? "").Trim();
                var m = Regex.Match(first, @"^(\d{5,12})\s+(.*)$");
                if (m.Success && !first.StartsWith("E1339", StringComparison.OrdinalIgnoreCase))
                {
                    var pdm = m.Groups[1].Value;
                    var mergedDesc = (m.Groups[2].Value + " " + (row.Length > 1 ? row[1] : "")).Trim();
                    mergedDesc = RemoveDuplicateFirstWord(mergedDesc);
                    mergedDesc = RemoveSpecSuffixFromDescription(mergedDesc);
                    var spec = row.Length > 2 ? (row[2] ?? "").Trim() : "";
                    var total = row.Length > 3 ? (row[3] ?? "").Trim() : "";
                    allRows[i] = new[] { pdm, mergedDesc, spec, total };
                }
            }
        }

        private static bool IsHeaderRow(string[] row)
        {
            var first = (row[0] ?? "").Trim().ToUpperInvariant();
            if (first.Contains("PDM") && (row.Length < 2 || (row[1] ?? "").ToUpperInvariant().Contains("DESCRIPTION"))) return true;
            if (first.Contains("DESCRIPTION") || first.Contains("SPECIFICATION") || first.Contains("TOTAL")) return true;
            if (first.Contains("품명") || first.Contains("세부품명") || first.Contains("수량")) return true;
            return false;
        }

        private static bool ShouldAddDeviceRow(string pdm, string desc, string spec, string total)
        {
            pdm = (pdm ?? "").Trim();
            desc = (desc ?? "").Trim();
            spec = (spec ?? "").Trim();
            total = (total ?? "").Trim();
            if (string.IsNullOrWhiteSpace(pdm))
                return false;
            if (string.IsNullOrWhiteSpace(desc) && string.IsNullOrWhiteSpace(spec))
                return false;
            if (pdm.StartsWith("[") || pdm.StartsWith("공사명") || pdm.StartsWith("기존교체") || pdm.StartsWith("송사명"))
                return false;
            if (pdm.StartsWith("E1339", StringComparison.OrdinalIgnoreCase))
                return false;
            if (!Regex.IsMatch(pdm, @"^\d{5,12}$"))
                return false;
            return true;
        }

        private sealed class PdfWord
        {
            public string Text;
            public double Left;
            public double Right;
        }

        /// <summary>엑셀 첫 시트에서 헤더 행 찾기(최대 15행 스캔). PDM/품명/규격 등이 포함된 행을 헤더로 사용.</summary>
        private static string[] GetExcelHeaderRow(ISheet sheet, out int headerRowIndex)
        {
            headerRowIndex = 0;
            var keywords = new[] { "PDM", "DESCRIPTION", "품명", "설명", "규격", "사양", "SPECIFICATION", "품목" };
            for (int r = 0; r <= Math.Min(15, sheet.LastRowNum); r++)
            {
                var row = sheet.GetRow(r);
                if (row == null) continue;
                var cells = new List<string>();
                for (int c = 0; c < row.LastCellNum; c++)
                    cells.Add(GetCellString(row.GetCell(c)));
                var line = string.Join(" ", cells).ToUpperInvariant();
                bool hasHeader = keywords.Any(k => line.IndexOf(k.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
                if (hasHeader && cells.Count > 0)
                {
                    headerRowIndex = r;
                    return cells.ToArray();
                }
            }
            var firstRow = sheet.GetRow(0);
            if (firstRow != null)
            {
                var list = new List<string>();
                for (int c = 0; c < firstRow.LastCellNum; c++)
                    list.Add(GetCellString(firstRow.GetCell(c)));
                return list.ToArray();
            }
            return null;
        }

        private static int FindColumnIndex(string[] headers, string[] names)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                var h = (headers[i] ?? "").Trim();
                if (string.IsNullOrEmpty(h)) continue;
                foreach (var n in names)
                    if (string.Equals(h, n.Trim(), StringComparison.OrdinalIgnoreCase))
                        return i;
            }
            return -1;
        }

        /// <summary>헤더 문자열이 names 중 하나를 포함하면 해당 열 인덱스 반환.</summary>
        private static int FindColumnIndexContains(string[] headers, string[] names)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                var h = (headers[i] ?? "").Trim();
                if (string.IsNullOrEmpty(h)) continue;
                foreach (var n in names)
                    if (h.IndexOf(n.Trim(), StringComparison.OrdinalIgnoreCase) >= 0)
                        return i;
            }
            return -1;
        }

        private void listViewDeviceList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
