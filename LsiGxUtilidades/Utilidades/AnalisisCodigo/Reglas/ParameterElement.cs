using System;
using System.Collections.Generic;
using System.Text;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Variables;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas
{
    /// <summary>
    /// En LSI ponermos documentacion en los parametros.
    /// Un elemento de la regla puede ser: o documentacion o un parametro.
    /// Esta clase contiene uno de los dos elementos.
    /// Por ejemplo, un parametro regla podria ser:
    /// parm(<br/>
    ///     &variable1,   // Documentacion variable1
    ///     &variable2    // Documentacion variable 2
    /// );
    /// En este caso, los elemento seria { [variable1] , [documentacion variable 1], [variable2] , [documentacion variable 2] }
    /// </summary>
    public class ParameterElement
    {

        /// <summary>
        /// Si no es nulo, el elemento contiene la documentacion indicada.
        /// </summary>
        public string Documentation;

        /// <summary>
        /// Only applied if Documentation is not null. If this is true, a line break will
        /// be added before the documentation
        /// </summary>
        public bool LineBreakBefore;

        /// <summary>
        /// True if the the parameter object is an attribute
        /// </summary>
        public bool IsAttribute;

        /// <summary>
        /// Parameter name. If the parameter is a variable, it does NOT contain the ampersand. With original case
        /// </summary>
        public string Name;

        /// <summary>
        /// Parameter data type
        /// </summary>
        public DataTypeInfo DataType = DataTypeInfo.NoType;

        /// <summary>
        /// Parameter type descripcion
        /// </summary>
        public string TypeDescription;

        /// <summary>
        /// The access type (in, out, inout)
        /// </summary>
        public RuleDefinition.ParameterAccess Accessor;

        /// <summary>
        /// Devuelve cierto si el elemento es de documentacion
        /// </summary>
        public bool EsDocumentacion
        {
            get { return !string.IsNullOrEmpty(Documentation); }
        }

        /// <summary>
        /// Devuelve cierto si el elemento es un parametro
        /// </summary>
        public bool EsParametro
        {
            get { return string.IsNullOrEmpty(Documentation); }
        }

        /// <summary>
        /// Si el parametro es documentacion devuelve null. Si no, devuelve el nombre
        /// del atributo / variable del parametro. Si es variable, lo devuelve con el ampersand
        /// inicial.
        /// </summary>
        public string NombreAtributoVariable
        {
            get
            {
                if (Name == null)
                    return null;
                string txtParameter = Name;
                if (!IsAttribute)
                    txtParameter = "&" + txtParameter;
                return txtParameter;
            }
        }

        /// <summary>
        /// Devuelve cierto si el parametro es out: o inout:
        /// </summary>
        public bool SeEscribe
        {
            get
            {
                return Accessor == RuleDefinition.ParameterAccess.PARM_OUT ||
                       Accessor == RuleDefinition.ParameterAccess.PARM_INOUT;
            }

        }

        /// <summary>
        /// Devuelve cierto si el parametro es in: o inout:
        /// </summary>
        public bool SeLee
        {
            get
            {
                return Accessor == RuleDefinition.ParameterAccess.PARM_IN ||
                       Accessor == RuleDefinition.ParameterAccess.PARM_INOUT;
            }

        }

        /// <summary>
        /// Parameter access type description
        /// </summary>
        public string ParameterAccessText
        {
            get
            {
                switch (Accessor)
                {
                    case RuleDefinition.ParameterAccess.PARM_IN:
                        return "in:";

                    case RuleDefinition.ParameterAccess.PARM_OUT:
                        return "out:";

                    default:
                        return "inout:";
                }
            }
        }

        /// <summary>
        /// Devuelve el texto del elemento del parametro
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Name != null)
            {
                // Es un parametro
                string txtParameter = ParameterAccessText;
                txtParameter += NombreAtributoVariable;
                return txtParameter;
            }

            if (Documentation != null)
            {
                string doc = "\t// " + Documentation + Environment.NewLine;
                if (LineBreakBefore)
                    doc = Environment.NewLine + "\t" + doc;
                return doc;
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the variable / attribute associated to the parameter
        /// </summary>
        /// <param name="o">The object owner of this parameter</param>
        /// <returns>The object. null if it was not found</returns>
        public ITypedObject GetParameterObject(KBObject o)
        {
            if (Name == null)
                return null;

            if (IsAttribute)
                return Artech.Genexus.Common.Objects.Attribute.Get(o.Model, Name);
            else
            {
                VariablesPart variables = o.Parts.LsiGet<VariablesPart>();
                if (variables == null)
                    return null;
                return variables.GetVariable(Name);
            }
        }

        /// <summary>
        /// Construye un parametro
        /// </summary>
        public ParameterElement(Parameter parameter)
        {
            Accessor = parameter.Accessor;
            IsAttribute = parameter.IsAttribute;
            Name = parameter.Name;
            TypeDescription = string.Empty;
            if (parameter.Object != null)
            {
                DataType = new DataTypeInfo(parameter.Object);
                if (IsAttribute) {
                    Artech.Genexus.Common.Objects.Attribute att = parameter.Object as Artech.Genexus.Common.Objects.Attribute;
                    if (att != null)
                        TypeDescription = AtributoGx.TypeDescription(att.Model, att);
                    else
                        TypeDescription = parameter.Name;
                }
                else
                {
                    Variable v = parameter.Object as Variable;
                    if (v != null)
                        TypeDescription = VariableGX.DescripcionTipo(v.Model, v);
                }
            }
        }

        /// <summary>
        /// Construye un elemento de documentacion
        /// </summary>
        public ParameterElement(string documentacion)
        {
            Documentation = documentacion;
        }

    }
}
