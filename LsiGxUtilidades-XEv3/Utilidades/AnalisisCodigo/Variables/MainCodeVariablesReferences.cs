using Artech.Architecture.Common.Objects;
using System.Collections.Generic;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Variables
{
	/// <summary>
	/// Procedure / events variables references analysis
	/// </summary>
	class MainCodeVariablesReferences : PartVariablesReferences
    {

        /// <summary>
        /// Written variables passed as parameter to object calls, with out: or inout: parameter
        /// qualifier.
        /// </summary>
        public HashSet<string> VariablesEscritasLlamadas = new HashSet<string>();

        /// <summary>
        /// Written variables passed as parameter to object calls, with out: parameter qualifier.
        /// </summary>
        public HashSet<string> CallsWrittenVariablesOut = new HashSet<string>();

        /// <summary>
        /// Written variables on code assignments
        /// </summary>
        public HashSet<string> AssignmentWrittenVariables = new HashSet<string>();

        /// <summary>
        /// Lista de mensajes de error por n. de parametros en llamadas incorrecto
        /// </summary>
        public List<string> ErroresEnLlamadas = new List<string>();

        /// <summary>
        /// Analyze form part
        /// </summary>
        /// <param name="codePart">Events / procedure to analyze. It can be null</param>
        public MainCodeVariablesReferences(KBObjectPart codePart, ObjectVariablesReferences references) :
            base(references)
        {
            if (codePart == null)
                return;

            ParsedCode code = new ParsedCode(codePart);
            HashSet<string> referenced = References.BuscadorVariables.SearchAllNames(code, false);
            AddVariablesToList(ReferencedVariables, referenced);

            if (References.CheckReadWrites)
                CheckReadWrites(code);
        }

        private void CheckReadWrites(ParsedCode code)
        {
            BuscadorLecturasEscrituras buscador = new BuscadorLecturasEscrituras();
            BuscadorLecturasEscrituras.InformacionBusqueda info =
                buscador.ObtenerInformacionBusqueda(code);

            // Guardar las variables leidas y escritas
            HashSet<string> readVariables = buscador.VariablesLeidas(info);
            AddVariablesToList(ReadedVariables, readVariables);
            AddVariablesToList(WrittenVariables, buscador.VariablesEscritas(info));
            AddVariablesToList(VariablesEscritasLlamadas,
                buscador.NombresVariablesEscritasLlamadas(info));
            AddVariablesToList(CallsWrittenVariablesOut,
                buscador.WrittenVariableNamesOut(info));

            // There are troubles here: If we are on a form, &Variable.Visible = 0 is found as an
            // assignment, but isn't. So, ignore them:
            AddVariablesToList(AssignmentWrittenVariables,
                buscador.WrittenVariableNamesAssigments(info, KBaseGX.IsForm(References.Object)));

            // Hack for external code variable references: They are not on referenced variables set, but they
            // are on readVariables. Add them now:
            AddVariablesToList(ReferencedVariables, readVariables);

            // Errores en llamadas:
            ErroresEnLlamadas = buscador.LlamadasNParametrosIncorrecto(info);
        }

    }
}
