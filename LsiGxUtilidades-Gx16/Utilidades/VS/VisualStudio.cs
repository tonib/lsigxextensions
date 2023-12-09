using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using System.Runtime.InteropServices;
using System.IO;

namespace LSI.Packages.Extensiones.Utilidades.VS
{
    /// <summary>
    /// Visual studio COM operations
    /// VER: http://blogs.msdn.com/b/kirillosenkov/archive/2009/03/03/how-to-start-visual-studio-programmatically.aspx
    /// VER: http://www.codeproject.com/Articles/261638/Automate-the-attach-to-process
    /// </summary>
    public class VisualStudio
    {

        /// <summary>
        /// COM reference name for VS 2008
        /// </summary>
        public const string VS_2008_COMID = @"VisualStudio.DTE.9.0";

        /// <summary>
        /// COM reference name for VS 2010
        /// </summary>
        public const string VS_2010_COMID = @"VisualStudio.DTE.10.0";

        /// <summary>
        /// COM reference name for VS 2012
        /// </summary>
        public const string VS_2012_COMID = @"VisualStudio.DTE.11.0";

        /// <summary>
        /// COM reference name for VS 2015
        /// </summary>
        public const string VS_2015_COMID = @"VisualStudio.DTE.14.0";

        public const string VS_2017_COMID = @"VisualStudio.DTE.15.0";

        public const string VS_2019_COMID = @"VisualStudio.DTE.16.0";


        [DllImport("User32.dll", EntryPoint = "SetForegroundWindow")]
        private static extern IntPtr SetForegroundWindow(int hWnd);

        /// <summary>
        /// Visual studio running instance reference
        /// </summary>
        static private DTE Dte;

        /// <summary>
        /// Visual studio COM string identifier
        /// </summary>
        private string VisualStudioComId;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="visualStudioComId">Visual studio COM string identifier</param>
        public VisualStudio(string visualStudioComId)
        {
            this.VisualStudioComId = visualStudioComId;

            // Get or create the Visual Studio Instance
            try
            {
                Dte = (DTE)Marshal.GetActiveObject(VisualStudioComId);
                Dte.MainWindow.Visible = true;
            }
            catch (COMException)
            {
                Dte = null;
            }

            if (Dte == null)
            {
                // No hay instacia activa del visual studio. Arrancarlo:
                Type visualStudioType = Type.GetTypeFromProgID(VisualStudioComId);
                if (visualStudioType == null)
                    throw new Exception("No Visual Studio found with COM id '" + VisualStudioComId +
                        "'. Check the extensions configuration");

                Dte = Activator.CreateInstance(visualStudioType) as DTE;
                Dte.MainWindow.Visible = true;
            }
            ActivateVSWindow();
        }


        /// <summary>
        /// Attach a process to VS to debug it
        /// </summary>
        /// <param name="processId">Process id to debug</param>
        /// <param name="breakDebug">True if the process should be stopped to debug</param>
        public void AttachProcess(int processId, bool breakDebug)
        {
            // Try loop - visual studio may not respond the first time.
            int tryCount = 10;
            bool attached = false;
            while (tryCount-- > 0)
            {
                try
                {
                    Process pToDebug = Dte.Debugger.LocalProcesses
                        .Cast<Process>()
                        .Where(p => p.ProcessID == processId)
                        .FirstOrDefault();
                    if (pToDebug == null)
                        throw new Exception("Process ID " + processId + " not found");
                    pToDebug.Attach();
                    attached = true;
                    break;
                }
                catch (COMException)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }

            if (!attached)
                throw new Exception("Error trying to attach the process");
            else
            {
                if (breakDebug)
                {
                    // Try to stop
                    tryCount = 5;
                    while (tryCount-- > 0)
                    {
                        try
                        {
                            // I have tried Break(true) and it does not wait...
                            Dte.Debugger.Break(false);
                            break;
                        }
                        catch
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// It sets the visual studio window as the active window
        /// </summary>
        public void ActivateVSWindow()
        {
            try 
            {
                Dte.MainWindow.WindowState = vsWindowState.vsWindowStateMaximize;
                Dte.MainWindow.Activate();
                SetForegroundWindow(Dte.MainWindow.HWnd);
            }
            catch {}
        }

        private void ActivateVSWindow(EnvDTE.Window w)
        {
            try
            {
                w.Activate();
            }
            catch { }
        }

        /// <summary>
        /// Abre un archivo .CS en el visual studio, posicionandose en la linea indicada
        /// Sacado de http://stackoverflow.com/questions/350323/open-a-file-in-visual-studio-at-a-specific-line-number
        /// </summary>
        /// <param name="filePath">Parth al archivo CS a abrir</param>
        /// <param name="lineNumber"Line number where to put cursor. TODO: 1 == first line?
        /// If < 0, no line will be selected</param>
        public void EditFile(string filePath, int lineNumber)
        {
            EnvDTE.Window w = Dte.ItemOperations.OpenFile(filePath, Constants.vsViewKindCode);
            ActivateVSWindow(w);

            object o = Dte.ActiveDocument;
            TextSelection s = Dte.ActiveDocument.Selection as TextSelection;
            if(lineNumber >= 0)
                s.GotoLine(lineNumber, true);
        }

        /// <summary>
        /// Open a file in Visual Studio and go to a line with a given text
        /// </summary>
        /// <param name="filePath">File path to open</param>
        /// <param name="textPatternLine">Text to search in the file to position the VS view</param>
        public void EditFile(string filePath, string textPatternLine)
        {
            EnvDTE.Window w = Dte.ItemOperations.OpenFile(filePath, Constants.vsViewKindCode);
            ActivateVSWindow(w);
            object o = Dte.ActiveDocument;
            TextSelection s = Dte.ActiveDocument.Selection as TextSelection;
            if (s.FindText(textPatternLine, (int) vsFindOptions.vsFindOptionsNone))
                s.SelectLine();
            else if (s.FindText(textPatternLine, (int) vsFindOptions.vsFindOptionsFromStart))
                s.SelectLine();
        }

        /// <summary>
        /// Abre un ejecutable con el visual studio, para que se pueda hacer el debug inicial
        /// </summary>
        /// <param name="pathEjecutable"></param>
        public void IniciarDepurando(string pathEjecutable)
        {
            Solution s = Dte.Solution as Solution;
            s.Close(false);
            s.AddFromFile(pathEjecutable, false);
        }

    }
}
