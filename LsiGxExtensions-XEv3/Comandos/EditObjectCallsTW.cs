using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Language.Parser.Data;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Llamadas.Edicion;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.UI;
using LSI.Packages.Extensiones.Utilidades.Variables;

namespace LSI.Packages.Extensiones.Comandos
{
    /// <summary>
    /// Toolwindow to edit calls to some object
    /// </summary>
    [Guid("79804B75-AFF0-4f61-BB9E-EABEA0853CC6")]
    public partial class EditObjectCallsTW : ToolWindowBase
    {

        private const string OP_NEW_LITERAL = "Add new parameter (Literal)";
        private const string OP_NEW_VARIABLE = "Add new parameter (Variable)";
        private const string OP_REMOVE_PARAM = "Remove parameter";
        private const string OP_REPLACE_OBJ = "Replace called object by other";

        /// <summary>
        /// Search on background process
        /// </summary>
        private BackgroundWorker BGWorker = new BackgroundWorker();

        /// <summary>
        /// La lista de resultados, sin filtrar
        /// </summary>
        private SortableList<InfoCambioLlamadasObjeto> ResultadosObjetos = new SortableList<InfoCambioLlamadasObjeto>();

        /// <summary>
        /// La lista de resultados, sin filtrar
        /// </summary>
        private SortableList<InfoCambioLlamadasObjeto> ResultadosFiltrados = new SortableList<InfoCambioLlamadasObjeto>();

        /// <summary>
        /// Ultimo nombre de objeto puesto en pantalla
        /// </summary>
        private string UltimoNombreObjeto = string.Empty;

        /// <summary>
        /// The callable object currently selected. null if it does not exist or it's not callable
        /// </summary>
        private ICallableInfo EditionObject
        {
            get
            {
                KBObject o = KBaseGX.GetCallableObject(TxtObjeto.Text.Trim());
                if (o == null || !(o is ICallableInfo))
                    return null;

                if (o is SDT)
                    // SDT are ICallableInfo. I don't know why
                    return null;

                return (ICallableInfo)o;
            }
        }

        /// <summary>
        /// El objeto por el que reemplazar las llamadas. null si no existe o no es llamable
        /// </summary>
        private ICallableInfo NuevoObjetoSeleccionado
        {
            get
            {
                KBObject o = KBaseGX.GetCallableObject(TxtNuevoObjeto.Text.Trim());
                if (o == null || !(o is ICallableInfo))
                    return null;
                return (ICallableInfo)o;
            }
        }

        /// <summary>
        /// La operacion actualmente seleccionada
        /// </summary>
        private string SelectedOperation
        {
            get
            {
                return (string)CmbOperacion.SelectedItem;
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public EditObjectCallsTW()
        {
            InitializeComponent();

            LblEstado.Text = "";

            // Poner el icono de LSI
            this.Icon = Resources.LSI;

            // Inicializar UI:
            CmbOperacion.Items.Add(OP_NEW_LITERAL);
            CmbOperacion.Items.Add(OP_NEW_VARIABLE);
            CmbOperacion.Items.Add(OP_REMOVE_PARAM);
            CmbOperacion.Items.Add(OP_REPLACE_OBJ);
            CmbOperacion.SelectedItem = OP_NEW_VARIABLE;

            ObjetoCambiado();
            Grid.SetObjetos<InfoCambioLlamadasObjeto>(ResultadosFiltrados);
            Grid.CrearColumnasEstandar();

            // Preparar la busqueda en segundo plano.
            BGWorker.DoWork += new DoWorkEventHandler(EjecutarBusqueda);
            BGWorker.ProgressChanged += new ProgressChangedEventHandler(BusquedaSegundoPlano_ProgressChanged);
            BGWorker.WorkerSupportsCancellation = true;
            BGWorker.WorkerReportsProgress = true;
            BGWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BusquedaSegundoPlano_RunWorkerCompleted);

        }

        /// <summary>
        /// Ejecutar la seleccion de objetos
        /// </summary>
        /// <param name="txt">Campo para el que se selecciona el objeto</param>
        private void SeleccionarObjeto(TextBox txt)
        {
            KBObject o = SelectKbObject.SeleccionarObjetoLlamables();
            if (o != null)
            {
                txt.Text = o.QualifiedName.ToString();
                if (txt == TxtObjeto)
                {
                    CmbParametro.Focus();
                    ObjetoCambiado();
                }
            }
        }

        /// <summary>
        /// Pulsado el boton de seleccionar objeto llamado
        /// </summary>
        private void BtnSelObjeto_Click(object sender, EventArgs e)
        {
            SeleccionarObjeto(TxtObjeto);
        }

        /// <summary>
        /// Llamar si se cambia el objeto seleccionado
        /// </summary>
        private void ObjetoCambiado()
        {

            ICallableInfo o = EditionObject;
            BtnRevisarCambios.Enabled = BtnHacerCambios.Enabled = (o != null);

            // Evitar repetir operaciones si no se ha cambiado el nombre del objeto
            if (UltimoNombreObjeto == TxtObjeto.Text)
                return;
            UltimoNombreObjeto = TxtObjeto.Text;

            RefreshParameters(o);
        }

        /// <summary>
        /// Refresh the object parameters list
        /// </summary>
        /// <param name="o">The object with the parameters</param>
        private void RefreshParameters(ICallableInfo o)
        {
            CmbParametro.Items.Clear();
            if (o == null)
                // Objeto no valido
                return;
            // Get the parameters:
            foreach (ParameterElement p in ReglaParm.ObtenerParametros(o))
                CmbParametro.Items.Add(p);
            // Select the last one
            if (CmbParametro.Items.Count > 0)
                CmbParametro.SelectedIndex = CmbParametro.Items.Count - 1;
        }

        /// <summary>
        /// Foco movido del campo del objeto
        /// </summary>
        private void TxtObjeto_Leave(object sender, EventArgs e)
        {
            ObjetoCambiado();
        }

        /// <summary>
        /// Abre el objeto seleccionado en el editor
        /// </summary>
        /// <param name="objeto"></param>
        private void AbrirObjeto(ICallableInfo objeto)
        {
            KBObject o = (KBObject)objeto;
            if (o != null)
                UIServices.Objects.Open(o, OpenDocumentOptions.CurrentVersion);
        }

        /// <summary>
        /// Pinchado el link del objeto llamado
        /// </summary>
        private void LnkObjeto_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AbrirObjeto(EditionObject);
        }

