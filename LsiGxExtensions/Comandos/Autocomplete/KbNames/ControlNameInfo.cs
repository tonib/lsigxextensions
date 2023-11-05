using Artech.Architecture.UI.Framework.Language;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Controls;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common.Parts.WebForm;
using Artech.Packages.Patterns.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Artech.Genexus.Common.Properties;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames
{
    /// <summary>
    /// Form control name info
    /// </summary>
    public class ControlNameInfo : ObjectNameInfo
    {

        /// <summary>
        /// Control type description
        /// </summary>
        public string ControlType;

        /// <summary>
        /// Create name info from WinForm control
        /// </summary>
        /// <param name="formElement">Form control</param>
        public ControlNameInfo(FormElement formElement) : base(formElement.Name, formElement.Type.ToString(), 
            ChoiceInfo.ChoiceType.Control)
        {
            ControlType = formElement.Type.ToString();
        }

        /// <summary>
        /// Create name info from WebForm control
        /// </summary>
        /// <param name="formElement">Form control</param>
        /// <param name="ctlName">Control name (see ControlNameInfo.GetControlName)</param>
        public ControlNameInfo(IWebTag formElement, string ctlName) : base(ctlName, formElement.Type.ToString(),
            ChoiceInfo.ChoiceType.Control)
        {
            ControlType = formElement.Type.ToString();
        }

		/// <summary>
		/// Create name info from IGxControl
		/// </summary>
		/// <param name="formElement">Form control</param>
		/// <param name="ctlName">Control name (see ControlNameInfo.GetControlName)</param>
		public ControlNameInfo(IGxControl formElement) : base(formElement.Name, formElement.Type.ToString(),
			ChoiceInfo.ChoiceType.Control)
		{
			ControlType = formElement.Type.ToString();
		}

		/// <summary>
		/// Create name info from SDPanel control
		/// </summary>
		/// <param name="element">Form control</param>
		/// <param name="ctlName">Control name</param>
		public ControlNameInfo(PatternInstanceElement element, string ctlName) : base(ctlName, ctlName, 
            ChoiceInfo.ChoiceType.Control)
        {
			ControlType = string.Empty;
			if (element.ChildSpecification != null && element.ChildSpecification.Name != null)
			{
				// This is the best I got. User controls (and others standard as SD Chronometer) gives a value that I don't know where it comes
				int? controlType = element.Attributes.GetPropertyValue(Properties.SDTITEM.ControlType) as int?;
				if (controlType != null && Enum.IsDefined(typeof(SDTITEM.ControlType_Enum), controlType))
					ControlType = ((SDTITEM.ControlType_Enum)controlType).ToString();
				else
					ControlType = element.ChildSpecification.Name;
			}
        }

        /// <summary>
        /// Create a generic control type
        /// </summary>
        /// <param name="controlName">Control name</param>
		/// <param name="controlType">Control type name</param>
        public ControlNameInfo(string controlName, string controlType) : 
            base(controlName, controlType, ChoiceInfo.ChoiceType.Control)
        {
            ControlType = controlType;
        }

        /// <summary>
        /// Get control name from WebForm control
        /// </summary>
        /// <param name="tag">WebForm control</param>
        /// <returns>Control name. null if it was not found</returns>
        static public string GetControlName(IWebTag tag)
        {
            string ctlName = tag.Properties.GetPropertyValue<string>(Properties.HTMLATT.ControlName);
            if (ctlName == null)
                // Brff... Control name can be "ControlName" or "id"... TODO: More?
                ctlName = tag.Properties.GetPropertyValue<string>(Properties.HTMLSPAN.ControlName);
            return ctlName;
        }


    }
}
