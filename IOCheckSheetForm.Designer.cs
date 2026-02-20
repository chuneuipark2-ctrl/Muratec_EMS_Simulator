namespace EMS_TEST_SIMULATOR
{
    partial class IOCheckSheetForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IOCheckSheetForm));
            this.panelTop = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.textBoxTesterName = new System.Windows.Forms.TextBox();
            this.labelDate = new System.Windows.Forms.Label();
            this.textBoxTestDate = new System.Windows.Forms.TextBox();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.panelRight = new System.Windows.Forms.Panel();
            this.labelLegendOutput = new System.Windows.Forms.Label();
            this.labelLegendInput = new System.Windows.Forms.Label();
            this.labelLegend = new System.Windows.Forms.Label();
            this.checkBoxUsedSignalFilter = new System.Windows.Forms.CheckBox();
            this.comboFilter = new System.Windows.Forms.ComboBox();
            this.labelFilter = new System.Windows.Forms.Label();
            this.dataGridViewIO = new System.Windows.Forms.DataGridView();
            this.buttonSavePdf = new System.Windows.Forms.Button();
            this.panelTop.SuspendLayout();
            this.panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewIO)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.labelTitle);
            this.panelTop.Controls.Add(this.textBoxTesterName);
            this.panelTop.Controls.Add(this.labelDate);
            this.panelTop.Controls.Add(this.textBoxTestDate);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1857, 56);
            this.panelTop.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(315, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 25);
            this.label1.TabIndex = 5;
            this.label1.Text = "수행원 :";
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.labelTitle.ForeColor = System.Drawing.Color.White;
            this.labelTitle.Location = new System.Drawing.Point(12, 10);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(249, 32);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "SKY-RAV DIO SHEET";
            this.labelTitle.Click += new System.EventHandler(this.labelTitle_Click);
            // 
            // textBoxTesterName
            // 
            this.textBoxTesterName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.textBoxTesterName.ForeColor = System.Drawing.Color.White;
            this.textBoxTesterName.Location = new System.Drawing.Point(397, 10);
            this.textBoxTesterName.Name = "textBoxTesterName";
            this.textBoxTesterName.Size = new System.Drawing.Size(140, 31);
            this.textBoxTesterName.TabIndex = 2;
            // 
            // labelDate
            // 
            this.labelDate.AutoSize = true;
            this.labelDate.ForeColor = System.Drawing.Color.White;
            this.labelDate.Location = new System.Drawing.Point(543, 10);
            this.labelDate.Name = "labelDate";
            this.labelDate.Size = new System.Drawing.Size(112, 25);
            this.labelDate.TabIndex = 3;
            this.labelDate.Text = "테스트 일자:";
            // 
            // textBoxTestDate
            // 
            this.textBoxTestDate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.textBoxTestDate.ForeColor = System.Drawing.Color.White;
            this.textBoxTestDate.Location = new System.Drawing.Point(661, 10);
            this.textBoxTestDate.Name = "textBoxTestDate";
            this.textBoxTestDate.Size = new System.Drawing.Size(120, 31);
            this.textBoxTestDate.TabIndex = 4;
            // 
            // panelLeft
            // 
            this.panelLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 56);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Padding = new System.Windows.Forms.Padding(8);
            this.panelLeft.Size = new System.Drawing.Size(280, 865);
            this.panelLeft.TabIndex = 1;
            // 
            // labelOptionTitle
            // 
            this.labelOptionTitle = new System.Windows.Forms.Label();
            this.labelOptionTitle.AutoSize = true;
            this.labelOptionTitle.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold);
            this.labelOptionTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(255)))));
            this.labelOptionTitle.Location = new System.Drawing.Point(12, 12);
            this.labelOptionTitle.Name = "labelOptionTitle";
            this.labelOptionTitle.Size = new System.Drawing.Size(124, 30);
            this.labelOptionTitle.TabIndex = 0;
            this.labelOptionTitle.Text = "I/O 옵션 설정";
            // 
            // labelHoistType
            // 
            this.labelHoistType = new System.Windows.Forms.Label();
            this.labelHoistType.AutoSize = true;
            this.labelHoistType.ForeColor = System.Drawing.Color.White;
            this.labelHoistType.Location = new System.Drawing.Point(12, 55);
            this.labelHoistType.Name = "labelHoistType";
            this.labelHoistType.Size = new System.Drawing.Size(79, 25);
            this.labelHoistType.TabIndex = 1;
            this.labelHoistType.Text = "승강대 :";
            // 
            // comboHoistType
            // 
            this.comboHoistType = new System.Windows.Forms.ComboBox();
            this.comboHoistType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboHoistType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboHoistType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboHoistType.ForeColor = System.Drawing.Color.White;
            this.comboHoistType.FormattingEnabled = true;
            this.comboHoistType.Items.AddRange(new object[] { "CHUCK", "CAGE", "컨베이어" });
            this.comboHoistType.Location = new System.Drawing.Point(12, 83);
            this.comboHoistType.Name = "comboHoistType";
            this.comboHoistType.Size = new System.Drawing.Size(250, 33);
            this.comboHoistType.TabIndex = 2;
            // 
            // labelCommType
            // 
            this.labelCommType = new System.Windows.Forms.Label();
            this.labelCommType.AutoSize = true;
            this.labelCommType.ForeColor = System.Drawing.Color.White;
            this.labelCommType.Location = new System.Drawing.Point(12, 130);
            this.labelCommType.Name = "labelCommType";
            this.labelCommType.Size = new System.Drawing.Size(97, 25);
            this.labelCommType.TabIndex = 3;
            this.labelCommType.Text = "통신방식 :";
            // 
            // comboCommType
            // 
            this.comboCommType = new System.Windows.Forms.ComboBox();
            this.comboCommType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboCommType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCommType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboCommType.ForeColor = System.Drawing.Color.White;
            this.comboCommType.FormattingEnabled = true;
            this.comboCommType.Items.AddRange(new object[] { "없음", "ROP", "8bit", "SS무선" });
            this.comboCommType.Location = new System.Drawing.Point(12, 158);
            this.comboCommType.Name = "comboCommType";
            this.comboCommType.Size = new System.Drawing.Size(250, 33);
            this.comboCommType.TabIndex = 4;
            // 
            // labelCollision
            // 
            this.labelCollision = new System.Windows.Forms.Label();
            this.labelCollision.AutoSize = true;
            this.labelCollision.ForeColor = System.Drawing.Color.White;
            this.labelCollision.Location = new System.Drawing.Point(12, 205);
            this.labelCollision.Name = "labelCollision";
            this.labelCollision.Size = new System.Drawing.Size(133, 25);
            this.labelCollision.TabIndex = 5;
            this.labelCollision.Text = "충돌방지센서 :";
            // 
            // comboCollision
            // 
            this.comboCollision = new System.Windows.Forms.ComboBox();
            this.comboCollision.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboCollision.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCollision.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboCollision.ForeColor = System.Drawing.Color.White;
            this.comboCollision.FormattingEnabled = true;
            this.comboCollision.Items.AddRange(new object[] { "없음", "1개소 전측", "1개소 후측", "2개소" });
            this.comboCollision.Location = new System.Drawing.Point(12, 233);
            this.comboCollision.Name = "comboCollision";
            this.comboCollision.Size = new System.Drawing.Size(250, 33);
            this.comboCollision.TabIndex = 6;
            // 
            // labelLayout
            // 
            this.labelLayout = new System.Windows.Forms.Label();
            this.labelLayout.AutoSize = true;
            this.labelLayout.ForeColor = System.Drawing.Color.White;
            this.labelLayout.Location = new System.Drawing.Point(12, 280);
            this.labelLayout.Name = "labelLayout";
            this.labelLayout.Size = new System.Drawing.Size(115, 25);
            this.labelLayout.TabIndex = 7;
            this.labelLayout.Text = "레이아웃 :";
            // 
            // comboLayout
            // 
            this.comboLayout = new System.Windows.Forms.ComboBox();
            this.comboLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLayout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboLayout.ForeColor = System.Drawing.Color.White;
            this.comboLayout.FormattingEnabled = true;
            this.comboLayout.Items.AddRange(new object[] { "직선형", "분기장치" });
            this.comboLayout.Location = new System.Drawing.Point(12, 308);
            this.comboLayout.Name = "comboLayout";
            this.comboLayout.Size = new System.Drawing.Size(250, 33);
            this.comboLayout.TabIndex = 8;
            // 
            // labelCargoProtrusion
            // 
            this.labelCargoProtrusion = new System.Windows.Forms.Label();
            this.labelCargoProtrusion.AutoSize = true;
            this.labelCargoProtrusion.ForeColor = System.Drawing.Color.White;
            this.labelCargoProtrusion.Location = new System.Drawing.Point(12, 355);
            this.labelCargoProtrusion.Name = "labelCargoProtrusion";
            this.labelCargoProtrusion.Size = new System.Drawing.Size(115, 25);
            this.labelCargoProtrusion.TabIndex = 9;
            this.labelCargoProtrusion.Text = "화물돌출센서 :";
            // 
            // comboCargoProtrusion
            // 
            this.comboCargoProtrusion = new System.Windows.Forms.ComboBox();
            this.comboCargoProtrusion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboCargoProtrusion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCargoProtrusion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboCargoProtrusion.ForeColor = System.Drawing.Color.White;
            this.comboCargoProtrusion.FormattingEnabled = true;
            this.comboCargoProtrusion.Items.AddRange(new object[] { "없음", "우측", "좌측", "좌/우" });
            this.comboCargoProtrusion.Location = new System.Drawing.Point(12, 383);
            this.comboCargoProtrusion.Name = "comboCargoProtrusion";
            this.comboCargoProtrusion.Size = new System.Drawing.Size(250, 33);
            this.comboCargoProtrusion.TabIndex = 10;
            // 
            // labelLiftStop
            // 
            this.labelLiftStop = new System.Windows.Forms.Label();
            this.labelLiftStop.AutoSize = true;
            this.labelLiftStop.ForeColor = System.Drawing.Color.White;
            this.labelLiftStop.Location = new System.Drawing.Point(12, 430);
            this.labelLiftStop.Name = "labelLiftStop";
            this.labelLiftStop.Size = new System.Drawing.Size(115, 25);
            this.labelLiftStop.TabIndex = 11;
            this.labelLiftStop.Text = "승강정지센서 :";
            // 
            // comboLiftStop
            // 
            this.comboLiftStop = new System.Windows.Forms.ComboBox();
            this.comboLiftStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboLiftStop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLiftStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboLiftStop.ForeColor = System.Drawing.Color.White;
            this.comboLiftStop.FormattingEnabled = true;
            this.comboLiftStop.Items.AddRange(new object[] { "없음", "우측", "좌측", "좌/우" });
            this.comboLiftStop.Location = new System.Drawing.Point(12, 458);
            this.comboLiftStop.Name = "comboLiftStop";
            this.comboLiftStop.Size = new System.Drawing.Size(250, 33);
            this.comboLiftStop.TabIndex = 12;
            // 
            // labelOption
            // 
            this.labelOption = new System.Windows.Forms.Label();
            this.labelOption.AutoSize = true;
            this.labelOption.ForeColor = System.Drawing.Color.White;
            this.labelOption.Location = new System.Drawing.Point(12, 505);
            this.labelOption.Name = "labelOption";
            this.labelOption.Size = new System.Drawing.Size(133, 25);
            this.labelOption.TabIndex = 13;
            this.labelOption.Text = "추가옵션(8bit) :";
            // 
            // comboOption
            // 
            this.comboOption = new System.Windows.Forms.ComboBox();
            this.comboOption.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboOption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboOption.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboOption.ForeColor = System.Drawing.Color.White;
            this.comboOption.FormattingEnabled = true;
            this.comboOption.Items.AddRange(new object[] { "없음", "있음" });
            this.comboOption.Location = new System.Drawing.Point(12, 533);
            this.comboOption.Name = "comboOption";
            this.comboOption.Size = new System.Drawing.Size(250, 33);
            this.comboOption.TabIndex = 14;
            // 
            // buttonApplyOptions
            // 
            this.buttonApplyOptions = new System.Windows.Forms.Button();
            this.buttonApplyOptions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.buttonApplyOptions.FlatAppearance.BorderSize = 0;
            this.buttonApplyOptions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonApplyOptions.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.buttonApplyOptions.ForeColor = System.Drawing.Color.White;
            this.buttonApplyOptions.Location = new System.Drawing.Point(12, 585);
            this.buttonApplyOptions.Name = "buttonApplyOptions";
            this.buttonApplyOptions.Size = new System.Drawing.Size(250, 38);
            this.buttonApplyOptions.TabIndex = 15;
            this.buttonApplyOptions.Text = "I/O 표 적용";
            this.buttonApplyOptions.UseVisualStyleBackColor = false;
            this.buttonApplyOptions.Click += new System.EventHandler(this.buttonApplyOptions_Click);
            // 
            // panelLeft controls (must be added after initialization)
            // 
            this.panelLeft.Controls.Add(this.buttonApplyOptions);
            this.panelLeft.Controls.Add(this.comboOption);
            this.panelLeft.Controls.Add(this.labelOption);
            this.panelLeft.Controls.Add(this.comboLiftStop);
            this.panelLeft.Controls.Add(this.labelLiftStop);
            this.panelLeft.Controls.Add(this.comboCargoProtrusion);
            this.panelLeft.Controls.Add(this.labelCargoProtrusion);
            this.panelLeft.Controls.Add(this.comboLayout);
            this.panelLeft.Controls.Add(this.labelLayout);
            this.panelLeft.Controls.Add(this.comboCollision);
            this.panelLeft.Controls.Add(this.labelCollision);
            this.panelLeft.Controls.Add(this.comboCommType);
            this.panelLeft.Controls.Add(this.labelCommType);
            this.panelLeft.Controls.Add(this.comboHoistType);
            this.panelLeft.Controls.Add(this.labelHoistType);
            this.panelLeft.Controls.Add(this.labelOptionTitle);
            // 
            // panelRight
            // 
            this.panelRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panelRight.Controls.Add(this.labelLegendOutput);
            this.panelRight.Controls.Add(this.labelLegendInput);
            this.panelRight.Controls.Add(this.labelLegend);
            this.panelRight.Controls.Add(this.checkBoxUsedSignalFilter);
            this.panelRight.Controls.Add(this.comboFilter);
            this.panelRight.Controls.Add(this.labelFilter);
            this.panelRight.Controls.Add(this.dataGridViewIO);
            this.panelRight.Controls.Add(this.buttonSavePdf);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(280, 56);
            this.panelRight.Name = "panelRight";
            this.panelRight.Padding = new System.Windows.Forms.Padding(6);
            this.panelRight.Size = new System.Drawing.Size(1577, 865);
            this.panelRight.TabIndex = 2;
            // 
            // labelLegendOutput
            // 
            this.labelLegendOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLegendOutput.AutoSize = true;
            this.labelLegendOutput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(220)))), ((int)(((byte)(100)))));
            this.labelLegendOutput.Location = new System.Drawing.Point(1392, 10);
            this.labelLegendOutput.Name = "labelLegendOutput";
            this.labelLegendOutput.Size = new System.Drawing.Size(82, 25);
            this.labelLegendOutput.TabIndex = 9;
            this.labelLegendOutput.Text = "OUTPUT";
            // 
            // labelLegendInput
            // 
            this.labelLegendInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLegendInput.AutoSize = true;
            this.labelLegendInput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.labelLegendInput.Location = new System.Drawing.Point(1322, 10);
            this.labelLegendInput.Name = "labelLegendInput";
            this.labelLegendInput.Size = new System.Drawing.Size(64, 25);
            this.labelLegendInput.TabIndex = 8;
            this.labelLegendInput.Text = "INPUT";
            // 
            // labelLegend
            // 
            this.labelLegend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLegend.AutoSize = true;
            this.labelLegend.ForeColor = System.Drawing.Color.White;
            this.labelLegend.Location = new System.Drawing.Point(1258, 10);
            this.labelLegend.Name = "labelLegend";
            this.labelLegend.Size = new System.Drawing.Size(58, 25);
            this.labelLegend.TabIndex = 7;
            this.labelLegend.Text = "범례: ";
            // 
            // checkBoxUsedSignalFilter
            // 
            this.checkBoxUsedSignalFilter.AutoSize = true;
            this.checkBoxUsedSignalFilter.Checked = true;
            this.checkBoxUsedSignalFilter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxUsedSignalFilter.ForeColor = System.Drawing.Color.White;
            this.checkBoxUsedSignalFilter.Location = new System.Drawing.Point(270, 12);
            this.checkBoxUsedSignalFilter.Name = "checkBoxUsedSignalFilter";
            this.checkBoxUsedSignalFilter.Size = new System.Drawing.Size(170, 29);
            this.checkBoxUsedSignalFilter.TabIndex = 6;
            this.checkBoxUsedSignalFilter.Text = "사용신호 필터링";
            this.checkBoxUsedSignalFilter.UseVisualStyleBackColor = true;
            this.checkBoxUsedSignalFilter.CheckedChanged += new System.EventHandler(this.checkBoxUsedSignalFilter_CheckedChanged);
            // 
            // comboFilter
            // 
            this.comboFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboFilter.ForeColor = System.Drawing.Color.White;
            this.comboFilter.FormattingEnabled = true;
            this.comboFilter.Location = new System.Drawing.Point(97, 10);
            this.comboFilter.Name = "comboFilter";
            this.comboFilter.Size = new System.Drawing.Size(160, 33);
            this.comboFilter.TabIndex = 1;
            this.comboFilter.SelectedIndexChanged += new System.EventHandler(this.comboFilter_SelectedIndexChanged);
            // 
            // labelFilter
            // 
            this.labelFilter.AutoSize = true;
            this.labelFilter.ForeColor = System.Drawing.Color.White;
            this.labelFilter.Location = new System.Drawing.Point(12, 14);
            this.labelFilter.Name = "labelFilter";
            this.labelFilter.Size = new System.Drawing.Size(130, 25);
            this.labelFilter.TabIndex = 0;
            this.labelFilter.Text = "어드레스 필터:";
            // 
            // dataGridViewIO
            // 
            this.dataGridViewIO.AllowUserToAddRows = false;
            this.dataGridViewIO.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewIO.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("맑은 고딕", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewIO.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewIO.ColumnHeadersHeight = 28;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("맑은 고딕", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewIO.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewIO.EnableHeadersVisualStyles = false;
            this.dataGridViewIO.Location = new System.Drawing.Point(12, 42);
            this.dataGridViewIO.Name = "dataGridViewIO";
            this.dataGridViewIO.RowHeadersVisible = false;
            this.dataGridViewIO.RowHeadersWidth = 62;
            this.dataGridViewIO.Size = new System.Drawing.Size(1553, 765);
            this.dataGridViewIO.TabIndex = 2;
            // 
            // buttonSavePdf
            // 
            this.buttonSavePdf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSavePdf.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.buttonSavePdf.FlatAppearance.BorderSize = 0;
            this.buttonSavePdf.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSavePdf.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.buttonSavePdf.ForeColor = System.Drawing.Color.White;
            this.buttonSavePdf.Location = new System.Drawing.Point(1364, 821);
            this.buttonSavePdf.Name = "buttonSavePdf";
            this.buttonSavePdf.Size = new System.Drawing.Size(174, 32);
            this.buttonSavePdf.TabIndex = 3;
            this.buttonSavePdf.Text = "PDF/이미지 저장";
            this.buttonSavePdf.UseVisualStyleBackColor = false;
            this.buttonSavePdf.Click += new System.EventHandler(this.buttonSavePdf_Click);
            // 
            // IOCheckSheetForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1857, 921);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.panelTop);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "IOCheckSheetForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "I.O 체크 성적서";
            this.Load += new System.EventHandler(this.IOCheckSheetForm_Load);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelRight.ResumeLayout(false);
            this.panelRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewIO)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.TextBox textBoxTesterName;
        private System.Windows.Forms.Label labelDate;
        private System.Windows.Forms.TextBox textBoxTestDate;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.ComboBox comboFilter;
        private System.Windows.Forms.Label labelFilter;
        private System.Windows.Forms.DataGridView dataGridViewIO;
        private System.Windows.Forms.Button buttonSavePdf;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxUsedSignalFilter;
        private System.Windows.Forms.Label labelLegend;
        private System.Windows.Forms.Label labelLegendInput;
        private System.Windows.Forms.Label labelLegendOutput;
        private System.Windows.Forms.Label labelOptionTitle;
        private System.Windows.Forms.Label labelHoistType;
        private System.Windows.Forms.ComboBox comboHoistType;
        private System.Windows.Forms.Label labelCommType;
        private System.Windows.Forms.ComboBox comboCommType;
        private System.Windows.Forms.Label labelCollision;
        private System.Windows.Forms.ComboBox comboCollision;
        private System.Windows.Forms.Label labelLayout;
        private System.Windows.Forms.ComboBox comboLayout;
        private System.Windows.Forms.Label labelCargoProtrusion;
        private System.Windows.Forms.ComboBox comboCargoProtrusion;
        private System.Windows.Forms.Label labelLiftStop;
        private System.Windows.Forms.ComboBox comboLiftStop;
        private System.Windows.Forms.Label labelOption;
        private System.Windows.Forms.ComboBox comboOption;
        private System.Windows.Forms.Button buttonApplyOptions;
    }
}
