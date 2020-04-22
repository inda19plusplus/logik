using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LogikUI.Simulation
{
    struct SetEvent : IEquatable<SetEvent>, IComparable<SetEvent>
    {
        public Node Port;
        public Value Value;
        public long When;
        // FIXME: Implement port datastructure

        public SetEvent(Node port, Value value, long when)
        {
            Port = port;
            Value = value;
            When = when;
        }

        public int CompareTo(SetEvent other)
        {
            return Math.Sign(When - other.When);
        }

        public override bool Equals(object? obj)
        {
            return obj is SetEvent @event && Equals(@event);
        }

        public bool Equals(SetEvent other)
        {
            return When == other.When &&
                   Value.Equals(other.Value) &&
                   EqualityComparer<Node>.Default.Equals(Port, other.Port);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(When, Value, Port);
        }

        public static bool operator ==(SetEvent left, SetEvent right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SetEvent left, SetEvent right)
        {
            return !(left == right);
        }
    }
}
