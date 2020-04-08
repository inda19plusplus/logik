using Cairo;
using Gtk;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Circut
{
    class ComponentInstance
    {
        public PointD Position;
        public Color Color;
        public float Width, Height;

        public ComponentInstance(PointD position, Color color, float width, float height)
        {
            Position = position;
            Color = color;
            Width = width;
            Height = height;
        }
    }

    class CircutEditor : DrawingArea
    {
        public List<ComponentInstance> Instances;

        public CircutEditor() : base()
        {
            Drawn += CircutEditor_Drawn;

            Instances = new List<ComponentInstance>()
            {
                new ComponentInstance(new PointD(10,10), new Color(0.5,0.5,0), 10, 10),
                new ComponentInstance(new PointD(40,40), new Color(0.5,0,0.5), 20, 20),
            };
        }

        private void CircutEditor_Drawn(object o, DrawnArgs args)
        {
            Draw(args.Cr);
        }

        protected void Draw(Context cr)
        {
            var context = StyleContext;
            var width = AllocatedWidth;
            var height = AllocatedHeight;

            context.RenderBackground(cr, 0, 0, width, height);

            float scale = 1;

            float x = 0;
            while (x <= width)
            {
                float y = 0;
                while (y <= height)
                {

                    cr.Rectangle(x, y, scale, scale);

                    y += 10 * scale;
                }


                x += 10 * scale;
            }

            //cr.Arc(width / 2.0, height / 2.0, Math.Min(width, height) / 2.0, 0, 2 * Math.PI);

            var color = context.GetColor(context.State);
            cr.SetSourceColor(new Color(color.Red, color.Green, color.Blue, color.Alpha));

            cr.Fill();

            foreach (var instance in Instances)
            {
                cr.Rectangle(instance.Position, instance.Width, instance.Height);
                cr.SetSourceColor(instance.Color);
                cr.Fill();
            }

            //return base.OnDrawn(cr);
        }
    }
}
