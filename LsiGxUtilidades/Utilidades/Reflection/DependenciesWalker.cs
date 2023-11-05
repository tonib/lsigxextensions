using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Windows.Forms;

namespace LSI.Packages.Extensiones.Utilidades.Reflection
{
    [Serializable]
    public class DependenciesWalker
    {

        /// <summary>
        /// Directory path where to search the dlls
        /// </summary>
        private string DirPath;

        /// <summary>
        /// Set of dlls of which to search dependencies
        /// </summary>
        private List<string> DllsToCheck;

        /// <summary>
        /// Set of dlls already checked
        /// </summary>
        private List<string> DllsAlreadyChecked = new List<string>();

        /// <summary>
        /// Dll dependencies found
        /// </summary>
        private List<string> DllsDependencies = new List<string>();

        /// <summary>
        /// Get dll dependencies for a given dll
        /// </summary>
        /// <param name="dllName">The dll name to check, lowercase</param>
        private void TraverseDllDependencies(string dllName)
        {
            // Check if the dll has been already checked
            if (DllsAlreadyChecked.Contains(dllName))
                return;
            DllsAlreadyChecked.Add(dllName);

            // Check the dll exists on the given directory
            string dllPath = Path.Combine(DirPath, dllName);
            if (!File.Exists(dllPath))
                return;

            // Do not add dlls to check as dependencies
            if( !DllsToCheck.Contains(dllName) )
                DllsDependencies.Add(dllName);

            // Traverse subdependents
            try
            {
                Assembly dependency = Assembly.ReflectionOnlyLoadFrom(dllPath);
                foreach (AssemblyName subdendencyName in dependency.GetReferencedAssemblies())
                {
                    string dependentDll = subdendencyName.Name.ToLower() + ".dll";
                        TraverseDllDependencies(dependentDll);
                }
            }
            catch
            {
                // Ignore errors loading dlls
            }

        }

        private DependenciesWalker(List<string> dllsToCheck, string dirPath)
        {
            DllsToCheck = dllsToCheck;
            DirPath = dirPath;
        }

        private List<string> Execute()
        {
            foreach (string dllName in DllsToCheck)
                TraverseDllDependencies(dllName.ToLower());

            return DllsDependencies;
        }

        static private void TraverseDllDependencies()
        {
            List<string> dllsToCheck = AppDomain.CurrentDomain.GetData("dlls")
                as List<string>;
            string dllsPath = AppDomain.CurrentDomain.GetData("dllsPath") as string;

            DependenciesWalker walker = new DependenciesWalker(dllsToCheck, dllsPath);
            AppDomain.CurrentDomain.SetData("dlls", walker.Execute());
        }

        static public List<string> Execute(List<string> dllsToCheck, string dllsPath)
        {
            // Get the extensions dir
            // TODO: These should be a Entorno properties
            string genexusDir = Path.GetDirectoryName(Application.ExecutablePath);
            string extensionsPath = Path.Combine(genexusDir, "packages");

            // Set up with the extensions dir as current
            AppDomainSetup info = new AppDomainSetup();
            info.ApplicationBase = extensionsPath;
            Evidence evidence = AppDomain.CurrentDomain.Evidence;
            AppDomain tempDomain = AppDomain.CreateDomain("TemporaryAppDomain", evidence, info);

            // Run the shit
            tempDomain.SetData("dlls", dllsToCheck);
            tempDomain.SetData("dllsPath", dllsPath);
            tempDomain.DoCallBack(new CrossAppDomainDelegate(TraverseDllDependencies));

            // Get new dlls
            List<string> dependencies = tempDomain.GetData("dlls") as List<string>;

            // Unload loaded dlls
            AppDomain.Unload(tempDomain);

            return dependencies;
        }

    }
}
