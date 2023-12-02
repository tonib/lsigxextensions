using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Genexus.Common;
using Artech.Common.Properties;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{
    /// <summary>
    /// ITypedObject extensions
    /// </summary>
    static public class ITypedObjectExtensions
    {

        /// <summary>
        /// True if the object type is alphanumeric
        /// </summary>
        static public bool LsiIsString(this ITypedObject typedObject)
        {
            return typedObject.Type == eDBType.CHARACTER || typedObject.Type == eDBType.LONGVARCHAR ||
                typedObject.Type == eDBType.VARCHAR;
        }

        /// <summary>
        /// True if the object type is numeric
        /// </summary>
        static public bool LsiIsNumeric(this ITypedObject typedObject)
        {
            return typedObject.Type == eDBType.INT || typedObject.Type == eDBType.NUMERIC;
        }

        /// <summary>
        /// True if the object type is date / datetime
        /// </summary>
        static public bool LsiIsDate(this ITypedObject typedObject)
        {
            return typedObject.Type == eDBType.DATE || typedObject.Type == eDBType.DATETIME;
        }

        /// <summary>
        /// Devuelve la longitud maxima en caracteres de un atributo / variable. Puede haber
        /// casos en que no se sepa que longitud maxima tendra realmente (p.ej. una imagen)
        /// </summary>
        /// <param name="objeto">Objeto con tipo del que obtener la longitud</param>
        /// <param name="longDefecto">Indica el valor a devolver si la funcion no puede
        /// calcular la longitud</param>
        /// <returns>La longitud maxima del campo.</returns>
        static public int LsiMaxLengthCharacters(this ITypedObject objeto, int longDefecto)
        {
            // En los strings, devolver la longitud
            if (objeto.LsiIsString())
                return objeto.Length;

            // Ver si tiene picture:
            if (objeto is IPropertyBag)
            {
                string picture = ((IPropertyBag)objeto).GetPropertyValue(Properties.ATT.Picture) as string;
                if (picture != null)
                    return picture.Length;
            }

            // Aqui empezamos con las teorias...
            if (objeto.LsiIsNumeric())
                return objeto.Length;

            if (objeto.Type == eDBType.DATE)
                // dd/mm/aaaa
                return 10;

            if (objeto.Type == eDBType.DATETIME)
                // dd/mm/aaaa hh:mm
                return 16;

            // Por devolver algo:
            return longDefecto;
        }
    }
}
