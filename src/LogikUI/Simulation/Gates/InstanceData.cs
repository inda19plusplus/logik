using LogikUI.Circuit;
using LogikUI.Util;
using System;

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
