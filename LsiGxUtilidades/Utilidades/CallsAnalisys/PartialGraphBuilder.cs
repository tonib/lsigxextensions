using System.Collections.Generic;
using System.Linq;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Udm.Framework;
using Artech.Udm.Framework.References;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.UI;

namespace LSI.Packages.Extensiones.Utilidades.CallsAnalisys
{
    /// <summary>
    /// Tool to build a partial graph
    /// </summary>
    /// <remarks>
    /// This class can be MUUUUCH MORE slower than FullGraphBuilder if the set of nodes to add to the 
    /// graph has a medium size
    /// </remarks>
    public class PartialGraphBuilder
    {

        /// <summary>
        /// The builded graph
        /// </summary>
        public ObjectsGraph Graph;

        /// <summary>
        /// Search user interface. It can be null.
        /// </summary>
        public UISearchBase SearchUI;

        private HashSet<EntityKey> OutcomingReviewed = new HashSet<EntityKey>();

        private HashSet<EntityKey> IncomingReviewed = new HashSet<EntityKey>();

        public PartialGraphBuilder(ObjectsGraph graph)
        {
            Graph = graph;
        }

        private IEnumerable<EntityReference> AddVerticesToGraph(EntityKey node, bool forward)
        {
            IEnumerable<EntityReference> references;
            if (forward)
                // Search forward, only callable references
                references =
                    Graph.Model.GetReferencesFrom(node, LinkType.UsedObject)
                    .Where(x => ObjClassLsi.LsiIsCallableType( x.To.Type ));
            else
                // Search backward, only callable references
                references =
                    Graph.Model.GetReferencesTo(node, LinkType.UsedObject)
                    .Where(x => ObjClassLsi.LsiIsCallableType(x.From.Type));
            Graph.AddVertices(references);
            return references;
        }

        private void Track(EntityKey startNode, bool forward)
        {

            GraphVisitState visitState = new GraphVisitState();
            visitState.Push(startNode);

            // Set to store nodes with vertices reviewed
            HashSet<EntityKey> reviewed = (forward ? OutcomingReviewed: IncomingReviewed);

            while (visitState.MoreToVisit)
            {
                if( SearchUI != null && SearchUI.SearchCanceled )
                    return;

                EntityKey current = visitState.Pop();

                // Check we have already stored the node and calls with the direction defined by the
                // variable "forward"
                if (reviewed.Contains(current))
                    continue;
                reviewed.Add(current);

                // Add node to the graph
                Graph.AddNode(current);

                if (Graph.IsMain(current))
                {
                    // It's a main node
                    if (forward && current != startNode)
                        // If we are searching forward and it's not the start node, we can stop
                        continue;
                    if (!forward)
                        // If we ware searching backward, we can stop, even if it's the start node
                        continue;
                }

                // Store vertices (calls)
                IEnumerable<EntityReference> references = AddVerticesToGraph(current, forward);

                // Traverse references
                foreach (EntityReference reference in references)
                {
                    EntityKey key = (forward ? reference.To : reference.From);
                    visitState.Push(key);
                }

                if( SearchUI != null )
                    SearchUI.IncreaseSearchedObjects();
            }
        }

        public bool AddNode(KBObject o, bool trackCallers, bool trackCalled)
        {
            if (!ObjectsGraph.IsValidNode(o))
                return false;

            Graph.AddNode(o);

            // Do tracking
            if (trackCallers)
                Track(o.Key, false);
            if (trackCalled)
                Track(o.Key, true);

            return true;
        }

        public bool AddNode(EntityKey key, bool trackCallers, bool trackCalled)
        {

            if (!ObjClassLsi.LsiIsCallableType(key.Type))
                return false;

            Graph.AddNode(key);

            // Do tracking
            if (trackCallers)
                Track(key, false);
            if (trackCalled)
                Track(key, true);

            return true;
        }

        public void AddNodes(List<KBObject> objects, bool trackCallers, bool trackCalled) 
        {
            foreach( KBObject o in objects)
                AddNode(o, trackCallers, trackCalled);
        }

        public void AddNodes(IEnumerable<EntityKey> keys, bool trackCallers, bool trackCalled)
        {
            foreach (EntityKey key in keys)
                AddNode(key, trackCallers, trackCalled);
        }
    }
}
