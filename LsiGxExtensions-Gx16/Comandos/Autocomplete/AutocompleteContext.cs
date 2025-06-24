using ActiproSoftware.SyntaxEditor;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.FrameworkDE.Text;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Comandos.Autocomplete.Commands;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using LSI.Packages.Extensiones.Comandos.Autocomplete.PredictionBindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete
{
    /// <summary>
    /// Current context information for autocompletion
    /// </summary>
    public class AutocompleteContext
    {
        /// <summary>
        /// Text editor
        /// </summary>
        public SyntaxEditor SyntaxEditor;

        /// <summary>
        /// Autocomplete members list 
        /// </summary>
        public IntelliPromptMemberList MemberList {  get {  return SyntaxEditor.IntelliPrompt.MemberList; } }

        /// <summary>
        /// Current editor text line details
        /// </summary>
        public LineParser LineParser;

        /// <summary>
        /// Kb names cache
        /// </summary>
        public ObjectNamesCache NamesCache;

        /// <summary>
        /// Prediction model. It will be null if it's no available
        /// </summary>
        internal AutocompletePrediction Predictor;

        // The current outline state
        public OutlineState OutlineState;

        /// <summary>
        /// Context of the current call parameter on the current caret
        /// </summary>
        public CallStatusFinder CallFinder;

        /// <summary>
        /// Current editing object
        /// </summary>
        public KBObject Object;

        /// <summary>
        /// Current editing part
        /// </summary>
        public KBObjectPart Part;

        /// <summary>
        /// Current object / part type
        /// </summary>
        public ObjectPartType ObjectPartType;

        public AutocompleteContext(SyntaxEditor syntaxEditor, ObjectNamesCache namesCache, AutocompletePrediction predictor)
        {
            SyntaxEditor = syntaxEditor;
            NamesCache = namesCache;
            Predictor = predictor;
            Object = UIServices.Environment.ActiveDocument.Object;
            Part = Entorno.CurrentEditingPart;
            ObjectPartType = new ObjectPartType(Part);

            // TODO: Lazy load?
            OutlineState = new OutlineState(SyntaxEditor);
            LineParser = new LineParser(SyntaxEditor);
            CallFinder = new CallStatusFinder(SyntaxEditor.Document, SyntaxEditor.Caret.Offset, NamesCache);
        }

        /// <summary>
        /// True if the autocomplete can be applied on the current context. On comments, strings and
        /// on native code lines ("CSHARP") autocomplete is not applicable
        /// </summary>
        public bool AutocompleteApplicable
        {
            get
            {
                // Special case: String DO identifier (Do '|')
                if (CursorInDoIdentifier)
                    return true;

                // Do not autocomplete inside comments or string constants
                IToken t = SyntaxEditor.SelectedView.GetCurrentToken();
                if (t != null)
                {
                    if (t.LexicalState.Key != StateKeys.DefaultState)
                        return false;
                }

                if (LineParser.CompletedTokensCount == 0)
                    return true;

				string firstLineToken = LineParser.GetCompletedTextToken(0).ToLower();

                // "[win] {" blocks are not supported
                if (firstLineToken == "[")
                    return false;

                // Autocomplete should be disabled after native commands. Check the line first token
                if (KeywordGx.NATIVECODEKEYWORDS.Any(x => x.ToLower() == firstLineToken))
                    // Native code: Disable autocomplete
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Cursor is in a sub call? (ex. DO 'SubName|')
        /// </summary>
        static public bool IsCursorInDoIdentifier(SyntaxEditor syntaxEditor, LineParser lineParser)
		{
            IToken t = syntaxEditor.SelectedView.GetCurrentToken();
            if (t == null)
                return false;

            // Ev3U3: This it what should be, but StateKeys seems to be declared wrong (StateKeys.StringState == "MultiLineCommentState") ¯\_(ツ)_/¯
            //if (t.LexicalState.Key != StateKeys.StringState && t.LexicalState.Key != StateKeys.CharacterState)
            if (t.LexicalState.Key != "StringState" && t.LexicalState.Key != "CharacterState")
                return false;

            if (lineParser.CompletedTokensCount == 0)
                return false;

            string firstLineToken = lineParser.GetCompletedTextToken(0).ToLower();
            return firstLineToken == "do";
        }

        /// <summary>
        /// Cursor is in a sub call? (ex. DO 'SubName|')
        /// </summary>
        public bool CursorInDoIdentifier => IsCursorInDoIdentifier(SyntaxEditor, LineParser);
        
    }
}
