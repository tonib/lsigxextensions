using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades;
using System.IO;

namespace LSI.Packages.Extensiones.Utilidades.VS
{
    /// <summary>
    /// C# compiler error
    /// </summary>
    public class CompilerError
    {

        /// <summary>
        /// Text added to log lines with custom errors
        /// </summary>
        public const string ERRORPREFIX = "[ERROR]";

        /// <summary>
        /// Text that contains C# compiler errors
        /// </summary>
        private const string CS_COMPILER_ERROR = "error C";

        /// <summary>
        /// Error file name, without path
        /// </summary>
        public string FileName;

        /// <summary>
        /// Get absolute file path on the current UI target environment of the current error
        /// </summary>
        /// <param name="webGenerator">True if we are using a web generator</param>
        public string GetFilePath(bool webGenerator)
        {
            string path = Entorno.TargetDirectory;
            if( webGenerator )
                path = Path.Combine(path, "web");
            return Path.Combine(path, FileName);
        }

        public int Line;

        public bool IsNameNotFoundError;

        public string NameNotFound;

        static public bool ContainsError(string logText)
        {
            return logText.Contains(CS_COMPILER_ERROR) || logText.StartsWith(ERRORPREFIX);
        }

        static public CompilerError ParseCompilerErrorMessage(string line)
        {
            try
            {
                if (!line.Contains(CS_COMPILER_ERROR))
                    return null;

                // El formato de la linea es asi:
                // procedure2.cs(91,11): error CS0103: El nombre 'achilipu' no existe en el contexto actual
                int idxParentesis = line.IndexOf("(");
                if (idxParentesis < 0)
                    return null;

                string filename = line.Substring(0, idxParentesis);
                int idxComa = line.IndexOf(",");
                if (idxComa < 0)
                    return null;

                CompilerError error = new CompilerError();
                error.Line = int.Parse(line.Substring(idxParentesis + 1, idxComa - idxParentesis - 1));
                error.FileName = filename;

                // Check if its name not found error:
                // uworkpanel1.cs(253,14): error CS0246: No se puede encontrar el tipo o el nombre de espacio de nombres 'procedure1' (¿falta una directiva using o una referencia de ensamblado?)
                if (line.Contains("CS0246"))
                {
                    error.IsNameNotFoundError = true;
                    char[] sep = { '\'' };
                    string[] parts = line.Split(sep);
                    error.NameNotFound = parts[1] + ".cs";
                    // Check if it's a SDT:
                    if (error.NameNotFound.StartsWith("Sdt"))
                        error.NameNotFound = "type_" + error.NameNotFound;
                }

                return error;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Open the file with this error in Visual Studio
        /// </summary>
        /// <param name="visualStudioComId">Visual studio COM string identifier</param>
        /// <param name="webGenerator">True if we are using a web generator</param>
        public void ShowErrorInVS(string visualStudioComId, bool webGenerator)
        {
            new VisualStudio(visualStudioComId).EditFile(GetFilePath(webGenerator), Line);
        }
    }
}
