using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LSI.Packages.Extensiones.Utilidades.UI;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;
using System.Runtime.InteropServices;

namespace LSI.Packages.Extensiones.Comandos
{

    /// <summary>
    /// Tool window to search objects modified by an user
    /// </summary>
    [Guid("C716CFC3-42F3-49D5-B803-07B05D6FD6B8")]
    public partial class ObjectsModifiedByUser : SearchKbObjectsToolWindowBase
    {
        public ObjectsModifiedByUser()
        {
            InitializeComponent();

            this.Icon = Resources.WindowsGroups_7309;

            // Initialize search toolwindow
            base.InitializeSearchTWCustomType<ObjectsModifiedByUserFinder.RefObjetGxWithUsers>(BtnSearch, PicActivity, LblState, Grid);

            // Add column with all modifier users
            // Last user column
            DataGridViewTextBoxColumn colAllUsers = new DataGridViewTextBoxColumn();
            colAllUsers.DataPropertyName = "ModifierUsers";
            colAllUsers.HeaderText = "Modifier users";
            colAllUsers.Width = 300;
            Grid.Columns.Add(colAllUsers);
        }

        /// <summary>
        /// Creates the object to run the search
        /// </summary>
        /// <returns>The finder object. Object if there was problems and the run cannot be started</returns>
        override protected UISearchBase CreateFinder()
        {

            if (string.IsNullOrEmpty(TxtDate.Text))
            {
                MessageBox.Show("Please, specify the date");
                TxtDate.Focus();
                return null;
            }

            DateTime date;
            try { date = DateTime.Parse(TxtDate.Text); }
            catch
            {
                MessageBox.Show("Wrong date format");
                return null;
            }

            return new ObjectsModifiedByUserFinder(date, TxtUser.Text.Trim());
        }

        protected override void EnableUIFields(bool enabled)
        {
            TxtUser.Enabled = enabled;
            TxtDate.Enabled = enabled;
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            SearchButtonPressed();
        }

        private void BtnAyuda_Click(object sender, EventArgs e)
        {
            OpenDocumentation.Open("modificadosporusuario.html");
        }
    }

}
