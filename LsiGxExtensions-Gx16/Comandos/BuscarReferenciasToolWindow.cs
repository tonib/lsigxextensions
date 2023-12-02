using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Entities;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using LSI.Packages.Extensiones.Utilidades.CallsAnalisys;
using LSI.Packages.Extensiones.Utilidades.UI;
using System.Linq;

namespace LSI.Packages.Extensiones.Comandos
{
    // TODO: The base class of this should be ToolWindowBase, and not SearchToolWindowBase
    /// <summary>
    /// Herramienta para buscar referencias detalladas a atributos, tablas y sdts.
    /// Permite buscar inserciones y actualizaciones de tablas y 
    /// escrituras y lecturas de atributos de SDTSs
    /// </summary>
    [Guid("97740DBC-AC46-411f-967E-7F41FBCE1123")]
    public partial class BuscarReferenciasToolWindow : SearchKbObjectsToolWindowBase
    {

        // Valores del campo de tipo de objeto a buscar
        private const string TIPO_ATRIBUTO = "Attribute";
        private const string TIPO_TABLA = "Table";
        private const string TIPO_SDT = "SDT";
        private const string TIPO_BC = "BC";
        private const string TIPO_ENUM = "Enumerated domain";
        private const string TIPO_VARIABLE = "Variables (by type)";
        private const string TYPE_GENERATOR = "Objects by generator";

        // Valores del campo de tipo de campo de sdt / bc a buscar
        private const string CAMPO_CAMPO = "Field";
        private const string CAMPO_FUNCION = "Function";

        /// <summary>
        /// Constructor
        /// </summary>
        public BuscarReferenciasToolWindow()
        {
            InitializeComponent();

            // Poner el icono de LSI
            this.Icon = Resources.Find_5650;

            // Initialize search toolwindow
            base.InitializeSearchTW(BtnBuscar, PicActividad, LblEstado, Grid);

            // Tipos de objetos:
            CmbTipo.Items.Clear();
            CmbTipo.Items.Add(TIPO_ATRIBUTO);
            CmbTipo.Items.Add(TIPO_TABLA);
            CmbTipo.Items.Add(TIPO_SDT);
            CmbTipo.Items.Add(TIPO_BC);
            CmbTipo.Items.Add(TIPO_VARIABLE);
            CmbTipo.Items.Add(TIPO_ENUM);
            CmbTipo.Items.Add(TYPE_GENERATOR);
            TipoObjetoSeleccionado = TIPO_ATRIBUTO;
            CmbTipo_SelectedValueChanged(null, null);

            // Tipos de campos:
            CmbTipoCampo.Items.Clear();
            CmbTipoCampo.Items.Add(CAMPO_CAMPO);
            CmbTipoCampo.Items.Add(CAMPO_FUNCION);
            TipoCampoSeleccionado = CAMPO_CAMPO;

            ChkLecturas_CheckedChanged(null, null);
        }

        /// <summary>
        /// El tipo de campo seleccionado
        /// </summary>
        private string TipoCampoSeleccionado
        {
            get { return (string)CmbTipoCampo.SelectedItem; }
            set { CmbTipoCampo.SelectedItem = value; }
        }

        /// <summary>
        /// El tipo de objeto seleccionado
        /// </summary>
        private string TipoObjetoSeleccionado
        {
            get { return (string)CmbTipo.SelectedItem; }
            set { CmbTipo.SelectedItem = value; }
        }

        /// <summary>
        /// La lista de referencias actualmente seleccionadas
        /// </summary>
        private List<TipoOperacion> TiposReferenciasSeleccionadas
        {
            get
            {
                List<TipoOperacion> tipoReferencias = new List<TipoOperacion>();
                if (ChkEscrituras.Visible && ChkEscrituras.Checked)
                    tipoReferencias.Add(TipoOperacion.ESCRITURA);
                if (ChkLecturas.Visible && ChkLecturas.Checked)
                    tipoReferencias.Add(TipoOperacion.LECTURA);
                if (ChkInserciones.Visible && ChkInserciones.Checked)
                    tipoReferencias.Add(TipoOperacion.INSERCION);
                if (ChkBorrado.Visible && ChkBorrado.Checked)
                    tipoReferencias.Add(TipoOperacion.BORRADO);
                return tipoReferencias;
            }
        }

