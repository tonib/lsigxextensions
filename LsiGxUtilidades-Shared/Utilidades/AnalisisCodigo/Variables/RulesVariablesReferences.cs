using Artech.Architecture.Language.Parser.Data;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using System.Collections.Generic;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Variables
{
	/// <summary>
	/// Rules variables references analisys
	/// </summary>
	class RulesVariablesReferences : PartVariablesReferences
    {

        /// <summary>
        /// La lista de variables referenciadas en los parametros (regla parm)
        /// </summary>
        private HashSet<string> VariablesReferenciadasParametros = new HashSet<string>();

        /// <summary>
        /// La lista de variables de salida en los parametros
        /// </summary>
        public HashSet<string> VariablesSalidaParametros = new HashSet<string>();

        /// <summary>
        /// La lista de variables de entrada en los parametros
        /// </summary>
        public HashSet<string> VariablesEntradaParametros = new HashSet<string>();

        /// <summary>
        /// La lista de variables de entrada/salida en los parametros
        /// </summary>
        public HashSet<string> VariablesEntradaSalidaParametros = new HashSet<string>();

        /// <summary>
        /// La lista de variables referenciadas en los parametros, fuera de la regla parm
        /// </summary>
        public HashSet<string> VariablesReferenciadasRulesNoParm = new HashSet<string>();

        /// <summary>
        /// Analyze rules part
        /// </summary>
        /// <param name="rules">Rules to analyze. It can be null</param>
        public RulesVariablesReferences(RulesPart rules, ObjectVariablesReferences references) :
            base(references)
        {
            if (rules == null)
                return;

            ParsedCode rulesCode = new ParsedCode(rules);

            // Get referenced variables:
            AddVariablesToList(ReferencedVariables, 
                References.BuscadorVariables.SearchAllNames(rulesCode, false) );

            // Analizar la regla parm
            AnalizarReglaParm();

            // Ver que variables de las reglas son referenciadas fuera de la regla parm
            AnalizarReglasNoParm(rulesCode);

            if (References.CheckReadWrites)
                CheckReadWrites(rulesCode);
        }

        /// <summary>
        /// Analiza la regla parm del objeto, si tiene. Clasifica las variables pasadas como
        /// parametro
        /// </summary>
        private void AnalizarReglaParm()
        {
            ICallableInfo callable = References.Object as ICallableInfo;
            if (callable == null)
                return;

            // El objeto es llamable: Puede tener regla parm. Analizarla
            List<ParameterElement> parameters = ReglaParm.ObtenerParametros(callable);

            // Guardar las variables referenciadas en los parametros
            foreach (ParameterElement parm in parameters)
            {
                if( !parm.IsAttribute )
                {
                    AddVariableToList(VariablesReferenciadasParametros, parm.Name);

                    // Ver si la variable es solo de entrada/salida en los parametros
                    if (parm.Accessor == RuleDefinition.ParameterAccess.PARM_OUT)
                        AddVariableToList(VariablesSalidaParametros, parm.Name);
                    else if (parm.Accessor == RuleDefinition.ParameterAccess.PARM_IN)
                        AddVariableToList(VariablesEntradaParametros, parm.Name);
                    else
                        AddVariableToList(VariablesEntradaSalidaParametros, parm.Name);
                }
            }
        }


        /// <summary>
        /// Revisa que variables referenciadas en las reglas son utilizadas fuera de la regla parm
        /// </summary>
        /// <param name="codigoRules">Codigo de las reglas</param>
        private void AnalizarReglasNoParm(ParsedCode codigoRules)
        {
            // Obtener la regla parm en el codigo:
            BuscadorReglas buscadorParm = new BuscadorReglas(BuscadorReglas.REGLAPARM);
            Rule reglaParm = buscadorParm.BuscarPrimera(codigoRules);
            if (reglaParm == null)
            {
                // No hay regla parm: Todas las variables de las rules estan fuera:
                VariablesReferenciadasRulesNoParm = ReferencedVariables;
                return;
            }

            // Obtener la lista de variables refernciadas en la declaracion del parm:
            HashSet<ObjectBase> variablesEnParm =
                new HashSet<ObjectBase>(References.BuscadorVariables.BuscarTodas(References.Object, reglaParm));

            // Buscar la posicion de las variables en las reglas, para ver si se referencian fuera
            // de la regla parm
            List<ObjectBase> variablesReglas = References.BuscadorVariables.BuscarTodas(codigoRules);
            foreach (ObjectBase ocurrenciaVariable in variablesReglas)
            {
                if (!variablesEnParm.Contains(ocurrenciaVariable))
                {
                    // Referencia fuera de la regla parm:
                    string nombreVariable = TokenGx.ObtenerNombre(ocurrenciaVariable, false);
                    AddVariableToList(VariablesReferenciadasRulesNoParm, nombreVariable);
                }
            }
        }

        private void CheckReadWrites(ParsedCode rulesCode)
        {
            // Default rules write variables:
            BuscadorReglas buscador = new BuscadorReglas(BuscadorReglas.REGLADEFAULT);
            foreach (Rule reglaDefault in buscador.BuscarTodas(rulesCode))
            {
                VariableName nombreVariable = ReglaDefault.ObtenerElementoAsignado(reglaDefault)
                    as VariableName;
                if (nombreVariable == null)
                    continue;

                WrittenVariables.Add(TokenGx.NormalizeName(nombreVariable, false));
            }

            // Las variables referenciadas en las rules, excepto en la regla parm, son leidas
            // En las transacciones son leidas y escritas. Falso, pero es una aproximacion
            bool isTransaction = References.Object is Transaction;
            foreach (string variable in VariablesReferenciadasRulesNoParm)
            {
                ReadedVariables.Add(variable);
                if (isTransaction)
                    WrittenVariables.Add(variable);
            }
        }

    }
}
