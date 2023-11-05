using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Types;
using System;
using System.Collections.Generic;

namespace LSI.Packages.Extensiones.Utilidades.Variables
{
    /// <summary>
    /// Utilidades para la gestion de variables
    /// TODO: Move all of this to a VariableExtensions class in Utilidades.GxClassExtensions
    /// </summary>
    public class VariableGX
    {

        /// <summary>
        /// Devuelve cierto la variables es escalar (y no vector, etc)
        /// </summary>
        static public bool EsEscalar(Variable v)
        {
            string txtDimensiones = v.GetPropertyValue<string>(Properties.ATT.Dimensions);
            if (txtDimensiones == null)
                return true;
            Properties.ATT.Dimensions_Enum dimensiones = (Properties.ATT.Dimensions_Enum)
                Enum.Parse(typeof(Properties.ATT.Dimensions_Enum), txtDimensiones);
            return dimensiones == Properties.ATT.Dimensions_Enum.Scalar;
        }

        /// <summary>
        /// Verifica si una variables es de un tipo sdt. Si es asi, devuelve el nombre del sdt
        /// </summary>
        /// <param name="modelo">Modelo en el que revisar</param>
        /// <param name="v">Variable a verificar si es un cierto sdt</param>
        /// <returns>El nombre del sdt de la variable, si lo es. null si no es un sdt</returns>
        static public string EsSdt(KBModel modelo, Variable v)
        {
            AttCustomType type = v.GetPropertyValue<AttCustomType>(Artech.Genexus.Common.Properties.ATT.DataType);
            if (type.DataType != (int)eDBType.GX_SDT)
                return null;

            StructureTypeReference strRef = StructureTypeReference.Deserialize(type);
            return StructureInfoProvider.GetName(modelo, strRef);
        }

        /// <summary>
        /// Verifica si una variables es de tipo de un cierto sdt
        /// Magia sacada de http://www.gxopen.com/forumsr/servlet/viewthread?ARTECH,23,173380
        /// </summary>
        /// <param name="modelo">Modelo en el que revisar</param>
        /// <param name="v">Variable a verificar si es un cierto sdt</param>
        /// <param name="sdt">Sdt del que ver si la variables es una instancia</param>
        /// <param name="aceptarSubtipos">
        /// Indica si hay que aceptar subtipos de un SDT compuesto. Por ejemplo, el SDT
        /// XXX puede tener un subnivel YYY, y por tanto, se puede declarar una variable
        /// de tipo XXX.YYY. En caso que la variable sea de tipo XXX.YYY se devolvera cierto
        /// solo si se pasa cierto en este parametro.
        /// </param>
        /// <returns>Cierto si la variable es del tipo sdt indicado</returns>
        static public bool EsSdt(KBModel modelo, Variable v, SDT sdt, bool aceptarSubtipos)
        {
            string sdtLevelFullName = EsSdt(modelo, v);
            if (sdtLevelFullName == null)
                return false;

            if( sdt.Name == sdtLevelFullName )
                return true;

            if (aceptarSubtipos && sdtLevelFullName.StartsWith(sdt.Name + "."))
                return true;

            return false;
        }

        /// <summary>
        /// Verifica si una variables es de un tipo BC. Si es asi, devuelve el nombre del BC
        /// </summary>
        /// <param name="modelo">Modelo en el que revisar</param>
        /// <param name="v">Variable a verificar si es un cierto sdt</param>
        /// <returns>El nombre del BC de la variable, si lo es. null si no es un BC</returns>
        static public string EsBc(KBModel modelo, Variable v)
        {
            AttCustomType type = v.GetPropertyValue<AttCustomType>(Artech.Genexus.Common.Properties.ATT.DataType);
            if (type.DataType != (int)eDBType.GX_BUSCOMP && type.DataType != (int)eDBType.GX_BUSCOMP_LEVEL )
                return null;

            StructureTypeReference strRef = StructureTypeReference.Deserialize(type);
            return StructureInfoProvider.GetName(modelo, strRef);
        }

