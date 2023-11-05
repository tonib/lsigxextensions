using Artech.Architecture.Language.Parser.Data;
using Artech.Genexus.Common.CustomTypes;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Llamadas;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using System.Collections.Generic;
using System.Linq;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
    /// <summary>
    /// Utilidad para decidir si una variable / atributo se escribe en un codigo.
    /// Para ver si un token se escribe, se buscan asignaciones y llamadas donde se pase
    /// el token en un parametro que es de salida o entrada/salida.
    /// </summary>
    public class BuscadorLecturasEscrituras
    {

        /// <summary>
        /// Informacion que necesita la clase para buscar lecturas y escrituras
        /// </summary>
        public class InformacionBusqueda
        {

            /// <summary>
            /// Todas las ocurrencias de variables
            /// </summary>
            public List<ObjectBase> TodasOcurrencias;

            /// <summary>
            /// Informacion de las llamadas en el codigo y sus parametros.
            /// La clave es el codigo de la llamada y el valor son la definicion de los parametros
            /// del objeto llamado
            /// </summary>
            public List<GxCallInfo> Llamadas;
           
            /// <summary>
            /// Todas las asignaciones a variables, en asignaciones directas o en sentencias FOR
            /// </summary>
            public List<BuscadorAsignaciones.Asignacion> Asignaciones;

            /// <summary>
            /// Written variables passed as parameter to object calls, with out: or inout: parameter
            /// qualifier.
            /// </summary>
            public HashSet<ObjectBase> VariablesEscritasLlamadas;

            /// <summary>
            /// Written variables passed as parameter to object calls, with out: parameter qualifier.
            /// </summary>
            public HashSet<ObjectBase> WrittenVariablesOut;

            /// <summary>
            /// Todas las referencias a variables donde se escriben, o se leen/escriben:
            /// Son las referencias en asignaciones y parametros de llamadas que se escriben
            /// </summary>
            public HashSet<ObjectBase> OcurrenciasEscritura;

            /// <summary>
            /// Referencias a variables de lectura/escritura: Instancias en parametros inout:,  
            /// sentencias FOR &amp;i = 1 TO 10 y indirecciones con llamadas (&amp;ExcelDocument.Open('xxx'))
            /// </summary>
            public HashSet<ObjectBase> OcurrenciasLecturaEscritura;

            /// <summary>
            /// Tokens instanced on external code. They are read and written.
            /// </summary>
            public HashSet<string> ExternalCodeInstances;
        }

        /// <summary>
        /// Tipo de token del que buscamos lecturas / escrituras
        /// </summary>
        private TokenGx TokenBusqueda;

        /// <summary>
        /// Buscador de asignaciones al token.
        /// </summary>
        private BuscadorAsignaciones BusAsignaciones;

        /// <summary>
        /// Buscador de llamadas en el codigo.
        /// </summary>
        private CallsFinder BusLlamadas;

        /// <summary>
        /// Buscador de variables (cualquier variable, indirecciones a miembros o funciones incluidas)
        /// </summary>
        private TokensFinder BusVariables;

        private void Inicializar(TokenGx token)
        {
            TokenBusqueda = token;

            // Create the token finder. Ignore nullvalue function calls, they dont really read the variable
            BusVariables = new TokensFinder(token);
            BusVariables.IgnoreNullValueParms = true;

            // Crear el buscador de asignaciones al token (buscar tambien en codigo externo)
            BusAsignaciones = new BuscadorAsignaciones(token, true, true);

            // Crear el buscador de llamadas a cualquier objeto:
            BusLlamadas = new CallsFinder();
        }

        /// <summary>
        /// Crea una busqueda de lecturas/escrituras de variables
        /// </summary>
        public BuscadorLecturasEscrituras()
        {
            TokenGx token = new TokenGx(TokenType.VARIABLE, null, true);
            token.AcceptIndirections = true;
            Inicializar(token);
        }

        /// <summary>
        /// Crea una busqueda de lecturas/escrituras al token indicado
        /// <param name="token">Definicion del token del que buscar lecturas/escrituras</param>
        /// </summary>
        public BuscadorLecturasEscrituras(TokenGx token)
        {
            Inicializar(token);
        }

        /// <summary>
        /// Devuelve la informacion que necesita la clase para hacer busquedas de escrituras/lecturas
        /// en el codigo
        /// </summary>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public InformacionBusqueda ObtenerInformacionBusqueda(ParsedCode codigo)
        {
            InformacionBusqueda i = new InformacionBusqueda();

            i.OcurrenciasEscritura = new HashSet<ObjectBase>();
            i.VariablesEscritasLlamadas = new HashSet<ObjectBase>();
            i.WrittenVariablesOut = new HashSet<ObjectBase>();
            i.OcurrenciasLecturaEscritura = new HashSet<ObjectBase>();
            i.ExternalCodeInstances = new HashSet<string>();

            // Buscar asignaciones
            i.Asignaciones = BusAsignaciones.BuscarTodas(codigo);

            // Buscar todas las ocurrencias de variables:
            i.TodasOcurrencias = BusVariables.BuscarTodas(codigo);

            // Put all external code references from the search to other list:
            List<ObjectBase> externalCodeRefs = 
                i.TodasOcurrencias.Where(x => x.LsiIsExternalCode()).ToList();
            externalCodeRefs.ForEach(x => i.TodasOcurrencias.Remove(x));

            // All external code references are read and write (code cannot be analyzed)
            foreach (ObjectBase externalCode in externalCodeRefs)
            {
                foreach (string varName in BusVariables.Token.GetExternalCodeTokens(externalCode, false))
                    i.ExternalCodeInstances.Add(varName);
            }

            // Buscar informacion de llamadas:
            i.Llamadas = BusLlamadas.BuscarLlamadasConParametros(codigo);

            // Buscar tokens escritos en asignaciones
            foreach (BuscadorAsignaciones.Asignacion asignacion in i.Asignaciones)
            {
                i.OcurrenciasEscritura.Add(asignacion.AssignedToken);
                if( asignacion.AssignmentStatement is ObjectPEM )
                    i.OcurrenciasEscritura.Add(asignacion.AssignmentStatement);

                if (asignacion.AlsoReads)
                {
                    // Las sentencias FOR &i = 1 TO 10 son de lectura/escritura
                    i.OcurrenciasLecturaEscritura.Add(asignacion.AssignedToken);
                }
            }

            // Buscar tokens escritos en llamadas:
            foreach (GxCallInfo llamada in i.Llamadas)
            {
                // Referencias de solo escritura
                foreach (ObjectBase refEscritura in llamada.GetParametersByAccessType(codigo.Object,
                    TokenBusqueda, RuleDefinition.ParameterAccess.PARM_OUT))
                {
                    i.OcurrenciasEscritura.Add(refEscritura);
                    i.VariablesEscritasLlamadas.Add(refEscritura);
                    i.WrittenVariablesOut.Add(refEscritura);
                }

                // Referencias de lectura/escritura
                foreach (ObjectBase refEscritura in llamada.GetParametersByAccessType(codigo.Object,
                    TokenBusqueda, RuleDefinition.ParameterAccess.PARM_INOUT))
                {
                    i.OcurrenciasEscritura.Add(refEscritura);
                    i.VariablesEscritasLlamadas.Add(refEscritura);
                    i.OcurrenciasLecturaEscritura.Add(refEscritura);
                }
            }

            return i;
        }

        /// <summary>
        /// Devuelve la lista de variables escritas (o leidas/escritas) en un cierto codigo.
        /// </summary>
        /// <param name="codigo">Codigo a analizar</param>
        /// <returns>La lista de nombres de variables, sin el ampersand inicial, y en minusculas,
        /// de las variables escritas en el codigo</returns>
        public HashSet<string> VariablesEscritas(ParsedCode codigo)
        {
            InformacionBusqueda i = ObtenerInformacionBusqueda(codigo);
            return VariablesEscritas(i);
        }

        static private HashSet<string> GetReferencesVariableNames(HashSet<ObjectBase> referencesSet)
        {
            HashSet<string> variableNames = new HashSet<string>();
            foreach (ObjectBase variable in referencesSet)
                variableNames.Add(TokenGx.ObtenerNombre(variable, false));

            return variableNames;
        }

        /// <summary>
        /// It returns the names of written variables in object calls, where the parameter qualifier 
        /// is inout: or out:
        /// </summary>
        /// <param name="info">Read / write code information</param>
        /// <returns>Normalized name of written variables</returns>
        public HashSet<string> NombresVariablesEscritasLlamadas(InformacionBusqueda info)
        {
            return GetReferencesVariableNames(info.VariablesEscritasLlamadas);
        }

        /// <summary>
        /// It returns the names of written variables in object calls, where the parameter qualifier 
        /// is out:
        /// </summary>
        /// <param name="info">Read / write code information</param>
        /// <returns>Normalized name of written variables</returns>
        public HashSet<string> WrittenVariableNamesOut(InformacionBusqueda info)
        {
            return GetReferencesVariableNames(info.WrittenVariablesOut);
        }

        /// <summary>
        /// It returns the names of variables written in assigments / for sentences
        /// </summary>
        /// <param name="info">Read / write code information</param>
        /// <param name="ignorePEMAssigments">True if assigments to indirections (ex. &amp;x.Visible = 0) 
        /// should not be returned</param>
        /// <returns>Normalized name of written variables in assignments / for</returns>
        public HashSet<string> WrittenVariableNamesAssigments(InformacionBusqueda info, 
            bool ignorePEMAssigments)
        {
            IEnumerable<BuscadorAsignaciones.Asignacion> assignedTokens = info.Asignaciones;
            if (ignorePEMAssigments)
                assignedTokens = assignedTokens.Where(x => 
                    !(x.AssignedToken is ObjectPEM && x.AssignmentStatement is Assignment)
                );

            return TokenGx.ObtenerNombres(assignedTokens.Select(x => x.AssignedToken).ToList(), false);
        }

        /// <summary>
        /// Devuelve la lista de variables escritas (o leidas/escritas) en un cierto codigo.
        /// </summary>
        /// <param name="informacion">Informacion de busqueda</param>
        /// <returns>La lista de nombres de variables, sin el ampersand inicial, y en minusculas,
        /// de las variables escritas en el codigo</returns>
        public HashSet<string> VariablesEscritas(InformacionBusqueda informacion)
        {
            HashSet<string> variablesEscritas = new HashSet<string>();

            foreach (ObjectBase referencia in informacion.OcurrenciasEscritura)
                variablesEscritas.Add(TokenGx.ObtenerNombre(referencia, false));
            foreach (string externalRef in informacion.ExternalCodeInstances)
                variablesEscritas.Add(externalRef);

            return variablesEscritas;
        }

        /// <summary>
        /// Devuelve la lista de variables leidas (o leidas/escritas) en un cierto codigo.
        /// Para buscar variables escritas o leidas, usar la clase BuscadorTokens.
        /// </summary>
        /// <param name="codigo">Codigo a analizar</param>
        /// <returns>La lista de nombres de variables, sin el ampersand inicial, y en minusculas,
        /// de las variables solo leidas en el codigo</returns>
        public HashSet<string> VariablesLeidas(ParsedCode codigo)
        {
            InformacionBusqueda i = ObtenerInformacionBusqueda(codigo);
            return VariablesLeidas(i);
        }

        /// <summary>
        /// Devuelve la lista de variables leidas (o leidas/escritas) en un cierto codigo.
        /// Para buscar variables escritas o leidas, usar la clase BuscadorTokens.
        /// </summary>
        /// <param name="informacion">Informacion de busqueda del objeto</param>
        /// <returns>La lista de nombres de variables, sin el ampersand inicial, y en minusculas,
        /// de las variables solo leidas en el codigo</returns>
        public HashSet<string> VariablesLeidas(InformacionBusqueda informacion)
        {

            // Buscar revisar todas las referencias a variables y ver cuales no son de escritura
            HashSet<string> variablesLeidas = new HashSet<string>();
            foreach (ObjectBase token in informacion.TodasOcurrencias)
            {
                if( informacion.OcurrenciasLecturaEscritura.Contains(token) )
                    variablesLeidas.Add(TokenGx.ObtenerNombre(token, false));
                else if (!informacion.OcurrenciasEscritura.Contains(token) )
                    variablesLeidas.Add(TokenGx.ObtenerNombre(token, false));
            }

            foreach (string externalRef in informacion.ExternalCodeInstances)
                variablesLeidas.Add(externalRef);

            return variablesLeidas;
        }

        /// <summary>
        /// Devuelve una lista con los mensajes de error de llamadas a objetos con un numero
        /// de parametros incorrecto
        /// </summary>
        /// <param name="informacion">Informacion de busqueda del objeto</param>
        /// <returns>Lista de mensajes de error con informacion de las llamadas incorrectas</returns>
        public List<string> LlamadasNParametrosIncorrecto(InformacionBusqueda informacion)
        {
            List<string> errores = new List<string>();
            foreach (GxCallInfo llamada in informacion.Llamadas)
            {
                if (llamada.WrongNumberOfParameters)
                {
                    // Wrong number of parameters in call
                    string msg = string.Format(
                        "{0}: Wrong number of parameters in call to {1}. Expected {2}, found {3}",
                        llamada.Call.TextoFilaColumna, llamada.Call.NombreObjeto.ToString(),
                        llamada.Parameters.Count,
                        llamada.NRealPassedParameters);
                    errores.Add(msg);
                }
            }
            return errores;
        }

    }
}
