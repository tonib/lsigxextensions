using Artech.Genexus.Common.Parts;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Variables
{
	/// <summary>
	/// Report layout variables references analysis
	/// </summary>
	class ReportLayoutVariablesReferences : PartVariablesReferences
    {
        /// <summary>
        /// Analyze conditions part
        /// </summary>
        /// <param name="conditions">Rules to analyze</param>
        public ReportLayoutVariablesReferences(LayoutPart layout, ObjectVariablesReferences references) :
            base(references)
        {
            AddVariablesToList(ReferencedVariables,
                References.BuscadorVariables.SearchAllNames(layout, false));

            if (References.CheckReadWrites)
                AddVariablesToList(ReadedVariables, ReferencedVariables);
        }

    }
}
