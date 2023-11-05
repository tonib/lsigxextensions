using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Objects;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using Artech.Genexus.Common;
using LSI.Packages.Extensiones.Utilidades.CodeGeneration;
using static Artech.Genexus.Common.CustomTypes.RuleDefinition;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{
    // TODO: Move functions for DataSelectors refactoring here

    /// <summary>
    /// DataSelector class extensions
    /// </summary>
    static public class DataSelectorExtensions
    {

        /// <summary>
        /// Get a standard part from the DataSelector
        /// </summary>
        /// <param name="ds">The data selector</param>
        /// <param name="partType">The part type to get</param>
        /// <returns>The object part. null if the part cannot be converted</returns>
        public static KBObjectPart LsiGetPart(this DataSelector ds, Type partType)
        {
            if (partType == typeof(RulesPart))
            {
                RulesPart rules = new RulesPart(ds);
                IEnumerable<DataSelectorParameter> parameters = ds.DataSelectorStructure.Parameters;

                ReglaParm parm = new ReglaParm();
                foreach (DataSelectorParameter p in parameters)
                {
                    Variable v = p.Content as Variable;
                    if (v == null)
                        continue;

                    parm.AgregarParametro(v);
                    if (!string.IsNullOrEmpty(p.Description))
                        parm.AddDocumentation(p.Description);
                }
                rules.Source = parm.ToString();
                return rules;
            }

            if (partType == typeof(ProcedurePart))
            {
                ProcedurePart procedure = new ProcedurePart(ds);
                ForEachGenerator forEach = new ForEachGenerator(ds);
                procedure.Source = forEach.ToString();
                return procedure;
            }

            if (partType == typeof(VariablesPart))
                return ds.DataSelectorStructure.Variables;

            return null;
        }

        static public IEnumerable<KBObjectPart> LsiEnumerateParts(this DataSelector ds)
        {
            yield return ds.LsiGetPart(typeof(RulesPart));
            yield return ds.LsiGetPart(typeof(ProcedurePart));
            yield return ds.LsiGetPart(typeof(VariablesPart)); 
        }

        static public void LsiUpdatePart(this DataSelector ds, KBObjectPart part)
        {
            // Mark the data selector as dirty:
            ds.DataSelectorStructure.Dirty = true;
        }

        /// <summary>
        /// Get the right parameters for a DataSelector
        /// </summary>
        /// <param name="ds">The data selector</param>
        /// <returns>The right parameters</returns>
        /// <remarks>
        /// EvU3: There is a bug with GetSignatures() for DataSelectors: Returned signature don't report
        /// the parameter object (att/var) ¯\_(ツ)_/¯
        /// </remarks>
        static public List<Parameter> LsiGetParameters(this DataSelector ds)
        {
            List<Parameter> parameters = new List<Parameter>();
            foreach (DataSelectorParameter dsParm in ds.DataSelectorStructure.Parameters)
            {
                ITypedObject typedObject = dsParm.Content as ITypedObject;
                if (typedObject == null)
                    continue;
                bool isAtt = typedObject is Artech.Genexus.Common.Objects.Attribute;
                parameters.Add(new Parameter(typedObject, isAtt, ParameterAccess.PARM_IN));
            }
            return parameters;
        }
    }
}
