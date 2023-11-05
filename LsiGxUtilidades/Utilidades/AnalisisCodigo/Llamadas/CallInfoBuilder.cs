using System;
using System.Collections.Generic;
using System.Linq;
using Artech.Architecture.Language.Parser.Data;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Llamadas
{
    /// <summary>
    /// Internal functions to get a call detalls
    /// </summary>
    public class CallInfoBuilder
    {

        /// <summary>
        /// Qualified, lowercase names of modules that can be referenced by the calls
        /// </summary>
        private HashSet<string> ModulesNames = new HashSet<string>();

        /// <summary>
        /// Devuelve cierto si un string es una palabra clave de llamada en genexus 
        /// (upd, call, submit, etc)
        /// </summary>
        internal bool EsPalabraClaveLlamada(string palabra)
        {
            return KeywordGx.CALL_KEYWORDS.Contains(palabra) || KeywordGx.UPD_KEYWORDS.Contains(palabra);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CallInfoBuilder()
        {
            // Get all kb modules:
            foreach (Module m in Module.GetAll(UIServices.KB.CurrentModel))
                ModulesNames.Add(m.QualifiedName.ToString().ToLower());
        }


        /// <summary>
        /// Check if a composite expression is an object call
        /// </summary>
        /// <param name="expression">Expression to check</param>
        /// <param name="searchStatus">Search on tree code status</param>
        /// <returns>True if the expression is an object call</returns>
        private Llamada CheckObjectPEMCall(ObjectPEM expression,
            ParsedCodeFinder.SearchState searchStatus)
        {
            // Ignore PEM subexpressions. We only analyze the entire expression
            if (searchStatus.Parent is ObjectPEM)
                return null;

            // Parse the expression:
            CallPemExpression expressionComponents = new CallPemExpression(expression, ModulesNames);
            // Ignore non calling function expressions:
            if (!expressionComponents.IsCallExpression)
                return null;

            // Ignore expressions starting with variable ("&achilipu.call()"), EXCEPT external objects variables
            VariableName varNameNode = expressionComponents.ObjectName as VariableName;
            bool isExternalObjectVariableCall = false;
            if (varNameNode != null)
            {
                string varName = TokenGx.NormalizeName(varNameNode, false);
                VariablesPart variables = searchStatus.OwnerObject.Parts.LsiGet<VariablesPart>();
                if (variables == null)
                    return null;
                Variable v = variables.GetVariable(varName);
                if (v == null)
                    return null;
                if( v.Type != eDBType.GX_EXTERNAL_OBJECT )
                    // Not a call to analyze
                    return null;
                // It's a call to an external object, we will track it
                isExternalObjectVariableCall = true;
            }

            // Get the function called name. It can be (call|upd|udf|...) or the object/ww section name
            string functionName = expressionComponents.CallFunctionName.Text.Trim().ToLower();

            // Check if the function called is (call|upd|udf|...) or the object name/ww section name
            if (EsPalabraClaveLlamada(functionName))
            {
                // Call type: [MODULEQUALIFIEDNAME .] OBJECTNAME [ . WWSECTIONNAME ] . (CALL|UPD|UDF|..)
                return new Llamada()
                {
                    NodoLlamada = expression,
                    EsUdp = KeywordGx.UPD_KEYWORDS.Contains(functionName),
                    IsSubmit = (functionName == KeywordGx.SUBMIT),
                    FormatoLlamadaAntiguo = false,
                    NombreObjeto = expressionComponents.ObjectName,
                    Parametros = expressionComponents.CallFunction.Parameters,
                    QualifiedNameCall = expressionComponents.QualifiedObjectName,
                    IsExternalObjectVariableCall = isExternalObjectVariableCall
                };
            }
            else
            {
                // Call type: [MODULEQUALIFIEDNAME .] OBJECTNAME [. WWSECTIONNAME ] (
                // Check if its a raw call or an udp:
                bool isUpd = true;
                Assignment a = searchStatus.Parent as Assignment;
                if (a != null && a.Target == null)
                    // Its a raw call:
                    isUpd = false;

                return new Llamada()
                {
                    NodoLlamada = expression,
                    EsUdp = isUpd,
                    IsSubmit = false,
                    FormatoLlamadaAntiguo = false,
                    NombreObjeto = expressionComponents.ObjectName,
                    Parametros = expressionComponents.CallFunction.Parameters,
                    QualifiedNameCall = expressionComponents.QualifiedObjectName,
                    IsExternalObjectVariableCall = isExternalObjectVariableCall
                };
            }
        }

        private ObjectBase GetUnqualifiedName(ObjectBase objectName)
        {
            ObjectPEM compositeName = objectName as ObjectPEM;
            if (compositeName == null)
                // Name is already unqualified
                return objectName;

            // Get the object name unqualified:
            CallPemExpression expressionComponents = new CallPemExpression(compositeName,
                ModulesNames);
            return expressionComponents.ObjectName;
        }

        /// <summary>
        /// Verifica si un nodo del codigo es una llamada a un objeto genexus. Se ignoran
        /// llamadas a variables (p.ej call( &amp;achilipu ) o Link( &amp;arriquitaun.campo )
        /// </summary>
        /// <param name="nodo">Nodo de codigo a revisar </param>
        /// <param name="path">Path al nodo de codigo que se esta revisando</param>
        /// <returns>Informacion sobre la llamada</returns>
        public Llamada VerificarLlamada(ObjectBase nodo, ParsedCodeFinder.SearchState path)
        {

            // Check call type
            if (nodo is ObjectCall)
            {
                // Call of type "LINESTART PUEmpNom( &EmpCod , &EmpNom )", without a return value
                // The object name does not contains any module name.
                Function llamada = (Function)nodo;
                Word nombreObjeto = llamada.Name as Word;
                if (nombreObjeto == null)
                    return null;

                Llamada call = new Llamada()
                {
                    NodoLlamada = nodo,
                    EsUdp = false,
                    IsSubmit = false,
                    FormatoLlamadaAntiguo = false,
                    Parametros = llamada.Parameters,
                    QualifiedNameCall = nombreObjeto.ToString(),
                    NombreObjeto = nombreObjeto
                };
                return call;
            }
            else if (nodo is FunctionCommand)
            {
                // Call of type "LINESTART FUNCTION ( ...
                Function llamada = (Function)nodo;
                string nombre = llamada.Name.ToString().ToLower().Trim();
                if (EsPalabraClaveLlamada(nombre) && llamada.Parameters.Count > 0)
                {
                    ObjectBase nombreObjeto = llamada.Parameters[0];
                    if (nombreObjeto != null)
                    {
                        // Call of type "LINESTART call( [ModuleName.]ObjectName , &EmpCod , &EmpNom ), 
                        // without a return value. It uses always the old calls format.
                        
                        // Get qualified and unqualified object name:
                        string qualifiedName = nombreObjeto.ToString();
                        ObjectBase unqualifiedName = GetUnqualifiedName(nombreObjeto);

                        Llamada call = new Llamada()
                        {
                            NodoLlamada = nodo,
                            EsUdp = false,
                            IsSubmit = (nombre == KeywordGx.SUBMIT),
                            FormatoLlamadaAntiguo = true,
                            Parametros = llamada.Parameters,
                            QualifiedNameCall = qualifiedName,
                            NombreObjeto = unqualifiedName
                        };
                        return call;
                    }
                }
            }
            else if (nodo is Function)
            {
                // Una funcion. Ignorar las que pertenezcan a una indireccion 
                // (p. ej. &x.ToString())
                Function funcion = (Function)nodo;
                if (path.Parent is ObjectPEM)
                    return null;

                // Las demas son del tipo &EmpNom = PUEmpNom( &EmpCod ) 
                string nombreFuncion = funcion.Name.ToString().ToLower().Trim();

                // Es &EmpNom = PUEmpNom( &EmpCod ) o udp( PUEmpNom , &EmpCod )?
                bool esLlamadaAntigua = EsPalabraClaveLlamada(nombreFuncion);

                // Get the called function name:
                ObjectBase objectName;
                if (esLlamadaAntigua && funcion.Parameters.Count >= 1)
                    // The first parameter is the called object name ( udp( [Module.]PUEmpNom , &EmpCod ) )
                    objectName = funcion.Parameters[0];
                else
                    // New format: Call of type PUEmpNom( &EmpCod )
                    objectName = funcion.Name as Word;
                if (objectName == null)
                    return null;

                // Ver si es un upd o un call:
                // Si es una regla de la parte Rules de un objeto, es un call (espera recibir todos los parametros).
                // Si no es una regla, es un upd, excepto en el caso de los links
                // Los links esperan recibir todos los parametros
                bool esUdp = !(nodo is Rule || nombreFuncion == KeywordGx.LINK || nombreFuncion == KeywordGx.CREATE);

                string qualifiedName = objectName.ToString();
                ObjectBase unqualifiedName = GetUnqualifiedName(objectName);

                Llamada call = new Llamada()
                {
                    NodoLlamada = funcion,
                    EsUdp = esUdp,
                    IsSubmit = false,
                    FormatoLlamadaAntiguo = esLlamadaAntigua,
                    Parametros = funcion.Parameters,
                    QualifiedNameCall = qualifiedName,
                    NombreObjeto = unqualifiedName
                };
                return call;
            }
            else if (nodo is ObjectPEM)
                // Es una indireccion ( "TARGET.SUBEXPRESSION" )
                return CheckObjectPEMCall((ObjectPEM)nodo, path);

            return null;
        }
    }
}
