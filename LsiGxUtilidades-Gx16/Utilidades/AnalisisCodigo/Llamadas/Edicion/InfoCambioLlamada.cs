using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Llamadas.Edicion
{
    /// <summary>
    /// Informacion sobre una substitucion en una llamada
    /// </summary>
    public class InfoCambioLlamada
    {

        /// <summary>
        /// Estado en que quedo el cambio de las llamadas en el objeto
        /// </summary>
        public Estado EstadoCambioLlamada = Estado.OK;

        /// <summary>
        /// Informacion sobre el cambio en la llamada
        /// </summary>
        public string InformacionCambio;

        /// <summary>
        /// Constructor para el cambio de una llamada
        /// </summary>
        /// <param name="l">Llamada que se esta modificando</param>
        public InfoCambioLlamada(Llamada l)
        {
            InformacionCambio = "Original call:" + Environment.NewLine + l.ToString();
        }

        /// <summary>
        /// Constructor para mostrar un mensaje de aviso / error
        /// </summary>
        /// <param name="estado">Nivel de error (aviso o error)</param>
        /// <param name="texto">Texto del error / aviso</param>
        public InfoCambioLlamada(Estado estado, string texto)
        {
            EstadoCambioLlamada = estado;
            InformacionCambio = texto;
        }

    }
}
