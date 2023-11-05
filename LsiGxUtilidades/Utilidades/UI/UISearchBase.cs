using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using LSI.Packages.Extensiones.Utilidades.UI;

namespace LSI.Packages.Extensiones.Utilidades.UI
{
    /// <summary>
    /// Base class for search processes with user interface
    /// </summary>
    public abstract class UISearchBase
    {

        /// <summary>
        /// Nº objetos revisados tras los cuales hay que actualizar el estado en la interface
        /// de usuario
        /// </summary>
        protected int MessagesMultiple = 1;

        /// <summary>
        /// Nº de objetos revisados hasta ahora
        /// </summary>
        private int NSearched;

        /// <summary>
        /// Nº de objetos a revisar. 
        /// Se usa para dar mensajes en la interface de usuario. Si es cero, no se aplica.
        /// </summary>
        protected int NToSearch = 0;

        /// <summary>
        /// Nº de objetos encontrados hasta ahora
        /// </summary>
        private int NFound;

        /// <summary>
        /// Si es cierto, la busqueda ha sido cancelada desde la interface de usuario
        /// </summary>
        public bool SearchCanceled
        {
            get { return BackgroundSearch != null && BackgroundSearch.CancellationPending; }
        }

        /// <summary>
        /// Proceso de busqueda en segundo plano. Puede ser nulo.
        /// </summary>
        public BackgroundWorker BackgroundSearch { get; set; }

        /// <summary>
        /// Publica un resultado en la interface de usuario
        /// </summary>
        /// <param name="result"></param>
        virtual protected void PublishUIResult(object result, bool forceFound = false)
        {
            if (BackgroundSearch == null)
                return;

            if( result is RefObjetoGX || forceFound)
                NFound++;
            BackgroundSearch.ReportProgress(0, result);
        }

        /// <summary>
        /// Incrementa el nº de objetos revisados en uno, y da un mensaje si
        /// este numero es multiplo de MultiploMensajes
        /// </summary>
        /// <remarks>
        /// TODO: Dar la opcion pasar como parametro el nombre del objeto que se esta revisando.
        /// TODO: Esto es util para detectar en que objetos van lentas las extensiones
        /// </remarks>
        public void IncreaseSearchedObjects()
        {
            NSearched++;

            if (BackgroundSearch == null)
                return;

            if ((NSearched % MessagesMultiple) == 0)
            {
                string msg = "Searched " + NSearched;
                if( NToSearch != 0 )
                    msg += " of " + NToSearch;
                msg += ". Found: " + NFound + "...";
                PublishUIResult(msg);
            }
        }

        /// <summary>
        /// Execute the search
        /// </summary>
        public abstract void ExecuteUISearch();

    }
}
