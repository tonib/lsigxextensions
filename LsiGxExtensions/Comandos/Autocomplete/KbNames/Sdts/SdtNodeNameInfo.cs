using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Language;
using Artech.Common.Helpers.Structure;
using Artech.Common.Properties;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.SDT;
using Artech.Udm.Framework;
using LSI.Packages.Extensiones.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames.Sdts
{
	public class SdtNodeNameInfo : TypedNameInfo
	{

		/// <summary>
		/// If sdt level has it's own data type, this is the data type identifier. Otherwise is null
		/// </summary>
		public EntityKey LevelKey;

		/// <summary>
		/// Structure node children
		/// </summary>
		List<SdtNodeNameInfo> Children = new List<SdtNodeNameInfo>();

		/// <summary>
		/// Children indexed by name, LOWERCASE
		/// </summary>
		Dictionary<string, SdtNodeNameInfo> ChildrenByName = new Dictionary<string, SdtNodeNameInfo>();

		public SdtNodeNameInfo() { }

		// TODO: This MUST to have to be implemented somewhere, I cannot find where
		string GetLevelTypeName(SDTLevel level)
		{
			string fullRealTypeName;
			if (level == level.SDTStructure.Root)
			{
				// Root level
				fullRealTypeName = level.Name;
			}
			else
			{
				// Traverse level parents, until root
				SDTStructurePart structure = level.SDT.SDTStructure;
				fullRealTypeName = level.CollectionItemName;
				while (true)
				{
					if (level == null)
						break;

					if (level.Parent == structure.Root)
					{
						// Finished
						fullRealTypeName = level.SDT.Name + "." + fullRealTypeName;
						break;
					}
					// Add parent name
					level = level.Parent;
					fullRealTypeName = level.IsCollection ? level.CollectionItemName : level.Name + "." + fullRealTypeName;
				}
			}

			// Add now the fucked module, I guess. With that twisted format
			Module module = level.SDT.Module;
			if (!module.IsRoot)
				fullRealTypeName += $", {module.QualifiedName.ToString()}";
			return fullRealTypeName;
		}

		public SdtNodeNameInfo(IStructureItem sdtStructureItem)
		{
			Name = sdtStructureItem.Name;
			_Description = (sdtStructureItem as IDescriptive)?.Description ?? Name;
			Type = ChoiceInfo.ChoiceType.SDT;

			SDTLevel sdtLevel = sdtStructureItem as SDTLevel;
			if (sdtLevel != null)
			{
				if (sdtLevel.IsCollection || sdtLevel.SDTStructure.Root == sdtLevel)
				{
					// Level has its own data type
					_DataType = new DataTypeInfo(eDBType.GX_SDT, sdtLevel.ItemEntity.Key, GetLevelTypeName(sdtLevel),
						sdtLevel.IsCollection);
					LevelKey = sdtLevel.ItemEntity.Key;
				}
				else
				{
					// Level no collection: A non instatiable level, it has no type
					_DataType = DataTypeInfo.NoType;
				}
				return;
			}
			else
			{
				// It should be a SDT leaf
				SDTItem sdtItem = (SDTItem)sdtStructureItem;
				_DataType = new DataTypeInfo(sdtItem);
			}
		}

		/// <summary>
		/// Traverse all SDT nodes in-order
		/// </summary>
		/// <returns>SDT nodes</returns>
		public IEnumerable<SdtNodeNameInfo> Traverse()
		{
			yield return this;
			foreach(var node in Children)
			{
				foreach (var descendant in node.Traverse())
					yield return descendant;
			}
		}

		/// <summary>
		/// Traverse level nodes in SDT with its own type (includes root node)
		/// </summary>
		/// <returns>SDT level</returns>
		public IEnumerable<SdtNodeNameInfo> TraverseLevels()
		{
			return Traverse().Where(n => n.LevelKey != null);
		}

		public void AddChild(SdtNodeNameInfo node)
		{
			Children.Add(node);
			string nameLowercase = node.Name.ToLower();
			// This should not happen, but...
			ChildrenByName[nameLowercase] = node;
		}

		/// <summary>
		/// Get a child name for this node
		/// </summary>
		/// <param name="name">Child name, any case</param>
		/// <param name="model">Current model</param>
		/// <param name="namesCache">Names cache, to store refences to any new SDT</param>
		/// <returns>Child reference. Null name was not found</returns>
		public SdtNodeNameInfo GetChildAnyCase(string name, KBModel model, ObjectNamesCache namesCache)
		{
			return GetChildLowercase(name.ToLower(), model, namesCache);
		}

		/// <summary>
		/// Get a child name for this node
		/// </summary>
		/// <param name="name">Child name, lowercase</param>
		/// <param name="model">Current model</param>
		/// <param name="namesCache">Names cache, to store refences to any new SDT</param>
		/// <returns>Child reference. Null name was not found</returns>
		public SdtNodeNameInfo GetChildLowercase(string nameLowercase, KBModel model, ObjectNamesCache namesCache)
		{
			// If last children was a SDT reference, go to that SDT, do not continue with current SDT
			SdtNodeNameInfo sdtLevel;
			if (DataType.ExtendedType != null && DataType.ExtendedType != LevelKey)
			{
				// Reference to other SDT
				sdtLevel = namesCache.SdtStructuresCache.GetNodeInfo(model, DataType.ExtendedType);
				if (sdtLevel == null)
					return null;
			}
			else
				// Reference in this sdt
				sdtLevel = this;

			SdtNodeNameInfo child;
			sdtLevel.ChildrenByName.TryGetValue(nameLowercase, out child);
			return child;
		}
	}
}
