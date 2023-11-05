using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition
{
	// TODO: Check if this should have a member defining the number of groups to split (ex. groups number = 3, "Gg1Gg2Gg3Gg4" -> ["Gg1","Gg2","Gg3Gg4"])
	// TODO: Probably this will simplify a lot of things

    /// <summary>
    /// Extracts name groups from a Gx variable / attribute / object name, splitted by uppercase letters
    /// </summary>
    /// <remarks>
    /// Ex. "FacCan" -> { "Fac" , "Can" }
    /// </remarks>
    public class GxNameSplitter
    {
        /// <summary>
        /// Cache for better performance
        /// </summary>
        static private string LastName;

		/// <summary>
		/// Cache for better performance
		/// </summary>
		static private List<string> LastResult;

		/// <summary>
		/// Number of groups to split
		/// </summary>
		private int NGroups;

		public GxNameSplitter(int nGroups)
		{
			if (nGroups <= 0)
				throw new ArgumentException("nGroups must to be > 0");
			NGroups = nGroups;
		}

        /// <summary>
        /// Split a name
        /// </summary>
        /// <param name="name">Name to split</param>
		/// <returns>Name groups, padded with empty strings up to NGroups. Groups are lowercase</returns>
        private List<string> SplitGroups(string name)
        {
			List<string> groups = new List<string>(NGroups);

			if (!string.IsNullOrEmpty(name))
			{
				name = name.Trim();

				bool lastCharIsUpper = true;
				int lastUpperIdx = 0;
				for (int i = 0; i < name.Length; i++)
				{
					bool isUpper = char.IsUpper(name[i]);
					if(!lastCharIsUpper && isUpper)
					{
						//XzFacCan
						// 0123456
						//  ^  ^
						groups.Add(name.Substring(lastUpperIdx, i - lastUpperIdx).ToLower());

						if (groups.Count == (NGroups - 1))
						{
							// This character, and everything after, will be the last group
							groups.Add(name.Substring(i).ToLower());
							return groups;
						}

						lastUpperIdx = i;
					}
					lastCharIsUpper = isUpper;
				}

				// Add last group (will never be empty)
				groups.Add(name.Substring(lastUpperIdx).ToLower());
			}

			// Pad with empty strings, up to NGroups
			while (groups.Count < NGroups)
				groups.Add(string.Empty);

			return groups;
		}

		/// <summary>
		/// Split a name
		/// </summary>
		/// <param name="name">Name to split</param>
		/// <returns>Name groups, padded with empty strings up to NGroups. Groups are lowercase</returns>
		public List<string> Split(string name)
        {
			if (LastName != null && LastName == name)
				return LastResult;

			LastName = name;
			LastResult = SplitGroups(LastName);
#if DEBUG
			if (LastResult.Count != NGroups)
				throw new Exception("Wrong number of groups: " + LastResult.Count + ", name=" + name);
#endif
			return LastResult;
		}

    }
}
