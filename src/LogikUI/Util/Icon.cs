using Gdk;
using Gtk;
using LogikUI.Properties;
using LogikUI.Simulation.Gates;
using System.ComponentModel;
using System.Reflection;

namespace LogikUI.Util
{
    class Icon
    {
        public static Image GetComponentImage(ComponentType type)
        {
            return type switch
            {
                ComponentType.Buffer => BufferGate(),
                ComponentType.And => AndGate(),
                ComponentType.Or  => OrGate(),
                ComponentType.Xor => XorGate(),

                // FIXME: We might want to display a graphic that represents an unknown type
                _ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(ComponentType)),
            };
        }

        public static Image Selector() {
            Pixbuf pb = new Pixbuf(Resources.selector);
            pb = pb.ScaleSimple(24, 24, InterpType.Bilinear);
            return new Image(pb);
        }

        public static Image Wire() {
            Pixbuf pb = new Pixbuf(Resources.wire);
            pb = pb.ScaleSimple(24, 24, InterpType.Bilinear);
            return new Image(pb);
        }

        public static Image BufferGate() {
            Pixbuf pb = new Pixbuf(Resources.gate_buffer);
            pb = pb.ScaleSimple(24, 24, InterpType.Bilinear);
            return new Image(pb);
        }

        public static Image AndGate() {
            Pixbuf pb = new Pixbuf(Resources.gate_and);
            pb = pb.ScaleSimple(24, 24, InterpType.Bilinear);
            return new Image(pb);
        }

        public static Image OrGate() {
            Pixbuf pb = new Pixbuf(Resources.gate_or);
            pb = pb.ScaleSimple(24, 24, InterpType.Bilinear);
            return new Image(pb);
        }

        public static Image XorGate() {
            Pixbuf pb = new Pixbuf(Resources.gate_xor);
            pb = pb.ScaleSimple(24, 24, InterpType.Bilinear);
            return new Image(pb);
        }
    }
}
