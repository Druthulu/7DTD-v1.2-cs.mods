using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine.Scripting;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item
{
	[Preserve]
	public class ItemData : IXMLParserBase, IXMLData
	{
		public string Name
		{
			get
			{
				return this.pName;
			}
		}

		public int Id
		{
			get
			{
				return this.pId;
			}
		}

		public DataItem<bool> Active
		{
			get
			{
				return this.pActive;
			}
			set
			{
				this.pActive = value;
			}
		}

		public DataItem<bool> AlwaysActive
		{
			get
			{
				return this.pAlwaysActive;
			}
			set
			{
				this.pAlwaysActive = value;
			}
		}

		public DataItem<int> FuelValue
		{
			get
			{
				return this.pFuelValue;
			}
			set
			{
				this.pFuelValue = value;
			}
		}

		public DataItem<string> ImageEffectOnActive
		{
			get
			{
				return this.pImageEffectOnActive;
			}
			set
			{
				this.pImageEffectOnActive = value;
			}
		}

		public DataItem<PartsData> PartTypes
		{
			get
			{
				return this.pPartTypes;
			}
			set
			{
				this.pPartTypes = value;
			}
		}

		public DataItem<string> PlaySoundOnActive
		{
			get
			{
				return this.pPlaySoundOnActive;
			}
			set
			{
				this.pPlaySoundOnActive = value;
			}
		}

		public DataItem<int> Weight
		{
			get
			{
				return this.pWeight;
			}
			set
			{
				this.pWeight = value;
			}
		}

		public List<IDataItem> GetDisplayValues(bool _recursive = true)
		{
			List<IDataItem> list = new List<IDataItem>();
			if (_recursive && this.pPartTypes != null)
			{
				list.AddRange(this.pPartTypes.Value.GetDisplayValues(true));
			}
			for (int i = 0; i < this.pAction.Length; i++)
			{
				if (_recursive && this.pAction[i] != null)
				{
					list.AddRange(this.pAction[i].Value.GetDisplayValues(true));
				}
			}
			if (_recursive && this.pArmor != null)
			{
				list.AddRange(this.pArmor.Value.GetDisplayValues(true));
			}
			if (_recursive && this.pPreview != null)
			{
				list.AddRange(this.pPreview.Value.GetDisplayValues(true));
			}
			if (_recursive && this.pAttributes != null)
			{
				list.AddRange(this.pAttributes.Value.GetDisplayValues(true));
			}
			if (_recursive && this.pExplosion != null)
			{
				list.AddRange(this.pExplosion.Value.GetDisplayValues(true));
			}
			if (_recursive && this.pUMA != null)
			{
				list.AddRange(this.pUMA.Value.GetDisplayValues(true));
			}
			return list;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static Dictionary<string, ItemClass> pElementMap = new Dictionary<string, ItemClass>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static ItemClass[] pElementIndexed;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string pName;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int pId;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pAlwaysActive;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pActive;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pPlaySoundOnActive;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pImageEffectOnActive;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pMeshfile;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pMaterial;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pHoldType;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pCanhold;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pStacknumber;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pDegradation;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pDegradationBreaksAfter;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pFuelValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pCritChance;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pGroup;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pDamageEntityMin;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pDamageEntityMax;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSmell;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pDropScale;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pCustomIcon;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pCustomIconTint;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<EPartType> pPartType;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pWeight;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pCandrop;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pUserHidden;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pLightSource;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<bool> pThrowableDecoy;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pFuseTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<ItemClass> pMoldTarget;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<EquipmentSlots> pEquipSlot;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pActivateObject;

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemData.DataItemArrayRepairTools pRepairTools;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<float> pRepairTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<int> pRepairAmount;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundDestroy;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundIdle;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string> pSoundJammed;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<PartsData> pPartTypes;

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemData.DataItemArrayAction pAction;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<ArmorData> pArmor;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<PreviewData> pPreview;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<AttributesData> pAttributes;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<ExplosionData> pExplosion;

		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<UMAData> pUMA;

		public class DataItemArrayRepairTools
		{
			public DataItem<string> this[int index]
			{
				get
				{
					if (index >= this.pRepairTools.Length)
					{
						throw new ArgumentOutOfRangeException("index", "index " + index.ToString() + " greater/equal than array length " + this.pRepairTools.Length.ToString());
					}
					return this.pRepairTools[index];
				}
				set
				{
					if (index >= this.pRepairTools.Length)
					{
						throw new ArgumentOutOfRangeException("index", "index " + index.ToString() + " greater/equal than array length " + this.pRepairTools.Length.ToString());
					}
					this.pRepairTools[index] = value;
				}
			}

			public int Length
			{
				get
				{
					return this.pRepairTools.Length;
				}
			}

			public DataItemArrayRepairTools(int _size)
			{
				this.pRepairTools = new DataItem<string>[_size];
			}

			public DataItemArrayRepairTools(DataItem<string>[] _init)
			{
				this.pRepairTools = _init;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public DataItem<string>[] pRepairTools;
		}

		public class DataItemArrayAction
		{
			public DataItem<ItemAction> this[int index]
			{
				get
				{
					if (index >= this.pAction.Length)
					{
						throw new ArgumentOutOfRangeException("index", "index " + index.ToString() + " greater/equal than array length " + this.pAction.Length.ToString());
					}
					return this.pAction[index];
				}
				set
				{
					if (index >= this.pAction.Length)
					{
						throw new ArgumentOutOfRangeException("index", "index " + index.ToString() + " greater/equal than array length " + this.pAction.Length.ToString());
					}
					this.pAction[index] = value;
				}
			}

			public int Length
			{
				get
				{
					return this.pAction.Length;
				}
			}

			public DataItemArrayAction(int _size)
			{
				this.pAction = new DataItem<ItemAction>[_size];
			}

			public DataItemArrayAction(DataItem<ItemAction>[] _init)
			{
				this.pAction = _init;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public DataItem<ItemAction>[] pAction;
		}

		public static class Parser
		{
			public static ItemClass Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
			{
				string pName = ParserUtils.ParseStringAttribute(_elem, "name", true, null);
				int pId = ParserUtils.ParseIntAttribute(_elem, "id", true, 0) + ItemData.Parser.idOffset;
				string text = _elem.HasAttribute("class") ? _elem.GetAttribute("class") : "ItemClass";
				Type type = Type.GetType(typeof(ItemData.Parser).Namespace + "." + text);
				if (type == null)
				{
					type = Type.GetType(text);
					if (type == null)
					{
						throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
					}
				}
				ItemClass itemClass = (ItemClass)Activator.CreateInstance(type);
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				itemClass.pName = pName;
				itemClass.pId = pId;
				List<DataItem<string>> list = new List<DataItem<string>>();
				List<DataItem<ItemAction>> list2 = new List<DataItem<ItemAction>>();
				foreach (object obj in _elem.ChildNodes)
				{
					XmlNode xmlNode = (XmlNode)obj;
					XmlNodeType nodeType = xmlNode.NodeType;
					if (nodeType != XmlNodeType.Element)
					{
						if (nodeType != XmlNodeType.Comment)
						{
							throw new UnexpectedElementException("Unknown node \"" + xmlNode.NodeType.ToString() + "\" found while parsing Item", ((IXmlLineInfo)xmlNode).LineNumber);
						}
					}
					else
					{
						PositionXmlElement positionXmlElement = (PositionXmlElement)xmlNode;
						if (!ItemData.Parser.knownAttributesMultiplicity.ContainsKey(positionXmlElement.Name))
						{
							throw new UnexpectedElementException("Unknown element \"" + xmlNode.Name + "\" found while parsing Item", ((IXmlLineInfo)xmlNode).LineNumber);
						}
						string name = positionXmlElement.Name;
						uint num = <PrivateImplementationDetails>.ComputeStringHash(name);
						if (num <= 2292684416U)
						{
							if (num <= 955858345U)
							{
								if (num <= 529077071U)
								{
									if (num <= 175614239U)
									{
										if (num != 91525164U)
										{
											if (num == 175614239U)
											{
												if (name == "Action")
												{
													ItemAction startValue = ItemActionData.Parser.Parse(positionXmlElement, _updateLater);
													DataItem<ItemAction> item = new DataItem<ItemAction>("Action", startValue);
													list2.Add(item);
												}
											}
										}
										else if (name == "Group")
										{
											string startValue2;
											try
											{
												startValue2 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
											DataItem<string> pGroup = new DataItem<string>("Group", startValue2);
											itemClass.pGroup = pGroup;
										}
									}
									else if (num != 372600678U)
									{
										if (num != 461563084U)
										{
											if (num == 529077071U)
											{
												if (name == "Active")
												{
													bool startValue3;
													try
													{
														startValue3 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
													DataItem<bool> active = new DataItem<bool>("Active", startValue3);
													itemClass.Active = active;
												}
											}
										}
										else if (name == "Candrop")
										{
											bool startValue4;
											try
											{
												startValue4 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
											DataItem<bool> pCandrop = new DataItem<bool>("Candrop", startValue4);
											itemClass.pCandrop = pCandrop;
										}
									}
									else if (name == "DropScale")
									{
										int startValue5;
										try
										{
											startValue5 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
										DataItem<int> pDropScale = new DataItem<int>("DropScale", startValue5);
										itemClass.pDropScale = pDropScale;
									}
								}
								else if (num <= 531232375U)
								{
									if (num != 530817272U)
									{
										if (num == 531232375U)
										{
											if (name == "DegradationBreaksAfter")
											{
												bool startValue6;
												try
												{
													startValue6 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
												DataItem<bool> pDegradationBreaksAfter = new DataItem<bool>("DegradationBreaksAfter", startValue6);
												itemClass.pDegradationBreaksAfter = pDegradationBreaksAfter;
											}
										}
									}
									else if (name == "Canhold")
									{
										bool startValue7;
										try
										{
											startValue7 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
										DataItem<bool> pCanhold = new DataItem<bool>("Canhold", startValue7);
										itemClass.pCanhold = pCanhold;
									}
								}
								else if (num != 759865468U)
								{
									if (num != 942707696U)
									{
										if (num == 955858345U)
										{
											if (name == "FuseTime")
											{
												float startValue8;
												try
												{
													startValue8 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
												DataItem<float> pFuseTime = new DataItem<float>("FuseTime", startValue8);
												itemClass.pFuseTime = pFuseTime;
											}
										}
									}
									else if (name == "MoldTarget")
									{
										ItemClass startValue9;
										try
										{
											startValue9 = null;
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
										DataItem<ItemClass> dataItem = new DataItem<ItemClass>("MoldTarget", startValue9);
										_updateLater.Add(positionXmlElement, dataItem);
										itemClass.pMoldTarget = dataItem;
									}
								}
								else if (name == "PartType")
								{
									EPartType startValue10;
									try
									{
										startValue10 = EnumParser.Parse<EPartType>(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
									DataItem<EPartType> pPartType = new DataItem<EPartType>("PartType", startValue10);
									itemClass.pPartType = pPartType;
								}
							}
							else if (num <= 1432876798U)
							{
								if (num <= 1012402666U)
								{
									if (num != 960815908U)
									{
										if (num == 1012402666U)
										{
											if (name == "SoundJammed")
											{
												string startValue11;
												try
												{
													startValue11 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
												DataItem<string> pSoundJammed = new DataItem<string>("SoundJammed", startValue11);
												itemClass.pSoundJammed = pSoundJammed;
											}
										}
									}
									else if (name == "ImageEffectOnActive")
									{
										string startValue12;
										try
										{
											startValue12 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
										DataItem<string> pMeshfile = new DataItem<string>("ImageEffectOnActive", startValue12);
										itemClass.pMeshfile = pMeshfile;
									}
								}
								else if (num != 1053088598U)
								{
									if (num != 1082430025U)
									{
										if (num == 1432876798U)
										{
											if (name == "SoundIdle")
											{
												string startValue13;
												try
												{
													startValue13 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
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
												DataItem<string> pSoundIdle = new DataItem<string>("SoundIdle", startValue13);
												itemClass.pSoundIdle = pSoundIdle;
											}
										}
									}
									else if (name == "CritChance")
									{
										float startValue14;
										try
										{
											startValue14 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException13)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException13);
										}
										DataItem<float> pCritChance = new DataItem<float>("CritChance", startValue14);
										itemClass.pCritChance = pCritChance;
									}
								}
								else if (name == "CustomIconTint")
								{
									string startValue15;
									try
									{
										startValue15 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException14)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException14);
									}
									DataItem<string> pCustomIconTint = new DataItem<string>("CustomIconTint", startValue15);
									itemClass.pCustomIconTint = pCustomIconTint;
								}
							}
							else if (num <= 2079154250U)
							{
								if (num != 1534719901U)
								{
									if (num == 2079154250U)
									{
										if (name == "Meshfile")
										{
											string startValue16;
											try
											{
												startValue16 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException15)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException15);
											}
											DataItem<string> pMeshfile2 = new DataItem<string>("Meshfile", startValue16);
											itemClass.pMeshfile = pMeshfile2;
										}
									}
								}
								else if (name == "PartTypes")
								{
									PartsData startValue17 = PartsData.Parser.Parse(positionXmlElement, _updateLater);
									DataItem<PartsData> pPartTypes = new DataItem<PartsData>("PartTypes", startValue17);
									itemClass.pPartTypes = pPartTypes;
								}
							}
							else if (num != 2104753456U)
							{
								if (num != 2226667892U)
								{
									if (num == 2292684416U)
									{
										if (name == "Explosion")
										{
											ExplosionData startValue18 = ExplosionData.Parser.Parse(positionXmlElement, _updateLater);
											DataItem<ExplosionData> pExplosion = new DataItem<ExplosionData>("Explosion", startValue18);
											itemClass.pExplosion = pExplosion;
										}
									}
								}
								else if (name == "Armor")
								{
									ArmorData startValue19 = ArmorData.Parser.Parse(positionXmlElement, _updateLater);
									DataItem<ArmorData> pArmor = new DataItem<ArmorData>("Armor", startValue19);
									itemClass.pArmor = pArmor;
								}
							}
							else if (name == "HoldType")
							{
								int startValue20;
								try
								{
									startValue20 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception innerException16)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, innerException16);
								}
								DataItem<int> pHoldType = new DataItem<int>("HoldType", startValue20);
								itemClass.pHoldType = pHoldType;
							}
						}
						else if (num <= 3419754368U)
						{
							if (num <= 2924404423U)
							{
								if (num <= 2570065113U)
								{
									if (num != 2517612320U)
									{
										if (num == 2570065113U)
										{
											if (name == "Weight")
											{
												int startValue21;
												try
												{
													startValue21 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException17)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException17);
												}
												DataItem<int> pWeight = new DataItem<int>("Weight", startValue21);
												itemClass.pWeight = pWeight;
											}
										}
									}
									else if (name == "Smell")
									{
										string startValue22;
										try
										{
											startValue22 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException18)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException18);
										}
										DataItem<string> pSmell = new DataItem<string>("Smell", startValue22);
										itemClass.pSmell = pSmell;
									}
								}
								else if (num != 2613839563U)
								{
									if (num != 2887776777U)
									{
										if (num == 2924404423U)
										{
											if (name == "RepairTools")
											{
												string startValue23;
												try
												{
													startValue23 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
												}
												catch (Exception innerException19)
												{
													throw new InvalidValueException(string.Concat(new string[]
													{
														"Could not parse attribute \"",
														positionXmlElement.Name,
														"\" value \"",
														ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
														"\""
													}), positionXmlElement.LineNumber, innerException19);
												}
												DataItem<string> item2 = new DataItem<string>("RepairTools", startValue23);
												list.Add(item2);
											}
										}
									}
									else if (name == "ThrowableDecoy")
									{
										bool startValue24;
										try
										{
											startValue24 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException20)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException20);
										}
										DataItem<bool> pThrowableDecoy = new DataItem<bool>("ThrowableDecoy", startValue24);
										itemClass.pThrowableDecoy = pThrowableDecoy;
									}
								}
								else if (name == "CustomIcon")
								{
									string startValue25;
									try
									{
										startValue25 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException21)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException21);
									}
									DataItem<string> pCustomIcon = new DataItem<string>("CustomIcon", startValue25);
									itemClass.pCustomIcon = pCustomIcon;
								}
							}
							else if (num <= 3131847555U)
							{
								if (num != 2948774643U)
								{
									if (num == 3131847555U)
									{
										if (name == "Degradation")
										{
											int startValue26;
											try
											{
												startValue26 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException22)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException22);
											}
											DataItem<int> pDegradation = new DataItem<int>("Degradation", startValue26);
											itemClass.pDegradation = pDegradation;
										}
									}
								}
								else if (name == "DamageEntityMax")
								{
									int startValue27;
									try
									{
										startValue27 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException23)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException23);
									}
									DataItem<int> pDamageEntityMax = new DataItem<int>("DamageEntityMax", startValue27);
									itemClass.pDamageEntityMax = pDamageEntityMax;
								}
							}
							else if (num != 3319162189U)
							{
								if (num != 3365715102U)
								{
									if (num == 3419754368U)
									{
										if (name == "Material")
										{
											string startValue28;
											try
											{
												startValue28 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException24)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException24);
											}
											DataItem<string> pMaterial = new DataItem<string>("Material", startValue28);
											itemClass.pMaterial = pMaterial;
										}
									}
								}
								else if (name == "Stacknumber")
								{
									int startValue29;
									try
									{
										startValue29 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException25)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException25);
									}
									DataItem<int> pStacknumber = new DataItem<int>("Stacknumber", startValue29);
									itemClass.pStacknumber = pStacknumber;
								}
							}
							else if (name == "DamageEntityMin")
							{
								int startValue30;
								try
								{
									startValue30 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception innerException26)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, innerException26);
								}
								DataItem<int> pDamageEntityMin = new DataItem<int>("DamageEntityMin", startValue30);
								itemClass.pDamageEntityMin = pDamageEntityMin;
							}
						}
						else if (num <= 3694724757U)
						{
							if (num <= 3463560063U)
							{
								if (num != 3430864052U)
								{
									if (num == 3463560063U)
									{
										if (name == "EquipSlot")
										{
											EquipmentSlots startValue31;
											try
											{
												startValue31 = EnumParser.Parse<EquipmentSlots>(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException27)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException27);
											}
											DataItem<EquipmentSlots> pEquipSlot = new DataItem<EquipmentSlots>("EquipSlot", startValue31);
											itemClass.pEquipSlot = pEquipSlot;
										}
									}
								}
								else if (name == "Attributes")
								{
									AttributesData startValue32 = AttributesData.Parser.Parse(positionXmlElement, _updateLater);
									DataItem<AttributesData> pAttributes = new DataItem<AttributesData>("Attributes", startValue32);
									itemClass.pAttributes = pAttributes;
								}
							}
							else if (num != 3519818983U)
							{
								if (num != 3646476408U)
								{
									if (num == 3694724757U)
									{
										if (name == "RepairTime")
										{
											float startValue33;
											try
											{
												startValue33 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
											}
											catch (Exception innerException28)
											{
												throw new InvalidValueException(string.Concat(new string[]
												{
													"Could not parse attribute \"",
													positionXmlElement.Name,
													"\" value \"",
													ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
													"\""
												}), positionXmlElement.LineNumber, innerException28);
											}
											DataItem<float> pRepairTime = new DataItem<float>("RepairTime", startValue33);
											itemClass.pRepairTime = pRepairTime;
										}
									}
								}
								else if (name == "RepairAmount")
								{
									int startValue34;
									try
									{
										startValue34 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
									}
									catch (Exception innerException29)
									{
										throw new InvalidValueException(string.Concat(new string[]
										{
											"Could not parse attribute \"",
											positionXmlElement.Name,
											"\" value \"",
											ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
											"\""
										}), positionXmlElement.LineNumber, innerException29);
									}
									DataItem<int> pRepairAmount = new DataItem<int>("RepairAmount", startValue34);
									itemClass.pRepairAmount = pRepairAmount;
								}
							}
							else if (name == "ActivateObject")
							{
								string startValue35;
								try
								{
									startValue35 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception innerException30)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, innerException30);
								}
								DataItem<string> pActivateObject = new DataItem<string>("ActivateObject", startValue35);
								itemClass.pActivateObject = pActivateObject;
							}
						}
						else if (num <= 3912538170U)
						{
							if (num != 3912144422U)
							{
								if (num == 3912538170U)
								{
									if (name == "UserHidden")
									{
										bool startValue36;
										try
										{
											startValue36 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException31)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException31);
										}
										DataItem<bool> pUserHidden = new DataItem<bool>("UserHidden", startValue36);
										itemClass.pUserHidden = pUserHidden;
									}
								}
							}
							else if (name == "SoundDestroy")
							{
								string startValue37;
								try
								{
									startValue37 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
								}
								catch (Exception innerException32)
								{
									throw new InvalidValueException(string.Concat(new string[]
									{
										"Could not parse attribute \"",
										positionXmlElement.Name,
										"\" value \"",
										ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
										"\""
									}), positionXmlElement.LineNumber, innerException32);
								}
								DataItem<string> pSoundDestroy = new DataItem<string>("SoundDestroy", startValue37);
								itemClass.pSoundDestroy = pSoundDestroy;
							}
						}
						else if (num != 4235238322U)
						{
							if (num != 4258942199U)
							{
								if (num == 4293145386U)
								{
									if (name == "LightSource")
									{
										string startValue38;
										try
										{
											startValue38 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
										}
										catch (Exception innerException33)
										{
											throw new InvalidValueException(string.Concat(new string[]
											{
												"Could not parse attribute \"",
												positionXmlElement.Name,
												"\" value \"",
												ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
												"\""
											}), positionXmlElement.LineNumber, innerException33);
										}
										DataItem<string> pLightSource = new DataItem<string>("LightSource", startValue38);
										itemClass.pLightSource = pLightSource;
									}
								}
							}
							else if (name == "Preview")
							{
								PreviewData startValue39 = PreviewData.Parser.Parse(positionXmlElement, _updateLater);
								DataItem<PreviewData> pPreview = new DataItem<PreviewData>("Preview", startValue39);
								itemClass.pPreview = pPreview;
							}
						}
						else if (name == "FuelValue")
						{
							int startValue40;
							try
							{
								startValue40 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null));
							}
							catch (Exception innerException34)
							{
								throw new InvalidValueException(string.Concat(new string[]
								{
									"Could not parse attribute \"",
									positionXmlElement.Name,
									"\" value \"",
									ParserUtils.ParseStringAttribute(positionXmlElement, "value", true, null),
									"\""
								}), positionXmlElement.LineNumber, innerException34);
							}
							DataItem<int> pFuelValue = new DataItem<int>("FuelValue", startValue40);
							itemClass.pFuelValue = pFuelValue;
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
				if (!dictionary.ContainsKey("Stacknumber"))
				{
					int startValue41;
					try
					{
						startValue41 = intParser.Parse("64");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"64\" for attribute \"Stacknumber\" could not be parsed", -1);
					}
					DataItem<int> pStacknumber2 = new DataItem<int>("Stacknumber", startValue41);
					itemClass.pStacknumber = pStacknumber2;
					dictionary["Stacknumber"] = 1;
				}
				if (!dictionary.ContainsKey("Degradation"))
				{
					int startValue42;
					try
					{
						startValue42 = intParser.Parse("0");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"0\" for attribute \"Degradation\" could not be parsed", -1);
					}
					DataItem<int> pDegradation2 = new DataItem<int>("Degradation", startValue42);
					itemClass.pDegradation = pDegradation2;
					dictionary["Degradation"] = 1;
				}
				if (!dictionary.ContainsKey("DegradationBreaksAfter"))
				{
					bool startValue43;
					try
					{
						startValue43 = boolParser.Parse("true");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"true\" for attribute \"DegradationBreaksAfter\" could not be parsed", -1);
					}
					DataItem<bool> pDegradationBreaksAfter2 = new DataItem<bool>("DegradationBreaksAfter", startValue43);
					itemClass.pDegradationBreaksAfter = pDegradationBreaksAfter2;
					dictionary["DegradationBreaksAfter"] = 1;
				}
				if (!dictionary.ContainsKey("CritChance"))
				{
					float startValue44;
					try
					{
						startValue44 = floatParser.Parse("0");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"0\" for attribute \"CritChance\" could not be parsed", -1);
					}
					DataItem<float> pCritChance2 = new DataItem<float>("CritChance", startValue44);
					itemClass.pCritChance = pCritChance2;
					dictionary["CritChance"] = 1;
				}
				if (!dictionary.ContainsKey("Group"))
				{
					string startValue45;
					try
					{
						startValue45 = stringParser.Parse("Miscellaneous");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"Miscellaneous\" for attribute \"Group\" could not be parsed", -1);
					}
					DataItem<string> pGroup2 = new DataItem<string>("Group", startValue45);
					itemClass.pGroup = pGroup2;
					dictionary["Group"] = 1;
				}
				if (!dictionary.ContainsKey("PartType"))
				{
					EPartType startValue46;
					try
					{
						startValue46 = EnumParser.Parse<EPartType>("None");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"None\" for attribute \"PartType\" could not be parsed", -1);
					}
					DataItem<EPartType> pPartType2 = new DataItem<EPartType>("PartType", startValue46);
					itemClass.pPartType = pPartType2;
					dictionary["PartType"] = 1;
				}
				if (!dictionary.ContainsKey("UserHidden"))
				{
					bool startValue47;
					try
					{
						startValue47 = boolParser.Parse("false");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"false\" for attribute \"UserHidden\" could not be parsed", -1);
					}
					DataItem<bool> pUserHidden2 = new DataItem<bool>("UserHidden", startValue47);
					itemClass.pUserHidden = pUserHidden2;
					dictionary["UserHidden"] = 1;
				}
				if (!dictionary.ContainsKey("ThrowableDecoy"))
				{
					bool startValue48;
					try
					{
						startValue48 = boolParser.Parse("false");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"false\" for attribute \"ThrowableDecoy\" could not be parsed", -1);
					}
					DataItem<bool> pThrowableDecoy2 = new DataItem<bool>("ThrowableDecoy", startValue48);
					itemClass.pThrowableDecoy = pThrowableDecoy2;
					dictionary["ThrowableDecoy"] = 1;
				}
				if (!dictionary.ContainsKey("EquipSlot"))
				{
					EquipmentSlots startValue49;
					try
					{
						startValue49 = EnumParser.Parse<EquipmentSlots>("None");
					}
					catch (Exception)
					{
						throw new InvalidValueException("Default value \"None\" for attribute \"EquipSlot\" could not be parsed", -1);
					}
					DataItem<EquipmentSlots> pEquipSlot2 = new DataItem<EquipmentSlots>("EquipSlot", startValue49);
					itemClass.pEquipSlot = pEquipSlot2;
					dictionary["EquipSlot"] = 1;
				}
				foreach (KeyValuePair<string, Range<int>> keyValuePair in ItemData.Parser.knownAttributesMultiplicity)
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
				itemClass.pRepairTools = new ItemData.DataItemArrayRepairTools(list.ToArray());
				itemClass.pAction = new ItemData.DataItemArrayAction(list2.ToArray());
				return itemClass;
			}

			public static List<ItemClass> ParseXml(string _filename, string _content, bool _clearFirst = true, bool _validateOnly = false)
			{
				PositionXmlDocument positionXmlDocument = new PositionXmlDocument();
				if (_clearFirst && !_validateOnly)
				{
					ItemData.Parser.Clear();
				}
				try
				{
					using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(_content ?? "")))
					{
						positionXmlDocument.Load(XmlReader.Create(stream));
					}
				}
				catch (XmlException e)
				{
					Log.Error("Failed parsing " + _filename + ":");
					Log.Exception(e);
					return null;
				}
				XmlNode documentElement = positionXmlDocument.DocumentElement;
				List<ItemClass> list = new List<ItemClass>();
				Dictionary<PositionXmlElement, DataItem<ItemClass>> updateLater = new Dictionary<PositionXmlElement, DataItem<ItemClass>>();
				foreach (object obj in documentElement.ChildNodes)
				{
					XmlNode xmlNode = (XmlNode)obj;
					XmlNodeType nodeType = xmlNode.NodeType;
					if (nodeType != XmlNodeType.Element)
					{
						if (nodeType != XmlNodeType.Comment)
						{
							Log.Error("Unexpected XML node: " + xmlNode.NodeType.ToString() + " at line " + ((IXmlLineInfo)xmlNode).LineNumber.ToString());
						}
					}
					else if (xmlNode.Name == "Item")
					{
						ItemClass itemClass = ItemData.Parser.Parse((PositionXmlElement)xmlNode, updateLater);
						if (itemClass != null)
						{
							list.Add(itemClass);
						}
					}
					else
					{
						Log.Error(string.Format("Unknown element found: {0} (file {1}, line {2})", xmlNode.Name, _filename, ((IXmlLineInfo)xmlNode).LineNumber));
					}
				}
				if (!_validateOnly)
				{
					ItemData.Parser.FillLists(_filename, list, _clearFirst);
					ItemData.Parser.UpdateXmlRefs(_filename, updateLater);
				}
				return list;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static void Clear()
			{
				ItemData.pElementMap.Clear();
				ItemData.pElementIndexed = null;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static void FillLists(string _filename, List<ItemClass> _entries, bool _clearFirst)
			{
				int num = -1;
				HashSet<int> hashSet = new HashSet<int>();
				foreach (ItemClass itemClass in _entries)
				{
					if (itemClass.Id > num)
					{
						num = itemClass.Id;
					}
					if (hashSet.Contains(itemClass.Id))
					{
						Log.Error(string.Format("Duplicate index: {0} in {1}", itemClass.Id, _filename));
					}
					hashSet.Add(itemClass.Id);
				}
				if (!_clearFirst && num >= ItemData.pElementIndexed.Length)
				{
					ItemClass[] array = new ItemClass[num + 1];
					Array.Copy(ItemData.pElementIndexed, array, ItemData.pElementIndexed.Length);
					ItemData.pElementIndexed = array;
				}
				else if (_clearFirst)
				{
					ItemData.pElementIndexed = new ItemClass[num + 1];
				}
				foreach (ItemClass itemClass2 in _entries)
				{
					if (ItemData.pElementIndexed[itemClass2.Id] != null)
					{
						Log.Warning(string.Format("Overwriting existing element index: {0} in {1}", itemClass2.Id, _filename));
					}
					ItemData.pElementIndexed[itemClass2.Id] = itemClass2;
					if (ItemData.pElementMap.ContainsKey(itemClass2.Name))
					{
						Log.Warning(string.Format("Overwriting existing element name: {0} in {1}", itemClass2.Name, _filename));
					}
					ItemData.pElementMap[itemClass2.Name] = itemClass2;
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static void UpdateXmlRefs(string _filename, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
			{
				foreach (KeyValuePair<PositionXmlElement, DataItem<ItemClass>> keyValuePair in _updateLater)
				{
					string text = ParserUtils.ParseStringAttribute(keyValuePair.Key, "value", true, null);
					if (!ItemData.pElementMap.ContainsKey(text))
					{
						throw new InvalidValueException(string.Concat(new string[]
						{
							"Element with name \"",
							text,
							"\" for attribute \"",
							keyValuePair.Value.Name,
							"\" not found (referencing an XML entry by name which is not defined)"
						}), keyValuePair.Key.LineNumber);
					}
					keyValuePair.Value.Value = ItemData.pElementMap[text];
				}
			}

			public static int idOffset = 0;

			[PublicizedFrom(EAccessModifier.Private)]
			public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
			{
				{
					"Active",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ImageEffectOnActive",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Meshfile",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Material",
					new Range<int>(true, 1, true, 1)
				},
				{
					"HoldType",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Canhold",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Stacknumber",
					new Range<int>(true, 1, true, 1)
				},
				{
					"Degradation",
					new Range<int>(true, 1, true, 1)
				},
				{
					"DegradationBreaksAfter",
					new Range<int>(true, 1, true, 1)
				},
				{
					"FuelValue",
					new Range<int>(true, 0, true, 1)
				},
				{
					"CritChance",
					new Range<int>(true, 1, true, 1)
				},
				{
					"Group",
					new Range<int>(true, 1, true, 1)
				},
				{
					"DamageEntityMin",
					new Range<int>(true, 0, true, 1)
				},
				{
					"DamageEntityMax",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Smell",
					new Range<int>(true, 0, true, 1)
				},
				{
					"DropScale",
					new Range<int>(true, 0, true, 1)
				},
				{
					"CustomIcon",
					new Range<int>(true, 0, true, 1)
				},
				{
					"CustomIconTint",
					new Range<int>(true, 0, true, 1)
				},
				{
					"PartType",
					new Range<int>(true, 1, true, 1)
				},
				{
					"Weight",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Candrop",
					new Range<int>(true, 0, true, 1)
				},
				{
					"UserHidden",
					new Range<int>(true, 1, true, 1)
				},
				{
					"LightSource",
					new Range<int>(true, 0, true, 1)
				},
				{
					"ThrowableDecoy",
					new Range<int>(true, 1, true, 1)
				},
				{
					"FuseTime",
					new Range<int>(true, 0, true, 1)
				},
				{
					"MoldTarget",
					new Range<int>(true, 0, true, 1)
				},
				{
					"EquipSlot",
					new Range<int>(true, 1, true, 1)
				},
				{
					"ActivateObject",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RepairTools",
					new Range<int>(true, 0, false, 0)
				},
				{
					"RepairTime",
					new Range<int>(true, 0, true, 1)
				},
				{
					"RepairAmount",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundDestroy",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundIdle",
					new Range<int>(true, 0, true, 1)
				},
				{
					"SoundJammed",
					new Range<int>(true, 0, true, 1)
				},
				{
					"PartTypes",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Action",
					new Range<int>(true, 0, true, 5)
				},
				{
					"Armor",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Preview",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Attributes",
					new Range<int>(true, 0, true, 1)
				},
				{
					"Explosion",
					new Range<int>(true, 0, true, 1)
				},
				{
					"UMA",
					new Range<int>(true, 0, true, 1)
				}
			};
		}
	}
}
