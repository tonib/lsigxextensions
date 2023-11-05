using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Drawing;
using LSI.Packages.Extensiones.Utilidades.VS;
using LSI.Packages.Extensiones.Utilidades.Compression;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace LSI.Packages.Extensiones
{
    /// <summary>
    /// Extensions configuration
    /// </summary>
    public class LsiExtensionsConfiguration
    {

        /// <summary>
        /// When to display extended parameter info
        /// </summary>
        public enum ParmInfoType
        {
            /// <summary>
            /// Never
            /// </summary>
            Never = 0,

            /// <summary>
            /// Only when genexus don't display it
            /// </summary>
            OnlyWhenNoGenexus = 1,

            /// <summary>
            /// Always (replaces Genexus parm. info)
            /// </summary>
            Always = 2
        }

		/// <summary>
		/// Use prediction model?
		/// </summary>
		public enum PredictionModelTypes
		{
			DoNotUse = 0,

			/// <summary>
			/// Use TF Lite model distributed with the extensions DLL
			/// </summary>
			UseDistributed = 1,

			/// <summary>
			/// Use a custom TF Lite model
			/// </summary>
			UseCustomModelTfLite = 2,

			/// <summary>
			/// Use a full TF model with Python server
			/// </summary>
			UseCustomFullTf = 3
		}

        /// <summary>
        /// Cache to check if LSI private extensions are installed
        /// </summary>
        static private bool? PrivateExtensionsInstalledCache = null;

        /// <summary>
        /// Clave del usuario en el registro en la que se guarda la configuracion de la extension
        /// </summary>
        private const string CLAVEREGISTRO = "LsiExtensions";

        /// <summary>
        /// Private extensions package name
        /// </summary>
        private const string PRIVATEEXTENSIONSPACKAGE = "LSI.Packages.PrivateExtensions";

        /// <summary>
        /// Default value for ZipFilesToAddAlways property
        /// </summary>
        private const string DEFAULTADDZIPFILES = "GeneXus.Programs.Common.dll;xExterno20_X1.dll";

        /// <summary>
        /// Cache con la ultima configuracion leida.
        /// </summary>
        static private LsiExtensionsConfiguration CfgCache;

        /// <summary>
        /// Indica si hay que revisar los objetos al guardarlos
        /// </summary>
        public bool RevisarObjetosAlGuardar = true;

        /// <summary>
        /// Indica si hay que revisar si hay variables N(4) al guardar
        /// </summary>
        public bool RevisarVariablesN4 = false;

        /// <summary>
        /// Indica si hay que revisar la lectura/escritura de variables al guardar
        /// </summary>
        public bool RevisarLecturasEscrituras = true;

        /// <summary>
        /// Indica si hay que revisar el tamaño de los winforms, para que no superen
        /// el tamaño maximo
        /// </summary>
        public bool RevisarTamanyoWinforms = false;

        /// <summary>
        /// El tamaño maximo permitido para los winforms
        /// </summary>
        public Size TamMaximoWinforms;

        /// <summary>
        /// Indica si hay que avisar si hay reglas hidden en un win/webform
        /// </summary>
        public bool RevisarReglasHidden = true;

        /// <summary>
        /// Indica si hay que avisar si hay variables no usadas
        /// </summary>
        public bool RevisarVariablesNoUsadas = true;

        /// <summary>
        /// Indica si hay que avisar si hay variables autodefinidas
        /// </summary>
        public bool RevisarVariablesAutodefinidas = false;

        /// <summary>
        /// Avisar si hay variables solo leidas?
        /// </summary>
        public bool RevisarVariablesSoloLeidas = true;

        /// <summary>
        /// Report read only variables with initial value?
        /// It's only applied if RevisarVariablesSoloLeidas it's true
        /// </summary>
        public bool ReportVariablesROWithInitialValue = true;

        /// <summary>
        /// Avisar si hay variables solo escritas?
        /// </summary>
        public bool RevisarVariablesSoloEscritas = true;

        /// <summary>
        /// Avisar si el nº de parametros en llamadas es incorrecto?
        /// </summary>
        public bool RevisarParametrosLlamadas = true;

        /// <summary>
        /// Avisar si hay parametros out no asignados?
        /// </summary>
        public bool RevisarParametrosOut = true;

        /// <summary>
        /// Revisar si hay variables solo usadas en la UI/conditions
        /// </summary>
        public bool RevisarVariablesSoloUI = true;

        /// <summary>
        /// Revisar variables solo usadas en la regla parm?
        /// </summary>
        public bool RevisarVariablesSoloReglaParm = true;

        /// <summary>
        /// Revisar columnas invisibles no usadas en grids?
        /// </summary>
        public bool RevisarColumnasInvisiblesGrids = true;

        /// <summary>
        /// Check attributes referenced outside FOR EACHs / NEWs
        /// </summary>
        public bool CheckOrphanAttributes = true;

		/// <summary>
		/// Only applies if CheckOrphanAttributes is true. Should we report attributes inside SUBs?
		/// </summary>
		/// <remarks>See https://sourceforge.net/p/lsigxextensions/tickets/4/ </remarks>
		public bool CheckOrphanInsideSubs = true;

		/// <summary>
		/// Check layout printblocks non printed?
		/// </summary>
		public bool CheckUnreferendPrintBlocks = true;

        /// <summary>
        /// Check objects with no folder assigned?
        /// </summary>
        public bool CheckObjectsWithNoFolder = true;

        /// <summary>
        /// Last kb sync exported file path
        /// </summary>
        public string LastKBSyncExportPath = string.Empty;

        /// <summary>
        /// Visual studio COM string identifier
        /// </summary>
        public string VisualStudioComId;

        /// <summary>
        /// Compressor definitions
        /// </summary>
        public Compressor Compressor;

        /// <summary>
        /// Work with mains: C# / Win generador: Set bin directory as current working dir. to run a module?
        /// </summary>
        public bool SetBinAsCurrentDir = true;

        /// <summary>
        /// Folder where to place zip's modules
        /// </summary>
        public string ZipDestinationFolder;

        /// <summary>
        /// Set of files to add always when a zip with modules is generated, separated by ";"
        /// </summary>
        public string ZipFilesToAddAlways;

        /// <summary>
        /// Add System.Environment.Exit(0) call to win / c# main objects when the custom
        /// compilation is executed?
        /// </summary>
        public bool PatchWinMainExits;

        /// <summary>
        /// Text editor path
        /// </summary>
        public string TextEditor;

        /// <summary>
        /// Prefixes for variables names always null, separated by ";"
        /// </summary>
        public string AlwaysNullVariablesPrefix;

        /// <summary>
        /// Report variables always null (with a prefix in AlwaysNullVariablesPrefix), but with 
        /// initial value?
        /// </summary>
        public bool ReportAlwaysNullWithInitialValue;

        /// <summary>
        /// Report deprecated referenced objects?
        /// </summary>
        /// <remarks>
        /// Deprecated objects are those contains a text "DeprecatedObjectsDescription" on its description
        /// </remarks>
        public bool ReportDeprecatedObjects;

        /// <summary>
        /// Text to include in deprecated objects description
        /// </summary>
        public string DeprecatedObjectsDescription;

        /// <summary>
        /// Ask confirmation for Rebuild all / Rebuild main operations?
        /// </summary>
        public bool ConfirmRebuildAll;

        /// <summary>
        /// Custom autocomplete enabled?
        /// </summary>
        public bool CustomAutocomplete = false;

        /// <summary>
        /// Autocomplete: Uppercase keywords?
        /// </summary>
        public bool UppercaseKeywords = false;

        /// <summary>
        /// Add options to create new variables in variables autocomplete?
        /// </summary>
        public bool AutocompleteCreateVariables = true;

        /// <summary>
        /// Autocomplete: Use prediction model for autocompletion?
        /// </summary>
        public PredictionModelTypes PredictionModelType = PredictionModelTypes.DoNotUse;

		/// <summary>
		/// Path to the prediction model directory. Only if PredictionModelType = PredictionModelTypes.UseCustomModel
		/// </summary>
		public string CustomModelPath = string.Empty;

        /// <summary>
        /// Autocomplete: Debug prediction model?
        /// </summary>
        public bool DebugPredictionModel = false;

        /// <summary>
        /// Location for prediction model Python scripts directory
        /// </summary>
        public string PredictionPythonScriptsDir = string.Empty;

        /// <summary>
        /// Location for Python virtualenv directory
        /// </summary>
        public string PythonVirtualEnvDir = string.Empty;

        /// <summary>
        /// When to dsiplay extended parameter info?
        /// </summary>
        public ParmInfoType ExtendedParmInfoType;

        /// <summary>
        /// Autoclose parenthesis and string quotes?
        /// </summary>
        public bool AutocloseParenthesis;

        /// <summary>
        /// Variable names to not check in any object. Comma separated, no ampersand, any case. Not null!
        /// </summary>
        public string VariableNamesNoCheck = string.Empty;

        /// <summary>
        /// Variable names to not check in any object, lowercase
        /// </summary>
        public string[] VariableNamesNoCheckArray
		{
            get
			{
                return VariableNamesNoCheck.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(n => n.Trim().ToLower())
                    .ToArray();
            }
		}

        /// <summary>
        /// Carga la configuracion de las extensiones desde el registro de windows.
        /// </summary>
        /// <returns>La configuracion leida</returns>
        static public LsiExtensionsConfiguration LoadFromRegistry()
        {
            LsiExtensionsConfiguration cfg = new LsiExtensionsConfiguration();
            try
            {
                RegistryKey registry = Registry.CurrentUser.CreateSubKey(CLAVEREGISTRO);
                cfg.RevisarObjetosAlGuardar = bool.Parse((string)registry.GetValue("RevisarVariablesAlGuardar", bool.FalseString));
                cfg.RevisarVariablesN4 = bool.Parse((string)registry.GetValue("RevisarVariablesN4", bool.FalseString));
                cfg.RevisarLecturasEscrituras = bool.Parse((string)registry.GetValue("RevisarLecturasEscrituras", bool.TrueString));
                cfg.RevisarTamanyoWinforms = bool.Parse((string)registry.GetValue("RevisarTamanyoWinforms", bool.FalseString));
                cfg.RevisarReglasHidden = bool.Parse((string)registry.GetValue("RevisarReglasHidden", bool.TrueString));
                cfg.RevisarVariablesNoUsadas = bool.Parse((string)registry.GetValue("RevisarVariablesNoUsadas", bool.TrueString));
                cfg.RevisarVariablesAutodefinidas = bool.Parse((string)registry.GetValue("RevisarVariablesAutodefinidas", bool.FalseString));
                cfg.RevisarVariablesSoloLeidas = bool.Parse((string)registry.GetValue("RevisarVariablesSoloLeidas", bool.TrueString));
                cfg.ReportVariablesROWithInitialValue = bool.Parse((string)registry.GetValue("ReportVariablesROWithInitialValue", bool.TrueString));
                cfg.RevisarVariablesSoloEscritas = bool.Parse((string)registry.GetValue("RevisarVariablesSoloEscritas", bool.TrueString));
                cfg.RevisarParametrosLlamadas = bool.Parse((string)registry.GetValue("RevisarParametrosLlamadas", bool.TrueString));
                cfg.RevisarParametrosOut = bool.Parse((string)registry.GetValue("RevisarParametrosOut", bool.TrueString));
                cfg.RevisarVariablesSoloUI = bool.Parse((string)registry.GetValue("RevisarVariablesSoloUI", bool.TrueString));
                cfg.RevisarVariablesSoloReglaParm = bool.Parse((string)registry.GetValue("RevisarVariablesSoloReglaParm", bool.TrueString));
                cfg.RevisarColumnasInvisiblesGrids = bool.Parse((string)registry.GetValue("RevisarColumnasInvisiblesGrids", bool.TrueString));
                cfg.CheckOrphanAttributes = bool.Parse((string)registry.GetValue("RevisarAtributosHuerfanos", bool.TrueString));
				cfg.CheckOrphanInsideSubs = bool.Parse((string)registry.GetValue("CheckOrphanInsideSubs", bool.TrueString));
				cfg.CheckUnreferendPrintBlocks = bool.Parse((string)registry.GetValue("CheckUnreferendPrintBlocks", bool.TrueString));
                cfg.LastKBSyncExportPath = (string)registry.GetValue("LastKBSyncExportPath", string.Empty);
                cfg.VisualStudioComId = (string)registry.GetValue("VisualStudioComId", VisualStudio.VS_2008_COMID);
                cfg.Compressor = Compressor.LoadFromRegistry(registry);
                cfg.SetBinAsCurrentDir = bool.Parse((string)registry.GetValue("SetBinAsCurrentDir", bool.TrueString));
                cfg.ZipDestinationFolder = (string)registry.GetValue("ZipDestinationFolder", @"\\192.168.42.251\soporte\archivos");
                cfg.PatchWinMainExits = bool.Parse((string)registry.GetValue("PatchWinMainExits", bool.FalseString));
                cfg.TextEditor = (string)registry.GetValue("TextEditor", @"C:\Program Files (x86)\Notepad++\notepad++.exe");
                cfg.ZipFilesToAddAlways = (string)registry.GetValue("ZipFilesToAddAlways", DEFAULTADDZIPFILES);
                cfg.CheckObjectsWithNoFolder = bool.Parse((string)registry.GetValue("CheckObjectsWithNoFolder", bool.TrueString));
                cfg.AlwaysNullVariablesPrefix = (string)registry.GetValue("AlwaysNullVariablesPrefix", "z; sz");
                cfg.ReportAlwaysNullWithInitialValue = bool.Parse((string)registry.GetValue("ReportAlwaysNullWithInitialValue", bool.TrueString));
                cfg.ReportDeprecatedObjects = bool.Parse((string)registry.GetValue("ReportDeprecatedObjects", bool.FalseString));
                cfg.DeprecatedObjectsDescription = (string)registry.GetValue("DeprecatedObjectsDescription ", "[DEPRECATED]");
                cfg.ConfirmRebuildAll = bool.Parse((string)registry.GetValue("ConfirmRebuildAll", bool.TrueString));
                cfg.CustomAutocomplete = bool.Parse((string)registry.GetValue("CustomAutocomplete", bool.FalseString));
                cfg.UppercaseKeywords = bool.Parse((string)registry.GetValue("UppercaseKeywords", bool.FalseString));
                cfg.AutocompleteCreateVariables = bool.Parse((string)registry.GetValue("AutocompleteCreateVariables", bool.TrueString));
                cfg.DebugPredictionModel = bool.Parse((string)registry.GetValue("DebugPredictionModel", bool.FalseString));
                cfg.PredictionPythonScriptsDir = (string)registry.GetValue("PredictionPythonScriptsDir", string.Empty);
                cfg.PythonVirtualEnvDir = (string)registry.GetValue("PythonVirtualEnvDir", string.Empty);
                cfg.ExtendedParmInfoType = (ParmInfoType)registry.GetValue("ExtendedParmInfoType", 0);
                cfg.AutocloseParenthesis = bool.Parse((string)registry.GetValue("AutocloseParenthesis", bool.TrueString));
				cfg.CustomModelPath = (string)registry.GetValue("CustomModelPath");
                cfg.VariableNamesNoCheck = (string)registry.GetValue("VariableNamesNoCheck", string.Empty);


                int x = int.Parse((string)registry.GetValue("TamMaximoWinformsX", "1257"));
                int y = int.Parse((string)registry.GetValue("TamMaximoWinformsY", "585"));
                cfg.TamMaximoWinforms = new Size(x, y);

				string predictionModelType = (string)registry.GetValue("PredictionModelType", PredictionModelTypes.DoNotUse.ToString());
				cfg.PredictionModelType = (PredictionModelTypes) Enum.Parse(typeof(PredictionModelTypes), predictionModelType);

				registry.Close();

            }
            catch { }

            return cfg;
        }

        /// <summary>
        /// Carga de la cache la configuracion de las extensiones.
        /// </summary>
        /// <returns>La configuracion leida</returns>
        static public LsiExtensionsConfiguration Load()
        {
            if (CfgCache == null)
                CfgCache = LoadFromRegistry();

            return CfgCache;
        }

        /// <summary>
        /// Borra la configuracion del registro de windows
        /// </summary>
        static public void BorrarDelRegistro()
        {
            Registry.CurrentUser.DeleteSubKey(CLAVEREGISTRO, false);
            CfgCache = null;
        }

        /// <summary>
        /// Guardar la configuracion de las extensiones en el registro de windows
        /// </summary>
        public void GuardarEnRegistro()
        {
            CfgCache = this;

            RegistryKey registro = Registry.CurrentUser.CreateSubKey(CLAVEREGISTRO);
            registro.SetValue("RevisarVariablesAlGuardar", RevisarObjetosAlGuardar.ToString());
            registro.SetValue("RevisarLecturasEscrituras", RevisarLecturasEscrituras.ToString());
            registro.SetValue("RevisarVariablesN4", RevisarVariablesN4.ToString());
            registro.SetValue("RevisarTamanyoWinforms", RevisarTamanyoWinforms.ToString());
            registro.SetValue("TamMaximoWinformsX", TamMaximoWinforms.Width.ToString());
            registro.SetValue("TamMaximoWinformsY", TamMaximoWinforms.Height.ToString());
            registro.SetValue("RevisarVariablesNoUsadas", RevisarVariablesNoUsadas.ToString());
            registro.SetValue("RevisarReglasHidden", RevisarReglasHidden.ToString());
            registro.SetValue("RevisarVariablesAutodefinidas", RevisarVariablesAutodefinidas.ToString());
            registro.SetValue("RevisarVariablesSoloUI", RevisarVariablesSoloUI.ToString());
            registro.SetValue("RevisarVariablesSoloReglaParm", RevisarVariablesSoloReglaParm.ToString());
            registro.SetValue("RevisarColumnasInvisiblesGrids", RevisarColumnasInvisiblesGrids.ToString());
            registro.SetValue("RevisarVariablesSoloLeidas", RevisarVariablesSoloLeidas.ToString());
            registro.SetValue("ReportVariablesROWithInitialValue", ReportVariablesROWithInitialValue.ToString());
            registro.SetValue("RevisarVariablesSoloEscritas", RevisarVariablesSoloEscritas.ToString());
            registro.SetValue("RevisarParametrosLlamadas", RevisarParametrosLlamadas.ToString());
            registro.SetValue("RevisarParametrosOut", RevisarParametrosOut.ToString());
            registro.SetValue("RevisarAtributosHuerfanos", CheckOrphanAttributes.ToString());
			registro.SetValue("CheckOrphanInsideSubs", CheckOrphanInsideSubs.ToString());
			registro.SetValue("CheckUnreferendPrintBlocks", CheckUnreferendPrintBlocks.ToString());
            registro.SetValue("LastKBSyncExportPath", LastKBSyncExportPath);
            registro.SetValue("VisualStudioComId", VisualStudioComId);
            Compressor.StoreAtRegistry(registro);
            registro.SetValue("SetBinAsCurrentDir", SetBinAsCurrentDir.ToString());
            registro.SetValue("ZipDestinationFolder", ZipDestinationFolder);
            registro.SetValue("PatchWinMainExits", PatchWinMainExits.ToString());
            registro.SetValue("TextEditor", TextEditor);
            registro.SetValue("ZipFilesToAddAlways", ZipFilesToAddAlways);
            registro.SetValue("CheckObjectsWithNoFolder", CheckObjectsWithNoFolder);
            registro.SetValue("AlwaysNullVariablesPrefix", AlwaysNullVariablesPrefix);
            registro.SetValue("ReportDeprecatedObjects", ReportDeprecatedObjects.ToString());
            registro.SetValue("DeprecatedObjectsDescription", DeprecatedObjectsDescription.ToString());
            registro.SetValue("ConfirmRebuildAll", ConfirmRebuildAll.ToString());
            registro.SetValue("CustomAutocomplete", CustomAutocomplete);
            registro.SetValue("UppercaseKeywords", UppercaseKeywords);
            registro.SetValue("AutocompleteCreateVariables", AutocompleteCreateVariables);
            registro.SetValue("PredictionModelType", PredictionModelType.ToString());
			registro.SetValue("CustomModelPath", CustomModelPath);
			registro.SetValue("DebugPredictionModel", DebugPredictionModel);
            registro.SetValue("PredictionPythonScriptsDir", PredictionPythonScriptsDir);
            registro.SetValue("PythonVirtualEnvDir", PythonVirtualEnvDir);
            registro.SetValue("ExtendedParmInfoType", (int)ExtendedParmInfoType);
            registro.SetValue("AutocloseParenthesis", AutocloseParenthesis.ToString());
            registro.SetValue("VariableNamesNoCheck", VariableNamesNoCheck);

            registro.Close();
        }

        /// <summary>
        /// Check if private extensions are installed
        /// </summary>
        static public bool PrivateExtensionsInstalled
        {
            get
            {
                try
                {
                    if (PrivateExtensionsInstalledCache != null)
                        return (bool)PrivateExtensionsInstalledCache;

                    PrivateExtensionsInstalledCache = AppDomain.CurrentDomain.GetAssemblies().Any(x => x.FullName.Contains(PRIVATEEXTENSIONSPACKAGE));
                    return (bool)PrivateExtensionsInstalledCache;
                }
                catch
                {
                    return false;
                }
            }
        }

        public void OpenFileWithTextEditor(string filePath)
        {
            if (!File.Exists(TextEditor))
            {
                MessageBox.Show("The text editor specified on the configuration does not exists: " +
                    TextEditor);
                return;
            }
            if( !File.Exists(filePath) )
            {
                MessageBox.Show("File " + filePath + " does not exists");
                return;
            }

            Process.Start(TextEditor, filePath);
        }

        /// <summary>
        /// Set of files names to add always when a zip with modules is generated
        /// </summary>
        public List<string> ZipFilesToAddAlwaysList
        {
            get
            {
                return ZipFilesToAddAlways.Split(new char[] { ';' })
                    .Select(f => f.Trim())
                    .Where(f => !string.IsNullOrEmpty(f))
                    .ToList();
            }
        }

        /// <summary>
        /// Set of prefixes for variables names always null, trimmed, case sensitive
        /// </summary>
        public List<string> AlwaysNullVariablesPrefixSet
        {
            get
            {
                return AlwaysNullVariablesPrefix
                    .Split(new char[] { ';' })
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
            }
        }
    }
}
