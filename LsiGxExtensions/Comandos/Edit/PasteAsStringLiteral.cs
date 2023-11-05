using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Framework.Commands;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LSI.Packages.Extensiones.Comandos.Edit
{

    /// <summary>
    /// Pastes the clipboard content as a a genexus string literal
    /// </summary>
    public class PasteAsStringLiteral : IExecutable
    {

        private const int MAXLINELENGTH = 120;

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
                status.State = Clipboard.ContainsText() ? CommandState.Enabled : CommandState.Disabled;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string FormatLine(string line)
        {
            string delimiter = "'";
            if (line.Contains('\''))
            {
                if (line.Contains('"'))
                    // Replace single quotes on text
                    line = Regex.Replace(line, @"\'+", m => "' + \"" + m.Value + "\" + '");
                else
                    delimiter = "\"";
            }

            // Ugly case
            if (line.EndsWith("+ ''"))
                line = line.Substring(0, line.Length - 4);

            return delimiter + line + delimiter;
        }

        private List<string> SplitByLength(string line)
        {
            // TODO: Try to do not break words
            List<string> result = new List<string>();
            while(line.Length > MAXLINELENGTH)
            {
                result.Add( FormatLine(line.Substring(0, MAXLINELENGTH)) + " + " + Environment.NewLine );
                line = line.Substring(MAXLINELENGTH);
            }
            result.Add(FormatLine(line));
            return result;
        }

        /// <summary>
        /// Execute the task
        /// </summary>
        public void Execute()
        {
            try
            {
                if (!Clipboard.ContainsText())
                    return;

                string text = Clipboard.GetText();
                if (string.IsNullOrEmpty(text))
                    return;

                text = Entorno.StringFormatoKbase(text);
                string newText = string.Empty;
                foreach (string line in text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                {
                    if (!string.IsNullOrEmpty(newText))
                        newText += " + NewLine() + " + Environment.NewLine;
                    foreach (string subline in SplitByLength(line))
                        newText += subline;
                }

                // Do the paste
                Clipboard.SetText(newText);
                UIServices.CommandDispatcher.Dispatch(new CommandKey(new Guid("98121D96-A7D8-468b-9310-B1F468F812AE"), "Paste"));

            }
            catch(Exception ex)
            {
                Log.ShowException(ex);
            }
        }
    }
}
