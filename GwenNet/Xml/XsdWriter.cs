using System;
using System.Collections.Generic;
using System.IO;

using System.Globalization;
using System.Reflection;
using System.Linq;
using System.Text;

using System.Xml.Linq;

namespace Gwen.Xml
{
    public class XsdWriter
    {
        private static Dictionary<string, ElementDef> m_ElementHandlers = new Dictionary<string, ElementDef>();

        private static Dictionary<Type, AttributeValueConverter> m_AttributeValueConverters = new Dictionary<Type, AttributeValueConverter>();
        private static Dictionary<Type, EventHandlerConverter> m_EventHandlerConverters = new Dictionary<Type, EventHandlerConverter>();

        private static XDocument m_comments;


        static XsdWriter()
        {

            Assembly assembly = typeof(Gwen.Control.ControlBase).Assembly;
            if (assembly != null)
            {
                ScanControls(assembly);


            }
        }

        public static string WriteXsd()
        {

            if (File.Exists("Gwen.xml"))
            {
                m_comments = XDocument.Load("Gwen.xml");
            }
            XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", null));

            XNamespace xs = "http://www.w3.org/2001/XMLSchema";
            XElement schema = new XElement(xs + "schema",
            new XAttribute(XNamespace.Xmlns + "xs", "http://www.w3.org/2001/XMLSchema"));
            doc.Add(schema);

            schema.Add(new XElement(xs+"simpleType",new XAttribute("name","text"),
                       new XElement(xs+"restriction",new XAttribute("base","xs:string"))
                       ));

            XElement cgroup = new XElement(xs + "group");
            cgroup.SetAttributeValue("name", "controls");
            XElement choice = new XElement(xs + "choice");
            cgroup.Add(choice);
            schema.Add(cgroup);
            foreach (KeyValuePair<string, ElementDef> element in m_ElementHandlers)
            {
                XElement ele = new XElement(xs + "element");
                ele.SetAttributeValue("name", element.Key);
                ele.SetAttributeValue("ref", element.Key);
                choice.Add(ele);
                schema.Add(CreateXElementForType(element.Value));
            }

            System.Xml.XmlTextWriter xwriter = new System.Xml.XmlTextWriter("Gwen.xsd", Encoding.UTF8);
            xwriter.Formatting = System.Xml.Formatting.Indented;
            doc.WriteTo(xwriter);
            xwriter.Flush();
            xwriter.Close();
            return File.ReadAllText("Gwen.xsd");
        }

        private static XElement CreateXElementForType(ElementDef definition)
        {
            XNamespace xs = "http://www.w3.org/2001/XMLSchema";
            XElement xele = new XElement(xs + "element", new XAttribute("name", definition.Type.Name));
            string fqan = definition.Type.FullName;
            XElement membersNode = m_comments.Root.Element("members");
            List<XElement> comments = membersNode.Elements("member").Where<XElement>(x => x.Attribute("name").Value.Contains(fqan)).ToList<XElement>();
            if (comments.Count != 0)
            {
                XElement comment = comments.SingleOrDefault<XElement>(x => x.Attribute("name").Value == $"T:{fqan}");
                if (comment != null)
                    xele.Add(new XElement(xs + "annotation", new XElement(xs + "documentation", comment.Element("summary").Value)));
            }

            XElement content;
            if (definition.Type.Name == "ControlBase")
            {
                content = new XElement(xs + "complextType", new XAttribute("mixed", "true"));
                xele.Add(content);

                content.Add(
                        new XElement(xs + "choice", new XAttribute("minOccurs", "0"), new XAttribute("maxOccurs", "unbounded"),
                        new XElement(xs + "group", new XAttribute("ref", "controls")),
                        new XElement(xs + "any", new XAttribute("maxOccurs", "unbounded"), new XAttribute("processContents", "lax"))
                ));
            }
            else
            {
                content = new XElement(xs + "extension", new XAttribute("base", "ControlBase"));
                xele.Add(new XElement(xs + "complextType", new XAttribute("mixed", "true"), new XElement(xs + "complexContent", content)));
            }


            foreach (KeyValuePair<string, MemberInfo> info in definition.Attributes)
            {
                if (definition.Type.Name != "ControlBase" && info.Value.DeclaringType == typeof(Gwen.Control.ControlBase))
                    continue;
                XElement xmi = new XElement(xs + "attribute",
                                            new XAttribute("name", info.Key),
                                            new XAttribute("type", "xs:text"));

                //assuming we only register events and properties
                string typeHint = info.Value.MemberType == MemberTypes.Event ? "E" : "P";
                XElement miComment = comments.SingleOrDefault<XElement>(x => x.Attribute("name").Value == $"{typeHint}:{fqan}.{info.Key}");
                if (miComment != null)
                {
                    string mt = info.Value.MemberType.ToString();
                    string xmlComment = miComment.Element("summary").Value;
                    string typeName= info.Value.ToString().Split(' ')[0];
                    List<XElement> ctorComments = membersNode.Elements("member").Where<XElement>(x => x.Attribute("name").Value.Contains($"M:{typeName}.#ctor")).ToList<XElement>();

                    string ctorDesc = "";
                    foreach(XElement ctorComment in ctorComments){
                        ctorDesc+=ctorComment.Value+",";
                    } 
                    xmi.Add(new XElement(xs + "annotation", new XElement(xs + "documentation",$"{mt} {typeName}, Description: {xmlComment} ,{ctorDesc} ")));
                }
                content.Add(xmi);
            }
            return xele;
        }

