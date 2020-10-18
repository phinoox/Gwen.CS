using System;
using System.IO;
using System.Text;
using Gwen.Control;
using Gwen.Control.Layout;
using Gwen.Xml;

namespace Gwen.UnitTest
{
	[UnitTest(Category = "Xml", Order = 600)]
	public class XmlWindow : GUnit
	{
		private string m_xml;

		public XmlWindow(ControlBase parent)
            : base(parent)
        {
			HorizontalLayout layout = new HorizontalLayout(this);
			layout.VerticalAlignment = VerticalAlignment.Top;

			Control.Button button1 = new Control.Button(layout);
			button1.Text = "Open a Window (method)";
			button1.Clicked += OpenMethodWindow;

			Control.Button button2 = new Control.Button(layout);
			button2.Text = "Open a Window (interface)";
			button2.Clicked += OpenInterfaceWindow;

			if(File.Exists("assets/XmlUnitTest.xml")){
				m_xml= File.ReadAllText("assets/XmlUnitTest.xml");
			}
		}

		private class MethodTestComponent : Component
		{
			public MethodTestComponent(GUnit unit, string xml)
				: base(unit, new XmlStringSource(xml))
			{
				m_unit = unit;
			}

            public void OnButtonClicked(ControlBase sender, ClickedEventArgs args)
			{
				m_unit.UnitPrint(sender.Name + ": Clicked");
			}

			public void OnItemSelected(ControlBase sender, ItemSelectedEventArgs args)
			{
				m_unit.UnitPrint(sender.Name + ": ItemSelected " + ((MenuItem)args.SelectedItem).Text);
			}

			public void OnSelectionChanged(ControlBase sender, ItemSelectedEventArgs args)
			{
				m_unit.UnitPrint(sender.Name + ": SelectionChanged " + ((LabeledRadioButton)args.SelectedItem).Text);
			}

			public void OnValueChanged(ControlBase sender, EventArgs args)
			{
				float value = 0.0f;
				if (sender is Gwen.Control.NumericUpDown)
					value = ((Gwen.Control.NumericUpDown)sender).Value;
				else if (sender is Gwen.Control.VerticalSlider)
					value = ((Gwen.Control.VerticalSlider)sender).Value;
				else if (sender is Gwen.Control.HorizontalSlider)
					value = ((Gwen.Control.HorizontalSlider)sender).Value;

				m_unit.UnitPrint(sender.Name + ": ValueChanged " + value);
			}

			public void OnTextChanged(ControlBase sender, EventArgs args)
			{
				if (sender is Gwen.Control.MultilineTextBox)
					m_unit.UnitPrint(sender.Name + ": TextChanged " + ((Gwen.Control.MultilineTextBox)sender).Text);
				else if (sender is Gwen.Control.TextBox)
					m_unit.UnitPrint(sender.Name + ": TextChanged " + ((Gwen.Control.TextBox)sender).Text);
			}

			public void OnSubmitPressed(ControlBase sender, EventArgs args)
			{
				if (sender is Gwen.Control.TextBox)
					m_unit.UnitPrint(sender.Name + ": SubmitPressed " + ((Gwen.Control.TextBox)sender).Text);
			}

			public void OnCheckChanged(ControlBase sender, EventArgs args)
			{
				if (sender is Gwen.Control.CheckBox)
					m_unit.UnitPrint(sender.Name + ": CheckChanged " + ((Gwen.Control.CheckBox)sender).IsChecked);
				else if (sender is Gwen.Control.LabeledCheckBox)
					m_unit.UnitPrint(sender.Name + ": CheckChanged " + ((Gwen.Control.LabeledCheckBox)sender).IsChecked);
			}

			public void OnRowSelected(ControlBase sender, ItemSelectedEventArgs args)
			{
				m_unit.UnitPrint(sender.Name + ": RowSelected " + ((ListBoxRow)((ItemSelectedEventArgs)args).SelectedItem).Text);
			}

