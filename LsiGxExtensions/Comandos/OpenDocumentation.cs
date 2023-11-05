using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Threading;
using System.Diagnostics;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos
{
    /// <summary>
    /// Abre de la documentacion de la extension en el navegador
    /// </summary>
    public class OpenDocumentation : IExecutable
    {
        /// <summary>
        /// Abre la pagina principal de la documentación
        /// </summary>
        public void Execute()
        {
            Open(null);
        }

        /// <summary>
        /// Abre una cierta pagina de la documentacion
        /// </summary>
        /// <param name="page">Path relativo de la pagina a abrir. null para abrir la 
        /// pagina principal</param>
        static public void Open(string page)
        {
            try
            {
                string url = "http://lsigxextensions.sf.net/";
                if (page != null)
                    url += page;
                Process.Start(url);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }
    }
}
