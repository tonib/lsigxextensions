using ActiproSoftware.SyntaxEditor;
using Artech.Architecture.UI.Framework.Language;
using Artech.Genexus.Common.Parts.Layout;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete
{
    public class AutocompleteItem : IntelliPromptMemberListItem
    {
        /// <summary>
        /// Specifies the probabilitiy of this item. Higher -> most probable
        /// </summary>
        public double Priority;

        public ChoiceInfo.ChoiceType Type;

        /// <summary>
        /// Name info. It can be null
        /// </summary>
        public ObjectNameInfo Info;

        public AutocompleteItem(IntelliPromptMemberList memberList, string name, ChoiceInfo.ChoiceType type, 
            string description = null) : base(name, 0, description)
        {
            Type = type;

            // Get item icon
            int icon = memberList.ImageList.Images.IndexOfKey(Type.ToString());
            if (icon >= 0)
                ImageIndex = icon;
        }

        // TODO: I Think memberList parm can be removed, context.MemberList is the same
        public AutocompleteItem(IntelliPromptMemberList memberList, ObjectNameInfo info, AutocompleteContext context) 
            : this(memberList, info.Name, info.Type, info.Description)
		{
            Info = info;

			// Set default priority
			// this is right on procedures, but not in conditions: You can start a line with a function
			/*if ((info.Type == ChoiceInfo.ChoiceType.Function || info.Type == ChoiceInfo.ChoiceType.Domain)
                && lineParser.CompletedTokensCount == 0)
                // Functions / domains should not go at line start
                Priority -= 1;*/

			AddExtraAttributesInfo(info, context);

            // If it's a callable object, add parenthesis
            // Don't do it: Maybe we want to type .Link(). More if it's a function
            // and you close the autocomplete typing "(", parenthesis will duplicated
            /*KbObjectNameInfo kbName = info as KbObjectNameInfo;
            if(kbName != null)
			{
                if (ObjClassLsi.LsiIsCallableType(kbName.ObjectKey.Type))
                    Tag = IntelliSenseAction.AddParenthesesCursorBetween;
            }
            else if(info is FunctionNameInfo)
                Tag = IntelliSenseAction.AddParenthesesCursorBetween;*/

            Priority += GetParameterProbability(info, context.CallFinder);
		}

		private void AddExtraAttributesInfo(ObjectNameInfo info, AutocompleteContext context)
		{
			AttributeNameInfo attName = info as AttributeNameInfo;
            if (attName == null)
                return;
			
			// Add info about container tables
			List<TableName> ownerTables;
			if (context.NamesCache.TablesCache.TablesByAttribute.TryGetValue(attName.ObjectKey, out ownerTables))
			{
				string tablesInfo;
				if (ownerTables.Count == 1)
				{
					TableName t = ownerTables[0];
					tablesInfo = $"Table: {t.Name} - {t.Description}";
				}
				else if (ownerTables.Count <= 5)
				{
					tablesInfo = "Tables: " + string.Join(", ", ownerTables.Select(t => t.Name).ToArray());
				}
				else
					tablesInfo = $"{ownerTables.Count} tables";
				Description += " / " + tablesInfo;
			}
		}

		/// <summary>
		/// Create autocomplete item for layout report band
		/// </summary>
		/// <param name="reportBand">The report band</param>
		public AutocompleteItem(IntelliPromptMemberList memberList, IReportBand reportBand) 
            : this(memberList, reportBand.Name, ChoiceInfo.ChoiceType.Control)
        { }

        static public double GetParameterProbability(ObjectNameInfo info, CallStatusFinder callFinder)
        {
            double priority = 0.0;

            if (callFinder == null || callFinder.ParameterInfo == null)
                // We are not on a KbObject parameter call
                return priority;

            // We are on a kbobject parameter. Set priority based on parameter info

            // Parameter type
            TypedNameInfo typedName = info as TypedNameInfo;
            if (typedName == null)
                // Names with type have priority as parameter
                priority -= 1;
            else
            {
                // TODO: Varchar, longvarchar and character differences should not be so negative
                priority += (typedName.DataType.Type == callFinder.ParameterInfo.DataType.Type ? +0.5 : -10);

                priority += (typedName.DataType.IsCollection == callFinder.ParameterInfo.DataType.IsCollection ? 0 : -10);

                priority += (typedName.DataType.Length == callFinder.ParameterInfo.DataType.Length ? +0.5 : -0.5);
                priority += (typedName.DataType.Decimals == callFinder.ParameterInfo.DataType.Decimals ? +0.5 : -0.5);

				// Parameter name
				if (typedName.Name.ToLower() == callFinder.ParameterInfo.Name.ToLower())
					priority += 1;
			}

            return priority;
        }

        /// <summary>
        /// Create a language keyword item
        /// </summary>
        /// <param name="text">Text keyword</param>
        public AutocompleteItem(string text, double priority = 0) : base(text, 0)
        {
            Type = ChoiceInfo.ChoiceType.None;
            Priority = priority;
        }

        static public AutocompleteItem GetHighestPriorityItem(List<AutocompleteItem> items)
        {
            if (items.Count == 0)
                return null;

			AutocompleteItem max = items[0];
			for (int i=1; i<items.Count; i++)
			{
				if (items[i].Priority > max.Priority)
					max = items[i];
			}
			return max;
        }
    }
}
