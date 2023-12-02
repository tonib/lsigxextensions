using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common.Parts.WebForm;
using LSI.Packages.Extensiones.Utilidades.WinForms;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Patterns.WorkWithDevices.Objects;
using Artech.Packages.Patterns.Objects;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using Artech.Genexus.Common.Controls;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.ObjectsInfoCache
{
    /// <summary>
    /// Stores object control names. Key is the control name, lowercase
    /// </summary>
    class ObjectControls : GenericTrie<char, ControlNameInfo>
    {

        private void SetupSDPanel(SDPanel sdPanel)
        {
            // Traverse controls
            foreach (PatternInstanceElement element in sdPanel.PatternPart.PanelElement.LsiEnumerateDescendants())
            {
                string controlName = element.Attributes["ControlName"] as string;
                if (string.IsNullOrEmpty(controlName))
                    continue;

                Add(controlName.ToLower(), new ControlNameInfo(element, controlName));
            }

            // Add extra "Form" control:
            Add("form", new ControlNameInfo("Form", "table"));

        }

		private void EnumerateWebControls(WebFormPart webForm)
		{
			// WebFormHelper.EnumerateWebTag is amazingly slow (10 seconds in Gx15 with an abstract layout with 200 controls)
			// So, get controls with FormHelper (better performance)
			IEnumerable<IGxControl> controls = FormHelper.GetControls(webForm.KBObject);
			foreach (IGxControl control in controls)
			{
				if(control?.Name != null)
					Add(control.Name.ToLower(), new ControlNameInfo(control));
			}

			/*foreach (IWebTag tag in WebFormHelper.EnumerateWebTag(webForm))
			{
				// Sometimes this gives a null reference exception:
				if (tag == null || tag.Properties == null)
					continue;

				string ctlName = ControlNameInfo.GetControlName(tag);
				if (ctlName != null)
					Add(ctlName.ToLower(), new ControlNameInfo(tag, ctlName));
			}*/
		}

		public ObjectControls(KBObject o)
        {
            SDPanel sdPanel = o as SDPanel;
            if(sdPanel != null)
            {
                SetupSDPanel(sdPanel);
                return;
            }

            // Transactions will have Win AND Web forms
            WinFormPart winForm = o.Parts.Get<WinFormPart>();
            WebFormPart webForm = o.Parts.Get<WebFormPart>();
            if (winForm == null && webForm == null)
                return;

            // TODO: Order is important here. Last generator will have priority (it will overwrite previous
            // TODO: control definitions). To do it right, we should check if there is some win / web
            // TODO: generator. If not, ignore the missed generator form 

            if (webForm != null)
				EnumerateWebControls(webForm);

			if (winForm != null)
            {
                // This is damn slow, that's why names are cached
                foreach (FormElement e in EnumeradorWinform.EnumerarControles(winForm))
                {
                    // Got NullReferenceException's here. I don't know where or when, so to be sure:
                    if (e == null || e.ControlName == null)
                        continue;
                    Add(e.ControlName.ToLower(), new ControlNameInfo(e));
                }
            }

            // Add extra "Form" control:
            Add("form", new ControlNameInfo("Form", "Form"));

            // Check WP Menubar property
            // TODO: I think there was a property member called "IsControl" or something like that?
            // TODO: If it's like that, check all object properties
            if (winForm != null)
            {
                KBObjectReference menu = o.GetPropertyValue<KBObjectReference>(Properties.WKP.Menubar);
                if (menu != null)
                    Add("menubar", new ControlNameInfo("Menubar", "Menubar"));
            }
        }
    }
}
