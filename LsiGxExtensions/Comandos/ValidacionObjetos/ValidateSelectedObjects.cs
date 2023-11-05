using Artech.Architecture.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LSI.Packages.Extensiones.Comandos.ValidacionObjetos
{
    /// <summary>
    /// Executes validations over the set of currently selected objects
    /// </summary>
    class ValidateSelectedObjects : IExecutable
    {
        private void ExecuteInternal()
        {
            using (Log log = new Log())
            {
                List<KBObject> selection = Entorno.NavigatorSelectedObjects;
                if (selection == null || selection.Count == 0)
                {
                    log.Output.AddWarningLine("There are no selected objects");
                    Log.MostrarVentana();
                    return;
                }

                log.StartTimeCount();
                int nObjects = 0;
                foreach(KBObject o in selection)
                {
                    ValidationTask validation = new ValidationTask(o, false, false);
                    ValidationService.Service.CreateRegisteredValidators(validation);
                    validation.Execute();
                    nObjects += 1;
                }

                log.Output.AddLine(nObjects + " objects checked");
            }
        }

        /// <summary>
        /// Run validations over selected objects in a new thread
        /// </summary>
        public void Execute()
        {
            new Thread(this.ExecuteInternal).Start();
        }
    }
}
