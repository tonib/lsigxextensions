using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades;
using System.IO;

namespace LSI.Packages.Extensiones.Utilidades.Compression
{
    /// <summary>
    /// Information about a zip to create
    /// </summary>
    public class ZipOperation
    {

        /// <summary>
        /// Base directory path for files to compress
        /// </summary>
        public string BaseSourcePath;

        /// <summary>
        /// Set of files / directories paths to compress
        /// </summary>
        public List<string> FilesToCompress = new List<string>();

        /// <summary>
        /// Set of file patterns to ignore (ex. "*.ini")
        /// </summary>
        public List<string> FilePatternsIgnore = new List<string>();

        /// <summary>
        /// Path of the zip to create
        /// </summary>
        public string DestinationPath;

        /// <summary>
        /// Path of the zip to create, without extension
        /// </summary>
        public string DestinationPathWithoutExtension 
        {
            get {
                return Path.Combine(Path.GetDirectoryName(DestinationPath),
                    Path.GetFileNameWithoutExtension(DestinationPath));
            } 
        }

        /// <summary>
        /// Set a destination path. If this already exists, a postfix number will be added.
        /// </summary>
        /// <param name="destinationPath">Path of the zip to create, without extension</param>
        /// <param name="compressor">Compressor for the operacion</param>
        public void SetUnusedDestinationPath(string destinationDirectory, string fileName, 
            Compressor compressor)
        {
            fileName = compressor.GetZipFileNameWithExtension(fileName);

            string destinationPath = Path.Combine(destinationDirectory, fileName);
            destinationPath = Entorno.GetUnusedFileName(destinationPath);

            DestinationPath = Path.Combine(destinationDirectory , 
                Path.GetFileNameWithoutExtension(destinationPath) );
        }
    }
}
