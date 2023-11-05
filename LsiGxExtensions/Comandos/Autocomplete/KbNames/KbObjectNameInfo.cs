using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Language;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Udm.Framework;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames
{
    /// <summary>
    /// Kbobject name, except attributes
    /// </summary>
    public class KbObjectNameInfo : TypedNameInfo, ITimestamped
    {

        /// <summary>
        /// The object identifier
        /// </summary>
        public EntityKey ObjectKey;

        /// <summary>
        /// Object timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Creates a KbObject name info
        /// </summary>
        /// <param name="o">The KbObject to store</param>
        public KbObjectNameInfo(KBObject o)
        {
            Name = o.Name;
            _Description = o.Description;
            ObjectKey = o.Key;
            Timestamp = o.Timestamp;

            // TODO: Check if there is a Gx function for this conversion
            // Meanwhile this and KbObjectNameInfo constructor should be paired
            // TODO: Change this to a switch?
            if (o.Type == ObjClass.DataSelector)
                Type = ChoiceInfo.ChoiceType.DataSelector;
            else if (o.Type == ObjClass.Domain)
            {
                Type = ChoiceInfo.ChoiceType.Domain;
                Domain d = (Domain)o;
                _DataType = new DataTypeInfo(d);
                _Description += " / " + VariableGX.TypeDescription(d, false);
            }
            else if (o.Type == ObjClass.ExternalObject)
                Type = ChoiceInfo.ChoiceType.ExternalObject;
            else if (o.Type == ObjClass.Procedure)
                Type = ChoiceInfo.ChoiceType.Procedure;
            else if (o.Type == ObjClass.Transaction)
                Type = ChoiceInfo.ChoiceType.Transaction;
            else if (o.Type == ObjClass.WebPanel)
                Type = ChoiceInfo.ChoiceType.WebPanel;
            else if (o.Type == ObjClass.WorkPanel)
                Type = ChoiceInfo.ChoiceType.WorkPanel;
            else if (o.Type == ObjClass.Image)
                Type = ChoiceInfo.ChoiceType.Image;
            else if (o.Type == ObjClass.DataProvider)
                Type = ChoiceInfo.ChoiceType.DataProvider;
            else if (o.Type == ObjClass.SDPanel)
                Type = ChoiceInfo.ChoiceType.SDPanel;
            else if (o.Type == ObjClass.Dashboard)
                Type = ChoiceInfo.ChoiceType.Dashboard;
            else if (o.Type == ObjClass.WorkWithDevices)
                Type = ChoiceInfo.ChoiceType.WorkWithDevices;
            else if (o.Type == ObjClassLsi.Module)
                Type = ChoiceInfo.ChoiceType.Module;
            else if (o.Type == ObjClass.Table)
                Type = ChoiceInfo.ChoiceType.None;
            else if(o.Type == ObjClass.SDT)
			{
                Type = ChoiceInfo.ChoiceType.SDT;
                _DataType = new DataTypeInfo(eDBType.GX_SDT, o.Key, o.Name, false);
			}
            else
            {
#if DEBUG
                throw new Exception("Unknown choice type for KbObject " + o.Name);
#else
                Type = ChoiceInfo.ChoiceType.Control;
#endif
            }

            if (_DataType == null)
                // Empty data type
                _DataType = DataTypeInfo.NoType;
        }

    }
}
