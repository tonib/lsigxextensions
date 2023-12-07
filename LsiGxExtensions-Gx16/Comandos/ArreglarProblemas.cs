using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Variables;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Threading;

namespace LSI.Packages.Extensiones.Comandos
{
    /// <summary>
    /// Dialogo para confirmar y arreglar problemas de un objeto (variables no usadas, reglas hidden, etc)
    /// </summary>
    public partial class ArreglarProblemas : Form , IExecutable
    {

        /// <summary>
        /// Las referencias a variables del objeto
        /// </summary>
        private ObjectVariablesReferences ReferencedVariables;

        /// <summary>
        /// El objeto de las variables
        /// </summary>
        private KBObject Objeto;

        /// <summary>
        /// Object editor
        /// </summary>
        private IDocumentView CurrentView;

        /// <summary>
        /// La configuracion de las extensiones
        /// </summary>
        private LsiExtensionsConfiguration Configuracion = LsiExtensionsConfiguration.Load();

        /// <summary>
        /// Constructor
        /// </summary>
        public ArreglarProblemas()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Ejecuta el comando
        /// </summary>
        public void Execute()
        {

            IGxDocument currentDoc = UIServices.Environment.ActiveDocument;
            if( currentDoc.Dirty ) 
            {
                MessageBox.Show("Object is modified. Please, save it before run this extension");
                return;
            }

            Objeto = currentDoc.Object;
            if (TokensFinder.IsUnsupportedObject(Objeto))
                return;

            CurrentView = UIServices.Environment.ActiveView;

            bool mostrarListaN4 = false;

            if (Objeto.Parts.LsiGet<VariablesPart>() != null)
            {
				// List unused variables
				// IMPORTANT: checkReadWrites must to be true: Otherwise variables used inside
				// native code (CSHARP, JAVA, ... statements) are reported as unused
				// This will be much more slow but, right now there is no alternative
				LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.Load();
                ReferencedVariables = new ObjectVariablesReferences(Objeto, true,
                    cfg.AlwaysNullVariablesPrefixSet, cfg.VariableNamesNoCheckArray);
                List<string> aBorrar = ReferencedVariables.GetUnusedVariables();
                foreach (string nombreVariable in aBorrar)
                    LstVariablesBorrar.Items.Add(nombreVariable);
                if (aBorrar.Count > 0)
                    ChkDeleteVariables.Checked = true;

                if (Configuracion.RevisarVariablesN4)
                {
                    // Listar las variables N(4) que no se vayan a borrar
                    foreach (string nombreVariable in ReferencedVariables.GetN4Variables())
                    {
                        if (!aBorrar.Contains(nombreVariable))
                        {
                            LstVariablesN4.Items.Add(nombreVariable);
                            mostrarListaN4 = true;
                        }
                    }
                }
            }

            if (!mostrarListaN4)
            {
                LblTituloN4.Visible = false;
                LstVariablesN4.Visible = false;
            }

            if (Objeto is WorkPanel)
            {
                // Ver si el objeto tiene una regla hidden:
                bool reglaHidden = ReglaHidden.ContieneReglaHidden(Objeto as WorkPanel);
                ChkBorrarReglaHidden.Visible = reglaHidden;
            }

            ShowDialog();
        }

        /// <summary>
        /// Pulsado el boton cancelar
        /// </summary>
        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        /// <summary>
        /// Pulsado el boton aceptar
        /// </summary>
        private void BtnBorrar_Click(object sender, EventArgs e)
        {
            try
            {
                using (Log log = new Log())
                {

                    // Cerrar el objeto del que se arreglan problemas. Necesario si se toca el winform
                    if( !CurrentView.Close() )
                        return;

                    if (ChkDeleteVariables.Checked || Configuracion.RevisarVariablesN4)
                    {
                        log.Output.AddLine("Updating variables...");
                        VariablesPart variables = Objeto.Parts.LsiGet<VariablesPart>();
                        if (ChkDeleteVariables.Checked)
                            variables.LsiRemove(ReferencedVariables.GetUnusedVariables());

                        if (Configuracion.RevisarVariablesN4)
                        {
                            foreach (string nombreVariable in ReferencedVariables.GetN4Variables())
                            {
                                Variable v = variables.GetVariable(nombreVariable);
                                if (v != null)
                                    v.Description = v.Description.Trim() + " N4";
                            }
                        }
                        Objeto.Parts.LsiUpdatePart(variables);
                    }

                    if (ChkBorrarReglaHidden.Checked)
                    {
                        log.Output.AddLine("Deleting hidden rule...");
                        ReglaHidden.BorrarReglaHidden(log, Objeto as WorkPanel);
                    }

                    if (ChkReplaceOldOperators.Checked)
                    {
                        log.Output.AddLine("Replacing old operators syntax...");
                        new RemoveLogicalOperatorsDots(Objeto).Execute();
                    }

                    Objeto.Save();
                    
                    // Reabrir el objeto
                    UIServices.Objects.Open(Objeto, OpenDocumentOptions.CurrentVersion);

                    Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// Abrir la ayuda
        /// </summary>
        private void ArreglarProblemas_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            OpenDocumentation.Open("verificacion.html#arregloerrores");
        }
    }
}
