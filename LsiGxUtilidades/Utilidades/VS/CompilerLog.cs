using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.CSharpWin;
using System.IO;
using System.Web;

namespace LSI.Packages.Extensiones.Utilidades.VS
{
    /// <summary>
    /// C# compiler log output parser
    /// </summary>
    public class CompilerLog
    {

        /// <summary>
        /// Label for module compilation
        /// </summary>
        private const string BUILDING = @"Building bin\";

        /// <summary>
        /// Log text. Windows break lines (\r\n) are changed to \n only
        /// </summary>
        private string LogText = string.Empty;

        /// <summary>
        /// Indices to break line characters into LogText
        /// </summary>
        private List<int> BreakLinesIdxs = new List<int>();


        /// <summary>
        /// Constructor
        /// </summary>
        public CompilerLog() : this(string.Empty)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logText">Log raw text</param>
        public CompilerLog(string logText)
        {
            UpdateLogText(logText);
        }

        /// <summary>
        /// Number of text lines into the log. If the log is empty it will return 1: One empty line
        /// </summary>
        public int LinesCount
        {
            get { return BreakLinesIdxs.Count + 1; }
        }

        private void SearchBreakLines(int startIdx)
        {
            int idx;
            while ((idx = LogText.IndexOf('\n', startIdx)) >= 0)
            {
                BreakLinesIdxs.Add(idx);
                startIdx = idx + 1;
            }
        }

        /// <summary>
        /// Replaces the current log text
        /// </summary>
        /// <param name="logText">New raw log text</param>
        public void UpdateLogText(string logText)
        {
            LogText = string.Empty;
            BreakLinesIdxs.Clear();
            AppendLogText(logText);
        }

        /// <summary>
        /// It appends text to the current log text
        /// </summary>
        /// <param name="logText">Text to append</param>
        public void AppendLogText(string logText)
        {
            // We will use \n ONLY
            logText = logText.Replace("\r\n", "\n");
            int idxStart = LogText.Length;
            LogText += logText;
            SearchBreakLines(idxStart);
        }

        /// <summary>
        /// Get the next error line index
        /// </summary>
        /// <param name="startFrom">Initial line index to serch. == -1 to search from the start</param>
        /// <returns>Next error line index. -1 if there are no more errors</returns>
        public int NextErrorLineIdx(int startFrom)
        {
            if (startFrom < -1)
                startFrom = -1;

            startFrom++;
            int end = LinesCount - 1;
            for (int i = 0; i < end; i++)
            {
                int idx = (startFrom + i) % LinesCount;
                if( CompilerError.ContainsError(GetLine(idx)) )
                    return idx;
            }
            return -1;
        }

        /// <summary>
        /// Get error lines indices
        /// </summary>
        /// <returns>Error line indices</returns>
        public List<int> GetErrorLinesIdx(int startLineIdx)
        {
            if (startLineIdx < 0)
                startLineIdx = 0;

            List<int> indices = new List<int>();
            for (int i = startLineIdx; i < LinesCount; i++)
            {
                if (CompilerError.ContainsError(GetLine(i)))
                    indices.Add(i);
            }
            return indices;
        }

        /// <summary>
        /// True if the log contains errors
        /// </summary>
        public bool ContainsAnyError
        {
            get { return ContainsErrors(-1); }
        }

        public bool ContainsErrors(int startLineIdx)
        {
            return NextErrorLineIdx(startLineIdx-1) > 0;
        }

        /// <summary>
        /// Get a log text line
        /// </summary>
        /// <param name="nLine">Line index</param>
        /// <returns>Text of the line</returns>
        public string GetLine(int nLine)
        {
            // Get the line start idx
            int startIdx;
            int breakLineIdx = nLine - 1;
            if (breakLineIdx < 0)
                startIdx = 0;
            else
                startIdx = BreakLinesIdxs[breakLineIdx] + 1;

            // Get the line end index
            int endIndex;
            breakLineIdx = nLine;
            if (breakLineIdx >= BreakLinesIdxs.Count)
                endIndex = LogText.Length - 1;
            else
                endIndex = BreakLinesIdxs[breakLineIdx] - 1;

            int length = endIndex - startIdx + 1;
            return LogText.Substring(startIdx, length);
        }

