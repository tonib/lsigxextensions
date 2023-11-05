using Artech.Architecture.Common.Objects;
using Artech.Architecture.Language.Parser;
using Artech.Architecture.Language.Parser.Data;
using Artech.Architecture.Language.Parser.Objects;
using Artech.Common.Properties;
using Artech.Genexus.Common.CustomTypes;
using Artech.Packages.Patterns.Specification;
using System;
using System.Collections.Generic;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
    /// <summary>
    /// The parsed code tree of a Genexus code
    /// </summary>
    public class ParsedCode
    {

        /// <summary>
        /// El codigo original a analizar.
        /// </summary>
        private string OriginalCode;

        /// <summary>
        /// Objeto al que pertenece el codigo
        /// </summary>
        public KBObject Object { get; private set; }

        /// <summary>
        /// Parte del objeto al que pertenece el codigo analizado. Puede ser nulo
        /// </summary>
        /// <remarks>
        /// Se usa si el codigo pertenece directamente a una parte ISource del objeto. Si no, es nulo.
        /// </remarks>
        public KBObjectPart Part { get; private set; }

        /// <summary>
        /// Tipo de parser a aplicar al codigo analizado. 
        /// </summary>
        /// <remarks>
        /// Se usa si el codigo no pertenece directamente a una parte del objeto (p.ej. el valor 
        /// de una propiedad "Order" en un grid). Si no, es nulo
        /// </remarks>
        private ParserType ParserType;

        /// <summary>
        /// El codigo parseado. Se inicializa al referenciar a _ArbolParseado. Puede ser nulo si
        /// el codigo a analizar era vacio
        /// </summary>
        private IParserObjectBase _ParsedTree;

        /// <summary>
        /// Indica si ya se ha inicializado _ArbolParseado
        /// </summary>
        private bool CodigoParseado;

        private void InicializarArbolParseado()
        {
            if (!CodigoParseado)
            {
                if (Part != null)
                    _ParsedTree = ParserGx.ParsearCodigo(Part, OriginalCode);
                else
                    // Caso especial para el codigo de las conditions de un grid
                    _ParsedTree = ParserGx.ParsearCodigo(OriginalCode, ParserType);
                CodigoParseado = true;
            }
        }

        /// <summary>
        /// El arbol de codigo parseado
        /// </summary>
        public IParserObjectBase ArbolParseado
        {
            get
            {
                InicializarArbolParseado();
                return _ParsedTree;
            }
        }

        /// <summary>
        /// The current code of the parsed tree
        /// </summary>
        public string ParsedCodeString
        {
            get
            {
                InicializarArbolParseado();
                if( _ParsedTree == null )
                    return string.Empty;

                return _ParsedTree.ToString();
            }
        }

        /// <summary>
        /// The current code of the parsed tree
        /// </summary>
        public override string ToString()
        {
            return ParsedCodeString;
        }

        /// <summary>
        /// Constructor para analizar el codigo de una parte de un objeto genexus
        /// </summary>
        /// <param name="parteCodigo">Parte de codigo del objeto a analizar</param>
        public ParsedCode(KBObjectPart parteCodigo)
        {
            if (!(parteCodigo is ISource))
                throw new Exception("La parte indicada no es de código");

            Part = parteCodigo;
            OriginalCode = ((ISource)parteCodigo).Source;
            Object = parteCodigo.KBObject;
        }

        /// <summary>
        /// Constructor para analizar un trozo de codigo de una parte de un objeto genexus
        /// </summary>
        /// <param name="codigo">El codigo a analizar</param>
        public ParsedCode(KBObjectPart parte, string codigo)
        {
            if (!(parte is ISource))
                throw new Exception("La parte indicada no es de código");

            Part = parte;
            OriginalCode = codigo;
            Object = parte.KBObject;
        }

        /// <summary>
        /// Constructor to parse a code from a ParseableString
        /// </summary>
        /// <param name="objeto">Object owner of the code to parse</param>
        /// <param name="codigo">The code to parse</param>
        public ParsedCode(KBObject objeto, ParseableString codigo)
        {
            ParserType = codigo.ParserType;
            OriginalCode = codigo.Data;
            Object = objeto;
        }

        /// <summary>
        /// Constructor to parse generic code
        /// </summary>
        /// <param name="o">Object owner of the code to parse</param>
        /// <param name="parserType">The parser type for the code</param>
        /// <param name="code">The code to parse</param>
        public ParsedCode(KBObject o, ParserType parserType, string code)
        {
            ParserType = parserType;
            OriginalCode = code;
            Object = o;
        }

        /// <summary>
        /// Constructor to parse a code string
        /// </summary>
        /// <param name="o">Object owner of the code to parse</param>
        /// <param name="p">Descriptor of the property that owns the code</param>
        /// <param name="code">The code to parse</param>
        public ParsedCode(KBObject o, PropertyManager.PropertySpecDescriptor p, CodeString code)
        {
            CodeStringSettingsAttribute codeSettings =
                p.Attributes[typeof(CodeStringSettingsAttribute)] as CodeStringSettingsAttribute;
            if (codeSettings == null)
                throw new Exception("Code string has no settings");
            ParserType = codeSettings.ParserType;
            OriginalCode = code.Data;
            Object = o;
        }

        /// <summary>
        /// Appends code to the end 
        /// </summary>
        /// <param name="code">Code to append</param>
        public void AppendCode(IParserObjectBaseCollection newCode)
        {
            InicializarArbolParseado();
            if (_ParsedTree == null)
                _ParsedTree = newCode;
            else
            {
                foreach (IParserObjectBase c in newCode)
                    ((IParserObjectBaseCollection)_ParsedTree).Add(c);
            }
        }

        /// <summary>
        /// Appends code to the end 
        /// </summary>
        /// <param name="code">Code to append</param>
        public void AppendCode(string newCode)
        {
            AppendCode(ParserGx.ParsearCodigo(Part, newCode) as IParserObjectBaseCollection);
        }

        /// <summary>
        /// Appends code to a command block body
        /// </summary>
        /// <param name="code">Code to add</param>
        /// <param name="cmdBlock">Command block where to append code</param>
        public void AppendCodeToCommandBlock(string code, CommandBlock cmdBlock)
        {
            if (cmdBlock.Body == null)
                cmdBlock.Body = new ObjectBaseCollection();

            IParserObjectBaseCollection parsedCode =
                (IParserObjectBaseCollection)ParserGx.ParsearCodigo(Part, code);
            foreach (IParserObjectBase c in parsedCode)
                cmdBlock.Body.Add(c.Data);
        }

        /// <summary>
        /// Check if some code node match some conditions
        /// </summary>
        /// <param name="predicate">Conditions to check on the node</param>
        /// <returns>True if there is some node that matchs that matchs the conditions</returns>
        public bool Any(Predicate<ObjectBase> predicate)
        {
            bool containsAny = false;
            ParsedCodeFinder finder = new ParsedCodeFinder(
                delegate(ObjectBase nodeData, ParsedCodeFinder.SearchState state)
                {
                    if (predicate(nodeData))
                    {
                        containsAny = true;
                        state.SearchFinished = true;
                    }
                }
            );
            finder.Execute(this);
            return containsAny;
        }

        /// <summary>
        /// Get code nodes that match some conditions
        /// </summary>
        /// <param name="predicate">Conditions to check</param>
        /// <returns>The code nodes that match the conditions</returns>
        public List<ObjectBase> Where(Predicate<ObjectBase> predicate, bool ignoreComments = true)
        {
            List<ObjectBase> result = new List<ObjectBase>();
            ParsedCodeFinder finder = new ParsedCodeFinder(
                delegate(ObjectBase nodeData, ParsedCodeFinder.SearchState state)
                {
                    if (predicate(nodeData))
                        result.Add(nodeData);
                }
            );
            finder.IgnoreComments = ignoreComments;
            finder.Execute(this);
            return result;
        }

        /// <summary>
        /// Do some action on all code nodes of the tree
        /// </summary>
        /// <param name="action">Action to peform on the node</param>
        public void ForEach(Action<ObjectBase> action)
        {
            ParsedCodeFinder finder = new ParsedCodeFinder(
                delegate(ObjectBase nodeData, ParsedCodeFinder.SearchState state)
                {
                    action(nodeData);
                }
            );
            finder.Execute(this);
        }

        /// <summary>
        /// Get the first code node that match some conditions
        /// </summary>
        /// <param name="predicate">The conditions to check</param>
        /// <returns>The first node that match the conditions. null if there is no node
        /// that match the conditions</returns>
        public ObjectBase FirstOrDefault(Predicate<ObjectBase> predicate)
        {
            ObjectBase result = null;
            ParsedCodeFinder finder = new ParsedCodeFinder(
                delegate(ObjectBase nodeData, ParsedCodeFinder.SearchState state)
                {
                    if (predicate(nodeData))
                    {
                        result = nodeData;
                        state.SearchFinished = true;
                    }
                }
            );
            finder.Execute(this);
            return result;
        }

    }
}
