using Cairo;
using Gtk;
using LogikUI.Circuit;
using LogikUI.Interop;
using LogikUI.Simulation.Gates;
using LogikUI.Transaction;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Toolbar
{
    class SelectTool : BasicTool
    {
        public bool Poking = false;
        public bool Selecting = false;
        public Vector2i SelectionStart = default;
        public Vector2i SelectionSize = default;

        public List<Wire> SelectedWires = new List<Wire>();
        public List<InstanceData> SelectedGates = new List<InstanceData>();

        public SelectTool(
            CircuitEditor circuitEditor,
            Gtk.Toolbar toolbar
        ) : base(Util.Icon.Selector(), "Select", circuitEditor, toolbar)
        { }

        public void ClearSelection(CircuitEditor editor)
        {
            SelectedWires.Clear();
            SelectedGates.Clear();
            editor.DrawingArea.QueueDraw();
        }

        public override bool KeyPressed(CircuitEditor editor, Gdk.EventKey eventKey)
        {
            var modifiers = eventKey.State & Accelerator.DefaultModMask;
            if (modifiers == Gdk.ModifierType.None)
            {
                // No modifiers pressed

                if (eventKey.Key == Gdk.Key.Delete)
                {
                    // Here we should create a delete transaction for the wires and the gates
                    // and apply them as a bundled transaction
                    if (SelectedWires.Count > 0 || SelectedGates.Count > 0)
                    {
                        // FIXME: Change namespace Transaction so we don't need to do this stuff...
                        List<Transaction.Transaction> transactions = new List<Transaction.Transaction>();

                        // Right now you have to apply each transaction before creating the
                        // next one in order for the created transactions to be consistent.
                        foreach (var wire in SelectedWires)
                        {
                            var t = editor.Scene.Wires.CreateRemoveWireTransaction(wire);
                            transactions.Add(t);
                            editor.Scene.DoTransactionNoPush(t);
                        }

                        foreach (var instance in SelectedGates)
                        {
                            var t = editor.Scene.Gates.CreateRemoveGateTransaction(editor.Scene.Wires, instance);
                            transactions.Add(t);
                            editor.Scene.DoTransactionNoPush(t);
                        }

                        BundledTransaction bundleTransaction = new BundledTransaction(transactions);
                        // Push the transaction onto the stack, we don't need to apply it because it's already applied.
                        editor.Scene.Transactions.PushTransaction(bundleTransaction);
                        ClearSelection(editor);
                        return true;
                    }
                }

                if (eventKey.Key == Gdk.Key.Escape)
                {
                    ClearSelection(editor);
                    return true;
                }
            }


            return false;
        }

        public override void GestureStart(CircuitEditor editor, Vector2d dragStartPos)
        {
            Selecting = true;
            SelectionStart = editor.RoundToGrid(dragStartPos);
            SelectionSize = Vector2i.Zero;
            editor.DrawingArea.QueueDraw();

            if (SelectedGates.Count == 1)
            {
                var r = editor.Scene.Gates.GetBounds(SelectedGates[0]);
                r = r.Rotate(SelectedGates[0].Position * CircuitEditor.DotSpacing, SelectedGates[0].Orientation);
                if (r.Contains(editor.FromGridToWorld(SelectionStart)))
                {
                    Poking = true;
                    LogLogic.PressComponent(Program.Backend, SelectedGates[0].ID);
                }
            }
        }

        public override void GestureUpdate(CircuitEditor editor, Vector2d offset)
        {
            SelectionSize = editor.RoundDistToGrid(offset);
            editor.DrawingArea.QueueDraw();
        }

        public override void GestureEnd(CircuitEditor editor, Vector2d endOffset)
        {
            SelectionSize = editor.RoundDistToGrid(endOffset);
            Selecting = false;

            // Make sure we don't have a negitive size

            if (SelectionSize.X < 0)
            {
                SelectionSize.X = -SelectionSize.X;
                SelectionStart.X -= SelectionSize.X;
            }

            if (SelectionSize.Y < 0)
            {
                SelectionSize.Y = -SelectionSize.Y;
                SelectionStart.Y -= SelectionSize.Y;
            }

            if (Poking)
            {
                Poking = false;
                LogLogic.ReleaseComponent(Program.Backend, SelectedGates[0].ID);
            }

            // FIXME: Do the selecting
            var wires = editor.Scene.Wires;

            // If the size is zero we want to check if we clicked a wire or gate
            if (SelectionSize == Vector2i.Zero)
            {
                ClearSelection(editor);

                bool foundGate = false;
                var pos = editor.FromGridToWorld(SelectionStart);
                foreach (var instance in editor.Scene.Gates.Instances)
                {
                    Rect r = editor.Scene.Gates.GetBounds(instance);
                    r = r.Rotate(instance.Position * CircuitEditor.DotSpacing, instance.Orientation);

                    if (r.Contains(pos))
                    {
                        SelectedGates.Add(instance);
                        break;
                    }
                }

                if (foundGate == false)
                {
                    foreach (var wire in wires.WiresList)
                    {
                        if (wire.IsPointOnWire(SelectionStart))
                        {
                            SelectedWires.Add(wire);
                            foundGate = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                ClearSelection(editor);

                // Here we should loop through all wires and see if they are contained in the area
                Recti selectionRect = new Recti(SelectionStart, SelectionSize);

                foreach (var wire in wires.WiresList)
                {
                    if (selectionRect.Contains(wire.Pos) && 
                        selectionRect.Contains(wire.EndPos))
                    {
                        SelectedWires.Add(wire);
                        Console.WriteLine($"Selected wire: {wire}");
                    }
                }

                Rect worldRect = editor.FromGridToWorld(selectionRect);
                foreach (var instance in editor.Scene.Gates.Instances)
                {
                    Rect r = editor.Scene.Gates.GetBounds(instance);
                    r = r.Rotate(instance.Position * CircuitEditor.DotSpacing, instance.Orientation);

                    if (worldRect.Contains(r))
                    {
                        SelectedGates.Add(instance);
                    }
                }
            }

            editor.DrawingArea.QueueDraw();
        }

        public override void Draw(CircuitEditor editor, Context cr)
        {
            if (Selecting)
            {
                var size = editor.FromGridDistToWorld(SelectionSize);
                cr.Rectangle(editor.FromGridToWorld(SelectionStart), size.X, size.Y);
                cr.SetSourceRGBA(0.1, 0.1, 0.3, 0.3);
                cr.Fill();
            }

            if (SelectedWires.Count > 0)
            {
                foreach (var wire in SelectedWires)
                {
                    var size = new Vector2d(10, 10);
                    var pos = editor.FromGridToWorld(wire.Pos) - (size / 2);
                    cr.Rectangle(pos, size.X, size.Y);

                    var endpos = editor.FromGridToWorld(wire.EndPos) - (size / 2);
                    cr.Rectangle(endpos, size.X, size.Y);
                }
                cr.SetSourceRGB(1, 1, 1);
                cr.FillPreserve();
                cr.SetSourceRGB(0.1, 0.1, 0.1);
                cr.Stroke();
            }

            if (SelectedGates.Count > 0)
            {
                foreach (var instance in SelectedGates)
                {
                    Rect r = editor.Scene.Gates.GetBounds(instance);
                    r = r.Rotate(instance.Position * CircuitEditor.DotSpacing, instance.Orientation);

                    var size = new Vector2d(10, 10);
                    var hSize = size / 2;
                    cr.Rectangle(r.TopLeft - hSize, size.X, size.Y);
                    cr.Rectangle(r.TopRight - hSize, size.X, size.Y);
                    cr.Rectangle(r.BottomLeft - hSize , size.X, size.Y);
                    cr.Rectangle(r.BottomRight - hSize, size.X, size.Y);
                }
                cr.SetSourceRGB(1, 1, 1);
                cr.FillPreserve();
                cr.SetSourceRGB(0.1, 0.1, 0.1);
                cr.Stroke();
            }
        }
    }
}
