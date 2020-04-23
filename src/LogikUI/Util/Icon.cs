using Gdk;
using Gtk;

namespace LogikUI.Util
{
    class Icon
    {
        public static Image Selector() {
            Pixbuf pb = new Pixbuf("/Users/weerox/logik/src/LogikUI/img/selector.png");
            pb = pb.ScaleSimple(24, 24, InterpType.Bilinear);
            return new Image(pb);
        }

        public static Image Wire() {
            Pixbuf pb = new Pixbuf("/Users/weerox/logik/src/LogikUI/img/wire.png");
            pb = pb.ScaleSimple(24, 24, InterpType.Bilinear);
            return new Image(pb);
        }

        public static Image AndGate() {
            Pixbuf pb = new Pixbuf("/Users/weerox/logik/src/LogikUI/img/gate_and.png");
            pb = pb.ScaleSimple(24, 24, InterpType.Bilinear);
            return new Image(pb);
				}

        public static Image OrGate() {
            Pixbuf pb = new Pixbuf("/Users/weerox/logik/src/LogikUI/img/gate_or.png");
            pb = pb.ScaleSimple(24, 24, InterpType.Bilinear);
            return new Image(pb);
				}

        public static Image XorGate() {
            Pixbuf pb = new Pixbuf("/Users/weerox/logik/src/LogikUI/img/gate_xor.png");
            pb = pb.ScaleSimple(24, 24, InterpType.Bilinear);
            return new Image(pb);
				}
    }
}
