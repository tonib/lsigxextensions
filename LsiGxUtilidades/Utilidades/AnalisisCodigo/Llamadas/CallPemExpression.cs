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

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Llamadas
{
    /// <summary>
    /// Tool to parse a PEM call expression
    /// </summary>
    class CallPemExpression
    {

        /// <summary>
        /// The module name used on the call, case sensitive. Empty string if there is no module name
        /// </summary>
        public string ModuleName = string.Empty;

        /// <summary>
        /// The object name, unqualified
        /// </summary>
        public ObjectBase ObjectName;

        /// <summary>
        /// Extra part on call. It can be the "udp" part on "object.upd()", or the section
        /// call on a wwdevices call ("list.call()" on "wwobject.list.call()").
        /// Empty list if there is no extras
        /// </summary>
        public List<ObjectBase> Extras = new List<ObjectBase>();

        /// <summary>
        /// The function part of the call. null if the PEM expression is not a call
        /// </summary>
        public Function CallFunction;

        /// <summary>
        /// The function name part of the call. null if the PEM expression is not a call
        /// </summary>
        public RuleName CallFunctionName;

        /// <summary>
        /// The qualified object name found on the call
        /// </summary>
        public string QualifiedObjectName;

        /// <summary>
        /// True if the expression is an object call
        /// </summary>
        public bool IsCallExpression
        {
            get { return CallFunction != null && CallFunctionName != null; }
        }

        private void ExtractModuleName(List<ObjectBase> expressionComponents, 
            IEnumerable<string> modulesNames)
        {
            string currentModule = null;
            // Don't extract the object name
            while (expressionComponents.Count > 1)
            {
                if (currentModule == null)
                    currentModule = expressionComponents[0].ToString();
                else
                    currentModule += "." + expressionComponents[0].ToString();

                if (!modulesNames.Contains(currentModule.ToLower()))
                    return;
                else
                {
                    // Belongs to the module name
                    ModuleName = currentModule;
                    expressionComponents.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expression">Expression to parse</param>
        /// <param name="modulesNames">Kb modules names, lowercase</param>
        public CallPemExpression(ObjectPEM expression, IEnumerable<string> modulesNames)
        {
            List<ObjectBase> expressionComponents = ObjectPEMGx.ConvertirALista(expression);

            // Check last component, to find call information:
            ObjectBase lastComponent = expressionComponents[expressionComponents.Count - 1];
            CallFunction = lastComponent as Function;
            if (CallFunction != null)
                CallFunctionName = CallFunction.Name as RuleName;

            // Extract the module part
            ExtractModuleName(expressionComponents, modulesNames);
            if (expressionComponents.Count == 0)
                return;

            // The object name
            ObjectName = expressionComponents[0];
            expressionComponents.RemoveAt(0);

            // Get only the object name
            if (ObjectName is Function)
                ObjectName = ((Function)ObjectName).Name;

            // Remaining are the funcions / ww sections
            Extras = expressionComponents;

            // The qualified object name found:
            if (!string.IsNullOrEmpty(ModuleName))
                QualifiedObjectName = ModuleName + "." + ObjectName.ToString();
            else
                QualifiedObjectName = ObjectName.ToString();
        }

    }
}
