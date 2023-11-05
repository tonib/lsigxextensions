using System;
using System.Collections.Generic;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.Threading;

namespace LSI.Packages.Extensiones.Comandos
{
    /// <summary>
    /// Abre la ventana de la configuracion de la extension
    /// </summary>
    public class EditarConfiguracion : IExecutable
    {
        /// <summary>
        /// Ejecuta el comando
        /// </summary>
        public void Execute()
        {
            new ConfigurationWindow().ShowDialog();
        }

    }
}