        /// <summary>
        /// Verifica si una variables de un cierto BC
        /// </summary>
        /// <param name="modelo">Modelo en el que revisar</param>
        /// <param name="v">Variable a verificar si es un cierto sdt</param>
        /// <param name="nombreStdBC">Nombre del Sdt / BC del que ver si la variables es una instancia</param>
        /// <param name="aceptarSubtipos">
        /// Indica si hay que aceptar subtipos de un SDT compuesto. Por ejemplo, el BC
        /// XXX puede tener un subnivel YYY, y por tanto, se puede declarar una variable
        /// de tipo XXX.YYY. En caso que la variable sea de tipo XXX.YYY se devolvera cierto
        /// solo si se pasa cierto en este parametro.
        /// </param>
        /// <returns>Cierto si la variable es del tipo sdt / BC indicado</returns>
        static public bool EsBc(KBModel modelo, Variable v, Transaction bc, bool aceptarSubtipos)
        {
            string bcLevelFullName = EsBc(modelo, v);
            if (bcLevelFullName == null)
                return false;

            if (bc.Name == bcLevelFullName)
                return true;

            if (aceptarSubtipos && bcLevelFullName.StartsWith(bc.Name + "."))
                return true;

            return false;
        }

        static private string TypeDescription(eDBType type, int length, int decimals, bool oneLetterForType)
        {
            string txtType;
            if (type == eDBType.Boolean)
                txtType = (oneLetterForType ? "B" : "Boolean");
            else
            {
                txtType = ( type == eDBType.GX_SDT ? "SDT" : type.ToString() );
                if (oneLetterForType && txtType.Length > 0 && type != eDBType.GX_SDT)
                    txtType = txtType.Substring(0, 1);

                if (length > 0 && type != eDBType.GX_SDT)
                {
                    txtType += "(";
                    txtType += length.ToString();
                    if (decimals > 0)
                        txtType += "." + decimals;
                    txtType += ")";
                }
            }
            return txtType;
        }

        static public string TypeDescription(ITypedObject v, bool oneLetterForType)
        {
            return TypeDescription(v.Type, v.Length, v.Decimals, oneLetterForType);
        }

        /// <summary>
        /// Get variable non simple data type description: SDT / BC / External object / "User defined type" name 
        /// </summary>
        /// <param name="v">Variable to get the data type</param>
        /// <returns>The data type description. null if the data type is simple or unknown</returns>
        static public string GetAttCustomTypDescription(Variable v)
        {
            string description = null;
            if (v.Type == eDBType.GX_BUSCOMP || v.Type == eDBType.GX_BUSCOMP_LEVEL)
                description = "BC:";
            else if (v.Type == eDBType.GX_EXTERNAL_OBJECT)
                description = "Ext:";
            else if (v.Type == eDBType.GX_SDT)
                description = "Sdt:";
            else if (v.Type == eDBType.GX_USRDEFTYP)
                description = string.Empty;
            else
                return null;

            AttCustomType type = v.GetPropertyValue<AttCustomType>(Artech.Genexus.Common.Properties.ATT.DataType);
            if (type == null)
                return null;

            return description + type.GetDescription(v.Model);
        }

        /// <summary>
        /// Devuelve una descripcion del tipo de una variable
        /// </summary>
        /// <param name="v">Variable de la que obtener la descripcion</param>
        /// <returns></returns>
        static public string DescripcionTipo(KBModel modelo, Variable v)
        {

            string txtType = string.Empty;

            // Check if variables has a non simple type (BC, SDT, etc)
            string customTypeName = GetAttCustomTypDescription(v);
            if (!string.IsNullOrEmpty(customTypeName))
                // It's a non simple type
                txtType = customTypeName;
            else
            {
                if (v.AttributeBasedOn != null)
                    txtType = "Atr:" + v.AttributeBasedOn.Name + " ";
                else if (v.DomainBasedOn != null)
                    txtType = "Dom:" + v.DomainBasedOn.Name + " ";

                if (!string.IsNullOrEmpty(txtType))
                    txtType += " / ";
                txtType += TypeDescription(v, true) + " ";
            }

            if (!EsEscalar(v))
                txtType = "Array<" + txtType.Trim() + ">";
            if (v.IsCollection)
                txtType = "Collection<" + txtType.Trim() + ">";

            return txtType.Trim();
        }

