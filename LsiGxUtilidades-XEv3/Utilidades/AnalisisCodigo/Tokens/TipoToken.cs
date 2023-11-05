using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens
{
    /// <summary>
    /// Kind of token references we can search
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Reference to variable
        /// </summary>
        VARIABLE,

        /// <summary>
        /// Reference to attribute / domain
        /// </summary>
        ATTRIBUTE,

        /// <summary>
        /// Reference to variable or attribute / domain
        /// </summary>
        VARIABLE_OR_ATTRIBUTE
    };
}
