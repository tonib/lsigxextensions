using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Entities;
using Artech.MsBuild.Common;
using LSI.Packages.Extensiones.Comandos.Build;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.CSharpWin;
using LSI.Packages.Extensiones.Utilidades.VS;
using Microsoft.Build.Framework;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.MsBuild
{
    /// <summary>
    /// Task for nightly builds
    /// </summary>
    public class LsiCompileTask : LsiMsBuildTask
    {

        /// <summary>
        /// The generator to compile. If it's null, all generators will be compiled
        /// </summary>
        public string Generator { get; set; }

        /// <summary>
        /// Kind of RSP files reparation: "default": Repair all if /debug is set, only BC if
        /// /debug is not set. "all": Repair all. "none": Do not repair RSP files.
        /// "onlybc": Repair only bussiness components.
        /// </summary>
        public string RepairRsp { get; set; }

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
                    Dictionary<GxEnvironment, List<KBObject>> mainsByGenerator = 
                        MainsGx.GetMainsByGenerator(this.KB.DesignModel.Environment.TargetModel);

                    // Check each generator
                    foreach (GxEnvironment generator in mainsByGenerator.Keys)
                    {

                        if ( !string.IsNullOrEmpty(Generator) && generator.Description.ToLower() != Generator.ToLower())
                            // This is not the right generator
                            continue;

                        // Compile generator mains
                        bool isWeb = MainsGx.IsWebGenerator(generator);
                        CompileGenerator(log, mainsByGenerator[generator], generator, isWeb);

                    }

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

        /// <summary>
        /// Kind of RSP files repair on win / c# environment
        /// </summary>
        private CustomWinCompiler.RepairRspsOption GetRepairRspValue(GxEnvironment generator)
        {
            if( string.IsNullOrEmpty(RepairRsp))
                RepairRsp = "default";
            RepairRsp = RepairRsp.ToLower();

            if (RepairRsp == "all")
                return CustomWinCompiler.RepairRspsOption.RepairAll;
            else if (RepairRsp == "none")
                return CustomWinCompiler.RepairRspsOption.DoNotRepair;
            else if(RepairRsp == "onlybc")
                return CustomWinCompiler.RepairRspsOption.RepairBcOnly;
            else if (RepairRsp == "default")
            {
                // If it's production, repair only BC
                if (CSharpUtils.GeneratorHasDebugOption(generator))
                    return CustomWinCompiler.RepairRspsOption.RepairAll;
                else
                    return CustomWinCompiler.RepairRspsOption.RepairBcOnly;
            }
            else
                throw new Exception("Wrong value for RepairRsp property: " + RepairRsp);
        }

        /// <summary>
        /// Compile the mains associated to a generator
        /// </summary>
        /// <param name="log">Main log</param>
        /// <param name="mains">Main objects to compile</param>
        /// <param name="generator">Generator of the main objects</param>
        /// <param name="isWeb">True if the generator is web</param>
        private void CompileGenerator(Log log, 
            List<KBObject> mains, GxEnvironment generator, 
            bool isWeb)
        {
            // Define the compilation process
            BuildProcess buildProcess;

            // Should we use custom compile (c# / win / assemblies by main) ?
            bool customCompile = false;
            if (generator.Generator == (int)GeneratorType.CSharpWin)
            {
                string type =
                    generator.Properties.GetPropertyValue(Properties.CSHARPWIN.AssembliesStructure)
                    as string;
                if (type == Properties.CSHARPWIN.AssembliesStructure_Values.ByMain)
                    customCompile = true;
            }

            if (customCompile)
            {
                // Custom compilation
                CustomWinCompiler compiler = new CustomWinCompiler(generator, mains);
                compiler.RepairRsp = GetRepairRspValue(generator);
                compiler.CompileCommons = true;
                buildProcess = compiler;
            }
            else
            {
                // Standard compilation
                CompileModules compiler =
                    new CompileModules(this.KB.DesignModel.Environment.TargetModel, mains, generator);
                buildProcess = compiler;
            }

            // Run the compilation process
            string description = generator.Description + " compilation";
            RunBuildProcess(buildProcess, log, description, isWeb);
        }

    }
}
