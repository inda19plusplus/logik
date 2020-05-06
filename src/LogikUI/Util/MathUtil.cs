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
    }
}
