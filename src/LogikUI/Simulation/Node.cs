using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Simulation
{
    [Flags]
    enum NodeFlags
    {
        None = 0,
        Input = 1,
        Output = 2,

        InOut = Input | Output,
    }

    struct Node : IEquatable<Node>
    {
        public Vector2i Pos;
        public NodeFlags Flags;

        public Node(Vector2i pos, NodeFlags flags)
        {
            Pos = pos;
            Flags = flags;
        }

        public override bool Equals(object? obj)
        {
            return obj is Node node && Equals(node);
        }

        public bool Equals(Node other)
        {
            return Pos.Equals(other.Pos) &&
                   Flags == other.Flags;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Pos, Flags);
        }

        public static bool operator ==(Node left, Node right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Node left, Node right)
        {
            return !(left == right);
        }
    }
}
