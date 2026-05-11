namespace codinglearning
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblTimer = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.btnStopSession = new System.Windows.Forms.ToolStripStatusLabel();
            this.learningTimer = new System.Windows.Forms.Timer(this.components);
            this.lblGitHubPush = new System.Windows.Forms.Label();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.groupBox14 = new System.Windows.Forms.GroupBox();
            this.lblMaxFocus = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lblWeeklyAvg = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.lblTodayTotal = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.groupBox15 = new System.Windows.Forms.GroupBox();
            this.dgvTimeRecords = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.chartTimeHistory = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.groupBox13 = new System.Windows.Forms.GroupBox();
            this.dgvRecentRecords = new System.Windows.Forms.DataGridView();
            this.ProblemId2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Title2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Result2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Date2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox12 = new System.Windows.Forms.GroupBox();
            this.chartAccuracy = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.lblAccuracy = new System.Windows.Forms.Label();
            this.label41 = new System.Windows.Forms.Label();
            this.lblWrong = new System.Windows.Forms.Label();
            this.label39 = new System.Windows.Forms.Label();
            this.lblCorrect = new System.Windows.Forms.Label();
            this.label37 = new System.Windows.Forms.Label();
            this.lblTotalSolved = new System.Windows.Forms.Label();
            this.label35 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox16 = new System.Windows.Forms.GroupBox();
            this.rbWrong = new System.Windows.Forms.RadioButton();
            this.rbCorrect = new System.Windows.Forms.RadioButton();
            this.rbAll = new System.Windows.Forms.RadioButton();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.btnExportWrongList = new System.Windows.Forms.Button();
            this.lblWrongProbTags = new System.Windows.Forms.Label();
            this.lblWrongProbDiff = new System.Windows.Forms.Label();
            this.lblWrongProbTitle = new System.Windows.Forms.Label();
            this.lblWrongProbNum = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.lblWrongProbResult = new System.Windows.Forms.Label();
            this.btnSolveAgain = new System.Windows.Forms.Button();
            this.btnViewWrongProblem = new System.Windows.Forms.Button();
            this.label28 = new System.Windows.Forms.Label();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.dgvWrongList = new System.Windows.Forms.DataGridView();
            this.ProblemId1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Title1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Rating1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Tags1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Result1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Date1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.txtResult = new System.Windows.Forms.RichTextBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.txtCode = new System.Windows.Forms.RichTextBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.btnRetryTranslate = new System.Windows.Forms.Button();
            this.btnResetCode = new System.Windows.Forms.Button();
            this.btnSubmitCF = new System.Windows.Forms.Button();
            this.btnRunSample = new System.Windows.Forms.Button();
            this.cbLanguage = new System.Windows.Forms.ComboBox();
            this.label20 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.lblCodeProbTags = new System.Windows.Forms.Label();
            this.lblCodeProbDiff = new System.Windows.Forms.Label();
            this.lblCodeProbTitle = new System.Windows.Forms.Label();
            this.lblCodeProbNum = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblDarkMode = new System.Windows.Forms.Label();
            this.lblSelProbTags = new System.Windows.Forms.Label();
            this.lblSelProbDiff = new System.Windows.Forms.Label();
            this.lblSelProbTitle = new System.Windows.Forms.Label();
            this.lblSelProbNum = new System.Windows.Forms.Label();
            this.btnViewProblem = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnSearchAll = new System.Windows.Forms.Button();
            this.btnResetSearch = new System.Windows.Forms.Button();
            this.btnSearch = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtKeyword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMaxDifficulty = new System.Windows.Forms.TextBox();
            this.txtMinDifficulty = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dgvProblems = new System.Windows.Forms.DataGridView();
            this.ProblemId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Rating = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Tags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Result = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.statusStrip1.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.groupBox14.SuspendLayout();
            this.groupBox15.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTimeRecords)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartTimeHistory)).BeginInit();
            this.tabPage4.SuspendLayout();
            this.groupBox13.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecentRecords)).BeginInit();
            this.groupBox12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartAccuracy)).BeginInit();
            this.groupBox11.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox16.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.groupBox9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWrongList)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProblems)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 23);
            // 
            // lblTimer
            // 
            this.lblTimer.Name = "lblTimer";
            this.lblTimer.Size = new System.Drawing.Size(72, 23);
            this.lblTimer.Text = "00:00:00";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.lblTimer,
            this.btnStopSession});
            this.statusStrip1.Location = new System.Drawing.Point(0, 621);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1111, 29);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // btnStopSession
            // 
            this.btnStopSession.Name = "btnStopSession";
            this.btnStopSession.Size = new System.Drawing.Size(0, 23);
            this.btnStopSession.Click += new System.EventHandler(this.btnStopSession_Click);
            // 
            // learningTimer
            // 
            this.learningTimer.Interval = 1000;
            this.learningTimer.Tick += new System.EventHandler(this.learningTimer_Tick);
            // 
            // lblGitHubPush
            // 
            this.lblGitHubPush.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGitHubPush.AutoSize = true;
            this.lblGitHubPush.BackColor = System.Drawing.Color.Transparent;
            this.lblGitHubPush.Font = new System.Drawing.Font("맑은 고딕", 9.857143F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblGitHubPush.Location = new System.Drawing.Point(977, 624);
            this.lblGitHubPush.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblGitHubPush.Name = "lblGitHubPush";
            this.lblGitHubPush.Size = new System.Drawing.Size(119, 23);
            this.lblGitHubPush.TabIndex = 2;
            this.lblGitHubPush.Text = "🌱 잔디 심기";
            this.lblGitHubPush.Click += new System.EventHandler(this.lblGitHubPush_Click);
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.groupBox14);
            this.tabPage5.Controls.Add(this.groupBox15);
            this.tabPage5.Controls.Add(this.groupBox4);
            this.tabPage5.Location = new System.Drawing.Point(4, 35);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage5.Size = new System.Drawing.Size(1103, 582);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "학습 시간 통계/그래프";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // groupBox14
            // 
            this.groupBox14.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox14.Controls.Add(this.lblMaxFocus);
            this.groupBox14.Controls.Add(this.label11);
            this.groupBox14.Controls.Add(this.lblWeeklyAvg);
            this.groupBox14.Controls.Add(this.label22);
            this.groupBox14.Controls.Add(this.lblTodayTotal);
            this.groupBox14.Controls.Add(this.label24);
            this.groupBox14.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox14.Location = new System.Drawing.Point(13, 11);
            this.groupBox14.Name = "groupBox14";
            this.groupBox14.Size = new System.Drawing.Size(1076, 76);
            this.groupBox14.TabIndex = 6;
            this.groupBox14.TabStop = false;
            this.groupBox14.Text = "학습 시간 요약";
            // 
            // lblMaxFocus
            // 
            this.lblMaxFocus.AutoSize = true;
            this.lblMaxFocus.Location = new System.Drawing.Point(801, 36);
            this.lblMaxFocus.Name = "lblMaxFocus";
            this.lblMaxFocus.Size = new System.Drawing.Size(0, 23);
            this.lblMaxFocus.TabIndex = 5;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(671, 36);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(124, 23);
            this.label11.TabIndex = 4;
            this.label11.Text = "최장 집중 시간";
            // 
            // lblWeeklyAvg
            // 
            this.lblWeeklyAvg.AutoSize = true;
            this.lblWeeklyAvg.Location = new System.Drawing.Point(518, 36);
            this.lblWeeklyAvg.Name = "lblWeeklyAvg";
            this.lblWeeklyAvg.Size = new System.Drawing.Size(0, 23);
            this.lblWeeklyAvg.TabIndex = 3;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(348, 36);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(164, 23);
            this.label22.TabIndex = 2;
            this.label22.Text = "주간 평균 학습 시간";
            // 
            // lblTodayTotal
            // 
            this.lblTodayTotal.AutoSize = true;
            this.lblTodayTotal.Location = new System.Drawing.Point(182, 36);
            this.lblTodayTotal.Name = "lblTodayTotal";
            this.lblTodayTotal.Size = new System.Drawing.Size(0, 23);
            this.lblTodayTotal.TabIndex = 1;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(12, 35);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(164, 23);
            this.label24.TabIndex = 0;
            this.label24.Text = "오늘의 총 학습 시간";
            // 
            // groupBox15
            // 
            this.groupBox15.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox15.Controls.Add(this.dgvTimeRecords);
            this.groupBox15.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox15.Location = new System.Drawing.Point(13, 406);
            this.groupBox15.Name = "groupBox15";
            this.groupBox15.Size = new System.Drawing.Size(1076, 166);
            this.groupBox15.TabIndex = 5;
            this.groupBox15.TabStop = false;
            this.groupBox15.Text = "학습 시간 기록";
            // 
            // dgvTimeRecords
            // 
            this.dgvTimeRecords.AllowUserToAddRows = false;
            this.dgvTimeRecords.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvTimeRecords.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTimeRecords.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTimeRecords.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4});
            this.dgvTimeRecords.Location = new System.Drawing.Point(16, 29);
            this.dgvTimeRecords.Name = "dgvTimeRecords";
            this.dgvTimeRecords.ReadOnly = true;
            this.dgvTimeRecords.RowHeadersWidth = 51;
            this.dgvTimeRecords.RowTemplate.Height = 27;
            this.dgvTimeRecords.Size = new System.Drawing.Size(1046, 121);
            this.dgvTimeRecords.TabIndex = 0;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "날짜";
            this.dataGridViewTextBoxColumn1.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "학습 구간";
            this.dataGridViewTextBoxColumn2.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "순공 시간";
            this.dataGridViewTextBoxColumn3.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "성과";
            this.dataGridViewTextBoxColumn4.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.chartTimeHistory);
            this.groupBox4.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox4.Location = new System.Drawing.Point(13, 98);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(1076, 294);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "학습 시간 그래프";
            // 
            // chartTimeHistory
            // 
            this.chartTimeHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.chartTimeHistory.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chartTimeHistory.Legends.Add(legend1);
            this.chartTimeHistory.Location = new System.Drawing.Point(15, 29);
            this.chartTimeHistory.Name = "chartTimeHistory";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartTimeHistory.Series.Add(series1);
            this.chartTimeHistory.Size = new System.Drawing.Size(1046, 247);
            this.chartTimeHistory.TabIndex = 0;
            this.chartTimeHistory.Text = "chart1";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.groupBox13);
            this.tabPage4.Controls.Add(this.groupBox12);
            this.tabPage4.Controls.Add(this.groupBox11);
            this.tabPage4.Location = new System.Drawing.Point(4, 35);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage4.Size = new System.Drawing.Size(1103, 582);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "문제 풀이 통계/그래프";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // groupBox13
            // 
            this.groupBox13.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox13.Controls.Add(this.dgvRecentRecords);
            this.groupBox13.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox13.Location = new System.Drawing.Point(14, 407);
            this.groupBox13.Name = "groupBox13";
            this.groupBox13.Size = new System.Drawing.Size(1076, 166);
            this.groupBox13.TabIndex = 2;
            this.groupBox13.TabStop = false;
            this.groupBox13.Text = "최근 풀이 기록";
            // 
            // dgvRecentRecords
            // 
            this.dgvRecentRecords.AllowUserToAddRows = false;
            this.dgvRecentRecords.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvRecentRecords.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvRecentRecords.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRecentRecords.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ProblemId2,
            this.Title2,
            this.Result2,
            this.Date2});
            this.dgvRecentRecords.Location = new System.Drawing.Point(16, 29);
            this.dgvRecentRecords.Name = "dgvRecentRecords";
            this.dgvRecentRecords.ReadOnly = true;
            this.dgvRecentRecords.RowHeadersWidth = 51;
            this.dgvRecentRecords.RowTemplate.Height = 27;
            this.dgvRecentRecords.Size = new System.Drawing.Size(1046, 121);
            this.dgvRecentRecords.TabIndex = 0;
            // 
            // ProblemId2
            // 
            this.ProblemId2.HeaderText = "번호";
            this.ProblemId2.MinimumWidth = 6;
            this.ProblemId2.Name = "ProblemId2";
            this.ProblemId2.ReadOnly = true;
            // 
            // Title2
            // 
            this.Title2.HeaderText = "제목";
            this.Title2.MinimumWidth = 6;
            this.Title2.Name = "Title2";
            this.Title2.ReadOnly = true;
            // 
            // Result2
            // 
            this.Result2.HeaderText = "결과";
            this.Result2.MinimumWidth = 6;
            this.Result2.Name = "Result2";
            this.Result2.ReadOnly = true;
            // 
            // Date2
            // 
            this.Date2.HeaderText = "날짜";
            this.Date2.MinimumWidth = 6;
            this.Date2.Name = "Date2";
            this.Date2.ReadOnly = true;
            // 
            // groupBox12
            // 
            this.groupBox12.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox12.Controls.Add(this.chartAccuracy);
            this.groupBox12.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox12.Location = new System.Drawing.Point(14, 97);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Size = new System.Drawing.Size(1076, 299);
            this.groupBox12.TabIndex = 1;
            this.groupBox12.TabStop = false;
            this.groupBox12.Text = "정답률 그래프";
            // 
            // chartAccuracy
            // 
            this.chartAccuracy.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea2.Name = "ChartArea1";
            this.chartAccuracy.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.chartAccuracy.Legends.Add(legend2);
            this.chartAccuracy.Location = new System.Drawing.Point(15, 29);
            this.chartAccuracy.Name = "chartAccuracy";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.chartAccuracy.Series.Add(series2);
            this.chartAccuracy.Size = new System.Drawing.Size(1046, 252);
            this.chartAccuracy.TabIndex = 0;
            this.chartAccuracy.Text = "chart1";
            // 
            // groupBox11
            // 
            this.groupBox11.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox11.Controls.Add(this.lblAccuracy);
            this.groupBox11.Controls.Add(this.label41);
            this.groupBox11.Controls.Add(this.lblWrong);
            this.groupBox11.Controls.Add(this.label39);
            this.groupBox11.Controls.Add(this.lblCorrect);
            this.groupBox11.Controls.Add(this.label37);
            this.groupBox11.Controls.Add(this.lblTotalSolved);
            this.groupBox11.Controls.Add(this.label35);
            this.groupBox11.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox11.Location = new System.Drawing.Point(14, 11);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(1076, 76);
            this.groupBox11.TabIndex = 0;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "학습 요약";
            // 
            // lblAccuracy
            // 
            this.lblAccuracy.AutoSize = true;
            this.lblAccuracy.Location = new System.Drawing.Point(658, 36);
            this.lblAccuracy.Name = "lblAccuracy";
            this.lblAccuracy.Size = new System.Drawing.Size(0, 23);
            this.lblAccuracy.TabIndex = 7;
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Location = new System.Drawing.Point(591, 36);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(61, 23);
            this.label41.TabIndex = 6;
            this.label41.Text = "정답률";
            // 
            // lblWrong
            // 
            this.lblWrong.AutoSize = true;
            this.lblWrong.Location = new System.Drawing.Point(483, 35);
            this.lblWrong.Name = "lblWrong";
            this.lblWrong.Size = new System.Drawing.Size(0, 23);
            this.lblWrong.TabIndex = 5;
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Location = new System.Drawing.Point(410, 36);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(67, 23);
            this.label39.TabIndex = 4;
            this.label39.Text = "오답 수";
            // 
            // lblCorrect
            // 
            this.lblCorrect.AutoSize = true;
            this.lblCorrect.Location = new System.Drawing.Point(304, 36);
            this.lblCorrect.Name = "lblCorrect";
            this.lblCorrect.Size = new System.Drawing.Size(0, 23);
            this.lblCorrect.TabIndex = 3;
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(231, 36);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(67, 23);
            this.label37.TabIndex = 2;
            this.label37.Text = "정답 수";
            // 
            // lblTotalSolved
            // 
            this.lblTotalSolved.AutoSize = true;
            this.lblTotalSolved.Location = new System.Drawing.Point(125, 35);
            this.lblTotalSolved.Name = "lblTotalSolved";
            this.lblTotalSolved.Size = new System.Drawing.Size(0, 23);
            this.lblTotalSolved.TabIndex = 1;
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(12, 35);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(107, 23);
            this.label35.TabIndex = 0;
            this.label35.Text = "전체 풀이 수";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox16);
            this.tabPage3.Controls.Add(this.groupBox10);
            this.tabPage3.Controls.Add(this.groupBox9);
            this.tabPage3.Location = new System.Drawing.Point(4, 35);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage3.Size = new System.Drawing.Size(1103, 582);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "문제 풀이 내역";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox16
            // 
            this.groupBox16.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox16.Controls.Add(this.rbWrong);
            this.groupBox16.Controls.Add(this.rbCorrect);
            this.groupBox16.Controls.Add(this.rbAll);
            this.groupBox16.Location = new System.Drawing.Point(13, 10);
            this.groupBox16.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox16.Name = "groupBox16";
            this.groupBox16.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox16.Size = new System.Drawing.Size(813, 67);
            this.groupBox16.TabIndex = 2;
            this.groupBox16.TabStop = false;
            this.groupBox16.Text = "조회 조건";
            // 
            // rbWrong
            // 
            this.rbWrong.AutoSize = true;
            this.rbWrong.Location = new System.Drawing.Point(372, 32);
            this.rbWrong.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbWrong.Name = "rbWrong";
            this.rbWrong.Size = new System.Drawing.Size(172, 27);
            this.rbWrong.TabIndex = 2;
            this.rbWrong.TabStop = true;
            this.rbWrong.Text = "❌ 미해결(오답)만";
            this.rbWrong.UseVisualStyleBackColor = true;
            this.rbWrong.CheckedChanged += new System.EventHandler(this.rbWrong_CheckedChanged);
            // 
            // rbCorrect
            // 
            this.rbCorrect.AutoSize = true;
            this.rbCorrect.Location = new System.Drawing.Point(158, 32);
            this.rbCorrect.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbCorrect.Name = "rbCorrect";
            this.rbCorrect.Size = new System.Drawing.Size(172, 27);
            this.rbCorrect.TabIndex = 1;
            this.rbCorrect.TabStop = true;
            this.rbCorrect.Text = "✅ 해결됨(정답)만";
            this.rbCorrect.UseVisualStyleBackColor = true;
            this.rbCorrect.CheckedChanged += new System.EventHandler(this.rbCorrect_CheckedChanged);
            // 
            // rbAll
            // 
            this.rbAll.AutoSize = true;
            this.rbAll.Checked = true;
            this.rbAll.Location = new System.Drawing.Point(15, 32);
            this.rbAll.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbAll.Name = "rbAll";
            this.rbAll.Size = new System.Drawing.Size(105, 27);
            this.rbAll.TabIndex = 0;
            this.rbAll.TabStop = true;
            this.rbAll.Text = "전체 보기";
            this.rbAll.UseVisualStyleBackColor = true;
            this.rbAll.CheckedChanged += new System.EventHandler(this.rbAll_CheckedChanged);
            // 
            // groupBox10
            // 
            this.groupBox10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox10.Controls.Add(this.btnExportWrongList);
            this.groupBox10.Controls.Add(this.lblWrongProbTags);
            this.groupBox10.Controls.Add(this.lblWrongProbDiff);
            this.groupBox10.Controls.Add(this.lblWrongProbTitle);
            this.groupBox10.Controls.Add(this.lblWrongProbNum);
            this.groupBox10.Controls.Add(this.label12);
            this.groupBox10.Controls.Add(this.label13);
            this.groupBox10.Controls.Add(this.label14);
            this.groupBox10.Controls.Add(this.label15);
            this.groupBox10.Controls.Add(this.lblWrongProbResult);
            this.groupBox10.Controls.Add(this.btnSolveAgain);
            this.groupBox10.Controls.Add(this.btnViewWrongProblem);
            this.groupBox10.Controls.Add(this.label28);
            this.groupBox10.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox10.Location = new System.Drawing.Point(843, 10);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(246, 564);
            this.groupBox10.TabIndex = 1;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "선택된 문제 정보";
            // 
            // btnExportWrongList
            // 
            this.btnExportWrongList.AutoSize = true;
            this.btnExportWrongList.BackColor = System.Drawing.Color.White;
            this.btnExportWrongList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportWrongList.Location = new System.Drawing.Point(26, 509);
            this.btnExportWrongList.Name = "btnExportWrongList";
            this.btnExportWrongList.Size = new System.Drawing.Size(196, 39);
            this.btnExportWrongList.TabIndex = 29;
            this.btnExportWrongList.Text = "📄 요약본 내보내기";
            this.btnExportWrongList.UseVisualStyleBackColor = false;
            this.btnExportWrongList.Click += new System.EventHandler(this.btnExportWrongList_Click);
            // 
            // lblWrongProbTags
            // 
            this.lblWrongProbTags.AutoSize = true;
            this.lblWrongProbTags.Location = new System.Drawing.Point(84, 242);
            this.lblWrongProbTags.MaximumSize = new System.Drawing.Size(145, 0);
            this.lblWrongProbTags.Name = "lblWrongProbTags";
            this.lblWrongProbTags.Size = new System.Drawing.Size(0, 23);
            this.lblWrongProbTags.TabIndex = 28;
            // 
            // lblWrongProbDiff
            // 
            this.lblWrongProbDiff.AutoSize = true;
            this.lblWrongProbDiff.Location = new System.Drawing.Point(84, 203);
            this.lblWrongProbDiff.MaximumSize = new System.Drawing.Size(145, 0);
            this.lblWrongProbDiff.Name = "lblWrongProbDiff";
            this.lblWrongProbDiff.Size = new System.Drawing.Size(0, 23);
            this.lblWrongProbDiff.TabIndex = 27;
            // 
            // lblWrongProbTitle
            // 
            this.lblWrongProbTitle.AutoSize = true;
            this.lblWrongProbTitle.Location = new System.Drawing.Point(84, 74);
            this.lblWrongProbTitle.MaximumSize = new System.Drawing.Size(145, 0);
            this.lblWrongProbTitle.Name = "lblWrongProbTitle";
            this.lblWrongProbTitle.Size = new System.Drawing.Size(0, 23);
            this.lblWrongProbTitle.TabIndex = 26;
            // 
            // lblWrongProbNum
            // 
            this.lblWrongProbNum.AutoSize = true;
            this.lblWrongProbNum.Location = new System.Drawing.Point(84, 36);
            this.lblWrongProbNum.MaximumSize = new System.Drawing.Size(145, 0);
            this.lblWrongProbNum.Name = "lblWrongProbNum";
            this.lblWrongProbNum.Size = new System.Drawing.Size(0, 23);
            this.lblWrongProbNum.TabIndex = 25;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(11, 242);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(44, 23);
            this.label12.TabIndex = 24;
            this.label12.Text = "태그";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(11, 203);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(61, 23);
            this.label13.TabIndex = 23;
            this.label13.Text = "난이도";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(10, 74);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(44, 23);
            this.label14.TabIndex = 22;
            this.label14.Text = "제목";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(10, 36);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(44, 23);
            this.label15.TabIndex = 21;
            this.label15.Text = "번호";
            // 
            // lblWrongProbResult
            // 
            this.lblWrongProbResult.AutoSize = true;
            this.lblWrongProbResult.Location = new System.Drawing.Point(84, 427);
            this.lblWrongProbResult.MaximumSize = new System.Drawing.Size(145, 0);
            this.lblWrongProbResult.Name = "lblWrongProbResult";
            this.lblWrongProbResult.Size = new System.Drawing.Size(0, 23);
            this.lblWrongProbResult.TabIndex = 20;
            // 
            // btnSolveAgain
            // 
            this.btnSolveAgain.AutoSize = true;
            this.btnSolveAgain.BackColor = System.Drawing.Color.White;
            this.btnSolveAgain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSolveAgain.Location = new System.Drawing.Point(127, 464);
            this.btnSolveAgain.Name = "btnSolveAgain";
            this.btnSolveAgain.Size = new System.Drawing.Size(111, 39);
            this.btnSolveAgain.TabIndex = 6;
            this.btnSolveAgain.Text = "다시 풀기";
            this.btnSolveAgain.UseVisualStyleBackColor = false;
            this.btnSolveAgain.Click += new System.EventHandler(this.btnSolveAgain_Click);
            // 
            // btnViewWrongProblem
            // 
            this.btnViewWrongProblem.AutoSize = true;
            this.btnViewWrongProblem.BackColor = System.Drawing.Color.White;
            this.btnViewWrongProblem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewWrongProblem.Location = new System.Drawing.Point(9, 464);
            this.btnViewWrongProblem.Name = "btnViewWrongProblem";
            this.btnViewWrongProblem.Size = new System.Drawing.Size(111, 39);
            this.btnViewWrongProblem.TabIndex = 5;
            this.btnViewWrongProblem.Text = "문제 보기";
            this.btnViewWrongProblem.UseVisualStyleBackColor = false;
            this.btnViewWrongProblem.Click += new System.EventHandler(this.btnViewWrongProblem_Click);
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(11, 427);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(44, 23);
            this.label28.TabIndex = 3;
            this.label28.Text = "결과";
            // 
            // groupBox9
            // 
            this.groupBox9.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox9.Controls.Add(this.dgvWrongList);
            this.groupBox9.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox9.Location = new System.Drawing.Point(13, 87);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(813, 487);
            this.groupBox9.TabIndex = 0;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "전체 학습 내역";
            // 
            // dgvWrongList
            // 
            this.dgvWrongList.AllowUserToAddRows = false;
            this.dgvWrongList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvWrongList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvWrongList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWrongList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ProblemId1,
            this.Title1,
            this.Rating1,
            this.Tags1,
            this.Result1,
            this.Date1});
            this.dgvWrongList.Location = new System.Drawing.Point(15, 29);
            this.dgvWrongList.Name = "dgvWrongList";
            this.dgvWrongList.ReadOnly = true;
            this.dgvWrongList.RowHeadersWidth = 51;
            this.dgvWrongList.RowTemplate.Height = 27;
            this.dgvWrongList.Size = new System.Drawing.Size(786, 444);
            this.dgvWrongList.TabIndex = 0;
            this.dgvWrongList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvWrongList_CellClick);
            // 
            // ProblemId1
            // 
            this.ProblemId1.HeaderText = "번호";
            this.ProblemId1.MinimumWidth = 6;
            this.ProblemId1.Name = "ProblemId1";
            this.ProblemId1.ReadOnly = true;
            // 
            // Title1
            // 
            this.Title1.HeaderText = "제목";
            this.Title1.MinimumWidth = 6;
            this.Title1.Name = "Title1";
            this.Title1.ReadOnly = true;
            // 
            // Rating1
            // 
            this.Rating1.HeaderText = "난이도";
            this.Rating1.MinimumWidth = 6;
            this.Rating1.Name = "Rating1";
            this.Rating1.ReadOnly = true;
            // 
            // Tags1
            // 
            this.Tags1.HeaderText = "태그";
            this.Tags1.MinimumWidth = 6;
            this.Tags1.Name = "Tags1";
            this.Tags1.ReadOnly = true;
            // 
            // Result1
            // 
            this.Result1.HeaderText = "결과";
            this.Result1.MinimumWidth = 6;
            this.Result1.Name = "Result1";
            this.Result1.ReadOnly = true;
            // 
            // Date1
            // 
            this.Date1.HeaderText = "날짜";
            this.Date1.MinimumWidth = 6;
            this.Date1.Name = "Date1";
            this.Date1.ReadOnly = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableLayoutPanel1);
            this.tabPage2.Controls.Add(this.groupBox7);
            this.tabPage2.Controls.Add(this.groupBox5);
            this.tabPage2.Location = new System.Drawing.Point(4, 35);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage2.Size = new System.Drawing.Size(1103, 582);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "코드 작성";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 1075F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox8, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.groupBox6, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(14, 186);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1075, 385);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.txtResult);
            this.groupBox8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox8.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox8.Location = new System.Drawing.Point(3, 313);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(1069, 69);
            this.groupBox8.TabIndex = 10;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "실행 결과";
            // 
            // txtResult
            // 
            this.txtResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtResult.Location = new System.Drawing.Point(13, 29);
            this.txtResult.Name = "txtResult";
            this.txtResult.Size = new System.Drawing.Size(1043, 28);
            this.txtResult.TabIndex = 1;
            this.txtResult.Text = "";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.txtCode);
            this.groupBox6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox6.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox6.Location = new System.Drawing.Point(3, 3);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(1069, 294);
            this.groupBox6.TabIndex = 7;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "코드 작성";
            // 
            // txtCode
            // 
            this.txtCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCode.Location = new System.Drawing.Point(13, 29);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(1039, 250);
            this.txtCode.TabIndex = 8;
            this.txtCode.Text = "";
            this.txtCode.Click += new System.EventHandler(this.txtCode_TextChanged);
            // 
            // groupBox7
            // 
            this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox7.Controls.Add(this.btnRetryTranslate);
            this.groupBox7.Controls.Add(this.btnResetCode);
            this.groupBox7.Controls.Add(this.btnSubmitCF);
            this.groupBox7.Controls.Add(this.btnRunSample);
            this.groupBox7.Controls.Add(this.cbLanguage);
            this.groupBox7.Controls.Add(this.label20);
            this.groupBox7.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox7.Location = new System.Drawing.Point(14, 90);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(1075, 86);
            this.groupBox7.TabIndex = 2;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "실행 영역";
            // 
            // btnRetryTranslate
            // 
            this.btnRetryTranslate.BackColor = System.Drawing.Color.White;
            this.btnRetryTranslate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRetryTranslate.Location = new System.Drawing.Point(695, 33);
            this.btnRetryTranslate.Name = "btnRetryTranslate";
            this.btnRetryTranslate.Size = new System.Drawing.Size(113, 39);
            this.btnRetryTranslate.TabIndex = 5;
            this.btnRetryTranslate.Text = "다시 번역";
            this.btnRetryTranslate.UseVisualStyleBackColor = false;
            this.btnRetryTranslate.Click += new System.EventHandler(this.btnRetryTranslate_Click);
            // 
            // btnResetCode
            // 
            this.btnResetCode.BackColor = System.Drawing.Color.White;
            this.btnResetCode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResetCode.Location = new System.Drawing.Point(575, 33);
            this.btnResetCode.Name = "btnResetCode";
            this.btnResetCode.Size = new System.Drawing.Size(94, 39);
            this.btnResetCode.TabIndex = 4;
            this.btnResetCode.Text = "초기화";
            this.btnResetCode.UseVisualStyleBackColor = false;
            this.btnResetCode.Click += new System.EventHandler(this.btnResetCode_Click);
            // 
            // btnSubmitCF
            // 
            this.btnSubmitCF.BackColor = System.Drawing.Color.White;
            this.btnSubmitCF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSubmitCF.Location = new System.Drawing.Point(457, 33);
            this.btnSubmitCF.Name = "btnSubmitCF";
            this.btnSubmitCF.Size = new System.Drawing.Size(93, 39);
            this.btnSubmitCF.TabIndex = 3;
            this.btnSubmitCF.Text = "CF 제출";
            this.btnSubmitCF.UseVisualStyleBackColor = false;
            this.btnSubmitCF.Click += new System.EventHandler(this.btnSubmitCF_Click);
            // 
            // btnRunSample
            // 
            this.btnRunSample.BackColor = System.Drawing.Color.White;
            this.btnRunSample.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRunSample.Location = new System.Drawing.Point(273, 33);
            this.btnRunSample.Name = "btnRunSample";
            this.btnRunSample.Size = new System.Drawing.Size(161, 39);
            this.btnRunSample.TabIndex = 2;
            this.btnRunSample.Text = "예제 테스트 실행";
            this.btnRunSample.UseVisualStyleBackColor = false;
            this.btnRunSample.Click += new System.EventHandler(this.btnRunSample_Click);
            // 
            // cbLanguage
            // 
            this.cbLanguage.FormattingEnabled = true;
            this.cbLanguage.Location = new System.Drawing.Point(87, 38);
            this.cbLanguage.Name = "cbLanguage";
            this.cbLanguage.Size = new System.Drawing.Size(137, 31);
            this.cbLanguage.TabIndex = 1;
            this.cbLanguage.SelectedIndexChanged += new System.EventHandler(this.cbLanguage_SelectedIndexChanged);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(22, 38);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(56, 23);
            this.label20.TabIndex = 0;
            this.label20.Text = "언어";
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.lblCodeProbTags);
            this.groupBox5.Controls.Add(this.lblCodeProbDiff);
            this.groupBox5.Controls.Add(this.lblCodeProbTitle);
            this.groupBox5.Controls.Add(this.lblCodeProbNum);
            this.groupBox5.Controls.Add(this.label19);
            this.groupBox5.Controls.Add(this.label18);
            this.groupBox5.Controls.Add(this.label17);
            this.groupBox5.Controls.Add(this.label16);
            this.groupBox5.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox5.Location = new System.Drawing.Point(14, 9);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(1075, 70);
            this.groupBox5.TabIndex = 0;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "선택된 문제 정보";
            // 
            // lblCodeProbTags
            // 
            this.lblCodeProbTags.AutoSize = true;
            this.lblCodeProbTags.Location = new System.Drawing.Point(859, 32);
            this.lblCodeProbTags.Name = "lblCodeProbTags";
            this.lblCodeProbTags.Size = new System.Drawing.Size(0, 23);
            this.lblCodeProbTags.TabIndex = 7;
            // 
            // lblCodeProbDiff
            // 
            this.lblCodeProbDiff.AutoSize = true;
            this.lblCodeProbDiff.Location = new System.Drawing.Point(664, 32);
            this.lblCodeProbDiff.Name = "lblCodeProbDiff";
            this.lblCodeProbDiff.Size = new System.Drawing.Size(0, 23);
            this.lblCodeProbDiff.TabIndex = 6;
            // 
            // lblCodeProbTitle
            // 
            this.lblCodeProbTitle.AutoSize = true;
            this.lblCodeProbTitle.Location = new System.Drawing.Point(238, 32);
            this.lblCodeProbTitle.Name = "lblCodeProbTitle";
            this.lblCodeProbTitle.Size = new System.Drawing.Size(0, 23);
            this.lblCodeProbTitle.TabIndex = 5;
            // 
            // lblCodeProbNum
            // 
            this.lblCodeProbNum.AutoSize = true;
            this.lblCodeProbNum.Location = new System.Drawing.Point(62, 32);
            this.lblCodeProbNum.Name = "lblCodeProbNum";
            this.lblCodeProbNum.Size = new System.Drawing.Size(0, 23);
            this.lblCodeProbNum.TabIndex = 4;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(809, 32);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(44, 23);
            this.label19.TabIndex = 3;
            this.label19.Text = "태그";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(597, 32);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(61, 23);
            this.label18.TabIndex = 2;
            this.label18.Text = "난이도";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(188, 32);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(44, 23);
            this.label17.TabIndex = 1;
            this.label17.Text = "제목";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(12, 32);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(44, 23);
            this.label16.TabIndex = 0;
            this.label16.Text = "번호";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Location = new System.Drawing.Point(4, 35);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage1.Size = new System.Drawing.Size(1103, 582);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "문제 탐색";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.lblDarkMode);
            this.groupBox3.Controls.Add(this.lblSelProbTags);
            this.groupBox3.Controls.Add(this.lblSelProbDiff);
            this.groupBox3.Controls.Add(this.lblSelProbTitle);
            this.groupBox3.Controls.Add(this.lblSelProbNum);
            this.groupBox3.Controls.Add(this.btnViewProblem);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox3.Location = new System.Drawing.Point(874, 10);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(215, 564);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "선택된 문제";
            // 
            // lblDarkMode
            // 
            this.lblDarkMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDarkMode.AutoSize = true;
            this.lblDarkMode.Font = new System.Drawing.Font("맑은 고딕", 9.857143F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblDarkMode.Location = new System.Drawing.Point(100, -11);
            this.lblDarkMode.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDarkMode.Name = "lblDarkMode";
            this.lblDarkMode.Size = new System.Drawing.Size(91, 23);
            this.lblDarkMode.TabIndex = 3;
            this.lblDarkMode.Text = "🌙 다크 모드";
            this.lblDarkMode.Click += new System.EventHandler(this.lblDarkMode_Click);
            // 
            // lblSelProbTags
            // 
            this.lblSelProbTags.AutoSize = true;
            this.lblSelProbTags.Location = new System.Drawing.Point(86, 273);
            this.lblSelProbTags.MaximumSize = new System.Drawing.Size(110, 0);
            this.lblSelProbTags.Name = "lblSelProbTags";
            this.lblSelProbTags.Size = new System.Drawing.Size(0, 23);
            this.lblSelProbTags.TabIndex = 8;
            // 
            // lblSelProbDiff
            // 
            this.lblSelProbDiff.AutoSize = true;
            this.lblSelProbDiff.Location = new System.Drawing.Point(86, 234);
            this.lblSelProbDiff.MaximumSize = new System.Drawing.Size(110, 0);
            this.lblSelProbDiff.Name = "lblSelProbDiff";
            this.lblSelProbDiff.Size = new System.Drawing.Size(0, 23);
            this.lblSelProbDiff.TabIndex = 7;
            // 
            // lblSelProbTitle
            // 
            this.lblSelProbTitle.AutoSize = true;
            this.lblSelProbTitle.Location = new System.Drawing.Point(86, 75);
            this.lblSelProbTitle.MaximumSize = new System.Drawing.Size(110, 0);
            this.lblSelProbTitle.Name = "lblSelProbTitle";
            this.lblSelProbTitle.Size = new System.Drawing.Size(0, 23);
            this.lblSelProbTitle.TabIndex = 6;
            // 
            // lblSelProbNum
            // 
            this.lblSelProbNum.AutoSize = true;
            this.lblSelProbNum.Location = new System.Drawing.Point(86, 37);
            this.lblSelProbNum.MaximumSize = new System.Drawing.Size(110, 0);
            this.lblSelProbNum.Name = "lblSelProbNum";
            this.lblSelProbNum.Size = new System.Drawing.Size(0, 23);
            this.lblSelProbNum.TabIndex = 5;
            // 
            // btnViewProblem
            // 
            this.btnViewProblem.AutoSize = true;
            this.btnViewProblem.BackColor = System.Drawing.Color.White;
            this.btnViewProblem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewProblem.Location = new System.Drawing.Point(12, 503);
            this.btnViewProblem.Name = "btnViewProblem";
            this.btnViewProblem.Size = new System.Drawing.Size(193, 44);
            this.btnViewProblem.TabIndex = 4;
            this.btnViewProblem.Text = "문제 보기";
            this.btnViewProblem.UseVisualStyleBackColor = false;
            this.btnViewProblem.Click += new System.EventHandler(this.btnViewProblem_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 273);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(44, 23);
            this.label8.TabIndex = 3;
            this.label8.Text = "태그";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 234);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(61, 23);
            this.label7.TabIndex = 2;
            this.label7.Text = "난이도";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 75);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 23);
            this.label6.TabIndex = 1;
            this.label6.Text = "제목";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 37);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 23);
            this.label5.TabIndex = 0;
            this.label5.Text = "번호";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnSearchAll);
            this.groupBox1.Controls.Add(this.btnResetSearch);
            this.groupBox1.Controls.Add(this.btnSearch);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtKeyword);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtMaxDifficulty);
            this.groupBox1.Controls.Add(this.txtMinDifficulty);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox1.Location = new System.Drawing.Point(15, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(844, 91);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "검색 조건";
            // 
            // btnSearchAll
            // 
            this.btnSearchAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.btnSearchAll.BackColor = System.Drawing.Color.White;
            this.btnSearchAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchAll.Location = new System.Drawing.Point(748, 37);
            this.btnSearchAll.Name = "btnSearchAll";
            this.btnSearchAll.Size = new System.Drawing.Size(80, 30);
            this.btnSearchAll.TabIndex = 5;
            this.btnSearchAll.Text = "전체";
            this.btnSearchAll.UseCompatibleTextRendering = true;
            this.btnSearchAll.UseVisualStyleBackColor = false;
            this.btnSearchAll.Click += new System.EventHandler(this.btnSearchAll_Click);
            // 
            // btnResetSearch
            // 
            this.btnResetSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.btnResetSearch.BackColor = System.Drawing.Color.White;
            this.btnResetSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResetSearch.Location = new System.Drawing.Point(651, 37);
            this.btnResetSearch.Name = "btnResetSearch";
            this.btnResetSearch.Size = new System.Drawing.Size(80, 30);
            this.btnResetSearch.TabIndex = 4;
            this.btnResetSearch.Text = "초기화";
            this.btnResetSearch.UseCompatibleTextRendering = true;
            this.btnResetSearch.UseVisualStyleBackColor = false;
            this.btnResetSearch.Click += new System.EventHandler(this.btnResetSearch_Click);
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.btnSearch.BackColor = System.Drawing.Color.White;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Location = new System.Drawing.Point(554, 37);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(80, 30);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "검색";
            this.btnSearch.UseCompatibleTextRendering = true;
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(186, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(22, 23);
            this.label2.TabIndex = 3;
            this.label2.Text = "~";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtKeyword
            // 
            this.txtKeyword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.txtKeyword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtKeyword.Location = new System.Drawing.Point(411, 37);
            this.txtKeyword.Name = "txtKeyword";
            this.txtKeyword.Size = new System.Drawing.Size(101, 30);
            this.txtKeyword.TabIndex = 0;
            this.txtKeyword.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(344, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 23);
            this.label3.TabIndex = 1;
            this.label3.Text = "키워드";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtMaxDifficulty
            // 
            this.txtMaxDifficulty.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.txtMaxDifficulty.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMaxDifficulty.Location = new System.Drawing.Point(211, 37);
            this.txtMaxDifficulty.Name = "txtMaxDifficulty";
            this.txtMaxDifficulty.Size = new System.Drawing.Size(101, 30);
            this.txtMaxDifficulty.TabIndex = 0;
            this.txtMaxDifficulty.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtMinDifficulty
            // 
            this.txtMinDifficulty.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.txtMinDifficulty.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMinDifficulty.Location = new System.Drawing.Point(79, 37);
            this.txtMinDifficulty.Name = "txtMinDifficulty";
            this.txtMinDifficulty.Size = new System.Drawing.Size(101, 30);
            this.txtMinDifficulty.TabIndex = 0;
            this.txtMinDifficulty.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "난이도";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.dgvProblems);
            this.groupBox2.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox2.Location = new System.Drawing.Point(15, 107);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(844, 467);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "문제 목록";
            // 
            // dgvProblems
            // 
            this.dgvProblems.AllowUserToAddRows = false;
            this.dgvProblems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvProblems.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProblems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProblems.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ProblemId,
            this.Title,
            this.Rating,
            this.Tags,
            this.Result});
            this.dgvProblems.Location = new System.Drawing.Point(16, 34);
            this.dgvProblems.Name = "dgvProblems";
            this.dgvProblems.ReadOnly = true;
            this.dgvProblems.RowHeadersWidth = 51;
            this.dgvProblems.RowTemplate.Height = 27;
            this.dgvProblems.Size = new System.Drawing.Size(812, 418);
            this.dgvProblems.TabIndex = 1;
            this.dgvProblems.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProblems_CellClick);
            // 
            // ProblemId
            // 
            this.ProblemId.HeaderText = "번호";
            this.ProblemId.MinimumWidth = 6;
            this.ProblemId.Name = "ProblemId";
            this.ProblemId.ReadOnly = true;
            // 
            // Title
            // 
            this.Title.HeaderText = "제목";
            this.Title.MinimumWidth = 6;
            this.Title.Name = "Title";
            this.Title.ReadOnly = true;
            // 
            // Rating
            // 
            this.Rating.HeaderText = "난이도";
            this.Rating.MinimumWidth = 6;
            this.Rating.Name = "Rating";
            this.Rating.ReadOnly = true;
            // 
            // Tags
            // 
            this.Tags.HeaderText = "태그";
            this.Tags.MinimumWidth = 6;
            this.Tags.Name = "Tags";
            this.Tags.ReadOnly = true;
            // 
            // Result
            // 
            this.Result.HeaderText = "결과";
            this.Result.MinimumWidth = 6;
            this.Result.Name = "Result";
            this.Result.ReadOnly = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.Buttons;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Cursor = System.Windows.Forms.Cursors.Default;
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1111, 621);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabControl1_DrawItem);
            this.tabControl1.Click += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1111, 650);
            this.Controls.Add(this.lblGitHubPush);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.Text = "코딩 학습 프로그램";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Click += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.groupBox14.ResumeLayout(false);
            this.groupBox14.PerformLayout();
            this.groupBox15.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTimeRecords)).EndInit();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartTimeHistory)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.groupBox13.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecentRecords)).EndInit();
            this.groupBox12.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartAccuracy)).EndInit();
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.groupBox16.ResumeLayout(false);
            this.groupBox16.PerformLayout();
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.groupBox9.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvWrongList)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProblems)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblTimer;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Timer learningTimer;
        private System.Windows.Forms.ToolStripStatusLabel btnStopSession;
        private System.Windows.Forms.Label lblGitHubPush;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.GroupBox groupBox14;
        private System.Windows.Forms.Label lblMaxFocus;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblWeeklyAvg;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label lblTodayTotal;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.GroupBox groupBox15;
        private System.Windows.Forms.DataGridView dgvTimeRecords;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartTimeHistory;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.GroupBox groupBox13;
        private System.Windows.Forms.DataGridView dgvRecentRecords;
        private System.Windows.Forms.DataGridViewTextBoxColumn ProblemId2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Title2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Result2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date2;
        private System.Windows.Forms.GroupBox groupBox12;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartAccuracy;
        private System.Windows.Forms.GroupBox groupBox11;
        private System.Windows.Forms.Label lblAccuracy;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.Label lblWrong;
        private System.Windows.Forms.Label label39;
        private System.Windows.Forms.Label lblCorrect;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.Label lblTotalSolved;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.Button btnExportWrongList;
        private System.Windows.Forms.Label lblWrongProbTags;
        private System.Windows.Forms.Label lblWrongProbDiff;
        private System.Windows.Forms.Label lblWrongProbTitle;
        private System.Windows.Forms.Label lblWrongProbNum;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label lblWrongProbResult;
        private System.Windows.Forms.Button btnSolveAgain;
        private System.Windows.Forms.Button btnViewWrongProblem;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.DataGridView dgvWrongList;
        private System.Windows.Forms.DataGridViewTextBoxColumn ProblemId1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Title1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Rating1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tags1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Result1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.RichTextBox txtResult;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.RichTextBox txtCode;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Button btnRetryTranslate;
        private System.Windows.Forms.Button btnResetCode;
        private System.Windows.Forms.Button btnSubmitCF;
        private System.Windows.Forms.Button btnRunSample;
        private System.Windows.Forms.ComboBox cbLanguage;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label lblCodeProbTags;
        private System.Windows.Forms.Label lblCodeProbDiff;
        private System.Windows.Forms.Label lblCodeProbTitle;
        private System.Windows.Forms.Label lblCodeProbNum;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblDarkMode;
        private System.Windows.Forms.Label lblSelProbTags;
        private System.Windows.Forms.Label lblSelProbDiff;
        private System.Windows.Forms.Label lblSelProbTitle;
        private System.Windows.Forms.Label lblSelProbNum;
        private System.Windows.Forms.Button btnViewProblem;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSearchAll;
        private System.Windows.Forms.Button btnResetSearch;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtKeyword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMaxDifficulty;
        private System.Windows.Forms.TextBox txtMinDifficulty;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgvProblems;
        private System.Windows.Forms.DataGridViewTextBoxColumn ProblemId;
        private System.Windows.Forms.DataGridViewTextBoxColumn Title;
        private System.Windows.Forms.DataGridViewTextBoxColumn Rating;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tags;
        private System.Windows.Forms.DataGridViewTextBoxColumn Result;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.GroupBox groupBox16;
        private System.Windows.Forms.RadioButton rbAll;
        private System.Windows.Forms.RadioButton rbWrong;
        private System.Windows.Forms.RadioButton rbCorrect;
    }
}

