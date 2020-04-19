using LogikUI.Circuit;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Transaction
{
    class WireTransaction : Transaction
    {
        public Wire Added;
        public List<Wire> Deleted;
        public List<Wire> Created;

        public WireTransaction(Wire added, List<Wire> deleted, List<Wire> created)
        {
            Added = added;
            Deleted = deleted;
            Created = created;
        }

        public override string ToString()
        {
            string added = $"Added: {Added}\n";
            string deleted = Deleted.Count > 0 ?  $"Deleted:\n - {string.Join("\n - ", Deleted)}\n" : "";
            string created = Created.Count > 0 ? $"Created:\n - {string.Join("\n - ", Created)}" : "";
            return $"{added}{deleted}{created}";
        }
    }
}
