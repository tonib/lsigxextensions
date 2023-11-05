using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.CodeGeneration;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.UI;

namespace LSI.Packages.Extensiones.Comandos.Procedures
{
    /// <summary>
    /// Crea un procedimiento de insercion de un registro
    /// </summary>
    public class CrearProcInsercion : IExecutable
    {

        /// <summary>
        /// Ejecuta el comando
        /// </summary>
        public void Execute()
        {
            // Seleccionar la tabla de la que hacer la insercion
            Table tabla = SelectKbObject.SeleccionarTabla();
            if (tabla == null)
                return;

            // Crear el procedimiento:
            ProcedureGenerator p = new ProcedureGenerator();
            p.Procedure.Name = KBaseGX.GetUnusedName(KBaseGX.NAMESPACE_OBJECTS, "P" + tabla.Name + "Gen");
            p.Procedure.Description = "Crear registro de " + tabla.Description;

            // Crear variables para todos los campos de la tabla, y añadirlos a los parametros
            foreach (TableAttribute atr in tabla.TableStructure.Attributes)
            {
                if (!atr.IsFormula)
                {
                    Variable v = p.CrearVariableBasadaEnAtributo(atr.Attribute);
                    p.Parm.AgregarParametro(v);
                    p.Parm.AddDocumentation(atr.Attribute.Description);
                }
            }

            // Crear variables de error:
            p.CrearVariablesGestionErrores();

            // Crear el new
            NewGenerator n = new NewGenerator(tabla);

            // Verificacion de errores
            n.CodigoWhenDuplicate =
                "&FlgErr = true\n" +
                "&MsgErr = 'Ya existe un registro con el código indicado'";

            // Guardar el codigo
            p.Procedure.ProcedurePart.Source = n.ToString();

            // Guardar y abrir el procedimiento
            p.GuardarYAbrirProcedimiento();
        }

    }
}
