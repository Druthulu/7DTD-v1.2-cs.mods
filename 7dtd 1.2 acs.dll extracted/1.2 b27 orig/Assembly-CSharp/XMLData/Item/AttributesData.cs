using System;
using System.Collections.Generic;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine.Scripting;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item
{
	[Preserve]
	public class AttributesData : IXMLData
	{
		public DataItem<string> EntityDamage
		{
			get
			{
				return this.pEntityDamage;
			}
			set
			{
				this.pEntityDamage = value;
			}
		}

		public DataItem<string> BlockDamage
		{
			get
			{
				return this.pBlockDamage;
			}
			set
			{
				this.pBlockDamage = value;
			}
		}

		public DataItem<string> Accuracy
		{
			get
			{
				return this.pAccuracy;
			}
			set
			{
				this.pAccuracy = value;
			}
		}

		public DataItem<string> FalloffRange
		{
			get
			{
				return this.pFalloffRange;
			}
			set
			{
				this.pFalloffRange = value;
			}
		}

		public DataItem<string> GainHealth
		{
			get
			{
				return this.pGainHealth;
			}
			set
			{
				this.pGainHealth = value;
			}
		}

		public DataItem<string> GainFood
		{
			get
			{
				return this.pGainFood;
			}
			set
			{
				this.pGainFood = value;
			}
		}

		public DataItem<string> GainWater
		{
			get
			{
				return this.pGainWater;
			}
			set
			{
				this.pGainWater = value;
			}
		}

		public DataItem<string> DegradationRate
		{
			get
			{
				return this.pDegradationRate;
			}
			set
			{
				this.pDegradationRate = value;
			}
		}

		public List<IDataItem> GetDisplayValues(bool _recursive = true)
		{
			return new List<IDataItem>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pEntityDamage;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pBlockDamage;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pAccuracy;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pFalloffRange;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pGainHealth;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pGainFood;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pGainWater;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pDegradationRate;

		public static class Parser
		{
			public static AttributesData Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
			{
				string text = _elem.HasAttribute("class") ? _elem.GetAttribute("class") : "AttributesData";
				Type type = Type.GetType(typeof(AttributesData.Parser).Namespace + "." + text);
				if (type == null)
				{
					type = Type.GetType(text);
					if (type == null)
					{
						throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
					}
				}
				AttributesData attributesData = (AttributesData)Activator.CreateInstance(type);
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				foreach (object obj in _elem.ChildNodes)
				{
					XmlNode xmlNode = (XmlNode)obj;
					XmlNodeType nodeType = xmlNode.NodeType;
					if (nodeType != XmlNodeType.Element)
					{
						if (nodeType != XmlNodeType.Comment)
						{
							throw new UnexpectedElementException("Unknown node \"" + xmlNode.NodeType.ToString() + "\" found while parsing Attributes", ((IXmlLineInfo)xmlNode).LineNumber);
						}
					}
					else
					{
						PositionXmlElement positionXmlElement = (PositionXmlElement)xmlNode;
						if (!AttributesData.Parser.knownAttributesMultiplicity.ContainsKey(positionXmlElement.Name))
						{
							throw new UnexpectedElementException("Unknown element \"" + xmlNode.Name + "\" found while parsing Attributes", ((IXmlLineInfo)xmlNode).LineNumber);
						}
						string name = positionXmlElement.Name;
						uint num = <PrivateImplementationDetails>.ComputeStringHash(name);
						if (num <= 2055669289U)
						{
							if (num <= 711561591U)
							{
								if (num != 535561810U)
								{
									if (num == 711561591U)
									{
										if (name == "EntityDamage")
										{
											string startValue;
											try
											{
												startValue = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
											DataItem<string> pEntityDamage = new DataItem<string>("EntityDamage", startValue);
											attributesData.pEntityDamage = pEntityDamage;
										}
									}
								}
								else if (name == "FalloffRange")
								{
									string startValue2;
									try
									{
										startValue2 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
									DataItem<string> pFalloffRange = new DataItem<string>("FalloffRange", startValue2);
									attributesData.pFalloffRange = pFalloffRange;
								}
							}
							else if (num != 1827041476U)
							{
								if (num == 2055669289U)
								{
									if (name == "DegradationRate")
									{
										string startValue3;
										try
										{
											startValue3 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
										DataItem<string> pDegradationRate = new DataItem<string>("DegradationRate", startValue3);
										attributesData.pDegradationRate = pDegradationRate;
									}
								}
							}
							else if (name == "Accuracy")
							{
								string startValue4;
								try
								{
									startValue4 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception innerException4)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, innerException4);
								}
								DataItem<string> pAccuracy = new DataItem<string>("Accuracy", startValue4);
								attributesData.pAccuracy = pAccuracy;
							}
						}
						else if (num <= 2614118511U)
						{
							if (num != 2391273097U)
							{
								if (num == 2614118511U)
								{
									if (name == "BlockDamage")
									{
										string startValue5;
										try
										{
											startValue5 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException5)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException5);
										}
										DataItem<string> pBlockDamage = new DataItem<string>("BlockDamage", startValue5);
										attributesData.pBlockDamage = pBlockDamage;
									}
								}
							}
							else if (name == "GainWater")
							{
								string startValue6;
								try
								{
									startValue6 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception innerException6)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, innerException6);
								}
								DataItem<string> pGainWater = new DataItem<string>("GainWater", startValue6);
								attributesData.pGainWater = pGainWater;
							}
						}
						else if (num != 3036802414U)
						{
							if (num == 3448204316U)
							{
								if (name == "GainHealth")
								{
									string startValue7;
									try
									{
										startValue7 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException7)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException7);
									}
									DataItem<string> pGainHealth = new DataItem<string>("GainHealth", startValue7);
									attributesData.pGainHealth = pGainHealth;
								}
							}
						}
						else if (name == "GainFood")
						{
							string startValue8;
							try
							{
								startValue8 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
							}
							catch (Exception innerException8)
							{
								throw new InvalidValueException(string.Concat(new string[]
								{
									"Could not parse attribute \"",
									positionXmlElement.Name,
									"\" value \"",
									ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
									"\""
								}), positionXmlElement.LineNumber, innerException8);
							}
							DataItem<string> pGainFood = new DataItem<string>("GainFood", startValue8);
							attributesData.pGainFood = pGainFood;
						}
						if (!dictionary.ContainsKey(positionXmlElement.Name))
						{
							dictionary[positionXmlElement.Name] = 0;
						}
						Dictionary<string, int> dictionary2 = dictionary;
						name = positionXmlElement.Name;
						int num2 = dictionary2[name];
						dictionary2[name] = num2 + 1;
					}
				}
				foreach (KeyValuePair<string, Range<int>> keyValuePair in AttributesData.Parser.knownAttributesMultiplicity)
				{
					int num3 = dictionary.ContainsKey(keyValuePair.Key) ? dictionary[keyValuePair.Key] : 0;
					if ((keyValuePair.Value.hasMin && num3 < keyValuePair.Value.min) || (keyValuePair.Value.hasMax && num3 > keyValuePair.Value.max))
					{
						throw new IncorrectAttributeOccurrenceException(string.Concat(new string[]
						{
							"Element has incorrect number of \"",
							keyValuePair.Key,
							"\" attribute instances, found ",
							num3.ToString(),
							", expected ",
							keyValuePair.Value.ToString()
						}), _elem.LineNumber);
					}
				}
				return attributesData;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
			{
				{
					"EntityDamage",
					new Range<int>(true, 0, true, 1)
				},
				{
					"BlockDamage",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Accuracy",
					new Range<int>(true, 0, true, 1)
				},
				{
					"FalloffRange",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainHealth",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainFood",
					new Range<int>(true, 0, true, 1)
				},
				{
					"GainWater",
					new Range<int>(true, 0, true, 1)
				},
				{
					"DegradationRate",
					new Range<int>(true, 0, true, 1)
				}
			};
		}
	}
}
