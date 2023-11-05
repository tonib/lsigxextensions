using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames.Sdts;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.Validation;
using System;

namespace LSI.Packages.Extensiones.Comandos.ValidacionObjetos
{

    /// <summary>
    /// Executes validations over the current object, on a new thread
    /// </summary>
    public class ValidarObjeto : IExecutable
    {
        // TODO: REMOVE THIS, just for testing
        void Pruebas()
		{
			try
			{
                SDT sdt = SDT.Get(UIServices.KB.CurrentModel, new QualifiedName(ObjClass.SDT, "TestModule.SDT1"));
                var sdtInfo = new SdtStructureInfo(sdt);
            }
            catch(Exception ex)
			{
                Log.ShowException(ex);
			}
        }

        /// <summary>
        /// Ejecuta la validacion del objeto, en el thread llamador o en uno nuevo
        /// </summary>
        public void Execute()
        {
            try
            {
                // Pruebas();

                ValidationTask validation = new ValidationTask(UIServices.Environment.ActiveDocument.Object, true, true);
                ValidationService.Service.CreateRegisteredValidators(validation);
                validation.Execute();
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

    }
}
