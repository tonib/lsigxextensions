using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.Threading;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.UI.Framework.Packages;
using Artech.Common.Framework.Selection;
using Artech.Architecture.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using Artech.Udm.Framework;
using System.IO;
using System.Xml.Serialization;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Utilidades.KBSync
{
    /// <summary>
    /// Kbase syncronization information
    /// </summary>
    public class KBSyncInfo
    {

        /// <summary>
        /// Export origin kbase path
        /// </summary>
        public string OriginKBase;

        /// <summary>
        /// Kbase where the export information was reviewed. null if the information
        /// has not been reviewed yet
        /// </summary>
        public string DestinationKBase;

        /// <summary>
        /// Objects syncronization information
        /// </summary>
        public List<KBSyncObjectInfo> ObjectsInfo = new List<KBSyncObjectInfo>();

        /// <summary>
        /// Constructor
        /// </summary>
        internal KBSyncInfo() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kbasePath">Export origin kbase path</param>
        public KBSyncInfo(string kbasePath)
        {
            OriginKBase = kbasePath;
        }

        /// <summary>
        /// Add objects and its version history
        /// </summary>
        /// <param name="objects">Object to add to the info</param>
        /// <param name="log">Log output to write status messages. It can be null</param>
        public void AddObjects(List<KBObject> objects, Log log)
        {
            int count = 0;
            foreach (KBObject o in objects)
            {
                ObjectsInfo.Add(new KBSyncObjectInfo(o));
                count++;
                if (log != null && (count % 100) == 0)
                    log.Output.AddLine(count + " objects exported...");
            }
        }

        /// <summary>
        /// Get versioning information about an object
        /// </summary>
        /// <param name="o">Object to search</param>
        /// <returns>Object information. null if it was not found</returns>
        public KBSyncObjectInfo GetObjectInfo(KBObject o)
        {
            // TODO: This should not be searched by GUID ???
            return ObjectsInfo
                .Where(x => x.ObjectType == o.Type && x.ObjectName.ToLower() == o.Name.ToLower())
                .FirstOrDefault();
        }

        /// <summary>
        /// Get versioning information about an object
        /// </summary>
        /// <param name="objectId">Object id to search</param>
        /// <returns>Object information. null if it was not found</returns>
        public KBSyncObjectInfo GetObjectInfo(Guid objectId)
        {
            return ObjectsInfo
                .Where(x => x.ObjectGuid == objectId)
                .FirstOrDefault();
        }

        /// <summary>
        /// Saves the sync information to a xml file
        /// </summary>
        /// <param name="filePath">Destination xml file path</param>
        public void SaveToFile(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(KBSyncInfo));
            TextWriter writer = new StreamWriter(filePath);
            serializer.Serialize(writer, this);
            writer.Close();
        }

        /// <summary>
        /// Load sync information from a xml file
        /// </summary>
        /// <param name="filePath">Xml file path to read</param>
        /// <returns>The sync information</returns>
        static public KBSyncInfo LoadFromFile(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(KBSyncInfo));
            TextReader reader = new StreamReader(filePath);
            KBSyncInfo info = (KBSyncInfo)serializer.Deserialize(reader);
            reader.Close();
            return info;
        }

        /// <summary>
        /// It checks if the objects can be exported or not.
        /// </summary>
        /// <remarks>
        ///  This function must to be executed on the destination kbase. It update the exportation
        ///  state of each object
        /// </remarks>
        /// <param name="reviewKBasePath">Kbase destination path</param>
        /// <param name="log">Log output to write status messages. It can be null</param>
        public void ReviewOnDestinationKB(string reviewKBasePath, Log log)
        {
            this.DestinationKBase = reviewKBasePath;
            int count = 0;
            foreach (KBSyncObjectInfo o in ObjectsInfo)
            {
                o.ReviewOnDestinationKB();
                count++;
                if (log != null && (count % 100) == 0)
                    log.Output.AddLine(count + " objects reviewed...");
            }
        }

        /// <summary>
        /// Return the number of objects with some export status
        /// </summary>
        /// <param name="status">Status to search</param>
        /// <returns>Number of objects with that status</returns>
        public int NObjectsWithStatus(KBSyncObjectInfo.ExportStatus status)
        {
            return ObjectsInfo.Count(x => x.Status == status);
        }

        public string ReviewStatusMessage
        {
            get
            {
                return NObjectsWithStatus(KBSyncObjectInfo.ExportStatus.UNMODIFIED) +
                    " obj. unmodified, " +
                    NObjectsWithStatus(KBSyncObjectInfo.ExportStatus.EXPORTABLE) +
                    " obj. exportable, " +
                    NObjectsWithStatus(KBSyncObjectInfo.ExportStatus.NOTEXPORTABLE) +
                    " obj. not exportable";
            }
        }
    }
}
