using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Language.Parser.Data;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
    /// <summary>
    /// Tool to remove dots of logical operators from gx code
    /// </summary>
    /// <remarks>
    /// This changes ".NOT." for "NOT", the same with "AND", "OR" (etc)
    /// </remarks>
    public class RemoveLogicalOperatorsDots : IExecutable
    {

        /// <summary>
        /// Object to modify
        /// </summary>
        private KBObject ObjectToModify;

        /// <summary>
        /// Currently analized part was modified?
        /// </summary>
        private bool CurrentPartModified;

        /// <summary>
        /// Operators to change: ".or." , ".and.", etc
        /// </summary>
        private string[] OperatorsToChange;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="o">Object to modify</param>
        public RemoveLogicalOperatorsDots(KBObject o)
        {
            ObjectToModify = o;
            OperatorsToChange = KeywordGx.BOOLEAN_OPERATORS.Select(x => "." + x + ".").ToArray();
        }

        /// <summary>
        /// Do the operators replacement
        /// </summary>
        public void Execute()
        {
            ParsedCodeFinder parser = new ParsedCodeFinder(ParseCodeNode);
            foreach (KBObjectPart part in ObjectToModify.Parts.LsiEnumerate())
            {
                // Ignore forms, etc.
                if (!(part is ISource))
                    continue;
                
                // Check conditions bug:
                ConditionsPart conditions = part as ConditionsPart;
                if( conditions != null ) 
                {
                    if (conditions.Source.ToLower().Contains("when"))
                        // Dont to the replacement. There is a bug with when and the parser
                        continue;
                }

                // Do the replacement
                ParsedCode code = new ParsedCode(part);
                CurrentPartModified = false;
                parser.Execute(code);
                if (CurrentPartModified)
                {
                    ISource source = (ISource)part;
                    source.Source = code.ParsedCodeString;
                    ObjectToModify.Parts.LsiUpdatePart(part);
                }
            }
        }

        /// <summary>
        /// Delegate called for each code node
        /// </summary>
        /// <param name="node">Code node to analize</param>
        /// <param name="state">Current parse state</param>
        private void ParseCodeNode(ObjectBase node, ParsedCodeFinder.SearchState state)
        {
            Word word = node as Word;
            if (word == null)
                return;
            string text = word.Text.ToLower();
            if (OperatorsToChange.Contains(text))
            {
                word.Text = word.Text.Substring(1, word.Text.Length - 2);
                CurrentPartModified = true;
            }
        }
    }
}
