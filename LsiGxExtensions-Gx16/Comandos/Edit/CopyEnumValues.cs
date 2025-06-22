using Artech.Architecture.Common.Objects;
using Artech.Common.Framework.Commands;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LSI.Packages.Extensiones.Comandos.Edit
{
	/// <summary>
	/// Copy selected domain values to clipboard
	/// </summary>
	public class CopyEnumValues
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
                status.State = commandData.LsiSelectedObjects<Domain>().Any() ?
					CommandState.Enabled :
					CommandState.Disabled;
				return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Copy selected domain values to clipboard
        /// </summary>
        /// <param name="commandData">Command data</param>
        /// <returns>True if the command has been executed (?)</returns>
        static public bool Execute(CommandData commandData)
		{
            try
            {
				Domain domain = commandData.LsiSelectedObjects<Domain>().FirstOrDefault();
                if (domain == null)
                    return false;
                EnumValues enumValues = domain.GetPropertyValue<EnumValues>(Properties.ATT.EnumValues);
                if (enumValues == null)
                    return false;

                string[] columns = { "Name", "Descripcion", "Value" };
                ClipboardTable table = new ClipboardTable(columns);
                List<string> row = new List<string>();
				foreach(EnumValue enumValue in enumValues.Values)
				{
                    row.Add(enumValue.Name);
                    row.Add(enumValue.Description);
                    row.Add(enumValue.Value);
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
