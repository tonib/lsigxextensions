using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Entities;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LSI.Packages.Extensiones.Utilidades.CSharpWin.UnusedFiles
{
    /// <summary>
    /// Searches unused specification files on the model
    /// </summary>
    public class UnusedSpecificationFilesFinder : UnusedFilesFinderBase
    {

        /// <summary>
        /// Target model to check
        /// </summary>
        private KBModel TargetModel;

        /// <summary>
        /// Desing model
        /// </summary>
        private KBModel DesignModel;

        /// <summary>
        /// Base path, relative to Kb path. It can be null
        /// </summary>
        private string _BaseDirectory;

        /// <summary>
        /// The base directory where the searches are done
        /// </summary>
        override public DirectoryInfo BaseDirectory {
            get
            {
                string dirPath = TargetModel.KB.Location;

                if (_BaseDirectory == null)
                    return new DirectoryInfo(dirPath);

                dirPath = Path.Combine(dirPath, _BaseDirectory);
                return new DirectoryInfo(dirPath);
            }
        }

        /// <summary>
        /// Base path, relative to Kb path
        /// </summary>
        override public string BaseDirectoryRelativeToKb
        {
            get { return _BaseDirectory == null ? string.Empty : _BaseDirectory; }
        }

        public UnusedSpecificationFilesFinder(KBModel targetModel)
        {
            TargetModel = targetModel;
            DesignModel = targetModel.GetDesignModel();

            // Calculate the base directory, relative to the kbase root
            string baseDir = Path.Combine(TargetModel.KB.Location, ModelSpecificationsDirName);
            // Get each C# win generator specifications dir
            GxModel gxModel = TargetModel.GetAs<GxModel>();
            foreach (GxEnvironment generator in gxModel.Environments.Where(x => x.Generator == (int)GeneratorType.CSharpWin))
            {
                string generatorDirName = "GEN" + generator.Generator;
                if (Directory.Exists(Path.Combine(baseDir, generatorDirName)))
                {
                    _BaseDirectory = Path.Combine(ModelSpecificationsDirName, generatorDirName);
                    break;
                }
            }
            
        }

        private string ModelSpecificationsDirName
        {
            get { return "GXSPC" + TargetModel.Id.ToString().PadLeft(3, '0'); }
        }

        private bool IsFileUsed(FileInfo file)
        {
            string objectName = file.Name.Substring(0, file.Name.Length - 4);

            // Special name for sdts:
            if (objectName.StartsWith("GXSDT_"))
                objectName = objectName.Substring(6);

            // TODO: Check if the file ends with "_BC"

            // Max length for file names is 16. So, if it has this lenght we cannot test if the object exists...
            // TODO: Check this is really true
            if (objectName.Length == 16)
                return true;

            QualifiedName qName = new QualifiedName(objectName);
            return DesignModel.Objects.GetKey(KBaseGX.NAMESPACE_OBJECTS, qName) != null;
        }

        override public void ExecuteUISearch()
        {
            if ( _BaseDirectory== null)
                return;

            // Get main names, without extension:
            HashSet<string> mainNames = MainsGx.GetMainsByGenerator(TargetModel, GeneratorType.CSharpWin)
                .Select( x => CSharpUtils.GetWinMainExeFileName(x) )
                .Select( x => Path.GetFileNameWithoutExtension(x).ToLower() )
                .LsiToHashSet();
            
            // Get each C# win generator specifications dir
            DirectoryInfo baseDir = BaseDirectory;
            foreach (FileInfo file in baseDir.GetFiles("*.sp0", SearchOption.TopDirectoryOnly))
            {
                IncreaseSearchedObjects();

                // Ignore mains names
                string fileName = Path.GetFileNameWithoutExtension(file.Name).ToLower();
                if (mainNames.Contains(fileName))
                    continue;

                if (!IsFileUsed(file))
                    PublishFile(baseDir.FullName, file.Name);
            }

            // TODO: Search on NVG subdirectory

        }

    }
}
