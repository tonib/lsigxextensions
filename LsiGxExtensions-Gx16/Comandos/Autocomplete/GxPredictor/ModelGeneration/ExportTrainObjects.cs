using ActiproSoftware.SyntaxEditor;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Helper;
using Artech.Architecture.UI.Framework.Language;
using Artech.FrameworkDE.Text;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.ModelGeneration
{
    /// <summary>
    /// Export procedures to CSV files to do training
    /// </summary>
    class ExportTrainObjects : IExecutable
    {

        /// <summary>
        /// Data definition
        /// </summary>
        private DataInfo DataInfo;

        /// <summary>
        /// Language info
        /// </summary>
        KbPredictorInfo KbInfo;

        /// <summary>
        /// Model generation UI
        /// </summary>
        private GenerateModelTW ToolWindow;

        /// <summary>
        /// Code documents for each object part type
        /// </summary>
        private Dictionary<ObjectPartType, Document> DocumentsPerType = new Dictionary<ObjectPartType, Document>();

		/// <summary>
		/// Directory where to export kb objects
		/// </summary>
		string ExportDirectory;

		/// <summary>
		/// Model to export
		/// </summary>
		KBModel Model;


		public ExportTrainObjects(GenerateModelTW toolWindow, DataInfo dataInfo, KBModel model)
        {
            ToolWindow = toolWindow;
            DataInfo = dataInfo;
            KbInfo = new KbPredictorInfo(Autocomplete.NamesCache, DataInfo);
			Model = model;
			ExportDirectory = Path.Combine(DataInfo.DataDirectory, Entorno.ToSafeFilename(Model.KB.Name));

			if (!Directory.Exists(ExportDirectory))
				Directory.CreateDirectory(ExportDirectory);
		}

        /// <summary>
        /// Print warning message about an object empty part
        /// </summary>
        /// <param name="o">Object part owner</param>
        /// <param name="partType">Part type</param>
        private void EmptyPartWarning(KBObject o, ObjectPartType partType)
        {
            // Warn only about procedure parts. Other parts can be empty
            if (partType.PartType != PartType.Procedure)
                return;
            ToolWindow.Logger.AddWarningLine(o.Name + " / " + partType.PartTypeName
                + " is empty");
        }

        private void ExportObjectPart(KBObject o, ObjectPartType partType)
        {
            // Export code
            try
            {				
                // Get a configured Document
                Document document = GetExportDocument(partType);

                // Set the document code
                if (!o.Parts.ContainsKey(partType.PartType))
                {
                    EmptyPartWarning(o, partType);
                    return;
                }

				// IMPORTANT: In SDPanels, Events Part has part type that is not the standard "Events" part.
				// LsiConvertPart will convert this non standard part to the standard "Events" part. So, part type will have been changed!
				SourcePart sourcePart = o.Parts.LsiConvertPart(o.Parts[partType.PartType]) as SourcePart;
                if(sourcePart == null)
                {
                    EmptyPartWarning(o, partType);
                    return;
                }
                document.Text = sourcePart.Source;

                // Tokenize code
                var tokenizer = new CodeTokenizer(KbInfo, sourcePart, ToolWindow);
                TokenizedPart tp = tokenizer.Tokenize(document);
                if (tp.Tokens.Count > 0)
                    tp.SerializeToFile(KbInfo.DataInfo, ExportDirectory);
                else
                    EmptyPartWarning(o, partType);

                // Clean document
                document.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                ToolWindow.Logger.AddErrorLine(o.Name + ": " + ex.Message + Environment.NewLine + ex.ToString());
            }
        }

        /// <summary>
        /// Export kb objects of a given type
        /// </summary>
        /// <param name="objClass">Object type</param>
        private void ExportObjectsOfType(Guid objClass)
        {
			string objClassName = ObjClassLsi.GetClassName(objClass);
			ToolWindow.Logger.AddLine("Exporting objects of type: " + objClassName);

            // Get parts to export
            List<ObjectPartType> exportPartTypes = DataInfo.SupportedPartTypes
                .Where(x => x.ObjectType == objClass)
                .ToList();

            int nObjectsExported = 0;
            foreach (KBObject o in Model.Objects.GetAll(objClass))
            {
                // Export object parts
                foreach (ObjectPartType partType in exportPartTypes)
                    ExportObjectPart(o, partType);
                nObjectsExported++;

                if (nObjectsExported % 100 == 0)
                    ToolWindow.Logger.AddLine(objClassName + ": " + nObjectsExported + " exported");

                if (DataInfo.NObjectsToExport > 0 && nObjectsExported >= DataInfo.NObjectsToExport)
                    break;

                if (ToolWindow.Worker.CancellationPending)
                    return;
            }
        }

        public void Execute()
        {

            /*
            // Recreate the directory (fuck: https://stackoverflow.com/questions/34981143/is-directory-delete-create-synchronous)
            if (Directory.Exists(ExportDir))
                Directory.Delete(ExportDir, true);
            while (Directory.Exists(ExportDir))
                Thread.Sleep(500);
            Directory.CreateDirectory(ExportDir);

            string debugExportDir = Path.Combine(ExportDir, "Debug");
            Directory.CreateDirectory(debugExportDir);*/

            ToolWindow.Logger.AddLine("Exporting to " + ExportDirectory);

            // Get object types to review
            List<Guid> objectTypes = DataInfo.SupportedPartTypes
                .Select(x => x.ObjectType)
                .Distinct()
                .ToList();

            foreach (Guid objClass in objectTypes)
            {
                ExportObjectsOfType(objClass);

                if (ToolWindow.Worker.CancellationPending)
                    return;
            }

            // Write column models info to file
            DataInfo.SerializeToDirectory();
        }

        /// <summary>
        /// Get configured document to parse tokens of procedures to exports 
        /// </summary>
        /// <returns>Configured document</returns>
        private Document GetExportDocument(ObjectPartType partType)
        {
            Document document;
            if (DocumentsPerType.TryGetValue(partType, out document))
                return document;

            document = new Document();
            GenericOutlinerParser parser = new GenericOutlinerParser();
            parser.Document = document;

			// PartTypeHelper.GetLanguageInfoFor return null for SDPanel (Ev3U3), use WebPanel instead (seems equivalent)
			ObjectPartType docPartTye = partType;
			if (docPartTye == ObjectPartType.SDPanelEvents)
				docPartTye = ObjectPartType.WebPanelEvents;
			else if(docPartTye == ObjectPartType.SDPanelRules)
				docPartTye = ObjectPartType.WebPanelRules;
			else if (docPartTye == ObjectPartType.SDPanelConditions)
				docPartTye = ObjectPartType.WebPanelConditions;
			ILanguageInfo languageInfo = PartTypeHelper.GetLanguageInfoFor(docPartTye.ObjectType, docPartTye.PartType);
            document.LoadLanguageFromXml(languageInfo.Colorizer.ConfigurationFile, 0);

            // TODO: Do not export wwplus parts

            DocumentsPerType.Add(partType, document);
            return document;
        }

    }
}
