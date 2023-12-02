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
using static Artech.Genexus.Common.Properties.CSHARP;

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
        /// C# compiler has defined the debug options?
        /// </summary>
        /// <param name="generator">Generator to check</param>
        /// <returns>True if the compiler flags contains the debug option</returns>
#if GX_17_OR_GREATER
        static public bool GeneratorHasDebugOption(GxGenerator generator)
#else
        static public bool GeneratorHasDebugOption(GxEnvironment generator)
#endif
        {
            string errorMessage;
            return GeneratorHasDebugOption(generator, out errorMessage);
        }

        /// <summary>
        /// C# compiler has defined debug options?
        /// </summary>
        /// <param name="generator">Generator to check</param>
        /// <param name="warningMessage">Message to display to user if generator has no set compiler options</param>
        /// <returns>True if the compiler flags contains the debug option</returns>
#if GX_17_OR_GREATER
        static public bool GeneratorHasDebugOption(GxGenerator generator, out string warningMessage)
#else
        static public bool GeneratorHasDebugOption(GxEnvironment generator, out string warningMessage)
#endif
        {
            warningMessage = null;
            string buildMode = generator.Properties.GetPropertyValue<string>(Properties.CSHARP.BuildMode);

            bool msBuildMode = false;
#if GX_17_OR_GREATER
            msBuildMode = buildMode == BuildMode_Values.Msbuild;
#endif
            if(msBuildMode)
			{
#if GX_17_OR_GREATER
                string msBuildOptions = generator.Properties.GetPropertyValue<string>(Properties.CSHARP.MsbuildOptions) ?? string.Empty;
                if(!msBuildOptions.ToLower().Contains("/p:configuration=debug"))
				{
                    warningMessage = "C# generator property 'MSBuild options' does not contain '/p:Configuration=Debug': Debug may not work";
                    return false;
				}
#endif
            }
            else
			{
                string compilerOptions = generator.Properties.GetPropertyValue<string>(Properties.CSHARP.CompilerFlags) ?? string.Empty;
                if(!compilerOptions.ToLower().Contains("/debug"))
				{
                    warningMessage = "C# generator property 'Compiler options' does not contain '/debug': Debug may not work";
                    return false;
				}
            }

            return true;
        }

    }
}
