using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Udm.Framework.References;
using Artech.Udm.Framework;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common;
using Artech.Architecture.Common.Converters;
using Artech.Genexus.Common.Entities;
using LSI.Packages.Extensiones.Utilidades.CSharpWin;
using LSI.Packages.Extensiones.Utilidades.WinForms;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common.Types;
using System.IO;
using Artech.Patterns.WorkWithDevices.Objects;
using Artech.Packages.Patterns.Objects;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using Artech.Patterns.WorkWithDevices;

namespace LSI.Packages.Extensiones.Utilidades.CSharpWin
{
    /// <summary>
    /// Information about generated files by an object
    /// </summary>
    public class ObjectSourceFiles
    {
        /// <summary>
        /// Object's last update. Used to detect if we need to refresh the files list.
        /// </summary>
        private int VersionId;

        /// <summary>
        /// Generated source file names that should be included into the module RSP. They dont
        /// include the module path.
        /// See the function GetModuleSourceFiles()
        /// </summary>
        public List<string> ModuleSourceFiles;

        /// <summary>
        /// Generated source file names that should be included into the commons RSP. They dont
        /// include the module path.
        /// See the function GetCommonsSourceFiles()
        /// </summary>
        public List<string> CommonsSourceFiles;

        /// <summary>
        /// Object is bussiness component?
        /// </summary>
        public bool IsBC;

        /// <summary>
        /// Object is main?
        /// </summary>
        public bool IsMain;

        /// <summary>
        /// Only applies if IsMain is true. It's the main file that calls the main
        /// </summary>
        public string MainEntryFile;

        /// <summary>
        /// Guid of the module owner of this object
        /// </summary>
        public Guid ModuleGuid;

        public ObjectSourceFiles() { }

        public ObjectSourceFiles(KBObject o) 
        {
            VersionId = o.VersionId;
            RecalculateSourceFiles(o);
        }

        /// <summary>
        /// Function used by the serializer to decide if CommonsSourceFiles should be serialized
        /// </summary>
        /// <returns>True if CommonsSourceFiles should be serialized</returns>
        //public bool ShouldSerializeCommonsSourceFiles()
        //{
        //    return CommonsSourceFiles != null && CommonsSourceFiles.Count > 0;
        //}

        /// <summary>
        /// Function used by the serializer to decide if IsMain should be serialized
        /// </summary>
        /// <returns>True if IsMain should be serialized</returns>
        //public bool ShouldSerializeIsMain()
        //{
        //    return IsMain;
        //}

        /// <summary>
        /// Function used by the serializer to decide if IsBC should be serialized
        /// </summary>
        /// <returns>True if IsBC should be serialized</returns>
        //public bool ShouldSerializeIsBC()
        //{
        //    return IsBC;
        //}
        
        /// <summary>
        /// Get the object program file name
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="includeExtension">True if the name should include the extension ".cs"</param>
        /// <returns>The program filename, without directory path</returns>
        private string GetProgramFileName(KBObject o, bool includeExtension)
        {
            string name;
            if (o is SDPanel)
                name = o.GetSignificantName().ToLower() + "_level_detail";
            else if (IsMain)
                name = KBaseGX.GetProgramFileName(o, MainsGx.GetMainGenerator(o), true, null);
            else
                name = o.GetSignificantName().ToLower();
            if (includeExtension)
                name += ".cs";
            return name;
        }

        /// <summary>
        /// The main source file for this object. null, if it cannot be determined
        /// </summary>
        /// <param name="o">Object owner of those source files</param>
        public string GetMainSourceFile(KBObject o)
        {
            string fileName;
            if (o is SDT || o is ExternalObject)
            {
                if (CommonsSourceFiles.Count == 0)
                    return null;
                fileName = CommonsSourceFiles[0];
            }
            else
                fileName = GetProgramFileName(o, true);
            return GetModulePath(fileName, o.Module);
        }

        public void RecalculateIfNeeded(KBObject o)
        {
            if (o.VersionId != VersionId)
                RecalculateSourceFiles(o);
        }

        private string GetModulePath(string fileName, Module module)
        {
            if (module == null || module.IsRoot)
                return fileName;

            // Modules seems not to cut the names by the significant object name length, so dont
            // use the GetSignificantName():
            string directory = module.QualifiedName.ToString()
                .Replace('.', Path.DirectorySeparatorChar);
            return Path.Combine(directory, fileName);
        }

        private string GetModulePath(string fileName, Dictionary<Guid, Module> modulesTable)
        {
            // Get the module
            Module module;
            if (ModuleGuid == null)
                return fileName;
            if (!modulesTable.TryGetValue(ModuleGuid, out module))
                return fileName;

            return GetModulePath(fileName, module);
        }

        /// <summary>
        /// Get the object source files to include into the module.
        /// </summary>
        /// <param name="modulesTable">Table kbase modules, indexed by their Guid</param>
        /// <param name="ignoreIsMain">Only applies to main objects. If it's false and object is 
        /// main, it will return the entry source for the main. If it's true, it will return the
        /// object own source files</param>
        /// <returns>The object source files</returns>
        public List<string> GetModuleSourceFiles(Dictionary<Guid, Module> modulesTable, 
            bool ignoreIsMain)
        {
            List<string> result;
            // Get file names
            if (!ignoreIsMain && IsMain)
            {
                result = new List<string>();
                // MainEntryFile can be null if cache format has changed
                if( MainEntryFile != null )
                    result.Add(MainEntryFile);
            }
            else
                result = ModuleSourceFiles;

            // Add module directories to the source file path:
            return result.Select(x => GetModulePath(x, modulesTable)).ToList();
        }

