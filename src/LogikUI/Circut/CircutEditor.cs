using Cairo;
using Gdk;
using Gtk;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Circut
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

    class CircutEditor : DrawingArea
    {
        public PointD Offset;
        public PointD DisplayOffset;

        public double Scale = 1;

        public double ScaleStep = 0.1d;

        const double DotSpacing = 10d;

        public List<ComponentInstance> Instances;

        public GestureDrag DragGesture;

        public CircutEditor() : base()
        {
            Offset = new PointD(0, 0);

            Drawn += CircutEditor_Drawn;

            DragGesture = new GestureDrag(this);
            
            // Sets middle click as the pan button.
            // This should be configurable later!
            DragGesture.Button = 2;

            AddEvents((int)EventMask.PointerMotionMask);
            AddEvents((int)EventMask.ScrollMask);

            DragGesture.DragBegin += DragGesture_DragBegin;
            DragGesture.DragEnd += DragGesture_DragEnd;
            DragGesture.DragUpdate += DragGesture_DragUpdate;

            ScrollEvent += CircutEditor_ScrollEvent;

            CanFocus = true;
            CanDefault = true;

            ButtonPressEvent += CircutEditor_ButtonPressEvent;

            Instances = new List<ComponentInstance>()
            {
                new ComponentInstance(new PointD(10,10), new Cairo.Color(0.5,0.5,0), 10, 10),
                new ComponentInstance(new PointD(40,40), new Cairo.Color(0.5,0,0.5), 20, 20),
            };
        }

        private void CircutEditor_ScrollEvent(object o, ScrollEventArgs args)
        {
            var @event = args.Event;
            Console.WriteLine($"dir: {@event.Direction}, x: {@event.X}, y: {@event.Y}");
            switch (@event.Direction)
            {
                case ScrollDirection.Up:
                    Scale = Math.Min(10, Scale + ScaleStep * Scale);
                    break;
                case ScrollDirection.Down:
                    Scale = Math.Max(0.1, Scale - ScaleStep * Scale);
                    break;
            }
            QueueDraw();
            Console.WriteLine($"Scale: {Scale}");
        }

        private void DragGesture_DragBegin(object o, DragBeginArgs args)
        {
            //DisplayOffset = Offset;
            DragGesture.GetStartPoint(out double x, out double y);
            Console.WriteLine($"Drag start ({x}, {y}), DispOffset: ({DisplayOffset.X}, {DisplayOffset.Y}), Offset: ({Offset.X}, {Offset.Y})");
            QueueDraw();
        }

        private void DragGesture_DragUpdate(object o, DragUpdateArgs args)
        {
            DragGesture.GetOffset(out double ox, out double oy);
            DisplayOffset = new PointD(Offset.X + ox, Offset.Y + oy);
            QueueDraw();

            Console.WriteLine($"Drag update ({ox}, {oy}), DispOffset: ({DisplayOffset.X}, {DisplayOffset.Y}), Offset: ({Offset.X}, {Offset.Y})");
        }

        private void DragGesture_DragEnd(object o, DragEndArgs args)
        {
            DragGesture.GetOffset(out double ox, out double oy);
            DisplayOffset = new PointD(Offset.X + ox, Offset.Y + oy);
            Offset = DisplayOffset;
            QueueDraw();

            DragGesture.GetOffset(out double x, out double y);
            Console.WriteLine($"Drag end ({x}, {y}), Offset: ({DisplayOffset.X}, {DisplayOffset.Y}), Offset: ({Offset.X}, {Offset.Y})");
        }

        private void CircutEditor_ButtonPressEvent(object o, ButtonPressEventArgs args)
        {
            Console.WriteLine($"{args.Event.Button}");
        }

        private void CircutEditor_Drawn(object o, DrawnArgs args)
        {
            DoDraw(args.Cr);
        }

        protected void DoDraw(Context cr)
        {
            var context = StyleContext;
            var width = AllocatedWidth;
            var height = AllocatedHeight;

            context.RenderBackground(cr, 0, 0, width, height);

            cr.Translate(DisplayOffset.X, DisplayOffset.Y);
            cr.Scale(Scale, Scale);

            // Draw the dots and compensate 
            //if (Scale < 0.4f)
            int dots = 0;

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

                    dots++;
                    y += dotDist;
                }
                x += dotDist;
            }

            Console.WriteLine($"Rendered {dots} dots");

            var color = context.GetColor(context.State);
            cr.SetSourceColor(new Cairo.Color(color.Red, color.Green, color.Blue, color.Alpha));

            cr.Fill();

            foreach (var instance in Instances)
            {
                cr.Rectangle(instance.Position, instance.Width, instance.Height);
                cr.SetSourceColor(instance.Color);
                cr.Fill();
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
