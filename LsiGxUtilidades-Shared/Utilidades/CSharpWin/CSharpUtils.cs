using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Artech.Architecture.Common.Events;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Entities;
using Artech.Genexus.Common.Services;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.CSharpWin;
using LSI.Packages.Extensiones.Utilidades.UI;
using Microsoft.Practices.CompositeUI.EventBroker;
using System.Diagnostics;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.ModelParts;
using Artech.Genexus.Common.Helpers;

namespace LSI.Packages.Extensiones.Utilidades.CSharpWin
{
    /// <summary>
    /// C# generator utils
    /// </summary>
    public class CSharpUtils
    {

        /// <summary>
        /// Commons dll name without extension
        /// </summary>
        public const string COMMONSNAME = "GeneXus.Programs.Common";

        /// <summary>
        /// Commons DLL file name
        /// </summary>
        public const string COMMONSDLLFILENAME = COMMONSNAME + ".dll";

        /// <summary>
        /// Commons RSP file name
        /// </summary>
        public const string COMMONSRSPFILENAME = COMMONSNAME + ".rsp";

        /// <summary>
        /// Get a win main exe file path
        /// </summary>
        /// <param name="main">Main object</param>
        /// <returns>Absolute exe file path of the main object</returns>
        static public string GetWinMainExeFileName(KBObject main) 
        {
            return Path.Combine( Entorno.GetTargetDirectoryFilePath("bin", main.KB.DesignModel.Environment.TargetModel) , 
                KBaseGX.GetProgramFileName(main, MainsGx.GetMainGenerator(main), true, "exe") 
                );
        }

        /// <summary>
        /// C# compiler has defined the "/debug" compiler flag
        /// </summary>
        /// <param name="generator">Generator to check</param>
        /// <returns>True if the compiler flags contains the debug option</returns>
        static public bool GeneratorHasDebugOption(GxEnvironment generator)
        {
            string compilerOptions =
                generator.Properties.GetPropertyValue(Properties.CSHARP.CompilerFlags) as string;
            return compilerOptions != null && compilerOptions.ToLower().Contains("/debug");
        }

    }
}
