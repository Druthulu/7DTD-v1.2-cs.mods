using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

public static class XmlExtensions
{
	public static string GetElementString(this XElement _elem)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('<');
		stringBuilder.Append(_elem.Name);
		foreach (XAttribute xattribute in _elem.Attributes())
		{
			stringBuilder.Append(' ');
			stringBuilder.Append(xattribute.Name);
			stringBuilder.Append("=\"");
			stringBuilder.Append(xattribute.Value);
			stringBuilder.Append('"');
		}
		stringBuilder.Append(' ');
		return stringBuilder.ToString();
	}

	public static string GetXPath(this XElement _elem)
	{
		StringBuilder stringBuilder = new StringBuilder();
		XmlExtensions.getXPath(stringBuilder, _elem);
		return stringBuilder.ToString();
	}

	public static string GetXPath(this XAttribute _attr)
	{
		StringBuilder stringBuilder = new StringBuilder();
		XmlExtensions.getXPath(stringBuilder, _attr.Parent);
		stringBuilder.Append("[@");
		stringBuilder.Append(_attr.Name);
		stringBuilder.Append(']');
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void getXPath(StringBuilder _sb, XElement _current)
	{
		if (_current.Parent != null)
		{
			XmlExtensions.getXPath(_sb, _current.Parent);
		}
		_sb.Append('/');
		_sb.Append(_current.Name);
	}

	public static bool HasAttribute(this XElement _element, XName _name)
	{
		return _element.Attribute(_name) != null;
	}

	public static string GetAttribute(this XElement _element, XName _name)
	{
		XAttribute xattribute = _element.Attribute(_name);
		if (xattribute == null)
		{
			return "";
		}
		return xattribute.Value;
	}

	public static bool TryGetAttribute(this XElement _element, XName _name, out string _result)
	{
		XAttribute xattribute = _element.Attribute(_name);
		_result = ((xattribute != null) ? xattribute.Value : null);
		return xattribute != null;
	}

	public static bool ParseAttribute(this XElement _element, XName _name, ref int _result)
	{
		string s;
		if (_element.TryGetAttribute(_name, out s))
		{
			_result = int.Parse(s);
			return true;
		}
		return false;
	}

	public static bool ParseAttribute(this XElement _element, XName _name, ref float _result)
	{
		string input;
		if (_element.TryGetAttribute(_name, out input))
		{
			_result = StringParsers.ParseFloat(input, 0, -1, NumberStyles.Any);
			return true;
		}
		return false;
	}

	public static bool ParseAttribute(this XElement _element, XName _name, ref ulong _result)
	{
		string s;
		if (_element.TryGetAttribute(_name, out s))
		{
			_result = ulong.Parse(s);
			return true;
		}
		return false;
	}

	public static List<XmlNode> ToList(this XmlNodeList _xmlNodeList)
	{
		List<XmlNode> list = new List<XmlNode>(_xmlNodeList.Count);
		foreach (object obj in _xmlNodeList)
		{
			XmlNode item = (XmlNode)obj;
			list.Add(item);
		}
		return list;
	}

	public static void CreateXmlDeclaration(this XmlDocument _doc)
	{
		XmlDeclaration newChild = _doc.CreateXmlDeclaration("1.0", "UTF-8", null);
		_doc.InsertBefore(newChild, _doc.DocumentElement);
	}

	public static XmlElement AddXmlElement(this XmlNode _node, string _name)
	{
		XmlDocument xmlDocument;
		if (_node.NodeType == XmlNodeType.Document)
		{
			xmlDocument = (XmlDocument)_node;
		}
		else
		{
			xmlDocument = _node.OwnerDocument;
		}
		XmlElement xmlElement = xmlDocument.CreateElement(_name);
		_node.AppendChild(xmlElement);
		return xmlElement;
	}

	public static XmlComment AddXmlComment(this XmlNode _node, string _content)
	{
		XmlDocument xmlDocument;
		if (_node.NodeType == XmlNodeType.Document)
		{
			xmlDocument = (XmlDocument)_node;
		}
		else
		{
			xmlDocument = _node.OwnerDocument;
		}
		XmlComment xmlComment = xmlDocument.CreateComment(_content);
		_node.AppendChild(xmlComment);
		return xmlComment;
	}

	public static XmlElement SetAttrib(this XmlElement _element, string _name, string _value)
	{
		_element.SetAttribute(_name, _value);
		return _element;
	}

	public static XmlElement AddXmlKeyValueProperty(this XmlNode _node, string _name, string _value)
	{
		return _node.AddXmlElement("property").SetAttrib("name", _name).SetAttrib("value", _value);
	}

	public static bool TryGetAttribute(this XmlElement _element, string _name, out string _result)
	{
		if (!_element.HasAttribute(_name))
		{
			_result = null;
			return false;
		}
		_result = _element.GetAttribute(_name);
		return true;
	}
}
