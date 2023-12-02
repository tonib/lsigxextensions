using Artech.Genexus.Common.Parts;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Variables
{

	/// <summary>
	/// Conditions variables references analysis
	/// </summary>
	class ConditionsVariablesReferences : PartVariablesReferences
    {

        /// <summary>
        /// Analyze conditions part
        /// </summary>
        /// <param name="conditions">Rules to analyze</param>
        public ConditionsVariablesReferences(ConditionsPart conditions, ObjectVariablesReferences references) :
            base(references)
        {
            ParsedCode code = new ParsedCode(conditions);
            AddVariablesToList(ReferencedVariables,
                References.BuscadorVariables.SearchAllNames(code, false));

            if (References.CheckReadWrites)
                AddVariablesToList(ReadedVariables, ReferencedVariables);
        }
    }
}
