using LogikUI.Circuit;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogikUI.Transaction
{
    class ConnectionPointsTransaction : Transaction
    {
        // NOTE: We might want to bring this inline with
        // WireTransaction and have two lists of removed and
        // added connection points. 
        // But for right now we are using a bool for this.
        public bool RemovingPoints = false;
        public Vector2i[] ControlPoints;
        public List<Wire>? CreatedWires;
        public List<Wire>? DeletedWires;

        public ConnectionPointsTransaction(bool removing, Span<Vector2i> addedPoints, List<Wire>? deleted, List<Wire>? created)
        {
            RemovingPoints = removing;
            ControlPoints = addedPoints.ToArray();
            DeletedWires = deleted;
            CreatedWires = created;
        }

        public override string ToString()
        {
            return $"Points {(RemovingPoints ? "removed" : "added")}: {string.Join(", ", ControlPoints)}, Wires created: \n\t{string.Join("\n\t", CreatedWires??Enumerable.Empty<Wire>())},\nWires Deleted: \n\t{string.Join("\n\t", DeletedWires??Enumerable.Empty<Wire>())}";
        }
    }
}
