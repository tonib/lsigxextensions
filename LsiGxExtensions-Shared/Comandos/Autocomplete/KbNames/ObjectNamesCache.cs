using Artech.Architecture.Common.Objects;
using Artech.Architecture.Datatypes;
using Artech.Architecture.Language;
using Artech.Architecture.UI.Framework.Language;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Types;
using Artech.Udm.Framework;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames.Sdts;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames.Tables;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames
{

    /// <summary>
    ///  Stores current kb kbobject names
    /// </summary>
    public class ObjectNamesCache
    {

        /// <summary>
        /// Stores SDT structure information
        /// </summary>
        SdtStructuresCache _SdtStructuresCache = new SdtStructuresCache();

        /// <summary>
        /// Stores SDT structure information
        /// </summary>
        public SdtStructuresCache SdtStructuresCache
		{
			get
			{
                if (Ready)
                    UpdateCache();
                return _SdtStructuresCache;
			}
		}

        /// <summary>
        /// Tables structure cache
        /// </summary>
        public TablesCache TablesCache;

        /// <summary>
        ///  Object names, except Attributes, grouped by Gx namespace. Key is the object lowercase name
        /// </summary>
        private NamespaceNames ObjectNames = new NamespaceNames();

		/// <summary>
		///  Attributes names. Key is the object lowercase name
		/// </summary>
		private Trie<ObjectNameInfo> AttributesNames = new Trie<ObjectNameInfo>();

        /// <summary>
		///  SDTs names. Key is the object lowercase name
		/// </summary>
		private Trie<KbObjectNameInfo> SdtNames = new Trie<KbObjectNameInfo>();

        /// <summary>
        /// Names by object key
        /// </summary>
        private Dictionary<EntityKey, ObjectNameInfo> NamesByKey = new Dictionary<EntityKey, ObjectNameInfo>();

        /// <summary>
        /// Stores names, hashed by name text. It can be null, if prediction is disabled
        /// </summary>
        public HashedNames HashedNames;

        /// <summary>
        /// Thread to load names in background
        /// </summary>
        private Thread NamesLoaderThread = null;

        /// <summary>
        /// Cache setup process (load all kb names) has been aborted?
        /// </summary>
        private bool SetupAborted;

        /// <summary>
        /// All KB names already loaded?. Its true if process has finished, successfully or with errors (see SetupFailed)
        /// </summary>
        public bool Ready;

        /// <summary>
        /// Setup process has finished with errors?
        /// </summary>
        public bool SetupFailed;

        /// <summary>
        /// Delegate to call after cache is loaded
        /// </summary>
        public delegate void OnReadyEvent(ObjectNamesCache cache);

        /// <summary>
        /// Event to call after cache is loaded
        /// </summary>
        public event OnReadyEvent OnReady;

        /// <summary>
        /// Last time names cache was updated
        /// </summary>
        private DateTime LastCacheUpdate;

        /// <summary>
        /// Object types to store in cache. USE CallableTypesToStore property instead this
        /// </summary>
        static private HashSet<Guid> _CallableTypesToStore;

        /// <summary>
        /// Object types to store in cache
        /// </summary>
        static private HashSet<Guid> CallableTypesToStore
        {
            get
            {
                if (_CallableTypesToStore == null)
                {
                    // There is no ObjClass.Module
                    // TODO: Check other SD object types (Dashboards, more?)
                    // !!! If some class of object is added here, add it too in KbObjectNameInfo constructor !!!!
                    _CallableTypesToStore = new HashSet<Guid>( new Guid[] { ObjClass.DataSelector , ObjClass.Domain , ObjClass.ExternalObject ,
                        ObjClass.Procedure ,
                        ObjClass.Transaction , ObjClass.WebPanel , ObjClass.WorkPanel , ObjClass.Image ,
                        ObjClass.DataProvider , ObjClass.SDPanel , ObjClass.Dashboard , ObjClass.WorkWithDevices ,
                        ObjClassLsi.Module
                    } );
                }
                return _CallableTypesToStore;
            }
        }

		private void AddNonDeclaredRules()
		{
			// Deprecated / not declared transaction rules
			Trie<ObjectNameInfo> trie = ObjectNames.GetTrie(NamespaceNames.KEYWORDSNAMESPACE);
			string[] rulesNames = new string[] { "AllowNulls", "NoCheck" };
			foreach(string ruleName in rulesNames)
				trie.Add(ruleName.ToLower(), new FunctionNameInfo(ruleName, ObjectPartType.TransactionRules, true));

			FunctionNameInfo f = new FunctionNameInfo("Hidden", ObjectPartType.WorkPanelRules, true);
			f.PartTypes.Add(ObjectPartType.WebPanelRules);
			trie.Add("hidden", f);
		}

		/// <summary>
		/// Add function/rules names for a part type
		/// </summary>
		/// <param name="partType">Part type for functions</param>
		private void AddFunctionNames(ObjectPartType partType)
        {
            try
			{
				Trie<ObjectNameInfo> trie = ObjectNames.GetTrie(NamespaceNames.KEYWORDSNAMESPACE);
				ILanguageManager lang = DataTypeProvider.GetProvider(UIServices.KB.CurrentModel).LanguageManager;

				// Add function names
				AddFunctionNames(partType, trie, lang.GetFunctions(partType.ObjectType, partType.PartType, MemberStatus.Portable), false);

				// Add rules names
				AddFunctionNames(partType, trie, lang.GetRules(partType.ObjectType, partType.PartType, MemberStatus.Portable), true);
			}
			catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

		private static void AddFunctionNames(ObjectPartType partType, Trie<ObjectNameInfo> trie, IEnumerable<IMethodInfo> functions, bool isRule)
		{
			foreach (IMethodInfo function in functions)
			{
				string name = function.Name.ToLower();
				FunctionNameInfo fName = trie.GetValueByExactName(name) as FunctionNameInfo;
				if (fName == null)
					trie.Add(name, new FunctionNameInfo(function, partType, isRule));
				else
					fName.PartTypes.Add(partType);
			}
		}

		private void AddNamespaces()
		{
			try
			{
				Trie<ObjectNameInfo> trie = ObjectNames.GetTrie(NamespaceNames.KEYWORDSNAMESPACE);
				ILanguageManager lang = DataTypeProvider.GetProvider(UIServices.KB.CurrentModel).LanguageManager;
				foreach (string nameSpace in lang.GetNamespaces(UIServices.KB.CurrentModel))
				{
					string name = nameSpace.ToLower();
					ObjectNameInfo nsName = trie.GetValueByExactName(name);
					if (nsName == null)
						trie.Add(name, new ObjectNameInfo(nameSpace, nameSpace, ChoiceInfo.ChoiceType.NameSpace));
				}
			}
			catch (Exception ex)
			{
				Log.ShowException(ex);
			}
		}

		private void AddKbObjectNames(IEnumerable<KBObject> objects)
        {
            if (SetupAborted)
                return;

            foreach (KBObject o in objects)
            {
                AddOrUpdate(o, checkObjectType:false, update:false);
                if (SetupAborted)
                    return;
            }
        }

		/// <summary>
		/// Add or update an object in cache
		/// </summary>
		/// <param name="o">Object to add or update</param>
		/// <param name="checkObjectType">True if we should check if the object is of type to store in cache (callable objects,etc).
		/// If true, and object type should not be cached, this will do nothing</param>
		/// <param name="update">If true and object is in cache, it will be removed and added (update). If false and object
		/// is in cache it will throw an excepcion</param>
        /// <param name="loadLazyObjectInfo">True if object is completly loaded in memory, and we can perform costly object inspections</param>
        public void AddOrUpdate(KBObject o, bool checkObjectType = true, bool update=true, bool loadLazyObjectInfo = false)
        {
            try
            {
                // TablesCache is null up to its construction
                // TODO: Ev3: This seems to be never called for tables, even if you change the table structure in transaction. I don't know why
                // TODO: ¯\_(ツ)_/¯
                // TODO: Try to update tables info when a transaction is updated ???
                if (TablesCache != null && TablesCache.AddOrUpdate(o))
                    // Object was a Table
                    return;

                // Add domains only if they contain enumerated values
                Domain d = o as Domain;
                if (d != null && !d.IsEnumerated)
                    return;

                SDT sdt = o as SDT;
                if (sdt != null)
                {
                    // SDT added or updated
                    if (loadLazyObjectInfo)
                    {
                        // SDT is completly loaded in memory: Add it to the sdt structures cache
                        _SdtStructuresCache.AddOrUpdate(sdt);
                    }
                    else if (update)
                    {
                        // SDT modified, but not completly loaded in memory. Remove it from cache, if it was there
                        _SdtStructuresCache.RemoveSdt(sdt);
                    }
                }

                ObjectNameInfo name;

				if (update)
				{
					if (NamesByKey.TryGetValue(o.Key, out name))
						// Object already added: Removed with its current name (it may be changed) and re-add
						RemoveObjectName(o, name.Name);
				}

                name = AddObjectToTrie(o, checkObjectType);
                if (name == null)
                    return;

                NamesByKey.Add(o.Key, name);

                if (HashedNames != null && name.Type != ChoiceInfo.ChoiceType.SDT)
                    HashedNames.Add(name);
            }
            catch(Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        private ObjectNameInfo AddObjectToTrie(KBObject o, bool checkObjectType)
		{
            Artech.Genexus.Common.Objects.Attribute a = o as Artech.Genexus.Common.Objects.Attribute;
            if (a != null)
            {
                AttributeNameInfo name = new AttributeNameInfo(a);
                AttributesNames.Add(o.Name.ToLower(), name);
                return name;
            }

            SDT sdt = o as SDT;
            if(sdt != null)
			{
                KbObjectNameInfo sdtName = new KbObjectNameInfo(o);
                SdtNames.Add(o.Name.ToLower(), sdtName);
                return sdtName;
			}
            
            if (checkObjectType && !CallableTypesToStore.Contains(o.Type))
                // Object type not to store
                return null;
            return ObjectNames.Add(o);
        }

        private void RemoveObjectName(KBObject o, string name)
        {
            string nameToRemove = name.ToLower();

            ObjectNameInfo objectName;
            if (o is Artech.Genexus.Common.Objects.Attribute)
                objectName = AttributesNames.Remove(nameToRemove);
            else if (o is SDT)
                objectName = SdtNames.Remove(nameToRemove);
            else
                objectName = ObjectNames.Remove(o, nameToRemove);

            NamesByKey.Remove(o.Key);

            if (HashedNames != null && objectName != null)
                HashedNames.Remove(objectName);
        }

        public void RemoveObject(KBObject o)
        {
            SDT sdt = o as SDT;
            if(sdt != null)
                _SdtStructuresCache.RemoveSdt(sdt);

			// Remove with its currently stored name (it may be changed)
			ObjectNameInfo name;
			if (!NamesByKey.TryGetValue(o.Key, out name))
				return;

            RemoveObjectName(o, name.Name);
        }

        public void AbortSetup()
        {
            SetupAborted = true;
        }

        public void Setup()
        {
            
            using (Log log = new Log())
            {
                try
                {
                    Ready = false;
                    SetupAborted = false;
                    SetupFailed = false;
                    log.StartTimeCount();
                    // TODO: I suspect these Clear() are not needed
                    ObjectNames.Clear();
                    AttributesNames.Clear();
                    LastCacheUpdate = DateTime.UtcNow;

                    log.Output.AddLine("Caching KB object names for autocomplete function");

                    KBModel model = UIServices.KB.CurrentModel;
                    if (model == null)
                    {
                        // This happens when creating a new KB, and I dont' find any way to wait to the kb creation process ends
                        // So, abort the cache setup and wait to the first key typed in any editor
                        log.Output.AddWarningLine("Not in KB, cache setup aborted. If you are creating a new KB, this error is expected and this process will be executed later");
                        SetupFailed = true;
                        return;
                    }

					// Add attributes
                    AddKbObjectNames(Artech.Genexus.Common.Objects.Attribute.GetAll(model).Cast<KBObject>());

                    // Add SDTs
                    AddKbObjectNames(SDT.GetAll(model).Cast<KBObject>());

                    // Store tables structure
                    TablesCache = new TablesCache(model, ref SetupAborted);

					// Add callable object types
					foreach (Guid objectType in CallableTypesToStore)
					{
						IEnumerable<KBObject> objects = model.Objects.GetAll(objectType);
						// Exception: Do not add "Root module" module (not referenceable from code editor)
						if (objectType == ObjClassLsi.Module)
						{
							Guid rootModuleId = Module.GetRoot(model).Guid;
							objects = objects.Where(x => x.Guid != rootModuleId);
						}

						AddKbObjectNames(objects);
					}

					if (!SetupAborted)
						AddNamespaces();

					if (!SetupAborted)
                    {
						// Add functions/rules
						foreach (ObjectPartType pType in Autocomplete.SupportedPartTypes)
							AddFunctionNames(pType);

						// Deprecated / not declared rules
						AddNonDeclaredRules();

						log.Output.AddLine(ObjectNames.Count + " object/keyword names");
                        log.Output.AddLine(AttributesNames.Count + " attributes");
                        log.Output.AddLine(TablesCache.TablesByTableId.Keys.Count + " tables");
                        log.Output.AddLine(SdtNames.Count + " SDTs");
                    }
                    if(!SetupAborted)
                        Ready = true;
                }
                catch(Exception ex)
                {
                    SetupFailed = true;
                    Ready = false;
                    Log.ShowException(ex);
                }
                finally
                {
                    if (SetupAborted)
                        SetupFailed = true;
                    NamesLoaderThread = null;
                    OnReady?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Launches a thread to setup this cache. 
        /// this.Ready is set to true when the thread ends, and OnReady is fired
        /// </summary>
        public void SetupInThread()
        {
            NamesLoaderThread = new Thread(this.Setup);
            NamesLoaderThread.Start();
        }

        /// <summary>
        /// Return object names (everything except Attributes) starting with a given prefix
        /// </summary>
        /// <param name="prefixLowercase">Prefix of names to return, lowercase</param>
        /// <param name="nNamesPerNamespace">Number of names to return for each namespace. If ==0, all names
        /// will be returned</param>
        /// <returns>Names starting with the given prefix</returns>
        public IEnumerable<ObjectNameInfo> GetObjectsByPrefix(string prefixLowercase, int nNamesPerNamespace = 0)
        {
            UpdateCache();

            prefixLowercase = prefixLowercase.ToLower();
            return ObjectNames.GetValuesByPrefix(prefixLowercase, nNamesPerNamespace);
        }

        // TODO: This should return IEnumerable<AttributeNameInfo> ???
        public IEnumerable<ObjectNameInfo> GetAttributesByPrefix(string prefixLowercase)
        {
            UpdateCache();

            // TODO: WTF, lowercase or not ???
            prefixLowercase = prefixLowercase.ToLower();
            return AttributesNames.GetValuesByPrefix(prefixLowercase);
        }

        /// <summary>
        /// Return SDT names starting with a given prefix
        /// </summary>
        /// <param name="prefixLowercase">Prefix of names to return, lowercase</param>
        /// <returns>Names starting with the given prefix</returns>
        public IEnumerable<KbObjectNameInfo> GetSdtsByPrefix(string prefixLowercase)
        {
            UpdateCache();
            return SdtNames.GetValuesByPrefix(prefixLowercase);
        }

        /// <summary>
        /// Get all callable, attribute and enumerated domain names, by its name
        /// </summary>
        /// <param name="nameLowercase">Object name, lowercase</param>
        /// <returns>List of matching names</returns>
        public List<ObjectNameInfo> GetAllByExactName(string nameLowercase)
        {
            UpdateCache();

            List<ObjectNameInfo> result = new List<ObjectNameInfo>();

            ObjectNameInfo a = AttributesNames.GetValueByExactName(nameLowercase);
            if (a != null)
                result.Add(a);

            result.AddRange(ObjectNames.GetAllByExactName(nameLowercase));
            return result;
        }

		/// <summary>
		/// Get an attribute by its name
		/// </summary>
		/// <param name="name">Attribute name, LOWERCASE</param>
		/// <returns>Attribute name info. null if it was not found</returns>
        public ObjectNameInfo GetAttributeByExactName(string name)
        {
            UpdateCache();

            return AttributesNames.GetValueByExactName(name);
        }

        public ObjectNameInfo GetNameByKey(EntityKey attKey)
        {
            UpdateCache();
            NamesByKey.TryGetValue(attKey, out ObjectNameInfo name);
            return name;
        }

        public void SetupHashedNames(HashedNames hashedNamesContainer)
        {
            HashedNames = hashedNamesContainer;
            HashedNames.AddRange(ObjectNames.GetValuesByPrefix(string.Empty));
            HashedNames.AddRange(AttributesNames.GetValuesByPrefix(string.Empty));
        }

        public void Clear()
        {
            if(NamesLoaderThread != null)
            {
                try
                {
                    AbortSetup();
                    NamesLoaderThread.Join(3000);
                }
                catch { }
            }
            NamesLoaderThread = null;
            ObjectNames = new NamespaceNames();
            HashedNames = null;
            Ready = false;
        }

        public void FixTimestamp(KBObject o)
		{
            // There is a problem with stored timestamps: We insert/update names it in Automplete.OnBeforeSaveKBObject, BEFORE object save.
            // I cannot remember why that is done before saving, I guess is because object content is still in memory, and maybe after save no.
            // Saved object timestamp will change AFTER object is saved. So, fix it here

            if (!Ready)
                return;

            SDT sdt = o as SDT;
            if(sdt != null)
                _SdtStructuresCache.FixTimestamp(sdt);

            if(NamesByKey.TryGetValue(o.Key, out ObjectNameInfo name))
			{
                ITimestamped timestamped = name as ITimestamped;
                if (timestamped != null)
                    timestamped.Timestamp = o.Timestamp;
            }
        }

        /// <summary>
        /// Update cache state with objects modified on other Gx instances (multiuser kb)
        /// </summary>
        private void UpdateCache()
        {
            try
            {
                // TODO: Use version number instead of timestamp to check if an object was modified. Timestamp has problems (see FixTimestamp())

                // Update cache each 30 seconds
                if (LastCacheUpdate == DateTime.MinValue || DateTime.UtcNow.Subtract(LastCacheUpdate).TotalSeconds < 30)
                    return;

                HashSet<Guid> callableObjects = CallableTypesToStore;

                // Get modified objects since last update, object types stored in cache
                KBModel model = UIServices.KB.CurrentModel;
                foreach (EntityKey key in model.Objects.GetKeys(LastCacheUpdate)
                    .Where(x => x.Type == ObjClass.Attribute || callableObjects.Contains(x.Type) || x.Type == ObjClass.SDT || x.Type == ObjClass.Table))
                {
                    KBObject o = model.Objects.Get(key);
                    if (o == null)
                        continue;

                    // Update attribute / callable / sdt names:
                    ObjectNameInfo name;
                    if (!NamesByKey.TryGetValue(o.Key, out name))
                    {
                        // New object created on other instance
                        AddOrUpdate(o);
                        continue;
                    }

                    DateTime timestamp;
                    if (name is ITimestamped)
                        timestamp = ((ITimestamped)name).Timestamp;
                    else
                        timestamp = DateTime.MinValue;
                    if (!ITimestampedEx.SameSecond(timestamp, o.Timestamp))
                    {
                        // Object changed on other instance
                        AddOrUpdate(o);
                    }
                    
                }

                LastCacheUpdate = DateTime.UtcNow;
            }
            catch(Exception ex)
            {
                Log.ShowException(ex);
            }
        }
    }

}
