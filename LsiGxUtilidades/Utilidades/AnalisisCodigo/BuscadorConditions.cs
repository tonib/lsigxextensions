using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Language.Parser.Data;
using Artech.Genexus.Common.Parts;
using Artech.Architecture.Language.Parser.Objects;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{

    /// <summary>
    /// Utilidad para enumerar condiciones una parte de conditions o en un grid
    /// </summary>
    public class BuscadorConditions
    {

        /// <summary>
        /// Parsea la parte de conditions de un objeto y devuelve la lista de condiciones aisladas
        /// </summary>
        /// <param name="parteCondiciones">La parte de condiciones a parsear</param>
        /// <returns>La lista de condiciones</returns>
        static public List<Condition> EnumerarCondiciones(ConditionsPart parteCondiciones)
        {
            return EnumerarCondiciones( new ParsedCode(parteCondiciones) );
        }

        /// <summary>
        /// Parsea un codigo de conditions y devuelve la lista de condiciones aisladas
        /// </summary>
        /// <param name="codigo">El codigo a analizar</param>
        /// <returns>La lista de condiciones</returns>
        static public List<Condition> EnumerarCondiciones(ParsedCode codigo)
        {
            return codigo.ArbolParseado.LsiGetRootNodes().OfType<Condition>().ToList();
        }

        /// <summary>
        /// Parsea el codigo de conditions de un grid y devuelve la lista de condiciones aisladas
        /// </summary>
        /// <param name="codigo">El codigo a analizar</param>
        /// <returns>La lista de condiciones</returns>
        static public List<Condition> EnumerarCondiciones(GridElement grid)
        {

            // Ver si el grid tiene puestas condiciones:
            ConditionType conditionsGrid = grid.GetPropertyValue(Properties.FORMSFL.Conditions) as ConditionType;
            if (conditionsGrid == null || string.IsNullOrEmpty(conditionsGrid.Data))
                return new List<Condition>();

            return EnumerarCondiciones(new ParsedCode(grid.KBObject, conditionsGrid));
        }
        
    }
}
