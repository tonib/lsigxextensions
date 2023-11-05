using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.UI;

namespace LSI.Packages.Extensiones.Utilidades.KBSync
{

    /// <summary>
    /// UI object reference with sync info
    /// </summary>
    public class KBSyncObjectRef : RefObjetoGX
    {

        /// <summary>
        /// Comments about export status
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Kb origin object version number with differences with the destination.
        /// Zero if no common version was found or the object is new on origin.
        /// This is the version number on the SOURCE kbase. On destination, it can be other
        /// </summary>
        public int VersionIdUpdateOrigin { get; set; }

        /// <summary>
        /// Kb destination object version number with differences with the destination.
        /// Zero if no common version was found or the object is new on origin.
        /// This is the version number on the DESTINATION kbase. On destination, it can be other
        /// </summary>
        public int VersionIdUpdateDestination { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="o">Object to reference</param>
        public KBSyncObjectRef(KBSyncObjectInfo o) : base(o.Object)
        {
            Comments = o.Comments;
            VersionIdUpdateOrigin = o.VersionIdUpdateOrigin;
            VersionIdUpdateDestination = o.VersionIdUpdateDestination;
        }

    }
}
