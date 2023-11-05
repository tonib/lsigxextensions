using LSI.Packages.Extensiones.Utilidades.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.CSharpWin.UnusedFiles
{
    /// <summary>
    /// Searches unused files
    /// </summary>
    public abstract class UnusedFilesFinderBase : UISearchBase
    {
        /// <summary>
        /// The base directory where the searches are done
        /// </summary>
        abstract public DirectoryInfo BaseDirectory { get; }

        /// <summary>
        /// Base path, relative to Kb path
        /// </summary>
        abstract public string BaseDirectoryRelativeToKb { get; }

        protected void PublishFile(string baseDirectory, string relativePath)
        {
            PublishUIResult(new UnusedFile(baseDirectory, relativePath), true);
        }
    }

}
