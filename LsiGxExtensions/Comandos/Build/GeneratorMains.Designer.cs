namespace LSI.Packages.Extensiones.Comandos.Build
{
    partial class GeneratorMains
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			this.LnkConfigFile = new System.Windows.Forms.LinkLabel();
			this.LnkBinDir = new System.Windows.Forms.LinkLabel();
			this.BtnZip = new System.Windows.Forms.Button();
			this.BtnProduction = new System.Windows.Forms.Button();
			this.LnkGMatCon = new System.Windows.Forms.LinkLabel();
			this.LblRepairRspsAuto = new System.Windows.Forms.Label();
			this.CmbRspRepair = new System.Windows.Forms.ComboBox();
			this.ToolBar = new System.Windows.Forms.ToolStrip();
			this.BtnRun = new System.Windows.Forms.ToolStripButton();
			this.BtnStartDebug = new System.Windows.Forms.ToolStripButton();
			this.BtnDebug = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.BtnBuildSingleGenerator = new System.Windows.Forms.ToolStripButton();
			this.BtnCompile = new System.Windows.Forms.ToolStripButton();
			this.BtnCustomCompile = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.BtnEditRsp = new System.Windows.Forms.ToolStripButton();
			this.BtnRepairRsps = new System.Windows.Forms.ToolStripButton();
			this.BtnOpenSource = new System.Windows.Forms.ToolStripButton();
			this.MenuBar = new System.Windows.Forms.MenuStrip();
			this.MenRun = new System.Windows.Forms.ToolStripMenuItem();
			this.MiRun = new System.Windows.Forms.ToolStripMenuItem();
			this.MiStartDebug = new System.Windows.Forms.ToolStripMenuItem();
			this.MiDebug = new System.Windows.Forms.ToolStripMenuItem();
			this.MenBuild = new System.Windows.Forms.ToolStripMenuItem();
			this.MiBuildSingleGenerator = new System.Windows.Forms.ToolStripMenuItem();
			this.MiCompile = new System.Windows.Forms.ToolStripMenuItem();
			this.MiCustomCompile = new System.Windows.Forms.ToolStripMenuItem();
			this.MiBuildQueryObjects = new System.Windows.Forms.ToolStripMenuItem();
			this.MenTools = new System.Windows.Forms.ToolStripMenuItem();
			this.MiEditRsp = new System.Windows.Forms.ToolStripMenuItem();
			this.MiRepairRsps = new System.Windows.Forms.ToolStripMenuItem();
			this.MiOpenSource = new System.Windows.Forms.ToolStripMenuItem();
			this.MiRemoveBackups = new System.Windows.Forms.ToolStripMenuItem();
			this.MiRemoveFromRsps = new System.Windows.Forms.ToolStripMenuItem();
			this.androidToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MiApisSourcesDir = new System.Windows.Forms.ToolStripMenuItem();
			this.MiEditExternalApi = new System.Windows.Forms.ToolStripMenuItem();
			this.LnkWebDir = new System.Windows.Forms.LinkLabel();
			this.GrdMains = new LSI.Packages.Extensiones.Utilidades.UI.GridObjetos();
			this.ToolBar.SuspendLayout();
			this.MenuBar.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GrdMains)).BeginInit();
			this.SuspendLayout();
			// 
			// LnkConfigFile
			// 
			this.LnkConfigFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.LnkConfigFile.AutoSize = true;
			this.LnkConfigFile.Location = new System.Drawing.Point(769, 554);
			this.LnkConfigFile.Name = "LnkConfigFile";
			this.LnkConfigFile.Size = new System.Drawing.Size(85, 13);
			this.LnkConfigFile.TabIndex = 16;
			this.LnkConfigFile.TabStop = true;
			this.LnkConfigFile.Text = "Client.exe.config";
			this.LnkConfigFile.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkConfigFile_LinkClicked);
			// 
			// LnkBinDir
			// 
			this.LnkBinDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.LnkBinDir.AutoSize = true;
			this.LnkBinDir.Location = new System.Drawing.Point(708, 554);
			this.LnkBinDir.Name = "LnkBinDir";
			this.LnkBinDir.Size = new System.Drawing.Size(55, 13);
			this.LnkBinDir.TabIndex = 17;
			this.LnkBinDir.TabStop = true;
			this.LnkBinDir.Text = "\\bin folder";
			this.LnkBinDir.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkBinDir_LinkClicked);
			// 
			// BtnZip
			// 
			this.BtnZip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BtnZip.Location = new System.Drawing.Point(84, 549);
			this.BtnZip.Name = "BtnZip";
			this.BtnZip.Size = new System.Drawing.Size(75, 23);
			this.BtnZip.TabIndex = 18;
			this.BtnZip.Text = "&Zip modules";
			this.BtnZip.UseVisualStyleBackColor = true;
			this.BtnZip.Click += new System.EventHandler(this.BtnZip_Click);
			// 
			// BtnProduction
			// 
			this.BtnProduction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BtnProduction.Location = new System.Drawing.Point(3, 549);
			this.BtnProduction.Name = "BtnProduction";
			this.BtnProduction.Size = new System.Drawing.Size(75, 23);
			this.BtnProduction.TabIndex = 20;
			this.BtnProduction.Text = "Production...";
			this.BtnProduction.UseVisualStyleBackColor = true;
			this.BtnProduction.Click += new System.EventHandler(this.BtnProduction_Click);
			// 
			// LnkGMatCon
			// 
			this.LnkGMatCon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.LnkGMatCon.AutoSize = true;
			this.LnkGMatCon.Location = new System.Drawing.Point(570, 554);
			this.LnkGMatCon.Name = "LnkGMatCon";
			this.LnkGMatCon.Size = new System.Drawing.Size(65, 13);
			this.LnkGMatCon.TabIndex = 21;
			this.LnkGMatCon.TabStop = true;
			this.LnkGMatCon.Text = "GMatCon.ini";
			this.LnkGMatCon.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkGMatCon_LinkClicked);
			// 
			// LblRepairRspsAuto
			// 
			this.LblRepairRspsAuto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.LblRepairRspsAuto.AutoSize = true;
			this.LblRepairRspsAuto.Location = new System.Drawing.Point(165, 554);
			this.LblRepairRspsAuto.Name = "LblRepairRspsAuto";
			this.LblRepairRspsAuto.Size = new System.Drawing.Size(156, 13);
			this.LblRepairRspsAuto.TabIndex = 22;
			this.LblRepairRspsAuto.Text = "RSP repair with custom compile";
			// 
			// CmbRspRepair
			// 
			this.CmbRspRepair.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CmbRspRepair.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CmbRspRepair.FormattingEnabled = true;
			this.CmbRspRepair.Location = new System.Drawing.Point(327, 549);
			this.CmbRspRepair.Name = "CmbRspRepair";
			this.CmbRspRepair.Size = new System.Drawing.Size(148, 21);
			this.CmbRspRepair.TabIndex = 23;
			// 
			// ToolBar
			// 
			this.ToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BtnRun,
            this.BtnStartDebug,
            this.BtnDebug,
            this.toolStripSeparator1,
            this.BtnBuildSingleGenerator,
            this.BtnCompile,
            this.BtnCustomCompile,
            this.toolStripSeparator2,
            this.BtnEditRsp,
            this.BtnRepairRsps,
            this.BtnOpenSource});
			this.ToolBar.Location = new System.Drawing.Point(0, 24);
			this.ToolBar.Name = "ToolBar";
			this.ToolBar.Size = new System.Drawing.Size(857, 25);
			this.ToolBar.TabIndex = 24;
			this.ToolBar.Text = "toolStrip1";
			// 
			// BtnRun
			// 
			this.BtnRun.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.BtnRun.Name = "BtnRun";
			this.BtnRun.Size = new System.Drawing.Size(32, 22);
			this.BtnRun.Text = "Run";
			this.BtnRun.Click += new System.EventHandler(this.BtnRun_Click);
			// 
			// BtnStartDebug
			// 
			this.BtnStartDebug.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.BtnStartDebug.Name = "BtnStartDebug";
			this.BtnStartDebug.Size = new System.Drawing.Size(72, 22);
			this.BtnStartDebug.Text = "Start debug";
			this.BtnStartDebug.Click += new System.EventHandler(this.BtnStartDebug_Click);
			// 
			// BtnDebug
			// 
			this.BtnDebug.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.BtnDebug.Name = "BtnDebug";
			this.BtnDebug.Size = new System.Drawing.Size(46, 22);
			this.BtnDebug.Text = "Debug";
			this.BtnDebug.Click += new System.EventHandler(this.BtnDebug_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// BtnBuildSingleGenerator
			// 
			this.BtnBuildSingleGenerator.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.BtnBuildSingleGenerator.Name = "BtnBuildSingleGenerator";
			this.BtnBuildSingleGenerator.Size = new System.Drawing.Size(53, 22);
			this.BtnBuildSingleGenerator.Text = "Build all";
			this.BtnBuildSingleGenerator.Click += new System.EventHandler(this.BtnBuildSingleGenerator_Click);
			// 
			// BtnCompile
			// 
			this.BtnCompile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.BtnCompile.Name = "BtnCompile";
			this.BtnCompile.Size = new System.Drawing.Size(71, 22);
			this.BtnCompile.Text = "Gx compile";
			this.BtnCompile.Click += new System.EventHandler(this.BtnCompile_Click);
			// 
			// BtnCustomCompile
			// 
			this.BtnCustomCompile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.BtnCustomCompile.Name = "BtnCustomCompile";
			this.BtnCustomCompile.Size = new System.Drawing.Size(99, 22);
			this.BtnCustomCompile.Text = "Custom compile";
			this.BtnCustomCompile.Click += new System.EventHandler(this.BtnCustomCompile_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// BtnEditRsp
			// 
			this.BtnEditRsp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.BtnEditRsp.Name = "BtnEditRsp";
			this.BtnEditRsp.Size = new System.Drawing.Size(54, 22);
			this.BtnEditRsp.Text = "Edit RSP";
			this.BtnEditRsp.Click += new System.EventHandler(this.BtnEditRsp_Click);
			// 
			// BtnRepairRsps
			// 
			this.BtnRepairRsps.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.BtnRepairRsps.Name = "BtnRepairRsps";
			this.BtnRepairRsps.Size = new System.Drawing.Size(91, 22);
			this.BtnRepairRsps.Text = "Repair RSP files";
			this.BtnRepairRsps.Click += new System.EventHandler(this.BtnRepairRsps_Click);
			// 
			// BtnOpenSource
			// 
			this.BtnOpenSource.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.BtnOpenSource.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.BtnOpenSource.Name = "BtnOpenSource";
			this.BtnOpenSource.Size = new System.Drawing.Size(78, 22);
			this.BtnOpenSource.Text = "Open source";
			this.BtnOpenSource.Click += new System.EventHandler(this.BtnOpenSource_Click);
			// 
			// MenuBar
			// 
			this.MenuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenRun,
            this.MenBuild,
            this.MenTools,
            this.androidToolStripMenuItem});
			this.MenuBar.Location = new System.Drawing.Point(0, 0);
			this.MenuBar.Name = "MenuBar";
			this.MenuBar.Size = new System.Drawing.Size(857, 24);
			this.MenuBar.TabIndex = 25;
			this.MenuBar.Text = "menuStrip1";
			// 
			// MenRun
			// 
			this.MenRun.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MiRun,
            this.MiStartDebug,
            this.MiDebug});
			this.MenRun.Name = "MenRun";
			this.MenRun.Size = new System.Drawing.Size(40, 20);
			this.MenRun.Text = "Run";
			// 
			// MiRun
			// 
			this.MiRun.Name = "MiRun";
			this.MiRun.Size = new System.Drawing.Size(239, 22);
			this.MiRun.Text = "Run";
			this.MiRun.Click += new System.EventHandler(this.BtnRun_Click);
			// 
			// MiStartDebug
			// 
			this.MiStartDebug.Name = "MiStartDebug";
			this.MiStartDebug.Size = new System.Drawing.Size(239, 22);
			this.MiStartDebug.Text = "Start debugging";
			this.MiStartDebug.Click += new System.EventHandler(this.BtnStartDebug_Click);
			// 
			// MiDebug
			// 
			this.MiDebug.Name = "MiDebug";
			this.MiDebug.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D)));
			this.MiDebug.Size = new System.Drawing.Size(239, 22);
			this.MiDebug.Text = "Debug running instance";
			this.MiDebug.Click += new System.EventHandler(this.BtnDebug_Click);
			// 
			// MenBuild
			// 
			this.MenBuild.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MiBuildSingleGenerator,
            this.MiCompile,
            this.MiCustomCompile,
            this.MiBuildQueryObjects});
			this.MenBuild.Name = "MenBuild";
			this.MenBuild.Size = new System.Drawing.Size(46, 20);
			this.MenBuild.Text = "Build";
			// 
			// MiBuildSingleGenerator
			// 
			this.MiBuildSingleGenerator.Name = "MiBuildSingleGenerator";
			this.MiBuildSingleGenerator.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.A)));
			this.MiBuildSingleGenerator.Size = new System.Drawing.Size(256, 22);
			this.MiBuildSingleGenerator.Text = "Build all with this generator";
			this.MiBuildSingleGenerator.Click += new System.EventHandler(this.BtnBuildSingleGenerator_Click);
			// 
			// MiCompile
			// 
			this.MiCompile.Name = "MiCompile";
			this.MiCompile.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.G)));
			this.MiCompile.Size = new System.Drawing.Size(256, 22);
			this.MiCompile.Text = "Genexus compilation";
			this.MiCompile.Click += new System.EventHandler(this.BtnCompile_Click);
			// 
			// MiCustomCompile
			// 
			this.MiCustomCompile.Name = "MiCustomCompile";
			this.MiCustomCompile.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.C)));
			this.MiCustomCompile.Size = new System.Drawing.Size(256, 22);
			this.MiCustomCompile.Text = "Custom compilation";
			this.MiCustomCompile.Click += new System.EventHandler(this.BtnCustomCompile_Click);
			// 
			// MiBuildQueryObjects
			// 
			this.MiBuildQueryObjects.Name = "MiBuildQueryObjects";
			this.MiBuildQueryObjects.Size = new System.Drawing.Size(256, 22);
			this.MiBuildQueryObjects.Text = "Build query objects";
			this.MiBuildQueryObjects.Click += new System.EventHandler(this.MiBuildQueryObjects_Click);
			// 
			// MenTools
			// 
			this.MenTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MiEditRsp,
            this.MiRepairRsps,
            this.MiOpenSource,
            this.MiRemoveBackups,
            this.MiRemoveFromRsps});
			this.MenTools.Name = "MenTools";
			this.MenTools.Size = new System.Drawing.Size(46, 20);
			this.MenTools.Text = "Tools";
			// 
			// MiEditRsp
			// 
			this.MiEditRsp.Name = "MiEditRsp";
			this.MiEditRsp.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.S)));
			this.MiEditRsp.Size = new System.Drawing.Size(241, 22);
			this.MiEditRsp.Text = "Edit module RSP";
			this.MiEditRsp.Click += new System.EventHandler(this.BtnEditRsp_Click);
			// 
			// MiRepairRsps
			// 
			this.MiRepairRsps.Name = "MiRepairRsps";
			this.MiRepairRsps.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.E)));
			this.MiRepairRsps.Size = new System.Drawing.Size(241, 22);
			this.MiRepairRsps.Text = "Repair all RSPs...";
			this.MiRepairRsps.Click += new System.EventHandler(this.BtnRepairRsps_Click);
			// 
			// MiOpenSource
			// 
			this.MiOpenSource.Name = "MiOpenSource";
			this.MiOpenSource.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F)));
			this.MiOpenSource.Size = new System.Drawing.Size(241, 22);
			this.MiOpenSource.Text = "Open object source file...";
			this.MiOpenSource.Click += new System.EventHandler(this.BtnOpenSource_Click);
			// 
			// MiRemoveBackups
			// 
			this.MiRemoveBackups.Name = "MiRemoveBackups";
			this.MiRemoveBackups.Size = new System.Drawing.Size(241, 22);
			this.MiRemoveBackups.Text = "Remove backup files...";
			this.MiRemoveBackups.Click += new System.EventHandler(this.MiRemoveBackups_Click);
			// 
			// MiRemoveFromRsps
			// 
			this.MiRemoveFromRsps.Name = "MiRemoveFromRsps";
			this.MiRemoveFromRsps.Size = new System.Drawing.Size(241, 22);
			this.MiRemoveFromRsps.Text = "Remove entry from all RSPs...";
			this.MiRemoveFromRsps.Click += new System.EventHandler(this.MiRemoveFromRsps_Click);
			// 
			// androidToolStripMenuItem
			// 
			this.androidToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MiApisSourcesDir,
            this.MiEditExternalApi});
			this.androidToolStripMenuItem.Name = "androidToolStripMenuItem";
			this.androidToolStripMenuItem.Size = new System.Drawing.Size(62, 20);
			this.androidToolStripMenuItem.Text = "Android";
			// 
			// MiApisSourcesDir
			// 
			this.MiApisSourcesDir.Name = "MiApisSourcesDir";
			this.MiApisSourcesDir.Size = new System.Drawing.Size(243, 22);
			this.MiApisSourcesDir.Text = "Open GX APIs sources directory";
			this.MiApisSourcesDir.Click += new System.EventHandler(this.MiApisSourcesDir_Click);
			// 
			// MiEditExternalApi
			// 
			this.MiEditExternalApi.Name = "MiEditExternalApi";
			this.MiEditExternalApi.Size = new System.Drawing.Size(243, 22);
			this.MiEditExternalApi.Text = "Edit UserExternalApiFactory.java";
			this.MiEditExternalApi.Click += new System.EventHandler(this.MiEditExternalApi_Click);
			// 
			// LnkWebDir
			// 
			this.LnkWebDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.LnkWebDir.AutoSize = true;
			this.LnkWebDir.Location = new System.Drawing.Point(641, 554);
			this.LnkWebDir.Name = "LnkWebDir";
			this.LnkWebDir.Size = new System.Drawing.Size(61, 13);
			this.LnkWebDir.TabIndex = 26;
			this.LnkWebDir.TabStop = true;
			this.LnkWebDir.Text = "\\web folder";
			this.LnkWebDir.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkWebDir_LinkClicked);
			// 
			// GrdMains
			// 
			this.GrdMains.AllowUserToAddRows = false;
			this.GrdMains.AllowUserToDeleteRows = false;
			this.GrdMains.AllowUserToResizeRows = false;
			this.GrdMains.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.GrdMains.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.GrdMains.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.GrdMains.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.GrdMains.DefaultCellStyle = dataGridViewCellStyle2;
			this.GrdMains.Location = new System.Drawing.Point(3, 52);
			this.GrdMains.Name = "GrdMains";
			this.GrdMains.Objetos = null;
			this.GrdMains.ReadOnly = true;
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.GrdMains.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.GrdMains.RowHeadersVisible = false;
			this.GrdMains.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.GrdMains.Size = new System.Drawing.Size(851, 491);
			this.GrdMains.TabIndex = 14;
			this.GrdMains.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GrdMains_CellDoubleClick);
			this.GrdMains.SelectionChanged += new System.EventHandler(this.GrdMains_SelectionChanged);
			this.GrdMains.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GrdMains_KeyDown);
			// 
			// GeneratorMains
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.LnkWebDir);
			this.Controls.Add(this.ToolBar);
			this.Controls.Add(this.MenuBar);
			this.Controls.Add(this.CmbRspRepair);
			this.Controls.Add(this.LblRepairRspsAuto);
			this.Controls.Add(this.LnkGMatCon);
			this.Controls.Add(this.BtnProduction);
			this.Controls.Add(this.BtnZip);
			this.Controls.Add(this.LnkBinDir);
			this.Controls.Add(this.LnkConfigFile);
			this.Controls.Add(this.GrdMains);
			this.Name = "GeneratorMains";
			this.Size = new System.Drawing.Size(857, 575);
			this.ToolBar.ResumeLayout(false);
			this.ToolBar.PerformLayout();
			this.MenuBar.ResumeLayout(false);
			this.MenuBar.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GrdMains)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        internal LSI.Packages.Extensiones.Utilidades.UI.GridObjetos GrdMains;
        private System.Windows.Forms.LinkLabel LnkConfigFile;
        private System.Windows.Forms.LinkLabel LnkBinDir;
        private System.Windows.Forms.Button BtnZip;
        private System.Windows.Forms.Button BtnProduction;
        private System.Windows.Forms.LinkLabel LnkGMatCon;
        private System.Windows.Forms.Label LblRepairRspsAuto;
        private System.Windows.Forms.ComboBox CmbRspRepair;
        private System.Windows.Forms.ToolStrip ToolBar;
        private System.Windows.Forms.ToolStripButton BtnRun;
        private System.Windows.Forms.ToolStripButton BtnStartDebug;
        private System.Windows.Forms.ToolStripButton BtnBuildSingleGenerator;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton BtnCompile;
        private System.Windows.Forms.ToolStripButton BtnCustomCompile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton BtnEditRsp;
        private System.Windows.Forms.ToolStripButton BtnDebug;
        private System.Windows.Forms.ToolStripButton BtnRepairRsps;
        private System.Windows.Forms.ToolStripMenuItem MenRun;
        private System.Windows.Forms.ToolStripMenuItem MiRun;
        private System.Windows.Forms.ToolStripMenuItem MiStartDebug;
        private System.Windows.Forms.ToolStripMenuItem MiDebug;
        private System.Windows.Forms.ToolStripMenuItem MenBuild;
        private System.Windows.Forms.ToolStripMenuItem MiBuildSingleGenerator;
        private System.Windows.Forms.ToolStripMenuItem MiCompile;
        private System.Windows.Forms.ToolStripMenuItem MenTools;
        private System.Windows.Forms.ToolStripMenuItem MiCustomCompile;
        private System.Windows.Forms.ToolStripMenuItem MiEditRsp;
        private System.Windows.Forms.ToolStripMenuItem MiRepairRsps;
        private System.Windows.Forms.ToolStripMenuItem MiOpenSource;
        private System.Windows.Forms.ToolStripButton BtnOpenSource;
        internal System.Windows.Forms.MenuStrip MenuBar;
        private System.Windows.Forms.ToolStripMenuItem MiRemoveBackups;
        private System.Windows.Forms.ToolStripMenuItem MiBuildQueryObjects;
        private System.Windows.Forms.LinkLabel LnkWebDir;
        private System.Windows.Forms.ToolStripMenuItem androidToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MiApisSourcesDir;
        private System.Windows.Forms.ToolStripMenuItem MiEditExternalApi;
        private System.Windows.Forms.ToolStripMenuItem MiRemoveFromRsps;
    }
}
