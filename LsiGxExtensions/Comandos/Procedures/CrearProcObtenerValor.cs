using System.Collections.Generic;
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
    /// Crea un procedimiento del tipo PU[Atributo] para devolver el valor de un campo de un registro.
    /// </summary>
    public class CrearProcObtenerValor : IExecutable
    {

        /// <summary>
        /// Ejecuta el proceso de creacion de un proceso para devolver el valor de un atributo
        /// </summary>
        public void Execute() {

            using( Log log = new Log() ) {

                // Seleccionar el atributo del que obtener la tabla:
                Attribute atributo = SelectKbObject.SelectAttribute();
                if (atributo == null)
                    return;

                if( !atributo.IsReferencedByTable() ) {
                    log.Output.AddErrorLine("Attribute " + atributo.Name + " is not referenced by any table");
                    Log.MostrarVentana();
                    return;
                }

                // Buscar las tablas en las que esta el atrituto:
                List<Table> tablas = AtributoGx.TablasQueReferencianAtributo(atributo);
                if (tablas.Count > 1)
                {
                    log.Output.AddErrorLine("Attribute " + atributo.Name + " is referenced by more than one table");
                    Log.MostrarVentana();
                    return;
                }

                Table tabla = tablas[0];

                // Crear el procedimiento
                ProcedureForEachGenerator p = new ProcedureForEachGenerator(tabla);
                p.Procedure.Name = KBaseGX.GetUnusedName(KBaseGX.NAMESPACE_OBJECTS, "PU" + atributo.Name);
                p.Procedure.Description = "Obtener " + atributo.Description;

                // Añadir el codigo al for each:
                p.ForEach.CodigoInterno = "&" + atributo.Name + " = " + atributo.Name;

                // Asignar nulo si no se ha encontrado
                p.ForEach.CodigoWhenNone = "&" + atributo.Name + " = NullValue( &" + atributo.Name + " )";

                // Crear la variable de retorno:
                if( p.Procedure.Variables.GetVariable(atributo.Name) != null ) 
                {
                    // Si la variable ya existe, es que el atributo a devolver formaba parte de la clave
                    // No tiene sentido:
                    log.Output.AddErrorLine("Attribute " + atributo.Name + " is contained in " + tabla.Name + " primary key");
                    Log.MostrarVentana();
                    return;
                }
                Variable vRetorno = p.CrearVariableBasadaEnAtributo(atributo);

                // Añadir el parametro de salida:
                p.Parm.AgregarParametro(vRetorno, RuleDefinition.ParameterAccess.PARM_OUT);
                p.Parm.AddDocumentation("Obtener " + atributo.Description);

                // Guadar y abrir
                p.GuardarYAbrirProcedimiento();
                log.Output.AddLine("Procedure " + p.Procedure.QualifiedName + " created");

            }
        }

    }
}
