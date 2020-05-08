using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.IO;
using Gtk;
using LogikUI.Util;
using LogikUI.Circuit;
using LogikUI.Simulation.Gates;

namespace LogikUI.File
{
    /// <summary>
    /// Handles writing and parsing of project files.
    /// </summary>
    static class FileManager
    {
        private static string filename;

        /// <summary>
        /// True, if FileManager.Open or FileManager.Save hasn't been called.
        /// </summary>
        public static bool IsNew { get; private set; } = true;
        public static List<Wire> Wires { get; set; } = new List<Wire>();
        public static List<IInstance> Components { get; set; } = new List<IInstance>();
        public static List<TextLabel> Labels { get; set; } = new List<TextLabel>();

        private static void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                Output.WriteWarning(args.Message);
            else
                throw args.Exception;
        }

        /// <summary>
        /// Parses a project file. The results are saved in FileManager.Wires, FileManager.Components, and FileManager.Labels.
        /// </summary>
        /// <param name="filename">The path of the project file.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidProjectDataException">The XML-file doesn't match with the schema.</exception>
        /// <exception cref="ArgumentException">Unable to access the file.</exception>
        public static void Load(string filename)
        {
            Vector2i getPos(XmlNode pos)
            {
                int x = int.Parse(pos.Attributes["x"].InnerText);
                int y = int.Parse(pos.Attributes["y"].InnerText);

                return new Vector2i(x, y);
            }

            XmlDocument doc = new XmlDocument();
            XmlReader? reader;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

            try
            {
                reader = XmlReader.Create(filename, settings);

                doc.Load(reader);

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
                            default/* case "not" */: ins = default(NotGateInstance); break;
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

                IsNew = false;
                FileManager.filename = filename;
            }
            catch (Exception e)
            {
                if (e is XmlException || e is XPathException || e is InvalidProjectDataException || e is XmlSchemaException)
                {
                    MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, "Logik is unable to read file. Please choose a valid project file.");
                    md.Run();
                    md.Dispose();

                    throw new ArgumentException("Not a project file.", e);
                }
                else
                {
                    MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Ok, "Logik is unable to access choosen file.");
                    md.Run();
                    md.Dispose();

                    throw new ArgumentException("Unable to access file.", e);
                }
            }

            if (reader != null) reader.Close();
        }

        /// <summary>
        /// Writes the project to an earlier specified file.
        /// </summary>
        /// <exception cref="InvalidOperationException">No target file has been specified.</exception>
        public static void Save()
        {
            if (IsNew) throw new InvalidOperationException("The file is new.");

            //FIXME: delete file before save
            
            Save(filename);

        }

        // <summary>
        // Writes the project to a specified file.
        // </summary>
        // <param name="filename">The path of the new project file.</param>
        public static void Save(string filename)
        {
           
            IsNew = false;
            FileManager.filename = filename;

            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.NewLineOnAttributes = true;

                XmlWriter xmlWriter = XmlWriter.Create(filename, settings);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("circuit");

                //wires
                xmlWriter.WriteStartElement("wires");
                foreach (var wire in Wires)
                {

                    xmlWriter.WriteStartElement("wire");

                    //from
                    xmlWriter.WriteStartElement("from");
                    xmlWriter.WriteAttributeString("x", wire.Pos.X.ToString());
                    xmlWriter.WriteAttributeString("y", wire.Pos.Y.ToString());
                    xmlWriter.WriteEndElement();

                    //to
                    xmlWriter.WriteStartElement("to");
                    xmlWriter.WriteAttributeString("x", wire.EndPos.X.ToString());
                    xmlWriter.WriteAttributeString("y", wire.EndPos.Y.ToString());
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();

                }
               
                xmlWriter.WriteEndElement();
                //end wires

                //components
                xmlWriter.WriteStartElement("components");

                foreach (var component in Components)
                {
                    xmlWriter.WriteStartElement("component");
                    
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();

                //end components

                //labels
                xmlWriter.WriteStartElement("labels");

                foreach (var text_label in Labels)
                {
                    xmlWriter.WriteStartElement("label");
                    xmlWriter.WriteAttributeString("size", text_label.TextSize.ToString());

                    //text
                    xmlWriter.WriteStartElement("text");
                    xmlWriter.WriteString(text_label.Text);
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                }

                
                xmlWriter.WriteEndElement();
                //end labels


                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
            }
            catch (Exception e)
            {
                //e is XmlException || e is XPathException || e is InvalidProjectDataException || e is XmlSchemaException
                if (e is XmlException || e is InvalidOperationException)
                {
                    MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, "Not Able To Save Data To File");
                    md.Run();
                    md.Dispose();

                    throw new ArgumentException("Failed to parse to XMl", e);
                } else
                {
                    MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, "Not Able To Save Data To File");
                    md.Run();
                    md.Dispose();

                    throw new ArgumentException("Failed to save to filename", e);
                }
            }

        }
    }
}
