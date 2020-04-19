using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LogikUI.Transaction
{
    class TransactionStack
    {
        public Transaction[] Transactions;
        public int Max;
        public int Caret;

        public TransactionStack()
        {
            Transactions = new Transaction[0];
            Max = 0;
            Caret = 0;
        }

        public void PushTransaction(Transaction transaction)
        {
            if (Caret + 1 >= Transactions.Length)
            {
                // Same resizing scheme as java lists
                Array.Resize(ref Transactions, Math.Max(Transactions.Length + Transactions.Length / 2, Caret + 1));
            }

            Transactions[Caret++] = transaction;
            Max = Caret;
        }

        public bool TryUndo([NotNullWhen(true)] out Transaction? transaction)
        {
            if (Caret > 0)
            {
                transaction = Transactions[--Caret];
                return true;
            }
            else
            {
                transaction = default;
                return false;
            }
        }

        public bool TryRedo([NotNullWhen(true)] out Transaction? transaction)
        {
            if (Caret < Max)
            {
                transaction = Transactions[Caret++];
                return true;
            }
            else
            {
                transaction = default;
                return false;
            }
        }
    }
}
