using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using Artech.Architecture.Common.Converters;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Commands;
using Artech.Genexus.Common.Entities;
using Artech.Genexus.Common.Services;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.CSharpWin;
using LSI.Packages.Extensiones.Utilidades.Threading;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using Artech.Genexus.Common.CustomTypes;
using System.ComponentModel;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos.Build
{
    /// <summary>
    /// Tool compile a set of Win C# mains
    /// </summary>
    public class CustomWinCompiler : BuildProcess
    {

        /// <summary>
        /// Operation to do with rsp files before compile
        /// </summary>
        public enum RepairRspsOption
        {
            RepairAll,
            RepairBcOnly,
            DoNotRepair
        }

        /// <summary>
        /// Set of mains to compile
        /// </summary>
        private IEnumerable<KBObject> MainsToCompile;

        /// <summary>
        /// Generator to compile
        /// </summary>
#if GX_17_OR_GREATER
        private GxGenerator Generator;
#else
        private GxEnvironment Generator;
#endif

        /// <summary>
        /// Compile Genexus.programs.commons.dll?
        /// </summary>
        public bool CompileCommons;

        /// <summary>
        /// Repair RSP files of modules to compile?
        /// </summary>
        public RepairRspsOption RepairRsp = RepairRspsOption.DoNotRepair;

        /// <summary>
        /// Current repair RSP files process. It will be not null only when process is running
        /// </summary>
        private RepairRspFiles RepairProcess;

        /// <summary>
        /// False (this class does not use the build functions of genexus)
        /// </summary>
        override public bool IsInternalGxBuild { get { return false; } }

        /// <summary>
        /// Constructor to compile a list of mains
        /// </summary>
        /// <param name="mainsToCompile">Set of mains to compile</param>
#if GX_17_OR_GREATER
        public CustomWinCompiler(GxGenerator generator, IEnumerable<KBObject> mainsToCompile)
#else
        public CustomWinCompiler(GxEnvironment generator, IEnumerable<KBObject> mainsToCompile)
#endif
        {
            Generator = generator;
            MainsToCompile = mainsToCompile;
        }

        /// <summary>
        /// It creates a BldAssembliesCustom.cs file into the target model directory to
        /// compile the modules
        /// </summary>
        /// <returns>The created BldAssembliesCustom.cs file absolute path</returns>
        private string CreateBldAssembliesFile()
        {
            // Get the file template:
            string template = Resources.BldAssemblies_cs;

            // Replace class name
            template = template.Replace("%BLDASSEMBLIESNAME%", "BldAssembliesCustom");

            // Create modules to compile entries
            string modulesEntries = string.Empty;

            if (CompileCommons)
            {
                string entry = "sc.Add(@\"bin\\" + CSharpUtils.COMMONSDLLFILENAME + "\", cs_path + @\"\\" + CSharpUtils.COMMONSRSPFILENAME + "\");";
                modulesEntries += entry + Environment.NewLine;
            }

            foreach (KBObject o in MainsToCompile)
            {
                // Add entry to compile the dll module
                string entry = string.Format("sc.Add(@\"bin\\{0}.dll\", cs_path + @\"\\{0}.rsp\");",
                    KBaseGX.GetProgramFileName(o, Generator, false, null));
                modulesEntries += entry + Environment.NewLine;

                // Check if the module .exe exists
                string exePath = CSharpUtils.GetWinMainExeFileName(o);
                if (!File.Exists(exePath))
                {
                    // Compile the exe
                    entry = string.Format("sc.Add(@\"bin\\{0}.exe\", cs_path + @\"\\{0}.rsp\");",
                        Path.GetFileNameWithoutExtension(exePath));
                    modulesEntries += entry + Environment.NewLine;
                }
            }

            // Replace modules to compile:
            template = template.Replace("%GETSORTEDBUILDLIST%", modulesEntries);

            // Save file:
            string filePath = Entorno.GetTargetDirectoryFilePath("BldAssembliesCustom.cs", Generator.Model);
            filePath = Entorno.GetUnusedFileName(filePath);
            LogLine("Generating " + filePath + " file");
            File.WriteAllText(filePath, template);
            return filePath;
        }

        private string GetCompilerPath()
        {
            ExeFileType compilerPath =
                Generator.Properties.GetPropertyValue(Properties.CSHARP.CompilerPath) as ExeFileType;
            if (compilerPath == null)
                throw new Exception(Generator.Description + " has no compiler path specified");
            return compilerPath.Location;
        }

        private void RunGxExec(string bldAssembliesPath)
        {
            // We will run a command line like this:
            //gxexec "C:\Users\toni\Documents\Compilador\kbasepruebas\Knowledge Base\CSharpModel\bldAssemblies.cs" 
            // -r:GxBaseBuilder.dll -arg:csc="C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\csc.exe"  -arg:mdlpath="C:\Users\toni\Documents\Compilador\kbasepruebas\Knowledge Base\CSharpModel"

            // Get the compiler path:
            string compilerPath = GetCompilerPath();
            if (compilerPath == null)
                return;

            // Build command line parameters
            string parameters = string.Format(
                @" ""{0}"" -r:GxBaseBuilder.dll -arg:csc=""{1}"" -arg:mdlpath=""{2}"" ",
                bldAssembliesPath, compilerPath, Entorno.GetTargetDirectory(Generator.Model));

            string gxExecPath = Entorno.GetTargetDirectoryFilePath( "GXEXEC.EXE" , Generator.Model);
            ExecuteProcess(gxExecPath, parameters);
        }

        private void RepaiRspFilesToCompile()
        {
            try
            {
                if (RepairRsp == RepairRspsOption.DoNotRepair)
                    return;

                LogLine("Checking RSP files...");
                RepairProcess = new RepairRspFiles(Generator.Model);
                RepairProcess.LogControl = this.LogControl;
                RepairProcess.ProcessLog = this.ProcessLog;
                RepairProcess.JustTest = false;
                RepairProcess.Verbose = false;
                RepairProcess.SaveLog = SaveLog;
                if (RepairRsp == RepairRspsOption.RepairAll)
                    RepairProcess.RepairSdts = true;
                else
                    // Repair BC only:
                    RepairProcess.RepairOnlyBC = true;
                RepairProcess.RepairRspModules(MainsToCompile);
                
                if (RepairProcess.CommonsRspUpdated)
                    // The commons.rsp has been updated: Compile it too
                    CompileCommons = true;

                if (RepairProcess.SaveLog)
                    // Append repair process log
                    TextLog.Append( RepairProcess.TextLog.ToString() );

                if (RepairProcess.BuildWithErrors)
                    // Save errors state
                    BuildWithErrors = true;

            }
            finally
            {
                RepairProcess = null;
            }
        }

        /// <summary>
        /// Do modifications over the generated main source code
        /// </summary>
        private void PatchMainSource()
        {

            // Is this LSI???
            bool atLsi = LsiExtensionsConfiguration.PrivateExtensionsInstalled;

            LogLine("Patching mains...");
            foreach (KBObject main in MainsToCompile)
            {
                // Get the call source file
                string fileName = "call_" + KBaseGX.GetProgramFileName(main, Generator, false, "cs");
                string path = Entorno.GetTargetDirectoryFilePath(fileName, Generator.Model);
                if (!File.Exists(path))
                    continue;
                string sourceCode = File.ReadAllText(path);

                bool codeModified = false;

                // 1) Adds a System.Environment.Exit(0) call to the mains execution finalization.
                // It's needed to avoid hangups due to a gx bug.

                // Check if it must to be patched
                const string CODETOREPLACE = ";\r\n   }\r\n\r\n}";
                if (sourceCode.Contains(CODETOREPLACE))
                {
                    codeModified = true;
                    // Do the patch:
                    LogLine("Adding System.Environment.Exit(0) call to " + fileName);
                    sourceCode = sourceCode.Replace(CODETOREPLACE, ";\r\n      System.Environment.Exit(0); /* LSI */\r\n   }\r\n\r\n}");
                }

                // 2) Add a verification for the J# installation. Only for LSI
                if (atLsi)
                {
                    string codeToReplace = "[STAThread]\r\n   static public void Main( string[] args )\r\n   {\r\n";
                    string newCode = codeToReplace + "      LSI.Excepciones.RepararJSharp.VerificarInstalacion(); /* LSI */\r\n\r\n";
                    if (!sourceCode.Contains(newCode))
                    {
                        LogLine("Adding LSI.Excepciones.RepararJSharp.VerificarInstalacion() call to " + fileName);
                        if (!sourceCode.Contains(codeToReplace))
                            LogErrorLine("Cannot find code to replace at " + fileName);
                        else
                        {
                            codeModified = true;
                            sourceCode = sourceCode.Replace(codeToReplace, newCode);
                        }
                    }
                }

                if (codeModified)
                    File.WriteAllText(path, sourceCode);
            }
        }


        override public void Execute()
        {
            try
            {

                RepaiRspFilesToCompile();

                if (BuildCancelled)
                    return;

                if( LsiExtensionsConfiguration.Load().PatchWinMainExits)
                    PatchMainSource();

                if (BuildCancelled)
                    return;

                string bldAssembliesPath = CreateBldAssembliesFile();

                if (BuildCancelled)
                    return;

                RunGxExec(bldAssembliesPath);

                if (BuildCancelled)
                    return;

                File.Delete(bldAssembliesPath);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        public override void Cancel()
        {
            base.Cancel();
            if (RepairProcess != null)
                RepairProcess.Cancel();
        }

        public override string ToString()
        {
            string txt = "Compile ";

            List<string> names = MainsToCompile.Select(x => x.QualifiedName.ToString()).ToList();
            if( CompileCommons )
                names.Insert(0, CSharpUtils.COMMONSDLLFILENAME);

            if (MainsToCompile.Count() <= 2)
                txt += string.Join(", ", names.ToArray());
            else
                txt += MainsToCompile.Count() + " modules";
            return txt;
        }

    }
}
