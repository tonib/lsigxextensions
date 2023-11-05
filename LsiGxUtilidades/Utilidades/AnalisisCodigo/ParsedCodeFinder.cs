using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Language.Parser.Data;
using Artech.Architecture.Language.Parser.Objects;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{

    /// <summary>
    /// Utilidad para buscar ciertos elementos en el arbol del parser de Genexus
    /// </summary>
    public class ParsedCodeFinder
    {
        
        /// <summary>
        /// Informacion sobre el estado de la busqueda
        /// </summary>
        public class SearchState
        {

            /// <summary>
            /// Code branch to the current node
            /// </summary>
            public Stack<ObjectBase> SearchPath = new Stack<ObjectBase>(30);

            /// <summary>
            /// Properties branch to the current node. It has only sense combined with SearchPath
            /// </summary>
            public Stack<PropertyInfo> PropertiesPath = new Stack<PropertyInfo>(30);

            /// <summary>
            /// Si se asigna a cierto, la busqueda finalizara inmediatamente
            /// </summary>
            public bool SearchFinished;

            /// <summary>
            /// Si se asigna falso, no se revisaran los descendientes del nodo actual
            /// </summary>
            public bool SearchDescendants;

            /// <summary>
            /// Object owner of the code
            /// </summary>
            public KBObject OwnerObject;

            /// <summary>
            /// The parent node owner of the current node. null if there is no parent node
            /// </summary>
            public ObjectBase Parent
            {
                get { return SearchPath.Count == 0 ? null : SearchPath.Peek(); }
            }

            /// <summary>
            /// The property of the parent node owner of the current node. null if there is no
            /// parent node
            /// </summary>
            public PropertyInfo ParentProperty
            {
                get { return PropertiesPath.Count == 0 ? null : PropertiesPath.Peek(); }
            }
        }

        /// <summary>
        /// Delegado para analizar el codigo de un nodo del arbol de parseado de un objeto genexus
        /// </summary>
        /// <param name="nodeData">Informacion del nodo de codigo a analizar</param>
        /// <param name="state">Informacion sobre el estado de la busqueda</param>
        public delegate void DelegadoAnalisisCodigo(ObjectBase nodeData, SearchState state);

        /// <summary>
        /// Delegado encargado de analizar el codigo
        /// </summary>
        private DelegadoAnalisisCodigo DelegadoAnalisis;

        /// <summary>
        /// Estado actual de la busqueda
        /// </summary>
        SearchState State = new SearchState();

        /// <summary>
        /// Tipo de los objetos analizar por el buscador en el arbol. Si es nulo, se buscan
        /// todos.
        /// </summary>
        /// <remarks>Tambien se analizan las clases heredadas de esta. La clase debe ser
        /// ObjectBase o un descendiente</remarks>
        public Type TipoObjetoBuscar;

        /// <summary>
        /// If its true, the search delegate function will not be called for code comments block.
        /// Default value is true.
        /// </summary>
        public bool IgnoreComments = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="delegado">Delegado responsable de analizar el codigo</param>
        public ParsedCodeFinder(DelegadoAnalisisCodigo delegado)
        {
            DelegadoAnalisis = delegado;
        }

        /// <summary>
        /// Ejecuta la busqueda en la parte del arbol de parseado de genexus indicado
        /// </summary>
        public void Execute(ParsedCode codigo)
        {
            Execute(codigo.Object, codigo.ArbolParseado);
        }

        /// <summary>
        /// Ejecuta la busqueda en la parte del arbol de parseado de genexus indicado
        /// </summary>
        public void Execute(KBObject objeto, ObjectBase subarbolParseado)
        {
            State.SearchFinished = false;
            State.SearchPath.Clear();
            State.OwnerObject = objeto;

            if (subarbolParseado == null)
                return;

            ParsearDatos(subarbolParseado);
        }

        /// <summary>
        /// Ejecuta la busqueda en el arbol de parseado de genexus indicado
        /// </summary>
        public void Execute(KBObject objeto, IParserObjectBase arbolParseado) 
        {
            State.SearchFinished = false;
            State.SearchPath.Clear();
            State.OwnerObject = objeto;

            if (arbolParseado == null)
                return;

            foreach( ObjectBase node in arbolParseado.LsiGetRootNodes() )
            {
                ParsearDatos(node);
                if (State.SearchFinished)
                    return;
            }
        }

        /// <summary>
        /// Analiza un nodo del arbol de parseado
        /// </summary>
        /// <param name="datos">Datos del nodo a analizar</param>
        private void ParsearDatos(ObjectBase datos)
        {
            if (datos == null || (datos is Blank && IgnoreComments) )
                // Ignorar comentarios y lineas en blanco
                return;

            State.SearchDescendants = true;
            AnalizarNodo(datos);
            if (State.SearchFinished || !State.SearchDescendants)
                // Se ha cancelado la busqueda, o se ha cancelado la busqueda en los descendientes
                return;

            // Save the parent node
            State.SearchPath.Push(datos);
            foreach(KeyValuePair<PropertyInfo, ObjectBase> child in datos.LsiGetChildrenByProperty())
            {
                // Save the parent property
                State.PropertiesPath.Push(child.Key);
                // Analize recursivelly the child node
                ParsearDatos(child.Value);
                // Remove the parent property
                State.PropertiesPath.Pop();

                if (State.SearchFinished)
                    return;
            }
            State.SearchPath.Pop();

        }

        private void AnalizarNodo(ObjectBase valor)
        {
            // Filtro por el tipo de los nodos:
            if (TipoObjetoBuscar != null && !TipoObjetoBuscar.IsAssignableFrom(valor.GetType()))
                return;

            DelegadoAnalisis(valor, State);
        }

    }
}
