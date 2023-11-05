namespace LSI.Packages.Extensiones.Comandos
{
    partial class ReorderWinTab
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReorderWinTab));
            this.label1 = new System.Windows.Forms.Label();
            this.CmbTabControls = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.LstTabPages = new System.Windows.Forms.ListBox();
            this.BtnMoveUp = new System.Windows.Forms.Button();
            this.BtnMoveDown = new System.Windows.Forms.Button();
            this.BtnAccept = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Tab &control";
            // 
            // CmbTabControls
            // 
            this.CmbTabControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.CmbTabControls.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbTabControls.FormattingEnabled = true;
            this.CmbTabControls.Location = new System.Drawing.Point(79, 24);
            this.CmbTabControls.Name = "CmbTabControls";
            this.CmbTabControls.Size = new System.Drawing.Size(441, 21);
            this.CmbTabControls.TabIndex = 1;
            this.CmbTabControls.SelectedIndexChanged += new System.EventHandler(this.CmbTabControls_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Tab &pages";
            // 
            // LstTabPages
            // 
            this.LstTabPages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LstTabPages.FormattingEnabled = true;
            this.LstTabPages.Location = new System.Drawing.Point(15, 77);
            this.LstTabPages.Name = "LstTabPages";
            this.LstTabPages.Size = new System.Drawing.Size(424, 212);
            this.LstTabPages.TabIndex = 3;
            // 
            // BtnMoveUp
            // 
            this.BtnMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnMoveUp.Location = new System.Drawing.Point(445, 77);
            this.BtnMoveUp.Name = "BtnMoveUp";
            this.BtnMoveUp.Size = new System.Drawing.Size(75, 23);
            this.BtnMoveUp.TabIndex = 4;
            this.BtnMoveUp.Text = "Move &up";
            this.BtnMoveUp.UseVisualStyleBackColor = true;
            this.BtnMoveUp.Click += new System.EventHandler(this.BtnMoveUp_Click);
            // 
            // BtnMoveDown
            // 
            this.BtnMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnMoveDown.Location = new System.Drawing.Point(445, 107);
            this.BtnMoveDown.Name = "BtnMoveDown";
            this.BtnMoveDown.Size = new System.Drawing.Size(75, 23);
            this.BtnMoveDown.TabIndex = 5;
            this.BtnMoveDown.Text = "Move &down";
            this.BtnMoveDown.UseVisualStyleBackColor = true;
            this.BtnMoveDown.Click += new System.EventHandler(this.BtnMoveDown_Click);
            // 
            // BtnAccept
            // 
            this.BtnAccept.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.BtnAccept.Location = new System.Drawing.Point(171, 298);
            this.BtnAccept.Name = "BtnAccept";
            this.BtnAccept.Size = new System.Drawing.Size(75, 23);
            this.BtnAccept.TabIndex = 6;
            this.BtnAccept.Text = "Accept";
            this.BtnAccept.UseVisualStyleBackColor = true;
            this.BtnAccept.Click += new System.EventHandler(this.BtnAccept_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Location = new System.Drawing.Point(286, 298);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(75, 23);
            this.BtnCancel.TabIndex = 7;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            // 
            // ReorderWinTab
            // 
            this.AcceptButton = this.BtnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancel;
            this.ClientSize = new System.Drawing.Size(532, 333);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnAccept);
            this.Controls.Add(this.BtnMoveDown);
            this.Controls.Add(this.BtnMoveUp);
            this.Controls.Add(this.LstTabPages);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CmbTabControls);
            this.Controls.Add(this.label1);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReorderWinTab";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Reorder winform tab pages";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.ReorderWinTab_HelpButtonClicked);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox CmbTabControls;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox LstTabPages;
        private System.Windows.Forms.Button BtnMoveUp;
        private System.Windows.Forms.Button BtnMoveDown;
        private System.Windows.Forms.Button BtnAccept;
        private System.Windows.Forms.Button BtnCancel;
    }
}