using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;
using Artech.Architecture.Language.Parser.Data;

namespace LSI.Packages.Extensiones.Utilidades
{

    /// <summary>
    /// Representa un bloque de codigo 'Event xxx' ... 'EndEvent' de Genexus
    /// </summary>
    public class EventoGX
    {

        /// <summary>
        /// Titulo del evento start
        /// </summary>
        public const string START = "Start";

        /// <summary>
        /// Evento de boton de navegacion en transacciones "Siguiente"
        /// </summary>
        public const string NEXT = "Next";

        /// <summary>
        /// Evento load de un work/webpanel
        /// </summary>
        public const string LOAD = "Load";

        /// <summary>
        /// Evento refresh de un work/webpanel
        /// </summary>
        public const string REFRESH = "Refresh";

        /// <summary>
        /// Work/webpanel enter event title
        /// </summary>
        public const string ENTER = "Enter";

        /// <summary>
        /// Workpanel exit event title
        /// </summary>
        public const string EXIT = "Exit";

        /// <summary>
        /// El titulo del evento. Debe incluir las comillas si no es estandar
        /// </summary>
        public string TituloEvento;

        /// <summary>
        /// El codigo contenido en el evento
        /// </summary>
        public string CodigoInterno = string.Empty;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="titulo">Titulo del evento</param>
        public EventoGX(string titulo) 
        {
            TituloEvento = titulo;
        }

        public void AgregarLinea(string lineaCodigo)
        {
            CodigoInterno += "\t" + lineaCodigo + Environment.NewLine;
        }

        /// <summary>
        /// Devuelve el texto del evento
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string txt = "Event " + TituloEvento + Environment.NewLine;
            txt += CodigoInterno;
            txt += "EndEvent" + Environment.NewLine;
            return Entorno.StringFormatoKbase(txt);
        }

        /// <summary>
        /// Devuelve cierto si el codigo interno del Evento esta vacio.
        /// </summary>
        public bool Vacio
        {
            get
            {
                return string.IsNullOrEmpty(CodigoInterno.Trim());
            }
        }

        /// <summary>
        /// Busca la implementacion de un evento en el codigo y lo devuelve
        /// </summary>
        /// <param name="codigo">El codigo en el que buscar el evento</param>
        /// <param name="nombreEvento">Nombre del evento a buscar</param>
        /// <returns>La implementacion del evento. null si no se ha encontrado</returns>
        static public EventoGX BuscarEvento(ParsedCode codigo, string nombreEvento)
        {
            // Buscar el evento en el codigo
            BuscadorEventos buscador = new BuscadorEventos(nombreEvento);
            CommandEvent eventoGx = buscador.Buscar(codigo);
            if (eventoGx == null)
                return null;

            // Devolver informacion del evento:
            EventoGX evento = new EventoGX(nombreEvento);
            if( eventoGx.Body != null )
                evento.CodigoInterno = eventoGx.Body.ToString();
            return evento;
        }

    }
}
