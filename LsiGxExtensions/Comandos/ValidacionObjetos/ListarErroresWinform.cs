using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.Form.DOM;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.WinForms;
using LSI.Packages.Extensiones.Utilidades.Validation;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Comandos.ValidacionObjetos
{
    /// <summary>
    /// Lista errores en las llamadas a la ayuda de una transaccion/workpanel
    /// </summary>
    public class ListarErroresWinform : IValidator
    {

        /// <summary>
        /// Informacion del objeto a validar
        /// </summary>
        private ValidationTask Validador;

        /// <summary>
        /// Extensions configuration
        /// </summary>
        private LsiExtensionsConfiguration Configuration;

        /// <summary>
        /// Ejecuta la revision
        /// </summary>
        public void Validate(ValidationTask task)
        {
            Validador = task;
            Configuration = LsiExtensionsConfiguration.Load();

            // Revisar llamadas a la ayuda en workpanels / transacions al guardar
            if (!(Validador.ObjectToCheck is WorkPanel || Validador.ObjectToCheck is Transaction))
                return;

            if (Configuration.RevisarTamanyoWinforms)
                RevisarTamanyo();

            if (Configuration.RevisarReglasHidden)
                RevisarReglaHidden();

            if (Configuration.RevisarColumnasInvisiblesGrids)
                RevisarAtributosSubfilesNoUtilidados();

        }

        /// <summary>
        /// Revisa si el tamaño del winform supera el maximo
        /// </summary>
        private void RevisarTamanyo()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Revisar el tamaño del winform:
            WinFormGx wf = new WinFormGx(Validador.ObjectToCheck);
            Size s = wf.TamanyoForm;
            bool tamanyoErroneo = (s.Width > Configuration.TamMaximoWinforms.Width ||
                s.Height > Configuration.TamMaximoWinforms.Height);
            stopWatch.Stop();

            if (tamanyoErroneo)
            {
                Validador.InitializeOutput();
                // 
                string mensaje = "Object " + Validador.ObjectToCheck.QualifiedName + ": Winform exceeds maximum allowed size." +
                    "Winform size: " + s.ToString() + ", maximum size: " + Configuration.TamMaximoWinforms.ToString();
                Validador.Log.Output.AddWarningLine(mensaje);
                Validador.Log.PrintExecutionTime(stopWatch);
            }
        }

        /// <summary>
        /// Revisa si es un workpanel que contiene una regla hidden
        /// </summary>
        private void RevisarReglaHidden()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            bool contieneHidden = (Validador.ObjectToCheck is WorkPanel &&
                ReglaHidden.ContieneReglaHidden((WorkPanel)Validador.ObjectToCheck));
            stopWatch.Stop();

            if (contieneHidden)
            {
                Validador.InitializeOutput();
                Validador.Log.Output.AddWarningLine("Object " + Validador.ObjectToCheck.QualifiedName + ": It contains a hidden rule");
                Validador.Log.PrintExecutionTime(stopWatch);
            }
        }

        // TODO: Duplicated code in TranslatableTextsVerifier and ListarErroresWinform
        private HashSet<string> BuscarAsignacionesVisible()
        {
            TokenGx token = new TokenGx(TokenType.VARIABLE_OR_ATTRIBUTE, (string)null);
            token.AcceptIndirections = true;
            token.IndirectionsFilter = new TokenIndirectionFilter()
            {
                Kind = KindOfIndirection.FIELD
            };
            token.IndirectionsFilter.MembersFilter.Add("visible");
            BuscadorAsignaciones buscador = new BuscadorAsignaciones(token, false, false);

            EventsPart eventos = Validador.ObjectToCheck.Parts.LsiGet<EventsPart>();
            ParsedCode analizador = new ParsedCode(eventos);

            List<BuscadorAsignaciones.Asignacion> asignacionesVisible = 
                buscador.BuscarTodas(analizador);
            HashSet<string> elementosCambioVisible = new HashSet<string>();
            foreach (BuscadorAsignaciones.Asignacion asignacion in asignacionesVisible)
                elementosCambioVisible.Add(TokenGx.ObtenerNombre(asignacion.AssignedToken, false));

            return elementosCambioVisible;
        }

        /// <summary>
        /// Revisar si los subfiles de un workpanel contiene referencias a atributos que no se usan 
        /// </summary>
        private void RevisarAtributosSubfilesNoUtilidados()
        {
            if (!(Validador.ObjectToCheck is WorkPanel))
                return;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Solo nos interesan atributos ocultos y que siempre estan ocultos: Esto es, atributos
            // o variables que aparecen en un grid, con la propidad visible = false, y de los
            // que no se referencia el valor <elemento>.visible en el codigo

            // Recorrer los grids del form:
            WinFormGx winForm = new WinFormGx(Validador.ObjectToCheck);
            List<GridElement> gridsForm = winForm.BuscarGrids();
            if (gridsForm.Count == 0)
                return;

            // Buscar asignaciones en el codigo a la propiedad ".visible" en el codigo:
            HashSet<string> elementosCambioVisible = BuscarAsignacionesVisible();

            // Obtener la lista de columna siempre invisibles
            List<string> columnasVariablesInvisibles = new List<string>(), 
                columnasAtributosInvisibles = new List<string>();
            foreach (GridElement grid in gridsForm)
            {
                // Solo nos interesan las columas del grid que sea elementos ocultos
                // Revisar las columnas del grid:
                foreach (GridColumnElement columna in grid.Children)
                {
                    bool columnaVisible = (bool) columna.GetPropertyValue(Properties.FORMSFC.Visible);
                    string elementoColumna = columna.Attribute.Name.ToLower();
                    if (!columnaVisible && !elementosCambioVisible.Contains(elementoColumna))
                    {
                        // Columna siempre invisible
                        if( elementoColumna.StartsWith("&") )
                            columnasVariablesInvisibles.Add(elementoColumna.Substring(1));
                        else
                            columnasAtributosInvisibles.Add(elementoColumna);
                    }
                }
            }

            if (columnasVariablesInvisibles.Count == 0 && columnasAtributosInvisibles.Count == 0)
                // No hay nada siempre invisible
                return;

            // Crear buscadores para ver si las columnas siempre invisibles son referenciadas en el codigo
            TokensFinder buscadorVariables = null;
            if (columnasVariablesInvisibles.Count > 0)
            {
                TokenGx token = new TokenGx(TokenType.VARIABLE, columnasVariablesInvisibles);
                buscadorVariables = new TokensFinder(token);
            }
            TokensFinder buscadorAtributos = null;
            if (columnasAtributosInvisibles.Count > 0)
            {
                TokenGx token = new TokenGx(TokenType.ATTRIBUTE, columnasAtributosInvisibles);
                buscadorAtributos = new TokensFinder(token);
            }

            // Buscar referencias a los elementos siempre invisibles en el codigo:
            HashSet<string> variablesReferenciadas = new HashSet<string>();
            HashSet<string> atributosReferenciados = new HashSet<string>();
            foreach (KBObjectPart parte in Validador.ObjectToCheck.Parts.LsiEnumerate())
            {
                if (parte is ISource)
                {
                    ISource parteCodigo = (ISource)parte;
                    ParsedCode analizador = new ParsedCode(parte);
                    if (buscadorVariables != null)
                    {
                        foreach (string variable in buscadorVariables.SearchAllNames(analizador, false))
                            variablesReferenciadas.Add(variable);
                    }
                    if (buscadorAtributos != null)
                    {
                        foreach (string atributo in buscadorAtributos.SearchAllNames(analizador, false))
                            atributosReferenciados.Add(atributo);
                    }
                }
            }

            // Los elementos siempre invisibles no referenciados en el codigo son sospechosos
            List<string> elementosNoReferenciados = new List<string>();
            foreach (string variable in columnasVariablesInvisibles)
            {
                if (!variablesReferenciadas.Contains(variable))
                    elementosNoReferenciados.Add("&" + variable);
            }
            List<string> atributosNoReferenciados = new List<string>();
            foreach (string atributo in columnasAtributosInvisibles)
            {
                if (!atributosReferenciados.Contains(atributo))
                    elementosNoReferenciados.Add(atributo);
            }

            stopWatch.Stop();

            if (elementosNoReferenciados.Count > 0)
            {
                Validador.InitializeOutput();
                Validador.Log.Output.AddWarningLine("Object " + Validador.ObjectToCheck.QualifiedName + ": There are grids hidden columns not referenced:");
                elementosNoReferenciados.Sort();
                foreach (string elemento in elementosNoReferenciados)
                    Validador.Log.Output.AddLine(elemento);
                Validador.Log.PrintExecutionTime(stopWatch);
            }
        }

    }
}
