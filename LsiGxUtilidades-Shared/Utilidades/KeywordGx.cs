using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades
{
    /// <summary>
    /// Genexus keyword definitions
    /// </summary>
    public class KeywordGx
    {
        /// <summary>
        /// Function / member to get url for object
        /// </summary>
        public const string LINK = "link";

        /// <summary>
        /// Function / member to create a web component
        /// </summary>
        public const string CREATE = "create";

        /// <summary>
        /// Function /member to do a submit call
        /// </summary>
        public const string SUBMIT = "submit";

        public const string FOR = "for";
        public const string EACH = "each";
        public const string LINE = "line";
        public const string IN = "in";

        /// <summary>
        /// Keywords to do "function" calls: all parameters except last are expected. Last parm. will be returned
        /// </summary>
        public readonly static string[] UPD_KEYWORDS = { "udp", "udf" };

        public const string POPUP = "popup";
        public const string CALL = "call";
        
        /// <summary>
        /// Keywords to do "procedure" calls: all parameters are expected
        /// </summary>
        public readonly static string[] CALL_KEYWORDS = new string[] { CALL, "prompt", SUBMIT, LINK, POPUP ,
            CREATE };

        /// <summary>
        /// Boolean operators
        /// </summary>
        static public readonly string[] BOOLEAN_OPERATORS = { IN , "like", "not", "and", "or" };

        /// <summary>
        /// Create new Sdt
        /// </summary>
        public const string NEWSDTOPERATOR = "new()";

        /// <summary>
        /// Native code commands
        /// </summary>
        static public readonly string[] NATIVECODEKEYWORDS = { "csharp", "java", "sql" , "dbase" , "vb" };

        /// <summary>
        /// Deprecated keywords
        /// </summary>
        static public readonly string[] DEPRECATEDKEYWORDS = { "vb", "dbase" , "udf" };

        /// <summary>
        /// Print command
        /// </summary>
        public const string PRINT = "print";

        /// <summary>
        /// Print commands
        /// </summary>
        static public readonly string[] PRINTCOMMANDS = { "cp", "mb", "mt", "pl", PRINT, "lineno" , "noskip", "eject" ,
            "print if detail" };

        /// <summary>
        /// Transactions commands
        /// </summary>
        static public readonly string[] TRANSACTIONCOMMANDS = { "commit", "rollback" };

        /// <summary>
        /// Refresh commands
        /// </summary>
        static public readonly string[] REFRESHCOMMANDS = { "refresh", "refresh keep" };

        public const string EVENT = "event";

        public const string FOREACH = FOR + " " + EACH;
        public const string FOREACHLINE = FOREACH + " " + LINE;
        public const string FOREACHLINEIN = FOREACHLINE + " " + IN;
		public const string FOREACHSELECTEDLINEIN = FOREACH + " selected " + LINE + " " + IN;

		public const string ENDFOR = "endfor";

        /// <summary>
        /// Aggregate functions
        /// </summary>
        static public readonly string[] AGGREGATE_FUNCTIONS = { "sum" , "count" , "average" , "max" , "min" , "find" };

		/// <summary>
		/// ThemeClass:classname keyword. KEEP THIS WITH ORIGINAL CASE (do not lowercase it)
		/// </summary>
		public const string THEME_CLASS = "ThemeClass";

        /// <summary>
        /// Item member for collections
        /// </summary>
        public const string ITEM = "item";

        /// <summary>
        /// CurrentItem member for collections
        /// </summary>
        public const string CURRENT_ITEM = "currentitem";

        static public string Capitalize(string word)
        {
            return char.ToUpper(word[0]) + word.Substring(1);
        }

    }
}
