using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Objects;
using Artech.Architecture.Language.Parser.Data;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common;
using LSI.Packages.Extensiones.Utilidades.Variables;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens
{

    /// <summary>
    /// Define un filtro de los tipos de tokens a buscar.
    /// <remarks>
    /// Por ejemplo, permite buscar solo tokens basados en uno o mas BC o un SDT.
    /// De momento solo se pueden filtrar variables.
    /// El filtro acepta cualquier token que tenga alguno de los tipos de listados en este filtro.
    /// </remarks>
    /// </summary>
    public class FiltroTiposToken
    {

        /// <summary>
        /// Lista de SDTs en los que puede estas basado la base de la expresion del token 
        /// </summary>
        public List<SDT> Sdts = new List<SDT>();

        /// <summary>
        /// Lista de BC en los que puede estas basado la base de la expresion del token 
        /// </summary>
        public List<Transaction> Bcs = new List<Transaction>();

        public bool CumpleFiltro(VariablesPart variables, string nombreVariable)
        {
            if (variables == null)
                return false;

            Variable variable = variables.GetVariable(nombreVariable);
            if (variable == null)
                return false;

            foreach (SDT sdt in Sdts)
            {
                if (VariableGX.EsSdt(variables.Model, variable, sdt, true))
                    return true;
            }

            foreach (Transaction bc in Bcs)
            {
                if (VariableGX.EsBc(variables.Model, variable, bc, true))
                    return true;
            }

            return false;
        }

        public bool CumpleFiltro(VariablesPart variables, ObjectBase campo)
        {
            VariableName nombreVariable = campo as VariableName;
            if (nombreVariable == null)
                return false;

            return CumpleFiltro(variables, nombreVariable.Text);
        }

        public IEnumerable<KBObject> ObjetosFiltro()
        {
            foreach (SDT sdt in Sdts)
                yield return sdt;
            foreach (Transaction bc in Bcs)
                yield return bc;
        }

    }
}
