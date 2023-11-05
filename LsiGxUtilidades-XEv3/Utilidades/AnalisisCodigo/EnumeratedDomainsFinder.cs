using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Objects;
using Artech.Udm.Framework.References;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using LSI.Packages.Extensiones.Utilidades.UI;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
    /// <summary>
    /// Tool to search references to enumerated domains values
    /// </summary>
    public class EnumeratedDomainsFinder : UISearchBase
    {

        /// <summary>
        /// The domain to search
        /// </summary>
        private Domain DomainToSearch;

        /// <summary>
        /// Enumerated value description to search
        /// </summary>
        private string EnumValue;

        /// <summary>
        /// Kind of indirection: Function member or enum value
        /// </summary>
        private KindOfIndirection IndirectionType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="domain">Domain to search</param>
        /// <param name="enumValue">Enumerated value description to search</param>
        /// <param name="IndirectionType">Kind of indirection: Function member or enum value</param>
        public EnumeratedDomainsFinder(Domain domain, string enumValue, 
            KindOfIndirection indirectionType)
        {
            this.DomainToSearch = domain;
            this.EnumValue = enumValue;
            this.IndirectionType = indirectionType;
        }

        /// <summary>
        /// Execute the search
        /// </summary>
        public override void ExecuteUISearch()
        {

            // Create the token finder:
            TokenGx enumValueToken = new TokenGx(TokenType.ATTRIBUTE, DomainToSearch.Name);
            enumValueToken.AcceptIndirections = true;
            enumValueToken.IndirectionsFilter = new TokenIndirectionFilter();
            enumValueToken.IndirectionsFilter.Kind = IndirectionType;
            enumValueToken.IndirectionsFilter.MembersFilter.Add(EnumValue.ToLower());
            TokensFinder enumValueFinder = new TokensFinder(enumValueToken);

            // Get domain referrers:
            foreach (EntityReference r in DomainToSearch.GetReferencesTo())
            {
                if (SearchCanceled)
                    break;

                // Check if the object referrer references the domain value:
                KBObject referrer = KBObject.Get(UIServices.KB.CurrentModel, r.From);
                if (TokensFinder.IsUnsupportedObject(referrer))
                {
                    // Unsupported object type:
                    PublishUIResult(new RefObjetoGX(referrer)
                    {
                        PosibleFalsoPositivo = true
                    });
                }
                else
                {
                    // Check if the object references the domain value:
                    if (enumValueFinder.ContainsReference(referrer))
                        PublishUIResult(new RefObjetoGX(referrer));
                }
                IncreaseSearchedObjects();
            }
        }

    }
}
