using Artech.Architecture.UI.Framework.Language;
using Artech.Genexus.Common;
using LSI.Packages.Extensiones.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames
{
    /// <summary>
    /// Kb name with datatype (variables, attributes, domains)
    /// </summary>
    public abstract class TypedNameInfo : ObjectNameInfo
    {

        /// <summary>
        /// Data type for name (function result, domain type, attribute type, variable name, etc)
        /// </summary>
        protected DataTypeInfo _DataType;

        public virtual DataTypeInfo DataType { get { return _DataType; } }

        protected TypedNameInfo() { }

        protected TypedNameInfo(string name, string description, ChoiceInfo.ChoiceType choiceType, eDBType dataType, int length,
            int decimals, bool isCollection)
            : base(name, description, choiceType)
        {
            _DataType = new DataTypeInfo(dataType, length, decimals, isCollection);
        }

        public TypedNameInfo(string name, string description, ChoiceInfo.ChoiceType choiceType, ITypedObject typedObject)
            : this(name, description, choiceType, new DataTypeInfo(typedObject))
        { }

        public TypedNameInfo(string name, string description, ChoiceInfo.ChoiceType choiceType, DataTypeInfo dataType)
            : base(name, description, choiceType)
        {
            _DataType = dataType;
        }

        protected TypedNameInfo(string name, string description, ChoiceInfo.ChoiceType choiceType)
            : base( name, description, choiceType)
        { }

    }
}