        /// <summary>
        /// Get the index into the log text of the first character of a line
        /// </summary>
        /// <param name="lineIdx">Line index</param>
        /// <returns>The character index</returns>
        public int GetFirstCharIndexFromLine(int lineIdx)
        {
            int breakLineIdx = lineIdx - 1;
            if (breakLineIdx < 0)
                return 0;
            else
                return BreakLinesIdxs[breakLineIdx] + 1;
        }

        /// <summary>
        /// Get the line index for a character index into the log text.
        /// </summary>
        /// <param name="charIdx">Character index</param>
        /// <returns>Line index. -1 if the character index is outside the log text</returns>
        public int GetLineIdxFromCharIdx(int charIdx)
        {
            if (BreakLinesIdxs.Count == 0)
                return 0;

            for (int i = 0; i < BreakLinesIdxs.Count; i++)
            {
                if (charIdx <= BreakLinesIdxs[i])
                    return i;
            }
            return BreakLinesIdxs.Count;
        }

        /// <summary>
        /// Get the error of a line
        /// </summary>
        /// <param name="lineIdx">Line index</param>
        /// <returns>The error line. null if the line does not contain an error</returns>
        public CompilerError GetLineError(int lineIdx)
        {
            return CompilerError.ParseCompilerErrorMessage(GetLine(lineIdx));
        }

        /// <summary>
        /// Check if a log line contains an error
        /// </summary>
        /// <param name="lineIdx">Line index</param>
        /// <returns>True if the line contains an error</returns>
        public bool LineContainsError(int lineIdx)
        {
            return GetLineError(lineIdx) != null;
        }

        private string ParseModuleName(string line)
        {
            if (line.StartsWith(BUILDING))
            {
                line = line.Substring(BUILDING.Length);
                int pointIdx = line.IndexOf(".");
                if (pointIdx < 0)
                    return null;
                return line.Substring(0, pointIdx );
            }
            return null;
        }

        /// <summary>
        /// Get log lines
        /// </summary>
        public IEnumerable<string> LogLines
        {
            get
            {
                for (int i = 0; i < LinesCount; i++)
                    yield return GetLine(i);
            }
        }

        /// <summary>
        /// Get the list of source files not found errors by module
        /// </summary>
        /// <returns>The key is the main object name, and the value is the set of objects
        /// not found</returns>
        public Dictionary<string, HashSet<string>> GetSourcesNotFoundByModule()
        {
            Dictionary<string, HashSet<string>> errors = new Dictionary<string, HashSet<string>>();
            string currentModule = string.Empty;
            foreach (string line in LogLines)
            {
                string module = ParseModuleName(line);
                if (module != null)
                {
                    // Module changed:
                    currentModule = module;
                    continue;
                }

                CompilerError error = CompilerError.ParseCompilerErrorMessage(line);
                if (error == null)
                    continue;

                if (error.IsNameNotFoundError)
                {
                    // Check if the wrong module is the current or Commons
                    string errorModule = currentModule;
                    if (error.NameNotFound.StartsWith("type_Sdt"))
                        errorModule = CSharpUtils.COMMONSNAME;

                    // Process the error:
                    HashSet<string> notFoundNames;
                    if (!errors.TryGetValue(errorModule, out notFoundNames))
                    {
                        notFoundNames = new HashSet<string>();
                        errors.Add(errorModule, notFoundNames);
                    }
                    notFoundNames.Add(error.NameNotFound);

                }
            }
            return errors;
        }

        /// <summary>
        /// Encode the log to an HTML version
        /// </summary>
        /// <param name="sourcesDirectory">Directory path that contains source files</param>
        /// <returns>HTML version of the log</returns>
        public string ToHtml(string sourcesDirectory)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string line in LogLines)
            {
                string modifiedLine = HttpUtility.HtmlEncode(line) + "<br/>";

                // Check line errors
                if (CompilerError.ContainsError(modifiedLine))
                {
                    // Try to parse the error text
                    CompilerError error = CompilerError.ParseCompilerErrorMessage(line);
                    if (error != null && error.FileName != null)
                    {
                        string filePath = Path.Combine(sourcesDirectory, error.FileName);
                        modifiedLine = modifiedLine.Replace(error.FileName,
                            string.Format("<a href=\"{0}\">{1}</a>", filePath, error.FileName));
                    }

                    // Highlight the error text
                    modifiedLine = "<span style=\"background-color: orange\">" +
                        modifiedLine + "</span>";
                }

                sb.AppendLine(modifiedLine);
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            return LogText + Environment.NewLine + "(" + LinesCount + " lines)";
        }
    }
}
