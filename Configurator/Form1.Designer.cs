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
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.plotterPanel = new System.Windows.Forms.Panel();
            this.gbPropGridTitle = new System.Windows.Forms.GroupBox();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.panelTrades = new System.Windows.Forms.Panel();
            this.gridControl2 = new DevExpress.XtraGrid.GridControl();
            this.gridView2 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.splitter3 = new System.Windows.Forms.Splitter();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imagesTrees = new System.Windows.Forms.ImageList(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.cbOnlyWorkingSet = new System.Windows.Forms.CheckBox();
            this.rbStrategies = new System.Windows.Forms.RadioButton();
            this.rbMarkets = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rbExchanges = new System.Windows.Forms.RadioButton();
            this.cbFlatList = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.plotterPanel.SuspendLayout();
            this.gbPropGridTitle.SuspendLayout();
            this.panelTrades.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 84);
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
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1140, 599);
            this.splitContainer1.SplitterDistance = 661;
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
            this.gridControl1.Size = new System.Drawing.Size(661, 594);
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
            this.splitter1.Location = new System.Drawing.Point(0, 594);
            this.splitter1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(661, 5);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.plotterPanel);
            this.splitContainer2.Panel1.Controls.Add(this.splitter2);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.panelTrades);
            this.splitContainer2.Panel2.Controls.Add(this.splitter3);
            this.splitContainer2.Size = new System.Drawing.Size(475, 599);
            this.splitContainer2.SplitterDistance = 343;
            this.splitContainer2.TabIndex = 0;
            // 
            // plotterPanel
            // 
            this.plotterPanel.Controls.Add(this.gbPropGridTitle);
            this.plotterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plotterPanel.Location = new System.Drawing.Point(0, 0);
            this.plotterPanel.Margin = new System.Windows.Forms.Padding(4);
            this.plotterPanel.Name = "plotterPanel";
            this.plotterPanel.Size = new System.Drawing.Size(468, 343);
            this.plotterPanel.TabIndex = 10;
            // 
            // gbPropGridTitle
            // 
            this.gbPropGridTitle.Controls.Add(this.propertyGrid1);
            this.gbPropGridTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbPropGridTitle.Location = new System.Drawing.Point(0, 0);
            this.gbPropGridTitle.Name = "gbPropGridTitle";
            this.gbPropGridTitle.Size = new System.Drawing.Size(468, 343);
            this.gbPropGridTitle.TabIndex = 1;
            this.gbPropGridTitle.TabStop = false;
            this.gbPropGridTitle.Text = "Selected items";
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(3, 19);
            this.propertyGrid1.Margin = new System.Windows.Forms.Padding(4);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(462, 321);
            this.propertyGrid1.TabIndex = 0;
            this.propertyGrid1.ToolbarVisible = false;
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter2.Location = new System.Drawing.Point(468, 0);
            this.splitter2.Margin = new System.Windows.Forms.Padding(4);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(7, 343);
            this.splitter2.TabIndex = 7;
            this.splitter2.TabStop = false;
            // 
            // panelTrades
            // 
            this.panelTrades.Controls.Add(this.gridControl2);
            this.panelTrades.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTrades.Location = new System.Drawing.Point(0, 0);
            this.panelTrades.Margin = new System.Windows.Forms.Padding(4);
            this.panelTrades.Name = "panelTrades";
            this.panelTrades.Size = new System.Drawing.Size(468, 252);
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
            this.gridControl2.Size = new System.Drawing.Size(468, 252);
            this.gridControl2.TabIndex = 6;
            this.gridControl2.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView2});
            // 
            // gridView2
            // 
            this.gridView2.GridControl = this.gridControl2;
            this.gridView2.Name = "gridView2";
            // 
            // splitter3
            // 
            this.splitter3.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter3.Location = new System.Drawing.Point(468, 0);
            this.splitter3.Margin = new System.Windows.Forms.Padding(4);
            this.splitter3.Name = "splitter3";
            this.splitter3.Size = new System.Drawing.Size(7, 252);
            this.splitter3.TabIndex = 7;
            this.splitter3.TabStop = false;
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
            this.cbOnlyWorkingSet.Location = new System.Drawing.Point(300, 10);
            this.cbOnlyWorkingSet.Margin = new System.Windows.Forms.Padding(4);
            this.cbOnlyWorkingSet.Name = "cbOnlyWorkingSet";
            this.cbOnlyWorkingSet.Size = new System.Drawing.Size(133, 21);
            this.cbOnlyWorkingSet.TabIndex = 2;
            this.cbOnlyWorkingSet.Text = "Only WorkingSet";
            this.cbOnlyWorkingSet.UseVisualStyleBackColor = true;
            this.cbOnlyWorkingSet.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // rbStrategies
            // 
            this.rbStrategies.AutoSize = true;
            this.rbStrategies.Location = new System.Drawing.Point(202, 9);
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
            this.rbMarkets.Location = new System.Drawing.Point(118, 9);
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
            this.panel1.Controls.Add(this.rbExchanges);
            this.panel1.Controls.Add(this.cbFlatList);
            this.panel1.Controls.Add(this.rbMarkets);
            this.panel1.Controls.Add(this.cbOnlyWorkingSet);
            this.panel1.Controls.Add(this.rbStrategies);
            this.panel1.Location = new System.Drawing.Point(0, 33);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(587, 40);
            this.panel1.TabIndex = 5;
            // 
            // rbExchanges
            // 
            this.rbExchanges.AutoSize = true;
            this.rbExchanges.Location = new System.Drawing.Point(15, 9);
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
            this.cbFlatList.Location = new System.Drawing.Point(441, 10);
            this.cbFlatList.Margin = new System.Windows.Forms.Padding(4);
            this.cbFlatList.Name = "cbFlatList";
            this.cbFlatList.Size = new System.Drawing.Size(72, 21);
            this.cbFlatList.TabIndex = 6;
            this.cbFlatList.Text = "FlatList";
            this.cbFlatList.UseVisualStyleBackColor = true;
            this.cbFlatList.CheckedChanged += new System.EventHandler(this.cbFlatList_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(844, 9);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 17);
            this.label2.TabIndex = 7;
            this.label2.Text = "label2";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(844, 33);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 17);
            this.label3.TabIndex = 8;
            this.label3.Text = "label3";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(407, 6);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 24);
            this.btnSave.TabIndex = 9;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1140, 683);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FTS Configurator";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.plotterPanel.ResumeLayout(false);
            this.gbPropGridTitle.ResumeLayout(false);
            this.panelTrades.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
        private System.Windows.Forms.Splitter splitter2;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ImageList imagesTrees;
        private System.Windows.Forms.Panel panelTrades;
        private System.Windows.Forms.Splitter splitter3;
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox gbPropGridTitle;
        private System.Windows.Forms.Button btnSave;
    }
}

