using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace LogikUI.Simulation
{
    public enum ValueState : byte
    {
        // The bit patterns here are important for the logic that
        // resolves two values states. It allows for resolving to be
        // a single bitwise-or operation.
        Floating = 0b00,
        Zero = 0b01,
        One = 0b10, 
        Error = 0b11,
    }

    // FIXME: Figure out what we want to do for alignment
    public struct Value : IEquatable<Value>
    {
        public static readonly Value Null = new Value(0, 0);

        public static readonly Value Floating = new Value(ValueState.Floating);
        public static readonly Value Zero = new Value(ValueState.Zero);
        public static readonly Value One = new Value(ValueState.One);
        public static readonly Value Error = new Value(ValueState.Error);

        public const ulong LOWER_MASK = 0x5555_5555_5555_5555;
        public const ulong UPPER_MASK = 0xAAAA_AAAA_AAAA_AAAA;

        // FIXME: Figure out what we want to do for alignment
        public ulong Values;
        public byte Width;

        public Value(ValueState state)
        {
            Values = (byte)state;
            Width = 1;
        }

        public Value(ulong values, byte width)
        {
            if (width > 32)
                throw new ArgumentException($"A value cannot contain more than 32 values! (Got {width})", nameof(width));

            Values = values;
            Width = width;
        }

        public ValueState GetValue(int i)
        {
            if (i >= Width)
                throw new ArgumentOutOfRangeException(nameof(i), $"The index {i} exceeded the length of this value ({Width})!");
            // Shift down and mask
            return (ValueState) ((Values >> (i * 2)) & 0b11);
        }

        public void SetValue(int i, ValueState state)
        {
            if (i >= Width)
                throw new ArgumentOutOfRangeException(nameof(i), $"The index {i} exceeded the length of this value ({Width})!");
            // Clear the bits we want to set
            ulong clearMask = 0b11ul << (i * 2);
            Values &= ~clearMask;
            // Shift up and or the value we want to set
            Values |= ((ulong)state) << (i * 2);
        }

        public ValueState this[int i]
        {
            get => GetValue(i);
            set => SetValue(i, value);
        }

        public static char GetValueChar(ValueState state)
        {
            return state switch
            {
                ValueState.Floating => 'F',
                ValueState.Zero => '0',
                ValueState.One => '1',
                ValueState.Error => 'X',
                _ => throw new InvalidEnumArgumentException(nameof(state), (int)state, typeof(ValueState)),
            };
        }

        public static ValueState Resolve(ValueState a, ValueState b)
        {
            // Resolution table
            //  a | 
            // b  | F 0 1 X 
            // ---+----------
            //  F | F 0 1 X
            //  0 | 0 0 X X
            //  1 | 1 X 1 X
            //  X | X X X X
            // Because of the bit pattern this will result in the correct value.
            return a | b;
        }

        public static ValueState And(ValueState a, ValueState b)
        {
            // Resolution table
            //  a | 
            // b  | F 0 1 X 
            // ---+----------
            //  F | X X X X
            //  0 | X 0 0 X
            //  1 | X 0 1 X
            //  X | X X X X

            var p1 = ~((int)a ^ ((int)a << 1));
            var p2 = (~((int)a << 1) & (int)b);
            var p3 = ~((int)b ^ ((int)b << 1));
            var r1 = (p1 | p2 | p3) & 0b10;

            var r2 = (~(((int)a) >> 1) | (int)a | ~(((int)b) >> 1) | (int)b) & 0b01;

            return (ValueState)((r1 | r2) & 0b11);
        }

        public static Value Resolve(Value a, Value b)
        {
            // IEEE 1364 Where?
            // Resolution table
            //  a | 
            // b  | F 0 1 X 
            // ---+---------
            //  F | F 0 1 X
            //  0 | 0 0 X X
            //  1 | 1 X 1 X
            //  X | X X X X

            if (a.Width != b.Width) 
                throw new WidthMismatchException(a.Width, b.Width, $"The two values had different widths! ({a.Width} != {b.Width})");

            Value c;
            c.Width = a.Width;
            // Because all of the values can be individually or:ed to get the right result
            // We can or the entire thing to get the right values.
            c.Values = a.Values | b.Values;
            return c;
        }

        public static Value And(Value a, Value b)
        {
            // IEEE 1364-2005 chapter 5.1.10
            // And table
            //  a | 
            // b  | F 0 1 X 
            // ---+----------
            //  F | X X X X
            //  0 | X 0 0 X
            //  1 | X 0 1 X
            //  X | X X X X

            if (a.Width != b.Width)
                throw new WidthMismatchException(a.Width, b.Width, $"The two values had different widths! ({a.Width} != {b.Width})");

            // FIXME: Document the logic expressions used to derive these

            ulong r1 = ((a.Values | ~(a.Values << 1)) & (b.Values | ~(b.Values << 1))) & UPPER_MASK;
            ulong r2 = (~(a.Values >> 1) | a.Values | ~(b.Values >> 1) | b.Values) & LOWER_MASK;

            ulong mask = 0xFFFF_FFFF_FFFF_FFFF >> (64 - (a.Width * 2));

            return new Value((r1 | r2) & mask, a.Width);
        }

        public static Value Or(Value a, Value b)
        {
            // IEEE 1364-2005 chapter 5.1.10
            // Or table
            //  a | 
            // b  | F 0 1 X 
            // ---+---------
            //  F | X X 1 X
            //  0 | X 0 1 X
            //  1 | 1 1 1 1
            //  X | X X 1 X

            if (a.Width != b.Width)
                throw new WidthMismatchException(a.Width, b.Width, $"The two values had different widths! ({a.Width} != {b.Width})");

            // FIXME: Document the logic expressions used to derive these

            ulong r1 = (a.Values | ~(a.Values << 1) | b.Values | ~(b.Values << 1)) & UPPER_MASK;
            ulong r2 = ((~(a.Values >> 1) | a.Values) & (~(b.Values >> 1) | b.Values)) & LOWER_MASK;

            ulong mask = 0xFFFF_FFFF_FFFF_FFFF >> (64 - (a.Width * 2));

            return new Value((r1 | r2) & mask, a.Width);
        }

        public static Value Not(Value a)
        {
            // IEEE 1364-2005 chapter 5.1.10
            // Not table
            //    | 
            // ---+---
            //  F | F
            //  0 | 1
            //  1 | 0
            //  X | X

            ulong r1 = (a.Values << 1) & UPPER_MASK;
            ulong r2 = (a.Values >> 1) & LOWER_MASK;

            return new Value(r1 | r2, a.Width);
        }

        public override bool Equals(object? obj)
        {
            return obj is Value value && Equals(value);
        }

        public bool Equals(Value other)
        {
            return Values == other.Values &&
                   Width == other.Width;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Values, Width);
        }

        public override string? ToString()
        {
            StringBuilder builder = new StringBuilder();
            int shift = (Width - 1) * 2;
            for (int i = 0; i < Width; i++)
            {
                builder.Append(GetValueChar((ValueState)((Values >> shift) & 0b11)));

                // Shift the values so that the next value is at the beginning
                shift -= 2;

                if (i + 1 < Width)
                {
                    builder.Append(", ");
                }
            }

            return builder.ToString();
        }

        public static bool operator ==(Value left, Value right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Value left, Value right)
        {
            return !(left == right);
        }

        // NOTE: Consider overloading && and || for and-ing and or-ing Values.
    }
}
