using LogikUI.Interop;
using LogikUI.Simulation;
using LogikUI.Simulation.Gates;
using LogikUI.Transaction;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

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
        // FIXME: We might not want to do it like this....
        public Gates Gates;

        public List<Wire> WiresList;
        public List<Subnet> Bundles = new List<Subnet>();

        // NOTE: This can be changed to a ref to Gates, idk what is better
        public List<Vector2i> ConnectionPoints = new List<Vector2i>();

        public Wires(Gates gates, Wire[] wires)
        {
            Gates = gates;

            WiresList = new List<Wire>(wires);
            Bundles = CreateBundlesFromWires(new List<Wire>(wires));
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
            foreach (var bundle in Bundles)
            {
                WireArray(cr, bundle.Wires);

                foreach (var connection in FindConnectionPoints(bundle.Wires, ConnectionPoints))
                {
                    double x = connection.X * CircuitEditor.DotSpacing;
                    double y = connection.Y * CircuitEditor.DotSpacing;
                    cr.Arc(x, y, ConnectionRadius, 0, Math.PI * 2);
                    cr.ClosePath();
                }

                // Get the value of the subnet if there is one.
                // FIXME: We shouldn't have to check that there is one...
                Value value = Value.Floating;
                if (bundle.ID != 0)
                {
                    var state = Logic.SubnetState(Program.Backend, bundle.ID);
                    value = state switch
                    {
                        ValueState.Floating => Value.Floating,
                        ValueState.Zero => Value.Zero,
                        ValueState.One => Value.One,
                        ValueState.Error => Value.Error,
                        _ => throw new InvalidOperationException($"We got an unknown subnet state from the backend! State: {state}"),
                    };
                }
                cr.SetSourceColor(GetValueColor(value));
                cr.Fill();
            }  

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

        public static List<Vector2i> FindConnectionPoints(List<Wire> wires, List<Vector2i> connectionPoints)
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

            for (int i = 0; i < points.Length; i++)
            {
                bool addConnection = false;
                int count = 1;
                Vector2i startOfGroup = points[i];

                // We always want a connection point if a wires connects
                // to a connection point.
                if (connectionPoints.Contains(startOfGroup))
                    addConnection = true;

                while (i + count < points.Length &&
                    startOfGroup == points[i + count])
                {
                    count++;
                }

                // We do -1 here because the for loop is going to add one.
                i += count - 1;

                // If there where more than one connection here
                // we want to display a connection here.
                if (count > 1) addConnection = true;

                if (addConnection) connections.Add(startOfGroup);
            }

            return connections;
        }
        
        public WireTransaction? CreateAddWireTransaction(Wire wire)
        {
            var wireEnd = wire.GetEndPosition();

            List<Wire> Deleted = new List<Wire>();
            List<Wire> Added = new List<Wire>();

            // FIXME: A better/cleaner order would be:
            // - Check if what wires the current wire splits
            //   - A the same time check if this wire is contained and early out in that case
            // - The figure out all of the splitpoints that are going to happen to this wire
            // - Merge this wire in with the splitpoint wires going in the same direction
            // - Add the connectionpoints to the points list and do the split of the current wire

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

            foreach (var connection in ConnectionPoints)
            {
                // We only need to check the added wire for these points
                // as the merge shouldn't merge over a connection.
                if (wire.IsPointInsideWire(connection))
                    Points.Add(connection);
            }

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
                else if (wire.Direction == splitWire.Direction && 
                    ConnectionPoints.Contains(location.Value) == false)
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

            // Here we check if either our start or end position
            // has a connection point, if we do we don't want to
            // merge in that direction.
            bool connectionPointAtStart = false;
            bool connectionPointAtEnd = false;
            foreach (var connection in ConnectionPoints)
            {
                connectionPointAtStart |= connection == wire.Pos;
                connectionPointAtEnd |= connection == wire.EndPos;
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
            // But if there is a connection point at that end we don't want to
            // merge because then we will merge over the connection point.
            if (connectionPointAtStart == false && StartPosAffectedWires.Count == 2)
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

            if (connectionPointAtEnd == false &&  EndPosAffectedWires.Count == 2)
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

            // FIXME: Consider connection points...

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
                bool removedPointHasConnection = false;
                foreach (var connection in ConnectionPoints)
                {
                    removedPointHasConnection |= removedPosition == connection;
                }

                // If there is a connection point at the removed position
                // we don't have to do any merging. We only have to consider
                // merging wires if there are no connection points at the
                // removed position.
                if (removedPointHasConnection == false)
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

        public ConnectionPointsTransaction CreateAddConnectionPointsTransaction(Span<Vector2i> connectionPoints)
        {
            // Here we want to check for wires that should be split as a result of adding the connection points.

            List<Wire> Added = new List<Wire>();
            List<Wire> Deleted = new List<Wire>();

            foreach (var connection in connectionPoints)
            {
                foreach (var bWire in WiresList)
                {
                    if (bWire.IsPointInsideWire(connection))
                    {
                        // Here we should split the wire
                        // The diff will be in non-zero in only one direction so this will work fine.
                        var diff = connection - bWire.Pos;
                        Deleted.Add(bWire);
                        Added.Add(new Wire(bWire.Pos, diff.ManhattanDistance, bWire.Direction));
                        Added.Add(new Wire(bWire.Pos + diff, bWire.Length - diff.ManhattanDistance, bWire.Direction));
                    }
                }
            }

            // No changes, so we don't create wire changes for this.
            if (Added.Count == 0 && Deleted.Count == 0)
                return new ConnectionPointsTransaction(false, connectionPoints, null, null);

            // FIXME: We want to de-deplicate the changes

            // FIXME: We want to do something better for the first argument!!!
            // This will lead to 0 debuggability as it will look like connectionpoints
            // are zero length wires at the origin...
            // We probably want to create it's own transaction type
            return new ConnectionPointsTransaction(false, connectionPoints, Deleted, Added);
        }

        public ConnectionPointsTransaction CreateRemoveConnectionPointsTransaction(Span<Vector2i> connectionPoints)
        {
            // Here we want to check for wires that should be merged as a result of removint the connection points.

            List<Wire> Added = new List<Wire>();
            List<Wire> Deleted = new List<Wire>();

            foreach (var connection in connectionPoints)
            {
                // There can only be four wires connected to this point
                Span<Wire> removedPointWires = stackalloc Wire[4];
                int wiresFound = 0;
                foreach (var bWire in WiresList)
                {
                    if (bWire.IsPointOnWire(connection))
                    {
                        removedPointWires[wiresFound] = bWire;
                        wiresFound++;
                    }
                }

                if (wiresFound == 2)
                {
                    // Here we should check if the two wires are going in the
                    // same direction, and merge them if they are.
                    Wire a = removedPointWires[0];
                    Wire b = removedPointWires[1];
                    if (a.Direction == b.Direction)
                    {
                        // Here we should merge these wires.
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

            // No changes, so we don't create wire changes for this.
            if (Added.Count == 0 && Deleted.Count == 0)
                return new ConnectionPointsTransaction(false, connectionPoints, null, null);

            // FIXME: We want to de-deplicate the changes

            // FIXME: We want to do something better for the first argument!!!
            // This will lead to 0 debuggability as it will look like connectionpoints
            // are zero length wires at the origin...
            // We probably want to create it's own transaction type
            return new ConnectionPointsTransaction(false, connectionPoints, Deleted, Added);
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

            // Update the subnets
            UpdateSubnets(transaction.Created, transaction.Deleted);
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

            // Update the subnets, but in reverse
            UpdateSubnets(transaction.Deleted, transaction.Created);
        }

        public void ApplyTransaction(ConnectionPointsTransaction transaction)
        {
            // Now we have figured out all of the additions and deletions.
            // So now we can actually add and remove the appropriate wires.
            if (transaction.DeletedWires != null)
            {
                foreach (var dwire in transaction.DeletedWires)
                {
                    if (WiresList.Remove(dwire) == false)
                        Console.WriteLine($"Warn: Tried to remove a wire that didn't exist! {dwire}");
                }
            }

            if (transaction.CreatedWires != null)
            {
                foreach (var awire in transaction.CreatedWires)
                {
                    WiresList.Add(awire);
                }
            }

            ConnectionPoints.AddRange(transaction.ControlPoints);

            // Update the subnets
            UpdateSubnets(
                transaction.CreatedWires ?? new List<Wire>(),
                transaction.DeletedWires ?? new List<Wire>()
                );
        }

        public void RevertTransaction(ConnectionPointsTransaction transaction)
        {
            // FIXME: We want to know if this is the last transaction created for the wires.

            // We want to delete the wires that where created and recreate the ones removed

            foreach (var wire in transaction.CreatedWires ?? Enumerable.Empty<Wire>())
            {
                if (WiresList.Remove(wire) == false)
                    Console.WriteLine($"Warn: Removing non-existent wire when reverting transaction! ({wire})");
            }

            foreach (var wire in transaction.DeletedWires ?? Enumerable.Empty<Wire>())
            {
                WiresList.Add(wire);
            }

            foreach (var point in transaction.ControlPoints)
            {
                if (ConnectionPoints.Remove(point) == false)
                    Console.WriteLine($"Warn: Removing non-existent connection point when reverting transaction! ({point})");
            }

            // Update the subnets, but in reverse
            UpdateSubnets(
                transaction.CreatedWires ?? new List<Wire>(),
                transaction.DeletedWires ?? new List<Wire>()
                );
        }

        private Subnet? FindSubnet(Vector2i pos)
        {
            foreach (var bundle in Bundles)
            {
                foreach (var wire in bundle.Wires)
                {
                    if (wire.IsPointOnWire(pos))
                    {
                        return bundle;
                    }
                        
                }
            }
            return null;
        }

        public void UpdateSubnets(List<Wire> added, List<Wire> removed)
        {
            List<Subnet> addedSubnets = new List<Subnet>();
            List<Subnet> deletedSubnets = new List<Subnet>();

            List<Subnet> checkSplitSubnets = new List<Subnet>();

            foreach (var old in removed)
            {
                var startNet = FindSubnet(old.Pos);
                var endNet = FindSubnet(old.EndPos);

                if (startNet != null && endNet != null)
                {
                    if (startNet == endNet)
                    {
                        // We are removing a wire from a subnet
                        // So here we want to figure out if we have to split this subnet
                        startNet.RemoveWire(old);
                        
                        // If there are no wires left, delete this subnet
                        // otherwise we need to check if the deletion
                        // led to any splits in the subnet
                        if (startNet.Wires.Count == 0)
                        {
                            deletedSubnets.Add(startNet);
                        }
                        else
                        {
                            checkSplitSubnets.Add(startNet);
                        }
                    }
                    else
                    {
                        // This is f***ed in more ways than one...
                        Console.WriteLine($"Error! This should not happen! Trying to remove a wire containing to more than one subnet! (Wire: {old}, Subnet1: {startNet}, Subnet2:{endNet})");
                    }
                }
                else if (startNet != null)
                {
                    // We are removing from one subnet
                    if (startNet.RemoveWire(old))
                    {
                        Console.WriteLine($"Warn: Tried to remove a wire from a subnet that didn't contain that wire. (Wire: {old}, Subnet: {startNet})");
                    }

                    if (startNet.Wires.Count == 0)
                        deletedSubnets.Add(startNet);

                    Console.WriteLine($"Removed wire to subnet: {startNet}");
                }
                else if (endNet != null)
                {
                    // We are removing from one subnet
                    if (endNet.RemoveWire(old))
                    {
                        Console.WriteLine($"Warn: Tried to remove a wire from a subnet that didn't contain that wire. (Wire: {old}, Subnet: {endNet})");
                    }

                    if (endNet.Wires.Count == 0)
                        deletedSubnets.Add(endNet);

                    Console.WriteLine($"Removed wire to subnet: {endNet}");
                }
                else
                {
                    // Here we are removing a wire that didn't belong to any subnet!?
                    Console.WriteLine($"Error! This should not happen! Trying to remove a wire not contained in any subnet! (Wire: {old})");
                }
            }

            foreach (var @new in added)
            {
                var startNet = FindSubnet(@new.Pos);
                var endNet = FindSubnet(@new.EndPos);

                if (startNet != null && endNet != null)
                {
                    if (startNet == endNet)
                    {
                        // Here they are the same subnet.
                        // So we just add the wire.
                        startNet.AddWire(@new);
                    }
                    else
                    {
                        // Here we need to merge the different subnets.
                        Console.WriteLine($"Merging subnet ({endNet}) into subnet ({startNet}).");
                        Bundles.Remove(endNet);
                        startNet.Merge(endNet);

                        // Don't forget to add the wire that merged these subnets
                        startNet.AddWire(@new);

                        Console.WriteLine($"\tResult: {startNet}");
                    }
                }
                else if (startNet != null)
                {
                    // Here we just add this wire to the subnet,
                    // it's not going to change anything.
                    startNet.AddWire(@new);
                    Console.WriteLine($"Added wire to subnet: {startNet}");
                }
                else if (endNet != null)
                {
                    // Here we just add this wire to the subnet,
                    // it's not going to change anything.
                    endNet.AddWire(@new);
                    Console.WriteLine($"Added wire to subnet: {endNet}");
                }
                else
                {
                    // This means that this wire should be in it's own subnet.
                    // It might get merged into another subnet later though..
                    var sub = new Subnet(0);
                    sub.AddWire(@new);
                    addedSubnets.Add(sub);

                    // NOTE: do we want to do this?
                    Bundles.Add(sub);
                    Console.WriteLine($"Added single wire subnet: {sub}");
                }
            }

            foreach (var split in checkSplitSubnets)
            {
                // Here we need to check if this subnet 
                // has to be split into multiple subnets

                if (split.ID == 0)
                {
                    Console.WriteLine($"We don't need to check for splits on this subnet because it has been removed! Subnet: {split}");
                    continue;
                }

                Console.WriteLine($"Checking subnet ({split}) for splits!");

                List<Wire> wiresLeft = new List<Wire>(split.Wires);

                bool usedSplitSubnet = false;

                while (wiresLeft.Count > 0)
                {
                    // This means that there still are wires left that doesn't 
                    // have a subnet

                    // FIXME: Switch to union find for this...
                    static void FloodFill(List<Wire> wires, List<Wire> toCheck, Wire currentWire)
                    {
                        if (wires.Contains(currentWire))
                            return;

                        wires.Add(currentWire);

                        for (int i = toCheck.Count - 1; i >= 0; i--)
                        {
                            Wire wire = toCheck[i];
                            if (wire.IsPointOnWire(currentWire.Pos))
                            {
                                //wires.Add(wire);
                                //toCheck.RemoveAt(i);
                                FloodFill(wires, toCheck, wire);
                            }
                            else if (wire.IsPointOnWire(currentWire.EndPos))
                            {
                                //wires.Add(wire);
                                //toCheck.RemoveAt(i);
                                FloodFill(wires, toCheck, wire);
                            }

                            // Do don't need to check anymore
                            if (wires.Count == toCheck.Count)
                                return;
                        }
                    }

                    Wire startWire = wiresLeft[0];
                    List<Wire> island = new List<Wire>();
                    FloodFill(island, wiresLeft, startWire);

                    // Remove all the used wires
                    foreach (var wire in island)
                    {
                        wiresLeft.Remove(wire);
                    }

                    // Now we have a self contained area of wires
                    Subnet componentCheckSubnet;
                    if (usedSplitSubnet == false)
                    {
                        if (island.Count == split.Wires.Count)
                        {
                            // Here we didn't have to split
                            // and we know we are done here.
                            Console.WriteLine($"No split.");
                            break;
                        }
                        else
                        {
                            // Here we just replace the list of wires in the split subnet
                            Console.WriteLine($"Split original subnet {split} to contain {island.Count} wires.");
                            split.Wires = island;

                            // We don't need to add this subnet to 
                            // Components because it is already in that list

                            componentCheckSubnet = split;

                            // If we are splitting we want to unlink all components from the original
                            // subnet so that we can recheck all connections.
                            // We only do this once. (n.b. this is because of the usedSplitSubnet bool)
                            foreach (var (comp, port) in split.ComponentPorts)
                            {
                                Logic.Unlink(Program.Backend, comp.ID, port, split.ID);
                            }

                            usedSplitSubnet = true;
                        }
                    }
                    else
                    {
                        // Here we create a new subnet
                        var sub = new Subnet(SubnetIDCounter++);
                        Logic.AddSubnet(Program.Backend, sub.ID);
                        sub.Wires = island;
                        Console.WriteLine($"Split part of subnet {split} into new subnet: {sub}.");

                        Bundles.Add(sub);

                        componentCheckSubnet = sub;
                    }

                    // Here we should re-link all of the components that are connected
                    // to this split
                    foreach (var (comp, port) in split.ComponentPorts)
                    {
                        Span<Vector2i> portLocs = stackalloc Vector2i[Gates.GetNumberOfPorts(comp)];
                        Gates.GetTransformedPorts(comp, portLocs);

                        for (int i = 0; i < portLocs.Length; i++)
                        {
                            var loc = portLocs[i];
                            foreach (var wire in island)
                            {
                                if (wire.IsConnectionPoint(loc))
                                {
                                    componentCheckSubnet.AddComponent(comp, i);
                                    Console.WriteLine($"Added component ({comp}, port: {i}) to split subnet: {componentCheckSubnet}.");
                                }
                            }
                        }
                    }
                }
            }

            foreach (var addedNet in addedSubnets)
            {
                // Here we should figure out a new subnet id
                addedNet.ID = SubnetIDCounter++;
                Logic.AddSubnet(Program.Backend, addedNet.ID);
                Console.WriteLine($"Added new subnet: {addedNet}");

                foreach (var comp in Gates.Instances)
                {
                    Span<Vector2i> portLocs = stackalloc Vector2i[Gates.GetNumberOfPorts(comp)];
                    Gates.GetTransformedPorts(comp, portLocs);

                    for (int i = 0; i < portLocs.Length; i++)
                    {
                        var loc = portLocs[i];
                        foreach (var wire in addedNet.Wires)
                        {
                            if (wire.IsConnectionPoint(loc))
                            {
                                addedNet.AddComponent(comp, i);
                                Console.WriteLine($"Added component ({comp}, port: {i}) to the new subnet: {addedNet}.");
                            }
                        }
                    }
                }
            }

            foreach (var removedNet in deletedSubnets)
            {
                Console.WriteLine($"Removed subnet: {removedNet}");
                Logic.RemoveSubnet(Program.Backend, removedNet.ID);
                removedNet.ID = 0;

                Bundles.Remove(removedNet);
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("---- Subnet status ----");
            foreach (var bundle in Bundles)
            {
                Console.WriteLine($"\t{bundle}");
                Logic.DirtySubnet(Program.Backend, bundle.ID);
            }

            Logic.Tick(Program.Backend);
        }

        // FIXME: Structure this better!
        // This is so that we can get something simulating.
        public void AddComponent(InstanceData data)
        {
            Span<Vector2i> ports = stackalloc Vector2i[Gates.GetNumberOfPorts(data)];
            Gates.GetTransformedPorts(data, ports);

            for (int i = 0; i < ports.Length; i++)
            {
                foreach (var bundle in Bundles)
                {
                    foreach (var wire in bundle.Wires)
                    {
                        if (wire.IsConnectionPoint(ports[i]))
                        {
                            bundle.AddComponent(data, i);
                            Console.WriteLine($"Added component ({data}, port: {i}) to the subnet: {bundle}.");
                            // We found a bundle so we don't have to look for
                            // any more bundles
                            goto breakBundles;
                        }
                    }
                }
            breakBundles:
                continue;
            }
        }

        // FIXME: Structure this better!
        // This is so that we can get something simulating.
        public void RemoveComponent(InstanceData data)
        {
            int ports = Gates.GetNumberOfPorts(data);
            int count = 0;
            foreach (var bundle in Bundles)
            {
                for (int i = 0; i < ports; i++)
                {
                    if (bundle.RemoveComponent(data, i))
                    {
                        Console.WriteLine($"Removed component ({data}, port: {i}) from subnet: {bundle}");
                        count++;
                    }
                }
            }

            if (count > ports)
                Console.WriteLine($"Warn: Removed component {data} from {count} subnets which is more than the number of ports the component has. {ports}");
        }


        // FIXME!!
        public int SubnetIDCounter = 1;
        
        // FIXME: We might want to make this more efficient!
        public static List<Subnet> CreateBundlesFromWires(List<Wire> wires)
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

            Dictionary<int, Subnet> bundlesDict = new Dictionary<int, Subnet>();
            foreach (var w in wires)
            {
                int root = nets.Find(w.Pos).Index;
                if (bundlesDict.TryGetValue(root, out var bundle) == false)
                {
                    bundle = new Subnet(0);
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
