using System;
using System.Collections.Generic;
using System.Text;
using Artech.Architecture.Common.Services;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Diagnostics;
using System.Windows.Forms;
using System.Media;
using System.Diagnostics;

namespace LSI.Packages.Extensiones.Utilidades.Logging
{

    /// <summary>
    /// Utilidad para el log de la extension.
    /// Al crear el objeto se crea una seccion, y al destruirlo (Dispose) se cierra
    /// </summary>
    public class Log : IDisposable
    {

        /// <summary>
        /// Output id for LSI extensions
        /// </summary>
        public const string LSIEXTENSIONS_OUTPUT_ID = "LSI.Extensions";

        /// <summary>
        /// Output id Genexus builds
        /// </summary>
        public const string BUILD_OUTPUT_ID = "Build";

        /// <summary>
        /// Output id for Genexus general
        /// </summary>
        public const string GENERAL_OUTPUT_ID = "General";

        /// <summary>
        /// Default title section for LSI extensions
        /// </summary>
        private const string LSIEXTENSIONS_SECTION = "LSI.Extensions";

        /// <summary>
        /// El guid de la ventana output.
        /// </summary>
        static private Guid GuidOutputTW
        {
            // TODO: This should be defined on genexus dlls. Search it.
            get { return new Guid("59CE53BC-F419-402b-AC09-AC275ED21AB9"); }
        }

        /// <summary>
        /// El log del proceso
        /// </summary>
        public IOutputService Output;

        /// <summary>
        /// Cierto si el proceso ha finalizado correctamente
        /// </summary>
        public bool ProcesoOk = true;

        /// <summary>
        /// If its not null, we are counting the elapsed process time
        /// </summary>
        private Stopwatch ElapsedTime;

        /// <summary>
        /// Constructor.
        /// Inicia la seccion del log
        /// </summary>
        /// <param name="borrarLogPrevio">Si es cierto, borra en contenido anterior la ventana de log</param>
        public Log(bool borrarLogPrevio, bool showWindow = true) : 
            this(LSIEXTENSIONS_OUTPUT_ID, borrarLogPrevio, showWindow)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="outputId">Output ID for this log messages</param>
        /*public Log(string outputId)
            : this(outputId, true)
        { }*/

        /// <summary>
        /// Constructor.
        /// Inicia la seccion del log
        /// </summary>
        /// <param name="borrarLogPrevio">Si es cierto, borra en contenido anterior la ventana de log</param>
        public Log(string outputId=LSIEXTENSIONS_SECTION, bool borrarLogPrevio=true, bool showWindow=true)
        {
            Output = CommonServices.Output;
            // Asegurase de que la ventana de output este creada:
            /*if(showWindow)
                MostrarVentana();
            Output.SelectOutput(outputId);
            Output.Show(outputId);*/
            SelectLog(outputId, showWindow);
            if (borrarLogPrevio)
                Output.Clear();
            Output.StartSection(LSIEXTENSIONS_SECTION);
        }

        static public void SelectLog(string outputId = LSIEXTENSIONS_SECTION, bool showWindow=true)
        {
            if (showWindow)
                MostrarVentana();
            CommonServices.Output.SelectOutput(outputId);
            CommonServices.Output.Show(outputId);
        }

        /// <summary>
        /// Constructor. Inicia la seccion del log, borrando el contenido anterior
        /// </summary>
        //public Log() : this(true) {}

        /// <summary>
        /// It closes the output section, and sets Output to null.
        /// </summary>
        public void CloseSection()
        {
            if (Output == null)
                return;

            if (ElapsedTime != null)
            {
                ElapsedTime.Stop();
                PrintExecutionTime(ElapsedTime);
            }

            Output.EndSection(LSIEXTENSIONS_SECTION, ProcesoOk);
            Output = null;
        }

        /// <summary>
        /// Cierra la seccion del log.
        /// </summary>
        public void Dispose() 
        {
            CloseSection();
        }

        /// <summary>
        /// It enables the time measurement for the process that uses this log.
        /// </summary>
        /// <remarks>
        /// If this function is called, when the log is disposed, it will print the execution
        /// time.
        /// </remarks>
        public void StartTimeCount()
        {
            ElapsedTime = new Stopwatch();
            ElapsedTime.Start();
        }

        /// <summary>
        /// Imprime el tiempo de ejecucion de un proceso
        /// </summary>
        /// <param name="stopWatch"></param>
        public void PrintExecutionTime(Stopwatch stopWatch)
        {
            Output.AddLine("Execution time: " + stopWatch.ElapsedMilliseconds + " ms.");
        }

        /// <summary>
        /// Fuerza que se muestre la ventana de log al usuario.
        /// </summary>
        static public void MostrarVentana()
        {
            try
            {
                // This will throw an exeption if it's executed from a msbuild file:
                UIServices.ToolWindows.FocusToolWindow(GuidOutputTW);
            }
            catch { }
        }

        /// <summary>
        /// Muestra informacion de una excepcion en la ventana de output
        /// </summary>
        /// <param name="ex"></param>
        static public void ShowException(Exception ex)
        {
            try
            {
                MostrarVentana();
                CommonServices.Output.SelectOutput(LSIEXTENSIONS_OUTPUT_ID);
                CommonServices.Output.Show(LSIEXTENSIONS_OUTPUT_ID);
                if (ex is ValidationException)
                {
                    // Mostrar errores de validacion. Sacado de http://www.gxopen.com/forumsr/servlet/viewthread?ARTECH,23,156372
                    ValidationException errorValidacion = (ValidationException)ex;
                    CommonServices.Output.AddErrorLine("Validation error");
                    CommonServices.Output.AddAll(errorValidacion.Output, false);
                }
                CommonServices.Output.AddErrorLine("Lsi.Extensions exception : " + ex.ToString());
                BeepError();
            }
            catch (Exception ex2)
            {
                MessageBox.Show("Error showing exception : " + ex2.ToString());
                MessageBox.Show("Original exception: " + ex.ToString());
            }
        }

        /// <summary>
        /// Lanza un sonido de error.
        /// </summary>
        static public void BeepError()
        {
            SystemSounds.Asterisk.Play();
        }

    }
}
