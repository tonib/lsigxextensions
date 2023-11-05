using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.Commands
{
    /// <summary>
    /// Code block type identifiers
    /// </summary>
    public abstract class ScopeId
    {
        public const string DoCaseCodeBlock = "DoCaseCodeBlock";
        public const string DoWhileCodeBlock = "DoWhileCodeBlock";
        public const string ForCodeBlock = "ForCodeBlock";
        public const string IfCodeBlock = "IfCodeBlock";
        public const string HeaderFooterCodeBlock = "HeaderFooterCodeBlock";
        public const string NewCodeBlock = "NewCodeBlock";
        public const string XforCodeBlock = "XforCodeBlock";
        public const string XnewCodeBlock = "XnewCodeBlock";
        public const string SubCodeBlock = "SubCodeBlock";
		public const string EventCodeBlock = "EventCodeBlock";
		public const string CompositeBlock = "CompositeBlock";

		// Not really Genexus block codes. These are "custom"
		public const string ForEachBlock = "ForEachBlock";
        public const string Root = "Root";
    }
}
