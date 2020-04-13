using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Hierarchy
{
    interface IHierarchyViewItem
    {
        public string ItemText { get; }
        public string IconName { get; }
    }
}