        /// <summary>
        /// El objeto actualmente seleccionado
        /// </summary>
        private KBObject ObjetoSeleccionado
        {
            get { 
                string nombre = TxtObjeto.Text.Trim();
                KBModel modelo = UIServices.KB.CurrentModel;
                if (TipoObjetoSeleccionado == TIPO_ATRIBUTO)
                    return Artech.Genexus.Common.Objects.Attribute.Get(modelo, nombre);
                else if (TipoObjetoSeleccionado == TIPO_TABLA)
                    return Table.Get(modelo, nombre);
                else if (TipoObjetoSeleccionado == TIPO_BC)
                    return Transaction.Get(modelo, new QualifiedName(nombre));
                else if (TipoObjetoSeleccionado == TIPO_SDT)
                    return SDT.Get(modelo, new QualifiedName(nombre));
                else if (TipoObjetoSeleccionado == TIPO_ENUM)
#if GX_15_OR_GREATER
                    return Domain.Get(modelo, new QualifiedName(nombre));
#else
                    return Domain.Get(modelo, nombre);
#endif
                else
                    return null;
            }
        }

        /// <summary>
        /// The currently selected generator. null if no generator has been selected
        /// </summary>
#if GX_17_OR_GREATER
        private List<GxGenerator> SelectedGenerators
#else
        private List<GxEnvironment> SelectedGenerators
#endif
        {
            get
            {
                IEnumerable<string> generatorsNames = TxtObjeto.Text.Split(new char[] { ';' }, 
                    StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim());
#if GX_17_OR_GREATER
                List<GxGenerator> generators = new List<GxGenerator>();
#else
                List<GxEnvironment> generators = new List<GxEnvironment>();
#endif

                GxModel gxModel = UIServices.KB.CurrentKB.DesignModel.Environment.TargetModel.GetAs<GxModel>();
                foreach(string generatorName in generatorsNames)
                {
                    if (string.IsNullOrEmpty(generatorName))
                        continue;
                    var generator = MainsGx.GetGeneratorByDescription(gxModel, generatorName);
                    if (generator == null)
                        throw new Exception("Generator with description " + generatorName + " not found");
                    generators.Add(generator);
                }
                return generators;
            }
        }

        /// <summary>
        /// Ejecuta la seleccion del objeto
        /// </summary>
        private void SeleccionarObjeto()
        {

            if (TipoObjetoSeleccionado == TYPE_GENERATOR)
            {
                SelectGenerator genSelection = new SelectGenerator(null, false, true);
                if (genSelection.ShowDialog() == DialogResult.OK)
                    TxtObjeto.Text = string.Join("; ", 
                        genSelection.SelectedEnvironments.Select(x => x.Description).ToArray());
                return;
            }

            SelectObjectOptions opciones = new SelectObjectOptions();
            if (TipoObjetoSeleccionado == TIPO_ATRIBUTO)
                opciones.ObjectTypes.Add(KBObjectDescriptor.Get<Artech.Genexus.Common.Objects.Attribute>());
            else if (TipoObjetoSeleccionado == TIPO_SDT)
                opciones.ObjectTypes.Add(KBObjectDescriptor.Get<SDT>());
            else if (TipoObjetoSeleccionado == TIPO_TABLA)
                opciones.ObjectTypes.Add(KBObjectDescriptor.Get<Table>());
            else if (TipoObjetoSeleccionado == TIPO_BC)
                opciones.ObjectTypes.Add(KBObjectDescriptor.Get<Transaction>());
            else if (TipoObjetoSeleccionado == TIPO_ENUM)
                opciones.ObjectTypes.Add(KBObjectDescriptor.Get<Domain>());
            else
                return;

            KBObject o = UIServices.SelectObjectDialog.SelectObject(opciones);
            if (o != null)
            {
                TxtObjeto.Text = o.QualifiedName.ToString();
                if (TipoObjetoSeleccionado == TIPO_BC || TipoObjetoSeleccionado == TIPO_SDT ||
                    TipoObjetoSeleccionado == TIPO_ENUM)
                    CmbTipoCampo.Focus();
                else
                    ChkEscrituras.Focus();
            }
        }

        /// <summary>
        /// Pulsado el boton de seleccionar objeto
        /// </summary>
        private void BtnSeleccionar_Click(object sender, EventArgs e)
        {
            SeleccionarObjeto();
        }

