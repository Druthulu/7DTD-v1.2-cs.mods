using System;
using System.Collections.Generic;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item
{
	public class PreviewData : IXMLData
	{
		public DataItem<int> Zoom
		{
			get
			{
				return this.pZoom;
			}
			set
			{
				this.pZoom = value;
			}
		}

		public DataItem<Vector2> Pos
		{
			get
			{
				return this.pPos;
			}
			set
			{
				this.pPos = value;
			}
		}

		public DataItem<Vector3> Rot
		{
			get
			{
				return this.pRot;
			}
			set
			{
				this.pRot = value;
			}
		}

		public List<IDataItem> GetDisplayValues(bool _recursive = true)
		{
			return new List<IDataItem>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pZoom;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<Vector2> pPos;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<Vector3> pRot;

		public static class Parser
		{
			public static PreviewData Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
			{
				string text = _elem.HasAttribute("class") ? _elem.GetAttribute("class") : "PreviewData";
				Type type = Type.GetType(typeof(PreviewData.Parser).Namespace + "." + text);
				if (type == null)
				{
					type = Type.GetType(text);
					if (type == null)
					{
						throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
					}
				}
				PreviewData previewData = (PreviewData)Activator.CreateInstance(type);
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				foreach (object obj in _elem.ChildNodes)
				{
					XmlNode xmlNode = (XmlNode)obj;
					XmlNodeType nodeType = xmlNode.NodeType;
					if (nodeType != XmlNodeType.Element)
					{
						if (nodeType != XmlNodeType.Comment)
						{
							throw new UnexpectedElementException("Unknown node \"" + xmlNode.NodeType.ToString() + "\" found while parsing Preview", ((IXmlLineInfo)xmlNode).LineNumber);
						}
					}
					else
					{
						PositionXmlElement positionXmlElement = (PositionXmlElement)xmlNode;
						if (!PreviewData.Parser.knownAttributesMultiplicity.ContainsKey(positionXmlElement.Name))
						{
							throw new UnexpectedElementException("Unknown element \"" + xmlNode.Name + "\" found while parsing Preview", ((IXmlLineInfo)xmlNode).LineNumber);
						}
						string name = positionXmlElement.Name;
						if (!(name == "Zoom"))
						{
							if (!(name == "Pos"))
							{
								if (name == "Rot")
								{
									Vector3 startValue;
									try
									{
										startValue = Vector3Parser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException);
									}
									DataItem<Vector3> pRot = new DataItem<Vector3>("Rot", startValue);
									previewData.pRot = pRot;
								}
							}
							else
							{
								Vector2 startValue2;
								try
								{
									startValue2 = Vector2Parser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception innerException2)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, innerException2);
								}
								DataItem<Vector2> pPos = new DataItem<Vector2>("Pos", startValue2);
								previewData.pPos = pPos;
							}
						}
						else
						{
							int startValue3;
							try
							{
								startValue3 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
							}
							catch (Exception innerException3)
							{
								throw new InvalidValueException(string.Concat(new string[]
								{
									"Could not parse attribute \"",
									positionXmlElement.Name,
									"\" value \"",
									ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
									"\""
								}), positionXmlElement.LineNumber, innerException3);
							}
							DataItem<int> pZoom = new DataItem<int>("Zoom", startValue3);
							previewData.pZoom = pZoom;
						}
						if (!dictionary.ContainsKey(positionXmlElement.Name))
						{
							dictionary[positionXmlElement.Name] = 0;
						}
						Dictionary<string, int> dictionary2 = dictionary;
						name = positionXmlElement.Name;
						int num = dictionary2[name];
						dictionary2[name] = num + 1;
					}
				}
				if (!dictionary.ContainsKey("Zoom"))
				{
					int startValue4;
					try
					{
						startValue4 = intParser.Parse("0");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"0\" for attribute \"Zoom\" could not be parsed", -1);
					}
					DataItem<int> pZoom2 = new DataItem<int>("Zoom", startValue4);
					previewData.pZoom = pZoom2;
					dictionary["Zoom"] = 1;
				}
				if (!dictionary.ContainsKey("Pos"))
				{
					Vector2 startValue5;
					try
					{
						startValue5 = Vector2Parser.Parse("0,0");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"0,0\" for attribute \"Pos\" could not be parsed", -1);
					}
					DataItem<Vector2> pPos2 = new DataItem<Vector2>("Pos", startValue5);
					previewData.pPos = pPos2;
					dictionary["Pos"] = 1;
				}
				if (!dictionary.ContainsKey("Rot"))
				{
					Vector3 startValue6;
					try
					{
						startValue6 = Vector3Parser.Parse("0,0,0");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"0,0,0\" for attribute \"Rot\" could not be parsed", -1);
					}
					DataItem<Vector3> pRot2 = new DataItem<Vector3>("Rot", startValue6);
					previewData.pRot = pRot2;
					dictionary["Rot"] = 1;
				}
				foreach (KeyValuePair<string, Range<int>> keyValuePair in PreviewData.Parser.knownAttributesMultiplicity)
				{
					int num2 = dictionary.ContainsKey(keyValuePair.Key) ? dictionary[keyValuePair.Key] : 0;
					if ((keyValuePair.Value.hasMin && num2 < keyValuePair.Value.min) || (keyValuePair.Value.hasMax && num2 > keyValuePair.Value.max))
					{
						throw new IncorrectAttributeOccurrenceException(string.Concat(new string[]
						{
							"Element has incorrect number of \"",
							keyValuePair.Key,
							"\" attribute instances, found ",
							num2.ToString(),
							", expected ",
							keyValuePair.Value.ToString()
						}), _elem.LineNumber);
					}
				}
				return previewData;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
			{
				{
					"Zoom",
					new Range<int>(true, 1, true, 1)
				},
				{
					"Pos",
					new Range<int>(true, 1, true, 1)
				},
				{
					"Rot",
					new Range<int>(true, 1, true, 1)
				}
			};
		}
	}
}
