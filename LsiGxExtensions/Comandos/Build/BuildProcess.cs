using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Threading;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using System.Xml.Serialization;

namespace LSI.Packages.Extensiones.Comandos.Build
{
    /// <summary>
    /// Build processes base class
    /// </summary>
    public abstract class BuildProcess : PlainTextLogger , IExecutable
    {

        /// <summary>
        /// Control to notify events. It can be null
        /// </summary>
        [XmlIgnore]
        public BuildLogControl LogControl;

        /// <summary>
        /// Current running process. null if no process is running
        /// </summary>
        [XmlIgnore]
        private Process CurrentProcess;

        /// <summary>
        /// True if the build process has been cancelled
        /// </summary>
        [XmlIgnore]
        public bool BuildCancelled { get; protected set; }

        /// <summary>
        /// True if the build process has errors
        /// </summary>
        [XmlIgnore]
        public bool BuildWithErrors { get; set; }

        /// <summary>
        /// True if this class uses the build functions of genexus.
        /// </summary>
        /// <remarks>
        /// As genexus does not support
        /// multiple concurrent builds, only one build that use those functions can be runing at
        /// same time
        /// </remarks>
        [XmlIgnore]
        abstract public bool IsInternalGxBuild { get; }

        public abstract void Execute();

        /// <summary>
        /// Log text into the work with mains log control
        /// </summary>
        /// <param name="text">Text to log</param>
        /// <param name="level">Text level</param>
        private void LogTextWorker(string text, LogLevel level)
        {
            if (LogControl == null)
                return;

            text = AddLogLevelPrefix(text, level);

            if (LogControl.Worker.IsBusy)
                LogControl.Worker.ReportProgress(0, text);
            else
                LogControl.LogText(text);
        }

        /// <summary>
        /// Log text into the process output
        /// </summary>
        /// <param name="text">Text to log</param>
        /// <param name="level">Text level</param>
        override public void LogText(string text, LogLevel level)
        {
            if( level == LogLevel.ERROR)
                BuildWithErrors = true;

            LogTextWorker(text, level);
            base.LogText(text, level);
        }

        public int ExecuteProcess(Process process, bool changeDirToExeDir)
        {
            if (CurrentProcess != null)
                throw new Exception("A process is already running");

            try
            {
                CurrentProcess = process;
                if (changeDirToExeDir)
                    CurrentProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(CurrentProcess.StartInfo.FileName);
                /*LogLine("\"" + process.StartInfo.FileName + "\" " + process.StartInfo.Arguments);

                // Crear y preparar el proceso para que redirija las salidas estandar
                CurrentProcess.StartInfo.RedirectStandardOutput = true;
                CurrentProcess.StartInfo.CreateNoWindow = true;
                CurrentProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                CurrentProcess.StartInfo.UseShellExecute = false;
                CurrentProcess.StartInfo.RedirectStandardOutput = true;
                CurrentProcess.StartInfo.RedirectStandardError = true;
                // Te cagas: Utiliza la codificacion original de MSDOS... (http://en.wikipedia.org/wiki/Code_page_437)
                CurrentProcess.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(437);

                CurrentProcess.Start();

                // Ir escribiendo la salida estandar a medida que se produzca
                while (!CurrentProcess.WaitForExit(1000))
                    LogStream(CurrentProcess.StandardOutput);

                // Write pending output
                LogStream(CurrentProcess.StandardOutput);

                // Write all error output:
                LogStream(CurrentProcess.StandardError);

                return CurrentProcess.ExitCode;*/
                return ProcessLogger.StartAndLogProcess(CurrentProcess, this);
            }
            finally
            {
                CurrentProcess = null;
            }
        }

        public void ExecuteProcess(string exePath, string parameters)
        {

            Process process = new Process();
            process.StartInfo.FileName = exePath;
            if (parameters != null)
                process.StartInfo.Arguments = parameters;
            ExecuteProcess(process, true);
        }

        /// <summary>
        /// It cancels the build process
        /// </summary>
        public virtual void Cancel() 
        {
            BuildCancelled = true;
            if (CurrentProcess != null)
            {
                try
                {
                    KillProcessAndChildren(CurrentProcess.Id);
                }
                catch (InvalidOperationException)
                {
                    // Process already finished
                }
                catch (Exception ex)
                {
                    Log.ShowException(ex);
                }
            }
        }

        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        private static void KillProcessAndChildren(int pid)
        {
            // Kill children process
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }

            // Kill main process
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

    }
}