        /// <summary>
        /// Crea la operacion a hacer en los llamadores a partir de la UI
        /// </summary>
        /// <returns></returns>
        private IOperacionLlamada CrearOperacion()
        {
            if (SelectedOperation == OP_REMOVE_PARAM)
                return new OpQuitarParametro((KBObject)EditionObject, CmbParametro.SelectedIndex);
            else if (SelectedOperation == OP_REPLACE_OBJ)
                return new OpReemplazarLlamador((KBObject)NuevoObjetoSeleccionado);
            else
            {
                // Agregar un parametro
                bool crearVariable = (SelectedOperation == OP_NEW_VARIABLE);
                return new OpAgregarParametro((KBObject)EditionObject, CmbParametro.SelectedIndex,
                    TxtValorParam.Text.Trim(), crearVariable);
            }
        }

        /// <summary>
        /// Pulsado el boton de revisar o hacer cambios
        /// </summary>
        private void BtnCambios_Click(object sender, EventArgs e)
        {
            try
            {
                if (BGWorker.IsBusy)
                {
                    if (MessageBox.Show("Are you sure you want to cancel the callers edition?",
                        "Cancel",
                        MessageBoxButtons.YesNo) == DialogResult.No)
                        return;

                    // Se quiere cancelar la busqueda:
                    BGWorker.CancelAsync();
                    return;
                }

                // Hacer validaciones
                VerificarLiteral();

                if (EditionObject == null)
                {
                    MessageBox.Show("Selected object does not exists or is not callable");
                    TxtObjeto.Focus();
                    return;
                }

                string op = SelectedOperation;

                if (op != OP_REPLACE_OBJ && CmbParametro.Items.Count == 0)
                {
                    MessageBox.Show("The object object has no parameters");
                    return;
                }

                if (op == OP_NEW_LITERAL && string.IsNullOrEmpty(TxtValorParam.Text.Trim()))
                {
                    MessageBox.Show("Please, specify the literal");
                    TxtValorParam.Focus();
                    return;
                }

                if (op == OP_NEW_VARIABLE)
                {
                    string nombreVariable = TxtValorParam.Text.Trim();
                    if (string.IsNullOrEmpty(nombreVariable) || nombreVariable.StartsWith("&") ||
                        nombreVariable.IndexOf(' ') > 0 || !Variable.IsValidName(nombreVariable) )
                    {
                        MessageBox.Show("Invalid variable name");
                        TxtValorParam.Focus();
                        return;
                    }
                }

                if (op == OP_REPLACE_OBJ && NuevoObjetoSeleccionado == null)
                {
                    MessageBox.Show("New object does not exists or is not callable");
                    TxtNuevoObjeto.Focus();
                    return;
                }

                if (sender == BtnHacerCambios)
                {
                    if (MessageBox.Show(
                        "Are you sure you want to make the chages?",
                        "Confirmation",
                        MessageBoxButtons.YesNo) != DialogResult.Yes)
                        return;
                }

                // Poner la UI en modo busqueda
                BtnRevisarCambios.Text = "Cancel";
                BtnHacerCambios.Enabled = false;
                ResultadosObjetos.Clear();
                ResultadosFiltrados.Clear();
                LblEstado.Text = "Searching...";
                HabilitarTodo(false);

                // Crear la busqueda
                KBObject objetoLlamado = (KBObject) EditionObject;
                bool soloRevisar = (sender == BtnRevisarCambios);
                EditorLlamadas editor = new EditorLlamadas(objetoLlamado, CrearOperacion() , soloRevisar);
                editor.ValidarObjetoAlRevisar = ChkValidarObjetos.Checked;
                editor.BackgroundSearch = BGWorker;
                editor.CallersNameFilter = TxtStartName.Text.Trim();

                PicActividad.Enabled = PicActividad.Visible = true;

                // Lanzar la busqueda
                BGWorker.RunWorkerAsync(editor);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                HabilitarTodo(true);
                ObjetoCambiado();
            }

        }

