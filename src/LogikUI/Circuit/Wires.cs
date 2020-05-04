using Gtk;
using LogikUI.Simulation;
using LogikUI.Transaction;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LogikUI.Circuit
{
    enum Direction
    {
        Vertical,
        Horizontal,
    }

    // FIXME: Move to it's own file.
    struct Wire : IEquatable<Wire>
    {
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

        /// <summary>
        /// Checks to see if a point lays on the wire including the wires start and end points.
        /// </summary>
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

        /// <summary>
        /// Checks to see if a point lays on the wire excluding the wires start and end points.
        /// </summary>
        public bool IsPointInsideWire(Vector2i point)
        {
            var diff = point - Pos;
            if (diff.X * diff.Y != 0)
                return false;
            else if (Direction == Direction.Vertical && diff.X == 0)
                return diff.Y > 0 && diff.Y < Length;
            else if (Direction == Direction.Horizontal && diff.Y == 0)
                return diff.X > 0 && diff.X < Length;
            else return false;
        }

        public bool IsConnectionPoint(Vector2i point)
        {
            return Pos == point || EndPos == point;
        }

        public override string ToString()
        {
            return $"Pos: {Pos}, Length: {Length}, Direction: {Direction}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Wire wire && Equals(wire);
        }

        public bool Equals(Wire other)
        {
            return Pos.Equals(other.Pos) &&
                   Length == other.Length &&
                   Direction == other.Direction;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Pos, Length, Direction);
        }

        public static bool operator ==(Wire left, Wire right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Wire left, Wire right)
        {
            return !(left == right);
        }
    }

    class Wires
    {
        public List<Wire> WiresList;
        public List<WireBundle> Bundles;

        public Wires(Wire[] wires)
        {
            WiresList = new List<Wire>(wires);
            Bundles = CreateBundlesFromWires(WiresList);
        }

        public const double WireWidth = 2;
        public const double HalfWireWidth = WireWidth / 2;

        public const double ConnectionRadius = 2.5;

        public static Cairo.Color GetValueColor(Value value)
        {
            if (value.Width == 1)
            {
                switch ((ValueState)value.Values)
                {
                    case ValueState.Floating:
                        return new Cairo.Color(0.2, 0.2, 0.9);
                    case ValueState.Zero:
                        return new Cairo.Color(0.1, 0.4, 0.1);
                    case ValueState.One:
                        return new Cairo.Color(0.2, 0.9, 0.2);
                    case ValueState.Error:
                        return new Cairo.Color(0.9, 0.2, 0.2);
                    default:
                        throw new InvalidEnumArgumentException(nameof(value), (int)value.Values, typeof(ValueState));
                }
            }
            else return new Cairo.Color(0.3, 0.3, 0.3);
        }

        public void Draw(Cairo.Context cr)
        {
            //WireArray(cr, WiresList);
            //cr.SetSourceRGB(0.2, 0.9, 0.2);
            //cr.SetSourceColor(GetValueColor(Value.Error));
            //cr.Fill();

            foreach (var bundle in Bundles)
            {
                WireArray(cr, bundle.Wires);

                foreach (var connection in FindConnectionPoints(bundle.Wires))
                {
                    double x = connection.X * CircuitEditor.DotSpacing;
                    double y = connection.Y * CircuitEditor.DotSpacing;
                    cr.Arc(x, y, ConnectionRadius, 0, Math.PI * 2);
                    cr.ClosePath();
                }

                cr.SetSourceRGB(0.2, 0.9, 0.2);
                cr.SetSourceColor(GetValueColor(bundle.Subnet?.Value ?? Value.Floating));
                cr.Fill();
            }
            
            //WireArray(cr, Powered);
            //cr.SetSourceRGB(0.2, 0.9, 0.2);
            //cr.Fill();

            //WireArray(cr, UnPowered);
            //cr.SetSourceRGB(0.1, 0.4, 0.1);
            //cr.Fill();

            /*
            foreach (var connection in FindConnectionPoints(WiresList))
            {
                double x = connection.X * CircuitEditor.DotSpacing;
                double y = connection.Y * CircuitEditor.DotSpacing;
                cr.Arc(x, y, ConnectionRadius, 0, Math.PI * 2);
                cr.ClosePath();
            }
            cr.SetSourceRGB(0.2, 0.9, 0.2);
            cr.Fill();*/

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
        
        public WireTransaction? CreateAddWireTransaction(Wire wire)
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
            // At the same time we check to see if there is a wire that 
            // contains both ends of the wire we added.
            // In this case we shouldn't create or delete any wires because
            // then there will be two wires on the same space.
            Wire? containingWire = null;
            foreach (var bWire in WiresList)
            {
                // We check to see if the point is *inside*
                // the wire because if the new wire touches
                // the other wire at it's end point we don't
                // need to do anything to it. It's only if the
                // new point is on the inside of the wire that
                // we need to do anything.

                bool containedStart = false;
                if (bWire.IsPointInsideWire(wire.Pos))
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

                    containedStart = true;
                }

                if (bWire.IsPointInsideWire(wire.EndPos))
                {
                    if (wire.Direction != bWire.Direction)
                    {
                        // The diff will be in non-zero in only one direction so this will work fine.
                        var diff = wire.EndPos - bWire.Pos;
                        Deleted.Add(bWire);
                        Added.Add(new Wire(bWire.Pos, diff.ManhattanDistance, bWire.Direction));
                        Added.Add(new Wire(bWire.Pos + diff, bWire.Length - diff.ManhattanDistance, bWire.Direction));
                    }

                    // This means that both the start and the end point of this wire was contained
                    // inside of bWire. So we basically shouldn't create any transaction.s
                    if (containedStart)
                    {
                        containingWire = bWire;
                        // We don't care about any more wires so it's fine to break.
                        break;
                    }
                }
            }

            // If the wire we are adding is entrierly contained within another wire
            // the addition won't change any state so we don't create a transaction for that.
            if (containingWire.HasValue)
            {
                // This addition wouldn't change any state.
                return null;
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

            // FIXME: We want to deduplicate the values in Added and Deleted
            // If both lists contain exactly the same wires we want to return
            // null instead as the transaction doesn't change anything.

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

        public WireTransaction CreateRemoveWireTransaction(Wire wire)
        {
            // When removing a wire we want to check and see if there are
            // any wires that should be merged after the removal.

            // We can think wire removal as removing two connection points,
            // we don't need to think about the points inside of the wire
            // we are removing because there shouldn't be anything connecting there.

            List<Wire> Added = new List<Wire>();
            List<Wire> Deleted = new List<Wire>();

            // We are deleting this wire.
            Deleted.Add(wire);

            List<Wire> StartPosAffectedWires = new List<Wire>();
            List<Wire> EndPosAffectedWires = new List<Wire>();
            foreach (var bWire in WiresList)
            {
                // Ignore the wire we are removing.
                if (bWire == wire) continue;

                if (bWire.IsConnectionPoint(wire.Pos))
                {
                    StartPosAffectedWires.Add(bWire);
                }

                if (bWire.IsConnectionPoint(wire.EndPos))
                {
                    EndPosAffectedWires.Add(bWire);
                }
            }

            // Do some sanity checking to see that nothing is going horribly wrong.
            
            if (StartPosAffectedWires.Count > 3)
            {
                Console.WriteLine($"Warn: The removed wires start position connected to more than 3 wires. This should be impossible!\n{string.Join(", ", StartPosAffectedWires.Select(w => $"({w})"))}");
            }

            if (EndPosAffectedWires.Count > 3)
            {
                Console.WriteLine($"Warn: The removed wires end position connected to more than 3 wires. This should be impossible!\n{string.Join(", ", EndPosAffectedWires.Select(w => $"({w})"))}");
            }

            // Now we have a list of the wires that could be affected by this wire removal
            // We want to find the wires that should merge. These are the wires that
            // connect and have the same direction even after the wire we are removing
            // has been removed.

            // We are only ever going to merge wires if there are two of them,
            // and only if they are going in the same direction.
            if (StartPosAffectedWires.Count == 2)
            {
                Wire a = StartPosAffectedWires[0];
                Wire b = StartPosAffectedWires[1];
                if (a.Direction == b.Direction)
                {
                    // Here we are going to do a merge.
                    Deleted.Add(a);
                    Deleted.Add(b);

                    var minPos = Vector2i.ComponentWiseMin(a.Pos, b.Pos);
                    var maxPos = Vector2i.ComponentWiseMax(a.EndPos, b.EndPos);
                    var diff = maxPos - minPos;

                    Wire merged;
                    merged.Direction = a.Direction;
                    merged.Pos = minPos;
                    merged.Length = diff.ManhattanDistance;
                    Added.Add(merged);
                }
            }

            if (EndPosAffectedWires.Count == 2)
            {
                Wire a = EndPosAffectedWires[0];
                Wire b = EndPosAffectedWires[1];
                if (a.Direction == b.Direction)
                {
                    // Here we are going to do a merge.
                    Deleted.Add(a);
                    Deleted.Add(b);

                    var minPos = Vector2i.ComponentWiseMin(a.Pos, b.Pos);
                    var maxPos = Vector2i.ComponentWiseMax(a.EndPos, b.EndPos);
                    var diff = maxPos - minPos;

                    Wire merged;
                    merged.Direction = a.Direction;
                    merged.Pos = minPos;
                    merged.Length = diff.ManhattanDistance;
                    Added.Add(merged);
                }
            }

            // Add the changes to the transaction.
            return new WireTransaction(wire, Deleted, Added);
        }

        public WireTransaction? CreateModifyWireTransaction(Wire modify, Wire modified)
        {
            // Here we need to consider the case where one of the points have moved
            // This means that the point that was should maybe trigger a merge
            // and that the new point should maybe trigger a split.

            // First thing is to find the point that is being moved
            Vector2i removedPosition;
            Vector2i addedPosition;
            if (modify.Pos == modified.Pos)
            {
                // They have the same start point so it's the end points that changed.
                removedPosition = modify.EndPos;
                addedPosition = modified.EndPos;
            }
            else
            {
                // They have the same end pos so it's the start point that changed.
                removedPosition = modify.Pos;
                addedPosition = modified.Pos;
            }

            List<Wire> Added = new List<Wire>();
            List<Wire> Deleted = new List<Wire>();

            // Remove the old wire and add the new one
            Deleted.Add(modify);
            Added.Add(modified);

            // Check merge at removed point
            {
                // There can only be three wires connected to this point
                Span<Wire> removedPointWires = stackalloc Wire[3];
                int wiresFound = 0;
                foreach (var bWire in WiresList)
                {
                    if (bWire == modify) continue;

                    if (bWire.IsPointOnWire(removedPosition))
                    {
                        removedPointWires[wiresFound] = bWire;
                        wiresFound++;
                    }
                }

                // Here we should check if we should merge the wiress
                if (wiresFound == 2)
                {
                    Wire a = removedPointWires[0];
                    Wire b = removedPointWires[1];
                    if (a.Direction == b.Direction)
                    {
                        Deleted.Add(a);
                        Deleted.Add(b);

                        var minPos = Vector2i.ComponentWiseMin(a.Pos, b.Pos);
                        var maxPos = Vector2i.ComponentWiseMax(a.EndPos, b.EndPos);
                        var diff = maxPos - minPos;

                        Wire merged;
                        merged.Direction = a.Direction;
                        merged.Pos = minPos;
                        merged.Length = diff.ManhattanDistance;
                        Added.Add(merged);
                    }
                }
            }

            // Now that we have potentially merged the wires
            // we want to check if there is any wire on the 
            // added point that should be split
            {
                foreach (var bWire in WiresList)
                {
                    if (bWire == modify) continue;

                    // If the new position is on this wire we want to split it
                    if (bWire.IsPointInsideWire(addedPosition))
                    {
                        // If the wires go in different direction we should split them.
                        if (bWire.Direction != modified.Direction)
                        {
                            // The diff will be in non - zero in only one direction so this will work fine.
                            var diff = addedPosition - bWire.Pos;
                            Deleted.Add(bWire);
                            Added.Add(new Wire(bWire.Pos, diff.ManhattanDistance, bWire.Direction));
                            Added.Add(new Wire(bWire.Pos + diff, bWire.Length - diff.ManhattanDistance, bWire.Direction));

                            // After we have done this there cannot be any more wires to split so we break
                            break;
                        }
                    }
                }
            }

            return new WireTransaction(modify, Deleted, Added);
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

            // Re-calculate the bundles
            // FIXME: We might want to make this more efficient!
            Bundles = CreateBundlesFromWires(WiresList);
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

            // Re-calculate the bundles
            // FIXME: We want to make this more efficient!
            Bundles = CreateBundlesFromWires(WiresList);
        }

        // FIXME: We might want to make this more efficient!
        public static List<WireBundle> CreateBundlesFromWires(List<Wire> wires)
        {
            HashSet<Vector2i> positions = new HashSet<Vector2i>();
            foreach (var w in wires)
            {
                positions.Add(w.Pos);
                positions.Add(w.EndPos);
            }

            UnionFind<Vector2i> nets = new UnionFind<Vector2i>(positions);

            foreach (var w in wires)
            {
                nets.Union(w.Pos, w.EndPos);
            }

            Dictionary<int, WireBundle> bundlesDict = new Dictionary<int, WireBundle>();
            foreach (var w in wires)
            {
                int root = nets.Find(w.Pos).Index;
                if (bundlesDict.TryGetValue(root, out var bundle) == false)
                {
                    bundle = new WireBundle();
                    bundlesDict[root] = bundle;
                }

                // Because C# doesn't detect that bundle != null here we do the '!'
                bundle!.AddWire(w);
            }

            var bundles = bundlesDict.Values.ToList();
            //Console.WriteLine($"Created bundles:");
            //foreach (var bundle in bundles)
            //{
            //    Console.WriteLine($"  Bundle:\n    {string.Join("\n    ", bundle.Wires)}\n\n");
            //}

            return bundles;
        }
    }
}
