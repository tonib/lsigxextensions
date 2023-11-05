using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Artech.Architecture.Common.Converters;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Commands;
using Artech.Genexus.Common.Entities;
using Artech.Genexus.Common.Services;
using Artech.Udm.Framework;
using Artech.Udm.Framework.Multiuser;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.CallsAnalisys;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.Logging;
using Artech.Architecture.Common.Services;

namespace LSI.Packages.Extensiones.Comandos.Build
{
    // TODO: If the generator is WIN, stop on runsets.ini, and do all the graph stuff.

    /// <summary>
    /// Tool to do a build all with a single generator
    /// </summary>
    public class BuildSingleGenerator : BuildProcess
    {

        /// <summary>
        /// Generator to run
        /// </summary>
        private GxEnvironment Generator;

        /// <summary>
        /// Compile process. It can be null
        /// </summary>
        private CompileModules Compiler;

        /// <summary>
        /// True if we should specify pending objects
        /// </summary>
        private bool SpecifyPendingObjects = true;

        /// <summary>
        /// True (this class uses the build functions of genexus)
        /// </summary>
        override public bool IsInternalGxBuild { get { return true; }  }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generator">Generator to run</param>
        public BuildSingleGenerator(GxEnvironment generator, bool specifyPendingObjects)
        {
            Generator = generator;
            SpecifyPendingObjects = specifyPendingObjects;
        }

        private bool Generate(Log log)
        {

            DevelopmentWorkingSet workingSet = new DevelopmentWorkingSet(
                UIServices.KB.CurrentModel,
                UIServices.KB.WorkingEnvironment.TargetModel);

            if (!KBaseGX.DoCopyModel(workingSet, this))
            {
                log.ProcesoOk = false;
                return false;
            }

            if (BuildCancelled)
                return true;

            // TODO: Gx15, remove keysToSpecify
            List<EntityKey> keysToSpecify = new List<EntityKey>();
            if (SpecifyPendingObjects)
            {
                // Gx15: The ObjectListCalcCommand no longer has has no public constructor, so this will no longer works
                // The problem is that we cannot longer calculate keysToSpecify...
                /*
                LogLine("Checking objects to specify...");
                ObjectListCalcCommand toSpecifyCalculator = new ObjectListCalcCommand(workingSet,
                    new List<EntityKey>(), BuildOptions.Specify);
                toSpecifyCalculator.Do();
                keysToSpecify = toSpecifyCalculator.ObjectsToSpecify.ToList();
                LogLine(keysToSpecify.Count + " to specify");

                if (BuildCancelled)
                    return true;

                // Specify objects
                if (keysToSpecify.Count > 0)
                {
                    if (!SpecifyObjects(workingSet, keysToSpecify))
                        return false;
                }

                if (BuildCancelled)
                    return true;
                */
                SpecifyObjects(workingSet);
                if (BuildCancelled)
                    return true;
            }

            // Generate resources
            GenerateResources(workingSet);

            if (BuildCancelled)
                return true;

            // Generate objects
            if (!GenerateObjects(workingSet))
                return false;

            if (BuildCancelled)
                return true;

            // Get main objects keys to compile
            LogLine("Searching main callers...");
            ObjectsGraph graph = new ObjectsGraph(workingSet.WorkingModel);
            FullGraphBuilder graphBuilder = new FullGraphBuilder(graph);
            graphBuilder.BuildGraph();
            List<EntityKey> mainCallersKeys = graph.GetMainCallers(keysToSpecify);

            if (BuildCancelled)
                return true;

            // Load main objects:
            List<KBObject> mainCallers = new List<KBObject>();
            mainCallersKeys.ForEach(x => mainCallers.Add(graph.GetObject(x)));

            if (BuildCancelled)
                return true;

            // Compile
            return Compile(workingSet, mainCallers);
        }

        /// <summary>
        /// Compile modules
        /// </summary>
        /// <param name="workingSet">Current design / target model</param>
        /// <param name="mainCallers">Main objects to compile</param>
        /// <returns>True if the compilation was succeed</returns>
        private bool Compile(DevelopmentWorkingSet workingSet, List<KBObject> mainCallers)
        {
            try
            {
                SubscribeGXOutput();
                // Compile
                LogLine("Compiling...");
                Compiler = new CompileModules(workingSet.WorkingModel, mainCallers,
                    Generator);
                Compiler.Execute();
                return !Compiler.BuildWithErrors;
            }
            finally
            {
                UnsuscribeGxOutput();
            }
        }

        private void GenerateResources(DevelopmentWorkingSet workingSet)
        {
            try
            {
                SubscribeGXOutput();
                GenexusBLServices.Generators.GenerateResources(workingSet.WorkingModel);
            }
            finally
            {
                UnsuscribeGxOutput();
            }
        }

        private bool GenerateObjects(DevelopmentWorkingSet workingSet)
        {
            LogLine("Generation started using " + Generator.Description + "...");
            try
            {
                SubscribeGXOutput();
                if (!GenexusBLServices.Generators.Generate(workingSet.WorkingModel,
                    Generator.Generator))
                    return false;
            }
            finally
            {
                UnsuscribeGxOutput();
            }
            return true;
        }

        /// <summary>
        /// Specifies all pending objects
        /// </summary>
        /// <param name="workingSet">Curent design / target model</param>
        /// <returns>True if the specification finished succesfully. False otherwise</returns>
        private bool SpecifyObjects(DevelopmentWorkingSet workingSet)
        {
            // Specify objects
            SubscribeGXOutput();
            try
            {
                return GenexusBLServices.Specifier.SpecifyAll(
                    workingSet.WorkingModel, BuildOptions.Specify);
            }
            catch
            {
                return false;
            }
            finally
            {
                UnsuscribeGxOutput();
            }
        }

        /// <summary>
        /// It cancels the build process
        /// </summary>
        override public void Cancel()
        {
            base.Cancel();
            GenexusBLServices.Specifier.Cancel();
            GenexusBLServices.Generators.Cancel();
            if (Compiler != null)
                Compiler.Cancel();
        }

        override public void Execute()
        {
            using (Log log = new Log(Log.BUILD_OUTPUT_ID, false))
            {
                try
                {
                    ProcessLog = log;
                    LogLine("Build all with " + Generator.Description + " started.");
                    log.StartTimeCount();
                    if (!Generate(log))
                        LogErrorLine("Build all finished WITH ERRORS");
                }
                catch (Exception ex)
                {
                    LogErrorLine("Error: " + ex.Message);
                    Log.ShowException(ex);
                }
            }
        }

        public override string ToString()
        {
            return "Build all (" + Generator.Description + ")";
        }

    }
}
