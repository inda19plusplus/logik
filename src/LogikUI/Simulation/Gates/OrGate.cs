using Atk;
using Cairo;
using LogikUI.Circuit;
using LogikUI.Util;
using System;
using System.Numerics;

namespace LogikUI.Simulation.Gates
{
    class OrGate : IComponent
    {
        public string Name => "Or Gate";
        public ComponentType Type => ComponentType.Or;
        public int NumberOfPorts => 3;

        public void GetPorts(Span<Vector2i> ports)
        {
            ports[0] = new Vector2i(0, 0);
            ports[1] = new Vector2i(-3, -1);
            ports[2] = new Vector2i(-3, 1);
        }

        public void Draw(Context cr, InstanceData data)
        {
            using var transform = IComponent.ApplyComponentTransform(cr, data);

            var gate = data;
            //foreach (var gate in instances)
            {
                var c1 = new Vector2d(-3.264, -2.448) * CircuitEditor.DotSpacing;
                var c2 = new Vector2d(-3.264, +2.448) * CircuitEditor.DotSpacing;
                var c3 = new Vector2d(-6.086, 0) * CircuitEditor.DotSpacing;

                const double r1 = 4.08 * CircuitEditor.DotSpacing;
                const double r2 = 3.264 * CircuitEditor.DotSpacing;
                cr.NewSubPath();
                cr.Arc(c1.X, c1.Y, r1, 37 * MathUtil.D2R, (37 + 53) * MathUtil.D2R);
                cr.NewSubPath();
                cr.Arc(c2.X, c2.Y, r1, -90 * MathUtil.D2R, (-90 + 53) * MathUtil.D2R);
                cr.NewSubPath();
                cr.Arc(c3.X, c3.Y, r2, -30 * MathUtil.D2R, (-30 + 60) * MathUtil.D2R);
            }
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
