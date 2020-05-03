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
    class ComponentTool<T> : BasicTool where T : IComponent
    {
        public T BaseComponent;
        public bool PlacingComponent = false;
        public bool DraggingComponent = false;
        public Vector2i StartPosition;
        public Vector2i VisualPosition;
        public Circuit.Orientation CompOrientation;

        public ComponentTool(Gtk.Image image, T component, CircuitEditor circuitEditor, Gtk.Toolbar toolbar) 
            : base(image, component.Name, circuitEditor, toolbar)
        {
            BaseComponent = component;
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

            var transaction = editor.Gates.CreateAddGateTrasaction(
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
