using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Language.Parser.Data;
using Artech.Architecture.Language.Parser.Factories;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using Artech.Genexus.Common.CustomTypes;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Udm.Framework;
using Artech.Genexus.Common;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Llamadas
{
    /// <summary>
    /// Informacion detallada sobre una llamada a un objeto genexus
    /// </summary>
    public class Llamada
    {

        /// <summary>
        /// Nodo raiz de la llamada
        /// </summary>
        public ObjectBase NodoLlamada;

        /// <summary>
        /// Called object name. 
        /// This name is unqualified, it does not contains the module name. 
        /// </summary>
        public ObjectBase NombreObjeto;

        /// <summary>
        /// Is this a call to a member of an external object variable?
        /// </summary>
        public bool IsExternalObjectVariableCall;

        /// <summary>
        /// Qualified object name found on the call, case sensitive
        /// </summary>
        public string QualifiedNameCall;

        /// <summary>
        /// Cierto si es una llamada UDP / UDF. Falso si es un call
        /// </summary>
        public bool EsUdp;

        /// <summary>
        /// True if the call is a submit
        /// </summary>
        public bool IsSubmit;

        /// <summary>
        /// Cierto si es un formato de llamada antiguo: Esto es, si el nombre del objeto
        /// llamado esta dentro de los parametros ( p.ej. call( achilipu ) ).
        /// Falso si es el formato nuevo ( p.ej. achilipu.udp() )
        /// </summary>
        public bool FormatoLlamadaAntiguo;

        /// <summary>
        /// Los parametros pasados en la llamada al objeto
        /// </summary>
        public ObjectBaseCollection Parametros;

        /// <summary>
        /// Devuelve el nº de parametros que aparecen en la llamada. Si es un UDP, el parametro devuelto no se cuenta.
        /// En llamadas con formato antiguo (p.ej. "call( achilipu , &amp;x )"), el nombre del objeto
        /// NO se cuenta como un parametro.
        /// </summary>
        public int NumeroParametros
        {
            get
            {
                int nParametros = Parametros.Count;
                if (FormatoLlamadaAntiguo)
                    // Quitar el nombre del objeto
                    nParametros--;
                return nParametros;
            }
        }

        /// <summary>
        /// Devuelve el nº de parametros de la llamada, teniendo en cuenta si es un UDP. 
        /// Si es un UDP, el parametro devuelto SI se tiene en cuenta.
        /// En llamadas con formato antiguo (p.ej. "call( achilipu , &amp;x )"), el nombre del objeto
        /// NO se cuenta como un parametro.
        /// </summary>
        public int NRealPassedParameters
        {
            get
            {
                int nParametros = NumeroParametros;
                if (EsUdp)
                    nParametros++;
                if(IsSubmit)
                    // Remove the initial extra parameter for submit
                    nParametros--;

                return nParametros;
            }
        }

        /// <summary>
        /// Called qualified object name, lowercase
        /// </summary>
        public string NombreObjetoNormalizado
        {
            get
            {
                return NombreObjeto.ToString().Trim().ToLower();
            }
        }

        /// <summary>
        /// Texto con formato "(Fil:XXX, Col:YYY)" indicando la ubicación del inicio de la llamada
        /// </summary>
        public string TextoFilaColumna
        {
            get
            {
                return "(Row:" + NodoLlamada.Row + ", Col:" + NodoLlamada.CharPosition + ")";
            }
        }

        /// <summary>
        /// Devuelve el codigo de la llamada
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return TextoFilaColumna + ": " +  NodoLlamada.ToString();
        }

        /// <summary>
        /// Devuelve el string correspondiente a un parametro de la llamada.
        /// </summary>
        /// <remarks>El string puede contener comentarios</remarks>
        /// <param name="idx">Indice efectivo del parametro. Se ignora el nombre del objeto
        /// si este esta incluido en la llamada (p.ej call( PUEmpNom ))</param>
        /// <returns>El string correspondiente al parametro. null si no se encontro</returns>
        public string ObtenerStringParametro(int idx)
        {
            ObjectBase parm = ObtenerParametro(idx);
            if (parm == null)
                return null;
            return parm.ToString().Trim();
        }

        /// <summary>
        /// Devuelve el nodo del parseado correspondiente a un parametro de la llamada
        /// </summary>
        /// <param name="idx">Indice efectivo del parametro. Se ignora el nombre del objeto
        /// si este esta incluido en la llamada (p.ej call( PUEmpNom ))</param>
        /// <returns>El nodo correspondiente al parametro. null si no se encontro</returns>
        public ObjectBase ObtenerParametro(int idx)
        {
            if (FormatoLlamadaAntiguo)
                // Ignorar el nombre del objeto llamado en llamadas "call( objeto, parm1..."
                idx++;
            if( IsSubmit )
                // Ignore the initial unused parameter for submits "object.submit( stupidparm , param1 , parm2..."
                idx++;

            if (idx < 0 || idx >= Parametros.Count)
                return null;

            ObjectBase parametro = Parametros[idx];
            if (parametro is ListItem)
                // Quitar la coma del parametro y devolver solo el contenido:
                parametro = ((ListItem)parametro).Content;
            return parametro;

        }

        /// <summary>
        /// Crea una expresion con una coma inicial para poner como parametro en una llamada
        /// </summary>
        /// <param name="expresion">Expresion a la que poner una coma</param>
        /// <returns>La expresion con una coma previa</returns>
        static private ListItem CrearParametroConComa(ObjectBase expresion)
        {
            ListItem parametro = FactoryManager.Factory.GetListItemFactory().CreateListItemData();
            Punctuation coma = FactoryManager.Factory.GetPunctuationFactory().CreatePunctuationData();
            coma.Text = ", ";
            parametro.Punctuation = coma;
            parametro.Content = expresion;
            return parametro;
        }

        
        /// <summary>
        /// Devuelve el texto de la llamada cuando se añada un nuevo parametro
        /// </summary>
        /// <param name="posicionParametro">Indice en que ha de quedar el nuevo parametro.
        /// En llamadas con formato antiguo, el nombre del objeto no cuenta como parametro</param>
        /// <param name="expresion">Expresion a poner como nuevo parametro</param>
        /// <returns>El texto que tendra la llamada con el nuevo parametro</returns>
        public string LlamadaConNuevoParametro(int posicionParametro, ObjectBase expresion)
        {
            expresion = NuevoParametro(posicionParametro, expresion);
            string textoLlamada = NodoLlamada.ToString();
            QuitarParametro(posicionParametro);
            return textoLlamada;
        }

        /// <summary>
        /// Modifica el codigo de la llamada para añadir un parametro.
        /// </summary>
        /// <param name="parameterPosition">Indice en que ha de quedar el nuevo parametro.
        /// En llamadas con formato antiguo, el nombre del objeto no cuenta como parametro</param>
        /// <param name="expresion">Expresion a poner como nuevo parametro</param>
        /// <returns>El nuevo parametro agregado. Puede ser la propia expresion o bien
        /// un ListItem con la expresion y una coma previa</returns>
        public ObjectBase NuevoParametro(int parameterPosition, ObjectBase expresion)
        {
            if (FormatoLlamadaAntiguo)
                // Ignorar el nombre del objeto llamado en llamadas "call( objeto, parm1..."
                parameterPosition++;
            if(IsSubmit)
                // Ignore the initial extra parameter "object.submit( '' , parm1 ..."
                parameterPosition++;

            if (parameterPosition > 0 && !(expresion is ListItem))
                // Añadir una coma
                expresion = CrearParametroConComa(expresion);
            else if (parameterPosition == 0 && Parametros.Count > 0 )
            {
                // Añadir la coma al segundo parametro
                ObjectBase segundoParametro = CrearParametroConComa(Parametros[0]);
                Parametros[0] = segundoParametro;
            }

            if (expresion == null)
                throw new Exception("Expression cannot be null");

            Parametros.Insert(parameterPosition, expresion);
            return expresion;
        }

        /// <summary>
        /// Borra un parametro de una llamada
        /// </summary>
        /// <param name="parameterPosition">Posicion del parametro a borrar.
        /// En llamadas con formato antiguo, el nombre del objeto no cuenta como parametro
        /// </param>
        /// <returns>El parametro quitado</returns>
        public ObjectBase QuitarParametro(int parameterPosition)
        {
            if (FormatoLlamadaAntiguo)
                // Ignorar el nombre del objeto llamado en llamadas "call( objeto, parm1..."
                parameterPosition++;
            if (IsSubmit)
                // Ignore the initial extra parameter "object.submit( '' , parm1 ..."
                parameterPosition++;

            if (parameterPosition < 0 | parameterPosition >= Parametros.Count)
                throw new Exception("Parameter index " + parameterPosition +
                    " not found at call " + NodoLlamada.ToString());

            // Si el parametro quitado era el primero, hay que quitar la coma inicial del segundo
            if (parameterPosition == 0 && Parametros.Count >= 2 && Parametros[1] is ListItem)
            {
                ListItem segundoParametro = (ListItem)Parametros[1];
                Parametros.RemoveAt(1);
                Parametros.Insert(1, segundoParametro.Content);
            }

            ObjectBase parametro = Parametros[parameterPosition];
            Parametros.RemoveAt(parameterPosition);
            return parametro;
        }

        /// <summary>
        /// Devuelve el texto de la llamada cuando se quite un parametro
        /// </summary>
        /// <param name="posicionParametro">Indice del parametro a quitar.
        /// En llamadas con formato antiguo, el nombre del objeto no cuenta como parametro</param>
        /// <returns>El texto que tendra la llamada quitando el parametro</returns>
        public string LlamadaQuitandoParametro(int posicionParametro)
        {
            ObjectBase parametro = QuitarParametro(posicionParametro);
            string llamada = NodoLlamada.ToString();
            NuevoParametro(posicionParametro, parametro);
            return llamada;
        }

        /// <summary>
        /// Reemplaza el objeto llamado por otro
        /// </summary>
        /// <param name="nuevoObjeto">Nuevo objeto a poner en la llamada</param>
        public void CambiarObjetoLlamado(KBObject nuevoObjeto)
        {
            Word word = this.NombreObjeto as Word;
            if (word != null)
                word.Text = nuevoObjeto.Name;
            else
                throw new Exception(this.ToString() + " has no constant function name");
        }

        /// <summary>
        /// Get the called object key
        /// </summary>
        /// <param name="caller">Caller object</param>
        /// <returns>The called object key. EntityKey.Empty if it was not found</returns>
        public EntityKey GetCalledObjectKey(KBObject caller)
        {
            ResolveKeyResult resolveResult;
            caller.Model.Objects.ResolveName(caller.Module, null,
                QualifiedNameCall, out resolveResult);
            
            // Get the first callable object on the result:
            foreach (EntityKey key in resolveResult.Matches)
            {
                if (key.Type == ObjClass.WorkWithDevices)
                    return key;
                if (key.Type == ObjClass.SDPanel)
                    return key;
                if (key.Type != ObjClass.SDT && ObjClassLsi.LsiIsCallableType(key.Type))
                    return key;
            }
            return EntityKey.Empty;
        }

        /// <summary>
        /// Get the called object
        /// </summary>
        /// <param name="caller">Caller object</param>
        /// <returns>The called object. null if it was not found</returns>
        public KBObject GetCalledObject(KBObject caller)
        {
            EntityKey key = GetCalledObjectKey(caller);
            if (key == null)
                return null;
            return caller.Model.Objects.Get(key);
        }

        /// <summary>
        /// True if the object name called is qualified (it contains a '.' character)
        /// </summary>
        public bool ObjectNameIsQualified
        {
            get { return QualifiedNameCall.Contains("."); }
        }

    }
}
