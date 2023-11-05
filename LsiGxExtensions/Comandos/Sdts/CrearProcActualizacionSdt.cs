using System.Collections.Generic;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.CodeGeneration;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.UI;

namespace LSI.Packages.Extensiones.Comandos.Sdts
{
    /// <summary>
    /// Crea un procedimiento que actualiza un registro de la base de datos a partir de un sdt
    /// </summary>
    public class CrearProcActualizacionSdt : IExecutable
    {

        /// <summary>
        /// Nombre con que se creara la variable sdt
        /// </summary>
        public const string NOMBREVARIABLESDT = "sRegistro";

        /// <summary>
        /// Ejecuta el comando
        /// </summary>
        public void Execute()
        {

            using (Log log = new Log())
            {
                // Seleccionar la tabla de la que crear el SDT
                Table tabla = SelectKbObject.SeleccionarTabla();
                if (tabla == null)
                    return;

                // Seleccionar el sdt:
                SDT sdtGx = SelectKbObject.SeleccionarSdt();
                if (sdtGx == null)
                    return;

                // Crear el procedimiento:
                ProcedureForEachGenerator p = new ProcedureForEachGenerator(tabla, sdtGx, CrearProcCargaSdt.NOMBREVARIABLESDT);
                p.Procedure.Name = KBaseGX.GetUnusedName(KBaseGX.NAMESPACE_OBJECTS, "P" + tabla.Name + "Act");
                p.Procedure.Description = "Actualizar " + tabla.Description;
                
                // Agregar errores de error
                p.CrearVariablesGestionErrores();

                // Obtener la variable sdt
                Variable v = p.Procedure.Variables.GetVariable(CrearProcCargaSdt.NOMBREVARIABLESDT);

                // Generar las asignaciones del sdt a la tabla
                List<string> avisos = new List<string>();
                p.ForEach.CodigoInterno = AssignmentsGenerator.AsignacionesDeSdtATabla(v, sdtGx, tabla, false, avisos);

                // Verificacion de errores
                p.ForEach.CodigoWhenNone =
                    "&FlgErr = true\n" +
                    "&MsgErr = 'No se ha encontrado el registro a actualizar'";

                // Ver si hay avisos:
                if (avisos.Count > 0)
                {
                    foreach (string aviso in avisos)
                    {
                        p.ForEach.CodigoInterno += "// " + aviso + "\n";
                        log.Output.AddWarningLine(aviso);
                    }
                    Log.MostrarVentana();
                }

                // Guardar y abrir
                p.GuardarYAbrirProcedimiento();
            }

        }

    }
}
