using Cairo;
using Gdk;
using Gtk;
using System;
using System.Collections.Generic;
using System.Text;
using LogikUI.Util;
using LogikUI.Transaction;
using System.Linq;
using LogikUI.Toolbar;
using GLib;

namespace LogikUI.Circuit
{
    class ComponentInstance
    {
        public PointD Position;
        public Cairo.Color Color;
        public float Width, Height;

        public ComponentInstance(PointD position, Cairo.Color color, float width, float height)
        {
            Position = position;
            Color = color;
            Width = width;
            Height = height;
        }
    }

    class CircuitEditor
    {
        public DrawingArea DrawingArea;
        public Vector2d Offset;
        public Vector2d DisplayOffset;

        public double Scale = 1;

        public double ScaleStep = 0.1d;

        public const double DotSpacing = 10d;

        public Wires Wires;
        public Gates Gates;
        public TextLabels Labels;

        public GestureDrag DragGesture;
        public GestureDrag DragGestureCreate;

        public ITool? CurrentTool;

        //public Stack<WireTransaction> Transactions = new Stack<WireTransaction>();
        public TransactionStack Transactions = new TransactionStack();

        public CircuitEditor()
        {
            DrawingArea = new DrawingArea();

            Offset = DisplayOffset = new Vector2d(0, 0);

            DrawingArea.Drawn += CircuitEditor_Drawn;

            DrawingArea.AddEvents((int)EventMask.PointerMotionMask);
            DrawingArea.AddEvents((int)EventMask.ScrollMask);

            DragGesture = new GestureDrag(DrawingArea);
            
            // Sets middle click as the pan button.
            // This should be configurable later!
            DragGesture.Button = 2;

            DragGesture.DragBegin += DragGesture_DragBegin;
            DragGesture.DragEnd += DragGesture_DragEnd;
            DragGesture.DragUpdate += DragGesture_DragUpdate;

            DragGestureCreate = new GestureDrag(DrawingArea);
            // Sets middle click as the wire creation button.
            // This should be configurable later!
            DragGestureCreate.Button = 1;
            DragGestureCreate.DragBegin += DragGestureCreate_DragBegin;
            DragGestureCreate.DragUpdate += DragGestureCreate_DragUpdate;
            DragGestureCreate.DragEnd += DragGestureCreate_DragEnd;

            DrawingArea.MotionNotifyEvent += DrawingArea_MotionNotifyEvent;

            DrawingArea.ScrollEvent += CircuitEditor_ScrollEvent;

            DrawingArea.QueryTooltip += CircuitEditor_QueryTooltip;

            DrawingArea.HasTooltip = true;

            DrawingArea.CanFocus = true;
            DrawingArea.FocusOnClick = true;

            DrawingArea.AddEvents((int)EventMask.KeyPressMask);
            DrawingArea.KeyPressEvent += DrawingArea_KeyPressEvent;

            // So that we can grab focus. Without focus we won't get any KeyPressEvents...
            // FIXME: We want to figure out how to do this in a good way where the user doesn't really
            // need to know where the current focus is for stuff like ctrl+z to work.
            DrawingArea.ParentSet += DrawingArea_ParentSet;
            DrawingArea.ButtonPressEvent += DrawingArea_ButtonPressEvent;

            var wires = new Wire[]
            {
                new Wire(new Vector2i(3, 3), 10, Direction.Vertical),
                new Wire(new Vector2i(3, 13), 10, Direction.Horizontal),
                new Wire(new Vector2i(0, 3), 3, Direction.Horizontal),
                new Wire(new Vector2i(3, 0), 3, Direction.Vertical),
                new Wire(new Vector2i(0, 13), 3, Direction.Horizontal),
                new Wire(new Vector2i(13, 12), -9, Direction.Vertical),
                new Wire(new Vector2i(4, 3), 9, Direction.Horizontal),
                new Wire(new Vector2i(13, 3), 4, Direction.Horizontal),
            };

            Wires = new Wires(wires);

            Gates = new Gates();

            Labels = new TextLabels(new TextLabel[]
            {
                new TextLabel(new Vector2d(0, 0), "This is some cool text.", 14),
                new TextLabel(new Vector2d(40, 10), "Even more cool text :O", 24),
                new TextLabel(new Vector2d(40, 40), "Woahhh :OOOoooO", 72),
            });
        }

