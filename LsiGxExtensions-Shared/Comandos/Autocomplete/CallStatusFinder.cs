using ActiproSoftware.SyntaxEditor;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Comandos.Autocomplete.Calls;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete
{

	/// <summary>
	/// Tool to search if the cursor is inside a kbobject call, and to get the current parameter info
	/// 
	/// </summary>
	[Obsolete("Use LSI.Packages.Extensiones.Comandos.Autocomplete.CallInfoFinder / CallInfo instead", false)]
	public class CallStatusFinder
	{

		CallInfo CallInfo;

		int CaretOffset;

		public CallStatusFinder(Document document, int caretOffset, ObjectNamesCache objectNames)
		{
			CaretOffset = caretOffset;

			CallInfoFinder callInfoFinder = new CallInfoFinder(document, objectNames);
			CallInfo = callInfoFinder.FindCallOverCaret(CaretOffset);
		}

		/// <summary>
		/// Info about the called object parameters. null if no call has been found, or object is not callable. 
		/// Only for KbObject calls (not for functions, etc)
		/// </summary>
		public List<ParameterElement> ObjectParameters
		{
			get
			{
				if (CallInfo == null)
					return null;
				return CallInfo.ParameterElements;
			}
		}

		/// <summary>
		/// Called kboject name info. Null if no kboject call has been found
		/// </summary>
		public KbObjectNameInfo KbObjectName
		{
			get
			{
				if (CallInfo == null)
					return null;
				return CallInfo.KbObjectName;
			}
		}

		public Parameter GetGxCurrentParameterInfo()
		{
			if (CallInfo == null)
				return null;

			int parmIdx = CallInfo.GetParameterIndex(CaretOffset);
			if (parmIdx < 0)
				return null;

			if (CallInfo.Callable == null)
				return null;

			Parameter[] parameters = CallInfo.Callable.LsiGetParametersWithTypeInfo().ToArray();
			if (parmIdx >= parameters.Length)
				return null;

			return parameters[parmIdx];
		}

		/// <summary>
		/// Info about parameter where the cursor is located. null if no call has been found, or the cursor is
		/// after the last parameter. Only for KbObject calls (not for functions, etc)
		/// </summary>
		public ParameterElement ParameterInfo
		{
			get
			{
				if (CallInfo == null)
					return null;

				return CallInfo.GetParameterElement(CaretOffset);
			}
		}

		/// <summary>
		/// Called function name. 
		/// </summary>
		public FunctionNameInfo FunctionName
		{
			get
			{
				if (CallInfo == null)
					return null;

				return CallInfo.FunctionName;
			}
		}

		/// <summary>
		/// Called object "parm" rule text. null if no call has been found
		/// </summary>
		public string CalledObjectParmRule
		{
			get
			{
				if (CallInfo == null)
					return null;

				return CallInfo.CalledObjectParmRule;
			}
		}
	}

}
