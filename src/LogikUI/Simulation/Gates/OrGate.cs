using Atk;
using Cairo;
using LogikUI.Circuit;
using LogikUI.Util;
using System;

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
            var gate = data;
            // FIXME: We want an easy way to apply the rotation of an instance
            //foreach (var gate in instances)
            {
                // FIXME: Consider orientation!
                // We should create a nicer way to do that...
                var c1 = (gate.Position + new Vector2d(-3.264, -2.448)) * CircuitEditor.DotSpacing;
                var c2 = (gate.Position + new Vector2d(-3.264, +2.448)) * CircuitEditor.DotSpacing;
                var c3 = (gate.Position + new Vector2d(-6.086, 0)) * CircuitEditor.DotSpacing;

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
                // FIXME: We kind of don't want to do this again?
                int horiz = gate.Orientation == Orientation.East ?
                    -1 : gate.Orientation == Orientation.West ?
                    1 : 0;
                int vert = gate.Orientation == Orientation.South ?
                    -1 : gate.Orientation == Orientation.North ?
                    1 : 0;

                var p1 = horiz * new Vector2i(3, 1) + vert * new Vector2i(1, 3);
                var p2 = horiz * new Vector2i(3, -1) + vert * new Vector2i(-1, 3);

                var in1 = (gate.Position + p1) * CircuitEditor.DotSpacing;
                var in2 = (gate.Position + p2) * CircuitEditor.DotSpacing;
                var out1 = gate.Position * CircuitEditor.DotSpacing;

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
