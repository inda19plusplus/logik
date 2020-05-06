using LogikUI.Circuit;
using LogikUI.Util;
using System;
using LogikUI.Interop;

namespace LogikUI.Simulation.Gates
{
    struct InstanceData : IEquatable<InstanceData>
    {
        public int ID;
        public ComponentType Type;
        public Vector2i Position;
        public Orientation Orientation;
        // TODO: Some way to identify custom components

        public static InstanceData Create(ComponentType type, Vector2i position, Orientation orientation)
        {
            return new InstanceData()
            {
                Type = type,
                Position = position,
                Orientation = orientation,
                ID = -1, // Represents unassigned beacuse rust will never give a component the ID -1
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

        public override string? ToString()
        {
            return $"ID: {ID}, Type {Type}, Position: {Position}, Orientation: {Orientation}";
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
}
