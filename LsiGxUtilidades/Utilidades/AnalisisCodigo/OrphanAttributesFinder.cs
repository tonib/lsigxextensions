using System.Collections.Generic;
using Artech.Architecture.Language.Parser.Data;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{

    /// <summary>
    /// Tool to search orphan attributes on code (attributes outside FOR EACHs / NEWs statements)
    /// </summary>
    public class OrphanAttributesFinder : TokensFinder
    {

        /// <summary>
        /// True if some FOR EACH statement has been found
        /// </summary>
        public bool ContainsForEachs;

        /// <summary>
        /// True is search has found some FOR EACH / NEW in code
        /// </summary>
        public bool HasDbAcessCommands;

		/// <summary>
		/// True if NEW commands have been found in code
		/// </summary>
		public bool HasNewCommands;

        /// <summary>
        /// Lista de los printblocks de los que se han encontrado impresiones fuera de FOR EACHs
        /// </summary>
        public HashSet<string> PrintBlocksImpresos = new HashSet<string>();

        /// <summary>
        /// Atributos referenciados en la regla parm
        /// </summary>
        public HashSet<string> AtributosParametros = new HashSet<string>();

		/// <summary>
		/// Report attributes referenced in SUB definitions?
		/// </summary>
		private bool ReportAttsInSubs;

		/// <summary>
		/// Constructor
		/// </summary>
		public OrphanAttributesFinder(bool reportAttsInSubs=true)
        {
            Token = new TokenGx(TokenType.ATTRIBUTE);
            Token.VerificarAtributos = true;
            // References to "Attribute" on "Nullvalue( Attribute )" should be ignored
            IgnoreNullValueParms = true;
			ReportAttsInSubs = reportAttsInSubs;
		}
        
        /// <summary>
        /// Analiza un nodo del parseado de codigo Genexus, para ver si se corresponde con el token
        /// que se busca
        /// </summary>
        /// <param name="nodo">Nodo a analizar</param>
        /// <param name="estado">Estado de la busqueda</param>
        override protected void AnalizarNodoCodigo(ObjectBase nodo, ParsedCodeFinder.SearchState estado)
        {

            string printBlockName, subName;

            if (BuscadorReglas.EsRegla(BuscadorReglas.REGLAPARM, nodo))
            {
                // Hacer una busqueda separada para la regla parm. Se analiza despues
                estado.SearchDescendants = false;
                TokensFinder buscadorAtributos = new TokensFinder(this.Token);
                AtributosParametros = buscadorAtributos.BuscarTodosNombres(estado.OwnerObject, nodo, false);
                return;
            }
            else if (nodo.LsiIsAggregatedFunction())
            {
                // Los atributos dentro de funciones agregadas (sum, average, etc) no son huerfanos
                estado.SearchDescendants = false;
                return;
            }
            else if (nodo.LsiIsForEachStatement())
            {
                // Los atributos dentro de sentencias FOR EACH no son huerfanos
                estado.SearchDescendants = false;
                ContainsForEachs = true;
                HasDbAcessCommands = true;
                return;
            }
            else if (nodo.LsiIsNewStatement())
            {
                // Los atributos dentro de sentencias NEW no son huerfanos
                estado.SearchDescendants = false;
                HasDbAcessCommands = true;
				HasNewCommands = true;
				return;
            }
            else if (nodo.LsiIsCmdPrint(out printBlockName))
            {
                // Guardar el nombre del printblock, para analizarlo despues
                estado.SearchDescendants = false;
                PrintBlocksImpresos.Add(printBlockName);
                return;
            }
			else if(nodo.LsiIsSubDefinition(out subName))
			{
				// Optionally ignore attributes referenced inside SUBs
				if(!ReportAttsInSubs)
				{
					estado.SearchDescendants = false;
					return;
				}
			}

            base.AnalizarNodoCodigo(nodo, estado);               
            
        }
    }
}
