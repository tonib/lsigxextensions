using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Udm.Framework;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LSI.Packages.Extensiones.Comandos
{

    /// <summary>
    /// Toolwindows to search unreferenced objects
    /// </summary>
    [Guid("2607CB28-5676-404c-BCE5-2E12079B1A72")]
    public partial class UnreferencedObjectsToolWindow : ToolWindowBase
    {

        /// <summary>
        /// Folder / module item to ignore
        /// </summary>
        private class FolderOrModuleToIgnore
        {
            /// <summary>
            /// Parent identifier
            /// </summary>
            public EntityKey Id;

            /// <summary>
            /// Parent name
            /// </summary>
            public string Name;

            public override string ToString() { return Name; }

            public FolderOrModuleToIgnore(IKBObjectParent f) {
                Id = f.Key;
                Name = ( f is Folder ? "F. " : "M. " ) + f.QualifiedName.ToString();
            }
        }

        /// <summary>
        /// Background search process
        /// </summary>
        private BackgroundWorker BackgroundSearch = new BackgroundWorker();

        /// <summary>
        /// Constructor
        /// </summary>
        public UnreferencedObjectsToolWindow()
        {
            InitializeComponent();

            // Initialize toolwindow:
            this.Icon = Resources.LSI;
            LblState.Text = "";
            Grid.CrearColumnasEstandar();
            Grid.SetObjetos<RefObjetoGX>(new SortableList<RefObjetoGX>());

            // Forms to check:
            foreach( UnreferencedObjectsFinder.FormsToCheckEnum f in Enum.GetValues(typeof(UnreferencedObjectsFinder.FormsToCheckEnum)) )
                CmbForms.Items.Add(f);
            CmbForms.SelectedItem = UnreferencedObjectsFinder.FormsToCheckEnum.WIN;

            // Initialize background search
            BackgroundSearch.WorkerSupportsCancellation = true;
            BackgroundSearch.WorkerReportsProgress = true;
            BackgroundSearch.DoWork += new DoWorkEventHandler(BackgroundSearch_DoWork);
            BackgroundSearch.ProgressChanged += new ProgressChangedEventHandler(BackgroundSearch_ProgressChanged);
            BackgroundSearch.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundSearch_RunWorkerCompleted);

            Grid_SelectionChanged(null, null);
            LstIgnoreFolders_SelectedIndexChanged(null, null);
            ChkAttributesForm_CheckedChanged(null, null);
        }

        private void EnableUIFields(bool enabled)
        {
            LstIgnoreFolders.Enabled = BtnAdd.Enabled = BtnRemoveSelected.Enabled =
                BtnDefaultIgnore.Enabled = ChkCallableObjects.Enabled = 
                ChkAttributesNoTable.Enabled = ChkAttributesOnlyTrn.Enabled = 
                ChkReadOnlyAtrs.Enabled = CmbForms.Enabled = chkIgnoreGxUser.Enabled =
                enabled;
            
            if( enabled )
                ChkAttributesForm_CheckedChanged(null, null);
        }

        /// <summary>
        /// Background search finished
        /// </summary>
        void BackgroundSearch_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LblState.Text = "N. found: " + Grid.GetObjetos<RefObjetoGX>().Count;
            BtnSearch.Text = "&Search";
            EnableUIFields(true);
            PicActivity.Enabled = PicActivity.Visible = false;
        }

        /// <summary>
        /// Background seach reported some progress
        /// </summary>
        void BackgroundSearch_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Se ha reportado una busqueda encontrada
            if (e.UserState is RefObjetoGX)
                Grid.GetObjetos<RefObjetoGX>().Add((RefObjetoGX)e.UserState);
            else if (e.UserState is Exception)
                Log.ShowException((Exception)e.UserState);
            else if (e.UserState is string)
                LblState.Text = (string)e.UserState;
        }

        /// <summary>
        /// Background search execution
        /// </summary>
        void BackgroundSearch_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // Obtener el objeto que ejecutar la busqueda
                UnreferencedObjectsFinder finder = (UnreferencedObjectsFinder)e.Argument;
                finder.ExecuteUISearch();
            }
            catch (Exception ex)
            {
                BackgroundSearch.ReportProgress(0, ex);
            }
        }

        private void AddIgnoreFolderOrModule(bool isFolder, string folderOrModuleName)
        {
            if (isFolder)
            {
                Folder f = Folder.Get(UIServices.KB.CurrentModel, new QualifiedName(folderOrModuleName));
                if (f != null)
                {
                    AddIgnoreFolderOrModule(f);
                    return;
                }
            }
            else
            {
                Module m = Module.Get(UIServices.KB.CurrentModel, new QualifiedName(folderOrModuleName));
                if (m != null)
                {
                    AddIgnoreFolderOrModule(m);
                    return;
                }
            }
        }

        private void AddIgnoreFolderOrModule(IKBObjectParent f)
        {
            if (ParentsToIgnoreList.Any(x => x.Guid == f.Guid))
                return;
            LstIgnoreFolders.Items.Add(new FolderOrModuleToIgnore(f));
        }

        /// <summary>
        /// Button to add folder to ignore clicked
        /// </summary>
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            IKBObjectParent f = SelectKbObject.SelectFolderOrModule();
            if (f == null)
                return;
            AddIgnoreFolderOrModule(f);
        }

        /// <summary>
        /// Returns the selected folders on the UI list
        /// </summary>
        private List<FolderOrModuleToIgnore> SelectedIgnoreModuleFolders()
        {
            List<FolderOrModuleToIgnore> folders = new List<FolderOrModuleToIgnore>();
            foreach (FolderOrModuleToIgnore f in LstIgnoreFolders.SelectedItems)
                folders.Add(f);
            return folders;
        }

        /// <summary>
        /// Button to remove selected folders to ignore clicked
        /// </summary>
        private void BtnRemoveSelected_Click(object sender, EventArgs e)
        {
            SelectedIgnoreModuleFolders().ForEach(x => LstIgnoreFolders.Items.Remove(x));
        }

        private List<IKBObjectParent> ParentsToIgnoreList
        {
            get
            {
                List<IKBObjectParent> parentsToIgnore = new List<IKBObjectParent>();
                foreach (FolderOrModuleToIgnore f in LstIgnoreFolders.Items)
                    parentsToIgnore.Add(KBObject.Get(UIServices.KB.CurrentModel, f.Id) as IKBObjectParent);
                return parentsToIgnore;
            }
        }

        /// <summary>
        /// Search / cancel button pressed
        /// </summary>
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (BackgroundSearch.IsBusy)
                {
                    // Search cancelled
                    BackgroundSearch.CancelAsync();
                    return;
                }

                if (!(ChkReadOnlyAtrs.Checked || ChkAttributesOnlyTrn.Checked ||
                       ChkAttributesNoTable.Checked || ChkCallableObjects.Checked))
                {
                    MessageBox.Show("Please, specify something to search");
                    ChkCallableObjects.Focus();
                    return;
                }

                // Get list of folders to ignore
                List<IKBObjectParent> foldersToIgnore = ParentsToIgnoreList;

                // Create the finder
                UnreferencedObjectsFinder search = new UnreferencedObjectsFinder(foldersToIgnore);
                search.BackgroundSearch = BackgroundSearch;
                search.CheckAttributesWithNoTable = ChkAttributesNoTable.Checked;
                search.CheckUnreferencedCallables = ChkCallableObjects.Checked;
                search.CheckAttributesOnlyTrn = ChkAttributesOnlyTrn.Checked;
                search.FormsToCheck = (UnreferencedObjectsFinder.FormsToCheckEnum)CmbForms.SelectedItem;
                search.CheckReadOnlyAttributes = ChkReadOnlyAtrs.Checked;
                search.IgnoreGenexusUser = chkIgnoreGxUser.Checked;

                // Launch the search
                BtnSearch.Text = "Cancel";
                EnableUIFields(false);
                Grid.GetObjetos<RefObjetoGX>().Clear();
                LblState.Text = "Searching...";
                PicActivity.Enabled = PicActivity.Visible = true;
                BackgroundSearch.RunWorkerAsync(search);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// Remove selected objects clicked
        /// </summary>
        private void BtnRemoveObjects_Click(object sender, EventArgs e)
        {
            Grid.DeleteSelectedObjects();
        }

        /// <summary>
        /// Replace variables button clicked
        /// </summary>
        private void BtnReplaceVariablesAtr_Click(object sender, EventArgs e)
        {
            List<KBObject> selection = Grid.ObjetosSeleccionados;
            if (selection.Count == 0)
                return;
            Artech.Genexus.Common.Objects.Attribute attr = selection[0]
                as Artech.Genexus.Common.Objects.Attribute;
            if (attr == null)
                return;

            new ReplaceVariablesAttributeBasedWindow(attr).Execute();
        }

        /// <summary>
        /// Grid selection changed. Enables operation buttons
        /// </summary>
        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            List<KBObject> selection = Grid.ObjetosSeleccionados;
            BtnRemoveObjects.Enabled = (selection.Count > 0);

            BtnReplaceVariablesAtr.Enabled = 
                (selection.Count == 1 && selection[0] is Artech.Genexus.Common.Objects.Attribute);
        }

        /// <summary>
        /// Unreferenced callable objects check clicked
        /// </summary>
        private void ChkCallableObjects_CheckedChanged(object sender, EventArgs e)
        {
            GrpIgnoreFolders.Enabled = ChkCallableObjects.Checked;
        }

        /// <summary>
        /// Clicked add default folders to ignore
        /// </summary>
        private void BtnDefaultIgnore_Click(object sender, EventArgs e)
        {
            AddIgnoreFolderOrModule(true, "SmartDevicesApi");
            AddIgnoreFolderOrModule(true, "CommonApi");
            AddIgnoreFolderOrModule(true, "WebApi");
            AddIgnoreFolderOrModule(true, "GeneralWeb");
            AddIgnoreFolderOrModule(true, "QueryViewer");

            if (LsiExtensionsConfiguration.PrivateExtensionsInstalled)
            {
                // LSI custom folders to ignore:
                AddIgnoreFolderOrModule(true, "Puntual");
                AddIgnoreFolderOrModule(true, "Modulos");
                AddIgnoreFolderOrModule(true, "mLocal");
                AddIgnoreFolderOrModule(true, "Estilos");
                AddIgnoreFolderOrModule(true, "Traspasos");     // Milton demand 16/5/17: Do not delete anything on this folder (HyA)
                AddIgnoreFolderOrModule(true, "Importar");      // Milton demand 16/5/17: Do not delete anything on this folder (HyA)
            }
        }

        /// <summary>
        /// Folders to ignore selection changed
        /// </summary>
        private void LstIgnoreFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            BtnRemoveSelected.Enabled = LstIgnoreFolders.SelectedIndices.Count > 0;
        }

        /// <summary>
        /// Check attributes only in transactions or Check read only attributes clicked
        /// </summary>
        private void ChkAttributesForm_CheckedChanged(object sender, EventArgs e)
        {
            CmbForms.Enabled = ChkAttributesOnlyTrn.Checked || ChkReadOnlyAtrs.Checked;
        }

        /// <summary>
        /// Help button clicked
        /// </summary>
        private void BtnHelp_Click(object sender, EventArgs e)
        {
            OpenDocumentation.Open("noreferenciados.html");
        }

    }
}
