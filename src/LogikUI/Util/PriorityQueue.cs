using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Util
{
    // From https://visualstudiomagazine.com/Articles/2012/11/01/Priority-Queues-with-C.aspx?Page=2
    // TODO: Maybe implement this ourselves
    public class PriorityQueue<T> where T : IComparable<T>
    {
        public readonly List<T> Data;

        public PriorityQueue()
        {
            this.Data = new List<T>();
        }

        public void Enqueue(T item)
        {
            Data.Add(item);
            int ci = Data.Count - 1; // child index; start at end
            while (ci > 0)
            {
                int pi = (ci - 1) / 2; // parent index
                if (Data[ci].CompareTo(Data[pi]) >= 0) break; // child item is larger than (or equal) parent so we're done
                T tmp = Data[ci]; Data[ci] = Data[pi]; Data[pi] = tmp;
                ci = pi;
            }
        }

        public T Dequeue()
        {
            // assumes pq is not empty; up to calling code
            int li = Data.Count - 1; // last index (before removal)
            T frontItem = Data[0];   // fetch the front
            Data[0] = Data[li];
            Data.RemoveAt(li);

            --li; // last index (after removal)
            int pi = 0; // parent index. start at front of pq
            while (true)
            {
                int ci = pi * 2 + 1; // left child index of parent
                if (ci > li) break;  // no children so done
                int rc = ci + 1;     // right child
                if (rc <= li && Data[rc].CompareTo(Data[ci]) < 0) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
                    ci = rc;
                if (Data[pi].CompareTo(Data[ci]) <= 0) break; // parent is smaller than (or equal to) smallest child so done
                T tmp = Data[pi]; Data[pi] = Data[ci]; Data[ci] = tmp; // swap parent and child
                pi = ci;
            }
            return frontItem;
        }

        public T Peek()
        {
            T frontItem = Data[0];
            return frontItem;
        }

        public int Count =>  Data.Count;

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < Data.Count; ++i)
                s += Data[i].ToString() + " ";
            s += "count = " + Data.Count;
            return s;
        }

        public bool IsConsistent()
        {
            // is the heap property true for all data?
            if (Data.Count == 0) return true;
            int li = Data.Count - 1; // last index
            for (int pi = 0; pi < Data.Count; ++pi) // each parent index
            {
                int lci = 2 * pi + 1; // left child index
                int rci = 2 * pi + 2; // right child index

                if (lci <= li && Data[pi].CompareTo(Data[lci]) > 0) return false; // if lc exists and it's greater than parent then bad.
                if (rci <= li && Data[pi].CompareTo(Data[rci]) > 0) return false; // check the right child too.
            }
            return true; // passed all checks
        }
    }
}
