using Cairo;
using LogikUI.Circuit;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace LogikUI.Simulation.Gates
{
    enum ComponentType : int
    {
        // If the component type is custom it's a user defined component
        // and requires additional data to operate on...
        Custom    = 0,

        // IO components (reserved range 1-49)
        Constant    = 1,
        Output      = 2,
        Input       = 3,
        InputOutput = 4,
        // LED = 5,
        // LEDMatrix = 6,
        // SevenSegment = 7,
        // Button = 8,
        // Switch = 9,

        // Normal/standard gates (reserved range 50-99)
        Buffer = 50,
        Not    = 51,
        And    = 52,
        Nand   = 53,
        Or     = 54,
        Nor    = 55,
        Xor    = 56,
        Xnor   = 57,
        //Imply  = 59,
        //Nimply  = 59,

        TriStateBuffer   = 60,
        TriStateInverter = 61,

        // More advanced/complex components (reserved range 100-299)
        DFlipFlop     = 100,
        TFlipFlop     = 101,
        JKFlipFlop    = 102,
        SRFlipFlop    = 103,
        Register      = 104,
        ShiftRegister = 105,
        Counter       = 106,

        RAM        = 110,
        ROM        = 111,

        // Figure out different sizes...
        // Mux       = xxx,
        // DeMux     = xxx,
        // 

        // Utility (reserved range 300-399)
        Probe = 300,
        Splitter = 301,
        Clock    = 302,
        // Tunnel   = 303,
    }

    struct InstanceData : IEquatable<InstanceData>
    {
        public int ID;
        public ComponentType Type;
        public Vector2i Position;
        public Orientation Orientation;
        // TODO: Some way to identify custom components

        public static InstanceData Create(ComponentType type, Vector2i position, Orientation orientation)
        {
            // FIXME: Do soemthing about the ID?
            return new InstanceData()
            {
                Type = type,
                Position = position,
                Orientation = orientation,
            };
        }

        public override bool Equals(object? obj)
        {
            return obj is InstanceData data && Equals(data);
        }

        public bool Equals(InstanceData other)
        {
            return ID == other.ID &&
                   Type == other.Type &&
                   Position.Equals(other.Position) &&
                   Orientation == other.Orientation;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, Type, Position, Orientation);
        }

        public static bool operator ==(InstanceData left, InstanceData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InstanceData left, InstanceData right)
        {
            return !(left == right);
        }
    }

    interface IComponent
    {
        public string Name { get; }
        public ComponentType Type { get; }
        public int NumberOfPorts { get; }

        public void GetPorts(Span<Vector2i> ports);
        
        public void Draw(Context cr, InstanceData data);
    }
}
