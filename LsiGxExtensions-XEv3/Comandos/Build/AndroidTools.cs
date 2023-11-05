using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Entities;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos.Build
{
    /// <summary>
    /// Android UI tools 
    /// </summary>
    public class AndroidTools
    {

        private const string APIS_DIR = @"mobile\Android\FlexibleClient\src\com\artech\android\api";

        /// <summary>
        /// The SD generator
        /// </summary>
        private GxEnvironment Generator;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generator">The SD generator</param>
        public AndroidTools(GxEnvironment generator)
        {
            Generator = generator;
        }

        /// <summary>
        /// Edit an object source file for Android
        /// </summary>
        /// <param name="o">The object to open</param>
        public void OpenJavaSource( KBObject o)
        {
            // Get the source file
            if (!(o is ExternalObject))
            {
                MessageBox.Show("Only external objects source files are supported for SD generator");
                return;
            }

            // TODO: Read the Android package / class names to get the real path ???
            string fileName = o.Name + ".java";
            string relativePath = Path.Combine(APIS_DIR,fileName);
            EditSourceFile(relativePath);
        }

        private void EditSourceFile(string relativePath)
        {
            string path = Entorno.GetTargetDirectoryFilePath(relativePath, Generator.Model);
            if (!File.Exists(path))
            {
                MessageBox.Show(path + " does no exists");
                return;
            }
            LsiExtensionsConfiguration.Load().OpenFileWithTextEditor(path);
        }

        /// <summary>
        /// Get the Android SDK directory
        /// </summary>
        public string SdkDirectory
        {
            get {
                DirectoryType dir = Generator.Properties.GetPropertyValue<DirectoryType>(Properties.SMARTDEVICE.AndroidSdkDirectory);
                if (dir == null || string.IsNullOrEmpty(dir.Location))
                    throw new Exception("The Android SDK directory was not specified on the SD generator");
                return dir.Location; 
            }
        }

        private void RunOnAndroidDir(string relativeExePath) 
        {
            try 
            {
                Process.Start(Path.Combine(SdkDirectory, relativeExePath));
            }
            catch(Exception ex) {
                Log.ShowException(ex);
            }
        }

        public void OpenApisFolder()
        {
            try
            {
                Process.Start(Entorno.GetTargetDirectoryFilePath(APIS_DIR));
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        public void EditUserExternalApiFactory()
        {
            EditSourceFile(@"mobile\Android\FlexibleClient\src\com\artech\externalapi\UserExternalApiFactory.java");
        }

    }
}
