using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LSI.Packages.Extensiones.Utilidades.UI
{
    /// <summary>
    /// List box utilities
    /// </summary>
    public class ListBoxUtils
    {
        /// <summary>
        /// Move the selected item up or down in the list
        /// </summary>
        /// <param name="list">List to modify</param>
        /// <param name="increment">+1 to move down the selected item. -1 to move up</param>
        static public void MoveSelectedItem(ListBox list, int increment)
        {

            // Only move once at same time is supported:
            if (list.SelectedIndices.Count > 1)
            {
                int index = list.SelectedIndices[0];
                list.SelectedIndices.Clear();
                list.SelectedIndex = index;
            }

            int idx = list.SelectedIndex;
            if (idx < 0)
                return;
            int newIdx = idx + increment;
            if (newIdx < 0 || newIdx >= list.Items.Count)
                return;

            object task = list.SelectedItem;
            list.Items.RemoveAt(idx);
            list.Items.Insert(newIdx, task);
            list.SelectedIndex = newIdx;
        }
    }
}
