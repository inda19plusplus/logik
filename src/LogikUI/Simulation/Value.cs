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

    public struct Value : IEquatable<Value>
    {
        public static readonly Value Null = new Value(0, 0);

        public static readonly Value Floating = new Value(ValueState.Floating);
        public static readonly Value Zero = new Value(ValueState.Zero);
        public static readonly Value One = new Value(ValueState.One);
        public static readonly Value Error = new Value(ValueState.Error);

        // FIXME: Figure out what we want to do for alignment
        public long Values;
        public byte Width;

        public Value(ValueState state)
        {
            Values = (byte)state;
            Width = 1;
        }

        public Value(long values, byte length)
        {
            if (length > 32)
                throw new ArgumentException($"A value cannot contain more than 32 values! (Got {length})", nameof(length));

            Values = values;
            Width = length;
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
            long clearMask = 0b11 << (i * 2);
            Values &= ~clearMask;
            // Shift up and or the value we want to set
            Values |= ((long)state) << (i * 2);
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

        public static Value Resolve(Value a, Value b)
        {
            // Resolution table
            //  a | 
            // b  | F 0 1 X 
            // ---+----------
            //  F | F 0 1 X
            //  0 | 0 0 X X
            //  1 | 1 X 1 X
            //  X | X X X X
            if (a.Width != b.Width) 
                throw new WidthMissmatchException(a.Width, b.Width, $"The two values had different widths! ({a.Width} != {b.Width})");

            Value c;
            c.Width = a.Width;
            // Because all of the values can be individually or:ed to get the right result
            // We can or the entire thing to get the right values.
            c.Values = a.Values | b.Values;
            return c;
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
