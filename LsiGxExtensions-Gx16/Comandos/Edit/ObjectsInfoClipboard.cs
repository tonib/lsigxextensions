using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Artech.Architecture.Common.Objects;
using Artech.Common.Framework.Commands;
using Artech.Common.Framework.Selection;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.UI;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Comandos.Edit
{
    /// <summary>
    /// Command to copy selected objects information as a table to the clipboard
    /// </summary>
    public class ObjectsInfoClipboard
    {
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
                if (commandData.LsiSelectedObjects<KBObject>().Any())
                    status.State = CommandState.Enabled;
                else
                    status.State = CommandState.Disabled;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Copy selected objects info to the clipboard as a table
        /// </summary>
        /// <param name="commandData">Command data</param>
        /// <returns>True if the command has been executed (?)</returns>
        static public bool CopyObjectsAsTable(CommandData commandData)
        {
            try
            {
                // Get selected obejcts and do the copy table
                string[] columns = { "Name", "Type" , "Description", "Folder" , "Modified Date" , 
                                       "Last User" };
                ClipboardTable table = new ClipboardTable( columns );
                List<string> row = new List<string>();
                foreach (KBObject o in commandData.LsiSelectedObjects<KBObject>())
                {
                    row.Add(o.Name);
                    row.Add(o.TypeDescriptor.Name);
                    row.Add(o.Description);
                    row.Add(o.Parent != null ? o.Parent.Name : string.Empty);
                    row.Add(o.LastUpdate.ToLocalTime().ToString());
                    row.Add(o.User != null ? o.User.Name : string.Empty);
                    table.AddRow(row);
                    row.Clear();
                }
                Clipboard.SetDataObject(table.GetClipboardContent());

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
