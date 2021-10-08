
namespace Configurator
{
    partial class ScheduleRecordEditor
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
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxSoftStop = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpSoftStop = new System.Windows.Forms.DateTimePicker();
            this.dtpHardStop = new System.Windows.Forms.DateTimePicker();
            this.dtpNoRestr = new System.Windows.Forms.DateTimePicker();
            this.cbtzStop = new System.Windows.Forms.ComboBox();
            this.cbtzDone = new System.Windows.Forms.ComboBox();
            this.cbKeepWindowOpened = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cbTarget = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textInfo = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(42, 67);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "HardStop";
            // 
            // checkBoxSoftStop
            // 
            this.checkBoxSoftStop.AutoSize = true;
            this.checkBoxSoftStop.Location = new System.Drawing.Point(29, 39);
            this.checkBoxSoftStop.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxSoftStop.Name = "checkBoxSoftStop";
            this.checkBoxSoftStop.Size = new System.Drawing.Size(81, 21);
            this.checkBoxSoftStop.TabIndex = 2;
            this.checkBoxSoftStop.Text = "SoftStop";
            this.checkBoxSoftStop.UseVisualStyleBackColor = true;
            this.checkBoxSoftStop.CheckedChanged += new System.EventHandler(this.checkBoxSoftStop_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 108);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "No Restriction";
            // 
            // dtpSoftStop
            // 
            this.dtpSoftStop.CustomFormat = "yyyy.MM.dd HH:mm";
            this.dtpSoftStop.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpSoftStop.Location = new System.Drawing.Point(116, 38);
            this.dtpSoftStop.Name = "dtpSoftStop";
            this.dtpSoftStop.Size = new System.Drawing.Size(137, 23);
            this.dtpSoftStop.TabIndex = 3;
            // 
            // dtpHardStop
            // 
            this.dtpHardStop.CustomFormat = "yyyy.MM.dd HH:mm";
            this.dtpHardStop.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpHardStop.Location = new System.Drawing.Point(116, 64);
            this.dtpHardStop.Name = "dtpHardStop";
            this.dtpHardStop.Size = new System.Drawing.Size(137, 23);
            this.dtpHardStop.TabIndex = 5;
            // 
            // dtpNoRestr
            // 
            this.dtpNoRestr.CustomFormat = "yyyy.MM.dd HH:mm";
            this.dtpNoRestr.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpNoRestr.Location = new System.Drawing.Point(116, 105);
            this.dtpNoRestr.Name = "dtpNoRestr";
            this.dtpNoRestr.Size = new System.Drawing.Size(137, 23);
            this.dtpNoRestr.TabIndex = 6;
            // 
            // cbtzStop
            // 
            this.cbtzStop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbtzStop.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbtzStop.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbtzStop.FormattingEnabled = true;
            this.cbtzStop.Location = new System.Drawing.Point(259, 37);
            this.cbtzStop.Name = "cbtzStop";
            this.cbtzStop.Size = new System.Drawing.Size(312, 24);
            this.cbtzStop.TabIndex = 4;
            this.cbtzStop.SelectedIndexChanged += new System.EventHandler(this.cbtzStop_SelectedIndexChanged);
            this.cbtzStop.TextUpdate += new System.EventHandler(this.cbtzStop_TextUpdate);
            this.cbtzStop.Leave += new System.EventHandler(this.cbtzStop_Leave);
            // 
            // cbtzDone
            // 
            this.cbtzDone.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbtzDone.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbtzDone.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbtzDone.FormattingEnabled = true;
            this.cbtzDone.Location = new System.Drawing.Point(259, 105);
            this.cbtzDone.Name = "cbtzDone";
            this.cbtzDone.Size = new System.Drawing.Size(312, 24);
            this.cbtzDone.TabIndex = 7;
            this.cbtzDone.SelectedIndexChanged += new System.EventHandler(this.cbtzDone_SelectedIndexChanged);
            this.cbtzDone.Leave += new System.EventHandler(this.cbtzDone_Leave);
            // 
            // cbKeepWindowOpened
            // 
            this.cbKeepWindowOpened.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbKeepWindowOpened.AutoSize = true;
            this.cbKeepWindowOpened.Location = new System.Drawing.Point(16, 213);
            this.cbKeepWindowOpened.Name = "cbKeepWindowOpened";
            this.cbKeepWindowOpened.Size = new System.Drawing.Size(161, 21);
            this.cbKeepWindowOpened.TabIndex = 9;
            this.cbKeepWindowOpened.Text = "Keep window opened";
            this.cbKeepWindowOpened.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(379, 209);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(93, 27);
            this.btnOK.TabIndex = 98;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(478, 209);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(93, 27);
            this.btnCancel.TabIndex = 99;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(194, 162);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 17);
            this.label3.TabIndex = 11;
            this.label3.Text = "Apply to";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(259, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 17);
            this.label4.TabIndex = 12;
            this.label4.Text = "Not set";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(256, 132);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 17);
            this.label5.TabIndex = 13;
            this.label5.Text = "Not set";
            // 
            // cbTarget
            // 
            this.cbTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTarget.FormattingEnabled = true;
            this.cbTarget.Location = new System.Drawing.Point(259, 159);
            this.cbTarget.Name = "cbTarget";
            this.cbTarget.Size = new System.Drawing.Size(312, 24);
            this.cbTarget.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 17);
            this.label6.TabIndex = 15;
            this.label6.Text = "Info";
            // 
            // textInfo
            // 
            this.textInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textInfo.Location = new System.Drawing.Point(50, 6);
            this.textInfo.Name = "textInfo";
            this.textInfo.Size = new System.Drawing.Size(521, 23);
            this.textInfo.TabIndex = 1;
            // 
            // ScheduleRecordEditor
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(583, 248);
            this.Controls.Add(this.textInfo);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cbTarget);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cbKeepWindowOpened);
            this.Controls.Add(this.cbtzDone);
            this.Controls.Add(this.cbtzStop);
            this.Controls.Add(this.dtpNoRestr);
            this.Controls.Add(this.dtpHardStop);
            this.Controls.Add(this.dtpSoftStop);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.checkBoxSoftStop);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScheduleRecordEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ScheduleRecordEditor";
            this.Load += new System.EventHandler(this.ScheduleRecordEditor_Load);
            this.Shown += new System.EventHandler(this.ScheduleRecordEditor_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxSoftStop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpSoftStop;
        private System.Windows.Forms.DateTimePicker dtpHardStop;
        private System.Windows.Forms.DateTimePicker dtpNoRestr;
        private System.Windows.Forms.ComboBox cbtzStop;
        private System.Windows.Forms.ComboBox cbtzDone;
        private System.Windows.Forms.CheckBox cbKeepWindowOpened;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ComboBox cbTarget;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textInfo;
    }
}