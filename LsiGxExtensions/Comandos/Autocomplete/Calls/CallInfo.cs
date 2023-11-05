using ActiproSoftware.SyntaxEditor;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.CallsAnalisys;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.Calls
{
	/// <summary>
	/// Information about a call, based on the text editor tokens
	/// </summary>
	public class CallInfo
	{

		private Document Document;

		private ObjectNamesCache ObjectNames;

		/// <summary>
		/// Token for called token. It can be a function name or an object name (TODO: or a member name?, ex. IndexOf?)
		/// </summary>
		private IToken CalledToken;

		/// <summary>
		/// Offsets where call parameters begins and ends
		/// </summary>
		public int OffsetStart, OffsetEnd;

		/// <summary>
		/// Parameters delimiters -> [ "(" , "," , "," ... , ")" ]
		/// </summary>
		private List<IToken> ParametersSeparators;

		/// <summary>
		/// True if _KbObjectName / _Callable / _FunctionName have been resolved
		/// </summary>
		private bool CalledObjectResolved;

		KbObjectNameInfo _KbObjectName;

		FunctionNameInfo _FunctionName;

		ICallableInfo _Callable;

		private bool ParameterElementsResolved;

		bool CalledObjectParmRuleResolved;

		private string _CalledObjectParmRule;

		/// <summary>
		/// Info about the called object parameters
		/// Only for KbObject calls (not for functions, etc)
		/// </summary>
		private List<ParameterElement> _ParameterElements;

		public CallInfo(IToken calledToken, List<IToken> parametersSeparators, Document document, ObjectNamesCache objectNames)
		{
			CalledToken = calledToken;
			Document = document;
			ObjectNames = objectNames;

			ParametersSeparators = new List<IToken>(parametersSeparators);
			OffsetStart = ParametersSeparators.Count > 0 ? ParametersSeparators[0].EndOffset : -1;
			OffsetEnd = ParametersSeparators.Count > 1 ? ParametersSeparators[ParametersSeparators.Count - 1].StartOffset : document.Length;
		}

		public int GetParameterIndex(int offset)
		{
			if (ParametersSeparators.Count == 0)
				return -1;

			if (ParametersSeparators.Count == 1)
				// Could happens if close parenthesis has not been typed
				return offset >= ParametersSeparators[0].EndOffset ? 0 : -1;

			for (int i=0; i< ParametersSeparators.Count - 1; i++)
			{
				if (offset >= ParametersSeparators[i].EndOffset && offset <= ParametersSeparators[i + 1].StartOffset)
					return i;
			}
			return -1;
		}

		private void ResolveCalledObject()
		{
			if (CalledObjectResolved)
				return;

			CalledObjectResolved = true;

			string name;

			// TODO: Confirm this. In ALL supported object/parts is used this token?
			if (CalledToken.Key == TokenKeysEx.FcnWordToken || CalledToken.Key == TokenKeysEx.FcnNpWordToken)
			{
				// Called object is a function
				name = Document.GetTokenText(CalledToken).ToLower();

				_FunctionName = ObjectNames.GetAllByExactName(name)
					.OfType<FunctionNameInfo>()
					.FirstOrDefault();

				return;
			}

			name = Document.GetTokenText(CalledToken).ToLower();

			_KbObjectName = ObjectNames.GetAllByExactName(name)
				.OfType<KbObjectNameInfo>()
				.Where(x => ObjClassLsi.LsiIsCallableType(x.ObjectKey.Type))
				.FirstOrDefault();

			if (_KbObjectName == null)
				return;

			KBObject o = UIServices.KB.CurrentModel.Objects.Get(_KbObjectName.ObjectKey);
			_Callable = o as ICallableInfo;
		}

		public ICallableInfo Callable
		{
			get
			{
				ResolveCalledObject();
				return _Callable;
			}
		}

		public KbObjectNameInfo KbObjectName
		{
			get
			{
				ResolveCalledObject();
				return _KbObjectName;
			}
		}

		public FunctionNameInfo FunctionName
		{
			get
			{
				ResolveCalledObject();
				return _FunctionName;
			}
		}

		public List<ParameterElement> ParameterElements
		{
			get
			{
				if (!ParameterElementsResolved)
				{
					ParameterElementsResolved = true;

					ICallableInfo callable = Callable;
					if (callable != null)
						// Parse parm() rule
						_ParameterElements = ReglaParm.ObtenerParametros(callable);
				}
				return _ParameterElements;
			}
		}

		/// <summary>
		/// Called object "parm" rule text. null if no call has been found
		/// </summary>
		public string CalledObjectParmRule
		{
			get
			{
				if(!CalledObjectParmRuleResolved)
				{
					CalledObjectParmRuleResolved = true;
					ICallableInfo callable = Callable;
					if (callable == null)
						return null;
					_CalledObjectParmRule = KbSignaturesCache.GetObjectParmRule(callable);
				}
				return _CalledObjectParmRule;
			}
		}

		public ParameterElement GetParameterElement(int offset)
		{
			int idx = GetParameterIndex(offset);
			if (idx < 0)
				return null;

			List<ParameterElement> parameterElements = ParameterElements;
			if (parameterElements == null || idx >= parameterElements.Count)
				return null;

			return parameterElements[idx];
		}

		public override string ToString()
		{
			return Document.GetTokenText(CalledToken) + " / " + ParametersSeparators.Count + " separators";
		}
	}
}
