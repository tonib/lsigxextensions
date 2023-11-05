using Artech.Genexus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{
    /// <summary>
    /// Variable class extensions
    /// </summary>
    static public class VariableExtensions
    {

        /// <summary>
        /// Create a variable clone, changing it's name
        /// </summary>
        /// <param name="variable">This variable</param>
        /// <param name="newName">New variable name</param>
        /// <returns>Cloned variable</returns>
        static public Variable LsiCloneRenamed(this Variable variable, string newName)
        {
            Variable v = new Variable(newName, variable.Part);
            v.CopyPropertiesFrom(variable);
            return v;
        }

        static public bool LsiHasDefaultType(this Variable v)
        {
            // This dont work:
            //return v.IsPropertyDefault(Properties.ATT.BasedOnAttribute) && v.IsPropertyDefault(Properties.ATT.DataType);
            return v.Type == eDBType.NUMERIC && v.Length == 4 && v.Decimals == 0 && v.AttributeBasedOn == null &&
                v.DomainBasedOn == null;
        }
    }
}
