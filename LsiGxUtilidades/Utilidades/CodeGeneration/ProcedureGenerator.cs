using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Types;
using Artech.Genexus.Common.CustomTypes;
using Artech.Architecture.Common.Objects;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;

namespace LSI.Packages.Extensiones.Utilidades.CodeGeneration
{
    
    /// <summary>
    /// Tool to generate a new procedure
    /// </summary>
    public class ProcedureGenerator
    {

        /// <summary>
        /// El procedimiento creado
        /// </summary>
        public Procedure Procedure;

        /// <summary>
        /// La regla de parametros del procedimiento.
        /// </summary>
        public ReglaParm Parm;

        /// <summary>
        /// Crea un procedimiento vacio en la kbase
        /// </summary>
        public ProcedureGenerator()
        {
            // Crear el nuevo procedimiento
            Procedure = Procedure.Create(UIServices.KB.CurrentModel);
            Parm = new ReglaParm();
        }

        /// <summary>
        /// Crea en el procedimiento una variable basada en un atributo.
        /// La variables se llama igual que el atributo
        /// </summary>
        /// <param name="atributo">Atributo en el que basar la variable</param>
        /// <returns>La variable creada</returns>
        public Variable CrearVariableBasadaEnAtributo(Attribute atributo)
        {
            Variable v = new Variable(atributo.Name, Procedure.Variables);
            v.AttributeBasedOn = atributo;
            Procedure.Variables.Add(v);
            return v;
        }

        /// <summary>
        /// Crear en el procedimiento una variable de un sdt
        /// </summary>
        /// <param name="nombre">Nombre de la variable</param>
        /// <param name="sdt">Sdt del tipo que crear la variable</param>
        /// <returns>La variable creada</returns>
        public Variable CrearVariableSdt(string nombre, SDT sdt)
        {
            Variable v = new Variable(nombre, Procedure.Variables);
            // Ev2:
            //DataType.ParseInto(sdt.Model, sdt.Name, v);
            // Ev3:
            // DataType.ParseInto(sdt.Model, sdt.QualifiedName.ToString(), v); < This does not work if the SDT is not inside the Root module
            DataTypeCaps caps = new DataTypeCaps(eDBType.GX_SDT, sdt.QualifiedName);
            DataType.SetType(v.Model, caps, v);
            
            Procedure.Variables.Add(v);
            return v;
        }

        /// <summary>
        /// Crea una variable de un cierto tipo
        /// </summary>
        /// <param name="nombre">Nombre de la variable</param>
        /// <param name="tipo">Tipo del que crear la variable</param>
        /// <param name="longitud">Si la longitud es mas grande que cero, la longitud de la variable</param>
        /// <returns>La variable creada</returns>
        public Variable CreateVariable(string nombre, eDBType tipo, int longitud)
        {
            Variable v = new Variable(nombre, Procedure.Variables);
            v.Type = tipo;
            if (longitud > 0)
                v.Length = longitud;
            Procedure.Variables.Add(v);
            return v;
        }

        /// <summary>
        /// Crea una variable de un cierto tipo
        /// </summary>
        /// <param name="nombre">Nombre de la variable</param>
        /// <param name="tipo">Tipo del que crear la variable</param>
        /// <returns>La variable creada</returns>
        public Variable CreateVariable(string nombre, eDBType tipo)
        {
            return CreateVariable(nombre, tipo, 0);
        }

        /// <summary>
        /// Copy a variable to this procedure
        /// </summary>
        /// <param name="variable">Variable to copy</param>
        /// <returns>The new variable</returns>
        public Variable CreateVariable(Variable variable)
        {
            if (variable == null)
                return null;
            Variable v = new Variable(variable.Name, Procedure.Variables);
            v.CopyPropertiesFrom(variable);
            Procedure.Variables.Add(v);
            return v;
        }

        /// <summary>
        /// Crea variables &amp;FlgErr booleana y &amp;MsgErr V(200) para la gestion de errores.
        /// Se añaden como out: al parm del procedimiento
        /// </summary>
        public void CrearVariablesGestionErrores()
        {
            Variable flgErr = CreateVariable("FlgErr", eDBType.Boolean);
            Parm.AgregarParametro(flgErr, RuleDefinition.ParameterAccess.PARM_OUT);
            Parm.AddDocumentation("Cierto si se ha producido algun error");

            Variable msgErr = CreateVariable("MsgErr", eDBType.VARCHAR, 200);
            Parm.AgregarParametro(msgErr, RuleDefinition.ParameterAccess.PARM_OUT);
            Parm.AddDocumentation("Mensaje del error, si &FlgErr es cierto");
        }

        public void SaveProcedure()
        {
            Procedure.Rules.Source = Parm.ToString();
            Procedure.Save();
        }

        virtual public void GuardarYAbrirProcedimiento()
        {
            // Poner el codigo del objeto y guardarlo:
            SaveProcedure();
            UIServices.Objects.Open(Procedure, OpenDocumentOptions.CurrentVersion);
        }

    }
}
