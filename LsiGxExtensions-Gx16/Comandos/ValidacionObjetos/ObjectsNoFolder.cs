using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Properties;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades.Validation;

namespace LSI.Packages.Extensiones.Comandos.ValidacionObjetos
{
    /// <summary>
    /// Check objects with no folder assigned
    /// </summary>
    public class ObjectsNoFolder : IValidator
    {

        /// <summary>
        /// Execute the verification
        /// </summary>
        public void Validate(ValidationTask task)
        {
            if (!LsiExtensionsConfiguration.Load().CheckObjectsWithNoFolder)
                return;

            // Do no check folders / modules / domains
            if (task.ObjectToCheck is Folder || task.ObjectToCheck is Module || 
                task.ObjectToCheck is Domain)
                return;

            // Do not check DataView indices. They seems to have a browsable property for Folder, but they don't really have it...
            if (task.ObjectToCheck is DataViewIndex)
                return;

            // Check if the object has a browsable and editable property for folder/module:
            PropertyManager.PropertySpecDescriptor p = task.ObjectToCheck.GetPropertyDescriptor(Properties.PRC.Folder);
            if( p == null || !p.IsBrowsable || p.IsReadOnly)
                return;

            // Get the owner module / folder
            KBObjectReference folderRef = task.ObjectToCheck.GetPropertyValue(Properties.PRC.Folder) 
                as KBObjectReference;
            if (folderRef == null)
            {
                printWarning(task);
                return;
            }

            // Check the owner module / folder
            Module root = Module.GetRoot(UIServices.KB.CurrentModel);
            if (root != null && root.Key == folderRef.EntityKey)
                printWarning(task);
        }

        private void printWarning(ValidationTask task)
        {
            task.InitializeOutput();
            task.Log.Output.AddWarningLine("Object " + task.ObjectToCheck.QualifiedName + " has no folder/module assigned");
        }
    }
}
