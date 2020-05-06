using Cairo;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Simulation.Gates
{
    class Constant : IComponent
    {
        public string Name => "Constant";
        public ComponentType Type => ComponentType.Constant;
        public int NumberOfPorts => 1;

        public void GetPorts(Span<Vector2i> ports)
        {
            ports[0] = new Vector2i(0, 0);
        }

        public void Draw(Context cr, InstanceData data)
        {
            throw new NotImplementedException();
        }
    }
}
