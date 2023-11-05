using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Artech.Architecture.UI.Framework.Services;

namespace LSI.Packages.Extensiones.Comandos.Build.Production
{
    /// <summary>
    /// Production task edit UI
    /// </summary>
    public partial class ProductionTaskDlg : Form
    {

        const string ALL = "All";

        /// <summary>
        /// Compress production task
        /// </summary>
        private const string TYPE_ZIP = "Compress";

        /// <summary>
        /// Copy production task
        /// </summary>
        private const string TYPE_COPY = "Copy";

        /// <summary>
        /// Execute powershell script task
        /// </summary>
        private const string TYPE_SCRIPT = "Powershell script";

        /// <summary>
        /// Constructor to create a new task
        /// </summary>
        public ProductionTaskDlg() : this(new ZipProduction())
        {
        }

        /// <summary>
        /// Constructor to edit a task
        /// </summary>
        public ProductionTaskDlg(ProductionTask task)
        {
            InitializeComponent();

            CmbEnvironmentType.Items.Add(EnvironmentType.WIN);
            CmbEnvironmentType.Items.Add(EnvironmentType.WEB);

            List<string> envNames = new List<string> { ALL };
            envNames.AddRange(UIServices.KB.WorkingEnvironment.Models.ToList().Select(env => env.Name).ToArray());
            CmbEnvironment.Items.AddRange(envNames.ToArray());

            CmbTaskType.Items.Add(TYPE_ZIP);
            CmbTaskType.Items.Add(TYPE_COPY);
            CmbTaskType.Items.Add(TYPE_SCRIPT);

            FillUI(task);
            UpdateUiState();
        }

        /// <summary>
        /// Fill UI fields with task values
        /// </summary>
        /// <param name="task">Task to get value</param>
        public void FillUI(ProductionTask task)
        {
            ChkEnabled.Checked = task.Enabled;

            EnvironmentTask envTask = task as EnvironmentTask;
            if( envTask != null )
                CmbEnvironmentType.SelectedItem = envTask.Environment;

            CmbEnvironment.SelectedItem = string.IsNullOrEmpty(task.OnlyForEnvironmentName) ? ALL : task.OnlyForEnvironmentName;

            ZipProduction zipTask = task as ZipProduction;
            if (zipTask != null)
            {
                CmbTaskType.SelectedItem = TYPE_ZIP;
                TxtMainFile.Text = zipTask.ZipPath;
                return;
            }

            CopyProduction copyTask = task as CopyProduction;
            if (copyTask != null)
            {
                CmbTaskType.SelectedItem = TYPE_COPY;
                TxtMainFile.Text = copyTask.CopyDestination;
                ChkKeepBackup.Checked = !copyTask.DeleteBackup;
                ChkKeepConfig.Checked = copyTask.KeepConfigFile;
                ChkStopIis.Checked = copyTask.StopIIS;
                return;
            }

            ExecuteScript scriptTask = task as ExecuteScript;
            if (scriptTask != null)
            {
                CmbTaskType.SelectedItem = TYPE_SCRIPT;
                TxtMainFile.Text = scriptTask.ScriptPath;
                return;
            }
        }

        private string SelectedTaskType
        {
            get { return CmbTaskType.SelectedItem as string; }
        }

        private EnvironmentType SelectedEnvironment
        {
            get { return (EnvironmentType)CmbEnvironmentType.SelectedItem; }
        }

        private void UpdateUiState()
        {
            if (SelectedTaskType == TYPE_ZIP)
                LnkMainFile.Text = "Zip path";
            else if (SelectedTaskType == TYPE_COPY)
            {
                if (SelectedEnvironment == EnvironmentType.WEB)
                    LnkMainFile.Text = "Dest. directory (web folder)";
                else
                    LnkMainFile.Text = "Dest. directory (bin folder)";
                ChkStopIis.Enabled = (SelectedEnvironment == EnvironmentType.WEB);
            }
            else if (SelectedTaskType == TYPE_SCRIPT)
                LnkMainFile.Text = "Script path";

            LblEnvironment.Visible = CmbEnvironmentType.Visible = 
                (SelectedTaskType != TYPE_SCRIPT);
            ChkKeepBackup.Visible = ChkKeepConfig.Visible = ChkStopIis.Visible = 
                (SelectedTaskType == TYPE_COPY);
        }

