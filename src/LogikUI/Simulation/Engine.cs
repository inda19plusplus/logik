using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Simulation
{
    class Engine
    {
        public long CurrentTime;

        // What is a dirty instance?
        // A dirty instance

        // Priority queue for events
        public readonly PriorityQueue<SetEvent> Events = new PriorityQueue<SetEvent>();

        // TODO: Change this to UniqueList<T> or something similar for faster itteration
        public readonly HashSet<Subnet> DirtyNets = new HashSet<Subnet>();
        //List<Instance> dirtyInstances = new List<Instance>();

        public bool IsStepPending()
        {
            // FIXME: There will be more things to consider than this!
            return Events.Count > 0;
        }

        public void QueueSet(Node port, Value value, int delay)
        {
            if (delay <= 0) 
                throw new ArgumentException($"Delay must be a positive value larger than zero! (Got {delay})");

            Events.Enqueue(new SetEvent(port, value, CurrentTime + delay));
        }

        public Value Get(Node port)
        {
            return Value.Floating;
        }

        public void Step()
        {
            long now = CurrentTime++;
            Console.WriteLine($"Simulation step: {now}");

            while (Events.Count > 0 && Events.Peek().When == now)
            {
                SetEvent @event = Events.Dequeue();
                
                // Check to see if the new value is different
                // from the current value
                if (true) // FIXME
                {
                    // Find the net the port belongs to
                    Subnet? net = null;
                    DirtyNets.Add(net!);
                }

                ProcessDirty();
            }
        }

        public void ProcessDirty()
        {
            // FIXME
        }
    }
}
