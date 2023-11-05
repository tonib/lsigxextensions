using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Language.Parser.Data;
using Artech.Architecture.Language.Parser.Objects;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas
{

    /// <summary>
    /// Utilidad para buscar declaracion de reglas de genexus en el codigo.
    /// </summary>
    public class BuscadorReglas
    {

        /// <summary>
        /// Nombre de la regla parm
        /// </summary>
        public const string REGLAPARM = "parm";

        /// <summary>
        /// Nombre de la regla order
        /// </summary>
        public const string REGLAORDER = "order";

        /// <summary>
        /// Nombre de la regla order
        /// </summary>
        public const string REGLAHIDDEN = "hidden";

        /// <summary>
        /// Nombre de la regla update
        /// </summary>
        public const string REGLAUPDATE = "update";

        /// <summary>
        /// Nombre de la regla default
        /// </summary>
        public const string REGLADEFAULT = "default";

        /// <summary>
        /// Nombre de la regla a buscar, en minusculas
        /// </summary>
        private string NombreRegla;

        /// <summary>
        /// Las reglas encontradas
        /// </summary>
        private List<Rule> ResultadosBusqueda;

        /// <summary>
        /// Cierto si buscamos solo la primera regla
        /// </summary>
        private bool SoloPrimera;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="nombreRegla">Nombre de la regla a buscar</param>
        public BuscadorReglas(string nombreRegla)
        {
            NombreRegla = nombreRegla.Trim().ToLower();
        }

        private void AnalizarNodo(ObjectBase nodo, ParsedCodeFinder.SearchState estado)
        {
            if(EsRegla(NombreRegla, nodo))
            {
                ResultadosBusqueda.Add((Rule)nodo);
                if (SoloPrimera)
                    estado.SearchFinished = true;
            }
        }

        private void EjecutarBusqueda(bool soloPrimera, ParsedCode analizador)
        {
            ResultadosBusqueda = new List<Rule>();
            SoloPrimera = soloPrimera;
            ParsedCodeFinder buscador = new ParsedCodeFinder(AnalizarNodo);
            buscador.Execute(analizador);
        }

        /// <summary>
        /// Devuelve la primera ocurrencia de la regla
        /// </summary>
        /// <param name="codigo">Codigo en el que buscar</param>
        /// <returns>La primera ocurrencia. null si no se encontro la regla</returns>
        public Rule BuscarPrimera(ParsedCode codigo)
        {
            EjecutarBusqueda(true, codigo);
            return ResultadosBusqueda.Count > 0 ? ResultadosBusqueda[0] : null;
        }

        /// <summary>
        /// Devuelve todas las ocurrencias de la regla
        /// </summary>
        /// <param name="codigo">Codigo en el que buscar</param>
        /// <returns>La lista de todas las reglas</returns>
        public List<Rule> BuscarTodas(ParsedCode codigo)
        {
            EjecutarBusqueda(false, codigo);
            return ResultadosBusqueda;
        }

        /// <summary>
        /// Devuelve cierto si el codigo contiene la regla a buscar
        /// </summary>
        /// <param name="codigo">Codigo a analizar</param>
        /// <returns>Cierto si el codigo contiene la regla indicada</returns>
        public bool ContieneRegla(ParsedCode codigo)
        {
            return BuscarPrimera(codigo) != null;
        }

        /// <summary>
        /// Devuelve los parametros (el contenido del parentesis), separado por las comas
        /// </summary>
        /// <param name="regla">La regla de la que obtener los parametros</param>
        /// <returns>Los parametros de la regla, sin las comas</returns>
        static public List<ObjectBase> ParametrosRegla(Rule regla)
        {
            List<ObjectBase> parametros = new List<ObjectBase>(regla.Parameters.Count);
            foreach (ObjectBase parametro in regla.Parameters)
            {
                ObjectBase p = parametro;
                if (p is ListItem)
                    p = ((ListItem)parametro).Content;
                parametros.Add(p);
            }
            return parametros;
        }

        /// <summary>
        /// Borra una regla del codigo
        /// </summary>
        /// <param name="codigoReglas">Codigo de las reglas del que borrar la regla</param>
        /// <param name="regla">Regla a borrar</param>
        /// <returns>Cierto si se encontro la regla a borrar. Falso si no</returns>
        static public bool BorrarRegla(ParsedCode codigoReglas, Rule regla)
        {
            IParserObjectBaseCollection raiz = (IParserObjectBaseCollection)codigoReglas.ArbolParseado;
            for (int i = 0; i < raiz.Count; i++)
            {
                if (raiz[i].Data is PunctuatedSentence)
                {
                    PunctuatedSentence ps = (PunctuatedSentence)raiz[i].Data;
                    if (ps.Sentence == regla)
                    {
                        raiz.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Verifica si un nodo de codigo es una regla de un cierto tipo
        /// </summary>
        /// <param name="nombreRegla">Nombre de la regla que se verifica. DEBE ESTAR EN MINUSCULAS</param>
        /// <param name="nodoCodigo">Nodo de codigo parseado a revisar si es la regla</param>
        /// <returns>Cierto si el nodo de codigo es una regla con el nombre indicado</returns>
        static public bool EsRegla(string nombreRegla, ObjectBase nodoCodigo)
        {
            Rule regla = nodoCodigo as Rule;
            if (regla == null)
                return false;

            Word nombreReglaNodo = regla.Name as Word;
            if (nombreReglaNodo == null)
                return false;

            return nombreReglaNodo.Text.ToLower() == nombreRegla;
        }

    }
}
