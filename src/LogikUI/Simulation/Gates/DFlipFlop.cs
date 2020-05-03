using Cairo;
using LogikUI.Circuit;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Simulation.Gates
{
    struct DFlipFlopInstance : IInstance
    {
        public string Name => "D FLip-Flop";
        public Vector2i Position { get; set; }
        public Orientation Orientation { get; set; }
        public int NumberOfPorts => 4;
        public UIntPtr BackendIdx { get; set; }

        UIntPtr IInstance.BackendId => new UIntPtr(69);

        public void GetPorts(Span<Vector2i> ports)
        {
            ports[0] = new Vector2i(0, 0);
            ports[1] = new Vector2i(0, 2);
            ports[2] = new Vector2i(-2, 0);
            ports[3] = new Vector2i(-2, 2);
        }

        public static void Draw(Context cr, Span<DFlipFlopInstance> instances)
        {
            throw new NotImplementedException();
        }
    }
}
