using System.Collections.Generic;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.CodeGeneration;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.UI;

namespace LSI.Packages.Extensiones.Comandos.Sdts
{
    /// <summary>
    /// Crea un procedimiento que carga un registro de la base de datos en un sdt
    /// </summary>
    public class CrearProcCargaSdt : IExecutable
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
                ProcedureForEachGenerator p = new ProcedureForEachGenerator(tabla);
                p.Procedure.Name = KBaseGX.GetUnusedName(KBaseGX.NAMESPACE_OBJECTS, "P" + tabla.Name + "Car");
                p.Procedure.Description = "Cargar " + tabla.Description;

                // Crear la variable sdt en el procedimiento.
                Variable v = p.CrearVariableSdt(NOMBREVARIABLESDT, sdtGx);

                // Generar las asignaciones de la tabla al sdt
                List<string> avisos = new List<string>();
                p.ForEach.CodigoInterno = AssignmentsGenerator.AsignacionesDeTablaASdt(tabla, v, sdtGx, avisos);

                // Añadir la variable del sdt para su retorno:
                p.Parm.AgregarParametro(v, RuleDefinition.ParameterAccess.PARM_OUT);
                p.Parm.AddDocumentation("Registro cargado");

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
