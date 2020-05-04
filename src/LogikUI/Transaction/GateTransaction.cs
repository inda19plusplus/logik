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

        public GateTransaction(InstanceData created)
        {
            Created = created;
        }

        public override string ToString()
        {
            return $"Gate: ({Created})";
        }
    }
}
