using ActiproSoftware.SyntaxEditor;
using Artech.FrameworkDE.Text;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using LSI.Packages.Extensiones.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.Calls
{
	/// <summary>
	/// Get calls info from code editor tokens
	/// </summary>
	internal class CallMatcher
	{

		Document Document;

		IToken NameToken;

		List<IToken> ParmSeparators = new List<IToken>(128);

		/// <summary>
		/// Check if the stream, located in a open parenthesis, is a call start
		/// </summary>
		/// <param name="stream">Stream, located at an open parenthesis</param>
		/// <param name="objectNames">Object names cache</param>
		/// <returns>If it was a call, the call information. Null otherwise</returns>
		public CallInfo Match(TextStream stream, ObjectNamesCache objectNames)
		{
			Document = stream.Document;
			ParmSeparators.Clear();

			if (!ParethesisMatches(stream))
				return null;

			// Now token name, call start and parameters start are located. Search parameter separators commas and end of call location
			FindParametersPositions(stream);

			return new CallInfo(NameToken, ParmSeparators, Document, objectNames);
		}

		private bool ParethesisMatches(TextStream stream)
		{
			// 1) ( call | upd ... ) "(" KbOobject ( "," | ")" )
			// 2) KbObject "." ( call | upd ... ) "("
			// 3) KbObject "("

			List<IToken> previousTokens = stream.LsiPeekPreviousTokens(3);
			if (PreviousTokenIsCallKeyword(previousTokens))
			{
				// Check 2 first. ORDER IS IMPORTANT (TODO: Why is important?)
				if (CheckObjectDotCall(previousTokens, stream.Token))
					return true;

				// Check 1
				return CheckCallParenthesisObject(stream);
			}
			else
				// Check 3
				return CheckObjectParenthesis(previousTokens, stream.Token);
		}

		/// <summary>
		/// Check if it's a call "object.call("
		/// </summary>
		/// <param name="previousTokens">Previous tokens to the parenthesis. previousTokens[0] is the most near to the parenthesis</param>
		/// <param name="parenthesis"></param>
		/// <returns></returns>
		private bool CheckObjectDotCall(List<IToken> previousTokens, IToken parenthesis)
		{
			if (previousTokens.Count < 3)
				return false;
			// Call to PreviousTokenIsCallKeyword has already tested that previousTokens[0] is a call/udp keyword
			if (Document.GetTokenText(previousTokens[1]) != ".")
				return false;
			if (previousTokens[2].Key != TokenKeys.IdentifierToken && previousTokens[2].Key != TokenKeys.OpenRuleToken)
				return false;

			NameToken = previousTokens[2];
			ParmSeparators.Add(parenthesis);
			return true;
		}

		/// <summary>
		/// Check if it's a "call( object , ..."
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		private bool CheckCallParenthesisObject(TextStream stream)
		{
			List<IToken> nextTokens = stream.LsiPeekNextTokens(2);
			if (nextTokens.Count < 2)
				return false;
			if (nextTokens[0].Key != TokenKeys.IdentifierToken)
				return false;
			// TODO: This will discard calls without parameters... OK? (ex. "udp( Object )"). NO IS NOT OK, FIX THIS
			// TODO: it can be a "call( module1.module2..object, ... ) or call( object , .... ). If there are modules, this will fail
			// TODO: as next token will be ","
			string tokenText = Document.GetTokenText(nextTokens[1]);
			if (tokenText != ",")
				return false;

			NameToken = nextTokens[0];
			ParmSeparators.Add(nextTokens[1]);
			return true;
		}

		/// <summary>
		/// Check if it's a "object( ..."
		/// </summary>
		/// <param name="previousTokens"></param>
		/// <param name="parenthesis"></param>
		/// <returns></returns>
		private bool CheckObjectParenthesis(List<IToken> previousTokens, IToken parenthesis)
		{
			if (previousTokens.Count < 1)
				return false;

			// TODO: Check this
			string tokenKey = previousTokens[0].Key;
			if (tokenKey != TokenKeys.IdentifierToken && tokenKey != TokenKeysEx.FcnWordToken && tokenKey != TokenKeysEx.FcnNpWordToken 
				&& tokenKey != TokenKeys.OpenRuleToken)
				return false;

			NameToken = previousTokens[0];
			ParmSeparators.Add(parenthesis);
			return true;
		}

		private bool PreviousTokenIsCallKeyword(List<IToken> previousTokens)
		{
			if (previousTokens.Count < 1)
				return false;
			string previousTokenText = Document.GetTokenText(previousTokens[0]).ToLower();
			if (!KeywordGx.CALL_KEYWORDS.Contains(previousTokenText) &&
				!KeywordGx.UPD_KEYWORDS.Contains(previousTokenText))
				return false;

			return true;
		}

		private void FindParametersPositions(TextStream stream)
		{
			int offsetBackup = stream.Offset;
			stream.Offset = ParmSeparators[0].StartOffset;

			while (stream.LsiGoToNextCodeToken())
			{

				if (stream.Token.Key == TokenKeys.OpenParenthesisToken)
				{
					// It can be a inner call to get a parameter, ignore the parenthesis content
					if (!GoToParenthesisContentEnd(stream))
						break;
				}
				else if (stream.TokenText == ",")
					ParmSeparators.Add(stream.Token);
				else if(stream.Token.Key == TokenKeys.CloseParenthesisToken)
				{
					ParmSeparators.Add(stream.Token);
					// Finished
					return;
				}
				else if (TokenKeysEx.BlockDelimiterTokens.Contains(stream.Token.Key))
					// Open parenthesis may not be closed: Stop if a block keyword is found
					break;
			}

			// If we are here, we have not found the call close. Add the document end as end of call
			ParmSeparators.Add(stream.Token);
		}

		private bool GoToParenthesisContentEnd(TextStream stream)
		{
			int parenthesisBalance = 1;
			while (stream.LsiGoToNextCodeToken())
			{
				if (stream.Token.Key == TokenKeys.OpenParenthesisToken)
					parenthesisBalance++;
				else if (stream.Token.Key == TokenKeys.CloseParenthesisToken)
				{
					parenthesisBalance--;
					if (parenthesisBalance == 0)
						return true;
				}
				else if (TokenKeysEx.BlockDelimiterTokens.Contains(stream.Token.Key))
					// Open parenthesis may not be closed: Stop if a block keyword is found
					break;
			}
			return false;
		}

		/// <summary>
		/// Check if the stream, located on a name (funcion/object), is a call  (ex. "name(", "call( name", "name.call(" etc)
		/// </summary>
		/// <param name="stream">Stream to check</param>
		/// <returns>True if the stream is located on a call</returns>
		static public bool CheckNameCall(TextStream stream)
		{
			Document document = stream.Document;
			List<IToken> nextTokens = stream.LsiPeekNextTokens(3);

			if(nextTokens.Count >= 1 && nextTokens[0].Key == TokenKeys.OpenParenthesisToken)
				// "name("
				return true;

			if(nextTokens.Count >= 3)
			{
				if (document.GetTokenText(nextTokens[0]) == "." && nextTokens[2].Key == TokenKeys.OpenParenthesisToken)
				{
					string callKeyword = document.GetTokenText(nextTokens[1]).ToLower();
					if(KeywordGx.CALL_KEYWORDS.Contains(callKeyword) || KeywordGx.UPD_KEYWORDS.Contains(callKeyword))
						// name.(upd|call|...)(
						return true;
				}
			}

			List<IToken> previousTokens = stream.LsiPeekPreviousTokens(3);
			if(previousTokens.Count >= 2)
			{
				if(previousTokens[0].Key == TokenKeys.OpenParenthesisToken )
				{
					string callKeyword = document.GetTokenText(previousTokens[1]).ToLower();
					if (KeywordGx.CALL_KEYWORDS.Contains(callKeyword) || KeywordGx.UPD_KEYWORDS.Contains(callKeyword))
					{
						// Maybe "(upd|call|...)( name" 
						// Discard ".(upd|call|...)( name": In this case name is not the called object. Example: "othername.Call( EnumeratedDomain.Value ..."
						if( previousTokens.Count <= 2)
							return true;
						string previousToken = document.GetTokenText(previousTokens[2]);
						if (previousToken != ".")
							return true;
					}
				}
			}

			return false;
		}
	}
}
