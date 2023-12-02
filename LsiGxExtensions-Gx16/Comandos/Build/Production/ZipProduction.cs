using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades;
using System.IO;
using System.Diagnostics;
using LSI.Packages.Extensiones.Utilidades.Compression;
using LSI.Packages.Extensiones.Utilidades.Threading;

namespace LSI.Packages.Extensiones.Comandos.Build.Production
{
    /// <summary>
    /// Create a zip of a GX environment
    /// </summary>
    public class ZipProduction : EnvironmentTask
    {

        /// <summary>
        /// Path of the zip to generate
        /// </summary>
        public string ZipPath;

        override public void Execute(PrepareProduction productionProcess)
        {
            base.Execute(productionProcess);

            ZipOperation zipInfo = new ZipOperation();
            zipInfo.BaseSourcePath = productionProcess.TargetDirectory;
            zipInfo.DestinationPath = ZipPath;

            // Directory to compress:
            List<string> pathsToZip = new List<string>();
            zipInfo.FilesToCompress.Add(RelativeEnvironmentPath);

            // File patterns to ignore:
            zipInfo.FilePatternsIgnore = productionProcess.FilePattensToIgnore;

            // Remove previous zip. Otherwise, it will be updated by the compressor
            if (File.Exists(ZipPath))
                File.Delete(ZipPath);

            // Run the compressor
            Process zipProcess = LsiExtensionsConfiguration.Load().Compressor.GetZipProcess(zipInfo);
            int exitCode = productionProcess.ExecuteProcess(zipProcess, false);
            if (exitCode != 0)
                // Compression failed (see http://acritum.com/winrar/console-rar-manual "Exit values")
                productionProcess.LogErrorLine("ERROR: Compressor exited with code " + exitCode);
        }

        /// <summary>
        /// List of files/directories that will be replaced by this task.
        /// </summary>
        override public List<string> ReplacedFiles
        {
            get
            {
                List<string> result = new List<string>();
                result.Add(ZipPath);
                return result;
            }
        }

        public override string ToString()
        {
            return base.ToString() + " Compress to " + ZipPath;
        }

    }
}
