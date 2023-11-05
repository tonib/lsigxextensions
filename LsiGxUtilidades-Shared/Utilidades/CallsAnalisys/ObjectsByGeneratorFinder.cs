using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Common.Objects;
using LSI.Packages.Extensiones.Utilidades.UI;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Objects;
using Artech.Udm.Framework.References;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Parts;
using System.Windows.Forms;
using Artech.Patterns.WorkWithDevices.Objects;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using Artech.GXplorer.Common.Objects;
using Artech.Genexus.Common.Entities;
using LSI.Packages.Extensiones.Utilidades.CallsAnalisys;
using Artech.Udm.Framework;
using Artech.Genexus.Common.Helpers;

namespace LSI.Packages.Extensiones.Utilidades.CallsAnalisys
{
    /// <summary>
    /// Tool to search objects that are referenced by some generator
    /// </summary>
    public class ObjectsByGeneratorFinder : UISearchBase
    {

        /// <summary>
        /// Generator which we will get the referenced objects
        /// </summary>
        private List<GxEnvironment> Generators;

        /// <summary>
        /// Should we publish only specifiable objects?
        /// </summary>
        public bool OnlySpecifiable;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generators">Generators which we will get the referenced objects</param>
        public ObjectsByGeneratorFinder(IEnumerable<GxEnvironment> generators)
        {
            Generators = generators.ToList();
            MessagesMultiple = 50;
        }

        /// <summary>
        /// Perform the search
        /// </summary>
        override public void ExecuteUISearch()
        {
            if (Generators.Count == 0)
                return;

            // Build the objects graph
            ObjectsGraph graph = new ObjectsGraph(Generators[0].Model);
            FullGraphBuilder graphBuilder = new FullGraphBuilder(graph);
            graphBuilder.SearchUI = this;
            graphBuilder.BuildGraph();

            // Get the generators mains
            GraphVisitState visitState = new GraphVisitState();
            foreach (GxEnvironment generator in Generators)
            {
                List<KBObject> mainObjects = graph.GetMainObjects(generator);

                // Traverse the graph starting from the mains, and report them
                foreach (KBObject main in mainObjects)
                {
                    if (SearchCanceled)
                        return;
                    PublishUIResult(new RefObjetoGX(main));
                    visitState.Push(graph.GetOutcomingCalls(main.Key));
                }
            }

            // Get specifiable object types, if it's needed
            List<Guid> specifiableTypes = GetObjectTypesToTraverse();

            while (visitState.MoreToVisit)
            {
                if (SearchCanceled)
                    return;

                EntityKey objectKey = visitState.Pop();

                IncreaseSearchedObjects();
                
                // Ignore not specifiable objects?: SDTs, external objects, data selectors...
                if (OnlySpecifiable && !specifiableTypes.Contains(objectKey.Type))
                    continue;
                // Ignore main objects
                else if (graph.IsMain(objectKey))
                    continue;

                // Report the object
                KBObject o = graph.GetObject(objectKey);
                PublishUIResult(new RefObjetoGX(o));
                visitState.Push(graph.GetOutcomingCalls(objectKey));
                
            }
        }

        private List<Guid> GetObjectTypesToTraverse()
        {
            if (!OnlySpecifiable)
                return null;

            List<Guid> objectTypes = new List<Guid>(KBObjectHelper.SpecifiableTypes);

            // If all generator are of the same type (win / web), don't follow other generator object types
            IEnumerable<UIType> uiTypes = Generators.Select(x => x.UIType).Distinct();
            if(uiTypes.Count() == 1)
            {
                UIType uiType = uiTypes.First();
                if (uiType == UIType.Web)
                    objectTypes.Remove(ObjClass.WorkPanel);
                else if (uiType == UIType.Win)
                    objectTypes.Remove(ObjClass.WebPanel);
            }

            return objectTypes;
        }
    }
}
