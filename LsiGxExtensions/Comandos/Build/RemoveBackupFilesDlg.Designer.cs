namespace LSI.Packages.Extensiones.Comandos.Build
{
    partial class RemoveBackupFilesDlg
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
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnAccept = new System.Windows.Forms.Button();
            this.LnkTargetDirectory = new System.Windows.Forms.LinkLabel();
            this.LstFilesToRemove = new System.Windows.Forms.ListBox();
            this.TxtTargetDirectory = new System.Windows.Forms.TextBox();
            this.TxtBackupPatterns = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BtnCancel
            // 
            this.BtnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Location = new System.Drawing.Point(379, 429);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(75, 23);
            this.BtnCancel.TabIndex = 6;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            // 
            // BtnAccept
            // 
            this.BtnAccept.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.BtnAccept.Location = new System.Drawing.Point(298, 429);
            this.BtnAccept.Name = "BtnAccept";
            this.BtnAccept.Size = new System.Drawing.Size(75, 23);
            this.BtnAccept.TabIndex = 5;
            this.BtnAccept.Text = "Accept";
            this.BtnAccept.UseVisualStyleBackColor = true;
            this.BtnAccept.Click += new System.EventHandler(this.BtnAccept_Click);
            // 
            // LnkTargetDirectory
            // 
            this.LnkTargetDirectory.AutoSize = true;
            this.LnkTargetDirectory.Location = new System.Drawing.Point(12, 9);
            this.LnkTargetDirectory.Name = "LnkTargetDirectory";
            this.LnkTargetDirectory.Size = new System.Drawing.Size(81, 13);
            this.LnkTargetDirectory.TabIndex = 0;
            this.LnkTargetDirectory.TabStop = true;
            this.LnkTargetDirectory.Text = "Target directory";
            this.LnkTargetDirectory.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkTargetDirectory_LinkClicked);
            // 
            // LstFilesToRemove
            // 
            this.LstFilesToRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LstFilesToRemove.BackColor = System.Drawing.SystemColors.Control;
            this.LstFilesToRemove.FormattingEnabled = true;
            this.LstFilesToRemove.HorizontalScrollbar = true;
            this.LstFilesToRemove.Location = new System.Drawing.Point(15, 66);
            this.LstFilesToRemove.Name = "LstFilesToRemove";
            this.LstFilesToRemove.Size = new System.Drawing.Size(724, 342);
            this.LstFilesToRemove.Sorted = true;
            this.LstFilesToRemove.TabIndex = 4;
            // 
            // TxtTargetDirectory
            // 
            this.TxtTargetDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtTargetDirectory.Location = new System.Drawing.Point(99, 6);
            this.TxtTargetDirectory.Name = "TxtTargetDirectory";
            this.TxtTargetDirectory.ReadOnly = true;
            this.TxtTargetDirectory.Size = new System.Drawing.Size(640, 20);
            this.TxtTargetDirectory.TabIndex = 1;
            // 
            // TxtBackupPatterns
            // 
            this.TxtBackupPatterns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtBackupPatterns.Location = new System.Drawing.Point(99, 32);
            this.TxtBackupPatterns.Name = "TxtBackupPatterns";
            this.TxtBackupPatterns.ReadOnly = true;
            this.TxtBackupPatterns.Size = new System.Drawing.Size(640, 20);
            this.TxtBackupPatterns.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Files to remove";
            // 
            // RemoveBackupFiles
            // 
            this.AcceptButton = this.BtnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancel;
            this.ClientSize = new System.Drawing.Size(751, 464);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TxtBackupPatterns);
            this.Controls.Add(this.TxtTargetDirectory);
            this.Controls.Add(this.LstFilesToRemove);
            this.Controls.Add(this.LnkTargetDirectory);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnAccept);
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RemoveBackupFiles";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Remove backup files";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.RemoveBackupFiles_HelpButtonClicked);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnAccept;
        private System.Windows.Forms.LinkLabel LnkTargetDirectory;
        private System.Windows.Forms.ListBox LstFilesToRemove;
        private System.Windows.Forms.TextBox TxtTargetDirectory;
        private System.Windows.Forms.TextBox TxtBackupPatterns;
        private System.Windows.Forms.Label label1;
    }
}