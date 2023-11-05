using System;
using System.Collections.Generic;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Language.Parser.Data;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Variables;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Llamadas.Edicion
{
    /// <summary>
    /// Operacion para añadir un parametro a una llamada de un objeto
    /// </summary>
    public class OpAgregarParametro : IOperacionLlamada
    {

        /// <summary>
        /// Posicion del parametro a añadir en las llamadas.
        /// </summary>
        private int PosicionParametro;

        /// <summary>
        /// Indica que el parametro que se va añadir es una variable. En este caso, se intentara
        /// crear en todos los llamadadores. Si es falso, se asume que se pone un texto literal en
        /// el nuevo parametro.
        /// </summary>
        private bool AgregarVariable;

        /// <summary>
        /// Valor a poner en el nuevo parametro. Si AgregarVariable es cierto, es el nombre de la 
        /// variable a poner sin "&amp;" inicial. Si es falso, es el literal a poner
        /// </summary>
        private string ValorNuevoParametro;

        /// <summary>
        /// Es el arbol de parseado de la expresion del nuevo parametro (ValorNuevoParametro)
        /// </summary>
        private ObjectBase ExpresionNuevoParametro;

        /// <summary>
        /// La lista de parametros del objeto llamado
        /// </summary>
        private List<ParameterElement> Parametros;

        /// <summary>
        /// Object (ICallableInfo) to edit calls
        /// </summary>
        private KBObject CalledObject;

        /// <summary>
        /// Constructor para añadir un parametro a los llamadores de un objeto
        /// </summary>
        /// <param name="o">Object (ICallableInfo) to edit calls</param>
        /// <param name="posicionParametro">Indice del parametro a añadir</param>
        /// <param name="valorNuevoParametro">Valor a poner como nuevo parametro en las llamadas</param>
        /// <param name="esVariable">Cierto si valorNuevoParametro es el nombre de una variable. 
        /// Falso si es un valor literal.</param>
        public OpAgregarParametro(KBObject o, int posicionParametro, string valorNuevoParametro,
            bool esVariable)
        {
            CalledObject = o;
            PosicionParametro = posicionParametro;
            ValorNuevoParametro = valorNuevoParametro;
            AgregarVariable = esVariable;
            Parametros = ReglaParm.ObtenerParametros(((ICallableInfo)CalledObject));

            InicializarExpresionParametro();
        }

        /// <summary>
        /// Inicializa el valor de ExpresionNuevoParametro
        /// </summary>
        private void InicializarExpresionParametro()
        {
            // String que se agregara a las llamadas:
            string codigoExpresion = ValorNuevoParametro;
            if (AgregarVariable)
            {
                codigoExpresion = "&" + ValorNuevoParametro.Trim();
                // Ver si la variable es un array. Si es asi, hay que añadir un "()" al valor pasado:
                ParameterElement parm = Parametros[PosicionParametro];
                if (!parm.IsAttribute)
                {
                    Variable varParametro = (Variable)parm.GetParameterObject(CalledObject);
                    if (!VariableGX.EsEscalar(varParametro))
                        codigoExpresion += "()";
                }
            }
            ExpresionNuevoParametro = ParserGx.ParsearExpresion(codigoExpresion);
        }

        /// <summary>
        /// Crea la variable usada como nuevo parametro en el objeto llamador
        /// </summary>
        /// <param name="llamador">El objeto llamador en el que crear la variable</param>
        /// <param name="infoObjeto">Informacion de la edicion del objeto</param>
        private void AgregarVariableParametro(KBObject llamador, InfoCambioLlamadasObjeto infoObjeto)
        {
            try
            {
                ParameterElement p = Parametros[PosicionParametro];
                VariablesPart parteVariables = llamador.Parts.LsiGet<VariablesPart>();
                if (parteVariables == null)
                {
                    InfoCambioLlamada info = new InfoCambioLlamada(Estado.WARNING, "Object " + llamador.QualifiedName + " cannot have variables");
                    infoObjeto.AgregarEstadoLlamada(info);
                    return;
                }

                // Verificar que no exista ya una variable con el mismo nombre que la que se va a crear
                Variable v = parteVariables.GetVariable(ValorNuevoParametro);
                if (v != null)
                {
                    // Ya existe una variable. 

                    // Verificar que sea del mismo tipo que la variable que se iba a crear:
                    bool varExistenteOk = true;
                    if (p.IsAttribute)
                    {
                        if (v.AttributeBasedOn == null || v.AttributeBasedOn.Id != ((Artech.Genexus.Common.Objects.Attribute)p.GetParameterObject(CalledObject)).Id)
                            // El parametro es un atributo, y la variable no esta basada en dicho atributo
                            varExistenteOk = false;
                    }
                    else
                    {
                        if (!VariableGX.TienenMismoTipo(v, (Variable)p.GetParameterObject(CalledObject)))
                            varExistenteOk = false;
                    }
                    if (!varExistenteOk)
                    {
                        // La variable existente no tiene el tipo esperado: Es un error.
                        InfoCambioLlamada info = new InfoCambioLlamada(Estado.ERROR,
                            "Variable with name " + ValorNuevoParametro + " already exists in caller " +
                            " and its type (" + VariableGX.DescripcionTipo(UIServices.KB.CurrentModel, v) +
                            ") does not match the parameter type"
                        );
                        infoObjeto.AgregarEstadoLlamada(info);
                    }

                    // Si no es una variable nula (empieza por z), dar un aviso: Cambiarle el valor
                    // podria romper el funcionamiento del objeto.
                    if (!ValorNuevoParametro.ToLower().StartsWith("z"))
                    {
                        InfoCambioLlamada info = new InfoCambioLlamada(Estado.WARNING, "Variable with name " + ValorNuevoParametro + " already exists");
                        infoObjeto.AgregarEstadoLlamada(info);
                    }
                    return;
                }

                // Crear la variable
                v = new Variable(ValorNuevoParametro, parteVariables);
                if (p.IsAttribute)
                    // El parametro es un atributo
                    v.AttributeBasedOn = (Artech.Genexus.Common.Objects.Attribute)p.GetParameterObject(CalledObject);
                else
                    // El parametro es una variable. Copiar sus propiedades
                    v.CopyPropertiesFrom((Variable)p.GetParameterObject(CalledObject));
                parteVariables.Add(v);
                llamador.Parts.LsiUpdatePart(parteVariables);

                infoObjeto.AgregarEstadoLlamada(new InfoCambioLlamada(Estado.OK, "Variable " + ValorNuevoParametro + " created"));

            }
            catch (Exception ex)
            {
                InfoCambioLlamada info = new InfoCambioLlamada(Estado.ERROR, "Error creating variable for parameter: " + ex.ToString());
                infoObjeto.AgregarEstadoLlamada(info);
            }
        }

        /// <summary>
        /// Hace los cambios necesarios en el objeto llamador, una unica vez, antes de empezar a cambiar
        /// cada una de las llamadas
        /// </summary>
        /// <param name="llamador">Objeto llamador a editar</param>
        /// <param name="infoObjeto">Informacion de edicion del objeto llamador</param>
        public void HacerCambiosGlobalesLlamador(KBObject llamador, InfoCambioLlamadasObjeto infoObjeto)
        {
            if (AgregarVariable)
                AgregarVariableParametro(llamador, infoObjeto);
        }

        /// <summary>
        /// Agrega el parametro a la llamada
        /// </summary>
        /// <param name="l">Llamada a editar</param>
        /// <returns>Informacion sobre el resultado del cambio en la llamada</returns>
        public InfoCambioLlamada EditarLlamada(Llamada l)
        {
            // Informacion de cambios en la llamada:
            InfoCambioLlamada infoCambioLlamada = new InfoCambioLlamada(l);

            // Ver que la llamada tenga el nº de parametros esperados:
            int nExpectedParameters = Parametros.Count - 1;

            if (l.EsUdp && !(CalledObject is DataSelector) )
                // Returned object will not be included as parameter
                nExpectedParameters--;

            if(l.IsSubmit)
                // The extra initial parameter
                nExpectedParameters++;

            if (l.NumeroParametros != nExpectedParameters)
            {
                infoCambioLlamada.EstadoCambioLlamada = Estado.ERROR;
                infoCambioLlamada.InformacionCambio += Environment.NewLine +
                    "ERROR: Call does not have the expected number of parameters . Expected: " +
                    nExpectedParameters + ", found: " + l.NumeroParametros;
                return infoCambioLlamada;
            }
            if (PosicionParametro > nExpectedParameters)
            {
                infoCambioLlamada.EstadoCambioLlamada = Estado.ERROR;
                infoCambioLlamada.InformacionCambio += Environment.NewLine +
                    "ERROR: Position where to insert the new parameter has not found.";
                if (l.EsUdp)
                    infoCambioLlamada.InformacionCambio += "Call is UDP. The modification " +
                        "of the returned parameter value is not supported";
                return infoCambioLlamada;
            }

            infoCambioLlamada.InformacionCambio += Environment.NewLine +
                "New call:" + Environment.NewLine +
                l.LlamadaConNuevoParametro(PosicionParametro, ExpresionNuevoParametro);

            // Hacer el cambio en el codigo:
            l.NuevoParametro(PosicionParametro, ExpresionNuevoParametro);

            return infoCambioLlamada;
        }

    }
}
