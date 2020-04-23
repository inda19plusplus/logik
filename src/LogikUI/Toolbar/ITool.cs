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
        public void Select(CircuitEditor editor);
        public void DeSelect(CircuitEditor editor);
        public void DragStart(CircuitEditor editor, Vector2d dragStartPos);
        public void DragUpdate(CircuitEditor editor, Vector2d offset);
        public void DragEnd(CircuitEditor editor, Vector2d endOffset);
        public void Draw(CircuitEditor editor, Cairo.Context cr);
    }

    abstract class BasicTool : ToolButton, ITool
    {
        public BasicTool(Image image, string name) : base(image, name)
        {
        }

        // FIXME: A way to trigger redraws!!

        public abstract void Select(CircuitEditor editor);
        public abstract void DeSelect(CircuitEditor editor);
        public abstract void DragEnd(CircuitEditor editor, Vector2d endOffset);
        public abstract void DragStart(CircuitEditor editor, Vector2d dragStartPos);
        public abstract void DragUpdate(CircuitEditor editor, Vector2d offset);
        public abstract void Draw(CircuitEditor editor, Cairo.Context cr);
    }
}
