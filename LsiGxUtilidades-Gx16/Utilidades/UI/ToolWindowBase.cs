using System;
using System.Collections;
using System.Linq;
using System.Text;
using Artech.FrameworkDE;
using Artech.Architecture.Common.Objects;
using System.Drawing;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Framework.Commands;
using Artech.Architecture.UI.Framework.Commands;
using System.Windows.Forms;

namespace LSI.Packages.Extensiones.Utilidades.UI
{

    /// <summary>
    /// An AbstractToolWindow base with some utilities
    /// </summary>
    public class ToolWindowBase : AbstractToolWindow
    {

        /// <summary>
        /// Command name to copy selected objects as table to the clipboard
        /// </summary>
        public const string COPYOBJECTSASTABLECMD = "CopyObjectsAsTable";

        public ToolWindowBase()
        {
            // Standard genexus color for toolwindows
            BackColor = Color.White;
        }

        /// <summary>
        /// Do not show lsi toolwindows in view gx menu
        /// </summary>
        public override bool ShowInViewMenu
        {
            get { return false; }
        }

        /// <summary>
        /// Selecciona un objeto en genexus
        /// </summary>
        /// <param name="objeto">El objeto a seleccionar</param>
        public void SeleccionarObjeto(object objeto)
        {
            SetSelection(objeto, objeto);
        }

        /// <summary>
        /// Selecciona una lista de objetos en genexus
        /// </summary>
        /// <param name="objetos">La lista de objetos a seleccionar</param>
        public void SeleccionarObjetos(ICollection objetos)
        {
            SetSelection(objetos, objetos);
        }

        public override void Reset()
        {
            base.Reset();
            UIServices.ToolWindows.CloseToolWindow(Id);
        }

        // http://stackoverflow.com/questions/435433/what-is-the-preferred-way-to-find-focused-control-in-winforms-app
        public Control FindFocusedControl()
        {
            Control control = this;
            IContainerControl container = control as IContainerControl;
            while (container != null)
            {
                control = container.ActiveControl;
                container = control as IContainerControl;
            }
            return control;
        }

        /// <summary>
        /// Updates clipboard commands states if a GridObjectos has the focus
        /// </summary>
        /// <param name="cmdKey">The command key</param>
        /// <param name="commandData">The command parameters</param>
        /// <param name="status">The command status</param>
        /// <returns>True if the status has been updated (?)</returns>
        public override bool QueryState(CommandKey cmdKey, CommandData commandData, ref CommandStatus status)
        {
            // We only handle commands over our customized grids:
            GridObjetos grid = FindFocusedControl() as GridObjetos;
            if (grid == null)
                return base.QueryState(cmdKey, commandData, ref status);

            if (cmdKey == CommandKeys.Core.Cut || cmdKey == CommandKeys.Core.Paste)
            {
                // Not appliable commands:
                status.State = CommandState.Invisible;
                return true;
            }

            if (cmdKey == CommandKeys.Core.Copy || cmdKey.Name == COPYOBJECTSASTABLECMD ||
                cmdKey == CommandKeys.Core.Delete)
            {
                status.Enable(SelectedObjects != null && SelectedObjects.Count > 0);
                return true;
            }

            return base.QueryState(cmdKey, commandData, ref status);
        }

        /// <summary>
        /// Execute clipboard commands
        /// </summary>
        /// <param name="cmdKey">The command key</param>
        /// <param name="commandData">The command parameters</param>
        /// <returns>True if the command has been executed (?)</returns>
        public override bool Exec(CommandKey cmdKey, CommandData commandData)
        {
            // We only handle commands over our customized grids:
            GridObjetos grid = FindFocusedControl() as GridObjetos;
            if (grid == null)
                return base.Exec(cmdKey, commandData);

            if (cmdKey == CommandKeys.Core.Copy)
            {
                if (SelectedObjects != null && SelectedObjects.Count > 0)
                {
                    UIServices.Clipboard.SetData(SelectedObjects);
                    return true;
                }
            }

            // TODO: Here we should check the package guid too...
            if (cmdKey.Name == COPYOBJECTSASTABLECMD)
            {
                DataObject o = grid.GetClipboardContent();
                Clipboard.SetDataObject(o);
                return true;
            }

            if (cmdKey == CommandKeys.Core.Delete)
            {
                grid.DeleteSelectedObjects();
                return true;
            }

            return base.Exec(cmdKey, commandData);
        }
    }
}
