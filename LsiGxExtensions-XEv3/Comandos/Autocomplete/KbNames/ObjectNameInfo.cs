using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Language;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames
{
    /// <summary>
    /// Object info
    /// </summary>
    public class ObjectNameInfo
    {

        /// <summary>
        /// Object name
        /// </summary>
        public string Name;

        protected string _Description;

        /// <summary>
        /// Object description
        /// </summary>
        virtual public string Description {  get { return _Description; } }

        /// <summary>
        /// Choice type
        /// </summary>
        public ChoiceInfo.ChoiceType Type;

        public ObjectNameInfo() { }

        public ObjectNameInfo(string name, string description, ChoiceInfo.ChoiceType type)
        {
            Name = name;
            _Description = description;
            Type = type;
        }

        public override string ToString()
        {
            return Name;
        }

		/// <summary>
		/// True if name can be used in a call as base (ex. "function()", "kbobjectname.udp()")
		/// </summary>
		public bool IsCallable
		{
			get
			{
				KbObjectNameInfo oName = this as KbObjectNameInfo;
				if (oName != null && ObjClassLsi.LsiIsCallableType(oName.ObjectKey.Type))
					return true;
				if (this is FunctionNameInfo)
					return true;
				return false;
			}
		}
    }
}
