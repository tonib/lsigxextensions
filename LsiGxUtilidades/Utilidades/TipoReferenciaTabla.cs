using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Genexus.Common;

namespace LSI.Packages.Extensiones.Utilidades
{
    /// <summary>
    /// Informacion de operaciones sobre una referencia a una tabla. 
    /// Parsea el texto del miembro LinkTypeInfo de una EntityReference, y registra
    /// si la tabla se inserta, actualiza, lee y/o borra
    /// </summary>
    public class TipoReferenciaTabla
    {
        /// <summary>
        /// Texto con informacion sobre el tipo de opercion en una tabla.
        /// Tiene un formato del tipo "R1I0U0D0", donde R, I, U y D indican la operacion 
        /// (lectura, insercion, actualizacion y borrado) y 0,1 indican si la operacion no
        /// se hace o si se hace
        /// </summary>
        public string LinkInfo;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="linkInfo">Texto del tipo de referencia</param>
        public TipoReferenciaTabla(string linkInfo)
        {
            this.LinkInfo = linkInfo;
        }

        /// <summary>
        /// Verifica si una operacion esta presente en la referencia
        /// </summary>
        /// <param name="operacion">Operacion a revisar</param>
        /// <returns>Cierto si la operacion esta presente</returns>
        public bool OperacionPresente(TipoOperacion operacion)
        {
            switch (operacion)
            {

                case TipoOperacion.BORRADO:
                    return ReferenceTypeInfo.HasDeleteAccess(LinkInfo);
                case TipoOperacion.ESCRITURA:
                    return ReferenceTypeInfo.HasUpdateAccess(LinkInfo);
                case TipoOperacion.INSERCION:
                    return ReferenceTypeInfo.HasInsertAccess(LinkInfo);
                case TipoOperacion.LECTURA:
                    return ReferenceTypeInfo.HasReadAccess(LinkInfo);
                default:
                    return false;
            }
        }

        /// <summary>
        /// True if the table reference operations are undefined.
        /// </summary>
        /// <remarks>
        /// This happens when the object is saved and not specified yet
        /// </remarks>
        public bool OperationsAreUndefined
        {
            get
            {
                bool r, i, u, d;
                bool isBase;
                ReferenceTypeInfo.ReadTableInfo(LinkInfo, out r, out i, out u, out d, out isBase);
                return !r && !i && !u && !d;
            }
        }

        public override string ToString()
        {
            return LinkInfo;
        }
    }
}
