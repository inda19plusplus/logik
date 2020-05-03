using Cairo;
using LogikUI.Circuit;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Simulation.Gates
{
    struct DFlipFlop : IComponent
    {
        public string Name => "D FLip-Flop";
        public ComponentType Type => ComponentType.DFlipFlop;
        public int NumberOfPorts => 4;

        public void GetPorts(Span<Vector2i> ports)
        {
            ports[0] = new Vector2i(0, 0);
            ports[1] = new Vector2i(0, 2);
            ports[2] = new Vector2i(-2, 0);
            ports[3] = new Vector2i(-2, 2);
        }

        public void Draw(Context cr, InstanceData data)
        {
            throw new NotImplementedException();
        }
    }
}
