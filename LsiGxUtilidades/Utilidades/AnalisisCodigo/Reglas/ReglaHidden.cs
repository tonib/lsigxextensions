using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades.WinForms;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common.Parts.Form;
using Artech.Genexus.Common.Types;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Architecture.Language.Parser.Data;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas
{

    /// <summary>
    /// Utilidad para borrar una regla hidden de un workpanel y poner los controles en el grid
    /// como campos ocultos
    /// </summary>
    public class ReglaHidden
    {

        /// <summary>
        /// Busca una regla hidden en un workpanel
        /// </summary>
        /// <param name="workpanel">Workpanel en el que buscar la regla hidden</param>
        /// <param name="reglas">Si se ha encontrado la regla, es la parte de reglas del objeto</param>
        /// <returns>La regla hidden. null si no se ha encontrado</returns>
        static private Rule BuscarReglaHidden(WorkPanel workpanel, out RulesPart reglas, out ParsedCode analizador)
        {
            analizador = null;
            reglas = workpanel.Parts.LsiGet<RulesPart>();
            if (reglas == null)
                // No hay reglas
                return null;

            // Buscar la regla hidden
            BuscadorReglas buscador = new BuscadorReglas(BuscadorReglas.REGLAHIDDEN);
            analizador = new ParsedCode(reglas);
            return buscador.BuscarPrimera(analizador);
        }

        /// <summary>
        /// Revisa si un workpanel contiene una regla hidden
        /// </summary>
        /// <param name="workpanel">Workpanel a revisar</param>
        /// <returns>Cierto si el objeto contiene una regla hidden</returns>
        static public bool ContieneReglaHidden(WorkPanel workpanel)
        {
            RulesPart reglas;
            ParsedCode analizador;
            return BuscarReglaHidden(workpanel, out reglas, out analizador) != null;
        }

        /// <summary>
        /// Borra una regla hidden y pone su contenido en el unico subfile del workpanel.
        /// No guarda los cambios en el objeto (no hace un save())
        /// </summary>
        /// <param name="workpanel">Workpanel en el que borrar la regla hidden</param>
        static public void BorrarReglaHidden(Log log, WorkPanel workpanel)
        {

            RulesPart reglas;
            ParsedCode analizador;
            Rule reglaHidden = BuscarReglaHidden(workpanel, out reglas, out analizador);
            if (reglaHidden == null)
                return;

            // Quitar la regla:
            if (!BuscadorReglas.BorrarRegla(analizador, reglaHidden))
                throw new Exception("Hidden rule to remove not found");

            reglas.Source = analizador.ArbolParseado.ToString();

            // Buscar el subfile del workpanel
            WinFormGx winForm = new WinFormGx(workpanel);
            List<GridElement> grids = winForm.BuscarGrids();

            // Si no hay ningun grid o hay mas de uno, no hay que tocar la UI
            if (grids.Count == 0)
            {
                log.Output.AddWarningLine("No grid found ind workpanel. Hidden rule deleted with no more actions");
                return;
            }
            if (grids.Count > 1)
            {
                log.Output.AddWarningLine("There are more than one grid. Hidden rule deleted with no more actions");
                return;
            }
            GridElement grid = grids[0];

            // Poner cada uno de los elementos en el grid
            VariablesPart variables = workpanel.Parts.LsiGet<VariablesPart>();

            foreach (Word elemento in BuscadorReglas.ParametrosRegla(reglaHidden) )
            {
                string nombreElemento = elemento.Text.ToLower();

                // Ver si el elemento ya esta en el grid
                bool yaExiste = false;
                foreach (GridColumnElement elementoGrid in grid.Children)
                {
                    string nombreElementoGrid = elementoGrid.Attribute.Name;
                    if (nombreElementoGrid.ToLower() == nombreElemento)
                    {
                        log.Output.AddWarningLine("Element " + elemento + " already is contained by the grid. Not added");
                        yaExiste = true;
                        break;
                    }
                }
                if (yaExiste)
                    continue;

                log.Output.AddLine("Element " + elemento + " added to the grid as invisible");
                GridColumnElement columna = FormFactory.CreateFormElement(FormType.Windows, RuntimeControlType.CTRL_COLUMN, workpanel) as GridColumnElement;
                ITypedObject e;
                if (nombreElemento.StartsWith("&"))
                    e = variables.GetVariable(nombreElemento.Substring(1));
                else
                    e = Artech.Genexus.Common.Objects.Attribute.Get(workpanel.Model, nombreElemento);
                if( e == null ) 
                    throw new Exception("Element " + elemento + " to remove from hidden rule not found");

                columna.SetPropertyValue(Properties.FORMSFC.Attribute, new AttributeVariableReference(e));
                columna.SetPropertyValue(Properties.FORMSFC.Visible, false);
                columna.SetPropertyValue(Properties.FORMSFC.Name, elemento);
                grid.AddElement(columna);
            }
            // Indicar que se ha modifcado el winform. Si no, no se guarda el cambio?
            // TODO: Ver si esto es necesario...
            winForm.ParteWinForm.Dirty = true;

        }
    }
}
