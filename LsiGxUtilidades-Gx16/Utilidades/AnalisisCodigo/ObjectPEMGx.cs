using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Language.Parser.Data;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
    /// <summary>
    /// Utilidades de expresiones de indireccion de un objeto en genexus. P.ej. "&sdt.Campo.item(1)".
    /// </summary>
    public class ObjectPEMGx
    {

        /// <summary>
        /// Obtiene la indireccion base (la primera indireccion) de un ObjectPEM
        /// </summary>
        /// <remarks>
        /// Por ejemplo, para una expresion "atributo.campo.funcion().campo2" devuelve 
        /// "atributo.campo"
        /// </remarks>
        /// <param name="expresion">La expresion de la que obtener la indireccion base</param>
        /// <returns>La indireccion base</returns>
        static public ObjectPEM ObtenenerExpresionBase(ObjectPEM expresion)
        {
            ObjectPEM expBase = expresion;
            while (expBase.Target is ObjectPEM)
                expBase = (ObjectPEM) expBase.Target;
            return expBase;
        }

        /// <summary>
        /// Verifica si una indireccion contiene alguna llamada a funcion.
        /// </summary>
        /// <param name="expresion">Expresion a revisar</param>
        /// <returns>Cierto si la expresion contiene alguna llamada a funcion</returns>
        static public bool ContieneLlamadaFuncion(ObjectPEM expresion)
        {

            if (expresion.PEMExpression is Function)
                return true;

            ObjectBase exp = expresion.Target;
            while( exp is ObjectPEM )
            {
                expresion = (ObjectPEM) exp;
                if (expresion.PEMExpression is Function)
                    return true;
                exp = expresion.Target;
            }
            return false;
        }

        /// <summary>
        /// Verifica si una indireccion contiene alguna llamada a funcion.
        /// </summary>
        /// <param name="expresion">Indirecciones de la expresion a revisar</param>
        /// <returns>Cierto si la expresion contiene alguna llamada a funcion</returns>
        static public bool ContieneLlamadaFuncion(List<ObjectBase> indirecciones)
        {
            foreach (ObjectBase indireccion in indirecciones)
            {
                if (indireccion is Function)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Convierte la cadena de indirecciones en una lista. P.ej. "atributo.campo.funcion()" se
        /// devuelve como { "atributo" , "campo" , "funcion()".
        /// </summary>
        /// <param name="expresion">Expresion a convertir en lista</param>
        /// <returns>La lista de indirecciones</returns>
        static public List<ObjectBase> ConvertirALista(ObjectPEM expresion)
        {
            List<ObjectBase> indirecciones = new List<ObjectBase>();
            ObjectBase actual = expresion;
            while (actual is ObjectPEM)
            {
                ObjectPEM pem = (ObjectPEM)actual;
                indirecciones.Insert(0, pem.PEMExpression);
                actual = pem.Target;
            }
            indirecciones.Insert(0, actual);

            return indirecciones;
        }

        /// <summary>
        /// Get a qualified name from a PEM expression components
        /// </summary>
        /// <param name="expressionComponents">The PEM expression components</param>
        /// <param name="ignoreLast">True if the last component should not be included on the 
        /// qualified name</param>
        /// <returns>The qualified name string, lowercase</returns>
        static public string GetQualifiedName(List<ObjectBase> expressionComponents, bool ignoreLast)
        {
            string objectName = string.Empty;
            int endIdx = expressionComponents.Count;
            if (ignoreLast)
                endIdx--;

            for (int i = 0; i < endIdx; i++)
            {
                if (i > 0)
                    objectName += ".";
                Word word = expressionComponents[i] as Word;
                if (word != null)
                    objectName += word.Text;
                else
                {
                    Function function = expressionComponents[i] as Function;
                    if (function != null)
                    {
                        Word functionName = function.Name as Word;
                        objectName += functionName.Text;
                    }
                    else
                        objectName += expressionComponents[i].ToString();
                }
            }
            return objectName;
        }

        // TODO: Remove this function?
        /// <summary>
        /// Get a qualified name from a PEM expression components
        /// </summary>
        /// <param name="expression">The PEM expression</param>
        /// <param name="ignoreLast">True if the last component should not be included on the 
        /// qualified name</param>
        /// <returns>The qualified name string, lowercase</returns>
        static public string GetQualifiedName(ObjectPEM expression, bool ignoreLast)
        {
            return GetQualifiedName(ConvertirALista(expression), ignoreLast);
        }
    }
}
