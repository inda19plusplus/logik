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
        public IInstance Created;

        public GateTransaction(IInstance created)
        {
            Created = created;
        }

        public override string ToString()
        {
            return $"Gate: ({Created})";
        }
    }
}
