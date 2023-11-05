using LSI.Packages.Extensiones.Utilidades.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.Threading
{
    /// <summary>
    /// Logs a process execution
    /// </summary>
    public class ProcessLogger
    {
        /// <summary>
        /// Prepare
        /// </summary>
        /// <param name="process"></param>
        /// <param name="logger"></param>
        static public int StartAndLogProcess(Process process, PlainTextLogger logger)
        {
            logger.LogLine("\"" + process.StartInfo.FileName + "\" " + process.StartInfo.Arguments);

            // Configure process to read std output and error
            ConfigureRedirectOutput(process, true);

            // Te cagas: Utiliza la codificacion original de MSDOS... (http://en.wikipedia.org/wiki/Code_page_437)
            process.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(437);

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                if (e.Data!=null) 
                    logger.AddLine(e.Data);
            };
            process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                    logger.AddLine(e.Data);
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return process.ExitCode;
        }

        public static void ConfigureRedirectOutput(Process process, bool redirectStdErr)
        {
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = redirectStdErr;
        }
    }
}
