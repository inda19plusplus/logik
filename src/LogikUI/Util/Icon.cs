using Gdk;
using Gtk;
using LogikUI.Properties;
using System.Reflection;

namespace LogikUI.Util
{
    class Icon
    {
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
