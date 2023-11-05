using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace LSI.Packages.Extensiones.Utilidades.UI
{
    /// <summary>
    /// Tool to copy a table into the clipboard
    /// </summary>
    public class ClipboardTable
    {

        /// <summary>
        /// The table header
        /// </summary>
        private List<string> TableHeader;

        /// <summary>
        /// Table rows
        /// </summary>
        private List<List<string>> Rows = new List<List<string>>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tableHeader">The table header</param>
        public ClipboardTable(IEnumerable<string> tableHeader)
        {
            TableHeader = tableHeader.ToList();
        }

        /// <summary>
        /// Add a row to the table
        /// </summary>
        /// <param name="tableRow">The table row</param>
        public void AddRow(IEnumerable<string> tableRow)
        {
            Rows.Add(tableRow.ToList());
        }

        /// <summary>
        /// Get the table data object to copy to the clipboard
        /// </summary>
        /// <returns></returns>
        public DataObject GetClipboardContent()
        {
            DataObject dataObject = new DataObject();
            dataObject.SetData(DataFormats.Html, CopyHtmlToClipBoard(HtmlTable));
            dataObject.SetData(DataFormats.Text, TextTable);
            return dataObject;
        }

        private void TextRow(StringBuilder sb, List<string> row)
        {
            int last = row.Count - 1;
            for( int i=0; i < row.Count; i++)
            {
                sb.Append(row[i]);
                if (i < last)
                    sb.Append('\t');
            }
        }

        private string TextTable
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                TextRow(sb, TableHeader);
                sb.Append(Environment.NewLine);

                foreach (List<string> row in Rows)
                {
                    TextRow(sb, row);
                    sb.Append(Environment.NewLine);
                }
                return sb.ToString();
            }
        }

        private void HtmlRow(StringBuilder sb, List<string> row, string openCell, string closeCell)
        {
            sb.Append("<tr>");
            foreach (string cell in row)
            {
                sb.Append(openCell);
                sb.Append(HttpUtility.HtmlEncode(cell));
                sb.Append(closeCell);
            }
            sb.Append("</tr>");
        }

        private string HtmlTable
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<table>");
                HtmlRow(sb, TableHeader, "<th><b>", "</b></th>");
                foreach (List<string> row in Rows)
                    HtmlRow(sb, row, "<td>", "</td>");
                sb.Append("</table>");

                return sb.ToString();
            }
        }

        /// <summary>
        /// Get html text formated for the Windows clipboard
        /// </summary>
        /// <remarks>Copied from http://www.tcx.be/blog/2005/copy-html-to-clipboard/</remarks>
        /// <param name="html">Html fragment to put in the clipboard</param>
        /// <returns>The data for the clipboard DataObject</returns>
        private static MemoryStream CopyHtmlToClipBoard(string html)
        {
            Encoding enc = Encoding.UTF8;

            string begin = "Version:0.9\r\nStartHTML:{0:000000}\r\nEndHTML:{1:000000}"
              + "\r\nStartFragment:{2:000000}\r\nEndFragment:{3:000000}\r\n";

            string html_begin = "<html>\r\n<head>\r\n"
              + "<meta http-equiv=\"Content-Type\""
              + " content=\"text/html; charset=" + enc.WebName + "\">\r\n"
              + "<title>HTML clipboard</title>\r\n</head>\r\n<body>\r\n"
              + "<!--StartFragment-->";

            string html_end = "<!--EndFragment-->\r\n</body>\r\n</html>\r\n";

            string begin_sample = String.Format(begin, 0, 0, 0, 0);

            int count_begin = enc.GetByteCount(begin_sample);
            int count_html_begin = enc.GetByteCount(html_begin);
            int count_html = enc.GetByteCount(html);
            int count_html_end = enc.GetByteCount(html_end);

            string html_total = String.Format(
              begin
              , count_begin
              , count_begin + count_html_begin + count_html + count_html_end
              , count_begin + count_html_begin
              , count_begin + count_html_begin + count_html
              ) + html_begin + html + html_end;

            //DataObject obj = new DataObject();
            //obj.SetData(DataFormats.Html, new MemoryStream(enc.GetBytes(html_total)));
            //Clipboard.SetDataObject(obj, true);
            return new MemoryStream(enc.GetBytes(html_total));
        }
    }
}
