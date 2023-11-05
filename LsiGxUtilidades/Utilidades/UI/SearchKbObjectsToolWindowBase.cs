using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Utilidades.UI
{
    /// <summary>
    /// Base for tool windows to search kbase objects
    /// </summary>
    public class SearchKbObjectsToolWindowBase : SearchToolWindowBase
    {

        protected void InitializeSearchTWCustomType<T>(Button twSearchButton, PictureBox twBusyPicture, Label twStatusLabel,
            GridObjetos twGrid) where T : RefObjetoGX
        {
            base.InitializeSearchToolWindow(twSearchButton, twBusyPicture, twStatusLabel, twGrid);

            twGrid.CrearColumnasEstandar();
            twGrid.SetObjetos<T>(new SortableList<T>());
        }

        protected void InitializeSearchTW(Button twSearchButton, PictureBox twBusyPicture, Label twStatusLabel,
            GridObjetos twGrid)
        {
            InitializeSearchTWCustomType<RefObjetoGX>(twSearchButton, twBusyPicture, twStatusLabel, twGrid);
        }
    }
}
