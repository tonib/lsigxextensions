using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LSI.Packages.Extensiones.Utilidades;
using System.Diagnostics;
using Microsoft.Win32;

namespace LSI.Packages.Extensiones.Utilidades.Compression
{
    /// <summary>
    /// External compresion application definition
    /// </summary>
    public class Compressor
    {

        // Winrar options

        public const string WINRARPATH = @"C:\Program Files (x86)\WinRAR\rar.exe";

        public const string WINRARCMDLINE = @"a {1} {0} -idq";

        public const string WINRAREXTENSION = @"rar";

        public const string WINRAREXCLUDEOPT = @"-x{0}";

        // 7-zip options

        public const string ZIP7PATH = @"C:\Program Files\7-Zip\7z.exe";

        public const string ZIP7CMDLINE = @"a -t7z {1} {0} ";

        public const string ZIP7EXTENSION = @"zip";

        public const string ZIP7EXCLUDEOPT = @"-xr!{0}";

        /// <summary>
        /// Compressor executable file path
        /// </summary>
        public string CompressorPath = ZIP7PATH;

        /// <summary>
        /// Compressor command line pattern. {0} are the files to compress, and {1}
        /// the zip path
        /// </summary>
        public string CompressorCommandLine = ZIP7CMDLINE;

        /// <summary>
        /// Files extension of compressor
        /// </summary>
        public string CompressorFilesExtension = ZIP7EXTENSION;

        /// <summary>
        /// Compressor exclude command line option pattern.
        /// </summary>
        public string CompressorExcludeOption = ZIP7EXCLUDEOPT;

        /// <summary>
        /// Load the compressor definition from a Windows registry entry
        /// </summary>
        /// <param name="registry">Registry entry to read</param>
        /// <returns>The compressor definition</returns>
        static public Compressor LoadFromRegistry(RegistryKey registry)
        {
            Compressor compressor = new Compressor();
            compressor.CompressorPath = (string)registry.GetValue("CompressorPath", ZIP7PATH);
            compressor.CompressorCommandLine = (string)registry.GetValue("CompressorCommandLine", ZIP7CMDLINE);
            compressor.CompressorFilesExtension = (string)registry.GetValue("CompressorFilesExtension", ZIP7EXTENSION);
            compressor.CompressorExcludeOption = (string)registry.GetValue("CompressorExcludeOption", ZIP7EXCLUDEOPT);
            return compressor;
        }

        /// <summary>
        /// Save the configuration at the registry
        /// </summary>
        /// <param name="registry">Registry entry to write</param>
        public void StoreAtRegistry(RegistryKey registry)
        {
            registry.SetValue("CompressorPath", CompressorPath);
            registry.SetValue("CompressorCommandLine", CompressorCommandLine);
            registry.SetValue("CompressorFilesExtension", CompressorFilesExtension);
            registry.SetValue("CompressorExcludeOption", CompressorExcludeOption);
        }

        public string GetZipFileNameWithExtension(string zipPath)
        {
            return zipPath + "." + CompressorFilesExtension;
        }

        /// <summary>
        /// Get a process to run the zip operation. The process will not be started
        /// </summary>
        /// <param name="zipInfo">Zip task to run</param>
        /// <returns>The process to run the external compressor</returns>
        public Process GetZipProcess(ZipOperation zipInfo)
        {
            if (!File.Exists(CompressorPath))
                throw new Exception("Compressor exe file (" + CompressorPath + ") does not exists");

            string filesToCompress = string.Join(" ",
                zipInfo.FilesToCompress.Select(x => "\"" + x.Trim() + "\"").ToArray()
                );

            string zipPath;
            if (!Path.HasExtension(zipInfo.DestinationPath))
            {
                // Add configured extension
                zipPath = zipInfo.DestinationPathWithoutExtension;
                zipPath += "." + CompressorFilesExtension;
            }
            else
                zipPath = zipInfo.DestinationPath;
            zipPath = "\"" + zipPath + "\"";

            string cmdLine;
            try
            {
                cmdLine = string.Format(CompressorCommandLine, filesToCompress, zipPath);
            }
            catch (Exception ex)
            {
                throw new Exception("Wrong compressor command line (" + CompressorCommandLine + ")", ex);
            }

            // File patterns to ignore:
            string excludeOption = this.CompressorExcludeOption.Trim();
            if (!string.IsNullOrEmpty(excludeOption))
            {
                foreach (string ignorePattern in zipInfo.FilePatternsIgnore)
                {
                    try
                    {
                        cmdLine += " " + string.Format(excludeOption, ignorePattern.Trim() );
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Wrong ignore option format (" + excludeOption + ")", ex);
                    }
                }
            }

            // Create the process to run
            Process process = new Process();
            process.StartInfo.WorkingDirectory = zipInfo.BaseSourcePath;
            process.StartInfo.FileName = CompressorPath;
            process.StartInfo.Arguments = cmdLine;
            return process;
        }

        public Process DoZip(ZipOperation zipInfo)
        {
            Process process = GetZipProcess(zipInfo);
            process.Start();
            return process;
        }

    }
}
