using LogikUI.Circuit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace LogikUI.Util
{
    static class MathUtil
    {
        public const double D2R = Math.PI / 180;
        public const double R2D = 180 / Math.PI;

        public static double ToAngle(this Orientation orientation) => orientation switch
        {
            Orientation.East => 0,
            Orientation.South => Math.PI / 2,
            Orientation.West => Math.PI,
            Orientation.North => 3*(Math.PI/2),
            _ => throw new InvalidEnumArgumentException(nameof(orientation), (int)orientation, typeof(Orientation)),
        };

        public static Vector2d Rotate(this Vector2d vec, Orientation orientation) => orientation switch
        {
            Orientation.East => vec,
            Orientation.South => new Vector2d(vec.Y, vec.X),
            Orientation.West => new Vector2d(-vec.X, -vec.Y),
            Orientation.North => new Vector2d(-vec.Y, -vec.X),
            _ => throw new InvalidEnumArgumentException(nameof(orientation), (int)orientation, typeof(Orientation)),
        };

        public static Rect Rotate(this Rect rect, Vector2d origin, Orientation orientation)
        {
            var offset = rect.Position - origin;
            offset = offset.Rotate(orientation);
            var size = rect.Size.Rotate(orientation);
            if (size.X < 0) offset.X += size.X;
            if (size.Y < 0) offset.Y += size.Y;
            return new Rect(offset + origin, Vector2d.Abs(rect.Size.Rotate(orientation)));
        }
            
    }
}
