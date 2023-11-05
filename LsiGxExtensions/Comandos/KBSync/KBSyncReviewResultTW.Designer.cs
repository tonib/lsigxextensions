namespace LSI.Packages.Extensiones.Comandos.KBSync
{
    partial class KBSyncReviewResultTW
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
            this.Grid = new LSI.Packages.Extensiones.Utilidades.UI.GridObjetos();
            this.label1 = new System.Windows.Forms.Label();
            this.TxtExportFile = new System.Windows.Forms.TextBox();
            this.BtnSelFile = new System.Windows.Forms.Button();
            this.BtnReloadFile = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.CmbObjectsType = new System.Windows.Forms.ComboBox();
            this.BtnViewChanges = new System.Windows.Forms.Button();
            this.BtnHelp = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.TxtDestinationKb = new System.Windows.Forms.TextBox();
            this.LblStatus = new System.Windows.Forms.Label();
            this.ColNotExportableReason = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColStartVersionId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
            this.SuspendLayout();
            // 
            // Grid
            // 
            this.Grid.AllowUserToAddRows = false;
            this.Grid.AllowUserToDeleteRows = false;
            this.Grid.AllowUserToResizeRows = false;
            this.Grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Grid.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            this.Grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColStartVersionId,
            this.ColNotExportableReason});
            this.Grid.Location = new System.Drawing.Point(17, 93);
            this.Grid.Name = "Grid";
            this.Grid.Objetos = null;
            this.Grid.ReadOnly = true;
            this.Grid.RowHeadersVisible = false;
            this.Grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Grid.Size = new System.Drawing.Size(716, 503);
            this.Grid.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Versioning info file";
            // 
            // TxtExportFile
            // 
            this.TxtExportFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtExportFile.Location = new System.Drawing.Point(112, 12);
            this.TxtExportFile.Name = "TxtExportFile";
            this.TxtExportFile.ReadOnly = true;
            this.TxtExportFile.Size = new System.Drawing.Size(507, 20);
            this.TxtExportFile.TabIndex = 1;
            // 
            // BtnSelFile
            // 
            this.BtnSelFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSelFile.Location = new System.Drawing.Point(625, 10);
            this.BtnSelFile.Name = "BtnSelFile";
            this.BtnSelFile.Size = new System.Drawing.Size(27, 23);
            this.BtnSelFile.TabIndex = 2;
            this.BtnSelFile.Text = "...";
            this.BtnSelFile.UseVisualStyleBackColor = true;
            this.BtnSelFile.Click += new System.EventHandler(this.BtnSelFile_Click);
            // 
            // BtnReloadFile
            // 
            this.BtnReloadFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnReloadFile.Location = new System.Drawing.Point(658, 10);
            this.BtnReloadFile.Name = "BtnReloadFile";
            this.BtnReloadFile.Size = new System.Drawing.Size(75, 23);
            this.BtnReloadFile.TabIndex = 3;
            this.BtnReloadFile.Text = "Reload file";
            this.BtnReloadFile.UseVisualStyleBackColor = true;
            this.BtnReloadFile.Click += new System.EventHandler(this.BtnReloadFile_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "View objects";
            // 
            // CmbObjectsType
            // 
            this.CmbObjectsType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbObjectsType.FormattingEnabled = true;
            this.CmbObjectsType.Location = new System.Drawing.Point(112, 64);
            this.CmbObjectsType.Name = "CmbObjectsType";
            this.CmbObjectsType.Size = new System.Drawing.Size(226, 21);
            this.CmbObjectsType.TabIndex = 7;
            this.CmbObjectsType.SelectedIndexChanged += new System.EventHandler(this.CmbObjectsType_SelectedIndexChanged);
            // 
            // BtnViewChanges
            // 
            this.BtnViewChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnViewChanges.Location = new System.Drawing.Point(544, 64);
            this.BtnViewChanges.Name = "BtnViewChanges";
            this.BtnViewChanges.Size = new System.Drawing.Size(145, 23);
            this.BtnViewChanges.TabIndex = 8;
            this.BtnViewChanges.Text = "&Show changes to do";
            this.BtnViewChanges.UseVisualStyleBackColor = true;
            this.BtnViewChanges.Click += new System.EventHandler(this.BtnViewChanges_Click);
            // 
            // BtnHelp
            // 
            this.BtnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnHelp.Image = global::LSI.Packages.Extensiones.Resources.HelpVS;
            this.BtnHelp.Location = new System.Drawing.Point(695, 64);
            this.BtnHelp.Name = "BtnHelp";
            this.BtnHelp.Size = new System.Drawing.Size(38, 23);
            this.BtnHelp.TabIndex = 9;
            this.BtnHelp.UseVisualStyleBackColor = true;
            this.BtnHelp.Click += new System.EventHandler(this.BtnHelp_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 41);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Destination kb";
            // 
            // TxtDestinationKb
            // 
            this.TxtDestinationKb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtDestinationKb.Location = new System.Drawing.Point(112, 38);
            this.TxtDestinationKb.Name = "TxtDestinationKb";
            this.TxtDestinationKb.ReadOnly = true;
            this.TxtDestinationKb.Size = new System.Drawing.Size(621, 20);
            this.TxtDestinationKb.TabIndex = 5;
            // 
            // LblStatus
            // 
            this.LblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblStatus.AutoSize = true;
            this.LblStatus.Location = new System.Drawing.Point(14, 599);
            this.LblStatus.Name = "LblStatus";
            this.LblStatus.Size = new System.Drawing.Size(55, 13);
            this.LblStatus.TabIndex = 11;
            this.LblStatus.Text = "Status bar";
            // 
            // ColNotExportableReason
            // 
            this.ColNotExportableReason.DataPropertyName = "Comments";
            this.ColNotExportableReason.HeaderText = "Comments";
            this.ColNotExportableReason.Name = "ColNotExportableReason";
            this.ColNotExportableReason.ReadOnly = true;
            this.ColNotExportableReason.Width = 250;
            // 
            // ColStartVersionId
            // 
            this.ColStartVersionId.DataPropertyName = "VersionIdUpdateOrigin";
            this.ColStartVersionId.HeaderText = "Revision";
            this.ColStartVersionId.Name = "ColStartVersionId";
            this.ColStartVersionId.ReadOnly = true;
            this.ColStartVersionId.Width = 70;
            // 
            // KBSyncReviewResultTW
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Grid);
            this.Controls.Add(this.LblStatus);
            this.Controls.Add(this.TxtDestinationKb);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.BtnHelp);
            this.Controls.Add(this.BtnViewChanges);
            this.Controls.Add(this.CmbObjectsType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BtnReloadFile);
            this.Controls.Add(this.BtnSelFile);
            this.Controls.Add(this.TxtExportFile);
            this.Controls.Add(this.label1);
            this.Name = "KBSyncReviewResultTW";
            this.Size = new System.Drawing.Size(752, 621);
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private LSI.Packages.Extensiones.Utilidades.UI.GridObjetos Grid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TxtExportFile;
        private System.Windows.Forms.Button BtnSelFile;
        private System.Windows.Forms.Button BtnReloadFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox CmbObjectsType;
        private System.Windows.Forms.Button BtnViewChanges;
        private System.Windows.Forms.Button BtnHelp;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TxtDestinationKb;
        private System.Windows.Forms.Label LblStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColStartVersionId;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColNotExportableReason;
    }
}
