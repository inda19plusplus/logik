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
        const string Lib = "Native/logik_simulation";
        const CallingConvention CallingConv = CallingConvention.Cdecl;

        [DllImport(Lib, EntryPoint = "init", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern void Init();

        [DllImport(Lib, EntryPoint = "exit", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern void Exit();

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
}
