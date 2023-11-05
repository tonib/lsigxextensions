using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.Layout;
using Artech.Genexus.Common;
using Artech.Architecture.UI.Framework.Services;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using Artech.Architecture.Common.Objects;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Comandos
{
    /// <summary>
    /// Clase para buscar fuentes no instaladas en los reports
    /// </summary>
    public class RevisarFuentesReports : IExecutable
    {

        private Log Log;

        private void RevisarObjeto(Procedure procedimiento)
        {
            LayoutPart layout = procedimiento.Parts.LsiGet<LayoutPart>();
            if (layout == null)
                return;

            // Buscar en cada uno de los printblocks
            foreach (ReportComponent control in LayoutGx.EnumerarComponentes<ReportComponent>(layout))
            {
                ReportComponent componente = (ReportComponent)control;
                Font fuente = componente.GetPropertyValue(Properties.RPT_GENERIC.Font) as Font;
                if (fuente == null)
                    continue;

                if (fuente.Name != fuente.OriginalFontName)
                {
                    // La fuente no esta instalada:
                    string msg = "Object " + procedimiento.QualifiedName;
                    if (control.Name != null)
                        msg += ", " + control.Name;
                    msg += ": Font " + fuente.OriginalFontName +
                        " is not installed. It has been replaced by " +
                        fuente.Name;
                    Log.Output.AddErrorLine(msg);
                    Log.ProcesoOk = false;
                }
            }
        }

        private void RevisarProcedimientosKbase()
        {
            using (Log = new Log())
            {
                int cnt = 0;
                foreach (Procedure p in Procedure.GetAll(UIServices.KB.CurrentModel))
                {
                    RevisarObjeto(p);
                    cnt++;
                    if (cnt % 100 == 0)
                        Log.Output.AddLine(cnt + " procedures checked...");
                }
            }
        }

        /// <summary>
        /// Ejecuta el comando
        /// </summary>
        public void Execute()
        {
            if (MessageBox.Show("This process cannot be cancelled and it can take some time. Are you sure you want to check report fonts?", "Confirm", MessageBoxButtons.OKCancel)
                == DialogResult.Cancel)
                return;

            Thread t = new Thread(new ThreadStart(this.RevisarProcedimientosKbase));
            t.Start();
        }

    }
}
