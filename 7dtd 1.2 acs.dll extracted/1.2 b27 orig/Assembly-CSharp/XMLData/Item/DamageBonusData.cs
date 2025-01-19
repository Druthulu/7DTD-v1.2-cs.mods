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
	public class DamageBonusData : IXMLData
	{
		public DataItem<float> Head
		{
			get
			{
				return this.pHead;
			}
			set
			{
				this.pHead = value;
			}
		}

		public DataItem<float> Glass
		{
			get
			{
				return this.pGlass;
			}
			set
			{
				this.pGlass = value;
			}
		}

		public DataItem<float> Stone
		{
			get
			{
				return this.pStone;
			}
			set
			{
				this.pStone = value;
			}
		}

		public DataItem<float> Cloth
		{
			get
			{
				return this.pCloth;
			}
			set
			{
				this.pCloth = value;
			}
		}

		public DataItem<float> Concrete
		{
			get
			{
				return this.pConcrete;
			}
			set
			{
				this.pConcrete = value;
			}
		}

		public DataItem<float> Boulder
		{
			get
			{
				return this.pBoulder;
			}
			set
			{
				this.pBoulder = value;
			}
		}

		public DataItem<float> Metal
		{
			get
			{
				return this.pMetal;
			}
			set
			{
				this.pMetal = value;
			}
		}

		public DataItem<float> Wood
		{
			get
			{
				return this.pWood;
			}
			set
			{
				this.pWood = value;
			}
		}

		public DataItem<float> Earth
		{
			get
			{
				return this.pEarth;
			}
			set
			{
				this.pEarth = value;
			}
		}

		public DataItem<float> Snow
		{
			get
			{
				return this.pSnow;
			}
			set
			{
				this.pSnow = value;
			}
		}

		public DataItem<float> Plants
		{
			get
			{
				return this.pPlants;
			}
			set
			{
				this.pPlants = value;
			}
		}

		public DataItem<float> Leaves
		{
			get
			{
				return this.pLeaves;
			}
			set
			{
				this.pLeaves = value;
			}
		}

		public List<IDataItem> GetDisplayValues(bool _recursive = true)
		{
			return new List<IDataItem>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pHead;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pGlass;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pStone;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pCloth;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pConcrete;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pBoulder;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pMetal;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pWood;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pEarth;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pSnow;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pPlants;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pLeaves;

		public static class Parser
		{
			public static DamageBonusData Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
			{
				string text = _elem.HasAttribute("class") ? _elem.GetAttribute("class") : "DamageBonusData";
				Type type = Type.GetType(typeof(DamageBonusData.Parser).Namespace + "." + text);
				if (type == null)
				{
					type = Type.GetType(text);
					if (type == null)
					{
						throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
					}
				}
				DamageBonusData damageBonusData = (DamageBonusData)Activator.CreateInstance(type);
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				foreach (object obj in _elem.ChildNodes)
				{
					XmlNode xmlNode = (XmlNode)obj;
					XmlNodeType nodeType = xmlNode.NodeType;
					if (nodeType != XmlNodeType.Element)
					{
						if (nodeType != XmlNodeType.Comment)
						{
							throw new UnexpectedElementException("Unknown node \"" + xmlNode.NodeType.ToString() + "\" found while parsing DamageBonus", ((IXmlLineInfo)xmlNode).LineNumber);
						}
					}
					else
					{
						PositionXmlElement positionXmlElement = (PositionXmlElement)xmlNode;
						if (!DamageBonusData.Parser.knownAttributesMultiplicity.ContainsKey(positionXmlElement.Name))
						{
							throw new UnexpectedElementException("Unknown element \"" + xmlNode.Name + "\" found while parsing DamageBonus", ((IXmlLineInfo)xmlNode).LineNumber);
						}
						string name = positionXmlElement.Name;
						uint num = <PrivateImplementationDetails>.ComputeStringHash(name);
						if (num <= 2545987019U)
						{
							if (num <= 1307099730U)
							{
								if (num != 78706450U)
								{
									if (num != 81868168U)
									{
										if (num == 1307099730U)
										{
											if (name == "Boulder")
											{
												float startValue;
												try
												{
													startValue = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
												DataItem<float> pBoulder = new DataItem<float>("Boulder", startValue);
												damageBonusData.pBoulder = pBoulder;
											}
										}
									}
									else if (name == "Wood")
									{
										float startValue2;
										try
										{
											startValue2 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
										DataItem<float> pWood = new DataItem<float>("Wood", startValue2);
										damageBonusData.pWood = pWood;
									}
								}
								else if (name == "Snow")
								{
									float startValue3;
									try
									{
										startValue3 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
									DataItem<float> pSnow = new DataItem<float>("Snow", startValue3);
									damageBonusData.pSnow = pSnow;
								}
							}
							else if (num != 1842662042U)
							{
								if (num != 1858281043U)
								{
									if (num == 2545987019U)
									{
										if (name == "Glass")
										{
											float startValue4;
											try
											{
												startValue4 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
											DataItem<float> pGlass = new DataItem<float>("Glass", startValue4);
											damageBonusData.pGlass = pGlass;
										}
									}
								}
								else if (name == "Plants")
								{
									float startValue5;
									try
									{
										startValue5 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
									DataItem<float> pPlants = new DataItem<float>("Plants", startValue5);
									damageBonusData.pPlants = pPlants;
								}
							}
							else if (name == "Stone")
							{
								float startValue6;
								try
								{
									startValue6 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
								DataItem<float> pStone = new DataItem<float>("Stone", startValue6);
								damageBonusData.pStone = pStone;
							}
						}
						else if (num <= 2995012523U)
						{
							if (num != 2553495518U)
							{
								if (num != 2840670588U)
								{
									if (num == 2995012523U)
									{
										if (name == "Cloth")
										{
											float startValue7;
											try
											{
												startValue7 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
											DataItem<float> pCloth = new DataItem<float>("Cloth", startValue7);
											damageBonusData.pCloth = pCloth;
										}
									}
								}
								else if (name == "Metal")
								{
									float startValue8;
									try
									{
										startValue8 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
									DataItem<float> pMetal = new DataItem<float>("Metal", startValue8);
									damageBonusData.pMetal = pMetal;
								}
							}
							else if (name == "Concrete")
							{
								float startValue9;
								try
								{
									startValue9 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception innerException9)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, innerException9);
								}
								DataItem<float> pConcrete = new DataItem<float>("Concrete", startValue9);
								damageBonusData.pConcrete = pConcrete;
							}
						}
						else if (num != 2996251363U)
						{
							if (num != 3947615209U)
							{
								if (num == 4159608695U)
								{
									if (name == "Earth")
									{
										float startValue10;
										try
										{
											startValue10 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException10)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException10);
										}
										DataItem<float> pEarth = new DataItem<float>("Earth", startValue10);
										damageBonusData.pEarth = pEarth;
									}
								}
							}
							else if (name == "Leaves")
							{
								float startValue11;
								try
								{
									startValue11 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception innerException11)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, innerException11);
								}
								DataItem<float> pLeaves = new DataItem<float>("Leaves", startValue11);
								damageBonusData.pLeaves = pLeaves;
							}
						}
						else if (name == "Head")
						{
							float startValue12;
							try
							{
								startValue12 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
							}
							catch (Exception innerException12)
							{
								throw new InvalidValueException(string.Concat(new string[]
								{
									"Could not parse attribute \"",
									positionXmlElement.Name,
									"\" value \"",
									ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
									"\""
								}), positionXmlElement.LineNumber, innerException12);
							}
							DataItem<float> pHead = new DataItem<float>("Head", startValue12);
							damageBonusData.pHead = pHead;
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
				foreach (KeyValuePair<string, Range<int>> keyValuePair in DamageBonusData.Parser.knownAttributesMultiplicity)
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
				return damageBonusData;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
			{
				{
					"Head",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Glass",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Stone",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Cloth",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Concrete",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Boulder",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Metal",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Wood",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Earth",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Snow",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Plants",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Leaves",
					new Range<int>(true, 0, true, 1)
				}
			};
		}
	}
}
