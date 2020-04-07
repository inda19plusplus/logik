using System;
using System.Collections.Generic;
using System.Text;
using Gtk;

namespace LogikUI
{
    class ComponentView
    {
        public TreeView TreeView;
        public TreeViewColumn Column;

        public ComponentView()
        {
            TreeView = new TreeView();
            TreeView.HeadersVisible = false;

            Column = new TreeViewColumn();
            Column.Title = "Components";

            var text = new CellRendererText();
            var pix = new CellRendererPixbuf();
            try
            {
                pix.Icon = IconTheme.Default.LoadIcon(IconTheme.Default.ExampleIconName, 16, 0);
            }
            catch (GLib.GException gex)
            {
                Console.WriteLine(gex);
            }

            Column.PackStart(pix, false);
            Column.PackStart(text, false);
            Column.SetCellDataFunc(text, GetCellData);
            Column.Expand = true;
            TreeView.AppendColumn(Column);

            TreeView.Model = BuildStore();
        }

        private TreeStore BuildStore()
        {
            TreeStore store = new TreeStore(typeof(string));
            for (int i = 0; i < 10; i++)
            {
                var a = store.AppendValues($"{i}");

                for (int j = 0; j < 4; j++)
                {
                    store.AppendValues(a, $"{j}");
                }
            }
            return store;
        }

        private void GetCellData(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
        {
            CellRendererText cellText = cell as CellRendererText;
            string val = GetItem(iter);
            if (val != null)
            {
                cellText.Text = val;
            }
        }

        private string GetItem(TreeIter iter)
        {
            return TreeView.Model is TreeStore store ? store.GetValue(iter, 0) as string : null;
        }
    }
}
