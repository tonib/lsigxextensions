using ActiproSoftware.SyntaxEditor;
using Artech.Architecture.Common.Objects;
using Artech.FrameworkDE.Text;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames.Sdts;
using LSI.Packages.Extensiones.Comandos.Autocomplete.ObjectsInfoCache;
using LSI.Packages.Extensiones.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor
{
	/// <summary>
	/// Extract info about PEM components datatype
	/// </summary>
	class PemTokenizer
	{

		class PemToken
		{
			public IToken Token;
			public SdtNodeNameInfo NameInfo;
		}

		VariablesPart VariablesPart;
		ObjectNamesCache ObjectNamesCache;
		KBModel Model;

		/// <summary>
		/// Cache for already classified tokens. Key = token start, Value = info about token
		/// </summary>
		Dictionary<int, PemToken> ParsedTokensCache = new Dictionary<int, PemToken>();

		/// <summary>
		/// Create a PEM tokenizer. As this caches already parsed expressiones MUST NOT be used with different object parts,
		/// or with parts that have been modified
		/// </summary>
		public PemTokenizer(ObjectNamesCache objectNamesCache, VariablesPart variablesPart, KBModel model)
		{
			VariablesPart = variablesPart;
			ObjectNamesCache = objectNamesCache;
			Model = model;
		}

		/// <summary>
		/// Get info about a As this caches already parsed expressiones MUST NOT be used with different object parts,
		/// or with parts that have been modified
		/// </summary>
		/// <param name="stream">Source stream</param>
		/// <param name="token">Token </param>
		/// <param name="finalMemberWord"></param>
		/// <returns></returns>
		public SdtNodeNameInfo GetTokenPemNameInfo(TextStream stream, IToken token, string finalMemberWord = null)
		{
			// Check if token is in cache
			PemToken cachedToken;
			if (ParsedTokensCache.TryGetValue(token.StartOffset, out cachedToken))
				return cachedToken.NameInfo;

			int offsetBackup = stream.Offset;
			try
			{
				// Move to the token to get
				stream.Offset = token.StartOffset;

				// PEM should be a chain of TOKEN.TOKEN.[...].TOKEN. Here, token could include parenthesis (ex. "Item(expression)" )

				// Get all tokens in chain
				List<PemToken> tokensChain = GetPemTokensChain(stream);
				if(tokensChain != null)
				{
					// Store info in cache
					tokensChain.ForEach(t => ParsedTokensCache[t.Token.StartOffset] = t);
				}
				// Return token where cursor was initially located
				return tokensChain?.FirstOrDefault(t => t.Token.StartOffset == token.StartOffset)?.NameInfo;
			}
			finally
			{
				stream.Offset = offsetBackup;
			}
		}

		List<PemToken> GetPemTokensChain(TextStream stream)
		{
			int startOffset = stream.Offset;
			MoveStreamToPemChainStartToken(stream);

			if (stream.Token?.Key != TokenKeys.VariableToken)
				// Only variable PEMs are supported
				return null;

			// Get variable type:
			string variableName = stream.Document.GetSubstring(stream.Token.TextRange);
			VariableNameInfo nameInfo = ObjectContextCache.GetVariableFromCache(VariablesPart, variableName);
			if (nameInfo == null)
				return null;

			// Currently only SDTs are supported
			if (nameInfo.DataType.Type != Artech.Genexus.Common.eDBType.GX_SDT || nameInfo.DataType.ExtendedType == null)
				return null;

			// SDT name could be a nested SDT type, ex "SDT.Level". 
			SdtNodeNameInfo sdtLevel = ObjectNamesCache.SdtStructuresCache.GetNodeInfo(Model, nameInfo.DataType.ExtendedType);
			if (sdtLevel == null)
				return null;

			// Add root token
			List<PemToken> chain = new List<PemToken>();
			chain.Add(new PemToken
			{
				NameInfo = sdtLevel,
				Token = stream.Token
			});

			while(true)
			{
				// Move to next point
				if (!MoveToNextTokenAndCheckIsPoint(stream))
					break;

				// Move to the next chain token
				if (!stream.LsiGoToNextCodeToken())
					break;

				// Special case: Support for "token.CurrentItem."
				if (IsIdentifier(stream.Token, stream, KeywordGx.CURRENT_ITEM))
				{
					// Same type as parent, but is not a collection. Keep parsing the chain
					// TODO: This is inconsistent but to fix it could be complicated: CurrentItem is NOT a collection, but it will be feeded to the model as a collection...
					chain.Add(new PemToken
					{
						NameInfo = sdtLevel,
						Token = stream.Token
					});
					continue;
				}

				// Special case: Support for "token.Item( xxx )" expressions
				if (IsIdentifier(stream.Token, stream, KeywordGx.ITEM))
				{
					// TOOD: Check here base token is really a collection

					// Move to the parenthesis start in ".Item("
					if (!stream.LsiGoToNextCodeToken())
						break;
					if (stream.Token.Key != TokenKeys.OpenParenthesisToken)
						break;

					// Move to the matching close parenthesis
					if (!MoveToCloseParenthesisForward(stream))
						break;

					// Add the close parenthesis with its base type. Neede to keep things consistent: Before a point there is shomething with type
					chain.Add(new PemToken
					{
						NameInfo = sdtLevel,
						Token = stream.Token
					});

					// Go to bucle start
					continue;
				}

				// Get new sdt level info
				sdtLevel = sdtLevel.GetChildAnyCase(stream.Document.GetSubstring(stream.Token.TextRange), Model, ObjectNamesCache);
				if (sdtLevel == null)
					break;
				chain.Add(new PemToken
				{
					NameInfo = sdtLevel,
					Token = stream.Token
				});
			}

			return chain;
		}
		
		bool MoveToNextTokenAndCheckIsPoint(TextStream stream)
		{
			// Move to next point
			if (!stream.LsiGoToNextCodeToken())
				return false;
			if (!IsPoint(stream.Token, stream))
				return false;
			return true;
		}

		/// <summary>
		/// Moves the stream cursor to the starto of the PEM expression
		/// </summary>
		/// <param name="stream">Stream, located at the token to resolve</param>
		void MoveStreamToPemChainStartToken(TextStream stream)
		{
			while(true)
			{
				if (stream.Token?.Key == TokenKeys.VariableToken)
					// Root node should be a variable
					return;

				// Check if it's a "Item( xxx )" expression
				if (stream.Token?.Key == TokenKeys.CloseParenthesisToken)
				{
					if (!MoveToOpenParenthesisBackwards(stream))
						// No matching open parenthesis has been found
						return;
					if (!stream.LsiGoToPreviousCodeToken())
						// Move to the caller name for parenthesis (ex. "caller(...)")
						return;
					if (!IsIdentifier(stream.Token, stream, KeywordGx.ITEM))
						return;
				}

				// Check if previous token is a point
				IToken previous = stream.LsiPeekCodeTokenReverse();
				if (previous == null)
					return;
				if (!IsPoint(previous, stream))
					return;

				// Move to the previous point
				stream.Offset = previous.StartOffset;

				// Skip the point, move to the previous token
				if (!stream.LsiGoToPreviousCodeToken())
					return;

			}
		}

		/// <summary>
		/// Moves the stream to the matching open parenthesis
		/// </summary>
		/// <param name="stream">Stream, located at the close parenthesis to match</param>
		/// <returns>True if the open parenthesis has been found</returns>
		bool MoveToOpenParenthesisBackwards(TextStream stream)
		{
			return MoveToMatchingParenthesis(stream, (str) => str.LsiGoToPreviousCodeToken(), TokenKeys.CloseParenthesisToken, TokenKeys.OpenParenthesisToken);
		}

		/// <summary>
		/// Moves the stream to the matching close parenthesis
		/// </summary>
		/// <param name="stream">Stream, located at the close parenthesis to match</param>
		/// <returns>True if the close parenthesis has been found</returns>
		bool MoveToCloseParenthesisForward(TextStream stream)
		{
			return MoveToMatchingParenthesis(stream, (str) => str.LsiGoToNextCodeToken(), TokenKeys.OpenParenthesisToken, TokenKeys.CloseParenthesisToken);
		}

		bool MoveToMatchingParenthesis(TextStream stream, Func<TextStream, bool> moveCursor, string increaseCountTokenKey, string decreaseCountTokenKey)
		{
			int openParenthesisCount = 1;
			while (openParenthesisCount != 0)
			{
				if (!moveCursor(stream))
					return false;

				// If we have found a block delimiter, stop search
				if (TokenKeysEx.BlockDelimiterTokens.Contains(stream.Token.Key))
					return false;

				if (stream.Token.Key == increaseCountTokenKey)
					openParenthesisCount++;
				else if (stream.Token.Key == decreaseCountTokenKey)
					openParenthesisCount--;
			}

			return true;
		}

		static bool IsPoint(IToken token, TextStream stream) { return token.Key == TokenKeys.PunctuationToken && stream.Document.GetSubstring(token.TextRange) == "."; }

		static bool IsIdentifier(IToken token, TextStream stream, string identifier)
		{
			return stream.Token.Key == TokenKeys.IdentifierToken && stream.Document.GetSubstring(stream.Token.TextRange).ToLower() == identifier;
		}
	}
}
