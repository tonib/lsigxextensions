using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Artech.Architecture.Language.Parser.Data;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common.CustomTypes;
using Artech.Common.Properties;
using Artech.Genexus.Common.Parts.WebForm;
using Artech.Genexus.Common;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Parts.Layout;
using Artech.Genexus.Common.Types;
using Artech.Genexus.Common.Parts;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens
{
    /// <summary>
    /// Definicion de un "token" al que buscar referencias en un codigo Genexus
    /// </summary>
    /// <remarks>Puede ser un cierto atributo, una cierta variable, cualquier variable,
    /// cualquier atributo, un campo de un SDT, etc.</remarks>
    public class TokenGx
    {

        /// <summary>
        /// Tipo de token a buscar: Variables, atributos o ambos
        /// </summary>
        public TokenType Tipo;

        /// <summary>
        /// Indica si el token a buscar debe incluir indirecciones.
        /// </summary>
        /// <remarks>
        /// Por ejemplo si buscamos variables con nombre x, y se encuentra una expresion
        /// "&amp;x.Campo.Funcion().OtroCampo", si AceptarIndirecciones es cierto, se devolvera
        /// la indireccion completa. Si es falso, solo se devolvera "&amp;x". 
        /// Por defecto es falso
        /// </remarks>
        public bool AcceptIndirections;

        /// <summary>
        /// Indica si hay que verificar que los nombre de atributos encontrados sean realmente
        /// atributos.
        /// </summary>
        /// <remarks>
        /// La busqueda de atributos tambien encuentra las referencias a dominios enumerados, 
        /// llamadas en formato antiguo ("call( objeto )"), etc.
        /// Si este valor se pone a cierto, se descarta todo lo que no sean realmente nombres
        /// de atributos . Si se marca esto, la busqueda sera MUCHO mas lenta. Por defecto es falso.
        /// </remarks>
        public bool VerificarAtributos;

        /// <summary>
        /// Lista de nombres de tokens a buscar, en minusculas y sin espacios
        /// </summary>
        /// <remarks>
        /// Si es un nombre de variable, debe ir sin ampersand.
        /// Si es nulo, se buscan todos los del tipo indicado, independientemente de su nombre.
        /// </remarks>
        private List<string> NombresToken;

        /// <summary>
        /// Filtro para buscar solo ciertos tipos de indirecciones. Solo se aplica si 
        /// AceptarIndirecciones es cierto. Si es nulo, no se filtra.
        /// </summary>
        public TokenIndirectionFilter IndirectionsFilter;

        /// <summary>
        /// Filter to match only variables of some types. If it's null this filter will be ignored.
        /// </summary>
        public FiltroTiposToken FiltroTipos;

        /// <summary>
        /// If its not null, the regular expression to search the token on external code nodes
        /// (ex. "CSHARP [!&amp;x!] = 1;"). If it's null external code regions will be ignored
        /// </summary>
        private Regex ExternalCodeRegex;

        private void InicializarListaNombres(List<string> nombres)
        {
            if (nombres == null)
                return;
            NombresToken = new List<string>(nombres.Count);
            foreach (string nombre in nombres)
                NombresToken.Add(nombre.ToLower().ToString());
        }

        /// <summary>
        /// Construye el buscador para buscar un cierto token en el codigo, sin indirecciones
        /// </summary>
        /// <param name="tipo">Tipo de elemento a buscar en el codigo: variables o atributos</param>
        /// <param name="token">El nombre del token (variable/atributo) a buscar en el codigo.
        /// Si es nulo, se busca cualquier nombre
        /// </param>
        public TokenGx(TokenType tipo, string token)
        {
            Tipo = tipo;
            List<string> nombres = null;
            if (token != null)
            {
                nombres = new List<string>(1);
                nombres.Add(token);
            }
            InicializarListaNombres(nombres);
        }

        /// <summary>
        /// Construye el buscador para buscar un cierto atributo en el codigo
        /// </summary>
        /// <param name="atributo">El nombre del atributo a buscar en el codigo</param>
        public TokenGx(string atributo) 
            : this(TokenType.ATTRIBUTE, atributo)
        { }

        /// <summary>
        /// Construye el buscador para buscar todas las variables en el codigo
        /// </summary>
        public TokenGx()
            : this(TokenType.VARIABLE, (string)null)
        { }

        /// <summary>
        /// Construye el buscador para buscar tokens del tipo dado, con cualquier nombre
        /// </summary>
        public TokenGx(TokenType tipo)
            : this(tipo, (string)null)
        { }

        /// <summary>
        /// Construye el buscador para buscar tokens del tipo y alguno de los nombres dados
        /// </summary>
        /// <param name="tipo">Tipo de token a buscar</param>
        /// <param name="nombres">La lista de nombres a buscar. Nulo para buscar cualquiera</param>
        public TokenGx(TokenType tipo, List<string> nombres)
            :this(tipo, nombres, false)
        { }

        /// <summary>
        /// Construye el buscador para buscar tokens del tipo y alguno de los nombres dados
        /// </summary>
        /// <param name="tipo">Tipo de token a buscar</param>
        /// <param name="nombres">La lista de nombres a buscar. Nulo para buscar cualquiera</param>
        /// <param name="searchExternalCode"> True if we should search on external code regions 
        /// (ex. "CSHARP [!&amp;x!] = 1;"). If it's false, external code regions will be ignored</param>
        public TokenGx(TokenType tipo, List<string> nombres, bool searchExternalCode)
        {
            Tipo = tipo;
            InicializarListaNombres(nombres);
            if (searchExternalCode)
            {
                // The variable/attribute identifier ("&x" and / or "Atribute")
                string idExp = @"\w+";
                if (Tipo == TokenType.VARIABLE)
                    idExp = @"\&" + idExp;
                else if( Tipo == TokenType.VARIABLE_OR_ATTRIBUTE)
                    idExp = @"\&?" + idExp;

                // Identifier capture
                string captureId = @"(?<Id>" + idExp + @")";

                // Instances into external code ("[!&x!]")
                string exp = @"\[\!" + captureId + @"\!\]";

                ExternalCodeRegex = new Regex(exp, RegexOptions.IgnoreCase);
            }
        }

        static public string NormalizeName(string nombre, bool devolverAmpersand)
        {
            // Normalizar el nombre
            nombre = nombre.ToLower().Trim();
            if (!devolverAmpersand && nombre.StartsWith("&"))
                nombre = nombre.Substring(1);

            return nombre;
        }

        static public string NormalizeName(WordWithId referencia, bool devolverAmpersand)
        {
            return NormalizeName(referencia.Text, devolverAmpersand);
        }

        private bool RevisarNombreReferencia(string nombre)
        {
            if (NombresToken == null)
                return true;

            nombre = NormalizeName(nombre, false);
            return NombresToken.Contains(nombre);
        }

        private bool RevisarNombreReferencia(WordWithId referencia)
        {
            if (NombresToken == null)
                return true;

            string nombre = NormalizeName(referencia, false);
            return NombresToken.Contains(nombre);
        }

        private bool EsReferenciaBase(KBObject objeto, ObjectBase nodoParser)
        {
            // Check external code ("CSHARP [!&x!] = 0;")
            string externalCode;
            if (ExternalCodeRegex != null && nodoParser.LsiIsExternalCode(out externalCode))
                return ExternalCodeRegex.Match(externalCode).Success;

            if (Tipo == TokenType.ATTRIBUTE || Tipo == TokenType.VARIABLE_OR_ATTRIBUTE)
            {
                // Ver si es un atributo
                AttributeName nombreAtributo = nodoParser as AttributeName;
                if (nombreAtributo != null)
                {
                    // Ver si el nombre es correcto:
                    if (!RevisarNombreReferencia(nombreAtributo))
                        return false;

                    if (VerificarAtributos && 
                            Artech.Genexus.Common.Objects.Attribute.Get(UIServices.KB.CurrentModel, 
                            nombreAtributo.Text) == null
                        )
                        // No es realmente un atributo
                        return false;

                    return true;
                }
            }

            if (Tipo == TokenType.VARIABLE || Tipo == TokenType.VARIABLE_OR_ATTRIBUTE)
            {
                // Ver si es una variable
                if (nodoParser is VariableName)
                {
                    // Revisar el nombre
                    if (!RevisarNombreReferencia((VariableName)nodoParser))
                        return false;

                    // Revisar el tipo de la variable
                    if( FiltroTipos != null && 
                        !FiltroTipos.CumpleFiltro(objeto.Parts.LsiGet<VariablesPart>(), nodoParser) )
                        return false;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica si un nodo de codigo del parser cumple con la definicion
        /// del token
        /// </summary>
        /// <param name="objeto">Objeto al que pertenece el nodo de codigo</param>
        /// <param name="nodoParser">Nodo a analizar</param>
        /// <param name="nodoPadre">Nodo padre de nodoParser</param>
        /// <returns>Cierto si el nodo cumple con la definicion</returns>
        public bool EsToken(KBObject objeto, ObjectBase nodoParser, ObjectBase nodoPadre)
        {
 
            if (AcceptIndirections)
            {
                // Si se aceptan indirecciones, no revisar nada cuyo padre sea una indireccion
                // porque el padre ya se habra revisado
                if( nodoPadre is ObjectPEM ) 
                    return false;
                
                if (IndirectionsFilter != null && !(nodoParser is ObjectPEM))
                    // Hay un filtro por indirecciones, pero el nodo no es una indireccion
                    return false;

                if (nodoParser is ObjectPEM)
                {
                    ObjectPEM indireccion = (ObjectPEM)nodoParser;

                    // Ver si se pasa el filtro de indirecciones
                    if (IndirectionsFilter != null && !IndirectionsFilter.CumpleConFiltro(indireccion))
                        return false;

                    // Ver si la base de la indireccion cumple con el filtro del token:
                    nodoParser = ObjectPEMGx.ObtenenerExpresionBase(indireccion).Target;
                }
            }

            return EsReferenciaBase(objeto, nodoParser);
        }

        /// <summary>
        /// It searches the set of tokens referenced into a external code node 
        /// </summary>
        /// <param name="codeNode">The code node to analyze</param>
        /// <param name="returnAmpersand">True if the character ampersand should be keept in variable
        /// names</param>
        /// <returns>Set of tokens found</returns>
        public HashSet<string> GetExternalCodeTokens(ObjectBase codeNode, bool returnAmpersand)
        {
            HashSet<string> refs = new HashSet<string>();
            string externalCode;
            if (ExternalCodeRegex != null && codeNode.LsiIsExternalCode(out externalCode))
            {
                foreach (Match m in ExternalCodeRegex.Matches(externalCode))
                    refs.Add( NormalizeName( m.Groups["Id"].Value , returnAmpersand ) );
            }
            return refs;
        }

        /// <summary>
        /// Revisa si una referencia a un atributo/variable se corresponde con el token
        /// </summary>
        /// <remarks>
        /// Esta funcion no revisa el filtro de indirecciones
        /// </remarks>
        /// <param name="referencia">Referencia a revisar</param>
        /// <returns>Cierto si es una referenci al token</returns>
        internal bool EsToken(KBObject objeto, AttributeVariableReference referencia) 
        {
            // Si filtramos para buscar solo indirecciones, AttributeVariableReference no
            // contiene informacion suficiente:
            if (IndirectionsFilter != null)
                return false;

            if (Tipo == TokenType.VARIABLE && referencia.IsAttribute)
                return false;
            if (Tipo == TokenType.ATTRIBUTE && referencia.IsVariable)
                return false;

            // Revisar el nombre
            string nombre = referencia.Name;
            if (!RevisarNombreReferencia(nombre))
                return false;

            // Revisar el tipo de la variable
            if (FiltroTipos != null && referencia.IsVariable && 
                !FiltroTipos.CumpleFiltro(objeto.Parts.LsiGet<VariablesPart>(), nombre) )
                return false;

            return true;
        }

        internal bool EsToken(KBObject objeto, SDTLevelType refSdt)
        {
            if (Tipo == TokenType.ATTRIBUTE)
                return false;

            if (refSdt.Variable == null)
                // Por si acaso (variables no declaradas)
                return false;

            // Revisar el nombre
            string nombre = refSdt.Variable.Name;
            if (!RevisarNombreReferencia(nombre))
                return false;
            
            // Revisar el tipo de la variable
            if (FiltroTipos != null && 
                !FiltroTipos.CumpleFiltro(objeto.Parts.LsiGet<VariablesPart>(), nombre))
                return false;

            // Revisar el filtro de indirecciones
            if (IndirectionsFilter != null && !IndirectionsFilter.CumpleConFiltro(refSdt))
                return false;

            return true;
        }

        /// <summary>
        /// Devuelve el nombre de la variable/atributo base de una instancia de un token
        /// </summary>
        /// <param name="referencia">Referencia al token</param>
        /// <param name="devolverAmpersand">Si es cierto, en los nombre de las variables se 
        /// devuelve el ampersand inicial</param>
        /// <returns>El nombre de la variable/atributo, en minusculas.</returns>
        static public string ObtenerNombre(ObjectBase referencia, bool devolverAmpersand) 
        {
            if (referencia is ObjectPEM) 
            {
                // Es una indireccion
                ObjectPEM expresion = (ObjectPEM)referencia;
                expresion = ObjectPEMGx.ObtenenerExpresionBase(expresion);
                return NormalizeName((WordWithId)expresion.Target, devolverAmpersand);
            }

            // Es una referencia a un atributo / variable directa
            return NormalizeName((WordWithId)referencia, devolverAmpersand);
            
        }

        /// <summary>
        /// Devuelve los nombres de las variables/atributos base de una lista de instancias de tokens
        /// </summary>
        /// <param name="ocurrencias">Referencias a los tokens</param>
        /// <param name="devolverAmpersand">Si es cierto, en los nombre de las variables se 
        /// devuelve el ampersand inicial</param>
        /// <returns>Los nombres de las variables/atributos, en minusculas. Si es una variable, se devuelve
        /// sin el ampersand inicial</returns>
        static public HashSet<string> ObtenerNombres(List<ObjectBase> ocurrencias, bool devolverAmpersand)
        {
            HashSet<string> nombres = new HashSet<string>();
            foreach (ObjectBase ocurrencia in ocurrencias)
                nombres.Add(ObtenerNombre(ocurrencia, devolverAmpersand));
            return nombres;
        }

    }
}
