using System.Collections.Generic;
using System.Diagnostics;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Variables;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Validation;

namespace LSI.Packages.Extensiones.Comandos.ValidacionObjetos
{
    /// <summary>
    /// Check errors on object variables
    /// </summary>
    public class CheckVariables : IValidator
    {

        /// <summary>
        /// Informacion del objeto a validar
        /// </summary>
        private ValidationTask Validator;

        /// <summary>
        /// Cierto si se ha llegado a listar algun mensaje sobre las variables
        /// </summary>
        private bool AlgunMensaje;

        /// <summary>
        /// Extensions configuration
        /// </summary>
        private LsiExtensionsConfiguration Configuration;

        /// <summary>
        /// Execute the validation on the object
        /// </summary>
        public void Validate(ValidationTask task)
        {

            Validator = task;
            Configuration = LsiExtensionsConfiguration.Load();

            // Calculate the execution time
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            AlgunMensaje = false;

            if (Validator.ObjectToCheck.Parts.LsiGet<VariablesPart>() == null)
                // El objeto no tiene variables
                return;

			LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.Load();
            ObjectVariablesReferences references =
                new ObjectVariablesReferences(Validator.ObjectToCheck, 
                    Configuration.RevisarLecturasEscrituras,
                    cfg.AlwaysNullVariablesPrefixSet, cfg.VariableNamesNoCheckArray);

            if (Configuration.RevisarVariablesAutodefinidas)
                ReportVariables(references.ObtenerAutodefinidas(), "There are autodefined variables", false);

            if (Configuration.RevisarVariablesNoUsadas)
                ReportVariables(references.GetUnusedVariables(), "There are unused variables", false);

            if (Configuration.RevisarVariablesN4)
            {
                if (ReportVariables(references.GetN4Variables(), "There are N(4) variables", false))
                    Validator.Log.Output.AddLine("(If you put a 'n4' text on the variable description this warning will not be shown)");
            }

            if (Configuration.RevisarLecturasEscrituras)
            {
                if (Configuration.RevisarVariablesSoloLeidas)
                    ReportVariables(references.ObtenerSoloLeidas(Configuration.ReportVariablesROWithInitialValue), "There are only read variables", false);
                if (Configuration.RevisarVariablesSoloEscritas)
                    ReportVariables(references.ObtenerSoloEscritas(), "There are only written variables", false);
                if (Configuration.RevisarParametrosOut)
                    ReportVariables(references.ObtenerDeSalidaNoAsignadas(), 
                        "There are out: parameter variables not written", false);

                ReportVariables(references.GetWrittenInParameters(), "There are in: parameter variables written", false);
            }

            if (Configuration.ReportAlwaysNullWithInitialValue)
            {
                ReportVariables(references.AlwaysNullWithInitialValue(), 
                    "There are variables with 'always null' prefix name (" + 
                    Configuration.AlwaysNullVariablesPrefix +
                    ") and 'Initial Value' set", false);
            }

            // Mostrar los errores (o lo que seguramente son errores) al final, no sea que no se vean por el scroll 
            // en la ventana de output
            if (Configuration.RevisarVariablesSoloUI)
            {
                ReportVariables(references.OnlyReferencedOnForm(),
                    "There are variables referenced only in Form parts", false);
                ReportVariables(references.OnlyReferencedOnConditions(),
                    "There are variables referenced only in Conditions parts", false);
            }

            if (Configuration.RevisarVariablesSoloReglaParm)
                ReportVariables(references.ObtenerSoloUsadasParm(), "There are variables referenced only in Parm rule", false);
            ReportVariables(references.ObtenerNoExistentes(), "There are undefined variables referenced", true);

            if (Configuration.RevisarLecturasEscrituras &&
                Configuration.RevisarParametrosLlamadas)
                ReportVariables(references.ObtenerErroresEnLlamadas(), "There are object calls with wrong number of parameters", true);

            stopWatch.Stop();

            if (AlgunMensaje)
                Validator.Log.PrintExecutionTime(stopWatch);

        }

        /// <summary>
        /// Muestra una lista de variables en el output de genexus
        /// </summary>
        /// <param name="nombreVariables">Lista de nombre de variables a mostrar. Si la lista esta vacia
        /// no se muestra ningun mensaje</param>
        /// <param name="mensaje">Mensaje de titulo para las variables</param>
        /// <param name="esError">Cierto si el mensaje se ha de mostrar como un error. Falso para mostrarlo
        /// como un warning</param>
        /// <returns>Cierto si se ha mostrado algun mensaje</returns>
        private bool ReportVariables(List<string> nombreVariables, string mensaje, bool esError)
        {
            if (nombreVariables.Count > 0)
            {
                Validator.InitializeOutput();
                string msg = "Object " + Validator.ObjectToCheck.QualifiedName + ": " + mensaje + ":";
                if( esError )
                    Validator.Log.Output.AddErrorLine(msg);
                else
                    Validator.Log.Output.AddWarningLine(msg);
                foreach (string v in nombreVariables)
                    Validator.Log.Output.AddLine(v);
                AlgunMensaje = true;
                return true;
            }
            return false;
        }

    }
}
