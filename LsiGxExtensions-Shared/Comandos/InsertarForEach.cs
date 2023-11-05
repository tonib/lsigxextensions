using System.Windows.Forms;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.CodeGeneration;
using LSI.Packages.Extensiones.Utilidades.UI;
using LSI.Packages.Extensiones.Utilidades.Threading;

namespace LSI.Packages.Extensiones.Comandos
{
    /// <summary>
    /// Inserta un for each para acceder a un registro de una tabla en el editor del objeto actual.
    /// </summary>
    public class InsertarForEach : IExecutable
    {

        public void Execute()
        {
            // Seleccionar la tabla de la que insertar el for each
            Table tabla = SelectKbObject.SeleccionarTabla();
            if (tabla == null)
                return;

            // Obtener el texto del foreach:
            string textoForEach = new ForEachGenerator(tabla).ToString();

            // No consigo que esto funcione bien. Funciona, pero el historial de ediciones (Ctrl+z) se pierde.
            // No me gusta como queda, copio el texto al portapapeles.
            // TODO: Ver si esto tiene solucion
            // Obtener el editor actual y volcar al objeto los cambios hechos
            /*EditorTextoGX editorTexto = new EditorTextoGX();
            editorTexto.InsertarCodigo(textoForEach);*/

            // Copiar al portapapeles el for each:
            Clipboard.SetText(textoForEach);
        }
    }
}
