using Cairo;
using LogikUI.Circuit;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace LogikUI.Simulation.Gates
{
    interface IComponent
    {
        public string Name { get; }
        public ComponentType Type { get; }
        public int NumberOfPorts { get; }

        public void GetPorts(Span<Vector2i> ports);
        
        public void Draw(Context cr, InstanceData data);

        // FIXME: There might exist a better place for this...
        static ComponentTransform ApplyComponentTransform(Context cr, InstanceData data)
        {
            var transform = new ComponentTransform(cr);
            cr.Translate(data.Position.X * CircuitEditor.DotSpacing, data.Position.Y * CircuitEditor.DotSpacing);
            cr.Rotate(data.Orientation.ToAngle());
            return transform;
        }

        readonly ref struct ComponentTransform
        {
            public readonly Matrix ResetMatrix;
            public readonly Context Context;

            public ComponentTransform(Context context)
            {
                ResetMatrix = context.Matrix;
                Context = context;
            }

            public void Dispose()
            {
                Context.Matrix = ResetMatrix;
            }
        }
    }
}
