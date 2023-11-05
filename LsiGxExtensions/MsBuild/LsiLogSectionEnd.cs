using System;
using Artech.Architecture.Common.Services;
using LSI.Packages.Extensiones.Comandos.Build;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.MsBuild
{
    /// <summary>
    /// Msbuild task to end record logging to send an email
    /// </summary>
    public class LsiLogSectionEnd : LsiMsBuildTask
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

                if (LsiLogSectionStart.CurrentSection != null)
                    LsiLogSectionStart.CurrentSection.CloseCurrentSection();

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
