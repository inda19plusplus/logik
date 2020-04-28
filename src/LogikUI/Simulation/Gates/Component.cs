using Cairo;
using LogikUI.Circuit;
using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace LogikUI.Simulation.Gates
{
    interface IInstance
    {
        public string Name { get; }
        public Vector2i Position { get; }
        public Orientation Orientation { get; }
        public int NumberOfPorts { get; }

        public IInstance Create(Vector2i pos, Orientation orientation);

        // This is the ports in the 
        public void GetPorts(Span<Vector2i> ports);
    }

    class InstanceList
    {
        public Type InstanceType;
        public int TypeSize;
        public Memory<byte> Memory;
        public int Size;
        public int Count;

        public static InstanceList Create<T>(int initSize) where T : struct, IInstance
        {
            return new InstanceList(typeof(T), Unsafe.SizeOf<T>(), initSize);
        }

        private InstanceList(Type type, int typeSize, int initSize)
        {
            InstanceType = type;
            TypeSize = typeSize;
            Memory = new byte[typeSize * initSize];
            Size = initSize;
            Count = 0;
        }

        public Span<T> GetWhole<T>() where T : struct, IInstance
        {
            return MemoryMarshal.Cast<byte, T>(Memory.Span);
        }

        public Span<T> Get<T>() where T : struct, IInstance
        {
            return MemoryMarshal.Cast<byte, T>(Memory.Span).Slice(0, Count);
        }

        public ref T Get<T>(int index) where T : struct, IInstance 
            => ref Get<T>()[index];

        public void EnsureCapacity(int capacity)
        {
            if (capacity >= Size)
            {
                int newSize = Math.Max(capacity, Size + Size / 2);

                // Resize
                Memory<byte> newMem = new byte[TypeSize * newSize];
                Memory.CopyTo(newMem);
                Memory = newMem;
                Size = newSize;
            }
        }

        public ref T Add<T>() where T : struct, IInstance
        {
            EnsureCapacity(Count + 1);

            return ref Get<T>(Count++);
        }

        public void Add<T>(T instance) where T : struct, IInstance
        {
            EnsureCapacity(Count + 1);

            Span<T> instanceList = GetWhole<T>();
            instanceList[Count++] = instance;
        }

        public bool Remove<T>(T instance) where T : struct, IInstance, IEquatable<T>
        {
            Span<T> list = Get<T>();
            int index = list.IndexOf(instance);
            if (index < 0) return false;
            list[index] = list[Count - 1];
            Count--;
            return true;
        }
    }

    delegate void DrawComponentsFunc<T>(Cairo.Context cr, Span<T> instances) where T : struct, IInstance;
    
    class Components
    {
        public delegate void DrawComponentsFuncInternal(Cairo.Context cr, Components components);
        public delegate void DrawSingleComponentsFuncInternal(Cairo.Context cr, IInstance component);
        public delegate void AddComponentFuncInternal(InstanceList list, IInstance instance);
        public delegate bool RemoveComponentFuncInternal(InstanceList list, IInstance instance);

        public Dictionary<Type, InstanceList> ComponentsDict = new Dictionary<Type, InstanceList>();
        public List<DrawComponentsFuncInternal> DrawFuncs = new List<DrawComponentsFuncInternal>();
        public Dictionary<Type, DrawSingleComponentsFuncInternal> DrawSingleFuncs = new Dictionary<Type, DrawSingleComponentsFuncInternal>();
        public Dictionary<Type, AddComponentFuncInternal> AddFuncDict = new Dictionary<Type, AddComponentFuncInternal>();
        public Dictionary<Type, RemoveComponentFuncInternal> RemoveFuncDict = new Dictionary<Type, RemoveComponentFuncInternal>();

        public void RegisterComponentType<TInst>(DrawComponentsFunc<TInst> drawFunc)
            where TInst : unmanaged, IInstance, IEquatable<TInst>
        {
            // FIXME: Better error
            if (ComponentsDict.ContainsKey(typeof(TInst)))
                throw new Exception($"Instance type already registered! (type: {typeof(TInst)})");

            InstanceList list = InstanceList.Create<TInst>(16);
            ComponentsDict.Add(typeof(TInst), list);
            DrawFuncs.Add((cr, comps) => drawFunc(cr, comps.GetInstances<TInst>()));
            DrawSingleFuncs.Add(typeof(TInst), (cr, instance) => drawFunc(cr, stackalloc TInst[1] { (TInst)instance }));
            AddFuncDict.Add(typeof(TInst), (list, inst) => list.Add((TInst)inst));
            RemoveFuncDict.Add(typeof(TInst), (list, inst) => list.Remove((TInst)inst));
        }

        public ref TInst CreateInstanceType<TInst>()
            where TInst : struct, IInstance
        {
            // FIXME!!
            var key = typeof(TInst);
            var list = ComponentsDict[key];
            return ref list.Add<TInst>();
        }

        public Span<TInst> GetInstances<TInst>()
            where TInst : struct, IInstance
        {
            var list = ComponentsDict[typeof(TInst)];
            return list.Get<TInst>();
        }

        public void AddComponent(IInstance instance)
        {
            Type type = instance.GetType();
            if (AddFuncDict.TryGetValue(type, out var addFunc) == false)
                throw new InvalidOperationException($"Trying to add an instance of an unregistered type: {type}!");

            if (ComponentsDict.TryGetValue(type, out var list) == false)
                throw new InvalidOperationException($"Trying to add an instance of an unregistered type: {type}!");

            // Because the compiler doesn't do proper null checking here we 
            // use ! to say that the parameter definetly isn't null here.
            addFunc!(list!, instance);
        }

        public bool RemoveComponent(IInstance instance)
        {
            Type type = instance.GetType();
            if (RemoveFuncDict.TryGetValue(type, out var removeFunc) == false)
                throw new InvalidOperationException($"Trying to add an instance of an unregistered type: {type}!");

            if (ComponentsDict.TryGetValue(type, out var list) == false)
                throw new InvalidOperationException($"Trying to add an instance of an unregistered type: {type}!");

            // Because the compiler doesn't do proper null checking here we 
            // use ! to say that the parameter definetly isn't null here.
            return removeFunc!(list!, instance);
        }

        public void DrawComponents(Cairo.Context cr)
        {
            foreach (var drawFunc in DrawFuncs)
            {
                drawFunc(cr, this);
            }
        }

        public void Draw(Context cr, IInstance instace)
        {
            if (DrawSingleFuncs.TryGetValue(instace.GetType(), out var drawSingleFunc) == false)
                throw new KeyNotFoundException($"{instace.GetType()} isn't a registered instance type!");
            drawSingleFunc!(cr, instace);
        }

        public void Draw<TInst>(Context cr, TInst instace)
            where TInst : struct, IInstance
        {
            if (DrawSingleFuncs.TryGetValue(instace.GetType(), out var drawSingleFunc) == false)
                throw new KeyNotFoundException($"{instace.GetType()} isn't a registered instance type!");
            drawSingleFunc!(cr, instace);
        }
    }
}
