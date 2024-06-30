using System;
using System.Net;
using System.Net.Mail;
using Artech.Architecture.Common.Services;
using Artech.MsBuild.Common;
using LSI.Packages.Extensiones.Utilidades;
using Microsoft.Build.Framework;
using LSI.Packages.Extensiones.Comandos.Build.Production;

namespace LSI.Packages.Extensiones.MsBuild
{
    /// <summary>
    /// Prepare kb production
    /// </summary>
    public class LsiPrepareProduction : LsiMsBuildTask
    {

        /// <summary>
        /// If false, if any previous task has failed, this task will not execute
        /// </summary>
        public bool RunWithErrors { get; set; } = false;

        /// <summary>
        /// Run the task
        /// </summary>
        /// <returns>False if the msbuild script should be stopped</returns>
        public override bool Execute()
        {
            try
            {
                OutputSubscribe();
                using (Utilidades.Logging.Log log = new Utilidades.Logging.Log())
                {
                    if(!RunWithErrors && !MsBuildLog.Instance.ProcessOk)
                    {
                        log.Output.AddWarningLine("LsiPrepareProduction: Some previous task has failed and RunWithErrors property is false. " +
                            "Prepare production skipped");
                        return true;
                    }

                    PrepareProduction production = PrepareProduction.LoadKbProduction(KB.DesignModel, 
                        KB.DesignModel.Environment.TargetModel);
                    RunBuildProcess(production, log, "Prepare production", false);
                    
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
