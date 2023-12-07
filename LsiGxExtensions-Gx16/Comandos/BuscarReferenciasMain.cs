using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LSI.Packages.Extensiones.Utilidades;
using Artech.Architecture.Common.Objects;
using Artech.FrameworkDE;
using System.Runtime.InteropServices;
using LSI.Packages.Extensiones.Utilidades.UI;
using LSI.Packages.Extensiones.Utilidades.CallsAnalisys;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos
{

    /// <summary>
    /// Toolwindow to search main objects that reference a set of objects
    /// </summary>
    [Guid("7AC2CD95-C5E0-45d1-B5FC-8A95559F5AE7")]
    public partial class BuscarReferenciasMain : SearchKbObjectsToolWindowBase
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public BuscarReferenciasMain()
        {
            InitializeComponent();

            // Initialize the search toolwindow
            this.Icon = Resources.Diagram_8283;
            InitializeSearchTW(BtnBuscar, PicActividad, LblEstado, Grid);

            LstObjetosBuscar_SelectedIndexChanged(null, null);
        }

        /// <summary>
        /// Pulsado boton seleccionar objeto
        /// </summary>
        private void BtnSelObjeto_Click(object sender, EventArgs e)
        {
            SelectObjectOptions opciones = new SelectObjectOptions();
            opciones.MultipleSelection = true;
            foreach (KBObject objeto in UIServices.SelectObjectDialog.SelectObjects(opciones))
            {
                AgregarObjeto(objeto);
            }
        }

        /// <summary>
        /// Añade un objeto a la lista de objetos a buscar
        /// </summary>
        private void AgregarObjeto(KBObject o)
        {
            if (!(o is ICallableInfo))
            {
                MessageBox.Show("Object " + o.QualifiedName + " - " + o.Description + " is not callable");
                return;
            }

            RefObjetoGX referencia = new RefObjetoGX(o);
            if( !LstObjetosBuscar.Items.Contains(referencia) )
                LstObjetosBuscar.Items.Add(referencia);
        }

        /// <summary>
        /// Agrega el objeto con el nombre indicado
        /// </summary>
        private void AgregarObjeto()
        {
            TxtObjeto.Text = TxtObjeto.Text.Trim();
            if (string.IsNullOrEmpty(TxtObjeto.Text))
            {
                MessageBox.Show("Please, specify the object name to add");
                TxtObjeto.Focus();
                return;
            }

            KBObject o = KBaseGX.GetCallableObject(TxtObjeto.Text);
            if (o == null)
                return;

            AgregarObjeto(o);

            TxtObjeto.Text = "";
            TxtObjeto.Focus();
        }

        /// <summary>
        /// Pulsado objeto sobre la tecla enter sobre el campo del nombre del objeto
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtObjeto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                AgregarObjeto();
            else if (e.KeyCode == Keys.F4)
                BtnSelObjeto_Click(null, null);
        }

        /// <summary>
        /// Pulsado el boton de agregar objeto a la busqueda
        /// </summary>
        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            AgregarObjeto();
        }

        /// <summary>
        /// Pulsado el boton de quitar objetos de la busqueda
        /// </summary>
        private void BtnQuitar_Click(object sender, EventArgs e)
        {
            List<RefObjetoGX> objetos = new List<RefObjetoGX>();
            foreach (RefObjetoGX o in LstObjetosBuscar.SelectedItems)
                objetos.Add(o);
            foreach (RefObjetoGX o in objetos)
                LstObjetosBuscar.Items.Remove(o);
        }

        /// <summary>
        /// Creates the object to run the search
        /// </summary>
        /// <returns>The finder object. null if there was problems and the run cannot be started</returns>
        override protected UISearchBase CreateFinder()
        {
            // Hacer validaciones
            if (LstObjetosBuscar.Items.Count == 0)
            {
                MessageBox.Show("You must specify some object to search");
                return null;
            }

            // Cargar los objetos a partir de las referencias
            List<KBObject> objetos = new List<KBObject>();
            foreach (RefObjetoGX r in LstObjetosBuscar.Items)
                objetos.Add(r.ObjetoGX);

            MainReferencesFinder finder = new MainReferencesFinder(objetos);
            finder.SearchRecursively = ChkSearchRecursively.Checked;
            return finder;
        }

        /// <summary>
        /// Pulsado el boton de buscar
        /// </summary>
        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            base.SearchButtonPressed();
        }

        private void HabilitarTodosCamposUI(bool habilitar)
        {
            TxtObjeto.Enabled = LstObjetosBuscar.Enabled = BtnAgregar.Enabled =
                BtnQuitar.Enabled = BtnPegar.Enabled = LstObjetosBuscar.Enabled = 
                BtnQuitarTodos.Enabled = habilitar;
            LstObjetosBuscar_SelectedIndexChanged(null, null);
        }

        /// <summary>
        /// Pulsado el boton de pegar
        /// </summary>
        private void BtnPegar_Click(object sender, EventArgs e)
        {
            foreach (KBObject objeto in UIServices.Clipboard.GetObjects())
                AgregarObjeto(objeto);
        }

        /// <summary>
        /// Cambiada la seleccion de la lista de objetos
        /// </summary>
        private void LstObjetosBuscar_SelectedIndexChanged(object sender, EventArgs e)
        {
            BtnQuitar.Enabled = (LstObjetosBuscar.SelectedItems.Count > 0);
        }

        /// <summary>
        /// Pulsado el boton de la ayuda
        /// </summary>
        private void BtnAyuda_Click(object sender, EventArgs e)
        {
            OpenDocumentation.Open("busquedamains.html");
        }

        /// <summary>
        /// Pulsado el boton de quitar todos
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnQuitarTodos_Click(object sender, EventArgs e)
        {
            LstObjetosBuscar.Items.Clear();
        }

        /// <summary>
        /// Doble click sobre un objeto de la lista
        /// </summary>
        private void LstObjetosBuscar_DoubleClick(object sender, EventArgs e)
        {
            foreach (RefObjetoGX r in LstObjetosBuscar.SelectedItems)
                r.AbrirReferencia();
        }

    }
}
