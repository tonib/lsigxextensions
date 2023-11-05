using Artech.Architecture.Common.Objects;
using Artech.Architecture.Language.Parser;
using Artech.Architecture.Language.Parser.Data;
using Artech.Architecture.Language.Parser.Objects;
using Artech.Common.Language.Parser;
using Artech.Genexus.Common.Parts;
using Artech.Patterns.WorkWithDevices.Objects;
using System;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
	/// <summary>
	/// Utilidad para parsear codigo fuente de genexus
	/// </summary>
	public class ParserGx
    {

        /// <summary>
        /// Parse una parte de codigo de un objeto de genexus
        /// </summary>
        /// <param name="parte">La parte del objeto</param>
        /// <returns>La parte parseada del objeto</returns>
        static public IParserObjectBase ParsearParte(KBObjectPart parte)
        {
            if (!(parte is ISource))
                throw new Exception("La parte no es un ISource");
            return ParsearCodigo(parte, ((ISource)parte).Source);
        }

        static private IParserObjectBase ParseCode(ParserInfo info, string code)
        {
            ParserManager manager = new ParserManager();
            IParserBuffer buffer = manager.GetStringBuffer(code);
            manager.ParseSource(info, buffer);
            IParserEngine2 parser = manager;
            return (IParserObjectBaseCollection)parser.Structure;
        }

        static public ParserInfo GetParserInfoFromKbPart(KBObjectPart part)
        {
            // Ugly things for SDPanel compatibility (EV2 U2)
            ParserInfo info;
            if (part.KBObject is SDPanel)
            {
                ParserType codeType;
                if (part is RulesPart)
                    codeType = ParserType.RulesSD;
                else if (part is EventsPart)
                    codeType = ParserType.EventsSD;
                else if (part is ConditionsPart)
                    codeType = ParserType.ConditionsSD;
                else
                    codeType = ParserType.Unknown;
                info = new ParserInfo(codeType);
            }
            else
                info = new ParserInfo(part);

            return info;
        }

        // TODO: This function should return IParserObjectBaseCollection instead of IParserObjectBase???
        /// <summary>
        /// Parsear un trozo de codigo de una parte de un objeto de genexus
        /// </summary>
        /// <param name="parte">La parte del objeto a la que pertenece el codigo</param>
        /// <param name="codigo">El codigo a parsear</param>
        /// <returns>Estructura del codigo parsesado</returns>
        static public IParserObjectBase ParsearCodigo(KBObjectPart parte, string codigo)
        {
            if (!(parte is ISource))
                throw new Exception("Part is not ISource");

            codigo = Entorno.StringFormatoKbase(codigo);

            // Ugly things for SDPanel compatibility (EV2 U2)
            ParserInfo info = GetParserInfoFromKbPart(parte);

            return ParseCode(info, codigo);
        }

        
        static public IParserObjectBaseCollection ParsearCodigo(string codigo, ParserType tipoCodigo)
        {
            ParserInfo info = new ParserInfo(tipoCodigo);
            return (IParserObjectBaseCollection) ParseCode(info, codigo);
        }

        static public ObjectBase ParsearExpresion(string codigo)
        {
            IParserObjectBaseCollection coleccion = ParsearCodigo(codigo, ParserType.Expressions);
            if (coleccion == null || coleccion.Count == 0)
                return null;

            // El parser añade un salto de linea al final: Quitarlo
            QuitarSaltosLinea(coleccion[0]);

            return coleccion[0].Data;
        }

        static private void DelegadoQuitarSaltosLinea(ObjectBase nodo, ParsedCodeFinder.SearchState estado)
        {
            if (!(nodo is WordWithBlanks))
                return;

            WordWithBlanks word = (WordWithBlanks)nodo;
            if (!(word.Blanks is Word))
                return;

            Word blancos = (Word)word.Blanks;
            blancos.Text = blancos.Text.Replace(Environment.NewLine, "");
        }

        static public void QuitarSaltosLinea(IParserObjectBase arbolParseado)
        {
            ParsedCodeFinder buscador = new ParsedCodeFinder(DelegadoQuitarSaltosLinea);
            buscador.Execute(null, arbolParseado);
        }

    }
}
