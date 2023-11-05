using Artech.Architecture.Common.Services;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.ModelGeneration;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.PredictionGeneration
{

    /// <summary>
    /// Python predictor client for Python model server
    /// </summary>
    class Predictor : PredictorBase
    {

        /// <summary>
        /// The python predictor process
        /// </summary>
        Process PredictorProcess;

        /// <summary>
        /// Launches the predictor process
        /// </summary>
        public Predictor(ObjectNamesCache namesCache): base(namesCache)
        {

            // Get prediction configuration
            LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.Load();
            if (!Directory.Exists(cfg.PredictionPythonScriptsDir))
                throw new Exception("Python scripts directory does not exists: " + cfg.PredictionPythonScriptsDir);
            if (!Directory.Exists(cfg.PythonVirtualEnvDir))
                throw new Exception("Python virtualenv directory does not exists: " + cfg.PythonVirtualEnvDir);
            if (!Directory.Exists(cfg.CustomModelPath))
                throw new Exception("Model directory does not exists: " + cfg.CustomModelPath);

            // Create the predictions server process
            string parameters = TrainModel.GetCmdLineParameters(cfg.CustomModelPath);
            PredictorProcess = new PythonVEnv(cfg.PythonVirtualEnvDir)
                .GetProcess(Path.Combine(cfg.PredictionPythonScriptsDir, "model_server.py"), parameters);
            ProcessLogger.ConfigureRedirectOutput(PredictorProcess, false);

            // Wait until process is ready
            Thread t = new Thread(SetupPredictorProcess);
            t.Start();
        }

        /// <summary>
        /// Called when the python process writes a line on the std error
        /// </summary>
        static private void OnStdErrorWritten(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            Log.SelectLog();
            CommonServices.Output.AddErrorLine("Python prediction process error log: " + e.Data);
        }

        /// <summary>
        /// Setup prediction process. It wil be executed inside a thread
        /// </summary>
        private void SetupPredictorProcess()
        {
            try
            {
                using (Log log = new Log(false, false))
                {
                    log.StartTimeCount();

                    // Check if the model requieres hash names. 
                    // TODO: This is wrong. KbInfo.DataInfo.GetNameHashes CAN be null, but hashedNamesContainer will not be null
                    HashedNames hashedNamesContainer = new HashedNames(KbInfo.DataInfo.GetNameHashes);
                    if (hashedNamesContainer != null)
                    {
                        log.Output.AddLine("Prediction model: Hashing names...");
                        KbInfo.ObjectNames.SetupHashedNames(hashedNamesContainer);
                    }

                    log.Output.AddLine("Prediction model: Starting Python model server process...");
                    PredictorProcess.StartInfo.RedirectStandardInput = true;
                    PredictorProcess.StartInfo.RedirectStandardError = true;
                    PredictorProcess.ErrorDataReceived += OnStdErrorWritten;
                    PredictorProcess.Start();
                    PredictorProcess.BeginErrorReadLine();

                    LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.Load();                    

                    // Wait until process is ready
                    while (true)
                    {
                        string line = PredictorProcess.StandardOutput.ReadLine();
                        if (cfg.DebugPredictionModel && line != null)
                            log.Output.AddLine(line);
                        if(line == null)
                            log.Output.AddWarningLine("Prediction process output null line! Broken pipe?");
                        if (line == null || line == "READY TO SERVE")
                            break;
                    }
                    log.Output.AddLine("Prediction model ready");
                    Ready = true;
                }
            }
            catch(Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        override public PredictionResult GetPredictionToken(TokensList tokens, TokenContext context)
        {
            if (!Ready)
                return null;

            PredictionTime.Reset();
            PredictionTime.Start();

			PredictorInput input = new PredictorInput(KbInfo.DataInfo, tokens, context);
			string inputJson = input.ToJson();
            PredictorProcess.StandardInput.WriteLine(inputJson);
            string outputJson = PredictorProcess.StandardOutput.ReadLine();

            PredictionTime.Stop();

            if (outputJson == null)
                // Broken pipe?
                return null;

			PredictionResult prediction = new PredictionResult(KbInfo, outputJson, input, context, PredictionTime.ElapsedMilliseconds);
			if (!string.IsNullOrEmpty(prediction.Error))
				throw new Exception(prediction.Error);
			return prediction;
        }

        override public void Dispose()
        {
            try
            {
                if(PredictorProcess != null)
                    PredictorProcess.Kill();
                PredictorProcess = null;
            }
            catch { }
        }
    }
}
