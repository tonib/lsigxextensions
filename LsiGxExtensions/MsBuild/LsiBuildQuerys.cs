using System;
using Artech.Architecture.Common.Services;
using LSI.Packages.Extensiones.Comandos.Build;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.MsBuild
{
    /// <summary>
    /// Generate query objects
    /// </summary>
    public class LsiBuildQuerys : LsiMsBuildTask
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
                    BuildQueryObjects buildProcess = new BuildQueryObjects(this.KB.DesignModel.Environment.TargetModel);
                    RunBuildProcess(buildProcess, log, "Query objects", true);
                    return true;
                }
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
