using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Patterns.WorkWithDevices.Parts;
using System.Collections.Generic;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{

    /// <summary>
    /// KBObjectPart extensions
    /// </summary>
    static public class KBObjectPartExtensions
    {
        
        /// <summary>
        /// True if the part is the main source part of the object (events or procedure part)
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <returns>True if the part is the main source of the object</returns>
        static public bool LsiIsMainSource(this KBObjectPart part)
        {
            return part is EventsPart || part is ProcedurePart || part is VirtualEventsPart;
        }

        static public bool LsiIsConditionsSource(this KBObjectPart part)
        {
            return part is ConditionsPart || part is VirtualConditionsPart;
        }

        /// <summary>
        /// Get standard event names for an EventsPart or VirtualEventsPart
        /// </summary>
        /// <param name="part">Part to check</param>
        /// <returns>Event names. Empty list if part is not an Events part</returns>
        static public IEnumerable<string> LsiGetStandardEventsNames(this KBObjectPart part)
        {
            EventsPart events = part as EventsPart;
            if (events != null)
                return events.GetStandardEventsNames();

            // SD panels don't have a GetStandardEventsNames() function, so they are unknow...  ¯\_(ツ)_/¯
            VirtualEventsPart virtualEvents = part as VirtualEventsPart;
            if(virtualEvents != null)
                return new string[] { "ClientStart" , "Back" , "Load" , "Refresh" , "Start" };

            return new string[] { };
        }

    }
}
