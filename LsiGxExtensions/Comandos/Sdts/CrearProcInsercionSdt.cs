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
    /// Comando para crear un procedimiento que inserta un registro con los valores de un sdt
    /// </summary>
    public class CrearProcInsercionSdt : IExecutable
    {

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
                ProcedureGenerator p = new ProcedureGenerator();
                p.Procedure.Name = KBaseGX.GetUnusedName(KBaseGX.NAMESPACE_OBJECTS, "P" + tabla.Name + "Gen");
                p.Procedure.Description = "Insertar " + tabla.Description;

                // Crear la variable sdt y añadirla a los parametros
                Variable v = p.CrearVariableSdt(CrearProcCargaSdt.NOMBREVARIABLESDT, sdtGx);
                p.Parm.AgregarParametro(v);
                p.Parm.AddDocumentation("Registro a guardar");

                // Agregar errores de error
                p.CrearVariablesGestionErrores();

                // Crear el new, con las asignaciones del sdt a la tabla
                NewGenerator n = new NewGenerator();
                List<string> avisos = new List<string>();
                n.CodigoInterno = AssignmentsGenerator.AsignacionesDeSdtATabla(v, sdtGx, tabla, true, avisos);

                // Verificacion de errores
                n.CodigoWhenDuplicate =
                    "&FlgErr = true\n" +
                    "&MsgErr = 'Ya existe un registro con el codigo indicado'";

                // Ver si hay avisos:
                if (avisos.Count > 0)
                {
                    foreach (string aviso in avisos)
                    {
                        n.CodigoInterno += "// " + aviso + "\n";
                        log.Output.AddWarningLine(aviso);
                    }
                    Log.MostrarVentana();
                }

                // Poner el codigo del procedimiento
                p.Procedure.ProcedurePart.Source = n.ToString();

                // Guardar y abrir
                p.GuardarYAbrirProcedimiento();
            }
        }

    }
}
