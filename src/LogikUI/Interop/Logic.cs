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
    static class LogLogic
    {
        public static void Print(string str)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[Logic] {str}");
            Console.ForegroundColor = c;
        }

        public static Data Init()
        {
            var data = Logic.Init();
            Print($"Init() -> {data}");
            return data;
        }

        public static void Exit(Data data)
        {
            Print($"Exit() -> {data}");
            Logic.Exit(data);
        }

        public static bool AddSubnet(Data data, int subnetId)
        {
            var ret = Logic.AddSubnet(data, subnetId);
            Print($"AddSubnet(Subnet: {subnetId}) -> {ret}");
            return ret;
        }

        public static bool RemoveSubnet(Data data, int subnetId)
        {
            var ret = Logic.RemoveSubnet(data, subnetId);
            Print($"RemoveSubnet(Subnet: {subnetId}) -> {ret}");
            return ret;
        }

        public static int AddComponent(Data data, ComponentType componentType)
        {
            var ret = Logic.AddComponent(data, componentType);
            Print($"AddComponent(Type: {componentType}) -> {ret}");
            return ret;
        }

        public static bool RemoveComponent(Data data, int componentId)
        {
            var ret = Logic.RemoveComponent(data, componentId);
            Print($"RemoveComponent(Component: {componentId}) -> {ret}");
            return ret;
        }

        public static bool Link(Data data, int componentId, int port, int subnetId)
        {
            var ret = Logic.Link(data, componentId, port, subnetId);
            Print($"Link(Component: {componentId}, Port: {port}, Subnet: {subnetId}) -> {ret}");
            return ret;
        }

        public static bool Unlink(Data data, int componentId, int port, int subnetId)
        {
            var ret = Logic.Unlink(data, componentId, port, subnetId);
            Print($"Unlink(Component: {componentId}, Port: {port}, Subnet: {subnetId}) -> {ret}");
            return ret;
        }

        public static void Tick(Data data)
        {
            Print($"Tick()");
            Logic.Tick(data);
        }

        public static ValueState SubnetState(Data data, int subnet)
        {
            var ret = Logic.SubnetState(data, subnet);
            //Print($"SubnetState(Subnet: {subnet}) -> {ret}");
            return ret;
        }

        public static ValueState PortState(Data data, int component, int port)
        {
            var ret = Logic.PortState(data, component, port);
            //Print($"SubnetState(Component: {component}, Port: {port}) -> {ret}");
            return ret;
        }

        public static ValueState PressComponent(Data data, int componentId)
        {
            var ret = Logic.PressComponent(data, componentId);
            Print($"PressComponent(Component: {componentId}) -> {ret}");
            return ret;
        }

        public static ValueState ReleaseComponent(Data data, int componentId)
        {
            var ret = Logic.ReleaseComponent(data, componentId);
            Print($"ReleaseComponent(Component: {componentId}) -> {ret}");
            return ret;
        }
    }

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
        public static extern bool Link(Data data, int componentId, int port, int subnetId);
        
        [DllImport(Lib, EntryPoint = "unlink", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern bool Unlink(Data data, int componentId, int port, int subnetId);
        
        [DllImport(Lib, EntryPoint = "tick", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern void Tick(Data data);

        [DllImport(Lib, EntryPoint = "subnet_state", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern ValueState SubnetState(Data data, int subnet);

        [DllImport(Lib, EntryPoint = "port_state", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern ValueState PortState(Data data, int component, int port);

        [DllImport(Lib, EntryPoint = "press_component", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern ValueState PressComponent(Data data, int componentId);

        [DllImport(Lib, EntryPoint = "release_component", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern ValueState ReleaseComponent(Data data, int componentId);
    }

    public struct Data
    {
        public IntPtr Handle;
    }
}
