using Artech.Common.Helpers.Structure;
using Artech.Genexus.Common.Parts.SDT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{
	static public class SDTLevelExtensions
	{
		static public IEnumerable<SDTItem> LsiEnumerateItems(this SDTLevel level)
		{
			return LsiEnumerateItemsLevels(level, true, false).Cast<SDTItem>();
		}

		static public IEnumerable<IStructureItem> LsiEnumerateItemsLevels(this SDTLevel level, bool includeItems=true, bool includeLevels=true)
		{
			if (includeItems)
			{
				foreach (SDTItem childItem in level.GetItems<SDTItem>())
					yield return childItem;
			}

			foreach (SDTLevel childLevel in level.GetItems<SDTLevel>())
			{
				if (includeLevels)
					yield return childLevel;

				// Traverse sublevel
				foreach (IStructureItem itm in LsiEnumerateItemsLevels(childLevel, includeItems, includeLevels))
					yield return itm;
			}
		}

	}
}