        /// <summary>
        /// Crea y devuelve el objeto que ejecutara la busqueda
        /// </summary>
        private UISearchBase CrearBuscador()
        {
            // Obtener el tipo de referencias a buscar
            List<TipoOperacion> tipoReferencias = TiposReferenciasSeleccionadas;

            // Crear el buscador de referencias
            UISearchBase buscador = null;
            KBObject o = ObjetoSeleccionado;
            if (o is SDT || KBaseGX.EsBussinessComponent(o))
            {
                KindOfIndirection tipo;
                if (TipoCampoSeleccionado == CAMPO_CAMPO)
                    tipo = KindOfIndirection.FIELD;
                else
                    tipo = KindOfIndirection.FUNCTION;
                buscador = new BuscadorTokensSdtGx(o, tipo, TxtCampo.Text.Trim());
            }
            else if (o is Domain)
            {
                KindOfIndirection tipo;
                if (TipoCampoSeleccionado == CAMPO_CAMPO)
                    tipo = KindOfIndirection.FIELD;
                else
                    tipo = KindOfIndirection.FUNCTION;
                buscador = new EnumeratedDomainsFinder((Domain)o, TxtCampo.Text.Trim(), tipo);
            }
            else if (o is Artech.Genexus.Common.Objects.Attribute)
            {
                BuscadorReferenciasAtributo attFinder =
                    new BuscadorReferenciasAtributo((Artech.Genexus.Common.Objects.Attribute)o,
                        tipoReferencias);
                if (!string.IsNullOrEmpty(TxtTableFilter.Text))
                    // Set the table filter
                    attFinder.TableFilter = Table.Get(UIServices.KB.CurrentModel, TxtTableFilter.Text);
                buscador = attFinder;
            }
            else if (o is Table)
                buscador = new BuscadorReferenciasTabla((Table)o, tipoReferencias);
            else if (TipoObjetoSeleccionado == TIPO_VARIABLE)
                buscador = new VariablesFinder(TxtObjeto.Text);
            else if (TipoObjetoSeleccionado == TYPE_GENERATOR)
            {
                buscador = new ObjectsByGeneratorFinder(SelectedGenerators)
                {
                    OnlySpecifiable = ChkOnlySpecifiable.Checked
                };
            }

            return buscador;
        }

        /// <summary>
        /// Creates the object to run the search
        /// </summary>
        /// <returns>The finder object. Object if there was problems and the run cannot be started</returns>
        protected override UISearchBase CreateFinder()
        {
            // Hacer validaciones
            string nombreCampo = TxtCampo.Text.Trim();
            KBObject objeto = ObjetoSeleccionado;

            bool selectedTypeIsObject = (TipoObjetoSeleccionado != TIPO_VARIABLE &&
                TipoObjetoSeleccionado != TYPE_GENERATOR);
            if (selectedTypeIsObject && objeto == null)
            {
                MessageBox.Show("No object found with this name");
                TxtObjeto.Focus();
                return null;
            }
            if (TipoObjetoSeleccionado == TYPE_GENERATOR && SelectedGenerators.Count == 0)
            {
                MessageBox.Show("No generator found with this description");
                TxtObjeto.Focus();
                return null;
            }

            if (TipoObjetoSeleccionado == TIPO_VARIABLE && TxtObjeto.Text.Trim() == string.Empty)
            {
                MessageBox.Show("You must specify the variable type to search");
                TxtObjeto.Focus();
                return null;
            }

            if (objeto is SDT || KBaseGX.EsBussinessComponent(objeto) || objeto is Domain)
            {
                if (string.IsNullOrEmpty(nombreCampo))
                {
                    MessageBox.Show("You must specify the field/function of the SDT/BC/Domain to search");
                    TxtCampo.Focus();
                    return null;
                }
            }

            if (TipoCampoSeleccionado == CAMPO_CAMPO && KBaseGX.EsBussinessComponent(objeto))
            {
                // Buscar el campo en el bc:
                Transaction bc = (Transaction)objeto;
                if (bc.Structure.GetAttribute(nombreCampo) == null)
                {
                    MessageBox.Show("BC has no field with name " + nombreCampo);
                    TxtCampo.Focus();
                    return null;
                }
            }

            if ((objeto is Table || objeto is Artech.Genexus.Common.Objects.Attribute)
                && TiposReferenciasSeleccionadas.Count == 0)
            {
                MessageBox.Show("You must specify the kind of references to search");
                ChkEscrituras.Focus();
                return null;
            }

            TxtTableFilter.Text = TxtTableFilter.Text.Trim();
            if (TipoObjetoSeleccionado == TIPO_ATRIBUTO && 
                !string.IsNullOrEmpty(TxtTableFilter.Text) &&
                Table.Get(UIServices.KB.CurrentModel, TxtTableFilter.Text) == null)
            {
                MessageBox.Show("Table " + TxtTableFilter.Text + " does not exists");
                TxtTableFilter.Focus();
                return null;
            }

            // Lanzar la busqueda
            return CrearBuscador();
        }

        /// <summary>
        /// Search button clicked
        /// </summary>
        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            base.SearchButtonPressed();
        }

