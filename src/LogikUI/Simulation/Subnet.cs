using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogikUI.Simulation
{
    class Subnet
    {
        // Should we handle all of the subnets globally?
        // static List<Subnet> Subnets;
        // Or should we do some other solution.
        public readonly List<Node> Nodes = new List<Node>();

        // The nodes that are driving the value of this subnet
        public IEnumerable<Node> Drivers => Nodes.Where(n => n.Flags.HasFlag(NodeFlags.Output));
        // The nodes that are reading the value of this subnet
        public IEnumerable<Node> Readers => Nodes.Where(n => n.Flags.HasFlag(NodeFlags.Input));

        public Value Value = Value.Floating;

        public void AddNode(Node node)
        {
            Nodes.Add(node);
        }

        public void UpdateValue()
        {
            Value v = Value.Floating;
            foreach (var driver in Drivers)
            {
                v = Value.Resolve(v, Value.Zero);
            }
        }
    }
}