        /// <summary>
        /// Punto de entrada del proceso de busqueda en segundo plano.
        /// </summary>
        private void EjecutarBusqueda(object sender, DoWorkEventArgs e)
        {
            try
            {
                EditorLlamadas editor = (EditorLlamadas)e.Argument;
                // Do no check objects if the modification is running.
                if (!editor.SoloRevisar)
                    Package.ObjectCheckingOnSaveEnabled = false;
                editor.ExecuteUISearch();
            }
            catch (Exception ex)
            {
                // Reportar el error
                BGWorker.ReportProgress(0, ex);
            }
            finally
            {
                Package.ObjectCheckingOnSaveEnabled = true;
            }
        }

        /// <summary>
        /// Agrega (o no) un resultado de un objeto segun sea el tipo de resultado (ok, error,aviso)
        /// y lo que el usuario ha indicado filtrar
        /// </summary>
        /// <param name="info">El nuevo resultado a añadir</param>
        private void AgregarResultadoFiltrado(InfoCambioLlamadasObjeto info)
        {
            bool ok = false;
            if (ChkErrores.Checked && info.EstadoCambiosObjeto == Estado.ERROR)
                ok = true;
            else if (ChkAvisos.Checked && info.EstadoCambiosObjeto == Estado.WARNING)
                ok = true;
            else if (ChkOk.Checked && info.EstadoCambiosObjeto == Estado.OK)
                ok = true;
            if (ok)
                ResultadosFiltrados.Add(info);
        }

        /// <summary>
        /// Llamado cuando la busqueda devuelve algun resultado
        /// </summary>
        void BusquedaSegundoPlano_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Se ha reportado una busqueda encontrada
            if (e.UserState is InfoCambioLlamadasObjeto)
            {
                InfoCambioLlamadasObjeto info = (InfoCambioLlamadasObjeto)e.UserState;
                ResultadosObjetos.Add(info);
                AgregarResultadoFiltrado(info);
            }
            else if (e.UserState is Exception)
                Log.ShowException((Exception)e.UserState);
            else if (e.UserState is string)
                LblEstado.Text = (string)e.UserState;
        }

