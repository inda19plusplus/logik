using Cairo;
using Gtk;
using LogikUI.Circuit;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Toolbar
{
    class SelectTool : BasicTool
    {
        public bool Selecting = false;
        public Vector2i SelectionStart = default;
        public Vector2i SelectionSize = default;

        public SelectTool(
            CircuitEditor circuitEditor,
            Gtk.Toolbar toolbar
        ) : base(Util.Icon.Selector(), "Select", circuitEditor, toolbar)
        {
        }

        public override void Select(CircuitEditor editor)
        {
        }

        public override void DeSelect(CircuitEditor editor)
        {
        }

        public override void GestureStart(CircuitEditor editor, Vector2d dragStartPos)
        {
            Selecting = true;
            SelectionStart = editor.RoundToGrid(dragStartPos);
            SelectionSize = Vector2i.Zero;
            Console.WriteLine($"Pos: {SelectionStart}, Screen: {dragStartPos}");
            editor.DrawingArea.QueueDraw();
        }

        public override void GestureUpdate(CircuitEditor editor, Vector2d offset)
        {
            SelectionSize = editor.RoundDistToGrid(offset);
            //Console.WriteLine($"Size: {SelectionSize}");
            editor.DrawingArea.QueueDraw();
        }

        public override void GestureEnd(CircuitEditor editor, Vector2d endOffset)
        {
            SelectionSize = editor.RoundDistToGrid(endOffset);
            Selecting = false;

            // FIXME: Do the selecting

            editor.DrawingArea.QueueDraw();
        }

        public override void Draw(CircuitEditor editor, Context cr)
        {
            if (Selecting)
            {
                var size = editor.FromGridDistToWorld(SelectionSize);
                Console.WriteLine($"Pos: {editor.FromGridToWorld(SelectionStart)}, Size: {size}");
                cr.Rectangle(editor.FromGridToWorld(SelectionStart), size.X, size.Y);
                cr.SetSourceRGBA(0.1, 0.1, 0.3, 0.3);
                cr.Fill();
            }
        }
    }
}
