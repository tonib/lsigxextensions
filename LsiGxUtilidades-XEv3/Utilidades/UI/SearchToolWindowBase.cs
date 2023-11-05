using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Utilidades.UI
{
    /// <summary>
    /// Base for Tool Windows to search generic things (kbobjects, or files)
    /// </summary>
    public class SearchToolWindowBase : ToolWindowBase
    {
        /// <summary>
        /// Background search process
        /// </summary>
        protected BackgroundWorker BackgroundSearchProcess = new BackgroundWorker();

        /// <summary>
        /// Toolwindow search button
        /// </summary>
        protected Button TWSearchButton;

        /// <summary>
        /// Busy Gif picture
        /// </summary>
        protected PictureBox TWBusyPicture;

        /// <summary>
        /// Label with current state
        /// </summary>
        protected Label TWStatusLabel;

        /// <summary>
        /// Tool window object found grid
        /// </summary>
        protected DataGridView TWGrid;

        /// <summary>
        /// Start search time
        /// </summary>
        protected DateTime SearchStart;

        /// <summary>
        /// Constructor
        /// </summary>
        public SearchToolWindowBase()
        {
            // Preparar la busqueda en segundo plano.
            BackgroundSearchProcess.DoWork += new DoWorkEventHandler(BackgroundSearchProcess_RunSearch);
            BackgroundSearchProcess.ProgressChanged += new ProgressChangedEventHandler(BackgroundSearchProcess_ProgressChanged);
            BackgroundSearchProcess.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundSearchProcess_RunWorkerCompleted);
            BackgroundSearchProcess.WorkerSupportsCancellation = true;
            BackgroundSearchProcess.WorkerReportsProgress = true;
        }

        protected void InitializeSearchToolWindow(Button twSearchButton, PictureBox twBusyPicture, Label twStatusLabel,
            DataGridView twGrid)
        {
            TWSearchButton = twSearchButton;
            TWBusyPicture = twBusyPicture;
            TWStatusLabel = twStatusLabel;
            TWGrid = twGrid;

            // Initialize status label
            TWStatusLabel.Text = "";

            // Initialize busy picture
            TWBusyPicture.Enabled = TWBusyPicture.Visible = false;
        }

        /// <summary>
        /// Enable / disable all UI fileds
        /// </summary>
        /// <param name="enabled">True to enable all fields. False to disable</param>
        protected virtual void EnableUIFields(bool enabled) { }

        /// <summary>
        /// Creates the object to run the search
        /// </summary>
        /// <returns>The finder object. null if there was problems and the run cannot be started</returns>
        protected virtual UISearchBase CreateFinder()
        {
            return null;
        }

        protected bool CancelProcess()
        {
            if (BackgroundSearchProcess.IsBusy)
            {
                // Cancel button was ressed:
                BackgroundSearchProcess.CancelAsync();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// The toolwindow Grid list
        /// </summary>
        protected System.Collections.IList GridList
        {
            get { return (System.Collections.IList)TWGrid.DataSource; }
        }

        /// <summary>
        /// Search / Cancel button pressed
        /// </summary>
        protected void SearchButtonPressed()
        {
            try
            {
                if (CancelProcess())
                    return;

                UISearchBase finder = CreateFinder();
                if (finder == null)
                    return;
                finder.BackgroundSearch = BackgroundSearchProcess;

                // Lanzar la busqueda
                TWSearchButton.Text = "Cancel";
                EnableUIFields(false);
                GridList.Clear();
                TWStatusLabel.Text = "Searching...";
                TWBusyPicture.Enabled = TWBusyPicture.Visible = true;
                SearchStart = DateTime.Now;
                BackgroundSearchProcess.RunWorkerAsync(finder);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// Background search process
        /// </summary>
        protected virtual void BackgroundSearchProcess_RunSearch(object sender, DoWorkEventArgs e)
        {
            try
            {
                // Obtener el objeto que ejecutar la busqueda
                UISearchBase buscador = (UISearchBase)e.Argument;
                buscador.ExecuteUISearch();
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// Background process reported some result
        /// </summary>
        protected virtual void BackgroundSearchProcess_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is Exception)
                Log.ShowException((Exception)e.UserState);
            else if (e.UserState is string)
                TWStatusLabel.Text = (string)e.UserState;
            else
                GridList.Add(e.UserState);
        }

        /// <summary>
        /// Background search finished
        /// </summary>
        protected virtual void BackgroundSearchProcess_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DateTime searchEnd = DateTime.Now;
            TimeSpan searchTime = searchEnd.Subtract(SearchStart);
            TWStatusLabel.Text = "N. found: " + GridList.Count +
                " / Search time: " + Math.Round(searchTime.TotalSeconds, 2) + " seconds";
            TWSearchButton.Text = "&Search";
            EnableUIFields(true);
            TWBusyPicture.Enabled = TWBusyPicture.Visible = false;
            SearchFinished();
        }

        /// <summary>
        /// Override this to perform some action when the search is finished
        /// </summary>
        protected virtual void SearchFinished()
        {
        }

        public override void Reset()
        {
            CancelProcess();
            base.Reset();
        }

    }
}
