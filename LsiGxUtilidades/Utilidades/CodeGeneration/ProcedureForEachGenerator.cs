using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;

namespace LSI.Packages.Extensiones.Utilidades.CodeGeneration
{
    /// <summary>
    /// Tool to generate a procedure with an unique FOR EACH
    /// </summary>
    public class ProcedureForEachGenerator : ProcedureGenerator
    {
        /// <summary>
        /// La sentencia for each del procedimiento
        /// </summary>
        public ForEachGenerator ForEach;

        /// <summary>
        /// Crea el procedimiento con un foreach para acceder un registro de la tabla.
        /// La clave para acceder a la tabla se hace a traves de variables que se crean en
        /// este constructor.
        /// </summary>
        /// <param name="tabla">Tabla a acceder</param>
        public ProcedureForEachGenerator(Table tabla)
        {
            // Generar el codigo del procedimiento
            ForEach = new ForEachGenerator(tabla);

            // Crear las variables de los parametros para la clave de la tabla
            foreach (TableAttribute atributoPK in tabla.TableStructure.PrimaryKey)
            {
                Variable v = new Variable(atributoPK.Name, Procedure.Variables);
                v.AttributeBasedOn = atributoPK;
                Procedure.Variables.Add(v);
                Parm.AgregarParametro(v);
            }
            Parm.AddDocumentation("Codigo de " + tabla.Description);
        }

        /// <summary>
        /// Crea el procedimiento con un foreach para acceder un registro de la tabla.
        /// La clave para acceder a la tabla se carga desde una variable sdt que se declara aqui.
        /// </summary>
        /// <param name="tabla">Tabla a acceder</param>
        /// <param name="sdt">Sdt desde el que cargar los parametros para acceder a la tabla</param>
        /// <param name="nombreVariableSdt">Nombre con que declarar la variable sdt</param>
        public ProcedureForEachGenerator(Table tabla, SDT sdt, string nombreVariableSdt)
        {
            // Crear la variable sdt de entrada y declararla
            Variable v = CrearVariableSdt(nombreVariableSdt, sdt);
            Parm.AgregarParametro(v);
            Parm.AddDocumentation(tabla.Description);

            // Crear el for each
            ForEach = new ForEachGenerator(nombreVariableSdt, tabla);
            
        }

        public override void GuardarYAbrirProcedimiento()
        {
            Procedure.ProcedurePart.Source = ForEach.ToString();
            base.GuardarYAbrirProcedimiento();
        }

    }
}
