using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.UI;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Common.Properties;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
    /// <summary>
    /// Tool to search object with variables that mach some pattern
    /// </summary>
    public class VariablesFinder : UISearchBase
    {

        /// <summary>
        /// Variable type name to search, lowercase. Ex. "varchar"
        /// </summary>
        private string VariableTypeName;

        /// <summary>
        /// Variable type name to search, lowercase, with parenthesis. Ex. "varchar("
        /// </summary>
        private string VariableTypeNameWithLength;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="variableTypeName">Type name of variables to search</param>
        public VariablesFinder(string variableTypeName)
        {
            VariableTypeName = variableTypeName.ToLower().Trim();
            VariableTypeNameWithLength = VariableTypeName + "(";
        }

        private void CheckObjectVariables(KBObject o)
        {

            // Fast test:
            if (!(o is ICallableInfo))
                return;

            VariablesPart variables = o.Parts.LsiGet<VariablesPart>();
            if (variables == null)
                return;

            foreach (Variable v in variables.Variables)
            {
                string vTypeName = v.GetPropertyValue(Properties.ATT.DataTypeString) as string;
                if (vTypeName == null)
                    continue;
                vTypeName = vTypeName.ToLower();

                if (vTypeName == VariableTypeName || vTypeName.StartsWith(VariableTypeNameWithLength))
                {
                    PublishUIResult(new RefObjetoGX(o));
                    return;
                }
            }
        }

        /// <summary>
        /// Ejecuta la busqueda desde la interface de usuario
        /// </summary>
        override public void ExecuteUISearch()
        {
            this.MessagesMultiple = 10;

            foreach (KBObject o in UIServices.KB.CurrentModel.Objects.GetAll())
            {
                if (SearchCanceled)
                    return;

                CheckObjectVariables(o);
                IncreaseSearchedObjects();
            }
        }

    }
}
