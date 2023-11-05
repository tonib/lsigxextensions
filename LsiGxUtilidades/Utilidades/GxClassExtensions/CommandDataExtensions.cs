using Artech.Architecture.UI.Framework.Editors;
using Artech.Common.Framework.Commands;
using Artech.FrameworkDE.Text;
using Artech.Patterns.WorkWithDevices.Editor;
using Artech.Patterns.WorkWithDevices.Editor.Virtuals;

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
    }
}
