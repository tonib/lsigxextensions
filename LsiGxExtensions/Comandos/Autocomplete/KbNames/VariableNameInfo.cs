using Artech.Genexus.Common;
using LSI.Packages.Extensiones.Utilidades;
using static Artech.Architecture.UI.Framework.Language.ChoiceInfo;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames
{
    public class VariableNameInfo : TypedNameInfo
    {

        /// <summary>
        /// Store info about the variable
        /// </summary>
        /// <param name="v">Info about the variable</param>
        public VariableNameInfo(Variable v) : 
            base(v.Name, v.Description, ChoiceType.Variable, v)
        { }

        /// <summary>
        /// Variable name with a given type
        /// </summary>
        /// <param name="v">Info about the variable</param>
        public VariableNameInfo(string name, DataTypeInfo dataType) :
            base(name, name, ChoiceType.Variable, dataType)
        { }

        /// <summary>
        /// Constructor for undefined variable
        /// </summary>
        /// <param name="name">Variable name</param>
        public VariableNameInfo(string name) : base(name, "", ChoiceType.Variable , eDBType.NUMERIC, 4 , 0, false) {}

    }
}
