using LogikUI.Circuit;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Transaction
{
    class WireTransaction : Transaction
    {
        public List<Wire> Deleted;
        public List<Wire> Created;

        public WireTransaction(List<Wire> deleted, List<Wire> created)
        {
            Deleted = deleted;
            Created = created;
        }
    }
}
