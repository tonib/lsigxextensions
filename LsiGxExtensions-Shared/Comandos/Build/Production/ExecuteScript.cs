using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.IO;
using System.Collections.ObjectModel;

namespace LSI.Packages.Extensiones.Comandos.Build.Production
{
    /// <summary>
    /// Production task to execute a powershell script
    /// </summary>
    public class ExecuteScript : ProductionTask
    {

        /// <summary>
        /// Absolute path of script file to execute
        /// </summary>
        public string ScriptPath;

        /// <summary>
        /// Run the script
        /// </summary>
        /// <param name="productionProcess"></param>
        public override void Execute(PrepareProduction productionProcess)
        {
            base.Execute(productionProcess);

            productionProcess.LogLine("Executing " + ScriptPath);

            using (PowerShell psInstance = PowerShell.Create())
            {
                string script = 
$@"$MODELPATH=""{productionProcess.TargetDirectory}""
$LASTCOPYDESTINATION=""{productionProcess.LastCopyPath}""
$GXENVNAME=""{productionProcess.TargetModel.Name}""
";

                script += File.ReadAllText(ScriptPath);

                psInstance.AddScript(script);
                
                // invoke execution on the pipeline (collecting output)
                Collection<PSObject> PSOutput = psInstance.Invoke();

                // loop through each output object item
                foreach (PSObject outputItem in PSOutput)
                    productionProcess.LogLine(outputItem.ToString());

                // Display warnings
                foreach (WarningRecord outputItem in psInstance.Streams.Warning)
                    productionProcess.LogWarningLine(outputItem.ToString());

                // Display errors
                foreach (ErrorRecord outputItem in psInstance.Streams.Error)
                    productionProcess.LogErrorLine(outputItem.ToString());
                
            }
        }

        public override string ToString()
        {
            return base.ToString() + "Execute " + ScriptPath;
        }

    }
}
