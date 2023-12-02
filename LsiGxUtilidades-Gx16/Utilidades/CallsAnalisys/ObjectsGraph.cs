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
using Artech.Architecture.Common.Converters;
using Artech.Genexus.Common.Entities;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.CallsAnalisys
{
    /// <summary>
    /// Genexus object calls graph
    /// TODO: Add graph kbobject's cache
    /// </summary>
    public class ObjectsGraph
    {

        /// <summary>
        /// KB main objects (all, not only in the graph)
        /// </summary>
        private HashSet<EntityKey> KBMainNodes = new HashSet<EntityKey>();

        /// <summary>
        /// Graph nodes
        /// </summary>
        public HashSet<EntityKey> Nodes = new HashSet<EntityKey>();

        /// <summary>
        /// Caller objects to an object. Key is destination of the call. Value are the callers
        /// </summary>
        public Dictionary<EntityKey, HashSet<EntityKey>> IncomingCallers = 
            new Dictionary<EntityKey, HashSet<EntityKey>>();

        /// <summary>
        /// Called objects from an object. Key is the caller. Value are the called
        /// </summary>
        public Dictionary<EntityKey, HashSet<EntityKey>> OutcomingCalls =
            new Dictionary<EntityKey, HashSet<EntityKey>>();

        /// <summary>
        /// Call vertices
        /// </summary>
        public HashSet<EntityReference> Vertices = new HashSet<EntityReference>();

        /// <summary>
        /// Cached objects in graph 
        /// </summary>
        private Dictionary<EntityKey, KBObject> ObjectNodesCache = new Dictionary<EntityKey, KBObject>();

        /// <summary>
        /// Model of the graph
        /// </summary>
        public KBModel Model { get; private set; }

        public ObjectsGraph(KBModel model)
        {
            Model = model;

            // Create mains list
            MainsGx.GetKBMainObjects(model).ForEach(x =>
            {
                KBMainNodes.Add(x.Key);
                ObjectNodesCache.Add(x.Key, x);
            });
        }

        public bool IsMain(EntityKey key)
        {
            return KBMainNodes.Contains(key);
        }

        private HashSet<EntityKey> DeclareOutcoming(EntityKey node)
        {
            HashSet<EntityKey> outcomingCalled;
            if (!OutcomingCalls.TryGetValue(node, out outcomingCalled))
            {
                outcomingCalled = new HashSet<EntityKey>();
                OutcomingCalls.Add(node, outcomingCalled);
            }
            return outcomingCalled;
        }

        private HashSet<EntityKey> DeclareIncoming(EntityKey node)
        {
            HashSet<EntityKey> incomingCallers;
            if (!IncomingCallers.TryGetValue(node, out incomingCallers))
            {
                incomingCallers = new HashSet<EntityKey>();
                IncomingCallers.Add(node, incomingCallers);
            }
            return incomingCallers;
        }

        public void AddVertice(EntityReference reference)
        {
            if (Vertices.Add(reference))
            {
                // Vertice added
                // Update outcoming calls
                DeclareOutcoming(reference.From).Add(reference.To);
                
                // Update incoming calls
                DeclareIncoming(reference.To).Add(reference.From);
            }

        }

        public void AddVertices(IEnumerable<EntityReference> references)
        {
            foreach (EntityReference r in references)
                AddVertice(r);
        }

        public bool AddNode(EntityKey key)
        {
            if (Nodes.Contains(key) || !ObjClassLsi.LsiIsCallableType(key.Type))
                return false;

            return Nodes.Add(key);
        }

        static public bool IsValidNode(KBObject o) {
            return o != null && o is ICallableInfo;
        }

        public bool AddNode(KBObject o)
        {
            if (o == null)
                return false;

            // Ignore attributes, tables and so
            if (!ObjClassLsi.LsiIsCallableType(o.Key.Type))
                return false;

            return Nodes.Add(o.Key);
        }

        public HashSet<EntityKey> GetIncomingCalls(EntityKey node)
        {
            HashSet<EntityKey> references;
            IncomingCallers.TryGetValue(node, out references);
            return references;       
        }

        public HashSet<EntityKey> GetOutcomingCalls(EntityKey node)
        {
            HashSet<EntityKey> references;
            OutcomingCalls.TryGetValue(node, out references);
            return references;
        }

        /// <summary>
        /// The objects on the graph marked as main
        /// </summary>
        public List<KBObject> GraphMainObjects
        {
            get { return GetMainObjects(null); }
        }

        /// <summary>
        /// The objects on the graph marked as main of a given generator
        /// </summary>
        /// <param name="generator">The generator of mains to retrieve. If it's null, all mains
        /// will be returned</param>
#if GX_17_OR_GREATER
        public List<KBObject> GetMainObjects(GxGenerator generator)
#else
        public List<KBObject> GetMainObjects(GxEnvironment generator)
#endif
        {

            // Get graph main objects:
            IEnumerable<KBObject> mains = KBMainNodes
                .Select(x => GetObject(x));

            if (generator != null)
            {
                // Filter by generator
                mains = mains.Where(x =>
                {
                    var mainGenerator = MainsGx.GetMainGenerator(x);
                    return mainGenerator != null && mainGenerator == generator;
                });
            }

            return mains.ToList();
        }

        public List<KBObject> GraphWinMainObjects
        {
            get
            {
                // Get graph main objects:
                List<KBObject> winObjects = new List<KBObject>();
                foreach (KBObject o in GraphMainObjects)
                {
                    if (!MainsGx.IsMainWeb(o))
                        winObjects.Add(o);
                }
                return winObjects;
            }
        }

        public List<KBObject> GetDeepCalledObjects(EntityKey initialNode, List<Guid> typesToIgnore = null)
        {
            return GetDeepCalledKeys(initialNode, typesToIgnore).Select(key => GetObject(key)).ToList();
        }

        public List<EntityKey> GetDeepCalledKeys(EntityKey initialNode, List<Guid> typesToIgnore = null)
		{
            return GetDeepCalledKeys(new EntityKey[] { initialNode }, typesToIgnore);
        }

        public List<EntityKey> GetDeepCalledKeys(IEnumerable<EntityKey> initialNodes, List<Guid> typesToIgnore = null)
        {
            List<EntityKey> result = new List<EntityKey>();
            GraphVisitState visitState = new GraphVisitState(initialNodes);
            while (visitState.MoreToVisit)
            {
                EntityKey current = visitState.Pop();

                if (typesToIgnore != null && typesToIgnore.Contains(current.Type))
                    continue;

                result.Add(current);
                // Dont follow mains, except the initial node
                if (!IsMain(current) || initialNodes.Contains(current))
                    visitState.Push(GetOutcomingCalls(current));
            }
            return result;
        }

        public List<EntityKey> GetDeepCalledKeysFromMains(List<Guid> typesToIgnore = null)
		{
            return GetDeepCalledKeys(KBMainNodes, typesToIgnore);
		}

        public List<EntityKey> GetMainCallers(IEnumerable<EntityKey> calledObjects)
        {
            List<EntityKey> result = new List<EntityKey>();
            GraphVisitState visitState = new GraphVisitState(calledObjects);
            while (visitState.MoreToVisit)
            {
                EntityKey current = visitState.Pop();
                if( IsMain(current) )
                    result.Add(current);
                else
                    visitState.Push(GetIncomingCalls(current));
            }
            return result;
        }

        public List<KBObject> GetDeepCalledObjects(EntityKey initialNode)
        {
            return GetDeepCalledObjects(initialNode, null);
        }

        public KBObject GetObject(EntityKey key)
        {
            KBObject o;
            if (!ObjectNodesCache.TryGetValue(key, out o))
            {
                o = Model.Objects.Get(key);
                ObjectNodesCache.Add(key, o);
            }
            return o;
        }

    }
}
