using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Comandos.Build;
using Artech.Architecture.UI.Framework.Services;
using LSI.Packages.Extensiones.Utilidades.CSharpWin;
using LSI.Packages.Extensiones.Utilidades.UI;
using LSI.Packages.Extensiones.Utilidades.Logging;
using Artech.Architecture.Common.Objects;

namespace LSI.Packages.Extensiones.Comandos.Build.Production
{
    /// <summary>
    /// Edit production tasks
    /// </summary>
    public partial class PrepareProductionDlg : Form
    {

        /// <summary>
        /// Tab that has called this dialog
        /// </summary>
        private GeneratorMains GeneratorTab;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generatorTab">Tab that has called this dialog</param>
        public PrepareProductionDlg(GeneratorMains generatorTab)
        {
            InitializeComponent();

            this.GeneratorTab = generatorTab;

            PrepareProduction production = PrepareProduction.LoadKbProduction(UIServices.KB.CurrentKB.DesignModel.Environment.TargetModel);
            TxtTargetDir.Text = production.TargetDirectory;
            TxtIgnorePatterns.Text = production.PatternsToIgnoreText;
            ChkCopyImagesTxt.Checked = production.CopyImagesTxt;

            production.Tasks.ForEach(x => LstTasks.Items.Add(x));
        }

        /// <summary>
        /// Edit the selected task
        /// </summary>
        private void EditTask()
        {
            int idx = LstTasks.SelectedIndex;
            if (idx < 0)
                return;
            ProductionTask task = LstTasks.SelectedItem as ProductionTask;

            ProductionTaskDlg dlg = new ProductionTaskDlg(task);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LstTasks.Items.Remove(task);
                LstTasks.Items.Insert(idx, dlg.Task);
                LstTasks.SelectedIndex = idx;
            }
            dlg.Dispose();
        }

        private PrepareProduction CurrentProduction
        {
            get
            {
                PrepareProduction production = new PrepareProduction(UIServices.KB.WorkingEnvironment.TargetModel);
                production.PatternsToIgnoreText = TxtIgnorePatterns.Text;
                production.Tasks = LstTasks.Items.Cast<ProductionTask>().ToList();
                production.CopyImagesTxt = ChkCopyImagesTxt.Checked;
                return production;
            }
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            ProductionTaskDlg dlg = new ProductionTaskDlg();
            if (dlg.ShowDialog() == DialogResult.OK)
                LstTasks.Items.Add(dlg.Task);
            dlg.Dispose();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            EditTask();
        }

        private void BtnRemoveTask_Click(object sender, EventArgs e)
        {
            List<ProductionTask> toRemove = LstTasks.SelectedItems.Cast<ProductionTask>().ToList();
            if( toRemove.Count == 0 )
                return;

            if (MessageBox.Show("Are you sure you want to remove the selected tasks?", "Confirm", MessageBoxButtons.OKCancel)
                == DialogResult.Cancel)
                return;

            toRemove.ForEach(x => LstTasks.Items.Remove(x));
        }

        private void LstTasks_DoubleClick(object sender, EventArgs e)
        {
            EditTask();
        }

        private void DoClose()
        {
            try
            {
                CurrentProduction.SaveToFile(UIServices.KB.CurrentKB);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
            Close();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            DoClose();
        }

        /// <summary>
        /// Run a set of tasks of the production
        /// </summary>
        /// <param name="taksToRun">Tasks to run. null to run all tasks</param>
        private void RunProduction(IEnumerable<ProductionTask> tasksToRun)
        {
            try
            {
                // Check if we are on prototype:
                if (CSharpUtils.GeneratorHasDebugOption(GeneratorTab.Generator))
                {
                    if (MessageBox.Show("Modules are compiled with /debug option. Are you sure you " +
                        "want to prepare production?", "Confirm", MessageBoxButtons.OKCancel) ==
                        DialogResult.Cancel)
                        return;
                }

                // Get the production to run, with the selected tasks (or all)
                PrepareProduction production = CurrentProduction;
                production.TasksToRun = tasksToRun;

                // Warn about existing files/directories that will be replaced
                string message = string.Empty;
                List<string> toReplace = production.FilesToReplaceByTasks();
                if (toReplace.Count > 0)
                {
                    message += "Following files/directories will be replaced by " +
                        "this process:" + Environment.NewLine + Environment.NewLine;
                    message += string.Join(Environment.NewLine, toReplace.ToArray());
                    message += Environment.NewLine + Environment.NewLine;
                }
                message += "Are you sure you want to prepare the production?";
                if (MessageBox.Show(message, "Confirm",
                    MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return;

                GeneratorTab.WWMains.StartBuild(GeneratorTab, production);
                DoClose();
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// Production button clicked
        /// </summary>
        private void BtnPrepareProduction_Click(object sender, EventArgs e)
        {
            RunProduction(null);
        }

        private void BtnMoveUp_Click(object sender, EventArgs e)
        {
            ListBoxUtils.MoveSelectedItem(LstTasks, -1);
        }

        private void BtnMoveDown_Click(object sender, EventArgs e)
        {
            ListBoxUtils.MoveSelectedItem(LstTasks, +1);
        }

        /// <summary>
        /// Help dialog button clicked
        /// </summary>
        private void PrepareProductionDlg_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            OpenDocumentation.Open("wwmains.shtml#produccion");
        }

        /// <summary>
        /// Run selected tasks button clicked
        /// </summary>
        private void BtnRunSelectedTasks_Click(object sender, EventArgs e)
        {
            List<ProductionTask> tasks = LstTasks.SelectedItems
                .Cast<ProductionTask>()
                .ToList();
            if (tasks.Count == 0)
                return;
            RunProduction(tasks);
        }

    }
}
