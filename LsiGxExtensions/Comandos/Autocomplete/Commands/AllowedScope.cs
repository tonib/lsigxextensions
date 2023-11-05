using Artech.Genexus.Common;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.Commands
{
    /// <summary>
    /// Scope where a command can be inserted
    /// </summary>
    class AllowedScope
    {

        /// <summary>
        /// Scope where it's allowed the command. null == All scopes
        /// </summary>
        public string BlockId;

        /// <summary>
        /// Only if BlockId != null. True if command is allowed on nested scopes
        /// </summary>
        public bool AllowInNestedBlocks;

        /// <summary>
        /// Part types where the command can be used. null if can be used in any part
        /// </summary>
        public ObjectPartType[] AllowedPartTypes;

        public AllowedScope(string blockId, bool allowInNestedBlocks = false)
        {
            BlockId = blockId;
            AllowInNestedBlocks = allowInNestedBlocks;
        }

        public AllowedScope(params ObjectPartType[] allowedPartTypes)
        {
            AllowedPartTypes = allowedPartTypes;
        }

        public bool Allowed(AutocompleteContext context)
        {
            if (AllowedPartTypes != null && !AllowedPartTypes.Any(x => x.Equals(context.ObjectPartType)))
                return false;

            if (!CheckScope(context))
                return false;

            return true;
        }

        private bool CheckScope(AutocompleteContext context)
        {
            if (BlockId == null)
                return true;

            if (AllowInNestedBlocks)
                return context.OutlineState.Contains(BlockId);
            else
            {
                if (context.OutlineState.CurrentScope == BlockId)
                    return true;
                if (context.LineParser.CompletedTokensCount == 0 && 
                    context.LineParser.LineStartOffset == context.OutlineState.CurrentScopeStartOffset)
                {
                    // Workarround for this case: "SUB |" and you press back key ("SUB|"). We need to check the parent scope
                    return context.OutlineState.ParentScope == BlockId;
                }
                return false;
            }
        }
    }
}
