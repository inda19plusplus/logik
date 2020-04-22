using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Simulation.Gates
{
    struct AndGateInstance : IInstance
    {
        // Indices for the ports
        public Node In1;
        public Node In2;
        public Node Out;
    }

    class AndGate : Component<AndGateInstance>
    {
        public override void Propagate(Engine engine, AndGateInstance instance)
        {
            Value res = Value.And(engine.Get(instance.In1), engine.Get(instance.In2));
            engine.QueueSet(instance.Out, res, 1);
        }
    }
}
