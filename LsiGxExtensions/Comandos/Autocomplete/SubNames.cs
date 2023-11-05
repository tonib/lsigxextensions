using ActiproSoftware.SyntaxEditor;
using Artech.Architecture.UI.Framework.Language;
using Artech.FrameworkDE.Text;
using Artech.Genexus.UI.Common.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete
{
    /// <summary>
    /// Suggest SUB names
    /// </summary>
    class SubNames
    {
        public static void SuggestSubIdentifiers(AutocompleteContext context, BaseSyntaxEditor syntaxEditor)
        {
            try
            {
                // Get the Gx editor:
                GxEventEditor gxEditor = GetGxEditorFromSyntaxEditor(syntaxEditor);
                if (gxEditor == null)
                    return;

				if (syntaxEditor.IntelliPrompt.MemberList.Visible)
					return;

                // There is a member that returns the SUB names, but is protected... ¯\_(ツ)_/¯
                MethodInfo getSubs = gxEditor.GetType().GetMethod("GetDefinedSubroutines", BindingFlags.NonPublic | BindingFlags.Instance);
                if (getSubs == null)
                    return;
				// It does not work in Ev3, but it does in Gx16 (probably a gx error, sub names even don't appear in Gx UI events selector)
                IEnumerable<TextRange> subNamesRanges = getSubs.Invoke(gxEditor, new object[] { }) as IEnumerable<TextRange>;
                if (subNamesRanges == null || subNamesRanges.Count() == 0)
                    return;

                // Get the current prefix. We cannot use the context prefix because it cares only about code tokens, and this will be a string 
                // literal token
                string currentPrefix = GetCurrentTokenPrefix(syntaxEditor);

                // Convert text ranges to sub names
                List<string> subNames = subNamesRanges
                    .Select(r => {
                        string subName = syntaxEditor.Document.GetSubstring(r);
                        // Remove quotes
                        return subName.Substring(1, subName.Length - 2);
                    })
                    .ToList();

				if (subNames.Count == 0)
					return;

                subNames.Sort();

                context.MemberList.Clear();
                foreach (string subName in subNames)
                    context.MemberList.Add(new AutocompleteItem(context.MemberList, subName, ChoiceInfo.ChoiceType.Procedure));
                context.MemberList.Show(syntaxEditor.Caret.Offset - currentPrefix.Length, currentPrefix.Length);
			}
            catch
            {
                // This is an ugly hack. If it fails, do not report it
            }
        }

        private static string GetCurrentTokenPrefix(BaseSyntaxEditor syntaxEditor)
        {
            TextStream textStream = syntaxEditor.Document.GetTextStream(syntaxEditor.Caret.Offset);
            // Current token will be the string end:
            if (!textStream.GoToPreviousToken())
                return string.Empty;

            IToken currentToken = textStream.Token;
            if (currentToken.Key == TokenKeys.StringStartToken || currentToken.Key == TokenKeys.CharacterStartToken)
                // Empty string
                return string.Empty;

            // Calculate prefix
            int currentTokenOffset = currentToken.StartOffset;
            int length = syntaxEditor.Caret.Offset - currentToken.StartOffset;
            return syntaxEditor.Document.GetSubstring(currentToken.StartOffset, length);
        }

        private static GxEventEditor GetGxEditorFromSyntaxEditor(BaseSyntaxEditor syntaxEditor)
        {
            Control c = syntaxEditor;
            while (c != null && c.Parent != null)
            {
                c = c.Parent;
                GxEventEditor editor = c as GxEventEditor;
                if (editor != null)
                    return editor;
            }
            return null;
        }
    }
}
