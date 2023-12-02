using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.UI;
using Artech.Architecture.Common.Objects;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Llamadas.Edicion
{
    /// <summary>
    /// Informacion sobre las substituciones de llamadas a un objeto genexus
    /// </summary>
    public class InfoCambioLlamadasObjeto : RefObjetoGX
    {

        /// <summary>
        /// Estado en que quedo los cambios en en objeto
        /// </summary>
        public Estado EstadoCambiosObjeto { get; set; }

        /// <summary>
        /// Informacion sobre las llamadas a cambiar dentro del objeto
        /// </summary>
        public List<InfoCambioLlamada> EstadosLlamadas = new List<InfoCambioLlamada>();

        /// <summary>
        /// Agrega informacion sobre el cambio en una llamada, y actualiza el estado de error del objeto
        /// </summary>
        /// <param name="info">Informacion de llamada cambiada a añadir</param>
        public void AgregarEstadoLlamada(InfoCambioLlamada info)
        {
            EstadosLlamadas.Add(info);
            // Verificar cambios en el estado de cambios en el objeto
            if (info.EstadoCambioLlamada > EstadoCambiosObjeto)
                EstadoCambiosObjeto = info.EstadoCambioLlamada;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="o">El obejeto en el que se estan cambiando las llamadas</param>
        public InfoCambioLlamadasObjeto(KBObject o)
            : base(o)
        {

            EstadoCambiosObjeto = Estado.OK;
        }

    }
}
