using Cairo;
using Gdk;
using Gtk;
using System;
using System.Collections.Generic;
using System.Text;
using LogikUI.Util;

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

        public CircuitEditor()
        {
            DrawingArea = new DrawingArea();

            Offset = DisplayOffset = new Vector2d(0, 0);

            DrawingArea.Drawn += CircuitEditor_Drawn;

            DragGesture = new GestureDrag(DrawingArea);
            
            // Sets middle click as the pan button.
            // This should be configurable later!
            DragGesture.Button = 2;

            DrawingArea.AddEvents((int)EventMask.PointerMotionMask);
            DrawingArea.AddEvents((int)EventMask.ScrollMask);

            DragGesture.DragBegin += DragGesture_DragBegin;
            DragGesture.DragEnd += DragGesture_DragEnd;
            DragGesture.DragUpdate += DragGesture_DragUpdate;

            DrawingArea.ScrollEvent += CircuitEditor_ScrollEvent;

            DrawingArea.QueryTooltip += CircuitEditor_QueryTooltip;

            DrawingArea.HasTooltip = true;

            DrawingArea.CanFocus = true;
            DrawingArea.CanDefault = true;

            DrawingArea.ButtonPressEvent += CircuitEditor_ButtonPressEvent;

            Random rand = new Random();
            int n = 1000;
            var a = new Wire[n];
            for (int i = 0; i < n; i++)
            {
                a[i] = new Wire(new Vector2i(rand.Next(100), rand.Next(100)), rand.Next(1, 21), rand.Next(2) == 0 ? Circuit.Direction.Horizontal : Circuit.Direction.Vertical);
            }
            var b = new Wire[n];
            for (int i = 0; i < n; i++)
            {
                b[i] = new Wire(new Vector2i(rand.Next(100), rand.Next(100)), rand.Next(1, 21), rand.Next(2) == 0 ? Circuit.Direction.Horizontal : Circuit.Direction.Vertical);
            }

            var powered = new Wire[]
            {
                new Wire(new Vector2i(3, 3), 10, Direction.Vertical),
                new Wire(new Vector2i(3, 13), 10, Direction.Horizontal),
                new Wire(new Vector2i(0, 3), 3, Direction.Horizontal),
                new Wire(new Vector2i(3, 0), 3, Direction.Vertical),
                new Wire(new Vector2i(0, 13), 3, Direction.Horizontal),
            };
            var unpowered = new Wire[]
            {
                new Wire(new Vector2i(13, 12), -9, Direction.Vertical),
                new Wire(new Vector2i(4, 3), 9, Direction.Horizontal),
                new Wire(new Vector2i(13, 3), 4, Direction.Horizontal),
            };

            Wires = new Wires(
                powered, 
                unpowered, 
                // For wires to connect their start/end point must be at the same location
                // A wire that start/ends in the middle of another wires doesn't connect
                // (We might want to change that but it becomes more complicated then...)
                Wires.FindConnectionPoints(powered).ToArray(),
                Wires.FindConnectionPoints(unpowered).ToArray());

            Gates = new Gates(new AndGate[]
            {
                new AndGate(new Vector2i(2, 2), Orientation.South),
                new AndGate(new Vector2i(3, 7), Orientation.East),
                new AndGate(new Vector2i(3, 10), Orientation.West),
                new AndGate(new Vector2i(5, 3), Orientation.North),
            });

            Labels = new TextLabels(new TextLabel[]
            {
                new TextLabel(new Vector2d(0, 0), "This is some cool text.", 14),
                new TextLabel(new Vector2d(40, 10), "Even more cool text :O", 24),
                new TextLabel(new Vector2d(40, 40), "Woahhh :OOOoooO", 72),
            });
        }

        private void CircuitEditor_QueryTooltip(object o, QueryTooltipArgs args)
        {
            var mouse = ToWorld(new Vector2d(args.X, args.Y));

            // FIXME: Better tooltips. This is a placeholder.
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
            }

            // FIXME: This is something to consider with out current data design!
            /*
            foreach (var instance in Instances)
            {
                var bounds = new Rect(instance.Position, new Vector2d(instance.Width, instance.Height));
                if (bounds.Contains(mouse))
                {
                    args.Tooltip.Text = $"{(Vector2d)instance.Position}";
                    args.Tooltip.TipArea = ToScreen(bounds);
                    args.RetVal = true;
                    return;
                }
            }
            */

            args.RetVal = false;
        }

        private Vector2d ToWorld(Vector2d ScreenPoint) => (ScreenPoint - DisplayOffset) / Scale;
        private Vector2d ToWorldDist(Vector2d ScreenDist) => ScreenDist / Scale;
        private Rect ToWorld(Rect ScreenRect) => new Rect(ToWorld(ScreenRect.Position), ToWorldDist(ScreenRect.Size));

        private Vector2d ToScreen(Vector2d WorldPoint) => (WorldPoint * Scale) + DisplayOffset;
        private Vector2d ToScreenDist(Vector2d WorldDist) => WorldDist * Scale;
        private Rect ToScreen(Rect WorldRect) => new Rect(ToScreen(WorldRect.Position), ToScreenDist(WorldRect.Size));

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

        private void CircuitEditor_ButtonPressEvent(object o, ButtonPressEventArgs args)
        {
            //Console.WriteLine($"{args.Event.Button}");
        }

        private void CircuitEditor_Drawn(object o, DrawnArgs args)
        {
            DoDraw(args.Cr);
        }

        protected void DoDraw(Context cr)
        {
            var context = DrawingArea.StyleContext;
            var width = DrawingArea.AllocatedWidth;
            var height = DrawingArea.AllocatedHeight;

            context.RenderBackground(cr, 0, 0, width, height);

            cr.Translate(DisplayOffset.X, DisplayOffset.Y);
            cr.Scale(Scale, Scale);

            //int dots = 0;

            double dotScale = GetDotScaleMultiple(Scale);
            double dotDist = DotSpacing * dotScale;

            double x = 0;
            while (x <= width / Scale)
            {
                double y = 0;
                while (y <= height / Scale)
                {
                    // Here we want to use the inverse transform
                    double posx = x - ((DisplayOffset.X / Scale)) + (DisplayOffset.X / Scale) % dotDist;
                    double posy = y - ((DisplayOffset.Y / Scale)) + (DisplayOffset.Y / Scale) % dotDist;

                    cr.Rectangle(posx - 0.5, posy - 0.5, dotScale, dotScale);

                    //dots++;

                    y += dotDist;
                }
                x += dotDist;
            }

            //Console.WriteLine($"Rendered {dots} dots");
            var color = context.GetColor(context.State);
            cr.SetSourceRGB(color.Red, color.Green, color.Blue);

            cr.Fill();

            Wires.Draw(cr);
            Gates.Draw(cr);
            Labels.Draw(cr);

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
