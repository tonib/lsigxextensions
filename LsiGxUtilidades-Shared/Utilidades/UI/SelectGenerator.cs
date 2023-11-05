using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Artech.Architecture.Common.Converters;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Commands;
using Artech.Genexus.Common.Entities;
using Artech.Genexus.Common.Services;
using Artech.Udm.Framework;
using Artech.Udm.Framework.Multiuser;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.CallsAnalisys;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.Logging;
using Artech.Architecture.Common.Services;

namespace LSI.Packages.Extensiones.Utilidades.UI
{
    /// <summary>
    /// Dialog to select an environment
    /// </summary>
    public partial class SelectGenerator : Form
    {

        /// <summary>
        /// The selected environments. empty if no enviroment was selected
        /// </summary>
        public List<GxEnvironment> SelectedEnvironments = new List<GxEnvironment>();

        /// <summary>
        /// The model where to get the generators
        /// </summary>
        private GxModel GxModel;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">Target model to get the generators. If it's null, the
        /// current kb target environment will be used</param>
        /// <param name="showSmartDevices">Show smart devices generators?</param>
        public SelectGenerator(KBModel model, bool showSmartDevices, bool selectMultiple)
        {
            InitializeComponent();

            if (selectMultiple)
                LstGenerators.SelectionMode = SelectionMode.MultiExtended;

            if (model == null)
                model = UIServices.KB.CurrentKB.DesignModel.Environment.TargetModel;
            GxModel = model.GetAs<GxModel>();

            // Filter smart devices?
            IEnumerable<GxEnvironment> generators = GxModel.Environments;
            if(!showSmartDevices)
                generators = generators.Where(x => x.Generator != (int)GeneratorType.SmartDevices);           

            foreach (GxEnvironment generator in generators)
                LstGenerators.Items.Add(generator.Description);
            if (LstGenerators.Items.Count > 0)
                LstGenerators.SelectedIndex = 0;
        }

        private void LstGenerators_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            BtnAccept_Click(sender, e);
        }

        private void BtnAccept_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            foreach (string generatorDescription in LstGenerators.SelectedItems)
            {
                GxEnvironment generator = MainsGx.GetGeneratorByDescription(GxModel, generatorDescription);
                if (generator != null)
                    SelectedEnvironments.Add(generator);
            }
            Dispose();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }

    }
}
