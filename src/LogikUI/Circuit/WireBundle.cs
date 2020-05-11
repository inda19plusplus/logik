using LogikUI.Simulation;
using LogikUI.Simulation.Gates;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Circuit
{
    class WireBundle
    {
        public Subnet? Subnet;
        public List<Wire> Wires = new List<Wire>();
        public List<(InstanceData, int)> GatePorts = new List<(InstanceData, int)>();

        public void AddWire(Wire wire)
        {
            if (Wires.Contains(wire))
                Console.WriteLine($"Warn: Adding duplicate wire to wire bundle! (Wire: {wire})");

            Wires.Add(wire);
        }

        public override string ToString()
        {
            return $"Subnet: {Subnet?.ID}, Wires: {Wires.Count}";
        }
    }
}
