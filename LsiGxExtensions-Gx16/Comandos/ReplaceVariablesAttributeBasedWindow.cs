using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.Variables;
using Artech.Genexus.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using System.Threading;
using Artech.Genexus.Common;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.UI;

namespace LSI.Packages.Extensiones.Comandos
{
    /// <summary>
    /// Tool to replace variables "attribute based on" by other attribute
    /// </summary>
    public partial class ReplaceVariablesAttributeBasedWindow : Form, IExecutable
    {

        /// <summary>
        /// Constructor
        /// <param name="oldAttribute">Attribute to propose as old. It can be null</param>
        /// </summary>
        public ReplaceVariablesAttributeBasedWindow(Artech.Genexus.Common.Objects.Attribute oldAttribute)
        {
            InitializeComponent();
            if (oldAttribute != null)
            {
                TxtOldAttribute.Text = oldAttribute.Name;
                SearchAttribute(TxtOldAttribute, LblTypeOld, LblPictureOld);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ReplaceVariablesAttributeBasedWindow() : this(null)
        {
        }

        /// <summary>
        /// Select attribute clicked
        /// </summary>
        private void BtnSelect_Click(object sender, EventArgs e)
        {
            Artech.Genexus.Common.Objects.Attribute a = SelectKbObject.SelectAttribute();
            if (a == null)
                return;

            if (sender == BtnSelectNew)
            {
                TxtNewAttribute.Text = a.Name;
                TxtNewAttribute_Leave(null, null);
            }
            else
            {
                TxtOldAttribute.Text = a.Name;
                TxtOldAttribute_Leave(null, null);
            }
        }

        /// <summary>
        /// Accept button pressed
        /// </summary>
        private void BtnAccept_Click(object sender, EventArgs e)
        {
            try
            {

                // Get old attribute
                Artech.Genexus.Common.Objects.Attribute oldAttribute =
                    SearchAttribute(TxtOldAttribute, LblTypeOld, LblPictureOld);
                if (oldAttribute == null)
                {
                    MessageBox.Show("Attribute " + TxtOldAttribute.Text + " does not exist");
                    TxtOldAttribute.Focus();
                    return;
                }

                // Get new attribute
                Artech.Genexus.Common.Objects.Attribute newAttribute = 
                    SearchAttribute(TxtNewAttribute, LblTypeNew, LblPictureNew);
                if (newAttribute == null && !string.IsNullOrEmpty(TxtNewAttribute.Text))
                {
                        MessageBox.Show("Attribute " + TxtNewAttribute.Text + " does not exist");
                        TxtNewAttribute.Focus();
                        return;
                }

                if (newAttribute != null)
                {
                    // Check old and new attribute have the same type:
                    if (oldAttribute.Type != newAttribute.Type ||
                        oldAttribute.Length != newAttribute.Length ||
                        oldAttribute.Decimals != newAttribute.Decimals)
                    {
                        if (MessageBox.Show("WARNING: Selected attributes have different types. " +
                            "Are you sure you want to continue?", "Confirm", MessageBoxButtons.YesNo)
                            == DialogResult.No)
                            return;
                    }

                    // Check old and new attribute have the same picture:
                    if (LblPictureNew.Text != LblPictureOld.Text)
                    {
                        if (MessageBox.Show("WARNING: Selected attributes have different pictures. " +
                            "Are you sure you want to continue?", "Confirm", MessageBoxButtons.YesNo)
                            == DialogResult.No)
                            return;
                    }
                }

                // Confirm
                string message = string.Empty;
                if (ChkJustTest.Checked)
                    message += "[TEST ONLY]: ";
                message += "This process cannot be cancelled and it can take some time. " +
                     "Are you sure you want to ";
                if (newAttribute == null)
                    message += "remove";
                else
                    message += "replace";
                message += " 'Attribute based on' variables property with '" + oldAttribute.Name + "' attribute";
                if (newAttribute != null)
                    message += " by '" + newAttribute.Name + "'";
                message += "?";

                if (MessageBox.Show(message, "Attribute based on replacement", MessageBoxButtons.OKCancel)
                    == DialogResult.Cancel)
                    return;

                // Run
                ReplaceVariablesAttributeBased replacement = new ReplaceVariablesAttributeBased(
                    oldAttribute, newAttribute);
                replacement.JustTest = ChkJustTest.Checked;

                Thread t = new Thread(new ThreadStart(replacement.Execute));
                t.Start();
                Dispose();
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        private Artech.Genexus.Common.Objects.Attribute SearchAttribute(TextBox attributeName, 
            Label typeLabel, Label pictureLabel)
        {
            try
            {
                attributeName.Text = attributeName.Text.Trim();
                Artech.Genexus.Common.Objects.Attribute a =
                        Artech.Genexus.Common.Objects.Attribute.Get(UIServices.KB.CurrentModel,
                        attributeName.Text);
                if (a == null)
                {
                    typeLabel.Text = "(Not found)";
                    pictureLabel.Text = "(Not found)";
                }
                else {
                    typeLabel.Text = AtributoGx.TypeDescription(UIServices.KB.CurrentModel, a);
                    pictureLabel.Text = a.GetPropertyValue(Properties.ATT.Picture) as string;
                }
                return a;
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                return null;
            }
        }

        /// <summary>
        /// Shows the dialog
        /// </summary>
        public void Execute()
        {
            this.ShowDialog();
        }

        /// <summary>
        /// Old attribute field lost focus
        /// </summary>
        private void TxtOldAttribute_Leave(object sender, EventArgs e)
        {
            SearchAttribute(TxtOldAttribute, LblTypeOld, LblPictureOld);
        }

        /// <summary>
        /// New attribute field lost focus
        /// </summary>
        private void TxtNewAttribute_Leave(object sender, EventArgs e)
        {
            SearchAttribute(TxtNewAttribute, LblTypeNew, LblPictureNew);
        }

        /// <summary>
        /// Help button clicked
        /// </summary>
        private void ReplaceVariablesAttributeBasedWindow_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            OpenDocumentation.Open("atrbasedon.html");
        }


    }
}
