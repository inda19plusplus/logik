using Gdk;
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
        public bool KeyPressed(CircuitEditor editor, Gdk.EventKey eventKey);
        public void MouseMoved(CircuitEditor editor, Vector2d mousePos);
        public void GestureStart(CircuitEditor editor, Vector2d dragStartPos);
        public void GestureUpdate(CircuitEditor editor, Vector2d offset);
        public void GestureEnd(CircuitEditor editor, Vector2d endOffset);
        public void Draw(CircuitEditor editor, Cairo.Context cr);
    }

    abstract class BasicTool : ToggleToolButton, ITool
    {
        public CircuitEditor CircuitEditor;
        public readonly Gtk.Toolbar Toolbar;

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

        public virtual void Select(CircuitEditor editor) { }
        public virtual void DeSelect(CircuitEditor editor) { }
        public virtual bool KeyPressed(CircuitEditor editor, EventKey eventKey) => false;
        public virtual void MouseMoved(CircuitEditor editor, Vector2d mousePos) { }

        public abstract void GestureStart(CircuitEditor editor, Vector2d dragStartPos);
        public abstract void GestureUpdate(CircuitEditor editor, Vector2d offset);
        public abstract void GestureEnd(CircuitEditor editor, Vector2d endOffset);
        public abstract void Draw(CircuitEditor editor, Cairo.Context cr);
    }
}
