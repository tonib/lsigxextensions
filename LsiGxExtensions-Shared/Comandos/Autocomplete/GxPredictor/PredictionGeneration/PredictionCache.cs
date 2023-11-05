using Artech.Architecture.Common.Services;
using Artech.Genexus.Common.Objects;
using Artech.Udm.Framework;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.PredictionGeneration
{
    /// <summary>
    /// Prediction cache for "runtime". It stores the last predicted keyword
    /// </summary>
    public class PredictionCache : IDisposable
    {
        /// <summary>
        /// Predictions generator
        /// </summary>
        internal PredictorBase Predictor;

        /// <summary>
        /// Procedure id for last prediction
        /// </summary>
        private EntityKey LastObjectKey;

        /// <summary>
        /// Part type id for last prediction
        /// </summary>
        private Guid LastPartTypeId;

        /// <summary>
        /// Word start offset for the last prediction
        /// </summary>
        private int LastWordOffset = -1;

        /// <summary>
        /// Last prediction. It can be null
        /// </summary>
        public PredictionResult LastPrediction;

        /// <summary>
        /// Last prediction time
        /// </summary>
        public DateTime LastPredictionTimestamp;

        /// <summary>
        /// Prediction execution time
        /// </summary>
        Stopwatch PredictionTime = new Stopwatch();

        public PredictionCache(ObjectNamesCache objectNames)
        {
			if (LsiExtensionsConfiguration.Load().PredictionModelType == LsiExtensionsConfiguration.PredictionModelTypes.UseCustomFullTf)
				Predictor = new Predictor(objectNames);
			else
				Predictor = new PredictorLite(objectNames);
        }

        /// <summary>
        /// Calculate prediction for the next token on the current editing object part
        /// </summary>
        /// <param name="context">Current autocomplete context</param>
        /// <param name="forceVariablePrediction">True if must force that next token will be a variable.
        /// Can be needed if the user has not typen the ampersand yet</param>
        /// <returns>Next token prediction. null if it cannot be calculated</returns>
        public PredictionResult GetPredictionFromCurrentPart(AutocompleteContext context, 
            bool forceVariablePrediction = false)
        {

            // The current token context
            TokenContext tokenContext = new TokenContext(context);
            if (forceVariablePrediction)
                tokenContext.IsVariable = false;

            // Check if the prediction is cached
            if (context.Object.Key == LastObjectKey && context.Part.Type == LastPartTypeId && context.LineParser.CurrentTokenOffset == LastWordOffset)
            {
                bool keepLastPrediction = true;
                
                if (LastWordOffset == -1)
                    // LastWordOffset == -1 means no word has been started yet
                    keepLastPrediction = false;
                else if (LastPrediction != null && LastPrediction.Context.IsVariable != tokenContext.IsVariable)
                    // Context has changed
                    keepLastPrediction = false;

                if(keepLastPrediction)
                    return LastPrediction;
            }

            // Check if the part is supported
            if (!Predictor.KbInfo.DataInfo.SupportedPartTypes.Any( x => x.Equals(context.ObjectPartType) ))
                return null;

            LastObjectKey = context.Object.Key;
            LastPartTypeId = context.Part.Type;
            LastWordOffset = context.LineParser.CurrentTokenOffset;

            PredictionTime.Reset();
            PredictionTime.Start();
            
            TokensList tokens = Predictor.GetSequenceFromCursor(context);
            LastPrediction = Predictor.GetPredictionToken(tokens, tokenContext);

            PredictionTime.Stop();
            LastPredictionTimestamp = DateTime.Now;

            if(LastPrediction != null)
                LastPrediction.EntireExecutionMiliseconds = PredictionTime.ElapsedMilliseconds;

            return LastPrediction;
        }

        public void Dispose()
        {
            try
            {
                if (Predictor != null)
                    Predictor.Dispose();
                Predictor = null;
            }
            catch { }
        }

    }
}
