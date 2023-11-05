using Artech.MsBuild.Common;
using LSI.Packages.Extensiones.Comandos.Build;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.MsBuild
{

    /// <summary>
    /// Base class for LSI custom MsBuild tasks
    /// </summary>
    public abstract class LsiMsBuildTask : ArtechTask
    {

        /// <summary>
        /// The task execution was successful?
        /// </summary>
        protected bool TaskOk = true;

        /// <summary>
        /// Run and log the process
        /// </summary>
        /// <param name="buildProcess">Process to run</param>
        /// <param name="log">Main log</param>
        /// <param name="buildTitle">Title of the process</param>
        /// <param name="isWeb">True if the process is associated to a web generator</param>
        protected void RunBuildProcess(BuildProcess buildProcess, Log log, string buildTitle, bool isWeb)
        {
            // If there is currently a subscription to the gx output, it should be suspended 
            // while the process is running (BuildProcess has its own log system)
            if (LsiLogSectionStart.CurrentSection != null)
                LsiLogSectionStart.CurrentSection.CurrentLogger.UnsuscribeGxOutput();

            try
            {
                // Run the process
                buildProcess.SaveLog = true;
                buildProcess.ProcessLog = log;
                buildProcess.Execute();

                // Store the process log
                if (!MsBuildLog.Instance.AddSection(this, isWeb, buildTitle, buildProcess.TextLog.ToString(),
                    buildProcess.BuildWithErrors))
                    TaskOk = false;
            }
            finally
            {
                if (LsiLogSectionStart.CurrentSection != null)
                    LsiLogSectionStart.CurrentSection.CurrentLogger.SubscribeGXOutput();
            }
        }

    }
}
