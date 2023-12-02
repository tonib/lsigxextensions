using ActiproSoftware.SyntaxEditor;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor
{

    /// <summary>
    /// Token context: Info about the place where the token is located: Part type, expected parameter type,
    /// object has printblocks, etc
    /// </summary>
    public class TokenContext
    {

		// Crap, see SetPartType():
		private static readonly ObjectPartType TrainedSdEventsPart = new ObjectPartType(ObjClass.SDPanel, Artech.Genexus.Common.PartType.Events);
		private static readonly ObjectPartType TrainedSdRulesPart = new ObjectPartType(ObjClass.SDPanel, Artech.Genexus.Common.PartType.Rules);
		private static readonly ObjectPartType TrainedSdConditionsPart = new ObjectPartType(ObjClass.SDPanel, Artech.Genexus.Common.PartType.Conditions);

		/// <summary>
		/// Expected parameter type
		/// </summary>
		public DataTypeInfo ParmType;

        /// <summary>
        /// Expected parameter name, original case, no initial ampersand. Empty string if we are not on a call
        /// </summary>
        public string ParmName;

        /// <summary>
        /// Expected parameter access type (in, out, inout)
        /// </summary>
        public RuleDefinition.ParameterAccess ParmAccess;

        /// <summary>
        /// Code part type
        /// </summary>
        public ObjectPartType PartType;

        /// <summary>
        /// Next token will be a variable?. True if the current token starts with ampersand
        /// </summary>
        public bool IsVariable;

        /// <summary>
        /// Empty context
        /// </summary>
        public TokenContext()
        {
            ParmType = DataTypeInfo.NoType;
            ParmName = string.Empty;
            ParmAccess = RuleDefinition.ParameterAccess.PARM_INOUT;
			SetPartType(ObjectPartType.Empty);
        }

        /// <summary>
        /// Create a token context from the current autocomplete (editing) context
        /// </summary>
        /// <param name="context">Current autocomplete context</param>
        public TokenContext(AutocompleteContext context)
        {
            SetCurrentParameterInfo(context.CallFinder.ParameterInfo);
			SetPartType(context.ObjectPartType);
            IsVariable = context.LineParser.CurrentTokenPrefix.StartsWith("&");
        }

		public TokenContext(ParameterElement parm, ObjectPartType partType, WordTypeKey wordType)
		{
			SetCurrentParameterInfo(parm);
			SetPartType(partType);
			IsVariable = (wordType == WordTypeKey.Variable);
		}

        /// <summary>
        /// Create a token from a code document position. For training
        /// </summary>
        /// <param name="document">Current code document</param>
        /// <param name="tokenStartOffset">Token start position</param>
        /// <param name="namesCache">Object names cache</param>
        /// <param name="partType">Code part type</param>
        /// <param name="wordType">The word type</param>
        public TokenContext(Document document, int tokenStartOffset, ObjectNamesCache namesCache,
            ObjectPartType partType, WordTypeKey wordType)
        {
            CallStatusFinder callFinder = new CallStatusFinder(document, tokenStartOffset, namesCache);
            SetCurrentParameterInfo(callFinder.ParameterInfo);
			SetPartType(partType);
            IsVariable = (wordType == WordTypeKey.Variable);
        }

		private void SetPartType(ObjectPartType partType)
		{
			// See ExportTrainObjects.ExportObjectPart(): SDPanels have been trained with part type as standard part. SDPanels have
			// different part types (non standard, ¯\_(ツ)_/¯)
			if (partType.Equals(ObjectPartType.SDPanelEvents))
				PartType = TrainedSdEventsPart;
			else if (partType.Equals(ObjectPartType.SDPanelRules))
				PartType = TrainedSdRulesPart;
			else if (partType.Equals(ObjectPartType.SDPanelConditions))
				PartType = TrainedSdConditionsPart;
			else
				PartType = partType;
		}

		private void SetCurrentParameterInfo(ParameterElement parm)
		{
			if (parm != null)
			{
				ParmType = parm.DataType;
				ParmName = parm.Name;
				if (ParmName.StartsWith("&"))
					ParmName = ParmName.Substring(1);
				ParmAccess = parm.Accessor;
			}
			else
			{
				ParmType = DataTypeInfo.NoType;
				ParmName = string.Empty;
				ParmAccess = RuleDefinition.ParameterAccess.PARM_INOUT;
			}
		}

		public override string ToString()
		{
			string txt;
			if (ParmType != DataTypeInfo.NoType)
				txt = "ParmType: " + ParmType + ", ParmAccess: " + ParmAccess;
			else
				txt = string.Empty;

			if(IsVariable)
			{
				if (!string.IsNullOrEmpty(txt))
					txt += ", ";
				txt += "IsVariable: True";
			}
			return txt;
		}
	}
}
