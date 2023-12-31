﻿using Artech.FrameworkDE.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor
{
    public abstract class TokenKeysEx : TokenKeys
    {

		// Tokens not defined at TokenKeys
		public const string CommentStartToken = "CommentStartToken";
        public const string MultiLineCommentWordToken = "MultiLineCommentWordToken";
        public const string RealNumberToken = "RealNumberToken";
        public const string IntegerNumberToken = "IntegerNumberToken";
        public const string OperatorToken = "OperatorToken";

		/// <summary>
		/// Single line comment end (line break)
		/// </summary>
		public const string CommentEndToken = "CommentEndToken";

		public const string FcnNpWordToken = "FcnNpWordToken";
        public const string DeprecatedWordToken = "DeprecatedWordToken";
        public const string HexIntegerNumberToken = "HexIntegerNumberToken";
        public const string NewOperatorToken = "NewOperatorToken";

        public const string DefaultToken = "DefaultToken";

        /// <summary>
        /// Function names token ???
        /// </summary>
        public const string FcnWordToken = "FcnWordToken";

		public const string RulesWordToken = "RulesWordToken";
		public const string RuleTriggerEventToken = "RuleTriggerEventToken";
		public const string NullToken = "NullToken";

		static private HashSet<string> _BlockDelimiterTokens;

		/// <summary>
		/// Token keys defining a start / end code block
		/// </summary>
		static public HashSet<string> BlockDelimiterTokens
		{
			get
			{
				if (_BlockDelimiterTokens == null)
				{
					_BlockDelimiterTokens = new HashSet<string>(new string[] {
						CaseToken , CloseDoCaseToken , CloseDoWhileToken , CloseEventToken , CloseForToken , CloseIfToken ,
						CloseNewToken , CloseSubToken , CloseXforToken , CloseXnewToken , ElseToken , OpenDoCaseToken , OpenDoWhileToken ,
						OpenEventToken , OpenForToken , OpenIfToken , OpenNewToken , OpenSubToken , OpenXforToken , OpenXnewToken , OtherwiseToken ,
						WhenNoneToken
					});
				}
				return _BlockDelimiterTokens;
			}
		}

	}
}
