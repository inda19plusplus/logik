using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LogikUI.Util
{
    internal struct Rect : IEquatable<Rect>
    {
        public Vector2d Position;
        public Vector2d Size;

        public double X => Position.X;
        public double Y => Position.Y;
        public double Width => Size.X;
        public double Height => Size.Y;

        public Vector2d TopLeft => Position;
        public Vector2d TopRight => Position + new Vector2d(Size.X, 0);
        public Vector2d BottomLeft => Position + new Vector2d(0, Size.Y);
        public Vector2d BottomRight => Position + Size;

        public Rect(Vector2d position, Vector2d size)
        {
            Position = position;
            Size = size;
        }

        public bool Contains(Vector2d Point)
        {
            return Point.X >= Position.X && Point.X <= Position.X + Size.X &&
                Point.Y >= Position.Y && Point.Y <= Position.Y + Size.Y;
        }

        public bool Contains(Rect rect)
        {
            return Contains(rect.Position) && Contains(rect.Position + rect.Size);
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

        public static implicit operator Cairo.Rectangle(Rect rect) => new Cairo.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
    }
}
