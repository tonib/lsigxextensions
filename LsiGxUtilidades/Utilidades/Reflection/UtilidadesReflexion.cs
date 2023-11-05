using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Artech.Packages.Patterns.Objects;
using Artech.Common.Properties;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Utilidades.Reflection
{
    /// <summary>
    /// Utilidades de reflexion de .NET para buscar clases en el runtime de GeneXus.
    /// </summary>
    public class UtilidadesReflexion
    {
        
        /// <summary>
        /// Escribe en la consola la lista de clases que implementan una interface
        /// </summary>
        /// <param name="type">Tipo de la interface de la que buscar implementaciones</param>
        static public void BuscarClasesImplementanInterface(Type type)
        {
            Assembly[] ensamblados = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly ensamblado in ensamblados)
            {
                try
                {
                    var types = ensamblado.GetTypes()
                    .Where(p => type.IsAssignableFrom(p));
                    foreach (Type t in types)
                    {
                        string nombre = t.ToString();
                        System.Console.WriteLine(nombre);
                    }
                }
                catch { }
            }
        }

        static public void BuscarEnsambladoPorNombreClase(string nombreClase)
        {
            Assembly[] ensamblados = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly ensamblado in ensamblados)
            {
                try
                {
                    var types = ensamblado.GetTypes()
                    .Where(p => p.Name == nombreClase);
                    foreach (Type t in types)
                    {
                        string nombre = t.Assembly.ToString();
                        System.Console.WriteLine(nombre);
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Devuelve las constantes declaradas en una clase de un cierto tipo
        /// </summary>
        /// <param name="clase">Clase de la que obtener las constantes</param>
        /// <param name="tipoConstante">Tipo de la constantes a buscar</param>
        /// <returns>La lista de constantes</returns>
        static public List<FieldInfo> ObtenerConstantes(Type clase, Type tipoConstante)
        {
            FieldInfo[] fields = clase.GetFields(BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.FlattenHierarchy);

            return fields.Where(fieldInfo => fieldInfo.IsLiteral
                && !fieldInfo.IsInitOnly
                && fieldInfo.FieldType == tipoConstante).ToList();
        }

        /// <summary>
        /// Devuelve el valor de una constante declarada en una clase
        /// </summary>
        /// <param name="clase">Clase que declara la constante</param>
        /// <param name="nombreConstante">Nombre de la constante</param>
        /// <returns>El valor de la constante. null si no se encontro la constante</returns>
        static public object ObtenerValorConstante(Type clase, string nombreConstante) 
        {
            FieldInfo field = clase.GetField(nombreConstante, BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.FlattenHierarchy);
            if (field == null)
                return null;
            if (field.IsLiteral && !field.IsInitOnly)
                return field.GetRawConstantValue();
            else
                return null;
        }

        /// <summary>
        /// Writes on the gx output the content of a pattern element
        /// </summary>
        /// <param name="element">Element to inspect</param>
        static public void InspectPatternElement(PatternInstanceElement element)
        {
            using (Log log = new Log())
            {
                InspectPatternElement(log, element, 0);
            }
        }

        static private void InspectPatternElement(Log log, PatternInstanceElement element, int level)
        {
            string tabs = new String('\t' , level);
            level++;

            log.Output.AddLine(string.Format("{0}*** {1} : {2} ({3}) ***" ,
                tabs , element.Name , element.ToString() , element.Type ));

            foreach (Property p in element.Attributes.Properties)
            {
                object value = element.Attributes[p.Name];
                if(value == null)
                    value = "[null]";
                string typeName = value == null ? "[null]" : value.GetType().Name;

                log.Output.AddLine(string.Format("{0}{1} = {2} ({3})", tabs,
                    p.Name, value.ToString(), typeName));
            }

            foreach (PatternInstanceElement child in element.Children)
                InspectPatternElement(log, child, level);
        }

    }
}
