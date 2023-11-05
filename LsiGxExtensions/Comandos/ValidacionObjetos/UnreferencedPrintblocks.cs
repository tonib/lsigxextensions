using System;
using System.Collections.Generic;
using System.Diagnostics;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Language.Parser.Data;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.Layout;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.Validation;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Comandos.ValidacionObjetos
{
    /// <summary>
    /// Tool to search unreferenced printblocks on an object
    /// </summary>
    public class UnreferencedPrintblocks : IValidator
    {

        /// <summary>
        /// Object to check
        /// </summary>
        private ValidationTask Validator;

        /// <summary>
        /// Set of printed printblocks
        /// </summary>
        private HashSet<string> ReferencedPrintBlocks;

        /// <summary>
        /// Check if a code node is a print command
        /// </summary>
        /// <param name="node">Code node to check</param>
        /// <param name="state">Parser state</param>
        private void CheckNodeCode(ObjectBase node, ParsedCodeFinder.SearchState state)
        {
            string printBlockName;
            if (node.LsiIsCmdPrint(out printBlockName))
                ReferencedPrintBlocks.Add(printBlockName);
        }

        /// <summary>
        /// Execute the verification
        /// </summary>
        public void Validate(ValidationTask task)
        {
            try
            {
                if (!LsiExtensionsConfiguration.Load().CheckUnreferendPrintBlocks)
                    return;

                Validator = task;

                // Process time watcher:
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                // Check only procedures:
                Procedure procedure = Validator.ObjectToCheck as Procedure;
                if (procedure == null)
                {
                    stopWatch.Stop();
                    return;
                }

                // Check if there is only the default band
                if (LayoutGx.OnlyDefaultBand(procedure.Layout.Layout))
                {
                    stopWatch.Stop();
                    return;
                }

                // Get report band names:
                HashSet<string> bandNames = LayoutGx.BandNames(procedure.Layout.Layout);
                
                // Get procedure part source code:
                ParsedCode code = new ParsedCode(procedure.ProcedurePart);

                // Check procedure part source code, to find PRINT command calls
                ReferencedPrintBlocks = new HashSet<string>();
                ParsedCodeFinder printsFinder = new ParsedCodeFinder(CheckNodeCode);
                printsFinder.Execute(code);

                // Get unreferenced printblocks:
                HashSet<string> unreferencedBands = new HashSet<string>();
                foreach (string bandName in bandNames)
                {
                    if (!ReferencedPrintBlocks.Contains(bandName))
                        unreferencedBands.Add(bandName);
                }

                stopWatch.Stop();

                if (unreferencedBands.Count > 0)
                {
                    // Report unreferenced bands
                    Validator.InitializeOutput();
                    Validator.Log.Output.AddWarningLine("Object " + task.ObjectToCheck.QualifiedName + ": There are unreferenced printblocks:");
                    foreach (string bandName in unreferencedBands)
                        Validator.Log.Output.AddLine(bandName);
                    Validator.Log.PrintExecutionTime(stopWatch);
                }
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }
    }
}
