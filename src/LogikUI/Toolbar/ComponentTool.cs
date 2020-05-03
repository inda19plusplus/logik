using Cairo;
using Gtk;
using LogikUI.Circuit;
using LogikUI.Component;
using LogikUI.Simulation.Gates;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Toolbar
{
    class ComponentTool : BasicTool
    {
        public IComponent BaseComponent;
        public bool PlacingComponent = false;
        public bool DraggingComponent = false;
        public Vector2i StartPosition;
        public Vector2i VisualPosition;
        public Circuit.Orientation CompOrientation;

        // FIXME: Technically we can get the name of the component from the ComponentType
        // So we shouldn't need to have the name argument here because it should be able to be derived.
        public ComponentTool(ComponentType type, string name, CircuitEditor circuitEditor, Gtk.Toolbar toolbar) 
            : base(Util.Icon.GetComponentImage(type), name, circuitEditor, toolbar)
        {
            if (circuitEditor.Gates.Components.TryGetValue(type, out var component) == false)
            {
                throw new InvalidOperationException($"There is no ICompoennt for the component type '{type}'");
            }
            BaseComponent = component!;
        }

        public override void Select(CircuitEditor editor)
        {
            PlacingComponent = true;
            editor.DrawingArea.QueueDraw();
        }

        public override void DeSelect(CircuitEditor editor)
        {
        }

        // FIXME: Keyboard and general mouse movement events!!

        public override void MouseMoved(CircuitEditor editor, Vector2d mousePos)
        {
            if (PlacingComponent && DraggingComponent == false)
            {
                StartPosition = editor.RoundToGrid(mousePos);
                VisualPosition = StartPosition;
                CompOrientation = Circuit.Orientation.East;
            }
            
            editor.DrawingArea.QueueDraw();
        }

        public override void GestureStart(CircuitEditor editor, Vector2d dragStartPos)
        {
            StartPosition = editor.RoundToGrid(dragStartPos);
            VisualPosition = StartPosition;
            CompOrientation = Circuit.Orientation.East;
            DraggingComponent = true;

            editor.DrawingArea.QueueDraw();
        }

        public override void GestureUpdate(CircuitEditor editor, Vector2d offset)
        {
            VisualPosition = StartPosition + editor.RoundDistToGrid(offset);

            editor.DrawingArea.QueueDraw();
        }

        public override void GestureEnd(CircuitEditor editor, Vector2d endOffset)
        {
            VisualPosition = StartPosition + editor.RoundDistToGrid(endOffset);
            StartPosition = VisualPosition;

            var transaction = editor.Gates.CreateAddGateTransaction(
                // FIXME!!!
                InstanceData.Create(BaseComponent.Type, VisualPosition, CompOrientation)
                //BaseComponent.Create(VisualPosition, CompOrientation)
                //new Circuit.AndGate(VisualPosition, CompOrientation)
                );

            editor.Gates.ApplyTransaction(transaction);
            editor.Transactions.PushTransaction(transaction);

            VisualPosition = StartPosition = Vector2i.Zero;
            DraggingComponent = false;

            editor.DrawingArea.QueueDraw();
            // FIXME: Add the component!
        }

        public override void Draw(CircuitEditor editor, Context cr)
        {
            if (PlacingComponent)
            {
                BaseComponent.Draw(cr, InstanceData.Create(BaseComponent.Type, VisualPosition, CompOrientation));
            }
        }
    }
}
