using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Language.Parser.Data;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Llamadas
{

    /// <summary>
    /// Information about a function / object call and the expected parameters of the called object / function.
    /// </summary>
    public class GxCallInfo
    {
        /// <summary>
        /// Kind of call
        /// </summary>
        public enum CallTypeEnum 
        {
            /// <summary>
            /// Call to genexus function ("str(1)")
            /// </summary>
            STANDARDFUNCTION,

            /// <summary>
            /// Call to kbase object ("PUEmpNom(1)")
            /// </summary>
            OBJECT, 

            /// <summary>
            /// Dynamic call("call( &amp;x )")
            /// </summary>
            DYNAMIC,

            /// <summary>
            /// Call to external function ("call( 'x' , &amp;y )")
            /// Work with devices are unsupported right now and are classified as EXTERNAL too.
            /// </summary>
            EXTERNAL

        }

        /// <summary>
        /// Cache con los nombre de funciones estandar de genexus: str, len, trim, etc.
        /// Se va llenando a medida que se buscan llamadas.
        /// </summary>
        static private HashSet<string> FuncionesEstandar = new HashSet<string>();

        /// <summary>
        /// Called object parameters declaration. It will be null if the called object is not an object.
        /// </summary>
        /// <remarks>
        /// As example, a call "call( &amp;variable )" or "call( "externalobject" )" will have this member null.
        /// </remarks>
        public List<ParameterElement> Parameters;

        /// <summary>
        /// The call found on code.
        /// </summary>
        public Llamada Call;

        /// <summary>
        /// Kind of call
        /// </summary>
        public CallTypeEnum CallType;

        /// <summary>
        /// The called object. If will be null if this.CallType != CallTypeEnum.OBJECT
        /// </summary>
        public KBObject CalledObject;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="caller">Caller object</param>
        /// <param name="call">Call to parse</param>
        public GxCallInfo(KBObject caller, Llamada call)
        {
            this.Call = call;

            string calledName = call.NombreObjeto.ToString().ToLower();
            if( call.IsExternalObjectVariableCall)
                CallType = CallTypeEnum.EXTERNAL;
            else if (calledName.StartsWith("&"))
                CallType = CallTypeEnum.DYNAMIC;
            else if (calledName.StartsWith("'") || calledName.StartsWith("\""))
                CallType = CallTypeEnum.EXTERNAL;
            else
            {
                KBObject o = GetCalledObject(caller, calledName);
                if (o != null && o.Type == ObjClass.WorkWithDevices)
                    // Unsupported
                    CallType = CallTypeEnum.EXTERNAL;
                else
                {
                    ICallableInfo callable = o as ICallableInfo;
                    if (callable != null)
                    {
                        Parameters = ReglaParm.ObtenerParametros(callable);
                        CallType = CallTypeEnum.OBJECT;
                        CalledObject = o;
                    }
                    else
                        CallType = CallTypeEnum.STANDARDFUNCTION;
                }
            }
        }

        /// <summary>
        /// Add all parameters in call to a tokens list
        /// </summary>
        /// <param name="finder">Tokens to add to the list</param>
        /// <param name="tokens">Tokens list to fill</param>
        private void AddAllParameters(KBObject objeto, TokensFinder finder, List<ObjectBase> tokens)
        {
            int nParameters = NRealPassedParameters;
            for (int i = 0; i < nParameters; i++)
            {
                ObjectBase valorParametro = Call.ObtenerParametro(i);
                if (valorParametro != null)
                    tokens.AddRange(finder.BuscarTodas(objeto, valorParametro));
            }
        }

        /// <summary>
        /// Devuelve la lista de tokens de un cierto tipo leidos y/o escritos en esta llamada
        /// </summary>
        /// <param name="objeto">The caller object</param>
        /// <param name="tokenEscritura">Tipo de token del que buscar lecturas/escrituras</param>
        /// <param name="tipoAcceso">Tipo de accesdo de los parametros a revisar</param>
        /// <returns>La lista de tokens que se leen/escriben</returns>
        public List<ObjectBase> GetParametersByAccessType(KBObject objeto,
            TokenGx tokenEscritura,
            RuleDefinition.ParameterAccess tipoAcceso)
        {
            TokensFinder buscador = new TokensFinder(tokenEscritura);
            List<ObjectBase> tokensLeidosEscritos = new List<ObjectBase>();

            if (Parameters != null)
            {
                // Call to kbase object
                for (int i = 0; i < Parameters.Count; i++)
                {
                    ParameterElement definicionParametro = Parameters[i];
                    if (definicionParametro.Accessor == tipoAcceso)
                    {
                        ObjectBase valorParametro = Call.ObtenerParametro(i);
                        if (valorParametro != null)
                            tokensLeidosEscritos.AddRange(buscador.BuscarTodas(objeto, valorParametro));
                    }
                }
            }
            else if( CallType == CallTypeEnum.DYNAMIC || CallType == CallTypeEnum.STANDARDFUNCTION )
            {
                // All parameters are read.
                if (tipoAcceso == RuleDefinition.ParameterAccess.PARM_IN)
                    AddAllParameters(objeto, buscador, tokensLeidosEscritos);
            }
            else if( CallType == CallTypeEnum.EXTERNAL ) 
            {
                // All parameteres are written / read.
                if (tipoAcceso == RuleDefinition.ParameterAccess.PARM_INOUT)
                    AddAllParameters(objeto, buscador, tokensLeidosEscritos);
            }

            return tokensLeidosEscritos;
        }

        /// <summary>
        /// The expected number of parameters in call, or -1 if there is no info about the 
        /// called object parameters.
        /// </summary>
        public int ExpectedNParameters
        {
            get { return Parameters != null ? Parameters.Count : -1; }
        }

        /// <summary>
        /// True if the number of parameters on the call does not match the expected.
        /// </summary>
        public bool WrongNumberOfParameters
        {
            get
            {
                if (Parameters == null)
                    return false;
                return NRealPassedParameters != Parameters.Count;
            }
        }

        /// <summary>
        /// Devuelve el objeto llamable con el nombre dado
        /// </summary>
        /// <param name="nombre">El nombre del objeto a obtener, en minusculas.</param>
        /// <returns>null si no existe un objeto en la kbase con dicho nombre</returns>
        private KBObject GetCalledObject(KBObject caller, string nombre)
        {
            // Ver si el nombre es el de una funcion estandar registrada:
            if (FuncionesEstandar.Contains(nombre))
                return null;

            // Ver si es el nombre de un objeto llamable
            KBObject objeto = Call.GetCalledObject(caller);
            if (objeto == null && !Call.ObjectNameIsQualified)
                // El nombre se corresponde con el de una funcion estandar:
                FuncionesEstandar.Add(nombre);

            return objeto;
        }

        /// <summary>
        /// The number of real passed parameters, having in mind it can be a DataSelector (dear lord...)
        /// </summary>
        public int NRealPassedParameters
        {
            get
            {
                int n = Call.NRealPassedParameters;
                if (CalledObject != null && CalledObject is DataSelector)
                    // Data selectors are UDP calls with no return value, so all parameters should be passed
                    n--;
                return n;
            }
        }

        /// <summary>
        /// Get a list of calls on the code, with the called objects parameters declaration.
        /// </summary>
        /// <param name="finder">Definition of calls to search</param>
        /// <param name="analizador">Code where to search calls</param>
        /// <returns>Calls found, with their parameter declarations</returns>
        static public List<GxCallInfo> SearchCalls(CallsFinder finder, ParsedCode analizador)
        {
            // Las llamadas, con informacion de parametros
            List<GxCallInfo> llamadas = new List<GxCallInfo>();

            // TODO: Store cache with called objects, to avoid fetch kbobject parameters twice.
            foreach (Llamada llamada in finder.BuscarTodas(analizador))
                llamadas.Add(new GxCallInfo(analizador.Object, llamada));

            return llamadas;
        }
    }
}
