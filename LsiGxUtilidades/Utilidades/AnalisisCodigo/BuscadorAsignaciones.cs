using Artech.Architecture.Language.Parser.Data;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using LSI.Packages.Extensiones.Utilidades.Variables;
using System.Collections.Generic;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{

    /// <summary>
    /// Utilidad para buscar asignaciones en codigo de genexus
    /// </summary>
    public class BuscadorAsignaciones
    {

        /// <summary>
        /// Informacion de la asignacion
        /// </summary>
        public class Asignacion
        {
            /// <summary>
            /// La sentencia de asignacion
            /// </summary>
            public ObjectBase AssignmentStatement;

            /// <summary>
            /// El token asignado
            /// </summary>
            public ObjectBase AssignedToken;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="asignacion">La sentencia de asignacion</param>
            /// <param name="tokenAsignado">El token asignado</param>
            public Asignacion(ObjectBase asignacion, ObjectBase tokenAsignado)
            {
                AssignmentStatement = asignacion;
                AssignedToken = tokenAsignado;
            }


            /// <summary>
            /// True if the assignment also reads the the token ("FOR &amp;i = 1 TO 10", "&amp;i += 1", etc)
            /// </summary>
            public bool AlsoReads
            {
                get { return IsForTo || IsIncrement; }
            }

            public bool IsIncrement
            {
                get
                {
                    // Is &amp;i += 1?
                    Assignment a = AssignmentStatement as Assignment;
                    if (a == null)
                        return false;
                    Word op = a.OperatorSymbol as Word;
                    if (op == null)
                        return false;
                    return op.Text == "+=" || op.Text == "-=";
                }
            }

            public bool IsForTo
            {
                get { return (AssignmentStatement is CommandFor && ((CommandFor)AssignmentStatement).Parameters[0] is Assignment); }
            }

            public override string ToString()
            {
                return AssignmentStatement.ToString();
            }
        }

        /// <summary>
        /// Informacion de trabajo mientras se esta haciendo una busqueda.
        /// </summary>
        private class BusquedaActualInfo
        {

            /// <summary>
            /// Codigo en el que estamos buscando actualmente
            /// </summary>
            public ParsedCode CodigoBusqueda;

            /// <summary>
            /// Las asignaciones encontradas en la busqueda actual
            /// </summary>
            public List<Asignacion> ResultadosBusqueda = new List<Asignacion>();

            /// <summary>
            /// Indica si buscamos solo la primera asignacion
            /// </summary>
            public bool BuscarSoloPrimera;

            /// <summary>
            /// Cache con la lista de miembros modificadores de un atributo
            /// </summary>
            private Dictionary<string, List<string>> MiembrosModificadores = new Dictionary<string, List<string>>();

            /// <summary>
            /// Devuelve la lista de miembros modificadores de un atributo/variable
            /// </summary>
            /// <param name="elemento">Un AttributeName/VariableName del que devolver la lista
            /// de miembros que lo modifican.</param>
            /// <returns>La lista de miembros del atributo que lo modifican. Por ejemplo,
            /// "Atributo.SetEmpty()" modifica a la variable lista. "setempty" estaria incluido la lista
            /// devuelta</returns>
            public List<string> ObtenerMiembrosModificadores(Word elemento)
            {
                // Obtener el nombre normalizado:
                string nombre = elemento.Text.ToLower();
                List<string> miembros;
                if (!MiembrosModificadores.TryGetValue(nombre, out miembros))
                {
                    // El elemento no se encontro en la cache. Buscar los miembros ahora
                    miembros = new List<string>();
                    if (elemento is AttributeName)
                    {
                        Artech.Genexus.Common.Objects.Attribute atributo =
                            Artech.Genexus.Common.Objects.Attribute.Get(CodigoBusqueda.Object.Model, nombre);
                        if (atributo != null)
                            miembros = AtributoGx.ModifierMembersNames(atributo);
                    }
                    /*if (elemento is VariableName)
                    {
                        // Remove "&"
                        string vName = nombre.Substring(1);
                        // Get variable info
                        Variable variable = CodigoBusqueda.Objeto.Parts.Get<VariablesPart>().Variables
                            .FirstOrDefault(v => v.Name.ToLower() == vName);
                        if (variable != null)
                            miembros = VariableGX.ModifierMemberNames(variable);
                    }*/

                    MiembrosModificadores.Add(nombre, miembros);
                }
                return miembros;
            }

            public void NuevoResultado(ObjectBase asignacion, ObjectBase token, ParsedCodeFinder.SearchState estado)
            {
                ResultadosBusqueda.Add(new Asignacion(asignacion, token));
                if (BuscarSoloPrimera)
                    estado.SearchFinished = true;
            }
        }

        /// <summary>
        /// Informacion sobre la busqueda en ejecucion actualmente
        /// </summary>
        private BusquedaActualInfo BusquedaActual;

        /// <summary>
        /// Indica si ademas de las asignaciones, buscamos sentencias for
        /// </summary>
        private bool BuscarSentenciasFor;

        /// <summary>
        /// Cierto si buscamos llamadas a miembros del token que modifican al propio token. Por ejemplo,
        /// si hay tokens del tipo Atributo.SetEmpty().
        /// SOLO SE APLICA A LOS ATRIBUTOS
        /// </summary>
        private bool BuscarMiembrosModificadores;

        /// <summary>
        /// Token del que se buscan asignaciones
        /// </summary>
        private TokenGx Token;

        /// <summary>
        /// Constructor.
        /// Crea un buscador de asignaciones a un cierto objeto
        /// </summary>
        /// <param name="token">Elemento al que buscar asignaciones.
        /// Ejemplo: "CliCod" busca asignaciones del tipo "CliCod = ..." o "FOR &amp;i ...
        /// </param>
        /// <param name="buscarSentenciasFor">Cierto si hay que buscar asignaciones en sentencias
        /// FOR xxx =/IN. Falso si no. Tener en cuenta que solo se pueden asignar variables
        /// en estas sentencias.</param>
        /// <param name="buscarMiembrosModificadores">
        /// Cierto si buscamos llamadas a miembros del token que modifican al propio token. Por ejemplo,
        /// si hay tokens del tipo Atributo.SetEmpty(). SOLO SE APLICA A LOS ATRIBUTOS, NO A LAS VARIABLES
        /// </param>
        public BuscadorAsignaciones(TokenGx token, bool buscarSentenciasFor, bool buscarMiembrosModificadores)
        {
            Token = token;
            BuscarSentenciasFor = buscarSentenciasFor;
            BuscarMiembrosModificadores = buscarMiembrosModificadores;
        }

        /// <summary>
        /// Ejecuta la busqueda de asignaciones
        /// </summary>
        /// <param name="analizador">Codigo a analizar</param>
        /// <param name="buscarSoloPrimera">Cierto si solo hay que buscar la primera ocurrencia</param>
        private void EjecutarBusqueda(ParsedCode analizador, bool buscarSoloPrimera)
        {
            BusquedaActual = new BusquedaActualInfo()
            {
                CodigoBusqueda = analizador,
                BuscarSoloPrimera = buscarSoloPrimera
            };
            ParsedCodeFinder buscador = new ParsedCodeFinder(AnalizarNodo);
            buscador.Execute(analizador);
        }

        /// <summary>
        /// Devuelve cierto si el codigo pasado como parametro contiene alguna asignacion a elemento
        /// </summary>
        /// <param name="analizador">Codigo en el que buscar asignaciones</param>
        /// <returns>Cierto si contiene alguna asignacion</returns>
        public bool ContieneAsignacion(ParsedCode analizador)
        {
            EjecutarBusqueda(analizador, true);
            return BusquedaActual.ResultadosBusqueda.Count > 0;
        }

        /// <summary>
        /// Devuelve una lista de las ocurrencias del token en las que este es asignado
        /// </summary>
        /// <param name="analizador">Codigo a analizar</param>
        /// <returns>La lista de ocurrencias del token</returns>
        public List<Asignacion> BuscarTodas(ParsedCode analizador)
        {
            EjecutarBusqueda(analizador, false);
            return BusquedaActual.ResultadosBusqueda;
        }

        /// <summary>
        /// Revisa si el nodo es una sentencia FOR
        /// </summary>
        /// <param name="nodo">Nodo a revisar</param>
        /// <param name="estado">Estado de la busqueda</param>
        /// <returns>Cierto si el nodo era una sentencia for</returns>
        private bool BuscarFor(ObjectBase nodo, ParsedCodeFinder.SearchState estado)
        {
            if (nodo is CommandFor)
            {
                // Es una sentencia FOR &i = 1 TO 10, y las estamos buscando
                CommandFor cmdFor = (CommandFor)nodo;
                ObjectBase parametroVariable = cmdFor.Parameters[0];

                ObjectBase variableFor;
                if (parametroVariable is Assignment)
                {
                    // En las sentencias FOR &i = 1 TO... el primer parametro es la asignacion &i = 1
                    variableFor = ((Assignment)parametroVariable).Target;

                    if (Token.EsToken(estado.OwnerObject, variableFor, cmdFor))
                    {
                        BusquedaActual.NuevoResultado(cmdFor, variableFor, estado);
                        return true;
                    }
                }
            }
            else if (nodo is CommandBlock)
            {
                // Ver si es una sentencia FOR &i IN &lista
                CommandBlock cmd = (CommandBlock)nodo;
                if (((Word)cmd.Name).Text.ToLower() != KeywordGx.FOR)
                    return false;
                if (cmd.Parameters.Count != 2)
                    return false;
                if (!(cmd.Parameters[1] is CommandLine))
                    return false;
                CommandLine clausulaIn = (CommandLine)cmd.Parameters[1];
                if (((Word)clausulaIn.Name).Text.ToLower() != KeywordGx.IN)
                    return false;

                ObjectBase variableFor = cmd.Parameters[0];
                if (Token.EsToken(estado.OwnerObject, variableFor, cmd))
                {
                    BusquedaActual.NuevoResultado(cmd, variableFor, estado);
                    return true;
                }
            }
            return false;
        }

        private void BuscarMiembroModificador(ObjectBase node, ParsedCodeFinder.SearchState state)
        {
            // Buscamos una instancia del token, que sea un ObjectPem, cuyo primer miembro sea una 
            // funcion modificadora (p.ej. &Lista.Clear())
            if (!(node is ObjectPEM && Token.EsToken(state.OwnerObject, node, state.Parent)))
                return;


            // Obtener la ultima indireccion. Si no es una funcion, no nos vale
            ObjectPEM expression = (ObjectPEM)node;
            List<ObjectBase> indirections = ObjectPEMGx.ConvertirALista(expression);
            if (indirections.Count < 2)
                return;
            Function function = indirections[indirections.Count - 1] as Function;
            if (function == null)
                return;

            Word indirectionBase = indirections[0] as Word;
            if (indirectionBase == null)
                return;

            if (indirectionBase is AttributeName)
            {
                // Attribute members functions store if they update the attribute: Search the function
                List<string> miembrosModificadores = BusquedaActual.ObtenerMiembrosModificadores(indirectionBase);
                if (miembrosModificadores != null &&
                    miembrosModificadores.Contains(((Word)function.Name).Text.ToLower()))
                    BusquedaActual.NuevoResultado(node, indirectionBase, state);
                return;
            }
            else if (indirectionBase is VariableName)
            {
                // Variable members does not store if they update  the attribute: Use an heuristic
                // by name to check if the function will write the variable
                if (VariableGX.IsModifierMember(function.Name.ToString()))
                    BusquedaActual.NuevoResultado(node, indirectionBase, state);
            }
        }

        /// <summary>
        /// Gx16: There are new standard members that have out: parameters (ex. FromJson). There is no way (or I did not ound it) to detected them. They are detected here
        /// </summary>
        private void FindPemOutParameters(ObjectBase node, ParsedCodeFinder.SearchState state)
        {
            // We search ObjectPem with single call: &var.[members].FromJson('', &Messages. Here &Messages is out:
            ObjectPEM expression = node as ObjectPEM;
            if (expression == null)
                return;

            // Get last called member
            List<ObjectBase> indirections = ObjectPEMGx.ConvertirALista(expression);
            if (indirections.Count < 2)
                return;
            Function function = indirections[indirections.Count - 1] as Function;
            if (function == null)
                return;

            // Check if called member has out parameters ("fromjson", "fromjsonfile", "fromxml", etc). Out parameter is the second
            string funcName = ((Word)function.Name).Text.ToLower().Trim();
            if (!funcName.StartsWith("from") || function.Parameters.Count != 2)
                return;

            ListItem outParm = function.Parameters[1] as ListItem;
            if (outParm == null)
                return;
            if (Token.EsToken(state.OwnerObject, outParm.Content, function))
                BusquedaActual.NuevoResultado(expression, outParm.Content, state);
        }

		/// <summary>
		/// There are functions with out: parameters, they are checked here
		/// </summary>
		private void FindFunctionsWithOutParameters(ObjectBase node, ParsedCodeFinder.SearchState state)
		{
			Function function = node as Function;
			if (function == null)
				return;

			string fName = function.Name?.ToString();
			if (string.IsNullOrEmpty(fName))
				return;

			fName = fName.ToLower();
			if(fName == "dfrgtxt")
			{
				// First parameter is written: https://wiki.genexus.com/commwiki/servlet/wiki?8367,DFRGTxt%20Function
				// See https://sourceforge.net/p/lsigxextensions/tickets/5/
				if (function.Parameters.Count == 0)
					return;
				ObjectBase parm = function.Parameters[0];
				if (Token.EsToken(state.OwnerObject, function.Parameters[0], function))
					BusquedaActual.NuevoResultado(function, parm, state);
			}
		}

		private void AnalizarNodo(ObjectBase node, ParsedCodeFinder.SearchState state)
        {
            if (node is Assignment)
            {
                Assignment asignacion = (Assignment)node;
                if (Token.EsToken(state.OwnerObject, asignacion.Target, asignacion))
                {
                    BusquedaActual.NuevoResultado(asignacion, asignacion.Target, state);
                    return;
                }
            }
            if (BuscarSentenciasFor)
            {
                // Ver si es una sentencia for
                if (BuscarFor(node, state))
                    return;
            }

            if (BuscarMiembrosModificadores)
                // Ver si es una expresion con un miembro modificador
                BuscarMiembroModificador(node, state);

			// Check "object" methods with out: pararmeters (version >= Gx16):
            FindPemOutParameters(node, state);

			// Check standard functions with out parameters
			FindFunctionsWithOutParameters(node, state);

		}

    }
}
