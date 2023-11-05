using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Language.Parser.Data;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
    /// <summary>
    /// Buscador de eventos en codigo genexus
    /// </summary>
    public class BuscadorEventos
    {

        /// <summary>
        /// Event name to search. It can be null if we dont search by name
        /// </summary>
        private string NombreEvento;

        /// <summary>
        /// Event key to search. It will be null if we dont search by key
        /// </summary>
        private string EventKey;

        /// <summary>
        /// Los eventos encontrados en la busqueda
        /// </summary>
        private List<CommandEvent> EventosEncontrados;

        /// <summary>
        /// Cierto si hay que buscar solo el primero.
        /// </summary>
        private bool BuscarSoloPrimero;

        /// <summary>
        /// Constructor to search an event by name
        /// </summary>
        /// <param name="nombreEvento">El nombre del evento a buscar</param>
        public BuscadorEventos(string nombreEvento)
        {
            NombreEvento = nombreEvento.ToLower();
        }

        /// <summary>
        /// Constructor to search an event by key
        /// </summary>
        /// <param name="key">Key to search</param>
        public BuscadorEventos(int key)
        {
            EventKey = key.ToString();
        }

        private void CheckEventName(CommandEvent evento, ParsedCodeFinder.SearchState estado) 
        {
            // Verificar un nombre simple: "Event load"
            Word nombreSimpleEvento = evento.EventName as Word;

            if (nombreSimpleEvento == null)
            {
                // TODO: Check this GX_15_OR_GREATER check is really needed
#if GX_15_OR_GREATER
                // Check composite names: "Event grid.load"
                ObjectPEM compositeName = evento.EventName as ObjectPEM;
                if (compositeName != null)
                    nombreSimpleEvento = compositeName.PEMExpression as Word;
#else
                // Ev3
                // Verificar un nombre compuesto: "Event grid.load"
                EventDefinition nombreCompuesto = evento.EventName as EventDefinition;
                if (nombreCompuesto != null)
                    nombreSimpleEvento = nombreCompuesto.PEMExpression as Word;
#endif
            }
            if (nombreSimpleEvento == null)
                return;

            string txtNombreEvento = nombreSimpleEvento.Text.ToLower();
            if (txtNombreEvento == NombreEvento)
            {
                EventosEncontrados.Add(evento);
                if (BuscarSoloPrimero)
                    estado.SearchFinished = true;
            }
        }

        private void CheckEventKey(CommandEvent cmdEvent, ParsedCodeFinder.SearchState state)
        {
            Word eventKey = cmdEvent.Key as Word;
            if (eventKey == null)
                return;

            if (eventKey.Text == EventKey)
            {
                EventosEncontrados.Add(cmdEvent);
                if (BuscarSoloPrimero)
                    state.SearchFinished = true;
            }
        }

        private void AnalizarNodo(ObjectBase nodo, ParsedCodeFinder.SearchState estado)
        {

            // Ver si es un evento:
            CommandEvent evento = nodo as CommandEvent;
            if( evento == null )
                return;

            if (NombreEvento != null)
                CheckEventName(evento, estado);
            else
                CheckEventKey(evento, estado);
        }

        /// <summary>
        /// Busca el primer evento en el codigo
        /// </summary>
        /// <param name="codigo">Codigo en el que buscar</param>
        /// <returns>El evento encontrado. null si no se encontro el evento</returns>
        public CommandEvent Buscar(ParsedCode code)
        {
            EventosEncontrados = new List<CommandEvent>();
            BuscarSoloPrimero = true;
            ParsedCodeFinder buscador = new ParsedCodeFinder(AnalizarNodo);
            buscador.Execute(code);
            if (EventosEncontrados.Count > 0)
                return EventosEncontrados[0];
            else
                return null;
        }

        /// <summary>
        /// Busca todos los eventos indicados en el codigo
        /// </summary>
        /// <param name="codigo">Codigo en el que buscar</param>
        /// <returns>Los eventos encontrados</returns>
        public List<CommandEvent> BuscarTodos(ParsedCode codigo)
        {
            EventosEncontrados = new List<CommandEvent>();
            BuscarSoloPrimero = false;
            ParsedCodeFinder buscador = new ParsedCodeFinder(AnalizarNodo);
            buscador.Execute(codigo);
            return EventosEncontrados;
        }

    }
}
