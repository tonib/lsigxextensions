using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens
{
    /// <summary>
    /// Stores token names referenced from controls by its type
    /// </summary>
    public class ControlsTokenReferences
    {
        /// <summary>
        /// Token names that reference directly and only to the token
        /// </summary>
        public HashSet<string> HardReferencedNames = new HashSet<string>();

        /// <summary>
        /// Token names that reference through code inside control properties
        /// </summary>
        public HashSet<string> CodeReferencedNames = new HashSet<string>();

        /// <summary>
        /// Get all referenced token names
        /// </summary>
        public HashSet<string> AllReferences
        {
            get
            {
                HashSet<string> result = new HashSet<string>(HardReferencedNames);
                result.LsiAddRange(CodeReferencedNames);
                return result;
            }
        }

        /// <summary>
        /// Add a set of references to this
        /// </summary>
        /// <param name="references">References to add</param>
        public void AddReferences(ControlsTokenReferences references)
        {
            HardReferencedNames.LsiAddRange(references.HardReferencedNames);
            CodeReferencedNames.LsiAddRange(references.CodeReferencedNames);
        }
    }
}
