using System;
using Artech.Architecture.Common.Services;
using LSI.Packages.Extensiones.Comandos.Build;
using LSI.Packages.Extensiones.Utilidades;

namespace LSI.Packages.Extensiones.MsBuild
{

    /// <summary>
    /// Task to remove backup files on the current environment
    /// </summary>
    public class LsiDeleteBackups : LsiMsBuildTask
    {

        /// <summary>
        /// Run the task
        /// </summary>
        /// <returns>False if the msbuild script should be stopped</returns>
        public override bool Execute()
        {
            try
            {
                OutputSubscribe();

                using (LSI.Packages.Extensiones.Utilidades.Logging.Log log =
                    new LSI.Packages.Extensiones.Utilidades.Logging.Log()) 
                {
                    RemoveBackupFiles build = new RemoveBackupFiles(this.KB.DesignModel.Environment.TargetModel);
                    RunBuildProcess(build, log, "Remove backup files", false);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                CommonServices.Output.AddErrorLine(ex.Message);
                return true;
            }
            finally
            {
                OutputUnsubscribe();
            }
        }

    }
}
