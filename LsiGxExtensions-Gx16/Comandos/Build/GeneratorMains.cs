using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Entities;
using Artech.Genexus.Common.Services;
using Artech.Udm.Framework;
using LSI.Packages.Extensiones.Comandos.Build.Production;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Compression;
using LSI.Packages.Extensiones.Utilidades.CSharpWin;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.UI;
using LSI.Packages.Extensiones.Utilidades.VS;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Comandos.Build
{
    /// <summary>
    /// Panel to show main kb objects with a given generator
    /// </summary>
    public partial class GeneratorMains : UserControl
    {

        /// <summary>
        /// Reference to Genexus object with last compilation date
        /// </summary>
        internal class CompilationObjectRef : RefObjetoGX
        {

            /// <summary>
            /// Last compilation date
            /// </summary>
            public DateTime LastCompilationDate { get; private set; }

            /// <summary>
            /// Constructor for main kb object
            /// </summary>
            /// <param name="o">Main object</param>
            /// <param name="binModuleDir">bin directory path where this module is compiled</param>
#if GX_17_OR_GREATER
            public CompilationObjectRef(KBObject o, string binModuleDir, GxGenerator enviroment)
#else
            public CompilationObjectRef(KBObject o, string binModuleDir, GxEnvironment enviroment)
#endif
                : base(o) 
            {
                UpdateCompilationDate(binModuleDir, enviroment);
            }

            /// <summary>
            /// Constructor for genexus.commons.programs.dll
            /// </summary>
            /// <param name="binModuleDir">bin directory path where this module is compiled</param>
#if GX_17_OR_GREATER
            public CompilationObjectRef(string binModuleDir, GxGenerator enviroment)
#else
            public CompilationObjectRef(string binModuleDir, GxEnvironment enviroment)
#endif
            {
                NombreObjeto = CSharpUtils.COMMONSNAME;
                DescripcionObjeto = "Dll for SDTs, domains, etc";
                UpdateCompilationDate(binModuleDir, enviroment);
            }

            /// <summary>
            /// Update the LastCompilationDate property with the module last compilation date
            /// </summary>
            /// <param name="binModuleDir">Bin directory path for this module</param>
#if GX_17_OR_GREATER
            public void UpdateCompilationDate(string binModuleDir, GxGenerator enviroment)
#else
            public void UpdateCompilationDate(string binModuleDir, GxEnvironment enviroment)
#endif
            {
                try
                {
                    string path = Path.Combine(binModuleDir, DllFileName(enviroment));
                    if (File.Exists(path))
                        LastCompilationDate = File.GetLastWriteTime(path);
                }
                catch { }
            }

            /// <summary>
            /// Get the main file name, without extension
            /// </summary>
            /// <param name="enviroment">Enviroment where to get the file name</param>
            /// <returns>The object file name</returns>
#if GX_17_OR_GREATER
            private string BinModuleName(GxGenerator enviroment)
#else
            private string BinModuleName(GxEnvironment enviroment)
#endif
            {
                if (NombreObjeto == CSharpUtils.COMMONSNAME)
                    return CSharpUtils.COMMONSNAME;
                else
                    return KBaseGX.GetProgramFileName(ObjetoGX, enviroment, false, null);
            }

            /// <summary>
            /// Get the file name of the dll file of this object
            /// </summary>
#if GX_17_OR_GREATER
            public string DllFileName(GxGenerator enviroment)
#else
            public string DllFileName(GxEnvironment enviroment)
#endif
            {
                return BinModuleName(enviroment) + ".dll";
            }

            /// <summary>
            /// Get the file name of the PDB  file (debug info) of this object
            /// </summary>
#if GX_17_OR_GREATER
            public string PdbFileName(GxGenerator enviroment)
#else
            public string PdbFileName(GxEnvironment enviroment)
#endif
            {
                return BinModuleName(enviroment) + ".pdb";
            }

        }

        // Values for the repair rsps combo
        private const string RSP_REPAIRALL = "Repair all";
        private const string RSP_REPAIRBC = "Repair BC only";
        private const string RSP_DONOTREPAIR = "Do not repair";

        /// <summary>
        /// Generator of this panel
        /// </summary>
#if GX_17_OR_GREATER
        public GxGenerator Generator { get; private set; }
#else
        public GxEnvironment Generator { get; private set; }
#endif
        /// <summary>
        /// Mains objects list
        /// </summary>
        private SortableList<CompilationObjectRef> Mains;

        /// <summary>
        /// Generator is c# web?
        /// </summary>
        public bool IsCSharpWebGenerator;

        /// <summary>
        /// Generator is c# win?
        /// </summary>
        public bool IsCSharpWinGenerator;

        /// <summary>
        /// Generator is SD
        /// </summary>
        public bool IsSdGenerator;

        /// <summary>
        /// Generator is C# win/web?
        /// </summary>
        private bool IsCSharpGenerator;

        /// <summary>
        /// Generate assembly by main? true = by main  / false = by folder
        /// </summary>
        private bool IsCSharpAssembliesByMainSet;

        /// <summary>
        /// Owner window
        /// </summary>
        public WorkWithMains WWMains;

        /// <summary>
        /// Genexus.common.programs reference on mains grid. It can be null if the environment is not C#
        /// </summary>
        private CompilationObjectRef RefCommons;

#if GX_17_OR_GREATER
        public GeneratorMains(WorkWithMains wwMains, GxGenerator generator, IEnumerable<KBObject> mains)
#else
        public GeneratorMains(WorkWithMains wwMains, GxEnvironment generator, IEnumerable<KBObject> mains)
#endif
        {
            try
            {
                InitializeComponent();

                this.Generator = generator;
                this.WWMains = wwMains;

                InitializeToolBar();

                // Generator settings
                StoreGeneratorSettings();

                // Initialize mains list:
                Mains = new SortableList<CompilationObjectRef>(
                    mains.Select(x => new CompilationObjectRef(x, BinModuleDir, Generator))
                    );

                // Initialize grid:
                GrdMains.CrearColumnasEstandar();

                // Hide not interesting columns:
                GrdMains.HideColumn(GridObjetos.COL_LASTUPDATE);
                GrdMains.HideColumn(GridObjetos.COL_USER);

                // Repair RSPs combo:
                CmbRspRepair.Items.Add(RSP_REPAIRALL);
                CmbRspRepair.Items.Add(RSP_REPAIRBC);
                CmbRspRepair.Items.Add(RSP_DONOTREPAIR);

                // Configure UI: Call this before call GrdMains.SetObjetos and after CrearColumnasEstandar()!
                ConfigureUIForCurrentEnviroment();

                GrdMains.SetObjetos<CompilationObjectRef>(Mains);
                GrdMains.OpenObjectsEnabled = false;
                GrdMains.OrderByObjectName();
            }
            catch { }
        }

        /// <summary>
        /// Store interesting current generator settins
        /// </summary>
        private void StoreGeneratorSettings()
        {
            IsCSharpWebGenerator = Generator.Generator == (int)GeneratorType.CSharpWeb;
            IsCSharpGenerator = (Generator.Generator == (int)GeneratorType.CSharpWin ||
                    Generator.Generator == (int)GeneratorType.CSharpWeb);
            IsCSharpWinGenerator = Generator.Generator == (int)GeneratorType.CSharpWin;
            IsSdGenerator = Generator.Generator == (int)GeneratorType.SmartDevices;

            if (IsCSharpWinGenerator)
            {
                // Check generate assemblyes by main / by folder
                string type = 
                    Generator.Properties.GetPropertyValue(Properties.CSHARPWIN.AssembliesStructure)
                    as string;
                IsCSharpAssembliesByMainSet = (type == Properties.CSHARPWIN.AssembliesStructure_Values.ByMain);
            }
        }

        /// <summary>
        /// Configure the UI for the environment settings
        /// </summary>
        private void ConfigureUIForCurrentEnviroment()
        {

            if (IsCSharpWinGenerator)
            {
                // Add column for compilation date, only for win c#
                DataGridViewTextBoxColumn colCompilationDate = new DataGridViewTextBoxColumn();
                colCompilationDate.DataPropertyName = "LastCompilationDate";
                colCompilationDate.HeaderText = "Compilation";
                colCompilationDate.Name = "ColCompilationDate";
                colCompilationDate.ReadOnly = true;
                GrdMains.Columns.Add(colCompilationDate);
            }
            else
            {
                // Remove c# / win only functions 
                RepairRspEnabled = false;
                CustomCompileEnabled = false;
                StartDebuggingEnabled = false;
                BtnZip.Visible = false;
                MiRemoveBackups.Visible = MiRemoveBackups.Enabled = false;
                MiRemoveFromRsps.Visible = MiRemoveFromRsps.Enabled = false;
            }

            if (IsCSharpGenerator)
            {
                // Add main entry for Genexus.Commons.Programs
                RefCommons = new CompilationObjectRef(BinModuleDir, Generator);
                Mains.Add(RefCommons);
            }
            else
            {
                // Remove c# only functions
                RepairRspEnabled = false;
                EditRspEnabled = false;
                StartDebuggingEnabled = DebugRunningInstanceEnabled = false;
                CustomCompileEnabled = false;
                LnkConfigFile.Visible = LnkBinDir.Visible = false;
                BtnProduction.Visible = false;
                BtnZip.Visible = false;
            }

            if(!IsCSharpGenerator && !IsSdGenerator)
                // Open source files only for C# and SD (Android)
                BtnOpenSource.Visible = MiOpenSource.Visible = MiOpenSource.Enabled = false;

            if (IsCSharpWinGenerator && !IsCSharpAssembliesByMainSet)
            {
                // Remove c# win / generate assembly by main functions
                RepairRspEnabled = false;
                EditRspEnabled = false;
                CustomCompileEnabled = false;
                BtnZip.Visible = false;
            }

            if (IsCSharpWebGenerator)
                LnkConfigFile.Text = "web.config";
            else
            {
                // Remove c# web only functions
                LnkWebDir.Visible = false;
            }

            // Default value for repair RSP files
            if (CmbRspRepair.Visible)
            {
                if (CSharpUtils.GeneratorHasDebugOption(Generator))
                    // By default, autorepair rsps on prototype, not in production.
                    CmbRspRepair.SelectedItem = RSP_REPAIRALL;
                else
                    CmbRspRepair.SelectedItem = RSP_REPAIRBC;
            }
            else
                // Repair RSP files disabled:
                CmbRspRepair.SelectedItem = RSP_DONOTREPAIR;

			// Disable options only for SD
			if (IsSdGenerator)
			{
				LnkWebDir.Visible = true;
				LnkWebDir.Text = @"\mobile";
			}
			if (!IsSdGenerator || ProductVersionHelperExtensions.MajorVersion() >= ProductVersionHelperExtensions.GX15)
            {
				// Source files directory / external files declaration is only for Ev3
                MiApisSourcesDir.Visible = MiApisSourcesDir.Enabled =
					MiEditExternalApi.Visible = MiEditExternalApi.Enabled = false;
            }

            // Link for LSI custom file:
            if (!File.Exists(GMatConIniPath))
                LnkGMatCon.Visible = false;

            // Hide menu tools?
            UpdateMenu();
        }


        /// <summary>
        /// Debug running instance function enabled?
        /// </summary>
        private bool DebugRunningInstanceEnabled
        {
            set { BtnDebug.Visible = BtnDebug.Enabled = MiDebug.Visible = MiDebug.Enabled = value; }
        }

        /// <summary>
        /// Start debugging function enabled?
        /// </summary>
        private bool StartDebuggingEnabled
        {
            set { BtnStartDebug.Visible = BtnStartDebug.Enabled = 
                MiStartDebug.Visible = MiStartDebug.Enabled = value; }
        }

        /// <summary>
        /// Repair RSP files function enabled?
        /// </summary>
        private bool RepairRspEnabled
        {
            set
            {
                BtnRepairRsps.Visible = BtnRepairRsps.Enabled = MiRepairRsps.Visible = 
                    MiRepairRsps.Enabled =
                    CmbRspRepair.Visible = LblRepairRspsAuto.Visible = value;
            }
        }

        /// <summary>
        /// Edit RSP file function enabled?
        /// </summary>
        private bool EditRspEnabled
        {
            set { BtnEditRsp.Visible = BtnEditRsp.Enabled = MiEditRsp.Visible = MiEditRsp.Enabled = value; }
        }

        /// <summary>
        /// Custom compile function enabled?
        /// </summary>
        private bool CustomCompileEnabled
        {
            set { BtnCustomCompile.Visible = BtnCustomCompile.Enabled = 
                MiCustomCompile.Visible = MiCustomCompile.Enabled = value; }
        }

        private void InitializeToolBar()
        {
            // This crap is here due to bugs on my computer with VS 2005: All of this should be set
            // on the form designer
            foreach (ToolStripButton button in ToolBar.Items.OfType<ToolStripButton>())
                button.DisplayStyle = ToolStripItemDisplayStyle.Image;

            BtnRun.Image = Resources.startwithoutdebugging_6556;
            BtnRun.Text = "Run (Enter)";

            BtnStartDebug.Image = Resources.Executequery_9958;
            BtnStartDebug.Text = "Start debugging";

            BtnDebug.Image = Resources.Symbols_Pause_16xLG;
            BtnDebug.Text = "Debug running instance";

            BtnBuildSingleGenerator.Image = Resources.build_Solution_16xLG;
            BtnBuildSingleGenerator.Text = "Build all with this generator";

            BtnCompile.Image = Resources.Hammer_Builder_16xLG;
            BtnCompile.Text = "Genexus compilation";

            BtnCustomCompile.Image = Resources.Compile_191;
            BtnCustomCompile.Text = "Custom compilation";

            BtnEditRsp.Image = Resources.CustomActionsEditor_5850;
            BtnEditRsp.Text = "Edit RSP file";

            BtnRepairRsps.Image = Resources.applycodechanges_6548;
            BtnRepairRsps.Text = "Repair RSP files";

            BtnOpenSource.Image = Resources.CSharpProject_SolutionExplorerNode;
            BtnOpenSource.Text = "Open object source file";

        }

        /// <summary>
        /// Remove shortcut keys from menu bar
        /// </summary>
        public void DettachMenuShortcuts()
        {
            foreach (ToolStripMenuItem menu in MenuBar.Items)
            {
                foreach (ToolStripMenuItem menuItem in menu.DropDownItems)
                    menuItem.ShortcutKeys = Keys.None;
            }
        }

        private void AttachMenuItemShortcut(ToolStripMenuItem item, Keys key)
        {
            if (item.Enabled)
                item.ShortcutKeys = key;
        }

        /// <summary>
        /// Update menu status
        /// </summary>
        public void UpdateMenu()
        {
            AttachMenuItemShortcut(MiDebug, Keys.Alt | Keys.D);
            AttachMenuItemShortcut(MiBuildSingleGenerator, Keys.Alt | Keys.A);
            AttachMenuItemShortcut(MiCompile, Keys.Alt | Keys.G);
            AttachMenuItemShortcut(MiCustomCompile, Keys.Alt | Keys.C);
            AttachMenuItemShortcut(MiEditRsp, Keys.Alt | Keys.S);
            AttachMenuItemShortcut(MiRepairRsps, Keys.Alt | Keys.E);
            AttachMenuItemShortcut(MiOpenSource, Keys.Alt | Keys.F);

            // Check if we should hide menus
            foreach( ToolStripMenuItem menu in MenuBar.Items.OfType<ToolStripMenuItem>() )
                menu.Visible = menu.DropDownItems.OfType<ToolStripMenuItem>().Any(x => x.Enabled);
        }

        private void DebugCSharpWin()
        {
            // Get win exe path:
            KBObject main = GrdMains.ObjetoSeleccionado;
            if (main == null)
                return;
            string exeName = Path.GetFileName(CSharpUtils.GetWinMainExeFileName(main));

            List<Process> processes = ProcessUtils.GetByExeName(exeName);
            if (processes.Count == 0)
            {
                MessageBox.Show("No process found running " + exeName);
                return;
            }
            Process p = ProcessChooser.ChooseProcess(processes);

            // Do the debugging
            if (p != null)
            {
                VisualStudio vs =
                    new VisualStudio(LsiExtensionsConfiguration.Load().VisualStudioComId);
                vs.AttachProcess(p.Id, true);
            }
        }

        private void DebugCsharpWeb()
        {
            List<Process> processes = ProcessUtils.GetByExeName("w3wp.exe");
            if (processes.Count == 0)
            {
                MessageBox.Show("No process found running w3wp.exe");
                return;
            }

            VisualStudio vs = new VisualStudio(LsiExtensionsConfiguration.Load().VisualStudioComId);
            foreach(Process p in processes)
                vs.AttachProcess(p.Id, false);
        }

        /// <summary>
        /// Debug module button clicked
        /// </summary>
        private void BtnDebug_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if debug is enabled
                string errorMessage;
                if (!CSharpUtils.GeneratorHasDebugOption(Generator, out errorMessage))
                {
                    MessageBox.Show(errorMessage);
                }

                if (IsCSharpWebGenerator)
                    DebugCsharpWeb();
                else
                    DebugCSharpWin();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Start debugging button clicked
        /// </summary>
        private void BtnStartDebug_Click(object sender, EventArgs e)
        {
            try
            {
                KBObject main = GrdMains.ObjetoSeleccionado;
                VisualStudio vs = 
                    new VisualStudio(LsiExtensionsConfiguration.Load().VisualStudioComId);
                vs.IniciarDepurando(CSharpUtils.GetWinMainExeFileName(main));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Build with this generator clicked
        /// </summary>
        private void BtnBuildSingleGenerator_Click(object sender, EventArgs e)
        {
            try
            {
                new BuildSingleGeneratorDlg(this).ShowDialog();
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// Absolute path of the bin directory of the current target directory
        /// </summary>
        private string BinModuleDir
        {
            get {
                string relativePath = IsCSharpWebGenerator ? "web\\bin" : "bin";
                return Entorno.GetTargetDirectoryFilePath(relativePath);
            }
        }

        /// <summary>
        /// Absolute path of the gmatcon.ini file of the current target directory
        /// </summary>
        private string GMatConIniPath
        {
            get { return Path.Combine(BinModuleDir, "gmatcon.ini"); }
        }

        /// <summary>
        /// Get the path to the DLL file for a main object
        /// </summary>
        private string DllModulePath(CompilationObjectRef main)
        {
            return Path.Combine(BinModuleDir, main.DllFileName(Generator));
        }

        /// <summary>
        /// Get the path to the PDB file for a main object
        /// </summary>
        private string PdbModulePath(CompilationObjectRef main)
        {
            return Path.Combine(BinModuleDir, main.PdbFileName(Generator));
        }

        /// <summary>
        /// It checks open modules, ask if they must to be renamed and it renames them.
        /// </summary>
        /// <returns>True if modules can be compiled</returns>
        private bool CheckOpenModules(IEnumerable<CompilationObjectRef> modules)
        {

            if (!IsCSharpGenerator)
                // Check not supported in this generator
                return true;

            // Check open dlls:
            List<CompilationObjectRef> openModules = new List<CompilationObjectRef>();
            foreach (CompilationObjectRef o in modules)
            {

                string moduleDll = DllModulePath(o);
                if (Entorno.FileIsOpen(moduleDll))
                    openModules.Add(o);
            }
            if (openModules.Count == 0)
                return true;

            // Ask to the user:
            string openModulesNames = string.Join(", ", openModules.Select(x => x.NombreObjeto).ToArray());
            if (MessageBox.Show("Following modules are open: " +
                openModulesNames + ". If you want to compile them, they must " +
                "to be renamed. Do you want to continue?", "Compile", MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
                return false;

            // Rename modules:
            foreach (CompilationObjectRef o in openModules)
            {
                Entorno.DoFileBackup(DllModulePath(o), true);
                string pdbFilePath = PdbModulePath(o);
                if (File.Exists(pdbFilePath))
                    Entorno.DoFileBackup(pdbFilePath, true);
            }
            return true;
        }

        /// <summary>
        /// It checks if the selected modules are open, ask if they must to be renamed and it renames them.
        /// </summary>
        /// <returns>True if modules can be compiled</returns>
        private bool CheckOpenModules()
        {
            return CheckOpenModules(GrdMains.ReferenciasSeleccionadas.Cast<CompilationObjectRef>());
        }

        private bool CommonsSelected
        {
            get { return GrdMains.ReferenciasSeleccionadas.Contains(RefCommons); }
        }

        private void BtnCompile_Click(object sender, EventArgs e)
        {
            try
            {
                if (!CheckOpenModules())
                    return;

                CompileModules compiler = new CompileModules(
                    UIServices.KB.WorkingEnvironment.TargetModel, GrdMains.ObjetosSeleccionados);
                WWMains.StartBuild(this, compiler);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        private void BtnRepairRsps_Click(object sender, EventArgs e)
        {
            try
            {
                RepairRspFiles repair = new RepairRspFiles(UIServices.KB.WorkingEnvironment.TargetModel);
                if (!repair.AskJustTest())
                    return;
                WWMains.StartBuild(this, repair);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// Edit RSP operation selected
        /// </summary>
        private void BtnEditRsp_Click(object sender, EventArgs e)
        {
            try
            {
                RefObjetoGX selectedRef = GrdMains.SelectedReference;
                if (selectedRef == null)
                    return;

                string rspPath;
                if (selectedRef == RefCommons)
                    rspPath = RspFile.GetCommonsRspPath(IsCSharpWebGenerator);
                else
                    rspPath = RspFile.RspFilePath(GrdMains.ObjetoSeleccionado);

                LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.Load();
                cfg.OpenFileWithTextEditor(rspPath);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        private void GrdMains_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ExecuteMain();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// It executes a main object
        /// </summary>
        /// <param name="mainKey">Object key</param>
        private void ExecuteMainGenexus(object mainKey)
        {
            using (Log log = new Log(Log.GENERAL_OUTPUT_ID, false))
            {
                GenexusBLServices.Run.Execute(UIServices.KB.WorkingEnvironment.TargetModel,
                    (EntityKey)mainKey, null, Generator);
            }
        }

        private void ExecuteMain()
        {
            try
            {
                KBObject o = GrdMains.ObjetoSeleccionado;
                if (o == null)
                    return;

                LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.Load();
                if (IsCSharpWinGenerator && cfg.SetBinAsCurrentDir)
                {
                    // Win / C#: Custom run (needed by LSI):
                    // Genexus does not set "bin" as the the current directory. We do it:
                    Process p = new Process();
                    p.StartInfo.FileName = CSharpUtils.GetWinMainExeFileName(o);
                    if (!File.Exists(p.StartInfo.FileName))
                        throw new Exception(p.StartInfo.FileName + " does not exist");
                    p.StartInfo.WorkingDirectory = BinModuleDir;
                    p.Start();
                }
                else
                {
                    // Standard run. Lauch the main in a new thread: The android emulator start
                    // takes a lot of time:
                    new Thread(ExecuteMainGenexus).Start(o.Key);
                }
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            ExecuteMain();
        }

        /// <summary>
        /// Grid double click event
        /// </summary>
        private void GrdMains_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex == -1)
                // Double click over header
                return;

            ExecuteMain();
        }

        private void EnabledBySelection(ToolStripButton button, ToolStripMenuItem menuItem, bool enabled)
        {
            if (button.Visible)
                button.Enabled = enabled;
            if (menuItem.Visible)
                menuItem.Enabled = enabled;
        }

        private void EnabledBySelection(Button button, bool enabled)
        {
            if (button.Visible)
                button.Enabled = enabled;
        }

        /// <summary>
        /// Grid selection changed event
        /// </summary>
        private void GrdMains_SelectionChanged(object sender, EventArgs e)
        {
            // Get selected mains, without the commons:
            List<CompilationObjectRef> selectedReferences = 
                GrdMains.ReferenciasSeleccionadas.Cast<CompilationObjectRef>().ToList();
            bool commonsSelected = selectedReferences.Contains(RefCommons);
            if( commonsSelected )
                selectedReferences.Remove(RefCommons);

            // Disable functions only for selected mains:
            bool enabled = (selectedReferences.Count > 0);
            EnabledBySelection(BtnCompile, MiCompile, enabled);
            EnabledBySelection(BtnRun, MiRun, enabled);
            EnabledBySelection(BtnDebug, MiDebug, enabled);
            EnabledBySelection(BtnStartDebug, MiStartDebug, enabled);

            // Disable functions only for selected mains or genexus commons:
            enabled = (selectedReferences.Count > 0 || commonsSelected);
            EnabledBySelection(BtnCustomCompile, MiCustomCompile, enabled);
            EnabledBySelection(BtnEditRsp, MiEditRsp, enabled);
            EnabledBySelection(BtnZip, enabled);

            if (IsCSharpWebGenerator)
                // Debug always enabled on c# web
                EnabledBySelection(BtnDebug, MiDebug, true);
        }

        private void ShellOpen(string file)
        {
            try
            {
                Process.Start(file);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        private void BtnZip_Click(object sender, EventArgs e)
        {
            try
            {

                LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.Load();
                if (!Directory.Exists(cfg.ZipDestinationFolder))
                {
                    MessageBox.Show("Destination directory " + cfg.ZipDestinationFolder +
                        " does not exists");
                    return;
                }

                if (CSharpUtils.GeneratorHasDebugOption(Generator))
                {
                    if (MessageBox.Show("Modules are compiled with /debug option. Are you sure you " +
                        "want to zip them for production?", "Confirm", MessageBoxButtons.OKCancel) == 
                        DialogResult.Cancel)
                        return;
                }

                // Zip information
                ZipOperation zipInfo = new ZipOperation();
                string binPath = BinModuleDir;
                zipInfo.BaseSourcePath = binPath;

                // Get files to zip
                List<string> modulesNames = new List<string>();
                List<string> fileNamesLowercase = new List<string>();
                foreach (CompilationObjectRef o in GrdMains.ReferenciasSeleccionadas)
                {
                    string dllFileName = o.DllFileName(Generator);
                    zipInfo.FilesToCompress.Add(dllFileName);
                    fileNamesLowercase.Add(dllFileName.ToLower());
                    modulesNames.Add(Entorno.ToSafeFilename(o.NombreObjeto));
                }

                // Add files configured to be always zipped
                foreach (string filename in cfg.ZipFilesToAddAlwaysList)
                {
                    if (File.Exists(Path.Combine(binPath, filename)) &&
                        !fileNamesLowercase.Contains(filename.ToLower()))
                        zipInfo.FilesToCompress.Add(filename);
                }

                // Get zip file name
                string zipFileName = Entorno.ToSafeFilename(Entorno.WindowsUsername);
                if (modulesNames.Count <= 2)
                    zipFileName += "-" + string.Join("-", modulesNames.ToArray());
                zipInfo.SetUnusedDestinationPath(cfg.ZipDestinationFolder, zipFileName, cfg.Compressor);

                // Build the zip
                cfg.Compressor.DoZip(zipInfo);
                ShellOpen(LsiExtensionsConfiguration.Load().ZipDestinationFolder);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        internal void UpdateCompilationDates()
        {
            Mains.ToList().ForEach(x => x.UpdateCompilationDate(BinModuleDir, Generator));
        }

        /// <summary>
        /// Repair RSPs combo selection
        /// </summary>
        private CustomWinCompiler.RepairRspsOption AutorepairRspSelection
        {
            get { 
                string selection = CmbRspRepair.SelectedItem as string; 
                if( selection == RSP_REPAIRALL )
                    return CustomWinCompiler.RepairRspsOption.RepairAll;
                else if( selection == RSP_REPAIRBC )
                    return CustomWinCompiler.RepairRspsOption.RepairBcOnly;
                else
                    return CustomWinCompiler.RepairRspsOption.DoNotRepair;
            }
        }

        private void BtnCustomCompile_Click(object sender, EventArgs e)
        {
            CompileModules(GrdMains.ReferenciasSeleccionadas.Cast<CompilationObjectRef>(),
                CommonsSelected, AutorepairRspSelection);
        }


        private void CompileModules(IEnumerable<CompilationObjectRef> modules,
            bool compileCommons, CustomWinCompiler.RepairRspsOption repairRspOption)
        {
            try
            {
                if (!CheckOpenModules(modules))
                    return;

                IEnumerable<KBObject> mainModules = modules
                    .Select(x => x.ObjetoGX)
                    .Where(x => x != null);

                CustomWinCompiler compiler = new CustomWinCompiler(Generator, mainModules);
                if (compileCommons)
                    compiler.CompileCommons = true;
                compiler.RepairRsp = repairRspOption;
                WWMains.StartBuild(this, compiler);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        public void CompileModules(IEnumerable<string> modulesNames)
        {
            IEnumerable<CompilationObjectRef> modules = 
                modulesNames.Select(
                    moduleName => Mains.FirstOrDefault(
                        mainRef => mainRef.NombreObjeto.ToLower() == moduleName.ToLower()
                    )
                );
            CompileModules(modules, true, CustomWinCompiler.RepairRspsOption.DoNotRepair);
        }

        private void BtnProduction_Click(object sender, EventArgs e)
        {
            PrepareProductionDlg dlg = new PrepareProductionDlg(this);
            dlg.ShowDialog();
            dlg.Dispose();
        }

        /// <summary>
        /// Open object source file clicked
        /// </summary>
        private void BtnOpenSource_Click(object sender, EventArgs e)
        {
            try
            {
				// TODO: If the generator is SD, select only external objects
				// Select the object
				KBObject o = SelectKbObject.SeleccionarObjetoLlamables();
                if (o == null)
                    return;

                if (IsSdGenerator)
                    new AndroidTools(Generator).OpenJavaSource(o);
                else
                    OpenCSharpSource(o);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        private void OpenCSharpSource(KBObject o)
        {
            // Get the main source file for the object
            ObjectSourceFiles sourceFiles = GeneratedSourceFilesCache.Cache(UIServices.KB.CurrentKB).GetSourceFiles(o);
			IEnumerable<string> sourceFilePaths = sourceFiles.GetAllSourceFiles(o, IsCSharpWebGenerator);
            if (!sourceFilePaths.Any())
            {
                MessageBox.Show($"Source file for {o.QualifiedName} cannot be calculated");
                return;
            }

            // Remove non existing
            var nonExisting = sourceFilePaths.Where(path => !File.Exists(path));
            sourceFilePaths = sourceFilePaths.Except(nonExisting);
            if(nonExisting.Any())
			{
                MessageBox.Show("These file does not exists:" + Environment.NewLine + string.Join(Environment.NewLine, nonExisting.ToArray()));
            }
            if (!sourceFilePaths.Any())
                return;

            // Get main source file
			List<string> sourceFilesList = sourceFilePaths.ToList();
            string mainFilePath = sourceFilesList[0];
            sourceFilesList.RemoveAt(0);

            // Open extra files
            var vs = new VisualStudio(LsiExtensionsConfiguration.Load().VisualStudioComId);
            sourceFilesList.ForEach(path => vs.EditFile(path, -1));

            // Open main file last, to set it visible
            // TODO: Allow to search by event / sub name
            vs.EditFile(mainFilePath, "void executePrivate( )");

            // Open the object, to see the source
            UIServices.Objects.Open(o, OpenDocumentOptions.CurrentVersion);
        }

        /// <summary>
        /// Remove backup files menu item clicked
        /// </summary>
        private void MiRemoveBackups_Click(object sender, EventArgs e)
        {
            new RemoveBackupFilesDlg(this).ShowDialog();
        }

        /// <summary>
        /// Build query objects selected
        /// </summary>
        private void MiBuildQueryObjects_Click(object sender, EventArgs e)
        {
            try
            {
                BuildQueryObjects buildQuery = new BuildQueryObjects(UIServices.KB.WorkingEnvironment.TargetModel);
                WWMains.StartBuild(this, buildQuery);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// Remove entry from all RSP files
        /// </summary>
        private void MiRemoveFromRsps_Click(object sender, EventArgs e)
        {
            // Select the file to remove:
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "C# source files|*.cs";
            dlg.InitialDirectory = Entorno.TargetDirectory;
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            if( MessageBox.Show("Are you sure you want to remove " + dlg.FileName + " from all RSP files?", 
                "Confirm", MessageBoxButtons.OKCancel) != DialogResult.OK)
                return;

            RspFile.RemoveFileFromAllRsps(Path.GetFileName(dlg.FileName));
        }

#region Link events

        /// <summary>
        /// GMatCon.ini link clicked
        /// </summary>
        private void LnkGMatCon_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(GMatConIniPath);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// Web / mobile link clicked
        /// </summary>
        private void LnkWebDir_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string path = IsSdGenerator ? "mobile" : "web";
                Process.Start(Entorno.GetTargetDirectoryFilePath(path));
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// bin link clicked
        /// </summary>
        private void LnkBinDir_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShellOpen(BinModuleDir);
        }

        /// <summary>
        /// Config target file link clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LnkConfigFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string path;
            if (IsCSharpWebGenerator)
                path = "web\\web.config";
            else
                path = "bin\\client.exe.config";
            ShellOpen(Entorno.GetTargetDirectoryFilePath(path));
        }

#endregion

#region Android


        private void MiApisSourcesDir_Click(object sender, EventArgs e)
        {
            new AndroidTools(Generator).OpenApisFolder();
        }

        private void MiEditExternalApi_Click(object sender, EventArgs e)
        {
            new AndroidTools(Generator).EditUserExternalApiFactory();
        }

#endregion

        
    }
}
