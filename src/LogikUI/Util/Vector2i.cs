using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LogikUI.Util
{
    internal struct Vector2i : IEquatable<Vector2i>
    {
        public static readonly Vector2i Zero = new Vector2i(0, 0);

        public int X;
        public int Y;

        public int ManhattanDistance => Math.Abs(X + Y);

        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Vector2i ComponentWiseMin(Vector2i a, Vector2i b) => 
            new Vector2i(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));

        public static Vector2i ComponentWiseMax(Vector2i a, Vector2i b) =>
            new Vector2i(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));

        public override bool Equals(object? obj)
        {
            return obj is Vector2i d && Equals(d);
        }

        public bool Equals([AllowNull] Vector2i other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string? ToString()
        {
            return $"{X:0.000}, {Y:0.000}";
        }

        internal void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        public static Vector2i operator +(Vector2i a, Vector2i b) => new Vector2i(a.X + b.X, a.Y + b.Y);
        public static Vector2i operator -(Vector2i a, Vector2i b) => new Vector2i(a.X - b.X, a.Y - b.Y);
        public static Vector2i operator *(int scalar, Vector2i a) => new Vector2i(a.X * scalar, a.Y * scalar);
        public static Vector2i operator *(Vector2i a, int scalar) => new Vector2i(a.X * scalar, a.Y * scalar);
        public static Vector2d operator *(double scalar, Vector2i a) => new Vector2d(a.X * scalar, a.Y * scalar);
        public static Vector2d operator *(Vector2i a, double scalar) => new Vector2d(a.X * scalar, a.Y * scalar);
        public static Vector2i operator /(Vector2i a, int scalar) => new Vector2i(a.X / scalar, a.Y / scalar);
        public static Vector2d operator /(Vector2i a, double scalar) => new Vector2d(a.X / scalar, a.Y / scalar);

        public static bool operator ==(Vector2i left, Vector2i right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2i left, Vector2i right)
        {
            return !(left == right);
        }

        public static implicit operator Vector2i(Gdk.Point point) => new Vector2i(point.X, point.Y);
        public static implicit operator Gdk.Point(Vector2i vec) => new Gdk.Point(vec.X, vec.Y);

        public static implicit operator Vector2d(Vector2i vec) => new Vector2d(vec.X, vec.Y);
        public static explicit operator Vector2i(Vector2d vec) => new Vector2i((int)vec.X, (int)vec.Y);
    }
}
