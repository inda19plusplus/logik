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

    abstract class BasicTool : ToolButton, ITool
    {
        public CircuitEditor CircuitEditor;

        public BasicTool(Image image, string name, CircuitEditor circuitEditor) : base(image, name)
        {
            CircuitEditor = circuitEditor;
            Clicked += BasicTool_Clicked;
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
