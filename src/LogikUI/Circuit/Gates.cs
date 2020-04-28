using Gtk;
using LogikUI.Simulation.Gates;
using LogikUI.Transaction;
using LogikUI.Util;
using Pango;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Text;

namespace LogikUI.Circuit
{
    // FIXME: Move to it's own file
    enum Orientation
    {
        East,
        South,
        West,
        North,
    }

    class Gates
    {
        public Components Components;
        
        public Gates()
        {
            Components = new Components();
            // FIXME: Register this somewhere else?
            Components.RegisterComponentType<AndGateInstance>(AndGateInstance.Draw);
            Components.RegisterComponentType<NotGateInstance>(NotGateInstance.Draw);
        }

        public void Draw(Cairo.Context cr)
        {
            Components.DrawComponents(cr);
        }

        public GateTransaction CreateAddGateTrasaction(IInstance gate)
        {
            // FIXME: Do some de-duplication stuff?
            return new GateTransaction(gate);
        }

        public void ApplyTransaction(GateTransaction transaction)
        {
            Components.AddComponent(transaction.Created);
        }

        public void RevertGateTransaction(GateTransaction transaction)
        {
            if (Components.RemoveComponent(transaction.Created) == false)
            {
                Console.WriteLine($"Warn: Removed non-existent gate! {transaction.Created}");
            }
        }
    }
}