        public void SetTool(ITool tool)
        {
            CurrentTool?.DeSelect(this);
            CurrentTool = tool;
            CurrentTool.Select(this);
        }

        /// <summary>
        /// This applies the given transaction and adds it to the undo stack.
        /// </summary>
        public void PushTransaction(Transaction.Transaction transaction)
        {
            DoTransactionNoPush(transaction);
            Transactions.PushTransaction(transaction);
        }

        /// <summary>
        /// Applies a transaction without pushing it to the undo stack.
        /// This method can cause inconsistencies in the undo system and should be used with care.
        /// </summary>
        public void DoTransactionNoPush(Transaction.Transaction transaction)
        {
            switch (transaction)
            {
                case WireTransaction wt:
                    Wires.ApplyTransaction(wt);
                    break;
                case GateTransaction gt:
                    Gates.ApplyTransaction(gt);
                    break;
                case BundledTransaction bt:
                    {
                        foreach (var bundled in bt.BundledTransactions)
                            DoTransactionNoPush(bundled);
                        break;
                    }
                default:
                    throw new Exception($"Unknown transaction type! {transaction.GetType()}");
            }
        }
        
        /// <summary>
        /// Undos a transaction without changing the undo stack.
        /// This method can cause inconsistencies in the undo system and should be used with care.
        /// </summary>
        public void UndoTransactionNoPush(Transaction.Transaction transaction)
        {
            switch (transaction)
            {
                case WireTransaction wt:
                    Wires.RevertTransaction(wt);
                    break;
                case GateTransaction gt:
                    Gates.RevertTransaction(gt);
                    break;
                case BundledTransaction bt:
                    {
                        foreach (var bundled in bt.BundledTransactions)
                            UndoTransactionNoPush(bundled);
                        break;
                    }
                default:
                    throw new Exception($"Unknown transaction type! {transaction.GetType()}");
            }
        }

        private void DrawingArea_MotionNotifyEvent(object o, MotionNotifyEventArgs args)
        {
            CurrentTool?.MouseMoved(this, new Vector2d(args.Event.X, args.Event.Y));
        }

        private void DrawingArea_ButtonPressEvent(object o, ButtonPressEventArgs args)
        {
            DrawingArea.GrabFocus();
        }

        private void DrawingArea_ParentSet(object o, ParentSetArgs args)
        {
            DrawingArea.GrabFocus();
        }

        private void DrawingArea_KeyPressEvent(object o, KeyPressEventArgs args)
        {
            var @event = args.Event;

            if (CurrentTool?.KeyPressed(this, @event) ?? false)
            {
                // The tool consumed this keypress.
                args.RetVal = true;
                return;
            }

            var modifier = @event.State & Accelerator.DefaultModMask;

            // Check that control is the only modifier pressed.
            if ((modifier ^ ModifierType.ControlMask) == 0)
            {
                if (@event.Key == Gdk.Key.z)
                {
                    // This is ctrl-z, i.e. undo
                    if (Transactions.TryUndo(out var transaction))
                    {
                        // We do the no push variant here because
                        // TryUndo already modified the undo stack.
                        UndoTransactionNoPush(transaction);
                        Console.WriteLine($"Undid transaction: {transaction}");
                        DrawingArea.QueueDraw();
                    }
                }
                else if (@event.Key == Gdk.Key.y)
                {
                    // This is ctrl-y, i.e. redo
                    if (Transactions.TryRedo(out var transaction))
                    {
                        // We do no push here because TryRedo already did the push.
                        DoTransactionNoPush(transaction);
                        Console.WriteLine($"Redid transaction: {transaction}");
                        DrawingArea.QueueDraw();
                    }
                }
            }

            // Check that control and shift are the only modifiers pressed.
            if ((modifier ^ (ModifierType.ControlMask | ModifierType.ShiftMask)) == 0)
            {
                if (@event.Key == Gdk.Key.Z)
                {
                    // This is ctrl-shift-z, i.e. redo
                    if (Transactions.TryRedo(out var transaction))
                    {
                        // We do no push here because TryRedo already did the push.
                        DoTransactionNoPush(transaction);
                        Console.WriteLine($"Redid transaction: {transaction}");
                        DrawingArea.QueueDraw();
                    }
                }
            }
        }

