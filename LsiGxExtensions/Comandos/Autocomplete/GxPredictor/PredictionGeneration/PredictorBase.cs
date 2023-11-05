using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.PredictionGeneration
{
    /// <summary>
    /// Base class for predictos
    /// </summary>
    abstract class PredictorBase : IDisposable
    {
        /// <summary>
        /// Kbase info for prediction
        /// </summary>
        internal KbPredictorInfo KbInfo;

        /// <summary>
        /// Predictor has been setup and ready to make predictions?
        /// </summary>
        public bool Ready { get; protected set; }

        /// <summary>
        /// Counts prediciton execution time
        /// </summary>
        protected Stopwatch PredictionTime = new Stopwatch();

        public PredictorBase(ObjectNamesCache namesCache)
        {
            // Read model columns information
			DataInfo dataInfo = DataInfo.DeserializeForPrediction();
			if (dataInfo.ModelType != DataInfo.MODEL_TYPE_GPT &&
				LsiExtensionsConfiguration.Load().PredictionModelType == LsiExtensionsConfiguration.PredictionModelTypes.UseCustomModelTfLite)
				throw new Exception("RNN Models are unsupported by the Tensorflow Lite dll distributed with Lsi.Extensions. You can " +
					"run the model with the Pyton model server, specifying it in Lsi.Extensions configuration");
			KbInfo = new KbPredictorInfo(namesCache, dataInfo);
        }

        public TokensList GetSequenceFromCursor(AutocompleteContext context)
        {
            CodeTokenizer tokenizer = new CodeTokenizer(KbInfo, context.Part);
            return tokenizer.TokenizeCursorSequenceBackwards(context);
        }

        abstract public PredictionResult GetPredictionToken(TokensList tokens, TokenContext context);

        abstract public void Dispose();
    }
}
