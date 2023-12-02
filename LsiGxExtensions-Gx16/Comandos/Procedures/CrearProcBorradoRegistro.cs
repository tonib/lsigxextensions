using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.CodeGeneration;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.UI;

namespace LSI.Packages.Extensiones.Comandos.Procedures
{
    /// <summary>
    /// Crea un proceso de borrardo de una tabla
    /// </summary>
    public class CrearProcBorradoRegistro : IExecutable
    {

        /// <summary>
        /// Ejecuta el comando
        /// </summary>
        public void Execute()
        {

            using (Log log = new Log())
            {
                // Seleccionar la tabla de la que crear el proceso
                Table tabla = SelectKbObject.SeleccionarTabla();
                if (tabla == null)
                    return;

                // Crear el procedimiento:
                ProcedureForEachGenerator p = new ProcedureForEachGenerator(tabla);

                // Crear variables de error:
                p.CrearVariablesGestionErrores();

                p.Procedure.Name = KBaseGX.GetUnusedName(KBaseGX.NAMESPACE_OBJECTS, "P" + tabla.Name + "Bor");
                p.Procedure.Description = "Borrar " + tabla.Description;
                p.ForEach.CodigoInterno = "DELETE";

                p.ForEach.CodigoWhenNone =
                    "&FlgErr = true\n" +
                    "&MsgErr = 'Registro a borrar no encontrado'";

                p.GuardarYAbrirProcedimiento();

                log.Output.AddLine("Procedure " + p.Procedure.Name + " created");
            }

        }
    }
}
