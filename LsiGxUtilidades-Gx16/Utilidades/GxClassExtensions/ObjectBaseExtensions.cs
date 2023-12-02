using System;
using System.Collections.Generic;
using Artech.Architecture.Language.Parser.Data;
using System.Reflection;
using System.Linq;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{
    /// <summary>
    /// ObjectBase class extensions
    /// </summary>
    static public class ObjectBaseExtensions
    {

        /// <summary>
        /// Cache de propiedades a revisar para un tipo de nodo en el arbol de parseado
        /// de codigo Genexus. La clave es el tipo del nodo, y el valor la lista de
        /// propiedades a revisar
        /// </summary>
        static private Dictionary<Type, List<PropertyInfo>> ParsingProperties =
            new Dictionary<Type, List<PropertyInfo>>();

        /// <summary>
        /// Check if the code node node is an external code block (VB, CHARP, JAVA, etc.)
        /// </summary>
        /// <param name="codeNode">Node to check</param>
        /// <param name="externalCode">If the node it's external code, this variable returns the external code.
        /// If not, it returns null</param>
        /// <returns>True if the node is external code</returns>
        static public bool LsiIsExternalCode(this ObjectBase codeNode, out string externalCode)
        {
            externalCode = null;

            CommandLine cmd = codeNode as CommandLine;
            if (cmd == null)
                return false;

            Word cmdName = cmd.Name as Word;
            if (cmdName == null)
                return false;

            string name = cmdName.Text.ToLower();
            if( KeywordGx.NATIVECODEKEYWORDS.Contains(name) )
            {
                if (cmd.Parameters.Count < 1)
                    return false;

                Word externalCodeNode = cmd.Parameters[0] as Word;
                if (externalCodeNode == null)
                    return false;

                externalCode = externalCodeNode.Text;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the code node node is an external code block (VB, CHARP, JAVA, etc.)
        /// </summary>
        /// <param name="codeNode">Node to check</param>
        /// <returns>True if the node is external code</returns>
        static public bool LsiIsExternalCode(this ObjectBase codeNode)
        {
            string externalCode;
            return codeNode.LsiIsExternalCode(out externalCode);
        }

        /// <summary>
        /// Check if a code node it's a PRINT command
        /// </summary>
        /// <param name="codeNode">Node to check</param>
        /// <param name="printBlockName">Si se devuelve cierto, aqui se indica el nombre
        /// del printblock impreso, en minusculas</param>
        /// <returns>True if the node was a PRINT command</returns>
        static public bool LsiIsCmdPrint(this ObjectBase codeNode, out string printBlockName)
        {
            printBlockName = null;

            CommandLine cmd = codeNode as CommandLine;
            if (cmd == null)
                return false;

            Word nombreComando = cmd.Name as Word;
            if (nombreComando == null)
                return false;

            if (nombreComando.Text.ToLower() != KeywordGx.PRINT)
                return false;

            if (cmd.Parameters.Count == 0)
                return false;

            Word printBlock = cmd.Parameters[0] as Word;
            if (printBlock == null)
                return false;

            printBlockName = printBlock.Text.ToLower();
            return true;
        }

        /// <summary>
        /// Check if a code node it's a PRINT command
        /// </summary>
        /// <param name="codeNode">Node to check</param>
        /// <returns>True if the node was a PRINT command</returns>
        static public bool LsiIsCmdPrint(this ObjectBase codeNode)
        {
            string printBlockName;
            return codeNode.LsiIsCmdPrint(out printBlockName);
        }

        /// <summary>
        /// Verifica si un nodo de codigo es una llamada a una funcion agregada (sum, average, etc)
        /// </summary>
        /// <param name="nodoCodigo">Nodo de codigo a revisar</param>
        /// <returns>Cierto si el nodo es una llamada a una funcion agregada</returns>
        static public bool LsiIsAggregatedFunction(this ObjectBase nodoCodigo)
        {
            Function funcion = nodoCodigo as Function;
            if (funcion == null)
                return false;

            Word nombreFuncion = funcion.Name as Word;
            if (nombreFuncion == null)
                return false;

            return KeywordGx.AGGREGATE_FUNCTIONS.Contains(nombreFuncion.Text.ToLower());
        }

        static private string LsiGetWordCommand(this ObjectBase nodoCodigo)
        {
            CommandBlock cmd = nodoCodigo as CommandBlock;
            if (cmd == null)
                return null;

            WordCommand nombre = cmd.Name as WordCommand;
            if (nombre == null)
                return null;

            return nombre.Text.ToLower();
        }

        /// <summary>
        /// Verifica si un nodo de codigo es una sentencia NEW o XNEW
        /// </summary>
        /// <param name="codeNode">Nodo de codigo a revisar</param>
        /// <returns>Cierto si es una sentencia X/NEW</returns>
        static public bool LsiIsNewStatement(this ObjectBase codeNode)
        {
            string texto = codeNode.LsiGetWordCommand();
            if (texto == null)
                return false;
            return texto.Contains("new");
        }

        /// <summary>
        /// Check if a code node is a FOR EACH or XFOR EACH statement.
        /// FOR EACH LINE sentences will return false.
        /// </summary>
        /// <param name="codeNode">Code node to check</param>
        /// <returns>True if it's a X/FOR EACH sentence</returns>
        static public bool LsiIsForEachStatement(this ObjectBase codeNode)
        {
            string text = codeNode.LsiGetWordCommand();
            if (text == null)
                return false;
            return text.Contains(KeywordGx.FOR) && text.Contains(KeywordGx.EACH) && !text.Contains(KeywordGx.LINE);
        }

        /// <summary>
        /// Check if a code node is a DO CASE block.
        /// </summary>
        /// <param name="codeNode">Code node to check</param>
        /// <returns>True if it's a DO CASE block</returns>
        static public bool LsiIsDoCase(this ObjectBase codeNode)
        {
            string text = codeNode.LsiGetWordCommand();
            if (text == null)
                return false;
            return text.StartsWith("do") && text.EndsWith("case");
        }

        /// <summary>
        /// Check if the code node is a SUB definition
        /// </summary>
        /// <param name="codeNode">Code node to check</param>
        /// <param name="subName">The subroutine name, without quotes</param>
        /// <returns>True if the node is a subroutine definition</returns>
        static public bool LsiIsSubDefinition(this ObjectBase codeNode, out string subName)
        {
            subName = null;
            Subroutine sub = codeNode as Subroutine;
            if (sub == null)
                return false;
            StringConstant subNameDef = sub.Parameters[0] as StringConstant;
            if (subNameDef == null)
                return false;

            subName = subNameDef.LsiGetTextWithoutQuotes();
            return true;
        }

        /// <summary>
        /// Check if the code node is a SUB definition
        /// </summary>
        /// <param name="codeNode">Code node to check</param>
        /// <param name="subName">The subroutine name, with quotes</param>
        /// <returns>True if the node is a subroutine definition</returns>
        static public bool LsiIsSubDefinition(this ObjectBase codeNode)
        {
            string subName;
            return codeNode.LsiIsSubDefinition(out subName);
        }

        /// <summary>
        /// Return the text of a string constant without the quotes (' or ")
        /// </summary>
        /// <param name="constant">The string constant</param>
        /// <returns>The constant without quotes (' or ")</returns>
        static public string LsiGetTextWithoutQuotes(this StringConstant constant)
        {
            string txt = constant.Text;
            return txt.Substring(1, txt.Length - 2);
        }

        /// <summary>
        /// Returns true if the string constant is untraslatable (starts with ! ex. !"foof")
        /// </summary>
        static public bool LsiIsUntranslatable(this StringConstant constant)
		{
            // TODO: It seems string not containg any letter are also untranslated, ex. " / "
            return constant.Text.Length > 0 && constant.Text[0] == '!';
        }

        /// <summary>
        /// Check if the node is a subroutine call (DO 'Subroutine')
        /// </summary>
        /// <param name="codeNode">Node to check</param>
        /// <param name="subName">The called subroutine name, without quotes</param>
        /// <returns>True if the node is a subroutine call</returns>
        static public bool LsiIsDoCommand(this ObjectBase codeNode, out string subName)
        {
            subName = null;

            CommandLine cmd = codeNode as CommandLine;
            if (cmd == null)
                return false;
            WordCommand cmdName = cmd.Name as WordCommand;
            if (cmdName == null)
                return false;
            if (cmdName.Text.ToLower() != "do")
                return false;
            StringConstant subNameConstant = cmd.Parameters[0] as StringConstant;
            if (subNameConstant == null)
                return false;

            subName = subNameConstant.LsiGetTextWithoutQuotes();
            return true;
        }

        /// <summary>
        /// Devuelve la lista de propiedades a revisar en un nodo de codigo de genexus
        /// para la busqueda en el codigo
        /// </summary>
        /// <param name="node">Nodo de codigo que se esta revisando</param>
        /// <returns>La lista de propiedades que contienen nodos hijos que pertenecen
        /// al arbol de parseado del codigo</returns>
        static private List<PropertyInfo> PropertiesToCheck(ObjectBase node)
        {
            Type tipoNodo = node.GetType();
            List<PropertyInfo> listaPropiedades;
            if (ParsingProperties.TryGetValue(tipoNodo, out listaPropiedades))
                return listaPropiedades;

            // El tipo del nodo aun no se ha revisado. Hacerlo ahora
            listaPropiedades = new List<PropertyInfo>();

            PropertyInfo[] propiedades = node.GetType().GetProperties();
            foreach (PropertyInfo propiedad in propiedades)
            {
                Type tipoPropiedad = propiedad.PropertyType;
                if (typeof(ObjectBase).IsAssignableFrom(tipoPropiedad) ||
                    typeof(ObjectBaseCollection).IsAssignableFrom(tipoPropiedad))
                    listaPropiedades.Add(propiedad);
            }
            ParsingProperties.Add(tipoNodo, listaPropiedades);
            return listaPropiedades;
        }

        /// <summary>
        /// Get the property / node children
        /// </summary>
        /// <param name="codeNode">The node where to get the children</param>
        /// <returns>The children list</returns>
        static public List<KeyValuePair<PropertyInfo, ObjectBase>> LsiGetChildrenByProperty(this ObjectBase codeNode)
        {

            List<KeyValuePair<PropertyInfo, ObjectBase>> result = new List<KeyValuePair<PropertyInfo, ObjectBase>>();

            // Buscar las propiedades que sean ObjectBase o ObjectBaseCollection del objeto
            List<PropertyInfo> properties = PropertiesToCheck(codeNode);
            if (properties.Count == 0)
                return result;

            foreach (PropertyInfo property in properties)
            {

                if (typeof(ObjectBase).IsAssignableFrom(property.PropertyType))
                {
                    // Propiedad perteneciente al arbol del parser
                    ObjectBase valor = (ObjectBase)property.GetValue(codeNode, null);
                    if (valor == null)
                        continue;

                    result.Add(new KeyValuePair<PropertyInfo, ObjectBase>(property, valor));
                }
                else
                {
                    // Es una coleccion de elementos pertenecientes al arbol del parser
                    ObjectBaseCollection collection = (ObjectBaseCollection)property.GetValue(codeNode, null);
                    if (collection == null)
                        continue;

                    foreach (ObjectBase elemento in collection)
                        result.Add(new KeyValuePair<PropertyInfo, ObjectBase>(property, elemento));
                }
            }

            return result;
        }

        /// <summary>
        /// Get the node children
        /// </summary>
        /// <param name="codeNode">The node where to get the children</param>
        /// <returns>The children list</returns>
        static public IEnumerable<ObjectBase> LsiGetChildren(this ObjectBase codeNode)
        {
            return codeNode.LsiGetChildrenByProperty().Select(x => x.Value);
        }

    }
}
