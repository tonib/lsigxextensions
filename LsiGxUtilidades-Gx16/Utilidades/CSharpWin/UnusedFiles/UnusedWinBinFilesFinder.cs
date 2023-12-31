﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Entities;
using Artech.Genexus.Common;
using LSI.Packages.Extensiones.Utilidades.Reflection;
using Artech.Genexus.Common.Objects;

namespace LSI.Packages.Extensiones.Utilidades.CSharpWin.UnusedFiles
{
    /// <summary>
    /// Search for unused files on model directory. This will work only if a dlls is generated by main
    /// </summary>
    public class UnusedWinBinFilesFinder : UnusedFilesFinderBase
    {

        /// <summary>
        /// Target model to check
        /// </summary>
        private KBModel TargetModel;

        /// <summary>
        /// Design model
        /// </summary>
        private KBModel DesignModel;

        /// <summary>
        /// Get the genexus dlls / exes for .NET generator
        /// </summary>
        /// <returns></returns>
        private List<string> GetGenexusFileNames()
        {
            // Get the genexus dir for .NET generator
            string netPath = Path.GetDirectoryName( Application.ExecutablePath );
            netPath = Path.Combine(netPath, "gxnetwin");
            DirectoryInfo baseDir = new DirectoryInfo(netPath);

            // Get dlls / exes names
            List<string> result = new List<string>();
            result.AddRange(baseDir.GetFiles("*.dll", SearchOption.AllDirectories).Select(x => x.Name.ToLower()));
            result.AddRange(baseDir.GetFiles("*.exe", SearchOption.AllDirectories).Select(x => x.Name.ToLower()));

            // Extra files:
            result.Add("reorganization.dll");
            result.Add("gxoffice2.dll");
            result.Add("gxoffice2lib.dll");
            result.Add("genexus.programs.common.dll");
            result.Add("developermenu.exe");

            return result;
        }

        /// <summary>
        /// The model bin directory
        /// </summary>
        override public DirectoryInfo BaseDirectory
        {
            get { return new DirectoryInfo(Entorno.GetTargetDirectoryFilePath("bin", TargetModel)); }
        }

        /// <summary>
        /// Base path, relative to Kb path
        /// </summary>
        override public string BaseDirectoryRelativeToKb
        {
            get { return Path.Combine(TargetModel.TargetPath, "bin"); }
        }

        /// <summary>
        /// Get the dll / exes names on the bin directory
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<string> GetBinFileNames()
        {
            List<string> result = new List<string>();
            result.AddRange(BaseDirectory.GetFiles("*.dll", SearchOption.TopDirectoryOnly).Select(x => x.Name.ToLower()));
            result.AddRange(BaseDirectory.GetFiles("*.exe", SearchOption.TopDirectoryOnly).Select(x => x.Name.ToLower()));
            return result;
        }

        private List<string> GetMainsFileNames(List<KBObject> mainObjects)
        {
            List<string> result = new List<string>();
            // Add main dlls
            result.AddRange(mainObjects.Select(x => x.Name.ToLower() + ".dll"));
            // Add exes
            result.AddRange( mainObjects.Select( x => Path.GetFileName( CSharpUtils.GetWinMainExeFileName(x) ).ToLower() ) );

            return result;
        }

        private List<string> GetCompilerOptionsDllsRefs(List<KBObject> mainObjects)
        {
            List<string> compilerOptionsDependencies = new List<string>();

            // I don't know how to get the generator from the model. I only know how to get it from a main...
#if GX_17_OR_GREATER
            GxGenerator generator = null;
#else
            GxEnvironment generator = null;
#endif
            if (mainObjects.Count == 0)
                return new List<string>();

            generator = MainsGx.GetMainGenerator(mainObjects[0]);

            string compilerOptions =
                generator.Properties.GetPropertyValue<string>(Properties.CSHARP.CompilerFlags);
            if (string.IsNullOrEmpty(compilerOptions))
                return compilerOptionsDependencies;

            // Example of compiler options:
            // /platform:x86 /debug /win32icon:Lsi.ico /lib:bin /r:xExterno20_X1.dll;AxInterop.SHDocVw.dll;Interop.SHDocVw.dll;AxInterop.MSHTML.dll;Microsoft.mshtml.dll;System.Web.dll;vjswfc.dll;vjslib.dll;ICSharpCode.SharpZipLib.dll;xFirmaElectronica.dll;ClienteFirma.dll /define:LSIDEBUG

            // Search the dll references:
            foreach( string option in compilerOptions.Split( new char[] { ' ' } ,StringSplitOptions.RemoveEmptyEntries ) )
            {
                if( option.StartsWith("/r:") )
                {
                    string references = option.Substring(3);
                    compilerOptionsDependencies.AddRange(
                        references.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => Path.GetFileName(x).ToLower())
                    );
                }
            }

