using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.UI;
using LSI.Packages.Extensiones.Utilidades.Variables;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.FrameworkDE;
using Artech.Genexus.Common.Objects;
using Artech.Udm.Framework.References;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common;
using Artech.Common.Diagnostics;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using Artech.Architecture.Language.Parser.Data;
using Artech.Udm.Framework.Multiuser;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Llamadas.Edicion
{
    /// <summary>
    /// Operacion para quitar un parametro a una llamada de un objeto
    /// </summary>
    public class OpQuitarParametro : IOperacionLlamada
    {

        /// <summary>
        /// Posicion del parametro a quitar en las llamadas.
        /// </summary>
        private int PosicionParametro;

        /// <summary>
        /// La lista de parametros del objeto llamado
        /// </summary>
        private List<ParameterElement> Parametros;

        /// <summary>
        /// Object (ICallableInfo) to edit calls
        /// </summary>
        private KBObject CalledObject;

        /// <summary>
        /// Constructor para quitar un parametro de los llamadores de un objeto
        /// </summary>
        /// <param name="o">Object (ICallableInfo) to edit calls</param>
        /// <param name="posicionParametro">Indice del parametro a quitar</param>
        public OpQuitarParametro(KBObject o, int posicionParametro)
        {
            PosicionParametro = posicionParametro;
            CalledObject = o;
            Parametros = ReglaParm.ObtenerParametros( ((ICallableInfo)o) );
        }

        /// <summary>
        /// Hace los cambios necesarios en el objeto llamador, una unica vez, antes de empezar a cambiar
        /// cada una de las llamadas
        /// </summary>
        /// <param name="llamador">Objeto llamador a editar</param>
        /// <param name="infoObjeto">Informacion de edicion del objeto llamador</param>
        public void HacerCambiosGlobalesLlamador(KBObject llamador, InfoCambioLlamadasObjeto infoObjeto)
        {
            // No hay que hacer nada
        }

        /// <summary>
        /// Quita el parametro en una llamada
        /// </summary>
        /// <param name="l">Llamada a editar</param>
        /// <returns>Informacion sobre el resultado del cambio en la llamada</returns>
        public InfoCambioLlamada EditarLlamada(Llamada l)
        {
            // Informacion de cambios en la llamada:
            InfoCambioLlamada infoCambioLlamada = new InfoCambioLlamada(l);

            // Ver que la llamada tenga el nº de parametros esperados:
            int nExpectedParameters = Parametros.Count;
            if (l.EsUdp && !(CalledObject is DataSelector) )
                // El valor devuelto no se incluira como parametro
                nExpectedParameters--;
            if (l.IsSubmit)
                // The extra initial parameter
                nExpectedParameters++;

            if (l.NumeroParametros != nExpectedParameters || PosicionParametro >= nExpectedParameters)
            {
                infoCambioLlamada.EstadoCambioLlamada = Estado.ERROR;
                infoCambioLlamada.InformacionCambio += Environment.NewLine +
                    "ERROR: Call does not have the expected number of parameters. Expected: " +
                    nExpectedParameters + ", found: " + l.NumeroParametros;
                return infoCambioLlamada;
            }

            infoCambioLlamada.InformacionCambio += Environment.NewLine +
                "New call:" + Environment.NewLine + l.LlamadaQuitandoParametro(PosicionParametro);

            // Hacer el cambio en el codigo:
            l.QuitarParametro(PosicionParametro);

            return infoCambioLlamada;
        }

    }
}
