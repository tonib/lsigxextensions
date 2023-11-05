using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Objects;
using Artech.Udm.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames.Tables
{
	/// <summary>
	/// Info about a table and its attributes
	/// </summary>
	public class TableName : KbObjectNameInfo
	{
		public TableName(Table o) : base(o) { }

		/// <summary>
		/// Attributes in table
		/// </summary>
		public HashSet<EntityKey> Attributes = new HashSet<EntityKey>();
	}
}
