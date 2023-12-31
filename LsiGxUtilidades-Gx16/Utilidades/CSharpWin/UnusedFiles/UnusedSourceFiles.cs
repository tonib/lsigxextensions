﻿using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Entities;
using Artech.Udm.Framework;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LSI.Packages.Extensiones.Utilidades.CSharpWin.UnusedFiles
{
    /// <summary>
    /// Search unused source files. This will work only if a dlls is generated by main
    /// </summary>
    public class UnusedSourceFiles
    {
        /// <summary>
        /// Target model to check
        /// </summary>
        private KBModel TargetModel;

        private KBModel DesignModel;

        /// <summary>
        /// Generated files by KB objects
        /// </summary>
        private GeneratedSourceFilesCache GeneratedSourcesCache;

        /// <summary>
        /// List of main objets rsps
        /// </summary>
        List<string> UsedRsps = new List<string>();

        private void AddRspIfExists(string name)
        {
            if (File.Exists(Path.Combine(Entorno.GetTargetDirectory(TargetModel), name)))
                UsedRsps.Add(name);
        }

        private void GetUsedRsps()
        {
            // Get used RSP
            UsedRsps = new List<string>();
            List<KBObject> mainObjects = MainsGx.GetMainsByGenerator(TargetModel, GeneratorType.CSharpWin);
            foreach(KBObject o in mainObjects)
            {
                // Add umain.rsp and main.rsp
                UsedRsps.Add(RspFile.RelativeRspFilePath(o, true).ToLower());
                UsedRsps.Add(RspFile.RelativeRspFilePath(o, false).ToLower());
            }
            AddRspIfExists("genexus.programs.common.rsp");
            AddRspIfExists("reorganization.rsp");
            // Do not add developermenu.rsp here, it can contain all kb objects
        }

        public List<string> GetUnusedRsps()
        {
            // Get all rsps:
            DirectoryInfo modelDir = new DirectoryInfo(Entorno.GetTargetDirectory(TargetModel));
            IEnumerable<string> realRsps =  modelDir.GetFiles("*.rsp", SearchOption.TopDirectoryOnly).Select(x => x.Name.ToLower());

            List<string> unusedRsps = realRsps.Where(x => !UsedRsps.Contains(x)).ToList();
            // Do not report developer menu:
            unusedRsps.Remove("developermenu.rsp");
            return unusedRsps;
        }

        public HashSet<string> GetUsedSources()
        {
            IEnumerable<RspFile> rsps = UsedRsps.Select(x => new RspFile(x, TargetModel));
            HashSet<string> usedSources = new HashSet<string>();
            foreach(RspFile rsp in rsps )
                usedSources.LsiAddRange(rsp.SourceLines);

            // Add bld files
            foreach(RspFile rsp in rsps )
            {
                string bldFile = "bld" + Path.GetFileNameWithoutExtension(rsp.RspPath) + ".cs";
                usedSources.Add(bldFile.ToLower());
            }
            usedSources.Add("bldassemblies.cs");
            usedSources.Add("developermenu.cs");
            usedSources.Add("blddevelopermenu.cs");
            usedSources.Add("gxmodelinfoprovider.cs");
            usedSources.Add("gxfulltextsearchreindexer.cs");
            usedSources.Add("gxrtctls.cs");
            usedSources.Add("gxlred.cs");

            return usedSources;
        }

        public string GetSubfileObjectname(string fileName)
        {
            if (!fileName.StartsWith("sub"))
                return null;
            if (fileName.Length < 6)
                return null;
            if (Char.IsDigit(fileName[fileName.Length - 1]) && Char.IsDigit(fileName[fileName.Length - 2]))
            {
                string objectName = fileName.Substring(3, fileName.Length - 5);
                // There can be 2 or 3 digits...
                if (Char.IsDigit(objectName[objectName.Length - 1]))
                    objectName = objectName.Substring(0, objectName.Length - 1);
                return objectName;
            }
                
            else
                return null;
        }

        private bool IsBcName(string objectName)
        {
            // Remove the trailing "_bc"
            objectName = objectName.Substring(0, objectName.Length - 3);
            QualifiedName qName = new QualifiedName(objectName);
            return DesignModel.Objects.GetKey(KBaseGX.NAMESPACE_OBJECTS, qName) != null;
        }

        public bool KeepSourceFile(string fileName)
        {
            // Remove the ".cs"
            string objectName = fileName.Substring(0, fileName.Length - 3);

            bool isSubfile = false;
            if( objectName.StartsWith("type_sdt") )
            {
                // It's a SDT
                objectName = objectName.Substring(8);
                // Check composite names (type_sdtppcscornotpedsdt_documento.cs)
                int idx = objectName.IndexOf('_');
                if (idx >= 0)
                    objectName = objectName.Substring(0, idx);
            }
            else if( objectName.StartsWith("gxdomain"))
            {
                // It's a domain
                objectName = objectName.Substring(8);
            }
            else if( objectName.EndsWith("_bc") )
            {
                // Check if the "_bc" belongs to the object name
                if (IsBcName(objectName))
                    // Keep the file
                    return true;
            }
            else
            {
                // Check if it's a subfile object name
                string subfileObjectName = GetSubfileObjectname(objectName);
                if (!string.IsNullOrEmpty(subfileObjectName))
                {
                    isSubfile = true;
                    objectName = subfileObjectName;
                }                
            }

            // Check if there is some kbase object with that name:
            QualifiedName qName = new QualifiedName(objectName);
            EntityKey key = DesignModel.Objects.GetKey(KBaseGX.NAMESPACE_OBJECTS, qName);
            if (key != null)
            {
                if( isSubfile )
                {
                    // Check if the object REALLY has that subfile
                    KBObject o = TargetModel.Objects.Get(key);
                    ObjectSourceFiles sources = GeneratedSourcesCache.GetSourceFiles(o);
                    return sources.ModuleSourceFiles.Select(x => x.ToLower()).Contains(fileName);
                }

                return true;
            }

            return false;
        }

        public List<string> GetUnusedSources()
        {
            HashSet<string> usedSources = GetUsedSources();
            List<string> unusedSources = new List<string>();

            DirectoryInfo modelDir = new DirectoryInfo(Entorno.GetTargetDirectory(TargetModel));
            IEnumerable<string> realSources = modelDir.GetFiles("*.cs", SearchOption.TopDirectoryOnly).Select(x => x.Name.ToLower());
            foreach(string sourceFile in realSources)
            {
                if (!usedSources.Contains(sourceFile) && !KeepSourceFile(sourceFile))
                    unusedSources.Add(sourceFile);
            }

            return unusedSources;
        }

        public List<string> GetUnusedSourceFiles()
        {
            GetUsedRsps();

            List<string> unusedFiles = new List<string>();
            unusedFiles.AddRange(GetUnusedRsps());
            unusedFiles.AddRange(GetUnusedSources());

            unusedFiles.Sort();
            return unusedFiles;
        }

        public UnusedSourceFiles(KBModel targetModel, GeneratedSourceFilesCache generatedSourcesCache)
        {
            TargetModel = targetModel;
            DesignModel = targetModel.GetDesignModel();
            GeneratedSourcesCache = generatedSourcesCache;
        }
    }
}
