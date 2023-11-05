using Artech.Architecture.Common.Objects;
using Artech.Architecture.Datatypes;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Language.Datatypes;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Parser.PEMDefinitions;
using Artech.Genexus.Common.Types;
using LSI.Packages.Extensiones.Comandos.Autocomplete.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition;
using Artech.Architecture.Language;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common.Parts.Form;
using LSI.Packages.Extensiones.Utilidades;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor
{
    /// <summary>
    /// Stores general information about Genexus keywords
    /// </summary>
    public class KbPredictorInfo
    {

        internal ObjectNamesCache ObjectNames;

        /// <summary>
        /// Entire set of available standard members
        /// </summary>
        internal HashSet<string> StandardMemberNames = new HashSet<string>();

        /// <summary>
        /// Entire set of available language word keywords
        /// </summary>
        internal HashSet<string> LanguageKeywords = new HashSet<string>();

        /// <summary>
        /// Words for native code commands ("SQL", "CSHARP",...) lowercase
        /// </summary>
        internal HashSet<string> NativeCodeCommands = new HashSet<string>();

        /// <summary>
        /// Model data definition.
        /// </summary>
        internal DataInfo DataInfo;

        public KbPredictorInfo(ObjectNamesCache objectNames, DataInfo dataInfo)
        {
            ObjectNames = objectNames;
            DataInfo = dataInfo;
            SetupLanguageWords();
        }

        private void AddPemsToStandardMembers(IEnumerable<IMemberInfo> pems)
        {
            foreach (IMemberInfo member in pems)
                StandardMemberNames.Add(member.Name.ToLower());
        }

        private void AddTypeMembers(ITypedObjectInfo typeInfo)
        {
            AddPemsToStandardMembers(typeInfo.GetPEMs());
        }

        private void AddBasicTypeMembers(KBModel model, DatatypesManager manager, DataTypeProvider dtProvider, BasicDataTypeInfo basicType, bool collection)
        {
            CompositeClassDefinitions composite = new CompositeClassDefinitions();
            AttCustomType customType = new AttCustomType(basicType.TypeId, (int)basicType.Type, basicType.Namespace, basicType.Name);

            composite.Add(dtProvider.GetObjectInfo(model, customType, collection));

            // Add variable members (I'm not sure about this, but, otherwise, ToFormattedString is not included)
            composite.Add(manager.GetClassByTypeId(248));

            // Add attribute members (.GetOldValue)
            composite.Add(manager.GetClassByTypeId(253));

            // SD objects (.CallOptions)
            composite.Add(manager.GetClassByTypeId(245));

            // TODO: Images ???
            // 73676603-94E6-48d8-A3BA-6DDC10A4B2E4

            // Genexus "programs" (procedures, workpanels, etc -> ".call()")
            composite.Add(manager.GetClassByTypeId(250));

            // Add enumerated domains members ("EnumeratedDescription()")
            composite.Add(manager.GetClass(new Guid("91546526-A0E4-4d47-A801-3E06473D450A")));

            AddTypeMembers(composite);
        }

        /// <summary>
        /// Add WinForm control names as standard members for winforms
        /// <param name="dtProvider">Language info types provider</param>
        /// </summary>
        private void AddControlMemberNamesWinforms(DataTypeProvider dtProvider)
        {
            WorkPanel fakeWp = new WorkPanel(UIServices.KB.CurrentModel);
            foreach (RuntimeControlType ctlType in Enum.GetValues(typeof(RuntimeControlType)))
            {
                try
                {
                    // This seems to return only properties ???
                    FormElement fakeControl = FormFactory.CreateFormElement(FormType.Windows, ctlType, fakeWp);
                    AddPemsToStandardMembers(fakeControl.GetPEMsByType(PEMType.All, PEMModifier.Common, PEMFlags.All));
                }
                catch
                { }

                // And this seems to return only members ???
                ITypedObjectInfo info = dtProvider.GetControlType(fakeWp, ctlType);
                if (info != null)
                    AddTypeMembers(info);
            }
        }

        /// <summary>
        /// Add form control names as standard members
        /// </summary>
        /// <param name="dtProvider">Language info types provider</param>
        /// <param name="o">Fake object needed to get members info</param>
        /*private void AddControlMemberNames(DataTypeProvider dtProvider, KBObject o)
        {
            foreach(RuntimeControlType ctlType in Enum.GetValues(typeof(RuntimeControlType)))
            {
                ITypedObjectInfo info = dtProvider.GetControlType(o, ctlType);
                if (info == null)
                    continue;
                AddTypeMembers(info);
            }
        }*/

        private void SetupLanguageWords()
        {
            // Add extended types members
            KBModel model = UIServices.KB.CurrentModel;
            DatatypesManager manager = DatatypesManager.GetManager(model);
            foreach (ITypedObjectInfo type in manager.GetPublicTypes())
                AddTypeMembers(type);

            // Add base types members
            DataTypeProvider dtProvider = new DataTypeProvider(model);
            bool first = true;
            foreach (BasicDataTypeInfo basicType in BasicDataTypeInfo.Types(UIServices.KB.CurrentModel).Values)
            {
                AddBasicTypeMembers(model, manager, dtProvider, basicType, false);
                if (first)
                {
                    AddBasicTypeMembers(model, manager, dtProvider, basicType, true);
                    first = false;
                }
            }

            // Language keywords
            LanguageKeywords = CommandsAutocomplete.KeywordsLowercase();
            // Add non-handled keywords:
            LanguageKeywords.Add("status"); // option for msg
            LanguageKeywords.Add("nowait"); // option for msg
            LanguageKeywords.Add("y"); // option for confirm
            LanguageKeywords.Add("n"); // option for confirm
            LanguageKeywords.Add("vb"); // Deprecated (Visual basic)
            AddEventNamesAsKeywords();

            // Native commands
            foreach (string cmd in KeywordGx.NATIVECODEKEYWORDS)
                NativeCodeCommands.Add(cmd);

            // Add UI control members as standard members
            AddControlMemberNamesWinforms(dtProvider);
            // This does not work. So web control properties are not supported
            // ¯\_(ツ)_/¯
            //AddControlMemberNames(dtProvider, new WebPanel(UIServices.KB.CurrentModel));
        }

        private void AddEventNamesAsKeywords()
        {
            ILanguageManager lang = DataTypeProvider.GetProvider(UIServices.KB.CurrentModel).LanguageManager;

            // Select object types with events parts
            IEnumerable<Guid> objectTypesWithEvents = DataInfo.SupportedPartTypes
                .Where(x => x.PartType == PartType.Events)
                .Select(x => x.ObjectType)
                .Distinct();
            
            foreach(Guid objectType in objectTypesWithEvents)
            {
                foreach(string eventName in lang.GetStandardEvents(objectType))
                {
                    // There are event names with more than one word "After Trn":
                    foreach(string word in eventName.Split(new char[] {  ' ' } , StringSplitOptions.RemoveEmptyEntries ))
                        LanguageKeywords.Add(word.ToLower());
                }
            }
            
        }

    }
}
