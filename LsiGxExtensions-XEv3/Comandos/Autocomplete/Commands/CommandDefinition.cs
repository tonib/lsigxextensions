using ActiproSoftware.SyntaxEditor;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.Commands
{

    /// <summary>
    /// Commands autocomplete definition
    /// </summary>
    public class CommandDefinition
    {

        /// <summary>
        /// Allowed scopes for this command. If empty, it's allowed anywere
        /// </summary>
        internal List<AllowedScope> Scopes = new List<AllowedScope>();

        /// <summary>
        /// Command start keywords, uppercase
        /// </summary>
        internal string[] StartKeywords;

        /// <summary>
        /// Keyword to close the block, uppercase. If the command is not a block, it will be null
        /// </summary>
        internal string BlockCloseKeyword;

        /// <summary>
        /// Scope identifier for this command block
        /// </summary>
        private string ScopeId;

        /// <summary>
        /// Text to insert before the cursor. null = no text
        /// </summary>
        public string PreText;

        /// <summary>
        /// Text to insert after the cursor. null = no text
        /// </summary>
        public string PostText;

        /// <summary>
        /// Command should be inserted at line start
        /// </summary>
        public bool OnlyAtLineStart = true;

        /// <summary>
        /// If false, autocomplete wil be disabled for the entire line after this command ("JAVA", "CSHARP"...)
        /// </summary>
        public bool AutocompleteAfterThisCommand = true;

        public int DefaultPriority = 0;

        public bool AllowChangeCase = true;

        public CommandDefinition(string keywordStartBlock, string keywordEndBlock, string scopeId)
        {
            StartKeywords = keywordStartBlock.Split(new char[] { ' ' });
            BlockCloseKeyword = keywordEndBlock;
            ScopeId = scopeId;
        }

        public CommandDefinition(string keywordStartBlock)
        {
            StartKeywords = keywordStartBlock.Split(new char[] { ' ' });
        }

        private string IndentationSpaces(LineParser lineParser)
        {
            int nSpaces = lineParser.IndentAmount;
            return new string(' ', nSpaces);
        }

        private string GetAutocompleteStartText(LineParser lineParser, out int priority)
        {
            priority = DefaultPriority;

            // Check if the start can be completed 
            if (lineParser.LineCompleteWords.Count >= StartKeywords.Length)
                return null;

            // Check if already inserted words are compatible
            for (int i = 0; i < lineParser.LineCompleteWords.Count; i++)
            {
                if (lineParser.LineCompleteWords[i].ToUpper() != StartKeywords[i].ToUpper())
                    return null;
            }

            // Get the start to suggest
            string startText = string.Join(" ", StartKeywords, lineParser.LineCompleteWords.Count,
                StartKeywords.Length - lineParser.LineCompleteWords.Count);
            priority += lineParser.LineCompleteWords.Count;

            return startText;
        }

        private string[] GetFirstStartWords(int nWords)
        {
            string[] words = new string[nWords];
            Array.Copy(StartKeywords, words, nWords);
            return words;
        }

        private string GetAutocompleteStartTextFromLineEnd(LineParser lineParser, out int priority)
        {
            priority = DefaultPriority;
            // Check previously completed words, sorting from more completed words to less
            for ( int nCompletedWords=(StartKeywords.Length-1); nCompletedWords>=0; nCompletedWords--)
            {
                string[] firstWords = GetFirstStartWords(nCompletedWords);
                if (lineParser.MatchFinalCompletedWords(firstWords) )
                {
                    // Got max of completed words. Suggest final words
                    priority += nCompletedWords;
                    string startText = string.Join(" ", StartKeywords, nCompletedWords, StartKeywords.Length - nCompletedWords);
                    return startText;
                }
            }
            return null;
        }

        private string GetAutocloseBlockText(LineParser lineParser)
        {
            if (BlockCloseKeyword == null)
                return null;
            
            // Do not re-close the block
            if (lineParser.TokenAfterCaret != null && lineParser.TokenAfterCaret.ToLower() == BlockCloseKeyword.ToLower() &&
                lineParser.IndentAmountNextLine == lineParser.IndentAmount)
                // Block already closed
                return null;

            if (lineParser.MoreTokensAfterCaret || lineParser.CurrentTokenPostfix.Length > 0)
                // There are more tokens after the cursor on the same line, do not autoclose (ex "IF| &boolean")
                return null;
            else
            {
                // There is no more lines, or next token is on the next line
                if (lineParser.IndentAmountNextLine > lineParser.IndentAmount)
                    // Next line is inside a nested block, no don't close block
                    return null;
            }

            return Environment.NewLine + IndentationSpaces(lineParser) + BlockCloseKeyword;
        }

        private void AddStartWords(LineParser lineParser, List<AutocompleteItem> words)
        {
            // Get the start to suggest
            string startText;
            int priority;
            if( OnlyAtLineStart)
                startText = GetAutocompleteStartText(lineParser, out priority);
            else
                startText = GetAutocompleteStartTextFromLineEnd(lineParser, out priority);
            if (string.IsNullOrEmpty(startText))
                return;

            // Check if the start text is compatible with the current word at caret
            if (!startText.ToLower().StartsWith(lineParser.CurrentTokenPrefix.ToLower()))
                return;

            AutocompleteItem item = new AutocompleteItem(startText);
            item.Priority = priority;

            // Usually words that can appear anywhere should not appear at line start
            // Right for procedure/event parts, wrong for conditions
            //if (!OnlyAtLineStart && lineParser.CompletedTokensCount == 0)
            //    item.Priority--;

            // Calculate posttext
            string postText = string.Empty;
            if (!string.IsNullOrEmpty(PostText))
                postText = PostText;
            string autoCloseBlockText = GetAutocloseBlockText(lineParser);
            if (!string.IsNullOrEmpty(autoCloseBlockText))
                postText += autoCloseBlockText;

            if (!string.IsNullOrEmpty(postText))
                item.AutoCompletePostText = postText;

            if (!string.IsNullOrEmpty(PreText))
                item.AutoCompletePreText = startText + PreText
                    .Replace(Environment.NewLine, Environment.NewLine + IndentationSpaces(lineParser))
                    .Replace("\t", "    ");

            words.Add(item);
        }

        private bool CheckAllowedOnCurrentScope(AutocompleteContext context)
        {
            if (Scopes.Count == 0)
                // No scope restrictions
                return true;

            foreach (AllowedScope scope in Scopes)
            {
                if (scope.Allowed(context))
                    return true;
            }
            return false;
        }

        private void AddCloseKeyword(LineParser lineParser, OutlineState outlineState, 
            List<AutocompleteItem> currentWords, List<AutocompleteItem> newWords)
        {
            if (BlockCloseKeyword == null)
                return;

            // Check the current scope
            // Allow to close, even if it's aready closed. Needed if you want to split an existing "IF" 
            //if (outlineState.CurrentScopeClosed)
            //    return;
            if (outlineState.CurrentScope != ScopeId)
                return;

            string currentPrefixLower = lineParser.CurrentTokenPrefix.ToLower();
            if (BlockCloseKeyword.ToLower().StartsWith(currentPrefixLower))
            {
                // Do not repeat single end blocks ("END" for "DO CASE" and "DO WHILE" case)
                if (!currentWords.Any(x => x.Text.ToLower() == BlockCloseKeyword.ToLower()))
                    newWords.Add(new AutocompleteItem(BlockCloseKeyword));
            }
        }

        private void WordsToUppercase(List<AutocompleteItem> words)
        {
            foreach(AutocompleteItem word in words)
            {
                word.Text = word.Text.ToUpper();
                if (!string.IsNullOrEmpty(word.AutoCompletePostText))
                    word.AutoCompletePostText = word.AutoCompletePostText.ToUpper();
                if (!string.IsNullOrEmpty(word.AutoCompletePreText))
                    word.AutoCompletePreText = word.AutoCompletePreText.ToUpper();
            }
        }

        /// <summary>
        /// Add autocomplete items for Gx commands
        /// </summary>
        /// <param name="context">Current autocomplete context</param>
        /// <param name="words">Autocomplete items where to add command words</param>
        internal void AddWords(AutocompleteContext context, List<AutocompleteItem> words)
        {
            List<AutocompleteItem> newWords = new List<AutocompleteItem>(10);

            if (CheckAllowedOnCurrentScope(context)) 
                AddStartWords(context.LineParser, newWords);

            // 1) Add close block keywords only if there are no words after cursor on current line
            // This is to avoid this: Escribir "EmpCod", 2) Edit: "IF|EmpCod", 3) Press space: It adds "ENDIFEmpCod" (wrong)
            // 2) WRONG: This is for add words to the list, not to do the autoclose. So, add the word
            //  if(!context.LineParser.MoreTokensAfterCaret)
            AddCloseKeyword(context.LineParser, context.OutlineState, words, newWords);

            if (AllowChangeCase && LsiExtensionsConfiguration.Load().UppercaseKeywords)
                WordsToUppercase(newWords);

            words.AddRange(newWords);
        }

        internal CommandDefinition AddScope(string blockId, bool allowInNestedBlocks = false)
        {
            Scopes.Add(new AllowedScope(blockId, allowInNestedBlocks));
            return this;
        }

        internal CommandDefinition OnlyInParts(params ObjectPartType[] partTypes)
        {
            if (Scopes.Count == 0)
                Scopes.Add(new AllowedScope(partTypes));
            else
                Scopes.ForEach(x => { x.AllowedPartTypes = partTypes; });
            return this;
        }

		internal CommandDefinition OnlyInParts(params ObjectPartType[][] partTypes)
		{
			List<ObjectPartType> partsList = new List<ObjectPartType>();
			foreach (ObjectPartType[] parts in partTypes)
				partsList.AddRange(parts);
			OnlyInParts(partsList.ToArray());
			return this;
		}

	}
}