        /// <summary>
        /// Verifica si dos variables tienen el mismo tipo
        /// </summary>
        /// <param name="v1">Variable a revisar</param>
        /// <param name="v2">Variable a revisar</param>
        /// <returns>Cierto si v1 y v2 tienen el mismo tipo</returns>
        static public bool TienenMismoTipo(Variable v1, Variable v2)
        {
            // TODO: Esto es una comparacion parcial. Deberian revisarse mas cosas:
            // TODO: Tipos de sdts, si una es array y otra no, longitud de arrays, dominios, etc.
            if (v1.Type != v2.Type)
                return false;
            if (v1.Length != v2.Length)
                return false;
            if (v1.Decimals != v2.Decimals)
                return false;
            if (v1.IsCollection != v2.IsCollection)
                return false;
            if (v1.AttributeBasedOn != null && v2.AttributeBasedOn == null)
                return false;
            if (v1.AttributeBasedOn == null && v2.AttributeBasedOn != null)
                return false;
            if (v1.AttributeBasedOn != null && v2.AttributeBasedOn != null
                && v1.AttributeBasedOn.Id != v2.AttributeBasedOn.Id)
                return false;

            return true;
        }

        /// <summary>
        /// Set the type and picture of an attribute to a variable 
        /// </summary>
        /// <param name="v">Variable to set</param>
        /// <param name="attribute">Attribute witch get the picture/type</param>
        static public void CopyTypePictureFromAttribute(Variable v,
            Artech.Genexus.Common.Objects.Attribute attribute)
        {
            // Set type
            v.Decimals = attribute.Decimals;
            v.Length = attribute.Length;
            v.Signed = attribute.Signed;
            v.Type = attribute.Type;

            // Set picture:
            string[] pictureProps = { Properties.ATT.Picture, Properties.ATT.Case ,
                                      Properties.ATT.DateFormat , Properties.ATT.HourFormat ,
                                      Properties.ATT.LeftFill , Properties.ATT.ThousandSeparator ,
                                      Properties.ATT.Prefix
                                    };
            foreach (string p in pictureProps)
                v.SetPropertyValue(p, attribute.GetPropertyValue(p));
        }

        /// <summary>
        /// Changes the "Attribute based on" variable property. It changes associated
        /// attribute properties too, if its needed
        /// </summary>
        /// <remarks>
        /// If new attribute is null, the variable picture and type will be keept.
        /// </remarks>
        /// <param name="v">Variable to change</param>
        /// <param name="newAttribute">New attribute base for variable. It can be null</param>
        static public void ReplaceAttBasedOn(Variable v, 
            Artech.Genexus.Common.Objects.Attribute newAttribute)
        {

            Artech.Genexus.Common.Objects.Attribute oldAttribute = v.AttributeBasedOn;
            v.AttributeBasedOn = newAttribute;
            if (newAttribute == null && oldAttribute != null)
            {
                if (oldAttribute.DomainBasedOn != null)
                    // Set the domain: This will change type and picture too
                    v.DomainBasedOn = oldAttribute.DomainBasedOn;
                else
                {
                    // This change resets the attribute based on: Variable
                    // type/picture properties will be reset too (type = N(4)). Assign attribute 
                    // properties to variables manually:
                    CopyTypePictureFromAttribute(v, oldAttribute);
                }
            }
        }

        /// <summary>
        /// Check if a variable member function will update the variable content
        /// </summary>
        /// <remarks>
        /// This function is an aproximation. Genexus does not store if a function will update the variable
        /// </remarks>
        /// <param name="memberFunctionName">The function member name</param>
        /// <returns>True if the member modifies the object caller</returns>
        static public bool IsModifierMember(string memberFunctionName)
        {
            memberFunctionName = memberFunctionName.ToLower();

            string[] modifierMembers = { "add", "clear", "remove", "sort" , "load" };
            List<string> list = new List<string>(modifierMembers);
            if (list.Contains(memberFunctionName))
                return true;

            // If the function starts with set ("SetEmpty", etc), it will modify the variable:
            // Exception: SetFocus
            if( memberFunctionName.StartsWith("set") && memberFunctionName != "setfocus")
                return true;

            // FromJson, Fromxml, etc.
            if (memberFunctionName.StartsWith("from"))
                return true;

            return false;
        }

    }
}
