using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LSI.Packages.Extensiones.Utilidades.UI
{

    /// <summary>
    /// Patch to ListBox have a similar behaviour to WPF with keyboard typing.
    /// </summary>
    /// <remarks>
    /// Taked from https://social.msdn.microsoft.com/Forums/windows/en-US/8cdc5217-4f06-465a-8602-bf25b927384c/listbox-type-ahead-issue-in-c?forum=winforms
    /// </remarks>
    public class ListBoxTypeAhead : ListBox
    {
        private StringBuilder selectionCache = new StringBuilder();

        protected override void OnEnter(EventArgs e)
        {
            // Reset selection cache when control receives focus
            selectionCache.Length = 0;
            base.OnEnter(e);
        }

        protected override void OnClick(EventArgs e)
        {
            selectionCache.Length = 0;
            base.OnClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            char key = (char)e.KeyValue;
            if (Char.IsLetterOrDigit(key))
            {
                selectionCache.Append(Char.ToLower(key));
                SelectFirstItemMatching(selectionCache.ToString());
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else
            {
                selectionCache.Length = 0;
                base.OnKeyDown(e);
            }

        }

        private void SelectFirstItemMatching(string selectionText)
        {
            for (int index = 0; index < Items.Count; index++)
            {
                string itemText = Items[index].ToString().ToLower();
                if (itemText.StartsWith(selectionText))
                {
                    if (index != SelectedIndex)
                    {
                        SelectedIndex = -1;
                        SelectedIndex = index;
                    }
                    break;
                }
            }
        }
    }
}
