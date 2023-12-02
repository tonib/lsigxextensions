using Artech.Architecture.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace LSI.Packages.Extensiones.Comandos.Build.Production
{
    /// <summary>
    /// Build production definition
    /// </summary>
    public class PrepareProduction : BuildProcess
    {

        /// <summary>
        /// Production configuration storage filename
        /// </summary>
        private const string PRODUCTIONFILENAME = "production.xml";

        /// <summary>
        /// List of file patterns ("*.ini" as example) to not include on the production
        /// </summary>
        public List<string> FilePattensToIgnore = new List<string>();

        /// <summary>
        /// Production directory
        /// </summary>
        [XmlIgnore]
        public string TargetDirectory { get; protected set; }

        /// <summary>
        /// Current KB target model
        /// </summary>
        [XmlIgnore]
        internal KBModel TargetModel;

        /// <summary>
        /// Bin directory for windows generated applications 
        /// </summary>
        [XmlIgnore]
        public string WinBinPath 
        {
            get { return Path.Combine(TargetDirectory, "bin"); }
        }

        /// <summary>
        /// Copy images.txt file to bin folder?
        /// </summary>
        public bool CopyImagesTxt = false;

        /// <summary>
        /// String with FilePattensToIgnore values. It's the pattern list, separated with semicolons
        /// </summary>
        [XmlIgnore]
        public string PatternsToIgnoreText
        {
            get
            {
                return string.Join(";", FilePattensToIgnore.ToArray());
            }
            set
            {
                FilePattensToIgnore.Clear();
                string[] patterns = value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string pattern in patterns)
                {
                    string p = pattern.Trim();
                    if (!string.IsNullOrEmpty(p))
                        FilePattensToIgnore.Add(p);
                }
            }
        }

        /// <summary>
        /// All production tasks definition. 
        /// </summary>
        public List<ProductionTask> Tasks = new List<ProductionTask>();

        /// <summary>
        /// If this is not null, only these tasks will be executed. If it's null, all tasks
        /// will be executed
        /// </summary>
        [XmlIgnore]
        public IEnumerable<ProductionTask> TasksToRun;

        /// <summary>
        /// Real tasks to run. It checks TasksToRun member, enabled tasks, and tasks only for current environment
        /// </summary>
        IEnumerable<ProductionTask> RealTasksToRun
        {
            get
            {
                var tasks = TasksToRun;
                if (tasks == null)
                    // Run all tasks
                    tasks = Tasks;

                return tasks
                    .Where(t => t.Enabled)
                    .Where(t => string.IsNullOrEmpty(t.OnlyForEnvironmentName) || t.OnlyForEnvironmentName == TargetModel.Name);
            }
        }

        /// <summary>
        /// False (this class does not use the build functions of genexus)
        /// </summary>
        override public bool IsInternalGxBuild { get { return false; } }

        /// <summary>
        /// Destination path of the last copy production task. It will be an empty string if 
        /// no enabled copy task has been found
        /// </summary>
        /// <see cref="CopyProduction"/>
        [XmlIgnore]
        public string LastCopyPath { get; private set; }

        /// <summary>
        /// Only for XML derialization
        /// </summary>
        private PrepareProduction() { }

        /// <summary>
        /// Only for XML derialization
        /// </summary>
        private void Setup(KBModel targetModel)
        {
            TargetModel = targetModel;
            TargetDirectory = Entorno.GetTargetDirectory(targetModel);
            LastCopyPath = string.Empty;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PrepareProduction(KBModel targetModel)
        {
            Setup(targetModel);
        }

        static private PrepareProduction CreateDefaultProduction(KBModel targetModel) 
        {
            PrepareProduction production = new PrepareProduction(targetModel);
            production.FilePattensToIgnore.Add("*.ini");
            production.FilePattensToIgnore.Add("*" + Entorno.BACKUPPOSTFIX + "*.dll");
            production.FilePattensToIgnore.Add("*-old*.dll");
            production.FilePattensToIgnore.Add("*.cs");
            production.FilePattensToIgnore.Add("*.pdb");
            production.FilePattensToIgnore.Add("*.rsp");
            production.FilePattensToIgnore.Add("thumbs.db");
            production.FilePattensToIgnore.Add("*.zip.tmp");
            return production;
        }

        /// <summary>
        /// Load the current kb production configuration. If it does not exists, it will return
        /// a default production
        /// </summary>
        /// <returns>The kb production, or default production if it was not found</returns>
        static public PrepareProduction LoadKbProduction(KBModel targetModel)
        {
            // Dont handle exceptions: If the file cannot be open, launch an exception
            //try
            //{
            string path = Entorno.GetLsiExtensionsFilePath(targetModel.KB, PRODUCTIONFILENAME);
            if (!File.Exists(path))
                return CreateDefaultProduction(targetModel);

            XmlSerializer serializer = new XmlSerializer(typeof(PrepareProduction));
            TextReader reader = new StreamReader(path);
            PrepareProduction info = (PrepareProduction)serializer.Deserialize(reader);
            reader.Close();
            info.Setup(targetModel);
            return info;
            //}
            //catch(Exception ex)
            //{
            //    Log.MostrarExcepcion(ex);
            //    return CreateDefaultProduction();
            //}
        }

        /// <summary>
        /// Save this as the current kb production configuration
        /// </summary>
        public void SaveToFile(KnowledgeBase kb)
        {
            string path = Entorno.GetLsiExtensionsFilePath(kb, PRODUCTIONFILENAME);

            XmlSerializer serializer = new XmlSerializer(typeof(PrepareProduction));
            TextWriter writer = new StreamWriter(path);
            serializer.Serialize(writer, this);
            writer.Close();
        }

        private void UpdateImagesTxt() 
        {
            // This is only for c# win models:
            if (!Directory.Exists(WinBinPath))
                return;

            string imagesTxtPath = Path.Combine(TargetDirectory, "images.txt");
            if (!File.Exists(imagesTxtPath))
                return;

            LogLine("Copying images.txt to bin");
            File.Copy(imagesTxtPath, Path.Combine(WinBinPath, "images.txt"), true);
        }

        /// <summary>
        /// Execute the production build
        /// </summary>
        public override void Execute()
        {

            if( CopyImagesTxt )
                UpdateImagesTxt();

            // Run enabled tasks
            foreach (ProductionTask task in RealTasksToRun)
            {
                try
                {
                    task.Execute(this);

                    // Margin between logs:
                    LogLine("");
                }
                catch (Exception ex)
                {
                    LogErrorLine("Error: " + ex.Message);
                    Log.ShowException(ex);
                }

                // Store the last copy destination:
                CopyProduction copyTask = task as CopyProduction;
                if (copyTask != null)
                    LastCopyPath = copyTask.CopyDestination;

                if (this.BuildCancelled)
                    return;
            }

        }

        /// <summary>
        /// The list of existing files / directories that will be replaced by a set of tasks
        /// </summary>
        /// <param name="tasks">The set of tasks to run. null to check all production tasks</param>
        public List<string> FilesToReplaceByTasks()
        {
            IEnumerable<string> result = RealTasksToRun.SelectMany(task => task.ReplacedFiles);

            // Get only existing files/directories:
            return result
                .Distinct()
                .Where(r => File.Exists(r) || Directory.Exists(r))
                .OrderBy(r => r)
                .ToList();
        }

        public override string ToString()
        {
            string txt = "Prepare production";
            var tasks = RealTasksToRun;
            if (tasks.Count() == 1)
                txt += " (" + tasks.First() + ")";
            else
                txt += " (" + tasks.Count() + " tasks)";
            return txt;
        }

    }
}
