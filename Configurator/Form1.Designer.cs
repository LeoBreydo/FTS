namespace Configurator
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.pageSettings = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.plotterPanel = new System.Windows.Forms.Panel();
            this.gbPropGridTitle = new System.Windows.Forms.GroupBox();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.panelTrades = new System.Windows.Forms.Panel();
            this.gridControl2 = new DevExpress.XtraGrid.GridControl();
            this.gridView2 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.pageSchedule = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.gridControl3 = new DevExpress.XtraGrid.GridControl();
            this.gridView3 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tbtnAddScheduleRec = new System.Windows.Forms.ToolStripButton();
            this.tbtnCloneScheduleRec = new System.Windows.Forms.ToolStripButton();
            this.tbtnEditScheduleRec = new System.Windows.Forms.ToolStripButton();
            this.tbtnDelScheduleRec = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tbtnCleanObsoleteScheduleRecs = new System.Windows.Forms.ToolStripButton();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imagesTrees = new System.Windows.Forms.ImageList(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.cbOnlyWorkingSet = new System.Windows.Forms.CheckBox();
            this.rbStrategies = new System.Windows.Forms.RadioButton();
            this.rbMarkets = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnUpload = new System.Windows.Forms.Button();
            this.btnVerify = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnRefreshStrategies = new System.Windows.Forms.Button();
            this.rbExchanges = new System.Windows.Forms.RadioButton();
            this.cbFlatList = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.configurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verifyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel2 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.pageSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.plotterPanel.SuspendLayout();
            this.gbPropGridTitle.SuspendLayout();
            this.panelTrades.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).BeginInit();
            this.pageSchedule.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView3)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.gridControl1);
            this.splitContainer1.Panel1.Controls.Add(this.splitter1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(1140, 617);
            this.splitContainer1.SplitterDistance = 584;
            this.splitContainer1.TabIndex = 1;
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(4);
            this.gridControl1.Location = new System.Drawing.Point(0, 0);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(4);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(584, 612);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 612);
            this.splitter1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(584, 5);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.pageSettings);
            this.tabControl1.Controls.Add(this.pageSchedule);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(552, 617);
            this.tabControl1.TabIndex = 1;
            // 
            // pageSettings
            // 
            this.pageSettings.BackColor = System.Drawing.SystemColors.Control;
            this.pageSettings.Controls.Add(this.splitContainer2);
            this.pageSettings.Location = new System.Drawing.Point(4, 25);
            this.pageSettings.Name = "pageSettings";
            this.pageSettings.Padding = new System.Windows.Forms.Padding(3);
            this.pageSettings.Size = new System.Drawing.Size(544, 588);
            this.pageSettings.TabIndex = 0;
            this.pageSettings.Text = "Settings";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.plotterPanel);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.panelTrades);
            this.splitContainer2.Size = new System.Drawing.Size(538, 582);
            this.splitContainer2.SplitterDistance = 331;
            this.splitContainer2.TabIndex = 0;
            // 
            // plotterPanel
            // 
            this.plotterPanel.Controls.Add(this.gbPropGridTitle);
            this.plotterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plotterPanel.Location = new System.Drawing.Point(0, 0);
            this.plotterPanel.Margin = new System.Windows.Forms.Padding(4);
            this.plotterPanel.Name = "plotterPanel";
            this.plotterPanel.Size = new System.Drawing.Size(538, 331);
            this.plotterPanel.TabIndex = 10;
            // 
            // gbPropGridTitle
            // 
            this.gbPropGridTitle.Controls.Add(this.propertyGrid1);
            this.gbPropGridTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbPropGridTitle.Location = new System.Drawing.Point(0, 0);
            this.gbPropGridTitle.Name = "gbPropGridTitle";
            this.gbPropGridTitle.Size = new System.Drawing.Size(538, 331);
            this.gbPropGridTitle.TabIndex = 1;
            this.gbPropGridTitle.TabStop = false;
            this.gbPropGridTitle.Text = "Selected items";
            this.gbPropGridTitle.TextChanged += new System.EventHandler(this.gbPropGridTitle_TextChanged);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(3, 19);
            this.propertyGrid1.Margin = new System.Windows.Forms.Padding(4);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(532, 309);
            this.propertyGrid1.TabIndex = 0;
            this.propertyGrid1.ToolbarVisible = false;
            // 
            // panelTrades
            // 
            this.panelTrades.Controls.Add(this.gridControl2);
            this.panelTrades.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTrades.Location = new System.Drawing.Point(0, 0);
            this.panelTrades.Margin = new System.Windows.Forms.Padding(4);
            this.panelTrades.Name = "panelTrades";
            this.panelTrades.Size = new System.Drawing.Size(538, 247);
            this.panelTrades.TabIndex = 8;
            // 
            // gridControl2
            // 
            this.gridControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl2.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(4);
            this.gridControl2.Location = new System.Drawing.Point(0, 0);
            this.gridControl2.MainView = this.gridView2;
            this.gridControl2.Margin = new System.Windows.Forms.Padding(4);
            this.gridControl2.Name = "gridControl2";
            this.gridControl2.Size = new System.Drawing.Size(538, 247);
            this.gridControl2.TabIndex = 6;
            this.gridControl2.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView2});
            // 
            // gridView2
            // 
            this.gridView2.GridControl = this.gridControl2;
            this.gridView2.Name = "gridView2";
            // 
            // pageSchedule
            // 
            this.pageSchedule.BackColor = System.Drawing.SystemColors.Control;
            this.pageSchedule.Controls.Add(this.groupBox1);
            this.pageSchedule.Location = new System.Drawing.Point(4, 22);
            this.pageSchedule.Name = "pageSchedule";
            this.pageSchedule.Padding = new System.Windows.Forms.Padding(3);
            this.pageSchedule.Size = new System.Drawing.Size(544, 591);
            this.pageSchedule.TabIndex = 1;
            this.pageSchedule.Text = "Schedule";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.gridControl3);
            this.groupBox1.Controls.Add(this.toolStrip1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(538, 585);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Selected Item";
            // 
            // gridControl3
            // 
            this.gridControl3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl3.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(4);
            this.gridControl3.Location = new System.Drawing.Point(3, 45);
            this.gridControl3.MainView = this.gridView3;
            this.gridControl3.Margin = new System.Windows.Forms.Padding(4);
            this.gridControl3.Name = "gridControl3";
            this.gridControl3.Size = new System.Drawing.Size(532, 537);
            this.gridControl3.TabIndex = 6;
            this.gridControl3.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView3});
            // 
            // gridView3
            // 
            this.gridView3.GridControl = this.gridControl3;
            this.gridView3.Name = "gridView3";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbtnAddScheduleRec,
            this.tbtnCloneScheduleRec,
            this.tbtnEditScheduleRec,
            this.tbtnDelScheduleRec,
            this.toolStripSeparator1,
            this.tbtnCleanObsoleteScheduleRecs});
            this.toolStrip1.Location = new System.Drawing.Point(3, 19);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(532, 26);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tbtnAddScheduleRec
            // 
            this.tbtnAddScheduleRec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tbtnAddScheduleRec.Image = ((System.Drawing.Image)(resources.GetObject("tbtnAddScheduleRec.Image")));
            this.tbtnAddScheduleRec.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnAddScheduleRec.Name = "tbtnAddScheduleRec";
            this.tbtnAddScheduleRec.Size = new System.Drawing.Size(38, 23);
            this.tbtnAddScheduleRec.Text = "Add";
            this.tbtnAddScheduleRec.Click += new System.EventHandler(this.tbtnAddScheduleRec_Click);
            // 
            // tbtnCloneScheduleRec
            // 
            this.tbtnCloneScheduleRec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tbtnCloneScheduleRec.Image = ((System.Drawing.Image)(resources.GetObject("tbtnCloneScheduleRec.Image")));
            this.tbtnCloneScheduleRec.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnCloneScheduleRec.Name = "tbtnCloneScheduleRec";
            this.tbtnCloneScheduleRec.Size = new System.Drawing.Size(48, 23);
            this.tbtnCloneScheduleRec.Text = "Clone";
            this.tbtnCloneScheduleRec.Click += new System.EventHandler(this.tbtnCloneScheduleRec_Click);
            // 
            // tbtnEditScheduleRec
            // 
            this.tbtnEditScheduleRec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tbtnEditScheduleRec.Image = ((System.Drawing.Image)(resources.GetObject("tbtnEditScheduleRec.Image")));
            this.tbtnEditScheduleRec.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnEditScheduleRec.Name = "tbtnEditScheduleRec";
            this.tbtnEditScheduleRec.Size = new System.Drawing.Size(36, 23);
            this.tbtnEditScheduleRec.Text = "Edit";
            this.tbtnEditScheduleRec.Click += new System.EventHandler(this.tbtnEditScheduleRec_Click);
            // 
            // tbtnDelScheduleRec
            // 
            this.tbtnDelScheduleRec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tbtnDelScheduleRec.Image = ((System.Drawing.Image)(resources.GetObject("tbtnDelScheduleRec.Image")));
            this.tbtnDelScheduleRec.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnDelScheduleRec.Name = "tbtnDelScheduleRec";
            this.tbtnDelScheduleRec.Size = new System.Drawing.Size(52, 23);
            this.tbtnDelScheduleRec.Text = "Delete";
            this.tbtnDelScheduleRec.Click += new System.EventHandler(this.tbtnDelScheduleRec_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 26);
            // 
            // tbtnCleanObsoleteScheduleRecs
            // 
            this.tbtnCleanObsoleteScheduleRecs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tbtnCleanObsoleteScheduleRecs.Image = ((System.Drawing.Image)(resources.GetObject("tbtnCleanObsoleteScheduleRecs.Image")));
            this.tbtnCleanObsoleteScheduleRecs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnCleanObsoleteScheduleRecs.Name = "tbtnCleanObsoleteScheduleRecs";
            this.tbtnCleanObsoleteScheduleRecs.Size = new System.Drawing.Size(111, 23);
            this.tbtnCleanObsoleteScheduleRecs.Text = "Clean Obsolette";
            this.tbtnCleanObsoleteScheduleRecs.Click += new System.EventHandler(this.tbtnCleanObsoleteScheduleRecs_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.White;
            this.imageList1.Images.SetKeyName(0, "checkOff.bmp");
            this.imageList1.Images.SetKeyName(1, "checkOn.bmp");
            this.imageList1.Images.SetKeyName(2, "candle.bmp");
            this.imageList1.Images.SetKeyName(3, "bar1.bmp");
            this.imageList1.Images.SetKeyName(4, "zoomIn.bmp");
            this.imageList1.Images.SetKeyName(5, "zoomOut.bmp");
            this.imageList1.Images.SetKeyName(6, "scrollToEnd.bmp");
            this.imageList1.Images.SetKeyName(7, "setup.bmp");
            this.imageList1.Images.SetKeyName(8, "zoomSz.bmp");
            this.imageList1.Images.SetKeyName(9, "showDetails.bmp");
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Variable";
            this.columnHeader1.Width = 193;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Value";
            this.columnHeader2.Width = 96;
            // 
            // imagesTrees
            // 
            this.imagesTrees.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imagesTrees.ImageStream")));
            this.imagesTrees.TransparentColor = System.Drawing.Color.White;
            this.imagesTrees.Images.SetKeyName(0, "folder.bmp");
            this.imagesTrees.Images.SetKeyName(1, "market.bmp");
            this.imagesTrees.Images.SetKeyName(2, "fx.bmp");
            this.imagesTrees.Images.SetKeyName(3, "fxGrayed.bmp");
            this.imagesTrees.Images.SetKeyName(4, "axis.bmp");
            this.imagesTrees.Images.SetKeyName(5, "strategy.bmp");
            this.imagesTrees.Images.SetKeyName(6, "portfol256.bmp");
            this.imagesTrees.Images.SetKeyName(7, "fund.bmp");
            this.imagesTrees.Images.SetKeyName(8, "fxGroup.bmp");
            this.imagesTrees.Images.SetKeyName(9, "checkOn.bmp");
            this.imagesTrees.Images.SetKeyName(10, "check3.bmp");
            this.imagesTrees.Images.SetKeyName(11, "checkOff.bmp");
            this.imagesTrees.Images.SetKeyName(12, "file.bmp");
            this.imagesTrees.Images.SetKeyName(13, "badfile.bmp");
            this.imagesTrees.Images.SetKeyName(14, "market_downloading.bmp");
            this.imagesTrees.Images.SetKeyName(15, "whilefolder.bmp");
            this.imagesTrees.Images.SetKeyName(16, "slantLine.bmp");
            this.imagesTrees.Images.SetKeyName(17, "searchUp.bmp");
            // 
            // cbOnlyWorkingSet
            // 
            this.cbOnlyWorkingSet.AutoSize = true;
            this.cbOnlyWorkingSet.Location = new System.Drawing.Point(451, 37);
            this.cbOnlyWorkingSet.Margin = new System.Windows.Forms.Padding(4);
            this.cbOnlyWorkingSet.Name = "cbOnlyWorkingSet";
            this.cbOnlyWorkingSet.Size = new System.Drawing.Size(133, 21);
            this.cbOnlyWorkingSet.TabIndex = 2;
            this.cbOnlyWorkingSet.Text = "Only WorkingSet";
            this.cbOnlyWorkingSet.UseVisualStyleBackColor = true;
            this.cbOnlyWorkingSet.CheckedChanged += new System.EventHandler(this.cbOnlyWorkingSet_CheckedChanged);
            // 
            // rbStrategies
            // 
            this.rbStrategies.AutoSize = true;
            this.rbStrategies.Location = new System.Drawing.Point(15, 37);
            this.rbStrategies.Margin = new System.Windows.Forms.Padding(4);
            this.rbStrategies.Name = "rbStrategies";
            this.rbStrategies.Size = new System.Drawing.Size(90, 21);
            this.rbStrategies.TabIndex = 3;
            this.rbStrategies.TabStop = true;
            this.rbStrategies.Text = "Strategies";
            this.rbStrategies.UseVisualStyleBackColor = true;
            this.rbStrategies.CheckedChanged += new System.EventHandler(this.rbStrategies_CheckedChanged);
            // 
            // rbMarkets
            // 
            this.rbMarkets.AutoSize = true;
            this.rbMarkets.Location = new System.Drawing.Point(118, 37);
            this.rbMarkets.Margin = new System.Windows.Forms.Padding(4);
            this.rbMarkets.Name = "rbMarkets";
            this.rbMarkets.Size = new System.Drawing.Size(76, 21);
            this.rbMarkets.TabIndex = 4;
            this.rbMarkets.TabStop = true;
            this.rbMarkets.Text = "Markets";
            this.rbMarkets.UseVisualStyleBackColor = true;
            this.rbMarkets.CheckedChanged += new System.EventHandler(this.rbMarkets_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnUpload);
            this.panel1.Controls.Add(this.btnVerify);
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Controls.Add(this.btnRefreshStrategies);
            this.panel1.Controls.Add(this.rbExchanges);
            this.panel1.Controls.Add(this.cbFlatList);
            this.panel1.Controls.Add(this.rbMarkets);
            this.panel1.Controls.Add(this.cbOnlyWorkingSet);
            this.panel1.Controls.Add(this.rbStrategies);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1140, 66);
            this.panel1.TabIndex = 5;
            // 
            // btnUpload
            // 
            this.btnUpload.Location = new System.Drawing.Point(184, 3);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(80, 28);
            this.btnUpload.TabIndex = 12;
            this.btnUpload.Text = "Upload";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // btnVerify
            // 
            this.btnVerify.Location = new System.Drawing.Point(12, 3);
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new System.Drawing.Size(80, 28);
            this.btnVerify.TabIndex = 11;
            this.btnVerify.Text = "Verify";
            this.btnVerify.UseVisualStyleBackColor = true;
            this.btnVerify.Click += new System.EventHandler(this.btnVerify_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(98, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 28);
            this.btnSave.TabIndex = 9;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnRefreshStrategies
            // 
            this.btnRefreshStrategies.Location = new System.Drawing.Point(321, 3);
            this.btnRefreshStrategies.Name = "btnRefreshStrategies";
            this.btnRefreshStrategies.Size = new System.Drawing.Size(158, 28);
            this.btnRefreshStrategies.TabIndex = 10;
            this.btnRefreshStrategies.Text = "Refresh strategies";
            this.btnRefreshStrategies.UseVisualStyleBackColor = true;
            this.btnRefreshStrategies.Click += new System.EventHandler(this.btnRefreshStrategies_Click);
            // 
            // rbExchanges
            // 
            this.rbExchanges.AutoSize = true;
            this.rbExchanges.Location = new System.Drawing.Point(207, 37);
            this.rbExchanges.Margin = new System.Windows.Forms.Padding(4);
            this.rbExchanges.Name = "rbExchanges";
            this.rbExchanges.Size = new System.Drawing.Size(95, 21);
            this.rbExchanges.TabIndex = 6;
            this.rbExchanges.TabStop = true;
            this.rbExchanges.Text = "Exchanges";
            this.rbExchanges.UseVisualStyleBackColor = true;
            this.rbExchanges.CheckedChanged += new System.EventHandler(this.rbExchanges_CheckedChanged);
            // 
            // cbFlatList
            // 
            this.cbFlatList.AutoSize = true;
            this.cbFlatList.Location = new System.Drawing.Point(321, 37);
            this.cbFlatList.Margin = new System.Windows.Forms.Padding(4);
            this.cbFlatList.Name = "cbFlatList";
            this.cbFlatList.Size = new System.Drawing.Size(96, 21);
            this.cbFlatList.TabIndex = 6;
            this.cbFlatList.Text = "As Flat List";
            this.cbFlatList.UseVisualStyleBackColor = true;
            this.cbFlatList.CheckedChanged += new System.EventHandler(this.cbFlatList_CheckedChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.configurationToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1140, 27);
            this.menuStrip1.TabIndex = 13;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.Visible = false;
            // 
            // configurationToolStripMenuItem
            // 
            this.configurationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.verifyToolStripMenuItem,
            this.uploadToolStripMenuItem});
            this.configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
            this.configurationToolStripMenuItem.Size = new System.Drawing.Size(105, 23);
            this.configurationToolStripMenuItem.Text = "Configuration";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(122, 24);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // verifyToolStripMenuItem
            // 
            this.verifyToolStripMenuItem.Name = "verifyToolStripMenuItem";
            this.verifyToolStripMenuItem.Size = new System.Drawing.Size(122, 24);
            this.verifyToolStripMenuItem.Text = "Verify";
            // 
            // uploadToolStripMenuItem
            // 
            this.uploadToolStripMenuItem.Name = "uploadToolStripMenuItem";
            this.uploadToolStripMenuItem.Size = new System.Drawing.Size(122, 24);
            this.uploadToolStripMenuItem.Text = "Upload";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.splitContainer1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 66);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1140, 617);
            this.panel2.TabIndex = 14;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1140, 683);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FTS Configurator";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.pageSettings.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.plotterPanel.ResumeLayout(false);
            this.gbPropGridTitle.ResumeLayout(false);
            this.panelTrades.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).EndInit();
            this.pageSchedule.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView3)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Splitter splitter1;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ImageList imagesTrees;
        private System.Windows.Forms.Panel panelTrades;
        private System.Windows.Forms.Panel plotterPanel;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox cbOnlyWorkingSet;
        private System.Windows.Forms.RadioButton rbStrategies;
        private System.Windows.Forms.RadioButton rbMarkets;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private DevExpress.XtraGrid.GridControl gridControl2;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton rbExchanges;
        private System.Windows.Forms.CheckBox cbFlatList;
        private System.Windows.Forms.GroupBox gbPropGridTitle;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnRefreshStrategies;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage pageSettings;
        private System.Windows.Forms.TabPage pageSchedule;
        private System.Windows.Forms.GroupBox groupBox1;
        private DevExpress.XtraGrid.GridControl gridControl3;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView3;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tbtnAddScheduleRec;
        private System.Windows.Forms.ToolStripButton tbtnEditScheduleRec;
        private System.Windows.Forms.ToolStripButton tbtnDelScheduleRec;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tbtnCleanObsoleteScheduleRecs;
        private System.Windows.Forms.ToolStripButton tbtnCloneScheduleRec;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem configurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem verifyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uploadToolStripMenuItem;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.Button btnVerify;
    }
}