        /// <summary>
        /// Llamado cuando la busqueda ha finalizado.
        /// </summary>
        void BusquedaSegundoPlano_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LblEstado.Text = "N. caller objects: " + ResultadosObjetos.Count;
            BtnRevisarCambios.Text = "Test changes";
            HabilitarTodo(true);
            ObjetoCambiado();
            PicActividad.Enabled = PicActividad.Visible = false;
        }

        /// <summary>
        /// Habilita / deshabilita toda la interfaz de usuario.
        /// </summary>
        /// <param name="habilitar">Cierto si hay que habilitar. Falso si hay que deshabilitar</param>
        private void HabilitarTodo(bool habilitar)
        {
            TxtObjeto.Enabled = BtnSelObjeto.Enabled = CmbParametro.Enabled = 
            TxtValorParam.Enabled = ChkValidarObjetos.Enabled = CmbOperacion.Enabled =
            TxtNuevoObjeto.Enabled = BtnSelNuevoObjeto.Enabled = BtnRefreshParms.Enabled =
                habilitar;
        }

        /// <summary>
        /// Se ha cambiado la seleccion en la lista de objetos
        /// </summary>
        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            string txt = string.Empty;
            IList<InfoCambioLlamadasObjeto> cambios = Grid.GetObjetos<InfoCambioLlamadasObjeto>();
            foreach (DataGridViewRow filaSeleccionada in Grid.SelectedRows)
            {
                InfoCambioLlamadasObjeto info = cambios[filaSeleccionada.Index];
                txt += "Object: " + info.NombreObjeto + Environment.NewLine;
                foreach (InfoCambioLlamada il in info.EstadosLlamadas)
                    txt += il.InformacionCambio + Environment.NewLine + Environment.NewLine;
            }
            TxtCambios.Text = txt;
        }

        /// <summary>
        /// Se ha seleccionado un parametro del objeto en el combo.
        /// Propone un nombre por defecto para nombre de la variable a poner en las llamadas
        /// </summary>
        private void CmbParametro_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {

                if (CmbParametro.SelectedIndex < 0)
                    return;

                ICallableInfo o = EditionObject;
                if (o == null)
                    return;

                /*if (o is DataSelector)
                    return;*/

                // Descripcion del parametro
                ParameterElement p = ReglaParm.ObtenerParametros(o)[CmbParametro.SelectedIndex];
                if (p.IsAttribute)
                    LblTipoParametro.Text = "Atr: " + p.Name;
                else
                    LblTipoParametro.Text = VariableGX.DescripcionTipo(UIServices.KB.CurrentModel, (Variable)p.GetParameterObject((KBObject)o));

                // Propose variable name
                if (SelectedOperation == OP_NEW_VARIABLE)
                {
                    if( p.Accessor == RuleDefinition.ParameterAccess.PARM_IN )
                        TxtValorParam.Text = "z" + p.Name;
                    else
                        TxtValorParam.Text = p.Name;
                }

            }
            catch { }
        }

        /// <summary>
        /// Cambiado un check del filtro de los estados
        /// </summary>
        private void ChkFiltroEstado_CheckedChanged(object sender, EventArgs e)
        {
            ResultadosFiltrados.Clear();
            foreach (InfoCambioLlamadasObjeto info in ResultadosObjetos)
                AgregarResultadoFiltrado(info);
        }

        /// <summary>
        /// Verifica la expresion del literal que se va a poner
        /// </summary>
        private void VerificarLiteral()
        {
            try
            {
                if (SelectedOperation != OP_NEW_LITERAL)
                    return;

                TxtValorParam.Text = TxtValorParam.Text.Trim();
                ObjectBase expresion = ParserGx.ParsearExpresion(TxtValorParam.Text);
                TxtValorParam.Text = expresion.ToString();
            }
            catch
            {
                TxtValorParam.Text = string.Empty;
            }
        }

        /// <summary>
        /// Llamado cuando el control del nuevo valor pierde el foco
        /// </summary>
        private void TxtValorParam_Leave(object sender, EventArgs e)
        {
            VerificarLiteral();
        }

        /// <summary>
        /// Abrir la ayuda
        /// </summary>
        private void BtnAyuda_Click(object sender, EventArgs e)
        {
            OpenDocumentation.Open("llamadas.shtml");
        }

        /// <summary>
        /// Cambiada la operacion a hacer
        /// </summary>
        private void CmbOperacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            string op = SelectedOperation;

            LblParametro.Visible = LblNuevoValor.Visible = TxtValorParam.Visible =
            CmbParametro.Visible = LnkNuevoObjeto.Visible = TxtNuevoObjeto.Visible =
            BtnSelNuevoObjeto.Visible = LblTipoParametro.Visible = BtnRefreshParms.Visible =
                false;

            if (op == OP_REMOVE_PARAM)
            {
                // Quitar un parametro
                LblParametro.Visible = CmbParametro.Visible = LblTipoParametro.Visible =
                    BtnRefreshParms.Visible = true;

                LblParametro.Text = "Parameter to remove";
            }
            else if (op == OP_REPLACE_OBJ)
            {
                // Reemplazar llamadas a un objeto por otro
                LnkNuevoObjeto.Visible = TxtNuevoObjeto.Visible = 
                BtnSelNuevoObjeto.Visible = 
                    true;
            }
            else
            {
                // Agregar un parametro
                LblParametro.Visible = CmbParametro.Visible = LblTipoParametro.Visible =
                LblNuevoValor.Visible = TxtValorParam.Visible = BtnRefreshParms.Visible =
                    true;

                LblParametro.Text = "Parameter to add";
                if (op == OP_NEW_LITERAL)
                {
                    // Agregar un literal
                    LblNuevoValor.Text = "New parameter value (literal)";
                    TxtValorParam.Text = string.Empty;
                }
                else
                {
                    // Agregar una variable
                    LblNuevoValor.Text = "New parameter value (variable name)";
                    CmbParametro_SelectedIndexChanged(null, null);
                }
            }
        }

        /// <summary>
        /// Pulsado el boton de seleccionar el nuevo objeto
        /// </summary>
        private void BtnSelNuevoObjeto_Click(object sender, EventArgs e)
        {
            SeleccionarObjeto(TxtNuevoObjeto);
        }

        /// <summary>
        /// Pinchado el link de nuevo objeto
        /// </summary>
        private void LnkNuevoObjeto_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AbrirObjeto(NuevoObjetoSeleccionado);
        }

        /// <summary>
        /// Refresh object parameters button clicked
        /// </summary>
        private void BtnRefreshParms_Click(object sender, EventArgs e)
        {
            RefreshParameters(EditionObject);
        }

    }
}
