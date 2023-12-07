using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using LSI.Packages.Extensiones.Utilidades;
using System.Diagnostics;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos.Build
{
    /// <summary>
    /// Tool to remove backup files from a target directory
    /// </summary>
    public partial class RemoveBackupFilesDlg : Form
    {

        /// <summary>
        /// Generator tab owner
        /// </summary>
        private GeneratorMains GeneratorTab;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="targetDirectory">Target model directory</param>
        public RemoveBackupFilesDlg(GeneratorMains generatorTab)
        {
            InitializeComponent();

            GeneratorTab = generatorTab;
            string targetDirectory = Entorno.GetTargetDirectory(generatorTab.Generator.Model);

            TxtBackupPatterns.Text = RemoveBackupFiles.BackupPatterns;
            TxtTargetDirectory.Text = targetDirectory;

            // Get files to remove
            foreach (string filePath in RemoveBackupFiles.GetBackupFiles(targetDirectory))
                LstFilesToRemove.Items.Add(filePath);
        }

        /// <summary>
        /// Put files with a pattern from a directory on the UI list
        /// </summary>
        /// <param name="directory">Directory where to search</param>
        /// <param name="pattern">Pattern of files to search</param>
        private void SearchFilesToRemove(string directory, string pattern)
        {
            if (!Directory.Exists(directory))
                return;

            foreach (string file in Directory.GetFiles(directory, pattern))
                LstFilesToRemove.Items.Add(file);
        }

        /// <summary>
        /// Accept button clicked. It deletes the files
        /// </summary>
        private void BtnAccept_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove " + LstFilesToRemove.Items.Count + " files?",
                "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            RemoveBackupFiles build = new RemoveBackupFiles(LstFilesToRemove.Items.Cast<string>().ToList());
            GeneratorTab.WWMains.StartBuild(GeneratorTab, build);

            Dispose();
        }

        /// <summary>
        /// Target model directory link clicked
        /// </summary>
        private void LnkTargetDirectory_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(TxtTargetDirectory.Text);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        private void RemoveBackupFiles_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            OpenDocumentation.Open("wwmains.html#borrarBackups");
        }

    }
}
