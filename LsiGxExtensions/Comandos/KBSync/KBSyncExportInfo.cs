using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.KBSync;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace LSI.Packages.Extensiones.Comandos.KBSync
{
    /// <summary>
    /// Tool to export versioning information about the selected objects
    /// </summary>
    public class KBSyncExportInfo : IExecutable
    {

        private void ExecuteThread()
        {
            try
            {
                using (Log log = new Log())
                {
                    // Get selected object
                    List<KBObject> selection = Entorno.NavigatorSelectedObjects;
                    if (selection == null || selection.Count == 0)
                    {
                        log.Output.AddWarningLine("There are no selected objects");
                        Log.MostrarVentana();
                        return;
                    }

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    // Get sync info
                    KBSyncInfo info = new KBSyncInfo(UIServices.KB.CurrentKB.Location);
                    info.AddObjects(selection, log);

                    // Get windows username:
                    string username = Entorno.ToSafeFilename(Entorno.WindowsUsername);

                    // Save the info to a file on the kb root
                    DateTime now = DateTime.Now;
                    string path = "syncInfo-" + now.Year + "-" +
                        now.Month.ToString().PadLeft(2, '0') + "-" +
                        now.Day.ToString().PadLeft(2, '0') + "-" +
                        now.Hour.ToString().PadLeft(2, '0') +
                        now.Minute.ToString().PadLeft(2, '0') +
                        now.Second.ToString().PadLeft(2, '0') + "-" +
                        username +
                        ".xml";
                    path = Entorno.GetLsiExtensionsFilePath(UIServices.KB.CurrentKB, path);
                    info.SaveToFile(path);

                    // Save path to registry:
                    LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.LoadFromRegistry();
                    cfg.LastKBSyncExportPath = path;
                    cfg.GuardarEnRegistro();

                    // Report status
                    log.Output.AddLine("Information exported for " + selection.Count + " objects");
                    log.Output.AddLine("Saved at " + path);

                    stopWatch.Stop();
                    log.PrintExecutionTime(stopWatch);

                    // Show sync review window, on UI thread
                    UIServices.Environment.Invoke(() =>
                    {
                        KBSyncReviewResultTW tw = 
                            UIServices.ToolWindows.CreateToolWindow(typeof(KBSyncReviewResultTW).GUID) 
                                as KBSyncReviewResultTW;
                        if (tw != null)
                        {
                            tw.ExportFilePath = path;
                            UIServices.ToolWindows.FocusToolWindow(tw.Id);
                        }
                    });

                }
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// Execute the sync info export
        /// </summary>
        public void Execute()
        {
            Thread t = new Thread(new ThreadStart(this.ExecuteThread));
            t.Start();
        }
    }
}
