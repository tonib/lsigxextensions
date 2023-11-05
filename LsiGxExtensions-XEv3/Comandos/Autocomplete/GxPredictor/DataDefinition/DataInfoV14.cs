using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Udm.Framework;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.PredictionGeneration;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using LSI.Packages.Extensiones.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition
{
    /// <summary>
    /// Model data columns definition and train info
    /// </summary>
    [DataContract]
    [KnownType(typeof(ColumnBoolean))]
    [KnownType(typeof(ColumnEnum<WordTypeKey>))]
    [KnownType(typeof(ColumnIntCategory))]
    [KnownType(typeof(ColumnStringCategory))]
    [KnownType(typeof(ColumnEnum<RuleDefinition.ParameterAccess>))]
	[KnownType(typeof(ColumnDebugString))]
	public class DataInfoV14 : DataInfo
    {
        /// <summary>
        /// Maximum hash size for extended data types
        /// </summary>
        private const int EXT_TYPE_MAXHASH = 16;

		/// <summary>
		/// Bucket limits for data type
		/// </summary>
        private static readonly int[] LengthBuckets = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 50, 100, 500, 1000 };

		/// <summary>
		/// Number of decimals limits for data type
		/// </summary>
		private static readonly int[] DecimalBuckets = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

		public DataInfoV14(int maxTexthash) : base(maxTexthash)
        {
			// We will try to learn all the language
			TrainableColumn = null;
		}
		
		/// <summary>
		/// Get a token type as model input/output
		/// </summary>
		/// <param name="token">Token info</param>
		/// <returns>Token representation as string</returns>
		private string GetTokenType(TokenInfo token)
		{

			string tokenType;
			switch(token.WordType)
			{
				case WordTypeKey.Keyword:
				case WordTypeKey.StandardMember:
				case WordTypeKey.Function:
					// Built-in language keyword
					tokenType = token.TextLowercase;
					break;

				case WordTypeKey.IntegerConstant:
					tokenType = ParseIntegerConstant(token);
					break;

				case WordTypeKey.Attribute:
				case WordTypeKey.Control:
				case WordTypeKey.DecimalConstant:
				case WordTypeKey.StringConstant:
				case WordTypeKey.OtherMember:
				case WordTypeKey.UnknownIdentifier:
				case WordTypeKey.Variable:
					// Return category as type
					tokenType = token.WordType.ToString();
					break;

				case WordTypeKey.KbObject:
					// Return KB object type
					tokenType = token.KbObjectTypeText;
					break;

				default:
					// This should not happens
					tokenType = string.Empty;
					break;
			}

			if(string.IsNullOrEmpty(tokenType))
			{
#if DEBUG
				throw new Exception("Unknown token type for token " + token.ToString());
#else
				tokenType = WordTypeKey.UnknownIdentifier.ToString();
#endif
			}

			return tokenType;
		}

		private string ParseIntegerConstant(TokenInfo token)
		{
			string keywordName = token.WordType.ToString();
			try
			{
				// Check if it's a number with "information"
				int number = int.Parse(token.Text);
				if (number <= 1)
					return keywordName + "_" + number;
				return keywordName;
			}
			catch
			{
				return keywordName;
			}
		}

		/// <summary>
		/// Convert entity key to string. Custom implementations, because if gx changes EntityKey.ToString() will break things
		/// learned by the model
		/// </summary>
		/// <param name="key">Key to convert. Cannot be null</param>
		/// <returns>String with key</returns>
		static private string EntityKeyToSring(EntityKey key)
		{
			return key.Type.ToString() + "-" + key.Id;
		}

		private string GetDataType(DataTypeInfo dataType)
		{
			if (dataType.ExtendedType != null)
			{
				// Return a hash for the extended (BC/SDT/External object/etc) type name
				uint hash = MurmurHash2.Hash(EntityKeyToSring(dataType.ExtendedType));
				return "Extended#" + (int)(hash % EXT_TYPE_MAXHASH);
			}

			return dataType.Type.ToString();
		}

		/// <summary>
		/// Create model/debug columns
		/// </summary>
		override protected void SetupColumns()
		{
			Compatibility = new CompatibilitySettings()
			{
				SetSdtsDataType = true,
				LineBreaksAsKeyword = true
			};

			// Shared labels: Data type
			var sharedDataType = new ColumnStringCategory("DataTypeShared");
			SharedLabelsDefinitions.Add(sharedDataType);
			// Length
			var sharedLength = new ColumnIntCategory("LengthShared", LengthBuckets, true);
			SharedLabelsDefinitions.Add(sharedLength);
			// Decimals
			var sharedDecimals = new ColumnIntCategory("DecimalsShared", DecimalBuckets, true);
			SharedLabelsDefinitions.Add(sharedDecimals);
			// Hashes
			var sharedHashes = new ColumnStringHashMurmur[NHashColumns];
			for (int i = 0; i < NHashColumns; i++)
			{
				sharedHashes[i] = new ColumnStringHashMurmur("HashShared" + i, MaxTextHash, true);
				SharedLabelsDefinitions.Add(sharedHashes[i]);
			}

			// Debug column. Token category (keyword, variable, etc)
			AddColumn(
				new ColumnEnum<WordTypeKey>("DbgGroup")
				{
					FeatureExtract = (token, context) => (int)token.WordType
				}
			);

			// Debug column: Raw token text
			AddColumn(
				new ColumnDebugString("DbgRawText")
				{
					FeatureExtract = (token, context) => token.Text
				}
			);

			// Debug column: Syntax editor token type
			AddColumn(
				new ColumnDebugString("DbgSyntaxEditorToken")
				{
					FeatureExtract = (token, context) => token.GxTokenType
				}
			);

			// Token type: Keyword type, kbobject type, or other types (constants, variable, attribute, etc)
			AddColumn(
				new ColumnStringCategory("Type")
				{
					FeatureExtract = (token, context) => GetTokenType(token)
				},
				isSequence: true, isOutput: true
			);

			// Data type, if token has one associated. If it's a simple type (ex. varchar, numeric, etc), it will be the 
			// type name (ex. "NUMERIC" for eDBType.NUMERIC). If it's a "extended type" (ex. SDT, BC, external object, etc), this
			// will be the hash for the extended type name. Ex.: If it's a SDT with hame "foo" this value will be "Extended#4", where 4 is the hash for
			// "foo" name.
			// If token has no associated type, it will be "NONE" (=> eDBType.NONE)
			AddColumn(
				new ColumnStringCategory("DataType", sharedLabelsColumn: sharedDataType)
				{
					FeatureExtract = (token, context) => GetDataType(token.DataType)
				},
				isSequence: true, isOutput: true
			);

			// Data type is collection? Null if token has no data type. True/false otherwise
			AddColumn(
				new ColumnBoolean("Collection", true)
				{
					FeatureExtract = (token, context) => {
						if (token.DataType.Type == eDBType.NONE)
							return null;
						return token.DataType.IsCollection;
					}
				},
				isSequence: true, isOutput: true
			);

			// Data type length. Null if token has no data type
			AddColumn(
				new ColumnIntCategory("Length", sharedLength)
				{
					FeatureExtract = (token, context) => {
						if (token.DataType.Type == eDBType.NONE)
							return null;
						return token.DataType.Length;
					}
				},
				isSequence: true, isOutput: true
			);

			// Data type decimals. Null if token has no data type
			AddColumn(
				new ColumnIntCategory("Decimals", sharedDecimals)
				{
					FeatureExtract = (token, context) => {
						if (token.DataType.Type == eDBType.NONE)
							return null;
						return token.DataType.Decimals;
					}
				},
				isSequence: true, isOutput: true
			);

			// If token has an associated name (variables, attributes and KbObjects), these are the name parts hashes. If token
			// has no name (keywords, standard members, etc), they are null
			for (int i = 0; i < NHashColumns; i++)
			{
				// IMPORTANT: C# closures require a local variable (https://stackoverflow.com/questions/271440/captured-variable-in-a-loop-in-c-sharp)
				int index = i;

				AddColumn(
					new ColumnStringHashMurmur("NameHash" + index, sharedHashes[index])
					{
						FeatureExtract = (token, context) => GetTextHashGroup(token, index)
					},
					isSequence: true, isOutput: true, isNameHash: true
				);
			}

			// If token is a variable/attribute/control name that appears in a win/web form, this is the control type name for the token in the form
			// If it does not appears in the form, it's an empty string
			AddColumn(
				new ColumnStringCategory("ControlType")
				{
					FeatureExtract = (token, context) => token.ControlNameType
				},
				isSequence: true
			);

			// Context - Parameter type. "NONE" if this token is not a call parameter
			AddColumn(
				new ColumnStringCategory("CtxParmType", sharedLabelsColumn: sharedDataType)
				{
					FeatureExtract = (token, context) => GetDataType(context.ParmType)
				},
				isContext: true
			);

			// Context - Parameter length. Null if this token is not a call parameter
			AddColumn(
				new ColumnIntCategory("CtxParmLength", sharedLength)
				{
					FeatureExtract = (token, context) =>
					{
						if (context.ParmType.Type == eDBType.NONE)
							return null;
						return context.ParmType.Length;
					}
				},
				isContext: true
			);

			// Context - Parameter decimals. Null if this token is not a call parameter
			AddColumn(
				new ColumnIntCategory("CtxParmDecimals", sharedDecimals)
				{
					FeatureExtract = (token, context) =>
					{
						if (context.ParmType.Type == eDBType.NONE)
							return null;
						return context.ParmType.Decimals;
					}
				},
				isContext: true
			);

			// Context - Parameter is collection?. Null if this token is not a call parameter
			AddColumn(
				new ColumnBoolean("CtxParmCollection", true)
				{
					FeatureExtract = (token, context) => {
						if (context.ParmType.Type == eDBType.NONE)
							return null;
						return context.ParmType.IsCollection;
					}
				},
				isContext: true
			);

			// Context - Parameter access (in, out, inout). Null if this token is not a call parameter
			AddColumn(
				new ColumnEnum<RuleDefinition.ParameterAccess>("CtxParmAccess", true)
				{

					FeatureExtract = (token, context) => {
						if (context.ParmType.Type == eDBType.NONE)
							return null;
						// Cast to int is required here, see ColumnEnum definition
						return (int)context.ParmAccess;
					}
				},
				isContext: true
			);

			// Context - Parameter name hash. Null if this token is not a call parameter
			for (int i = 0; i < NHashColumns; i++)
			{
				// IMPORTANT: C# closures require a local variable (https://stackoverflow.com/questions/271440/captured-variable-in-a-loop-in-c-sharp)
				int index = i;
				AddColumn(
					new ColumnStringHashMurmur("CtxParmNameHash" + index, sharedHashes[index])
					{
						FeatureExtract = (token, context) =>
						{
							if (context.ParmType.Type == eDBType.NONE)
								return null;
							return NameSplitter.Split(context.ParmName)[index];
						}
					},
					isContext: true
				);
			}

			// Context: Next token will be a variable? (== "&" has been typed?)
			AddColumn(
				new ColumnBoolean("CtxIsVariable", false)
				{
					FeatureExtract = (token, context) => context.IsVariable
				},
				isContext: true
			);

			// Context: Object and part type
			AddColumn(
				new ColumnStringCategory("CtxObjectType")
				{
					FeatureExtract = (token, context) => context.PartType.ObjectType.ToString()
				},
				isContext: true
			);
			AddColumn(
				new ColumnStringCategory("CtxPartType")
				{
					FeatureExtract = (token, context) => context.PartType.PartType.ToString()
				},
				isContext: true
			);
		}

        private string GetTextHashGroup(TokenInfo token, int idxGroup)
        {
            if (!token.WordType.HasName())
                return null;
            return NameSplitter.Split(token.TextNoAmpersand)[idxGroup];
        }

        override public double GetTokenProbability(TokenInfo token, PredictionResult prediction)
        {
            // Combine probabilities of all columns
            double prob = 1.0;
            foreach (string columnName in OutputColumns)
            {
                ColumnInfo column = GetColumnDefinition(columnName);
                int idx = column.GetIntValue(token, null, false);
				// If idx < 0, label was not found: Give zero probability
				prob *= idx < 0 ? 0.0 : prediction.GetColumn(columnName).Probabilities[idx];
            }

            return prob;
        }

        /// <summary>
        /// Model version
        /// </summary>
        override public int ModelVersion { get { return 14; } }

		/// <summary>
		/// Model number of hash columns
		/// </summary>
		override public int NHashColumns { get { return 3; } }

		/// <summary>
		/// Supported part types by the predicion
		/// </summary>
		override public ObjectPartType[] SupportedPartTypes
        {
            get
            {
                if (_SupportedPartTypes == null)
                {
					_SupportedPartTypes = new ObjectPartType[] {
                        ObjectPartType.WebPanelEvents , ObjectPartType.Procedure , ObjectPartType.TransactionEvents ,
                        ObjectPartType.WorkPanelEvents , ObjectPartType.SDPanelEvents ,

						ObjectPartType.TransactionRules, ObjectPartType.WorkPanelRules,
						ObjectPartType.WebPanelRules , ObjectPartType.ProcedureRules , 
						ObjectPartType.SDPanelRules,

						ObjectPartType.WorkPanelConditions , ObjectPartType.WebPanelConditions , ObjectPartType.ProcedureConditions ,
						ObjectPartType.SDPanelConditions
					};
				}
                return _SupportedPartTypes;
            }
        }

	}
}
