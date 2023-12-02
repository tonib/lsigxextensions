using ActiproSoftware.SyntaxEditor;
using Artech.FrameworkDE.Text;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.Calls
{
	/// <summary>
	/// Tool to find calls on a code segment based on text editor tokens
	/// </summary>
	class CallInfoFinder
	{
		Document Document;

		ObjectNamesCache ObjectNames;

		CallMatcher Matcher;

		public CallInfoFinder(Document document, ObjectNamesCache objectNames)
		{
			Document = document;
			ObjectNames = objectNames;
			Matcher = new CallMatcher();
		}

		public CallInfo FindCallOverCaret(int caretOffset)
		{
			// Search unbalanced open parenthesis tokens backward
			TextStream stream = Document.GetTextStream(caretOffset);

			// Search backward
			int parenthesisBalance = 0;
			while (stream.LsiGoToPreviousCodeToken())
			{
				IToken token = stream.Token;

				// If we have found a block delimiter, stop search (these cannot be in the middle of a call)
				if (TokenKeysEx.BlockDelimiterTokens.Contains(token.Key))
					break;

				if (token.Key == TokenKeys.CloseParenthesisToken)
					parenthesisBalance--;
				else if (token.Key == TokenKeys.OpenParenthesisToken)
				{
					if (parenthesisBalance >= 0)
					{
						// Not closed parenthesis, or closed after the cursor. Check if it's an object call
						CallInfo callInfo = Matcher.Match(stream, ObjectNames);
						if (callInfo != null)
							return callInfo;
					}
					else
						parenthesisBalance++;
				}
			}

			return null;
		}

		public CallInfoTree FindCallsInRange(int startOffset, int endOffset)
		{
			// First tokens in range can be parameters of a call started before the range. So, we need to
			// start searching before. The safest place where to start the search is at the last block start / end
			TextStream stream = Document.GetTextStream(startOffset);
			while(stream.LsiGoToPreviousCodeToken())
			{
				if (TokenKeysEx.BlockDelimiterTokens.Contains(stream.Token.Key))
					break;
			}

			CallInfoTree callsTree = new CallInfoTree();
			while(stream.LsiGoToNextCodeToken())
			{
				if (stream.Token.StartOffset > endOffset)
					break;

				if (stream.Token.Key == TokenKeys.OpenParenthesisToken)
				{
					CallInfo callInfo = Matcher.Match(stream, ObjectNames);
					if (callInfo != null)
						callsTree.Add(callInfo);
				}
			}

			return callsTree;
		}
	}
}
