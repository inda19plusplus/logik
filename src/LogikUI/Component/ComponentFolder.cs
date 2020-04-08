using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Component
{
    class ComponentFolder : IComponentViewItem
    {
        public string Name;
        public List<Component> Components;

        public string ItemText => Name;
        public string IconName => "folder";

        public ComponentFolder(string name, List<Component> components)
        {
            Name = name;
            Components = components;
        }
    }
}
