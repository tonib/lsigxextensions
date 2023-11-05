using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Language.Parser.Data;
using Artech.Architecture.Language.Parser.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common.Parts.Layout;
using Artech.Genexus.Common.Parts.WebForm;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using LSI.Packages.Extensiones.Utilidades.WinForms;
using Artech.Packages.Patterns.Objects;
using Artech.Genexus.Common.Objects;
using Artech.Patterns.WorkWithDevices.Objects;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using Artech.Patterns.WorkWithDevices.Parts;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
    /// <summary>
    /// Permite buscar si un objeto o una de sus partes hace referencia a un TokenGx.
    /// </summary>
    public class TokensFinder
    {

        /// <summary>
        /// El tipo de token a buscar
        /// </summary>
        public TokenGx Token;

        /// <summary>
        /// El conjunto de resultados de la busqueda en el codigo
        /// </summary>
        private List<ObjectBase> ResultadosBusqueda;

        /// <summary>
        /// Buscamos solo la primera referencia al token?
        /// </summary>
        private bool BuscarSoloPrimero;

        /// <summary>
        /// If it's true, parameters of "NullValue" GX function will be ignored
        /// </summary>
        /// <remarks>
        /// Nullvalue function don't use its parameter on execution time
        /// </remarks>
        public bool IgnoreNullValueParms;

        /// <summary>
        /// Construye el buscador para buscar cualquier variable
        /// </summary>
        public TokensFinder()
        {
            Token = new TokenGx();
        }

        /// <summary>
        /// Construye el buscador para buscar un cierto tipo de token
        /// </summary>
        /// <param name="token">Tokens a encontrar en el codigo</param>
        public TokensFinder(TokenGx token)
        {
            Token = token;
        }

        private void Ejecutar(bool soloPrimera, ParsedCode analizador)
        {
            BuscarSoloPrimero = soloPrimera;
            ResultadosBusqueda = new List<ObjectBase>();
            ParsedCodeFinder buscador = new ParsedCodeFinder(AnalizarNodoCodigo);
            buscador.Execute(analizador);
        }

        private void Ejecutar(KBObject objeto, bool soloPrimera, ObjectBase subarbolParseado)
        {
            BuscarSoloPrimero = soloPrimera;
            ResultadosBusqueda = new List<ObjectBase>();
            ParsedCodeFinder buscador = new ParsedCodeFinder(AnalizarNodoCodigo);
            buscador.Execute(objeto, subarbolParseado);
        }


        /// <summary>
        /// Cierto si el codigo contiene referencias al token buscado
        /// </summary>
        /// <param name="analizador">El codigo a analizar</param>
        /// <returns>Cierto si el codigo contiene alguna referencia</returns>
        public bool ContieneReferencia(ParsedCode analizador)
        {
            Ejecutar(true, analizador);
            return ResultadosBusqueda.Count > 0;
        }

        /// <summary>
        /// Cierto si el codigo contiene referencias al token buscado
        /// </summary>
        /// <param name="subarbol">El codigo a analizar</param>
        /// <returns>Cierto si el codigo contiene alguna referencia</returns>
        public bool ContieneReferencia(KBObject objeto, ObjectBase subarbol)
        {
            Ejecutar(objeto, true, subarbol);
            return ResultadosBusqueda.Count > 0;
        }

        /// <summary>
        /// Verifica si el webform contiene referencias al token buscado
        /// </summary>
        /// <param name="variables">Variables del objeto</param>
        /// <param name="webForm">El webform a revisar</param>
        /// <returns>Cierto si contiene una referencia al campo del sdt</returns>
        public bool ContieneReferencia(WebFormPart webForm)
        {
            return ContieneReferencia(webForm, true);
        }

        /// <summary>
        /// Verifica si el webform contiene referencias al token buscado
        /// </summary>
        /// <param name="variables">Variables del objeto</param>
        /// <param name="webForm">El webform a revisar</param>
        /// <param name="fullSearch">If its true, all control properties will be analized.
        /// Otherwise, only the main property ("Attribute") will be checked. The full search
        /// is much slower.</param>
        /// <returns>Cierto si contiene una referencia al campo del sdt</returns>
        public bool ContieneReferencia(WebFormPart webForm, bool fullSearch)
        {
            ControlTokenFinder buscador = new ControlTokenFinder(this);
            // Recorrer los tags web:
            foreach (IWebTag tag in WebFormHelper.EnumerateWebTag(webForm))
            {
                if (buscador.ReferencesToken(tag, fullSearch))
                    return true;
            }
            return false;
        }

        public bool ContieneReferencia(WinFormPart winForm)
        {
            return ContieneReferencia(winForm, true);
        }

        public bool ContieneReferencia(WinFormPart winForm, bool fullSearch)
        {
            ControlTokenFinder buscador = new ControlTokenFinder(this);
            foreach (FormElement control in EnumeradorWinform.EnumerarControles(winForm))
            {
                if (buscador.ReferencesToken(control, fullSearch))
                    return true;
            }
            return false;
        }

        public bool ContieneReferencia(LayoutPart layout)
        {
            ControlTokenFinder buscador = new ControlTokenFinder(this);
            foreach (ReportAttribute control in LayoutGx.EnumerarComponentes<ReportAttribute>(layout))
            {
                if (buscador.ReferencesToken(control))
                    return true;
            }
            return false;
        }

        public bool ContainsReference(VirtualLayoutPart layout)
        {
            return ContainsReference(layout, true);
        }

        public bool ContainsReference(VirtualLayoutPart layout, bool fullSearch)
        {
            SDPanel sd = layout.KBObject as SDPanel;
            if( sd == null )
                return false;

            ControlTokenFinder buscador = new ControlTokenFinder(this);
            foreach (PatternInstanceElement element in sd.PatternPart.PanelElement.LsiEnumerateDescendants())
            {
                if (buscador.ReferencesToken(element, fullSearch))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check if an object contains references to the token
        /// </summary>
        /// <param name="o">Object to check</param>
        /// <returns>True if the object contains references</returns>
        public bool ContainsReference(KBObject o)
        {

            // Search references on the object parts
            foreach (KBObjectPart part in o.Parts.LsiEnumerate())
            {
                if (part is ISource)
                {
                    // On sources is faster to search the code references
                    if (ContieneReferencia(new ParsedCode(part)))
                        return true;
                }
                else
                {
                    if (part is LayoutPart)
                    {
                        if (ContieneReferencia((LayoutPart)part))
                            return true;
                    }
                    else if (part is WinFormPart)
                    {
                        if (ContieneReferencia((WinFormPart)part))
                            return true;
                    }
                    else if (part is WebFormPart)
                    {
                        if (ContieneReferencia((WebFormPart)part))
                            return true;
                    }
                    else if (part is VirtualLayoutPart)
                    {
                        if (ContainsReference((VirtualLayoutPart)part))
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Search all token names that appear on a winform
        /// </summary>
        /// <param name="winForm">Winform to check</param>
        /// <param name="devolverAmpersand">Should the variables ampersand char be kept on names?</param>
        /// <returns>The token references on the winform</returns>
        public ControlsTokenReferences SearchAllNamesWithDetail(WinFormPart winForm, bool returnAmpersand)
        {
            ControlsTokenReferences references = new ControlsTokenReferences();
            ControlTokenFinder finder = new ControlTokenFinder(this);
            foreach (FormElement control in EnumeradorWinform.EnumerarControles(winForm))
            {
                if (ControlTokenFinder.CanContainReferences(control))
                    references.AddReferences(finder.SearchAllNamesWithDetail(control, returnAmpersand));
            }
            return references;
        }

        /// <summary>
        /// Devuelve todos los nombres de los tokens que aparecen en un printblock de un layout
        /// </summary>
        /// <param name="printBlock">El print block a analizar</param>
        /// <returns>Los nombres de las ocurrencias, en minusculas</returns>
        public HashSet<string> BuscarTodosNombres(IReportBand printBlock, bool devolverAmpersand)
        {
            HashSet<string> nombres = new HashSet<string>();
            ControlTokenFinder buscador = new ControlTokenFinder(this);
            foreach (ReportAttribute control in LayoutGx.EnumerarComponentes<ReportAttribute>(printBlock))
                nombres.LsiAddRange(buscador.SearchAllNames(control, devolverAmpersand));
            return nombres;
        }

        public HashSet<string> SearchAllNames(LayoutPart layout, bool returnAmpersand)
        {
            HashSet<string> names = new HashSet<string>();
            foreach (IReportBand printBlock in layout.Layout.ReportBands)
            {
                foreach (string name in BuscarTodosNombres(printBlock, returnAmpersand))
                    names.Add(name);
            }
            return names;
        }

        /// <summary>
        /// Devuelve todos los nombres de los tokens que aparecen en un winform
        /// </summary>
        /// <param name="winform">El form a analizar</param>
        /// <returns>Los nombres de las ocurrencias, en minusculas</returns>
        public ControlsTokenReferences SearchAllNamesWithDetail(WebFormPart webForm, bool returnAmpersand)
        {
            ControlsTokenReferences references = new ControlsTokenReferences();
            ControlTokenFinder finder = new ControlTokenFinder(this);
            // Recorrer los tags web:
            foreach (IWebTag control in WebFormHelper.EnumerateWebTag(webForm))
            {
                if (ControlTokenFinder.CanContainReferences(control))
                    references.AddReferences(finder.SearchAllNamesWithDetail(control, returnAmpersand));
            }
            return references;
        }

        /// <summary>
        /// Get all token names referenced by an abstract layout
        /// </summary>
        /// <param name="abstractLayout">The layout to check</param>
        /// <param name="devolverAmpersand">Should the variables ampersand char be kept on names?</param>
        /// <returns>All instance names, lowercase</returns>
        public ControlsTokenReferences SearchAllNamesWithDetail(VirtualLayoutPart abstractLayout, bool returnAmpersand)
        {
            ControlsTokenReferences references = new ControlsTokenReferences();
            SDPanel sd = abstractLayout.KBObject as SDPanel;
            if (sd == null)
                return references;

            ControlTokenFinder finder = new ControlTokenFinder(this);
            // Traverse the sd panel controls
            foreach (PatternInstanceElement element in sd.PatternPart.PanelElement.LsiEnumerateDescendants())
            {
                if (ControlTokenFinder.CanContainReferences(element))
                    references.AddReferences(finder.SearchAllNamesWithDetail(element, returnAmpersand));
            }
            return references;
        }

        /// <summary>
        /// Analiza un nodo del parseado de codigo Genexus, para ver si se corresponde con el token
        /// que se busca
        /// </summary>
        /// <param name="nodo">Nodo a analizar</param>
        /// <param name="state">Search on code tree state</param>
        protected virtual void AnalizarNodoCodigo(ObjectBase nodo, ParsedCodeFinder.SearchState state)
        {
            if (IgnoreNullValueParms)
            {
                Function function = nodo as Function;
                if (function != null && function.Name.ToString().ToLower() == "nullvalue")
                {
                    // It's a nullvalue function call. Ignore it.
                    state.SearchDescendants = false;
                    return;
                }
            }

            if (Token.EsToken(state.OwnerObject, nodo, state.Parent))
            {
                ResultadosBusqueda.Add(nodo);
                if (BuscarSoloPrimero)
                    state.SearchFinished = true;
            }
        }

        /// <summary>
        /// Devuelve todas las ocurrencias del token en el codigo
        /// </summary>
        /// <param name="analizador">El codigo a analizar</param>
        /// <returns>La lista de todas las ocurrencias</returns>
        public List<ObjectBase> BuscarTodas(ParsedCode analizador)
        {
            Ejecutar(false, analizador);
            return ResultadosBusqueda;
        }

        /// <summary>
        /// Devuelve todos los nombres de los tokens que aparecen en el codigo
        /// </summary>
        /// <param name="analizador">El codigo a analizar</param>
        /// <returns>Los nombres de las ocurrencias, en minusculas</returns>
        public HashSet<string> SearchAllNames(ParsedCode analizador, bool devolverAmpersand)
        {
            List<ObjectBase> ocurrencias = BuscarTodas(analizador);
            return TokenGx.ObtenerNombres(ocurrencias, devolverAmpersand);
        }

        /// <summary>
        /// Devuelve todos los nombres de los tokens que aparecen en el codigo
        /// </summary>
        /// <param name="analizador">El codigo a analizar</param>
        /// <returns>Los nombres de las ocurrencias, en minusculas</returns>
        public HashSet<string> BuscarTodosNombres(KBObject objeto, ObjectBase subarbol, bool devolverAmpersand)
        {
            List<ObjectBase> ocurrencias = BuscarTodas(objeto, subarbol);
            return TokenGx.ObtenerNombres(ocurrencias, devolverAmpersand);
        }

        /// <summary>
        /// Devuelve todas las ocurrencias del token en el codigo
        /// </summary>
        /// <param name="arbolParseado">El codigo a analizar</param>
        /// <returns>La lista de todas las ocurrencias</returns>
        public List<ObjectBase> BuscarTodas(KBObject objeto, ObjectBase subarbolParseado)
        {
            Ejecutar(objeto, false, subarbolParseado);
            return ResultadosBusqueda;
        }

        /// <summary>
        /// Check if the tokens search is not implemented for a given object
        /// </summary>
        /// <param name="o">Object to check</param>
        /// <returns>True if the object type is not supported</returns>
        static public bool IsUnsupportedObject(KBObject o)
        {
            return o is PatternInstance || o is DataProvider || o is ExternalObject /* || o is DataSelector */;
        }

    }
}
