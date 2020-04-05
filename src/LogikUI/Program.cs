using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LogikUI
{
    class Program
    {
        [DllImport("Native/logik_simulation", EntryPoint = "test", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Test();

        [DllImport("Native/logik_simulation", EntryPoint = "test2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Test2(int i);

        [DllImport("Native/logik_simulation", EntryPoint = "add", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Add(int a, int b);

        [DllImport("Native/logik_simulation", EntryPoint = "do_cool_stuff", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
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
