using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Component
{
    class Component : IComponentViewItem
    {
        public string Name;
        public string Icon;

        public string ItemText => Name;
        public string IconName => Icon;

        public Component(string name, string icon)
        {
            Name = name;
            Icon = icon;
        }
    }
}