        /// <summary>
        /// Task type changed
        /// </summary>
        private void CmbTaskType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUiState();
        }

        /// <summary>
        /// Get the current task on the UI
        /// </summary>
        public ProductionTask Task
        {
            get
            {
                ProductionTask task = null;
                TxtMainFile.Text = TxtMainFile.Text.Trim();

                if (SelectedTaskType == TYPE_ZIP)
                {
                    task = new ZipProduction() {
                        ZipPath = TxtMainFile.Text
                    };
                }
                else if (SelectedTaskType == TYPE_COPY)
                {
                    task = new CopyProduction() {
                        CopyDestination = TxtMainFile.Text,
                        KeepConfigFile = ChkKeepConfig.Checked,
                        StopIIS = ChkStopIis.Checked,
                        DeleteBackup = !ChkKeepBackup.Checked
                    };
                }
                else if (SelectedTaskType == TYPE_SCRIPT)
                {
                    task = new ExecuteScript() {
                        ScriptPath = TxtMainFile.Text
                    };
                }

                string selEnvironmentName = (string)CmbEnvironment.SelectedItem;
                task.OnlyForEnvironmentName = selEnvironmentName == ALL ? string.Empty : selEnvironmentName;

                task.Enabled = ChkEnabled.Checked;
                EnvironmentTask envTask = task as EnvironmentTask;
                if( envTask != null )
                    envTask.Environment = SelectedEnvironment;
                return task;
            }
        }

        private void BtnAccept_Click(object sender, EventArgs e)
        {
            TxtMainFile.Text = TxtMainFile.Text.Trim();
            if (string.IsNullOrEmpty(TxtMainFile.Text))
            {
                MessageBox.Show("Target path is mandatory");
                TxtMainFile.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnSelectMain_Click(object sender, EventArgs e)
        {
            if (SelectedTaskType == TYPE_ZIP)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                try
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(TxtMainFile.Text);
                    dlg.FileName = Path.GetFileName(TxtMainFile.Text);
                }
                catch { }
                LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.Load();
                string filter = string.Format("Compressed files (*.{0})|*.{0}|All files (*.*)|*.*",
                    cfg.Compressor.CompressorFilesExtension);
                dlg.Filter = filter;

                if (dlg.ShowDialog() == DialogResult.OK)
                    TxtMainFile.Text = dlg.FileName;
            }
            else if (SelectedTaskType == TYPE_SCRIPT)
            {
                OpenFileDialog dlg = new OpenFileDialog();
                try
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(TxtMainFile.Text);
                    dlg.FileName = Path.GetFileName(TxtMainFile.Text);
                }
                catch { }
                dlg.Filter = "Powershell scripts (*.ps1)|*.ps1|All files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                    TxtMainFile.Text = dlg.FileName;
            }
            else if (SelectedTaskType == TYPE_COPY)
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                try
                {
                    dlg.SelectedPath = TxtMainFile.Text;
                }
                catch { }
                if (dlg.ShowDialog() == DialogResult.OK)
                    TxtMainFile.Text = dlg.SelectedPath;
            }
        }

        /// <summary>
        /// Main file link clicked
        /// </summary>
        private void LnkMainFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                ProcessStartInfo i = new ProcessStartInfo(TxtMainFile.Text);
                if( Path.GetExtension(TxtMainFile.Text).ToLower() == ".ps1" )
                    i.Verb = "Edit";
                Process.Start(i);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Environment type changed
        /// </summary>
        private void CmbEnvironmentType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUiState();
        }

        /// <summary>
        /// Help button clicked
        /// </summary>
        private void ProductionTaskDlg_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            OpenDocumentation.Open("wwmains.shtml#produccion");
        }

    }
}
