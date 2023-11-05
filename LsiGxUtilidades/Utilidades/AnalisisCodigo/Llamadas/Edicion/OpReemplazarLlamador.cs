using System;
using System.Collections.Generic;
using Artech.Architecture.Common.Objects;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Llamadas.Edicion
{
    /// <summary>
    /// Operacion para reemplazar el objeto llamado por otro
    /// </summary>
    public class OpReemplazarLlamador : IOperacionLlamada
    {

        /// <summary>
        /// Nuevo objeto por el que reemplazar las llamadas
        /// </summary>
        private KBObject NuevoObjeto;

        public OpReemplazarLlamador(KBObject nuevoObjeto)
        {
            NuevoObjeto = nuevoObjeto;
        }

        /// <summary>
        /// Hace los cambios necesarios en el objeto llamador, una unica vez, antes de empezar a cambiar
        /// cada una de las llamadas
        /// </summary>
        /// <param name="llamador">Objeto llamador a editar</param>
        /// <param name="infoObjeto">Informacion de edicion del objeto llamador</param>
        public void HacerCambiosGlobalesLlamador(KBObject llamador, InfoCambioLlamadasObjeto infoObjeto)
        {
            // No hacer nada
        }

        /// <summary>
        /// Reemplaza el objeto llamado por el nuevo
        /// </summary>
        /// <param name="l">Llamada a editar</param>
        /// <returns>Informacion sobre el resultado del cambio en la llamada</returns>
        public InfoCambioLlamada EditarLlamada(Llamada l)
        {

            // Informacion de cambios en la llamada:
            InfoCambioLlamada infoCambioLlamada = new InfoCambioLlamada(l);

            l.CambiarObjetoLlamado(NuevoObjeto);

            infoCambioLlamada.InformacionCambio += Environment.NewLine +
                "New call:" + Environment.NewLine +
                l.NodoLlamada.ToString();

            return infoCambioLlamada;
        }
    }
}
