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
    /// Form to confirm RSP repair
    /// </summary>
    public partial class RepairRspConfirm : Form
    {
        public RepairRspConfirm()
        {
            InitializeComponent();
        }

        public bool JustTest
        {
            get { return ChkJustTest.Checked; }
        }

        private void BtnAccept_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Help button clicked
        /// </summary>
        private void RepairRspConfirm_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            OpenDocumentation.Open("wwmains.html#repararRsp");
        }

    }
}
