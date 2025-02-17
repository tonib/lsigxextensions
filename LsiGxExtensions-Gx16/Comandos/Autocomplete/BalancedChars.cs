using ActiproSoftware.SyntaxEditor;
using Artech.FrameworkDE.Text;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor;
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

            if (CheckEnterOnCommentOrString(syntaxEditor, e))
                return true;

            return CheckDeleteOpenChar(syntaxEditor, e);
        }

        /// <summary>
        /// Checks if a Enter has been pressed in the middle of a string constant or a single line comment
        /// </summary>
        /// <param name="syntaxEditor"></param>
        /// <param name="e"></param>
        /// <returns></returns>
		private static bool CheckEnterOnCommentOrString(SyntaxEditor syntaxEditor, KeyTypingEventArgs e)
		{
            if (e.KeyData != Keys.Enter)
                return false;

            TextStream textStream = syntaxEditor.Document.GetTextStream(syntaxEditor.Caret.Offset);
			IToken token = textStream?.Token;
            if (token == null)
                return false;

            string currentTokenKey = token.Key;
            if(currentTokenKey == TokenKeysEx.CommentWhitespaceToken || currentTokenKey == TokenKeysEx.CommentWordToken)
			{
                // Enter inside a single line comment. Check is not at the line end
                int lengthToLineEnd = textStream.DocumentLine.EndOffset - syntaxEditor.Caret.Offset;
                string textToLineEnd = syntaxEditor.Document.GetSubstring(syntaxEditor.Caret.Offset, lengthToLineEnd);
                if(textToLineEnd.Trim().Length > 0)
				{
                    // Continue the comment line in a new line
                    e.Cancel = true;
                    syntaxEditor.SelectedView.InsertSurroundingText(Environment.NewLine +
                        new string(' ', textStream.DocumentLine.IndentAmount) + "// ", "");
                    return true;
				}
            }

            bool isStringConstant = false, isAtEnd = false;
            string endStringToken = null;
            string stringDelimiter = null;
            if(currentTokenKey == TokenKeysEx.CharacterWhitespaceToken || currentTokenKey == TokenKeysEx.CharacterWordToken || 
                currentTokenKey == TokenKeysEx.CharacterEndToken)
			{
                isStringConstant = true;
                stringDelimiter = "'";
                isAtEnd = currentTokenKey == TokenKeysEx.CharacterEndToken;
                endStringToken = TokenKeysEx.CharacterEndToken;
            }
            else if(currentTokenKey == TokenKeysEx.StringWordToken || currentTokenKey == TokenKeysEx.StringWhitespaceToken ||
                currentTokenKey == TokenKeysEx.StringEndToken)
			{
                isStringConstant = true;
                stringDelimiter = "\"";
                isAtEnd = currentTokenKey == TokenKeysEx.StringEndToken;
                endStringToken = TokenKeysEx.StringEndToken;
            }
            if(isStringConstant)
			{
                // Enter inside a string literal. Continue the literal on the next line
                int textLength;
                if (isAtEnd)
                    textLength = 0;
                else
                {
                    // Search next string end, only in the current line
                    int endOfLineOffset = textStream.DocumentLine.EndOffset;
                    while (textStream.Token != null && textStream.Token.Key != endStringToken && textStream.Token.StartOffset < endOfLineOffset)
                        textStream.GoToNextToken();
                    if (textStream.Token == null || textStream.Token.StartOffset >= endOfLineOffset)
                        return false;

                    textLength = textStream.Token.StartOffset - syntaxEditor.Caret.Offset;
                }

                string nextLineText = syntaxEditor.Document.GetSubstring(syntaxEditor.Caret.Offset, textLength);

                // Continue the comment line in a new line
                e.Cancel = true;
                syntaxEditor.SelectedView.InsertSurroundingText(stringDelimiter + " +" + 
                    Environment.NewLine +
                    new string(' ', textStream.DocumentLine.IndentAmount) + stringDelimiter, "");
                return true;
            }
            return false;
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
