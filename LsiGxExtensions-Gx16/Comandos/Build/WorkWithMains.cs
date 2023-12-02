using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Artech.Architecture.Common.Events;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Framework.Commands;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Entities;
using Artech.Genexus.Common.Services;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.UI;
using Microsoft.Practices.CompositeUI.EventBroker;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos.Build
{
    /// <summary>
    /// Work with mains toolwindow
    /// </summary>
    [Guid("5939F42F-3672-494a-8B9F-49B99D59C9C3")]
    public partial class WorkWithMains : ToolWindowBase , ICommandTarget
    {

        private const int ShowMaximized = 3;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("User32.dll", EntryPoint = "SetForegroundWindow")]
        private static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Systray to display build notifications
        /// </summary>
        public NotifyIcon BuildNotifyIcon;

        /// <summary>
        /// Constructor
        /// </summary>
        public WorkWithMains()
        {
            InitializeComponent();

            this.Icon = Resources.WindowsForm_817;

            UpdateSplitState();

            // Notify icon initialization
            BuildNotifyIcon = new NotifyIcon();
            BuildNotifyIcon.Text = "GX build";
            BuildNotifyIcon.Click += new EventHandler(BuildNotifyIcon_Clicked);
            BuildNotifyIcon.BalloonTipClicked += new EventHandler(BuildNotifyIcon_Clicked);
            UpdateNotifyIcon();

            UIServices.KB.CurrentModelChanged += new ModelEventHandler(KB_CurrentModelChanged);
        }

        void BuildNotifyIcon_Clicked(object sender, EventArgs e)
        {
            try
            {
                SetForegroundWindow(UIServices.Environment.MainWindow.Handle);
                ShowWindow(UIServices.Environment.MainWindow.Handle, ShowMaximized);
                UIServices.ToolWindows.FocusToolWindow(typeof(WorkWithMains).GUID);
            }
            catch { }
        }

        public void DisplayNotification(string title, string message, ToolTipIcon icon)
        {
            BuildNotifyIcon.Visible = true;
            BuildNotifyIcon.ShowBalloonTip(30000, title, message, icon);
        }

        /// <summary>
        /// Updates the notify icon status from the current builds status
        /// </summary>
        public void UpdateNotifyIcon()
        {
            List<BuildProcess> processes = CurrentBuilds;
            if (processes.Count == 0)
            {
                BuildNotifyIcon.Visible = false;
                return;
            }

            BuildNotifyIcon.Visible = true;
            if( processes.Any(x => x.BuildWithErrors) ) 
            {
                BuildNotifyIcon.Icon = Resources.Error;
                return;
            }
            if (processes.Any(x => !x.LogControl.ProcessFinished))
            {
                BuildNotifyIcon.Icon = Resources.Processing;
                return;
            }
            BuildNotifyIcon.Icon = Resources.Ok;
        }

        /// <summary>
        /// Builds with a tab open on this toolwindow
        /// </summary>
        public List<BuildProcess> CurrentBuilds
        {
            get
            {
                List<BuildProcess> processes = new List<BuildProcess>();
                foreach (TabPage tab in TabBuilds.TabPages)
                {
                    try
                    {
                        BuildLogControl buildControl = tab.Controls[0] as BuildLogControl;
                        if (buildControl == null)
                            continue;
                        processes.Add(buildControl.Process);
                    }
                    catch { }
                }
                return processes;
            }
        }

        /// <summary>
        /// Reload mains button clicked
        /// </summary>
        private void BtnReloadMains_Click(object sender, EventArgs e)
        {
            ReloadMains();
        }

        static private TabPage CreateTabPageForControl(Control control, string title)
        {
            TabPage page = new TabPage(title);
            page.Width = 100;
            page.Height = 100;

            page.Controls.Add(control);
            control.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            control.Width = 100;
            control.Height = 100;

            return page;
        }

        /// <summary>
        /// Reloads main lists content
        /// </summary>
        private void ReloadMains()
        {
            try
            {

                TxtTargetModel.Text = string.Empty;
                TabGenerators.TabPages.Clear();

                if (!UIServices.IsKBAvailable)
                    return;

                TxtTargetModel.Text = Entorno.TargetDirectory;

                // Get mains for each generator
#if GX_17_OR_GREATER
                Dictionary<GxGenerator, List<KBObject>> mainsByGenerator = MainsGx.GetMainsByGenerator();
#else
                Dictionary<GxEnvironment, List<KBObject>> mainsByGenerator = MainsGx.GetMainsByGenerator();
#endif

                // Create tab for each generator, with the same generation declaration order
                GxModel gxModel = UIServices.KB.WorkingEnvironment.TargetModel.GetAs<GxModel>();
#if GX_17_OR_GREATER
                foreach (GxGenerator environment in gxModel.Generators) {
#else
                foreach (GxEnvironment environment in gxModel.Environments) {
#endif
                    List<KBObject> mains;
                    if( mainsByGenerator.TryGetValue(environment, out mains) )
                    {
                        GeneratorMains generatorTab = new GeneratorMains(this, environment, mains);
                        TabPage tabPage = CreateTabPageForControl(generatorTab, environment.Description);
                        TabGenerators.TabPages.Add(tabPage);
                    }
                }

                FocusCurrentMainsGrid();
            }
            catch { }
        }

        /// <summary>
        /// Set focus on the current mains grid
        /// </summary>
        private void FocusCurrentMainsGrid()
        {
            // Set focus on the current main objects list
            GeneratorMains tab = SelectedGeneratorTab;
            if (tab != null)
                tab.GrdMains.Select();
        }

        /// <summary>
        /// The currently selected mains list
        /// </summary>
        public GridObjetos CurrentMainsList
        {
            get
            {
                GeneratorMains panel = null;
                if (TabGenerators.SelectedTab == null)
                    return null;
                if (TabGenerators.SelectedTab.Controls.Count > 0)
                    panel = TabGenerators.SelectedTab.Controls[0] as GeneratorMains;
                if( panel == null )
                    return null;
                return panel.GrdMains;
            }
        }

        /// <summary>
        /// Generator tab changed
        /// </summary>
        private void TabGenerators_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CurrentMainsList == null)
                return;
            CurrentMainsList.UpdateToolWindowSelection();
            CurrentMainsList.Focus();

            // There is a issue with menus inside tabcontrols. The shorcuts are always attached
            // to the first tabpage, and not the current. So, here is the crap:
            // Disable all menu menus:
            GeneratorPanels.ForEach(x => x.DettachMenuShortcuts());
            // Enable the current generator panel menu keys
            SelectedGeneratorTab.UpdateMenu();
        }

        public void UpdateSplitState()
        {
            Split.Panel2Collapsed = (TabBuilds.Controls.Count == 0);
        }

        public void RemoveBuilTab(TabPage buildTab)
        {
            TabBuilds.Controls.Remove(buildTab);
            UpdateSplitState();
            UpdateNotifyIcon();
        }

        /// <summary>
        /// KB closed event handler
        /// </summary>
        [EventSubscription(ArchitectureEvents.AfterCloseKB)]
        private void AfterCloseKB(object sender, EventArgs e)
        {
            ReloadMains();
        }

        /// <summary>
        /// Current model changed event handler
        /// </summary>
        /// <remarks>
        /// This should be executed whitin AfterOpenKb, but when this event is executed, there is no
        /// current model defined.
        /// </remarks>
        private void KB_CurrentModelChanged(KBModel previousModel, KBModel model)
        {
            ReloadMains();
        }

        /// <summary>
        /// Environment changed event
        /// </summary>
        [EventSubscription(ArchitectureEvents.KBEnvironmentChanged)]
        public void OnKBEnvironmentChanged(object sender, KBEnvironmentEventArgs args)
        {
            ReloadMains();
        }

        /// <summary>
        /// Target model link clicked
        /// </summary>
        private void LnkTargetModel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(TxtTargetModel.Text);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// The selected generator tab. null if there is no generator
        /// </summary>
        private GeneratorMains SelectedGeneratorTab
        {
            get
            {
                try
                {
                    return TabGenerators.SelectedTab?.Controls[0] as GeneratorMains;
                }
                catch
                {
                    return null;
                }
            }
        }

        private List<GeneratorMains> GeneratorPanels 
        {
            get
            {
                return TabGenerators.TabPages
                    .Cast<TabPage>()
                    .Select(x => x.Controls[0] as GeneratorMains)
                    .ToList();
            }
        }

        internal void UpdateCompilationDates()
        {
            GeneratorPanels.ForEach(x => {
                x.UpdateCompilationDates();
                x.GrdMains.Refresh();
            });
        }

        /// <summary>
        /// User control created. It sets the focus on the current generator objects grid
        /// </summary>
        private void WorkWithMains_Load(object sender, EventArgs e)
        {
            FocusCurrentMainsGrid();
        }

        /// <summary>
        /// The build that is running some internal gx build functions. null if there are no
        /// build running any gx build function
        /// </summary>
        public BuildProcess CurrentRunningGxBuild
        {
            get { return CurrentBuilds.FirstOrDefault(x => x.LogControl.Worker.IsBusy && x.IsInternalGxBuild); }
        }

        /// <summary>
        /// It starts a build process.
        /// </summary>
        /// <remarks>
        /// This function avoids run more than one gx internal build concurrently
        /// </remarks>
        /// <param name="generatorTab">Generator tab owner of this task</param>
        /// <param name="newBuild">Build to run</param>
        public void StartBuild(GeneratorMains generatorTab, BuildProcess newBuild)
        {
            if (newBuild.IsInternalGxBuild)
            {
                // Check if there is a GX build running:
                if (GenexusUIServices.Build.IsBuilding)
                {
                    MessageBox.Show("There is a Genexus build running. This build cannot be executed");
                    return;
                }

                // Check if there is already some internal GX build running
                BuildProcess internalBuild = CurrentRunningGxBuild;
                    
                if( internalBuild != null ) 
                {
                    MessageBox.Show("There is a build running a Genexus internal build (" +
                        internalBuild.ToString() + "). This build cannot be executed");
                    return;
                }
            }

            // Create a tab for the new build process
            BuildLogControl logControl = new BuildLogControl(generatorTab, newBuild);
            TabPage page = CreateTabPageForControl(logControl, newBuild.ToString());
            TabBuilds.Controls.Add(page);
            TabBuilds.SelectedTab = page;

            // Update toolwindow status
            UpdateSplitState();
            UpdateNotifyIcon();

            // Launch the build
            logControl.StartBuild();
        }

        /// <summary>
        /// It checks if there is some internal gx build runnind: In this case the build
        /// will be cancelled.
        /// </summary>
        [EventSubscription(GXEvents.BeforeBuild)]
        public void OnBeforeBuild(object sender, BuildEventArgs args)
        {
            try
            {
                // Check if there is already some internal GX build running
                BuildProcess internalBuild = CurrentRunningGxBuild;

                if (internalBuild != null)
                {
                    args.Cancel = true;
                    args.CancelMessage = "Genexus build cancelled by Work with mains";
                    UIServices.Environment.Invoke(() =>
                    {
                        MessageBox.Show("There is a build running a Genexus internal build (" +
                        internalBuild.ToString() + ") on the Work with mains toolwindow. " +
                        "The Genexus build has been cancelled");
                        return;
                    });
                }
            }
            catch { }
        }

        /// <summary>
        /// Help button clicked
        /// </summary>
        private void BtnHelp_Click(object sender, EventArgs e)
        {
            OpenDocumentation.Open("wwmains.shtml");
        }

    }
}
