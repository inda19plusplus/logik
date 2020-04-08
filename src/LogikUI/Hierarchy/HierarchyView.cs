using Gtk;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Hierarchy
{
    class HierarchyView
    {
        public TreeView TreeView;
        public TreeViewColumn Column;

        // FIXME: We might want to change some names in this folder...
        // Component, ComponentFolder and ComponentView should maybe reflect 
        // that they are actually not the actual "components" but only a
        // GUI model of them. But because we don't have the real implementation
        // of the components we really don't know how we want to structure it
        // so these classes might as well become the "real" ones in the end. Who knows...

        public HierarchyView(HierarchyComponent hierarchy)
        {
            TreeView = new TreeView();
            TreeView.HeadersVisible = false;
            Column = new TreeViewColumn();
            Column.Title = "Hierarchy";

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

            TreeView.Model = BuildStore(hierarchy);
        }

        private TreeStore BuildStore(HierarchyComponent item)
        {

            TreeStore store = new TreeStore(typeof(IHierarchyViewItem));
            var iter = store.AppendValues(item);
            if (item.Children.Count > 0)
            {
                AddChildren(store, iter, item.Children);
            }
            return store;

            static void AddChildren(TreeStore store, TreeIter iter, List<HierarchyComponent> children)
            {
                foreach (var child in children)
                {
                    var childIter = store.AppendValues(iter, child);
                    if (child.Children.Count > 0)
                    {
                        AddChildren(store, childIter, child.Children);
                    }
                }
            }
        }

        private void GetTextCellData(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
        {
            CellRendererText cellText = cell as CellRendererText;
            IHierarchyViewItem val = GetItem(iter);
            if (val != null)
            {
                cellText.Text = val.ItemText;
            }
        }

        private void GetIconCellData(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
        {
            CellRendererPixbuf cellText = cell as CellRendererPixbuf;
            IHierarchyViewItem val = GetItem(iter);
            if (val != null)
            {
                cellText.IconName = val.IconName;
            }
        }

        private IHierarchyViewItem GetItem(TreeIter iter)
        {
            return TreeView.Model is TreeStore store ? store.GetValue(iter, 0) as IHierarchyViewItem : null;
        }
    }
}
