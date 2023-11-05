using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Language.Parser.Data;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas
{
    /// <summary>
    /// Utilidades de la regla default
    /// </summary>
    public class ReglaDefault
    {

        /// <summary>
        /// Devuelve el elemento asignado en una regla default
        /// </summary>
        /// <param name="reglaDefault">La regla default a analizar</param>
        /// <returns>El elemento asignado. null si no se ha encontrado</returns>
        static public Word ObtenerElementoAsignado(Rule reglaDefault)
        {
            if( reglaDefault.Parameters.Count < 1 )
                return null;
            return reglaDefault.Parameters[0] as Word;
        }

    }
}
