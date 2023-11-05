using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LSI.Packages.Extensiones.Utilidades.UI
{
    /// <summary>
    /// Winforms controls utils
    /// </summary>
    public class ControlUtils
    {
        public static IEnumerable<T> GetControlsOfType<T>(Control root) where T : Control
        {
            var t = root as T;
            if (t != null)
                yield return t;

            if (root.Controls != null && root.Controls.Count > 0)
            {
                foreach (Control c in root.Controls)
                    foreach (var i in GetControlsOfType<T>(c))
                        yield return i;
            }
        }
    }
}
