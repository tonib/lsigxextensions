using ActiproSoftware.SyntaxEditor;
using Artech.Architecture.Common.Objects;
using Artech.FrameworkDE.Text;
using Artech.Genexus.Common.Parts;
using Artech.Patterns.WorkWithDevices.Parts;
using LSI.Packages.Extensiones.Comandos.Autocomplete.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete
{
    public class OutlineState
    {

        /// <summary>
        ///  Current outlining scopes
        /// </summary>
        private List<OutliningNode> CurrentBranch = new List<OutliningNode>();

        /// <summary>
        /// Block identifiers, with Scope.ForCodeBlock replaced by Scope.ForEachBlock if needed
        /// </summary>
        private List<string> BlockTypes = new List<string>();

        public OutlineState(SyntaxEditor syntaxEditor)
        {
            
            // Find the leaf containing the caret
            OutliningNode root = syntaxEditor.Document.Outlining.RootNode;
            if (root == null)
                return;
            OutliningNode node = root.FindNodeRecursive(syntaxEditor.Caret.Offset);
            if(node == null && root.Count > 0)
            {
                // This seems a bug of FindNodeRecursive: If the cursor is at the end of the document, and the 
                // last node is open (ex. "ENDI|" for if/endif), the node is not found:
                OutliningNode lastRootChildNode = root[root.Count - 1];
                if (lastRootChildNode.IsOpen && lastRootChildNode.EndOffset == syntaxEditor.Caret.Offset)
                    node = lastRootChildNode;
            }
            if (node == null)
                return;

            while (!node.IsRoot)
            {
                CurrentBranch.Insert(0, node);
                BlockTypes.Insert(0, GetRightScope(syntaxEditor, node));
                node = node.ParentNode;
            }
        }

        private string GetRightScope(SyntaxEditor editor, OutliningNode node)
        {
            if (node.ParseData.Key != ScopeId.ForCodeBlock)
                return node.ParseData.Key;

            TextStream textStream = editor.Document.GetTextStream(node.StartOffset);
            if (textStream.Token == null)
                return node.ParseData.Key;
            if(textStream.TokenText.ToLower() != "for each")
                return node.ParseData.Key;

            return ScopeId.ForEachBlock;
        }

        /// <summary>
        /// Current editing scope identifier
        /// </summary>
        public string CurrentScope
        {
            get 
            {
                if (BlockTypes.Count == 0)
                    return ScopeId.Root;
                return BlockTypes.Last();
            }
        }

        /// <summary>
        /// Offset where starts the current scope
        /// </summary>
        public int CurrentScopeStartOffset
        {
            get
            {
                if (BlockTypes.Count == 0)
                    return 0;
                return CurrentBranch.Last().StartOffset;
            }
        }

        /// <summary>
        /// Parent scope
        /// </summary>
        public string ParentScope
        {
            get
            {
                if (BlockTypes.Count < 2)
                    return ScopeId.Root;
                return BlockTypes[BlockTypes.Count-2];
            }
        }

        /// <summary>
        /// True if the current scope is closed
        /// </summary>
        public bool CurrentScopeClosed
        {
            get
            {
                if (CurrentBranch.Count == 0)
                    return true;
                return !CurrentBranch.Last().IsOpen;
            }
        }

        public bool Contains(string blockId)
        {
            return BlockTypes.Any(x => x == blockId);
        }

        /// <summary>
        /// Returns true if the current scope can contain attributes
        /// </summary>
        /// <param name="part">Current object part</param>
        /// <returns>True if the scope can contain attributes</returns>
        public bool CanContainAttributes(KBObjectPart part)
        {
            if (part is EventsPart || part is VirtualEventsPart)
                return CurrentScope != ScopeId.Root;

            if (part is ConditionsPart || part is VirtualConditionsPart || part is RulesPart || part is VirtualRulesPart )
                return true;

            // Procedures
            return BlockTypes
                .Any(x => x == ScopeId.XforCodeBlock || x == ScopeId.ForEachBlock || 
                x == ScopeId.NewCodeBlock || x == ScopeId.XnewCodeBlock);
        }
    }
}
