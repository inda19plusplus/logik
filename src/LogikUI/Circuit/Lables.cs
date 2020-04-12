using LogikUI.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace LogikUI.Circuit
{
    struct TextLabel
    {
        public Vector2d Position;
        public string Text;
        public double TextSize;

        public TextLabel(Vector2d position, string text, double textSize)
        {
            Position = position;
            Text = text;
            TextSize = textSize;
        }
    }

    class TextLabels
    {
        public TextLabel[] Labels;

        public TextLabels(TextLabel[] labels)
        {
            Labels = labels;
        }

        public void Draw(Cairo.Context cr)
        {
            var pcr = Pango.CairoHelper.CreateContext(cr);
            var layout = new Pango.Layout(pcr);

            // FIXME: Font choice
            Pango.FontDescription fd = Pango.FontDescription.FromString("Arial");
            layout.Wrap = Pango.WrapMode.Word;
            layout.Alignment = Pango.Alignment.Left;
            layout.FontDescription = fd;
            //layout.Attributes = new Pango.AttrList();
            //layout.Attributes.Insert(new Pango.AttrForeground(10, 10, 10));

            foreach (var label in Labels)
            {
                fd.Size = (int)label.TextSize * 1024;
                layout.FontDescription = fd;
                layout.SetText(label.Text);

                cr.MoveTo(label.Position * CircuitEditor.DotSpacing);
                cr.SetSourceRGB(0.0, 0.0, 0.0);
                Pango.CairoHelper.ShowLayout(cr, layout);
            }
        }
    }
}
