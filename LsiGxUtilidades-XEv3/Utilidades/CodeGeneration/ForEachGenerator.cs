using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Architecture.Language.Parser.Data;

namespace LSI.Packages.Extensiones.Utilidades.CodeGeneration
{

    /// <summary>
    /// Tool to generate a FOR EACH statement
    /// </summary>
    public class ForEachGenerator
    {

        /// <summary>
        /// La lista de wheres a añadir a las condiciones del for each
        /// Los textos no deben contener el "WHERE". P.ej. debe ser "a = &a"
        /// </summary>
        public List<string> ListaWheres = new List<string>();

        /// <summary>
        /// La lista de orders a poner en el for each. Puede haber mas de uno en caso que se
        /// usen clausulas WHEN
        /// </summary>
        public List<string> ListaOrders = new List<string>();

        /// <summary>
        /// List of attribute names to include in the DEFINED BY
        /// </summary>
        public List<string> DefinedByList = new List<string>();

        /// <summary>
        /// Codigo interno del for each, sin tabular
        /// </summary>
        public string CodigoInterno = String.Empty;

        /// <summary>
        /// Codigo del when none. Si esta vacio, no se genera
        /// </summary>
        public string CodigoWhenNone = String.Empty;

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public ForEachGenerator() { }

        /// <summary>
        /// Constructor. 
        /// Crea un for each para leer un registro de la tabla por su clave primaria
        /// </summary>
        /// <param name="tabla">Tabla de la que cargar el registro</param>
        public ForEachGenerator(Table tabla)
        {
            AgregarWheresRegistroTabla(null, tabla);
        }

        /// <summary>
        /// Constructor. 
        /// Crea un for each para leer un registro de la tabla por su clave primaria
        /// </summary>
        /// <param name="tabla">Tabla de la que cargar el registro</param>
        public ForEachGenerator(string nombreVariableSdt, Table tabla)
        {
            AgregarWheresRegistroTabla(nombreVariableSdt, tabla);
        }

        /// <summary>
        /// Create for each from a Data Selector
        /// </summary>
        /// <param name="ds">The equivalent data selector</param>
        public ForEachGenerator(DataSelector ds)
        {
            // Orders
            foreach (DataSelectorOrderItem orderItem in ds.DataSelectorStructure.GetOrders())
                this.ListaOrders.Add(orderItem.Order.ToString());

            // Conditions
            foreach (DataSelectorCondition c in ds.DataSelectorStructure.GetConditions())
                this.ListaWheres.Add(c.ToString());

            // Defined by
            DefinedByList.AddRange(ds.DataSelectorStructure.GetAttributes().Select(x => x.Name));
        }

        /// <summary>
        /// Indenta (pone tabuladores) las lineas de un texto
        /// </summary>
        /// <param name="codigoInterno">Texto a indentar</param>
        /// <returns>Texto indentado</returns>
        static public string IndentarCodigo(string codigoInterno) {
            
            StringBuilder sb = new StringBuilder();
            codigoInterno = Entorno.StringFormatoKbase(codigoInterno);
            string[] lineas = codigoInterno.Split(new string[] { Environment.NewLine }, StringSplitOptions.None );
            foreach (string linea in lineas)
            {
                sb.Append("\t");
                sb.AppendLine(linea);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Devuelve el codigo del FOR EACH
        /// </summary>
        public override string ToString()
        {
            // Añadir el for each
            StringBuilder sb = new StringBuilder(KeywordGx.FOREACH.ToUpper());
            
            // Añadir orders...
            for (int i = 0; i < ListaOrders.Count; i++)
            {
                if (i > 0)
                    sb.Append("        ");
                sb.Append(" ORDER ");
                sb.AppendLine(ListaOrders[i]);
            }
            if (ListaOrders.Count == 0)
                sb.AppendLine();

            // Añadir wheres
            foreach (string where in ListaWheres)
            {
                sb.Append("\tWHERE ");
                sb.AppendLine(where);
            }
            sb.AppendLine();

            // Add defined by
            if (DefinedByList.Count > 0)
            {
                sb.Append("\tDEFINED BY ");
                sb.Append(string.Join(", ", DefinedByList.ToArray()));
                sb.AppendLine();
            }

            // Añadir el codigo tabulado
            sb.Append(IndentarCodigo(CodigoInterno));

            if (!string.IsNullOrEmpty(CodigoWhenNone))
            {
                sb.AppendLine("WHEN NONE");
                string whenNone = IndentarCodigo(CodigoWhenNone);
                if (!whenNone.EndsWith("\n"))
                    whenNone += "\n";
                sb.Append(whenNone);
            }

            sb.AppendLine(KeywordGx.ENDFOR.ToUpper());
            return Entorno.StringFormatoKbase( sb.ToString() );
        }

        /// <summary>
        /// Añade un WHERE Atributo = &Atributo
        /// </summary>
        /// <param name="nombreAtributo">Nombre del atributo del que añadir la asignacion</param>
        public void AgregarWhereAsignacion(string nombreVariableSdt, string nombreAtributo)
        {
            ListaWheres.Add(AssignmentsGenerator.AsignacionAtributo(nombreVariableSdt, nombreAtributo));
        }

        /// <summary>
        /// Agrega una lista de condiciones al where del for each
        /// </summary>
        /// <param name="condiciones">La lista de condiciones a agregar</param>
        public void AgregarWheres(List<string> condiciones)
        {
            ListaWheres.AddRange(condiciones);
        }

        /// <summary>
        /// Agrega una lista de condiciones al where del for each
        /// </summary>
        /// <param name="condiciones">La lista de condiciones a agregar</param>
        public void AgregarWheres(List<Condition> condiciones)
        {
            ListaWheres.AddRange(condiciones.Select(c => c.Expression.ToString()).ToList());
        }

        /// <summary>
        /// Agrega una lista de orders al for each.
        /// </summary>
        /// <param name="orders">La lista de orders a agregar</param>
        public void AgregarOrders(List<string> orders)
        {
            ListaOrders.AddRange(orders);
        }

        /// <summary>
        /// Añade los WHERE necesarios para cargar un registro de una tabla por su clave primaria
        /// </summary>
        /// <param name="tabla"></param>
        public void AgregarWheresRegistroTabla(string nombreVariableSdt, Table tabla)
        {
            foreach (TableAttribute atributoPK in tabla.TableStructure.PrimaryKey)
                AgregarWhereAsignacion(nombreVariableSdt, atributoPK.Attribute.Name);
        }

        /// <summary>
        /// Agrega una linea al codigo interno del for each
        /// </summary>
        /// <param name="lineaCodigo">La linea de codigo, sin tabular ni salto de linea final</param>
        public void AgregarLinea(string lineaCodigo)
        {
            CodigoInterno += lineaCodigo + "\n";
        }

    }
}
