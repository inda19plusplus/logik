using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Hierarchy
{
    class HierarchyComponent : IHierarchyViewItem
    {
        public string Name;
        public string Icon;

        public List<HierarchyComponent> Children;

        public string ItemText => Name;
        public string IconName => Icon;

        public HierarchyComponent(string name, string icon, List<HierarchyComponent> children)
        {
            Name = name;
            Icon = icon;
            Children = children;
        }
    }
}
