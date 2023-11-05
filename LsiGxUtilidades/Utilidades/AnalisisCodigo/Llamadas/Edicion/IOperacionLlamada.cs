using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Common.Objects;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Llamadas.Edicion
{
    /// <summary>
    /// Operacion que ha de implementar una operacion de edicion de llamadas
    /// </summary>
    public interface IOperacionLlamada
    {

        /// <summary>
        /// Hace los cambios necesarios en el objeto llamador, una unica vez, antes de empezar a cambiar
        /// cada una de las llamadas
        /// </summary>
        /// <param name="llamador">Objeto llamador a editar</param>
        /// <param name="infoObjeto">Informacion de edicion del objeto llamador</param>
        void HacerCambiosGlobalesLlamador(KBObject llamador, InfoCambioLlamadasObjeto infoObjeto);

        /// <summary>
        /// Edita la llamada
        /// </summary>
        /// <param name="l">Llamada a editar</param>
        /// <returns>Informacion sobre el resultado del cambio en la llamada</returns>
        InfoCambioLlamada EditarLlamada(Llamada l);

    }
}
