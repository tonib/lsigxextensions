using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LSI.Packages.Extensiones.Utilidades.UI;
using System.Runtime.InteropServices;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;
using Artech.Genexus.Common;

namespace LSI.Packages.Extensiones.Comandos
{
    /// <summary>
    /// Toool to search texts in genexus sources
    /// </summary>
    [Guid("0FBEC0F1-88B2-4c23-AA07-42849285E790")]
    public partial class GxSourcesSearchTW : SearchKbObjectsToolWindowBase
    {

        /// <summary>
        /// Search strings anywhere
        /// </summary>
        private const string WHERE_ANYWHERE = "Anywhere";

        /// <summary>
        /// Search strings on string literals
        /// </summary>
        private const string WHERE_STRINGLITERALS = "String literals";

        /// <summary>
        /// Search strings on comment blocks
        /// </summary>
        private const string WHERE_COMMENTS = "Comments";

        /// <summary>
        /// Constructor
        /// </summary>
        public GxSourcesSearchTW()
        {
            InitializeComponent();

            this.Icon = Resources.LSI;

            // Where to search combo box:
            CmbWhere.Items.Clear();
            CmbWhere.Items.Add(WHERE_ANYWHERE);
            CmbWhere.Items.Add(WHERE_STRINGLITERALS);
            CmbWhere.Items.Add(WHERE_COMMENTS);
            CmbWhere.SelectedItem = WHERE_ANYWHERE;

            // Base tool window initilization:
            base.InitializeSearchTW(BtnSearch, PicBusy, LblState, Grid);
        }

        /// <summary>
        /// Enable / disable UI fields
        /// </summary>
        /// <param name="enabled"></param>
        protected override void EnableUIFields(bool enabled)
        {
            TxtSeachText.Enabled = CmbWhere.Enabled = ChkCase.Enabled = ChkCase.Enabled = 
                ChRegExp.Enabled = ChkRules.Enabled = ChkProcedure.Enabled = ChkEvents.Enabled =
                ChkConditions.Enabled = CmbWhere.Enabled = enabled;
        }

        /// <summary>
        /// List of selected object parts where to search
        /// </summary>
        private List<Guid> SelectedParts
        {
            get
            {
                List<Guid> selectedParts = new List<Guid>();
                if (ChkRules.Checked)
                    selectedParts.Add(PartType.Rules);
                if (ChkProcedure.Checked)
                    selectedParts.Add(PartType.Procedure);
                if (ChkEvents.Checked)
                    selectedParts.Add(PartType.Events);
                if (ChkConditions.Checked)
                    selectedParts.Add(PartType.Conditions);
                return selectedParts;
            }
        }

        /// <summary>
        /// Returns the "Where" selected value
        /// </summary>
        private GxSourcesFinder.TSearchWhere SelectedWhere
        {
            get
            {
                if ((string)CmbWhere.SelectedItem == WHERE_COMMENTS)
                    return GxSourcesFinder.TSearchWhere.COMMENTS;
                else if ((string)CmbWhere.SelectedItem == WHERE_STRINGLITERALS)
                    return GxSourcesFinder.TSearchWhere.STRINGLITERALS;
                else
                    return GxSourcesFinder.TSearchWhere.ANYWHERE;
            }
        }

        /// <summary>
        /// Creates the finder object
        protected override UISearchBase CreateFinder()
        {
            string textPattern = TxtSeachText.Text.Trim();
            if (string.IsNullOrEmpty(textPattern))
            {
                MessageBox.Show("Please, specify the text to search");
                TxtSeachText.Focus();
                return null;
            }

            List<Guid> parts = SelectedParts;
            if (parts.Count == 0)
            {
                MessageBox.Show("Please, specify the parts to search");
                ChkRules.Focus();
                return null;
            }

            return new GxSourcesFinder(textPattern)
            {
                PartTypesSearch = parts,
                WhereSearch = SelectedWhere,
                IsCaseSensitive = ChkCase.Checked,
                IsRegularExpression = ChRegExp.Checked
            };
        }

        /// <summary>
        /// Search button clicked
        /// </summary>
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            base.SearchButtonPressed();
        }

        /// <summary>
        /// Help button clicked
        /// </summary>
        private void BtnAyuda_Click(object sender, EventArgs e)
        {
            OpenDocumentation.Open("buscartexto.html");
        }

    }
}