            // Traverse dependencies
            string binPath = Entorno.GetTargetDirectoryFilePath("bin", TargetModel);
            compilerOptionsDependencies.AddRange( DependenciesWalker.Execute(compilerOptionsDependencies, binPath) );

            return compilerOptionsDependencies;
        }

        private void GetUnusedPdbs(List<string> usedFiles)
        {
            IEnumerable<string> usedPdbs = usedFiles.Select(x => Path.GetFileNameWithoutExtension(x) + ".pdb");

            IEnumerable<string> realPdbs = BaseDirectory
                .GetFiles("*.pdb", SearchOption.TopDirectoryOnly)
                .Select(x => x.Name.ToLower());

            string baseDir = BaseDirectory.FullName;
            foreach (string pdbName in realPdbs.Where(x => !usedPdbs.Contains(x)))
            {
                PublishFile(baseDir, pdbName);
                IncreaseSearchedObjects();
            }
        }

        private void GetTmpFiles()
        {
            string baseDir = BaseDirectory.FullName;
            IEnumerable<string> tmpFiles = BaseDirectory.GetFiles("*.tmp", SearchOption.TopDirectoryOnly).Select(x => x.Name.ToLower());
            foreach (string tmp in tmpFiles)
                PublishFile(baseDir, tmp);
        }

        new private void PublishFile(string baseDir, string fileName)
        {
            // Do no report messages dlls:
            if (fileName.StartsWith("messages.") && fileName.EndsWith(".dll"))
                return;

            base.PublishFile(baseDir, fileName);
        }

        /// <summary>
        /// Search dlls referenced by external objects
        /// </summary>
        /// <returns>List of referenced dlls by external objects, lowercase and trimmed</returns>
        private List<string> ExternalObjectsReferences()
        {
            List<string> result = new List<string>();
            foreach(ExternalObject eo in ExternalObject.GetAll(DesignModel))
            {
                string assemblyName = eo.GetPropertyValue<string>(Properties.EXO.AssemblyName);
                if (!string.IsNullOrEmpty(assemblyName))
                {
                    assemblyName = assemblyName.Trim().ToLower();
                    if (!result.Contains(assemblyName))
                        result.Add(assemblyName);
                }
            }
            
            // Traverse dependencies
            string binPath = Entorno.GetTargetDirectoryFilePath("bin", TargetModel);
            result.AddRange(DependenciesWalker.Execute(result, binPath));

            return result;
        }

        override public void ExecuteUISearch()
        {
            List<KBObject> mainObjects = MainsGx.GetMainsByGenerator(DesignModel, GeneratorType.CSharpWin);

            List<string> usedFiles = new List<string>();

            // Genexus dlls / exes
            usedFiles.AddRange( GetGenexusFileNames() );

            // Referenced dlls by compiler options
            usedFiles.AddRange(GetCompilerOptionsDllsRefs(mainObjects));

            // Rerenced dlls by external objects
            usedFiles.AddRange(ExternalObjectsReferences());

            // Main objects dlls / exes
            usedFiles.AddRange(GetMainsFileNames(mainObjects));

            // Check real dlls / exes in bin dir:
            string baseDir = BaseDirectory.FullName;
            foreach (string fileName in GetBinFileNames().Where(x => !usedFiles.Contains(x)))
            {
                PublishFile(baseDir, fileName);
                IncreaseSearchedObjects();
            }

            // Check pdbs in bin dir:
            GetUnusedPdbs(usedFiles);

            // tmp files:
            GetTmpFiles();

        }

        public UnusedWinBinFilesFinder(KBModel targetModel)
        {
            TargetModel = targetModel;
            DesignModel = TargetModel.GetDesignModel();
        }
    }
}
