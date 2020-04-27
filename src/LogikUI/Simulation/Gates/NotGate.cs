using Atk;
using Cairo;
using LogikUI.Circuit;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Simulation.Gates
{
    struct NotGateInstance : IInstance, IEquatable<NotGateInstance>
    {
        public string Name => "Not Gate";
        public Vector2i Position { get; set; }
        public Orientation Orientation { get; set; }
        public int NumberOfPorts => 2;

        public IInstance Create(Vector2i pos, Orientation orientation)
        {
            // Copy all fields
            NotGateInstance instance = this;
            // Set the position and orientation
            instance.Position = pos;
            instance.Orientation = orientation;
            return instance;
        }

        public void GetPorts(Span<Vector2i> ports)
        {
            ports[0] = new Vector2i(0, 0);
            ports[0] = new Vector2i(-2, 0);
        }

        public static void Draw(Context cr, Span<NotGateInstance> instances)
        {
            if (instances.Length == 0) return;

            double height = CircuitEditor.DotSpacing * 1;
            double width = CircuitEditor.DotSpacing * 2;

            cr.LineJoin = LineJoin.Bevel;
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
                var p3 = pos + horiz * new Vector2d(width * 0.3, 0) + vert * new Vector2d(0, height * 0.3);
                var p4 = pos + horiz * new Vector2d(width * 0.15, 0) + vert * new Vector2d(0, height * 0.15);

                cr.MoveTo(p1);
                cr.LineTo(p2);
                cr.LineTo(p3);
                cr.ClosePath();

                const float r = 3;
                cr.MoveTo(p4 + new Vector2d(r, 0));
                cr.Arc(p4.X, p4.Y, r, 0, Math.PI * 2);
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

                var p1 = horiz * new Vector2i(2, 0) + vert * new Vector2i(0, 2);

                var in1 = (gate.Position + p1) * CircuitEditor.DotSpacing;
                var out1 = gate.Position * CircuitEditor.DotSpacing;

                // FIXME: Magic number radius...
                cr.Arc(in1.X, in1.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
                cr.Arc(out1.X, out1.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
            }
            cr.SetSourceRGB(0.2, 0.9, 0.2);
            cr.Fill();
        }

        public override bool Equals(object? obj)
        {
            return obj is NotGateInstance instance && Equals(instance);
        }

        public bool Equals(NotGateInstance other)
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

        public static bool operator ==(NotGateInstance left, NotGateInstance right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NotGateInstance left, NotGateInstance right)
        {
            return !(left == right);
        }
    }

    class NotGate
    {
        public string Name => "Not Gate";

        // FIXME: What should this do? Is this even needed?
        public void InitDefault(ref NotGateInstance instance) { }

        public void Propagate(Engine engine, Span<NotGateInstance> instances)
        {
            throw new InvalidOperationException();
        }
    }
}
