using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.Validation
{
    /// <summary>
    /// Interface to implement by an object validator
    /// </summary>
    public interface IValidator
    {
        void Validate(ValidationTask task);
    }
}
