using LogikUI.Circuit;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogikUI.Transaction
{
    class AddControlPointsTransaction : Transaction
    {
        public Vector2i[] AddedControlPoints;
        public List<Wire>? CreatedWires;
        public List<Wire>? DeletedWires;

        public AddControlPointsTransaction(Span<Vector2i> addedPoints, List<Wire>? deleted, List<Wire>? created)
        {
            AddedControlPoints = addedPoints.ToArray();
            DeletedWires = deleted;
            CreatedWires = created;
        }

        public override string ToString()
        {
            return $"Points added: {string.Join(", ", AddedControlPoints)}, Wires created: \n\t{string.Join("\n\t", CreatedWires??Enumerable.Empty<Wire>())},\nWires Deleted: \n\t{string.Join("\n\t", DeletedWires??Enumerable.Empty<Wire>())}";
        }
    }
}
