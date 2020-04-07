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

            wnd.Destroyed += Wnd_Destroyed;
            
            HPaned vPaned = new HPaned();

            //Create a label and put some text in it.
            Label myLabel = new Label("Hello World!!!!");

            Notebook nbook = new Notebook();
            nbook.AppendPage(myLabel, new Label("Circut editor"));
            nbook.AppendPage(new Label("Some new content"), new Label("Package editor"));

            Notebook sideBar = new Notebook();
            var components = new ComponentView();
            sideBar.AppendPage(components.TreeView, new Label("Components"));
            sideBar.AppendPage(new Label("TODO: Hierarchy view"), new Label("Hierarchy"));

            vPaned.Pack1(sideBar, true, true);
            vPaned.Pack2(nbook, true, true);

            //Add the label to the form
            wnd.Add(vPaned);

            wnd.ShowAll();

            Application.Run();
        }

        private static void Wnd_Destroyed(object? sender, EventArgs e)
        {
            Application.Quit();
        }
    }
}
