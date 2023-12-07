using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using LSI.Packages.Extensiones.Utilidades.UI;
using System.Runtime.InteropServices;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition;
using System.Diagnostics;
using System.IO;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.ModelGeneration
{
	// TODO: Remove all stuff related to train. Training within Gx is no longer supported

	/// <summary>
	/// Toolwindow to generate a model to predict Genexus code
	/// </summary>
	[Guid("5EDD2C9C-D03B-4C3A-8A53-78723C795924")]
    public partial class GenerateModelTW : ToolWindowBase
    {
        // Operations
        const string OP_TRAIN = "Train";
        const string OP_EXPORT = "Export";
        const string OP_EVAL = "Eval";
        const string OP_DEBUGEVAL = "DebugEval";
		const string OP_EXPORTKBTRAIN = "ExportKbTrain";

		/// <summary>
		/// Current model version
		/// </summary>
		const int CURRENT_MODEL_VERSION = 14;

        /// <summary>
        /// Current operation, one of OP_*
        /// </summary>
        private string CurrentOperation;

        internal BackgroundWorker Worker;

        internal BackgroundWorkerLogger Logger;

        /// <summary>
        /// Current train process, null if no process is running
        /// </summary>
        private TrainModel TrainProcess;

		public GenerateModelTW()
        {
            InitializeComponent();

			// Model types combo values:
			cmbModelType.Items.Clear();
			cmbModelType.DisplayMember = "Text";
			cmbModelType.ValueMember = "Value";
			cmbModelType.DataSource = new [] {
				new { Text = "GPT", Value = DataInfo.MODEL_TYPE_GPT },
				new { Text = "RNN (context only for item to predict)", Value = DataInfo.MODEL_TYPE_RNN },
				new { Text = "RNN (context for all timesteps)", Value = DataInfo.MODEL_TYPE_EXP }
			};

			Worker = new BackgroundWorker();
            Worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
            Worker.ProgressChanged += new ProgressChangedEventHandler(Worker_ProgressChanged);
            Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;

			// Initial values
			SetUIValues(new DataInfoV14(32));

			if (!LsiExtensionsConfiguration.PrivateExtensionsInstalled)
            {
                // Function to debug evaluations. Nof for final users
                btnDebugEval.Visible = false;
            }


            Logger = new BackgroundWorkerLogger(Worker);

            UpdateUiState();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateUiState();
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            txtLog.AppendText(e.UserState as string);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                DataInfo dataInfo = e.Argument as DataInfo;
                TrainProcess = new TrainModel(this, dataInfo);
				if (CurrentOperation == OP_TRAIN)
					TrainProcess.Execute();
				else if (CurrentOperation == OP_EXPORT)
					TrainProcess.ExportModel();
				else if (CurrentOperation == OP_EVAL)
					TrainProcess.EvaluateModel();
				else if (CurrentOperation == OP_DEBUGEVAL)
					TrainProcess.DebugEvalSamples();
				else if (CurrentOperation == OP_EXPORTKBTRAIN)
					TrainProcess.ExportKbObjects();

				Logger.AddLine("DONE!");
            }
            catch(Exception ex)
            {
                Logger.AddErrorLine(ex.ToString());
            }
            finally
            {
                TrainProcess = null;
            }
        }

        private void CancelProcess()
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to cancel the operation?", "Confirm",
                               MessageBoxButtons.OKCancel) != DialogResult.OK)
                    return;

                Worker.CancelAsync();
                if (TrainProcess != null)
                    TrainProcess.CancelProcess();
            }
            catch { }
        }

		private void SetValue<T>(TextBox txt, T value)
		{
			try
			{
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
				txt.Text = converter.ConvertToString(value);
			}
			catch(Exception ex)
			{
				Log.ShowException(ex);
			}
		}

        private bool GetValue<T>(TextBox txt, Label lbl, out T value, T? minValue = null, 
            T? maxValue = null) where T : struct, IComparable
        {
            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                value = (T)converter.ConvertFromString(txt.Text);
            }
            catch
            {
                MessageBox.Show(lbl.Text + ": Wrong value");
                txt.Focus();
                value = default(T);
                return false;
            }

            if( minValue != null && value.CompareTo(minValue) < 0)
            {
                MessageBox.Show(lbl.Text + ": Must be greater or equal to " + minValue);
                txt.Focus();
                return false;
            }

            if (maxValue != null && value.CompareTo(maxValue) > 0)
            {
                MessageBox.Show(lbl.Text + ": Must be less or equal to " + maxValue);
                txt.Focus();
                return false;
            }

            return true;
        }

        private DataInfo GetModelDefinition()
        {
            int maxEpochs;
            if (!GetValue(txtNEpochs, lblNEpochs, out maxEpochs, 0))
                return null;

			int maxBatchesPerEpoch;
			if (!GetValue(txtMaxBatchEpoch, lblMaxBachEpoch, out maxBatchesPerEpoch, 0))
				return null;

			int logEach;
			if (!GetValue(txtLogEach, lblLogEach, out logEach, 0))
				return null;

			int nObjectsToExport;
            if(!GetValue(txtNExport, lblNExport, out nObjectsToExport, 0))
                return null;

            decimal percentageEvaluation;
            if(!GetValue(txtPercentageEvaluation, lblPercentageEvaluation, out percentageEvaluation, 0.001M))
                return null;

            int nNetworkElements;
            if (!GetValue(txtRnnSize, lblRnnSize, out nNetworkElements, 1))
                return null;

            int sequenceLength;
            if (!GetValue(txtSeqLength, lblSeqLength, out sequenceLength, 1))
                return null;

            decimal dropout;
            if (!GetValue(txtDropout, lblDropout, out dropout, 0, 1))
                return null;

			// GPT
			int gptNLayers;
			if (!GetValue(txtGptNLayers, lblGptNLayers, out gptNLayers, 1))
				return null;

			int gptEmbeddingSize;
			if (!GetValue(txtGptEmbeddingSize, lblGptEmbSize, out gptEmbeddingSize, 1))
				return null;

			decimal gptResDroput;
			if (!GetValue(txtGptResidualDropout, lblGptResDroput, out gptResDroput, 0, 1))
				return null;

			decimal gptAttDropout;
			if (!GetValue(txtGptAttDropout, lblGptAttDropout, out gptAttDropout, 0, 1))
				return null;

			int gptNAttHeads;
			if (!GetValue(txtGptAttHeads, lblGptAttHeads, out gptNAttHeads, 1, gptEmbeddingSize))
				return null;

			decimal gptEmbDropout;
			if (!GetValue(txtGptEmbDropout, lblGptEmbDropout, out gptEmbDropout, 0, 1))
				return null;

			if(gptEmbeddingSize % gptNAttHeads != 0)
			{
				MessageBox.Show("GPT: Embedding size must to be divisible by N. attention heads");
				return null;
			}
			// END GPT


			if (string.IsNullOrEmpty(txtModelDirectory.Text))
            {
                MessageBox.Show("Model directory cannot be empty");
                txtModelDirectory.Focus();
                return null;
            }

            DataInfo model = GetExistingModel();

            if (model == null && Directory.Exists(txtModelDirectory.Text) && 
                Directory.GetFiles(txtModelDirectory.Text, "*.csv").Any())
            {
                MessageBox.Show("The model directory for a new model cannot contain .CSV files");
                txtModelDirectory.Focus();
                return null;
            }
            
            if( model != null)
            {
                // Update existing model
                model.MaxEpochs = maxEpochs;
				model.MaxBatchesPerEpoch = maxBatchesPerEpoch;
				model.LogEachBatches = logEach;
				model.NObjectsToExport = nObjectsToExport;

				// Model maybe has been moved:
				model.DataDirectory = txtModelDirectory.Text;
                model.SerializeToDirectory();

				// Return existing model
                return model;
            }

			// Return a new model
			// IMPORTANT: If model version is changed, change CURRENT_MODEL_VERSION constant too
			return new DataInfoV14(int.Parse(txtMaxHash.Text))
            {
                NObjectsToExport = nObjectsToExport,
                DataDirectory = txtModelDirectory.Text,
                PercentageEvaluation = percentageEvaluation,
                SequenceLength = sequenceLength,

                MaxEpochs = maxEpochs,
				MaxBatchesPerEpoch = maxBatchesPerEpoch,
				LogEachBatches = logEach,

				ModelType = cmbModelType.SelectedValue as string,

				// RNN parameters
				NNetworkElements = nNetworkElements,
				CellType = cmbCellType.SelectedItem.ToString().ToLower(),
				Dropout = dropout,

				// GPT parameters
				GptAttentionDropout = gptAttDropout,
				GptEmbeddingDropout = gptEmbDropout,
				GptEmbeddingSize = gptEmbeddingSize,
				GptNHeads = gptNAttHeads,
				GptNLayers = gptNLayers,
				GptResidualDropout = gptResDroput
            }; 
        }

        private void StartOperation(string op)
        {
            DataInfo dataInfo = GetModelDefinition();
            if (dataInfo == null)
                // Wrong parameters
                return;

            CurrentOperation = op;
            txtLog.Clear();
            btnGenerate.Text = "Stop";
            EnableModelFields(false);
            EnableNonModelFields(false);
            btnExport.Enabled = btnEval.Enabled = btnExportKbObjects.Enabled = false;

            Worker.RunWorkerAsync(dataInfo);
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                if(Worker.IsBusy)
                {
                    CancelProcess();
                    return;
                }

                MessageBox.Show("Training within Gx is no longer supported. Use the pythons scripts instead");
                return;

                //if (MessageBox.Show("Are you sure you want to train?", "Confirm",
                //        MessageBoxButtons.OKCancel) != DialogResult.OK)
                //    return;

                // StartOperation(OP_TRAIN);
            }
            catch(Exception ex)
            {
                Log.ShowException(ex);
                MessageBox.Show("Error: " + ex.Message);
            }
        }

		private void btnExportKbObjects_Click(object sender, EventArgs e)
		{
			try {
				if (MessageBox.Show("Are you sure you want to export KB train files?", "Confirm",
						MessageBoxButtons.OKCancel) != DialogResult.OK)
					return;

				StartOperation(OP_EXPORTKBTRAIN);
			}
			catch (Exception ex)
			{
				Log.ShowException(ex);
				MessageBox.Show("Error: " + ex.Message);
			}
		}

		private void lnkModel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(txtModelDirectory.Text);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnSelModel_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(txtModelDirectory.Text))
                    dlg.SelectedPath = txtModelDirectory.Text;
                if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dlg.SelectedPath))
                    txtModelDirectory.Text = dlg.SelectedPath;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to export the model?", "Confirm",
                        MessageBoxButtons.OKCancel) != DialogResult.OK)
                    return;

                StartOperation(OP_EXPORT);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Launch tensorboard link clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lnkTensorboard_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                TrainModel.LaunchTensorboard(txtModelDirectory.Text);
            }
            catch(Exception ex)
            {
                Log.ShowException(ex);
                MessageBox.Show(ex.Message);
            }
        }

        private void txtModelDirectory_TextChanged(object sender, EventArgs e)
        {
            UpdateUiState();
        }

        private void UpdateUiState()
        {
            DataInfo model = GetExistingModel();
            EnableModelFields(model == null);
            EnableNonModelFields(true);
            btnExport.Enabled = btnEval.Enabled = (model != null);
			btnExportKbObjects.Enabled = true;
			btnGenerate.Text = (model == null ? "Train new model" : "Continue training");
            if (model != null)
				SetUIValues(model);
        }

		private void SetUIValues(DataInfo model)
		{
			SetValue(txtNExport, model.NObjectsToExport);
			SetValue(txtSeqLength, model.SequenceLength);
			SetValue(txtPercentageEvaluation, model.PercentageEvaluation);
			SetValue(txtRnnSize, model.NNetworkElements);
			SetValue(txtMaxHash, model.MaxTextHash);
			SetValue(txtNEpochs, model.MaxEpochs);
			SetValue(txtMaxBatchEpoch, model.MaxBatchesPerEpoch);
			SetValue(txtLogEach, model.LogEachBatches);
			SetValue(txtDropout, model.Dropout);
			SetValue(txtGptNLayers, model.GptNLayers);
			SetValue(txtGptEmbeddingSize, model.GptEmbeddingSize);
			SetValue(txtGptResidualDropout, model.GptResidualDropout);
			SetValue(txtGptAttDropout, model.GptAttentionDropout);
			SetValue(txtGptAttHeads, model.GptNHeads);
			SetValue(txtGptEmbDropout, model.GptEmbeddingDropout);

			cmbCellType.SelectedItem = model.CellType.ToUpper();
			cmbModelType.SelectedValue = model.ModelType;
		}

		private void EnableModelFields(bool enabled)
        {
            txtSeqLength.ReadOnly = txtPercentageEvaluation.ReadOnly = 
                txtRnnSize.ReadOnly = txtMaxHash.ReadOnly = 
                txtDropout.ReadOnly =
				txtGptNLayers.ReadOnly = txtGptEmbeddingSize.ReadOnly = txtGptResidualDropout.ReadOnly =
				txtGptAttDropout.ReadOnly = txtGptAttHeads.ReadOnly = txtGptEmbDropout.ReadOnly =
				!enabled;
            cmbCellType.Enabled = cmbModelType.Enabled = enabled;
        }
        
        private void EnableNonModelFields(bool enabled)
        {
            txtNEpochs.ReadOnly = txtModelDirectory.ReadOnly = txtMaxBatchEpoch.ReadOnly = 
				txtLogEach.ReadOnly = txtNExport.ReadOnly = !enabled;
            btnSelModel.Enabled = enabled;
        }

        private DataInfo GetExistingModel()
        {
            if (!Directory.Exists(txtModelDirectory.Text))
                return null;

            try
            {
                DataInfo model = DataInfo.DeserializeFromDirectory(txtModelDirectory.Text);
                if (model.ModelVersion != CURRENT_MODEL_VERSION)
                    // Continue training of previous versions is unsupported
                    return null;
                return model;
            }
            catch
            {
                return null;
            }
        }

        private void btnEval_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to evaluate the model?", "Confirm",
                        MessageBoxButtons.OKCancel) != DialogResult.OK)
                    return;
                StartOperation(OP_EVAL);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDebugEval_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to generate debug info. for test dataset?", "Confirm",
                        MessageBoxButtons.OKCancel) != DialogResult.OK)
                    return;
                StartOperation(OP_DEBUGEVAL);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnAyuda_Click(object sender, EventArgs e)
        {
            OpenDocumentation.Open("prediccion.html");
        }
	}
}
