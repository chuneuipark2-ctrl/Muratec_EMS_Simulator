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
    /// <summary>DIO I.O 체크 성적서 폼. CSV 로드, 어드레스 필터, 테스트 실시자 입력, PDF 저장.</summary>
    public partial class IOCheckSheetForm : Form
    {
        private DataTable _allIoTable = new DataTable();
        private string _csvPath = "";
        private const string DefaultCsvPath = @"06. 시스템파라미터\DIO_F0455_G01_HSR-050-HWAL.csv";

        public string TesterName => textBoxTesterName?.Text?.Trim() ?? "";
        public string TestDate => textBoxTestDate?.Text?.Trim() ?? "";

        public IOCheckSheetForm()
        {
            InitializeComponent();
        }

        private void IOCheckSheetForm_Load(object sender, EventArgs e)
        {
            textBoxTestDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            _csvPath = FindCsvPath();
            if (!string.IsNullOrEmpty(_csvPath))
                LoadCsvAndBind();
            else
                LoadEmbeddedData();
            if (_allIoTable.Rows.Count == 0)
                LoadEmbeddedData();
            FillAddressFilterCombo();
            ApplyFilter();
            SetGridColumnWidths();
            dataGridViewIO.CellBeginEdit += dataGridViewIO_CellBeginEdit;
            dataGridViewIO.CellFormatting += dataGridViewIO_CellFormatting;
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

        private string FindCsvPath()
        {
            var baseDir = Application.StartupPath;
            var candidates = new[]
            {
                Path.Combine(baseDir, DefaultCsvPath),
                Path.Combine(baseDir, "..", "..", DefaultCsvPath),
                Path.Combine(Environment.CurrentDirectory, DefaultCsvPath),
                @"c:\Users\CE-PARK\OneDrive - 현대그룹\바탕 화면\EMS 테스트 시뮬레이터 개발\06. 시스템파라미터\DIO_F0455_G01_HSR-050-HWAL.csv"
            };
            foreach (var p in candidates)
            {
                if (File.Exists(p)) return p;
            }
            return "";
        }

        private void LoadCsvAndBind()
        {
            try
            {
                var encoding = Encoding.GetEncoding(949);
                string fullText = File.ReadAllText(_csvPath, encoding);
                // 따옴표 안에 줄바꿈이 있으면 한 논리적 행이 여러 파일 줄로 쪼개짐 → 논리적 행 단위로 먼저 나눔
                string[] logicalRows = GetLogicalCsvRows(fullText);
                BuildDataTableFromCsv(logicalRows);
            }
            catch (Exception ex)
            {
                MessageBox.Show("CSV 로드 실패: " + ex.Message);
                LoadEmbeddedData();
            }
        }

        /// <summary>따옴표 안의 줄바꿈은 필드 일부로 간주하고, 따옴표 밖의 줄바꿈만 행 구분으로 사용.</summary>
        private static string[] GetLogicalCsvRows(string fullText)
        {
            var rows = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < fullText.Length; i++)
            {
                char c = fullText[i];
                if (c == '"') { inQuotes = !inQuotes; current.Append(c); continue; }
                if (!inQuotes && (c == '\n' || c == '\r'))
                {
                    if (c == '\r' && i + 1 < fullText.Length && fullText[i + 1] == '\n') i++;
                    rows.Add(current.ToString());
                    current.Clear();
                    continue;
                }
                current.Append(c);
            }
            if (current.Length > 0) rows.Add(current.ToString());
            return rows.ToArray();
        }

        /// <summary>CSV를 도표/이미지 열 구조에 맞춰 파싱. CSV 오른쪽 영역 컬럼 6~17 기준.</summary>
        private void BuildDataTableFromCsv(string[] lines)
        {
            _allIoTable.Clear();
            _allIoTable.Columns.Clear();
            // IO구분(INPUT/OUTPUT)은 표시용 색상에만 사용, 그리드에서는 숨김
            _allIoTable.Columns.Add("어드레스", typeof(string));
            _allIoTable.Columns.Add("IO구분", typeof(string));  // "INPUT" / "OUTPUT" → 빨강/초록 표기
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

            string currentAddress = "";
            string currentIoConn = "";
            string currentIoType = "";  // "INPUT" / "OUTPUT" → 행별 색상용

            for (int i = 0; i < lines.Length; i++)
            {
                var parts = ParseCsvLine(lines[i]);
                if (parts.Count < 14) continue;

                string col6 = GetPart(parts, 6);
                string col7 = GetPart(parts, 7).Trim();
                string col9 = GetPart(parts, 9).Replace("\r", "").Replace("\n", " ").Trim();

                if (col6 == "INPUT" || col6 == "OUTPUT")
                    currentIoType = col6;
                if (!string.IsNullOrEmpty(col7))
                {
                    if (col6 == "INPUT" || col6 == "OUTPUT")
                        currentAddress = col7;
                    else if (!string.IsNullOrEmpty(currentAddress) && LooksLikeAddress(col7))
                        currentAddress = col7;
                }
                if (!string.IsNullOrEmpty(col9))
                    currentIoConn = col9;
                if (string.IsNullOrEmpty(currentAddress))
                    continue;

                string bit = GetPart(parts, 8);
                if (string.IsNullOrEmpty(bit)) bit = GetPart(parts, 10);
                string ioPinNo = GetPart(parts, 10);
                string boardName = GetPart(parts, 11);
                string connNo = GetPart(parts, 12);
                string relayPinNo = GetPart(parts, 13);
                string signalName = GetPart(parts, 14);
                string logic = parts.Count > 16 ? GetPart(parts, 16) : "";
                string remark = parts.Count > 17 ? GetPart(parts, 17) : "";

                _allIoTable.Rows.Add(currentAddress, currentIoType, bit, currentIoConn, ioPinNo, boardName, connNo, relayPinNo, signalName, logic, false, remark);
            }
        }

        private List<string> ParseCsvLine(string line)
        {
            var list = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"') { inQuotes = !inQuotes; continue; }
                if (!inQuotes && c == ',') { list.Add(sb.ToString().Trim()); sb.Clear(); continue; }
                if (c != '\r') sb.Append(c);
            }
            list.Add(sb.ToString().Trim());
            return list;
        }

        private string GetPart(List<string> parts, int index)
        {
            return index < parts.Count ? parts[index] : "";
        }

        /// <summary>CSV col7이 어드레스(예: 800010, 800012, 80001A) 형태인지 판별. 헤더·문자열 제외.</summary>
        private static bool LooksLikeAddress(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length < 5 || s.Length > 6) return false;
            foreach (char c in s)
                if (!Uri.IsHexDigit(c)) return false;
            return true;
        }

        /// <summary>CSV 없을 때 사용. 도표와 동일한 11열 구조로 내장 샘플 로드.</summary>
        private void LoadEmbeddedData()
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

            // 각 행: 어드레스, IO구분, BIT, I.O기판/커넥터No., IO기판/PIN-NO, 중계기판/기판명칭, 중계기판/커넥터NO, 중계기판 PIN-NO., 신호명칭, 논리, 비고
            var rows = new[]
            {
                new[] { "800010", "INPUT", "1", "I/O CN31", "1", "DW", "CNH1", "10", "승강대적재1", "b", "" },
                new[] { "800010", "INPUT", "2", "I/O CN31", "2", "DW", "CNH1", "11", "", "", "" },
                new[] { "800010", "INPUT", "4", "I/O CN31", "5", "DW", "CNH2", "10", "우측 돌출감지", "b", "" },
                new[] { "800010", "INPUT", "5", "I/O CN31", "6", "DW", "CNH2", "11", "좌측 돌출감지", "b", "" },
                new[] { "800010", "INPUT", "8", "I/O CN31", "9", "DW", "CNO1A,B", "2", "승강원점감지", "a", "" },
                new[] { "800014", "INPUT", "1", "I/O CN41", "1", "UW", "CNS1", "1", "승강일시정지(추가)", "a", "8bit IN-1" },
                new[] { "800014", "INPUT", "2", "I/O CN41", "2", "UW", "CNS1", "2", "하강가", "a", "8bit IN-2" },
                new[] { "800014", "INPUT", "3", "I/O CN41", "3", "UW", "CNS1", "3", "승강일시정지해제(추가)", "a", "8bit IN-3" },
                new[] { "800014", "INPUT", "9", "I/O CN41", "9", "UW", "CN01", "2", "정지1", "a", "" },
                new[] { "800016", "INPUT", "1", "I/O CN41", "17", "UW", "CND1", "10", "현재치확인1", "b", "" },
                new[] { "800016", "INPUT", "2", "I/O CN41", "18", "UW", "CND1", "11", "현재치확인2", "b", "" },
            };
            foreach (var r in rows)
                _allIoTable.Rows.Add(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7], r[8], r[9], false, r[10]);
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
