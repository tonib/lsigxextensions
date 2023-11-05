using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Udm.Framework;
using Artech.Udm.Framework.References;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames.Tables
{
	/// <summary>
	/// Cache of tables structure
	/// </summary>
	public class TablesCache
	{

		/// <summary>
		/// Tables by its key. Key = table key, value = table info.
		/// </summary>
		public Dictionary<EntityKey, TableName> TablesByTableId = new Dictionary<EntityKey, TableName>();


		/// <summary>
		/// Tables where an attribute is contained. Key = attribue key, value = list of tables where attribute is contained
		/// </summary>
		public Dictionary<EntityKey, List<TableName>> TablesByAttribute = new Dictionary<EntityKey, List<TableName>>();

		/// <summary>
		/// Creates the initial tables cache
		/// </summary>
		/// <param name="model">Current model</param>
		/// <param name="setupAborted">Cache setup has been aborte?</param>
		public TablesCache(KBModel model, ref bool setupAborted)
		{
			// Setup tables
			foreach(Table t in Table.GetAll(model))
			{
				TablesByTableId[t.Key] = new TableName(t);
				if (setupAborted)
					return;
			}

			// Setup references from tables to attributes
			IEnumerable<EntityReference> refsTableToAttributes = model.GetReferences(LinkType.UsedObject, new Guid[] { ObjClass.Table }, new Guid[] { ObjClass.Attribute });
			foreach(EntityReference reference in refsTableToAttributes)
			{
				// Find referrer table
				TableName tableInfo;
				if (TablesByTableId.TryGetValue(reference.From, out tableInfo))
				{
					// Add atribute to table
					tableInfo.Attributes.Add(reference.To);

					// Get referrer tables to this attribute
					List<TableName> attTables;
					if(!TablesByAttribute.TryGetValue(reference.To, out attTables))
					{
						// New attribute: Add referrer tables list to this attribute
						attTables = new List<TableName>();
						TablesByAttribute.Add(reference.To, attTables);
					}
					// Add referrer table
					attTables.Add(tableInfo);
				}

				if (setupAborted)
					return;
			}
		}

		void RemoveTable(Table t)
		{
			TableName tableInfo;
			if (TablesByTableId.TryGetValue(t.Key, out tableInfo))
			{
				// Remove table from referrer attributes
				foreach(EntityKey attribute in tableInfo.Attributes)
				{
					List<TableName> attTables;
					if(TablesByAttribute.TryGetValue(attribute, out attTables))
						attTables.Remove(tableInfo);
				}

				// Remove table
				TablesByTableId.Remove(t.Key);
			}
		}

		void AddTable(Table t)
		{
			// Add table
			var tableName = new TableName(t);
			TablesByTableId[t.Key] = tableName;

			// Add table references
			IEnumerable<EntityReference> tableRefsTo = t.GetReferencesTo(LinkType.UsedObject).Where(r => r.To.Type == ObjClass.Attribute);
			foreach(EntityReference r in tableRefsTo)
				tableName.Attributes.Add(r.To);
		}

		public bool AddOrUpdate(KBObject o)
		{
			Table table = o as Table;
			if (table == null)
				return false;

			RemoveTable(table);
			AddTable(table);

			return true;
		}
	}
}
