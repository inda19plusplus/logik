using LogikUI.Util;
using System;

namespace LogikUI.Circuit
{
    enum Direction
    {
        Vertical,
        Horizontal,
    }

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
}
