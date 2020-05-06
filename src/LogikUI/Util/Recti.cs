using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LogikUI.Util
{
    struct Recti : IEquatable<Recti>
    {
        public Vector2i Position;
        public Vector2i Size;

        public int X => Position.X;
        public int Y => Position.Y;
        public int Width => Size.X;
        public int Height => Size.Y;

        public Recti(Vector2i position, Vector2i size)
        {
            Position = position;
            Size = size;
        }

        public bool Contains(Vector2i Point)
        {
            return Point.X >= Position.X && Point.X <= Position.X + Size.X &&
                Point.Y >= Position.Y && Point.Y <= Position.Y + Size.Y;
        }

        public override bool Equals(object? obj)
        {
            return obj is Recti rect && Equals(rect);
        }

        public bool Equals([AllowNull] Recti other)
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

        public static bool operator ==(Recti left, Recti right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Recti left, Recti right)
        {
            return !(left == right);
        }

        // FIXME: Consider if we should do rounding? Or if we should have a separate RectI Struct.
        public static implicit operator Gdk.Rectangle(Recti rect) => new Gdk.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
    }
}
