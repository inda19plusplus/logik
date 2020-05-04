using Cairo;
using LogikUI.Circuit;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogikUI.Simulation.Gates
{
    class AndGate : IComponent
    {
        // Indices for the ports
        public string Name => "And Gate";
        public ComponentType Type => ComponentType.And;
        public int NumberOfPorts => 3;

        public void GetPorts(Span<Vector2i> ports)
        {
            ports[0] = new Vector2i(0, 0);
            ports[1] = new Vector2i(-3, -1);
            ports[2] = new Vector2i(-3, 1);
        }
        
        // FIXME: Cleanup and possibly split draw into a 'outline' and 'fill'
        // call so we can do more efficient cairo rendering.
        public void Draw(Context cr, InstanceData data)
        {
            using var transform = IComponent.ApplyComponentTransform(cr, data);

            double height = CircuitEditor.DotSpacing * 3;
            double width = CircuitEditor.DotSpacing * 3;

            //foreach (var gate in instances)
            {
                var p1 = new Vector2d(-width, -height / 2);
                var p2 = new Vector2d(-width, height / 2);
                var p3 = new Vector2d(-width / 2, 0);

                // FIXME: This might be simplyfiable. If it's not maybe write a comment about why this works.
                double a1 = Math.PI / 2;
                double a2 = a1 + Math.PI;

                cr.MoveTo(p1);
                cr.Arc(p3.X, p3.Y, width / 2, -Math.PI / 2, Math.PI / 2);
                cr.LineTo(p2);
                cr.ClosePath();
            }

            // FIXME: We probably shouldn't hardcode the color
            cr.SetSourceRGB(0.1, 0.1, 0.1);
            cr.LineWidth = Wires.WireWidth;
            cr.Stroke();

            //foreach (var gate in instances)
            {
                var p1 = new Vector2i(-3, 1);
                var p2 = new Vector2i(-3, -1);

                var in1 = p1 * CircuitEditor.DotSpacing;
                var in2 = p2 * CircuitEditor.DotSpacing;
                var out1 = Vector2d.Zero;

                // FIXME: Magic number radius...
                cr.Arc(in1.X, in1.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
                cr.Arc(in2.X, in2.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
                cr.Arc(out1.X, out1.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
            }
            cr.SetSourceRGB(0.2, 0.9, 0.2);
            cr.Fill();
        }
    }
}
