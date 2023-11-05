using ActiproSoftware.SyntaxEditor;
using Artech.FrameworkDE.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete
{

    public class LineParser
    {

        /// <summary>
        /// Complete tokens on current line before the caret (only code tokens, no strings or comments)
        /// </summary>
        private List<IToken> LineCompleteTokens = new List<IToken>(30);

        /// <summary>
        /// Next complete token after caret. It can be on other next line. null if there is no more tokens
        /// </summary>
        public string TokenAfterCaret { get; private set; }

        /// <summary>
        /// True if there are more tokens on the current line after the caret
        /// </summary>
        public bool MoreTokensAfterCaret { get; private set; }

        /// <summary>
        /// The text editor
        /// </summary>
        public SyntaxEditor SyntaxEditor { get; private set; }

        /// <summary>
        /// Current line indentation in characters count
        /// </summary>
        public int IndentAmount { get; private set; }

        /// <summary>
        /// Line start offset
        /// </summary>
        public int LineStartOffset { get; private set;  }

        /// <summary>
        /// Next line indentation in characters count. Zero if there is no next line
        /// </summary>
        public int IndentAmountNextLine { get; private set; }

        /// <summary>
        /// The text on the current token, before the caret (original case). Empty string if none
        /// </summary>
        public string CurrentTokenPrefix { get; private set; }

        /// <summary>
        /// The text on the current token, before the caret. Empty string if none
        /// </summary>
        public string CurrentTokenPostfix { get; private set;  }

        /// <summary>
        /// The current token start offset. -1 if none
        /// </summary>
        public int CurrentTokenOffset { get; private set; }

        /// <summary>
        /// Number of complete tokens before the caret. If th caret is over a token, this will not be counted
        /// </summary>
        public int CompletedTokensCount { get { return LineCompleteTokens.Count; } }

        /// <summary>
        /// Complete WORDS (not tokens) before the caret
        /// </summary>
        public List<string> LineCompleteWords { get; private set; }

        /// <summary>
        /// Parse the current line
        /// </summary>
        /// <param name="syntaxEditor">The text editor</param>
        public LineParser(SyntaxEditor syntaxEditor)
        {
            SyntaxEditor = syntaxEditor;
            TextStream textStream = syntaxEditor.Document.GetTextStream(syntaxEditor.Caret.Offset);

            DocumentLine currentLine = syntaxEditor.SelectedView.CurrentDocumentLine;
            IndentAmount = currentLine.IndentAmount;
            LineStartOffset = currentLine.StartOffset;

            // Current token:
            AddTokenAtStart(textStream.Token);
            IToken token = textStream.ReadTokenReverse();

            // Previous line tokens
            while (token != null && !textStream.IsAtDocumentLineEnd)
            {
                AddTokenAtStart(token);
                token = textStream.ReadTokenReverse();
            }

            // Text on the current token before the caret
            CalculateCurrentTokenPrefix();

            // Token after caret
            textStream.Offset = syntaxEditor.Caret.Offset;
            int currentDocLineIdx = textStream.DocumentLineIndex;
            if (textStream.GoToNextNonWhitespaceOrCommentToken() && textStream.Token != null)
            {
                //TokenAfterCaret = SyntaxEditor.Document.GetSubstring(textStream.Token.TextRange);
                TokenAfterCaret = textStream.TokenText;
                MoreTokensAfterCaret = (textStream.DocumentLineIndex == currentDocLineIdx);

                if( !MoreTokensAfterCaret)
                    // We are on the next line
                    IndentAmountNextLine = textStream.DocumentLine.IndentAmount;
            }

            // Words before the caret
            LineCompleteWords = new List<string>(LineCompleteTokens.Count + 4);
            char[] space = { ' ' };
            foreach(IToken t in LineCompleteTokens)
            {
                string[] words = SyntaxEditor.Document.GetTokenText(t).Split(space, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                    LineCompleteWords.Add(word);
            }
        }

        /// <summary>
        /// If the cursor is over a token, it divides the text token before and after the caret
        /// </summary>
        private void CalculateCurrentTokenPrefix()
        {
            CurrentTokenPrefix = string.Empty;
            CurrentTokenPostfix = string.Empty;
            CurrentTokenOffset = -1;

            if (LineCompleteTokens.Count == 0)
                return;

            // Right now LineCompleteTokens contains non empty tokens before, or over, the caret. 
            // Check if the caret is on the last token
            IToken lastToken = LineCompleteTokens.Last();
            //if (!lastToken.Contains(SyntaxEditor.Caret.Offset)) < don't work if caret is at the word end
            if (SyntaxEditor.Caret.Offset <= lastToken.TextRange.StartOffset || SyntaxEditor.Caret.Offset > lastToken.TextRange.EndOffset)
                return;

            // Calculate prefix
            CurrentTokenOffset = lastToken.StartOffset;
            int length = SyntaxEditor.Caret.Offset - lastToken.StartOffset;
            CurrentTokenPrefix = SyntaxEditor.Document.GetSubstring(lastToken.StartOffset, length);

            // Calculate postfix
            length = lastToken.EndOffset - SyntaxEditor.Caret.Offset;
            if(length > 0)
                CurrentTokenPostfix = SyntaxEditor.Document.GetSubstring(SyntaxEditor.Caret.Offset, length);

            // LineCompleteTokens should contain only complete tokens. Remove the token over the cursor, it's incomplete
            LineCompleteTokens.Remove(lastToken);

        }

        public bool MatchFinalCompletedWords(string[] words)
        {
            if (words.Length == 0)
                // Nothing to match
                return true;

            if (CompletedTokensCount < words.Length)
                // No words enought
                return false;

            // Index for the first completed word to check
            int idxCompleteWordBase = CompletedTokensCount - words.Length;

            for ( int i=0; i< words.Length; i++)
            {
                string completedWord = GetCompletedTextToken(idxCompleteWordBase + i);
                if (completedWord.ToLower() != words[i].ToLower())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// The text of the last completed token before the caret, original case. Empty string if there are no previous tokens
        /// </summary>
        public string TokenBeforeCaret { get { return GetCompletedTokenTextBackwards(0); } }

        /// <summary>
        /// Returns the text of the i-th completed token on the current line, relative to the line start
        /// </summary>
        /// <param name="idx">Token index. 0 = first complete token of line</param>
        /// <returns>The token text. Empty string if index is not in completed text tokens range. IT CAN CONTAIN MORE THAN ONE WORD (ex. "FOR EACH")</returns>
        public string GetCompletedTextToken(int idx)
        {
            IToken token = GetCompletedToken(idx);
            return token != null ? SyntaxEditor.Document.GetTokenText(LineCompleteTokens[idx]) : string.Empty;
        }

        /// <summary>
        /// Returns the i-th completed token on the current line, relative to the line start
        /// </summary>
        /// <param name="idx">Token index. 0 = first complete token of line</param>
        /// <returns>The token. Null if index is not in completed text tokens range</returns>
        public IToken GetCompletedToken(int idx)
        {
            if (idx < 0 || idx >= LineCompleteTokens.Count)
                return null;
            return LineCompleteTokens[idx];
        }

        /// <summary>
        /// Returns the text of the i-th completed token on the current line, relative to the line end
        /// </summary>
        /// <param name="idx">Token index. 0 = last complete token of line</param>
        /// <returns>The token text. Empty string if index is not in completed text tokens range. IT CAN CONTAIN MORE THAN ONE WORD (ex. "FOR EACH")</returns>
        public string GetCompletedTokenTextBackwards(int idx)
		{
			return GetCompletedTextToken(LineCompleteTokens.Count - 1 - idx);
		}

        /// <summary>
        /// Returns the i-th completed token on the current line, relative to the line end
        /// </summary>
        /// <param name="idx">Token index. 0 = last complete token of line</param>
        /// <returns>The token. Null if index is not in completed text tokens range</returns>
        public IToken GetCompletedTokenBackwards(int idx)
        {
            return GetCompletedToken(LineCompleteTokens.Count - 1 - idx);
        }

        private void AddTokenAtStart(IToken token)
        {
            if (!token.LsiIsCode())
                return;

            // Ignore tokens starting after the caret (they are included if they are just after the caret 
            // (ex : "foo|)", parenthesis will be added )
            if (token.TextRange.StartOffset >= SyntaxEditor.Caret.Offset)
                return;

            LineCompleteTokens.Insert(0, token);
        }

        public override string ToString()
        {
            string text = string.Empty;
            foreach (IToken token in LineCompleteTokens)
                text += token.Key + ": \"" + SyntaxEditor.Document.GetSubstring(token.TextRange) + "\"" + Environment.NewLine;
            return text;
        }

    }
}
