using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LSI.Packages.Extensiones.Utilidades;
using System.Diagnostics;
using LSI.Packages.Extensiones.Utilidades.Threading;
using Artech.Architecture.UI.Framework.Services;
using LSI.Packages.Extensiones.Utilidades.VS;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos.Build
{
    /// <summary>
    /// Control that logs a build process
    /// </summary>
    public partial class BuildLogControl : UserControl
    {

        /// <summary>
        /// Maximum log lenght, in characters
        /// </summary>
        private const int MAXLOGLENGHT = 1024 * 1024;

        /// <summary>
        /// Process to run
        /// </summary>
        internal BuildProcess Process;

        /// <summary>
        /// Worker to run the build process in background
        /// </summary>
        internal BackgroundWorker Worker;

        /// <summary>
        /// True if we should close the tab when it has finished its work
        /// </summary>
        private bool CloseWhenFinished;

        /// <summary>
        /// Owner generator tab
        /// </summary>
        private GeneratorMains GeneratorTab;

        /// <summary>
        /// Elapsed execution time
        /// </summary>
        private Stopwatch ExecutionTime = new Stopwatch();

        /// <summary>
        /// Process has finished its execution?
        /// </summary>
        public bool ProcessFinished;

        /// <summary>
        /// The compiler parsed log
        /// </summary>
        private CompilerLog ParsedLog = new CompilerLog();

        private bool FistError = true;

        public BuildLogControl(GeneratorMains generatorTab, BuildProcess process)
        {
            InitializeComponent();

            this.GeneratorTab = generatorTab;

            // Prepare background worker
            Worker = new BackgroundWorker();
            Worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
            Worker.ProgressChanged += new ProgressChangedEventHandler(Worker_ProgressChanged);
            Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;

            // Prepare process
            Process = process;
            Process.LogControl = this;

            // Milton request:
            LogLine("Build started " + UIServices.KB.WorkingEnvironment.TargetModel.Name + 
                " (" + DateTime.Now + ")" );

            BtnOpenError.Visible = BtnNextError.Visible = BtnRepairAndRecompile.Visible = false;
            TxtLog.Focus();
        }

        /// <summary>
        /// Runs the background build
        /// </summary>
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Process.Execute();
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// Background  build reported some change
        /// </summary>
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            LogText(e.UserState as string);
        }

        /// <summary>
        /// Background build finished
        /// </summary>
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ExecutionTime.Stop();
            double seconds = Math.Round( ExecutionTime.Elapsed.TotalMilliseconds / 1000.0 , 2 );
            LogLine("Build finished (" + DateTime.Now + " - " + seconds + " seconds)");

            ProcessFinished = true;
            BtnCancel.Enabled = false;
            BtnClose.Enabled = true;

            GeneratorTab.WWMains.UpdateCompilationDates();
            GeneratorTab.WWMains.UpdateNotifyIcon();

            if (CloseWhenFinished)
                DoClose();
            else
            {
                UpdateTabTitle();
                if (!Process.BuildCancelled)
                {
                    ToolTipIcon i = Process.BuildWithErrors ? ToolTipIcon.Error : ToolTipIcon.None;
                    GeneratorTab.WWMains.DisplayNotification("Build finished", Process.ToString(), i);
                }

                // Check missing source files errors:
                if( GeneratorTab.IsCSharpWinGenerator )
                    CheckMissingSources();
            }
        }

        /// <summary>
        /// Parse the compiler log to search missing source files
        /// </summary>
        private void CheckMissingSources()
        {
            Dictionary<string, HashSet<string>> missing = ParsedLog.GetSourcesNotFoundByModule();
            if (missing.Count > 0)
            {
                BtnRepairAndRecompile.Visible = true;
                LogLine("Missed following source files:");
                foreach (string module in missing.Keys)
                {
                    LogLine("* " + module + ":");
                    foreach (string sourceFile in missing[module])
                        LogLine("    " + sourceFile);
                }
            }
        }

        /// <summary>
        /// Starts the build process
        /// </summary>
        public void StartBuild()
        {
            ExecutionTime.Start();
            Worker.RunWorkerAsync();
        }

        /// <summary>
        /// It appends text to the log
        /// </summary>
        /// <param name="text">Text to append</param>
        public void LogText(string text)
        {
            if (text == null)
                return;

            if (TxtLog.TextLength > MAXLOGLENGHT)
                return;

            if ((TxtLog.TextLength + text.Length) > MAXLOGLENGHT)
                text += Environment.NewLine + "MAXIMUM LOG LENGHT REACHED. NO MORE LOG WILL BE DISPLAYED";

            int startLineToCheck = ParsedLog.LinesCount - 1;

            // Do this before AppendText because it fires the SelectionChange event, and there
            // ParsedLog must to be updated
            ParsedLog.AppendLogText(text);
            TxtLog.AppendText(text);

            // Check errors:
            if (ParsedLog.ContainsErrors(startLineToCheck))
            {
                // There are compilation errors
                if (FistError)
                {
                    FistError = false;
                    GeneratorTab.WWMains.DisplayNotification("Build errors", Process.ToString() + ": " +
                        "There are build errors", ToolTipIcon.Error);
                    GeneratorTab.WWMains.UpdateNotifyIcon();
                }
                BtnOpenError.Visible = BtnNextError.Visible = true;
                Process.BuildWithErrors = true;
                HightlightErrors(startLineToCheck);
            }

            // Scroll to end
            TxtLog.SelectionStart = TxtLog.TextLength;
            TxtLog.ScrollToCaret();
        }

        /// <summary>
        /// It appends a text line to the log
        /// </summary>
        /// <param name="line">Line to append</param>
        public void LogLine(string line)
        {
            LogText(line + Environment.NewLine);
        }

        private void CancelBuild()
        {
            Worker.CancelAsync();
            Process.Cancel();

            BtnCancel.Enabled = false;
            BtnClose.Enabled = false;
        }

        /// <summary>
        /// Ask the user to cancel the build process
        /// </summary>
        /// <returns>True if the process can be cancelled</returns>
        private bool AskCancel()
        {
            if (!Worker.IsBusy)
                return true;

            if (MessageBox.Show("Are you sure you want to cancel the build process?", "Cancel",
                MessageBoxButtons.YesNo) == DialogResult.No)
                return false;

            CancelBuild();
            return true;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            AskCancel();
        }

        private void DoClose()
        {
            TabPage tabPage = this.Parent as TabPage;
            if (tabPage != null)
                GeneratorTab.WWMains.RemoveBuilTab(tabPage);
 
            this.Dispose();
            GeneratorTab.WWMains.CurrentMainsList.Focus();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            if (Worker.IsBusy)
            {
                if (AskCancel())
                    CloseWhenFinished = true;
            }
            else
                DoClose();
        }

        private void UpdateTabTitle()
        {
            TabPage tabPage = this.Parent as TabPage;
            if (tabPage == null)
                return;

            string status = Process.ToString();
            if (Process.BuildCancelled)
                status = "CANCELLED - " + status;
            else if( Process.BuildWithErrors )
                status = "ERROR - " + status;
            else
                status = "OK - " + status;
            tabPage.Text = status;
        }

        /// <summary>
        /// Get log text line
        /// </summary>
        /// <param name="lineIdx">Line index</param>
        /// <returns>Line text</returns>
        private string GetLineText(int lineIdx)
        {
            int lineStartIdx = TxtLog.GetFirstCharIndexFromLine(lineIdx);
            // Rich text box does NOT use \r\n, only \n
            int idxEnd = TxtLog.Text.IndexOf('\n', lineStartIdx);
            if (idxEnd < lineStartIdx)
                idxEnd = lineStartIdx;
            return TxtLog.Text.Substring(lineStartIdx, idxEnd - lineStartIdx);
        }

        private int CurrentLineIndex
        {
            get { return ParsedLog.GetLineIdxFromCharIdx(TxtLog.SelectionStart); }
        }

        /// <summary>
        /// Hightlight errors on log control
        /// </summary>
        private void HightlightErrors(int startLineIdx)
        {
            List<int> errorLinesIdx = ParsedLog.GetErrorLinesIdx(startLineIdx);
            foreach( int i in errorLinesIdx )
            {
                int lineStartIdx = ParsedLog.GetFirstCharIndexFromLine(i);
                TxtLog.Select(lineStartIdx, ParsedLog.GetLine(i).Length);
                TxtLog.SelectionBackColor = Color.Orange;
            }
        }

        /// <summary>
        /// Open source file error button clicked
        /// </summary>
        private void BtnOpenError_Click(object sender, EventArgs e)
        {
            try {
                CompilerError error = ParsedLog.GetLineError(CurrentLineIndex);
                if (error != null)
                    error.ShowErrorInVS(LsiExtensionsConfiguration.Load().VisualStudioComId, 
                        GeneratorTab.IsCSharpWebGenerator);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// Log cursor moved event handler
        /// </summary>
        private void TxtLog_SelectionChanged(object sender, EventArgs e)
        {
            BtnOpenError.Enabled = ParsedLog.LineContainsError(CurrentLineIndex);
        }

        /// <summary>
        /// Go to next log error line button clicked
        /// </summary>
        private void BtnNextError_Click(object sender, EventArgs e)
        {
            int currentLineIdx = ParsedLog.GetLineIdxFromCharIdx(TxtLog.SelectionStart);
            int nextErrIdx = ParsedLog.NextErrorLineIdx(currentLineIdx);
            if (nextErrIdx >= 0)
            {
                int lineStartIdx = ParsedLog.GetFirstCharIndexFromLine(nextErrIdx);
                TxtLog.Select(lineStartIdx, ParsedLog.GetLine(nextErrIdx).Length);
                TxtLog.ScrollToCaret();
                TxtLog.Focus();
            }
        }

        /// <summary>
        /// Repair RSPs and recompile button clicked
        /// </summary>
        private void BtnRepairAndRecompile_Click(object sender, EventArgs e)
        {
            try
            {

                Dictionary<string, HashSet<string>> missingSources = 
                    ParsedLog.GetSourcesNotFoundByModule();

                // Repair rsps
                RepairRspFiles repair = new RepairRspFiles(GeneratorTab.Generator.Model);
                repair.LogControl = this;
                repair.JustTest = false;
                repair.RepairSdts = true;
                repair.AddSourcesToModules(missingSources);

                // Recompile modules
                GeneratorTab.CompileModules(missingSources.Keys.ToList());

            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

    }
}
