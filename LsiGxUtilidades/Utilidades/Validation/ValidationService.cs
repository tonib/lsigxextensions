using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using LSI.Packages.Extensiones.Utilidades.Threading;
using Artech.Architecture.Common.Objects;

namespace LSI.Packages.Extensiones.Utilidades.Validation
{
    /// <summary>
    /// Sigleton with a objects queue to validate
    /// </summary>
    public class ValidationService
    {

        /// <summary>
        /// Singleton instance
        /// </summary>
        static private ValidationService _Service;

        /// <summary>
        /// Pending validations queue
        /// </summary>
        public ColaTareas<IExecutable> TasksQueue = new ColaTareas<IExecutable>(1, 20);

        /// <summary>
        /// Validation classes to run over each object. 
        /// The classes must implement IValidator
        /// </summary>
        public HashSet<Type> RegisteredValidationOps = new HashSet<Type>();

        /// <summary>
        /// Un thread que ejecuta tareas en segundo plano. 
        /// </summary>
        static public ValidationService Service
        {
            get
            {
                if (_Service == null)
                    _Service = new ValidationService();
                return _Service;
            }
        }

        private ValidationTask CreateValidationTask(KBObject o)
        {
            ValidationTask task = new ValidationTask(o);
            CreateRegisteredValidators(task);
            return task;
        }

        public void CreateRegisteredValidators(ValidationTask task)
        {
            foreach (Type validatorClass in RegisteredValidationOps)
            {
                IValidator v = (IValidator)Activator.CreateInstance(validatorClass);
                task.Validators.Add(v);
            }
        }

        public void EnqueueObject(KBObject o)
        {
            TasksQueue.NuevaTarea(CreateValidationTask(o));
        }

    }
}
