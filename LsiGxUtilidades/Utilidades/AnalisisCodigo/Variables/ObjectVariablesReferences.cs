using System;
using System.Collections.Generic;
using System.Linq;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Variables;
using Artech.Genexus.Common.Parts.Variables;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Variables
{

    /// <summary>
    /// Guarda y clasifica las referencias de un objeto a sus variables.
    /// Permite buscar cosas sospechosas como variables no utilizadas, variables que se utilizan
    /// solo en la UI, etc.
    /// </summary>
    public class ObjectVariablesReferences
    {

        /// <summary>
        /// El buscador de variables
        /// </summary>
        internal TokensFinder BuscadorVariables = new TokensFinder();

        /// <summary>
        /// Declared variables on object, lowercase
        /// </summary>
        private HashSet<string> DeclaredVariablesLowerCase = new HashSet<string>();

        /// <summary>
        /// Declared variables on object, with the original case
        /// </summary>
        private HashSet<string> DeclaredVariablesOriginalCase = new HashSet<string>();

        /// <summary>
        /// La lista de variables declaradas autodefinidas
        /// </summary>
        private HashSet<string> VariablesAutodefinidas = new HashSet<string>();

        /// <summary>
        /// La lista de variables con tipo N(4), el tipo por defecto
        /// </summary>
        private HashSet<string> VariablesN4 = new HashSet<string>();

        /// <summary>
        /// La lista de variables estandar y arrays declaradas en el objeto
        /// </summary>
        internal HashSet<string> ToIgnore = new HashSet<string>();

        /// <summary>
        /// El objeto del que se estan analizando las variables 
        /// </summary>
        public KBObject Object;

        /// <summary>
        /// True if we should analyze the reading / writting of variables
        /// </summary>
        internal bool CheckReadWrites;

        /// <summary>
        /// Los parametros del objeto que se esta analizando
        /// </summary>
        private List<ParameterElement> ParametrosObjeto = new List<ParameterElement>();

        /// <summary>
        /// Object to check variables
        /// </summary>
        internal VariablesPart VariablesPart;

        /// <summary>
        /// Variables references of each part
        /// </summary>
        private List<PartVariablesReferences> PartsReferences = new List<PartVariablesReferences>();

        /// <summary>
        /// Prefixes for variables names alway null, trimmed. They are case sensitive
        /// </summary>
        private IEnumerable<string> AlwaysNullVarsPrefix;

        /// <summary>
        /// Constructor.
        /// It checks the object variables
        /// </summary>
        /// <param name="o">Object from which check variables</param>
        /// <param name="checkReadWrites">True if variables reads and writes on the object code 
        /// should be checked. If its true, the process will take much longer</param>
        /// <param name="alwaysNullVarsPrefix">Prefixes for variables names alway null, 
        /// trimmed. They are case sensitive</param>
        /// <param name="variableNamesToIgnoreLowercase">List of variable names to ignore, lowercase, no ampersand. If null, it's ignored</param>
        public ObjectVariablesReferences(KBObject o, bool checkReadWrites,
            IEnumerable<string> alwaysNullVarsPrefix, IEnumerable<string> variableNamesToIgnoreLowercase)
        {

            Object = o;
            CheckReadWrites = checkReadWrites;
            AlwaysNullVarsPrefix = alwaysNullVarsPrefix;

            // Obtener y recorrer la lista de variables declaradas:
            VariablesPart = o.Parts.LsiGet<VariablesPart>();
            if (VariablesPart == null)
                throw new Exception($"Object {o.Name} has no variables");

            // Revisar las variables declaradas:
            foreach (Variable v in VariablesPart.Variables)
            {
                if (v == null || v.Name == null)
                    // This has happened, no idea why...
                    continue;

                string name = v.Name.ToLower();

                if (v.IsStandard || !VariableGX.EsEscalar(v) || 
                    v.Description.ToLower().Contains("[nocheck]") ||
                    (variableNamesToIgnoreLowercase != null && variableNamesToIgnoreLowercase.Contains(name))
                )
                {
                    // Do not analyze standard variables and arrays. 
                    // Arrays are not reported by IHasVariableReferences
                    // [NOCHECK] on description means force to ignore
                    ToIgnore.Add(name);
                    continue;
                }

                DeclaredVariablesLowerCase.Add(name);
                DeclaredVariablesOriginalCase.Add(v.Name);

                if(
                    v.Type == eDBType.NUMERIC && v.Length == 4 && 
                    v.Decimals == 0 &&
                    //v.Name.Length > 1 && < &i , &j.. variables could be wrong too
                    !v.Description.ToLower().Contains("n4") &&
                    v.AttributeBasedOn == null && v.DomainBasedOn == null && 
                    v.Name.ToLower() != "generate" )
                    // Variable n(4) no marcada como correcta, ni tiene un nombre del &i, &j, etc ni es 
                    // la &generate:
                    VariablesN4.Add(name);

                if (v.IsAutoDefined)
                    // Variable autodefinida
                    VariablesAutodefinidas.Add(name);

            }

            // Ignorar tambien &Err, que no es reportada como estandar, pero lo es
            ToIgnore.Add("err");

            // Ev3U3: There is a bug with DataSelector and standard variables: They are not declared
            // on the variables part. So, they are not declared as "to ignore". So, they are reported as read only
            if (Object.Type == ObjClass.DataSelector)
            {
                // GetStandardVariablesNames() seems not suport ObjClass.DataSelector, that's why ObjClass.Procedure
                foreach (string stdVariableName in VariableDefinition.GetStandardVariablesNames(ObjClass.Procedure))
                    ToIgnore.Add(stdVariableName.ToLower());
            }

            foreach (KBObjectPart part in Object.Parts.LsiEnumerate())
            {
                PartVariablesReferences refs = PartVariablesReferences.GetReferences(this, part);
                if( refs != null )
                    PartsReferences.Add(refs);
            }

            // Be sure there is all kind of references parts:
            if( GetPartReferences<RulesVariablesReferences>() == null )
                PartsReferences.Add(new RulesVariablesReferences(null, this));
            if (GetPartReferences<MainCodeVariablesReferences>() == null)
                PartsReferences.Add(new MainCodeVariablesReferences(null, this));
        }

        /// <summary>
        /// Devuelve cierto si la variables es de tipo extendido o es un external object
        /// </summary>
        private bool EsTipoExtendido(string nombreVariable)
        {
            Variable v = VariablesPart.GetVariable(nombreVariable);
            if (v == null)
                return false;

            if (v.Type == eDBType.GX_USRDEFTYP || v.Type == eDBType.GX_EXTERNAL_OBJECT)
                return true;

            return false;
        }

        /// <summary>
        /// Get referenced variables
        /// </summary>
        /// <returns>Referenced variables names</returns>
        public HashSet<string> GetReferencedVariables()
        {
            return PartsReferences.SelectMany(x => x.ReferencedVariables).LsiToHashSet();
        }

        /// <summary>
        /// Get non existing referenced variables
        /// </summary>
        /// <returns></returns>
        public HashSet<string> GetNonExistingReferenced()
        {
            return PartsReferences.SelectMany(x => x.NonExistentVariables).LsiToHashSet();
        }

        private HashSet<string> GetReferencedVariables(Type[] partTypes, bool exclude)
        {
            return PartsReferences
                .Where(x => exclude ? !partTypes.Contains(x.GetType()) : partTypes.Contains(x.GetType()))
                .SelectMany(x => x.ReferencedVariables)
                .LsiToHashSet();
        }

        private HashSet<string> GetReferencedVariables(Type[] partTypes)
        {
            return GetReferencedVariables(partTypes, false);
        }

        private HashSet<string> GetReferencedVariablesOutside(Type[] partTypes)
        {
            return GetReferencedVariables(partTypes, true);
        }

        private HashSet<string> GetWrittenVariables()
        {
            return PartsReferences.SelectMany(x => x.WrittenVariables).LsiToHashSet();
        }

        private HashSet<string> GetReadedVariables()
        {
            return PartsReferences.SelectMany(x => x.ReadedVariables).LsiToHashSet();
        }

        private T GetPartReferences<T>() where T : PartVariablesReferences
        {
            return PartsReferences
                .OfType<T>()
                .FirstOrDefault();
        }

        /// <summary>
        /// Get the names of variables declared and unused
        /// </summary>
        public List<string> GetUnusedVariables()
        {
            List<string> noUsadas = new List<string>();
            HashSet<string> referenced = GetReferencedVariables();
            foreach (string varDeclarada in DeclaredVariablesLowerCase)
            {
                if (!referenced.Contains(varDeclarada))
                    noUsadas.Add(varDeclarada);
            }
            noUsadas.Sort();
            return noUsadas;
        }

        /// <summary>
        /// Devuelve la lista de nombres de variables autodefinidas
        /// </summary>
        public List<string> ObtenerAutodefinidas()
        {
            List<string> autodefinidas = VariablesAutodefinidas.ToList();
            autodefinidas.Sort();
            return autodefinidas;
        }

        /// <summary>
        /// Devuelve la lista de variables referenciadas que no existe
        /// </summary>
        public List<string> ObtenerNoExistentes()
        {
            List<string> noExistentes = GetNonExistingReferenced().ToList();
            noExistentes.Sort();
            return noExistentes;
        }

        /// <summary>
        /// Devuelve las variables de tipo N(4)
        /// </summary>
        public List<string> GetN4Variables()
        {
            List<string> unused = GetUnusedVariables();
            // If it's unused, do not report as N(4)
            List<string> n4 = VariablesN4.ToList().Except( unused ).ToList();
            n4.Sort();
            return n4;
        }

        private List<string> GetReferencedOnlyOnPart(Type partType)
        {
            List<string> onlyInPart = new List<string>();
            // Revisar variables usadas en la UI
            Type[] partTypes = new Type[] { partType };
            HashSet<string> partReferences = GetReferencedVariables(partTypes);
            HashSet<string> nonPartReferences = GetReferencedVariablesOutside(partTypes);

            return partReferences
                .Where(x => !nonPartReferences.Contains(x))
                .OrderBy(x => x)
                .ToList();
        }

        /// <summary>
        /// Devuelve la lista de variables usadas en la UI y no usadas en ningun otro sitio
        /// </summary>
        public List<string> OnlyReferencedOnForm()
        {
            FormVariablesReferences formReferences = GetPartReferences<FormVariablesReferences>();
            if (formReferences == null)
                return new List<string>();

            return formReferences.VariablesOnlyOnForm(GetReferencedOnlyOnPart(typeof(FormVariablesReferences)));
        }

        public List<string> OnlyReferencedOnConditions()
        {
            return GetReferencedOnlyOnPart(typeof(ConditionsVariablesReferences));
        }

        /// <summary>
        /// Devuelve la lista de variables declaradas de salida en los parametros pero que no
        /// se asignan
        /// </summary>
        public List<string> ObtenerDeSalidaNoAsignadas()
        {
            // Revisar las variables de salida en los parametros:
            List<string> noEscritas = new List<string>();
            RulesVariablesReferences rulesRef = GetPartReferences<RulesVariablesReferences>();
            HashSet<string> written = GetWrittenVariables();

            foreach (string varSalida in rulesRef.VariablesSalidaParametros)
            {
                if (!written.Contains(varSalida))
                    noEscritas.Add(varSalida);
            }
            noEscritas.Sort();
            return noEscritas;
        }

        /// <summary>
        /// It returns a list of in: written variable parameters
        /// </summary>
        /// <returns>Written in: parameters</returns>
        public List<string> GetWrittenInParameters()
        {
            List<string> inWritten = new List<string>();
            RulesVariablesReferences rulesRef = GetPartReferences<RulesVariablesReferences>();
            MainCodeVariablesReferences codeRef = GetPartReferences<MainCodeVariablesReferences>();

            foreach (string inVariable in rulesRef.VariablesEntradaParametros)
            {
                bool sureIsWritten;
                if (codeRef.CallsWrittenVariablesOut.Contains(inVariable))
                    // Written on a out: parameter of an object call
                    sureIsWritten = true;
                else if (codeRef.AssignmentWrittenVariables.Contains(inVariable))
                    sureIsWritten = true;
                else
                    sureIsWritten = false;

                if( sureIsWritten )
                    inWritten.Add(inVariable);
            }
            inWritten.Sort();
            return inWritten;
        }

        /// <summary>
        /// Devuelve la lista de variables solo escritas en el objeto, pero ni leidas ni devueltas
        /// </summary>
        /// <returns></returns>
        public List<string> ObtenerSoloEscritas()
        {
            List<string> soloEscritas = new List<string>();
            HashSet<string> readedVariables = GetReadedVariables();
            MainCodeVariablesReferences mainRefs = GetPartReferences<MainCodeVariablesReferences>();
            RulesVariablesReferences rulesRefs = GetPartReferences<RulesVariablesReferences>();

            foreach (string varEscrita in GetWrittenVariables())
            {
                // Si la variable no se lee en ningun sitio ni se devuelve, es un error
                if (!readedVariables.Contains(varEscrita) &&
                    !rulesRefs.VariablesSalidaParametros.Contains(varEscrita) &&
                    !rulesRefs.VariablesEntradaSalidaParametros.Contains(varEscrita))
                {
                    // Una excepcion es cuando la variable se escribe en una llamada
                    // En este caso, hay que pasarla como parametro si o si, aunque no se use
                    if (mainRefs.VariablesEscritasLlamadas.Contains(varEscrita))
                        continue;

                    // Si la variable es external object/extended type, no dar avisos: Falla,p.ej., 
                    // con HttpResponse
                    if (EsTipoExtendido(varEscrita))
                        continue;

                    soloEscritas.Add(varEscrita);
                }
            }
            soloEscritas.Sort();
            return soloEscritas;
        }

        /// <summary>
        /// Get the variables that are always null (with prefix "z", etc)
        /// </summary>
        /// <returns>The variables always null names, lowercase</returns>
        private HashSet<string> AlwaysNullVariables()
        {
            return DeclaredVariablesOriginalCase
                .Where(varName =>
                    AlwaysNullVarsPrefix.Any(prefix => varName.StartsWith(prefix))
                )
                .Select( varName => varName.ToLower() )
                .LsiToHashSet();
        }

        /// <summary>
        /// Devuelve la lista de variables solo leidas en el objeto, pero ni recibidas como
        /// parametro ni escritas
        /// </summary>
        /// <param name="reportVarInitialValue">True if read only variables with some value
        /// on its Initial value property should be returned</param>
        /// <returns>The list of read only variables names</returns>
        public List<string> ObtenerSoloLeidas(bool reportVarInitialValue)
        {
            List<string> readOnly = new List<string>();
            HashSet<string> writtenVariables = GetWrittenVariables();
            RulesVariablesReferences rulesRefs = GetPartReferences<RulesVariablesReferences>();

            // Variable names always null
            HashSet<string> alwaysNullVarNames = AlwaysNullVariables();

            foreach (string readVariable in GetReadedVariables())
            {
                // Ignore always null variables
                if (alwaysNullVarNames.Contains(readVariable) )
                    continue;

                if (!writtenVariables.Contains(readVariable) &&
                    !rulesRefs.VariablesEntradaParametros.Contains(readVariable) &&
                    !rulesRefs.VariablesEntradaSalidaParametros.Contains(readVariable))
                {
                    // Si la variable es external object/extended type, no dar avisos: Falla,p.ej., 
                    // con HttpRequest
                    if (EsTipoExtendido(readVariable))
                        continue;

                    if (!reportVarInitialValue)
                    {
                        // If the variable has initial value, don't report it
                        Variable v = VariablesPart.GetVariable(readVariable);
                        if (v != null && v.GetPropertyValue(Properties.ATT.InitialValue) != null)
                            continue;
                    }

                    readOnly.Add(readVariable);
                }
            }
            readOnly.Sort();
            return readOnly;
        }

        /// <summary>
        /// Get variables always null (variables with name with a prefix "z", etc) that have its
        /// "Initial value" property set
        /// </summary>
        /// <returns>List of variable names</returns>
        public List<string> AlwaysNullWithInitialValue()
        {
            List<string> result = new List<string>();

            // Variable names always null
            foreach (string variable in AlwaysNullVariables())
            {
                Variable v = VariablesPart.GetVariable(variable);
                if (v == null)
                    continue;
                
                // initialValue seems to be a Artech.Genexus.Common.Objects.Formula
                object initialValue = v.GetPropertyValue(Properties.ATT.InitialValue);
                if (initialValue == null)
                    continue;

                // Check if the initial value is the default value for the variable
                // Ex: If the variable is number, and the initial value is zero, do not warn about it
                // This does not cover all cases, but it cover our cases
                string txtInitialValue = initialValue.ToString().Trim();
                if( v.LsiIsNumeric() && ( txtInitialValue == "0" || txtInitialValue == "0.0" ) )
                    continue;
                if( v.LsiIsString() && ( txtInitialValue == "''" || txtInitialValue == "\"\"" ) )
                    continue;

                result.Add(variable);                
            }

            result.Sort();
            return result;
        }

        /// <summary>
        /// Devuelve la lista de variables que se pasan como parametro, pero que no se utilizan
        /// en ningun otro sitio
        /// </summary>
        public List<string> ObtenerSoloUsadasParm()
        {
            List<string> soloParm = new List<string>();
            RulesVariablesReferences rulesRefs = GetPartReferences<RulesVariablesReferences>();
            HashSet<string> refsOutsideRules = 
                GetReferencedVariablesOutside(new Type[] { typeof(RulesVariablesReferences ) } );

            // Revisar las variables usadas en la regla parm:
            foreach (string varParm in rulesRefs.ReferencedVariables)
            {
                if (!refsOutsideRules.Contains(varParm) &&
                    !rulesRefs.VariablesReferenciadasRulesNoParm.Contains(varParm))
                    soloParm.Add(varParm);
            }
            soloParm.Sort();
            return soloParm;
        }

        /// <summary>
        /// Devuelve una lista de mensajes de error por llamadas incorrectas. Solo tendra valores
        /// si se han analizado las lecturas/escrituras
        /// </summary>
        /// <returns></returns>
        public List<string> ObtenerErroresEnLlamadas()
        {
            return GetPartReferences<MainCodeVariablesReferences>().ErroresEnLlamadas;
        }

    }
}
