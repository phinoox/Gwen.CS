using System;
using System.IO;
using System.Text;
using Gwen.Control;
using Gwen.Control.Layout;
using Gwen.Xml;

namespace Gwen.UnitTest
{
    [UnitTest(Category = "Xml", Order = 602)]
    public class XsdWriting : GUnit
    {
        MultilineTextBox xsdContent;
        public XsdWriting(ControlBase parent)
                    : base(parent)
        {
            Control.Layout.GridLayout layout = new Control.Layout.GridLayout(this);
            layout.VerticalAlignment = VerticalAlignment.Top;
            layout.VerticalAlignment = VerticalAlignment.Stretch;

			layout.SetColumnWidths(Control.Layout.GridLayout.Fill);
			layout.SetRowHeights(30.0f, Control.Layout.GridLayout.Fill);
            
            Control.Button button1 = new Control.Button(layout);
            button1.Text = "Write Xsd File ";
            button1.Clicked += WriteXsd;
            button1.Width=100;
            button1.HorizontalAlignment=HorizontalAlignment.Left;
            xsdContent  = new Control.MultilineTextBox(layout);
            
        }

        private void WriteXsd(ControlBase sender, ClickedEventArgs arguments)
        {
            xsdContent.Text=XsdWriter.WriteXsd();
        }
    }
}