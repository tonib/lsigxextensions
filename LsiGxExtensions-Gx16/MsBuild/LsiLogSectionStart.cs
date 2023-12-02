using System;
using Artech.Architecture.Common.Services;
using LSI.Packages.Extensiones.Comandos.Build;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.MsBuild
{
    /// <summary>
    /// Msbuild task to start record logging to send an email
    /// </summary>
    public class LsiLogSectionStart : LsiMsBuildTask
    {

        /// <summary>
        /// Section title for the current log
        /// </summary>
        public string SectionTitle { get; set; }

        /// <summary>
        /// Section refers to web generator?
        /// </summary>
        public bool IsWeb { get; set; }

        /// <summary>
        /// Current logger
        /// </summary>
        internal PlainTextLogger CurrentLogger;

        /// <summary>
        /// Current section
        /// </summary>
        static internal LsiLogSectionStart CurrentSection;

        public LsiLogSectionStart() 
        {
            SectionTitle = "GxBuild";
            IsWeb = true;
        }

        /// <summary>
        /// Run the task
        /// </summary>
        /// <returns>False if the msbuild script should be stopped</returns>
        public override bool Execute()
        {
            try
            {
                OutputSubscribe();

                CurrentSection = this;
                CurrentLogger = new PlainTextLogger();
                CurrentLogger.SaveLog = true;
                CurrentLogger.SubscribeGXOutput();

                return true;
            }
            catch (Exception ex)
            {
                CommonServices.Output.AddErrorLine(ex.Message);
                return true;
            }
            finally
            {
                OutputUnsubscribe();
            }
        }

        internal void CloseCurrentSection()
        {
            CurrentLogger.UnsuscribeGxOutput();

            string logText = CurrentLogger.TextLog.ToString();
            if( !string.IsNullOrEmpty(logText))
                MsBuildLog.Instance.AddSection(this, IsWeb, SectionTitle, logText, false);

            CurrentSection = null;
        }
    }
}
