namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.ModelGeneration
{
    partial class GenerateModelTW
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
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
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
			this.btnGenerate = new System.Windows.Forms.Button();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.lblNExport = new System.Windows.Forms.Label();
			this.txtNExport = new System.Windows.Forms.TextBox();
			this.lblNEpochs = new System.Windows.Forms.Label();
			this.txtNEpochs = new System.Windows.Forms.TextBox();
			this.txtModelDirectory = new System.Windows.Forms.TextBox();
			this.lblPercentageEvaluation = new System.Windows.Forms.Label();
			this.txtPercentageEvaluation = new System.Windows.Forms.TextBox();
			this.lnkModel = new System.Windows.Forms.LinkLabel();
			this.btnSelModel = new System.Windows.Forms.Button();
			this.lblMaxHash = new System.Windows.Forms.Label();
			this.txtMaxHash = new System.Windows.Forms.TextBox();
			this.lblRnnSize = new System.Windows.Forms.Label();
			this.txtRnnSize = new System.Windows.Forms.TextBox();
			this.btnExport = new System.Windows.Forms.Button();
			this.lnkTensorboard = new System.Windows.Forms.LinkLabel();
			this.lblSeqLength = new System.Windows.Forms.Label();
			this.txtSeqLength = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cmbModelType = new System.Windows.Forms.ComboBox();
			this.label8 = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.cmbCellType = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.lblDropout = new System.Windows.Forms.Label();
			this.txtDropout = new System.Windows.Forms.TextBox();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.txtGptEmbDropout = new System.Windows.Forms.TextBox();
			this.lblGptEmbDropout = new System.Windows.Forms.Label();
			this.txtGptAttHeads = new System.Windows.Forms.TextBox();
			this.lblGptAttHeads = new System.Windows.Forms.Label();
			this.txtGptAttDropout = new System.Windows.Forms.TextBox();
			this.lblGptAttDropout = new System.Windows.Forms.Label();
			this.txtGptResidualDropout = new System.Windows.Forms.TextBox();
			this.lblGptResDroput = new System.Windows.Forms.Label();
			this.txtGptEmbeddingSize = new System.Windows.Forms.TextBox();
			this.lblGptEmbSize = new System.Windows.Forms.Label();
			this.txtGptNLayers = new System.Windows.Forms.TextBox();
			this.lblGptNLayers = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.txtLogEach = new System.Windows.Forms.TextBox();
			this.lblLogEach = new System.Windows.Forms.Label();
			this.txtMaxBatchEpoch = new System.Windows.Forms.TextBox();
			this.lblMaxBachEpoch = new System.Windows.Forms.Label();
			this.btnEval = new System.Windows.Forms.Button();
			this.btnDebugEval = new System.Windows.Forms.Button();
			this.BtnAyuda = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.btnExportKbObjects = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnGenerate
			// 
			this.btnGenerate.Location = new System.Drawing.Point(9, 3);
			this.btnGenerate.Name = "btnGenerate";
			this.btnGenerate.Size = new System.Drawing.Size(161, 26);
			this.btnGenerate.TabIndex = 0;
			this.btnGenerate.Text = "Train new model";
			this.btnGenerate.UseVisualStyleBackColor = true;
			this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
			// 
			// txtLog
			// 
			this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtLog.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtLog.Location = new System.Drawing.Point(8, 381);
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ReadOnly = true;
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtLog.Size = new System.Drawing.Size(846, 159);
			this.txtLog.TabIndex = 12;
			// 
			// lblNExport
			// 
			this.lblNExport.AutoSize = true;
			this.lblNExport.Location = new System.Drawing.Point(474, 50);
			this.lblNExport.Name = "lblNExport";
			this.lblNExport.Size = new System.Drawing.Size(168, 13);
			this.lblNExport.TabIndex = 6;
			this.lblNExport.Text = "N. objects to export per type(0=all)";
			// 
			// txtNExport
			// 
			this.txtNExport.Location = new System.Drawing.Point(661, 47);
			this.txtNExport.Name = "txtNExport";
			this.txtNExport.Size = new System.Drawing.Size(100, 20);
			this.txtNExport.TabIndex = 7;
			// 
			// lblNEpochs
			// 
			this.lblNEpochs.AutoSize = true;
			this.lblNEpochs.Location = new System.Drawing.Point(14, 26);
			this.lblNEpochs.Name = "lblNEpochs";
			this.lblNEpochs.Size = new System.Drawing.Size(81, 13);
			this.lblNEpochs.TabIndex = 0;
			this.lblNEpochs.Text = "N. max. epochs";
			// 
			// txtNEpochs
			// 
			this.txtNEpochs.Location = new System.Drawing.Point(101, 22);
			this.txtNEpochs.Name = "txtNEpochs";
			this.txtNEpochs.Size = new System.Drawing.Size(66, 20);
			this.txtNEpochs.TabIndex = 1;
			// 
			// txtModelDirectory
			// 
			this.txtModelDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtModelDirectory.Location = new System.Drawing.Point(89, 57);
			this.txtModelDirectory.Name = "txtModelDirectory";
			this.txtModelDirectory.Size = new System.Drawing.Size(718, 20);
			this.txtModelDirectory.TabIndex = 7;
			this.txtModelDirectory.TextChanged += new System.EventHandler(this.txtModelDirectory_TextChanged);
			// 
			// lblPercentageEvaluation
			// 
			this.lblPercentageEvaluation.AutoSize = true;
			this.lblPercentageEvaluation.Location = new System.Drawing.Point(496, 30);
			this.lblPercentageEvaluation.Name = "lblPercentageEvaluation";
			this.lblPercentageEvaluation.Size = new System.Drawing.Size(119, 13);
			this.lblPercentageEvaluation.TabIndex = 4;
			this.lblPercentageEvaluation.Text = "% objects for evaluation";
			// 
			// txtPercentageEvaluation
			// 
			this.txtPercentageEvaluation.Location = new System.Drawing.Point(621, 23);
			this.txtPercentageEvaluation.Name = "txtPercentageEvaluation";
			this.txtPercentageEvaluation.Size = new System.Drawing.Size(66, 20);
			this.txtPercentageEvaluation.TabIndex = 5;
			// 
			// lnkModel
			// 
			this.lnkModel.AutoSize = true;
			this.lnkModel.Location = new System.Drawing.Point(4, 60);
			this.lnkModel.Name = "lnkModel";
			this.lnkModel.Size = new System.Drawing.Size(79, 13);
			this.lnkModel.TabIndex = 6;
			this.lnkModel.TabStop = true;
			this.lnkModel.Text = "Model directory";
			this.lnkModel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkModel_LinkClicked);
			// 
			// btnSelModel
			// 
			this.btnSelModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSelModel.Location = new System.Drawing.Point(813, 57);
			this.btnSelModel.Name = "btnSelModel";
			this.btnSelModel.Size = new System.Drawing.Size(39, 23);
			this.btnSelModel.TabIndex = 8;
			this.btnSelModel.Text = "...";
			this.btnSelModel.UseVisualStyleBackColor = true;
			this.btnSelModel.Click += new System.EventHandler(this.btnSelModel_Click);
			// 
			// lblMaxHash
			// 
			this.lblMaxHash.AutoSize = true;
			this.lblMaxHash.Location = new System.Drawing.Point(257, 26);
			this.lblMaxHash.Name = "lblMaxHash";
			this.lblMaxHash.Size = new System.Drawing.Size(134, 13);
			this.lblMaxHash.TabIndex = 2;
			this.lblMaxHash.Text = "Max. hash value for names";
			// 
			// txtMaxHash
			// 
			this.txtMaxHash.Location = new System.Drawing.Point(424, 23);
			this.txtMaxHash.Name = "txtMaxHash";
			this.txtMaxHash.Size = new System.Drawing.Size(66, 20);
			this.txtMaxHash.TabIndex = 3;
			// 
			// lblRnnSize
			// 
			this.lblRnnSize.AutoSize = true;
			this.lblRnnSize.Location = new System.Drawing.Point(5, 16);
			this.lblRnnSize.Name = "lblRnnSize";
			this.lblRnnSize.Size = new System.Drawing.Size(101, 13);
			this.lblRnnSize.TabIndex = 0;
			this.lblRnnSize.Text = "RNN layer elements";
			// 
			// txtRnnSize
			// 
			this.txtRnnSize.Location = new System.Drawing.Point(168, 13);
			this.txtRnnSize.Name = "txtRnnSize";
			this.txtRnnSize.Size = new System.Drawing.Size(66, 20);
			this.txtRnnSize.TabIndex = 1;
			// 
			// btnExport
			// 
			this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExport.Location = new System.Drawing.Point(724, 3);
			this.btnExport.Name = "btnExport";
			this.btnExport.Size = new System.Drawing.Size(95, 26);
			this.btnExport.TabIndex = 4;
			this.btnExport.Text = "Export model";
			this.btnExport.UseVisualStyleBackColor = true;
			this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
			// 
			// lnkTensorboard
			// 
			this.lnkTensorboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lnkTensorboard.AutoSize = true;
			this.lnkTensorboard.Location = new System.Drawing.Point(750, 32);
			this.lnkTensorboard.Name = "lnkTensorboard";
			this.lnkTensorboard.Size = new System.Drawing.Size(102, 13);
			this.lnkTensorboard.TabIndex = 5;
			this.lnkTensorboard.TabStop = true;
			this.lnkTensorboard.Text = "Launch tensorboard";
			this.lnkTensorboard.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkTensorboard_LinkClicked);
			// 
			// lblSeqLength
			// 
			this.lblSeqLength.AutoSize = true;
			this.lblSeqLength.Location = new System.Drawing.Point(8, 22);
			this.lblSeqLength.Name = "lblSeqLength";
			this.lblSeqLength.Size = new System.Drawing.Size(141, 13);
			this.lblSeqLength.TabIndex = 0;
			this.lblSeqLength.Text = "Sequence length (n. tokens)";
			// 
			// txtSeqLength
			// 
			this.txtSeqLength.Location = new System.Drawing.Point(181, 19);
			this.txtSeqLength.Name = "txtSeqLength";
			this.txtSeqLength.Size = new System.Drawing.Size(66, 20);
			this.txtSeqLength.TabIndex = 1;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.cmbModelType);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.tabControl1);
			this.groupBox1.Location = new System.Drawing.Point(9, 147);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(844, 141);
			this.groupBox1.TabIndex = 10;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Model definition";
			// 
			// cmbModelType
			// 
			this.cmbModelType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbModelType.FormattingEnabled = true;
			this.cmbModelType.Location = new System.Drawing.Point(93, 19);
			this.cmbModelType.Name = "cmbModelType";
			this.cmbModelType.Size = new System.Drawing.Size(306, 21);
			this.cmbModelType.TabIndex = 1;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(12, 22);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(59, 13);
			this.label8.TabIndex = 0;
			this.label8.Text = "Model type";
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Location = new System.Drawing.Point(11, 46);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(827, 89);
			this.tabControl1.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.lblRnnSize);
			this.tabPage1.Controls.Add(this.cmbCellType);
			this.tabPage1.Controls.Add(this.txtRnnSize);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.lblDropout);
			this.tabPage1.Controls.Add(this.txtDropout);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(819, 63);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "RNN";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// cmbCellType
			// 
			this.cmbCellType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbCellType.FormattingEnabled = true;
			this.cmbCellType.Items.AddRange(new object[] {
            "GRU",
            "LSTM"});
			this.cmbCellType.Location = new System.Drawing.Point(322, 13);
			this.cmbCellType.Name = "cmbCellType";
			this.cmbCellType.Size = new System.Drawing.Size(121, 21);
			this.cmbCellType.TabIndex = 3;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(243, 19);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(73, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "RNN cell type";
			// 
			// lblDropout
			// 
			this.lblDropout.AutoSize = true;
			this.lblDropout.Location = new System.Drawing.Point(449, 19);
			this.lblDropout.Name = "lblDropout";
			this.lblDropout.Size = new System.Drawing.Size(125, 13);
			this.lblDropout.TabIndex = 4;
			this.lblDropout.Text = "Dropout (fraction to drop)";
			// 
			// txtDropout
			// 
			this.txtDropout.Location = new System.Drawing.Point(580, 15);
			this.txtDropout.Name = "txtDropout";
			this.txtDropout.Size = new System.Drawing.Size(66, 20);
			this.txtDropout.TabIndex = 5;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.txtGptEmbDropout);
			this.tabPage2.Controls.Add(this.lblGptEmbDropout);
			this.tabPage2.Controls.Add(this.txtGptAttHeads);
			this.tabPage2.Controls.Add(this.lblGptAttHeads);
			this.tabPage2.Controls.Add(this.txtGptAttDropout);
			this.tabPage2.Controls.Add(this.lblGptAttDropout);
			this.tabPage2.Controls.Add(this.txtGptResidualDropout);
			this.tabPage2.Controls.Add(this.lblGptResDroput);
			this.tabPage2.Controls.Add(this.txtGptEmbeddingSize);
			this.tabPage2.Controls.Add(this.lblGptEmbSize);
			this.tabPage2.Controls.Add(this.txtGptNLayers);
			this.tabPage2.Controls.Add(this.lblGptNLayers);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(819, 63);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "GPT";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// txtGptEmbDropout
			// 
			this.txtGptEmbDropout.Location = new System.Drawing.Point(297, 33);
			this.txtGptEmbDropout.Name = "txtGptEmbDropout";
			this.txtGptEmbDropout.Size = new System.Drawing.Size(100, 20);
			this.txtGptEmbDropout.TabIndex = 10;
			// 
			// lblGptEmbDropout
			// 
			this.lblGptEmbDropout.AutoSize = true;
			this.lblGptEmbDropout.Location = new System.Drawing.Point(210, 36);
			this.lblGptEmbDropout.Name = "lblGptEmbDropout";
			this.lblGptEmbDropout.Size = new System.Drawing.Size(82, 13);
			this.lblGptEmbDropout.TabIndex = 9;
			this.lblGptEmbDropout.Text = "Embed. dropout";
			// 
			// txtGptAttHeads
			// 
			this.txtGptAttHeads.Location = new System.Drawing.Point(104, 33);
			this.txtGptAttHeads.Name = "txtGptAttHeads";
			this.txtGptAttHeads.Size = new System.Drawing.Size(100, 20);
			this.txtGptAttHeads.TabIndex = 8;
			// 
			// lblGptAttHeads
			// 
			this.lblGptAttHeads.AutoSize = true;
			this.lblGptAttHeads.Location = new System.Drawing.Point(6, 36);
			this.lblGptAttHeads.Name = "lblGptAttHeads";
			this.lblGptAttHeads.Size = new System.Drawing.Size(94, 13);
			this.lblGptAttHeads.TabIndex = 8;
			this.lblGptAttHeads.Text = "N. attention heads";
			// 
			// txtGptAttDropout
			// 
			this.txtGptAttDropout.Location = new System.Drawing.Point(691, 7);
			this.txtGptAttDropout.Name = "txtGptAttDropout";
			this.txtGptAttDropout.Size = new System.Drawing.Size(100, 20);
			this.txtGptAttDropout.TabIndex = 7;
			// 
			// lblGptAttDropout
			// 
			this.lblGptAttDropout.AutoSize = true;
			this.lblGptAttDropout.Location = new System.Drawing.Point(603, 10);
			this.lblGptAttDropout.Name = "lblGptAttDropout";
			this.lblGptAttDropout.Size = new System.Drawing.Size(82, 13);
			this.lblGptAttDropout.TabIndex = 6;
			this.lblGptAttDropout.Text = "Attention droput";
			// 
			// txtGptResidualDropout
			// 
			this.txtGptResidualDropout.Location = new System.Drawing.Point(496, 7);
			this.txtGptResidualDropout.Name = "txtGptResidualDropout";
			this.txtGptResidualDropout.Size = new System.Drawing.Size(100, 20);
			this.txtGptResidualDropout.TabIndex = 5;
			// 
			// lblGptResDroput
			// 
			this.lblGptResDroput.AutoSize = true;
			this.lblGptResDroput.Location = new System.Drawing.Point(403, 10);
			this.lblGptResDroput.Name = "lblGptResDroput";
			this.lblGptResDroput.Size = new System.Drawing.Size(87, 13);
			this.lblGptResDroput.TabIndex = 4;
			this.lblGptResDroput.Text = "Residual dropout";
			// 
			// txtGptEmbeddingSize
			// 
			this.txtGptEmbeddingSize.Location = new System.Drawing.Point(297, 7);
			this.txtGptEmbeddingSize.Name = "txtGptEmbeddingSize";
			this.txtGptEmbeddingSize.Size = new System.Drawing.Size(100, 20);
			this.txtGptEmbeddingSize.TabIndex = 3;
			// 
			// lblGptEmbSize
			// 
			this.lblGptEmbSize.AutoSize = true;
			this.lblGptEmbSize.Location = new System.Drawing.Point(210, 10);
			this.lblGptEmbSize.Name = "lblGptEmbSize";
			this.lblGptEmbSize.Size = new System.Drawing.Size(81, 13);
			this.lblGptEmbSize.TabIndex = 2;
			this.lblGptEmbSize.Text = "Embedding size";
			// 
			// txtGptNLayers
			// 
			this.txtGptNLayers.Location = new System.Drawing.Point(104, 7);
			this.txtGptNLayers.Name = "txtGptNLayers";
			this.txtGptNLayers.Size = new System.Drawing.Size(100, 20);
			this.txtGptNLayers.TabIndex = 1;
			// 
			// lblGptNLayers
			// 
			this.lblGptNLayers.AutoSize = true;
			this.lblGptNLayers.Location = new System.Drawing.Point(8, 10);
			this.lblGptNLayers.Name = "lblGptNLayers";
			this.lblGptNLayers.Size = new System.Drawing.Size(45, 13);
			this.lblGptNLayers.TabIndex = 0;
			this.lblGptNLayers.Text = "N layers";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.txtLogEach);
			this.groupBox2.Controls.Add(this.lblLogEach);
			this.groupBox2.Controls.Add(this.txtMaxBatchEpoch);
			this.groupBox2.Controls.Add(this.lblMaxBachEpoch);
			this.groupBox2.Controls.Add(this.lblNExport);
			this.groupBox2.Controls.Add(this.txtNExport);
			this.groupBox2.Controls.Add(this.lblNEpochs);
			this.groupBox2.Controls.Add(this.txtNEpochs);
			this.groupBox2.Location = new System.Drawing.Point(9, 294);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(845, 81);
			this.groupBox2.TabIndex = 11;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Training";
			// 
			// txtLogEach
			// 
			this.txtLogEach.Location = new System.Drawing.Point(661, 21);
			this.txtLogEach.Name = "txtLogEach";
			this.txtLogEach.Size = new System.Drawing.Size(100, 20);
			this.txtLogEach.TabIndex = 5;
			// 
			// lblLogEach
			// 
			this.lblLogEach.AutoSize = true;
			this.lblLogEach.Location = new System.Drawing.Point(474, 26);
			this.lblLogEach.Name = "lblLogEach";
			this.lblLogEach.Size = new System.Drawing.Size(180, 13);
			this.lblLogEach.TabIndex = 4;
			this.lblLogEach.Text = "Log msg. each n. batches (0=no log)";
			// 
			// txtMaxBatchEpoch
			// 
			this.txtMaxBatchEpoch.Location = new System.Drawing.Point(367, 22);
			this.txtMaxBatchEpoch.Name = "txtMaxBatchEpoch";
			this.txtMaxBatchEpoch.Size = new System.Drawing.Size(100, 20);
			this.txtMaxBatchEpoch.TabIndex = 3;
			// 
			// lblMaxBachEpoch
			// 
			this.lblMaxBachEpoch.AutoSize = true;
			this.lblMaxBachEpoch.Location = new System.Drawing.Point(176, 26);
			this.lblMaxBachEpoch.Name = "lblMaxBachEpoch";
			this.lblMaxBachEpoch.Size = new System.Drawing.Size(185, 13);
			this.lblMaxBachEpoch.TabIndex = 2;
			this.lblMaxBachEpoch.Text = "Limit epoch to n.batches (0=unlimited)";
			// 
			// btnEval
			// 
			this.btnEval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnEval.Location = new System.Drawing.Point(643, 3);
			this.btnEval.Name = "btnEval";
			this.btnEval.Size = new System.Drawing.Size(75, 26);
			this.btnEval.TabIndex = 3;
			this.btnEval.Text = "Evaluate";
			this.btnEval.UseVisualStyleBackColor = true;
			this.btnEval.Click += new System.EventHandler(this.btnEval_Click);
			// 
			// btnDebugEval
			// 
			this.btnDebugEval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDebugEval.Location = new System.Drawing.Point(534, 3);
			this.btnDebugEval.Name = "btnDebugEval";
			this.btnDebugEval.Size = new System.Drawing.Size(103, 26);
			this.btnDebugEval.TabIndex = 2;
			this.btnDebugEval.Text = "Gen. Debug Info.";
			this.btnDebugEval.UseVisualStyleBackColor = true;
			this.btnDebugEval.Click += new System.EventHandler(this.btnDebugEval_Click);
			// 
			// BtnAyuda
			// 
			this.BtnAyuda.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BtnAyuda.Image = global::LSI.Packages.Extensiones.Resources.HelpVS;
			this.BtnAyuda.Location = new System.Drawing.Point(825, 3);
			this.BtnAyuda.Name = "BtnAyuda";
			this.BtnAyuda.Size = new System.Drawing.Size(27, 26);
			this.BtnAyuda.TabIndex = 13;
			this.BtnAyuda.UseVisualStyleBackColor = true;
			this.BtnAyuda.Click += new System.EventHandler(this.BtnAyuda_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox3.Controls.Add(this.lblSeqLength);
			this.groupBox3.Controls.Add(this.txtSeqLength);
			this.groupBox3.Controls.Add(this.lblMaxHash);
			this.groupBox3.Controls.Add(this.txtMaxHash);
			this.groupBox3.Controls.Add(this.lblPercentageEvaluation);
			this.groupBox3.Controls.Add(this.txtPercentageEvaluation);
			this.groupBox3.Location = new System.Drawing.Point(7, 88);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(847, 53);
			this.groupBox3.TabIndex = 9;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Data definition";
			// 
			// btnExportKbObjects
			// 
			this.btnExportKbObjects.Location = new System.Drawing.Point(188, 5);
			this.btnExportKbObjects.Name = "btnExportKbObjects";
			this.btnExportKbObjects.Size = new System.Drawing.Size(150, 23);
			this.btnExportKbObjects.TabIndex = 1;
			this.btnExportKbObjects.Text = "Export KB training files";
			this.btnExportKbObjects.UseVisualStyleBackColor = true;
			this.btnExportKbObjects.Click += new System.EventHandler(this.btnExportKbObjects_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(406, 22);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(339, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "* RNN models will only work with Python model server, not with TF Lite";
			// 
			// GenerateModelTW
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.btnExportKbObjects);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.BtnAyuda);
			this.Controls.Add(this.btnDebugEval);
			this.Controls.Add(this.btnEval);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.lnkTensorboard);
			this.Controls.Add(this.btnExport);
			this.Controls.Add(this.btnSelModel);
			this.Controls.Add(this.lnkModel);
			this.Controls.Add(this.txtModelDirectory);
			this.Controls.Add(this.txtLog);
			this.Controls.Add(this.btnGenerate);
			this.Name = "GenerateModelTW";
			this.Size = new System.Drawing.Size(857, 543);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblNExport;
        private System.Windows.Forms.TextBox txtNExport;
        private System.Windows.Forms.Label lblNEpochs;
        private System.Windows.Forms.TextBox txtNEpochs;
        private System.Windows.Forms.TextBox txtModelDirectory;
        private System.Windows.Forms.Label lblPercentageEvaluation;
        private System.Windows.Forms.TextBox txtPercentageEvaluation;
        private System.Windows.Forms.LinkLabel lnkModel;
        private System.Windows.Forms.Button btnSelModel;
        private System.Windows.Forms.Label lblMaxHash;
        private System.Windows.Forms.TextBox txtMaxHash;
        private System.Windows.Forms.Label lblRnnSize;
        private System.Windows.Forms.TextBox txtRnnSize;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.LinkLabel lnkTensorboard;
        private System.Windows.Forms.Label lblSeqLength;
        private System.Windows.Forms.TextBox txtSeqLength;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnEval;
        private System.Windows.Forms.Button btnDebugEval;
        private System.Windows.Forms.TextBox txtDropout;
        private System.Windows.Forms.Label lblDropout;
        private System.Windows.Forms.ComboBox cmbCellType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BtnAyuda;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TextBox txtGptResidualDropout;
		private System.Windows.Forms.Label lblGptResDroput;
		private System.Windows.Forms.TextBox txtGptEmbeddingSize;
		private System.Windows.Forms.Label lblGptEmbSize;
		private System.Windows.Forms.TextBox txtGptNLayers;
		private System.Windows.Forms.Label lblGptNLayers;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.TextBox txtGptAttDropout;
		private System.Windows.Forms.Label lblGptAttDropout;
		private System.Windows.Forms.TextBox txtGptAttHeads;
		private System.Windows.Forms.Label lblGptAttHeads;
		private System.Windows.Forms.TextBox txtGptEmbDropout;
		private System.Windows.Forms.Label lblGptEmbDropout;
		private System.Windows.Forms.ComboBox cmbModelType;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox txtLogEach;
		private System.Windows.Forms.Label lblLogEach;
		private System.Windows.Forms.TextBox txtMaxBatchEpoch;
		private System.Windows.Forms.Label lblMaxBachEpoch;
		private System.Windows.Forms.Button btnExportKbObjects;
		private System.Windows.Forms.Label label1;
	}
}
