namespace LSI.Packages.Extensiones.Comandos
{
    partial class GxSourcesSearchTW
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
            this.PicBusy = new System.Windows.Forms.PictureBox();
            this.Grid = new LSI.Packages.Extensiones.Utilidades.UI.GridObjetos();
            this.LblState = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.TxtSeachText = new System.Windows.Forms.TextBox();
            this.CmbWhere = new System.Windows.Forms.ComboBox();
            this.ChRegExp = new System.Windows.Forms.CheckBox();
            this.ChkCase = new System.Windows.Forms.CheckBox();
            this.BtnAyuda = new System.Windows.Forms.Button();
            this.BtnSearch = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ChkRules = new System.Windows.Forms.CheckBox();
            this.ChkProcedure = new System.Windows.Forms.CheckBox();
            this.ChkEvents = new System.Windows.Forms.CheckBox();
            this.ChkConditions = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.PicBusy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
            this.SuspendLayout();
            // 
            // PicBusy
            // 
            this.PicBusy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PicBusy.Enabled = false;
            this.PicBusy.Image = global::LSI.Packages.Extensiones.Resources.Actividad;
            this.PicBusy.Location = new System.Drawing.Point(689, 545);
            this.PicBusy.Name = "PicBusy";
            this.PicBusy.Size = new System.Drawing.Size(22, 18);
            this.PicBusy.TabIndex = 27;
            this.PicBusy.TabStop = false;
            this.PicBusy.Visible = false;
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
            this.Grid.Location = new System.Drawing.Point(10, 56);
            this.Grid.Name = "Grid";
            this.Grid.Objetos = null;
            this.Grid.ReadOnly = true;
            this.Grid.RowHeadersVisible = false;
            this.Grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Grid.Size = new System.Drawing.Size(701, 483);
            this.Grid.TabIndex = 26;
            // 
            // LblState
            // 
            this.LblState.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblState.AutoSize = true;
            this.LblState.Location = new System.Drawing.Point(16, 545);
            this.LblState.Name = "LblState";
            this.LblState.Size = new System.Drawing.Size(46, 13);
            this.LblState.TabIndex = 25;
            this.LblState.Text = "LblState";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 28;
            this.label1.Text = "Search &text:";
            // 
            // TxtSeachText
            // 
            this.TxtSeachText.Location = new System.Drawing.Point(88, 7);
            this.TxtSeachText.Name = "TxtSeachText";
            this.TxtSeachText.Size = new System.Drawing.Size(182, 20);
            this.TxtSeachText.TabIndex = 29;
            // 
            // CmbWhere
            // 
            this.CmbWhere.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbWhere.FormattingEnabled = true;
            this.CmbWhere.Location = new System.Drawing.Point(375, 29);
            this.CmbWhere.Name = "CmbWhere";
            this.CmbWhere.Size = new System.Drawing.Size(139, 21);
            this.CmbWhere.TabIndex = 31;
            // 
            // ChRegExp
            // 
            this.ChRegExp.AutoSize = true;
            this.ChRegExp.Location = new System.Drawing.Point(375, 9);
            this.ChRegExp.Name = "ChRegExp";
            this.ChRegExp.Size = new System.Drawing.Size(122, 17);
            this.ChRegExp.TabIndex = 32;
            this.ChRegExp.Text = "Is &regular expression";
            this.ChRegExp.UseVisualStyleBackColor = true;
            // 
            // ChkCase
            // 
            this.ChkCase.AutoSize = true;
            this.ChkCase.Location = new System.Drawing.Point(503, 9);
            this.ChkCase.Name = "ChkCase";
            this.ChkCase.Size = new System.Drawing.Size(94, 17);
            this.ChkCase.TabIndex = 33;
            this.ChkCase.Text = "&Case sensitive";
            this.ChkCase.UseVisualStyleBackColor = true;
            // 
            // BtnAyuda
            // 
            this.BtnAyuda.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnAyuda.Image = global::LSI.Packages.Extensiones.Resources.HelpVS;
            this.BtnAyuda.Location = new System.Drawing.Point(673, 29);
            this.BtnAyuda.Name = "BtnAyuda";
            this.BtnAyuda.Size = new System.Drawing.Size(38, 23);
            this.BtnAyuda.TabIndex = 35;
            this.BtnAyuda.UseVisualStyleBackColor = true;
            this.BtnAyuda.Click += new System.EventHandler(this.BtnAyuda_Click);
            // 
            // BtnSearch
            // 
            this.BtnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSearch.Location = new System.Drawing.Point(541, 29);
            this.BtnSearch.Name = "BtnSearch";
            this.BtnSearch.Size = new System.Drawing.Size(126, 23);
            this.BtnSearch.TabIndex = 34;
            this.BtnSearch.Text = "&Search";
            this.BtnSearch.UseVisualStyleBackColor = true;
            this.BtnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(291, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 36;
            this.label3.Text = "Text options:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(42, 13);
            this.label4.TabIndex = 37;
            this.label4.Text = "Where:";
            // 
            // ChkRules
            // 
            this.ChkRules.AutoSize = true;
            this.ChkRules.Checked = true;
            this.ChkRules.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ChkRules.Location = new System.Drawing.Point(88, 33);
            this.ChkRules.Name = "ChkRules";
            this.ChkRules.Size = new System.Drawing.Size(53, 17);
            this.ChkRules.TabIndex = 38;
            this.ChkRules.Text = "&Rules";
            this.ChkRules.UseVisualStyleBackColor = true;
            // 
            // ChkProcedure
            // 
            this.ChkProcedure.AutoSize = true;
            this.ChkProcedure.Checked = true;
            this.ChkProcedure.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ChkProcedure.Location = new System.Drawing.Point(148, 33);
            this.ChkProcedure.Name = "ChkProcedure";
            this.ChkProcedure.Size = new System.Drawing.Size(75, 17);
            this.ChkProcedure.TabIndex = 39;
            this.ChkProcedure.Text = "&Procedure";
            this.ChkProcedure.UseVisualStyleBackColor = true;
            // 
            // ChkEvents
            // 
            this.ChkEvents.AutoSize = true;
            this.ChkEvents.Checked = true;
            this.ChkEvents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ChkEvents.Location = new System.Drawing.Point(229, 33);
            this.ChkEvents.Name = "ChkEvents";
            this.ChkEvents.Size = new System.Drawing.Size(59, 17);
            this.ChkEvents.TabIndex = 40;
            this.ChkEvents.Text = "&Events";
            this.ChkEvents.UseVisualStyleBackColor = true;
            // 
            // ChkConditions
            // 
            this.ChkConditions.AutoSize = true;
            this.ChkConditions.Checked = true;
            this.ChkConditions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ChkConditions.Location = new System.Drawing.Point(294, 33);
            this.ChkConditions.Name = "ChkConditions";
            this.ChkConditions.Size = new System.Drawing.Size(75, 17);
            this.ChkConditions.TabIndex = 41;
            this.ChkConditions.Text = "&Conditions";
            this.ChkConditions.UseVisualStyleBackColor = true;
            // 
            // GxSourcesSearchTW
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ChkConditions);
            this.Controls.Add(this.ChkEvents);
            this.Controls.Add(this.ChkProcedure);
            this.Controls.Add(this.ChkRules);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.BtnAyuda);
            this.Controls.Add(this.BtnSearch);
            this.Controls.Add(this.ChkCase);
            this.Controls.Add(this.ChRegExp);
            this.Controls.Add(this.CmbWhere);
            this.Controls.Add(this.TxtSeachText);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PicBusy);
            this.Controls.Add(this.Grid);
            this.Controls.Add(this.LblState);
            this.Name = "GxSourcesSearchTW";
            this.Size = new System.Drawing.Size(714, 566);
            ((System.ComponentModel.ISupportInitialize)(this.PicBusy)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox PicBusy;
        private LSI.Packages.Extensiones.Utilidades.UI.GridObjetos Grid;
        private System.Windows.Forms.Label LblState;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TxtSeachText;
        private System.Windows.Forms.ComboBox CmbWhere;
        private System.Windows.Forms.CheckBox ChRegExp;
        private System.Windows.Forms.CheckBox ChkCase;
        private System.Windows.Forms.Button BtnAyuda;
        private System.Windows.Forms.Button BtnSearch;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox ChkRules;
        private System.Windows.Forms.CheckBox ChkProcedure;
        private System.Windows.Forms.CheckBox ChkEvents;
        private System.Windows.Forms.CheckBox ChkConditions;

    }
}
