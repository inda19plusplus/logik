using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LogikUI.Util
{
    internal struct Rect : IEquatable<Rect>
    {
        public Vector2D Position;
        public Vector2D Size;

        public double X => Position.X;
        public double Y => Position.Y;
        public double Width => Size.X;
        public double Height => Size.Y;

        public Rect(Vector2D position, Vector2D size)
        {
            Position = position;
            Size = size;
        }

        public bool Contains(Vector2D Point)
        {
            return Point.X >= Position.X && Point.X <= Position.X + Size.X &&
                Point.Y >= Position.Y && Point.Y <= Position.Y + Size.Y;
        }

        public override bool Equals(object? obj)
        {
            return obj is Rect rect && Equals(rect);
        }

        public bool Equals([AllowNull] Rect other)
        {
            return Position.Equals(other.Position) &&
                   Size.Equals(other.Size);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Size);
        }

        public override string? ToString()
        {
            return $"Pos ({Position}), Size ({Size})";
        }

        public static bool operator ==(Rect left, Rect right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Rect left, Rect right)
        {
            return !(left == right);
        }

        // FIXME: Consider if we should do rounding? Or if we should have a separate RectI Struct.
        public static implicit operator Gdk.Rectangle(Rect rect) => new Gdk.Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
    }
}
