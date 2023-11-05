namespace LSI.Packages.Extensiones.Comandos.Build
{
    partial class WorkWithMains
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
            this.TabGenerators = new System.Windows.Forms.TabControl();
            this.BtnReloadMains = new System.Windows.Forms.Button();
            this.Split = new System.Windows.Forms.SplitContainer();
            this.TabBuilds = new System.Windows.Forms.TabControl();
            this.LnkTargetModel = new System.Windows.Forms.LinkLabel();
            this.TxtTargetModel = new System.Windows.Forms.TextBox();
            this.BtnHelp = new System.Windows.Forms.Button();
            this.Split.Panel1.SuspendLayout();
            this.Split.Panel2.SuspendLayout();
            this.Split.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabGenerators
            // 
            this.TabGenerators.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TabGenerators.Location = new System.Drawing.Point(3, 3);
            this.TabGenerators.Name = "TabGenerators";
            this.TabGenerators.SelectedIndex = 0;
            this.TabGenerators.Size = new System.Drawing.Size(757, 456);
            this.TabGenerators.TabIndex = 0;
            this.TabGenerators.SelectedIndexChanged += new System.EventHandler(this.TabGenerators_SelectedIndexChanged);
            // 
            // BtnReloadMains
            // 
            this.BtnReloadMains.Location = new System.Drawing.Point(4, 3);
            this.BtnReloadMains.Name = "BtnReloadMains";
            this.BtnReloadMains.Size = new System.Drawing.Size(96, 23);
            this.BtnReloadMains.TabIndex = 2;
            this.BtnReloadMains.Text = "Refres&h";
            this.BtnReloadMains.UseVisualStyleBackColor = true;
            this.BtnReloadMains.Click += new System.EventHandler(this.BtnReloadMains_Click);
            // 
            // Split
            // 
            this.Split.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Split.Location = new System.Drawing.Point(4, 32);
            this.Split.Name = "Split";
            this.Split.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // Split.Panel1
            // 
            this.Split.Panel1.Controls.Add(this.TabGenerators);
            // 
            // Split.Panel2
            // 
            this.Split.Panel2.Controls.Add(this.TabBuilds);
            this.Split.Panel2MinSize = 0;
            this.Split.Size = new System.Drawing.Size(763, 591);
            this.Split.SplitterDistance = 462;
            this.Split.SplitterWidth = 10;
            this.Split.TabIndex = 4;
            // 
            // TabBuilds
            // 
            this.TabBuilds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TabBuilds.Location = new System.Drawing.Point(7, 3);
            this.TabBuilds.Name = "TabBuilds";
            this.TabBuilds.SelectedIndex = 0;
            this.TabBuilds.Size = new System.Drawing.Size(749, 113);
            this.TabBuilds.TabIndex = 0;
            // 
            // LnkTargetModel
            // 
            this.LnkTargetModel.AutoSize = true;
            this.LnkTargetModel.Location = new System.Drawing.Point(106, 8);
            this.LnkTargetModel.Name = "LnkTargetModel";
            this.LnkTargetModel.Size = new System.Drawing.Size(83, 13);
            this.LnkTargetModel.TabIndex = 5;
            this.LnkTargetModel.TabStop = true;
            this.LnkTargetModel.Text = "Environment dir.";
            this.LnkTargetModel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkTargetModel_LinkClicked);
            // 
            // TxtTargetModel
            // 
            this.TxtTargetModel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtTargetModel.Location = new System.Drawing.Point(195, 5);
            this.TxtTargetModel.Name = "TxtTargetModel";
            this.TxtTargetModel.ReadOnly = true;
            this.TxtTargetModel.Size = new System.Drawing.Size(535, 20);
            this.TxtTargetModel.TabIndex = 6;
            // 
            // BtnHelp
            // 
            this.BtnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnHelp.Image = global::LSI.Packages.Extensiones.Resources.HelpVS;
            this.BtnHelp.Location = new System.Drawing.Point(736, 3);
            this.BtnHelp.Name = "BtnHelp";
            this.BtnHelp.Size = new System.Drawing.Size(28, 23);
            this.BtnHelp.TabIndex = 7;
            this.BtnHelp.UseVisualStyleBackColor = true;
            this.BtnHelp.Click += new System.EventHandler(this.BtnHelp_Click);
            // 
            // WorkWithMains
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BtnHelp);
            this.Controls.Add(this.TxtTargetModel);
            this.Controls.Add(this.LnkTargetModel);
            this.Controls.Add(this.Split);
            this.Controls.Add(this.BtnReloadMains);
            this.Name = "WorkWithMains";
            this.Size = new System.Drawing.Size(775, 626);
            this.Load += new System.EventHandler(this.WorkWithMains_Load);
            this.Split.Panel1.ResumeLayout(false);
            this.Split.Panel2.ResumeLayout(false);
            this.Split.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl TabGenerators;
        private System.Windows.Forms.Button BtnReloadMains;
        private System.Windows.Forms.SplitContainer Split;
        internal System.Windows.Forms.TabControl TabBuilds;
        private System.Windows.Forms.LinkLabel LnkTargetModel;
        private System.Windows.Forms.TextBox TxtTargetModel;
        private System.Windows.Forms.Button BtnHelp;
    }
}
