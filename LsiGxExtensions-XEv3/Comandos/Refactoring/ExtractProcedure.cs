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
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Variables;
using LSI.Packages.Extensiones.Utilidades.CodeGeneration;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos.Refactoring
{

    /// <summary>
    /// Tool to extract code to a procedure
    /// </summary>
    class ExtractProcedure
    {

        /// <summary>
        /// Code marker used that will replace the code to extract temporally
        /// </summary>
        private const string CALLMARKER = "/*--ExtractProcedure-CALLMARKER--*/";

        /// <summary>
        /// Object where to extract code
        /// </summary>
        private KBObject ObjectToRefactor;

        /// <summary>
        /// Code to extract
        /// </summary>
        private string CodeToExtract;

        /// <summary>
        /// Attributes replaced by variables on the extracted code. The key is the variable name (with
        /// ampersand, case sensitive), and the value the attribute name, lowercase
        /// </summary>
        private Dictionary<string, string> AttributesReplacements = new Dictionary<string, string>();

        /// <summary>
        /// Variables for attributes replacements, without ampersand, lowercase
        /// </summary>
        private HashSet<string> VariablesForAttributes = new HashSet<string>();

        /// <summary>
        /// Variables used on the new extracted procedure
        /// </summary>
        private HashSet<string> VariablesUsedOnNewProc;

        /// <summary>
        /// Read/write variables on the extracted code, lowercase, without ampersand
        /// </summary>
        private List<string> InOutParams;

        /// <summary>
        /// The process log
        /// </summary>
        private Log Log;

        /// <summary>
        /// True if the process has been aborted
        /// </summary>
        private bool Aborted;

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

                KBObjectPart part = Entorno.CurrentEditingPart;
                if (part == null)
                    return true;

                if (!part.LsiIsMainSource())
                    return true;

                if (string.IsNullOrEmpty(commandData.LsiGetSelectedText()))
                    return true;

                IGxView view = Entorno.EditorParteActual;
                if (view == null || view.ReadOnly)
                    // Unsupported. Currently the process expects to do intermediate modifications in source code to work
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
        /// Extract the selected code to a procedure
        /// </summary>
        static public bool Execute(CommandData commandData)
        {
            KBObject sourceObject = null;
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
                sourceObject = currentDoc.Object;
                if (TokensFinder.IsUnsupportedObject(sourceObject))
                    return false;

                string code = commandData.LsiGetSelectedText();
                if (string.IsNullOrEmpty(code))
                    return false;

                // Close the current object
                if (!UIServices.Environment.ActiveView.Close())
                    return false;

                // Do the refactor
                ExtractProcedure refactor = new ExtractProcedure(sourceObject, code);
                Procedure p = refactor.ExecuteExtract();
                // Reopen objects
                UIServices.Objects.Open(sourceObject, OpenDocumentOptions.CurrentVersion);
                if( p != null )
                    UIServices.Objects.Open(p, OpenDocumentOptions.CurrentVersion);

                return true;
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                // Reopen the source object:
                if( sourceObject != null )
                    UIServices.Objects.Open(sourceObject, OpenDocumentOptions.CurrentVersion);
                return false;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectToRefactor">Object where to extract code</param>
        /// <param name="codeToExtract">Code to extract</param>
        public ExtractProcedure(KBObject objectToRefactor, string codeToExtract)
        {
            this.ObjectToRefactor = objectToRefactor;
            this.CodeToExtract = codeToExtract;
        }

        /// <summary>
        /// Replate a code fragment on all code parts of the origin object by other code
        /// </summary>
        /// <param name="oldValue">The code to replace</param>
        /// <param name="newValue">The new code</param>
        private void ReplaceCodeOnObjectToRefactor(string oldValue, string newValue)
        {
            foreach (ISource sourcePart in ObjectToRefactor.Parts.LsiEnumerate().OfType<ISource>())
            {
                string source = Entorno.StringFormatoKbase(sourcePart.Source);
                if (source.Contains(oldValue))
                {
                    // Replace the code by a marker. After, they will be replaced by the right call
                    sourcePart.Source = source.Replace(oldValue, newValue);
                    ObjectToRefactor.Parts.LsiUpdatePart((KBObjectPart)sourcePart);
                }
            }
        }

        /// <summary>
        /// Do the code refactoring
        /// </summary>
        /// <returns>The new procedure. null if there was some error</returns>
        public Procedure ExecuteExtract()
        {
            using (Log = new Log())
            {
                // Replace the extracted code on the object by a marker:
                ReplaceCodeOnObjectToRefactor(CodeToExtract, CALLMARKER);

                // Create the new procedure
                ProcedureGenerator procedure = CreateProcedure();
                if (Aborted)
                    return null;

                // Get the procedure call code:
                string callCode = GetCallCode(procedure);

                // Replace the markers on the origin object by the new procedure call:
                ReplaceCodeOnObjectToRefactor(CALLMARKER, callCode);

                ObjectToRefactor.Save();

                return procedure.Procedure;
            }
        }

        /// <summary>
        /// Get the code to call the extracted procedure
        /// </summary>
        /// <param name="procedure">The extracted procedure</param>
        /// <returns>The code to do the call</returns>
        private string GetCallCode(ProcedureGenerator procedure)
        {
            VariablesPart variables = ObjectToRefactor.Parts.LsiGet<VariablesPart>();
            string attsToVarsAssignments = string.Empty, varsToAttsAssignments = string.Empty;
            Dictionary<string, string> finalReplacements = new Dictionary<string, string>();
            foreach (string variable in AttributesReplacements.Keys)
            {
                // Check if the variable is written (remove ampersand!):
                string normalizedVarName = variable.Substring(1).ToLower();
                if (InOutParams.Contains(normalizedVarName))
                {
                    // Normalize the attribute name
                    string att = AttributesReplacements[variable].ToLower();

                    // Written attribute: Add a variable and add the required assignments
                    string newVarName = "&" + CreateVariableForAttribute(variables, att);
                    finalReplacements[variable] = newVarName;

                    // Assignations to add to the caller code, before and after
                    attsToVarsAssignments += newVarName + " = " + att + Environment.NewLine;
                    varsToAttsAssignments += att + " = " + newVarName + Environment.NewLine;
                }
                else
                    // It's not written, do the original replacement
                    finalReplacements[variable] = AttributesReplacements[variable];
            }

            return attsToVarsAssignments +
                procedure.Parm.GenerateCall(procedure.Procedure.Name, finalReplacements) +
                ( string.IsNullOrEmpty(varsToAttsAssignments) ? "" : Environment.NewLine ) +
                varsToAttsAssignments;

        }

        /// <summary>
        /// Declare a list of parameters on the extracted procedure
        /// </summary>
        /// <param name="p">The new procedure</param>
        /// <param name="variables">Variables part of the new procedure</param>
        /// <param name="variableNames">Variables names to declare on the parm rule</param>
        /// <param name="accessType">Access type of the parameters (in:, out: or inout:)</param>
        private void DeclareParams(ProcedureGenerator p, VariablesPart variables, 
            IEnumerable<string> variableNames, RuleDefinition.ParameterAccess accessType)
        {
            // Variables to do not pass as parameters:
            string[] varsToIgnore = { "today", "pgmname", "pgmdesc" , "time" };

            foreach (string variableName in variableNames)
            {
                if( varsToIgnore.Contains(variableName.ToLower()) )
                    continue;

                Variable v = variables.GetVariable(variableName);
                if( v != null )
                    p.Parm.AgregarParametro(v, accessType);
            }
        }

        /// <summary>
        /// Create the procedure with the code to extract
        /// </summary>
        /// <returns>The new procedure</returns>
        private ProcedureGenerator CreateProcedure()
        {
            ProcedureGenerator p = new ProcedureGenerator();
            p.Procedure.Name = KBaseGX.GetUnusedName(KBaseGX.NAMESPACE_OBJECTS, "PProcedure");
            p.Procedure.Description = "Code refactoring";
            p.Procedure.ProcedurePart.Source = CodeToExtract + Environment.NewLine;
            p.Procedure.Parent = ObjectToRefactor.Parent;

            // Parse code
            ParsedCode parsedCode = new ParsedCode(p.Procedure.ProcedurePart);

            // Validate the code to extract:
            if (parsedCode.Any(x => x.LsiIsCmdPrint() || x.LsiIsSubDefinition() || x is CommandEvent))
            {
                Log.Output.AddErrorLine(
                    "The code to extract cannot contain PRINT commands or SUB / Events definitions");
                Aborted = true;
                return null;
            }

            // Move referenced subroutines by the extracted code to the new procedure
            if( HandleCalledSubroutines(parsedCode, p.Procedure) )
                // The procedure code has been modified, reparse it
                parsedCode = new ParsedCode(p.Procedure.ProcedurePart);
            if (Aborted)
                return null;

            // Declare used variables on the new procedure
            DeclareUsedVariables(p, parsedCode);

            // Replace orphan attributes by parameter variables
            if( ReplaceOrphanAttsByVariables(p.Procedure, parsedCode) )
                // The procedure code has been modified, reparse it
                parsedCode = new ParsedCode(p.Procedure.ProcedurePart);

            // Declare parameters on new procedure
            DeclareParms(p, parsedCode);

            // Save the new procedure
            p.SaveProcedure();

            return p;
        }

        /// <summary>
        /// Declare used variables on new procedure
        /// </summary>
        /// <param name="p">The new procedure</param>
        /// <param name="parsedCode">The new procedure parsed code</param>
        private void DeclareUsedVariables(ProcedureGenerator p, ParsedCode parsedCode)
        {
            // Get used variables on the new procedure:
            TokensFinder variablesFinder = new TokensFinder();
            VariablesUsedOnNewProc = variablesFinder.SearchAllNames(parsedCode, false);

            // Copy variables from source object
            VariablesPart sourceVariables = ObjectToRefactor.Parts.LsiGet<VariablesPart>();
            foreach (string variableName in VariablesUsedOnNewProc)
            {
                Variable sourceVariable = sourceVariables.GetVariable(variableName);
                if (sourceVariable != null && !sourceVariable.IsStandard)
                    p.CreateVariable(sourceVariable);
            }
        }

        /// <summary>
        /// Declare new procedure parameters
        /// </summary>
        /// <param name="p">The new procedure</param>
        /// <param name="parsedCode">The new procedure parsed code</param>
        private void DeclareParms(ProcedureGenerator p, ParsedCode parsedCode)
        {

            // Get the unused variables of the source object. They will not be declared on the
            // new procedure parameters
            ObjectVariablesReferences varReferences =
                new ObjectVariablesReferences(ObjectToRefactor, false,
                    LsiExtensionsConfiguration.Load().AlwaysNullVariablesPrefixSet, null);
            List<string> unusedVarsOnOrigin = varReferences.GetUnusedVariables();

            // Analyze variables edition on new procedure code:
            BuscadorLecturasEscrituras readWritesAnalyzer = new BuscadorLecturasEscrituras();
            BuscadorLecturasEscrituras.InformacionBusqueda rwAnalysis =
                readWritesAnalyzer.ObtenerInformacionBusqueda(parsedCode);
            HashSet<string> readedVariables = readWritesAnalyzer.VariablesLeidas(rwAnalysis);
            HashSet<string> writtenVariables = readWritesAnalyzer.VariablesEscritas(rwAnalysis);

            // Variables part on the new procedure
            VariablesPart targetVariables = p.Procedure.Parts.LsiGet<VariablesPart>();

            // Variables to declare on parm rule (used in origin object, or added now):
            IEnumerable<string> varsToDeclare = VariablesUsedOnNewProc
                .Where(x => !unusedVarsOnOrigin.Contains(x))
                .Union(VariablesForAttributes);

            // Declare in: parameters
            IEnumerable<string> parms = varsToDeclare
                .Where(x => readedVariables.Contains(x) && !writtenVariables.Contains(x));
            DeclareParams(p, targetVariables, parms, RuleDefinition.ParameterAccess.PARM_IN);

            // Declare inout: parameters
            InOutParams = varsToDeclare
                .Where(x => readedVariables.Contains(x) && writtenVariables.Contains(x))
                .ToList();
            DeclareParams(p, targetVariables, InOutParams, RuleDefinition.ParameterAccess.PARM_INOUT);

            // Declare only written variables as inout: parameters (we cannot guarantee the variable
            // is written ALWAYS, the writting can be contidioned)
            IEnumerable<string> writtenOnlyVars = varsToDeclare
                .Where(x => !readedVariables.Contains(x) && writtenVariables.Contains(x));
            if (writtenOnlyVars.Any())
            {
                p.Parm.AddDocumentation("TODO: Following variables could be out:, check it out", true);
                DeclareParams(p, targetVariables, writtenOnlyVars, 
                    RuleDefinition.ParameterAccess.PARM_INOUT);
                InOutParams.AddRange(writtenOnlyVars);
            }

            // Remove the unused variables on the origin object:
            VariablesPart sourceVariables = ObjectToRefactor.Parts.LsiGet<VariablesPart>();
            sourceVariables.LsiRemove(unusedVarsOnOrigin);
            ObjectToRefactor.Parts.LsiUpdatePart(sourceVariables);

        }

        /// <summary>
        /// Moves subs called on the extracted code to the new procedure, and it removes them from the
        /// source object
        /// </summary>
        /// <param name="parsedCode">Parsed code of the new procedure</param>
        /// <param name="newProcedure">The new procedure</param>
        /// <returns>True if some change has been done on the new procedure code</returns>
        private bool HandleCalledSubroutines(ParsedCode parsedCode, Procedure newProcedure)
        {
            // Find subroutines calls on extracted procedure:
            SubroutinesFinder subsFinder = new SubroutinesFinder();
            List<string> subsToExtract = 
                subsFinder.SearchCalledSubroutinesNames(parsedCode).ToList();

            if (!subsToExtract.Any())
                return false;

            // Removed subs definitions from the source object, and add them to the new procedure
            // Do it recursivelly
            ParsedCode sourceCode = new ParsedCode(ObjectToRefactor.Parts.LsiGetMainSoucePart());
            HashSet<string> alreadyExtractedSubs = new HashSet<string>();
            while( subsToExtract.Count > 0 )
            {
                // Get the next sub to extract
                string subName = subsToExtract[0];
                subsToExtract.RemoveAt(0);
                alreadyExtractedSubs.Add(subName);

                // Get the sub code to extract
                Subroutine sub = subsFinder.SearchSubroutineDefinition(sourceCode, subName);
                if (sub == null)
                    continue;
                string subCode = sub.ToString();

                // Move the sub from the source object to the destination
                ReplaceCodeOnObjectToRefactor(subCode, Environment.NewLine);
                newProcedure.ProcedurePart.Source += Environment.NewLine + subCode + Environment.NewLine;

                // Search other subs calls inside the moved sub
                ParsedCode subParsedCode = new ParsedCode(sourceCode.Part, subCode);
                List<string> calledSubsOnSub = 
                    subsFinder.SearchCalledSubroutinesNames(subParsedCode).ToList();

                // Add these news subs to the subs list to extract
                subsToExtract.AddRange(
                    calledSubsOnSub
                        .Where(x => !alreadyExtractedSubs.Contains(x) && !subsToExtract.Contains(x) )
                );
            }

            // Re-parse the origin object code, it has been modified:
            sourceCode = new ParsedCode(ObjectToRefactor.Parts.LsiGetMainSoucePart());

            // Check if some of the moved subs it's still called from the origin object. If they are, 
            // the refactoring cannot be done
            HashSet<string> subsStillCalledOnOrigin = subsFinder.SearchCalledSubroutinesNames(sourceCode);
            IEnumerable<string> wrongSubs = subsStillCalledOnOrigin.Intersect(alreadyExtractedSubs);
            if (wrongSubs.Any())
            {
                Log.Output.AddErrorLine("Extract cannot be done: Following SUBs to extract are still " +
                    "called from the object: " + string.Join(", ", wrongSubs.ToArray()));
                Aborted = true;
            }

            return true;
        }

        /// <summary>
        /// Replace orphan attributes by variables on the extracted procedure
        /// </summary>
        /// <param name="targetProcedure">The extracted procedure</param>
        /// <param name="parsedCode">The parsed code of the extracted procedure</param>
        /// <returns>True if the extracted procedure code was modified</returns>
        private bool ReplaceOrphanAttsByVariables(Procedure targetProcedure, 
            ParsedCode parsedCode)
        {
            VariablesPart variables = parsedCode.Object.Parts.LsiGet<VariablesPart>();

            // Replace orphan attributes by variables:
            bool codeModified = false;
            OrphanAttributesFinder orphanFinder = new OrphanAttributesFinder();
            foreach (string orphanAtt in orphanFinder.SearchAllNames(parsedCode, false)) 
            {
                // Get the original attribute name:
                string varName = CreateVariableForAttribute(variables, orphanAtt);

                // Keep track of these attributes / variables
                AttributesReplacements.Add("&" + varName , orphanAtt );
                VariablesForAttributes.Add(varName.ToLower());

                // Do replacements on procedure code (single word replacement only). Do not do the replacement inside NEWs and FOR EACHs
                // because it will broke things ( WHERE att = &att will be replaced by WHERE &att1 = &att )
                ObjectBaseReplacer replacer = ObjectBaseReplacer.ReplaceAttributeByVariable(orphanAtt, "&" + varName);
                replacer.ExcludeNewsAndForEachs = true;
                replacer.Execute(parsedCode);

                codeModified = true;
            }

            if (codeModified)
                targetProcedure.ProcedurePart.Source = parsedCode.ParsedCodeString;

            return codeModified;
        }

        /// <summary>
        /// Create a variable based on attribute
        /// </summary>
        /// <param name="variables">The variables part where to add the variable</param>
        /// <param name="attName">The attribute name</param>
        /// <returns>The created variable name</returns>
        private static string CreateVariableForAttribute(VariablesPart variables, string attName)
        {
            Artech.Genexus.Common.Objects.Attribute att =
                Artech.Genexus.Common.Objects.Attribute.Get(UIServices.KB.CurrentModel, attName);

            // Create the variable
            string varName = variables.LsiGetUnusedVariableName(att.Name);
            Variable attVariable = new Variable(varName, variables);
            attVariable.AttributeBasedOn = att;
            variables.Add(attVariable);
            return varName;
        }

    }
}
