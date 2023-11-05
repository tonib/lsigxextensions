using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Genexus.Common.Parts;
using Artech.Architecture.Common.Objects;
using Artech.Udm.Framework.References;
using Artech.Udm.Framework;
using Artech.Genexus.Common;

namespace LSI.Packages.Extensiones.Utilidades
{
    /// <summary>
    /// Utilidades sobre partes de un objeto genexus
    /// </summary>
    public class ParteObjetoKb
    {

        // TODO: Borrar esta clase, se puede hacer con una linea de linq
        /// <summary>
        /// Devuelve cierto si una parte de un objeto referencia una entidad
        /// </summary>
        /// <param name="parte">Parte del objeto a revisar</param>
        /// <param name="claveEntidad">Clave de la entidad a ver si es referenciada</param>
        /// <returns>Cierto si la parte referencia a la entidad indicada</returns>
        static public bool ReferenciaEntidad(KBObjectPart parte, EntityKey claveEntidad)
        {
            foreach (EntityReference r in parte.GetPartReferences())
            {
                if (r.To == claveEntidad)
                    return true;
            }
            return false;
        }

    }
}
