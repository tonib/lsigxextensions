using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.Logging
{
    public class BackgroundWorkerLogger : PlainTextLogger
    {
        private BackgroundWorker Worker;

        public BackgroundWorkerLogger(BackgroundWorker worker)
        {
            Worker = worker;
        }

        /// <summary>
        /// Log text into the process output
        /// </summary>
        /// <param name="text">Text to log</param>
        /// <param name="level">Text level</param>
        override public void LogText(string text, LogLevel level)
        {
            Worker.ReportProgress(0, AddLogLevelPrefix(text, level));
            base.LogText(text, level);
        }
    }
}
