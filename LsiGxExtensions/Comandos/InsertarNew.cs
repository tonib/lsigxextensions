using System.Windows.Forms;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.CodeGeneration;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.UI;

namespace LSI.Packages.Extensiones.Comandos
{
    /// <summary>
    /// Comando para insertar en el codigo una sentencia NEW de una tabla
    /// </summary>
    public class InsertarNew : IExecutable
    {

        public void Execute()
        {
            // Seleccionar la tabla de la que insertar el for each
            Table tabla = SelectKbObject.SeleccionarTabla();
            if (tabla == null)
                return;

            // Obtener el texto del new:
            string textoNew = new NewGenerator(tabla).ToString();

            // Copiar al portapapeles el for each:
            Clipboard.SetText(textoNew);
        }

    }
}
