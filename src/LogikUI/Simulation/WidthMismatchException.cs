using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Simulation
{
    public class WidthMismatchException : Exception
    {
        public int Width1;
        public int Width2;

        public WidthMismatchException(int width1, int width2) : base($"The widths did not match! ({width1} != {width2})")
        {
            Width1 = width1;
            Width2 = width2;
        }

        public WidthMismatchException(int width1, int width2, string? message) : base(message)
        {
            Width1 = width1;
            Width2 = width2;
        }

        public WidthMismatchException(int width1, int width2, string? message, Exception? innerException) : base(message, innerException)
        {
            Width1 = width1;
            Width2 = width2;
        }
    }
}
