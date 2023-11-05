using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.ModelGeneration
{
    class PythonVEnv
    {

        string VEnvPath;

        public PythonVEnv(string vEnvPath)
        {
            VEnvPath = vEnvPath;
        }

        private string PythonPath
        {
            get { return Path.Combine(VEnvPath, @"Scripts\python.exe"); }
        }

		public Process GetModuleProcess(string workingDirectory, string moduleName, string parameters = null)
		{
			string parms = "-u -m " + moduleName;
			if (parameters != null)
				parms += " " + parameters;
			return GetProcessWithParms(parms, workingDirectory);
		}

        public Process GetProcess(string scriptPath, string parameters = null)
        {
			string parms = "-u " + scriptPath;
			if (parameters != null)
				parms += " " + parameters;
			return GetProcessWithParms(parms, Path.GetDirectoryName(scriptPath));
        }

		private Process GetProcessWithParms(string parameters, string workingDirectory)
		{
			Process p = new Process();
			p.StartInfo.FileName = PythonPath;
			// -u is for unbuffered std input / output
			p.StartInfo.Arguments = parameters;
			p.StartInfo.EnvironmentVariables["VIRTUAL_ENV"] = VEnvPath;
			p.StartInfo.WorkingDirectory = workingDirectory;
			return p;
		}

        /// <summary>
        /// Virtual environment scripts path
        /// </summary>
        public string ScriptsPath
        {
            get { return Path.Combine(VEnvPath, "Scripts"); }
        }

    }
}
