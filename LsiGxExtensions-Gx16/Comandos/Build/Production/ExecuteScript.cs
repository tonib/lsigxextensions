using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.IO;
using System.Collections.ObjectModel;
using System.Diagnostics;
using LSI.Packages.Extensiones.Utilidades.Threading;

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

        // This has stopped running after Windows update, in .NET 3.5 (Gxxev3)
        //        /// <summary>
        //        /// Run the script
        //        /// </summary>
        //        /// <param name="productionProcess"></param>
        //        public override void Execute(PrepareProduction productionProcess)
        //        {
        //            base.Execute(productionProcess);

        //            productionProcess.LogLine("Executing " + ScriptPath);

        //            using (PowerShell psInstance = PowerShell.Create())
        //            {
        //                string script = 
        //$@"$MODELPATH=""{productionProcess.TargetDirectory}""
        //$LASTCOPYDESTINATION=""{productionProcess.LastCopyPath}""
        //$GXENVNAME=""{productionProcess.TargetModel.Name}""
        //";

        //                script += File.ReadAllText(ScriptPath);

        //                psInstance.AddScript(script);

        //                // invoke execution on the pipeline (collecting output)
        //                Collection<PSObject> PSOutput = psInstance.Invoke();

        //                // loop through each output object item
        //                foreach (PSObject outputItem in PSOutput)
        //                    productionProcess.LogLine(outputItem.ToString());

        //                // Display warnings
        //                foreach (WarningRecord outputItem in psInstance.Streams.Warning)
        //                    productionProcess.LogWarningLine(outputItem.ToString());

        //                // Display errors
        //                foreach (ErrorRecord outputItem in psInstance.Streams.Error)
        //                    productionProcess.LogErrorLine(outputItem.ToString());

        //            }
        //        }

        /// <summary>
        /// Run the script
        /// </summary>
        /// <param name="productionProcess"></param>
        public override void Execute(PrepareProduction productionProcess)
        {
            base.Execute(productionProcess);

            productionProcess.LogLine("Executing " + ScriptPath);

            // You can inject variable values with a trick:
            // powershell.exe -Command "$variable='value'; & 'C:\x\script.ps1'"
            
            Dictionary<string, string> varValues = new Dictionary<string, string>();
            varValues.Add("$MODELPATH", productionProcess.TargetDirectory);
            varValues.Add("$LASTCOPYDESTINATION", productionProcess.LastCopyPath);
            varValues.Add("$GXENVNAME", productionProcess.TargetModel.Name);
            string varAssignment = string.Join(" ; ", varValues.Select(kv => $"{kv.Key}='{kv.Value}'").ToArray());

            // Be sure return zero code if successful, -1 if error (and write error message)
            string scriptExecution = varAssignment + $" ; & '{ScriptPath}' ; exit 0 ";
            scriptExecution = $@"try {{ {scriptExecution} }} catch {{ $_ ; exit -1 }} ";

            Process process = new Process();
            process.StartInfo.FileName = "powershell";
            process.StartInfo.Arguments = $@"-Command ""{scriptExecution}""";
            // productionProcess.LogLine($"{process.StartInfo.FileName} {process.StartInfo.Arguments}");
            int exitCode = ProcessLogger.StartAndLogProcess(process, productionProcess);
            if(exitCode != 0)
                productionProcess.LogErrorLine("Powershell execution failed, exit code " + exitCode);
            else
                productionProcess.LogLine("Powershell execution succeeded");
        }

        public override string ToString()
        {
            return base.ToString() + "Execute " + ScriptPath;
        }

    }
}
