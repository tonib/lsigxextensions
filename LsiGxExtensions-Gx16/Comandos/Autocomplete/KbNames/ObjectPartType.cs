using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames
{
    /// <summary>
    /// Object / part type combination
    /// </summary>
    public class ObjectPartType : IEquatable<ObjectPartType>
    {
        /// <summary>
        /// Object type
        /// </summary>
        public Guid ObjectType;

        /// <summary>
        /// Part type
        /// </summary>
        public Guid PartType;

        // Work with plus adds it's own part types (¯\_(ツ)_/¯)
        private static readonly Guid ProcedurePartWwplus = new Guid("73ea6ce6-5ef1-4dd5-8d8a-62057920a70b");
        private static readonly Guid EventsPartWwplus = new Guid("572deeff-da39-4934-ae0e-8590f0228bd5");

        public static readonly ObjectPartType Empty = new ObjectPartType();

		public static readonly ObjectPartType ProcedureRules = new ObjectPartType(ObjClass.Procedure, Artech.Genexus.Common.PartType.Rules);
		public static readonly ObjectPartType Procedure = new ObjectPartType(ObjClass.Procedure, Artech.Genexus.Common.PartType.Procedure);
        public static readonly ObjectPartType ProcedureConditions = new ObjectPartType(ObjClass.Procedure, Artech.Genexus.Common.PartType.Conditions);
        public static readonly ObjectPartType ProcedureWwPlus = new ObjectPartType(ObjClass.Procedure, ProcedurePartWwplus);

        public static readonly ObjectPartType WorkPanelRules = new ObjectPartType(ObjClass.WorkPanel, Artech.Genexus.Common.PartType.Rules);
		public static readonly ObjectPartType WorkPanelEvents = new ObjectPartType(ObjClass.WorkPanel, Artech.Genexus.Common.PartType.Events);
        public static readonly ObjectPartType WorkPanelConditions = new ObjectPartType(ObjClass.WorkPanel, Artech.Genexus.Common.PartType.Conditions);

		public static readonly ObjectPartType TransactionRules = new ObjectPartType(ObjClass.Transaction, Artech.Genexus.Common.PartType.Rules);
		public static readonly ObjectPartType TransactionEvents = new ObjectPartType(ObjClass.Transaction, Artech.Genexus.Common.PartType.Events);
        public static readonly ObjectPartType TransactionEventsWwPlus = new ObjectPartType(ObjClass.Transaction, EventsPartWwplus);

        public static readonly ObjectPartType WebPanelRules = new ObjectPartType(ObjClass.WebPanel, Artech.Genexus.Common.PartType.Rules);
		public static readonly ObjectPartType WebPanelEvents = new ObjectPartType(ObjClass.WebPanel, Artech.Genexus.Common.PartType.Events);
        public static readonly ObjectPartType WebPanelEventsWwPlus = new ObjectPartType(ObjClass.WebPanel, EventsPartWwplus);
        public static readonly ObjectPartType WebPanelConditions = new ObjectPartType(ObjClass.WebPanel, Artech.Genexus.Common.PartType.Conditions);

		// These SOMETIMES (not always) give me a "Lsi.Extensions exception : System.TypeInitializationException: Se produjo una excepción en el inicializador de tipo de 'LSI.Packages.Extensiones.Comandos.Autocomplete.Commands.CommandsAutocomplete'. ---> System.TypeInitializationException: The type initializer for 'LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames.ObjectPartType' threw an exception. ---> System.IO.FileNotFoundException: Could not load file or assembly 'Artech.Patterns.WorkWithDevices, Version=10.1.0.0, Culture=neutral, PublicKeyToken=6f5bf81c27b6b8aa' or one of its dependencies. El sistema no puede encontrar el archivo especificado."
		// So, that's why part types GUIDs are hardcoded
		//public static readonly ObjectPartType SDPanelEvents = new ObjectPartType(ObjClass.SDPanel, typeof(VirtualEventsPart).GUID);
		//public static readonly ObjectPartType SDPanelConditions = new ObjectPartType(ObjClass.SDPanel, typeof(VirtualConditionsPart).GUID);
		public static readonly ObjectPartType SDPanelRules = new ObjectPartType(ObjClass.SDPanel, new Guid("1B0A32A3-DE6D-4be1-A4DD-1B85D3741534"));
		public static readonly ObjectPartType SDPanelEvents = new ObjectPartType(ObjClass.SDPanel, new Guid("144BD5FF-F918-415b-98E6-ACA44FED84FA"));
        public static readonly ObjectPartType SDPanelConditions = new ObjectPartType(ObjClass.SDPanel, new Guid("163F0D8B-D8AC-4db4-8DD4-DE8979F2B5B9"));

        /// <summary>
        /// "Empty" part type
        /// </summary>
        public ObjectPartType() { }

        public ObjectPartType(Guid objectType, Guid partType)
        {
            ObjectType = objectType;
            PartType = partType;
        }

        /// <summary>
        /// Get the object / part type from a KBObject part
        /// </summary>
        /// <param name="part">The source part</param>
        public ObjectPartType(KBObjectPart part)
        {
            ObjectType = part.KBObject.Type;
            PartType = part.Type;
        }

        /// <summary>
        /// Return true if the object / part type is equal to this instance
        /// </summary>
        /// <param name="part">Part to check</param>
        /// <returns>True if types are equal</returns>
        public bool Match(KBObjectPart part)
        {
            return ObjectType == part.KBObject.Type && PartType == part.Type;
        }

        /// <summary>
        /// Part type name
        /// </summary>
        public string PartTypeName
        {
            get { return Artech.Genexus.Common.PartType.GetType(PartType).Name; }
        }

        public bool Equals(ObjectPartType objectPartType)
        {
            if (objectPartType == null)
                return false;
            return ObjectType == objectPartType.ObjectType && PartType == objectPartType.PartType;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ObjectPartType);
        }

        public override int GetHashCode()
        {
            return ObjectType.GetHashCode() * 17 + PartType.GetHashCode();
        }
    }
}
