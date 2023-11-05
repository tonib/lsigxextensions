using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Threading;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common;
using Artech.Udm.Framework;
using Artech.Architecture.Common.Objects;
using LSI.Packages.Extensiones.Utilidades.CallsAnalisys;
using LSI.Packages.Extensiones.Utilidades.CSharpWin;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos.Build
{
    public class CleanObjectInfoCache : IExecutable
    {

        public void Execute()
        {
            if (MessageBox.Show("Are you sure you want to clean kbase objects cached information?",
                "Confirm", MessageBoxButtons.OKCancel) != DialogResult.OK)
                return;

            using (Log log = new Log())
            {
                try
                {
                    GeneratedSourceFilesCache.Cache(UIServices.KB.CurrentKB).Clean(UIServices.KB.CurrentKB);
                    log.Output.AddLine("Cache cleaned");
                }
                catch (Exception ex)
                {
                    Log.ShowException(ex);
                }
            }
        }
    }
}
