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
	public class ExplosionData : IXMLData
	{
		public DataItem<int> BlockDamage
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

		public DataItem<int> EntityDamage
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

		public DataItem<int> ParticleIndex
		{
			get
			{
				return this.pParticleIndex;
			}
			set
			{
				this.pParticleIndex = value;
			}
		}

		public DataItem<int> RadiusBlocks
		{
			get
			{
				return this.pRadiusBlocks;
			}
			set
			{
				this.pRadiusBlocks = value;
			}
		}

		public DataItem<int> RadiusEntities
		{
			get
			{
				return this.pRadiusEntities;
			}
			set
			{
				this.pRadiusEntities = value;
			}
		}

		public List<IDataItem> GetDisplayValues(bool _recursive = true)
		{
			return new List<IDataItem>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pBlockDamage;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pEntityDamage;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pParticleIndex;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pRadiusBlocks;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pRadiusEntities;

		public static class Parser
		{
			public static ExplosionData Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
			{
				string text = _elem.HasAttribute("class") ? _elem.GetAttribute("class") : "ExplosionData";
				Type type = Type.GetType(typeof(ExplosionData.Parser).Namespace + "." + text);
				if (type == null)
				{
					type = Type.GetType(text);
					if (type == null)
					{
						throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
					}
				}
				ExplosionData explosionData = (ExplosionData)Activator.CreateInstance(type);
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				foreach (object obj in _elem.ChildNodes)
				{
					XmlNode xmlNode = (XmlNode)obj;
					XmlNodeType nodeType = xmlNode.NodeType;
					if (nodeType != XmlNodeType.Element)
					{
						if (nodeType != XmlNodeType.Comment)
						{
							throw new UnexpectedElementException("Unknown node \"" + xmlNode.NodeType.ToString() + "\" found while parsing Explosion", ((IXmlLineInfo)xmlNode).LineNumber);
						}
					}
					else
					{
						PositionXmlElement positionXmlElement = (PositionXmlElement)xmlNode;
						if (!ExplosionData.Parser.knownAttributesMultiplicity.ContainsKey(positionXmlElement.Name))
						{
							throw new UnexpectedElementException("Unknown element \"" + xmlNode.Name + "\" found while parsing Explosion", ((IXmlLineInfo)xmlNode).LineNumber);
						}
						string name = positionXmlElement.Name;
						if (!(name == "BlockDamage"))
						{
							if (!(name == "EntityDamage"))
							{
								if (!(name == "ParticleIndex"))
								{
									if (!(name == "RadiusBlocks"))
									{
										if (name == "RadiusEntities")
										{
											int startValue;
											try
											{
												startValue = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
											DataItem<int> pRadiusEntities = new DataItem<int>("RadiusEntities", startValue);
											explosionData.pRadiusEntities = pRadiusEntities;
										}
									}
									else
									{
										int startValue2;
										try
										{
											startValue2 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
										DataItem<int> pRadiusBlocks = new DataItem<int>("RadiusBlocks", startValue2);
										explosionData.pRadiusBlocks = pRadiusBlocks;
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
									DataItem<int> pParticleIndex = new DataItem<int>("ParticleIndex", startValue3);
									explosionData.pParticleIndex = pParticleIndex;
								}
							}
							else
							{
								int startValue4;
								try
								{
									startValue4 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
								DataItem<int> pEntityDamage = new DataItem<int>("EntityDamage", startValue4);
								explosionData.pEntityDamage = pEntityDamage;
							}
						}
						else
						{
							int startValue5;
							try
							{
								startValue5 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
							DataItem<int> pBlockDamage = new DataItem<int>("BlockDamage", startValue5);
							explosionData.pBlockDamage = pBlockDamage;
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
				foreach (KeyValuePair<string, Range<int>> keyValuePair in ExplosionData.Parser.knownAttributesMultiplicity)
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
				return explosionData;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
			{
				{
					"BlockDamage",
					new Range<int>(true, 0, true, 1)
				},
				{
					"EntityDamage",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ParticleIndex",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RadiusBlocks",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RadiusEntities",
					new Range<int>(true, 0, true, 1)
				}
			};
		}
	}
}