        /// <summary>
        /// Get the object source files to include into the Genexus.Commons
        /// </summary>
        /// <param name="modulesTable">Table kbase modules, indexed by their Guid</param>
        /// <returns>The object source files</returns>
        public List<string> GetCommonsSourceFiles(Dictionary<Guid, Module> modulesTable)
        {
            // Add module directories to the source file path:
            return CommonsSourceFiles.Select(x => GetModulePath(x, modulesTable)).ToList();
        }

        private string GetWinformGridFilename(KBObject o, KeyValuePair<GridElement, int> pair)
        {
            string number = pair.Value.ToString();
            if (number.Length < 2)
                number = number.PadLeft(2, '0');

            return "sub" + GetProgramFileName(o,false) + number + ".cs";
        }

        static private string SdtFileName(string fullName)
        {
            return "type_Sdt" + fullName.Replace('.', '_') + ".cs";
        }

        private void RecalculateSourcesSdt(SDT sdt)
        {
            foreach (StructuredTypeInfo t in sdt.GetStructuredTypes(true))
            {
                if (t.Name == sdt.Name && sdt.SDTStructure.Root.IsCollection)
                    // Special case?
                    continue;

                CommonsSourceFiles.Add(SdtFileName(t.Name));
            }
        }

        private void RecalculateSourceFiles(KBObject o)
        {
            ModuleSourceFiles = new List<string>();
            CommonsSourceFiles = new List<string>();

            VersionId = o.VersionId;
            IsMain = MainsGx.IsMain(o);
            ModuleGuid = o.Module.Guid;

            if (o == null)
                return;

            SDT sdt = o as SDT;
            if (sdt != null)
            {
                // Special case:
                RecalculateSourcesSdt(sdt);
                return;
            }

            if (WinFormGx.IsWinform(o))
            {
                // Add grids
                WinFormGx wf = new WinFormGx(o);
                Dictionary<GridElement, int> gridsPosition = wf.GetGridPositions();
                foreach (KeyValuePair<GridElement, int> pair in gridsPosition)
                    ModuleSourceFiles.Add(GetWinformGridFilename(o, pair));
            }

            if(o is SDPanel sd)
			{
                // Special case. Add grids
                var grids = sd.PatternPart.PanelElement.LsiEnumerateDescendants().Where(element => element.Type == InstanceElements.LayoutGrid);
                foreach (PatternInstanceElement element in grids)
                {
                    string controlName = element.Attributes.GetPropertyValue("ControlName") as string ?? "";
                    ModuleSourceFiles.Add($"{sd.Name}_level_detail_{controlName}.cs");
                }
                return;
            }

            if (o is Transaction && KBaseGX.EsBussinessComponent(o))
            {
                IsBC = true;

                // Add BC file:
                ModuleSourceFiles.Add(o.GetSignificantName().ToLower() + "_bc.cs");
                // Add SDT file:
                // TODO: If the transaction is multilevel, check if more than one file is generated...
                CommonsSourceFiles.Add(SdtFileName(o.GetSignificantName()));
            }

            // TODO: Domains have its own file too... This should be check here

            // Add own object
            if (o is ExternalObject)
                // Special case
                // TODO: If the data structure is multilevel, check if more than one file is generated...
                CommonsSourceFiles.Add(SdtFileName(o.GetSignificantName()));
            else
            {
                string programName = o.GetSignificantName().ToLower() + ".cs";
                if (IsMain)
                {
                    // Add uprogram.cs
                    ModuleSourceFiles.Add(KBaseGX.GetProgramFileName(o, MainsGx.GetMainGenerator(o), true, "cs"));
                    // Store the main entry point
                    MainEntryFile = programName;
                }
                else
                    // Add program.cs
                    ModuleSourceFiles.Add(programName);
            }
        }

        /// <summary>
        /// Get list of all source files for object
        /// </summary>
        /// <param name="o">Object to get source files</param>
        /// <returns>Source files, absolute paths. Main source file will be the first element</returns>
        public List<string> GetAllSourceFiles(KBObject o, bool isCSharpWebGenerator)
		{
            var sourceFiles = ModuleSourceFiles.Select(f => GetModulePath(f, o.Module))
                .Union(CommonsSourceFiles.Select(f => GetModulePath(f, o.Module)))
                .ToList();
            if (MainEntryFile != null)
                sourceFiles.Add(GetModulePath(MainEntryFile, o.Module));

            string mainSourceFile = this.GetMainSourceFile(o);
            if (mainSourceFile != null)
            {
                sourceFiles.Remove(mainSourceFile);
                sourceFiles.Insert(0, mainSourceFile);
            }

            // Calculate real paths
            if (isCSharpWebGenerator)
                sourceFiles = sourceFiles.Select(path => Path.Combine("web", path)).ToList();
            sourceFiles = sourceFiles.Select(path => Entorno.GetTargetDirectoryFilePath(path)).ToList();

            if(o is SDPanel)
			{
                // Ugly hack: Some (not all) sdpanels have an extra file sdsvc_xxx_Level_Detail
                // I thik is only generated for panels with dinamic comboboxes and so
                // Return only if exists
                string sdsvcPath = $"sdsvc_{o.GetSignificantName().ToLower()}_level_detail.cs";
                sdsvcPath = GetModulePath(sdsvcPath, o.Module);
                if (isCSharpWebGenerator)
                    sdsvcPath = Path.Combine("web", sdsvcPath);
                sdsvcPath = Entorno.GetTargetDirectoryFilePath(sdsvcPath);
                if (File.Exists(sdsvcPath))
                    sourceFiles.Add(sdsvcPath);
            }
            return sourceFiles;
        }
    }
}
