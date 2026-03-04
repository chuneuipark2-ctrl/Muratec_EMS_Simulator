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
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonApplyOptions = new System.Windows.Forms.Button();
            this.IO_CBOX2 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.IO_CBOX1 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.combo8bitStop = new System.Windows.Forms.ComboBox();
            this.label8bitStop = new System.Windows.Forms.Label();
            this.comboTransferMotor = new System.Windows.Forms.ComboBox();
            this.labelTransferMotor = new System.Windows.Forms.Label();
            this.comboLimitDetect = new System.Windows.Forms.ComboBox();
            this.labelLimitDetect = new System.Windows.Forms.Label();
            this.comboCollisionDetect = new System.Windows.Forms.ComboBox();
            this.labelCollisionDetect = new System.Windows.Forms.Label();
            this.comboOption = new System.Windows.Forms.ComboBox();
            this.labelOption = new System.Windows.Forms.Label();
            this.comboLiftStop = new System.Windows.Forms.ComboBox();
            this.labelLiftStop = new System.Windows.Forms.Label();
            this.comboCargoProtrusion = new System.Windows.Forms.ComboBox();
            this.labelCargoProtrusion = new System.Windows.Forms.Label();
            this.comboLayout = new System.Windows.Forms.ComboBox();
            this.labelLayout = new System.Windows.Forms.Label();
            this.comboCollision = new System.Windows.Forms.ComboBox();
            this.labelCollision = new System.Windows.Forms.Label();
            this.comboCommType = new System.Windows.Forms.ComboBox();
            this.labelCommType = new System.Windows.Forms.Label();
            this.comboHoistType = new System.Windows.Forms.ComboBox();
            this.labelHoistType = new System.Windows.Forms.Label();
            this.labelOptionTitle = new System.Windows.Forms.Label();
            this.panelRight = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panelDeviceList = new System.Windows.Forms.Panel();
            this.panelDeviceListTop = new System.Windows.Forms.Panel();
            this.buttonLoadExcel = new System.Windows.Forms.Button();
            this.listViewDeviceList = new System.Windows.Forms.ListView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dataGridViewIO = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            this.labelLegendOutput = new System.Windows.Forms.Label();
            this.labelLegendInput = new System.Windows.Forms.Label();
            this.labelLegend = new System.Windows.Forms.Label();
            this.checkBoxUsedSignalFilter = new System.Windows.Forms.CheckBox();
            this.comboFilter = new System.Windows.Forms.ComboBox();
            this.labelFilter = new System.Windows.Forms.Label();
            this.buttonSavePdf = new System.Windows.Forms.Button();
            this.panelTop.SuspendLayout();
            this.panelLeft.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panelDeviceList.SuspendLayout();
            this.panelDeviceListTop.SuspendLayout();
            this.tabPage2.SuspendLayout();
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
            this.panelTop.Size = new System.Drawing.Size(1984, 56);
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
            this.textBoxTesterName.Click += new System.EventHandler(this.textBoxTesterName_Click);
            this.textBoxTesterName.TextChanged += new System.EventHandler(this.textBoxTesterName_TextChanged);
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
            this.panelLeft.Controls.Add(this.comboBox3);
            this.panelLeft.Controls.Add(this.label4);
            this.panelLeft.Controls.Add(this.buttonApplyOptions);
            this.panelLeft.Controls.Add(this.IO_CBOX2);
            this.panelLeft.Controls.Add(this.label2);
            this.panelLeft.Controls.Add(this.IO_CBOX1);
            this.panelLeft.Controls.Add(this.label3);
            this.panelLeft.Controls.Add(this.combo8bitStop);
            this.panelLeft.Controls.Add(this.label8bitStop);
            this.panelLeft.Controls.Add(this.comboTransferMotor);
            this.panelLeft.Controls.Add(this.labelTransferMotor);
            this.panelLeft.Controls.Add(this.comboLimitDetect);
            this.panelLeft.Controls.Add(this.labelLimitDetect);
            this.panelLeft.Controls.Add(this.comboCollisionDetect);
            this.panelLeft.Controls.Add(this.labelCollisionDetect);
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
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 56);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Padding = new System.Windows.Forms.Padding(8);
            this.panelLeft.Size = new System.Drawing.Size(280, 1045);
            this.panelLeft.TabIndex = 1;
            // 
            // comboBox3
            // 
            this.comboBox3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboBox3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox3.ForeColor = System.Drawing.Color.White;
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Items.AddRange(new object[] {
            "없음",
            "있음"});
            this.comboBox3.Location = new System.Drawing.Point(10, 807);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(250, 33);
            this.comboBox3.TabIndex = 33;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(11, 779);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 25);
            this.label4.TabIndex = 32;
            this.label4.Text = "극한검출 :";
            // 
            // buttonApplyOptions
            // 
            this.buttonApplyOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApplyOptions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.buttonApplyOptions.FlatAppearance.BorderSize = 0;
            this.buttonApplyOptions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonApplyOptions.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.buttonApplyOptions.ForeColor = System.Drawing.Color.White;
            this.buttonApplyOptions.Location = new System.Drawing.Point(45, 982);
            this.buttonApplyOptions.Name = "buttonApplyOptions";
            this.buttonApplyOptions.Size = new System.Drawing.Size(174, 32);
            this.buttonApplyOptions.TabIndex = 16;
            this.buttonApplyOptions.Text = "I/O 적용";
            this.buttonApplyOptions.UseVisualStyleBackColor = false;
            this.buttonApplyOptions.Click += new System.EventHandler(this.buttonApplyOptions_Click);
            // 
            // IO_CBOX2
            // 
            this.IO_CBOX2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.IO_CBOX2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.IO_CBOX2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.IO_CBOX2.ForeColor = System.Drawing.Color.White;
            this.IO_CBOX2.FormattingEnabled = true;
            this.IO_CBOX2.Items.AddRange(new object[] {
            "없음",
            "있음"});
            this.IO_CBOX2.Location = new System.Drawing.Point(12, 162);
            this.IO_CBOX2.Name = "IO_CBOX2";
            this.IO_CBOX2.Size = new System.Drawing.Size(250, 33);
            this.IO_CBOX2.TabIndex = 31;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(12, 134);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 25);
            this.label2.TabIndex = 30;
            this.label2.Text = "분기장치 :";
            // 
            // IO_CBOX1
            // 
            this.IO_CBOX1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.IO_CBOX1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.IO_CBOX1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.IO_CBOX1.ForeColor = System.Drawing.Color.White;
            this.IO_CBOX1.FormattingEnabled = true;
            this.IO_CBOX1.Items.AddRange(new object[] {
            "직선형",
            "루프"});
            this.IO_CBOX1.Location = new System.Drawing.Point(12, 96);
            this.IO_CBOX1.Name = "IO_CBOX1";
            this.IO_CBOX1.Size = new System.Drawing.Size(250, 33);
            this.IO_CBOX1.TabIndex = 29;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(12, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 25);
            this.label3.TabIndex = 28;
            this.label3.Text = "형태 :";
            // 
            // combo8bitStop
            // 
            this.combo8bitStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.combo8bitStop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combo8bitStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.combo8bitStop.ForeColor = System.Drawing.Color.White;
            this.combo8bitStop.FormattingEnabled = true;
            this.combo8bitStop.Items.AddRange(new object[] {
            "1점",
            "2점"});
            this.combo8bitStop.Location = new System.Drawing.Point(12, 934);
            this.combo8bitStop.Name = "combo8bitStop";
            this.combo8bitStop.Size = new System.Drawing.Size(250, 33);
            this.combo8bitStop.TabIndex = 27;
            // 
            // label8bitStop
            // 
            this.label8bitStop.AutoSize = true;
            this.label8bitStop.ForeColor = System.Drawing.Color.White;
            this.label8bitStop.Location = new System.Drawing.Point(12, 906);
            this.label8bitStop.Name = "label8bitStop";
            this.label8bitStop.Size = new System.Drawing.Size(125, 25);
            this.label8bitStop.TabIndex = 26;
            this.label8bitStop.Text = "8bit전송정지 :";
            // 
            // comboTransferMotor
            // 
            this.comboTransferMotor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboTransferMotor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTransferMotor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboTransferMotor.ForeColor = System.Drawing.Color.White;
            this.comboTransferMotor.FormattingEnabled = true;
            this.comboTransferMotor.Items.AddRange(new object[] {
            "없음",
            "1개",
            "2개"});
            this.comboTransferMotor.Location = new System.Drawing.Point(11, 870);
            this.comboTransferMotor.Name = "comboTransferMotor";
            this.comboTransferMotor.Size = new System.Drawing.Size(250, 33);
            this.comboTransferMotor.TabIndex = 24;
            // 
            // labelTransferMotor
            // 
            this.labelTransferMotor.AutoSize = true;
            this.labelTransferMotor.ForeColor = System.Drawing.Color.White;
            this.labelTransferMotor.Location = new System.Drawing.Point(12, 842);
            this.labelTransferMotor.Name = "labelTransferMotor";
            this.labelTransferMotor.Size = new System.Drawing.Size(118, 25);
            this.labelTransferMotor.TabIndex = 23;
            this.labelTransferMotor.Text = "이재모터 수 :";
            // 
            // comboLimitDetect
            // 
            this.comboLimitDetect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboLimitDetect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLimitDetect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboLimitDetect.ForeColor = System.Drawing.Color.White;
            this.comboLimitDetect.FormattingEnabled = true;
            this.comboLimitDetect.Items.AddRange(new object[] {
            "없음",
            "있음"});
            this.comboLimitDetect.Location = new System.Drawing.Point(11, 746);
            this.comboLimitDetect.Name = "comboLimitDetect";
            this.comboLimitDetect.Size = new System.Drawing.Size(251, 33);
            this.comboLimitDetect.TabIndex = 21;
            // 
            // labelLimitDetect
            // 
            this.labelLimitDetect.AutoSize = true;
            this.labelLimitDetect.ForeColor = System.Drawing.Color.White;
            this.labelLimitDetect.Location = new System.Drawing.Point(13, 718);
            this.labelLimitDetect.Name = "labelLimitDetect";
            this.labelLimitDetect.Size = new System.Drawing.Size(94, 25);
            this.labelLimitDetect.TabIndex = 20;
            this.labelLimitDetect.Text = "충돌검출 :";
            // 
            // comboCollisionDetect
            // 
            this.comboCollisionDetect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboCollisionDetect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCollisionDetect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboCollisionDetect.ForeColor = System.Drawing.Color.White;
            this.comboCollisionDetect.FormattingEnabled = true;
            this.comboCollisionDetect.Items.AddRange(new object[] {
            "없음",
            "우측",
            "좌측",
            "좌/우"});
            this.comboCollisionDetect.Location = new System.Drawing.Point(12, 683);
            this.comboCollisionDetect.Name = "comboCollisionDetect";
            this.comboCollisionDetect.Size = new System.Drawing.Size(249, 33);
            this.comboCollisionDetect.TabIndex = 18;
            // 
            // labelCollisionDetect
            // 
            this.labelCollisionDetect.AutoSize = true;
            this.labelCollisionDetect.ForeColor = System.Drawing.Color.White;
            this.labelCollisionDetect.Location = new System.Drawing.Point(13, 654);
            this.labelCollisionDetect.Name = "labelCollisionDetect";
            this.labelCollisionDetect.Size = new System.Drawing.Size(130, 25);
            this.labelCollisionDetect.TabIndex = 17;
            this.labelCollisionDetect.Text = "승강정지센서 :";
            // 
            // comboOption
            // 
            this.comboOption.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboOption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboOption.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboOption.ForeColor = System.Drawing.Color.White;
            this.comboOption.FormattingEnabled = true;
            this.comboOption.Items.AddRange(new object[] {
            "없음",
            "우측",
            "좌측",
            "좌/우"});
            this.comboOption.Location = new System.Drawing.Point(12, 618);
            this.comboOption.Name = "comboOption";
            this.comboOption.Size = new System.Drawing.Size(250, 33);
            this.comboOption.TabIndex = 14;
            // 
            // labelOption
            // 
            this.labelOption.AutoSize = true;
            this.labelOption.ForeColor = System.Drawing.Color.White;
            this.labelOption.Location = new System.Drawing.Point(12, 591);
            this.labelOption.Name = "labelOption";
            this.labelOption.Size = new System.Drawing.Size(130, 25);
            this.labelOption.TabIndex = 13;
            this.labelOption.Text = "화물돌출센서 :";
            // 
            // comboLiftStop
            // 
            this.comboLiftStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboLiftStop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLiftStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboLiftStop.ForeColor = System.Drawing.Color.White;
            this.comboLiftStop.FormattingEnabled = true;
            this.comboLiftStop.Items.AddRange(new object[] {
            "없음",
            "1개소",
            "2개소",
            "3개소",
            "4개소"});
            this.comboLiftStop.Location = new System.Drawing.Point(12, 555);
            this.comboLiftStop.Name = "comboLiftStop";
            this.comboLiftStop.Size = new System.Drawing.Size(250, 33);
            this.comboLiftStop.TabIndex = 12;
            // 
            // labelLiftStop
            // 
            this.labelLiftStop.AutoSize = true;
            this.labelLiftStop.ForeColor = System.Drawing.Color.White;
            this.labelLiftStop.Location = new System.Drawing.Point(12, 526);
            this.labelLiftStop.Name = "labelLiftStop";
            this.labelLiftStop.Size = new System.Drawing.Size(130, 25);
            this.labelLiftStop.TabIndex = 11;
            this.labelLiftStop.Text = "화물감지센서 :";
            // 
            // comboCargoProtrusion
            // 
            this.comboCargoProtrusion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboCargoProtrusion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCargoProtrusion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboCargoProtrusion.ForeColor = System.Drawing.Color.White;
            this.comboCargoProtrusion.FormattingEnabled = true;
            this.comboCargoProtrusion.Items.AddRange(new object[] {
            "없음",
            "있음"});
            this.comboCargoProtrusion.Location = new System.Drawing.Point(12, 490);
            this.comboCargoProtrusion.Name = "comboCargoProtrusion";
            this.comboCargoProtrusion.Size = new System.Drawing.Size(250, 33);
            this.comboCargoProtrusion.TabIndex = 10;
            // 
            // labelCargoProtrusion
            // 
            this.labelCargoProtrusion.AutoSize = true;
            this.labelCargoProtrusion.ForeColor = System.Drawing.Color.White;
            this.labelCargoProtrusion.Location = new System.Drawing.Point(12, 462);
            this.labelCargoProtrusion.Name = "labelCargoProtrusion";
            this.labelCargoProtrusion.Size = new System.Drawing.Size(112, 25);
            this.labelCargoProtrusion.TabIndex = 9;
            this.labelCargoProtrusion.Text = "이재인터록 :";
            // 
            // comboLayout
            // 
            this.comboLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLayout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboLayout.ForeColor = System.Drawing.Color.White;
            this.comboLayout.FormattingEnabled = true;
            this.comboLayout.Items.AddRange(new object[] {
            "1점",
            "2점",
            "3점",
            "4점"});
            this.comboLayout.Location = new System.Drawing.Point(12, 424);
            this.comboLayout.Name = "comboLayout";
            this.comboLayout.Size = new System.Drawing.Size(250, 33);
            this.comboLayout.TabIndex = 8;
            // 
            // labelLayout
            // 
            this.labelLayout.AutoSize = true;
            this.labelLayout.ForeColor = System.Drawing.Color.White;
            this.labelLayout.Location = new System.Drawing.Point(12, 397);
            this.labelLayout.Name = "labelLayout";
            this.labelLayout.Size = new System.Drawing.Size(94, 25);
            this.labelLayout.TabIndex = 7;
            this.labelLayout.Text = "분기신호 :";
            // 
            // comboCollision
            // 
            this.comboCollision.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboCollision.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCollision.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboCollision.ForeColor = System.Drawing.Color.White;
            this.comboCollision.FormattingEnabled = true;
            this.comboCollision.Items.AddRange(new object[] {
            "없음",
            "1개소 전측",
            "1개소 후측",
            "2개소"});
            this.comboCollision.Location = new System.Drawing.Point(12, 360);
            this.comboCollision.Name = "comboCollision";
            this.comboCollision.Size = new System.Drawing.Size(250, 33);
            this.comboCollision.TabIndex = 6;
            // 
            // labelCollision
            // 
            this.labelCollision.AutoSize = true;
            this.labelCollision.ForeColor = System.Drawing.Color.White;
            this.labelCollision.Location = new System.Drawing.Point(12, 333);
            this.labelCollision.Name = "labelCollision";
            this.labelCollision.Size = new System.Drawing.Size(130, 25);
            this.labelCollision.TabIndex = 5;
            this.labelCollision.Text = "충돌방지센서 :";
            // 
            // comboCommType
            // 
            this.comboCommType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboCommType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCommType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboCommType.ForeColor = System.Drawing.Color.White;
            this.comboCommType.FormattingEnabled = true;
            this.comboCommType.Items.AddRange(new object[] {
            "ROP",
            "8bit 센서",
            "SS무선"});
            this.comboCommType.Location = new System.Drawing.Point(12, 295);
            this.comboCommType.Name = "comboCommType";
            this.comboCommType.Size = new System.Drawing.Size(250, 33);
            this.comboCommType.TabIndex = 4;
            // 
            // labelCommType
            // 
            this.labelCommType.AutoSize = true;
            this.labelCommType.ForeColor = System.Drawing.Color.White;
            this.labelCommType.Location = new System.Drawing.Point(12, 267);
            this.labelCommType.Name = "labelCommType";
            this.labelCommType.Size = new System.Drawing.Size(94, 25);
            this.labelCommType.TabIndex = 3;
            this.labelCommType.Text = "교신방법 :";
            // 
            // comboHoistType
            // 
            this.comboHoistType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.comboHoistType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboHoistType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboHoistType.ForeColor = System.Drawing.Color.White;
            this.comboHoistType.FormattingEnabled = true;
            this.comboHoistType.Items.AddRange(new object[] {
            "CHUCK",
            "CAGE",
            "컨베이어"});
            this.comboHoistType.Location = new System.Drawing.Point(12, 229);
            this.comboHoistType.Name = "comboHoistType";
            this.comboHoistType.Size = new System.Drawing.Size(250, 33);
            this.comboHoistType.TabIndex = 2;
            // 
            // labelHoistType
            // 
            this.labelHoistType.AutoSize = true;
            this.labelHoistType.ForeColor = System.Drawing.Color.White;
            this.labelHoistType.Location = new System.Drawing.Point(12, 201);
            this.labelHoistType.Name = "labelHoistType";
            this.labelHoistType.Size = new System.Drawing.Size(76, 25);
            this.labelHoistType.TabIndex = 1;
            this.labelHoistType.Text = "승강대 :";
            // 
            // labelOptionTitle
            // 
            this.labelOptionTitle.AutoSize = true;
            this.labelOptionTitle.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold);
            this.labelOptionTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(255)))));
            this.labelOptionTitle.Location = new System.Drawing.Point(12, 12);
            this.labelOptionTitle.Name = "labelOptionTitle";
            this.labelOptionTitle.Size = new System.Drawing.Size(109, 30);
            this.labelOptionTitle.TabIndex = 0;
            this.labelOptionTitle.Text = "옵션 설정";
            // 
            // panelRight
            // 
            this.panelRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.panelRight.Controls.Add(this.tabControl1);
            this.panelRight.Controls.Add(this.button1);
            this.panelRight.Controls.Add(this.labelLegendOutput);
            this.panelRight.Controls.Add(this.labelLegendInput);
            this.panelRight.Controls.Add(this.labelLegend);
            this.panelRight.Controls.Add(this.checkBoxUsedSignalFilter);
            this.panelRight.Controls.Add(this.comboFilter);
            this.panelRight.Controls.Add(this.labelFilter);
            this.panelRight.Controls.Add(this.buttonSavePdf);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(280, 56);
            this.panelRight.Name = "panelRight";
            this.panelRight.Padding = new System.Windows.Forms.Padding(6);
            this.panelRight.Size = new System.Drawing.Size(1704, 1045);
            this.panelRight.TabIndex = 2;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tabControl1.Location = new System.Drawing.Point(9, 49);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1656, 805);
            this.tabControl1.TabIndex = 11;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.tabPage1.Controls.Add(this.panelDeviceList);
            this.tabPage1.Location = new System.Drawing.Point(4, 34);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1648, 767);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "DEVICE LIST";
            // 
            // panelDeviceList
            // 
            this.panelDeviceList.Controls.Add(this.panelDeviceListTop);
            this.panelDeviceList.Controls.Add(this.listViewDeviceList);
            this.panelDeviceList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDeviceList.Location = new System.Drawing.Point(3, 3);
            this.panelDeviceList.Name = "panelDeviceList";
            this.panelDeviceList.Padding = new System.Windows.Forms.Padding(8);
            this.panelDeviceList.Size = new System.Drawing.Size(1642, 761);
            this.panelDeviceList.TabIndex = 0;
            // 
            // panelDeviceListTop
            // 
            this.panelDeviceListTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panelDeviceListTop.Controls.Add(this.buttonLoadExcel);
            this.panelDeviceListTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelDeviceListTop.Location = new System.Drawing.Point(8, 8);
            this.panelDeviceListTop.Name = "panelDeviceListTop";
            this.panelDeviceListTop.Size = new System.Drawing.Size(1626, 45);
            this.panelDeviceListTop.TabIndex = 2;
            // 
            // buttonLoadExcel
            // 
            this.buttonLoadExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLoadExcel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.buttonLoadExcel.FlatAppearance.BorderSize = 0;
            this.buttonLoadExcel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonLoadExcel.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.buttonLoadExcel.ForeColor = System.Drawing.Color.White;
            this.buttonLoadExcel.Location = new System.Drawing.Point(1468, 6);
            this.buttonLoadExcel.MinimumSize = new System.Drawing.Size(150, 32);
            this.buttonLoadExcel.Name = "buttonLoadExcel";
            this.buttonLoadExcel.Size = new System.Drawing.Size(150, 32);
            this.buttonLoadExcel.TabIndex = 0;
            this.buttonLoadExcel.Text = "파일 불러오기";
            this.buttonLoadExcel.UseVisualStyleBackColor = false;
            this.buttonLoadExcel.Click += new System.EventHandler(this.buttonLoadExcel_Click);
            // 
            // listViewDeviceList
            // 
            this.listViewDeviceList.AllowDrop = true;
            this.listViewDeviceList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.listViewDeviceList.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.listViewDeviceList.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.listViewDeviceList.ForeColor = System.Drawing.Color.White;
            this.listViewDeviceList.FullRowSelect = true;
            this.listViewDeviceList.HideSelection = false;
            this.listViewDeviceList.Location = new System.Drawing.Point(8, 52);
            this.listViewDeviceList.Name = "listViewDeviceList";
            this.listViewDeviceList.Size = new System.Drawing.Size(1626, 701);
            this.listViewDeviceList.TabIndex = 1;
            this.listViewDeviceList.UseCompatibleStateImageBehavior = false;
            this.listViewDeviceList.View = System.Windows.Forms.View.Details;
            this.listViewDeviceList.SelectedIndexChanged += new System.EventHandler(this.listViewDeviceList_SelectedIndexChanged);
            this.listViewDeviceList.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewDeviceList_DragDrop);
            this.listViewDeviceList.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewDeviceList_DragEnter);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dataGridViewIO);
            this.tabPage2.Location = new System.Drawing.Point(4, 34);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1648, 767);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "I.O LIST";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridViewIO
            // 
            this.dataGridViewIO.AllowUserToAddRows = false;
            this.dataGridViewIO.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewIO.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewIO.ColumnHeadersHeight = 28;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewIO.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewIO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewIO.EnableHeadersVisualStyles = false;
            this.dataGridViewIO.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewIO.Name = "dataGridViewIO";
            this.dataGridViewIO.RowHeadersVisible = false;
            this.dataGridViewIO.RowHeadersWidth = 62;
            this.dataGridViewIO.Size = new System.Drawing.Size(1642, 761);
            this.dataGridViewIO.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(184, 873);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(174, 32);
            this.button1.TabIndex = 10;
            this.button1.Text = "상태저장";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // labelLegendOutput
            // 
            this.labelLegendOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLegendOutput.AutoSize = true;
            this.labelLegendOutput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(220)))), ((int)(((byte)(100)))));
            this.labelLegendOutput.Location = new System.Drawing.Point(1519, 10);
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
            this.labelLegendInput.Location = new System.Drawing.Point(1449, 10);
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
            this.labelLegend.Location = new System.Drawing.Point(1385, 10);
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
            // buttonSavePdf
            // 
            this.buttonSavePdf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSavePdf.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.buttonSavePdf.FlatAppearance.BorderSize = 0;
            this.buttonSavePdf.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSavePdf.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.buttonSavePdf.ForeColor = System.Drawing.Color.White;
            this.buttonSavePdf.Location = new System.Drawing.Point(1491, 873);
            this.buttonSavePdf.Name = "buttonSavePdf";
            this.buttonSavePdf.Size = new System.Drawing.Size(174, 32);
            this.buttonSavePdf.TabIndex = 3;
            this.buttonSavePdf.Text = "체크시트 출력";
            this.buttonSavePdf.UseVisualStyleBackColor = false;
            this.buttonSavePdf.Click += new System.EventHandler(this.buttonSavePdf_Click);
            // 
            // IOCheckSheetForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1984, 1101);
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
            this.panelLeft.ResumeLayout(false);
            this.panelLeft.PerformLayout();
            this.panelRight.ResumeLayout(false);
            this.panelRight.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panelDeviceList.ResumeLayout(false);
            this.panelDeviceListTop.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
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
        private System.Windows.Forms.Label labelCollisionDetect;
        private System.Windows.Forms.ComboBox comboCollisionDetect;
        private System.Windows.Forms.Label labelLimitDetect;
        private System.Windows.Forms.ComboBox comboLimitDetect;
        private System.Windows.Forms.Label labelTransferMotor;
        private System.Windows.Forms.ComboBox comboTransferMotor;
        private System.Windows.Forms.Label label8bitStop;
        private System.Windows.Forms.ComboBox combo8bitStop;
        private System.Windows.Forms.ComboBox IO_CBOX2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox IO_CBOX1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Panel panelDeviceList;
        private System.Windows.Forms.Panel panelDeviceListTop;
        private System.Windows.Forms.Button buttonLoadExcel;
        private System.Windows.Forms.ListView listViewDeviceList;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dataGridViewIO;
    }
}
