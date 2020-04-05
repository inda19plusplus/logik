using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable enable

namespace LogikUI
{
    class Program
    {
        const string Lib = "Native/logik_simulation";
        const CallingConvention CallingConv = CallingConvention.Cdecl;

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

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World from C#!");

            Test2(1337);
            Test();
            
            Console.WriteLine($"Number added in rust: {Add(14, 8)}");

            CoolStruct stuff;
            stuff.ID = 5;
            stuff.ThisIsAnInterestingThing = "CoolName";
            DoCoolStuff(ref stuff);
        }
    }
}
