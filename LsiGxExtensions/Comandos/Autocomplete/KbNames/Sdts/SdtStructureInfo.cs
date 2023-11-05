using Artech.Common.Helpers.Structure;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts.SDT;
using System;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames.Sdts
{
	/// <summary>
	/// Information about an SDT structure
	/// </summary>
	public class SdtStructureInfo : ITimestamped
	{

		/// <summary>
		/// SDT timestamp
		/// </summary>
		public DateTime Timestamp { get; set; }

		public SdtNodeNameInfo Root;

		public SdtStructureInfo(SDT sdt)
		{
			Timestamp = sdt.Timestamp;
			Root = TraverseSdtNote(sdt.SDTStructure.Root);
		}

		SdtNodeNameInfo TraverseSdtNote(SDTLevel level)
		{
			var nameNode = new SdtNodeNameInfo(level);

			foreach(IStructureItem subItem in level.Items)
			{
				if (subItem.IsLeafItem)
					nameNode.AddChild(new SdtNodeNameInfo(subItem));
				else
					nameNode.AddChild(TraverseSdtNote((SDTLevel)subItem));
			}
			return nameNode;
		}

	}
}
