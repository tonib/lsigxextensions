using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Udm.Framework.References;
using Artech.Udm.Framework;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.CallsAnalisys
{
    /// <summary>
    /// Tool to store visited and pending to visit nodes on a graph
    /// </summary>
    public class GraphVisitState
    {

        private HashSet<EntityKey> PendingToVisit = new HashSet<EntityKey>();

        private HashSet<EntityKey> Visited = new HashSet<EntityKey>();

        public GraphVisitState() { }

        public GraphVisitState(IEnumerable<KBObject> objectsToVisit)
        {
            Push(objectsToVisit);
        }

        public GraphVisitState(IEnumerable<EntityKey> objectsToVisit)
        {
            Push(objectsToVisit);
        }

        public GraphVisitState(EntityKey keyToVisit)
        {
            Push(keyToVisit);
        }

        public void Push(EntityKey node)
        {
            if (!ObjClassLsi.LsiIsCallableType(node.Type))
                return;

            if (PendingToVisit.Contains(node) || Visited.Contains(node))
                return;

            PendingToVisit.Add(node);
        }

        public void Push(KBObject o)
        {
            if (!ObjectsGraph.IsValidNode(o))
                return;
            Push(o.Key);
        }

        public void Push(IEnumerable<KBObject> objects)
        {
            foreach (KBObject o in objects)
                Push(o);
        }

        public void Push(IEnumerable<EntityKey> nodes)
        {
            if (nodes == null)
                return;
            foreach (EntityKey node in nodes)
                Push(node);
        }

        public EntityKey Pop()
        {
            if (!MoreToVisit)
                return null;

            EntityKey current = PendingToVisit.First();
            PendingToVisit.Remove(current);
            Visited.Add(current);
            return current;
        }

        public bool MoreToVisit
        {
            get { return PendingToVisit.Count > 0; }
        }

    }
}
