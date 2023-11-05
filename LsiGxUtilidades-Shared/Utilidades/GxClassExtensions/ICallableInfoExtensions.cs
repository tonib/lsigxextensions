using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Objects;
using Artech.Patterns.WorkWithDevices.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{
    /// <summary>
    /// KBObject class extensions
    /// </summary>
    static public class ICallableInfoExtensions
    {

        /// <summary>
        /// Get parameters of default signature, with the parameter object typed info loaded (fixes bugs with SDPanels and DataSelectors)
        /// </summary>
        /// <param name="o">Callable object from which to get parameters</param>
        /// <returns>The parameters</returns>
        static public IEnumerable<Parameter> LsiGetParametersWithTypeInfo(this ICallableInfo o)
        {
            DataSelector ds = o as DataSelector;
            if (ds != null)
                return ds.LsiGetParameters();

            SDPanel sdPanel = o as SDPanel;
            if (sdPanel != null)
                return sdPanel.LsiGetParameters();

            Signature signature = o.GetSignatures().FirstOrDefault();
            if (signature == null)
                return new List<Parameter>();
            return signature.Parameters;
        }
    }
}
