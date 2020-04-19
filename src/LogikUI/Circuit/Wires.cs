using LogikUI.Transaction;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
                return diff.X >= 0 && diff.X <= Length;
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

        public void Wire(Cairo.Context cr, Wire wire)
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
        
        public WireTransaction CreateAddWireTransaction(Wire wire)
        {
            var wireEnd = wire.GetEndPosition();

            List<Wire> Deleted = new List<Wire>();
            List<Wire> Added = new List<Wire>();

            // This dictionary maps wires to the point where they intersect this wire
            // If there exists a mapping but the value is null, that means both ends
            // of the wire are contained inside of the addeed wire.
            Dictionary<Wire, Vector2i?> SplitPoints = new Dictionary<Wire, Vector2i?>();

            // First we get all of the points where we need to split the current wire
            foreach (var bWire in WiresList)
            {
                bool start = wire.IsPointOnWire(bWire.Pos);
                bool end = wire.IsPointOnWire(bWire.EndPos);
                if (start && end)
                    // Null indicates that both ends are contained in this wire
                    SplitPoints[bWire] = null;
                else if (start)
                    SplitPoints[bWire] = bWire.Pos;
                else if (end)
                    SplitPoints[bWire] = bWire.EndPos;
            }

            // FIXME: Add other split points like component connection points!

            // After we've found all of the intersection points we can check
            // if the ends of the added wire will split some other wire.
            foreach (var bWire in WiresList)
            {
                if (bWire.IsPointOnWire(wire.Pos))
                {
                    // We only care if the wires are going in different direction
                    // as that means we should split the wire.
                    // The case where they are going the same direction
                    // will be handled in the next step.
                    if (wire.Direction != bWire.Direction)
                    {
                        // The diff will be in non-zero in only one direction so this will work fine.
                        var diff = wire.Pos - bWire.Pos;
                        Deleted.Add(bWire);
                        Added.Add(new Wire(bWire.Pos, diff.ManhattanDistance, bWire.Direction));
                        Added.Add(new Wire(bWire.Pos + diff, bWire.Length - diff.ManhattanDistance, bWire.Direction));
                    }
                }

                if (bWire.IsPointOnWire(wire.EndPos))
                {
                    if (wire.Direction != bWire.Direction)
                    {
                        // The diff will be in non-zero in only one direction so this will work fine.
                        var diff = wire.EndPos - bWire.Pos;
                        Deleted.Add(bWire);
                        Added.Add(new Wire(bWire.Pos, diff.ManhattanDistance, bWire.Direction));
                        Added.Add(new Wire(bWire.Pos + diff, bWire.Length - diff.ManhattanDistance, bWire.Direction));
                    }
                }
            }

            List<Vector2i> Points = new List<Vector2i>();

            // Now we should merge or remove wires that are going in the same direction as us.
            Wire mergedWire = wire;
            foreach (var (splitWire, location) in SplitPoints)
            {
                if (location == null)
                {
                    // Both ends are contained. 
                    // So to merge we can just delete the contained wire.
                    Console.WriteLine($"Wire ({splitWire}) was completely contained in ({wire}), so it was removed.");
                    Deleted.Add(splitWire);
                    continue;
                }
                else if (wire.Direction == splitWire.Direction)
                {
                    // This means that we should merge the two wires.
                    // We can do that by deleting the existing wire and
                    // extending the wire we are adding to include that wire.
                    var minPos = Vector2i.ComponentWiseMin(mergedWire.Pos, splitWire.Pos);
                    var maxPos = Vector2i.ComponentWiseMax(mergedWire.EndPos, splitWire.EndPos);
                    var diff = maxPos - minPos;

                    Wire newMerged;
                    newMerged.Direction = wire.Direction;
                    newMerged.Pos = minPos;
                    newMerged.Length = diff.ManhattanDistance;
                    Console.WriteLine($"Merged ({splitWire}) with ({mergedWire}). Result: ({newMerged})");
                    mergedWire = newMerged;

                    Deleted.Add(splitWire);
                }
                else
                {
                    // This will not fail due to the null check above.
                    Console.WriteLine($"The wire ({splitWire}) should split the current wire at ({location}).");
                    if (Points.Contains(location.Value) == false) Points.Add(location.Value);
                }
            }

            // FIXME: If the added wire is entierly contained within another wire
            // we shouldn't add anything!

            // Lastly we split the merged wire on all of the remaining points.
            // We do this by first sorting the points in the wires direction 
            // and then go through linearly and add the parts of the wire.

            // We do the comparison outside of the lambda to avoid a capture.
            // (this is a premature optimization)
            Points.Sort(
                wire.Direction == Direction.Vertical ?
                (Comparison<Vector2i>)((v1, v2) => v1.Y - v2.Y) :
                ((v1, v2) => v1.X - v2.X));

            Vector2i pos = mergedWire.Pos;
            foreach (var split in Points)
            {
                Wire newWire;
                newWire.Direction = mergedWire.Direction;
                newWire.Pos = pos;
                newWire.Length = (split - pos).ManhattanDistance;
                if (newWire.Length < 0) Debugger.Break();
                pos = newWire.EndPos;
                Added.Add(newWire);
                Console.WriteLine($"Split wire at {split}. Result: ({newWire})");
            }
            // Here we need to add the last part of the wire
            Wire w = new Wire(pos, (mergedWire.EndPos - pos).ManhattanDistance, mergedWire.Direction);
            Added.Add(w);
            Console.WriteLine($"End part of split: {w}");

            // Remove all wires with zero length
            Added.RemoveAll(w =>
            {
                if (w.Length == 0)
                {
                    Console.WriteLine($"Warn: Trying to add a wire with zero length! {w}");
                    return true;
                }
                else return false;
            });

            // Now we just return the transaction.
            // To apply this transaction ApplyTransaction(...) must be called.

            return new WireTransaction(wire, Deleted, Added);
        }

        public void ApplyTransaction(WireTransaction transaction)
        {
            // Now we have figured out all of the additions and deletions.
            // So now we can actually add and remove the appropriate wires.
            foreach (var dwire in transaction.Deleted)
            {
                if (WiresList.Remove(dwire) == false)
                    Console.WriteLine($"Warn: Tried to remove a wire that didn't exist! {dwire}");
            }

            foreach (var awire in transaction.Created)
            {
                WiresList.Add(awire);
            }
        }

        public void RevertTransaction(WireTransaction transaction)
        {
            // FIXME: We want to know if this is the last transaction created for the wires.

            // We want to delete the wires that where created and recreate the ones removed

            foreach (var wire in transaction.Created)
            {
                if (WiresList.Remove(wire) == false)
                    Console.WriteLine($"Warn: Removing non-existent wire when reverting transaction! ({wire})");
            }

            foreach (var wire in transaction.Deleted)
            {
                WiresList.Add(wire);
            }
        }
    }
}
