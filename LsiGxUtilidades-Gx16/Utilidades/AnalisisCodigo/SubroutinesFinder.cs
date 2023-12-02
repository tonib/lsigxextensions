using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Language.Parser.Data;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
    // TODO: All members of this class should be static

    /// <summary>
    /// Tool to search subroutines definitions and calls
    /// </summary>
    public class SubroutinesFinder
    {

        /// <summary>
        /// Search sobroutines names called on some code
        /// </summary>
        /// <param name="code">Code to check</param>
        /// <returns>The distinct subroutines names called on the code, lowercase, without 
        /// enclosing quotes</returns>
        public HashSet<string> SearchCalledSubroutinesNames(ParsedCode code)
        {
            HashSet<string> result = new HashSet<string>();
            code.ForEach(x =>
            {
                string subName;
                if (x.LsiIsDoCommand(out subName))
                    result.Add(subName);
            });
            return result;
        }

        /// <summary>
        /// Search a subroutine definition on some code
        /// </summary>
        /// <param name="code">Code where to search the subs definitions</param>
        /// <param name="subNames">The sub names to search, with quotes ("'sub'")</param>
        /// <returns>The subroutine definition. null if it was not found</returns>
        public Subroutine SearchSubroutineDefinition(ParsedCode code, string subName)
        {
            subName = subName.ToLower();
            return (Subroutine) code.FirstOrDefault(x =>
            {
                string subDefName;
                if (!x.LsiIsSubDefinition(out subDefName))
                    return false;
                return subDefName.ToLower() == subName;
            });
        }
    }
}
