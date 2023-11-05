using System;
using System.Collections.Generic;
using System.Text;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Architecture.Language.Parser.Data;
using Artech.Patterns.WorkWithDevices.Objects;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.CallsAnalisys;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas
{
    /// <summary>
    /// Utilidad para la creacion de una regla de parametros de un objeto
    /// </summary>
    public class ReglaParm
    {

        /// <summary>
        /// Lista de parametros de la regla
        /// </summary>
        private List<ParameterElement> ParameterElements = new List<ParameterElement>();

        /// <summary>
        /// Constructor
        /// </summary>
        public ReglaParm() { }

        /// <summary>
        /// Get parameters information about an object.
        /// </summary>
        /// <param name="callableObject">Object to get parameters information</param>
        public ReglaParm(ICallableInfo callableObject)
        {
            ParameterElements = ObtenerParametros(callableObject);
        }

        /// <summary>
        /// Devuelve cierto si hay algun elemento elemento por delante del indicado que 
        /// sea parametro (y no documentacion)
        /// </summary>
        /// <param name="idxElemento">Indice del elemento a revisar</param>
        /// <returns>Cierto si hay algun parametro mas por delante</returns>
        private bool HayMasParametrosPorDelante(int idxElemento)
        {
            for (int i = (idxElemento + 1); i < ParameterElements.Count; i++)
            {
                if (!ParameterElements[i].EsDocumentacion)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Devuelve el texto de la regla parm().
        /// </summary>
        public override string ToString()
        {
            if (ParameterElements.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder("parm(\n\t\t");
            int nCaracteresLinea = 0;
            for (int i = 0; i < ParameterElements.Count; i++)
            {
                ParameterElement elemento = ParameterElements[i];
                string textoElemento = elemento.ToString();
                sb.Append(textoElemento);
                nCaracteresLinea += textoElemento.Length;

                if (elemento.EsParametro)
                {
                    if (HayMasParametrosPorDelante(i))
                    {
                        sb.Append(", ");
                        nCaracteresLinea += 2;
                    }
                }
                else if (elemento.EsDocumentacion)
                {
                    sb.Append("\t\t");
                    nCaracteresLinea = 0;
                }
                if (nCaracteresLinea > Entorno.LONGMAXLINEACODIGO)
                {
                    // No hacer demasiado largas las lineas:
                    sb.AppendLine();
                    sb.Append("\t\t");
                    nCaracteresLinea = 0;
                }
            }

            string codigo = sb.ToString();

            // Quitar los ultimos tabuladores, si hay
            if (codigo.EndsWith("\t\t"))
                codigo = codigo.Substring(0, codigo.Length - 2);

            // Añadir salto de linea final para los parametros, si hace falta.
            if (!codigo.EndsWith("\n"))
                codigo += "\n";
            codigo += ");\n";

            return Entorno.StringFormatoKbase(codigo);
        }

        /// <summary>
        /// Añade una parametro a la regla
        /// </summary>
        /// <param name="variable">Variable parametro</param>
        /// <param name="tipoAcceso">Tipo de acceso (in, out o inout)</param>
        public void AgregarParametro(Variable variable, RuleDefinition.ParameterAccess tipoAcceso) {

            ParameterElements.Add(new ParameterElement(new Parameter(variable, false, tipoAcceso)));
        }

        /// <summary>
        /// Añade una parametro de tipo in: a la regla
        /// </summary>
        /// <param name="variable">Variable parametro</param>
        public void AgregarParametro(Variable variable)
        {
            ParameterElements.Add(new ParameterElement(new Parameter(variable, false, RuleDefinition.ParameterAccess.PARM_IN)));
        }

        /// <summary>
        /// Add documentation a documentation element
        /// </summary>
        /// <param name="documentation">The documentation text</param>
        public void AddDocumentation(string documentation)
        {
            AddDocumentation(documentation, false);
        }

        /// <summary>
        /// Add documentation a documentation element
        /// </summary>
        /// <param name="documentation">The documentation text</param>
        /// <param name="lineBreakBefore">True to add a line break before the documentation</param>
        public void AddDocumentation(string documentation, bool lineBreakBefore)
        {
            ParameterElements.Add(new ParameterElement(documentation)
            {
                LineBreakBefore = lineBreakBefore
            });
        }

        /// <summary>
        /// Generar un call a un objeto con los parametros indicados en esta regla parm, pasando
        /// variables con el mismo nombre.
        /// </summary>
        /// <param name="nombreObjeto">Nombre del objeto a llamar</param>
        /// <returns>El codigo de la llamada</returns>
        public string GenerarCall(string nombreObjeto)
        {
            return GenerarLlamada(nombreObjeto, true, null);
        }

        public string GenerateCall(string nombreObjeto, Dictionary<string, string> replacements)
        {
            return GenerarLlamada(nombreObjeto, true, replacements);
        }

        public string GenerateUdpCall(string nombreObjeto)
        {
            return GenerarLlamada(nombreObjeto, false, null);
        }

        /// <summary>
        /// Generar un call a un objeto con los parametros indicados en esta regla parm, pasando
        /// variables con el mismo nombre.
        /// </summary>
        /// <param name="nombreObjeto">Nombre del objeto a llamar</param>
        /// <param name="addLasPaaddLastParameterrameter">True if we must include the last parameter</param>
        /// <returns>El codigo de la llamada</returns>
        private string GenerarLlamada(string nombreObjeto, bool addLastParameter, Dictionary<string, string> replacements)
        {
            string llamada = nombreObjeto + "(";
            int nCaracteresLinea = nombreObjeto.Length + 1;
            bool primerParametro = true;

            int nElements = ParameterElements.Count;
            if (!addLastParameter && nElements > 0)
                nElements--;

            for (int i = 0; i < nElements; i++)
            {
                ParameterElement e = ParameterElements[i];

                if (!e.EsParametro)
                    continue;

                if (primerParametro)
                    primerParametro = false;
                else
                {
                    llamada += ", ";
                    nCaracteresLinea += 2;
                }

                string valorParametro = e.NombreAtributoVariable;
                string replacement;
                if (replacements != null && replacements.TryGetValue(valorParametro, out replacement))
                    valorParametro = replacement;

                llamada += valorParametro;
                nCaracteresLinea += valorParametro.Length;
                if (nCaracteresLinea > Entorno.LONGMAXLINEACODIGO)
                {
                    llamada += Environment.NewLine + "\t";
                    nCaracteresLinea = 0;
                }
            }
            llamada += ")";
            return llamada;
        }

        /// <summary>
        /// Devuelve la lista de parametros de un objeto llamable
        /// </summary>
        /// <param name="o">El objeto llamable</param>
        /// <returns>La lista de parametros</returns>
        static public List<ParameterElement> ObtenerParametros(ICallableInfo o)
        {
            return KbSignaturesCache.GetMainSignature(o);
        }

        /// <summary>
        /// Devuelve el texto de la regla parm del objeto.
        /// </summary>
        /// <returns>El texto de la regla parm. null si el objeto no tiene reglas o no tiene una
        /// regla parm</returns>
        static public Rule ObtenerReglaParm(RulesPart reglas)
        {
            if (reglas == null)
                return null;

            BuscadorReglas buscador = new BuscadorReglas(BuscadorReglas.REGLAPARM);
            ParsedCode analizador = new ParsedCode(reglas);
            return buscador.BuscarPrimera(analizador);
        }

        static public string GetParmRuleDocumentation(KBObject o, string defaultDocumentation)
        {
            RulesPart parteReglas = o.Parts.LsiGet<RulesPart>();
            if (parteReglas == null)
                return defaultDocumentation;

            Rule reglaParm = ObtenerReglaParm(parteReglas);
            if (reglaParm == null)
                return defaultDocumentation;
            
            return "/* " + reglaParm.ToString() + " */";
        }

    }
}
