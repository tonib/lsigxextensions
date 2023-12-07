using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.Form.DOM;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.UI;
using LSI.Packages.Extensiones.Utilidades.WinForms;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Comandos
{
    /// <summary>
    /// Tool to reorder tab pages of a winform tab control
    /// </summary>
    public partial class ReorderWinTab : Form, IExecutable
    {

        /// <summary>
        /// Object to edit
        /// </summary>
        private KBObject ObjectToEdit;

        /// <summary>
        /// Object editor
        /// </summary>
        private IDocumentView CurrentView;

        /// <summary>
        /// Construtor
        /// </summary>
        public ReorderWinTab()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Winform tab controls
        /// </summary>
        private IEnumerable<TabElement> TabControls
        {
            get
            {
                return EnumeradorWinform.EnumerarControles(
                    ObjectToEdit.Parts.LsiGet<WinFormPart>())
                    .Where(x => x is TabElement)
                    .Cast<TabElement>();
            }
        }

        /// <summary>
        /// Get tab control by its name
        /// </summary>
        /// <param name="name">Tab name</param>
        /// <returns>The tab control</returns>
        private TabElement GetTabControlByName(string name)
        {
            return TabControls.Where(x =>
                x.Name == name)
                .FirstOrDefault();
        }

        /// <summary>
        /// The currently selected tab
        /// </summary>
        private TabElement SelectedTab
        {
            get
            {
                return GetTabControlByName(CmbTabControls.SelectedItem as string);
            }
        }

        /// <summary>
        /// Run the extension
        /// </summary>
        public void Execute()
        {
            // Get the object to edit (the current object)
            IGxDocument doc = UIServices.Environment.ActiveDocument;
            if (doc == null)
                return;
            ObjectToEdit = doc.Object;
            if (ObjectToEdit == null)
                return;
            if (!WinFormGx.IsWinform(ObjectToEdit))
                return;
            if (doc.Dirty) 
            {
                MessageBox.Show("Object is modified. Please, save it before run this extension");
                return;
            }
            CurrentView = UIServices.Environment.ActiveView;

            // Search tabs:
            IEnumerable<TabElement> tabs = EnumeradorWinform.EnumerarControles(
                ObjectToEdit.Parts.LsiGet<WinFormPart>())
                .Where(x => x is TabElement)
                .Cast<TabElement>();

            foreach (FormElement tab in tabs)
                CmbTabControls.Items.Add(tab.Name);

            if (CmbTabControls.Items.Count == 0)
            {
                MessageBox.Show("No tab controls found");
                return;
            }
            CmbTabControls.SelectedIndex = 0;

            ShowDialog();
        }

        /// <summary>
        /// Selected tab changed
        /// </summary>
        private void CmbTabControls_SelectedIndexChanged(object sender, EventArgs e)
        {
            LstTabPages.Items.Clear();

            TabElement tab = SelectedTab;
            foreach (TabPageElement page in tab.Children)
                LstTabPages.Items.Add(GetPageCaption(page));
        }

        /// <summary>
        /// Move up clicked
        /// </summary>
        private void BtnMoveUp_Click(object sender, EventArgs e)
        {
            ListBoxUtils.MoveSelectedItem(LstTabPages, -1);
        }

        /// <summary>
        /// Move down clicked
        /// </summary>
        private void BtnMoveDown_Click(object sender, EventArgs e)
        {
            ListBoxUtils.MoveSelectedItem(LstTabPages, +1);
        }

        /// <summary>
        /// Get a tab page caption
        /// </summary>
        /// <param name="page">The page</param>
        /// <returns>The caption</returns>
        private string GetPageCaption(TabPageElement page)
        {
            return page.GetPropertyValue(Properties.FORMTABPAGE.Caption).ToString();
        }

        /// <summary>
        /// Search a tab page by its caption
        /// </summary>
        /// <param name="tab">The tab control</param>
        /// <param name="pageCaption">The caption to search</param>
        /// <returns>The tab page. null if the caption was not found</returns>
        private TabPageElement FindPageByCaption(TabElement tab, string pageCaption)
        {
            return tab.Children
                .Cast<TabPageElement>()
                .FirstOrDefault(x => GetPageCaption(x) == pageCaption);
        }

        /// <summary>
        /// Accept button clicked
        /// </summary>
        private void BtnAccept_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if there are duplicated captions: So we cannot trust this code:
                bool thereAreDuplicates = LstTabPages.Items
                    .Cast<string>()
                    .GroupBy(x => x)
                    .Where(g => g.Count() > 1)
                    .Any();
                if (thereAreDuplicates)
                {
                    MessageBox.Show("There pages with the same caption. The tab cannot be reordered");
                    return;
                }

                if (!CurrentView.Close())
                    return;

                // Get the new pages sorted
                List<TabPageElement> sortedPages = new List<TabPageElement>();
                TabElement tab = SelectedTab;
                foreach (string pageCaption in LstTabPages.Items)
                    sortedPages.Add(FindPageByCaption(tab, pageCaption));

                // Remove current pages
                foreach (TabPageElement page in sortedPages)
                    tab.RemoveElement(page);

                // Add pages sorted
                foreach (TabPageElement page in sortedPages)
                    tab.AddElement(page);

                // Needed to save changes on winform
                WinFormPart winForm = ObjectToEdit.Parts.LsiGet<WinFormPart>();
                winForm.Dirty = true;

                ObjectToEdit.Save();

                // Reopen the objeto
                UIServices.Objects.Open(ObjectToEdit, OpenDocumentOptions.CurrentVersion);

                Dispose();
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// Help button clicked
        /// </summary>
        private void ReorderWinTab_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OpenDocumentation.Open("ordentab.html");
        }

    }
}
