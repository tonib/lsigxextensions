using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Language.Parser.Data;
using Artech.Architecture.UI.Framework.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Framework.Commands;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Logging;
using Artech.Genexus.Common.Parts.WebForm;
using Artech.Genexus.Common.CustomTypes;
using Artech.Common.Diagnostics;
using Artech.Patterns.WorkWithDevices.Parts;

namespace LSI.Packages.Extensiones.Comandos.Refactoring
{

    /// <summary>
    /// Tool to extract code to a DataSelector
    /// </summary>
    class ExtractDataSelector
    {

        /// <summary>
        /// Part where to extract code
        /// </summary>
        private KBObjectPart PartToRefactor;

        /// <summary>
        /// Code to extract
        /// </summary>
        private string CodeToExtract;

        static private string GetSelectedText(CommandData commandData, out KBObjectPart part)
        {
            part = Entorno.CurrentEditingPart;
            if (part == null)
                return null;

            if (TokensFinder.IsUnsupportedObject(part.KBObject))
                return null;

            if (!part.LsiIsConditionsSource() && !part.LsiIsMainSource())
                return null;

            string selectedText = commandData.LsiGetSelectedText();
            if (string.IsNullOrEmpty(selectedText))
                return null;

            return selectedText;
        }

        /// <summary>
        /// Update the command status
        /// </summary>
        /// <param name="commandData">Command data sent by Genexus</param>
        /// <param name="status">Command status</param>
        /// <returns>True</returns>
        static public bool Query(CommandData commandData, ref CommandStatus status)
        {
            try
            {
                status.State = CommandState.Disabled;

                // Enable the procedure extraction if we are on a procedure / events part and
                // we have some text selected
                KBObjectPart part;
                if (string.IsNullOrEmpty(GetSelectedText(commandData, out part)))
                    return true;

                status.State = CommandState.Enabled;
                return true;
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                return false;
            }
        }

        /// <summary>
        /// Extract the selected code to a DataSelector
        /// </summary>
        static public bool Execute(CommandData commandData)
        {
            try
            {
                // Check the current object
                IGxDocument currentDoc = UIServices.Environment.ActiveDocument;
                if (currentDoc == null)
                    return false;
                if (currentDoc.Dirty)
                {
                    MessageBox.Show("Object is modified. Please, save it before run this extension");
                    return false;
                }

                KBObjectPart part;
                string code = GetSelectedText(commandData, out part);
                if (string.IsNullOrEmpty(code))
                    return false;

                // Get the "Standard" part (to avoid SD virtual parts)
                part = part.KBObject.Parts.LsiConvertPart(part);
                if (part == null)
                    return false;

                // Do the refactor
                ExtractDataSelector refactor = new ExtractDataSelector(part, code);
                DataSelector ds = refactor.ExecuteExtract();
                // Open generated data selector
                UIServices.Objects.Open(ds, OpenDocumentOptions.CurrentVersion);

                return true;
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                return false;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="partToRefactor">Object part where to extract code</param>
        /// <param name="codeToExtract">Code to extract</param>
        public ExtractDataSelector(KBObjectPart partToRefactor, string codeToExtract)
        {
            this.PartToRefactor = partToRefactor;
            this.CodeToExtract = codeToExtract;
        }

        public DataSelector ExecuteExtract()
        {
            using (Log log = new Log())
            {
                // Create the data selector
                DataSelector ds = new DataSelector(PartToRefactor.KBObject.Model);
                ds.Name = KBaseGX.GetUnusedName(KBaseGX.NAMESPACE_OBJECTS, "DSDataSelector");
                ds.Description = "Code refactoring";
                ds.Parent = PartToRefactor.KBObject.Parent;

                // Get the conditions on the selected code and put it on the DataSelector
                // Also, get the selected code parsed
                DataSelectorLevel dsData = ds.DataSelectorStructure.Root;
                ParsedCode parsedCode;
                if (PartToRefactor is ConditionsPart)
                    parsedCode = ParseConditions(dsData);
                else
                    parsedCode = ParseForEachConditions(dsData);

                // Get used variables on extracted code, and declare them as DataSelector parameters
                TokensFinder variablesFinder = new TokensFinder();
                HashSet<string> varNames = variablesFinder.SearchAllNames(parsedCode, false);
                VariablesPart originalVariables = PartToRefactor.KBObject.Parts.LsiGet<VariablesPart>();
                VariablesPart newVariables = ds.DataSelectorStructure.Variables;
                foreach (string varName in varNames)
                {
                    Variable originalVariable = originalVariables.GetVariable(varName);

                    if (originalVariable.IsStandard)
                        // Not declarable (ex. &Today)
                        continue;

                    Variable newVariable = new Variable(originalVariable.Name, newVariables);
                    newVariable.CopyPropertiesFrom(originalVariable);
                    newVariables.Add(newVariable);
                    DataSelectorParameter parameter =
                        new DataSelectorParameter(ds.DataSelectorStructure, newVariable);
                    ds.DataSelectorStructure.Parameters.Add(parameter);
                }

                // Save the data selector
                ds.Save();

                return ds;
            }
        }

        /// <summary>
        /// Get data selector call parameters
        /// </summary>
        /// <param name="ds">The extracted data selector</param>
        /// <returns>String with the call parameters, without parenthesis</returns>
        private static string DSCallParameters(DataSelector ds)
        {
            return string.Join(", ",
                ds.DataSelectorStructure.Parameters.Select(x => "&" + x.Name).ToArray()
            );
        }

        private ParsedCode ParseConditions(DataSelectorLevel dsData)
        {
            ParsedCode parsedCode = new ParsedCode(PartToRefactor, CodeToExtract);
            // Parse selected conditions, and add them to the data selector:
            foreach (Condition c in BuscadorConditions.EnumerarCondiciones(parsedCode))
            {
                // Remove semicolon (";")
                c.Punctuation = null;
                dsData.AddCondition(c.ToString());
            }
            return parsedCode;
        }

        private IEnumerable<CommandBlock> GetWhereStatements(ParsedCode forEachCode)
        {
            List<CommandBlock> result = new List<CommandBlock>();

            // It shold be only one root (the FOR EACH)
            List<ObjectBase> rootNodes = forEachCode.ArbolParseado.LsiGetRootNodes();
            if (rootNodes.Count == 0)
                return result;

            // Get wheres
            return rootNodes[0].LsiGetChildren()
                .OfType<CommandBlock>()
                .Where(cmd => cmd.Name is WordCommand &&
                              ((WordCommand)cmd.Name).Text.Trim().ToLower() == "where");

        }

        private ParsedCode ParseForEachConditions(DataSelectorLevel dsData)
        {
            // Create FOR EACH with the selected code
            string codeToParse = KeywordGx.FOREACH + Environment.NewLine + 
                CodeToExtract + Environment.NewLine +
                KeywordGx.ENDFOR;

            // Parse the selected code
            ParsedCode parsedCode = new ParsedCode(PartToRefactor, codeToParse);

            foreach (CommandBlock cmd in GetWhereStatements(parsedCode))
            {
                // Remove the "WHERE" text:
                cmd.Name = null;
                dsData.AddCondition(cmd.ToString());
            }

            return parsedCode;
        }
    }
}
