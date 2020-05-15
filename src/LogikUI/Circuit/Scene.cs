using Cairo;
using LogikUI.Interop;
using LogikUI.Simulation;
using LogikUI.Simulation.Gates;
using LogikUI.Transaction;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogikUI.Circuit
{
    class Scene
    {
        public Wires Wires;
        public Gates Gates;
        public TextLabels Labels;

        // FIXME!!
        public int SubnetIDCounter = 1;

        public List<Subnet> Subnets = new List<Subnet>();

        public TransactionStack Transactions = new TransactionStack();

        public Scene(Wires wires, Gates gates, TextLabels labels)
        {
            Wires = wires;
            Gates = gates;
            Labels = labels;

            // FIXME: Register these subnets with the backend!
            Subnets = CreateSubnetsFromWires(Wires.WiresList);
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
                    // Update the subnets
                    UpdateSubnets(wt.Created, wt.Deleted);
                    break;
                case GateTransaction gt:
                    Gates.ApplyTransaction(gt);
                    if (gt.ConnectionPointWireEdits != null)
                        Wires.ApplyTransaction(gt.ConnectionPointWireEdits);
                    // FIXME: Clean this up, this is just so that we can get something simulating
                    if (gt.RemoveComponent)
                    {
                        this.RemoveComponent(gt.Gate);
                    }
                    else
                    {
                        this.AddComponent(gt.Gate);
                    }
                    // Update the subnets
                    UpdateSubnets(
                        gt.ConnectionPointWireEdits?.CreatedWires ?? new List<Wire>(),
                        gt.ConnectionPointWireEdits?.DeletedWires ?? new List<Wire>());
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
                    // Update the subnets, but in reverse
                    UpdateSubnets(wt.Deleted, wt.Created);
                    break;
                case GateTransaction gt:
                    Gates.RevertTransaction(gt);
                    if (gt.ConnectionPointWireEdits != null)
                        Wires.RevertTransaction(gt.ConnectionPointWireEdits);
                    // FIXME: Clean this up, this is just so that we can get something simulating
                    this.RemoveComponent(gt.Gate);
                    // Update the subnets, but in reverse
                    UpdateSubnets(
                        gt.ConnectionPointWireEdits?.DeletedWires ?? new List<Wire>(),
                        gt.ConnectionPointWireEdits?.CreatedWires ?? new List<Wire>());
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
            // FIXME: Make this a static method and do the subnet loop here instead.
            Wires.Draw(cr, Subnets);
            Gates.Draw(cr);
            Labels.Draw(cr);

            // Draw the subnet id next to all connections (this is for debug)
            foreach (var net in Subnets)
            {
                foreach (var (comp, port) in net.ComponentPorts)
                {
                    Span<Vector2i> ports = stackalloc Vector2i[Gates.GetNumberOfPorts(comp)];
                    Gates.GetTransformedPorts(comp, ports);

                    cr.MoveTo(ports[port] * CircuitEditor.DotSpacing);
                    cr.ShowText(net.ID.ToString());
                }
            }

            foreach (var instance in Gates.Instances)
            {
                var bounds = Gates.GetBounds(instance);
                bounds = bounds.Rotate(instance.Position * CircuitEditor.DotSpacing, instance.Orientation);
                cr.Rectangle(bounds);
            }
            cr.SetSourceRGB(0.2, 0.2, 0.2);
            cr.Stroke();
        }

        private Subnet? FindSubnet(Vector2i pos)
        {
            foreach (var bundle in Subnets)
            {
                foreach (var wire in bundle.Wires)
                {
                    if (wire.IsPointOnWire(pos))
                    {
                        return bundle;
                    }

                }
            }
            return null;
        }

        public void UpdateSubnets(List<Wire> added, List<Wire> removed)
        {
            List<Subnet> checkSplitSubnets = new List<Subnet>();

            foreach (var old in removed)
            {
                var startNet = FindSubnet(old.Pos);
                var endNet = FindSubnet(old.EndPos);

                // This is null if there was an error removing the wire
                Subnet? removedFromSubnet;
                if (startNet != null && endNet != null)
                {
                    if (startNet == endNet)
                    {
                        // We are removing a wire from a subnet
                        // So here we want to figure out if we have to split this subnet
                        startNet.RemoveWire(old);

                        // If there are wires left, we need to check if 
                        // the deletion led to any splits in the subnet
                        if (startNet.Wires.Count > 0)
                        {
                            if (checkSplitSubnets.Contains(startNet) == false)
                            {
                                checkSplitSubnets.Add(startNet);
                            }
                        }

                        removedFromSubnet = startNet;
                    }
                    else
                    {
                        // This is f***ed in more ways than one...
                        Console.WriteLine($"Error! This should not happen! Trying to remove a wire containing to more than one subnet! (Wire: {old}, Subnet1: {startNet}, Subnet2:{endNet})");
                        removedFromSubnet = null;
                    }
                }
                else if (startNet != null)
                {
                    // We are removing from one subnet
                    if (startNet.RemoveWire(old))
                    {
                        Console.WriteLine($"Warn: Tried to remove a wire from a subnet that didn't contain that wire. (Wire: {old}, Subnet: {startNet})");
                    }

                    removedFromSubnet = startNet;

                    Console.WriteLine($"Removed wire to subnet: {startNet}");
                }
                else if (endNet != null)
                {
                    // We are removing from one subnet
                    if (endNet.RemoveWire(old))
                    {
                        Console.WriteLine($"Warn: Tried to remove a wire from a subnet that didn't contain that wire. (Wire: {old}, Subnet: {endNet})");
                    }

                    removedFromSubnet = endNet;

                    Console.WriteLine($"Removed wire to subnet: {endNet}");
                }
                else
                {
                    // Here we are removing a wire that didn't belong to any subnet!?
                    Console.WriteLine($"Error! This should not happen! Trying to remove a wire not contained in any subnet! (Wire: {old})");
                    removedFromSubnet = null;
                }

                // If we removed a wire from a subnet, check if we need to deleted that subnet
                if (removedFromSubnet?.Wires.Count <= 0)
                {
                    Console.WriteLine($"Removed subnet: {removedFromSubnet}");
                    LogLogic.RemoveSubnet(Program.Backend, removedFromSubnet.ID);
                    removedFromSubnet.ID = 0;

                    Subnets.Remove(removedFromSubnet);
                }
            }

            List<Subnet> addedSubnets = new List<Subnet>();
            List<(Wire, Subnet)> addedWiresAndTheirNewSubnet = new List<(Wire, Subnet)>();

            foreach (var @new in added)
            {
                var startNet = FindSubnet(@new.Pos);
                var endNet = FindSubnet(@new.EndPos);

                Subnet subnetAddedTo;
                if (startNet != null && endNet != null)
                {
                    if (startNet == endNet)
                    {
                        // Here they are the same subnet.
                        // So we just add the wire.
                        startNet.AddWire(@new);
                        subnetAddedTo = startNet;
                    }
                    else
                    {
                        // Figure out what subnet to merge into.
                        var (merged, mergee) = startNet.ID != 0 ?
                            (startNet, endNet) :
                            (endNet, startNet);

                        Console.WriteLine($"Merging subnet ({mergee}) into subnet ({merged}).");
                        Subnets.Remove(mergee);
                        merged.Merge(mergee);

                        // Transfer over all of the added wires if there are any
                        for (int i = 0; i < addedWiresAndTheirNewSubnet.Count; i++)
                        {
                            var (wire, subnet) = addedWiresAndTheirNewSubnet[i];
                            if (subnet == mergee)
                            {
                                addedWiresAndTheirNewSubnet[i] = (wire, merged);
                            }
                        }

                        // Don't forget to add the wire that merged these subnets
                        merged.AddWire(@new);
                        subnetAddedTo = merged;

                        Console.WriteLine($"\tResult: {merged}");
                    }
                }
                else if (startNet != null)
                {
                    // Here we just add this wire to the subnet,
                    // it's not going to change anything.
                    startNet.AddWire(@new);
                    subnetAddedTo = startNet;
                    Console.WriteLine($"Added wire to subnet: {startNet}");
                }
                else if (endNet != null)
                {
                    // Here we just add this wire to the subnet,
                    // it's not going to change anything.
                    endNet.AddWire(@new);
                    subnetAddedTo = endNet;
                    Console.WriteLine($"Added wire to subnet: {endNet}");
                }
                else
                {
                    // This means that this wire should be in it's own subnet.
                    // It might get merged into another subnet later though..
                    var sub = new Subnet(0);
                    sub.AddWire(@new);

                    addedSubnets.Add(sub);

                    // NOTE: do we want to do this?
                    Subnets.Add(sub);

                    subnetAddedTo = sub;
                    Console.WriteLine($"Added single wire subnet: {sub}");
                }

                addedWiresAndTheirNewSubnet.Add((@new, subnetAddedTo));
            }

            // Because we have figured out all of the merging we know 
            // that all of the subnets in this list will have their own
            // id. When splitting we can only ever create new subnets 
            // so we don't have to worry about merging so we give those
            // subnets their id directly when they are created.
            foreach (var addedNet in addedSubnets)
            {
                // Here we should figure out a new subnet id
                addedNet.ID = SubnetIDCounter++;
                LogLogic.AddSubnet(Program.Backend, addedNet.ID);
                Console.WriteLine($"Added new subnet: {addedNet}");
            }

            // Here we go though all new wires and their subnet to see if the new wires
            // connects any new components to the subnet.
            foreach (var (wire, subnet) in addedWiresAndTheirNewSubnet)
            {
                foreach (var comp in Gates.Instances)
                {
                    // FIXME: This stackalloc is going to blow up...
                    Span<Vector2i> portLocs = stackalloc Vector2i[Gates.GetNumberOfPorts(comp)];
                    Gates.GetTransformedPorts(comp, portLocs);

                    for (int i = 0; i < portLocs.Length; i++)
                    {
                        var loc = portLocs[i];
                        if (wire.IsConnectionPoint(loc))
                        {
                            if (subnet.ComponentPorts.Contains((comp, i)) == false)
                            {
                                subnet.AddComponent(comp, i);
                                Console.WriteLine($"Added component ({comp}, port: {i}) to the subnet: {subnet}.");
                            }
                        }
                    }
                }
            }

            // Here we check all of the splitting that needs to be done.
            foreach (var split in checkSplitSubnets)
            {
                // Here we need to check if this subnet 
                // has to be split into multiple subnets
                
                if (split.ID == 0)
                {
                    Console.WriteLine($"We don't need to check for splits on this subnet because it has been removed! Subnet: {split}");
                    continue;
                }

                if (split.Wires.Count == 0)
                {
                    Console.WriteLine($"We don't need to try to split a subnet without wires. Subnet: {split}");
                    continue;
                }
                
                Console.WriteLine($"Checking subnet ({split}) for splits!");

                List<Wire> wiresLeft = new List<Wire>(split.Wires);

                bool assignedOriginal = false;
                var newSubnets = new List<Subnet>();

                while (wiresLeft.Count > 0)
                {
                    // FIXME: Switch to union find for this...
                    static void FloodFill(List<Wire> wires, List<Wire> toCheck, Wire currentWire)
                    {
                        if (wires.Contains(currentWire))
                            return;

                        wires.Add(currentWire);

                        for (int i = toCheck.Count - 1; i >= 0; i--)
                        {
                            Wire wire = toCheck[i];
                            if (wire.IsPointOnWire(currentWire.Pos))
                            {
                                FloodFill(wires, toCheck, wire);
                            }
                            else if (wire.IsPointOnWire(currentWire.EndPos))
                            {
                                FloodFill(wires, toCheck, wire);
                            }

                            // Do don't need to check anymore
                            if (wires.Count == toCheck.Count)
                                return;
                        }
                    }
                    
                    Wire startWire = wiresLeft[0];
                    List<Wire> island = new List<Wire>();
                    FloodFill(island, wiresLeft, startWire);

                    // Remove all the used wires
                    foreach (var wire in island)
                    {
                        wiresLeft.Remove(wire);
                    }

                    if (assignedOriginal == false)
                    {
                        split.Wires = island;
                        assignedOriginal = true;
                    }
                    else
                    {
                        // Create a new subnet for this island

                        var newSubnet = new Subnet(SubnetIDCounter++);
                        LogLogic.AddSubnet(Program.Backend, newSubnet.ID);

                        newSubnet.Wires = island;

                        Console.WriteLine($"Split part of subnet {split} into new subnet: {newSubnet}.");

                        Subnets.Add(newSubnet);
                        newSubnets.Add(newSubnet);
                    }
                }
                
                var unclaimedComponents = new List<(InstanceData, int)>(split.ComponentPorts);

                foreach (var (comp, port) in unclaimedComponents)
                {
                    // FIXME: This stackalloc will probably spiral out of control...
                    Span<Vector2i> portLocs = stackalloc Vector2i[Gates.GetNumberOfPorts(comp)];
                    Gates.GetTransformedPorts(comp, portLocs);
                    var portLoc = portLocs[port];

                    // Check if this component should be transfered to one of the new subnets
                    bool foundHome = false;
                    foreach (var newNet in newSubnets)
                    {
                        foreach (var wire in newNet.Wires)
                        {
                            if (wire.IsConnectionPoint(portLoc))
                            {
                                // Here we should transfer the compoennt to this split off subnet
                                split.RemoveComponent(comp, port);
                                newNet.AddComponent(comp, port);
                                foundHome = true;

                                // Hello Dijkstra :)
                                goto breakContinueWithNextComponent;
                            }
                        }
                    }

                    // If it didn't find a home in any of the new subnets, we check that is still
                    // has its home in the original subnet
                    if (foundHome == false)
                    {
                        foreach (var wire in split.Wires)
                        {
                            if (wire.IsConnectionPoint(portLoc))
                            {
                                foundHome = true;
                            }
                        }

                        if (foundHome == false)
                        {
                            // This component is no longer contained in any of the split subnets
                            split.RemoveComponent(comp, port);
                        }
                    }

                breakContinueWithNextComponent:;
                }
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("---- Subnet status ----");
            foreach (var net in Subnets)
            {
                Console.WriteLine($"\t{net}");
            }
            Console.WriteLine("---- ---- ---- ----");
        }

        // FIXME: Structure this better!
        // This is so that we can get something simulating.
        public void AddComponent(InstanceData data)
        {
            Span<Vector2i> ports = stackalloc Vector2i[Gates.GetNumberOfPorts(data)];
            Gates.GetTransformedPorts(data, ports);

            for (int i = 0; i < ports.Length; i++)
            {
                foreach (var net in Subnets)
                {
                    foreach (var wire in net.Wires)
                    {
                        if (wire.IsConnectionPoint(ports[i]))
                        {
                            net.AddComponent(data, i);
                            Console.WriteLine($"Added component ({data}, port: {i}) to the subnet: {net}.");
                            // We found a bundle so we don't have to look for
                            // any more bundles
                            goto breakBundles;
                        }
                    }
                }
            breakBundles:
                continue;
            }
        }

        // FIXME: Structure this better!
        // This is so that we can get something simulating.
        public void RemoveComponent(InstanceData data)
        {
            int ports = Gates.GetNumberOfPorts(data);
            int count = 0;
            foreach (var net in Subnets)
            {
                for (int i = 0; i < ports; i++)
                {
                    if (net.RemoveComponent(data, i))
                    {
                        Console.WriteLine($"Removed component ({data}, port: {i}) from subnet: {net}");
                        count++;
                    }
                }
            }

            if (count > ports)
                Console.WriteLine($"Warn: Removed component {data} from {count} subnets which is more than the number of ports the component has. {ports}");
        }

        // FIXME: We might want to make this more efficient!
        public static List<Subnet> CreateSubnetsFromWires(List<Wire> wires)
        {
            HashSet<Vector2i> positions = new HashSet<Vector2i>();
            foreach (var w in wires)
            {
                positions.Add(w.Pos);
                positions.Add(w.EndPos);
            }

            UnionFind<Vector2i> nets = new UnionFind<Vector2i>(positions);

            foreach (var w in wires)
            {
                nets.Union(w.Pos, w.EndPos);
            }

            Dictionary<int, Subnet> bundlesDict = new Dictionary<int, Subnet>();
            foreach (var w in wires)
            {
                int root = nets.Find(w.Pos).Index;
                if (bundlesDict.TryGetValue(root, out var bundle) == false)
                {
                    bundle = new Subnet(0);
                    bundlesDict[root] = bundle;
                }

                // Because C# doesn't detect that bundle != null here we do the '!'
                bundle!.AddWire(w);
            }

            var bundles = bundlesDict.Values.ToList();
            //Console.WriteLine($"Created bundles:");
            //foreach (var bundle in bundles)
            //{
            //    Console.WriteLine($"  Bundle:\n    {string.Join("\n    ", bundle.Wires)}\n\n");
            //}

            return bundles;
        }
    }
}
