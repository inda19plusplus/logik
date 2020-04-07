using Gtk;
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
            Application.Init();

            Window wnd = new Window("Logik");
            wnd.Resize(1600, 800);
            
            HPaned hPaned = new HPaned();

            Notebook nbook = new Notebook();
            nbook.AppendPage(new Label("TODO: Circut editor"), new Label("Circut editor"));
            nbook.AppendPage(new Label("TODO: Package editor"), new Label("Package editor"));

            Notebook sideBar = new Notebook();
            var components = new ComponentView();
            sideBar.AppendPage(components.TreeView, new Label("Components"));
            sideBar.AppendPage(new Label("TODO: Hierarchy view"), new Label("Hierarchy"));

            hPaned.Pack1(sideBar, false, false);
            hPaned.Pack2(nbook, true, false);

            //Add the label to the form
            wnd.Add(hPaned);

            wnd.Destroyed += Wnd_Destroyed;

            wnd.ShowAll();
            Application.Run();
        }

        private static void Wnd_Destroyed(object? sender, EventArgs e)
        {
            Application.Quit();
        }
    }
}
