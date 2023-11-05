using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Language.Parser.Data;
using System.Reflection;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
    /// <summary>
    /// Tool to replace a Genexus code token by other on a parsed code tree
    /// </summary>
    public class ObjectBaseReplacer
    {

        /// <summary>
        /// Token to replace on the code
        /// </summary>
        private Word TokenToReplace;

        /// <summary>
        /// String version of the token to replace on the code, lowercase and trimmed
        /// </summary>
        private string TxtTokenToReplace;

        /// <summary>
        /// The replacement for the token
        /// </summary>
        private Word Replacement;

        /// <summary>
        /// Should we do not replace instances inside NEW  / FOR EACH statements?
        /// </summary>
        public bool ExcludeNewsAndForEachs = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tokenToReplace">Code token to replace</param>
        /// <param name="replacement">The new code token</param>
        public ObjectBaseReplacer(Word tokenToReplace, Word replacement)
        {
            this.TokenToReplace = tokenToReplace;
            this.Replacement = replacement;

            TxtTokenToReplace = TokenToReplace.ToString().Trim();
        }

        /// <summary>
        /// Delegate called for each code node
        /// </summary>
        /// <param name="node">Code node to analize</param>
        /// <param name="state">Current parse state</param>
        private void ParseCodeNode(ObjectBase node, ParsedCodeFinder.SearchState state)
        {

            if (ExcludeNewsAndForEachs && (node.LsiIsForEachStatement() || node.LsiIsNewStatement()))
            {
                // Ignore for each / new statements
                state.SearchDescendants = false;
                return;
            }

            if (node.GetType() != TokenToReplace.GetType())
                return;
            if (node.ToString().ToLower().Trim() != TxtTokenToReplace)
                return;

            // This does not work when node is a WordWithBlanks, because it removes the blanks
            // And blanks can contain line breaks, and they cannot be removed...
            
            /*
            // This a node to replace. Do it on the parent:
            ObjectBase parent = state.Parent;
            PropertyInfo parentProperty = state.ParentProperty;
            if (parent == null || parentProperty == null)
                return;

            object propertyValue = parentProperty.GetValue(parent, null);
            ObjectBaseCollection collection = propertyValue as ObjectBaseCollection;
            if (collection != null)
            {
                // The parent property it's a collection. Do the replacement on it:
                int idx = collection.IndexOf(node);
                if (idx >= 0)
                {
                    collection[idx] = Replacement;
                    state.SearchDescendants = false;
                }
            }
            else
            {
                // Property is not a collection:
                parentProperty.SetValue(parent, Replacement, null);
                state.SearchDescendants = false;
            }
             */

            // Patch
            Word wordToReplace = node as Word;
            if (wordToReplace == null)
                return;
            wordToReplace.Text = Replacement.Text;

        }

        /// <summary>
        /// Do the replacements on the code
        /// </summary>
        /// <param name="code">Code where to do the replacements</param>
        public void Execute(ParsedCode code)
        {
            ParsedCodeFinder parser = new ParsedCodeFinder(ParseCodeNode);
            parser.Execute(code);
        }

        /// <summary>
        /// Creates an instance to replace an attribute by a variable
        /// </summary>
        /// <param name="attributeName">The attribute name</param>
        /// <param name="variableName">The variable name. It must include the ampersand (&amp;)</param>
        /// <returns>The instance to do the replacement</returns>
        static public ObjectBaseReplacer ReplaceAttributeByVariable(string attributeName,
            string variableName)
        {
            AttributeName att = new AttributeName();
            att.Text = attributeName;
            VariableName var = new VariableName();
            var.Text = variableName;
            return new ObjectBaseReplacer(att, var);
        }

    }
}
