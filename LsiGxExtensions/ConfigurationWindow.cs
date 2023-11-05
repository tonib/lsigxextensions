using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using LSI.Packages.Extensiones.Comandos;
using LSI.Packages.Extensiones.Comandos.Build;
using LSI.Packages.Extensiones.Utilidades.Validation;
using LSI.Packages.Extensiones.Utilidades.Compression;
using System.IO;
using System.Diagnostics;
using LSI.Packages.Extensiones.Utilidades.VS;
using LSI.Packages.Extensiones.Comandos.Autocomplete;
using Artech.Architecture.UI.Framework.Services;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones
{
    /// <summary>
    /// Extensions configuration window
    /// </summary>
    public partial class ConfigurationWindow : Form
    {

        /// <summary>
        /// Lista de checkboxes dependientes de la revision avanzada
        /// </summary>
        private List<CheckBox> AdvancedChecks;

        /// <summary>
        /// Habilitar eventos de los checks avanzados?
        /// </summary>
        private bool EventosChkAvanzados = false;

        /// <summary>
        /// Configuracion
        /// </summary>
        public ConfigurationWindow()
        {
            InitializeComponent();

            AdvancedChecks = new List<CheckBox> { ChkSoloLeidas, ChkSoloEscritas, ChkRevisarNParametros , 
                ChkRevisarParametrosOut , ChkReportDeprecated };

            LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.LoadFromRegistry();
            ChkRevisarVariables.Checked = cfg.RevisarObjetosAlGuardar;
            ChkRevisarN4.Checked = cfg.RevisarVariablesN4;
            ChkRevisarLecturasEscrituras.Checked = cfg.RevisarLecturasEscrituras;
            ChkCheckWinformSize.Checked = cfg.RevisarTamanyoWinforms;
            TxtX.Text = cfg.TamMaximoWinforms.Width.ToString();
            TxtY.Text = cfg.TamMaximoWinforms.Height.ToString();
            ChkReglasHidden.Checked = cfg.RevisarReglasHidden;
            ChkVariablesNoUsadas.Checked = cfg.RevisarVariablesNoUsadas;
            ChkRevisarAutodefinidas.Checked = cfg.RevisarVariablesAutodefinidas;
            ChkSoloLeidas.Checked = cfg.RevisarVariablesSoloLeidas;
            ChkSoloEscritas.Checked = cfg.RevisarVariablesSoloEscritas;
            ChkRevisarNParametros.Checked = cfg.RevisarParametrosLlamadas;
            ChkRevisarParametrosOut.Checked = cfg.RevisarParametrosOut;
            ChkSoloUiConditions.Checked = cfg.RevisarVariablesSoloUI;
            ChkSoloReglaParm.Checked = cfg.RevisarVariablesSoloReglaParm;
            ChkColumnasGrids.Checked = cfg.RevisarColumnasInvisiblesGrids;
            ChkAtributosHuerfanos.Checked = cfg.CheckOrphanAttributes;
			ChkOrphanInsideSubs.Checked = cfg.CheckOrphanInsideSubs;
            ChkUnusedPrintblocks.Checked = cfg.CheckUnreferendPrintBlocks;
            ChkReportInitialValue.Checked = cfg.ReportVariablesROWithInitialValue;
            TxtAlwaysNullPrefix.Text = cfg.AlwaysNullVariablesPrefix;
            ChkNoFolder.Checked = cfg.CheckObjectsWithNoFolder;
            ChkAlwaysNullIIniValue.Checked = cfg.ReportAlwaysNullWithInitialValue;
            ChkReportDeprecated.Checked = cfg.ReportDeprecatedObjects;
            TxtDeprecatedObjectsDescription.Text = cfg.DeprecatedObjectsDescription;
            ChkConfirmRebuildAll.Checked = cfg.ConfirmRebuildAll;
            txtVarNamesNoCheck.Text = cfg.VariableNamesNoCheck;

            TxtVSVersionId.Text = cfg.VisualStudioComId;
            TxtZipsPath.Text = cfg.ZipDestinationFolder;
            TxtZipAddAlways.Text = cfg.ZipFilesToAddAlways;

            TxtCompressorPath.Text = cfg.Compressor.CompressorPath;
            TxtCompressorCmd.Text = cfg.Compressor.CompressorCommandLine;
            TxtCompressorExtension.Text = cfg.Compressor.CompressorFilesExtension;
            TxtExclude.Text = cfg.Compressor.CompressorExcludeOption;

            TxtTextEditor.Text = cfg.TextEditor;
            ChkSetBinDir.Checked = cfg.SetBinAsCurrentDir;
            ChkPatchMainExit.Checked = cfg.PatchWinMainExits;

            ChkAutocomplete.Checked = cfg.CustomAutocomplete;
            chkUppercaseKeywords.Checked = cfg.UppercaseKeywords;
            chkCreateVariables.Checked = cfg.AutocompleteCreateVariables;
            cmbUsePredictionModel.SelectedIndex = (int)cfg.PredictionModelType;
            chkDebugPredictionModel.Checked = cfg.DebugPredictionModel;
			txtCustomModelPath.Text = cfg.CustomModelPath;
            txtPythonScripts.Text = cfg.PredictionPythonScriptsDir;
            txtVirtualEnv.Text = cfg.PythonVirtualEnvDir;
            cmbParmInfo.SelectedIndex = (int)cfg.ExtendedParmInfoType;
            chkAutocloseParenthesis.Checked = cfg.AutocloseParenthesis;

            // Button only for LSI:
            BtnBorRegistro.Visible = LsiExtensionsConfiguration.PrivateExtensionsInstalled;

            // Habilitar eventos ahora que esta todo asignado
            EventosChkAvanzados = true;

            RevisarChecksAvanzados(false);

            UpdateAutocompleteState();

			ChkAtributosHuerfanos_CheckedChanged(null, null);

			if (ProductVersionHelperExtensions.MajorVersion() >= ProductVersionHelperExtensions.GX15)
            {
                // No winforms in this Gx version
                grpWinforms.Visible = false;
            }
        }

        /// <summary>
        /// Pulsado el boton de aceptar
        /// </summary>
        private void BtnAceptar_Click(object sender, EventArgs e)
        {
			int x = 0, y = 0;
			try { x = int.Parse(TxtX.Text); }
			catch { }
			try { y = int.Parse(TxtY.Text); }
			catch { }

			// No winforms in version >= GX15
			if (ProductVersionHelperExtensions.MajorVersion() <= ProductVersionHelperExtensions.GXX_EV3 && ChkCheckWinformSize.Checked)
			{
				if (x <= 0 || y <= 0)
				{
					MessageBox.Show("Winform sizes must to be greater or equal to zero");
					return;
				}
			}

            if (ChkAutocomplete.Checked)
            {
                if (!string.IsNullOrEmpty(txtPythonScripts.Text) && !CheckDirectory(txtPythonScripts))
                    return;
                if (!string.IsNullOrEmpty(txtVirtualEnv.Text) && !CheckDirectory(txtVirtualEnv))
                    return;

				if(SelectedModelType == LsiExtensionsConfiguration.PredictionModelTypes.UseCustomModelTfLite ||
					SelectedModelType == LsiExtensionsConfiguration.PredictionModelTypes.UseCustomFullTf)
				{
					if(!CheckDirectory(txtCustomModelPath))
						// TODO: Check directory contains a TF Lite model and the data_info.json file
						return;
				}
            }

            LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.Load();
            bool oldAutocomplete = cfg.CustomAutocomplete;
            try
            {
                cfg.RevisarObjetosAlGuardar = ChkRevisarVariables.Checked;
                cfg.RevisarVariablesN4 = ChkRevisarN4.Checked;
                cfg.RevisarLecturasEscrituras = ChkRevisarLecturasEscrituras.Checked;
                cfg.RevisarTamanyoWinforms = ChkCheckWinformSize.Checked;
                cfg.TamMaximoWinforms = new Size(x, y);
                cfg.RevisarReglasHidden = ChkReglasHidden.Checked;
                cfg.RevisarVariablesNoUsadas = ChkVariablesNoUsadas.Checked;
                cfg.RevisarVariablesAutodefinidas = ChkRevisarAutodefinidas.Checked;
                cfg.RevisarVariablesSoloLeidas = ChkSoloLeidas.Checked;
                cfg.RevisarVariablesSoloEscritas = ChkSoloEscritas.Checked;
                cfg.RevisarParametrosLlamadas = ChkRevisarNParametros.Checked;
                cfg.RevisarParametrosOut = ChkRevisarParametrosOut.Checked;
                cfg.RevisarVariablesSoloUI = ChkSoloUiConditions.Checked;
                cfg.RevisarVariablesSoloReglaParm = ChkSoloReglaParm.Checked;
                cfg.RevisarColumnasInvisiblesGrids = ChkColumnasGrids.Checked;
                cfg.CheckOrphanAttributes = ChkAtributosHuerfanos.Checked;
				cfg.CheckOrphanInsideSubs = ChkOrphanInsideSubs.Checked;
				cfg.CheckUnreferendPrintBlocks = ChkUnusedPrintblocks.Checked;
                cfg.ReportVariablesROWithInitialValue = ChkReportInitialValue.Checked;
                cfg.ReportAlwaysNullWithInitialValue = ChkAlwaysNullIIniValue.Checked;
                cfg.AlwaysNullVariablesPrefix = TxtAlwaysNullPrefix.Text;
                cfg.CheckObjectsWithNoFolder = ChkNoFolder.Checked;
                cfg.ReportDeprecatedObjects = ChkReportDeprecated.Checked;
                cfg.DeprecatedObjectsDescription = TxtDeprecatedObjectsDescription.Text;
                cfg.ConfirmRebuildAll = ChkConfirmRebuildAll.Checked;
                cfg.VariableNamesNoCheck = txtVarNamesNoCheck.Text;

                cfg.VisualStudioComId = TxtVSVersionId.Text;
                cfg.ZipDestinationFolder = TxtZipsPath.Text;
                cfg.ZipFilesToAddAlways = TxtZipAddAlways.Text;
                cfg.Compressor.CompressorPath = TxtCompressorPath.Text;
                cfg.Compressor.CompressorCommandLine = TxtCompressorCmd.Text;
                cfg.Compressor.CompressorFilesExtension = TxtCompressorExtension.Text;
                cfg.Compressor.CompressorExcludeOption = TxtExclude.Text;
                cfg.SetBinAsCurrentDir = ChkSetBinDir.Checked;
                cfg.PatchWinMainExits = ChkPatchMainExit.Checked;
                cfg.TextEditor = TxtTextEditor.Text.Trim();

                cfg.CustomAutocomplete = ChkAutocomplete.Checked;
                cfg.UppercaseKeywords = chkUppercaseKeywords.Checked;
                cfg.AutocompleteCreateVariables = chkCreateVariables.Checked;
                cfg.PredictionModelType = SelectedModelType;
                cfg.DebugPredictionModel = chkDebugPredictionModel.Checked;
				cfg.CustomModelPath = txtCustomModelPath.Text;
                cfg.PredictionPythonScriptsDir = txtPythonScripts.Text;
                cfg.PythonVirtualEnvDir = txtVirtualEnv.Text;
                cfg.ExtendedParmInfoType = (LsiExtensionsConfiguration.ParmInfoType)cmbParmInfo.SelectedIndex;
                cfg.AutocloseParenthesis = chkAutocloseParenthesis.Checked;

                cfg.GuardarEnRegistro();

                if (!cfg.RevisarObjetosAlGuardar)
                    // Si ahora no se revisan objetos al guardar, vaciar la cola de objetos pendientes
                    // de revisar
                    ValidationService.Service.TasksQueue.VaciarCola();

                if (oldAutocomplete != cfg.CustomAutocomplete)
                {
                    // Autocomplete mode changed
                    if (cfg.CustomAutocomplete)
                    {
                        if (UIServices.KB.CurrentKB != null)
                            // Currently in a KB
                            Autocomplete.SetupAutocompleteCurrentKb();
                    }
                    else
                        Autocomplete.DisableAutocompleteCurrentKb();
                }
                // Be sure python prediction prediciton process is killed
                Autocomplete.DisposePredictor();

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Pulsado el boton de cancelar
        /// </summary>
        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Abre la documentacion
        /// </summary>
        private void VentanaConfiguracion_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            OpenDocumentation.Open("configuracion.shtml");
        }

        private void RevisarChecksAvanzados(bool cambiarChecks)
        {
            if (!EventosChkAvanzados)
                return;

            EventosChkAvanzados = false;
            // Habilitar / deshabilitar opcones avanzadas
            foreach (CheckBox chk in AdvancedChecks)
            {
                chk.Enabled = ChkRevisarLecturasEscrituras.Checked;
                if( cambiarChecks )
                    chk.Checked = ChkRevisarLecturasEscrituras.Checked;
            }
            EventosChkAvanzados = true;
        }

        /// <summary>
        /// Se ha hecho click en "Revisiones avanzadas"
        /// </summary>
        private void ChkRevisarLecturasEscrituras_CheckedChanged(object sender, EventArgs e)
        {
            RevisarChecksAvanzados(true);
        }

        /// <summary>
        /// Pulsado boton de borrar configuracion del registro
        /// </summary>
        private void BtnBorRegistro_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Seguro que quiere borrar la configuración del registro de Windows?", "Confirmación",
                MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            try
            {
                LsiExtensionsConfiguration.BorrarDelRegistro();
                Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Some advanced check clicked event
        /// </summary>
        private void ChkAvanzadas_CheckedChanges(object sender, EventArgs e)
        {
            // Only applied if ChkSoloLeidas is set
            ChkReportInitialValue.Enabled = ChkSoloLeidas.Checked;

            TxtDeprecatedObjectsDescription.Enabled = ChkReportDeprecated.Checked;

            if (!EventosChkAvanzados)
                return;

            // Ver si todos los checks avanzados estan desmarcados
            foreach( CheckBox chk in AdvancedChecks )
            {
                if (chk.Checked)
                    // No hacer nada
                    return;
            }
            // Si estan todos desmarcados, desmarcar el check de avanzados
            ChkRevisarLecturasEscrituras.Checked = false;
        }

        private void BtnSelZipDestination_Click(object sender, EventArgs e)
        {
            SelectDirectory(TxtZipsPath);
        }

        private void BtnSelExecutable_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "EXE files|*.exe|All files|*.*";
            if (File.Exists(TxtCompressorPath.Text))
                open.FileName = TxtCompressorPath.Text;
            if (open.ShowDialog() == DialogResult.OK)
            {
                if( sender == BtnSelCompressor )
                    TxtCompressorPath.Text = open.FileName;
                else
                    TxtTextEditor.Text = open.FileName;
            }
        }

        /// <summary>
        /// Set 7-zip configuration for compression button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn7Zip_Click(object sender, EventArgs e)
        {
            TxtCompressorPath.Text = Compressor.ZIP7PATH;
            TxtCompressorExtension.Text = Compressor.ZIP7EXTENSION;
            TxtCompressorCmd.Text = Compressor.ZIP7CMDLINE;
            TxtExclude.Text = Compressor.ZIP7EXCLUDEOPT;
        }

        /// <summary>
        /// Set winrar configuration for compression button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnWinrar_Click(object sender, EventArgs e)
        {
            TxtCompressorPath.Text = Compressor.WINRARPATH;
            TxtCompressorExtension.Text = Compressor.WINRAREXTENSION;
            TxtCompressorCmd.Text = Compressor.WINRARCMDLINE;
            TxtExclude.Text = Compressor.WINRAREXCLUDEOPT;
        }

        /// <summary>
        /// Clean cached generated objects information
        /// </summary>
        private void BtnCleanSourcesCache_Click(object sender, EventArgs e)
        {
            new CleanObjectInfoCache().Execute();
        }

        /// <summary>
        /// 7zip web link clicked
        /// </summary>
        private void Lnk7zip_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("http://www.7-zip.org/");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Visual studio version clicked
        /// </summary>
        private void BtnVSVersion_Click(object sender, EventArgs e)
        {
            string comId = string.Empty;
            if (sender == BtnVS2008)
                comId = VisualStudio.VS_2008_COMID;
            else if (sender == BtnVS2010)
                comId = VisualStudio.VS_2010_COMID;
            else if (sender == BtnVS2012)
                comId = VisualStudio.VS_2012_COMID;
            else if (sender == BtnVS2015)
                comId = VisualStudio.VS_2015_COMID;
            else if (sender == BtnVS2019)
                comId = VisualStudio.VS_2019_COMID;

            if (!string.IsNullOrEmpty(comId))
                TxtVSVersionId.Text = comId;
        }

        private bool CheckDirectory(TextBox field)
        {
            if (!Directory.Exists(field.Text))
            {
                MessageBox.Show("Directory '" + field.Text + "' does not exists");
                field.Focus();
                return false;
            }
            return true;
        }

        private void SelectDirectory(TextBox field)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(field.Text))
                    dlg.SelectedPath = field.Text;
                if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dlg.SelectedPath))
                    field.Text = dlg.SelectedPath;
            }
        }

        private void btnSelVirtualEnv_Click(object sender, EventArgs e)
        {
            SelectDirectory(txtVirtualEnv);
        }

        private void btnSelPythonScripts_Click(object sender, EventArgs e)
        {
            SelectDirectory(txtPythonScripts);
        }

        private void OpenDir(TextBox field)
        {
            try
            {
                if (string.IsNullOrEmpty(field.Text))
                    return;
                Process.Start(field.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void lblVirtualEnv_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenDir(txtVirtualEnv);
        }

        private void lblPythonScripts_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenDir(txtPythonScripts);
        }

		private LsiExtensionsConfiguration.PredictionModelTypes SelectedModelType
		{
			get { return (LsiExtensionsConfiguration.PredictionModelTypes)cmbUsePredictionModel.SelectedIndex; }
		}

		private void UpdateAutocompleteState()
        {
            chkAutocloseParenthesis.Enabled = chkUppercaseKeywords.Enabled = chkCreateVariables.Enabled = cmbParmInfo.Enabled =
                cmbUsePredictionModel.Enabled = txtCustomModelPath.Enabled = btnSelCustomModel.Enabled = ChkAutocomplete.Checked;

            txtVirtualEnv.Enabled = btnSelVirtualEnv.Enabled =
                txtPythonScripts.Enabled = btnSelPythonScripts.Enabled = ChkAutocomplete.Checked;

			chkDebugPredictionModel.Enabled = ChkAutocomplete.Checked && SelectedModelType != LsiExtensionsConfiguration.PredictionModelTypes.DoNotUse;
			bool customPathEnabled = ChkAutocomplete.Checked && 
				( SelectedModelType == LsiExtensionsConfiguration.PredictionModelTypes.UseCustomModelTfLite ||
				  SelectedModelType == LsiExtensionsConfiguration.PredictionModelTypes.UseCustomFullTf );
			txtCustomModelPath.Enabled = btnSelCustomModel.Enabled = customPathEnabled;
		}

        private void ChkAutocomplete_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAutocompleteState();
        }

        private void chkUseModel_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAutocompleteState();
        }

		private void cmbUsePredictionModel_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateAutocompleteState();
		}

		/// <summary>
		/// Select custom model
		/// </summary>
		private void btnSelCustomModel_Click(object sender, EventArgs e)
		{
			SelectDirectory(txtCustomModelPath);
			// TODO: Check directory contains a TF Lite model and the data_info.json file
		}

		private void lnkCustomModel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			OpenDir(txtCustomModelPath);
		}

		private void ChkAtributosHuerfanos_CheckedChanged(object sender, EventArgs e)
		{
			ChkOrphanInsideSubs.Enabled = ChkAtributosHuerfanos.Checked;
		}

	}
}
