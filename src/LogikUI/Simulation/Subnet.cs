using LogikUI.Circuit;
using LogikUI.Interop;
using LogikUI.Simulation.Gates;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogikUI.Simulation
{
    class Subnet
    {
        public int ID;
        public List<Wire> Wires = new List<Wire>();
        public List<(InstanceData Instance, int Port)> ComponentPorts = new List<(InstanceData, int)>();

        public Subnet(int id)
        {
            ID = id;
        }

        public void AddWire(Wire wire)
        {
            if (Wires.Contains(wire))
                Console.WriteLine($"Warn: Adding duplicate wire to wire bundle! (Wire: {wire})");

            Wires.Add(wire);
        }

        public bool RemoveWire(Wire wire)
        {
            return Wires.Remove(wire);
        }

        public void AddComponent(InstanceData data, int port)
        {
            if (ComponentPorts.Contains((data, port)))
                Console.WriteLine($"Warn: Adding duplicate gate port to wire bundle! (Instance: {data}, port: {port})");

            ComponentPorts.Add((data, port));

            // Tell the backend that this component was connected to this subnet.
            LogLogic.Link(Program.Backend, data.ID, port, ID);
        }

        public bool RemoveComponent(InstanceData data, int port)
        {
            if (ComponentPorts.Remove((data, port)))
            {
                if(LogLogic.Unlink(Program.Backend, data.ID, port, ID))
                {
                    Console.WriteLine($"Warn: Could not unlink component {data.ID} port {port} from subnet {ID}");
                    return false;
                }
                else return true;
            }
            else return false;
        }

        public void Merge(Subnet toMerge)
        {
            Wires.AddRange(toMerge.Wires);

            if (ID == 0)
                Console.WriteLine($"Warn: Trying to merge wires into a subnet with ID zero! {this}");

            // Here we link all of the components to this subnet
            foreach (var (instance, port) in toMerge.ComponentPorts)
            {
                AddComponent(instance, port);
            }

            // Remove the subnet that we merged
            LogLogic.RemoveSubnet(Program.Backend, toMerge.ID);
            toMerge.ID = 0;
        }

        public override string ToString()
        {
            return $"Subnet: {ID}, Wires: {Wires.Count}, Component ports: {ComponentPorts.Count}";
        }
    }
}
