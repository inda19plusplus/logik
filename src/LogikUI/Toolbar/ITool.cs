using Gtk;
using LogikUI.Circuit;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Toolbar
{
    interface ITool
    {
        // FIXME: Remove CircuitEditor in the params! Find something better
        // We want to 
        public void Select(CircuitEditor editor);
        public void DeSelect(CircuitEditor editor);
        public void GestureStart(CircuitEditor editor, Vector2d dragStartPos);
        public void GestureUpdate(CircuitEditor editor, Vector2d offset);
        public void GestureEnd(CircuitEditor editor, Vector2d endOffset);
        public void Draw(CircuitEditor editor, Cairo.Context cr);
    }

    abstract class BasicTool : ToggleToolButton, ITool
    {
        public CircuitEditor CircuitEditor;
        private Gtk.Toolbar Toolbar;

        public BasicTool(
            Image image, string name,
            CircuitEditor circuitEditor,
            Gtk.Toolbar toolbar
        ) : base()
        {
            this.IconWidget = image;
            this.Label = name;
            Toolbar = toolbar;
            CircuitEditor = circuitEditor;
            Clicked += BasicTool_Clicked;
        }

        protected override void OnToggled() {
            // If the button was activated, we have to make
            // all other buttons unactive
            if (this.Active) {
                for (int i = 0; i < this.Toolbar.NItems; i++) {
                    ToolItem item = this.Toolbar.GetNthItem(i);
                    // Make sure that it is a toggle and not a separator
                    if (item is ToggleToolButton && this != item) {
                        ToggleToolButton toggle = (ToggleToolButton) item;
                        if (toggle.Active) {
                            toggle.Active = false;
                        }
                    }
                }
            }
        }

        private void BasicTool_Clicked(object? sender, EventArgs e)
        {
            CircuitEditor.SetTool(this);
        }

        // FIXME: A good way to trigger redraws!!

        public abstract void Select(CircuitEditor editor);
        public abstract void DeSelect(CircuitEditor editor);
        public abstract void GestureStart(CircuitEditor editor, Vector2d dragStartPos);
        public abstract void GestureUpdate(CircuitEditor editor, Vector2d offset);
        // Apparently ToolButton has a DragEnd, we create out own with 'new' here.
        public abstract new void GestureEnd(CircuitEditor editor, Vector2d endOffset);
        public abstract void Draw(CircuitEditor editor, Cairo.Context cr);
    }
}
