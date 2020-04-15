using Gtk;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace LogikUI.Circuit
{
    enum Direction
    {
        Vertical,
        Horizontal,
    }

    struct Wire {
        public Vector2i Pos;
        public int Length;
        public Direction Direction;

        public Wire(Vector2i pos, int length, Direction orientation)
        {
            Pos = pos;
            Length = length;
            Direction = orientation;

            // TODO: Remove this as we shouldn't create wires with negative length
            if (Length < 0)
            {
                int vertical = Direction == Direction.Vertical ? 1 : 0;
                int horizontal = 1 - vertical;

                Pos += new Vector2i(horizontal, vertical) * Length;
                Length = -Length;
            }
        }
    }

    class Wires
    {
        public Wire[] Powered;
        public Wire[] UnPowered;
        public Vector2i[] PoweredConnections;
        public Vector2i[] UnPoweredConnections;

        public Wires(Wire[] powered, Wire[] unPowered, Vector2i[] poweredConnections, Vector2i[] unPoweredConnections)
        {
            PoweredConnections = poweredConnections;
            UnPoweredConnections = unPoweredConnections;
            Powered = powered;
            UnPowered = unPowered;
        }

        public const double WireWidth = 2;
        public const double HalfWireWidth = WireWidth / 2;

        public const double ConnectionRadius = 2.5;

        public void Draw(Cairo.Context cr)
        {
            WireArray(cr, Powered);
            cr.SetSourceRGB(0.2, 0.9, 0.2);
            cr.Fill();

            WireArray(cr, UnPowered);
            cr.SetSourceRGB(0.1, 0.4, 0.1);
            cr.Fill();

            foreach (var connection in PoweredConnections)
            {
                double x = connection.X * CircuitEditor.DotSpacing;
                double y = connection.Y * CircuitEditor.DotSpacing;
                cr.Arc(x, y, ConnectionRadius, 0, Math.PI * 2);
                cr.ClosePath();
            }
            cr.SetSourceRGB(0.2, 0.9, 0.2);
            cr.Fill();

            foreach (var connection in UnPoweredConnections)
            {
                double x = connection.X * CircuitEditor.DotSpacing;
                double y = connection.Y * CircuitEditor.DotSpacing;
                cr.Arc(x, y, ConnectionRadius, 0, Math.PI * 2);
                cr.ClosePath();
            }
            cr.SetSourceRGB(0.1, 0.4, 0.1);
            cr.Fill();
        }

        public void WireArray(Cairo.Context cr, Wire[] wires)
        {
            foreach (var wire in wires)
            {
                int vertical = wire.Direction == Direction.Vertical ? 1 : 0;
                int horizontal = 1 - vertical;

                double x = wire.Pos.X * CircuitEditor.DotSpacing - HalfWireWidth;
                double y = wire.Pos.Y * CircuitEditor.DotSpacing - HalfWireWidth;

                double length = CircuitEditor.DotSpacing * wire.Length;
                // If we are drawing a horizontal line the width is length, othervise it's WireWidth.
                double width = (horizontal * (length + WireWidth)) + (vertical * WireWidth);
                // The opposite of the above.
                double height = (vertical * (length + WireWidth)) + (horizontal * WireWidth);

                cr.Rectangle(x, y, width, height);
            }
        }

        public static List<Vector2i> FindConnectionPoints(Wire[] wires)
        {
            // Calculate all points
            Vector2i[] points = new Vector2i[wires.Length * 2];
            for (int i = 0; i < wires.Length; i++)
            {
                points[i * 2] = wires[i].Pos;
                
                int vert = wires[i].Direction == Direction.Vertical ? 1 : 0;
                int horiz = 1 - vert;
                points[i * 2 + 1] = wires[i].Pos + new Vector2i(horiz, vert) * wires[i].Length;
            }

            Array.Sort(points, (p1, p2) => p1.X == p2.X ? p1.Y - p2.Y : p1.X - p2.X);

            List<Vector2i> connections = new List<Vector2i>();

            Vector2i last = points[0];
            int count = 0;
            for (int i = 1; i < points.Length; i++)
            {
                if (last == points[i]) count++;
                else
                {
                    if (count > 1) connections.Add(last);
                    count = 0;
                }
                last = points[i];
            }

            return connections;
        }
    }
}
