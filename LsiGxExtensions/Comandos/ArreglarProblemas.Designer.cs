namespace LSI.Packages.Extensiones.Comandos
{
    partial class ArreglarProblemas
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArreglarProblemas));
            this.BtnBorrar = new System.Windows.Forms.Button();
            this.BtnCancelar = new System.Windows.Forms.Button();
            this.LstVariablesBorrar = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ChkBorrarReglaHidden = new System.Windows.Forms.CheckBox();
            this.LblTituloN4 = new System.Windows.Forms.Label();
            this.LstVariablesN4 = new System.Windows.Forms.ListBox();
            this.ChkReplaceOldOperators = new System.Windows.Forms.CheckBox();
            this.ChkDeleteVariables = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // BtnBorrar
            // 
            this.BtnBorrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnBorrar.Location = new System.Drawing.Point(168, 491);
            this.BtnBorrar.Name = "BtnBorrar";
            this.BtnBorrar.Size = new System.Drawing.Size(75, 23);
            this.BtnBorrar.TabIndex = 2;
            this.BtnBorrar.Text = "&Fix object";
            this.BtnBorrar.UseVisualStyleBackColor = true;
            this.BtnBorrar.Click += new System.EventHandler(this.BtnBorrar_Click);
            // 
            // BtnCancelar
            // 
            this.BtnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancelar.Location = new System.Drawing.Point(249, 491);
            this.BtnCancelar.Name = "BtnCancelar";
            this.BtnCancelar.Size = new System.Drawing.Size(75, 23);
            this.BtnCancelar.TabIndex = 3;
            this.BtnCancelar.Text = "&Cancel";
            this.BtnCancelar.UseVisualStyleBackColor = true;
            this.BtnCancelar.Click += new System.EventHandler(this.BtnCancelar_Click);
            // 
            // LstVariablesBorrar
            // 
            this.LstVariablesBorrar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LstVariablesBorrar.BackColor = System.Drawing.SystemColors.Control;
            this.LstVariablesBorrar.FormattingEnabled = true;
            this.LstVariablesBorrar.Location = new System.Drawing.Point(12, 58);
            this.LstVariablesBorrar.Name = "LstVariablesBorrar";
            this.LstVariablesBorrar.Size = new System.Drawing.Size(473, 251);
            this.LstVariablesBorrar.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(251, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Are you sure you want to do the following changes?";
            // 
            // ChkBorrarReglaHidden
            // 
            this.ChkBorrarReglaHidden.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ChkBorrarReglaHidden.AutoSize = true;
            this.ChkBorrarReglaHidden.Location = new System.Drawing.Point(12, 445);
            this.ChkBorrarReglaHidden.Name = "ChkBorrarReglaHidden";
            this.ChkBorrarReglaHidden.Size = new System.Drawing.Size(378, 17);
            this.ChkBorrarReglaHidden.TabIndex = 5;
            this.ChkBorrarReglaHidden.Text = "Delete &Hidden rule, and add elements to the grid with property Visible=false";
            this.ChkBorrarReglaHidden.UseVisualStyleBackColor = true;
            this.ChkBorrarReglaHidden.Visible = false;
            // 
            // LblTituloN4
            // 
            this.LblTituloN4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblTituloN4.AutoSize = true;
            this.LblTituloN4.Location = new System.Drawing.Point(12, 328);
            this.LblTituloN4.Name = "LblTituloN4";
            this.LblTituloN4.Size = new System.Drawing.Size(223, 13);
            this.LblTituloN4.TabIndex = 7;
            this.LblTituloN4.Text = "Add \"N4\" to description of following variables:";
            // 
            // LstVariablesN4
            // 
            this.LstVariablesN4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LstVariablesN4.BackColor = System.Drawing.SystemColors.Control;
            this.LstVariablesN4.FormattingEnabled = true;
            this.LstVariablesN4.Location = new System.Drawing.Point(12, 344);
            this.LstVariablesN4.Name = "LstVariablesN4";
            this.LstVariablesN4.Size = new System.Drawing.Size(473, 95);
            this.LstVariablesN4.TabIndex = 8;
            // 
            // ChkReplaceOldOperators
            // 
            this.ChkReplaceOldOperators.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ChkReplaceOldOperators.AutoSize = true;
            this.ChkReplaceOldOperators.Location = new System.Drawing.Point(12, 468);
            this.ChkReplaceOldOperators.Name = "ChkReplaceOldOperators";
            this.ChkReplaceOldOperators.Size = new System.Drawing.Size(415, 17);
            this.ChkReplaceOldOperators.TabIndex = 9;
            this.ChkReplaceOldOperators.Text = "Replace &operators old syntax (\".AND.\", \".OR.\", \".NOT.\" by \"AND\", \"OR\", \"NOT\")";
            this.ChkReplaceOldOperators.UseVisualStyleBackColor = true;
            // 
            // ChkDeleteVariables
            // 
            this.ChkDeleteVariables.AutoSize = true;
            this.ChkDeleteVariables.Location = new System.Drawing.Point(12, 35);
            this.ChkDeleteVariables.Name = "ChkDeleteVariables";
            this.ChkDeleteVariables.Size = new System.Drawing.Size(187, 17);
            this.ChkDeleteVariables.TabIndex = 10;
            this.ChkDeleteVariables.Text = "&Delete following unused variables:";
            this.ChkDeleteVariables.UseVisualStyleBackColor = true;
            // 
            // ArreglarProblemas
            // 
            this.AcceptButton = this.BtnBorrar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancelar;
            this.ClientSize = new System.Drawing.Size(497, 526);
            this.Controls.Add(this.ChkDeleteVariables);
            this.Controls.Add(this.ChkReplaceOldOperators);
            this.Controls.Add(this.LstVariablesN4);
            this.Controls.Add(this.LblTituloN4);
            this.Controls.Add(this.ChkBorrarReglaHidden);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LstVariablesBorrar);
            this.Controls.Add(this.BtnCancelar);
            this.Controls.Add(this.BtnBorrar);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ArreglarProblemas";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Fix object problems";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.ArreglarProblemas_HelpButtonClicked);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnBorrar;
        private System.Windows.Forms.Button BtnCancelar;
        private System.Windows.Forms.ListBox LstVariablesBorrar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox ChkBorrarReglaHidden;
        private System.Windows.Forms.Label LblTituloN4;
        private System.Windows.Forms.ListBox LstVariablesN4;
        private System.Windows.Forms.CheckBox ChkReplaceOldOperators;
        private System.Windows.Forms.CheckBox ChkDeleteVariables;
    }
}