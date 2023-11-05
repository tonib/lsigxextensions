using System;
using System.Collections.Generic;
using System.Linq;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Udm.Framework;
using Artech.Udm.Framework.References;
using LSI.Packages.Extensiones.Utilidades.UI;
using LSI.Packages.Extensiones.Utilidades.Logging;
using System.IO;
using System.Diagnostics;
using Artech.Genexus.Common.Objects;
using System.Windows.Forms;
using System.Threading;

namespace LSI.Packages.Extensiones.Utilidades.CSharpWin
{

    /// <summary>
    /// Tool to read / write Genexus Win / C# RSP compilation files
    /// </summary>
    public class RspFile
    {

        /// <summary>
        /// List with RSP file lines 
        /// </summary>
        private List<string> ContentLines;

        /// <summary>
        /// Object source file lines, normalized (lowercase and trimmed)
        /// </summary>
        public HashSet<string> SourceLines = new HashSet<string>();

        /// <summary>
        /// Full RSP file path
        /// </summary>
        public string RspPath { get; private set; }

        /// <summary>
        /// Constructor. It reads the RSP file content for the object
        /// </summary>
        /// <param name="o">Main object of the RSP file</param>
        public RspFile(KBObject o)
            : this(RelativeRspFilePath(o), o.KB.DesignModel.Environment.TargetModel)
        { }

        /// <summary>
        /// Constructor. It reads the RSP file content for the object
        /// </summary>
        /// <param name="fileName">Relative RSP file path to the target model</param>
        /// <param name="targetModel">The target model where the RSP is located</param>
        public RspFile(string fileName, KBModel targetModel)
        {
            RspPath = Entorno.GetTargetDirectoryFilePath(fileName, targetModel);

            if (!File.Exists(RspPath))
                throw new Exception("RSP file " + RspPath + " does not exist");

            ContentLines = File.ReadAllText(RspPath)
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                .ToList();

            GetSourceLines();
        }

        private void GetSourceLines()
        {
            foreach (string originalLine in ContentLines)
            {
                string line = originalLine.Trim().ToLower();
                if (!line.StartsWith("/"))
                    SourceLines.Add(line.Trim().ToLower());
            }
        }

        public bool ContainsFile(string  sourceFile)
        {
            return SourceLines.Contains(sourceFile.Trim().ToLower());
        }

        public List<string> NotContained(IEnumerable<string> sourceFiles)
        {
            List<string> notContained = new List<string>();
            foreach (string o in sourceFiles)
            {
                if (!ContainsFile(o))
                    notContained.Add(o);
            }
            return notContained;
        }

        public void AddFiles(List<string> files)
        {
            foreach (string o in files)
                AddFile(o);
        }

        public void AddFile(string file)
        {
            if (SourceLines.Add(file.Trim().ToLower()))
                ContentLines.Add(file);
        }

        public void Save()
        {
            StreamWriter writer = new StreamWriter(RspPath);
            foreach (string line in ContentLines)
                writer.WriteLine(line);
            writer.Close();
        }


        public bool RemoveFile(string fileName)
        {
            if (!ContainsFile(fileName))
                return false;

            return ContentLines.Remove(fileName);
        }

        /// <summary>
        /// Get the path of the RSP file of an object relative to the target model
        /// </summary>
        /// <param name="main">Main object</param>
        /// <param name="withInitialLetter">True if the initial main letter should be added (ex. MAIN -> aMAIN.rsp)</param>
        /// <returns>RSP file relative to the target model</returns>
        static public string RelativeRspFilePath(KBObject main, bool withInitialLetter = false)
        {
            string relativePath =
                KBaseGX.GetProgramFileName(main, MainsGx.GetMainGenerator(main), withInitialLetter, "rsp");
            if (MainsGx.IsMainWeb(main))
            {
                if (main is Procedure)
                    // Inconsistent: Web main procedures start with a
                    relativePath = "a" + relativePath;

                relativePath = Path.Combine( "web", relativePath);
            }
            return relativePath;
        }

        /// <summary>
        /// Get the absolute path of a main RSP file
        /// </summary>
        /// <param name="main">Main object</param>
        /// <returns>Absolute path of the main RSP file. It will be inside the current UI 
        /// current target model</returns>
        static public string RspFilePath(KBObject main)
        {
            return Entorno.GetTargetDirectoryFilePath(RelativeRspFilePath(main));
        }

        /// <summary>
        /// Get the RSP file path to compile Genexus.Commons.Programs.dll
        /// </summary>
        static public string GetCommonsRspPath(bool web)
        {
            string relativePath = CSharpUtils.COMMONSRSPFILENAME;
            if (web)
                relativePath = Path.Combine( "web" , relativePath );
            return Entorno.GetTargetDirectoryFilePath(relativePath);
        }

        /// <summary>
        /// Get all RSP files from the current C# win model
        /// </summary>
        static public IEnumerable<RspFile> AllWinModelRsps
        {
            get
            {
                KBModel model = UIServices.KB.WorkingEnvironment.TargetModel;
                foreach (string rspPath in Directory.GetFiles(Entorno.TargetDirectory, "*.rsp"))
                    yield return new RspFile(Path.GetFileName(rspPath), model);
            }
        }

        /// <summary>
        /// Remove a win C# source file from all model RSPs
        /// </summary>
        /// <param name="fileName">File name to remove</param>
        static public void RemoveFileFromAllRsps(string fileName)
        {
            new Thread(() =>
            {
                using (Log log = new Log())
                {
                    log.StartTimeCount();
                    int nRsps = 0, nRemoved = 0;
                    foreach (RspFile rsp in AllWinModelRsps)
                    {
                        nRsps++;
                        if (rsp.RemoveFile(fileName))
                        {
                            nRemoved++;
                            log.Output.AddLine("Entry removed from " + rsp.RspPath );
                            rsp.Save();
                        }
                    }
                    log.Output.AddLine("Entry removed from " + nRemoved + " files");
                }
            }).Start();
        }
    }
}