        private void DragGestureCreate_DragBegin(object o, DragBeginArgs args)
        {
            DragGestureCreate.GetStartPoint(out double x, out double y);
            CurrentTool?.GestureStart(this, new Vector2d(x, y));
        }

        private void DragGestureCreate_DragUpdate(object o, DragUpdateArgs args)
        {
            DragGestureCreate.GetOffset(out double x, out double y);
            CurrentTool?.GestureUpdate(this, new Vector2d(x, y));
        }

        private void DragGestureCreate_DragEnd(object o, DragEndArgs args)
        {
            // FIXME: Do l-shaped wire addition
            DragGestureCreate.GetOffset(out double x, out double y);
            CurrentTool?.GestureEnd(this, new Vector2d(x, y));
        }

        private void CircuitEditor_QueryTooltip(object o, QueryTooltipArgs args)
        {
            var mouse = ToWorld(new Vector2d(args.X, args.Y));

            // FIXME: Better tooltips. This is a placeholder.
            /* FIXME!!
            foreach (var andGate in Gates.AndGates)
            {
                var pos = andGate.GetTopLeft() * DotSpacing;
                var size = new Vector2d(3 * DotSpacing, 3 * DotSpacing);

                var bounds = new Rect(pos, size);
                if (bounds.Contains(mouse))
                {
                    args.Tooltip.Text = "And Gate";
                    args.Tooltip.TipArea = ToScreen(bounds);
                    args.RetVal = true;
                    return;
                }
            }*/

            args.RetVal = false;
        }

        public double ToWorldDist(double ScreenDist) => ScreenDist / Scale;
        public double ToScreenDist(double WorldDist) => WorldDist * Scale;

        public Vector2d ToWorld(Vector2d ScreenPoint) => (ScreenPoint - DisplayOffset) / Scale;
        public Vector2d ToWorldDist(Vector2d ScreenDist) => ScreenDist / Scale;
        public Rect ToWorld(Rect ScreenRect) => new Rect(ToWorld(ScreenRect.Position), ToWorldDist(ScreenRect.Size));

        public Vector2i RoundToGrid(Vector2d ScreenPoint) => (ToWorld(ScreenPoint) / DotSpacing).Round();
        public Vector2i RoundDistToGrid(Vector2d ScreenDist) => (ToWorldDist(ScreenDist) / DotSpacing).Round();
        
        public Vector2d FromGridToWorld(Vector2i GridPoint) => GridPoint * DotSpacing;
        public Vector2d FromGridDistToWorld(Vector2i GridDist) => GridDist * DotSpacing;

        public Vector2d ToScreen(Vector2d WorldPoint) => (WorldPoint * Scale) + DisplayOffset;
        public Vector2d ToScreenDist(Vector2d WorldDist) => WorldDist * Scale;
        public Rect ToScreen(Rect WorldRect) => new Rect(ToScreen(WorldRect.Position), ToScreenDist(WorldRect.Size));

        private void CircuitEditor_ScrollEvent(object o, ScrollEventArgs args)
        {
            var mousePos = new Vector2d(args.Event.X, args.Event.Y);

            var prevPos = ToWorld(mousePos);

            switch (args.Event.Direction)
            {
                case ScrollDirection.Up:
                    Scale = Math.Min(10, Scale + ScaleStep * Scale);
                    break;
                case ScrollDirection.Down:
                    Scale = Math.Max(0.1, Scale - ScaleStep * Scale);
                    break;
            }

            // The applied scale is centered around (0, 0) so the 
            // world position the mouse is hovering over will have changed after the scale.
            var newPos = ToWorld(mousePos);

            // Calculate how much the world position changed due to the scale            
            var diff = prevPos - newPos;

            // This is the amount of pixels the 'prevPos' world position moved.
            var sdiff = ToScreenDist(diff);

            // Compensate so that the 'prevPos' world position is a the same screen position after the zoom.
            Offset -= sdiff;
            DisplayOffset -= sdiff;

            DrawingArea.QueueDraw();
        }

