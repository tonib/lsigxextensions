using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Sdts;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.UI;

namespace LSI.Packages.Extensiones.Comandos.Sdts
{
    /// <summary>
    /// Crea un SDT a partir de la estructura de una tabla
    /// </summary>
    public class CrearSdtDeTabla : IExecutable
    {

        /// <summary>
        /// Ejecuta el comando
        /// </summary>
        public void Execute()
        {

            // Seleccionar la tabla de la que crear el SDT
            Table tabla = SelectKbObject.SeleccionarTabla();
            if (tabla == null)
                return;

            // Crear el sdt
            StdGX nuevoSdt = new StdGX(tabla);

            // Guardar y abrirlo
            nuevoSdt.GuardarYAbrir();
        }
    }
}
