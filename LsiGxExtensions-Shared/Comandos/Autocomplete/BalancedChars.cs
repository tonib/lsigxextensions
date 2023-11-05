using ActiproSoftware.SyntaxEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete
{
    /// <summary>
    /// Auto-close parenthesis and quotes
    /// </summary>
    class BalancedChars
    {
        /// <summary>
        /// Key has been pressed on the text editor
        /// </summary>
        /// <param name="syntaxEditor">Text editor</param>
        /// <param name="e">Key pressed event</param>
        /// <returns>True if some action has been done for balaced chars</returns>
        static public bool KeyTyping(SyntaxEditor syntaxEditor, KeyTypingEventArgs e)
        {
            if (!LsiExtensionsConfiguration.Load().AutocloseParenthesis)
                return false;

            if (CheckOverwriteCloseChar(syntaxEditor, e))
                return true;

            return CheckDeleteOpenChar(syntaxEditor, e);
        }

        private static bool CheckDeleteOpenChar(SyntaxEditor syntaxEditor, KeyTypingEventArgs e)
        {
            if (e.KeyData != Keys.Back)
                return false;

            int caretOffSet = syntaxEditor.Caret.Offset;
            if (caretOffSet <= 0 || caretOffSet >= (syntaxEditor.Document.Length-1))
                return false;

            char previousChar = syntaxEditor.Document[caretOffSet-1];
            char nextChar = syntaxEditor.Document[caretOffSet];
            if( (previousChar == '(' && nextChar == ')') || (previousChar == '"' && nextChar == '"') ||
                (previousChar == '\'' && nextChar == '\'') )
            {
                syntaxEditor.Document.DeleteText(DocumentModificationType.Custom, caretOffSet, 1);
                return true;
            }

            return false;
        }

        static private bool CheckOverwriteCloseChar(SyntaxEditor syntaxEditor, KeyTypingEventArgs e)
        {
            if (e.Overwrite)
                return false;
            if (syntaxEditor.Caret.Offset >= syntaxEditor.Document.Length)
                return false;

			if (syntaxEditor.SelectedView.Selection.Length > 0)
				return false;

            char currentChar = syntaxEditor.Document[syntaxEditor.Caret.Offset];
            if (!(currentChar == '(' || currentChar == ')' || currentChar == '\'' || currentChar == '"'))
                return false;

            if (e.KeyChar != currentChar)
                return false;

            e.Cancel = true;
            syntaxEditor.Caret.Offset++;
            return true;
        }

        static private bool LineIsBalanced(SyntaxEditor syntaxEditor, char openChar, char closechar)
        {
            DocumentLine line = syntaxEditor.Document.Lines[syntaxEditor.Caret.DocumentPosition.Line];
            if (line == null)
                return false;

            if (openChar == closechar)
                // This function is called after the open char is typed: So now, it should be unbalanced
                return line.Text.Count(x => x == openChar) % 2 != 0;
            else
            {
                int startCharCount = line.Text.Count(x => x == openChar);
                // This function is called after the open char is typed, so ignore this new char:
                startCharCount--;
                return startCharCount == line.Text.Count(x => x == closechar);
            }
        }

        static private bool CheckAutocloseChar(SyntaxEditor syntaxEditor, KeyTypedEventArgs e, char openChar, char closechar)
        {
            if (e.KeyChar != openChar)
                return false;

            if (!LineIsBalanced(syntaxEditor, openChar, closechar))
                return false;

			// If cursor is at word start, do not autoclose (probably user wants to wrap the code
			if (syntaxEditor.Caret.Offset < syntaxEditor.Document.Length)
			{
				char currentChar = syntaxEditor.Document[syntaxEditor.Caret.Offset];
				if (char.IsLetter(currentChar) || char.IsNumber(currentChar) || currentChar == '&')
					return false;
			}

			// Auto-close
			syntaxEditor.SelectedView.InsertSurroundingText(string.Empty, new string(closechar, 1));
            return true;
        }

        /// <summary>
        /// Key has been typed on the text editor
        /// </summary>
        /// <param name="syntaxEditor">Text editor</param>
        /// <param name="e">Key pressed event</param>
        /// <returns>True if some action has been done for balaced chars</returns>
        static public bool KeyTyped(SyntaxEditor syntaxEditor, KeyTypedEventArgs e)
        {
            if (!LsiExtensionsConfiguration.Load().AutocloseParenthesis)
                return false;
            
            
            if (CheckAutocloseChar(syntaxEditor, e, '(', ')'))
                return true;
            if (CheckAutocloseChar(syntaxEditor, e, '\'', '\''))
                return true;
            if (CheckAutocloseChar(syntaxEditor, e, '"', '"'))
                return true;

            return false;
        }
    }
}
