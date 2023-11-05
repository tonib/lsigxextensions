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
using System.Text.RegularExpressions;
using Artech.Architecture.Language.Parser.Data;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
    /// <summary>
    /// Tool to search text patterns on genexus sources
    /// </summary>
    public class GxSourcesFinder : UISearchBase
    {

        /// <summary>
        /// Where to search the text pattern
        /// </summary>
        public enum TSearchWhere
        {
            ANYWHERE,
            COMMENTS,
            STRINGLITERALS
        };

        /// <summary>
        /// Text pattern to search
        /// </summary>
        private string TextPattern;

        /// <summary>
        /// Regular expression to search
        /// </summary>
        private Regex RegularExpression = null;

        /// <summary>
        /// String search options
        /// </summary>
        private StringComparison StringOptions;

        /// <summary>
        /// Part types (PartType class values) where to search. If it's empty, all ISource parts 
        /// will be searched
        /// </summary>
        public List<Guid> PartTypesSearch = new List<Guid>();

        /// <summary>
        /// Where to search
        /// </summary>
        public TSearchWhere WhereSearch = TSearchWhere.ANYWHERE;

        /// <summary>
        /// Parser to search text in some places of the text.
        /// </summary>
        private ParsedCodeFinder SourceParser;

        /// <summary>
        /// Search case sensitive text?
        /// </summary>
        public bool IsCaseSensitive = false;

        /// <summary>
        /// Texto to search is a regular expresion?
        /// </summary>
        public bool IsRegularExpression = false;

        /// <summary>
        /// Used when a source code is parsed. Will be true if the parted code match the text
        /// </summary>
        private bool CurrentParseMatch;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="textPattern">Text pattern to search</param>
        public GxSourcesFinder(string textPattern)
        {
            this.TextPattern = textPattern.Trim();
        }

        private void InitializeSearch()
        {
            // Source code parser
            this.SourceParser = new ParsedCodeFinder(ParseCodeNode)
            {
                IgnoreComments = ( WhereSearch != TSearchWhere.COMMENTS )
            };

            if (IsRegularExpression)
            {
                // Text regular expression
                RegexOptions opts = RegexOptions.Multiline | RegexOptions.Compiled;
                if (!IsCaseSensitive)
                    opts |= RegexOptions.IgnoreCase;
                RegularExpression = new Regex(TextPattern, opts);
            }
            else
            {
                // Plain text search options
                if (!IsCaseSensitive)
                    StringOptions = StringComparison.CurrentCultureIgnoreCase;
            }
        }

        private bool SourceMatchText(string source)
        {
            if (RegularExpression != null)
                return RegularExpression.Match(source).Success;
            else
                return source.IndexOf(TextPattern, StringOptions) >= 0;
        }

        private bool ParseObjectPart(KBObjectPart part)
        {
            if (WhereSearch == TSearchWhere.ANYWHERE)
                return true;

            // Do parse
            CurrentParseMatch = false;
            SourceParser.Execute(new ParsedCode(part));

            return CurrentParseMatch;
        }

        /// <summary>
        /// Searches the text on a object part
        /// </summary>
        /// <param name="part">Part where to search the text</param>
        /// <returns>True if the text was found</returns>
        private bool SearchOnPart(KBObjectPart part)
        {
            if (part == null)
                return false;

            ISource source = part as ISource;
            if (source == null)
                return false;

            if (SourceMatchText(source.Source))
            {
                // Text found: Do now the parse if its needed.
                if (!ParseObjectPart(part))
                    return false;

                PublishUIResult(new RefObjetoGX(part.KBObject));
                return true;
            }
            return false;
        }


        private void CheckObject(KBObject o)
        {
            // Fast test:
            if (!(o is ICallableInfo))
                return;
            
            // Parts where to search:
            // TODO: This will get all parts from db, even all never checked parts (forms)
            // TODO: Try to get only parts that will be checked
            IEnumerable<KBObjectPart> searchParts = o.Parts.LsiEnumerate();
            if (PartTypesSearch.Count > 0) 
            {
                // Not all parts
                searchParts = searchParts
                    .Where(part => PartTypesSearch.Contains(part.Type));
            }

            // Search on parts
            foreach (KBObjectPart part in searchParts)
            {
                if (SearchOnPart(part))
                    return;
            }
        }

        /// <summary>
        /// Run search
        /// </summary>
        public override void ExecuteUISearch()
        {
            InitializeSearch();

            this.MessagesMultiple = 10;

            foreach (KBObject o in UIServices.KB.CurrentModel.Objects.GetAll())
            {
                if (SearchCanceled)
                    return;

                CheckObject(o);
                IncreaseSearchedObjects();
            }
        }

        /// <summary>
        /// Delegate function to analize a node code on the genexus source
        /// </summary>
        /// <param name="codeNode">Code to analize</param>
        /// <param name="searchState">Current parse state</param>
        private void ParseCodeNode(ObjectBase codeNode, ParsedCodeFinder.SearchState searchState)
        {
            if (WhereSearch == TSearchWhere.STRINGLITERALS)
            {
                StringConstant str = codeNode as StringConstant;
                if (str == null)
                    return;

                if (SourceMatchText(str.Text))
                {
                    CurrentParseMatch = true;
                    searchState.SearchFinished = true;
                }
            }
            else if (WhereSearch == TSearchWhere.COMMENTS)
            {
                Blank comment = codeNode as Blank;
                if (comment == null)
                    return;
                if (SourceMatchText(comment.Text))
                {
                    CurrentParseMatch = true;
                    searchState.SearchFinished = true;
                }
            }
        }
    }
}
