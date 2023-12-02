using System.Collections.Generic;
using Artech.Architecture.Language.Parser.Data;
using Artech.Architecture.Language.Parser.Objects;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{
    /// <summary>
    /// IParserObjectBase extensions
    /// </summary>
    static public class IParserObjectBaseExtensions
    {
        /// <summary>
        /// Get the root nodes of the parsed code tree
        /// </summary>
        /// <param name="parsedCode">The parsed code tree</param>
        /// <returns>The tree root nodes</returns>
        static public List<ObjectBase> LsiGetRootNodes(this IParserObjectBase parsedCode)
        {
            List<ObjectBase> rootNodes = new List<ObjectBase>();
            if (parsedCode == null)
                return rootNodes;

            if (parsedCode is IParserObjectBaseCollection)
            {
                foreach (IParserObjectBase node in ((IParserObjectBaseCollection)parsedCode))
                    rootNodes.Add(node.Data);
            }
            else
            {
                ObjectBase o = parsedCode.Data;
                if (o != null)
                    rootNodes.Add(o);
            }

            return rootNodes;
        }
    }
}
