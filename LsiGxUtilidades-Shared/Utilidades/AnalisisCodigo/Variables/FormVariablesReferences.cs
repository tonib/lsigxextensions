using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Patterns.WorkWithDevices.Parts;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Variables
{
    /// <summary>
    /// Form variables references analysis
    /// </summary>
    class FormVariablesReferences : PartVariablesReferences
    {

        /// <summary>
        /// References to variables detail information
        /// </summary>
        private ControlsTokenReferences ReferencesDetail;

        /// <summary>
        /// Analyze form part
        /// </summary>
        /// <param name="rules">Rules to analyze</param>
        public FormVariablesReferences(KBObjectPart form, ObjectVariablesReferences references) :
            base(references)
        {
            if (form is WinFormPart)
                ReferencesDetail = References.BuscadorVariables
                    .SearchAllNamesWithDetail((WinFormPart)form, false);
            else if (form is WebFormPart)
                ReferencesDetail = References.BuscadorVariables
                    .SearchAllNamesWithDetail((WebFormPart)form, false);
            else if (form is VirtualLayoutPart)
                ReferencesDetail = References.BuscadorVariables
                    .SearchAllNamesWithDetail((VirtualLayoutPart)form, false);
            else
                throw new Exception("Part is not a form");

            AddVariablesToList(ReferencedVariables, ReferencesDetail.AllReferences);
            if (References.CheckReadWrites)
            {
                // Variables on form are readed / written. This is false when variables are readonly,
                // but it's a safe aproximation that dont give wrong warnings
                AddVariablesToList(ReadedVariables, ReferencesDetail.HardReferencedNames);
                AddVariablesToList(WrittenVariables, ReferencesDetail.HardReferencedNames);

                // Variables on code properties are read only:
                AddVariablesToList(ReadedVariables, ReferencesDetail.CodeReferencedNames);
            }
        }

        /// <summary>
        /// Check variables that are only on the form part and must to be reported. Variables that
        /// appear on form and in code properties will be ignored
        /// </summary>
        /// <param name="variables">Variables to check</param>
        /// <returns>Variables that must to be reported</returns>
        public List<string> VariablesOnlyOnForm(List<string> variables)
        {
            return variables.Where(v =>
                ReferencesDetail.HardReferencedNames.Contains(v) ^
                ReferencesDetail.CodeReferencedNames.Contains(v)
            )
            .ToList();
        }

    }
}
