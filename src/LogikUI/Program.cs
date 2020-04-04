using System;
using System.Runtime.InteropServices;

namespace LogikUI
{
    class Program
    {
        [DllImport("Native/logik_simulation", EntryPoint = "main2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Main2(int i);

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World from C#!");
            Main2(1337);
        }
    }
}
