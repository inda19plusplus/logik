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
        Buffer    = 1,
        Not       = 2,
        And       = 3,
        Nand      = 4,
        Or        = 5,
        Nor       = 6,
        Xor       = 7,
        Xnor      = 8,
        Gate      = 9,
        
        DFlipFlop = 10,
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
