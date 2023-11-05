namespace LSI.Packages.Extensiones.Comandos.Build.Production
{
    partial class PrepareProductionDlg
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
            this.TxtTargetDir = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.LstTasks = new System.Windows.Forms.ListBox();
            this.BtnClose = new System.Windows.Forms.Button();
            this.BtnRemoveTask = new System.Windows.Forms.Button();
            this.BtnPrepareProduction = new System.Windows.Forms.Button();
            this.BtnEdit = new System.Windows.Forms.Button();
            this.BtnNew = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.TxtIgnorePatterns = new System.Windows.Forms.TextBox();
            this.BtnMoveUp = new System.Windows.Forms.Button();
            this.BtnMoveDown = new System.Windows.Forms.Button();
            this.ChkCopyImagesTxt = new System.Windows.Forms.CheckBox();
            this.BtnRunSelectedTasks = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Target directory";
            // 
            // TxtTargetDir
            // 
            this.TxtTargetDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtTargetDir.Location = new System.Drawing.Point(126, 13);
            this.TxtTargetDir.Name = "TxtTargetDir";
            this.TxtTargetDir.ReadOnly = true;
            this.TxtTargetDir.Size = new System.Drawing.Size(577, 20);
            this.TxtTargetDir.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "&Tasks";
            // 
            // LstTasks
            // 
            this.LstTasks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LstTasks.FormattingEnabled = true;
            this.LstTasks.HorizontalScrollbar = true;
            this.LstTasks.Location = new System.Drawing.Point(15, 97);
            this.LstTasks.Name = "LstTasks";
            this.LstTasks.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LstTasks.Size = new System.Drawing.Size(607, 251);
            this.LstTasks.TabIndex = 5;
            this.LstTasks.DoubleClick += new System.EventHandler(this.LstTasks_DoubleClick);
            // 
            // BtnClose
            // 
            this.BtnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnClose.Location = new System.Drawing.Point(15, 362);
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.Size = new System.Drawing.Size(75, 23);
            this.BtnClose.TabIndex = 9;
            this.BtnClose.Text = "Close";
            this.BtnClose.UseVisualStyleBackColor = true;
            this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // BtnRemoveTask
            // 
            this.BtnRemoveTask.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnRemoveTask.Location = new System.Drawing.Point(628, 156);
            this.BtnRemoveTask.Name = "BtnRemoveTask";
            this.BtnRemoveTask.Size = new System.Drawing.Size(75, 23);
            this.BtnRemoveTask.TabIndex = 8;
            this.BtnRemoveTask.Text = "&Remove";
            this.BtnRemoveTask.UseVisualStyleBackColor = true;
            this.BtnRemoveTask.Click += new System.EventHandler(this.BtnRemoveTask_Click);
            // 
            // BtnPrepareProduction
            // 
            this.BtnPrepareProduction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnPrepareProduction.Location = new System.Drawing.Point(431, 362);
            this.BtnPrepareProduction.Name = "BtnPrepareProduction";
            this.BtnPrepareProduction.Size = new System.Drawing.Size(194, 23);
            this.BtnPrepareProduction.TabIndex = 10;
            this.BtnPrepareProduction.Text = "Prepare production (run all tasks)...";
            this.BtnPrepareProduction.UseVisualStyleBackColor = true;
            this.BtnPrepareProduction.Click += new System.EventHandler(this.BtnPrepareProduction_Click);
            // 
            // BtnEdit
            // 
            this.BtnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnEdit.Location = new System.Drawing.Point(628, 127);
            this.BtnEdit.Name = "BtnEdit";
            this.BtnEdit.Size = new System.Drawing.Size(75, 23);
            this.BtnEdit.TabIndex = 7;
            this.BtnEdit.Text = "&Edit...";
            this.BtnEdit.UseVisualStyleBackColor = true;
            this.BtnEdit.Click += new System.EventHandler(this.BtnEdit_Click);
            // 
            // BtnNew
            // 
            this.BtnNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnNew.Location = new System.Drawing.Point(628, 97);
            this.BtnNew.Name = "BtnNew";
            this.BtnNew.Size = new System.Drawing.Size(75, 23);
            this.BtnNew.TabIndex = 6;
            this.BtnNew.Text = "&New...";
            this.BtnNew.UseVisualStyleBackColor = true;
            this.BtnNew.Click += new System.EventHandler(this.BtnNew_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(108, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "&File patterns to ignore";
            // 
            // TxtIgnorePatterns
            // 
            this.TxtIgnorePatterns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtIgnorePatterns.Location = new System.Drawing.Point(126, 39);
            this.TxtIgnorePatterns.Name = "TxtIgnorePatterns";
            this.TxtIgnorePatterns.Size = new System.Drawing.Size(577, 20);
            this.TxtIgnorePatterns.TabIndex = 3;
            // 
            // BtnMoveUp
            // 
            this.BtnMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnMoveUp.Location = new System.Drawing.Point(628, 295);
            this.BtnMoveUp.Name = "BtnMoveUp";
            this.BtnMoveUp.Size = new System.Drawing.Size(75, 23);
            this.BtnMoveUp.TabIndex = 11;
            this.BtnMoveUp.Text = "Move up";
            this.BtnMoveUp.UseVisualStyleBackColor = true;
            this.BtnMoveUp.Click += new System.EventHandler(this.BtnMoveUp_Click);
            // 
            // BtnMoveDown
            // 
            this.BtnMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnMoveDown.Location = new System.Drawing.Point(628, 325);
            this.BtnMoveDown.Name = "BtnMoveDown";
            this.BtnMoveDown.Size = new System.Drawing.Size(75, 23);
            this.BtnMoveDown.TabIndex = 12;
            this.BtnMoveDown.Text = "Move down";
            this.BtnMoveDown.UseVisualStyleBackColor = true;
            this.BtnMoveDown.Click += new System.EventHandler(this.BtnMoveDown_Click);
            // 
            // ChkCopyImagesTxt
            // 
            this.ChkCopyImagesTxt.AutoSize = true;
            this.ChkCopyImagesTxt.Location = new System.Drawing.Point(126, 65);
            this.ChkCopyImagesTxt.Name = "ChkCopyImagesTxt";
            this.ChkCopyImagesTxt.Size = new System.Drawing.Size(174, 17);
            this.ChkCopyImagesTxt.TabIndex = 13;
            this.ChkCopyImagesTxt.Text = "Copy file images.txt to bin folder";
            this.ChkCopyImagesTxt.UseVisualStyleBackColor = true;
            // 
            // BtnRunSelectedTasks
            // 
            this.BtnRunSelectedTasks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnRunSelectedTasks.Location = new System.Drawing.Point(295, 362);
            this.BtnRunSelectedTasks.Name = "BtnRunSelectedTasks";
            this.BtnRunSelectedTasks.Size = new System.Drawing.Size(130, 23);
            this.BtnRunSelectedTasks.TabIndex = 14;
            this.BtnRunSelectedTasks.Text = "Run selected tasks...";
            this.BtnRunSelectedTasks.UseVisualStyleBackColor = true;
            this.BtnRunSelectedTasks.Click += new System.EventHandler(this.BtnRunSelectedTasks_Click);
            // 
            // PrepareProductionDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnClose;
            this.ClientSize = new System.Drawing.Size(715, 397);
            this.Controls.Add(this.BtnRunSelectedTasks);
            this.Controls.Add(this.ChkCopyImagesTxt);
            this.Controls.Add(this.BtnMoveDown);
            this.Controls.Add(this.BtnMoveUp);
            this.Controls.Add(this.TxtIgnorePatterns);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.BtnNew);
            this.Controls.Add(this.BtnEdit);
            this.Controls.Add(this.BtnPrepareProduction);
            this.Controls.Add(this.BtnRemoveTask);
            this.Controls.Add(this.BtnClose);
            this.Controls.Add(this.LstTasks);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TxtTargetDir);
            this.Controls.Add(this.label1);
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PrepareProductionDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Prepare production";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.PrepareProductionDlg_HelpButtonClicked);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TxtTargetDir;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox LstTasks;
        private System.Windows.Forms.Button BtnClose;
        private System.Windows.Forms.Button BtnRemoveTask;
        private System.Windows.Forms.Button BtnPrepareProduction;
        private System.Windows.Forms.Button BtnEdit;
        private System.Windows.Forms.Button BtnNew;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TxtIgnorePatterns;
        private System.Windows.Forms.Button BtnMoveUp;
        private System.Windows.Forms.Button BtnMoveDown;
        private System.Windows.Forms.CheckBox ChkCopyImagesTxt;
        private System.Windows.Forms.Button BtnRunSelectedTasks;
    }
}