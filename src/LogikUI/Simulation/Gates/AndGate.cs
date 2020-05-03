using Cairo;
using LogikUI.Circuit;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogikUI.Simulation.Gates
{
    struct AndGateInstance : IInstance, IEquatable<AndGateInstance>
    {
        // Indices for the ports
        public string Name => "And Gate";
        public Vector2i Position { get; set; }
        public Orientation Orientation { get; set; }
        public int NumberOfPorts => 3;
        public UIntPtr BackendIdx { get; set; }

        UIntPtr IInstance.BackendId => new UIntPtr(1);

        public void GetPorts(Span<Vector2i> ports)
        {
            ports[0] = new Vector2i(0, 0);
            ports[1] = new Vector2i(-3, -1);
            ports[2] = new Vector2i(-3, 1);
        }

        public static void Draw(Context cr, Span<AndGateInstance> instances)
        {
            double height = CircuitEditor.DotSpacing * 3;
            double width = CircuitEditor.DotSpacing * 3;

            foreach (var gate in instances)
            {
                double horiz = gate.Orientation == Orientation.East ?
                    -1 : gate.Orientation == Orientation.West ?
                    1 : 0;
                double vert = gate.Orientation == Orientation.South ?
                    -1 : gate.Orientation == Orientation.North ?
                    1 : 0;

                var pos = gate.Position * CircuitEditor.DotSpacing;
                var p1 = pos + horiz * new Vector2d(width, height / 2) + vert * new Vector2d(-width / 2, height);
                var p2 = pos + horiz * new Vector2d(width, -height / 2) + vert * new Vector2d(width / 2, height);
                var p3 = pos + new Vector2d(horiz * (width / 2), vert * (height / 2));

                // FIXME: This might be simplyfiable. If it's not maybe write a comment about why this works.
                double a1 = horiz * (Math.PI / 2) + ((-vert + 3 * Math.Abs(vert)) / 2) * (Math.PI);
                double a2 = a1 + Math.PI;

                cr.MoveTo(p1);
                cr.Arc(p3.X, p3.Y, width / 2, a1, a2);
                cr.LineTo(p2);
                cr.ClosePath();
            }

            // FIXME: We probably shouldn't hardcode the color
            cr.SetSourceRGB(0.1, 0.1, 0.1);
            cr.LineWidth = Wires.WireWidth;
            cr.Stroke();

            foreach (var gate in instances)
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

        public override bool Equals(object? obj)
        {
            return obj is AndGateInstance instance && Equals(instance);
        }

        public bool Equals(AndGateInstance other)
        {
            return Name == other.Name &&
                   Position.Equals(other.Position) &&
                   Orientation == other.Orientation &&
                   NumberOfPorts == other.NumberOfPorts;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Position, Orientation, NumberOfPorts);
        }

        public static bool operator ==(AndGateInstance left, AndGateInstance right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AndGateInstance left, AndGateInstance right)
        {
            return !(left == right);
        }
    }

    class AndGate
    {
        public string Name => "And Gate";

        public void InitDefault(ref AndGateInstance instance)
        {
        }

        public void Propagate(Engine engine, Span<AndGateInstance> instances)
        {
            // We don't worry about the simulation!
            throw new NotImplementedException();
        }

        
    }
}
