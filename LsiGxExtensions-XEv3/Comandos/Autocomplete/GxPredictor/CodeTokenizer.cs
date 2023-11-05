using ActiproSoftware.SyntaxEditor;
using Artech.Architecture.Common.Objects;
using Artech.FrameworkDE.Text;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.ModelGeneration;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using LSI.Packages.Extensiones.Comandos.Autocomplete.ObjectsInfoCache;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Artech.Architecture.UI.Framework.Language.ChoiceInfo;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Comandos.Autocomplete.Calls;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using System;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames.Sdts;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor
{
	/// <summary>
	/// Tool to extract tokens info from Genexus code editor document
	/// </summary>
    public class CodeTokenizer
    {

        /// <summary>
        /// KB info
        /// </summary>
        public KbPredictorInfo KbInfo { get; protected set; }

        /// <summary>
        /// Stores raw conversion of Genexus token keys to prediction token keys
        /// </summary>
        static private Dictionary<string, WordTypeKey> TokenKeyToWordKey = null;

        /// <summary>
        /// Generate model toolwindow. It can be null
        /// </summary>
        private GenerateModelTW ToolWindow;

        /// <summary>
        /// Object part owner of the current code to tokenize
        /// </summary>
        public KBObjectPart CurrentPart { get; protected set; }

		/// <summary>
		/// Object part type owner of the current code to tokenize
		/// </summary>
		private ObjectPartType CurrentPartType;

        /// <summary>
        /// Variables of current object. It can be null
        /// </summary>
        private VariablesPart CurrentVariables;

		/// <summary>
		/// Tokenizer for PEM expressions
		/// </summary>
		PemTokenizer PemTokenizer;

		/// <summary>
		/// Create a Offline tokenizer
		/// </summary>
		/// <param name="kbInfo">Kb tokenization info</param>
		/// <param name="part">Code part where tokenize</param>
		/// <param name="toolWindow">Generate prediction model toolwindow</param>
		public CodeTokenizer(KbPredictorInfo kbInfo, KBObjectPart part, GenerateModelTW toolWindow = null)
        {
            KbInfo = kbInfo;
            ToolWindow = toolWindow;

            SetupTokenKeysDictionary();

			CurrentPart = part;
			CurrentVariables = part.KBObject.Parts.LsiGet<VariablesPart>();
			CurrentPartType = new ObjectPartType(part);

			PemTokenizer = new PemTokenizer(KbInfo.ObjectNames, CurrentVariables, CurrentPart.KBObject.Model);
		}

        /// <summary>
        /// Tokenize an entire object part
        /// </summary>
        /// <param name="part">Code part to tokenize</param>
        /// <param name="document">Procedure part code document</param>
        /// <returns>The tokenized part</returns>
        public TokenizedPart Tokenize(Document document)
        {
            TokenizedPart tokenP = new TokenizedPart(CurrentPart);

            // Get tokens
            TextStream textStream = document.GetTextStream(0);
            tokenP.Tokens = TokenizeForward(document, textStream);
			AddContextToSequence(document, tokenP.Tokens);

            return tokenP;
        }

		/// <summary>
		/// Check if token over the cursor should be included
		/// </summary>
		/// <param name="document">Code document</param>
		/// <param name="textStream">Code stream, located at token over the cursor</param>
		/// <param name="cursorOffset">Cursor position offet</param>
		/// <returns>True if the cursor over the token should be included. False if not, or cursor is not located over the current textStream token</returns>
		private bool ShouldIncludeLastToken(Document document, TextStream textStream, int cursorOffset)
		{
			// EXCEPTION: If the current token is a ".", it should be included 
			if (textStream.Token.EndOffset > cursorOffset)
				// Word ends after the cursor: Ignore it
				return false;

			// Current token ends on cursor position. Operators and symbols should be included (".", "+", , "(", etc)
			// TODO: Check if SyntaxEditor Token has info for this
			// TODO: This could be wrong. What if cursor is here?: "IF 1 >|=" (">" should be included...)
			if (textStream.Token.Length != 1)
				return false;

			char tokenChar = document[textStream.Token.StartOffset];
			// EXCEPTION: if "&" was typed, do not feed it to the model (part of token to predict: Some variable)
			if (tokenChar == '&' || char.IsLetterOrDigit(tokenChar))
				return false;

			return true;
		}

		/// <summary>
		/// Only for backwards tokenization. Removes tokens already included if current line starts with a native code command
		/// </summary>
		/// <param name="textStream">Current code stream, located over the native code command</param>
		/// <param name="tokens">Already tokenized code. All native commands will be removed</param>
		private void RemoveNativeCodeTokens(TextStream textStream, TokensList tokens)
		{
			int currentOffset = textStream.Offset;

			textStream.GoToCurrentDocumentLineEnd();
			// Keep the first token (the native code command). Remove others up to line end (we work with a reversed list!)
			while (tokens.Count >= 2 && tokens[tokens.Count-2].TokenOffset <= textStream.Offset)
				tokens.RemoveAt(tokens.Count - 2);

			textStream.Offset = currentOffset;
		}

		/// <summary>
		/// Get tokens info from document source backwards from cursor position, up to a given number of tokens
		/// </summary>
		/// <param name="document">Document to tokenize</param>
		/// <param name="textStream">The document text stream, initialized at cursor position</param>
		/// <param name="cursorOffset">Cursor position</param>
		/// <param name="nMaxTokens">Number of tokens to return</param>
		/// <returns>Extracted tokens</returns>
		private TokensList TokenizeBackwards(Document document, TextStream textStream, int cursorOffset, int nMaxTokens)
		{
			// We work with a reversed list here (index 0 = word nearest to the cursor)
			TokensList result = new TokensList(nMaxTokens + 128);

			// Initialize
			if (!textStream.Token.LsiIsCode(lineBreakIsCode: KbInfo.DataInfo.Compatibility.LineBreaksAsKeyword))
			{
				if (!textStream.LsiGoToPreviousCodeToken(lineBreakIsCode: KbInfo.DataInfo.Compatibility.LineBreaksAsKeyword))
					return result;
			}

			// Ignore tokens beginning after cursor. Ex "foo(&|)", here starting token is ")"
			while (textStream.Token.StartOffset >= cursorOffset)
			{
				if (!textStream.LsiGoToPreviousCodeToken(lineBreakIsCode: KbInfo.DataInfo.Compatibility.LineBreaksAsKeyword))
					return result;
			}

			// Check if token over cursor, if there is, should be included. Ex "&foo.|", here "." should be included
			if(textStream.Token.EndOffset >= cursorOffset && !ShouldIncludeLastToken(document, textStream, cursorOffset))
			{
				if (!textStream.LsiGoToPreviousCodeToken(lineBreakIsCode: KbInfo.DataInfo.Compatibility.LineBreaksAsKeyword))
					return result;
			}

			while (true)
			{
				// Check if we have finished. If we have reached nMaxTokens, we must continue to the line start because tokens
				// may be discarded by FixOldBooleanOps / RemoveNativeCodeTokens later
				if (textStream.IsCharacterLineTerminator && result.Count >= nMaxTokens)
					break;

				if (textStream.Token.LsiIsCode(lineBreakIsCode: KbInfo.DataInfo.Compatibility.LineBreaksAsKeyword))
				{
					string tokenText = AddTokensInfo(textStream.Token, textStream, result, false);
					if (tokenText != null)
					{
						// There is a problem: Old style boolean operators (".not.", ".or.", etc) are tokenized wrong (UnknownMember)
						// Fix it
						FixOldBooleanOps(result);

						if (KbInfo.NativeCodeCommands.Contains(tokenText))
							// Native code command found ("CSHARP", "JAVA", etc). Other tokens in this line must to be removed (they have aready added)
							RemoveNativeCodeTokens(textStream, result);
					}
				}

				if (!textStream.GoToPreviousToken())
					break;
			}

			// Remove not needed tokens
			int nToRemove = result.Count - nMaxTokens;
			if (nToRemove > 0)
				result.RemoveRange(nMaxTokens, nToRemove);

			// We were working with a inverse list. Return it with it's right ordering
			result.Reverse();

			return result;
		}

		/// <summary>
		/// Get tokens info from a full document source object part
		/// </summary>
		/// <param name="document">Document to tokenize</param>
		/// <param name="textStream">The document text stream, initialized at the part start</param>
		/// <returns>Extracted tokens</returns>
		private TokensList TokenizeForward(Document document, TextStream textStream)
        {
            TokensList result = new TokensList(16384);

            while (textStream.Token != null && !textStream.Token.IsDocumentEnd)
            {
				string tokenText = AddTokensInfo(textStream.Token, textStream, result, true);

                if (tokenText != null)
                {
                    // There is a problem: Old style boolean operators (".not.", ".or.", etc) are tokenized wrong (UnknownMember)
                    // Fix it
                    FixOldBooleanOps(result);

                    if (KbInfo.NativeCodeCommands.Contains(tokenText))
                        // Other tokens in this line are native code to ignore
                        textStream.GoToCurrentDocumentLineEnd();
                }

                if (!textStream.LsiGoToNextCodeToken(lineBreakIsCode: KbInfo.DataInfo.Compatibility.LineBreaksAsKeyword))
                    break;
            }
            return result;
        }

        /// <summary>
        /// Given the current editor caret position, it returns a tokens sequence with the length expected by the predictor
        /// </summary>
        /// <param name="context">Current autocomplete context (editing position)</param>
        /// <returns>A padded sequence</returns>
        public TokensList TokenizeCursorSequenceBackwards(AutocompleteContext context)
        {
            // Get the stream, located at the current caret position
            TextStream textStream = context.SyntaxEditor.Document.GetTextStream(context.SyntaxEditor.Caret.Offset);

			// Tokenize code backwards, up to KbInfo.DataInfo.SequenceLength tokens
			TokensList tokens = TokenizeBackwards(context.SyntaxEditor.Document, textStream, context.SyntaxEditor.Caret.Offset, KbInfo.DataInfo.SequenceLength);

			// Add context to tokens
			AddContextToSequence(context.SyntaxEditor.Document, tokens);

			return tokens;
        }

		/// <summary>
		/// Adds context info to all tokens in a given list
		/// </summary>
		/// <param name="document">Code document</param>
		/// <param name="tokens">Tokens to wich add context</param>
		private void AddContextToSequence(Document document, TokensList tokens)
		{
			if (tokens.Count == 0)
				return;

			ObjectPartType partType = new ObjectPartType(CurrentPart);

			// Resolve context is the most expensive operation, specially resolve call parameters. This is why is done like this
			CallInfoFinder callsFinder = new CallInfoFinder(document, KbInfo.ObjectNames);
			CallInfoTree callsTree = callsFinder.FindCallsInRange(tokens[0].TokenOffset, tokens.Last().TokenOffset);

			// TODO: context for non parameters could be two instances, one for variables and other for non variables, shared for all tokens
			// TODO: -> better performance (no new objects instances)
			foreach (TokenInfo token in tokens)
			{
				CallInfo callInfo = callsTree.GetCallForOffset(token.TokenOffset);
				ParameterElement parameterInfo = callInfo == null ? null : callInfo.GetParameterElement(token.TokenOffset);

				token.Context = new TokenContext(parameterInfo, partType, token.WordType);
			}
		}

        private void FixOldBooleanOps(List<TokenInfo> tokens)
        {
            int count = tokens.Count;
            if (count < 3)
                return;

            TokenInfo previous = tokens[count - 3], current = tokens[count - 2], next= tokens[count - 1];
            if (current.FixOldBoolOperator(previous, next))
            {
                // Has been fixed
                tokens.Remove(previous);
                tokens.Remove(next);
            }
        }

        private string AddTokensInfo(IToken token, TextStream stream, List<TokenInfo> currentTokens, bool forward)
        {
            string text = stream.Document.GetSubstring(token.TextRange);
            string textLowercase = text.ToLower();

            if (!token.LsiIsCode(lineBreakIsCode: KbInfo.DataInfo.Compatibility.LineBreaksAsKeyword))
                return null;

            // Get word type
            ObjectNameInfo nameInfo;
            WordTypeKey wordType = GetWordType(token, textLowercase, stream, out nameInfo);
			int offsetIncrement = 0;
			TokensList newTokens = new TokensList(4);
            foreach (string word in text.Split(new char[] { ' ' }))
            {
				// Text will be included in a CSV with semicolons: Replace them
				const string newLine = "[linebreak]";
				string normalizedWord = word
					.Replace(";", "[semicolon]")
					.Replace(Environment.NewLine, newLine)
					.Replace("\n", newLine);
				newTokens.Add(new TokenInfo(wordType, normalizedWord, token.Key, nameInfo, KbInfo, CurrentPart.KBObject, null, token.StartOffset + offsetIncrement));
				offsetIncrement += word.Length;
			}

			if (!forward)
				newTokens.Reverse();
			currentTokens.AddRange(newTokens);

            return textLowercase;
        }

        private void LogWarning(string message, IToken token, string tokenText, Document document)
        {
			DocumentPosition position = document.OffsetToPosition(token.StartOffset);
			LogWarning(message + ": " + token.Key + ", '" + tokenText + "', line " + (position.Line + 1) + ", character " + (position.Character + 1) + ", " + 
				CurrentPartType?.PartTypeName );
        }

		private void LogWarning(string message)
		{
			message = CurrentPart.KBObject.Name + ": " + message;

			if (ToolWindow == null)
			{
				// Prediction time (not training)
#if DEBUG
				// Print a warning: We must detect these !!!
				using (Log log = new Log(false))
				{
					log.Output.AddWarningLine(message);
				}
#endif
				return;
			}

			ToolWindow.Logger.AddWarningLine(message);
		}

		/// <summary>
		/// If one string resolves to multiple names, try to discard options
		/// </summary>
		/// <param name="nameInfos">Original names</param>
		/// <returns>Filtered options</returns>
		private List<ObjectNameInfo> ResolveAmbiguousNames(IToken token, TextStream stream, List<ObjectNameInfo> nameInfos)
		{
			// TODO: These are heuristics, unreliable, for our dataset
			// TODO: Check module names before the name
			List<ObjectNameInfo> result = new List<ObjectNameInfo>(nameInfos);

			// Remove functions not appliable on this object part
			foreach (FunctionNameInfo f in result.OfType<FunctionNameInfo>().Where(f => !f.CanBeUsed(CurrentPartType)).ToArray())
				result.Remove(f);

			// If we are in a call, discard non callable names. If we are not in a call, discard callable names
			bool keepCallables = CallMatcher.CheckNameCall(stream);
			foreach (ObjectNameInfo n in result.Where(n => n.IsCallable != keepCallables).ToArray())
				result.Remove(n);

			// Other heuristics
			List<IToken> nextTokens = stream.LsiPeekNextTokens(2);
			string[] nextTokensText = nextTokens.Select(t => stream.Document.GetTokenText(t)).ToArray();

			if(nextTokensText.Length >= 1)
			{
				if (nextTokensText[0] != "(")
				{
					// With functions, next token should be "("
					foreach (FunctionNameInfo f in result.OfType<FunctionNameInfo>().ToArray())
						result.Remove(f);
				}

				if (nextTokensText[0] != ".")
				{
					// Domains are only available in code if next token is "." (Enumerated domains)
					foreach (KbObjectNameInfo o in result.OfType<KbObjectNameInfo>().Where(o => o.Type == ChoiceType.Domain).ToArray())
						result.Remove(o);
				}
			}

			if (nextTokensText.Length >= 2 && nextTokensText[0] == "." && nextTokensText[1].ToLower() != KeywordGx.LINK)
			{
				// Ony availiable image method is ".Link()". Remove images:
				foreach (KbObjectNameInfo o in result.OfType<KbObjectNameInfo>().Where(o => o.Type == ChoiceType.Image).ToArray())
					result.Remove(o);
			}

			List<IToken> previousTokens = stream.LsiPeekPreviousTokens(2);
			string[] previousTokensText = previousTokens.Select(t => stream.Document.GetTokenText(t)).ToArray();

			if(previousTokensText.Length >= 2 && previousTokensText[0] == "(" && previousTokensText[1].ToLower() == "fromimage")
			{
				// It must to be an image
				foreach (ObjectNameInfo n in result.Where(n => !(n is KbObjectNameInfo) || ((KbObjectNameInfo)n).Type != ChoiceType.Image).ToArray())
					result.Remove(n);
			}

			// If still there are duplicates, give priorities:
			if(result.Count > 1)
			{
				result.Sort((x, y) => {
					return NamePriority(x) - NamePriority(y);
				});
			}

			return result;
		}

		static private int NamePriority(ObjectNameInfo name)
		{
			if (name is FunctionNameInfo)
				return 1;
			if (name is AttributeNameInfo)
				return 2;

			KbObjectNameInfo kbName = name as KbObjectNameInfo;
			if (kbName == null)
				return 3; // Other name types (there is any?)
			if (kbName.Type != ChoiceType.Domain && kbName.Type != ChoiceType.Image)
				return 4;
			if (kbName.Type == ChoiceType.Domain)
				return 5;
			if (kbName.Type == ChoiceType.Image)
				return 6;
			return 7; // Should not happens
		}

		private SdtNodeNameInfo GetPemNodeType(string tokenTextLowercase, ref ObjectNameInfo info, TextStream stream, IToken baseToken, IToken memberToken)
		{
			// Check if it's a SDT member name
			// Get sdt level type for the token, or the base token
			SdtNodeNameInfo pemType = PemTokenizer.GetTokenPemNameInfo(stream, memberToken != null ? memberToken : baseToken);
			if (pemType != null && memberToken == null)
				// We are typing a member, not getting a existing word type. So, get the word info
				pemType = pemType.GetChildLowercase(tokenTextLowercase, this.CurrentPart.Model, KbInfo.ObjectNames);
			return pemType;
		}

		/// <summary>
		/// Get a token type for a member word
		/// </summary>
		/// <param name="tokenTextLowercase">Member word, lowercase</param>
		/// <param name="info">If the word is an attribute name, this will return the attribute info</param>
		/// <param name="baseToken">Base token form member (expression baseToken.tokenTextLowercase). Null if there is no base token (ex. BOF.foo)</param>
		/// <returns>Token type (StandardMember, Attribute, or OtherMember)</returns>
		public WordTypeKey GetMemberType(string tokenTextLowercase, ref ObjectNameInfo info, TextStream stream, IToken baseToken, IToken memberToken = null)
        {
			if (baseToken == null)
				return WordTypeKey.OtherMember;

			if(KbInfo.DataInfo.Compatibility.SetSdtsDataType)
			{
				// Check if it's a SDT member name
				SdtNodeNameInfo pemType = GetPemNodeType(tokenTextLowercase, ref info, stream, baseToken, memberToken);
				if(pemType != null)
				{
					info = pemType;
					return WordTypeKey.OtherMember;
				}
			}

			// Check if it's a module reference
			string baseTokenTextLowercase = stream.Document.GetSubstring(baseToken.TextRange).ToLower();
			if (KbInfo.ObjectNames.GetAllByExactName(baseTokenTextLowercase).Where(x => x.Type == ChoiceType.Module).Any())
			{
				ObjectNameInfo kbName = KbInfo.ObjectNames.GetAllByExactName(tokenTextLowercase).Where(x => x is KbObjectNameInfo).FirstOrDefault();
				if (kbName != null)
				{
					info = kbName;
					return WordTypeKey.KbObject;
				}
			}

            if (KbInfo.StandardMemberNames.Contains(tokenTextLowercase))
                // Standard language member ("IndexOf", "IsEmpty", etc)
                return WordTypeKey.StandardMember;

            // Check if it's a name attribute
            info = KbInfo.ObjectNames.GetAttributeByExactName(tokenTextLowercase);
            if (info != null)
                return WordTypeKey.Attribute;

            return WordTypeKey.OtherMember;
        }

        private WordTypeKey GetWordType(IToken token, string tokenText, TextStream stream, out ObjectNameInfo nameInfo)
        {
            nameInfo = null;

            WordTypeKey wordType;
            if (!TokenKeyToWordKey.TryGetValue(token.Key, out wordType))
            {
                LogWarning("Unknown token type", token, tokenText, stream.Document);
                return WordTypeKey.UnknownIdentifier;
            }

			if (token.Key == TokenKeys.OpenRuleToken)
			{
				// This is special: can be a variable, attribute, control, or rule(function). Handle variables here:
				if (tokenText.StartsWith("&"))
					wordType = WordTypeKey.Variable;
			}

			if (wordType != WordTypeKey.UnknownIdentifier)
            {
				if (wordType == WordTypeKey.Variable)
				{
					if (CurrentVariables == null)
						throw new Exception("Got variable token but CurrentVariables is null");
					else
						nameInfo = ObjectContextCache.GetVariableFromCache(CurrentVariables, tokenText);
				}
                return wordType;
            }

            return GetWordTypeForIdentifier(token, tokenText, stream, out nameInfo);
        }

		/// <summary>
		/// Returns the word type for a GX token
		/// </summary>
		/// <param name="token">The GX token</param>
		/// <param name="tokenText">The token text, lowercase</param>
		/// <param name="currentTokens">Tokens list, previous to the "token" parameter</param>
		/// <param name="info">If the token matches a Kb name object, this returns the name details. Otherwise is null</param>
		/// <returns>The token type</returns>
		private WordTypeKey GetWordTypeForIdentifier(IToken token, string tokenText, TextStream stream, out ObjectNameInfo info)
		{
			info = null;

			IToken previousToken = stream.LsiPeekCodeTokenReverse();
			string previousTokenText = null;

			if (previousToken != null)
			{
				previousTokenText = stream.Document.GetSubstring(previousToken.TextRange);
				if (previousToken.Key == TokenKeys.PunctuationToken && previousTokenText == ".")
				{
					// Could be a member. If it's an old style operator (".AND. Round("), it's not a member
					IToken baseToken = stream.LsiPeekCodeTokenReverse(2);
					string baseTokenText = baseToken == null ? null : stream.Document.GetSubstring(baseToken.TextRange).ToLower();
					if (baseTokenText == null || !KeywordGx.BOOLEAN_OPERATORS.Contains(baseTokenText))
						// It's a member reference. Check it's real type
						return GetMemberType(tokenText, ref info, stream, baseToken);
				}
			}

			if (KbInfo.LanguageKeywords.Contains(tokenText))
				return WordTypeKey.Keyword;

			// Check if it's a callable object / attribute / function
			List<ObjectNameInfo> nameInfos = KbInfo.ObjectNames.GetAllByExactName(tokenText);
			if (nameInfos.Count > 1)
			{
				// There are ambiguous names. Try to remove unappliable
				nameInfos = ResolveAmbiguousNames(token, stream, nameInfos);
				if (ToolWindow != null)
				{
					// Print warnings only in train time
					if (nameInfos.Count > 1)
						LogWarning("Ambiguous token type: " + string.Join(", ", nameInfos.Select(n => n.Type.ToString()).ToArray()), token, tokenText, stream.Document);
					else if (nameInfos.Count == 0)
						LogWarning("Removed all tokens after resolving ambiguous tokens", token, tokenText, stream.Document);
				}
			}
			if (nameInfos.Count > 0)
			{
				info = nameInfos[0];
				if (info.Type == ChoiceType.Attribute)
					return WordTypeKey.Attribute;
				else if (info.Type == ChoiceType.Function)
					return WordTypeKey.Function;
				else if (info.Type == ChoiceType.NameSpace || info.Type == ChoiceType.None)
					// ChoiceType.None == Keyword. In this case, it should be a rule
					return WordTypeKey.Keyword;
				else
					return WordTypeKey.KbObject;
			}

			// Check if it's a UI control
			ObjectControls controlNames = ObjectContextCache.GetObjectCache(CurrentPart.KBObject).ControlNames;
			if (controlNames.GetValueByExactName(tokenText) != null)
				return WordTypeKey.Control;

			// In Ev3U3, in SDPanels, event names ClientStart and Back are not TokenKeys.EventNameToken  ¯\_(ツ)_/¯
			if (previousTokenText != null && previousTokenText.ToLower() == "event" && (tokenText == "clientstart" || tokenText == "back"))
				return WordTypeKey.Keyword;

			// Unknown token
			if (ToolWindow != null)
			{
				bool logWarning = true;

				if (logWarning && previousToken != null)
				{
					if (previousToken.Key == TokenKeys.KeyWordToken && previousTokenText.ToLower() == KeywordGx.PRINT)
						// Printblock identifiers are unknown. Don't show warnings for them
						logWarning = false;
					if (previousTokenText == ":")
						// ThemeClass:<classname>: Class names are unknown
						logWarning = false;
				}

				// Sdts can appear on procedures ("= new SDT()"). Don't show warnings for them
				if (logWarning && SDT.Get(CurrentPart.Model, new QualifiedName(tokenText)) != null)
					logWarning = false;

				// Dont' show warnings for [web] / [win] / [bc]
				if (logWarning && (tokenText == "web" || tokenText == "win" || tokenText == "bc"))
					logWarning = false;

				if (logWarning)
					LogWarning("Unknown identifier", token, tokenText, stream.Document);

			}
			return WordTypeKey.UnknownIdentifier;
		}

		private void SetupTokenKeysDictionary()
        {
			if (TokenKeyToWordKey != null)
				return;
			TokenKeyToWordKey = new Dictionary<string, WordTypeKey>();

			// TokenKeyToWordKey could be a static member, but
			// no performance issues doing this for each CodeTokenizer instance, as CodeTokenizer is instantiated a single time
			TokenKeyToWordKey.Add(TokenKeys.VariableToken, WordTypeKey.Variable);

            TokenKeyToWordKey.Add(TokenKeys.CharacterStartToken, WordTypeKey.StringConstant);
            TokenKeyToWordKey.Add(TokenKeys.StringStartToken, WordTypeKey.StringConstant);

            // Why was EventNameToken as string constant?
            //TokenKeyToWordKey.Add(TokenKeys.EventNameToken, WordTypeKey.StringConstant);
            TokenKeyToWordKey.Add(TokenKeys.EventNameToken, WordTypeKey.Keyword);

            TokenKeyToWordKey.Add(TokenKeysEx.IntegerNumberToken, WordTypeKey.IntegerConstant);
            TokenKeyToWordKey.Add(TokenKeysEx.HexIntegerNumberToken, WordTypeKey.IntegerConstant);

            TokenKeyToWordKey.Add(TokenKeysEx.RealNumberToken, WordTypeKey.DecimalConstant);

            TokenKeyToWordKey.Add(TokenKeysEx.OperatorToken, WordTypeKey.Keyword);
            TokenKeyToWordKey.Add(TokenKeysEx.DeprecatedWordToken, WordTypeKey.Keyword);
            TokenKeyToWordKey.Add(TokenKeysEx.NewOperatorToken, WordTypeKey.Keyword);

            // Identifiers (attribute names, object names) and functions are returned as unknown identifiers. 
            // They are resolved later in GetWordTypeForIdentifier with extra name info from the names tries
            // Identifiers (object names, function names)
            TokenKeyToWordKey.Add(TokenKeysEx.IdentifierToken, WordTypeKey.UnknownIdentifier);
            // Function names
            TokenKeyToWordKey.Add(TokenKeysEx.FcnWordToken, WordTypeKey.UnknownIdentifier);
            TokenKeyToWordKey.Add(TokenKeysEx.FcnNpWordToken, WordTypeKey.UnknownIdentifier);

			// Rules:
			TokenKeyToWordKey.Add(TokenKeysEx.RulesWordToken, WordTypeKey.Keyword);
			TokenKeyToWordKey.Add(TokenKeysEx.RuleTriggerEventToken, WordTypeKey.Keyword);
			TokenKeyToWordKey.Add(TokenKeysEx.NullToken, WordTypeKey.Keyword);
			TokenKeyToWordKey.Add(TokenKeys.OpenRuleToken, WordTypeKey.UnknownIdentifier); // This is special: can be a variable, attribute, control, or rule(function)

			// Line break is a keyword:
			TokenKeyToWordKey.Add(TokenKeys.LineTerminatorToken, WordTypeKey.Keyword);
			TokenKeyToWordKey.Add(TokenKeysEx.CommentEndToken, WordTypeKey.Keyword);

			// All other declared tokens in TokenKeys are keywords
			// https://stackoverflow.com/questions/10261824/how-can-i-get-all-constants-of-a-type-by-reflection
			List<string> tokenTypes =
                typeof(TokenKeys).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(x => (string)x.GetRawConstantValue())
                // Ignore already declared tokens
                .Where(x => !TokenKeyToWordKey.Keys.Contains(x))
                .ToList();
            foreach (string weywordType in tokenTypes)
                TokenKeyToWordKey.Add(weywordType, WordTypeKey.Keyword);
        }

    }
}
