using Artech.Architecture.Common.Objects;
using Artech.Architecture.Language.Parser.Data;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Udm.Framework;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using System.Collections.Generic;

namespace LSI.Packages.Extensiones.Utilidades.CallsAnalisys
{
    /// <summary>
    /// Stores signatures of objects on the kbase. Needed because ICallableInfo.GetSignatures is pretty slow
    /// </summary>
    public class KbSignaturesCache
    {

        /// <summary>
        /// Stores info about an object signature
        /// </summary>
        class ObjectSignatureItem
        {
            /// <summary>
            /// The object version
            /// </summary>
            public int ObjectVersion;

            /// <summary>
            /// The object main signature
            /// </summary>
            public List<ParameterElement> MainSignature = new List<ParameterElement>();

            /// <summary>
            /// Parameters rule text
            /// </summary>
            internal string ParmRuleText;

        }

        /// <summary>
        /// The cache storage. The key is the object key, and the value the stored signature
        /// </summary>
        static private Dictionary<EntityKey, ObjectSignatureItem> Cache = new Dictionary<EntityKey, ObjectSignatureItem>();

        /// <summary>
        /// Get the main signature of a kb object
        /// </summary>
        /// <param name="o">The kb object</param>
        /// <returns>The object main signature</returns>
        static private List<ParameterElement> RetrieveMainSignature(ICallableInfo o)
        {
            List<ParameterElement> parameters = new List<ParameterElement>();

            // EvU3: There is a bug with GetSignatures() for some object types: Returned signature don't report
            // the parameter object (att/var) ¯\_(ツ)_/¯
            // TODO: What types ??? !!! Signatures reported here are not cached, and this is important for OpenObjectsPrefech class
            foreach (Parameter p in o.LsiGetParametersWithTypeInfo())
                parameters.Add(new ParameterElement(p));

            return parameters;
        }

        static private string GetParmRule(KBObject o)
        {
            RulesPart rules = o.Parts.LsiGet<RulesPart>();
            if (rules == null)
                return string.Empty;
            Rule parmRule = ReglaParm.ObtenerReglaParm(rules);
            if (parmRule == null)
                return string.Empty;
            return parmRule.ToString();
        }

        static private ObjectSignatureItem GetSignatureItem(KBObject o, ICallableInfo c)
        {
            ObjectSignatureItem item;
            if (!Cache.TryGetValue(o.Key, out item))
            {
                // Not stored object
                item = new ObjectSignatureItem();
                item.MainSignature = RetrieveMainSignature(c);
                item.ObjectVersion = o.VersionId;
                Cache.Add(o.Key, item);
            }

            if (item.ObjectVersion != o.VersionId)
            {
                // Object changed:
                item.MainSignature = RetrieveMainSignature(c);
                item.ObjectVersion = o.VersionId;
                item.ParmRuleText = null;
            }
            return item;
        }

        /// <summary>
        /// Get the main signature of a callable object
        /// </summary>
        /// <param name="c">The callable object</param>
        /// <returns>The object signature</returns>
        static public List<ParameterElement> GetMainSignature(ICallableInfo c)
        {
            // Only kbobjects are cached:
            KBObject o = c as KBObject;
            if (o == null)
                // TODO: Document what cases are ICallableInfo that is not a KBObject! (functions?)
                return RetrieveMainSignature(c);

            return GetSignatureItem(o, c).MainSignature;
        }

        static public string GetObjectParmRule(ICallableInfo c)
        {
            KBObject o = c as KBObject;
            if (o == null)
                return string.Empty;

            ObjectSignatureItem item = GetSignatureItem(o, c);
            if (item.ParmRuleText == null)
                item.ParmRuleText = GetParmRule(o);

            return item.ParmRuleText;
        }

        /// <summary>
        /// Returns true if object signature is already cached
        /// </summary>
        /// <param name="entityKey">Object key</param>
        /// <returns>True if object signature is already cached</returns>
        static public bool ObjectSignatureCached(EntityKey entityKey)
		{
            return Cache.ContainsKey(entityKey);
		}

        /// <summary>
        /// Clear the stored signatures
        /// </summary>
        static public void ClearCache()
        {
            Cache.Clear();
        }
    }
}
