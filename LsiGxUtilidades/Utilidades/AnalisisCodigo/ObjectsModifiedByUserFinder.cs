using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Udm.Framework;
using LSI.Packages.Extensiones.Utilidades.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
    /// <summary>
    /// Search modified objects
    /// </summary>
    public class ObjectsModifiedByUserFinder : UISearchBase
    {

        /// <summary>
        /// Reference to a kbobject with all the modifier users
        /// </summary>
        public class RefObjetGxWithUsers : RefObjetoGX
        {
            /// <summary>
            /// Users that have modified the object on the period
            /// </summary>
            public string ModifierUsers { get; set; }

            public RefObjetGxWithUsers(KBObject o, IEnumerable<KBObject> objectVersions): base(o)
            {
                string[] modifierUsers = objectVersions
                    .Select(x => GetSafeLastUser(x) )
                    .Distinct()
                    .OrderBy(x => x)
                    .ToArray();
                ModifierUsers = string.Join(", ", modifierUsers);
            }
        }

        /// <summary>
        /// The initial date to check
        /// </summary>
        private DateTime Date;

        /// <summary>
        /// The user to check
        /// </summary>
        private string User;

        public ObjectsModifiedByUserFinder(DateTime date, string user)
        {
            this.Date = date;
            this.User = user.ToLower().Trim();
        }

        static private string GetSafeLastUser(KBObject o)
        {
            return o.User != null ? o.User.Name.ToLower() : string.Empty;
        }

        /// <summary>
        /// Execute the search
        /// </summary>
        override public void ExecuteUISearch()
        {
            KBModel model = UIServices.KB.CurrentModel;
            foreach ( KBObject o in model.Objects.GetAll().Where( x => x.LastUpdate >= Date ) )
            {
                if (SearchCanceled)
                    return;

                // Check modifications history
                IEnumerable<KBObject> versions = o.GetVersionsDescendent().Where(x => x.LastUpdate >= Date);
                foreach (KBObject version in versions)
                {
                    if (SearchCanceled)
                        return;

                    if(GetSafeLastUser(version).Contains(User))
                    {
                        PublishUIResult(new RefObjetGxWithUsers(o, versions));
                        break;
                    }
                }

                IncreaseSearchedObjects();
            }
        }
    }
}
