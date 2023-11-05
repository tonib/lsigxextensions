using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.ServiceProcess;

namespace LSI.Packages.Extensiones.Comandos.Build.Production
{

    /// <summary>
    /// Do a copy of a kbase production
    /// </summary>
    public class CopyProduction : EnvironmentTask
    {

        /// <summary>
        /// Destination directory path of the copy
        /// </summary>
        public string CopyDestination;

        /// <summary>
        /// Should we keep the web/client.exe.config file if the destination directory already
        /// exists?
        /// </summary>
        public bool KeepConfigFile = true;

        /// <summary>
        /// Should we stop / start IIS to do the copy? Used only for web environment
        /// </summary>
        public bool StopIIS = true;

        /// <summary>
        /// Should we delete the existing directory backup after the copy?
        /// </summary>
        public bool DeleteBackup;

        /// <summary>
        /// List of regular expressions to ignore
        /// </summary>
        private List<Regex> IgnoreRegex = new List<Regex>();

        private string DestinationBackupPath
        {
            get { return CopyDestination + "-old"; }
        }

        /// <summary>
        /// List of files/directories that will be replaced by this task.
        /// </summary>
        override public List<string> ReplacedFiles
        {
            get
            {
                List<string> result = new List<string>();
                result.Add(CopyDestination);
                result.Add(DestinationBackupPath);
                return result;
            }
        }

        /// <summary>
        /// Rename the destination directory as backup
        /// </summary>
        /// <returns>True if a backup directory has been created</returns>
        private bool DoBackup(PrepareProduction productionProcess)
        {
            if (!Directory.Exists(CopyDestination))
                return false;

            if (Directory.Exists(DestinationBackupPath))
            {
                productionProcess.LogLine("Deleting " + DestinationBackupPath);
                Directory.Delete(DestinationBackupPath, true);
            }

            productionProcess.LogLine("Moving " + CopyDestination + " to " + DestinationBackupPath);
            Directory.Move(CopyDestination, DestinationBackupPath);
            return true;
        }

        private bool IgnoreName(string name)
        {
            foreach (Regex e in IgnoreRegex)
            {
                if (e.IsMatch(name))
                    return true;
            }
            return false;
        }

        private void CopyRecursivelly(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                if( !IgnoreName(dir.Name) )
                    CopyRecursivelly(dir, target.CreateSubdirectory(dir.Name));
            }

            foreach (FileInfo file in source.GetFiles())
            {
                if (!IgnoreName(file.Name))
                    file.CopyTo(Path.Combine(target.FullName, file.Name));
            }
        }

        /// <summary>
        /// Converts a wildcard to a regex.
        /// </summary>
        /// <remarks>Taken from http://www.codeproject.com/Articles/11556/Converting-Wildcards-to-Regexes</remarks>
        /// <param name="wildcard">The wildcard pattern to convert.</param>
        /// <returns>A regex equivalent of the given wildcard.</returns>
        private static Regex WildcardToRegex(string wildcard)
        {
            string pattern = "^" + Regex.Escape(wildcard).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return new Regex(pattern, RegexOptions.IgnoreCase);
        }

        private void RestoreConfigFile(PrepareProduction productionProcess)
        {
            string backupConfigPath = Path.Combine(DestinationBackupPath, ConfigFileName);
            if (!File.Exists(backupConfigPath))
                return;

            string configPath = Path.Combine(CopyDestination, ConfigFileName);
            productionProcess.LogLine("Copying from " + backupConfigPath + " to " + configPath);
            File.Copy(backupConfigPath, configPath, true);
        }

        private void StartStopIis(bool start, PrepareProduction productionProcess)
        {
            try
            {
                Uri uri = new Uri(CopyDestination);

                string msg;
                if (start)
                    msg = "Starting IIS";
                else
                    msg = "Stopping IIS";
                if (!string.IsNullOrEmpty(uri.Host))
                    msg += " at " + uri.Host;
                else
                    msg += " on local machine";

                productionProcess.LogLine(msg);

                ServiceController sc;
                if (!string.IsNullOrEmpty(uri.Host))
                    sc = new ServiceController("w3svc", uri.Host);
                else
                    sc = new ServiceController("w3svc");
                if (start)
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                }
                else
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                }
            }
            catch (Exception ex)
            {
                productionProcess.LogWarningLine("Error starting / stopping IIS: " + ex.Message);
            }            
        }

        override public void Execute(PrepareProduction productionProcess)
        {
            base.Execute(productionProcess);

            // Get source directory info
            DirectoryInfo sourceDir = new DirectoryInfo(GetAbsolutePath(productionProcess));
            if (!sourceDir.Exists)
            {
                productionProcess.LogErrorLine(sourceDir.ToString() + " does not exist");
                return;
            }

            // Create regular expressions to ignore
            foreach (string wildcard in productionProcess.FilePattensToIgnore)
                IgnoreRegex.Add(WildcardToRegex(wildcard));

            if (productionProcess.BuildCancelled)
                return;

            if (Environment == EnvironmentType.WEB && StopIIS)
                // Stop IIS
                StartStopIis(false, productionProcess);

            // Do a backup if the destination exists
            bool backupCreated = DoBackup(productionProcess);

            if (productionProcess.BuildCancelled)
                return;

            DirectoryInfo destinationDir = new DirectoryInfo(CopyDestination);
            productionProcess.LogLine("Copying from " + sourceDir.ToString() + " to " + destinationDir.ToString());

            // Create destination directory
            destinationDir.Create();

            if (productionProcess.BuildCancelled)
                return;

            // Copy recursivelly
            CopyRecursivelly(sourceDir, destinationDir);

            if (productionProcess.BuildCancelled)
                return;

            if (KeepConfigFile)
                // Restore the config file
                RestoreConfigFile(productionProcess);

            if (productionProcess.BuildCancelled)
                return;

            if (DeleteBackup && backupCreated)
            {
                // Delete backup
                productionProcess.LogLine("Deleting " + DestinationBackupPath);
                Directory.Delete(DestinationBackupPath, true);
            }

            if (productionProcess.BuildCancelled)
                return;

            if (Environment == EnvironmentType.WEB && StopIIS)
                // Start IIS
                StartStopIis(true, productionProcess);

        }

        public override string ToString()
        {
            return base.ToString() + " Copy to " + CopyDestination;
        }

    }
}
