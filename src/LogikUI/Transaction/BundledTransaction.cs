using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Transaction
{
    class BundledTransaction : Transaction
    {
        public List<Transaction> BundledTransactions;

        public BundledTransaction(List<Transaction> transactions)
        {
            BundledTransactions = transactions;
        }

        public override string ToString()
        {
            return $"Bundled Transaction: \n\t{string.Join("\n\t", BundledTransactions)}";
        }
    }
}
