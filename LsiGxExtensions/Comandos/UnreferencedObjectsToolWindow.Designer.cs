namespace LSI.Packages.Extensiones.Comandos
{
    partial class UnreferencedObjectsToolWindow
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
            this.LblState = new System.Windows.Forms.Label();
            this.PicActivity = new System.Windows.Forms.PictureBox();
            this.GrpIgnoreFolders = new System.Windows.Forms.GroupBox();
            this.BtnDefaultIgnore = new System.Windows.Forms.Button();
            this.BtnRemoveSelected = new System.Windows.Forms.Button();
            this.BtnAdd = new System.Windows.Forms.Button();
            this.LstIgnoreFolders = new System.Windows.Forms.ListBox();
            this.BtnHelp = new System.Windows.Forms.Button();
            this.BtnSearch = new System.Windows.Forms.Button();
            this.BtnRemoveObjects = new System.Windows.Forms.Button();
            this.BtnReplaceVariablesAtr = new System.Windows.Forms.Button();
            this.ChkCallableObjects = new System.Windows.Forms.CheckBox();
            this.ChkAttributesNoTable = new System.Windows.Forms.CheckBox();
            this.ChkAttributesOnlyTrn = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CmbForms = new System.Windows.Forms.ComboBox();
            this.ChkReadOnlyAtrs = new System.Windows.Forms.CheckBox();
            this.chkIgnoreGxUser = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicActivity)).BeginInit();
            this.GrpIgnoreFolders.SuspendLayout();
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
            this.Grid.Location = new System.Drawing.Point(15, 151);
            this.Grid.Name = "Grid";
            this.Grid.Objetos = null;
            this.Grid.ReadOnly = true;
            this.Grid.RowHeadersVisible = false;
            this.Grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Grid.Size = new System.Drawing.Size(782, 372);
            this.Grid.TabIndex = 10;
            this.Grid.SelectionChanged += new System.EventHandler(this.Grid_SelectionChanged);
            // 
            // LblState
            // 
            this.LblState.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblState.AutoSize = true;
            this.LblState.Location = new System.Drawing.Point(12, 534);
            this.LblState.Name = "LblState";
            this.LblState.Size = new System.Drawing.Size(46, 13);
            this.LblState.TabIndex = 13;
            this.LblState.Text = "LblState";
            // 
            // PicActivity
            // 
            this.PicActivity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PicActivity.Enabled = false;
            this.PicActivity.Image = global::LSI.Packages.Extensiones.Resources.Actividad;
            this.PicActivity.Location = new System.Drawing.Point(775, 529);
            this.PicActivity.Name = "PicActivity";
            this.PicActivity.Size = new System.Drawing.Size(22, 18);
            this.PicActivity.TabIndex = 25;
            this.PicActivity.TabStop = false;
            this.PicActivity.Visible = false;
            // 
            // GrpIgnoreFolders
            // 
            this.GrpIgnoreFolders.Controls.Add(this.BtnDefaultIgnore);
            this.GrpIgnoreFolders.Controls.Add(this.BtnRemoveSelected);
            this.GrpIgnoreFolders.Controls.Add(this.BtnAdd);
            this.GrpIgnoreFolders.Controls.Add(this.LstIgnoreFolders);
            this.GrpIgnoreFolders.Location = new System.Drawing.Point(15, 3);
            this.GrpIgnoreFolders.Name = "GrpIgnoreFolders";
            this.GrpIgnoreFolders.Size = new System.Drawing.Size(338, 112);
            this.GrpIgnoreFolders.TabIndex = 7;
            this.GrpIgnoreFolders.TabStop = false;
            this.GrpIgnoreFolders.Text = "Folders / modules to ignore";
            // 
            // BtnDefaultIgnore
            // 
            this.BtnDefaultIgnore.Location = new System.Drawing.Point(256, 78);
            this.BtnDefaultIgnore.Name = "BtnDefaultIgnore";
            this.BtnDefaultIgnore.Size = new System.Drawing.Size(75, 23);
            this.BtnDefaultIgnore.TabIndex = 3;
            this.BtnDefaultIgnore.Text = "Default";
            this.BtnDefaultIgnore.UseVisualStyleBackColor = true;
            this.BtnDefaultIgnore.Click += new System.EventHandler(this.BtnDefaultIgnore_Click);
            // 
            // BtnRemoveSelected
            // 
            this.BtnRemoveSelected.Location = new System.Drawing.Point(256, 48);
            this.BtnRemoveSelected.Name = "BtnRemoveSelected";
            this.BtnRemoveSelected.Size = new System.Drawing.Size(75, 23);
            this.BtnRemoveSelected.TabIndex = 2;
            this.BtnRemoveSelected.Text = "&Remove";
            this.BtnRemoveSelected.UseVisualStyleBackColor = true;
            this.BtnRemoveSelected.Click += new System.EventHandler(this.BtnRemoveSelected_Click);
            // 
            // BtnAdd
            // 
            this.BtnAdd.Location = new System.Drawing.Point(256, 19);
            this.BtnAdd.Name = "BtnAdd";
            this.BtnAdd.Size = new System.Drawing.Size(75, 23);
            this.BtnAdd.TabIndex = 1;
            this.BtnAdd.Text = "&Add...";
            this.BtnAdd.UseVisualStyleBackColor = true;
            this.BtnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            // 
            // LstIgnoreFolders
            // 
            this.LstIgnoreFolders.FormattingEnabled = true;
            this.LstIgnoreFolders.Location = new System.Drawing.Point(6, 19);
            this.LstIgnoreFolders.Name = "LstIgnoreFolders";
            this.LstIgnoreFolders.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LstIgnoreFolders.Size = new System.Drawing.Size(243, 82);
            this.LstIgnoreFolders.Sorted = true;
            this.LstIgnoreFolders.TabIndex = 0;
            this.LstIgnoreFolders.SelectedIndexChanged += new System.EventHandler(this.LstIgnoreFolders_SelectedIndexChanged);
            // 
            // BtnHelp
            // 
            this.BtnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnHelp.Image = global::LSI.Packages.Extensiones.Resources.HelpVS;
            this.BtnHelp.Location = new System.Drawing.Point(759, 7);
            this.BtnHelp.Name = "BtnHelp";
            this.BtnHelp.Size = new System.Drawing.Size(38, 38);
            this.BtnHelp.TabIndex = 9;
            this.BtnHelp.UseVisualStyleBackColor = true;
            this.BtnHelp.Click += new System.EventHandler(this.BtnHelp_Click);
            // 
            // BtnSearch
            // 
            this.BtnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSearch.Location = new System.Drawing.Point(702, 121);
            this.BtnSearch.Name = "BtnSearch";
            this.BtnSearch.Size = new System.Drawing.Size(95, 23);
            this.BtnSearch.TabIndex = 8;
            this.BtnSearch.Text = "&Search";
            this.BtnSearch.UseVisualStyleBackColor = true;
            this.BtnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // BtnRemoveObjects
            // 
            this.BtnRemoveObjects.Location = new System.Drawing.Point(15, 121);
            this.BtnRemoveObjects.Name = "BtnRemoveObjects";
            this.BtnRemoveObjects.Size = new System.Drawing.Size(136, 23);
            this.BtnRemoveObjects.TabIndex = 11;
            this.BtnRemoveObjects.Text = "Delete selected objects";
            this.BtnRemoveObjects.UseVisualStyleBackColor = true;
            this.BtnRemoveObjects.Click += new System.EventHandler(this.BtnRemoveObjects_Click);
            // 
            // BtnReplaceVariablesAtr
            // 
            this.BtnReplaceVariablesAtr.Location = new System.Drawing.Point(157, 121);
            this.BtnReplaceVariablesAtr.Name = "BtnReplaceVariablesAtr";
            this.BtnReplaceVariablesAtr.Size = new System.Drawing.Size(192, 23);
            this.BtnReplaceVariablesAtr.TabIndex = 12;
            this.BtnReplaceVariablesAtr.Text = "Replace variables \'Atr. based on\' ...";
            this.BtnReplaceVariablesAtr.UseVisualStyleBackColor = true;
            this.BtnReplaceVariablesAtr.Click += new System.EventHandler(this.BtnReplaceVariablesAtr_Click);
            // 
            // ChkCallableObjects
            // 
            this.ChkCallableObjects.AutoSize = true;
            this.ChkCallableObjects.Checked = true;
            this.ChkCallableObjects.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ChkCallableObjects.Location = new System.Drawing.Point(359, 10);
            this.ChkCallableObjects.Name = "ChkCallableObjects";
            this.ChkCallableObjects.Size = new System.Drawing.Size(199, 17);
            this.ChkCallableObjects.TabIndex = 0;
            this.ChkCallableObjects.Text = "Check unreferenced callable objects";
            this.ChkCallableObjects.UseVisualStyleBackColor = true;
            this.ChkCallableObjects.CheckedChanged += new System.EventHandler(this.ChkCallableObjects_CheckedChanged);
            // 
            // ChkAttributesNoTable
            // 
            this.ChkAttributesNoTable.AutoSize = true;
            this.ChkAttributesNoTable.Checked = true;
            this.ChkAttributesNoTable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ChkAttributesNoTable.Location = new System.Drawing.Point(358, 54);
            this.ChkAttributesNoTable.Name = "ChkAttributesNoTable";
            this.ChkAttributesNoTable.Size = new System.Drawing.Size(133, 17);
            this.ChkAttributesNoTable.TabIndex = 2;
            this.ChkAttributesNoTable.Text = "Attributes with no table";
            this.ChkAttributesNoTable.UseVisualStyleBackColor = true;
            // 
            // ChkAttributesOnlyTrn
            // 
            this.ChkAttributesOnlyTrn.AutoSize = true;
            this.ChkAttributesOnlyTrn.Checked = true;
            this.ChkAttributesOnlyTrn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ChkAttributesOnlyTrn.Location = new System.Drawing.Point(359, 75);
            this.ChkAttributesOnlyTrn.Name = "ChkAttributesOnlyTrn";
            this.ChkAttributesOnlyTrn.Size = new System.Drawing.Size(163, 17);
            this.ChkAttributesOnlyTrn.TabIndex = 3;
            this.ChkAttributesOnlyTrn.Text = "Attributes only in transactions";
            this.ChkAttributesOnlyTrn.UseVisualStyleBackColor = true;
            this.ChkAttributesOnlyTrn.CheckedChanged += new System.EventHandler(this.ChkAttributesForm_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(522, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Forms to check";
            // 
            // CmbForms
            // 
            this.CmbForms.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbForms.FormattingEnabled = true;
            this.CmbForms.Location = new System.Drawing.Point(610, 73);
            this.CmbForms.Name = "CmbForms";
            this.CmbForms.Size = new System.Drawing.Size(81, 21);
            this.CmbForms.TabIndex = 5;
            // 
            // ChkReadOnlyAtrs
            // 
            this.ChkReadOnlyAtrs.AutoSize = true;
            this.ChkReadOnlyAtrs.Location = new System.Drawing.Point(359, 98);
            this.ChkReadOnlyAtrs.Name = "ChkReadOnlyAtrs";
            this.ChkReadOnlyAtrs.Size = new System.Drawing.Size(161, 17);
            this.ChkReadOnlyAtrs.TabIndex = 4;
            this.ChkReadOnlyAtrs.Text = "Read only attributes (SLOW)";
            this.ChkReadOnlyAtrs.UseVisualStyleBackColor = true;
            this.ChkReadOnlyAtrs.CheckedChanged += new System.EventHandler(this.ChkAttributesForm_CheckedChanged);
            // 
            // chkIgnoreGxUser
            // 
            this.chkIgnoreGxUser.AutoSize = true;
            this.chkIgnoreGxUser.Checked = true;
            this.chkIgnoreGxUser.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIgnoreGxUser.Location = new System.Drawing.Point(359, 31);
            this.chkIgnoreGxUser.Name = "chkIgnoreGxUser";
            this.chkIgnoreGxUser.Size = new System.Drawing.Size(201, 17);
            this.chkIgnoreGxUser.TabIndex = 1;
            this.chkIgnoreGxUser.Text = "Ignore last user modifier = \"Genexus\"";
            this.chkIgnoreGxUser.UseVisualStyleBackColor = true;
            // 
            // UnreferencedObjectsToolWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkIgnoreGxUser);
            this.Controls.Add(this.ChkReadOnlyAtrs);
            this.Controls.Add(this.CmbForms);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ChkAttributesOnlyTrn);
            this.Controls.Add(this.ChkAttributesNoTable);
            this.Controls.Add(this.ChkCallableObjects);
            this.Controls.Add(this.BtnReplaceVariablesAtr);
            this.Controls.Add(this.BtnRemoveObjects);
            this.Controls.Add(this.BtnHelp);
            this.Controls.Add(this.BtnSearch);
            this.Controls.Add(this.GrpIgnoreFolders);
            this.Controls.Add(this.PicActivity);
            this.Controls.Add(this.LblState);
            this.Controls.Add(this.Grid);
            this.Name = "UnreferencedObjectsToolWindow";
            this.Size = new System.Drawing.Size(809, 557);
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicActivity)).EndInit();
            this.GrpIgnoreFolders.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private LSI.Packages.Extensiones.Utilidades.UI.GridObjetos Grid;
        private System.Windows.Forms.Label LblState;
        private System.Windows.Forms.PictureBox PicActivity;
        private System.Windows.Forms.GroupBox GrpIgnoreFolders;
        private System.Windows.Forms.Button BtnRemoveSelected;
        private System.Windows.Forms.Button BtnAdd;
        private System.Windows.Forms.ListBox LstIgnoreFolders;
        private System.Windows.Forms.Button BtnHelp;
        private System.Windows.Forms.Button BtnSearch;
        private System.Windows.Forms.Button BtnRemoveObjects;
        private System.Windows.Forms.Button BtnReplaceVariablesAtr;
        private System.Windows.Forms.CheckBox ChkCallableObjects;
        private System.Windows.Forms.CheckBox ChkAttributesNoTable;
        private System.Windows.Forms.CheckBox ChkAttributesOnlyTrn;
        private System.Windows.Forms.Button BtnDefaultIgnore;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox CmbForms;
        private System.Windows.Forms.CheckBox ChkReadOnlyAtrs;
        private System.Windows.Forms.CheckBox chkIgnoreGxUser;
    }
}
