using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Artech.Architecture.UI.Framework.Services;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.ModelGeneration
{
    /// <summary>
    /// Train the model
    /// </summary>
    public class TrainModel
    {

        /// <summary>
        /// Model generation UI
        /// </summary>
        GenerateModelTW Toolwindow;

        /// <summary>
        /// Data definition
        /// </summary>
        DataInfo DataInfo;

        /// <summary>
        /// Configuration
        /// </summary>
        LsiExtensionsConfiguration Cfg;

        /// <summary>
        /// Current Python running process. It can be null
        /// </summary>
        Process CurrentPythonProcess;

        /// <summary>
        /// Parameters for Python process execution
        /// </summary>
        string Parameters;

        /// <summary>
        /// Python virtualenv info
        /// </summary>
        PythonVEnv VirtualEnv;

        public TrainModel(GenerateModelTW toolwindow, DataInfo dataInfo)
        {
            Toolwindow = toolwindow;
            DataInfo = dataInfo;
            Cfg = LsiExtensionsConfiguration.Load();
            if (!Directory.Exists(Cfg.PredictionPythonScriptsDir))
                throw new Exception("Python scripts directory does not exists: " + Cfg.PredictionPythonScriptsDir);
            if (!Directory.Exists(Cfg.PythonVirtualEnvDir))
                throw new Exception("Python virtualenv directory does not exists: " + Cfg.PythonVirtualEnvDir);
            if (!Autocomplete.NamesCache.Ready)
                throw new Exception("Autcomplete extension is not enabled or names cache is not ready");

            // Python execution
            Parameters = GetCmdLineParameters(DataInfo.DataDirectory);
            VirtualEnv = new PythonVEnv(Cfg.PythonVirtualEnvDir);
        }

        /// <summary>
        /// Get command line parameters for prediction scripts
        /// </summary>
        /// <param name="dataDirectory">Model data directory</param>
        /// <returns></returns>
        static public string GetCmdLineParameters(string dataDirectory)
        {
            return "--notfwarnings --datadir " + dataDirectory;
        }

        /// <summary>
        /// Export model
        /// </summary>
        public void ExportModel()
        {
            try
            {
                ExportInternal();
            }
            finally
            {
                CurrentPythonProcess = null;
            }
        }

        /// <summary>
        /// Export model (internal version)
        /// </summary>
        private void ExportInternal()
        {
            // Export model for model server
            CurrentPythonProcess = VirtualEnv.GetProcess(Path.Combine(Cfg.PredictionPythonScriptsDir, "export.py"),
                Parameters);
            ProcessLogger.StartAndLogProcess(CurrentPythonProcess, Toolwindow.Logger);

			// Export TF Lite model
			CurrentPythonProcess = VirtualEnv.GetModuleProcess(Cfg.PredictionPythonScriptsDir, "tflite.convert", Parameters);
			ProcessLogger.StartAndLogProcess(CurrentPythonProcess, Toolwindow.Logger);
		}

        /// <summary>
        /// Launch the evaluate model operation only
        /// </summary>
        public void EvaluateModel()
        {
            try
            {
                // Export model
                CurrentPythonProcess = VirtualEnv.GetProcess(Path.Combine(Cfg.PredictionPythonScriptsDir, "eval.py"),
                    Parameters);
                ProcessLogger.StartAndLogProcess(CurrentPythonProcess, Toolwindow.Logger);
            }
            finally
            {
                CurrentPythonProcess = null;
            }
        }

        /// <summary>
        /// Launch the evaluate model operation only
        /// </summary>
        public void DebugEvalSamples()
        {
            try
            {
                // Export model
                CurrentPythonProcess = VirtualEnv.GetProcess(Path.Combine(Cfg.PredictionPythonScriptsDir, "debug_exportedmodel.py"),
                    Parameters);
                ProcessLogger.StartAndLogProcess(CurrentPythonProcess, Toolwindow.Logger);
            }
            finally
            {
                CurrentPythonProcess = null;
            }
        }

		private void CheckModelDirExists()
		{
			if (!Directory.Exists(DataInfo.DataDirectory))
				Directory.CreateDirectory(DataInfo.DataDirectory);
		}

        public void Execute()
        {
            try
            {
				CheckModelDirExists();

				if (Toolwindow.Worker.CancellationPending)
                    return;

                if (!File.Exists(Path.Combine(DataInfo.DataDirectory, DataInfo.FILENAME)))
					ExportKbObjects();

                if (Toolwindow.Worker.CancellationPending)
                    return;

                // Train
                CurrentPythonProcess = VirtualEnv.GetProcess(Path.Combine(Cfg.PredictionPythonScriptsDir, "train.py"), 
                    Parameters);
                ProcessLogger.StartAndLogProcess(CurrentPythonProcess, Toolwindow.Logger);
                // DO NOT exit if train is cancelled: Always export the model, to get something to test
                //if (Toolwindow.Worker.CancellationPending)
                //    return;

                // Export model
                ExportInternal();
            }
            finally
            {
                CurrentPythonProcess = null;
            }
        }

		public void ExportKbObjects()
		{
			CheckModelDirExists();

			// Export kb objects
			ExportTrainObjects export = new ExportTrainObjects(Toolwindow, DataInfo, UIServices.KB.CurrentModel);
			export.Execute();
		}

        public void CancelProcess()
        {
            try
            {
                if (CurrentPythonProcess != null)
                    CurrentPythonProcess.Kill();
            }
            catch { }
        }

        /// <summary>
        /// Launch tensorboard to monitor model training
        /// </summary>
        /// <param name="modelDirectory"></param>
        static public void LaunchTensorboard(string modelDirectory)
        {
            if (!Directory.Exists(modelDirectory))
                throw new Exception(modelDirectory + " directory does not exists");

            // Start tensorboard
            PythonVEnv vEnv = new PythonVEnv(LsiExtensionsConfiguration.Load().PythonVirtualEnvDir);
            string tensorBoardPath = Path.Combine(vEnv.ScriptsPath, "tensorboard.exe");
            Process tensorBoardProcess = vEnv.GetProcess(tensorBoardPath, "--logdir " + modelDirectory);
            tensorBoardProcess.StartInfo.UseShellExecute = false;
            tensorBoardProcess.Start();

            // Display on browser
            Process.Start("http://localhost:6006");
        }
    }
}
