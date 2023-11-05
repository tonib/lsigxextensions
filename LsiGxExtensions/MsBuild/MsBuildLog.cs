using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.VS;
using Artech.Architecture.Common.Objects;
using Artech.MsBuild.Common;
using LSI.Packages.Extensiones.Utilidades;
using System.IO;

namespace LSI.Packages.Extensiones.MsBuild
{
    /// <summary>
    /// Log storage for a genexus msbuild script (Singleton)
    /// </summary>
    public class MsBuildLog
    {

        /// <summary>
        /// Log sections. The key is the log title, and the value is the HTML encoded version
        /// of the log
        /// </summary>
        private List<KeyValuePair<string, string>> LogSections = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// The log does not contains errors?
        /// </summary>
        public bool ProcessOk { get; private set; }

        /// <summary>
        /// Singleton instance
        /// </summary>
        static private MsBuildLog _Instance;

        /// <summary>
        /// Singleton instance
        /// </summary>
        static public MsBuildLog Instance 
        {
            get
            {
                if (_Instance == null)
                    _Instance = new MsBuildLog();
                return _Instance;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private MsBuildLog()
        {
            ProcessOk = true;
        }

        /// <summary>
        /// Clear the current log
        /// </summary>
        public void Clear()
        {
            LogSections.Clear();
        }

        /// <summary>
        /// Add a new log section
        /// </summary>
        /// <param name="Kb">The task that creates the section</param>
        /// <param name="isWeb">True if the log refers to a web environment</param>
        /// <param name="title">Log title, plain text</param>
        /// <param name="content">Log content, plain text</param>
        /// <param name="withErrors">True if there was some error</param>
        /// <returns>True if no errors were found on the log section</returns>
        public bool AddSection(ArtechTask task, bool isWeb, string title, 
            string content, bool withErrors)
        {
            // Parse the log:
            CompilerLog logParser = new CompilerLog(content.ToString());
            string targetModelDir = Entorno.GetTargetDirectory(task.KB.DesignModel.Environment.TargetModel);
            if (isWeb)
                targetModelDir = Path.Combine(targetModelDir, "web");
            string htmlLog = logParser.ToHtml(targetModelDir);

            // Store the log:
            KeyValuePair<string, string> logSection = new KeyValuePair<string, string>(
                title, htmlLog);
            LogSections.Add(logSection);

            if (logParser.ContainsAnyError || withErrors)
            {
                ProcessOk = false;
                return false;
            }
            else
                return true;
        }

        /// <summary>
        /// The HTML version of the log
        /// </summary>
        public string Html
        {
            get
            {
                // Format the logs:
                string html = string.Empty;
                if (!ProcessOk)
                    html += "<p><b>WITH ERRORS</b></p>";

                foreach (KeyValuePair<string, string> log in LogSections)
                {
                    html +=
                        string.Format("<h2>{0}</h2><div style=\"font-family: Courier New,Monospace; font-size:12px\">{1}</div>",
                        log.Key, log.Value);
                }

                return html;
            }
        }

    }
}
