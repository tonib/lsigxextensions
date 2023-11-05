using ActiproSoftware.SyntaxEditor;
using Artech.Common.Framework.Commands;
using Artech.FrameworkDE.Text;
using Artech.Genexus.Common;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.ParmsInfo
{
    /// <summary>
    /// Tool to display parameters info when Genexus don't
    /// </summary>
    class ParametersInfo
    {

        BaseSyntaxEditor SyntaxEditor;
        ObjectNamesCache Names;
        IntelliPromptParameterInfo PInfo;
        LsiExtensionsConfiguration Cfg;

        public ParametersInfo(BaseSyntaxEditor syntaxEditor, ObjectNamesCache names)
        {
            SyntaxEditor = syntaxEditor;
            Names = names;
            PInfo = SyntaxEditor.IntelliPrompt.ParameterInfo;
            Cfg = LsiExtensionsConfiguration.Load();
        }

        /// <summary>
        /// Current selection on text editor has changed event handler
        /// </summary>
        public void SelectionChanged(SelectionEventArgs e)
        {
            if (Cfg.ExtendedParmInfoType == LsiExtensionsConfiguration.ParmInfoType.Never)
                return;

            if (!PInfo.Visible)
                return;

            bool isGenexusParmInfo = IsGenexusParamInfo();
            if (Cfg.ExtendedParmInfoType == LsiExtensionsConfiguration.ParmInfoType.OnlyWhenNoGenexus && isGenexusParmInfo)
                return;

            CallStatusFinder callFinder = new CallStatusFinder(SyntaxEditor.Document, SyntaxEditor.Caret.Offset, Names);

            if (callFinder.ParameterInfo == null)
            {
                // Do not hide genexus parms info, it can be a function or a method parm info, not handled by this extension
                if( !isGenexusParmInfo )
                    PInfo.Hide();
                return;
            }

            bool parameterChanged = SetParameterInfoText(callFinder);

            if (e.LastCaretDocumentPosition.Line != e.CaretDocumentPosition.Line)
                // Move the tip under the new line
                ShowParameterInfo(callFinder);
            else if (parameterChanged)
                // Refresh tooltip
                PInfo.MeasureAndResize(PInfo.Bounds.Location);
        }

        /// <summary>
        /// Key typed on text editor event handler
        /// </summary>
        public void KeyTyped(KeyTypedEventArgs e)
        {
            if (Cfg.ExtendedParmInfoType == LsiExtensionsConfiguration.ParmInfoType.Never)
                return;

            if (PInfo.Visible)
            {
                if(!IsGenexusParamInfo() || Cfg.ExtendedParmInfoType == LsiExtensionsConfiguration.ParmInfoType.OnlyWhenNoGenexus)
                    return;
            }

            if (e.KeyChar != ',' && e.KeyChar != '(')
                return;

            // If we are editing comments, or strings, or other, do not update info
            IToken t = SyntaxEditor.SelectedView.GetCurrentToken();
            if (t != null && t.LexicalState.Key != StateKeys.DefaultState)
                return;

            DisplayCurrentParameterInfo();
        }

        private void DisplayCurrentParameterInfo()
        {
            CallStatusFinder callFinder = new CallStatusFinder(SyntaxEditor.Document, SyntaxEditor.Caret.Offset, Names);
            if (callFinder.ParameterInfo == null)
                return;

            SetParameterInfoText(callFinder);
            ShowParameterInfo(callFinder);
        }

        /// <summary>
        /// Check if the displayed parameter info was displayed by Genexus
        /// </summary>
        /// <returns>True if it was displayed by Genexus</returns>
        private bool IsGenexusParamInfo()
        {
            return PInfo.Info.Count != 1 || !PInfo.Info[0].StartsWith("*");
        }

        /// <summary>
        /// Set the text to display for the current parameter
        /// </summary>
        /// <param name="callFinder">Current parameter info</param>
        /// <returns>True if the text has changed. False if the new text is equal to the previous</returns>
        private bool SetParameterInfoText(CallStatusFinder callFinder)
        {
            // If assign this, Genexus throws exceptions. So, I'll check parameter change with text change
            //PInfo.ParameterIndex = callFinder.IdxParameter;

            string newText = ParametersInfoText(callFinder);
            if (PInfo.Info.Count == 1 && PInfo.Info[0] == newText)
                return false;

            PInfo.Info.Clear();
            PInfo.Info.Add(newText);
            return true;
        }

        private void ShowParameterInfo(CallStatusFinder callFinder)
        {
            DocumentLine currentLine = SyntaxEditor.Document.Lines[SyntaxEditor.Caret.EditPosition.Line];
            PInfo.Show(currentLine.StartOffset + currentLine.IndentAmount);
        }

        static private string ParametersInfoText(CallStatusFinder callFinder)
        {
            string text = "* " + ToHtml(callFinder.KbObjectName.Description) + "<br/>parm( ";
            bool first = true;
            foreach(ParameterElement p in callFinder.ObjectParameters)
            {
                string parmText = string.Empty;
                if (first)
                    first = false;
                else
                    parmText += " , ";

                if (p == callFinder.ParameterInfo)
                    // Do not use <b>: This sometimes breaks Gx own parameter info
                    parmText += "<span style=\"font-weight: bold\">";
                parmText += ParameterToText(p);
                if (p == callFinder.ParameterInfo)
                    parmText += "</span>";

                text += parmText;
            }
            text += " );";
            
            // Append parm rule documentation
            if (!string.IsNullOrEmpty(callFinder.CalledObjectParmRule) && callFinder.ParameterInfo != null)
            {
                ParmDocumentationParser docParser = new ParmDocumentationParser(callFinder.CalledObjectParmRule);
                string doc = docParser.GetDocumentation(callFinder.ParameterInfo.IsAttribute, callFinder.ParameterInfo.Name);
                if(!string.IsNullOrEmpty(doc))
                {
                    doc = ToHtml(doc);
                    //text += "<br/><span style=\"font-weight: bold\">" + ParameterToText(callFinder.ParameterInfo) + 
                    //    "</span>  <span style=\"color:DarkGreen\">// " + doc + "</span>";
                    text += "<br/><span style=\"font-weight: bold; color:DarkGreen\">// " + doc + "</span>";
                }
            }
            return text;
        }


        static private string ParameterToText(ParameterElement p)
        {
            string text = "<i>" + p.ParameterAccessText + "</i> ";
            text += ToHtml(p.NombreAtributoVariable);
            text += ": <span style=\"color:blue\">" +
                ToHtml(p.TypeDescription) +
                "</span>";
            return text;
        }

        static private string ToHtml(string text)
        {
            text = HttpUtility.HtmlEncode(text);
            // To avoid break genexus stuff, dont put parenthesis or commas on the tooltip (needed, really)
            return text
                .Replace("(", "&#40;")
                .Replace(")", "&#41;")
                .Replace(",", "&#44;");
        }

        /// <summary>
        /// Display current parameter info
        /// </summary>
        /// <param name="commandData">Command data</param>
        /// <returns>True if the command has been executed (?)</returns>
        static public bool DisplayParameterInfo(CommandData commandData)
        {
            try
            {
                BaseSyntaxEditor syntaxEditor = Autocomplete.GetEditorFromCommandData(commandData);
                if (syntaxEditor == null)
                    return false;

                if (!Autocomplete.NamesCache.Ready)
                    return false;

                new ParametersInfo(syntaxEditor, Autocomplete.NamesCache).DisplayCurrentParameterInfo();
                return true;
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                return false;
            }
        }

    }
}
