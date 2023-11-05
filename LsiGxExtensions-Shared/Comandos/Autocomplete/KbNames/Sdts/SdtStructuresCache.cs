using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Objects.Sdt;
using Artech.Udm.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames.Sdts
{
	/// <summary>
	/// Cache with SDT data types
	/// </summary>
	public class SdtStructuresCache
	{

		Dictionary<EntityKey, SdtNodeNameInfo> LevelsByKey = new Dictionary<EntityKey, SdtNodeNameInfo>();
		Dictionary<EntityKey, SdtStructureInfo> SdtsBySdtKey = new Dictionary<EntityKey, SdtStructureInfo>();

		/// <summary>
		/// Get info about a SDT level with it's own type. If SDT was not in cache, it will be loaded
		/// </summary>
		/// <param name="model">KB model</param>
		/// <param name="levelId">SDT level type id</param>
		/// <returns>SDT level info. null if was not found</returns>
		public SdtNodeNameInfo GetNodeInfo(KBModel model, EntityKey levelId)
		{
			if (levelId.Type == ObjClass.SDT)
				// It's a reference to an entire SDT, not to a SDT level
				return GetSdtRoot(model, levelId);

			SdtNodeNameInfo level;
			if(!LevelsByKey.TryGetValue(levelId, out level))
			{
				SDT sdt = SDTLoader.GetParent(model, levelId);
				if (sdt == null)
					// SDT not found in kb
					return null;
				if (SdtsBySdtKey.ContainsKey(sdt.Key))
					// SDT already loaded, but level not found:
					return null;
				AddNonExistentSdt(sdt);

				// Try again
				LevelsByKey.TryGetValue(levelId, out level);
			}
			return level;
		}

		public SdtStructureInfo GetSdtInfo(KBModel model, EntityKey sdtKey, bool addToCacheIfNotExists = true)
		{
			SdtStructureInfo sdtInfo;
			if (!SdtsBySdtKey.TryGetValue(sdtKey, out sdtInfo))
			{
				if (!addToCacheIfNotExists)
					return null;

				SDT sdt = SDT.Get(model, sdtKey) as SDT;
				if (sdt == null)
					return null;
				sdtInfo = AddNonExistentSdt(sdt);
			}
			return sdtInfo;
		}

		private SdtStructureInfo AddNonExistentSdt(SDT sdt)
		{
			var sdtInfo = new SdtStructureInfo(sdt);
			SdtsBySdtKey[sdt.Key] = sdtInfo;
			foreach (var node in sdtInfo.Root.TraverseLevels())
				LevelsByKey[node.LevelKey] = node;
			return sdtInfo;
		}

		SdtNodeNameInfo GetSdtRoot(KBModel model, EntityKey sdtId)
		{
			return GetSdtInfo(model, sdtId)?.Root;
		}

		/// <summary>
		/// Remove sdt from this cache. If it does not exists, it does nothing
		/// </summary>
		/// <param name="sdt">SDT to remove</param>
		public void RemoveSdt(SDT sdt)
		{
			SdtStructureInfo sdtInfo;
			if (!SdtsBySdtKey.TryGetValue(sdt.Key, out sdtInfo))
				return;
			SdtsBySdtKey.Remove(sdt.Key);

			foreach (var node in sdtInfo.Root.TraverseLevels())
				LevelsByKey.Remove(node.LevelKey);
		}

		/// <summary>
		/// Add SDT to the cache. If sdt already exists, it does nothing
		/// </summary>
		/// <param name="sdt">SDT to add</param>
		public void AddSdt(SDT sdt)
		{
			if (SdtsBySdtKey.ContainsKey(sdt.Key))
				// Already exists
				return;
			AddNonExistentSdt(sdt);
		}

		/// <summary>
		/// Adds or updates and SDT in cache
		/// </summary>
		/// <param name="o">SDT to update. If it's not a SDT, it does nothing</param>
		/// <returns>True if cache has been updated</returns>
		public bool AddOrUpdate(KBObject o)
		{
			SDT sdt = o as SDT;
			if (sdt == null)
				return false;

			RemoveSdt(sdt);
			AddNonExistentSdt(sdt);
			return true;
		}

		public void FixTimestamp(SDT sdt)
		{
			SdtStructureInfo sdtInfo = GetSdtInfo(null, sdt.Key, false);
			if (sdtInfo == null)
				return;
			sdtInfo.Timestamp = sdt.Timestamp;
		}
	}
}
