namespace LSI.Packages.Extensiones.Comandos.Build.Production
{
    partial class ProductionTaskDlg
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
			this.label1 = new System.Windows.Forms.Label();
			this.CmbTaskType = new System.Windows.Forms.ComboBox();
			this.TxtMainFile = new System.Windows.Forms.TextBox();
			this.BtnAccept = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.LblEnvironment = new System.Windows.Forms.Label();
			this.CmbEnvironmentType = new System.Windows.Forms.ComboBox();
			this.ChkEnabled = new System.Windows.Forms.CheckBox();
			this.BtnSelectMain = new System.Windows.Forms.Button();
			this.ChkKeepConfig = new System.Windows.Forms.CheckBox();
			this.ChkStopIis = new System.Windows.Forms.CheckBox();
			this.ChkKeepBackup = new System.Windows.Forms.CheckBox();
			this.LnkMainFile = new System.Windows.Forms.LinkLabel();
			this.label2 = new System.Windows.Forms.Label();
			this.CmbEnvironment = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(14, 19);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(54, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Task type";
			// 
			// CmbTaskType
			// 
			this.CmbTaskType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CmbTaskType.FormattingEnabled = true;
			this.CmbTaskType.Location = new System.Drawing.Point(148, 16);
			this.CmbTaskType.Name = "CmbTaskType";
			this.CmbTaskType.Size = new System.Drawing.Size(283, 21);
			this.CmbTaskType.TabIndex = 1;
			this.CmbTaskType.SelectedIndexChanged += new System.EventHandler(this.CmbTaskType_SelectedIndexChanged);
			// 
			// TxtMainFile
			// 
			this.TxtMainFile.Location = new System.Drawing.Point(148, 100);
			this.TxtMainFile.Name = "TxtMainFile";
			this.TxtMainFile.Size = new System.Drawing.Size(368, 20);
			this.TxtMainFile.TabIndex = 5;
			// 
			// BtnAccept
			// 
			this.BtnAccept.Location = new System.Drawing.Point(200, 226);
			this.BtnAccept.Name = "BtnAccept";
			this.BtnAccept.Size = new System.Drawing.Size(75, 23);
			this.BtnAccept.TabIndex = 11;
			this.BtnAccept.Text = "Accept";
			this.BtnAccept.UseVisualStyleBackColor = true;
			this.BtnAccept.Click += new System.EventHandler(this.BtnAccept_Click);
			// 
			// BtnCancel
			// 
			this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.BtnCancel.Location = new System.Drawing.Point(281, 226);
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.Size = new System.Drawing.Size(75, 23);
			this.BtnCancel.TabIndex = 12;
			this.BtnCancel.Text = "Cancel";
			this.BtnCancel.UseVisualStyleBackColor = true;
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// LblEnvironment
			// 
			this.LblEnvironment.AutoSize = true;
			this.LblEnvironment.Location = new System.Drawing.Point(14, 46);
			this.LblEnvironment.Name = "LblEnvironment";
			this.LblEnvironment.Size = new System.Drawing.Size(89, 13);
			this.LblEnvironment.TabIndex = 2;
			this.LblEnvironment.Text = "Environment type";
			// 
			// CmbEnvironmentType
			// 
			this.CmbEnvironmentType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CmbEnvironmentType.FormattingEnabled = true;
			this.CmbEnvironmentType.Location = new System.Drawing.Point(148, 43);
			this.CmbEnvironmentType.Name = "CmbEnvironmentType";
			this.CmbEnvironmentType.Size = new System.Drawing.Size(121, 21);
			this.CmbEnvironmentType.TabIndex = 3;
			this.CmbEnvironmentType.SelectedIndexChanged += new System.EventHandler(this.CmbEnvironmentType_SelectedIndexChanged);
			// 
			// ChkEnabled
			// 
			this.ChkEnabled.AutoSize = true;
			this.ChkEnabled.Checked = true;
			this.ChkEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ChkEnabled.Location = new System.Drawing.Point(148, 195);
			this.ChkEnabled.Name = "ChkEnabled";
			this.ChkEnabled.Size = new System.Drawing.Size(65, 17);
			this.ChkEnabled.TabIndex = 10;
			this.ChkEnabled.Text = "Enabled";
			this.ChkEnabled.UseVisualStyleBackColor = true;
			// 
			// BtnSelectMain
			// 
			this.BtnSelectMain.Location = new System.Drawing.Point(522, 100);
			this.BtnSelectMain.Name = "BtnSelectMain";
			this.BtnSelectMain.Size = new System.Drawing.Size(26, 23);
			this.BtnSelectMain.TabIndex = 6;
			this.BtnSelectMain.Text = "...";
			this.BtnSelectMain.UseVisualStyleBackColor = true;
			this.BtnSelectMain.Click += new System.EventHandler(this.BtnSelectMain_Click);
			// 
			// ChkKeepConfig
			// 
			this.ChkKeepConfig.AutoSize = true;
			this.ChkKeepConfig.Checked = true;
			this.ChkKeepConfig.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ChkKeepConfig.Location = new System.Drawing.Point(148, 126);
			this.ChkKeepConfig.Name = "ChkKeepConfig";
			this.ChkKeepConfig.Size = new System.Drawing.Size(246, 17);
			this.ChkKeepConfig.TabIndex = 7;
			this.ChkKeepConfig.Text = "Keep current client.exe.config / web.config file";
			this.ChkKeepConfig.UseVisualStyleBackColor = true;
			// 
			// ChkStopIis
			// 
			this.ChkStopIis.AutoSize = true;
			this.ChkStopIis.Checked = true;
			this.ChkStopIis.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ChkStopIis.Location = new System.Drawing.Point(148, 149);
			this.ChkStopIis.Name = "ChkStopIis";
			this.ChkStopIis.Size = new System.Drawing.Size(217, 17);
			this.ChkStopIis.TabIndex = 8;
			this.ChkStopIis.Text = "Stop IIS before copy and start after copy";
			this.ChkStopIis.UseVisualStyleBackColor = true;
			// 
			// ChkKeepBackup
			// 
			this.ChkKeepBackup.AutoSize = true;
			this.ChkKeepBackup.Checked = true;
			this.ChkKeepBackup.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ChkKeepBackup.Location = new System.Drawing.Point(148, 172);
			this.ChkKeepBackup.Name = "ChkKeepBackup";
			this.ChkKeepBackup.Size = new System.Drawing.Size(262, 17);
			this.ChkKeepBackup.TabIndex = 9;
			this.ChkKeepBackup.Text = "Keep a backup of the current destination directory";
			this.ChkKeepBackup.UseVisualStyleBackColor = true;
			// 
			// LnkMainFile
			// 
			this.LnkMainFile.AutoSize = true;
			this.LnkMainFile.Location = new System.Drawing.Point(14, 105);
			this.LnkMainFile.Name = "LnkMainFile";
			this.LnkMainFile.Size = new System.Drawing.Size(46, 13);
			this.LnkMainFile.TabIndex = 4;
			this.LnkMainFile.TabStop = true;
			this.LnkMainFile.Text = "Main file";
			this.LnkMainFile.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkMainFile_LinkClicked);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(14, 75);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(104, 13);
			this.label2.TabIndex = 13;
			this.label2.Text = "Only for environment";
			// 
			// CmbEnvironment
			// 
			this.CmbEnvironment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CmbEnvironment.FormattingEnabled = true;
			this.CmbEnvironment.Location = new System.Drawing.Point(148, 71);
			this.CmbEnvironment.Name = "CmbEnvironment";
			this.CmbEnvironment.Size = new System.Drawing.Size(368, 21);
			this.CmbEnvironment.TabIndex = 14;
			// 
			// ProductionTaskDlg
			// 
			this.AcceptButton = this.BtnAccept;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.BtnCancel;
			this.ClientSize = new System.Drawing.Size(564, 257);
			this.Controls.Add(this.CmbEnvironment);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.LnkMainFile);
			this.Controls.Add(this.ChkKeepBackup);
			this.Controls.Add(this.ChkStopIis);
			this.Controls.Add(this.ChkKeepConfig);
			this.Controls.Add(this.BtnSelectMain);
			this.Controls.Add(this.ChkEnabled);
			this.Controls.Add(this.CmbEnvironmentType);
			this.Controls.Add(this.LblEnvironment);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnAccept);
			this.Controls.Add(this.TxtMainFile);
			this.Controls.Add(this.CmbTaskType);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ProductionTaskDlg";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Production task";
			this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.ProductionTaskDlg_HelpButtonClicked);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox CmbTaskType;
        private System.Windows.Forms.TextBox TxtMainFile;
        private System.Windows.Forms.Button BtnAccept;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Label LblEnvironment;
        private System.Windows.Forms.ComboBox CmbEnvironmentType;
        private System.Windows.Forms.CheckBox ChkEnabled;
        private System.Windows.Forms.Button BtnSelectMain;
        private System.Windows.Forms.CheckBox ChkKeepConfig;
        private System.Windows.Forms.CheckBox ChkStopIis;
        private System.Windows.Forms.CheckBox ChkKeepBackup;
        private System.Windows.Forms.LinkLabel LnkMainFile;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox CmbEnvironment;
	}
}