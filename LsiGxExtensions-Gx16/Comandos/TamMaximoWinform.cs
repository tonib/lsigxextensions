using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.WinForms;
using LSI.Packages.Extensiones.Utilidades;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades.Threading;
using Artech.Genexus.Common;
using Artech.Architecture.UI.Framework.Objects;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos
{

    /// <summary>
    /// Establece el tamaño del winform actual en el tamaño maximo
    /// </summary>
    public class TamMaximoWinform : IExecutable
    {

        public void Execute()
        {
            try
            {
                LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.Load();
                KBObject o = UIServices.Environment.ActiveDocument.Object;

                IDocumentView vistaDocumento = UIServices.Environment.ActiveView;
                IGxView vistaActiva = vistaDocumento.ActiveView;
                IGxDocument documentoActual = vistaDocumento.Document;
                IGxDocumentPart parteWinform = documentoActual.GetDocumentPart(PartType.WinForm);

                // Guardar los cambios de los editores en el objeto
                vistaActiva.UpdateData();

                // Poner el tamaño maximo
                WinFormGx wf = new WinFormGx(o);
                wf.TamanyoForm = cfg.TamMaximoWinforms;

                // Actualizar el editor
                vistaActiva.UpdateView();

                // Marcar en la UI la parte de winform como modificada
                documentoActual.Dirty = true;
                parteWinform.Dirty = true;

            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

    }
}
