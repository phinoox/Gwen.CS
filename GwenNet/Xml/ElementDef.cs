using System;
using System.Collections.Generic;
using System.Reflection;

namespace Gwen.Xml{
public class ElementDef
		{
			public Type Type { get; set; }
			public ElementHandler Handler { get; set; }

			internal Dictionary<string, MemberInfo> Attributes { get { return m_Attributes; } }

			public ElementDef(Type type, ElementHandler handler)
			{
				Type = type;
				Handler = handler;
			}

			public void AddAttribute(string name, MemberInfo memberInfo)
			{
				m_Attributes[name] = memberInfo;
			}

			public MemberInfo GetAttribute(string name)
			{
				MemberInfo mi;
				if (m_Attributes.TryGetValue(name, out mi))
					return mi;
				else
					return null;
			}

			private Dictionary<string, MemberInfo> m_Attributes = new Dictionary<string, MemberInfo>();
		}
}