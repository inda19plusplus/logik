using Cairo;
using LogikUI.Circuit;
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
            using var transform = IComponent.ApplyComponentTransform(cr, data);

            //foreach (var gate in instances)
            {
                cr.Rectangle(-30, -15, 30, 30);
                cr.ClosePath();
            }

            // FIXME: We probably shouldn't hardcode the color
            cr.SetSourceRGB(0.1, 0.1, 0.1);
            cr.LineWidth = Wires.WireWidth;
            cr.Stroke();

            //foreach (var gate in instances)
            {
                Span<Vector2i> points = stackalloc Vector2i[NumberOfPorts];
                GetPorts(points);

                for (int i = 0; i < NumberOfPorts; i++)
                {
                    IComponent.DrawRoundPort(cr, data, points, i);
                }
            }
        }
    }
}
