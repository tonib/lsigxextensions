using Artech.Architecture.UI.Framework.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.CSharpWin.UnusedFiles
{
    /// <summary>
    /// Info about an unused file
    /// </summary>
    public class UnusedFile
    {
        /// <summary>
        /// The relative file path 
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// The file full path
        /// </summary>
        public string FullPath;

        /// <summary>
        /// The size in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Last modification date
        /// </summary>
        public DateTime LastModification { get; set; }

        /// <summary>
        /// The size in KB
        /// </summary>
        public double SizeKb
        {
            get { return ToKBytes(Size); }
        }

        static public double ToKBytes(long bytes)
        {
            return Math.Round(bytes / 1024.0, 2);
        }

        public UnusedFile(string basePath, string relativePath)
        {
            RelativePath = relativePath;
            FullPath = Path.Combine(basePath, relativePath);
            FileInfo info = new FileInfo(FullPath);
            Size = info.Length;
            LastModification = info.LastWriteTime;
        }

    }
}