        /// <summary>
        /// Habilita / deshabilita todos los campos de la interface de usuario
        /// </summary>
        protected override void EnableUIFields(bool habilitar)
        {
            TxtObjeto.Enabled = BtnSeleccionar.Enabled = TxtCampo.Enabled = ChkEscrituras.Enabled =
                ChkLecturas.Enabled = ChkInserciones.Enabled = ChkBorrado.Enabled = 
                CmbTipo.Enabled = CmbTipoCampo.Enabled = TxtTableFilter.Enabled =
                BtnTableFilterSelect.Enabled = ChkOnlySpecifiable.Enabled = habilitar;

            if (habilitar)
                // Disable not applicable fields
                CmbTipo_SelectedValueChanged(null, null);

        }

        /// <summary>
        /// Pulsado el link del objeto a buscar
        /// </summary>
        private void LnkObjeto_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            KBObject o = ObjetoSeleccionado;
            if( o != null )
                UIServices.Objects.Open(o, OpenDocumentOptions.CurrentVersion);
        }

        /// <summary>
        /// Cambiado el tipo de objeto seleccionado
        /// </summary>
        private void CmbTipo_SelectedValueChanged(object sender, EventArgs e)
        {
            // Set defaults:
            TxtCampo.Enabled = CmbTipoCampo.Enabled = false;
            ChkLecturas.Visible = ChkEscrituras.Visible = true;
            ChkLecturas.Enabled = ChkEscrituras.Enabled = true;
            LblSearch.Visible = true;

            ChkInserciones.Visible = ChkBorrado.Visible = (TipoObjetoSeleccionado == TIPO_TABLA);
            if (TipoObjetoSeleccionado == TIPO_SDT || TipoObjetoSeleccionado == TIPO_BC ||
                TipoObjetoSeleccionado == TIPO_ENUM)
            {
                TxtCampo.Enabled = CmbTipoCampo.Enabled = true;
                ChkLecturas.Enabled = ChkEscrituras.Enabled = false;
                ChkLecturas.Checked = ChkEscrituras.Checked = true;
            }
            else if (TipoObjetoSeleccionado == TIPO_VARIABLE || 
                     TipoObjetoSeleccionado == TYPE_GENERATOR)
                ChkLecturas.Visible = ChkEscrituras.Visible = false;
            ChkOnlySpecifiable.Visible = (TipoObjetoSeleccionado == TYPE_GENERATOR);

            ShowTablesWarning();

            switch (TipoObjetoSeleccionado)
            {
                case TIPO_VARIABLE:
                    LnkObjeto.Text =  "V. type";
                    break;
                case TYPE_GENERATOR:
                    LnkObjeto.Text = "Generator";
                    break;
                default:
                    LnkObjeto.Text =  "Object";
                    break;
            }
            BtnSeleccionar.Visible = (TipoObjetoSeleccionado != TIPO_VARIABLE);

            TxtTableFilter.Enabled = BtnTableFilterSelect.Enabled = 
                (TipoObjetoSeleccionado == TIPO_ATRIBUTO);

            ChkLecturas_CheckedChanged(null, null);
        }

        /// <summary>
        /// Shows / hides a warning message about tables references and kbase specificacion
        /// </summary>
        private void ShowTablesWarning()
        {
            LblAvisoTablas.Visible = (TipoObjetoSeleccionado == TIPO_TABLA ||
                (TipoObjetoSeleccionado == TIPO_ATRIBUTO && !string.IsNullOrEmpty(TxtTableFilter.Text))
                ) ;
        }

        /// <summary>
        /// Cambiado el check de lecturas
        /// </summary>
        private void ChkLecturas_CheckedChanged(object sender, EventArgs e)
        {
            if (TipoObjetoSeleccionado == TIPO_ATRIBUTO)
            {
                if (ChkLecturas.Checked)
                {
                    // La busqueda de lectura de atributos implica la busqueda de escrituras
                    ChkEscrituras.Checked = true;
                    ChkEscrituras.Enabled = false;
                }
                else
                    ChkEscrituras.Enabled = true;
            }
        }

        /// <summary>
        /// Abrir la documentacion
        /// </summary>
        private void BtnAyuda_Click(object sender, EventArgs e)
        {
            OpenDocumentation.Open("referencias.shtml");
        }

        /// <summary>
        /// Select filter table table
        /// </summary>
        private void SelectTableFilter()
        {
            Table table = SelectKbObject.SeleccionarTabla();
            if (table == null)
                return;
            TxtTableFilter.Text = table.Name;
            ShowTablesWarning();
        }

        /// <summary>
        /// Select filter table table button clicked
        /// </summary>
        private void BtnTableFilterSelect_Click(object sender, EventArgs e)
        {
            SelectTableFilter();
        }

        /// <summary>
        /// Table filter field focus lost
        /// </summary>
        private void TxtTableFilter_Leave(object sender, EventArgs e)
        {
            ShowTablesWarning();
        }

    }
}