        /// <summary>
        /// Register a XML element. All XML elements must be registered before usage. 
        /// </summary>
        /// <param name="name">Name of the element.</param>
        /// <param name="type">Type of the control or component.</param>
        /// <param name="handler">Handler function for creating the control or component.</param>
        /// <returns>True if registered successfully or false is already registered.</returns>
        public static bool RegisterElement(string name, Type type, ElementHandler handler)
        {
            if (!m_ElementHandlers.ContainsKey(name))
            {
                ElementDef elementDef = new ElementDef(type, handler);

                m_ElementHandlers[name] = elementDef;

                ScanProperties(elementDef);
                ScanEvents(elementDef);

                return true;
            }

            return false;
        }

        /// <summary>
		/// Scan an assembly to find all controls that can be created using XML.
		/// </summary>
		/// <param name="assembly">Assembly.</param>
		public static void ScanControls(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes().Where(t => t.IsDefined(typeof(XmlControlAttribute), false)))
            {
                object[] attribs = type.GetCustomAttributes(typeof(XmlControlAttribute), false);
                if (attribs.Length > 0)
                {
                    XmlControlAttribute attrib = attribs[0] as XmlControlAttribute;
                    if (attrib != null)
                    {
                        ElementHandler handler;
                        if (attrib.CustomHandler != null)
                        {
                            MethodInfo mi = type.GetMethod(attrib.CustomHandler, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                            if (mi != null)
                            {
                                handler = Delegate.CreateDelegate(typeof(ElementHandler), mi) as ElementHandler;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            handler = DefaultElementHandler;
                        }

                        string name = attrib.ElementName != null ? attrib.ElementName : type.Name;

                        RegisterElement(name, type, handler);
                    }
                }
            }
        }

        private static Gwen.Control.ControlBase DefaultElementHandler(Parser parser, Type type, Gwen.Control.ControlBase parent)
        {
            Gwen.Control.ControlBase element = Activator.CreateInstance(type, parent) as Gwen.Control.ControlBase;

            parser.ParseAttributes(element);
            if (parser.MoveToContent())
            {
                parser.ParseContainerContent(element);
            }

            return element;
        }
        private static void ScanProperties(ElementDef elementDef)
        {
            foreach (var propertyInfo in elementDef.Type.GetProperties().Where(pi => pi.IsDefined(typeof(XmlPropertyAttribute), false)))
            {
                elementDef.AddAttribute(propertyInfo.Name, propertyInfo);
            }
        }

        private static void ScanEvents(ElementDef elementDef)
        {
            foreach (var eventInfo in elementDef.Type.GetEvents().Where(ei => ei.IsDefined(typeof(XmlEventAttribute), false)))
            {
                elementDef.AddAttribute(eventInfo.Name, eventInfo);
            }
        }

    }
}