using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Collections;
using Artech.Patterns.WorkWithDevices.Objects;
using Artech.Patterns.WorkWithDevices.Parts;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Objects;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{
    /// <summary>
    /// Extensions of KBPartCollection class
    /// </summary>
    static public class KBObjectPartCollectionExtensions
    {

        /// <summary>
        /// Get an object part. If the object is a SD panel or a DataSelector, a fake part will be returned (fuck)
        /// </summary>
        /// <typeparam name="TPart"></typeparam>
        /// <returns></returns>
        static public TPart LsiGet<TPart>(this KBObjectPartCollection parts) where TPart : KBObjectPart
        {
            SDPanel sd = parts.Object as SDPanel;
            if (sd != null)
            {
                // Fuck. Create a fake part for compatibility
                return sd.LsiGetPart<TPart>();
            }

            DataSelector ds = parts.Object as DataSelector;
            if( ds != null )
                // Create a fake part for compatibility
                return ds.LsiGetPart(typeof(TPart)) as TPart;

            return parts.Get<TPart>();
        }

        /// <summary>
        /// Convert a virtual object part to a "standard" one, non virtual 
        /// </summary>
        /// <param name="parts">Kb object parts</param>
        /// <param name="part">The part to convert</param>
        /// <returns>The standard equivalent part</returns>
        static public KBObjectPart LsiConvertPart(this KBObjectPartCollection parts, KBObjectPart part)
        {
            if (part == null)
                return null;
            SDPanel sd = part.KBObject as SDPanel;
            if (sd == null)
                return part;

            // Fuck: Get the equivalent part type
            Type newType;
            if (part is VirtualRulesPart)
                newType = typeof(RulesPart);
            else if (part is VirtualEventsPart)
                newType = typeof(EventsPart);
            else if (part is VirtualConditionsPart)
                newType = typeof(ConditionsPart);
            else
                // VirtualLayoutPart or unknown type
                return part;

            return sd.LsiGetPart(newType);
        }

        /// <summary>
        /// Get the source code main part for this object: The procedure part for procedures, or the
        /// events part for forms
        /// </summary>
        /// <param name="parts">The object parts</param>
        /// <returns>The main source part. null if no main source part was found on the object</returns>
        static public SourcePart LsiGetMainSoucePart(this KBObjectPartCollection parts)
        {
            SourcePart source = parts.LsiGet<ProcedurePart>() as SourcePart;
            if(source == null)
                source = parts.LsiGet<EventsPart>() as SourcePart;
            return source;
        }

        /// <summary>
        /// Enumerate the object parts, with compatibility with SDPanels (fuck)
        /// </summary>
        /// <returns>The object parts</returns>
        static public IEnumerable<KBObjectPart> LsiEnumerate(this KBObjectPartCollection parts)
        {
            IEnumerable<KBObjectPart> partsEnum;
            SDPanel sd = parts.Object as SDPanel;
            if (sd != null)
                partsEnum = sd.LsiEnumerateParts();
            else
            {
                DataSelector ds = parts.Object as DataSelector;
                if (ds != null)
                    partsEnum = ds.LsiEnumerateParts();
                else
                    partsEnum = parts;
            }
            foreach (KBObjectPart part in partsEnum)
                yield return part;
        }

        /// <summary>
        /// Update an object part, with compatibility with SDPanels (fuck)
        /// </summary>
        /// <param name="parts">The object parts</param>
        /// <param name="part">The part to update</param>
        static public void LsiUpdatePart(this KBObjectPartCollection parts, KBObjectPart part)
        {
            SDPanel sd = parts.Object as SDPanel;
            if (sd != null)
            {
                // Fuck
                sd.LsiUpdatePart(part);
                return;
            }

            DataSelector ds = parts.Object as DataSelector;
            if( ds != null )
                ds.LsiUpdatePart(part);
        }

    }
}
