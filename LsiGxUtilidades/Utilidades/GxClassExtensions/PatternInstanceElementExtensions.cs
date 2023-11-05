using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Language.Parser.Data;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common.CustomTypes;
using Artech.Common.Properties;
using Artech.Genexus.Common.Parts.WebForm;
using Artech.Genexus.Common;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Parts.Layout;
using Artech.Genexus.Common.Types;
using System.ComponentModel;
using Artech.Packages.Patterns.Objects;
using Artech.Patterns.WorkWithDevices;
using Artech.Packages.Patterns.Specification;
using Artech.Patterns.WorkWithDevices.Parts;
using Artech.Patterns.WorkWithDevices.Objects;
using Artech.Architecture.Language.Parser;
using Artech.Genexus.Common.Services;
using Artech.Patterns.WorkWithDevices.Custom;
using Artech.Packages.Patterns.Custom;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{
    /// <summary>
    /// PatternInstanceElement class extensions
    /// </summary>
    static public class PatternInstanceElementExtensions
    {

        /// <summary>
        /// Enumerate the descendants of this pattern instance element
        /// </summary>
        /// <param name="element">The root element to traverse</param>
        /// <returns>All descendants of the element</returns>
        static public IEnumerable<PatternInstanceElement> LsiEnumerateDescendants(this PatternInstanceElement element)
        {
            // Revisar los hijos de este contenedor:
            foreach (PatternInstanceElement child in element.Children)
            {
                yield return (child);
                foreach (PatternInstanceElement descendant in LsiEnumerateDescendants(child))
                    yield return descendant;
            }
        }

        /// <summary>
        /// Get the reference to a SDT field of this pattern instance element
        /// </summary>
        /// <param name="element">This pattern instance element</param>
        /// <returns>The reference to the SDT field, with the base (ex. "&ampvariable.member").
        /// It returns an empty string if no reference was found.</returns>
        static public string LsiGetFullFieldSpecifier(this PatternInstanceElement element)
        {
            // Get the property value
            StringValue stringCode = 
                element.Attributes.GetPropertyValue("fieldSpecifier") as StringValue;
            if (stringCode == null)
                return string.Empty;

            // Get the indirection
            LayoutDataItemFieldSpecifierTypeConverter converter = new LayoutDataItemFieldSpecifierTypeConverter();
            string field = converter.ToString(element, new SpecificationAttribute(), CustomTypeConversion.UserInterface, stringCode);
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            // Get the base variable:
            AttributeVariableReference reference =
                element.Attributes.GetPropertyValue("attribute") as AttributeVariableReference;
            if (reference == null)
                return string.Empty;

            return reference.Name + "." + field;
        }
    }
}
