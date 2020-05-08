using LogikUI.Simulation.Gates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using LogikUI.Simulation;

namespace LogikUI.Interop
{
    static class Logic
    {
        const string Lib = "logik_simulation";

        const CallingConvention CallingConv = CallingConvention.Cdecl;

        [DllImport(Lib, EntryPoint = "init", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern Data Init();

        [DllImport(Lib, EntryPoint = "exit", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern void Exit(Data data);
        
        [DllImport(Lib, EntryPoint = "add_subnet", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern bool AddSubnet(Data data, int subnetId);
        
        [DllImport(Lib, EntryPoint = "remove_subnet", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern bool RemoveSubnet(Data data, int subnetId);
        
        [DllImport(Lib, EntryPoint = "add_component", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern int AddComponent(Data data, ComponentType componentType);
        
        [DllImport(Lib, EntryPoint = "remove_component", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern bool RemoveComponent(Data data, int componentId);
        
        [DllImport(Lib, EntryPoint = "link", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern void Link(Data data, int componentId, int port, int subnetId);
        
        [DllImport(Lib, EntryPoint = "unlink", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern void Unlink(Data data, int componentId, int port, int subnetId);
        
        [DllImport(Lib, EntryPoint = "tick", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern void Tick(Data data);
        
        [DllImport(Lib, EntryPoint = "dirty_subnet", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern void DirtySubnet(Data data, int subnet);
        
        [DllImport(Lib, EntryPoint = "subnet_state", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern ValueState SubnetState(Data data, int subnet);

        [DllImport(Lib, EntryPoint = "port_state", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern ValueState PortState(Data data, int component, UIntPtr port);
    }

    public struct Data
    {
        public IntPtr Handle;
    }
}
