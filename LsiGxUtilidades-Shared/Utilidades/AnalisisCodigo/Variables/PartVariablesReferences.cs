using System.Collections.Generic;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Parts;
using Artech.Patterns.WorkWithDevices.Parts;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Variables
{
    /// <summary>
    /// Part variables references analisys
    /// </summary>
    abstract class PartVariablesReferences
    {

        /// <summary>
        /// Variables referenced by the analyzed part
        /// </summary>
        public HashSet<string> ReferencedVariables = new HashSet<string>();

        /// <summary>
        /// Variables readed on the part
        /// </summary>
        public HashSet<string> ReadedVariables = new HashSet<string>();

        /// <summary>
        /// Variables written on the part
        /// </summary>
        public HashSet<string> WrittenVariables = new HashSet<string>();

        /// <summary>
        /// Non existent referenced variables
        /// </summary>
        public HashSet<string> NonExistentVariables = new HashSet<string>();

        /// <summary>
        /// The variables analyzer owner
        /// </summary>
        protected ObjectVariablesReferences References;

        public PartVariablesReferences(ObjectVariablesReferences references)
        {
            References = references;
        }

        /// <summary>
        /// Añade el nombre en minusculas de una variable a una lista, siempre que esta no sea una 
        /// estandar ni un array.
        /// Las estandar no se guardan porque no interesa revisarlas y las array porque
        /// genexus no guarda referencias a ellas.
        /// </summary>
        /// <param name="listaVariables">La lista en que guardar el nombre</param>
        /// <param name="variables">Lista de nombres de variables de las que guardar el nombre</param>
        protected void AddVariablesToList(HashSet<string> listaVariables, HashSet<string> variables)
        {
            foreach (string variable in variables)
                AddVariableToList(listaVariables, variable);
        }

        /// <summary>
        /// Añade el nombre en minusculas de una variable a una lista, siempre que esta no sea una 
        /// estandar ni un array.
        /// Las estandar no se guardan porque no interesa revisarlas y las array porque
        /// genexus no guarda referencias a ellas.
        /// </summary>
        /// <param name="variablesSet">La lista en que guardar el nombre</param>
        /// <param name="v">Nombre de la variable de la que guardar el nombre</param>
        protected void AddVariableToList(HashSet<string> variablesSet, string variableName)
        {
            if (variableName == null)
                return;

            Variable v = References.VariablesPart.GetVariable(variableName);
            if (v != null)
                AddVariableToList(variablesSet, v);
            else
            {
                NonExistentVariables.Add(TokenGx.NormalizeName(variableName, false));
                AddVariableInternal(variablesSet, variableName);
            }
        }

        /// <summary>
        /// Añade el nombre en minusculas de una variable a una lista, siempre que esta no sea una 
        /// estandar ni un array.
        /// Las estandar no se guardan porque no interesa revisarlas y las array porque
        /// genexus no guarda referencias a ellas.
        /// </summary>
        /// <param name="listaVariables">La lista en que guardar el nombre</param>
        /// <param name="v">Variable de la que guardar el nombre</param>
        protected void AddVariableToList(HashSet<string> listaVariables, Variable v)
        {
            if (v == null)
                return;

            AddVariableInternal(listaVariables, v.Name);
        }

        private void AddVariableInternal(HashSet<string> variablesSet, string variableName)
        {
            variableName = TokenGx.NormalizeName(variableName, false);
            if (References.ToIgnore.Contains(variableName))
                return;

            variablesSet.Add(variableName);
        }

        static public PartVariablesReferences GetReferences(ObjectVariablesReferences owner, KBObjectPart part)
        {
            if (part is RulesPart)
                return new RulesVariablesReferences((RulesPart)part, owner);
            else if (part is WinFormPart || part is WebFormPart || part is VirtualLayoutPart)
                return new FormVariablesReferences(part, owner);
            else if (part.LsiIsMainSource())
                return new MainCodeVariablesReferences(part, owner);
            else if (part is ConditionsPart)
                return new ConditionsVariablesReferences((ConditionsPart)part, owner);
            else if(part is LayoutPart)
                return new ReportLayoutVariablesReferences((LayoutPart)part, owner);
            else
                return null;
        }

    }
}
