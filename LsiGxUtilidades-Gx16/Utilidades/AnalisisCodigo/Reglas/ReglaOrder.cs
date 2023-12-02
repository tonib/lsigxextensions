using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Architecture.Language.Parser.Data;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas
{

    /// <summary>
    /// Utilidades de la regla order
    /// </summary>
    public class ReglaOrder
    {

        /// <summary>
        /// Devuelve la lista de orders aplicables a un grid.
        /// Puede haber mas de uno si se usa la clausula WHEN
        /// </summary>
        /// <param name="parteReglas">Parte de reglas del objeto donde esta el grid</param>
        /// <param name="grid">El grid del que obtener el order</param>
        /// <returns>La lista de orders aplicables al grid.</returns>
        static public List<string> ObtenerOrders(RulesPart parteReglas, GridElement grid)
        {
            List<string> orders = new List<string>();

            // Ver si el grid tiene la propiedad orders asignada:
            if (grid != null)
            {
                OrderType orderGrid = grid.GetPropertyValue(Properties.FORMSFL.Order) as OrderType;
                if (orderGrid != null && !string.IsNullOrEmpty(orderGrid.Data) )
                {
                    // El grid tiene orden. Parsearlo y usarlo:
                    string txtOrderGrid = Entorno.StringFormatoKbase(orderGrid.Data);
                    string[] ordersParseados = txtOrderGrid.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string o in ordersParseados)
                    {
                        string aux = o.Trim();
                        if (!string.IsNullOrEmpty(aux))
                            orders.Add(aux);
                    }
                    return orders;
                }
            }

            // Ver si existe una regla order:
            BuscadorReglas buscadorReglaOrder = new BuscadorReglas(BuscadorReglas.REGLAORDER);
            Rule reglaOrder = buscadorReglaOrder.BuscarPrimera(new ParsedCode(parteReglas));
            if (reglaOrder != null)
                orders.Add(reglaOrder.Parameters.ToString().Trim());

            return orders;
        }

    }
}
