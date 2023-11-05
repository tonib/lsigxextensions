namespace LSI.Packages.Extensiones.Comandos
{
    partial class ReplaceVariablesAttributeBasedWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReplaceVariablesAttributeBasedWindow));
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnAccept = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.TxtOldAttribute = new System.Windows.Forms.TextBox();
            this.BtnSelectOld = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.TxtNewAttribute = new System.Windows.Forms.TextBox();
            this.BtnSelectNew = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.ChkJustTest = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.LblTypeOld = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.LblTypeNew = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.LblPictureOld = new System.Windows.Forms.Label();
            this.LblPictureNew = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BtnCancel
            // 
            this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Location = new System.Drawing.Point(289, 244);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(75, 23);
            this.BtnCancel.TabIndex = 9;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            // 
            // BtnAccept
            // 
            this.BtnAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnAccept.Location = new System.Drawing.Point(183, 244);
            this.BtnAccept.Name = "BtnAccept";
            this.BtnAccept.Size = new System.Drawing.Size(100, 23);
            this.BtnAccept.TabIndex = 8;
            this.BtnAccept.Text = "Do replacement";
            this.BtnAccept.UseVisualStyleBackColor = true;
            this.BtnAccept.Click += new System.EventHandler(this.BtnAccept_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Old attribute";
            // 
            // TxtOldAttribute
            // 
            this.TxtOldAttribute.Location = new System.Drawing.Point(112, 47);
            this.TxtOldAttribute.Name = "TxtOldAttribute";
            this.TxtOldAttribute.Size = new System.Drawing.Size(224, 20);
            this.TxtOldAttribute.TabIndex = 1;
            this.TxtOldAttribute.Leave += new System.EventHandler(this.TxtOldAttribute_Leave);
            // 
            // BtnSelectOld
            // 
            this.BtnSelectOld.Location = new System.Drawing.Point(342, 47);
            this.BtnSelectOld.Name = "BtnSelectOld";
            this.BtnSelectOld.Size = new System.Drawing.Size(30, 23);
            this.BtnSelectOld.TabIndex = 2;
            this.BtnSelectOld.Text = "...";
            this.BtnSelectOld.UseVisualStyleBackColor = true;
            this.BtnSelectOld.Click += new System.EventHandler(this.BtnSelect_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 128);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "&New attribute";
            // 
            // TxtNewAttribute
            // 
            this.TxtNewAttribute.Location = new System.Drawing.Point(112, 125);
            this.TxtNewAttribute.Name = "TxtNewAttribute";
            this.TxtNewAttribute.Size = new System.Drawing.Size(224, 20);
            this.TxtNewAttribute.TabIndex = 4;
            this.TxtNewAttribute.Leave += new System.EventHandler(this.TxtNewAttribute_Leave);
            // 
            // BtnSelectNew
            // 
            this.BtnSelectNew.Location = new System.Drawing.Point(342, 125);
            this.BtnSelectNew.Name = "BtnSelectNew";
            this.BtnSelectNew.Size = new System.Drawing.Size(30, 23);
            this.BtnSelectNew.TabIndex = 5;
            this.BtnSelectNew.Text = "...";
            this.BtnSelectNew.UseVisualStyleBackColor = true;
            this.BtnSelectNew.Click += new System.EventHandler(this.BtnSelect_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(379, 128);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(147, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "(null =Remove attr. reference)";
            // 
            // ChkJustTest
            // 
            this.ChkJustTest.AutoSize = true;
            this.ChkJustTest.Checked = true;
            this.ChkJustTest.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ChkJustTest.Location = new System.Drawing.Point(112, 203);
            this.ChkJustTest.Name = "ChkJustTest";
            this.ChkJustTest.Size = new System.Drawing.Size(209, 17);
            this.ChkJustTest.TabIndex = 7;
            this.ChkJustTest.Text = "&Test only. Do not save object changes";
            this.ChkJustTest.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(112, 70);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Type:";
            // 
            // LblTypeOld
            // 
            this.LblTypeOld.AutoSize = true;
            this.LblTypeOld.Location = new System.Drawing.Point(162, 70);
            this.LblTypeOld.Name = "LblTypeOld";
            this.LblTypeOld.Size = new System.Drawing.Size(60, 13);
            this.LblTypeOld.TabIndex = 11;
            this.LblTypeOld.Text = "(Not found)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(112, 152);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Type:";
            // 
            // LblTypeNew
            // 
            this.LblTypeNew.AutoSize = true;
            this.LblTypeNew.Location = new System.Drawing.Point(162, 152);
            this.LblTypeNew.Name = "LblTypeNew";
            this.LblTypeNew.Size = new System.Drawing.Size(60, 13);
            this.LblTypeNew.TabIndex = 13;
            this.LblTypeNew.Text = "(Not found)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(112, 90);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Picture:";
            // 
            // LblPictureOld
            // 
            this.LblPictureOld.AutoSize = true;
            this.LblPictureOld.Location = new System.Drawing.Point(162, 90);
            this.LblPictureOld.Name = "LblPictureOld";
            this.LblPictureOld.Size = new System.Drawing.Size(60, 13);
            this.LblPictureOld.TabIndex = 15;
            this.LblPictureOld.Text = "(Not found)";
            // 
            // LblPictureNew
            // 
            this.LblPictureNew.AutoSize = true;
            this.LblPictureNew.Location = new System.Drawing.Point(162, 173);
            this.LblPictureNew.Name = "LblPictureNew";
            this.LblPictureNew.Size = new System.Drawing.Size(60, 13);
            this.LblPictureNew.TabIndex = 17;
            this.LblPictureNew.Text = "(Not found)";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(112, 173);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(43, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "Picture:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.Color.Red;
            this.label7.Location = new System.Drawing.Point(15, 19);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(338, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "* ALWAYS DO A KBASE BACKUP BEFORE RUN THIS EXTENSION";
            // 
            // ReplaceVariablesAttributeBasedWindow
            // 
            this.AcceptButton = this.BtnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancel;
            this.ClientSize = new System.Drawing.Size(546, 279);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.LblPictureNew);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.LblPictureOld);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.LblTypeNew);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.LblTypeOld);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ChkJustTest);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.BtnSelectNew);
            this.Controls.Add(this.TxtNewAttribute);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BtnSelectOld);
            this.Controls.Add(this.TxtOldAttribute);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnAccept);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReplaceVariablesAttributeBasedWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Replace variables \'Attribute based on\'";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.ReplaceVariablesAttributeBasedWindow_HelpButtonClicked);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnAccept;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TxtOldAttribute;
        private System.Windows.Forms.Button BtnSelectOld;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TxtNewAttribute;
        private System.Windows.Forms.Button BtnSelectNew;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox ChkJustTest;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label LblTypeOld;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label LblTypeNew;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label LblPictureOld;
        private System.Windows.Forms.Label LblPictureNew;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
    }
}