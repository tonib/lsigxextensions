using ActiproSoftware.SyntaxEditor;
using Artech.FrameworkDE.Text;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete
{
    static class ITokenExtensions
    {
        static public bool LsiIsCode(this IToken token, bool lineBreakIsCode = false)
        {
			if(token == null)
				return false;

			if (lineBreakIsCode && (token.Key == TokenKeys.LineTerminatorToken || token.Key == TokenKeysEx.CommentEndToken))
				return true;

            if (token.IsWhitespace || token.IsComment)
                return false;

            // Multiline comments don't have .IsComment = true... Also, "MultiLineCommentWordToken" is not declared in TokenKeys
            if (token.Key == TokenKeys.MultiLineCommentStartToken || token.Key == TokenKeys.MultiLineCommentEndToken ||
                token.Key == TokenKeysEx.MultiLineCommentWordToken || // This is really needed?
                token.Key == TokenKeysEx.CommentStartToken
                )
                return false;

            if (token.Key == TokenKeysEx.DefaultToken)
                // This is "\" or "!"
                return false;

            // Ignore "comment words" and "literal string words"
            if (token.LexicalState.Key != StateKeys.DefaultState)
                return false;

            return true;
        }
    }

    /// <summary>
    /// Extensions for TextStream
    /// </summary>
    static class TextStreamExtensions
    {
        static public bool LsiGoToNextCodeToken(this TextStream stream, bool lineBreakIsCode = false)
        {
            while(true)
            {
				if (!stream.GoToNextToken())
					return false;
                if (stream.Token.LsiIsCode(lineBreakIsCode))
                    return true;
            }
        }

        static public bool LsiGoToPreviousCodeToken(this TextStream stream, bool lineBreakIsCode = false)
        {
            while (true)
            {
                if (!stream.GoToPreviousToken())
                    return false;
                if (stream.Token.LsiIsCode(lineBreakIsCode))
                    return true;
            }
        }

		/// <summary>
		/// Get previous token, without moving current stream position
		/// </summary>
		/// <param name="stream">Code stream</param>
		/// <param name="nTokensBackward">Number of tokens to back (positive number)</param>
		/// <returns>The token. Null if it was not found</returns>
		static public IToken LsiPeekCodeTokenReverse(this TextStream stream, int nTokensBackward=1)
		{
			int offsetBackup = stream.Offset;
			for (int i = 0; i < nTokensBackward; i++)
			{
				if (!stream.LsiGoToPreviousCodeToken())
					break;
			}
			IToken previousToken = stream.Token;
			stream.Offset = offsetBackup;
			return previousToken;
		}

		static public IToken LsiPeekNextCodeToken(this TextStream stream, int nTokensForward = 1)
		{
			int offsetBackup = stream.Offset;
			for (int i = 0; i < nTokensForward; i++)
			{
				if (!stream.LsiGoToNextCodeToken())
					break;
			}
			IToken nextToken = stream.Token;
			stream.Offset = offsetBackup;
			return nextToken;
		}

		static public List<IToken> LsiPeekPreviousTokens(this TextStream stream, int nTokens)
		{
			int offsetBackup = stream.Offset;
			List<IToken> result = new List<IToken>(nTokens);
			while (stream.LsiGoToPreviousCodeToken())
			{
				result.Add(stream.Token);
				if (result.Count >= nTokens)
					break;
			}
			stream.Offset = offsetBackup;
			return result;
		}

		static public List<IToken> LsiPeekNextTokens(this TextStream stream, int nTokens)
		{
			int offsetBackup = stream.Offset;
			List<IToken> result = new List<IToken>(nTokens);
			while (stream.LsiGoToNextCodeToken())
			{
				result.Add(stream.Token);
				if (result.Count >= nTokens)
					break;
			}
			stream.Offset = offsetBackup;
			return result;
		}
	}
}
