using Artech.Architecture.Datatypes;
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
    /// Gx function/rule name. The type is the function return type (NONE for rules)
    /// </summary>
    public class FunctionNameInfo : TypedNameInfo
    {

        /// <summary>
        /// Part types where the function can be used
        /// </summary>
        public List<ObjectPartType> PartTypes = new List<ObjectPartType>();

		/// <summary>
		/// Function is really a rule?
		/// </summary>
		public bool IsRule;

        public FunctionNameInfo(IMethodInfo function, ObjectPartType partType, bool isRule) 
            : base(function.Name, function.Signature, isRule ? ChoiceInfo.ChoiceType.None : ChoiceInfo.ChoiceType.Function) 
        {
			IsRule = isRule;

			// Try to get the function return value
			eDBType returnType = eDBType.NONE;
			if (!IsRule)
			{
				int typeId;
				if (function.Type != null && function.Type.TypeId != null && int.TryParse(function.Type.TypeId, out typeId))
				{
					if (Enum.IsDefined(typeof(eDBType), typeId))
						returnType = (eDBType)typeId;
				}
			}
            _DataType = new DataTypeInfo(returnType, 0, 0, false);

            PartTypes.Add(partType);
        }

		public FunctionNameInfo(string name, ObjectPartType partType, bool isRule)
			: base(name, name, isRule ? ChoiceInfo.ChoiceType.None : ChoiceInfo.ChoiceType.Function)
		{
			IsRule = isRule;
			_DataType = new DataTypeInfo(eDBType.NONE, 0, 0, false);
			PartTypes.Add(partType);
		}

		public bool CanBeUsed(ObjectPartType objectPartType)
        {
            // OK. It seems functions can be used on SD panels are not reported by ILanguageManager.GetFunctions
            // (see AddFunctionNames.ObjectNamesCache) so, fuck off, and make them all available. ¯\_(ツ)_/¯
            if (objectPartType.ObjectType == ObjClass.SDPanel)
                return true;

            return PartTypes.Any(x => x.Equals(objectPartType));
        }
    }
}
