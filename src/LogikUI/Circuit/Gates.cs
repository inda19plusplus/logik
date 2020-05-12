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
            { ComponentType.Constant, new Constant() },
            { ComponentType.Buffer, new BufferGate() },
            { ComponentType.Not, new NotGate() },
            { ComponentType.And, new AndGate() },
            { ComponentType.Or, new OrGate() },
            { ComponentType.Xor, new XorGate() },
        };

        public List<InstanceData> Instances = new List<InstanceData>();

        public int GetNumberOfPorts(InstanceData data)
        {
            if (Components.TryGetValue(data.Type, out var comp) == false)
                return -1;

            return comp!.NumberOfPorts;
        }

        public void GetTransformedPorts(InstanceData data, Span<Vector2i> ports)
        {
            if (Components.TryGetValue(data.Type, out var comp) == false)
            {
                Console.WriteLine($"Component '{data.Type}' doesn't have a IComponent implementation. Either you forgot to implement the gate or you've not registered that IComponent in the Dictionary. (Instance: {data})");
                return;
            }

            comp!.GetPorts(ports);
            IComponent.TransformPorts(data, ports);
        }

        public void Draw(Cairo.Context cr)
        {
            foreach (var instance in Instances)
            {
                if (Components.TryGetValue(instance.Type, out var comp) == false)
                {
                    if (Enum.IsDefined(typeof(ComponentType), instance.Type))
                        // This component is defined in the enum but doesn't have a dictionary entry
                        Console.WriteLine($"Component '{instance.Type}' doesn't have a IComponent implementation. Either you forgot to implement the gate or you've not registered that IComponent in the Dictionary. (Instance: {instance})");
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

        public GateTransaction CreateAddGateTransaction(Wires wires, InstanceData gate)
        {
            // FIXME: This transaction will have to modify wires too
            // Should we bundle the wire edits necessary into this transaction
            // or should that be handled somewhere else?
            // Because we don't have access to the wires here we might want to
            // do the wires sync outside of this class in like CircuitEditor.

            if (Components.TryGetValue(gate.Type, out var component) == false)
                throw new System.ComponentModel.InvalidEnumArgumentException(nameof(gate.Type), (int)gate.Type, typeof(ComponentType));

            Span<Vector2i> ports = stackalloc Vector2i[component!.NumberOfPorts];
            component!.GetPorts(ports);

            // FIXME: Consider orientation
            // Move the ports to be not be relative to the component
            for (int i = 0; i < ports.Length; i++)
            {
                ports[i] += gate.Position;
            }

            var wTransaction = wires.CreateAddConnectionPointsTransaction(ports);

            // FIXME: Do some de-duplication stuff?
            return new GateTransaction(false, gate, wTransaction);
        }

        public void ApplyTransaction(GateTransaction transaction)
        {
            if (transaction.RemoveComponent == false)
            {
                transaction.Gate.ID = LogLogic.AddComponent(Program.Backend, transaction.Gate.Type);

                Instances.Add(transaction.Gate);
            }
            else
            {
                if (transaction.Gate.ID == 0)
                    throw new InvalidOperationException("Cannot delete a gate that doesn't have a valid id! (Maybe you forgot to apply the transaction before?)");

                if (LogLogic.RemoveComponent(Program.Backend, transaction.Gate.ID) == false)
                {
                    Console.WriteLine($"RemoveComponent(Type: {transaction.Gate.ID}) -> false");
                    Console.WriteLine($"Warn: Rust said we couldn't remove this gate id: {transaction.Gate.ID}. ({transaction.Gate})");
                }
                else
                {
                    Console.WriteLine($"RemoveComponent(Type: {transaction.Gate.ID}) -> true");
                }

                if (Instances.Remove(transaction.Gate) == false)
                {
                    Console.WriteLine($"Warn: Removed non-existent gate! {transaction.Gate}");
                }
            }
        }

        // FIXME: Revert here is just a mirrored copy if Apply.
        // We could make this less error prone by consolidating them into one thing?
        public void RevertTransaction(GateTransaction transaction)
        {
            if (transaction.RemoveComponent == false)
            {
                // Here we should revert a add transation, i.e. removing the component
                if (transaction.Gate.ID == 0)
                    throw new InvalidOperationException("Cannot revert a transaction where the gates doesn't have a valid id! (Maybe you forgot to apply the transaction before?)");

                if (LogLogic.RemoveComponent(Program.Backend, transaction.Gate.ID) == false)
                {
                    Console.WriteLine($"Warn: Rust said we couldn't remove this gate id: {transaction.Gate.ID}. ({transaction.Gate})");
                }

                if (Instances.Remove(transaction.Gate) == false)
                {
                    Console.WriteLine($"Warn: Removed non-existent gate! {transaction.Gate}");
                }
            }
            else
            {
                // Here we are reverting a delete, i.e. adding it back again
                transaction.Gate.ID = LogLogic.AddComponent(Program.Backend, transaction.Gate.Type);
                Instances.Add(transaction.Gate);
            }
            
        }
    }
}
