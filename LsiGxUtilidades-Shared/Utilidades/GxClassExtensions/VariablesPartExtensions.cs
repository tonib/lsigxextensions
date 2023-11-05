using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{
    /// <summary>
    /// VariablesPart extensions
    /// </summary>
    static public class VariablesPartExtensions
    {

        /// <summary>
        /// Remove a collection of variables
        /// </summary>
        /// <param name="variables">The variables part</param>
        /// <param name="variablesNames">The variables names to remove</param>
        static public void LsiRemove(this VariablesPart variables, IEnumerable<string> variablesNames) 
        {
            foreach (string varName in variablesNames)
            {
                Variable v = variables.GetVariable(varName);
                if( v != null )
                    variables.Remove(v);
            }
        }

        /// <summary>
        /// Get a variable name not assigned yet on the object
        /// </summary>
        /// <param name="variables">The variables part</param>
        /// <param name="variableName">The preferred variable name</param>
        /// <returns>A non used variable name</returns>
        static public string LsiGetUnusedVariableName(this VariablesPart variables, string variableName)
        {
            int counter = 0;
            string currentVariableName = variableName;
            while (variables.GetVariable(currentVariableName) != null)
                currentVariableName = variableName + (++counter);
            return currentVariableName;
        }

    }
}
