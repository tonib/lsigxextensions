using LSI.Packages.Extensiones.Utilidades.CSharpWin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LSI.Packages.Extensiones.Comandos.Build
{
    /// <summary>
    /// Build all with single generator dialog
    /// </summary>
    public partial class BuildSingleGeneratorDlg : Form
    {

        /// <summary>
        /// The generator to build
        /// </summary>
        private GeneratorMains GeneratorMains;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generatorMains">The generator to build</param>
        public BuildSingleGeneratorDlg(GeneratorMains generatorMains)
        {
            InitializeComponent();

            this.GeneratorMains = generatorMains;

            txtGenerator.Text = GeneratorMains.Generator.Description;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            BuildSingleGenerator build = new BuildSingleGenerator(GeneratorMains.Generator, 
                chkSpecify.Checked);
            GeneratorMains.WWMains.StartBuild(GeneratorMains, build);
            Dispose();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void BuildSingleGeneratorDlg_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            OpenDocumentation.Open("wwmains.shtml");
        }
    }
}
