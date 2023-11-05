using Artech.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{
    /// <summary>
    /// ProductVersionHelper extensions
    /// </summary>
    static public class ProductVersionHelperExtensions
    {
        /// <summary>
        /// Evolution 3
        /// </summary>
        public const int GXX_EV3 = 10;

        /// <summary>
        /// Gx 15
        /// </summary>
        public const int GX15 = 11;

        /// <summary>
        /// Gx 16
        /// </summary>
        public const int GX16 = 12;

        /// <summary>
        /// Returns the major number version of running Genexus
        /// </summary>
        /// <returns></returns>
        public static int LsiMajorVersion(this ProductVersionHelper productVersionHelper)
        {
            try
            {
                return new Version(productVersionHelper.VersionNumber).Major;
            }
            catch
            {
                return GXX_EV3;
            }
        }

        static public int MajorVersion() {
            return ProductVersionHelper.Info.LsiMajorVersion();
        }
    }
}
