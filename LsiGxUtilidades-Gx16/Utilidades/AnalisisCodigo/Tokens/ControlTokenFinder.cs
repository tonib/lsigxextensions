using System;
using System.Collections.Generic;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Language.Parser;
using Artech.Common.Properties;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common.Parts.Layout;
using Artech.Genexus.Common.Parts.WebForm;
using Artech.Packages.Patterns.Objects;
using Artech.Packages.Patterns.Specification;
using Artech.Patterns.WorkWithDevices;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens
{
    /// <summary>
    /// Tool to search references to some token on forms controls
    /// </summary>
    public class ControlTokenFinder
    {

        /// <summary>
        /// Buscador que usa esta clase
        /// </summary>
        private TokensFinder Buscador;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buscador">Buscador que usa esta clase</param>
        public ControlTokenFinder(TokensFinder buscador)
        {
            this.Buscador = buscador;
        }

        /// <summary>
        /// Devuelve la lista de propiedades actualmente aplicables a un control.
        /// </summary>
        /// <param name="objeto">Control a revisar</param>
        /// <returns>La lista de propiedades del control que pueden tener actualmente valor</returns>
        static private List<PropertyManager.PropertySpecDescriptor> ObtenerPropiedadesAplicables(
            PropertiesObject objeto)
        {

            List<PropertyManager.PropertySpecDescriptor> propiedadesVisibles = new List<PropertyManager.PropertySpecDescriptor>();
            foreach (PropertyManager.PropertySpecDescriptor p in objeto.GetPropertiesDescriptors())
            {
                if (p.IsBrowsable)
                    propiedadesVisibles.Add(p);
            }
            return propiedadesVisibles;
        }

        private ControlsTokenReferences SearchAllNames(KBObject objeto, PropertiesObject
            controlProperties, object control, bool devolverAmpersand)
        {
            // REMEMBER: ReferencesToken and SearchAllNames members are associated. Change both
            //HashSet<string> referencias = new HashSet<string>();
            ControlsTokenReferences references = new ControlsTokenReferences();

            // Las propiedades realmente aplicables al control actualmente (no todas)
            List<PropertyManager.PropertySpecDescriptor> propiedadesRevisar =
                ObtenerPropiedadesAplicables(controlProperties);

            // Referencias a strings con codigo.
            Type tipoRefVarAtr = typeof(AttributeVariableReference);
            Type tipoParseable = typeof(ParseableString);
            Type tipoRefSdt = typeof(SDTLevelType);
            Type typeCodeString = typeof(CodeString);

            bool isPatternInstance = (control is PatternInstanceElement);

            foreach (PropertyManager.PropertySpecDescriptor p in propiedadesRevisar)
            {
                Type propertyType = p.PropertyType;
                if (tipoRefVarAtr.IsAssignableFrom(propertyType))
                {
                    AttributeVariableReference referencia = controlProperties.GetPropertyValue(p.Name)
                        as AttributeVariableReference;
                    if (referencia != null && Buscador.Token.EsToken(objeto, referencia))
                        references.HardReferencedNames.Add(TokenGx.NormalizeName(referencia.Name, devolverAmpersand));
                }
                else if (tipoParseable.IsAssignableFrom(propertyType))
                {
                    ParseableString ps = controlProperties.GetPropertyValue(p.Name)
                        as ParseableString;
                    if (ps != null)
                    {
                        ParsedCode codigo = new ParsedCode(objeto, ps);
                        references.CodeReferencedNames.LsiAddRange(
                            Buscador.SearchAllNames(codigo, devolverAmpersand));
                    }
                }
                else if (tipoRefSdt.IsAssignableFrom(propertyType))
                {
                    SDTLevelType refSdt = controlProperties.GetPropertyValue(p.Name) as SDTLevelType;
                    if (refSdt != null)
                    {
                        if (Buscador.Token.EsToken(objeto, refSdt))
                            references.HardReferencedNames.Add(TokenGx.NormalizeName(refSdt.Variable.Name, devolverAmpersand));
                    }
                }
                else if (typeCodeString.IsAssignableFrom(propertyType))
                {
                    CodeString code = controlProperties.GetPropertyValue(p.Name) as CodeString;
                    if (code != null)
                    {
                        ParsedCode codeParser = new ParsedCode(objeto, p, code);
                        references.CodeReferencedNames.LsiAddRange( 
                            Buscador.SearchAllNames(codeParser, devolverAmpersand) );
                    }
                }
                else if (isPatternInstance && p.Name == "fieldSpecifier")
                {
                    // SDPanels dont store the field specifier as a ParseableString (fuck), so
                    // we need a extra conversion: Get the full reference (&variable.member):
                    string fullReference = ((PatternInstanceElement)control).LsiGetFullFieldSpecifier();
                    if (!string.IsNullOrEmpty(fullReference))
                    {
                        ParsedCode codeParser = new ParsedCode(objeto, ParserType.Expressions, fullReference);
                        references.CodeReferencedNames.LsiAddRange(
                            Buscador.SearchAllNames(codeParser, devolverAmpersand));
                    }
                }
            }

            return references;
        }

        /// <summary>
        /// Return all name references on control properties
        /// </summary>
        /// <param name="element">Control to check</param>
        /// <param name="returnAmpersand">True if the "&amp;" of variables should be included on the
        /// returned names</param>
        /// <returns>Reference names</returns>
        public ControlsTokenReferences SearchAllNamesWithDetail(IWebTag control, bool devolverAmpersand)
        {
            return SearchAllNames(control.ContainerObject, control.Properties, control, devolverAmpersand);
        }

        /// <summary>
        /// Return all name references on control properties
        /// </summary>
        /// <param name="element">Control to check</param>
        /// <param name="returnAmpersand">True if the "&amp;" of variables should be included on the
        /// returned names</param>
        /// <returns>Reference names</returns>
        public ControlsTokenReferences SearchAllNamesWithDetail(PatternInstanceElement element, bool returnAmpersand)
        {
            return SearchAllNames(element.Instance.OwnerObject, element.Attributes, element, returnAmpersand);
        }

        /// <summary>
        /// Return the references to the token on a control
        /// </summary>
        /// <param name="control">Control to check</param>
        /// <param name="returnAmpersand">True if the variables ampersand should be included on 
        /// variables names</param>
        /// <returns>The set of names of the references</returns>
        public HashSet<string> SearchAllNames(FormElement control, bool returnAmpersand)
        {
            // Revisar las propiedades aplicables:
            return SearchAllNamesWithDetail(control, returnAmpersand).AllReferences;
        }

        /// <summary>
        /// Return the references to the token on a control
        /// </summary>
        /// <param name="control">Control to check</param>
        /// <param name="returnAmpersand">True if the variables ampersand should be included on 
        /// variables names</param>
        /// <returns>The set of names of the references</returns>
        public ControlsTokenReferences SearchAllNamesWithDetail(FormElement control, bool returnAmpersand)
        {
            // Revisar las propiedades aplicables:
            return SearchAllNames(control.KBObject, control, control, returnAmpersand);
        }

        /// <summary>
        /// Devuelve las referencias al token en propiedades un control 
        /// </summary>
        /// <param name="control">Control a revisar</param>
        /// <returns>La lista de nombres de las referencias</returns>
        public HashSet<string> SearchAllNames(ReportAttribute control,
            bool devolverAmpersand)
        {
            // Revisar las propiedades aplicables:
            return SearchAllNames(control.KBObject, control, control, devolverAmpersand).AllReferences;
        }

        private bool ReferencesToken(KBObject objeto, PropertiesObject controlProperties, object control)
        {
            // REMEMBER: ReferencesToken and SearchAllNames members are associated. Change both

            // Las propiedades realmente aplicables al control actualmente (no todas)
            List<PropertyManager.PropertySpecDescriptor> propiedadesRevisar =
                ObtenerPropiedadesAplicables(controlProperties);

            // Tipos de las propiedades que buscamos: Referencias a variables/atributos y strings
            // con codigo.
            Type tipoRefVarAtr = typeof(AttributeVariableReference);
            Type tipoParseable = typeof(ParseableString);
            Type tipoRefSdt = typeof(SDTLevelType);
            Type typeCodeString = typeof(CodeString);

            bool isPatternInstance = (control is PatternInstanceElement);

            foreach (PropertyManager.PropertySpecDescriptor p in propiedadesRevisar)
            {
                Type tipoPropiedad = p.PropertyType;
                if (tipoRefVarAtr.IsAssignableFrom(tipoPropiedad))
                {
                    AttributeVariableReference referencia = controlProperties.GetPropertyValue(p.Name)
                        as AttributeVariableReference;
                    if (referencia != null && Buscador.Token.EsToken(objeto, referencia))
                        return true;
                }
                else if (tipoParseable.IsAssignableFrom(tipoPropiedad))
                {
                    ParseableString ps = controlProperties.GetPropertyValue(p.Name)
                        as ParseableString;
                    if (ps != null)
                    {
                        ParsedCode codigo = new ParsedCode(objeto, ps);
                        if (Buscador.ContieneReferencia(codigo))
                            return true;
                    }
                }
                else if (tipoRefSdt.IsAssignableFrom(tipoPropiedad))
                {
                    SDTLevelType refSdt = controlProperties.GetPropertyValue(p.Name) as SDTLevelType;
                    if (refSdt != null)
                    {
                        if (Buscador.Token.EsToken(objeto, refSdt))
                            return true;
                    }
                }
                else if (typeCodeString.IsAssignableFrom(tipoPropiedad))
                {
                    CodeString code = controlProperties.GetPropertyValue(p.Name) as CodeString;
                    if (code != null)
                    {
                        ParsedCode codeParser = new ParsedCode(objeto, p, code);
                        if (Buscador.ContieneReferencia(codeParser))
                            return true;
                    }
                }
                else if (isPatternInstance && p.Name == "fieldSpecifier")
                {
                    // SDPanels dont store the field specifier as a ParseableString (fuck), so
                    // we need a extra conversion: Get the full reference (&variable.member):
                    string fullReference = ((PatternInstanceElement)control).LsiGetFullFieldSpecifier();
                    if (!string.IsNullOrEmpty(fullReference))
                    {
                        ParsedCode codeParser = new ParsedCode(objeto, ParserType.Expressions, fullReference);
                        if (Buscador.ContieneReferencia(codeParser))
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica si un control en un layout hace referencia a un token de los que buscamos
        /// </summary>
        /// <param name="control">Control a revisar</param>
        /// <returns>Cierto si es un token</returns>
        public bool ReferencesToken(ReportAttribute control)
        {
            return ReferencesToken(control.KBObject, control, control);
        }

        static public bool CanContainReferences(IWebTag control)
        {
            return control.Type == WebTagType.Attribute || control.Type == WebTagType.Column || 
                control.Type == WebTagType.UserControl || control.Type == WebTagType.FreeStyleGrid ||
                control.Type == WebTagType.Grid;
        }

        /// <summary>
        /// Verifica si un control en un webform hace referencia a un token de los que buscamos
        /// </summary>
        /// <param name="control">Control a revisar</param>
        /// <param name="fullSearch">If its true, all control properties will be analized.
        /// Otherwise, only the main property ("Attribute") will be checked. The full search
        /// is much slower.</param>
        /// <returns>Cierto si es un token</returns>
        public bool ReferencesToken(IWebTag control, bool fullSearch)
        {
            if (fullSearch)
            {
                if (CanContainReferences(control))
                    return ReferencesToken(control.ContainerObject, control.Properties, control);
            }
            else
            {
                // Check only the main property
                AttributeVariableReference att = null;

                if (control.Type == WebTagType.Attribute)
                    att = control.Properties.GetPropertyValue<AttributeVariableReference>(Properties.HTMLATT.Attribute);
                else if (control.Type == WebTagType.Column)
                    att = control.Properties.GetPropertyValue<AttributeVariableReference>(Properties.HTMLSFLCOL.Attribute);

                if (att != null)
                    return Buscador.Token.EsToken(control.ContainerObject, att);
            }
            return false;
        }

        /// <summary>
        /// Verifica si un control en un winform hace referencia a un token de los que buscamos
        /// </summary>
        /// <param name="control">Control a revisar</param>
        /// <param name="fullSearch">If its true, all control properties will be analized.
        /// Otherwise, only the main property ("Attribute") will be checked. The full search
        /// is much slower.</param>
        /// <returns>Cierto si es un token</returns>
        public bool ReferencesToken(FormElement control, bool fullSearch)
        {
            if (fullSearch)
            {
                // Ver si el control referencia directamente al token:
                if (CanContainReferences(control))
                    return ReferencesToken(control.KBObject, control, control);
            }
            else
            {
                // Check only the main property
                AttributeElement ae = control as AttributeElement;
                if (ae != null)
                    return Buscador.Token.EsToken(control.KBObject, ae.Attribute);
                GridColumnElement gc = control as GridColumnElement;
                if( gc != null)
                    return Buscador.Token.EsToken(control.KBObject, gc.Attribute);
            }
            return false;
        }

        static public bool CanContainReferences(FormElement control)
        {
            return control is AttributeElement || control is GridColumnElement ||
                control is GridElement;
        }

        // ************************************************************
        // SMART DEVICES
        // ************************************************************

        /// <summary>
        /// Check if a smart devices control references the token
        /// </summary>
        /// <param name="element">Control to check</param>
        /// <param name="fullSearch">If its true, all control properties will be analized.
        /// Otherwise, only the main property ("Attribute") will be checked. The full search
        /// is much slower.</param>
        /// <returns>True if the control references the token</returns>
        public bool ReferencesToken(PatternInstanceElement element, bool fullSearch)
        {
            if (fullSearch)
            {
                if (CanContainReferences(element))
                    return ReferencesToken(element.Instance.OwnerObject, element.Attributes, element);
            }
            else
            {
                // Check only the main property
                AttributeVariableReference att = null;

                if (element.Type == InstanceElements.LayoutDataItem)
                    att = element.Attributes.GetPropertyValue<AttributeVariableReference>(InstanceAttributes.LayoutDataItem.Attribute);

                if (att != null)
                    return Buscador.Token.EsToken(element.Instance.OwnerObject, att);
            }
            return false;
        }

        /// <summary>
        /// Return true if the element can contain references to attributes / variables
        /// </summary>
        /// <param name="element">Element to check</param>
        /// <returns>True if element can contain references</returns>
        static public bool CanContainReferences(PatternInstanceElement element)
        {
            // (Ev3 U3) FilterAttribute are ignored because they are crap. They reference variables that
            // MUST not be declared.
            return element.Type == InstanceElements.LayoutDataItem || 
                element.Type == InstanceElements.LayoutGrid || 
                element.Type == InstanceElements.GridData ||
                element.Type == InstanceElements.FilterSearchAttribute ||
                //element.Type == InstanceElements.FilterAttribute ||
                element.Type == InstanceElements.BreakByAttribute ||
                element.Type == InstanceElements.OrderAttribute ||
                element.Type == InstanceElements.LayoutComponent;
        }

    }
}
