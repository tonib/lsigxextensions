using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Variables;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Validation;
using System.Diagnostics;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.Common.Objects;
using Artech.Udm.Framework.References;

namespace LSI.Packages.Extensiones.Comandos.ValidacionObjetos
{
    /// <summary>
    /// Check deprecated objects used
    /// </summary>
    public class DeprecatedObjectsCalled : IValidator
    {

        /// <summary>
        /// Execute the validation on the object
        /// </summary>
        public void Validate(ValidationTask task)
        {

            LsiExtensionsConfiguration cfg = LsiExtensionsConfiguration.Load();
            if(!cfg.ReportDeprecatedObjects)
                return;
            
            string deprecatedTextMarker = cfg.DeprecatedObjectsDescription.ToLower().Trim();
            if( string.IsNullOrEmpty( deprecatedTextMarker ) )
                return;

            // Calculate the execution time
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Check referenced objects:
            bool someObjectReported = false;
            KBModel currentModel = UIServices.KB.CurrentModel;
            foreach (EntityReference r in task.ObjectToCheck.GetReferences())
            {
                KBObject o = KBObject.Get(currentModel, r.To);
                if (o == null)
                    continue;
                string description = o.Description;
                if (description != null && description.ToLower().Contains(deprecatedTextMarker))
                {
                    someObjectReported = true;
                    task.InitializeOutput();
                    task.Log.Output.AddWarningLine(
                        "Object " + task.ObjectToCheck.QualifiedName + ": " + o.QualifiedName + 
                        " is deprecated");
                }
            }

            stopWatch.Stop();

            if (someObjectReported)
                task.Log.PrintExecutionTime(stopWatch);

        }

    }
}
