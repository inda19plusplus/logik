using Gtk;
using LogikUI.Simulation.Gates;
using LogikUI.Transaction;
using LogikUI.Util;
using Pango;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Text;

namespace LogikUI.Circuit
{
    enum Orientation
    {
        East,
        South,
        West,
        North,
    }

    struct AndGate
    {
        public Vector2i Pos;
        public Orientation Orientation;

        public AndGate(Vector2i pos, Orientation orientation)
        {
            Pos = pos;
            Orientation = orientation;
        }

        public Vector2d GetTopLeft()
        {
            return Pos + Orientation switch
            {
                Orientation.North => new Vector2d(-1.5, 0),
                Orientation.East => new Vector2d(-3, -1.5),
                Orientation.South => new Vector2d(-1.5, -3),
                Orientation.West => new Vector2d(0, -1.5),
                _ => throw new InvalidEnumArgumentException(nameof(Orientation), (int)Orientation, typeof(Orientation)),
            };
        }
    }

    class Gates
    {
        public Components Components;
        
        public Gates()
        {
            Components = new Components();
            // FIXME: Register this somewhere else?
            Components.RegisterComponentType<AndGateInstance>(AndGateInstance.Draw);
            Components.RegisterComponentType<NotGateInstance>(NotGateInstance.Draw);
        }

        public void Draw(Cairo.Context cr)
        {
            Components.DrawComponents(cr);
        }

        public GateTransaction CreateAddGateTrasaction(IInstance gate)
        {
            // FIXME: Do some de-duplication stuff?
            return new GateTransaction(gate);
        }

        public void ApplyTransaction(GateTransaction transaction)
        {
            Components.AddComponent(transaction.Created);
        }

        public void RevertGateTransaction(GateTransaction transaction)
        {
            if (Components.RemoveComponent(transaction.Created) == false)
            {
                Console.WriteLine($"Warn: Removed non-existent gate! {transaction.Created}");
            }
        }

