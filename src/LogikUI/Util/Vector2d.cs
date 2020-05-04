using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LogikUI.Util
{
    internal struct Vector2d : IEquatable<Vector2d>
    {
        public static readonly Vector2d Zero = new Vector2d(0, 0);

        public double X;
        public double Y;

        public Vector2d(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector2i Round() => new Vector2i((int)Math.Round(X), (int)Math.Round(Y));

        public override bool Equals(object? obj)
        {
            return obj is Vector2d d && Equals(d);
        }

        public bool Equals([AllowNull] Vector2d other)
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

        internal void Deconstruct(out double x, out double y)
        {
            x = X;
            y = Y;
        }

        public static Vector2d operator -(Vector2d a) => new Vector2d(-a.X, -a.Y);
        public static Vector2d operator +(Vector2d a, Vector2d b) => new Vector2d(a.X + b.X, a.Y + b.Y);
        public static Vector2d operator -(Vector2d a, Vector2d b) => new Vector2d(a.X - b.X, a.Y - b.Y);
        public static Vector2d operator *(double scalar, Vector2d a) => new Vector2d(a.X * scalar, a.Y * scalar);
        public static Vector2d operator *(Vector2d a, double scalar) => new Vector2d(a.X * scalar, a.Y * scalar);
        public static Vector2d operator /(Vector2d a, double scalar) => new Vector2d(a.X / scalar, a.Y / scalar);
        public static Vector2d operator %(Vector2d a, double scalar) => new Vector2d(a.X % scalar, a.Y % scalar);

        public static bool operator ==(Vector2d left, Vector2d right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2d left, Vector2d right)
        {
            return !(left == right);
        }

        public static implicit operator Vector2d(Cairo.PointD point) => new Vector2d(point.X, point.Y);
        public static implicit operator Cairo.PointD(Vector2d vec) => new Cairo.PointD(vec.X, vec.Y);

        
    }
}
