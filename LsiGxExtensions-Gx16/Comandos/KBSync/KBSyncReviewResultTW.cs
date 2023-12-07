using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LSI.Packages.Extensiones.Utilidades.UI;
using System.Runtime.InteropServices;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Services;
using Artech.Architecture.Common.Services;
using LSI.Packages.Extensiones.Utilidades;
using Artech.Architecture.BL.Framework.Services;
using Artech.Architecture.Common.Objects;
using LSI.Packages.Extensiones.Utilidades.KBSync;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos.KBSync
{
    /// <summary>
    /// Tool to check exportable / not exportable objects
    /// </summary>
    [Guid("179940A1-D4CC-422f-8AC7-0DA6392C2BCA")]
    public partial class KBSyncReviewResultTW : ToolWindowBase
    {

        public const string EXPORTABLE = "Exportable";
        public const string NOTEXPORTABLE = "Not exportable";
        public const string UNMODIFIED = "Unmodified on source KB";

        /// <summary>
        /// Current sync info
        /// </summary>
        private KBSyncInfo SyncInfo;

        /// <summary>
        /// Constructor
        /// </summary>
        public KBSyncReviewResultTW()
        {
            InitializeComponent();

            // Initialize toolwindow:
            this.Icon = Resources.LSI;

            Grid.CrearColumnasEstandar();
            Grid.SetObjetos<KBSyncObjectRef>(new SortableList<KBSyncObjectRef>());

            CmbObjectsType.Items.Add(EXPORTABLE);
            CmbObjectsType.Items.Add(NOTEXPORTABLE);
            CmbObjectsType.Items.Add(UNMODIFIED);
            CmbObjectsType.SelectedItem = NOTEXPORTABLE;
        }

        public string ExportFilePath
        {
            set
            {
                FileChanged(value);
            }
        }

        private void BtnSelFile_Click(object sender, EventArgs e)
        {
            string path = KBSyncReviewInfo.AskForExportFilePath();
            if (path == null)
                return;

            FileChanged(path);
        }

        private void FileChanged(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                KBSyncInfo info = KBSyncInfo.LoadFromFile(path);
                if (info.OriginKBase.ToLower() != UIServices.KB.CurrentKB.Location.ToLower())
                {
                    MessageBox.Show("Sync file origin kbase " + info.OriginKBase +
                        " does not match this kbase");
                    return;
                }

                TxtExportFile.Text = path;
                SyncInfo = info;
                if (string.IsNullOrEmpty(SyncInfo.DestinationKBase))
                    TxtDestinationKb.Text = "* SYNC FILE NOT REVIEWED YET ON DESTINATION KB";
                else
                    TxtDestinationKb.Text = SyncInfo.DestinationKBase;
                CmbObjectsType_SelectedIndexChanged(null, null);
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        private KBSyncObjectInfo.ExportStatus SelectedStatus
        {
            get
            {
                string selectedType = (string) CmbObjectsType.SelectedItem;
                if (selectedType == EXPORTABLE)
                    return KBSyncObjectInfo.ExportStatus.EXPORTABLE;
                else if (selectedType == NOTEXPORTABLE)
                    return KBSyncObjectInfo.ExportStatus.NOTEXPORTABLE;
                else
                    return KBSyncObjectInfo.ExportStatus.UNMODIFIED;
            }
        }

        private void CmbObjectsType_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            ColStartVersionId.Visible =
            BtnViewChanges.Enabled = 
                (SelectedStatus != KBSyncObjectInfo.ExportStatus.UNMODIFIED);

            if (SyncInfo == null)
                return;

            IList<KBSyncObjectRef> objectsList = Grid.GetObjetos<KBSyncObjectRef>();
            objectsList.Clear();

            foreach (KBSyncObjectInfo o in SyncInfo.ObjectsInfo.Where(x => x.Status == SelectedStatus))
                objectsList.Add(o.ObjectRef);
        }

        private void BtnReloadFile_Click(object sender, EventArgs e)
        {
            FileChanged(TxtExportFile.Text);
        }

        private void BtnViewChanges_Click(object sender, EventArgs e)
        {
            KBObject o = Grid.ObjetoSeleccionado;
            if( o == null )
                return;

            KBSyncObjectInfo info = SyncInfo.GetObjectInfo(o);
            if( info == null ) 
            {
                MessageBox.Show("Object versioning information not found");
                return;
            }

            if( info.VersionIdUpdateOrigin == 0 )
                // Object must to be entirely reviewed
                UIServices.Objects.Open(o, OpenDocumentOptions.CurrentVersion);
            else {
                // TODO: There is not other way to do this???
                foreach (KBObject oldVersion in o.GetVersionsDescendent())
                {
                    if (oldVersion.VersionId == info.VersionIdUpdateOrigin)
                    {
                        UIServices.Comparer.CompareWithCurrentRevision(oldVersion);
                        return;
                    }
                }
                MessageBox.Show("Version " + info.VersionIdUpdateOrigin + " not found");
            }
        }

        private void UpdateStatusBar()
        {
            if (SyncInfo == null)
                LblStatus.Text = string.Empty;
            else
                LblStatus.Text = SyncInfo.ReviewStatusMessage;
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            OpenDocumentation.Open("sync.html");
        }

    }
}
