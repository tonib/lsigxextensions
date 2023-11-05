using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace LSI.Packages.Extensiones.Utilidades.Threading
{

    /// <summary>
    /// Form to select a process from a list
    /// </summary>
    public partial class ProcessChooser : Form
    {
        /// <summary>
        /// Items to display on the form list
        /// </summary>
        private class ProcessItem
        {
            public Process Process;

            private string DisplayName;

            public override string ToString()
            {
                return DisplayName;
            }

            public ProcessItem(Process process)
            {
                Process = process;
                DisplayName = ProcessUtils.GetWindowTitle(process);
            }
        }

        protected ProcessChooser(List<Process> processes)
        {
            InitializeComponent();

            processes.ForEach(x => LstProcesses.Items.Add(new ProcessItem(x)));
            LstProcesses.SelectedIndex = 0;
        }

        static public Process ChooseProcess(List<Process> processes)
        {
            if (processes.Count == 0)
                return null;
            if (processes.Count == 1)
                return processes[0];

            ProcessChooser chooser = new ProcessChooser(processes);
            if (chooser.ShowDialog() == DialogResult.Cancel)
                return null;

            return (chooser.LstProcesses.SelectedItem as ProcessItem).Process;
        }

        private void BtnAccept_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Dispose();
        }

        private void LstProcesses_DoubleClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Dispose();
        }

        private void ProcessChooser_Load(object sender, EventArgs e)
        {
            LstProcesses.Focus();
        }
    }
}
