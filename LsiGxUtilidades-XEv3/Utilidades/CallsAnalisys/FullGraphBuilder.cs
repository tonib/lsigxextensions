using System;
using System.Collections.Generic;
using System.Linq;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Udm.Framework.References;
using LSI.Packages.Extensiones.Utilidades.Reflection;
using Artech.Architecture.Common.Objects;
using LSI.Packages.Extensiones.Utilidades.UI;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.CallsAnalisys
{
    
    /// <summary>
    /// Tool to create a full graph of the kbase references
    /// </summary>
    public class FullGraphBuilder
    {

        /// <summary>
        /// Graph to build
        /// </summary>
        private ObjectsGraph Graph;

        /// <summary>
        /// Owner search user interface. It can be null.
        /// </summary>
        public UISearchBase SearchUI;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graph">Graph to build</param>
        public FullGraphBuilder(ObjectsGraph graph)
        {
            Graph = graph;
        }

        /// <summary>
        /// Create the full graph of callable objects on the kbase
        /// </summary>
        public void BuildGraph()
        {
            List<Guid> callableTypes = ObjClassLsi.GetCallableTypes();

            foreach (EntityReference current in
                Graph.Model.GetReferences(LinkType.UsedObject, callableTypes, callableTypes))
            {
                Graph.AddNode(current.From);
                Graph.AddVertice(current);

                if (SearchUI != null)
                {
                    if (SearchUI.SearchCanceled)
                        return;
                    SearchUI.IncreaseSearchedObjects();
                }
            }
        }

    }
}
