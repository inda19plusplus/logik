using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace LogikUI.Interop
{
    static class Logic
    {
#if WINDOWS
        const string Lib = "Native/logik_simulation";
#else
        const string Lib = "Native/liblogik_simulation";
#endif
        const CallingConvention CallingConv = CallingConvention.Cdecl;

        [DllImport(Lib, EntryPoint = "init", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern unsafe Data* Init();

        [DllImport(Lib, EntryPoint = "exit", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern unsafe void Exit(Data* data);
        
        [DllImport(Lib, EntryPoint = "add_subnet", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern unsafe bool AddSubnet(Data* data, UIntPtr id);
        
        [DllImport(Lib, EntryPoint = "remove_subnet", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern unsafe bool RemoveSubnet(Data* data, UIntPtr id);
        
        [DllImport(Lib, EntryPoint = "add_component", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern unsafe UIntPtr AddComponent(Data* data, UIntPtr component);
        
        [DllImport(Lib, EntryPoint = "remove_component", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern unsafe bool RemoveComponent(Data* data, UIntPtr id);
        
        [DllImport(Lib, EntryPoint = "link", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern unsafe void Link(Data* data, UIntPtr component, UIntPtr port, UIntPtr subnet, bool direction);
        
        [DllImport(Lib, EntryPoint = "unlink", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern unsafe void Unlink(Data* data, UIntPtr component, UIntPtr port, UIntPtr subnet);

        // --------------------------
        // ---- Interop examples ----
        // --------------------------

        [DllImport(Lib, EntryPoint = "test", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern void Test();

        [DllImport(Lib, EntryPoint = "test2", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern void Test2(int i);

        [DllImport(Lib, EntryPoint = "add", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern int Add(int a, int b);

        [DllImport(Lib, EntryPoint = "do_cool_stuff", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern void DoCoolStuff(ref CoolStruct stuff);

        [StructLayout(LayoutKind.Sequential)]
        public struct CoolStruct
        {
            public int ID;
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public string ThisIsAnInterestingThing;
        }
    }

    public struct Data
    {
        
    }
}
