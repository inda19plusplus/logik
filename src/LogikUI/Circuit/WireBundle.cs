using LogikUI.Simulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Circuit
{
    class WireBundle
    {
        public List<Wire> Wires = new List<Wire>();
        public Subnet Subnet;

        public void AddWire(Wire wire)
        {
            if (Wires.Contains(wire))
                Console.WriteLine($"Warn: Adding duplicate wire to wire bundle! (Wire: {wire})");

            Wires.Add(wire);
        }
    }
}
