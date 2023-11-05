using Artech.Architecture.Common.Services;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using LSI.Packages.Extensiones.Utilidades.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TfLiteNetWrapper;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.PredictionGeneration
{
    /// <summary>
    /// Class for predictions with TF Lite
    /// </summary>
    class PredictorLite : PredictorBase
    {

        /// <summary>
        /// TF Lite model
        /// </summary>
        Interpreter ModelLite;

        /// <summary>
        /// Model output real names
        /// </summary>
        List<string> ModelOutputRealNames;

        /// <summary>
        /// Buffers for model inputs (label indices)
        /// </summary>
        Dictionary<string, Int32[]> InputBuffers;

		Interpreter.TensorInfo[] InputTensors;

		Interpreter.TensorInfo[] OutputTensors;

		/// <summary>
		/// Creates the TF lite predictor
		/// </summary>
		public PredictorLite(ObjectNamesCache namesCache) : base(namesCache)
        {
			using (Log log = new Log(false, false))
			{
				LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.Load();

				Interpreter.Options options = new Interpreter.Options()
				{
					LogCallback = ReportError,
					Threads = 4 // TODO: Configure number of threads ???
				};

				if (cfg.PredictionModelType == LsiExtensionsConfiguration.PredictionModelTypes.UseCustomModelTfLite)
				{
					string modelPath = Path.Combine(cfg.CustomModelPath, @"model\model.tflite");
					log.Output.AddLine("Loading Tensorflow Lite model from " + modelPath);
					if (!File.Exists(modelPath))
						throw new Exception(modelPath + " does not exists");
					ModelLite = new Interpreter(modelPath, options);
				}
				else if (cfg.PredictionModelType == LsiExtensionsConfiguration.PredictionModelTypes.UseDistributed)
				{
					log.Output.AddLine("Loading Tensorflow Lite model distributed as resource with Lsi.Extensions");
					ModelLite = new Interpreter(Resources.model, options);
				}
				else
					throw new Exception("Trying to load a TF Lite model with other configuration: " + cfg.PredictionModelType.ToString());

				ModelLite.AllocateTensors();

				// Output names for TF lite are wrong ("Identity", "Identity_1",...
				// They keep a pattern: Order is the same as the original names sorted alphabetically:
				ModelOutputRealNames = new List<string>(KbInfo.DataInfo.OutputColumns);
				ModelOutputRealNames.Sort();

				AllocateBuffers();

				// Setup names hashes
				log.Output.AddLine("Hashing KB names");
				HashedNames hashedNamesContainer = new HashedNames(KbInfo.DataInfo.GetNameHashes);
				KbInfo.ObjectNames.SetupHashedNames(hashedNamesContainer);

				Ready = true;
				log.Output.AddLine("Model ready");
			}
        }

        private void AllocateBuffers()
        {
            InputBuffers = new Dictionary<string, int[]>();
			InputTensors = new Interpreter.TensorInfo[ModelLite.GetInputTensorCount()];
			for (int i = 0; i < InputTensors.Length; i++)
			{
				InputTensors[i] = ModelLite.GetInputTensorInfo(i);
				// tensor.Dimensions.Count == 0 -> scalar, store as an array of 1 element
				int size = InputTensors[i].Dimensions.Length == 0 ? 1 : InputTensors[i].Dimensions[0];
                InputBuffers.Add(InputTensors[i].Name, new Int32[size]);
            }

			OutputTensors = new Interpreter.TensorInfo[ModelLite.GetOutputTensorCount()];
			for (int i = 0; i < OutputTensors.Length; i++)
				OutputTensors[i] = ModelLite.GetOutputTensorInfo(i);
        }

        override public PredictionResult GetPredictionToken(TokensList tokens, TokenContext context)
        {
            PredictionTime.Reset();
            PredictionTime.Start();

            PredictorInput input = new PredictorInput(KbInfo.DataInfo, tokens, context);

			// Set input values and pad if needed
			for (int tensorIdx = 0; tensorIdx < InputTensors.Length; tensorIdx++)
			{
				Interpreter.TensorInfo tensor = InputTensors[tensorIdx];
				Int32[] inputValues = input.GetColumnValues(tensor.Name).ToArray();
				Int32[] inputBuffer = InputBuffers[tensor.Name];
				Array.Copy(inputValues, inputBuffer, inputValues.Length);
				// Pad if needed
				for (int i = inputValues.Length; i < inputBuffer.Length; i++)
					inputBuffer[i] = -1;

				ModelLite.SetInputTensorData(tensorIdx, inputBuffer);
			}

			ModelLite.Invoke();

			// Get outputs
			Dictionary<string, double[]> columnProbabilities = new Dictionary<string, double[]>();
			for (int i = 0; i < OutputTensors.Length; i++)
			{
				Interpreter.TensorInfo tensor = OutputTensors[i];
				string realTensorName = ModelOutputRealNames[i];
				float[] outputValues = new float[tensor.Dimensions[0]];
				ModelLite.GetOutputTensorData(i, outputValues);

				// This lauches a invalid cast exception (.NET 3.5)...
				// columnProbabilities.Add(realTensorName, outputValues.Cast<double>().ToArray());
				double[] doubleValues = new double[outputValues.Length];
				for (int j = 0; j < outputValues.Length; j++)
					doubleValues[j] = outputValues[j];
				columnProbabilities.Add(realTensorName, doubleValues);
			}

			PredictionTime.Stop();

            return new PredictionResult(KbInfo, columnProbabilities, input, context, PredictionTime.ElapsedMilliseconds);
        }

        override public void Dispose()
        {
            try
            {
                if (ModelLite != null)
                    ModelLite.Dispose();
            }
            catch { }
            ModelLite = null;
        }

		static void ReportError(string errorMsg)
		{
			Log.SelectLog();
			CommonServices.Output.AddErrorLine("Tensorflow Lite error: " + errorMsg);
		}
    }
}
