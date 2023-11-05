using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.Logging;
using Artech.Patterns.WorkWithDevices;
using Artech.Patterns.WorkWithDevices.Objects;
using Artech.Patterns.WorkWithDevices.Parts;
using Artech.Patterns.WorkWithDevices.Helpers;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Reflection;

namespace LSI.Packages.Extensiones.Utilidades.Validation
{

    /// <summary>
    /// Run verifications over an object
    /// </summary>
    public class ValidationTask : IExecutable
    {

        /// <summary>
        /// Objeto a revisar
        /// </summary>
        public KBObject ObjectToCheck;

        /// <summary>
        /// La ventana de output. Solo se inicializa si se ha escrito algun error, sino es null.
        /// </summary>
        public Log Log;

        /// <summary>
        /// Cierto si hay que ejecutar el listado en un thread aparte
        /// </summary>
        private bool EjecutarEnThread;

        /// <summary>
        /// Cierto si hay que mostrar siempre el log, aunque no haya errores
        /// </summary>
        private bool MostrarSiempreLog;

        /// <summary>
        /// Validators to run over the object
        /// </summary>
        public List<IValidator> Validators = new List<IValidator>();

        /// <summary>
        /// Constructor to validate an object. 
        /// The execution is done on the same thread, and it does not show the log if there are errors
        /// </summary>
        /// <param name="o">Object to check</param>
        public ValidationTask(KBObject o, bool executeInThread = false, bool showAlwaysLog = false)
        {
            // There is some kind of trouble with gx caching. If the object parms are modified,
            // those modifications are not stored at the parameter KBObject. 
            // So, reload the object without caching:
            ObjectToCheck = o.Model.Objects.Get(o.Key);
            EjecutarEnThread = executeInThread;
            MostrarSiempreLog = showAlwaysLog;
        }

        /// <summary>
        /// Ejecuta la validacion del objeto, en el thread llamador o en uno nuevo
        /// </summary>
        public void Execute()
        {
            if (EjecutarEnThread)
                new Thread(this.EjecucionInterna).Start();
            else
                EjecucionInterna();
        }

        /// <summary>
        /// Cierra el log, si estaba creado
        /// </summary>
        /// <param name="stopWatch">Tiempo total de ejecucion, para imprimirlo. Puede ser nulo.</param>
        private void FinalizarLog(Stopwatch stopWatch)
        {
            if (Log != null)
            {
                // Cerrar la seccion
                if (stopWatch != null)
                {
                    Log.Output.AddText(DateTime.Now.ToLongTimeString() + " - TOTAL TIME: ");
                    Log.PrintExecutionTime(stopWatch);
                }
                Log.Dispose();
            }
        }

        /// <summary>
        /// Ejecuta la validacion del objeto
        /// </summary>
        private void EjecucionInterna()
        {
            try
            {

                if (UIServices.KB.CurrentModel == null)
                    // Esto pasa cuando se esta creando una kbase: Se guarda el objeto pero el modelo 
                    // es nulo: No hacer la validacion del objeto
                    return;

                if (MostrarSiempreLog)
                    InicializarOutput(true);

                //Pruebas();

                // Ver si es un objeto no soportado:
                if (TokensFinder.IsUnsupportedObject(ObjectToCheck))
                {
                    // No revisar patters / dashboards / sdpanels... Si se referencia una variable
                    // dentro de un composite, IHasVariableReferences no encuentra las variables
                    // referenciadas.
                    // TODO: Ver si en la Tilo sigue haciendo lo mismo
                    FinalizarLog(null);
                    return;
                }

                // Calcular cuanto tiempo tarda el proceso.
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                foreach (IValidator validator in Validators)
                    validator.Validate(this);

                stopWatch.Stop();

                // Cerrar el log
                FinalizarLog(stopWatch);
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        /// <summary>
        /// Inicializa la ventana de log, si no se ha hecho ya.
        /// Registra que el objeto tiene errores.
        /// </summary>
        public void InitializeOutput()
        {
            InicializarOutput(false);
        }

        /// <summary>
        /// Inicializa la ventana de log, si no se ha hecho ya.
        /// </summary>
        /// <param name="procesoOk">Falso si el objeto tiene errores</param>
        public void InicializarOutput(bool procesoOk)
        {
            if (Log != null)
            {
                Log.ProcesoOk = procesoOk;
                // Mostrar la ventana, para indicar que hay un aviso nuevo
                Log.MostrarVentana();
                return;
            }

            Log = new Log(false);
            Log.ProcesoOk = procesoOk;
        }

        private void Pruebas()
        {
            
            SDPanel sd = ObjectToCheck as SDPanel;
            if (sd == null)
                return;

            UtilidadesReflexion.InspectPatternElement(sd.PatternPart.PanelElement);
        }
    }
}
