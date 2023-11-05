using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.Threading;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.UI.Framework.Packages;
using Artech.Common.Framework.Selection;
using Artech.Architecture.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using Artech.Udm.Framework;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using LSI.Packages.Extensiones.Utilidades.KBSync;
using System.Diagnostics;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos.KBSync
{

    /// <summary>
    /// Tool to review objects versioning info on destination KB, to check importable /
    /// not importable objects.
    /// </summary>
    public class KBSyncReviewInfo : IExecutable
    {

        /// <summary>
        /// Sync file to review
        /// </summary>
        private string SyncFilePath;

        static public string AskForExportFilePath()
        {
            // Get the last export file path:
            LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.LoadFromRegistry();
            string lastPath = cfg.LastKBSyncExportPath;

            // Ask the export file path:
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Xml files|*.xml";
            if (File.Exists(lastPath))
            {
                dlg.InitialDirectory = Path.GetDirectoryName(lastPath);
                dlg.FileName = Path.GetFileName(lastPath);
            }
            if (dlg.ShowDialog() != DialogResult.OK)
                return null;

            return dlg.FileName;
        }

        private void ExecuteThread()
        {
            try
            {
                using (Log log = new Log())
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    // Load the sync info
                    log.Output.AddLine("Loading " + SyncFilePath);
                    KBSyncInfo info = KBSyncInfo.LoadFromFile(SyncFilePath);

                    if (info.OriginKBase.ToLower() == UIServices.KB.CurrentKB.Location.ToLower())
                    {
                        log.Output.AddErrorLine("Sync file cannot be reviewed on the origin kb");
                        Log.MostrarVentana();
                        return;
                    }

                    // Review info
                    log.Output.AddLine("Reviewing objects");
                    info.ReviewOnDestinationKB(UIServices.KB.CurrentKB.Location, log);

                    log.Output.AddLine(info.ReviewStatusMessage);

                    // Save file
                    log.Output.AddLine("Saving file");
                    info.SaveToFile(SyncFilePath);

                    stopWatch.Stop();
                    log.PrintExecutionTime(stopWatch);
                    Log.MostrarVentana();
                }
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// Run the review process
        /// </summary>
        public void Execute()
        {

            SyncFilePath = AskForExportFilePath();
            if (SyncFilePath == null)
                return;

            Thread t = new Thread(new ThreadStart(this.ExecuteThread));
            t.Start();
        }
    }
}
