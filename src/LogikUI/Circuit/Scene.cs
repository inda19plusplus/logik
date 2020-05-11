using Cairo;
using LogikUI.Transaction;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Circuit
{
    class Scene
    {
        public Wires Wires;
        public Gates Gates;
        public TextLabels Labels;

        public TransactionStack Transactions = new TransactionStack();

        public Scene(Wires wires, Gates gates, TextLabels labels)
        {
            Wires = wires;
            Gates = gates;
            Labels = labels;
        }

        /// <summary>
        /// This applies the given transaction and adds it to the undo stack.
        /// </summary>
        public void PushTransaction(Transaction.Transaction transaction)
        {
            DoTransactionNoPush(transaction);
            Transactions.PushTransaction(transaction);
        }

        /// <summary>
        /// Applies a transaction without pushing it to the undo stack.
        /// This method can cause inconsistencies in the undo system and should be used with care.
        /// </summary>
        public void DoTransactionNoPush(Transaction.Transaction transaction)
        {
            switch (transaction)
            {
                case WireTransaction wt:
                    Wires.ApplyTransaction(wt);
                    break;
                case GateTransaction gt:
                    Gates.ApplyTransaction(gt);
                    if (gt.ConnectionPointWireEdits != null)
                        Wires.ApplyTransaction(gt.ConnectionPointWireEdits);
                    // FIXME: Clean this up, this is just so that we can get something simulating
                    Wires.AddComponent(gt.Gate);
                    break;
                case BundledTransaction bt:
                    {
                        foreach (var bundled in bt.BundledTransactions)
                            DoTransactionNoPush(bundled);
                        break;
                    }
                default:
                    throw new Exception($"Unknown transaction type! {transaction.GetType()}");
            }
        }

        /// <summary>
        /// Undos a transaction without changing the undo stack.
        /// This method can cause inconsistencies in the undo system and should be used with care.
        /// </summary>
        public void UndoTransactionNoPush(Transaction.Transaction transaction)
        {
            switch (transaction)
            {
                case WireTransaction wt:
                    Wires.RevertTransaction(wt);
                    break;
                case GateTransaction gt:
                    Gates.RevertTransaction(gt);
                    if (gt.ConnectionPointWireEdits != null)
                        Wires.RevertTransaction(gt.ConnectionPointWireEdits);
                    // FIXME: Clean this up, this is just so that we can get something simulating
                    Wires.RemoveComponent(gt.Gate);
                    break;
                case BundledTransaction bt:
                    {
                        foreach (var bundled in bt.BundledTransactions)
                            UndoTransactionNoPush(bundled);
                        break;
                    }
                default:
                    throw new Exception($"Unknown transaction type! {transaction.GetType()}");
            }
        }

        public bool Undo()
        {
            if (Transactions.TryUndo(out var transaction))
            {
                // We use the no-push variant here because
                // TryUndo already modified the undo stack.
                UndoTransactionNoPush(transaction);
                Console.WriteLine($"Undid transaction: {transaction}");
                return true;
            }
            else return false;
        }

        public bool Redo()
        {
            if (Transactions.TryRedo(out var transaction))
            {
                // We use no-push here because TryRedo already did the push.
                DoTransactionNoPush(transaction);
                Console.WriteLine($"Redid transaction: {transaction}");
                return true;
            }
            else return false;
        }

        public void Draw(Context cr)
        {
            Wires.Draw(cr);
            Gates.Draw(cr);
            Labels.Draw(cr);
        }
    }
}
