using Gtk;
using LogikUI.Simulation.Gates;
using LogikUI.Transaction;
using LogikUI.Util;
using Pango;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogikUI.Circuit
{
    // FIXME: Move to it's own file
    enum Orientation : int
    {
        East,
        South,
        West,
        North,
    }

    class Gates
    {
        public Dictionary<ComponentType, IComponent> Components = new Dictionary<ComponentType, IComponent>()
        {
            { ComponentType.Not, new NotGate() },
            { ComponentType.And, new AndGate() },
            { ComponentType.Or, new OrGate() },
        };

        public List<InstanceData> Instances = new List<InstanceData>();

        public Gates()
        {
        }

        public void Draw(Cairo.Context cr)
        {
            //Components.DrawComponents(cr);
            foreach (var instance in Instances)
            {
                if (Components.TryGetValue(instance.Type, out var comp) == false)
                {
                    // This is an unknown component type!!
                    Console.WriteLine($"Unknown component type '{instance.Type}'! ({instance})");
                    continue;
                }

                // The compiler doesn't do correct null analysis so we do '!' to tell it
                // that comp cannot be null here.
                comp!.Draw(cr, instance);
            }
        }

        public GateTransaction CreateAddGateTrasaction(InstanceData gate)
        {
            // FIXME: Do some de-duplication stuff?
            return new GateTransaction(gate);
        }

        public void ApplyTransaction(GateTransaction transaction)
        {
            Instances.Add(transaction.Created);
        }

        public void RevertGateTransaction(GateTransaction transaction)
        {
            if (Instances.Remove(transaction.Created) == false)
            {
                Console.WriteLine($"Warn: Removed non-existent gate! {transaction.Created}");
            }
        }
    }
}
