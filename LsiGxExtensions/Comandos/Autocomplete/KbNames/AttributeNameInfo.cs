using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Genexus.Common.Objects;
using Artech.Architecture.UI.Framework.Language;
using LSI.Packages.Extensiones.Utilidades.Variables;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Udm.Framework;
using Artech.Architecture.UI.Framework.Services;
using LSI.Packages.Extensiones.Utilidades;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames
{

    // TODO: I suspect AttributeNameInfo should inherit from KbObjectNameInfo instead of TypedNameInfo (ObjectKey and Timestamp are duplicated)

    /// <summary>
    /// Stores information about an attribute
    /// </summary>
    class AttributeNameInfo : TypedNameInfo, ITimestamped
    {
        /// <summary>
        /// The object identifier
        /// </summary>
        public EntityKey ObjectKey;

        /// <summary>
        /// Object timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        public override DataTypeInfo DataType
        {
            get
            {
                LoadAttribute();
                return _DataType;
            }
        }

        public override string Description
        {
            get
            {
                LoadAttribute();
                return _Description;
            }
        }

        /// <summary>
        /// Attribute identifier
        /// </summary>
        private int AttributeId;

        public AttributeNameInfo(Artech.Genexus.Common.Objects.Attribute a, bool lazyLoad = true)
        {
            Name = a.Name;
            _Description = a.Description;
            Timestamp = a.Timestamp;
            ObjectKey = a.Key;

            Type = ChoiceInfo.ChoiceType.Attribute;

            // This is VERY slow. Do a lazy load
            //Description += " / " + VariableGX.TypeDescription(a, false);
            //DataType = a.Type;
            AttributeId = a.Id;

            if (!lazyLoad)
                SetAttributeInfo(a);
        }

        /// <summary>
        /// Do a lazy load of the attribute information. If it's already loaded, it does nothing
        /// </summary>
        internal void LoadAttribute()
        {
            if (_DataType != null)
                return;

            EntityKey key = new EntityKey(ObjClass.Attribute, AttributeId);
            Artech.Genexus.Common.Objects.Attribute a = Artech.Genexus.Common.Objects.Attribute.Get(
                UIServices.KB.CurrentModel, key) as Artech.Genexus.Common.Objects.Attribute;
            SetAttributeInfo(a);
        }

        private void SetAttributeInfo(Artech.Genexus.Common.Objects.Attribute a)
        {
            if (a == null)
                _DataType = DataTypeInfo.NoType;
            else
            {
                _DataType = new DataTypeInfo(a);
                _Description += " / " + VariableGX.TypeDescription(a, false);
            }
        }
    }
}
