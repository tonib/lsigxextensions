using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Artech.Architecture.Common.Converters;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Commands;
using Artech.Genexus.Common.Entities;
using Artech.Genexus.Common.Services;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.CSharpWin;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos.Build
{
    /// <summary>
    /// Tool to compile a set of modules with Genexus functions
    /// </summary>
    public class CompileModules : BuildProcess
    {

        /// <summary>
        /// Main objects to compile
        /// </summary>
        private IEnumerable<KBObject> MainsToCompile;

        /// <summary>
        /// Model where to compile
        /// </summary>
        private KBModel TargetModel;

        /// <summary>
        /// Generator to use with compilation.
        /// </summary>
        private GxEnvironment Generator;

        /// <summary>
        /// True (this class uses the build functions of genexus)
        /// </summary>
        override public bool IsInternalGxBuild { get { return true; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public CompileModules(KBModel targetModel, 
            IEnumerable<KBObject> mainsToCompile, GxEnvironment generator) 
        {
            TargetModel = targetModel;
            MainsToCompile = mainsToCompile;
            Generator = generator;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CompileModules(KBModel targetModel, IEnumerable<KBObject> mainsToCompile)
        {
            TargetModel = targetModel;
            MainsToCompile = mainsToCompile;
            
            // Get the generator of the first main:
            KBObject main = mainsToCompile.FirstOrDefault();
            if (main == null)
                return;

            // Get the main generator:
            Generator = MainsGx.GetMainGenerator(main);
            if( Generator == null )
                throw new Exception(main.QualifiedName + " is not main");
        }

        override public void Execute()
        {
            try
            {
                using (Log log = new Log(Log.BUILD_OUTPUT_ID, false))
                {
                    ProcessLog = log;

                    try
                    {
                        if (GenexusUIServices.Build.IsBuilding)
                        {
                            LogErrorLine("There is a Genexus build running. This extension cannot be executed");
                            return;
                        }
                    }
                    catch
                    {
                        // This will fail if we are running a msbuild script
                    }

                    // Get compilation information:
                    List<CompileItemInfo> toCompileInfo = new List<CompileItemInfo>();
                    foreach (KBObject main in MainsToCompile)
                    {
                        if (BuildCancelled)
                            return;

                        if (main == null)
                            continue;

                        // Be sure to compile only one generator at time
                        GxEnvironment mainGenerator = MainsGx.GetMainGenerator(main);
                        if (mainGenerator != null && mainGenerator.Generator == Generator.Generator)
                        {
                            toCompileInfo.Add(new CompileItemInfo(main.Key)
                            {
                                IsMain = true,
                                ShouldCompile = true,
                                WasSpecified = true
                            });
                        }
                    }

                    if (BuildCancelled)
                        return;

                    // Compile
                    try
                    {
                        SubscribeGXOutput();
                        this.BuildWithErrors = !GenexusBLServices.Run.Compile(TargetModel, toCompileInfo, Generator);
                    }
                    finally
                    {
                        UnsuscribeGxOutput();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                BuildWithErrors = true;
            }
        }

        override public void Cancel()
        {
            base.Cancel();
            GenexusBLServices.Run.Cancel();
        }

        public override string ToString()
        {
            string txt = "Compile ";
            if (MainsToCompile.Count() <= 2)
                txt += string.Join(", ", MainsToCompile.Select(x => x.Name).ToArray());
            else
                txt += MainsToCompile.Count() + " modules";
            return txt;
        }

    }
}
