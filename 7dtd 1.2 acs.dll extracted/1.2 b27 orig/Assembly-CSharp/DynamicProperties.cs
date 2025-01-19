using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using UnityEngine;

[MemoryPackable(GenerateType.Object)]
public class DynamicProperties : IMemoryPackable<DynamicProperties>, IMemoryPackFormatterRegister
{
	public DictionarySave<string, string> ParseKeyData(string key)
	{
		try
		{
			string data;
			if (this.Data.TryGetValue(key, out data))
			{
				return DynamicProperties.ParseData(data);
			}
		}
		catch (Exception ex)
		{
			Log.Error("ParseKeyData error parsing key {0}, {1}", new object[]
			{
				key,
				ex
			});
		}
		return null;
	}

	public static DictionarySave<string, string> ParseData(string data)
	{
		DictionarySave<string, string> dictionarySave = null;
		try
		{
			dictionarySave = new DictionarySave<string, string>();
			string[] array = data.Split(DynamicProperties.semicolonSeparator, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(DynamicProperties.equalSeparator, StringSplitOptions.RemoveEmptyEntries);
				if (array2.Length >= 2)
				{
					dictionarySave[array2[0]] = array2[1];
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("ParseData error parsing {0}, {1}", new object[]
			{
				data,
				ex
			});
		}
		return dictionarySave;
	}

	public bool Load(string _directory, string _name, bool _addClassesToMain = true)
	{
		try
		{
			foreach (XElement propertyNode in new XmlFile(_directory, _name, false, false).XmlDoc.Root.Elements(XNames.property))
			{
				this.Add(propertyNode, _addClassesToMain);
			}
			return true;
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		return false;
	}

	public bool Save(string _rootNodeName, Stream stream)
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlDeclaration newChild = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
			xmlDocument.InsertBefore(newChild, xmlDocument.DocumentElement);
			XmlNode parent = xmlDocument.AppendChild(xmlDocument.CreateElement(_rootNodeName));
			this.toXml(xmlDocument, parent);
			xmlDocument.Save(stream);
			return true;
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		return false;
	}

	public bool Save(string _rootNodeName, string _path, string _name)
	{
		try
		{
			using (Stream stream = SdFile.Create(Path.Join(_path, _name + ".xml")))
			{
				return this.Save(_rootNodeName, stream);
			}
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toXml(XmlDocument _doc, XmlNode _parent)
	{
		foreach (KeyValuePair<string, string> keyValuePair in this.Values.Dict)
		{
			XmlElement xmlElement = _doc.CreateElement("property");
			XmlAttribute xmlAttribute = _doc.CreateAttribute("name");
			xmlAttribute.Value = keyValuePair.Key;
			xmlElement.Attributes.Append(xmlAttribute);
			XmlAttribute xmlAttribute2 = _doc.CreateAttribute("value");
			xmlAttribute2.Value = this.Values[keyValuePair.Key];
			xmlElement.Attributes.Append(xmlAttribute2);
			if (this.Params1.ContainsKey(keyValuePair.Key))
			{
				XmlAttribute xmlAttribute3 = _doc.CreateAttribute("param1");
				xmlAttribute3.Value = this.Params1[keyValuePair.Key];
				xmlElement.Attributes.Append(xmlAttribute3);
			}
			if (this.Data.ContainsKey(keyValuePair.Key))
			{
				XmlAttribute xmlAttribute4 = _doc.CreateAttribute("fields");
				xmlAttribute4.Value = this.Data[keyValuePair.Key];
				xmlElement.Attributes.Append(xmlAttribute4);
			}
			_parent.AppendChild(xmlElement);
		}
		if (this.Classes.Count > 0)
		{
			foreach (KeyValuePair<string, DynamicProperties> keyValuePair2 in this.Classes.Dict)
			{
				XmlElement xmlElement2 = _doc.CreateElement("property");
				XmlAttribute xmlAttribute5 = _doc.CreateAttribute("class");
				xmlAttribute5.Value = keyValuePair2.Key;
				xmlElement2.Attributes.Append(xmlAttribute5);
				keyValuePair2.Value.toXml(_doc, xmlElement2);
				_parent.AppendChild(xmlElement2);
			}
		}
	}

	public void Add(XElement _propertyNode, bool _addClassesToMain = true)
	{
		this.Parse(null, _propertyNode, this, _addClassesToMain);
	}

	public void Parse(string _className, XElement elementProperty, DynamicProperties _mainProperties, bool _addClassesToMain)
	{
		if (elementProperty.HasAttribute(XNames.class_))
		{
			string attribute = elementProperty.GetAttribute(XNames.class_);
			string text = (_className != null) ? (_className + "." + attribute) : attribute;
			DynamicProperties dynamicProperties;
			if (!this.Classes.TryGetValue(attribute, out dynamicProperties))
			{
				dynamicProperties = new DynamicProperties();
				this.Classes.Add(attribute, dynamicProperties);
				if (_addClassesToMain && _mainProperties != this)
				{
					_mainProperties.Classes[text] = dynamicProperties;
				}
			}
			using (IEnumerator<XElement> enumerator = elementProperty.Elements().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					XElement elementProperty2 = enumerator.Current;
					dynamicProperties.Parse(text, elementProperty2, _mainProperties, _addClassesToMain);
				}
				return;
			}
		}
		string attribute2 = elementProperty.GetAttribute(XNames.name);
		if (attribute2.Length == 0)
		{
			throw new Exception("Attribute 'name' missing on property");
		}
		string attribute3 = elementProperty.GetAttribute(XNames.value);
		string text2 = null;
		if (elementProperty.HasAttribute(XNames.param1))
		{
			text2 = elementProperty.GetAttribute(XNames.param1);
		}
		string text3 = null;
		if (elementProperty.HasAttribute(XNames.param2))
		{
			text3 = elementProperty.GetAttribute(XNames.param2);
		}
		string text4 = (_className != null) ? (_className + "." + attribute2) : attribute2;
		if (attribute3 != null && text2 != null)
		{
			this.Params1[attribute2] = text2;
			if (_addClassesToMain && _mainProperties != this)
			{
				_mainProperties.Params1[text4] = text2;
			}
		}
		if (attribute3 != null && text3 != null)
		{
			this.Params2[attribute2] = text3;
			if (_addClassesToMain && _mainProperties != this)
			{
				_mainProperties.Params2[text4] = text3;
			}
		}
		if (attribute3 != null)
		{
			string attribute4 = elementProperty.GetAttribute(XNames.data);
			if (attribute4.Length > 0)
			{
				this.Data[attribute2] = attribute4;
				if (_addClassesToMain && _mainProperties != this)
				{
					_mainProperties.Data[text4] = attribute4;
				}
			}
		}
		this.Values[attribute2] = attribute3;
		if (_addClassesToMain && _mainProperties != this)
		{
			_mainProperties.Values[text4] = attribute3;
		}
		if (elementProperty.HasAttribute(XNames.display) && StringParsers.ParseBool(elementProperty.GetAttribute(XNames.display), 0, -1, true))
		{
			this.Display.Add(attribute2);
			if (_addClassesToMain && _mainProperties != this)
			{
				_mainProperties.Display.Add(text4);
			}
		}
	}

	public void Clear()
	{
		this.Values.Clear();
		this.Params1.Clear();
		this.Params2.Clear();
	}

	public bool Contains(string _propName)
	{
		return this.Values.ContainsKey(_propName);
	}

	public bool Contains(string _className, string _propName)
	{
		DynamicProperties dynamicProperties;
		return this.Classes.TryGetValue(_className, out dynamicProperties) && dynamicProperties.Contains(_propName);
	}

	public bool GetBool(string _propName)
	{
		bool result;
		StringParsers.TryParseBool(this.Values[_propName], out result, 0, -1, true);
		return result;
	}

	public float GetFloat(string _propName)
	{
		float result;
		StringParsers.TryParseFloat(this.Values[_propName], out result, 0, -1, NumberStyles.Any);
		return result;
	}

	public int GetInt(string _propName)
	{
		int result;
		int.TryParse(this.Values[_propName], out result);
		return result;
	}

	public string GetStringValue(string _propName)
	{
		return this.Values[_propName].ToString();
	}

	public string GetString(string _propName)
	{
		string result;
		if (this.Values.TryGetValue(_propName, out result))
		{
			return result;
		}
		return string.Empty;
	}

	public string GetString(string _className, string _propName)
	{
		DynamicProperties dynamicProperties;
		if (this.Classes.TryGetValue(_className, out dynamicProperties))
		{
			return dynamicProperties.GetString(_propName);
		}
		return string.Empty;
	}

	public void SetValue(string _propName, string _value)
	{
		if (this.Values.ContainsKey(_propName))
		{
			this.Values[_propName] = _value;
			return;
		}
		this.Values.Add(_propName, _value);
	}

	public void SetValue(string _className, string _propName, string _value)
	{
		DynamicProperties dynamicProperties;
		if (!this.Classes.TryGetValue(_className, out dynamicProperties))
		{
			dynamicProperties = new DynamicProperties();
			this.Classes[_className] = dynamicProperties;
		}
		dynamicProperties.SetValue(_propName, _value);
		string propName = string.Format("{0}.{1}", _className, _propName);
		this.SetValue(propName, _value);
	}

	public void SetParam1(string _propName, string _param1)
	{
		if (!this.Values.ContainsKey(_propName))
		{
			this.Values.Add(_propName, null);
		}
		if (this.Params1.ContainsKey(_propName))
		{
			this.Params1[_propName] = _param1;
			return;
		}
		this.Params1.Add(_propName, _param1);
	}

	public void ParseString(string _propName, ref string optionalValue)
	{
		string text;
		if (this.Values.TryGetValue(_propName, out text))
		{
			optionalValue = text;
		}
	}

	public string GetLocalizedString(string _propName)
	{
		string key;
		if (this.Values.TryGetValue(_propName, out key))
		{
			return Localization.Get(key, false);
		}
		return string.Empty;
	}

	public void ParseLocalizedString(string _propName, ref string optionalValue)
	{
		string key;
		if (this.Values.TryGetValue(_propName, out key))
		{
			optionalValue = Localization.Get(key, false);
		}
	}

	public void ParseBool(string _propName, ref bool optionalValue)
	{
		string text;
		if (this.Values.TryGetValue(_propName, out text))
		{
			bool flag;
			if (StringParsers.TryParseBool(text, out flag, 0, -1, true))
			{
				optionalValue = flag;
				return;
			}
			Log.Warning("Can't parse bool {0} '{1}'", new object[]
			{
				_propName,
				text
			});
		}
	}

	public void ParseColorHex(string _propName, ref Color optionalValue)
	{
		string input;
		if (this.Values.TryGetValue(_propName, out input))
		{
			optionalValue = StringParsers.ParseHexColor(input);
		}
	}

	public void ParseEnum<T>(string _propName, ref T optionalValue) where T : struct, IConvertible
	{
		string name;
		T t;
		if (this.Values.TryGetValue(_propName, out name) && EnumUtils.TryParse<T>(name, out t, true))
		{
			optionalValue = t;
		}
	}

	public void ParseFloat(string _propName, ref float optionalValue)
	{
		string text;
		if (this.Values.TryGetValue(_propName, out text))
		{
			float num;
			if (StringParsers.TryParseFloat(text, out num, 0, -1, NumberStyles.Any))
			{
				optionalValue = num;
				return;
			}
			Log.Warning("Can't parse float {0} '{1}'", new object[]
			{
				_propName,
				text
			});
		}
	}

	public static void ParseFloat(XElement _e, string _propName, ref float optionalValue)
	{
		XAttribute xattribute = _e.Attribute(_propName);
		if (xattribute != null)
		{
			string value = xattribute.Value;
			float num;
			if (StringParsers.TryParseFloat(value, out num, 0, -1, NumberStyles.Any))
			{
				optionalValue = num;
				return;
			}
			Log.Warning("Can't parse float {0} '{1}'", new object[]
			{
				_propName,
				value
			});
		}
	}

	public void ParseInt(string _propName, ref int optionalValue)
	{
		string text;
		if (this.Values.TryGetValue(_propName, out text))
		{
			int num;
			if (StringParsers.TryParseSInt32(text, out num, 0, -1, NumberStyles.Integer))
			{
				optionalValue = num;
				return;
			}
			Log.Warning("Can't parse int {0} '{1}'", new object[]
			{
				_propName,
				text
			});
		}
	}

	public void ParseVec(string _propName, ref Vector2 optionalValue)
	{
		string input;
		if (this.Values.TryGetValue(_propName, out input))
		{
			optionalValue = StringParsers.ParseVector2(input);
		}
	}

	public void ParseVec(string _propName, ref Vector3 optionalValue)
	{
		string input;
		if (this.Values.TryGetValue(_propName, out input))
		{
			optionalValue = StringParsers.ParseVector3(input, 0, -1);
		}
	}

	public void ParseVec(string _propName, ref Vector3i optionalValue)
	{
		string input;
		if (this.Values.TryGetValue(_propName, out input))
		{
			optionalValue = StringParsers.ParseVector3i(input, 0, -1, false);
		}
	}

	public void ParseVec(string _propName, ref float optionalValue1, ref float optionalValue2)
	{
		string input;
		if (this.Values.TryGetValue(_propName, out input))
		{
			Vector2 vector = StringParsers.ParseVector2(input);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
		}
	}

	public void ParseVec(string _propName, ref float optionalValue1, ref float optionalValue2, ref float optionalValue3)
	{
		string input;
		if (this.Values.TryGetValue(_propName, out input))
		{
			Vector3 vector = StringParsers.ParseVector3(input, 0, -1);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
			optionalValue3 = vector.z;
		}
	}

	public void ParseVec(string _propName, ref float optionalValue1, ref float optionalValue2, ref float optionalValue3, ref float optionalValue4)
	{
		string input;
		if (this.Values.TryGetValue(_propName, out input))
		{
			Vector4 vector = StringParsers.ParseVector4(input);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
			optionalValue3 = vector.z;
			optionalValue4 = vector.w;
		}
	}

	public void CopyFrom(DynamicProperties _other, HashSet<string> _exclude = null)
	{
		DynamicProperties.copyDict(_other.Values, this.Values, _exclude);
		DynamicProperties.copyDict(_other.Params1, this.Params1, _exclude);
		DynamicProperties.copyDict(_other.Params2, this.Params2, _exclude);
		DynamicProperties.copyDict(_other.Data, this.Data, _exclude);
		foreach (string text in _other.Display)
		{
			if (DynamicProperties.copyKey(text, _exclude))
			{
				this.Display.Add(text);
			}
		}
		foreach (KeyValuePair<string, DynamicProperties> keyValuePair in _other.Classes.Dict)
		{
			if (DynamicProperties.copyKey(keyValuePair.Key, _exclude))
			{
				DynamicProperties dynamicProperties = this.Classes.ContainsKey(keyValuePair.Key) ? this.Classes[keyValuePair.Key] : new DynamicProperties();
				this.Classes[keyValuePair.Key] = dynamicProperties;
				dynamicProperties.CopyFrom(keyValuePair.Value, null);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void copyDict(DictionarySave<string, string> _source, DictionarySave<string, string> _dest, HashSet<string> _exclude)
	{
		foreach (KeyValuePair<string, string> keyValuePair in _source.Dict)
		{
			if (DynamicProperties.copyKey(keyValuePair.Key, _exclude))
			{
				_dest[keyValuePair.Key] = keyValuePair.Value;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool copyKey(string _key, HashSet<string> _exclude)
	{
		if (_exclude == null)
		{
			return true;
		}
		if (_exclude.Contains(_key))
		{
			return false;
		}
		if (_key.IndexOf('.') <= 0)
		{
			return true;
		}
		foreach (string str in _exclude)
		{
			if (_key.StartsWith(str + "."))
			{
				return false;
			}
		}
		return true;
	}

	public string PrettyPrint()
	{
		StringBuilder stringBuilder = new StringBuilder();
		this.PrettyPrint(stringBuilder, "");
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrettyPrint(StringBuilder sb, string indent)
	{
		sb.AppendFormat("{0}Properties:\n", indent);
		foreach (KeyValuePair<string, string> keyValuePair in from kvp in this.Values.Dict
		orderby kvp.Key
		select kvp)
		{
			sb.AppendFormat("{2}    name={0}, value={1}", keyValuePair.Key, this.Values[keyValuePair.Key], indent);
			if (this.Params1.ContainsKey(keyValuePair.Key))
			{
				sb.AppendFormat(", param1={0}", this.Params1[keyValuePair.Key]);
			}
			if (this.Data.ContainsKey(keyValuePair.Key))
			{
				sb.AppendFormat(", fields={0}", this.Params1[keyValuePair.Key]);
			}
			sb.AppendLine();
		}
		if (this.Classes.Count > 0)
		{
			sb.AppendFormat("{0}Classes:\n", indent);
			foreach (KeyValuePair<string, DynamicProperties> keyValuePair2 in this.Classes.Dict)
			{
				sb.AppendFormat("{1}    class={0}\n", keyValuePair2.Key, indent);
				keyValuePair2.Value.PrettyPrint(sb, string.Format("{0}    ", indent));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static DynamicProperties()
	{
		DynamicProperties.RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DynamicProperties>())
		{
			MemoryPackFormatterProvider.Register<DynamicProperties>(new DynamicProperties.DynamicPropertiesFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DynamicProperties[]>())
		{
			MemoryPackFormatterProvider.Register<DynamicProperties[]>(new ArrayFormatter<DynamicProperties>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<HashSet<string>>())
		{
			MemoryPackFormatterProvider.Register<HashSet<string>>(new HashSetFormatter<string>());
		}
	}

	[NullableContext(2)]
	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DynamicProperties value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(6);
		writer.WritePackable<DictionarySave<string, string>>(value.Params1);
		writer.WritePackable<DictionarySave<string, string>>(value.Params2);
		writer.WritePackable<DictionarySave<string, string>>(value.Data);
		writer.WriteValue<HashSet<string>>(value.Display);
		writer.WritePackable<DictionarySave<string, DynamicProperties>>(value.Classes);
		writer.WritePackable<DictionarySave<string, string>>(value.Values);
	}

	[NullableContext(2)]
	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DynamicProperties value)
	{
		byte b;
		if (!reader.TryReadObjectHeader(out b))
		{
			value = null;
			return;
		}
		DictionarySave<string, string> @params;
		DictionarySave<string, string> params2;
		DictionarySave<string, string> data;
		HashSet<string> display;
		DictionarySave<string, DynamicProperties> classes;
		DictionarySave<string, string> values;
		if (b == 6)
		{
			if (value == null)
			{
				@params = reader.ReadPackable<DictionarySave<string, string>>();
				params2 = reader.ReadPackable<DictionarySave<string, string>>();
				data = reader.ReadPackable<DictionarySave<string, string>>();
				display = reader.ReadValue<HashSet<string>>();
				classes = reader.ReadPackable<DictionarySave<string, DynamicProperties>>();
				values = reader.ReadPackable<DictionarySave<string, string>>();
				goto IL_194;
			}
			@params = value.Params1;
			params2 = value.Params2;
			data = value.Data;
			display = value.Display;
			classes = value.Classes;
			values = value.Values;
			reader.ReadPackable<DictionarySave<string, string>>(ref @params);
			reader.ReadPackable<DictionarySave<string, string>>(ref params2);
			reader.ReadPackable<DictionarySave<string, string>>(ref data);
			reader.ReadValue<HashSet<string>>(ref display);
			reader.ReadPackable<DictionarySave<string, DynamicProperties>>(ref classes);
			reader.ReadPackable<DictionarySave<string, string>>(ref values);
		}
		else
		{
			if (b > 6)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DynamicProperties), 6, b);
				return;
			}
			if (value == null)
			{
				@params = null;
				params2 = null;
				data = null;
				display = null;
				classes = null;
				values = null;
			}
			else
			{
				@params = value.Params1;
				params2 = value.Params2;
				data = value.Data;
				display = value.Display;
				classes = value.Classes;
				values = value.Values;
			}
			if (b != 0)
			{
				reader.ReadPackable<DictionarySave<string, string>>(ref @params);
				if (b != 1)
				{
					reader.ReadPackable<DictionarySave<string, string>>(ref params2);
					if (b != 2)
					{
						reader.ReadPackable<DictionarySave<string, string>>(ref data);
						if (b != 3)
						{
							reader.ReadValue<HashSet<string>>(ref display);
							if (b != 4)
							{
								reader.ReadPackable<DictionarySave<string, DynamicProperties>>(ref classes);
								if (b != 5)
								{
									reader.ReadPackable<DictionarySave<string, string>>(ref values);
								}
							}
						}
					}
				}
			}
			if (value == null)
			{
				goto IL_194;
			}
		}
		value.Params1 = @params;
		value.Params2 = params2;
		value.Data = data;
		value.Display = display;
		value.Classes = classes;
		value.Values = values;
		return;
		IL_194:
		value = new DynamicProperties
		{
			Params1 = @params,
			Params2 = params2,
			Data = data,
			Display = display,
			Classes = classes,
			Values = values
		};
	}

	public DictionarySave<string, string> Params1 = new DictionarySave<string, string>();

	public DictionarySave<string, string> Params2 = new DictionarySave<string, string>();

	public DictionarySave<string, string> Data = new DictionarySave<string, string>();

	public HashSet<string> Display = new HashSet<string>();

	public DictionarySave<string, DynamicProperties> Classes = new DictionarySave<string, DynamicProperties>();

	public DictionarySave<string, string> Values = new DictionarySave<string, string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] semicolonSeparator = new char[]
	{
		';'
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] equalSeparator = new char[]
	{
		'='
	};

	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class DynamicPropertiesFormatter : MemoryPackFormatter<DynamicProperties>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DynamicProperties value)
		{
			DynamicProperties.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DynamicProperties value)
		{
			DynamicProperties.Deserialize(ref reader, ref value);
		}
	}
}
