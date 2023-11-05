using System;
using System.Collections.Generic;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Helper;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Framework.Commands;
using Artech.FrameworkDE;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Utilidades.Threading
{
    /// <summary>
    /// Command delegator with some common tools to do state queries and command
    /// executions
    /// </summary>
    public class CommandDelegatorBase : CommandDelegator
    {
        /// <summary>
        /// Habilita un comando solo si estamos dentro de una kbase.
        /// </summary>
        /// <param name="data">Comando a habilitar</param>
        /// <param name="status">Estado de habilitacion</param>
        /// <returns>Cierto si hemos indicado el estado del comando. 
        /// Falso si dejamos el trabajo a genexus</returns>
        protected bool QueryInKB(CommandData data, ref CommandStatus status)
        {
            status.State = CommandState.Disabled;
            if (Entorno.EstamosEnUnaKbase)
                status.State = CommandState.Enabled;
            return true;
        }

        /// <summary>
        /// Habilita un comando solo si estamos editando un objeto
        /// </summary>
        /// <param name="data">Comando a habilitar</param>
        /// <param name="status">Estado de habilitacion </param>
        /// <returns>Cierto si hemos indicado el estado del comando. 
        /// Falso si dejamos el trabajo a genexus</returns>
        protected bool QueryEnObjeto(CommandData data, ref CommandStatus status)
        {
            try
            {
                status.State = CommandState.Disabled;
                if (Entorno.EstamosEnObjeto)
                    status.State = CommandState.Enabled;
                return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Habilita un comando solo si estamos editando el source de un objeto
        /// </summary>
        /// <param name="data">Comando a habilitar</param>
        /// <param name="status">Estado de habilitacion </param>
        /// <returns>Cierto si hemos indicado el estado del comando. 
        /// Falso si dejamos el trabajo a genexus</returns>
        protected bool QueryEnParteSource(CommandData data, ref CommandStatus status)
        {
            try
            {
                status.State = CommandState.Disabled;
                if (Entorno.EstamosEnSourceDeObjeto)
                    status.State = CommandState.Enabled;
                return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Habilita un comando solo si estamos en un objeto con winform (workpanel / transaction)
        /// </summary>
        /// <param name="data">Comando a habilitar</param>
        /// <param name="status">Estado de habilitacion </param>
        /// <returns>Cierto si hemos indicado el estado del comando. 
        /// Falso si dejamos el trabajo a genexus</returns>
        protected bool QueryEnWinForm(CommandData data, ref CommandStatus status)
        {
            try
            {
                status.State = CommandState.Disabled;
                if (Entorno.EstamosEnWinform)
                    status.State = CommandState.Enabled;
                return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Enable a command only if we are editing a workpanel
        /// </summary>
        /// <param name="data">Command to enable</param>
        /// <param name="status">Command state</param>
        /// <returns>True if this function has changed the command status.</returns>
        protected bool QueryAtWorkpanel(CommandData data, ref CommandStatus status)
        {
            try
            {
                status.State = CommandState.Disabled;
                if (Entorno.AtWorkpanel)
                    status.State = CommandState.Enabled;
                return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Habilita un comando solo si en el editor actual tenemos seleccionado un objeto llamable (
        /// procedimientos, transacciones, etc.)
        /// </summary>
        /// <param name="data">Comando a habilitar</param>
        /// <param name="status">Estado de habilitacion </param>
        /// <returns>Cierto si hemos indicado el estado del comando. 
        /// Falso si dejamos el trabajo a genexus</returns>
        protected bool QueryObjetoLlamableSeleccionado(CommandData data, ref CommandStatus status)
        {
            try
            {
                status.State = CommandState.Disabled;
                if (Entorno.ObjetoLlamableActualmenteSeleccionado)
                    status.State = CommandState.Enabled;
                return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Habilita un comando solo si en el editor actual tenemos seleccionado una transaccion
        /// </summary>
        /// <param name="data">Command to enable</param>
        /// <param name="status">Command status</param>
        /// <returns>True if the command state was set by this funcion</returns>
        protected bool QueryTransaccionSeleccionada(CommandData data, ref CommandStatus status)
        {
            try
            {
                status.State = CommandState.Disabled;
                if (Entorno.EstamosEnTransaccion)
                    status.State = CommandState.Enabled;
                return true;
            }
            catch { }

            return false;
        }

        protected bool QueryAlwaysEnabled(CommandData data, ref CommandStatus status)
        {
            status.State = CommandState.Enabled;

            return true;
        }

        /// <summary>
        /// Command query to check if there a selected objects on the navigator window
        /// </summary>
        /// <param name="data">Command to enable</param>
        /// <param name="status">Command status</param>
        /// <returns>True if the command state was set by this funcion</returns>
        protected bool QueryNavigatorSomethigSelected(CommandData data, ref CommandStatus status)
        {
            try
            {
                status.State = CommandState.Disabled;
                List<KBObject> selection = Entorno.NavigatorSelectedObjects;
                if (selection != null && selection.Count > 0)
                    status.State = CommandState.Enabled;
                return true;
            }
            catch 
            {
                return false;
            }
        }

        /// <summary>
        /// Ejecuta un comando que implemente la interface IExecutable
        /// </summary>
        /// <typeparam name="T">Tipo del comando a ejecutar</typeparam>
        /// <param name="commandData">Datos del comando</param>
        /// <returns>Cierto si la extension ha ejecutado el comando</returns>
        public bool ExecuteCommand<T>(CommandData commandData) where T : IExecutable, new()
        {
            try
            {
                // Ejecutar el comando
                new T().Execute();
                return true;
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                return false;
            }
        }

        /// <summary>
        /// It shows and focuses a singleton toolwindow
        /// </summary>
        /// <typeparam name="T">Toolwindow class to show.</typeparam>
        /// <param name="commandData">Command execution parameters</param>
        /// <returns>True if the toolwindow can be show</returns>
        public bool ShowToolWindow<T>(CommandData commandData)
        {
            try
            {
                Type toolWindowType = typeof(T);
                return UIServices.ToolWindows.FocusToolWindow(toolWindowType.GUID);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                return false;
            }
        }

        public bool CreateToolWindowInstance<T>(CommandData commandData)
        {
            try
            {
                Type toolWindowType = typeof(T);
                AbstractToolWindow tw = (AbstractToolWindow)Activator.CreateInstance(toolWindowType);
                UIServices.ToolWindows.AddMdiToolWindow(tw, true);
                return UIServices.ToolWindows.FocusToolWindow(tw.Id);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                return false;
            }
        }

    }
}
