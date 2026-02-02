using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Management;

namespace LSI.Packages.Extensiones.Utilidades.Threading
{
    /// <summary>
    /// Running processes utilities
    /// </summary>
    public class ProcessUtils
    {

        /// <summary>
        /// Get processes runing an exe file.
        /// </summary>
        /// <param name="exeName">Exe name. It can be the single file name or the absolute 
        /// path</param>
        /// <returns>Processes running the file</returns>
        static public List<Process> GetByExeName(string exeName)
        {
            exeName = Path.GetFileNameWithoutExtension(exeName).ToLower();
            return Process.GetProcessesByName(exeName).ToList();
        }

        /// <summary>
        /// Returns the command line that launched the process
        /// </summary>
        /// <param name="process">Process to check</param>
        /// <returns>The command line. null if not found</returns>
        static public string GetCommandLine(Process process)
        {
            using (var searcher = new ManagementObjectSearcher(
                "SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["CommandLine"]?.ToString();
                }
            }
            return null;
        }

        static public string GetWindowTitle(Process p)
        {
            string title = p.MainWindowTitle;
            if (string.IsNullOrEmpty(title))
                title = p.ProcessName;

            try
            {
                NativeWindowEnumerator e = new NativeWindowEnumerator(p);
                Dictionary<IntPtr, string> windows = e.GetProcessWindows();
                if (windows.Count > 0)
                    title += " - Child windows: ( " + string.Join(", ", windows.Values.ToArray()) + " )";
            }
            catch { }
            title += " - PID: " + p.Id ;
            return title;
        }

    }
}
