using Artech.Common.Framework.Commands;
using LSI.Packages.Extensiones.Comandos;
using LSI.Packages.Extensiones.Comandos.Edit;
using LSI.Packages.Extensiones.Comandos.Refactoring;
using LSI.Packages.Extensiones.Comandos.Build;
using LSI.Packages.Extensiones.Comandos.KBSync;
using LSI.Packages.Extensiones.Comandos.Procedures;
using LSI.Packages.Extensiones.Comandos.Sdts;
using LSI.Packages.Extensiones.Comandos.ValidacionObjetos;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.UI;
using LSI.Packages.Extensiones.Comandos.Autocomplete;
using LSI.Packages.Extensiones.Comandos.Autocomplete.ParmsInfo;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.ModelGeneration;
using LSI.Packages.Extensiones.Comandos.Edit.AddVariable;

namespace LSI.Packages.Extensiones
{
    /// <summary>
    /// Gestion de los comandos de la extension.
    /// Aqui se revisa si los comandos de la extension estan disponibles en el momento actual
    /// y lanza su ejecucion.
    /// </summary>
    class CommandManager : CommandDelegatorBase
    {
        /// <summary>
        /// Constructor.
        /// Añade la gestion de los comandos de la extension
        /// </summary>
        public CommandManager()
        {

            // Query handlers:
            QueryHandler qTransaccionSeleccionada = new QueryHandler(QueryTransaccionSeleccionada);
            QueryHandler qInKbase = new QueryHandler(QueryInKB);
            QueryHandler qSelectionNavigator = new QueryHandler(QueryNavigatorSomethigSelected);
            QueryHandler qInWinform = new QueryHandler(QueryEnWinForm);
            QueryHandler qAlways = new QueryHandler(QueryAlwaysEnabled);

            // TODO: No hace falta crear un QueryHandler distinto para cada comando, se pueden reutilizar
            AddCommand(CommandKeys.CrearProcObtenerValor, new ExecHandler(ExecuteCommand<CrearProcObtenerValor>), new QueryHandler(QueryInKB));
            AddCommand(CommandKeys.CrearProcBorradoRegistro, new ExecHandler(ExecuteCommand<CrearProcBorradoRegistro>), new QueryHandler(QueryInKB));
            AddCommand(CommandKeys.InsertarForEach, new ExecHandler(ExecuteCommand<InsertarForEach>), new QueryHandler(QueryInKB));
            AddCommand(CommandKeys.InsertarNew, new ExecHandler(ExecuteCommand<InsertarNew>), new QueryHandler(QueryInKB));
            AddCommand(CommandKeys.CrearSdtDeTabla, new ExecHandler(ExecuteCommand<CrearSdtDeTabla>), new QueryHandler(QueryInKB));
            AddCommand(CommandKeys.CrearProcCargaSdt, new ExecHandler(ExecuteCommand<CrearProcCargaSdt>), new QueryHandler(QueryInKB));
            AddCommand(CommandKeys.CrearProcActualizacionSdt, new ExecHandler(ExecuteCommand<CrearProcActualizacionSdt>), new QueryHandler(QueryInKB));
            AddCommand(CommandKeys.CrearProcInsercionSdt, new ExecHandler(ExecuteCommand<CrearProcInsercionSdt>), new QueryHandler(QueryInKB));
            AddCommand(CommandKeys.CrearProcInsercion, new ExecHandler(ExecuteCommand<CrearProcInsercion>), new QueryHandler(QueryInKB));
            AddCommand(CommandKeys.CrearProcAsignarAtributo, new ExecHandler(ExecuteCommand<CrearProcAsignarAtributo>), new QueryHandler(QueryInKB));
            AddCommand(CommandKeys.TamMaximoWinform, new ExecHandler(ExecuteCommand<TamMaximoWinform>), qInWinform);
            AddCommand(CommandKeys.RevisarFuentes, new ExecHandler(ExecuteCommand<RevisarFuentesReports>), new QueryHandler(QueryInKB));
            AddCommand(CommandKeys.VerificarObjeto, new ExecHandler(ExecuteCommand<ValidarObjeto>), new QueryHandler(QueryEnObjeto));
            AddCommand(CommandKeys.ArreglarProblemas, new ExecHandler(ExecuteCommand<ArreglarProblemas>), new QueryHandler(QueryEnObjeto));
            AddCommand(CommandKeys.EditarConfiguracion, new ExecHandler(ExecuteCommand<EditarConfiguracion>), qAlways);

            // Copy parm rule of the selected object to clipboard
            CommandKey cmd = new CommandKey(Package.Guid, "CopyParmRule");
            AddCommand(cmd, new ExecHandler(ExecuteCommand<CopyParmRule>), new QueryHandler(QueryObjetoLlamableSeleccionado));

            // Verificar si el borrado de la transaccion actual reorganizara la base de datos:
            cmd = new CommandKey(Package.Guid, "VerificarBorradoTransaccion");
            AddCommand(cmd, new ExecHandler(ExecuteCommand<VerificarBorradoTransaccion>), qTransaccionSeleccionada);

            // Replace variables 'Attribute based on' property dialog
            cmd = new CommandKey(Package.Guid, "ReplaceVariablesAttributeBased");
            AddCommand(cmd, new ExecHandler(ExecuteCommand<ReplaceVariablesAttributeBasedWindow>), qInKbase);

            // Export versioning information about the selected objects
            cmd = new CommandKey(Package.Guid, "KBSyncExportInfo");
            AddCommand(cmd, new ExecHandler(ExecuteCommand<KBSyncExportInfo>), qSelectionNavigator);

            // Review objects versioning info on destination KB
            cmd = new CommandKey(Package.Guid, "KBSyncReviewInfo");
            AddCommand(cmd, new ExecHandler(ExecuteCommand<KBSyncReviewInfo>), qInKbase);

            // Search references toolwindow
            cmd = new CommandKey(Package.Guid, "SearchReferencesTW");
            AddCommand(cmd, new ExecHandler(ShowToolWindow<BuscarReferenciasToolWindow>), qInKbase);

            // Edit object calls toolwindow
            cmd = new CommandKey(Package.Guid, "EditCallsTW");
            AddCommand(cmd, new ExecHandler(ShowToolWindow<EditObjectCallsTW>), qInKbase);
            
            // Edit object calls toolwindow
            cmd = new CommandKey(Package.Guid, "SearchUnreferencedTW");
            AddCommand(cmd, new ExecHandler(ShowToolWindow<UnreferencedObjectsToolWindow>), qInKbase);
           
            // Search genexus sources toolwindow
            cmd = new CommandKey(Package.Guid, "SearchSourcesTW");
            AddCommand(cmd, new ExecHandler(ShowToolWindow<GxSourcesSearchTW>), qInKbase);
            
            // Search main references toolwindow
            cmd = new CommandKey(Package.Guid, "SearchMainTW");
            AddCommand(cmd, new ExecHandler(ShowToolWindow<BuscarReferenciasMain>), qInKbase);

            // Check exportable objects
            cmd = new CommandKey(Package.Guid, "CheckExportableTW");
            AddCommand(cmd, new ExecHandler(ShowToolWindow<KBSyncReviewResultTW>), qInKbase);

            // Reorder winform tab control pages
            cmd = new CommandKey(Package.Guid, "ReorderWinTab");
            AddCommand(cmd, new ExecHandler(ExecuteCommand<ReorderWinTab>), qInWinform);

            // Show work with mains
            cmd = new CommandKey(Package.Guid, "WorkWithMains");
            AddCommand(cmd, new ExecHandler(ShowToolWindow<WorkWithMains>), new QueryHandler(QueryInKB));

            // Extract selected code to new procedure
            cmd = new CommandKey(Package.Guid, "ExtractProcedure");
            AddCommand(cmd, new ExecHandler(ExtractProcedure.Execute), new QueryHandler(ExtractProcedure.Query));

            // Extract selected code to DataSelector
            cmd = new CommandKey(Package.Guid, "ExtractDataSelector");
            AddCommand(cmd, new ExecHandler(ExtractDataSelector.Execute), new QueryHandler(ExtractDataSelector.Query));

            // Open documentation
            cmd = new CommandKey(Package.Guid, "OpenDocumentation");
            AddCommand(cmd, new ExecHandler(ExecuteCommand<OpenDocumentation>), qAlways);

            // Objects modified by users
            cmd = new CommandKey(Package.Guid, "ObjectsModifiedByUser");
            AddCommand(cmd, new ExecHandler(ShowToolWindow<ObjectsModifiedByUser>), new QueryHandler(QueryInKB));

            // Add variable with fast key. TODO: Change query
            cmd = new CommandKey(Package.Guid, "AddVariableFastKey");
            AddCommand(cmd, new ExecHandler(AddVariableFastKey.AddVariable), new QueryHandler(QueryEnParteSource));

            // Autocomplete
            cmd = new CommandKey(Package.Guid, "Autocomplete");
            AddCommand(cmd, new ExecHandler(Autocomplete.Execute), new QueryHandler(Autocomplete.QueryCommandAutocomplete));

            // Display current parameter info (same query handler as autocomplete, both support the same object part types)
            cmd = new CommandKey(Package.Guid, "ParameterInfo");
            AddCommand(cmd, new ExecHandler(ParametersInfo.DisplayParameterInfo), new QueryHandler(Autocomplete.QueryCommandAutocomplete));

            // Generate model to predict code
            cmd = new CommandKey(Package.Guid, "GenerateModel");
            AddCommand(cmd, new ExecHandler(ShowToolWindow<GenerateModelTW>), new QueryHandler(QueryInKB));

            // Verify selected objects
            cmd = new CommandKey(Package.Guid, "VerifySelectedObjects");
            AddCommand(cmd, new ExecHandler(ExecuteCommand<ValidateSelectedObjects>), qSelectionNavigator);

            #region Menu edit

            // Option in selection contextual menus to copy objects information as table to the clipboard
            cmd = new CommandKey(Package.Guid, ToolWindowBase.COPYOBJECTSASTABLECMD);
            AddCommand(cmd, new ExecHandler(ObjectsInfoClipboard.CopyObjectsAsTable),
                new QueryHandler(ObjectsInfoClipboard.Query));

            // Paste clipboard content as a genexus string literal
            cmd = new CommandKey(Package.Guid, "PasteAsStringLiteral");
            AddCommand(cmd, new ExecHandler(ExecuteCommand<PasteAsStringLiteral>), new QueryHandler(PasteAsStringLiteral.Query));

            // Copy enum domain values to clipboard
            cmd = new CommandKey(Package.Guid, "CopyEnumValues");
            AddCommand(cmd, new ExecHandler(CopyEnumValues.Execute), new QueryHandler(CopyEnumValues.Query));

            #endregion

        }

    }
}
