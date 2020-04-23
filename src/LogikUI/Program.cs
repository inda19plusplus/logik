using Gtk;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LogikUI.Component;
using LogikUI.Hierarchy;
using LogikUI.Circuit;
using LogikUI.Util;
using System.Globalization;

namespace LogikUI
{
    class Program
    {
        static Toolbar createToolbar() {
            Toolbar toolbar = new Toolbar();
            ToolButton tb_selector = new ToolButton(
                Util.Icon.Selector(), "Selector"
            );
            ToolButton tb_and = new ToolButton(
                Util.Icon.AndGate(), "And Gate"
            );
            ToolButton tb_or = new ToolButton(
                Util.Icon.OrGate(), "Or Gate"
            );
            ToolButton tb_xor = new ToolButton(
                Util.Icon.XorGate(), "Xor Gate"
            );

            SeparatorToolItem sep = new SeparatorToolItem();

            toolbar.Insert(tb_selector, 0);
            toolbar.Insert(sep, 1);
            toolbar.Insert(tb_and, 2);
            toolbar.Insert(tb_or, 3);
            toolbar.Insert(tb_xor, 4);

            return toolbar;
        }

        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            Application.Init();

            Window wnd = new Window("Logik");
            wnd.Resize(1600, 800);

            // FIXME: Move this somewhere else and hook up callbacks
            MenuBar bar = new MenuBar();
            
            // FIXME: On windows there should be no delay on the ability to close the menu when you've opened it.
            // Atm there is a delay after opening the menu and when you can close it...
            MenuItem file = new MenuItem("File");
            file.AddEvents((int)Gdk.EventMask.AllEventsMask);
            bar.Append(file);
            Menu fileMenu = new Menu();
            file.Submenu = fileMenu;
            fileMenu.Append(new MenuItem("Open..."));

            Notebook nbook = new Notebook();
            var circuitEditor = new CircuitEditor();
            nbook.AppendPage(circuitEditor, new Label("Circuit editor"));
            nbook.AppendPage(new Label("TODO: Package editor"), new Label("Package editor"));

            Notebook sideBar = new Notebook();
            var components = new ComponentView(new List<ComponentFolder> { 
                new ComponentFolder("Test folder 1", new List<Component.Component>()
                {
                    new Component.Component("Test comp 1", "x-office-document"),
                    new Component.Component("Test comp 2", "x-office-document"),
                    new Component.Component("Test comp 3", "x-office-document"),
                }),
                new ComponentFolder("Test folder 2", new List<Component.Component>()
                {
                    new Component.Component("Another test comp 1", "x-office-document"),
                    new Component.Component("Another test comp 2", "x-office-document"),
                    new Component.Component("Another test comp 3", "x-office-document"),
                }),
            });
            sideBar.AppendPage(components.TreeView, new Label("Components"));
            var hierarchy = new HierarchyView(new HierarchyComponent("Top comp", "x-office-document", new List<HierarchyComponent>()
            {
                new HierarchyComponent("Test Comp 1", "x-office-document", new List<HierarchyComponent>(){
                    new HierarchyComponent("Test Nested Comp 1", "x-office-document", new List<HierarchyComponent>()),
                }),
                new HierarchyComponent("Test Comp 2", "x-office-document", new List<HierarchyComponent>(){
                    new HierarchyComponent("Test Nested Comp 1", "x-office-document", new List<HierarchyComponent>()),
                    new HierarchyComponent("Test Nested Comp 2", "x-office-document", new List<HierarchyComponent>()),
                }),
                new HierarchyComponent("Test Comp 3", "x-office-document", new List<HierarchyComponent>()),
            }));
            sideBar.AppendPage(hierarchy.TreeView, new Label("Hierarchy"));

            HPaned hPaned = new HPaned();
            hPaned.Pack1(sideBar, false, false);
            hPaned.Pack2(nbook, true, false);

            //Add the label to the form
            VBox box = new VBox(false, 0);
            box.PackStart(bar, false, false, 0);
            box.PackStart(createToolbar(), false, false, 0);
            box.PackEnd(hPaned, true, true, 0);
            box.Expand = true;
            wnd.Add(box);

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
