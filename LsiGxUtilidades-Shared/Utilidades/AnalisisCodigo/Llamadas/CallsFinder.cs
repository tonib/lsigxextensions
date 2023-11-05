using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Language.Parser.Data;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Udm.Framework;
using Artech.Genexus.Common;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Llamadas
{

    /// <summary>
    /// Tool to search calls on GX code
    /// </summary>
    public class CallsFinder
    {

        /// <summary>
        /// Objects to to search keys . Null if we search calls to any object
        /// </summary>
        private HashSet<EntityKey> ObjectsToSearchKeys;

        /// <summary>
        /// Indica si buscamo solo la primera llamada, o todas
        /// </summary>
        private bool BuscarSoloPrimera;

        /// <summary>
        /// La lista de llamadas encontradas
        /// </summary>
        private List<Llamada> ResultadosBusqueda = new List<Llamada>();

        /// <summary>
        /// Lowercase object names to search. Null if we search calls to any object
        /// </summary>
        private HashSet<string> ObjectNamesToSearch;

        /// <summary>
        /// The calls parser
        /// </summary>
        private CallInfoBuilder CallBuilder = new CallInfoBuilder();

        /// <summary>
        /// It creates a finder to search calls to a list of objects
        /// </summary>
        /// <param name="objectsToSearch">Objects to search</param>
        public CallsFinder(IEnumerable<KBObject> objectsToSearch)
        {
            Initialize(objectsToSearch);
        }

        /// <summary>
        /// It creates a finder to search calls to an objects
        /// </summary>
        /// <param name="objectToSearch">Object to search</param>
        public CallsFinder(KBObject objectToSearch)
        {
            List<KBObject> objectsToSearch = new List<KBObject>();
            objectsToSearch.Add(objectToSearch);
            Initialize(objectsToSearch);
        }

        /// <summary>
        /// It creates a finder to search calls to any object.
        /// WARNING: This will find calls to standard functions too, ex: "(val( &amp;achilipu)"
        /// </summary>
        public CallsFinder()
        {
            Initialize(null);
        }

        /// <summary>
        /// It stores into ModuleNames the module names referenced by objects to search
        /// </summary>
        private void Initialize(IEnumerable<KBObject> objectsToSearch)
        {
            if (objectsToSearch != null)
            {
                ObjectsToSearchKeys = new HashSet<EntityKey>();
                foreach (KBObject o in objectsToSearch)
                    ObjectsToSearchKeys.Add(o.Key);

                ObjectNamesToSearch = new HashSet<string>();
                foreach (KBObject o in objectsToSearch)
                    ObjectNamesToSearch.Add(o.Name.ToLower());

            }
        }

        private void AnalizarNodoCodigo(ObjectBase nodo, ParsedCodeFinder.SearchState estado)
        {
            Llamada call = CallBuilder.VerificarLlamada(nodo, estado);
            if (call != null)
            {
                // It's a call: Check the object name
                if (ObjectNamesToSearch != null && !ObjectNamesToSearch.Contains(call.NombreObjetoNormalizado))
                    return;

                // Check object itself
                if (ObjectsToSearchKeys != null && !ObjectsToSearchKeys.Contains(
                    call.GetCalledObjectKey(estado.OwnerObject)))
                    return;

                ResultadosBusqueda.Add(call);
                if (BuscarSoloPrimera)
                    estado.SearchFinished = true;
            }
        }

        private void Buscar(ParsedCode analizador, bool soloPrimera)
        {
            BuscarSoloPrimera = soloPrimera;
            ResultadosBusqueda.Clear();
            ParsedCodeFinder buscador = new ParsedCodeFinder(AnalizarNodoCodigo);
            buscador.Execute(analizador);
        }

        /// <summary>
        /// Busca la primera llamada al objeto en el codigo
        /// </summary>
        /// <param name="analizador">Analizador que contiene el codigo</param>
        /// <returns>La llamada encontrada. null si no se encontro ninguna llamada</returns>
        public Llamada BuscarPrimeraLlamada(ParsedCode analizador)
        {
            Buscar(analizador, true);
            return ResultadosBusqueda.Count == 0 ? null : ResultadosBusqueda[0];
        }

        public List<Llamada> BuscarTodas(ParsedCode analizador)
        {
            Buscar(analizador, false);
            return ResultadosBusqueda;
        }

        /// <summary>
        /// Busca todas la llamadas y la informacion asociada a los parametros
        /// </summary>
        /// <param name="analizador">Analizador que contiene el codigo</param>
        /// <returns>La lista de llamadas encontradas, con la informacion correspondiente
        /// de los parametros</returns>
        public List<GxCallInfo> BuscarLlamadasConParametros(ParsedCode analizador)
        {
            return GxCallInfo.SearchCalls(this, analizador);
        }

    }
}
