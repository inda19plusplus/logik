using Gtk;
using LogikUI.Transaction;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public Vector2i EndPos => GetEndPosition();

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

        public Vector2i GetEndPosition()
        {
            return Pos + (Direction == Direction.Vertical ?
                new Vector2i(0, Length) :
                new Vector2i(Length, 0));
        }

        public bool IsPointOnWire(Vector2i point)
        {
            var diff = point - Pos;
            if (diff.X * diff.Y != 0)
                return false;
            else if (Direction == Direction.Vertical && diff.X == 0)
                return diff.Y >= 0 && diff.Y <= Length;
            else if (Direction == Direction.Horizontal && diff.Y == 0)
                return diff.X >= 0 && diff.Y <= Length;
            else return false;
        }

        public override string ToString()
        {
            return $"Pos: {Pos}, Length: {Length}, Direction: {Direction}";
        }
    }

    class Wires
    {
        public List<Wire> WiresList;
        public Wire[] Powered;
        public Wire[] UnPowered;
        public Vector2i[] PoweredConnections;
        public Vector2i[] UnPoweredConnections;

        public Wires(Wire[] powered, Wire[] unPowered, Vector2i[] poweredConnections, Vector2i[] unPoweredConnections)
        {
            WiresList = new List<Wire>(powered);
            WiresList.AddRange(unPowered);
            Powered = powered;
            UnPowered = unPowered;
            PoweredConnections = poweredConnections;
            UnPoweredConnections = unPoweredConnections;
        }

        public const double WireWidth = 2;
        public const double HalfWireWidth = WireWidth / 2;

        public const double ConnectionRadius = 2.5;

        public void Draw(Cairo.Context cr)
        {
            WireArray(cr, WiresList);
            cr.SetSourceRGB(0.2, 0.9, 0.2);
            cr.Fill();

            //WireArray(cr, Powered);
            //cr.SetSourceRGB(0.2, 0.9, 0.2);
            //cr.Fill();

            //WireArray(cr, UnPowered);
            //cr.SetSourceRGB(0.1, 0.4, 0.1);
            //cr.Fill();

            foreach (var connection in FindConnectionPoints(WiresList))
            {
                double x = connection.X * CircuitEditor.DotSpacing;
                double y = connection.Y * CircuitEditor.DotSpacing;
                cr.Arc(x, y, ConnectionRadius, 0, Math.PI * 2);
                cr.ClosePath();
            }
            cr.SetSourceRGB(0.2, 0.9, 0.2);
            cr.Fill();

            /*
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
            */
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

        public void WireArray(Cairo.Context cr, List<Wire> wires)
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

        public static List<Vector2i> FindConnectionPoints(List<Wire> wires)
        {
            // Calculate all points
            Vector2i[] points = new Vector2i[wires.Count * 2];
            for (int i = 0; i < wires.Count; i++)
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
        
        public WireTransaction AddWire(Wire wire)
        {
            var wireEnd = wire.GetEndPosition();

            List<Wire> Deleted = new List<Wire>();
            List<Wire> Added = new List<Wire>();
            foreach (var bWire in WiresList)
            {
                bool connected = false;

                // Check if any of the endpoins lands on any of the existing wires.
                if (bWire.IsPointOnWire(wire.Pos))
                {
                    connected = true;

                    // We need to connect the start of the wire with the existing wire
                    if (wire.Direction == bWire.Direction)
                    {
                        // Here we need to check the ends and stuff...
                        throw new NotImplementedException("Merging wires that go in the same direction");
                    }
                    else
                    {
                        // Here we do a clean connect with by dividing the line in half

                        // The diff will be in non-zero in only one direction so this will work fine.
                        var diff = wire.Pos - bWire.Pos;
                        Deleted.Add(bWire);
                        Added.Add(new Wire(bWire.Pos, diff.ManhattanDistance, bWire.Direction));
                        Added.Add(new Wire(bWire.Pos + diff, bWire.Length - diff.ManhattanDistance, bWire.Direction));
                    }
                }
                
                if (bWire.IsPointOnWire(wireEnd))
                {
                    connected = true;

                    if (wire.Direction == bWire.Direction)
                    {
                        // Here we need to check the ends and stuff...
                        throw new NotImplementedException("Merging wires that go in the same direction");
                    }
                    else
                    {
                        // Here we do a clean connect with by dividing the line in half

                        // The diff will be in non-zero in only one direction so this will work fine.
                        var diff = wireEnd - bWire.Pos;
                        Deleted.Add(bWire);
                        Added.Add(new Wire(bWire.Pos, diff.ManhattanDistance, bWire.Direction));
                        Added.Add(new Wire(bWire.Pos + diff, bWire.Length - diff.ManhattanDistance, bWire.Direction));
                    }
                }

                // If the wire we wanted to add didn't have end point on this wire
                // we need to check in this wire has endpoints that touch this wire.
                if (connected == false)
                {
                    // FIXME: If one of the bWires ends is inside of this wire we need to
                    // split this wire.
                    if (wire.IsPointOnWire(bWire.Pos) || wire.IsPointOnWire(bWire.EndPos))
                        throw new NotImplementedException("Splitting the currently added wire...");
                }
            }

            foreach (var dwire in Deleted)
            {
                WiresList.Remove(dwire);
                Console.WriteLine($"Removed: {dwire}");
            }

            Console.WriteLine($"Replaced by:");
            foreach (var awire in Added)
            {
                if (awire.Length != 0)
                {
                    WiresList.Add(awire);
                    Console.WriteLine($"- {awire}");
                }
            }

            WiresList.Add(wire);
            Console.WriteLine($"Added: {wire}");

            return new WireTransaction(Deleted, Added);
        }

        public void RevertTransaction(WireTransaction transaction)
        {
            
        }
    }
}