        public static void AndGate(Cairo.Context cr, AndGate gate)
        {
            double height = CircuitEditor.DotSpacing * 3;
            double width = CircuitEditor.DotSpacing * 3;

            // Shape
            {
                double horiz = gate.Orientation == Orientation.East ?
                    -1 : gate.Orientation == Orientation.West ?
                    1 : 0;
                double vert = gate.Orientation == Orientation.South ?
                    -1 : gate.Orientation == Orientation.North ?
                    1 : 0;

                var pos = gate.Pos * CircuitEditor.DotSpacing;
                var p1 = pos + horiz * new Vector2d(width, height / 2) + vert * new Vector2d(-width / 2, height);
                var p2 = pos + horiz * new Vector2d(width, -height / 2) + vert * new Vector2d(width / 2, height);
                var p3 = pos + new Vector2d(horiz * (width / 2), vert * (height / 2));

                // FIXME: This might be simplyfiable. If it's not maybe write a comment about why this works.
                double a1 = horiz * (Math.PI / 2) + ((-vert + 3 * Math.Abs(vert)) / 2) * (Math.PI);
                double a2 = a1 + Math.PI;

                cr.MoveTo(p1);
                cr.Arc(p3.X, p3.Y, width / 2, a1, a2);
                cr.LineTo(p2);
                cr.ClosePath();
            }
            cr.SetSourceRGB(0.1, 0.1, 0.1);
            cr.LineWidth = Wires.WireWidth;
            cr.Stroke();

            // Connection points
            {
                // FIXME: We kind of don't want to do this again?
                int horiz = gate.Orientation == Orientation.East ?
                    -1 : gate.Orientation == Orientation.West ?
                    1 : 0;
                int vert = gate.Orientation == Orientation.South ?
                    -1 : gate.Orientation == Orientation.North ?
                    1 : 0;

                var p1 = horiz * new Vector2i(3, 1) + vert * new Vector2i(1, 3);
                var p2 = horiz * new Vector2i(3, -1) + vert * new Vector2i(-1, 3);

                var in1 = (gate.Pos + p1) * CircuitEditor.DotSpacing;
                var in2 = (gate.Pos + p2) * CircuitEditor.DotSpacing;
                var out1 = gate.Pos * CircuitEditor.DotSpacing;

                // FIXME: Magic number radius...
                cr.Arc(in1.X, in1.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
                cr.Arc(in2.X, in2.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
                cr.Arc(out1.X, out1.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
            }
            cr.SetSourceRGB(0.2, 0.9, 0.2);
            cr.Fill();
        }

        public static void AndGate(Cairo.Context cr, List<AndGate> gates)
        {
            double height = CircuitEditor.DotSpacing * 3;
            double width = CircuitEditor.DotSpacing * 3;

            // Shape
            foreach (var gate in gates)
            {
                double horiz = gate.Orientation == Orientation.East ?
                    -1 : gate.Orientation == Orientation.West ?
                    1 : 0;
                double vert = gate.Orientation == Orientation.South ?
                    -1 : gate.Orientation == Orientation.North ?
                    1 : 0;

                var pos = gate.Pos * CircuitEditor.DotSpacing;
                var p1 = pos + horiz * new Vector2d(width, height / 2) + vert * new Vector2d(-width / 2, height);
                var p2 = pos + horiz * new Vector2d(width, -height / 2) + vert * new Vector2d(width / 2, height);
                var p3 = pos + new Vector2d(horiz * (width / 2), vert * (height / 2));

                // FIXME: This might be simplyfiable. If it's not maybe write a comment about why this works.
                double a1 = horiz * (Math.PI / 2) + ((-vert + 3 * Math.Abs(vert)) / 2) * (Math.PI);
                double a2 = a1 + Math.PI;

                cr.MoveTo(p1);
                cr.Arc(p3.X, p3.Y, width / 2, a1, a2);
                cr.LineTo(p2);
                cr.ClosePath();
            }
            cr.SetSourceRGB(0.1, 0.1, 0.1);
            cr.LineWidth = Wires.WireWidth;
            cr.Stroke();

            // Connection points
            foreach (var gate in gates)
            {
                // FIXME: We kind of don't want to do this again?
                int horiz = gate.Orientation == Orientation.East ?
                    -1 : gate.Orientation == Orientation.West ?
                    1 : 0;
                int vert = gate.Orientation == Orientation.South ?
                    -1 : gate.Orientation == Orientation.North ?
                    1 : 0;

                var p1 = horiz * new Vector2i(3, 1) + vert * new Vector2i(1, 3);
                var p2 = horiz * new Vector2i(3, -1) + vert * new Vector2i(-1, 3);

                var in1 = (gate.Pos + p1) * CircuitEditor.DotSpacing;
                var in2 = (gate.Pos + p2) * CircuitEditor.DotSpacing;
                var out1 = gate.Pos * CircuitEditor.DotSpacing;

                // FIXME: Magic number radius...
                cr.Arc(in1.X, in1.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
                cr.Arc(in2.X, in2.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
                cr.Arc(out1.X, out1.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
            }
            //cr.SetSourceRGB(0.3, 0.3, 0.3);
            cr.SetSourceRGB(0.2, 0.9, 0.2);
            cr.Fill();
        }

        public static void AndGate(Cairo.Context cr, AndGate[] gates)
        {
            double height = CircuitEditor.DotSpacing * 3;
            double width = CircuitEditor.DotSpacing * 3;

            // Shape
            foreach (var gate in gates)
            {
                double horiz = gate.Orientation == Orientation.East ?
                    -1 : gate.Orientation == Orientation.West ?
                    1 : 0;
                double vert = gate.Orientation == Orientation.South ?
                    -1 : gate.Orientation == Orientation.North ?
                    1 : 0;

                var pos = gate.Pos * CircuitEditor.DotSpacing;
                var p1 = pos + horiz * new Vector2d(width, height / 2) + vert * new Vector2d(-width / 2, height);
                var p2 = pos + horiz * new Vector2d(width, -height / 2) + vert * new Vector2d(width / 2, height);
                var p3 = pos + new Vector2d(horiz * (width/2), vert * (height / 2));

                // FIXME: This might be simplyfiable. If it's not maybe write a comment about why this works.
                double a1 = horiz * (Math.PI / 2) + ((-vert + 3 * Math.Abs(vert)) / 2) * (Math.PI);
                double a2 = a1 + Math.PI;

                cr.MoveTo(p1);
                cr.Arc(p3.X, p3.Y, width / 2, a1, a2);
                cr.LineTo(p2);
                cr.ClosePath();
            }
            cr.SetSourceRGB(0.1, 0.1, 0.1);
            cr.LineWidth = Wires.WireWidth;
            cr.Stroke();

            // Connection points
            foreach (var gate in gates)
            {
                // FIXME: We kind of don't want to do this again?
                int horiz = gate.Orientation == Orientation.East ?
                    -1 : gate.Orientation == Orientation.West ?
                    1 : 0;
                int vert = gate.Orientation == Orientation.South ?
                    -1 : gate.Orientation == Orientation.North ?
                    1 : 0;

                var p1 = horiz * new Vector2i(3, 1) + vert * new Vector2i(1, 3);
                var p2 = horiz * new Vector2i(3, -1) + vert * new Vector2i(-1, 3);

                var in1 = (gate.Pos + p1) * CircuitEditor.DotSpacing;
                var in2 = (gate.Pos + p2) * CircuitEditor.DotSpacing;
                var out1 = gate.Pos * CircuitEditor.DotSpacing;

                // FIXME: Magic number radius...
                cr.Arc(in1.X, in1.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
                cr.Arc(in2.X, in2.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
                cr.Arc(out1.X, out1.Y, 2, 0, Math.PI * 2);
                cr.ClosePath();
            }
            //cr.SetSourceRGB(0.3, 0.3, 0.3);
            cr.SetSourceRGB(0.2, 0.9, 0.2);
            cr.Fill();
        }
    }
}
