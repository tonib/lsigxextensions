using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Threading;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common;
using Artech.Udm.Framework;
using Artech.Architecture.Common.Objects;
using LSI.Packages.Extensiones.Utilidades.CallsAnalisys;
using LSI.Packages.Extensiones.Utilidades.CSharpWin;
using System.Diagnostics;
using System.Threading;
using System.IO;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos.Build
{

    /// <summary>
    /// Tool to repair RSP files of a Win / C# environment.
    /// </summary>
    public class RepairRspFiles : BuildProcess
    {

        /// <summary>
        /// Number of days to check
        /// </summary>
        private const int NDAYSTOCHECK = 2;

        /// <summary>
        /// Messages SDT source file
        /// </summary>
        private const string MESSAGESFILE = "type_SdtMessages_Message.cs";

        /// <summary>
        /// True if we just test the changes to do. False to do the needed changes
        /// </summary>
        public bool JustTest = true;

        /// <summary>
        /// Object types to ignore in graph visits
        /// </summary>
        private List<Guid> TypesToIgnore = new List<Guid>();

        /// <summary>
        /// Table kbase modules, indexed by their Guid
        /// </summary>
        private Dictionary<Guid, Module> ModulesTable;

        /// <summary>
        /// Source files that should go to the commons RSP file.
        /// </summary>
        private HashSet<string> CommonsSourceFiles = new HashSet<string>();

        /// <summary>
        /// Should we repair the genexus.commons?
        /// </summary>
        public bool RepairSdts = true;

        /// <summary>
        /// Should we repair only bussiness components source files?
        /// </summary>
        public bool RepairOnlyBC = false;

        /// <summary>
        /// Verbose log?
        /// </summary>
        public bool Verbose = true;

        /// <summary>
        /// True if the the Genexus.Commons.Programs.rsp has been updated
        /// </summary>
        public bool CommonsRspUpdated = false;

        /// <summary>
        /// Target model where repair RSPs
        /// </summary>
        private KBModel TargetModel;

        /// <summary>
        /// False (this class does not use the build functions of genexus)
        /// </summary>
        override public bool IsInternalGxBuild { get { return false; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">Target model where to repair RSP files</param>
        public RepairRspFiles(KBModel targetModel)
        {
            // File types not included on c# win RSP files:
            TypesToIgnore.Add(ObjClass.DataSelector);
            TypesToIgnore.Add(ObjClass.WebPanel);

            // Get kbase modules:
            ModulesTable = Module.GetAll(targetModel).ToDictionary(x => x.Guid);
            TargetModel = targetModel;
        }

        private ObjectsGraph BuildGraph()
        {
            if( Verbose )
                LogLine("Building calls graph...");

            Stopwatch graphTime = new Stopwatch();
            graphTime.Start();

            ObjectsGraph graph = new ObjectsGraph(TargetModel);
            FullGraphBuilder builder = new FullGraphBuilder(graph);
            builder.BuildGraph();

            graphTime.Stop();

            if( Verbose )
                LogLine(string.Format(
                    "{0} objects on graph, {1} vertices. Build time: {2} ms.",
                    graph.Nodes.Count , graph.Vertices.Count , graphTime.ElapsedMilliseconds));

            return graph;
        }

        private void RepairCommons()
        {
            try
            {
                RspFile rsp = new RspFile(CSharpUtils.COMMONSRSPFILENAME, TargetModel);
                List<string> sourceFiles = CommonsSourceFiles.ToList();

                // Check if type_SdtMessages_Message.cs exists (its needed by BC's)
                string messagesPath = Path.Combine( Path.GetDirectoryName(rsp.RspPath) , MESSAGESFILE );
                if( File.Exists(messagesPath) )
                    sourceFiles.Add(MESSAGESFILE);

                if (RepairRspFile(CSharpUtils.COMMONSRSPFILENAME, sourceFiles, rsp))
                    CommonsRspUpdated = true;
            }
            catch (Exception ex)
            {
                LogErrorLine("Error checking " + CSharpUtils.COMMONSRSPFILENAME + ": " + ex.Message);
            }
        }

        private List<string> GetMainSourceFiles(ObjectsGraph graph, KBObject main)
        {
            // Get main object source files:
            List<KBObject> mainObjects = graph.GetDeepCalledObjects(main.Key, TypesToIgnore);
            List<string> sourceFiles = new List<string>();
            GeneratedSourceFilesCache sourcesCache = GeneratedSourceFilesCache.Cache(TargetModel.KB);
            foreach (KBObject o in mainObjects)
            {
                // Get sources for the module rsp:
                ObjectSourceFiles mainSources = sourcesCache.GetSourceFiles(o);
                if (RepairOnlyBC && !mainSources.IsBC)
                    // We want to repare only BC, ignore this object
                    continue;

                sourceFiles.AddRange(mainSources.GetModuleSourceFiles(ModulesTable, false));
                // Get sources for the commons rsp:
                foreach (string sourceFile in mainSources.CommonsSourceFiles)
                    CommonsSourceFiles.Add(sourceFile);
            }

            // Add the own main source files. On the previous loop was added the entry point only
            ObjectSourceFiles mainOwnSources = sourcesCache.GetSourceFiles(main);
            sourceFiles.AddRange(mainOwnSources.GetModuleSourceFiles(ModulesTable, true));

            return sourceFiles;
        }

        private void CheckMain(ObjectsGraph graph, KBObject main)
        {
            try
            {
                // Get main object source files:
                List<string> sourceFiles = GetMainSourceFiles(graph, main);

                RspFile rsp = new RspFile(main);
                RepairRspFile(main.Name, sourceFiles, rsp);
            }
            catch (Exception ex)
            {
                LogErrorLine("Error checking " + main.Name + ": " + ex.Message);
            }
        }

        private bool RepairRspFile(string mainName, IEnumerable<string> sourceFiles, RspFile rsp)
        {
            List<string> missingFiles = rsp.NotContained(sourceFiles);
            if (missingFiles.Count > 0)
            {
                LogLine("File for " + mainName + " missed following files:");
                List<string> filesToAdd = new List<string>();
                foreach (string filename in missingFiles)
                {
                    if (File.Exists(Entorno.GetTargetDirectoryFilePath(filename, TargetModel)))
                    {
                        LogLine(filename);
                        filesToAdd.Add(filename);
                    }
                    else
                        LogErrorLine(filename + " (It does not exists, not added to RSP)");
                }

                if (!JustTest)
                {
                    // Repair RSP
                    string backupFile = Entorno.DoFileBackup(rsp.RspPath, false);
                    if( Verbose )
                        LogLine("RSP backup saved on " + backupFile);
                    rsp.AddFiles(filesToAdd);
                    rsp.Save();
                    return true;
                }
            }
            return false;
        }

        public void RepairRspModules(IEnumerable<KBObject> modules)
        {
            try
            {
                if( Verbose )
                    LogLine("Checking directory " + Entorno.GetTargetDirectory(TargetModel));

                // Build graph
                ObjectsGraph graph = BuildGraph();

                if (BuildCancelled)
                    return;

                // Check each main RSP file
                if( Verbose )
                    LogLine("Checking RSP files...");
                if (modules == null)
                    modules = graph.GraphWinMainObjects;
                foreach (KBObject main in modules)
                {
                    CheckMain(graph, main);
                    if (BuildCancelled)
                        return;
                }

                // Check SDTs
                if( RepairSdts )
                    RepairCommons();

                if (BuildCancelled)
                    return;

                // Save generated source file names cache
                if( Verbose )
                    LogLine("Saving generated source file names cache...");
                try
                {
                    GeneratedSourceFilesCache.Cache(TargetModel.KB).Save(TargetModel.KB);
                }
                catch (Exception exSave)
                {
                    // Sometimes there are concurrent saves with concurrent compilations
                    LogWarningLine("Failed to save cache: " + exSave.Message);
                }
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                base.BuildWithErrors = true;
            }
        }

        public bool AskJustTest()
        {
            RepairRspConfirm confirm = new RepairRspConfirm();
            if (confirm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return false;
            JustTest = confirm.JustTest;
            confirm.Dispose();
            return true;
        }

        override public void Execute()
        {
            RepairRspModules(null);
        }

        public override string ToString()
        {
            return "Repair RSP";
        }

        /// <summary>
        /// Add missing source files to some modules
        /// </summary>
        /// <param name="missingSources">The key is the main object name, and the value is the set 
        /// of objects not found</param>
        public void AddSourcesToModules(Dictionary<string, HashSet<string>> missingSources)
        {
            foreach (string module in missingSources.Keys)
            {
                RspFile rsp = new RspFile(module + ".rsp", TargetModel);
                RepairRspFile(module, missingSources[module], rsp);
            }
        }
    }
}
