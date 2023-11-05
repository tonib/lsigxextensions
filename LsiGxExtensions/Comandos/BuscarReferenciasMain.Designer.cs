namespace LSI.Packages.Extensiones.Comandos
{
    partial class BuscarReferenciasMain
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
            this.label1 = new System.Windows.Forms.Label();
            this.TxtObjeto = new System.Windows.Forms.TextBox();
            this.LstObjetosBuscar = new System.Windows.Forms.ListBox();
            this.BtnSelObjeto = new System.Windows.Forms.Button();
            this.PicActividad = new System.Windows.Forms.PictureBox();
            this.LblEstado = new System.Windows.Forms.Label();
            this.BtnAgregar = new System.Windows.Forms.Button();
            this.BtnQuitar = new System.Windows.Forms.Button();
            this.BtnBuscar = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.Grid = new LSI.Packages.Extensiones.Utilidades.UI.GridObjetos();
            this.BtnPegar = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BtnQuitarTodos = new System.Windows.Forms.Button();
            this.BtnAyuda = new System.Windows.Forms.Button();
            this.ChkSearchRecursively = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.PicActividad)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Object";
            // 
            // TxtObjeto
            // 
            this.TxtObjeto.Location = new System.Drawing.Point(50, 19);
            this.TxtObjeto.Name = "TxtObjeto";
            this.TxtObjeto.Size = new System.Drawing.Size(257, 20);
            this.TxtObjeto.TabIndex = 1;
            this.TxtObjeto.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtObjeto_KeyDown);
            // 
            // LstObjetosBuscar
            // 
            this.LstObjetosBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LstObjetosBuscar.FormattingEnabled = true;
            this.LstObjetosBuscar.Location = new System.Drawing.Point(6, 45);
            this.LstObjetosBuscar.Name = "LstObjetosBuscar";
            this.LstObjetosBuscar.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LstObjetosBuscar.Size = new System.Drawing.Size(708, 82);
            this.LstObjetosBuscar.Sorted = true;
            this.LstObjetosBuscar.TabIndex = 5;
            this.LstObjetosBuscar.SelectedIndexChanged += new System.EventHandler(this.LstObjetosBuscar_SelectedIndexChanged);
            this.LstObjetosBuscar.DoubleClick += new System.EventHandler(this.LstObjetosBuscar_DoubleClick);
            // 
            // BtnSelObjeto
            // 
            this.BtnSelObjeto.Location = new System.Drawing.Point(394, 17);
            this.BtnSelObjeto.Name = "BtnSelObjeto";
            this.BtnSelObjeto.Size = new System.Drawing.Size(27, 22);
            this.BtnSelObjeto.TabIndex = 3;
            this.BtnSelObjeto.Text = "...";
            this.BtnSelObjeto.UseVisualStyleBackColor = true;
            this.BtnSelObjeto.Click += new System.EventHandler(this.BtnSelObjeto_Click);
            // 
            // PicActividad
            // 
            this.PicActividad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PicActividad.Enabled = false;
            this.PicActividad.Image = global::LSI.Packages.Extensiones.Resources.Actividad;
            this.PicActividad.Location = new System.Drawing.Point(704, 676);
            this.PicActividad.Name = "PicActividad";
            this.PicActividad.Size = new System.Drawing.Size(22, 18);
            this.PicActividad.TabIndex = 14;
            this.PicActividad.TabStop = false;
            this.PicActividad.Visible = false;
            // 
            // LblEstado
            // 
            this.LblEstado.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblEstado.AutoSize = true;
            this.LblEstado.Location = new System.Drawing.Point(3, 681);
            this.LblEstado.Name = "LblEstado";
            this.LblEstado.Size = new System.Drawing.Size(64, 13);
            this.LblEstado.TabIndex = 5;
            this.LblEstado.Text = "Searching...";
            // 
            // BtnAgregar
            // 
            this.BtnAgregar.Location = new System.Drawing.Point(313, 16);
            this.BtnAgregar.Name = "BtnAgregar";
            this.BtnAgregar.Size = new System.Drawing.Size(75, 23);
            this.BtnAgregar.TabIndex = 2;
            this.BtnAgregar.Text = "&Add";
            this.BtnAgregar.UseVisualStyleBackColor = true;
            this.BtnAgregar.Click += new System.EventHandler(this.BtnAgregar_Click);
            // 
            // BtnQuitar
            // 
            this.BtnQuitar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnQuitar.Location = new System.Drawing.Point(555, 133);
            this.BtnQuitar.Name = "BtnQuitar";
            this.BtnQuitar.Size = new System.Drawing.Size(159, 23);
            this.BtnQuitar.TabIndex = 8;
            this.BtnQuitar.Text = "&Remove selected";
            this.BtnQuitar.UseVisualStyleBackColor = true;
            this.BtnQuitar.Click += new System.EventHandler(this.BtnQuitar_Click);
            // 
            // BtnBuscar
            // 
            this.BtnBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnBuscar.Location = new System.Drawing.Point(561, 175);
            this.BtnBuscar.Name = "BtnBuscar";
            this.BtnBuscar.Size = new System.Drawing.Size(165, 23);
            this.BtnBuscar.TabIndex = 2;
            this.BtnBuscar.Text = "&Search";
            this.BtnBuscar.UseVisualStyleBackColor = true;
            this.BtnBuscar.Click += new System.EventHandler(this.BtnBuscar_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 184);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Search results:";
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
            this.Grid.Location = new System.Drawing.Point(6, 200);
            this.Grid.Name = "Grid";
            this.Grid.Objetos = null;
            this.Grid.ReadOnly = true;
            this.Grid.RowHeadersVisible = false;
            this.Grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Grid.Size = new System.Drawing.Size(720, 470);
            this.Grid.TabIndex = 4;
            // 
            // BtnPegar
            // 
            this.BtnPegar.Location = new System.Drawing.Point(6, 133);
            this.BtnPegar.Name = "BtnPegar";
            this.BtnPegar.Size = new System.Drawing.Size(196, 23);
            this.BtnPegar.TabIndex = 6;
            this.BtnPegar.Text = "&Paste objects from clipboard";
            this.BtnPegar.UseVisualStyleBackColor = true;
            this.BtnPegar.Click += new System.EventHandler(this.BtnPegar_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.BtnQuitarTodos);
            this.groupBox1.Controls.Add(this.BtnAyuda);
            this.groupBox1.Controls.Add(this.LstObjetosBuscar);
            this.groupBox1.Controls.Add(this.BtnPegar);
            this.groupBox1.Controls.Add(this.BtnQuitar);
            this.groupBox1.Controls.Add(this.BtnAgregar);
            this.groupBox1.Controls.Add(this.TxtObjeto);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.BtnSelObjeto);
            this.groupBox1.Location = new System.Drawing.Point(6, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(720, 166);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Objects to search";
            // 
            // BtnQuitarTodos
            // 
            this.BtnQuitarTodos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnQuitarTodos.Location = new System.Drawing.Point(474, 133);
            this.BtnQuitarTodos.Name = "BtnQuitarTodos";
            this.BtnQuitarTodos.Size = new System.Drawing.Size(75, 23);
            this.BtnQuitarTodos.TabIndex = 7;
            this.BtnQuitarTodos.Text = "Remove all";
            this.BtnQuitarTodos.UseVisualStyleBackColor = true;
            this.BtnQuitarTodos.Click += new System.EventHandler(this.BtnQuitarTodos_Click);
            // 
            // BtnAyuda
            // 
            this.BtnAyuda.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnAyuda.Image = global::LSI.Packages.Extensiones.Resources.HelpVS;
            this.BtnAyuda.Location = new System.Drawing.Point(687, 15);
            this.BtnAyuda.Name = "BtnAyuda";
            this.BtnAyuda.Size = new System.Drawing.Size(27, 26);
            this.BtnAyuda.TabIndex = 4;
            this.BtnAyuda.UseVisualStyleBackColor = true;
            this.BtnAyuda.Click += new System.EventHandler(this.BtnAyuda_Click);
            // 
            // ChkSearchRecursively
            // 
            this.ChkSearchRecursively.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ChkSearchRecursively.AutoSize = true;
            this.ChkSearchRecursively.Location = new System.Drawing.Point(412, 175);
            this.ChkSearchRecursively.Name = "ChkSearchRecursively";
            this.ChkSearchRecursively.Size = new System.Drawing.Size(143, 17);
            this.ChkSearchRecursively.TabIndex = 1;
            this.ChkSearchRecursively.Text = "Search mains &recursively";
            this.ChkSearchRecursively.UseVisualStyleBackColor = true;
            // 
            // BuscarReferenciasMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ChkSearchRecursively);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.BtnBuscar);
            this.Controls.Add(this.PicActividad);
            this.Controls.Add(this.LblEstado);
            this.Controls.Add(this.Grid);
            this.Name = "BuscarReferenciasMain";
            this.Size = new System.Drawing.Size(735, 699);
            ((System.ComponentModel.ISupportInitialize)(this.PicActividad)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private LSI.Packages.Extensiones.Utilidades.UI.GridObjetos Grid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TxtObjeto;
        private System.Windows.Forms.ListBox LstObjetosBuscar;
        private System.Windows.Forms.Button BtnSelObjeto;
        private System.Windows.Forms.PictureBox PicActividad;
        private System.Windows.Forms.Label LblEstado;
        private System.Windows.Forms.Button BtnAgregar;
        private System.Windows.Forms.Button BtnQuitar;
        private System.Windows.Forms.Button BtnBuscar;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button BtnPegar;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BtnAyuda;
        private System.Windows.Forms.Button BtnQuitarTodos;
        private System.Windows.Forms.CheckBox ChkSearchRecursively;
    }
}
