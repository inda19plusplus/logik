using System;
using System.Collections.Generic;
using System.Text;
using Gtk;

namespace LogikUI.Component
{
    class ComponentView
    {
        public TreeView TreeView;
        public TreeViewColumn Column;
        // FIXME: We might want to change some names in this folder...
        // Component, ComponentFolder and ComponentView should maybe reflect 
        // that they are actually not the actual "components" but only a
        // GUI model of them. But because we don't have the real implementation
        // of the components we really don't know how we want to structure it
        // so these classes might as well become the "real" ones in the end. Who knows...

        public ComponentView(List<ComponentFolder> folders)
        {
            TreeView = new TreeView();
            TreeView.HeadersVisible = false;
            Column = new TreeViewColumn();
            Column.Title = "Components";

            var text = new CellRendererText();
            var pix = new CellRendererPixbuf();
            try
            {
                pix.Icon = IconTheme.Default.LoadIcon("x-office-document", 16, 0);
            }
            catch (GLib.GException gex)
            {
                Console.WriteLine(gex);
            }

            Column.PackStart(pix, false);
            Column.PackStart(text, false);
            Column.SetCellDataFunc(text, GetTextCellData);
            Column.SetCellDataFunc(pix, GetIconCellData);
            Column.Expand = true;
            TreeView.AppendColumn(Column);

            TreeView.Model = BuildStore(folders);
        }

        private TreeStore BuildStore(List<ComponentFolder> folders)
        {
            TreeStore store = new TreeStore(typeof(IComponentViewItem));
            foreach (var folder in folders)
            {
                var folderIter = store.AppendValues(folder);
                foreach (var comp in folder.Components)
                {
                    store.AppendValues(folderIter, comp);
                }
            }
            return store;
        }

        private void GetTextCellData(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
        {
            CellRendererText cellText = cell as CellRendererText;
            IComponentViewItem val = GetItem(iter);
            if (val != null)
            {
                cellText.Text = val.ItemText;
            }
        }

        private void GetIconCellData(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
        {
            CellRendererPixbuf cellText = cell as CellRendererPixbuf;
            IComponentViewItem val = GetItem(iter);
            if (val != null)
            {
                cellText.IconName = val.IconName;
            }
        }

        private IComponentViewItem GetItem(TreeIter iter)
        {
            return TreeView.Model is TreeStore store ? store.GetValue(iter, 0) as IComponentViewItem : null;
        }
    }
}
