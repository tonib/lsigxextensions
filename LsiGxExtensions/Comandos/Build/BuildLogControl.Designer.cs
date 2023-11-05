namespace LSI.Packages.Extensiones.Comandos.Build
{
    partial class BuildLogControl
    {
        /// <summary> 
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar 
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.BtnClose = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnOpenError = new System.Windows.Forms.Button();
            this.BtnNextError = new System.Windows.Forms.Button();
            this.TxtLog = new System.Windows.Forms.RichTextBox();
            this.BtnRepairAndRecompile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnClose
            // 
            this.BtnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnClose.Location = new System.Drawing.Point(604, 172);
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.Size = new System.Drawing.Size(75, 23);
            this.BtnClose.TabIndex = 1;
            this.BtnClose.Text = "Cl&ose";
            this.BtnClose.UseVisualStyleBackColor = true;
            this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnCancel.Location = new System.Drawing.Point(3, 172);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(75, 23);
            this.BtnCancel.TabIndex = 2;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnOpenError
            // 
            this.BtnOpenError.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnOpenError.Location = new System.Drawing.Point(162, 172);
            this.BtnOpenError.Name = "BtnOpenError";
            this.BtnOpenError.Size = new System.Drawing.Size(113, 23);
            this.BtnOpenError.TabIndex = 3;
            this.BtnOpenError.Text = "Open error source file";
            this.BtnOpenError.UseVisualStyleBackColor = true;
            this.BtnOpenError.Click += new System.EventHandler(this.BtnOpenError_Click);
            // 
            // BtnNextError
            // 
            this.BtnNextError.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnNextError.Location = new System.Drawing.Point(281, 172);
            this.BtnNextError.Name = "BtnNextError";
            this.BtnNextError.Size = new System.Drawing.Size(101, 23);
            this.BtnNextError.TabIndex = 4;
            this.BtnNextError.Text = "Go to next error";
            this.BtnNextError.UseVisualStyleBackColor = true;
            this.BtnNextError.Click += new System.EventHandler(this.BtnNextError_Click);
            // 
            // TxtLog
            // 
            this.TxtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TxtLog.Location = new System.Drawing.Point(3, 3);
            this.TxtLog.Name = "TxtLog";
            this.TxtLog.ReadOnly = true;
            this.TxtLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.TxtLog.Size = new System.Drawing.Size(676, 163);
            this.TxtLog.TabIndex = 5;
            this.TxtLog.Text = "";
            this.TxtLog.SelectionChanged += new System.EventHandler(this.TxtLog_SelectionChanged);
            // 
            // BtnRepairAndRecompile
            // 
            this.BtnRepairAndRecompile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnRepairAndRecompile.Location = new System.Drawing.Point(474, 172);
            this.BtnRepairAndRecompile.Name = "BtnRepairAndRecompile";
            this.BtnRepairAndRecompile.Size = new System.Drawing.Size(124, 23);
            this.BtnRepairAndRecompile.TabIndex = 6;
            this.BtnRepairAndRecompile.Text = "Repair and recompile";
            this.BtnRepairAndRecompile.UseVisualStyleBackColor = true;
            this.BtnRepairAndRecompile.Click += new System.EventHandler(this.BtnRepairAndRecompile_Click);
            // 
            // BuildLogControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BtnRepairAndRecompile);
            this.Controls.Add(this.TxtLog);
            this.Controls.Add(this.BtnNextError);
            this.Controls.Add(this.BtnOpenError);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnClose);
            this.Name = "BuildLogControl";
            this.Size = new System.Drawing.Size(682, 198);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnClose;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnOpenError;
        private System.Windows.Forms.Button BtnNextError;
        private System.Windows.Forms.RichTextBox TxtLog;
        private System.Windows.Forms.Button BtnRepairAndRecompile;
    }
}
