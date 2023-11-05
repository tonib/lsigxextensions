using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens
{
    /// <summary>
    /// Kind of indirections to search
    /// </summary>
    public enum KindOfIndirection
    {
        /// <summary>
        /// Se busca un campo del token. Puede ser un miembro de un BC/SDT (p.ej "&x.campo") 
        /// o una propiedad de una variable/atributo (p.ej. "&x.visible")
        /// </summary>
        FIELD,

        /// <summary>
        /// Se busca una llamada a una funcion miembro (p.ej. "&x.ToString()")
        /// </summary>
        FUNCTION
    };
}
