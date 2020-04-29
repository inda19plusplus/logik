using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using Gtk;
using LogikUI.Util;
using LogikUI.Circuit;
using LogikUI.Simulation.Gates;

namespace LogikUI.File
{
    class FileManager
    {
        public List<Wire> Wires { get; private set; } = new List<Wire>();
        public List<IInstance> Components { get; private set; } = new List<IInstance>();
        public List<TextLabel> Labels { get; private set; } = new List<TextLabel>(); 

        public FileManager(string fileName)
        {
            Vector2i getPos(XmlNode pos)
            {
                try
                {
                    int x = int.Parse(pos.Attributes["x"].InnerText);
                    int y = int.Parse(pos.Attributes["y"].InnerText);

                    return new Vector2i(x, y);
                }
                catch
                {
                    throw new InvalidProjectDataException("Failed to parse location parameters. Make sure to follow the schema.");
                }
            }

            XmlDocument doc = new XmlDocument();

            try
            {
                doc.Load(fileName);

                /* STRUCTURE:
                 *  circuit
                 *    wires
                 *      wire
                 *        from < -x: int, y: int
                 *        to < -x: int, y: int
                 *      ...
                 *    components
                 *      component
                 *        type
                 *          "and" | "dFlipFlop" | "not"
                 *        location < -x: int, y: int
                 *        orientation
                 *          "north" | "east" | "south" | "west"...
                 *    labels
                 *      label < -size: int(10, 100)
                 *        location < -x: int, y: int
                 *        text
                 *          <string>...
                 */

                #region Parse Wires

                foreach (var _wire in doc.SelectNodes("/circuit/wires"))
                {
                    if (_wire != null)
                    {
                        XmlNode wire = ((XmlNode)_wire).FirstChild;

                        Vector2i start = getPos(wire.SelectSingleNode("from"));
                        Vector2i end = getPos(wire.SelectSingleNode("to"));

                        if (start.X == end.X)
                        {
                            int length = Math.Abs(Math.Abs(end.Y) - start.Y);

                            Wires.Add(new Wire(start, length, Direction.Horizontal));
                        }
                        else if (start.Y == end.Y)
                        {
                            int length = Math.Abs(Math.Abs(end.X) - start.X);

                            Wires.Add(new Wire(start, length, Direction.Vertical));
                        }
                        else throw new InvalidProjectDataException($"Start ({ start.X }, { start.Y }) and end ({ end.X }, { end.Y }) of wire #{ Wires.Count + 1 } has to be on the same axis.");

                        // FIXME: Diagonal wire support
                    }
                }

                #endregion

                #region Parse Components

                foreach (var _component in doc.SelectNodes("/circuit/components"))
                {
                    if (_component != null)
                    {
                        XmlNode component = ((XmlNode)_component).FirstChild;

                        IInstance ins;
                        switch (component.SelectSingleNode("type").InnerText)
                        {
                            case "and": ins = default(AndGateInstance); break;
                            case "dFlipFlop": ins = default(DFlipFlopInstance); break;
                            case "not": ins = default(NotGateInstance); break;
                            default: throw new InvalidProjectDataException($"Invalid type given for component #{ Components.Count + 1 }");
                        }

                        Vector2i location = getPos(component.SelectSingleNode("location"));

                        Circuit.Orientation orientation = Circuit.Orientation.North;
                        switch (component.SelectSingleNode("orientation").InnerText)
                        {
                            case "south": orientation = Circuit.Orientation.South; break;
                            case "east": orientation = Circuit.Orientation.East; break;
                            case "west": orientation = Circuit.Orientation.West; break;
                        }

                        Components.Add(ins.Create(location, orientation));

                        // FIXME: Update list of gates.
                    }
                }

                #endregion

                #region Parse Labels

                foreach (var _label in doc.SelectNodes("/circuit/labels"))
                {
                    if (_label != null)
                    {
                        XmlNode label = ((XmlNode)_label).FirstChild;

                        int size = int.Parse(label.Attributes["size"].InnerText);

                        Vector2i location = getPos(label.SelectSingleNode("location"));

                        string text = label.SelectSingleNode("text").InnerText;

                        Labels.Add(new TextLabel(location, text, size));
                    }
                }

                #endregion

                // FIXME: Save wires, components, and labels in higher level objects
            }
            catch (Exception e)
            {
                if (e is XmlException || e is XPathException || e is InvalidProjectDataException)
                {
                    MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, "Logik is unable to read file. Please choose a valid project file.");
                    md.Run();
                    md.Dispose();

                    throw new InvalidOperationException("Not a project file.", e);
                }
                else
                {
                    MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Ok, "Logik is unable to access choosen file.");
                    md.Run();
                    md.Dispose();

                    throw new InvalidOperationException("Unable to access file.", e);
                }
            }
        }
    }
}
