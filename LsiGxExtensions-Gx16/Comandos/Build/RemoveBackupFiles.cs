using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Artech.Architecture.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;

namespace LSI.Packages.Extensiones.Comandos.Build
{
    /// <summary>
    /// Tool to remove win / c# backup files
    /// </summary>
    public class RemoveBackupFiles : BuildProcess
    {
        /// <summary>
        /// Rsp file patterns to remove
        /// </summary>
        private const string RSPBACKUPSPATTERN = "*" + Entorno.BACKUPPOSTFIX + "*.rsp";

        /// <summary>
        /// Dll file patterns to remove
        /// </summary>
        private const string DLLBACKUPSPATTERN = "*" + Entorno.BACKUPPOSTFIX + "*.dll";

        /// <summary>
        /// Pdb file patterns to remove
        /// </summary>
        private const string PDBBACKUPSPATTERN = "*" + Entorno.BACKUPPOSTFIX + "*.pdb";

        /// <summary>
        /// Files to delete
        /// </summary>
        private IList<string> FilesToDelete;

        override public bool IsInternalGxBuild { get { return false; } }

        /// <summary>
        /// Backup file patterns description
        /// </summary>
        static public string BackupPatterns
        {
            get
            {
                return RSPBACKUPSPATTERN + "; " + DLLBACKUPSPATTERN + "; " +
                    PDBBACKUPSPATTERN;
            }
        }

        /// <summary>
        /// Get backup files at some directory
        /// </summary>
        /// <param name="directory">Directory where to search</param>
        /// <param name="pattern">Pattern of files to search</param>
        /// <returns>The full path list of backups files at that directory</returns>
        static public List<string> GetBackupFiles(string directory, string pattern)
        {
            if (!Directory.Exists(directory))
                return new List<string>();

            return Directory.GetFiles(directory, pattern).ToList();
        }

        /// <summary>
        /// Get backup files at some environment directory
        /// </summary>
        /// <param name="targetDirectory">The target directory of the environment</param>
        /// <returns>The full path list of backups files at the environment</returns>
        static public List<string> GetBackupFiles(string targetDirectory)
        {
            List<string> files = new List<string>();

            string binDir = Path.Combine(targetDirectory, "bin");
            files.AddRange(GetBackupFiles(targetDirectory, RSPBACKUPSPATTERN));
            files.AddRange(GetBackupFiles(binDir, DLLBACKUPSPATTERN));
            files.AddRange(GetBackupFiles(binDir, PDBBACKUPSPATTERN));

            return files;
        }

        /// <summary>
        /// Constructor to delete a set of files
        /// </summary>
        /// <param name="filesToDelete">Files to delete</param>
        public RemoveBackupFiles(IList<string> filesToDelete)
        {
            this.FilesToDelete = filesToDelete;
        }

        /// <summary>
        /// Constructor to delete the current set of backup files on a target model
        /// </summary>
        /// <param name="targetModel">The target model where to delete backup files</param>
        public RemoveBackupFiles(KBModel targetModel)
        {
            this.FilesToDelete = GetBackupFiles(Entorno.GetTargetDirectory(targetModel));
        }

        /// <summary>
        /// Remove backup files
        /// </summary>
        override public void Execute()
        {
            int nDeleted = 0;
            foreach (string file in FilesToDelete)
            {
                try
                {
                    File.Delete(file);
                    LogLine(file + " deleted");
                    nDeleted++;
                }
                catch (Exception ex)
                {
                    LogErrorLine("File " + file + " cannot be deleted: " + ex.Message);
                }
            }
            this.LogLine(nDeleted + " files deleted");
        }

        public override string ToString()
        {
            return "Remove backup files";
        }
    }
}
