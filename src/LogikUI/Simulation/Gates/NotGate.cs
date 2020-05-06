using Atk;
using Cairo;
using LogikUI.Circuit;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Simulation.Gates
{
    class NotGate : IComponent
    {
        public string Name => "Not Gate";
        public ComponentType Type => ComponentType.Not;
        public int NumberOfPorts => 2;

        public void GetPorts(Span<Vector2i> ports)
        {
            ports[0] = new Vector2i(0, 0);
            ports[0] = new Vector2i(-3, 0);
        }
        
        public void Draw(Context cr, InstanceData data)
        {
            using var transform = IComponent.ApplyComponentTransform(cr, data);

            double height = CircuitEditor.DotSpacing * 1.5;
            double width = CircuitEditor.DotSpacing * 3;

            cr.LineJoin = LineJoin.Miter;
            //foreach (var gate in instances)
            {
                var p1 = new Vector2d(-width, height / 2);
                var p2 = new Vector2d(-width, -height / 2);
                var p3 = new Vector2d(-width * 0.365, 0);
                var p4 = new Vector2d(-width * 0.15, 0);

                cr.MoveTo(p1);
                cr.LineTo(p2);
                cr.LineTo(p3);
                cr.ClosePath();

                const double r = 4.8;
                cr.MoveTo(p4 + new Vector2d(r + 0.2, 0));
                cr.Arc(p4.X, p4.Y, r, 0, Math.PI * 2);
                cr.ClosePath();
            }
            // FIXME: We probably shouldn't hardcode the color
            cr.SetSourceRGB(0.1, 0.1, 0.1);

            cr.LineWidth = Wires.WireWidth;
            cr.Stroke();

            //foreach (var gate in instances)
            {
                var p1 = new Vector2i(-3, 0);

                var in1 = p1 * CircuitEditor.DotSpacing;
                var out1 = Vector2d.Zero;

                // FIXME: Magic number radius...
                cr.Arc(in1.X, in1.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
                cr.Arc(out1.X, out1.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
            }
            cr.SetSourceRGB(0.2, 0.9, 0.2);
            cr.Fill();
        }
    }
}
