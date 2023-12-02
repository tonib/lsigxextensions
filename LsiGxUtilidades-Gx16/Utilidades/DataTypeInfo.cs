using Artech.Common.Properties;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Types;
using Artech.Udm.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades
{
    /// <summary>
    /// Basic data type information
    /// </summary>
    public class DataTypeInfo
    {

        /// <summary>
        /// Constant value for "no type"
        /// </summary>
        static public readonly DataTypeInfo NoType = new DataTypeInfo();

        /// <summary>
        /// Data type
        /// </summary>
        public eDBType Type = eDBType.NONE;

        /// <summary>
        /// Length (string lenght, or number total length, etc)
        /// </summary>
        public int Length;

        /// <summary>
        /// Number of decimals on numeric types. Also used on dates
        /// </summary>
        public int Decimals;

        /// <summary>
        /// Data type is collection?
        /// </summary>
        public bool IsCollection;

        /// <summary>
        /// Type name if it references a BC, SDT, ExternalObject, etc, LOWERCASE. Null otherwise.
        /// TODO: This name can be wrong. Is cached, but SDT, as example, can change their name with time. And this will not be updated!
        /// </summary>
        public string ExtendedTypeName;

        /// <summary>
        /// Extended type, if it references a BC, SDT, ExternalObject, etc. Its the unserialized StructureTypeReference for the SDT level
        /// null oherwise
        /// </summary>
        public EntityKey ExtendedType;

        public DataTypeInfo(eDBType type, int length, int decimals, bool isCollection)
        {
            Type = type;

            // Gx is adding values on this properties for other values (sdt...)
            if (Type == eDBType.NUMERIC || Type == eDBType.CHARACTER || Type == eDBType.DATE || Type == eDBType.LONGVARCHAR ||
               Type == eDBType.DATETIME || Type == eDBType.VARCHAR)
                Length = length;

            if (Type == eDBType.NUMERIC || Type == eDBType.DATE || Type == eDBType.LONGVARCHAR ||
               Type == eDBType.DATETIME)
                Decimals = decimals;

            IsCollection = isCollection;
        }

        /// <summary>
        /// Constructor for extended types (sdts, external types, etc)
        /// </summary>
        public DataTypeInfo(eDBType type, EntityKey extendedType, string extendedTypeName, bool isCollection)
        {
            Type = type;
            ExtendedType = extendedType;
            ExtendedTypeName = extendedTypeName;
            IsCollection = isCollection;
        }

        public DataTypeInfo(ITypedObject typedObject) :
            this(typedObject.Type, typedObject.Length, typedObject.Decimals, typedObject.IsCollection)
        {

            // Previously this was checkedn only for variables. I don't remember why. Currently, this also needs be checked for SDT structure items:
            /*Variable v = typedObject as Variable;
            if (v == null)
                return;

            if(v.Type == eDBType.GX_BUSCOMP || v.Type == eDBType.GX_BUSCOMP_LEVEL ||
                v.Type == eDBType.GX_EXTERNAL_OBJECT || v.Type == eDBType.GX_SDT || 
                v.Type == eDBType.GX_USRDEFTYP)
            {
                // Store the type name, it will be used as hash in prediction model
                AttCustomType type = v.GetPropertyValue<AttCustomType>(Artech.Genexus.Common.Properties.ATT.DataType);
                if (type == null)
                    return;
                
                ExtendedTypeName = type.GetDescription(v.Model);
                if (ExtendedTypeName != null)
                    ExtendedTypeName = ExtendedTypeName.ToLower();
            }*/
            IPropertyBag propertyBag = typedObject as IPropertyBag;
            if (propertyBag == null)
                return;

            if (typedObject.Type == eDBType.GX_BUSCOMP || typedObject.Type == eDBType.GX_BUSCOMP_LEVEL ||
                typedObject.Type == eDBType.GX_EXTERNAL_OBJECT || typedObject.Type == eDBType.GX_SDT ||
                typedObject.Type == eDBType.GX_USRDEFTYP)
            {
                // Store the type name, it will be used as hash in prediction model
                AttCustomType type = propertyBag.GetPropertyValue<AttCustomType>(Artech.Genexus.Common.Properties.ATT.DataType);
                if (type == null)
                    return;

                ExtendedType = StructureTypeReference.Deserialize(type)?.Key;
                ExtendedTypeName = type.GetDescription(typedObject.Model)?.ToLower();
            }
        }

        /// <summary>
        /// Clone a DataTypeInfo
        /// </summary>
        /// <param name="source">Data type to clone</param>
        public DataTypeInfo(DataTypeInfo source)
        {
            Type = source.Type;
            Length = source.Length;
            Decimals = source.Decimals;
            IsCollection = source.IsCollection;
            ExtendedTypeName = source.ExtendedTypeName;
            ExtendedType = source.ExtendedType;
        }

        /// <summary>
        /// Type with type "none"
        /// </summary>
        protected DataTypeInfo() { }

        /*public bool TypeEqual(DataTypeInfo dt)
        {
            return dt.Type == Type && dt.Decimals == Decimals && dt.IsCollection == IsCollection && 
                dt.Length == Length && dt.ExtendedTypeName == ExtendedTypeName;
        }*/

		public override string ToString()
		{
			string txt;
			if (ExtendedTypeName != null)
				txt = ExtendedTypeName;
			else
				txt = Type.ToString();

			if (Length > 0 || Decimals > 0)
				txt += "(" + Length + "." + Decimals + ")";

			if (IsCollection)
				txt = "Col<" + txt + ">";

			return txt;
		}
	}
}
