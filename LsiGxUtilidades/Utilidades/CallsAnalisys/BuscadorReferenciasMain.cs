using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Genexus.Common.Objects;
using Artech.Architecture.Common.Objects;
using Artech.Udm.Framework.References;
using Artech.Architecture.UI.Framework.Services;
using Artech.Udm.Framework;
using LSI.Packages.Extensiones.Utilidades.UI;

namespace LSI.Packages.Extensiones.Utilidades.CallsAnalisys
{

    /// <summary>
    /// Tool to search main objects that call a set of objects
    /// </summary>
    public class MainReferencesFinder : UISearchBase
    {

        /// <summary>
        /// La lista de objetos a los que buscar referencias desde objetos main
        /// </summary>
        private List<KBObject> ObjetosReferenciados = new List<KBObject>();

        /// <summary>
        /// La lista de objetos main que referencian a los objetos indicados en ObjetosReferenciados
        /// </summary>
        private HashSet<string> ObjetosMain = new HashSet<string>();

        /// <summary>
        /// La lista de trabajo de los objetos pendientes de visitar
        /// </summary>
        private HashSet<EntityKey> ListaTrabajo = new HashSet<EntityKey>();

        /// <summary>
        /// La lista de trabajo de los objetos visitados
        /// </summary>
        private HashSet<EntityKey> ObjetosVisitados = new HashSet<EntityKey>();

        /// <summary>
        /// El modelo de trabajo
        /// </summary>
        private KBModel ModeloActual;

        /// <summary>
        /// Objects graph that will be used to do the search
        /// </summary>
        public ObjectsGraph Graph;

        /// <summary>
        /// Search mains that call to mains?
        /// </summary>
        public bool SearchRecursively;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objetosReferenciados">Objetos de los que 
        /// buscar referencias desde objetos main.</param>
        public MainReferencesFinder(List<KBObject> objetosReferenciados)
        {
            ObjetosReferenciados = objetosReferenciados;
            ModeloActual = UIServices.KB.CurrentModel;
            Graph = new ObjectsGraph(ModeloActual);
            MessagesMultiple = 50;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nombreObjeto">Nombre del objeto al que buscar referencias desde mains</param>
        public MainReferencesFinder(KBObject objeto)
        {
            ObjetosReferenciados.Add(objeto);
            ModeloActual = UIServices.KB.CurrentModel;
            Graph = new ObjectsGraph(ModeloActual);
        }

        /// <summary>
        /// Ejecuta la busqueda de los objetos main
        /// </summary>
        /// <returns>La lista de los nombres de los objetos main que referencian a los objetos indicados</returns>
        public List<string> EjecutarBusqueda()
        {

            // Build the calls graph
            FullGraphBuilder graphBuilder = new FullGraphBuilder(Graph);
            graphBuilder.SearchUI = this;
            graphBuilder.BuildGraph();

            // Initial objects to review:
            GraphVisitState visitState = new GraphVisitState();
            foreach (KBObject objectToSearch in ObjetosReferenciados)
            {
                // If the object is main, add it to the result and search its callers
                // Otherwise, if we dont search recursivelly, itself will be reported only
                if (Graph.IsMain(objectToSearch.Key))
                {
                    MainEncontrado(objectToSearch);
                    visitState.Push(Graph.GetIncomingCalls(objectToSearch.Key));
                }
                else
                    visitState.Push(objectToSearch);
            }

            while (visitState.MoreToVisit)
            {
                if (SearchCanceled)
                    break;

                EntityKey current = visitState.Pop();
                if (Graph.IsMain(current))
                {
                    MainEncontrado(Graph.GetObject(current));
                    if( SearchRecursively )
                        visitState.Push(Graph.GetIncomingCalls(current));
                }
                else
                    visitState.Push(Graph.GetIncomingCalls(current));

                IncreaseSearchedObjects();
            }

            // Return mains list
            List<string> mains = ObjetosMain.ToList();
            mains.Sort();
            return mains;

        }

        /// <summary>
        /// Ejecuta la busqueda desde la interface de usuario
        /// </summary>
        public override void  ExecuteUISearch()
        {
            EjecutarBusqueda();
        }

        /// <summary>
        /// Añade, si no se ha hecho ya, un main referenciador a la lista de objetos a buscar
        /// </summary>
        /// <param name="objetoMain">El objeto a buscar</param>
        private void MainEncontrado(KBObject objetoMain)
        {
            if (!ObjetosMain.Contains(objetoMain.QualifiedName.ToString()))
            {
                ObjetosMain.Add(objetoMain.QualifiedName.ToString());
                PublishUIResult(new RefObjetoGX(objetoMain));
            }
        }

    }
}
