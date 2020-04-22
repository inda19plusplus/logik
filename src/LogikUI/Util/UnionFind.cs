using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace LogikUI.Util
{
    struct UnionFindNode<T>
    {
        public int Index;
        public int Parent;
        public int Rank;
        public int Size;
        public T Value;

        public UnionFindNode(int index, int parent, int rank, int size, T value)
        {
            Index = index;
            Parent = parent;
            Size = size;
            Rank = rank;
            Value = value;
        }
    }

    class UnionFind<T> where T : notnull
    {
        public UnionFindNode<T>[] Nodes;
        public Dictionary<T, int> ValueToIndex = new Dictionary<T, int>();

        public IEnumerable<UnionFindNode<T>> Roots
        {
            get
            {
                for (int i = 0; i < Nodes.Length; i++)
                {
                    if (Nodes[i].Index == Nodes[i].Parent)
                        yield return Nodes[i];
                }
            }
        }

        public UnionFind(IEnumerable<T> values)
        {
            List<UnionFindNode<T>> nodes = new List<UnionFindNode<T>>();
            int index = 0;
            foreach (var val in values)
            {
                nodes.Add(new UnionFindNode<T>(index, index, 1, 0, val));
                ValueToIndex.Add(val, index);
                index++;
            }

            Nodes = nodes.ToArray();
        }

        public ref UnionFindNode<T> Find(T t)
        {
            return ref Find(ref Nodes[ValueToIndex[t]]);
        }

        public ref UnionFindNode<T> Find(ref UnionFindNode<T> x)
        {
            ref var root = ref x;
            while (root.Parent != root.Index)
            {
                root = ref Nodes[root.Parent];
            }

            while (x.Parent != root.Index)
            {
                ref var parent = ref Nodes[x.Parent];
                x.Parent = root.Index;
                x = parent;
            }

            return ref root;
        }

        public void Union(T xv, T yv)
        {
            ref var xRoot = ref Find(ref Nodes[ValueToIndex[xv]]);
            ref var yRoot = ref Find(ref Nodes[ValueToIndex[yv]]);

            if (xRoot.Index == yRoot.Index) return;

            if (xRoot.Rank < yRoot.Rank)
            {
                ref var temp = ref xRoot;
                xRoot = ref yRoot;
                yRoot = ref temp;
            }

            yRoot.Parent = xRoot.Index;
            if (xRoot.Rank == yRoot.Rank)
                xRoot.Rank++;
        }

        public IEnumerable<UnionFindNode<T>> ConnectedComponents(int index)
        {
            var root = Find(ref Nodes[index]);
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (root.Index == Find(ref Nodes[i]).Index)
                    yield return Nodes[i];
            }
        }
    }
}
