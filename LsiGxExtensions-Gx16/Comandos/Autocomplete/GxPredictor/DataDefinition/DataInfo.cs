using Artech.Genexus.Common;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.PredictionGeneration;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition
{

	/// <summary>
	/// Model and data definition
	/// </summary>
	[DataContract]
	public abstract class DataInfo
	{

		/// <summary>
		/// Settings to configure compatibility between models
		/// </summary>
		public class CompatibilitySettings
		{
			/// <summary>
			/// Try to assing data types to SDT members
			/// </summary>
			public bool SetSdtsDataType;

			/// <summary>
			/// Include line breaks as language keywords?
			/// </summary>
			public bool LineBreaksAsKeyword;
		}

		/// <summary>
		/// File name where model definitions are stored
		/// </summary>
		public const string FILENAME = "data_info.json";

		/// <summary>
		/// GRU cell type
		/// </summary>
		public const string CELL_GRU = "gru";

		/// <summary>
		/// LSTM cell type
		/// </summary>
		public const string CELL_LSTM = "lstm";

		/// <summary>
		/// RNN model, with current only for token to predict
		/// </summary>
		public const string MODEL_TYPE_RNN = "rnn";

		/// <summary>
		/// RNN model, with context for each timestep, plus context for token to predict
		/// </summary>
		public const string MODEL_TYPE_EXP = "exp";

		/// <summary>
		/// GPT model
		/// </summary>
		public const string MODEL_TYPE_GPT = "gpt";

		private const string VERSION_FILE = "model_version.txt";

		/// <summary>
		/// Model type
		/// </summary>
		[DataMember]
		public string ModelType = MODEL_TYPE_GPT;

		/// <summary>
		/// Maximum value for each text hash
		/// </summary>
		[DataMember]
		public int MaxTextHash = 32;

		/// <summary>
		/// Number of procedures to export for training. 0 = all
		/// </summary>
		[DataMember]
		public int NObjectsToExport = 0;

		/// <summary>
		/// Path to the data train directory
		/// </summary>
		public string DataDirectory;

		/// <summary>
		/// Number of tokens to feed to the predictor
		/// </summary>
		[DataMember]
		public int SequenceLength = 96;

		/// <summary>
		/// Maximum number of epochs to train. 0 = unlimited
		/// </summary>
		[DataMember]
		public int MaxEpochs = 32;

		/// <summary>
		/// List of shared labels definitions
		/// </summary>
		[DataMember]
		internal List<ColumnInfo> SharedLabelsDefinitions { get; set; }

		/// <summary>
		/// Column definitions used by the model. This don't contain CSV debug columns
		/// </summary>
		[DataMember]
		protected List<ColumnInfo> ColumnDefinitions { get; set; }

		/// <summary>
		/// Input sequence column names
		/// </summary>
		[DataMember]
		public List<string> SequenceColumns { get; set; }

		/// <summary>
		/// Input context column names
		/// </summary>
		[DataMember]
		public List<string> ContextColumns { get; set; }

		/// <summary>
		/// Debug column names. They are references (keys) to ColumnDefinitions
		/// </summary>
		[DataMember]
		protected List<string> DebugColumns { get; set; }

		/// <summary>
		/// Output column names
		/// </summary>
		[DataMember]
		public List<string> OutputColumns { get; set; }

		/// <summary>
		/// % of objects to use for evaluation
		/// </summary>
		[DataMember]
		public decimal PercentageEvaluation = 10.0M;

		/// <summary>
		/// Number of RNN layer elements
		/// </summary>
		[DataMember]
		public int NNetworkElements = 256;

		/// <summary>
		/// RNN cell type
		/// </summary>
		[DataMember]
		public string CellType = CELL_GRU;

		/// <summary>
		/// RNN output drop out: Fraction of outputs that will be dropped
		/// </summary>
		[DataMember]
		public decimal Dropout = 0.1M;

		/// <summary>
		/// Batch size
		/// </summary>
		[DataMember]
		public int BatchSize = 64;

		/// <summary>
		/// Limit of batches number for each epoch. 0 = no limit
		/// </summary>
		[DataMember]
		public int MaxBatchesPerEpoch = 34290;

		/// <summary>
		/// Print a message each X batches
		/// </summary>
		[DataMember]
		public int LogEachBatches = 1000;

		/// <summary>
		/// Only for GPT model. Embedding layer dropout
		/// </summary>
		[DataMember]
		public decimal GptEmbeddingDropout = 0.1M;

		/// <summary>
		/// Only for GPT model. Residual layer dropout
		/// </summary>
		[DataMember]
		public decimal GptResidualDropout = 0.1M;

		/// <summary>
		/// Only for GPT model. Attention layer dropout
		/// </summary>
		[DataMember]
		public decimal GptAttentionDropout = 0.1M;

		/// <summary>
		/// Only for GPT model. Number of model encoding layers
		/// </summary>
		[DataMember]
		public int GptNLayers = 3;

		/// <summary>
		/// Only for GPT model. Number of attention heads. GptEmbeddingSize must to be divisble by GptEmbeddingSize
		/// </summary>
		[DataMember]
		public int GptNHeads = 4;

		/// <summary>
		/// Only for GPT model. Token embedding size. GptEmbeddingSize must to be divisble by GptEmbeddingSize
		/// </summary>
		[DataMember]
		public int GptEmbeddingSize = 256;

		/// <summary>
		/// Activation function for GPT layers
		/// </summary>
		[DataMember]
		public string GptActivationFunction = "relu_square";

		/// <summary>
		/// Number of CSV files to feed concurrently to train model
		/// </summary>
		[DataMember]
		public int CsvCycleLength = 32;

		/// <summary>
		/// Number of parallel calls to get sequences for training
		/// </summary>
		[DataMember]
		public int CsvParallelCalls = 8;

		/// <summary>
		/// Train shuffle buffer size (number of sequences)
		/// </summary>
		[DataMember]
		public int ShuffleBufferSize = 4096;

		/// <summary>
		/// Column definitions indexed by name
		/// </summary>
		private Dictionary<string, ColumnInfo> ColumnsByName;

		/// <summary>
		/// Stores feature extract functions for each column. Needed to restore functions after JSON deserialization.
		/// After object construction / deserialization, this member will be null
		/// </summary>
		protected Dictionary<string, object> ExtractFeatureFunctions = new Dictionary<string, object>();

		protected const string COLUMN_TRAINABLE = "trainable";

		/// <summary>
		/// Column name for indicator of trainable token. If null, all language will be we trained, including language elements
		/// not included in autocomplete function (ex. "(", "=", etc). If not null, only words that can be purposed by autocomplete
		/// function will be trainable (keyword, variable/attribute names, functions, etc).
		/// I'm not really sure about this. Both options are supported, but I guess GPT should require train all the language
		/// </summary>
		[DataMember]
		public string TrainableColumn = COLUMN_TRAINABLE;

		/// <summary>
		/// Column names to hash token names. Empty if model don't support hashing names
		/// </summary>
		public List<string> TextHashColumnNames = new List<string>();

		/// <summary>
		/// Class to split Genexus names
		/// </summary>
		protected GxNameSplitter NameSplitter;

		public DataInfo(int maxTexthash)
		{
			MaxTextHash = maxTexthash;

			// TODO: Move these assignments to member definitions
			SharedLabelsDefinitions = new List<ColumnInfo>();
			ColumnDefinitions = new List<ColumnInfo>();
			SequenceColumns = new List<string>();
			ContextColumns = new List<string>();
			DebugColumns = new List<string>();
			OutputColumns = new List<string>();

			SetupColumns();
			FinalSetup();
		}

		private void FinalSetup()
		{
			BuildColumnsByName();

			// Rebind shared labels for string categories
			foreach(ColumnStringCategory c in ColumnDefinitions.OfType<ColumnStringCategory>().Where(c => c.SharedLabelsId != null))
				c.BindLabels((ColumnStringCategory)SharedLabelsDefinitions.Where(sharedColumn => sharedColumn.Name == c.SharedLabelsId).First());

			// Restore extract features functions. Needed after deserialization
			foreach (string colName in ExtractFeatureFunctions.Keys)
			{
				ColumnInfo columnDefinition = GetColumnDefinition(colName);
				// It can be null for debug columns
				if (columnDefinition != null)
					columnDefinition.FeatureExtractFunction = ExtractFeatureFunctions[colName];
			}
			// No longer needed
			ExtractFeatureFunctions = null;

			NameSplitter = new GxNameSplitter(NHashColumns);
		}

		/// <summary>
		/// Called before deserialize this instance
		/// </summary>
		/// <param name="context"></param>
		[OnDeserializing]
		private void OnDeserializing(StreamingContext context)
		{
			// When deserializing all members are null. We need this:
			ExtractFeatureFunctions = new Dictionary<string, object>();
			TextHashColumnNames = new List<string>(NHashColumns);
			SharedLabelsDefinitions = new List<ColumnInfo>();

			// This is called just to generate extract feature functions, it will not store columns
			SetupColumns();
		}

		/// <summary>
		/// Called after deserialize this instance
		/// </summary>
		/// <param name="context"></param>
		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			FinalSetup();
		}

		/// <summary>
		/// Create model/debug columns
		/// </summary>
		abstract protected void SetupColumns();

		/// <summary>
		/// Add a model/debug column definition
		/// </summary>
		/// <typeparam name="T">Column type, inherited from ColumnInfo</typeparam>
		/// <param name="columnInfo">Column definition</param>
		/// <param name="isOutput">True if it's a model output</param>
		/// <param name="isSequence">True if it's a sequence model input</param>
		/// <param name="isContext">True if it's a context model input</param>
		/// <param name="addDebugColumn">True if original column value should be added to the CSV files for debugging</param>
		/// <param name="isNameHash">True if it's a column to hash a name part</param>
		/// <returns>The columnInfo parameter</returns>
		protected T AddColumn<T>(T columnInfo, bool isOutput = false, bool isSequence = false, bool isContext = false, bool addDebugColumn = true,
			bool isNameHash = false) where T : ColumnInfo
		{
			// When deserializing all members, except ExtractFeatureFunctions, are null. We are interesed only in save ExtractFeatureFunctions
			// to restore them after deserialization
			if (ColumnDefinitions != null)
			{
				if (isSequence && isContext)
					throw new Exception("isSequence and isContext cannot be true both");

				bool isModelColumn = isOutput || isSequence || isContext || columnInfo.Name == COLUMN_TRAINABLE;
				bool isDebugColumn = !isModelColumn && addDebugColumn;

				if (isModelColumn || isDebugColumn)
					ColumnDefinitions.Add(columnInfo);
				else
					throw new Exception("isOutput, isSequence, isOutput and/or addDebugColumn should be true");

				if (isSequence)
					SequenceColumns.Add(columnInfo.Name);
				else if (isContext)
					ContextColumns.Add(columnInfo.Name);

				if (isOutput)
					OutputColumns.Add(columnInfo.Name);

				if (addDebugColumn)
					DebugColumns.Add(columnInfo.Name);
			}

			ExtractFeatureFunctions.Add(columnInfo.Name, columnInfo.FeatureExtractFunction);

			if (isNameHash)
				TextHashColumnNames.Add(columnInfo.Name);

			return columnInfo;
		}

		public void SerializeToDirectory()
		{
			// Write version file
			string filePath = Path.Combine(DataDirectory, VERSION_FILE);
			File.WriteAllText(filePath, ModelVersion.ToString());

			// Write DataInfo
			filePath = Path.Combine(DataDirectory, FILENAME);
			using (FileStream writer = new FileStream(filePath, FileMode.Create))
			{
				DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(this.GetType());
				jsonSerializer.WriteObject(writer, this);
			}
		}

		static private DataInfo DeserializeFromStream(Stream stream, Type dataInfoVersion, string directoryPath = null)
		{
			DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(dataInfoVersion);
			DataInfo dataInfo = jsonSerializer.ReadObject(stream) as DataInfo;
			if (dataInfo != null)
				dataInfo.DataDirectory = directoryPath;
			return dataInfo;
		}

		static private DataInfo DeserializeFromFile(string directoryPath, Type dataInfoVersion)
		{
			// If there are exceptions on this function, check this: https://stackoverflow.com/questions/3971271/json-deseralization-to-abstract-list-using-datacontractjsonserializer
			// There cannot be spaces between "{" and "___type:..." (BUG)
			string filePath = Path.Combine(directoryPath, FILENAME);
			using (FileStream reader = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				return DeserializeFromStream(reader, dataInfoVersion, directoryPath);
			}
		}

		/// <summary>
		/// Deserializes model info from a directory
		/// </summary>
		/// <param name="directoryPath">Directory from where to deserialize</param>
		/// <returns>Model information</returns>
		static public DataInfo DeserializeFromDirectory(string directoryPath)
		{
			// Read version file
			string filePath = Path.Combine(directoryPath, VERSION_FILE);
			int version = 1;
			if (File.Exists(filePath))
				version = int.Parse(File.ReadAllText(filePath));
			else
				throw new Exception("Error reading prediction model: Model version file not found: " + filePath);

			Type dataInfoVersion;
			if (version == 12)
				dataInfoVersion = typeof(DataInfoV12);
			else if (version == 13)
				dataInfoVersion = typeof(DataInfoV13);
			else if (version == 14)
				dataInfoVersion = typeof(DataInfoV14);
			else
				throw new Exception("Unknown or unsupported model version: " + version);

			return DeserializeFromFile(directoryPath, dataInfoVersion);
		}

		/// <summary>
		/// Deserializes model info from directory, or from dll resources, as specified in extensions settings
		/// </summary>
		/// <returns>Model information</returns>
		static public DataInfo DeserializeForPrediction()
		{
			LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.Load();
			if (cfg.PredictionModelType == LsiExtensionsConfiguration.PredictionModelTypes.UseDistributed)
			{
				// Load from resources
				using (MemoryStream stream = new MemoryStream(Resources.data_info))
				{
					return DeserializeFromStream(stream, typeof(DataInfoV13));
				}
			}
			else if (cfg.PredictionModelType == LsiExtensionsConfiguration.PredictionModelTypes.UseCustomModelTfLite ||
				cfg.PredictionModelType == LsiExtensionsConfiguration.PredictionModelTypes.UseCustomFullTf)
			{
				return DeserializeFromDirectory(cfg.CustomModelPath);
			}
			else
				throw new Exception("Tried to load model when configuration specifies do not use any");
		}

		/// <summary>
		/// Builds the columns by name dictionary
		/// </summary>
		private void BuildColumnsByName()
		{
			if (ColumnDefinitions == null)
				// Before deserialization, this will be null: Do nothing. Before deserializadion BuildColumnsByName() should be called again
				return;

			ColumnsByName = new Dictionary<string, ColumnInfo>();
			foreach (ColumnInfo c in ColumnDefinitions)
				ColumnsByName.Add(c.Name, c);
		}

		/// <summary>
		/// Get a column definition
		/// </summary>
		/// <param name="name">Column name</param>
		/// <returns>The column definition. null if it was not found</returns>
		public ColumnInfo GetColumnDefinition(string name)
		{
			ColumnInfo columnInfo;
			ColumnsByName.TryGetValue(name, out columnInfo);
			return columnInfo;
		}

		/// <summary>
		/// CSV file titles row
		/// </summary>
		public string CsvTitlesRow {
			get
			{
				string result = string.Join(";", ColumnDefinitions.Select(x => x.Name).ToArray());
				if (DebugColumns.Count > 0)
				{
					result += ";" + string.Join(";",
						DebugColumns.Select(x => "dbg_" + GetColumnDefinition(x).Name).ToArray());
				}
				return result;
			}
		}

		public string ToCsvRow(TokenInfo token, bool addNewValues)
		{
			string row = string.Join(";",
				ColumnDefinitions.Select(x => x.GetIntValue(token, token.Context, addNewValues).ToString()).ToArray());
			// TODO: Remove any ";" from debug value string (it can break CSV format)?
			if (DebugColumns.Count > 0)
				row += ";" + string.Join(";",
					DebugColumns.Select(x => GetColumnDefinition(x).GetCsvDebugValue(token, token.Context)).ToArray());
			return row;
		}

		/// <summary>
		/// Get token name hashes
		/// </summary>
		/// <param name="token">Token to get name hashes</param>
		/// <param name="context">Token context</param>
		/// <returns>Name hashes. Null if token has no name or model don't support name hashing</returns>
		public int[] GetNameHashes(TokenInfo token, TokenContext context)
		{
			if (!token.WordType.HasName() || TextHashColumnNames.Count == 0)
				return null;

			int[] hashes = new int[TextHashColumnNames.Count];
			for (int i = 0; i < TextHashColumnNames.Count; i++)
				hashes[i] = (GetColumnDefinition(TextHashColumnNames[i]) as ColumnStringHashMurmur).GetIntValue(token, context, false);
			return hashes;
		}

		/// <summary>
		/// Get name hashes
		/// </summary>
		/// <param name="name">Name to get name hashes</param>
		/// <returns>Name hashes. Null if model don't support name hashing</returns>
		public int[] GetNameHashes(string name)
		{
			if (TextHashColumnNames.Count == 0)
				return null;

			// All hash columns will give the same hash for same string, so get only the first hash column
			ColumnStringHashMurmur hashColumn = GetColumnDefinition(TextHashColumnNames[0]) as ColumnStringHashMurmur;

			int[] hashes = new int[NHashColumns];
			List<string> groupNames = NameSplitter.Split(name);
			for (int i = 0; i < NHashColumns; i++)
				hashes[i] = hashColumn.GetHash(groupNames[i]);
			return hashes;
		}

		/// <summary>
		/// Supported part types by the predicion
		/// </summary>
		abstract public ObjectPartType[] SupportedPartTypes { get; }
		protected ObjectPartType[] _SupportedPartTypes;

		abstract public double GetTokenProbability(TokenInfo token, PredictionResult prediction);

		/// <summary>
		/// Model version
		/// </summary>
		abstract public int ModelVersion { get; }

		/// <summary>
		/// True if context should be feeded for each timestep, and token to predict. False if context should be feeded
		/// only for token to predict
		/// </summary>
		public bool FeedContextAllTimesteps { get { return ModelType != MODEL_TYPE_RNN; } }

		/// <summary>
		/// Model number of hash columns
		/// </summary>
		abstract public int NHashColumns { get; }

		/// <summary>
		/// Model compatibility settings
		/// </summary>
		public CompatibilitySettings Compatibility;
	}
}