			public void OnSelected(ControlBase sender, EventArgs args)
			{
				m_unit.UnitPrint(((Gwen.Control.TreeNode)sender).TreeControl.Name + ": Selected " + ((Gwen.Control.TreeNode)sender).Text);
			}

			public void OnClosed(ControlBase sender, EventArgs args)
			{
				m_unit.UnitPrint(sender.Name + ": Closed ");
			}

			private GUnit m_unit;
		}

		private class InterfaceTestComponent : Component
		{
			public InterfaceTestComponent(GUnit unit, string xml)
				: base(unit, new XmlStringSource(xml))
			{
				m_unit = unit;
			}

			public override bool HandleEvent(string eventName, string handlerName, Gwen.Control.ControlBase sender, System.EventArgs args)
			{
				if (handlerName == "OnButtonClicked")
				{
					m_unit.UnitPrint(sender.Name + ": Clicked");
					return true;
                }
				else if (handlerName == "OnItemSelected")
				{
					m_unit.UnitPrint(sender.Name + ": ItemSelected " + ((MenuItem)((ItemSelectedEventArgs)args).SelectedItem).Text);
					return true;
				}
				else if (handlerName == "OnSelectionChanged")
				{
					m_unit.UnitPrint(sender.Name + ": SelectionChanged " + ((LabeledRadioButton)((ItemSelectedEventArgs)args).SelectedItem).Text);
					return true;
				}
				else if (handlerName == "OnValueChanged")
				{
					float value = 0.0f;
					if (sender is Gwen.Control.NumericUpDown)
						value = ((Gwen.Control.NumericUpDown)sender).Value;
					else if (sender is Gwen.Control.VerticalSlider)
						value = ((Gwen.Control.VerticalSlider)sender).Value;
					else if (sender is Gwen.Control.HorizontalSlider)
						value = ((Gwen.Control.HorizontalSlider)sender).Value;

					m_unit.UnitPrint(sender.Name + ": ValueChanged " + value);
					return true;
				}
				else if (handlerName == "OnTextChanged")
				{
					if (sender is Gwen.Control.TextBox)
						m_unit.UnitPrint(sender.Name + ": TextChanged " + ((Gwen.Control.TextBox)sender).Text);
					else if (sender is Gwen.Control.MultilineTextBox)
						m_unit.UnitPrint(sender.Name + ": TextChanged " + ((Gwen.Control.MultilineTextBox)sender).Text);
					return true;
				}
				else if (handlerName == "OnSubmitPressed")
				{
					m_unit.UnitPrint(sender.Name + ": SubmitPressed " + ((Gwen.Control.TextBox)sender).Text);
					return true;
				}
				else if (handlerName == "OnCheckChanged")
				{
					if (sender is Gwen.Control.CheckBox)
						m_unit.UnitPrint(sender.Name + ": CheckChanged " + ((Gwen.Control.CheckBox)sender).IsChecked);
					else if (sender is Gwen.Control.LabeledCheckBox)
						m_unit.UnitPrint(sender.Name + ": CheckChanged " + ((Gwen.Control.LabeledCheckBox)sender).IsChecked);
					return true;
				}
				else if (handlerName == "OnRowSelected")
				{
					m_unit.UnitPrint(sender.Name + ": RowSelected " + ((ListBoxRow)((ItemSelectedEventArgs)args).SelectedItem).Text);
					return true;
				}
				else if (handlerName == "OnSelected")
				{
					m_unit.UnitPrint(((Gwen.Control.TreeNode)sender).TreeControl.Name + ": Selected " + ((Gwen.Control.TreeNode)sender).Text);
					return true;
				}
				else if (handlerName == "OnClosed")
				{
					m_unit.UnitPrint(sender.Name + ": Closed ");
					return true;
				}
				else
				{
					return false;
				}
			}

			private GUnit m_unit;
		}

		void OpenMethodWindow(ControlBase control, EventArgs args)
		{
			new MethodTestComponent(this, m_xml);
		}

		void OpenInterfaceWindow(ControlBase control, EventArgs args)
		{
			new InterfaceTestComponent(this, m_xml);
		}

		
	}
}
