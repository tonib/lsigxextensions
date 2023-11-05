using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSI.Packages.Extensiones.Utilidades.KBSync
{
    /// <summary>
    /// Object syncronization information
    /// </summary>
    public class KBSyncObjectInfo
    {

        /// <summary>
        /// Object export status
        /// </summary>
        public enum ExportStatus
        {
            /// <summary>
            /// Object is not modified on origin kb, and it does not need an export
            /// </summary>
            UNMODIFIED,

            /// <summary>
            /// Object needs to export, and it can be entirelly exported
            /// </summary>
            EXPORTABLE,

            /// <summary>
            /// Object needs to export, but it cannot be done (modified on destination kb)
            /// </summary>
            NOTEXPORTABLE
        }

        /// <summary>
        /// Maximum number of versions to store for an object
        /// </summary>
        public const int MAXNUMBERVERSION = 70;

        /// <summary>
        /// Object version information
        /// </summary>
        public class VersionInfo 
        {

            /// <summary>
            /// Object version modification date
            /// </summary>
            public DateTime LastUpdate;

            /// <summary>
            /// Object version number
            /// </summary>
            public int VersionId;

            public override string ToString()
            {
                return VersionId + " - " + LastUpdate;
            }
        }

        /// <summary>
        /// Object type identifier
        /// </summary>
        public Guid ObjectType;

        /// <summary>
        /// Object identifier
        /// </summary>
        public Guid ObjectGuid;

        /// <summary>
        /// Object name (into the module)
        /// </summary>
        public string ObjectName;

        /// <summary>
        /// Module name
        /// </summary>
        public string ModuleName;

        /// <summary>
        /// Object namespace, to check name conflicts on destination kb 
        /// </summary>
        public string ObjectNamespace;

        /// <summary>
        /// Is object entirely exportable?
        /// </summary>
        public ExportStatus Status = ExportStatus.UNMODIFIED;

        /// <summary>
        /// Comments about exportation status
        /// </summary>
        public string Comments;

        /// <summary>
        /// Kb origin object version number with differences with the destination.
        /// Zero if no common version was found or the object is new on origin.
        /// This is the version number on the SOURCE kbase. On destination, it can be other
        /// </summary>
        public int VersionIdUpdateOrigin;

        /// <summary>
        /// Kb destination object version number with differences with the destination.
        /// Zero if no common version was found or the object is new on origin.
        /// This is the version number on the DESTINATION kbase. On destination, it can be other
        /// </summary>
        public int VersionIdUpdateDestination;

        /// <summary>
        /// Last objects versions, up to MAXNUMBERVERSION
        /// </summary>
        public List<VersionInfo> Versions = new List<VersionInfo>();

        /// <summary>
        /// Constructor
        /// </summary>
        public KBSyncObjectInfo()
        { }

        /// <summary>
        /// Get object history information
        /// </summary>
        /// <param name="o">Object to get history information</param>
        public KBSyncObjectInfo(KBObject o)
        {
            ObjectName = o.Name;
            ModuleName = o.Module.QualifiedName.ToString();
            ObjectType = o.Type;
            ObjectGuid = o.Guid;
            ObjectNamespace = ((IKBObject) o).Namespace;    // No cast == compiler error. Why???

            int nVersions = 0;
            foreach (KBObject version in o.GetVersionsDescendent())
            {
                Versions.Add(new VersionInfo()
                {
                    LastUpdate = version.LastUpdate,
                    VersionId = version.VersionId
                });
                if (++nVersions >= MAXNUMBERVERSION)
                    break;
            }
        }

        /// <summary>
        /// KBase object. null if the object was not found
        /// </summary>
        public KBObject Object
        {
            get
            {
                // try to get the object by id:
                KBObject o = UIServices.KB.CurrentModel.Objects.Get(this.ObjectGuid);
                if (o == null)
                {
                    // Try to get the object with the same name and type:
                    QualifiedName qName = new QualifiedName(ModuleName, this.ObjectName);
                    o = KBObject.Get(UIServices.KB.CurrentModel, ObjectType, qName);
                }
                return o;
            }
        }

        /// <summary>
        /// Reference to the object to use on UI grids
        /// </summary>
        public KBSyncObjectRef ObjectRef
        {
            get
            {
                return new KBSyncObjectRef(this);
            }
        }

        /// <summary>
        /// Set the object export status
        /// </summary>
        /// <param name="status">Object status</param>
        /// <param name="comments">Object comments</param>
        /// <param name="destinationKbObject">Object on the destination KB. Used to 
        /// check if the object name has changed. It can be null, if the object was not found</param>
        public void SetStatus(ExportStatus status, string comments, KBObject destinationKbObject)
        {
            Status = status;
            Comments = comments;
            if (destinationKbObject != null && destinationKbObject.Name.ToLower() != ObjectName.ToLower())
                Comments += " (name on destination is " + destinationKbObject.Name + ")";
        }

        public void ReviewOnDestinationKB()
        {
            Comments = null;
            VersionIdUpdateOrigin = 0;
            VersionIdUpdateDestination = 0;

            KBObject o = Object;
            if (o == null)
            {
                // Check if there is already an object with the same name on the same namespace:
                QualifiedName qName = new QualifiedName(ModuleName, ObjectName);
                o = UIServices.KB.CurrentModel.Objects.Get(ObjectNamespace, qName);
                if (o != null)
                    SetStatus(ExportStatus.NOTEXPORTABLE,
                        "New on origin, but there is a " + ObjectName +
                        " on destination with different type", o);
                else
                    // New object on destination
                    SetStatus(ExportStatus.EXPORTABLE, "Object is new on origin kb", o);
                return;
            }

            // Object exists on destination kb. Check version:
            if (Versions.Count == 0)
            {
                // This should not happen
                SetStatus(ExportStatus.NOTEXPORTABLE, "History info not found on exported XML", o);
                return;
            }
            if (o.LastUpdate == Versions[0].LastUpdate)
            {
                // Unmodified object
                SetStatus(ExportStatus.UNMODIFIED, "Object unmodified", o);
                return;
            }

            // Modified object: Get first common version on both kbases:
            // For each version (descendant) on destination kb
            bool firstDestinationVersion = true;
            foreach (KBObject version in o.GetVersionsDescendent())
            {
                // Get common version on origin kb
                VersionInfo commonVersionOrigin =
                    Versions.Where(x => x.LastUpdate == version.LastUpdate).LastOrDefault();
                if (commonVersionOrigin != null)
                {
                    VersionIdUpdateOrigin = commonVersionOrigin.VersionId;
                    VersionIdUpdateDestination = version.VersionId;
                    if (commonVersionOrigin.VersionId == Versions[0].VersionId)
                        // Object was modified on destination, but not in origin
                        SetStatus(ExportStatus.UNMODIFIED, "Modified on destination KB, but not in origin", o);
                    else if (firstDestinationVersion)
                        // Object was modified on origin, but not on destination
                        SetStatus(ExportStatus.EXPORTABLE, "Modified on origin KB", o);
                    else
                        SetStatus(ExportStatus.NOTEXPORTABLE, "Object modified on origin and destination KB", o);
                    return;
                }
                firstDestinationVersion = false;
            }

            // Modified object on destination kb, but no common version found:
            SetStatus(ExportStatus.NOTEXPORTABLE, "Object modified on origin and destination KB", o);

        }

        public override string ToString()
        {
            string txt = this.ObjectName;
            if (Versions.Count > 0)
                txt += Versions[0].ToString();
            return txt;
        }
    }
}
