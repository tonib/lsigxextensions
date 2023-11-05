using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Patterns.WorkWithDevices;
using Artech.Patterns.WorkWithDevices.Helpers;
using Artech.Patterns.WorkWithDevices.Objects;
using Artech.Patterns.WorkWithDevices.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{
    /// <summary>
    /// SDPanel class extensions
    /// </summary>
    static public class SDPanelExtensions
    {

        /// <summary>
        /// Get the right parameters for a SDPanels
        /// </summary>
        /// <param name="sdPanel">The SD Panel</param>
        /// <returns>The right parameters</returns>
        /// <remarks>
        /// EvU3: There is a bug with GetSignatures() for SDPanels: Returned signature don't report
        /// the parameter variable object ¯\_(ツ)_/¯
        /// </remarks>
        static public List<Parameter> LsiGetParameters(this SDPanel sdPanel)
        {
            List<Parameter> parameters = new List<Parameter>();

            Signature signature = sdPanel.GetSignatures().FirstOrDefault();
            if (signature == null)
                return parameters;

            VariablesPart variables = sdPanel.Parts.LsiGet<VariablesPart>();

            foreach (Parameter p in signature.Parameters)
            {
                if (p.Object != null)
                {
                    // Right: Parameter has typed object. In Ev3U3 this is not happening for variables
                    parameters.Add(p);
                    continue;
                }

                ITypedObject typedObject;
                if (p.IsAttribute)
                    typedObject = Artech.Genexus.Common.Objects.Attribute.Get(sdPanel.Model, p.Name);
                else
                    // Variable
                    typedObject = variables.GetVariable(p.Name);
                parameters.Add(new Parameter(typedObject, p.IsAttribute, p.Accessor));
            }

            return parameters;
        }

        /// <summary>
        /// Get a standard part from the SDPanel
        /// </summary>
        /// <param name="sd">The SDPanel</param>
        /// <param name="partType">The part type to get</param>
        /// <returns>The object part. null if the part cannot be converted</returns>
        public static KBObjectPart LsiGetPart(this SDPanel sd, Type partType)
        {
            if (partType == typeof(RulesPart))
                return AssignCode(new RulesPart(sd), sd, InstanceAttributes.Section.Rules);
            else if (partType == typeof(EventsPart))
                return AssignCode(new EventsPart(sd), sd, InstanceAttributes.Section.Events);
            else if (partType == typeof(VariablesPart))
                return WorkWithDevicesSources.GetVariablesPartForPanel(sd.PatternPart.PanelElement);
            else if (partType == typeof(VirtualLayoutPart))
                return sd.Parts.Get<VirtualLayoutPart>();
            else if (partType == typeof(ConditionsPart))
                return AssignCode(new ConditionsPart(sd), sd, InstanceAttributes.Section.Conditions);

            return null;
        }

        static public IEnumerable<KBObjectPart> LsiEnumerateParts(this SDPanel sd)
		{
            yield return sd.LsiGetPart<RulesPart>();
            yield return sd.LsiGetPart<EventsPart>();
            yield return sd.LsiGetPart<VariablesPart>();
            yield return sd.Parts.Get<VirtualLayoutPart>();
            yield return sd.LsiGetPart<ConditionsPart>();
        }

        /// <summary>
        /// Get a standard part from the SDPanel
        /// </summary>
        /// <typeparam name="T">The part type to get</typeparam>
        /// <param name="sd">The SDPanel</param>
        /// <returns>The object part. null if the part cannot be converted</returns>
        public static T LsiGetPart<T>(this SDPanel sd) where T : KBObjectPart
		{
            return (T) sd.LsiGetPart(typeof(T));
        }


        static public void LsiUpdatePart(this SDPanel sd, KBObjectPart part)
		{
            VariablesPart variables = part as VariablesPart;
            if (variables != null)
            {
                WorkWithDevicesSources.SetVariablesPartIfChangedForPanel(sd.PatternPart.PanelElement,
                    variables);
                return;
            }
            RulesPart rules = part as RulesPart;
            if (rules != null)
            {
                sd.PatternPart.PanelElement.Attributes.SetPropertyValue(
                    InstanceAttributes.Section.Rules, rules.Source);
                return;
            }
            EventsPart events = part as EventsPart;
            if (events != null)
            {
                sd.PatternPart.PanelElement.Attributes.SetPropertyValue(
                    InstanceAttributes.Section.Events, events.Source);
                return;
            }
            ConditionsPart conditions = part as ConditionsPart;
            if (conditions != null)
            {
                sd.PatternPart.PanelElement.Attributes.SetPropertyValue(
                    InstanceAttributes.Section.Conditions, conditions.Source);
                return;
            }
        }

        static private KBObjectPart AssignCode(ISource source, SDPanel sd, string sourceProperty)
        {
            source.Source = sd.PatternPart.PanelElement.Attributes
                .GetPropertyValueString(sourceProperty);
            return source as KBObjectPart;
        }
    }
}
