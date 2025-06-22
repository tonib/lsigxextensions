using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Editors;
using Artech.Common.Framework.Commands;
using Artech.Common.Framework.Selection;
using Artech.FrameworkDE.Text;
using Artech.Patterns.WorkWithDevices.Editor;
using Artech.Patterns.WorkWithDevices.Editor.Virtuals;
using System.Collections.Generic;
using System.Linq;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{
    /// <summary>
    /// CommandData extensions
    /// </summary>
    static public class CommandDataExtensions
    {
        /// <summary>
        /// Get the selected code on the active editor
        /// </summary>
        /// <param name="commandData">Command context (the text editor)</param>
        /// <returns>The selected code. null if no code was selected</returns>
        static public string LsiGetSelectedText(this CommandData commandData)
        {
            try
            {
                StandaloneTextEditor editor = commandData.LsiGetCurrentTextEditor();
                if (editor == null)
                    return null;

                string code = editor.SelectedText;
                if (string.IsNullOrEmpty(code))
                    return null;
                // Return null for empty text 
                if (string.IsNullOrEmpty(code.Trim()))
                    return null;
                return Entorno.StringFormatoKbase(code);
            }
            catch
            {
                return null;
            }
        }

        static public StandaloneTextEditor LsiGetCurrentTextEditor(this CommandData commandData)
        {
            // Get the selected text
            StandaloneTextEditor editor = commandData.Context as StandaloneTextEditor;
            if (editor == null)
            {
                // Try for sd editors:
                if (!(commandData.Context is VirtualEventsEditor ||
                    commandData.Context is VirtualConditionsEditor ||
                    commandData.Context is VirtualRulesEditor))
                    return null;

                BaseEditorContainer virtualEditor = commandData.Context as BaseEditorContainer;
                if (virtualEditor == null)
                    return null;
                SourceEditor eventsEditor = virtualEditor.GetEditor(null) as SourceEditor;
                if (eventsEditor == null)
                    return null;
                editor = eventsEditor.GetEditor(null) as StandaloneTextEditor;
            }

            return editor;
        }

        /// <summary>
        /// Get the selected objects on the current context (current MDI window?)
        /// </summary>
        /// <typeparam name="T">The type of the objects to be selected</typeparam>
        /// <param name="commandData">Command data sent by Genexus</param>
        /// <returns>The set of selected objects</returns>
        static public IEnumerable<T> LsiSelectedObjects<T>(this CommandData commandData)
        {
            ISelectionContainer selectionContainer = commandData.Context as ISelectionContainer;
            if (selectionContainer == null || selectionContainer.Count <= 0)
                return Enumerable.Empty<T>();
            if (selectionContainer.SelectedObjects == null)
                return Enumerable.Empty<T>();
            return selectionContainer.SelectedObjects.OfType<T>();
        }
    }
}
