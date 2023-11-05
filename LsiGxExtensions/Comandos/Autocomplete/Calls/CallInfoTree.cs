using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.Calls
{

	/// <summary>
	/// Contains set of calls, with a tree structure: Calls inside call, inside calls,...
	/// </summary>
	class CallInfoTree
	{

		class TreeNode
		{
			public CallInfo CallInfo;

			/// <summary>
			/// Key is the call offset start. It contains all calls inside this call
			/// </summary>
			public SortedList<int, TreeNode> Childs = new SortedList<int, TreeNode>();

			public TreeNode(CallInfo callInfo) { CallInfo = callInfo; }

			public TreeNode FindInnerCall(int offset)
			{
				if (CallInfo != null && (CallInfo.OffsetStart > offset || CallInfo.OffsetEnd < offset ))
					// This call does not contain the offset. Root node has CallInfo == null and "contains" all calls
					return null;

				// Check if any child contains the offset
				// TODO: Do a binary search here could improve performance...
				foreach(TreeNode child in Childs.Values)
				{
					if (child.CallInfo.OffsetStart > offset)
						// Childs list is sorted by start offset. So if current child starts is after the offset to search, we have finished
						return this;
					TreeNode searchResult = child.FindInnerCall(offset);
					if (searchResult != null)
						return searchResult;
				}

				return this;
			}

			public void Add(CallInfo callInfo)
			{
				Childs.Add(callInfo.OffsetStart, new TreeNode(callInfo));
			}

			public override string ToString()
			{
				return CallInfo == null ? "Root" : CallInfo.ToString() + " / " + Childs.Count + " inner calls";
			}
		}

		TreeNode Root = new TreeNode(null);

		public void Add(CallInfo callInfo)
		{
			Root.FindInnerCall(callInfo.OffsetStart).Add(callInfo);
		}

		public CallInfo GetCallForOffset(int offset)
		{
			return Root.FindInnerCall(offset).CallInfo;
		}
	}
}