        private void DragGesture_DragBegin(object o, DragBeginArgs args)
        {
            DisplayOffset = Offset;
            //DragGesture.GetStartPoint(out double x, out double y);
            //Console.WriteLine($"Drag start ({x}, {y}), DispOffset: ({DisplayOffset.X}, {DisplayOffset.Y}), Offset: ({Offset.X}, {Offset.Y})");
            DrawingArea.QueueDraw();
        }

        private void DragGesture_DragUpdate(object o, DragUpdateArgs args)
        {
            DragGesture.GetOffset(out double ox, out double oy);
            DisplayOffset = new PointD(Offset.X + ox, Offset.Y + oy);
            DrawingArea.QueueDraw();
            
            //Console.WriteLine($"Drag update ({ox}, {oy}), DispOffset: ({DisplayOffset.X}, {DisplayOffset.Y}), Offset: ({Offset.X}, {Offset.Y})");
        }

        private void DragGesture_DragEnd(object o, DragEndArgs args)
        {
            DragGesture.GetOffset(out double ox, out double oy);
            DisplayOffset = new PointD(Offset.X + ox, Offset.Y + oy);
            Offset = DisplayOffset;
            DrawingArea.QueueDraw();

            //DragGesture.GetOffset(out double x, out double y);
            //Console.WriteLine($"Drag end ({x}, {y}), Offset: ({DisplayOffset.X}, {DisplayOffset.Y}), Offset: ({Offset.X}, {Offset.Y})");
        }

        private void CircuitEditor_Drawn(object o, DrawnArgs args)
        {
            DoDraw(args.Cr);
        }

        private void DoDraw(Context cr)
        {
            var context = DrawingArea.StyleContext;
            var width = DrawingArea.AllocatedWidth;
            var height = DrawingArea.AllocatedHeight;

            context.RenderBackground(cr, 0, 0, width, height);

            cr.Translate(DisplayOffset.X, DisplayOffset.Y);
            cr.Scale(Scale, Scale);

            // FIXME
            // Because C# % is remainder I needed this.
            // But this should probably be moved somewhere else.
            static double Mod(double x, double m)
            {
                double r = x % m;
                return r < 0 ? r + m : r;
            }

            double dotScale = GetDotScaleMultiple(Scale);
            double hDotScale = dotScale / 2;
            double dotDist = DotSpacing * dotScale;

            int dotNumbersWidth = (int)((ToWorldDist(width) + dotDist) / dotDist);
            int dotNumbersHeight = (int)((ToWorldDist(height) + dotDist) / dotDist);

            var worldOffset = ToWorldDist(-DisplayOffset);
            for (int ix = -1; ix < dotNumbersWidth; ix++)
            {
                for (int iy = -1; iy < dotNumbersHeight; iy++)
                {
                    double x = worldOffset.X + (ix * dotDist) + Mod(-worldOffset.X, dotDist);
                    double y = worldOffset.Y + (iy * dotDist) + Mod(-worldOffset.Y, dotDist);

                    cr.Rectangle(x - hDotScale, y - hDotScale, dotScale, dotScale);
                }
            }
            var color = context.GetColor(context.State);
            cr.SetSourceRGB(color.Red, color.Green, color.Blue);
            cr.Fill();

            Wires.Draw(cr);
            Gates.Draw(cr);
            Labels.Draw(cr);

            CurrentTool?.Draw(this, cr);
            
            if (cr.Status != Cairo.Status.Success)
            {
                // FIXME: Figure out how to call 'cairo_status_to_string()'
                Console.WriteLine($"Cairo Error: {cr.Status}");
            }
        }

        private double GetDotScaleMultiple(double scale)
        {
            if (scale < 0.15) return 16;
            else if (scale < 0.25) return 8;
            else if (scale < 0.5) return 4;
            else if (scale < 0.95) return 2;
            else return 1;
        }
    }
}
