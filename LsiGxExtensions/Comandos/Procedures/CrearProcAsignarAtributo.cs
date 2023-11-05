using System.Collections.Generic;
using System.Linq;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.CodeGeneration;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.UI;

namespace LSI.Packages.Extensiones.Comandos.Procedures
{
    /// <summary>
    /// Crea un procedimiento del tipo P[Atributo] para asignar el valor de un atributo de un registro.
    /// </summary>
    public class CrearProcAsignarAtributo : IExecutable
    {

        /// <summary>
        /// Ejecuta el proceso de creacion de un proceso para asignar el valor de un atributo
        /// </summary>
        public void Execute() {

            using( Log log = new Log() ) {

                // Seleccionar el atributo del que obtener la tabla:
                Attribute atributo = SelectKbObject.SelectAttribute();
                if (atributo == null)
                    return;

                if( !atributo.IsReferencedByTable() ) {
                    //log.Output.AddErrorLine("El atributo " + atributo.Name + " no es referenciado por ninguna tabla");
                    log.Output.AddErrorLine("Attribute " + atributo.Name + " is not referenced by any table");
                    log.ProcesoOk = false;
                    Log.MostrarVentana();
                    return;
                }

                // Buscar las tablas en las que esta el atrituto:
                List<Table> tablas = AtributoGx.TablasQueReferencianAtributo(atributo);
                if (tablas.Count > 1)
                {
                    log.Output.AddErrorLine("Attribute " + atributo.Name + " is referenced by more than one table");
                    log.ProcesoOk = false;
                    Log.MostrarVentana();
                    return;
                }

                Table tabla = tablas[0];
                bool esClave = tabla.TableStructure.PrimaryKey.Any(atr => atr.Attribute.Name.ToLower() == atributo.Name.ToLower());
                if (esClave)
                {
                    log.Output.AddErrorLine("Attribute " + atributo.Name + " belongs to " + tabla.Name + " primary key");
                    log.ProcesoOk = false;
                    Log.MostrarVentana();
                    return;
                }

                // Crear el procedimiento
                ProcedureForEachGenerator p = new ProcedureForEachGenerator(tabla);
                p.Procedure.Name = KBaseGX.GetUnusedName( KBaseGX.NAMESPACE_OBJECTS , "P" + atributo.Name );
                p.Procedure.Description = "Asignar " + atributo.Description;

                // Añadir el codigo al for each:
                p.ForEach.CodigoInterno = AssignmentsGenerator.AsignacionAtributo(atributo.Name);

                // Crear la variable de retorno:
                Variable vRetorno = p.CrearVariableBasadaEnAtributo(atributo);

                // Añadir el parametro de salida:
                p.Parm.AgregarParametro(vRetorno, RuleDefinition.ParameterAccess.PARM_IN);
                p.Parm.AddDocumentation("Valor de " + atributo.Description);

                // Guadar y abrir
                p.GuardarYAbrirProcedimiento();
                log.Output.AddLine("Procedure " + p.Procedure.Name + " created");

            }
        }

    }
}
