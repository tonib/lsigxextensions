using System;
using Artech.Genexus.Common.Objects;
using System.Windows.Forms;
using LSI.Packages.Extensiones.Utilidades;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.Threading;
using Artech.Architecture.Language.Parser.Data;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Comandos
{
    /// <summary>
    /// Copia la regla parm del objeto seleccionado actualmente al portapapeles.
    /// </summary>
    public class CopyParmRule : IExecutable
    {
        /// <summary>
        /// Ejecuta el comando
        /// </summary>
        public void Execute()
        {
            try
            {
                // Obtener el objeto actualmente seleccionado:
                object seleccion = Entorno.ObjetoSeleccionadoActualmenteEnEditor;
                if (seleccion == null || !(seleccion is ICallableInfo))
                    return;

                // Obtener el texto de la regla parm y guardarla en el portapapeles
                KBObject objeto = (KBObject)seleccion;

                RulesPart parteReglas = objeto.Parts.LsiGet<RulesPart>();
                Rule reglaParm = ReglaParm.ObtenerReglaParm(parteReglas);
                if (reglaParm == null)
                    Clipboard.SetText("// Parm rule of " + objeto.QualifiedName + " not found\n");
                else
                    Clipboard.SetText("/* " + reglaParm.ToString() + " */");
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }
    }
}
