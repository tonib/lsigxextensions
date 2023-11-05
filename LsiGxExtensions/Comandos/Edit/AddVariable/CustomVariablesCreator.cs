using ActiproSoftware.SyntaxEditor;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Framework.Commands;
using Artech.FrameworkDE.Text;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Types;
using LSI.Packages.Extensiones.Comandos.Autocomplete;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Comandos.Edit.AddVariable
{
    class CustomVariablesCreator
    {
        VariablesPart Variables;

        public CustomVariablesCreator(VariablesPart variables)
        {
            this.Variables = variables;
        }

        public Variable CreateFromName(string varName)
        {
            if (LsiExtensionsConfiguration.PrivateExtensionsInstalled && varName.EndsWith("Lis") && varName.Length > 3)
            {
                // At LSI collections are declared with a "lis" postfix
                Variable v = CreateFromPartialName(varName.Substring(0, varName.Length - 3), true);
                if(v != null)
                {
                    v = v.LsiCloneRenamed(varName);
                    v.IsCollection = true;
                    return v;
                }
            }

            return CreateFromPartialName(varName, false);
        }

        private Variable CreateFromPartialName(string varName, bool createFromGenexus)
        {
            Variable v;

            // Check if the variable is declared with the same name on other open object
            Variable vOtherObject = GetVariableFromOtherObjects(varName);
            if (vOtherObject != null)
            {
                v = new Variable(varName, Variables);
                v.CopyPropertiesFrom(vOtherObject);
                return v;
            }

            v = CheckCustomLsiVariables(varName);
            if (v != null)
                return v;

            if (createFromGenexus)
            {
                // Check if Genexus has a default data type for this variable name
                v = new Variable(varName, Variables);
                DataType.SetDefault(v);
                if (!v.LsiHasDefaultType())
                    return v;
            }

            return null;
        }

        private Variable CheckCustomLsiVariables(string varName)
        {
            if (!LsiExtensionsConfiguration.PrivateExtensionsInstalled)
                // Only for LSI
                return null;

            string lowerName = varName.ToLower();
            if (lowerName == "flgerr" || lowerName == "flgok" || lowerName == "confirmado")
            {
                Variable v = new Variable(varName, Variables);
                v.Type = eDBType.Boolean;
                return v;
            }

            if (lowerName == "modo")
            {
                Variable v = new Variable(varName, Variables);
                v.Type = eDBType.CHARACTER;
                v.Length = 3;
                return v;
            }

            if (lowerName == "msgerr")
            {
                Variable v = new Variable(varName, Variables);
                v.Type = eDBType.VARCHAR;
                v.Length = 200;
                return v;
            }

            // Indices variables
            if (lowerName.Length == 1 && lowerName.CompareTo("i") >= 0 && lowerName.CompareTo("k") <= 0)
            {
                Variable v = new Variable(varName, Variables);
                v.Length = 8;
                return v;
            }

            return null;
        }

        private static Variable GetVariableFromOtherObjects(string varName)
        {
            try
            {
                // Check if there is an open object with the same variable name declared
                foreach (KBObject openObject in UIServices.DocumentManager.OpenedDocuments())
                {
                    VariablesPart variables = openObject.Parts.LsiGet<VariablesPart>();
                    if (variables == null)
                        continue;
                    Variable v = variables.GetVariable(varName);
                    if (v != null)
                        return v;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// If editor caret is on a object call parameter, it will create the variable copying the data type from the parameter type
        /// </summary>
        /// <param name="syntaxEditor">Current editor</param>
        /// <param name="variableName">Variable name to create</param>
        /// <returns>The created variable. Null if no variable was created</returns>
        public Variable CreateFromCurrentCallParameter(SyntaxEditor syntaxEditor, string variableName)
        {
            // CallStatusFinder requires the objects name cache. If it's not available, do nothing
            if (!Autocomplete.Autocomplete.NamesCache.Ready)
                return null;

            // Find info about the call parameters for cursor position
            CallStatusFinder callFinder = new CallStatusFinder(syntaxEditor.Document, syntaxEditor.Caret.Offset, Autocomplete.Autocomplete.NamesCache);
            if (callFinder.KbObjectName == null || callFinder.ParameterInfo == null)
                return null;

            // callFinder.ParameterInfo has no full info about the parameter (it has not the variable/attribute definition)
            Parameter parmInfo = callFinder.GetGxCurrentParameterInfo();
            if (parmInfo == null || parmInfo.Object == null)
                return null;

            // Create and return variable based on parameter object
            Variable v = new Variable(variableName, Variables);
            if (parmInfo.IsAttribute)
            {
                Artech.Genexus.Common.Objects.Attribute att = parmInfo.Object as Artech.Genexus.Common.Objects.Attribute;
                if (att == null)
                    return null;
                v.AttributeBasedOn = att;
            }
            else
            {
                Variable parmVariable = parmInfo.Object as Variable;
                if (parmVariable == null)
                    return null;
                v.CopyPropertiesFrom(parmVariable);
            }
            return v;
        }
    }
}
