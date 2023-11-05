//using System;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Datatypes;
using Artech.Architecture.Language.Datatypes.External;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Language;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Udm.Framework.References;
using System.Collections.Generic;

namespace LSI.Packages.Extensiones.Utilidades
{
	/// <summary>
	/// Utilidades de gestion y consulta de atributos
	/// TODO: Move all of this to a AttributeExtensions class in Utilidades.GxClassExtensions
	/// </summary>
	public class AtributoGx
    {
        /// <summary>
        /// Obtiene la lista de tablas que referencian a un atributo
        /// </summary>
        /// <param name="atributo">El atributo que buscar</param>
        /// <returns>La lista de tablas que referencian al atributo indicado</returns>
        static public List<Table> TablasQueReferencianAtributo(Attribute atributo)
        {
            List<Table> tablas = new List<Table>();

            foreach (EntityReference referencia in atributo.GetReferencesTo())
            {
                // Vudu magico sacado de http://svn2.assembla.com/svn/gxextensions/trunk/KBDoctor/AttributesHelper.cs
                if (referencia.From.Type == ObjClass.Table)
                {
                    Table tabla = (Table)Table.Get(UIServices.KB.CurrentModel, referencia.From);
                    tablas.Add(tabla);
                }
            }
            return tablas;
        }

        /// <summary>
        /// Devuelve la lista de metodos que modifican a un llamador
        /// </summary>
        /// <param name="info">Objeto del que revisar los metodos disponibles</param>
        /// <returns>La lista de metodos que modifican al llamador</returns>
        static internal List<IMemberInfo> MiembrosModificadores(ITypedObjectInfo info)
        {
            List<IMemberInfo> miembrosModificadores = new List<IMemberInfo>();
            foreach (IMemberInfo miembro in info.GetPEMsByType(PEMType.Method, PEMModifier.Common, PEMFlags.All))
            {
                if (miembro is IMemberDefinition)
                {
                    IMemberDefinition md = (IMemberDefinition)miembro;
                    foreach (IModifierDefinition modificador in md.Modifiers)
                    {
                        if( modificador.Name == ModifiersConstants.UpdatesValueModifier ) 
                        {
                            miembrosModificadores.Add(miembro);
                            break;
                        }
                    }
                }
            }
            return miembrosModificadores;
        }

        /// <summary>
        /// Devuelve una lista de miembros que modifican al atributo
        /// </summary>
        /// <remarks>
        /// Por ejemplo, funciones "SetEmpty" o "SetNull"
        /// </remarks>
        /// <param name="atributo">Atributo del que obtener las funciones</param>
        /// <returns>La lista de miembros del atributo que lo modifican</returns>
        static public List<IMemberInfo> MiembrosModificadores(Attribute atributo)
        {
            LanguageManager lm = new LanguageManager(atributo.Model);
            ITypedObjectInfo info = lm.GetAttributePEMs(atributo);

            return MiembrosModificadores(info);
        }

        static public List<string> ModifierMembersNames(Attribute attribute)
        {
            List<string> names = new List<string>();
            foreach (IMemberInfo m in MiembrosModificadores(attribute))
                names.Add(m.Name.ToLower());
            return names;
        }

        /// <summary>
        /// Returns string with the attribute type descripcion
        /// </summary>
        /// <param name="a">Attribute to get its description</param>
        /// <returns>The attribute type description</returns>
        static public string TypeDescription(KBModel modelo, Attribute a)
        {

            string txtType = string.Empty;

            if (a.DomainBasedOn != null)
                txtType += "Dom: " + a.DomainBasedOn.Name + " ";

            txtType += a.Type.ToString().Substring(0, 1);
            if (a.Length > 0)
            {
                txtType += "(";
                txtType += a.Length.ToString();
                if (a.Decimals > 0)
                    txtType += "." + a.Decimals;
                txtType += ") ";
            }

            return txtType.Trim();
        }

        /// <summary>
        /// Get the table where an attribute is key with the smallest primary key
        /// </summary>
        /// <param name="model">Model where to search</param>
        /// <param name="a">Attribute to search as key</param>
        /// <returns>The table. Null if no table was found</returns>
        static public Table GetTableWhereIsKey(KBModel model, Attribute a)
        {
            Table tableMinKey = null;

            foreach (Table table in Table.GetWithKeyAttribute(model, a.Key))
            {
                if (tableMinKey == null || table.TableStructure.PrimaryKey.Count <
                    tableMinKey.TableStructure.PrimaryKey.Count)
                    tableMinKey = table;
            }
            return tableMinKey;
        }
    }
}
