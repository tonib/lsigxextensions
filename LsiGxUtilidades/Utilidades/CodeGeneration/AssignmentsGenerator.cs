using System.Collections.Generic;
using System.Text;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.SDT;
using Artech.Common.Helpers.Structure;

namespace LSI.Packages.Extensiones.Utilidades.CodeGeneration
{
    /// <summary>
    /// Utilidad para generar sentencias de asignaciones entre variables y atributos.
    /// </summary>
    public class AssignmentsGenerator
    {

        /// <summary>
        ///  Devuelve una asignacion [nombreAtributo] = &amp;[nombreAtributo]
        /// </summary>
        static public string AsignacionAtributo(string nombreAtributo)
        {
            return string.Format("{0} = &{0}", nombreAtributo);
        }

        /// <summary>
        ///  Devuelve una asignacion [nombreAtributo] = &amp;[nombreVariableSdt].[nombreAtributo]
        /// </summary>
        /// <param name="nombreVariableSdt">Nombre de la variable a la que hacer la asignacion.
        ///  Si es nulo, es como si fuera una asignacion de variable a atributo</param>
        /// <param name="nombreAtributo">Nombre del atributo a asignar</param>
        static public string AsignacionAtributo(string nombreVariableSdt, string nombreAtributo)
        {
            if( nombreVariableSdt == null )
                return AsignacionAtributo(nombreAtributo);

            return string.Format("{0} = &{1}.{0}", nombreAtributo, nombreVariableSdt);
        }

        /// <summary>
        /// Devuelve un string con asignaciones de los campos de una tabla a un sdt.
        /// </summary>
        /// <param name="tablaOrigen">Tabla origen de la copia de datos</param>
        /// <param name="variableSdt">Nombre de la variable del tipo sdt en la que se copiaran los datos
        /// de la tabla</param>
        /// <param name="sdtDestino">Sdt donde se van a copiar los datos del registro de la tabla</param>
        /// <param name="avisos">Si algun campo del sdt no se encuentra en la tabla, se añadira 
        /// un string con el aviso en esta tabla</param>
        /// <returns>El string con las asignaciones</returns>
        static public string AsignacionesDeTablaASdt(Table tablaOrigen, Variable variableSdt, SDT sdtDestino, List<string> avisos)
        {
            StringBuilder sb = new StringBuilder();
            // Recorrer los campos del sdt:
            foreach (IStructureItem si in sdtDestino.SDTStructure.Root.Items)
            {
                SDTItem item = si as SDTItem;
                if (item == null)
                {
                    avisos.Add("No se soportan niveles del SDT: Item " + si.Name + " ignorado");
                    continue;
                }

                // Buscar el nombre del campo del sdt en la tabla:
                TableAttribute atr = tablaOrigen.TableStructure.Attributes.Find(x => x.Name == item.Name);
                if (atr == null)
                {
                    avisos.Add("No se ha encontrado un campo llamado " + item.Name + " en la tabla " + tablaOrigen.Name);
                    continue;
                }
                sb.AppendFormat("&{0}.{1} = {1}\n", variableSdt.Name, atr.Name);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Devuelve un string con asignaciones de los campos de una tabla a un sdt.
        /// </summary>
        /// <param name="variableSdt">Nombre de la variable del tipo sdt desde la que se copiaran los datos
        /// de la tabla</param>
        /// <param name="sdtDestino">Sdt de origen de la copia de los datos del registro de la tabla</param>
        /// <param name="tablaOrigen">Tabla origen de la copia de datos</param>
        /// <param name="incluirClaveTabla">Cierto si hay que copiar tambien los atributos de la 
        /// clave primaria de la tabla.</param>
        /// <param name="avisos">Si algun campo de la tabla no se encuentra en el sdt, se añadira 
        /// un string con el aviso en esta tabla</param>
        /// <returns>El string con las asignaciones</returns>
        static public string AsignacionesDeSdtATabla(Variable variableSdt, SDT sdtDestino,
            Table tablaOrigen, bool incluirClaveTabla, List<string> avisos)
        {

            StringBuilder sb = new StringBuilder();

            // Obtener la lista de items de la raiz del sdt:
            List<SDTItem> items = new List<SDTItem>();
            foreach (IStructureItem i in sdtDestino.SDTStructure.Root.Items)
            {
                SDTItem item = i as SDTItem;
                if( item != null )
                    items.Add(item);
            }

            // Recorrer los campos de la tabla
            foreach (TableAttribute atr in tablaOrigen.TableStructure.Attributes)
            {
                if (atr.IsFormula)
                    // No esta guardado en la tabla
                    continue;

                if (!incluirClaveTabla && tablaOrigen.TableStructure.PrimaryKey.Contains(atr))
                    // El atributo es clave y no hay que incluir la clave.
                    continue;

                // Buscar el atributo en el sdt:
                SDTItem item = items.Find(x => x.Name == atr.Name);
                if (item == null)
                {
                    avisos.Add(string.Format("No field with name {0} found in {1} SDT", atr.Name, sdtDestino.Name));
                    continue;
                }

                sb.AppendFormat("{0} = &{1}.{0}\n", atr.Name, variableSdt.Name);
            }

            return sb.ToString();
        }

    }
}
