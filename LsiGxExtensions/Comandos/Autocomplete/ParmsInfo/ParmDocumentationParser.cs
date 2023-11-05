using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.ParmsInfo
{
    /// <summary>
    /// Extracts documentation for a parameter from the parm rule
    /// </summary>
    class ParmDocumentationParser
    {

        /// <summary>
        /// Parameter lines, without documentation
        /// </summary>
        List<string> Lines = new List<string>();

        /// <summary>
        /// Documentation of each line
        /// </summary>
        List<string> LinesDocumentation = new List<string>();

        public ParmDocumentationParser(string parmRule)
        {
            if (string.IsNullOrEmpty(parmRule))
                return;

            foreach (string line in parmRule.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                ProcessLine(line);
        }

        private void ProcessLine(string line)
        {
            int idxDoc = line.IndexOf("//");
            string lineContent, lineDoc;
            if (idxDoc >= 0) { 
                lineContent = line.Substring(0, idxDoc);
                lineDoc = line.Substring(idxDoc + 2).Trim();
            }
            else
            {
                lineContent = line;
                lineDoc = string.Empty;
            }
            lineContent = lineContent.Trim().ToLower();

            if( string.IsNullOrEmpty(lineContent) && Lines.Count > 0 && !string.IsNullOrEmpty(lineDoc))
                // This is documentation continuation for the previous line
                LinesDocumentation[LinesDocumentation.Count - 1] += " " + lineDoc;
            else
            {
                Lines.Add(lineContent);
                LinesDocumentation.Add(lineDoc);
            }
        }

        public string GetDocumentation(bool isAttribute, string parmName)
        {
            string textSearch;
            if (isAttribute)
                // Do not find "&name", just "name" (https://www.regular-expressions.info/lookaround.html)
                textSearch = @"(?<!\&)\b";
            else
                // Search with ampersand
                textSearch = @"\&";
            textSearch += parmName.ToLower();

            Regex regex = new Regex(textSearch + @"\b");
            for (int i = 0; i <Lines.Count;i++)
            {
                if (regex.IsMatch(Lines[i]))
                    return LinesDocumentation[i];
            }
            return string.Empty;
        }

    }
}
