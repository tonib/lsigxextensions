using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Editors;
using Artech.Architecture.UI.Framework.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Parts;
using Artech.Patterns.WorkWithDevices.Parts;
using Artech.Udm.Framework;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System.Collections.Generic;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.ObjectsInfoCache
{
    /// <summary>
    /// Cached object information for open objects. Needed to get an acceptable autocomplete peformance
    /// </summary>
    class ObjectContextCache
    {

        /// <summary>
        /// Object identifier
        /// </summary>
        public EntityKey ObjectId;

        /// <summary>
        /// Cache with currently found variables. Needed because get a variable type is dementially slow
        /// </summary>
        private Dictionary<string, VariableNameInfo> Variables = new Dictionary<string, VariableNameInfo>();

        /// <summary>
        /// Cache with win/web form control names
        /// </summary>
        public ObjectControls ControlNames = null;

        /// <summary>
        /// Current instance
        /// </summary>
        static private ObjectContextCache Instance;

        private ObjectContextCache() { }

        /// <summary>
        /// Returns the cache instance for the given object
        /// </summary>
        /// <param name="o">Kb object to get</param>
        /// <returns>Cached info about the given object</returns>
        static public ObjectContextCache GetObjectCache(KBObject o)
        {
            if(Instance == null || o.Key != Instance.ObjectId)
            {
                // Object changed
                Instance = new ObjectContextCache();
                Instance.ObjectId = o.Key;

				// Register events once
				// This Get<VariablesPart>() will not work for SDPanels, as they have a VirtualVariablesPart
				// We cannot use a LsiGet<VariablesPart>(), as it returns a fake part
				VariablesPart variables = o.Parts.Get<VariablesPart>();
                if(variables != null)
                {
                    variables.AfterModified -= Variables_AfterModified;
                    variables.AfterModified += Variables_AfterModified;
                }
				else
				{
					VirtualVariablesPart sdVariables = o.Parts.Get<VirtualVariablesPart>();
					if(sdVariables != null)
					{
						// Ev3U3: This does NOT work for SDPanels (event dont fire). So, there will not be cache for SDPanels ¯\_(ツ)_/¯ (SLOW)
						// TODO: Check in later gx versions...
						sdVariables.AfterModified -= Variables_AfterModified;
						sdVariables.AfterModified += Variables_AfterModified;
					}
				}

                WebFormPart webForm = o.Parts.Get<WebFormPart>();
                if(webForm != null)
                {
                    webForm.AfterModified -= WebForm_AfterModified;
                    webForm.AfterModified += WebForm_AfterModified;
                }
				
                // Do not remove this comments:
                // Ev3U3: This do NOT work (event is not called...). ALSO is not called for SDPanels form
                // This is the reason of RegisterEventHandlers() existence
                //WinFormPart winForm = o.Parts.Get<WinFormPart>();
                //if(winForm != null)
                //{
                //    winForm.AfterModified -= WinForm_AfterModified;
                //    winForm.AfterModified += WinForm_AfterModified;
                //}
            }

            if (Instance.ControlNames == null)
                Instance.ControlNames = new ObjectControls(o);

            return Instance;
        }

        private static void SDForm_AfterModified(object sender, EntityEventArgs e)
        {
            ClearControlNames(sender as VirtualLayoutPart);
        }

        private static void WebForm_AfterModified(object sender, EntityEventArgs e)
        {
            ClearControlNames(sender as WebFormPart);
        }

        private static void ClearControlNames(KBObjectPart formPart)
        {
            if (formPart == null)
                // This should no happen
                return;

            if (Instance == null || formPart.KBObject.Key != Instance.ObjectId)
                return;
            Instance.ControlNames = null;
        }

        private static void Variables_AfterModified(object sender, EntityEventArgs e)
        {
            VariablesPart variables = sender as VariablesPart;
            if (variables == null)
                // This should no happen
                return;

            if (Instance == null || variables.KBObject.Key != Instance.ObjectId)
                return;

			ClearVariablesCache();
		}

		static public void ClearVariablesCache()
		{
			if(Instance != null)
				Instance.Variables.Clear();
		}

		/// <summary>
		/// Register events to keep object cache updated
		/// </summary>
		static public void RegisterEventHandlers()
        {
            UIServices.DocumentManager.DocumentPartEditorCreated += DocumentManager_DocumentPartEditorCreated;
        }

        /// <summary>
        /// Gx editor created
        /// </summary>
        private static void DocumentManager_DocumentPartEditorCreated(object sender, DocumentPartEventArgs e)
        {
            if (!(e.DocumentPart.Part is WinFormPart) && !(e.DocumentPart.Part is VirtualLayoutPart))
                return;

            BaseEditor editor = UIServices.Environment.ActiveView.ActiveView as BaseEditor;
            if( editor != null)
                editor.DocumentDataChanged += WinSDFormEditor_DocumentDataChanged;
        }

        /// <summary>
        /// Winform part modified
        /// </summary>
        /// <param name="sender">Winform part editor</param>
        /// <param name="data">No idea</param>
        private static void WinSDFormEditor_DocumentDataChanged(object sender, object data)
        {
            BaseEditor editor = sender as BaseEditor;
            if (editor == null)
                return;
            ClearControlNames(editor.Part);
        }

		/// <summary>
		/// Get information about a variable
		/// </summary>
		/// <param name="variables">Object variables part</param>
		/// <param name="variableName">Variable name, with or without ampersand, any case</param>
		/// <returns>Information about the variable. A default value (NUMERIC(4)) if variable don't exists</returns>
		static public VariableNameInfo GetVariableFromCache(VariablesPart variables, string variableName)
		{
			Dictionary<string, VariableNameInfo> variablesCache = GetObjectCache(variables.KBObject).Variables;

			// Format variable name
			if (variableName.Length > 0 && variableName[0] == '&')
				variableName = variableName.Substring(1);
			variableName = variableName.ToLower();

			VariableNameInfo vName;
			if (variablesCache.TryGetValue(variableName, out vName))
				return vName;

			Variable v = variables.GetVariable(variableName);
			if (v == null)
				// Undefined variable. Do not add to cache
				return new VariableNameInfo(variableName);
			else
				// This is DAMN slow
				vName = new VariableNameInfo(v);

			// See GetObjectCache(): This will not work. As a palliative, we will keep the cache, but it will be cleaned
			// each time a prediction begins. With this, if a variable appears more than once, it will be reused
			// on the same prediction ¯\_(ツ)_/¯
			variablesCache.Add(variableName, vName);

			return vName;
		}
		
	}
}
