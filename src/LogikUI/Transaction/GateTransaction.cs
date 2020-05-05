using Gdk;
using LogikUI.Circuit;
using LogikUI.Simulation.Gates;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Transaction
{
    class GateTransaction : Transaction
    {
        //public IInstance Created;
        // FIXME: We might want to store something else...?
        public InstanceData Created;

        // FIXME: This feels a bit like a hack...
        public AddControlPointsTransaction? ConnectionPointWireEdits;

        public GateTransaction(InstanceData created, AddControlPointsTransaction? wireEdits)
        {
            Created = created;
            ConnectionPointWireEdits = wireEdits;
        }

        public override string ToString()
        {
            return $"Gate: ({Created}), {(ConnectionPointWireEdits != null ? $"Wires changed: ({ConnectionPointWireEdits})" : "No wires changed.")}";
        }
    }
}
