using Gtk;
using LogikUI.Simulation.Gates;
using LogikUI.Transaction;
using LogikUI.Util;
using Pango;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using LogikUI.Interop;

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

        public void Draw(Cairo.Context cr)
        {
            foreach (var instance in Instances)
            {
                if (Components.TryGetValue(instance.Type, out var comp) == false)
                {
                    if (Enum.IsDefined(typeof(ComponentType), instance.Type))
                        // This component is defined in the enum but doesn't have a dictionary entry
                        Console.WriteLine($"Component '{instance}' doesn't have a IComponent implementation. Either you forgot to implement the gate or you've not registered that IComponent in the Dictionary. (Instance: {instance})");
                    else
                        // This is an unknown component type!!
                        Console.WriteLine($"Unknown component type '{instance.Type}'! (Instance: {instance})");

                    // We won't try to draw this component
                    continue;
                }

                // The compiler doesn't do correct null analysis so we do '!' to tell it
                // that comp cannot be null here.
                comp!.Draw(cr, instance);
            }
        }

        public GateTransaction CreateAddGateTransaction(InstanceData gate)
        {
            // FIXME: This transaction will have to modify wires too
            // Should we bundle the wire edits necessary into this transaction
            // or should that be handled somewhere else?
            // Because we don't have access to the wires here we might want to
            // do the wires sync outside of this class in like CircuitEditor.

            // FIXME: Do some de-duplication stuff?
            return new GateTransaction(gate);
        }

        public void ApplyTransaction(GateTransaction transaction)
        {
            UIntPtr id;
            unsafe
            {
                id = Logic.AddComponent(Program.backend, new UIntPtr((uint) transaction.Created.Type));
            }
            transaction.Created.ID = (int) id;
            Instances.Add(transaction.Created);
        }

        public void RevertTransaction(GateTransaction transaction)
        {
            unsafe
            {
                if (transaction.Created.ID >= 0)
                {
                    Logic.RemoveComponent(Program.backend, new UIntPtr((uint) transaction.Created.ID));
                }
            }
            if (Instances.Remove(transaction.Created) == false)
            {
                Console.WriteLine($"Warn: Removed non-existent gate! {transaction.Created}");
            }
        }
    }
}
