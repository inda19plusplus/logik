using Cairo;
using LogikUI.Circuit;
using LogikUI.Transaction;
using LogikUI.Util;
using System;
using System.Collections.Generic;

namespace LogikUI.Toolbar
{
    class WireTool : BasicTool
    {
        public bool DraggingWire = false;
        public Vector2i DragStartPos = default;
        public Wire CurrentWire = default;

        public WireTool(
            CircuitEditor circuitEditor,
            Gtk.Toolbar toolbar
        ) : base(Icon.Wire(), "Wire", circuitEditor, toolbar)
        {

        }

        public override void GestureStart(CircuitEditor editor, Vector2d dragStartPos)
        {
            DraggingWire = true;
            DragStartPos = editor.RoundToGrid(dragStartPos);
            CurrentWire = new Wire(DragStartPos, 0, Circuit.Direction.Horizontal);
            editor.DrawingArea.QueueDraw();
        }

        public override void GestureUpdate(CircuitEditor editor, Vector2d offset)
        {
            var diff = editor.RoundDistToGrid(offset);

            if (Math.Abs(diff.X) > Math.Abs(diff.Y))
            {
                // Here we are drawing a horizontal line
                CurrentWire = new Wire(DragStartPos, diff.X, Circuit.Direction.Horizontal);
            }
            else
            {
                //This should be a vertical line 
                CurrentWire = new Wire(DragStartPos, diff.Y, Circuit.Direction.Vertical);
            }

            editor.DrawingArea.QueueDraw();
        }

        public override void GestureEnd(CircuitEditor editor, Vector2d endOffset)
        {
            // FIXME: Do l-shaped wire addition
            var diff = editor.RoundDistToGrid(endOffset);
            if (Math.Abs(diff.X) > Math.Abs(diff.Y))
            {
                // FIXME: We shouldn't rely on the constructor to flip negative diffs
                // because we are going to remove that so we should do it ourselves here.

                // Here we are drawing a horizontal line
                CurrentWire = new Wire(DragStartPos, diff.X, Circuit.Direction.Horizontal);
            }
            else
            {
                //This should be a vertical line 
                CurrentWire = new Wire(DragStartPos, diff.Y, Circuit.Direction.Vertical);
            }

            DraggingWire = false;

            if (CurrentWire.Length != 0)
            {
                // Here we should figure out if we are modifying an existing wire.
                // Because adding wires handles merging wires that go in the same
                // direction, we only want to detect the case where we are shrinking
                // a wire. This is easy, because we only need to check that we grabbed
                // one end of the wire and the the other end is inside the wire we want
                // to shrink.
                Wire? modifying = null;
                bool movingEnd = false;
                foreach (var bWire in editor.Wires.WiresList)
                {
                    if (CurrentWire.Direction != bWire.Direction)
                        continue;

                    // We do 'IsPointInsideWire' because we don't want to detect the case
                    // where we are dragging a wire left from a junction to create a new wire.
                    // What would happen if we used 'IsPointOnWire' is that the start pos would
                    // be equal and at the same time 'CurrentWire.Pos' would be on the wire,
                    // making us think that we want to edit the wire while we don't want to.
                    if (DragStartPos == bWire.Pos && bWire.IsPointInsideWire(CurrentWire.EndPos))
                    {
                        modifying = bWire;
                        movingEnd = false;
                    }
                    else if (DragStartPos == bWire.EndPos && bWire.IsPointInsideWire(CurrentWire.Pos))
                    {
                        modifying = bWire;
                        movingEnd = true;
                    }
                    else if (CurrentWire == bWire)
                    {
                        // If they are the same wire we want to remove the bWire
                        modifying = bWire;
                    }
                }

                // If there there more than one endpoint at the start position
                // We don't want to modify a wire, we want to create a new one
                // If there was only one point we check that we actually want to
                // modify that wire (ie modifying is not null).
                if (modifying is Wire modify)
                {
                    // Here we are modifying an existing wire.
                    // So we should figure out how the wire should be modified.
                    // If the wire is deleted we need to check for places where we should merge
                    // other connecting wires. The creation of the remove transaction should
                    // probably be created in a Wires.CreateRemoveWireTransaction(...).

                    Wire modifiedWire = modify;
                    if (movingEnd == false)
                    {
                        // Here we are moving the start poistion of the wire.
                        diff = CurrentWire.EndPos - modify.Pos;
                        modifiedWire.Pos = CurrentWire.EndPos;
                        modifiedWire.Length -= diff.ManhattanDistance;
                    }
                    else
                    {
                        // Here we only need to change the length of the wire.
                        diff = modify.EndPos - CurrentWire.Pos;
                        modifiedWire.Length -= diff.ManhattanDistance;
                    }

                    if (modifiedWire.Length > 0)
                    {
                        // FIMXE: Figure out if there are any edge-cases when modifying wires like this.
                        // One of the cases is where the modified wire should split another already
                        // existing wire.

                        WireTransaction? modifyTransaction = editor.Wires.CreateModifyWireTransaction(modify, modifiedWire);
                        if (modifyTransaction != null)
                        {
                            editor.Wires.ApplyTransaction(modifyTransaction);
                            editor.Transactions.PushTransaction(modifyTransaction);
                            Console.WriteLine($"Modified existing wire! ({modify}) -> ({modifiedWire})\n{modifyTransaction}\n");
                        }
                    }
                    else
                    {
                        // Here we should remove the wire completely
                        var deleteTransaction = editor.Wires.CreateRemoveWireTransaction(modify);
                        editor.Wires.ApplyTransaction(deleteTransaction);
                        editor.Transactions.PushTransaction(deleteTransaction);
                        Console.WriteLine($"Deleted wire! ({modify})\n{deleteTransaction}\n");
                    }
                }
                else
                {
                    var transaction = editor.Wires.CreateAddWireTransaction(CurrentWire);
                    if (transaction != null)
                    {
                        editor.Wires.ApplyTransaction(transaction);
                        editor.Transactions.PushTransaction(transaction);
                        Console.WriteLine($"End wire!\n{transaction}\n");
                    }
                    else
                    {
                        Console.WriteLine($"No wire created!\n");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Zero length wire!\n\n");
            }

            editor.DrawingArea.QueueDraw();
        }

        public override void Draw(CircuitEditor editor, Context cr)
        {
            if (DraggingWire)
            {
                editor.Wires.Wire(cr, CurrentWire);
                cr.SetSourceRGB(0.3, 0.4, 0.3);
                cr.Fill();
            }
        }
    }
}
