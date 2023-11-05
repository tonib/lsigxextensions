namespace LSI.Packages.Extensiones.Comandos
{
    partial class EditObjectCallsTW
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
            this.TxtObjeto = new System.Windows.Forms.TextBox();
            this.LnkObjeto = new System.Windows.Forms.LinkLabel();
            this.BtnSelObjeto = new System.Windows.Forms.Button();
            this.LblParametro = new System.Windows.Forms.Label();
            this.CmbParametro = new System.Windows.Forms.ComboBox();
            this.LblNuevoValor = new System.Windows.Forms.Label();
            this.TxtValorParam = new System.Windows.Forms.TextBox();
            this.BtnRevisarCambios = new System.Windows.Forms.Button();
            this.BtnHacerCambios = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Grid = new LSI.Packages.Extensiones.Utilidades.UI.GridObjetos();
            this.EstadoCambiosObjeto = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LblObjetos = new System.Windows.Forms.Label();
            this.TxtCambios = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.LblEstado = new System.Windows.Forms.Label();
            this.ChkOk = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ChkAvisos = new System.Windows.Forms.CheckBox();
            this.ChkErrores = new System.Windows.Forms.CheckBox();
            this.LblTipoParametro = new System.Windows.Forms.Label();
            this.ChkValidarObjetos = new System.Windows.Forms.CheckBox();
            this.PicActividad = new System.Windows.Forms.PictureBox();
            this.BtnAyuda = new System.Windows.Forms.Button();
            this.TxtNuevoObjeto = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.BtnSelNuevoObjeto = new System.Windows.Forms.Button();
            this.CmbOperacion = new System.Windows.Forms.ComboBox();
            this.LnkNuevoObjeto = new System.Windows.Forms.LinkLabel();
            this.TxtStartName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BtnRefreshParms = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicActividad)).BeginInit();
            this.SuspendLayout();
            // 
            // TxtObjeto
            // 
            this.TxtObjeto.Location = new System.Drawing.Point(58, 3);
            this.TxtObjeto.Name = "TxtObjeto";
            this.TxtObjeto.Size = new System.Drawing.Size(128, 20);
            this.TxtObjeto.TabIndex = 1;
            this.TxtObjeto.Leave += new System.EventHandler(this.TxtObjeto_Leave);
            // 
            // LnkObjeto
            // 
            this.LnkObjeto.AutoSize = true;
            this.LnkObjeto.Location = new System.Drawing.Point(14, 6);
            this.LnkObjeto.Name = "LnkObjeto";
            this.LnkObjeto.Size = new System.Drawing.Size(38, 13);
            this.LnkObjeto.TabIndex = 0;
            this.LnkObjeto.TabStop = true;
            this.LnkObjeto.Text = "&Object";
            this.LnkObjeto.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkObjeto_LinkClicked);
            // 
            // BtnSelObjeto
            // 
            this.BtnSelObjeto.Location = new System.Drawing.Point(192, 1);
            this.BtnSelObjeto.Name = "BtnSelObjeto";
            this.BtnSelObjeto.Size = new System.Drawing.Size(28, 23);
            this.BtnSelObjeto.TabIndex = 2;
            this.BtnSelObjeto.Text = "...";
            this.BtnSelObjeto.UseVisualStyleBackColor = true;
            this.BtnSelObjeto.Click += new System.EventHandler(this.BtnSelObjeto_Click);
            // 
            // LblParametro
            // 
            this.LblParametro.AutoSize = true;
            this.LblParametro.Location = new System.Drawing.Point(14, 36);
            this.LblParametro.Name = "LblParametro";
            this.LblParametro.Size = new System.Drawing.Size(134, 13);
            this.LblParametro.TabIndex = 5;
            this.LblParametro.Text = "&Parameter to add / remove";
            // 
            // CmbParametro
            // 
            this.CmbParametro.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbParametro.FormattingEnabled = true;
            this.CmbParametro.Location = new System.Drawing.Point(203, 33);
            this.CmbParametro.Name = "CmbParametro";
            this.CmbParametro.Size = new System.Drawing.Size(190, 21);
            this.CmbParametro.TabIndex = 6;
            this.CmbParametro.SelectedIndexChanged += new System.EventHandler(this.CmbParametro_SelectedIndexChanged);
            // 
            // LblNuevoValor
            // 
            this.LblNuevoValor.AutoSize = true;
            this.LblNuevoValor.Location = new System.Drawing.Point(14, 63);
            this.LblNuevoValor.Name = "LblNuevoValor";
            this.LblNuevoValor.Size = new System.Drawing.Size(183, 13);
            this.LblNuevoValor.TabIndex = 8;
            this.LblNuevoValor.Text = "New parameter value (variable name)";
            // 
            // TxtValorParam
            // 
            this.TxtValorParam.Location = new System.Drawing.Point(203, 60);
            this.TxtValorParam.Name = "TxtValorParam";
            this.TxtValorParam.Size = new System.Drawing.Size(190, 20);
            this.TxtValorParam.TabIndex = 9;
            this.TxtValorParam.Leave += new System.EventHandler(this.TxtValorParam_Leave);
            // 
            // BtnRevisarCambios
            // 
            this.BtnRevisarCambios.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnRevisarCambios.Location = new System.Drawing.Point(546, 104);
            this.BtnRevisarCambios.Name = "BtnRevisarCambios";
            this.BtnRevisarCambios.Size = new System.Drawing.Size(107, 23);
            this.BtnRevisarCambios.TabIndex = 20;
            this.BtnRevisarCambios.Text = "Test changes";
            this.BtnRevisarCambios.UseVisualStyleBackColor = true;
            this.BtnRevisarCambios.Click += new System.EventHandler(this.BtnCambios_Click);
            // 
            // BtnHacerCambios
            // 
            this.BtnHacerCambios.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnHacerCambios.Location = new System.Drawing.Point(659, 104);
            this.BtnHacerCambios.Name = "BtnHacerCambios";
            this.BtnHacerCambios.Size = new System.Drawing.Size(94, 23);
            this.BtnHacerCambios.TabIndex = 21;
            this.BtnHacerCambios.Text = "Do changes";
            this.BtnHacerCambios.UseVisualStyleBackColor = true;
            this.BtnHacerCambios.Click += new System.EventHandler(this.BtnCambios_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.splitContainer1.Location = new System.Drawing.Point(17, 133);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel1.Controls.Add(this.Grid);
            this.splitContainer1.Panel1.Controls.Add(this.LblObjetos);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel2.Controls.Add(this.TxtCambios);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Size = new System.Drawing.Size(739, 559);
            this.splitContainer1.SplitterDistance = 277;
            this.splitContainer1.SplitterWidth = 10;
            this.splitContainer1.TabIndex = 22;
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
            this.EstadoCambiosObjeto});
            this.Grid.Location = new System.Drawing.Point(12, 25);
            this.Grid.Name = "Grid";
            this.Grid.Objetos = null;
            this.Grid.ReadOnly = true;
            this.Grid.RowHeadersVisible = false;
            this.Grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Grid.Size = new System.Drawing.Size(724, 239);
            this.Grid.TabIndex = 1;
            this.Grid.SelectionChanged += new System.EventHandler(this.Grid_SelectionChanged);
            // 
            // EstadoCambiosObjeto
            // 
            this.EstadoCambiosObjeto.DataPropertyName = "EstadoCambiosObjeto";
            this.EstadoCambiosObjeto.HeaderText = "Result";
            this.EstadoCambiosObjeto.Name = "EstadoCambiosObjeto";
            this.EstadoCambiosObjeto.ReadOnly = true;
            // 
            // LblObjetos
            // 
            this.LblObjetos.AutoSize = true;
            this.LblObjetos.Location = new System.Drawing.Point(9, 9);
            this.LblObjetos.Name = "LblObjetos";
            this.LblObjetos.Size = new System.Drawing.Size(89, 13);
            this.LblObjetos.TabIndex = 0;
            this.LblObjetos.Text = "Objects with calls";
            // 
            // TxtCambios
            // 
            this.TxtCambios.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtCambios.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtCambios.Location = new System.Drawing.Point(12, 25);
            this.TxtCambios.Multiline = true;
            this.TxtCambios.Name = "TxtCambios";
            this.TxtCambios.ReadOnly = true;
            this.TxtCambios.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.TxtCambios.Size = new System.Drawing.Size(724, 232);
            this.TxtCambios.TabIndex = 1;
            this.TxtCambios.WordWrap = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Object &changes";
            // 
            // LblEstado
            // 
            this.LblEstado.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblEstado.AutoSize = true;
            this.LblEstado.Location = new System.Drawing.Point(17, 701);
            this.LblEstado.Name = "LblEstado";
            this.LblEstado.Size = new System.Drawing.Size(54, 13);
            this.LblEstado.TabIndex = 23;
            this.LblEstado.Text = "LblEstado";
            // 
            // ChkOk
            // 
            this.ChkOk.AutoSize = true;
            this.ChkOk.Checked = true;
            this.ChkOk.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ChkOk.Location = new System.Drawing.Point(203, 110);
            this.ChkOk.Name = "ChkOk";
            this.ChkOk.Size = new System.Drawing.Size(41, 17);
            this.ChkOk.TabIndex = 16;
            this.ChkOk.Text = "O&K";
            this.ChkOk.UseVisualStyleBackColor = true;
            this.ChkOk.CheckedChanged += new System.EventHandler(this.ChkFiltroEstado_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 114);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "View objects:";
            // 
            // ChkAvisos
            // 
            this.ChkAvisos.AutoSize = true;
            this.ChkAvisos.Checked = true;
            this.ChkAvisos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ChkAvisos.Location = new System.Drawing.Point(250, 110);
            this.ChkAvisos.Name = "ChkAvisos";
            this.ChkAvisos.Size = new System.Drawing.Size(71, 17);
            this.ChkAvisos.TabIndex = 17;
            this.ChkAvisos.Text = "&Warnings";
            this.ChkAvisos.UseVisualStyleBackColor = true;
            this.ChkAvisos.CheckedChanged += new System.EventHandler(this.ChkFiltroEstado_CheckedChanged);
            // 
            // ChkErrores
            // 
            this.ChkErrores.AutoSize = true;
            this.ChkErrores.Checked = true;
            this.ChkErrores.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ChkErrores.Location = new System.Drawing.Point(327, 110);
            this.ChkErrores.Name = "ChkErrores";
            this.ChkErrores.Size = new System.Drawing.Size(53, 17);
            this.ChkErrores.TabIndex = 18;
            this.ChkErrores.Text = "&Errors";
            this.ChkErrores.UseVisualStyleBackColor = true;
            this.ChkErrores.CheckedChanged += new System.EventHandler(this.ChkFiltroEstado_CheckedChanged);
            // 
            // LblTipoParametro
            // 
            this.LblTipoParametro.AutoSize = true;
            this.LblTipoParametro.Location = new System.Drawing.Point(443, 36);
            this.LblTipoParametro.Name = "LblTipoParametro";
            this.LblTipoParametro.Size = new System.Drawing.Size(84, 13);
            this.LblTipoParametro.TabIndex = 7;
            this.LblTipoParametro.Text = "(Parameter type)";
            // 
            // ChkValidarObjetos
            // 
            this.ChkValidarObjetos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ChkValidarObjetos.AutoSize = true;
            this.ChkValidarObjetos.Location = new System.Drawing.Point(546, 86);
            this.ChkValidarObjetos.Name = "ChkValidarObjetos";
            this.ChkValidarObjetos.Size = new System.Drawing.Size(164, 17);
            this.ChkValidarObjetos.TabIndex = 19;
            this.ChkValidarObjetos.Text = "&Validate objects when testing";
            this.ChkValidarObjetos.UseVisualStyleBackColor = true;
            // 
            // PicActividad
            // 
            this.PicActividad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PicActividad.Enabled = false;
            this.PicActividad.Image = global::LSI.Packages.Extensiones.Resources.Actividad;
            this.PicActividad.Location = new System.Drawing.Point(734, 696);
            this.PicActividad.Name = "PicActividad";
            this.PicActividad.Size = new System.Drawing.Size(22, 18);
            this.PicActividad.TabIndex = 23;
            this.PicActividad.TabStop = false;
            this.PicActividad.Visible = false;
            // 
            // BtnAyuda
            // 
            this.BtnAyuda.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnAyuda.Image = global::LSI.Packages.Extensiones.Resources.HelpVS;
            this.BtnAyuda.Location = new System.Drawing.Point(725, 3);
            this.BtnAyuda.Name = "BtnAyuda";
            this.BtnAyuda.Size = new System.Drawing.Size(28, 23);
            this.BtnAyuda.TabIndex = 24;
            this.BtnAyuda.UseVisualStyleBackColor = true;
            this.BtnAyuda.Click += new System.EventHandler(this.BtnAyuda_Click);
            // 
            // TxtNuevoObjeto
            // 
            this.TxtNuevoObjeto.Location = new System.Drawing.Point(466, 60);
            this.TxtNuevoObjeto.Name = "TxtNuevoObjeto";
            this.TxtNuevoObjeto.Size = new System.Drawing.Size(179, 20);
            this.TxtNuevoObjeto.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(227, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(85, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Callers operation";
            // 
            // BtnSelNuevoObjeto
            // 
            this.BtnSelNuevoObjeto.Location = new System.Drawing.Point(651, 57);
            this.BtnSelNuevoObjeto.Name = "BtnSelNuevoObjeto";
            this.BtnSelNuevoObjeto.Size = new System.Drawing.Size(27, 23);
            this.BtnSelNuevoObjeto.TabIndex = 12;
            this.BtnSelNuevoObjeto.Text = "...";
            this.BtnSelNuevoObjeto.UseVisualStyleBackColor = true;
            this.BtnSelNuevoObjeto.Click += new System.EventHandler(this.BtnSelNuevoObjeto_Click);
            // 
            // CmbOperacion
            // 
            this.CmbOperacion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbOperacion.FormattingEnabled = true;
            this.CmbOperacion.Location = new System.Drawing.Point(318, 3);
            this.CmbOperacion.Name = "CmbOperacion";
            this.CmbOperacion.Size = new System.Drawing.Size(283, 21);
            this.CmbOperacion.TabIndex = 4;
            this.CmbOperacion.SelectedIndexChanged += new System.EventHandler(this.CmbOperacion_SelectedIndexChanged);
            // 
            // LnkNuevoObjeto
            // 
            this.LnkNuevoObjeto.AutoSize = true;
            this.LnkNuevoObjeto.Location = new System.Drawing.Point(399, 63);
            this.LnkNuevoObjeto.Name = "LnkNuevoObjeto";
            this.LnkNuevoObjeto.Size = new System.Drawing.Size(61, 13);
            this.LnkNuevoObjeto.TabIndex = 10;
            this.LnkNuevoObjeto.TabStop = true;
            this.LnkNuevoObjeto.Text = "New object";
            this.LnkNuevoObjeto.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkNuevoObjeto_LinkClicked);
            // 
            // TxtStartName
            // 
            this.TxtStartName.Location = new System.Drawing.Point(203, 84);
            this.TxtStartName.Name = "TxtStartName";
            this.TxtStartName.Size = new System.Drawing.Size(190, 20);
            this.TxtStartName.TabIndex = 14;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 91);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(160, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Callers with name that starts with";
            // 
            // BtnRefreshParms
            // 
            this.BtnRefreshParms.Image = global::LSI.Packages.Extensiones.Resources.refresh_16xLG;
            this.BtnRefreshParms.Location = new System.Drawing.Point(399, 33);
            this.BtnRefreshParms.Name = "BtnRefreshParms";
            this.BtnRefreshParms.Size = new System.Drawing.Size(38, 21);
            this.BtnRefreshParms.TabIndex = 25;
            this.BtnRefreshParms.UseVisualStyleBackColor = true;
            this.BtnRefreshParms.Click += new System.EventHandler(this.BtnRefreshParms_Click);
            // 
            // EdicionLlamadasObjeto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BtnRefreshParms);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TxtStartName);
            this.Controls.Add(this.LnkNuevoObjeto);
            this.Controls.Add(this.CmbOperacion);
            this.Controls.Add(this.BtnSelNuevoObjeto);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.TxtNuevoObjeto);
            this.Controls.Add(this.BtnAyuda);
            this.Controls.Add(this.PicActividad);
            this.Controls.Add(this.ChkValidarObjetos);
            this.Controls.Add(this.LblTipoParametro);
            this.Controls.Add(this.ChkErrores);
            this.Controls.Add(this.ChkAvisos);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ChkOk);
            this.Controls.Add(this.LblEstado);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.BtnHacerCambios);
            this.Controls.Add(this.BtnRevisarCambios);
            this.Controls.Add(this.TxtValorParam);
            this.Controls.Add(this.LblNuevoValor);
            this.Controls.Add(this.CmbParametro);
            this.Controls.Add(this.LblParametro);
            this.Controls.Add(this.BtnSelObjeto);
            this.Controls.Add(this.LnkObjeto);
            this.Controls.Add(this.TxtObjeto);
            this.Name = "EdicionLlamadasObjeto";
            this.Size = new System.Drawing.Size(771, 717);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicActividad)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TxtObjeto;
        private System.Windows.Forms.LinkLabel LnkObjeto;
        private System.Windows.Forms.Button BtnSelObjeto;
        private System.Windows.Forms.Label LblParametro;
        private System.Windows.Forms.ComboBox CmbParametro;
        private System.Windows.Forms.Label LblNuevoValor;
        private System.Windows.Forms.TextBox TxtValorParam;
        private System.Windows.Forms.Button BtnRevisarCambios;
        private System.Windows.Forms.Button BtnHacerCambios;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private LSI.Packages.Extensiones.Utilidades.UI.GridObjetos Grid;
        private System.Windows.Forms.Label LblObjetos;
        private System.Windows.Forms.TextBox TxtCambios;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label LblEstado;
        private System.Windows.Forms.CheckBox ChkOk;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox ChkAvisos;
        private System.Windows.Forms.CheckBox ChkErrores;
        private System.Windows.Forms.Label LblTipoParametro;
        private System.Windows.Forms.CheckBox ChkValidarObjetos;
        private System.Windows.Forms.PictureBox PicActividad;
        private System.Windows.Forms.Button BtnAyuda;
        private System.Windows.Forms.TextBox TxtNuevoObjeto;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button BtnSelNuevoObjeto;
        private System.Windows.Forms.ComboBox CmbOperacion;
        private System.Windows.Forms.LinkLabel LnkNuevoObjeto;
        private System.Windows.Forms.DataGridViewTextBoxColumn EstadoCambiosObjeto;
        private System.Windows.Forms.TextBox TxtStartName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BtnRefreshParms;

    }
}
