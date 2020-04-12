using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogikUI.Circut
{
    enum Orientation
    {
        North,
        East,
        South,
        West,
    }

    struct AndGate
    {
        public Vector2i Pos;
        public Orientation Orientation;

        public AndGate(Vector2i pos, Orientation orientation)
        {
            Pos = pos;
            Orientation = orientation;
        }
    }

    class Gates
    {
        public AndGate[] AndGates;

        public Gates(AndGate[] andGates)
        {
            AndGates = andGates;
        }

        public void Draw(Cairo.Context cr)
        {
            AndGate(cr, AndGates);
        }

        public void AndGate(Cairo.Context cr, AndGate[] gates)
        {
            foreach (var gate in gates)
            {
                double x = gate.Pos.X * CircutEditor.DotSpacing;
                double y = gate.Pos.Y * CircutEditor.DotSpacing - Wires.WireWidth * 1.5;

                double size = CircutEditor.DotSpacing * 2;

                cr.Rectangle(x, y, size, size + Wires.WireWidth * 3);
            }
            cr.SetSourceRGB(0.9, 0.9, 0.9);
            cr.FillPreserve();
            cr.SetSourceRGB(0.1, 0.1, 0.1);
            cr.LineWidth = Wires.WireWidth;
            cr.Stroke();

            // Connection points
            foreach (var gate in gates)
            {
                var in1 = (gate.Pos + new Vector2i(0, 0)) * CircutEditor.DotSpacing;
                var in2 = (gate.Pos + new Vector2i(0, 2)) * CircutEditor.DotSpacing;
                var out1 = (gate.Pos + new Vector2i(2, 1)) * CircutEditor.DotSpacing;

                // FIXME: Magic number radius...
                cr.Arc(in1.X, in1.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
                cr.Arc(in2.X, in2.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
                cr.Arc(out1.X, out1.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
            }
            cr.SetSourceRGB(0.3, 0.3, 0.3);
            cr.Fill();
        }
    }
}
