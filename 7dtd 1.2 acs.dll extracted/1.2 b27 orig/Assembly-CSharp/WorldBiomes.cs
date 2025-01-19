using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using UnityEngine;

public class WorldBiomes
{
	public WorldBiomes(XDocument _genxml, bool _instantiateReferences)
	{
		WorldBiomes.Instance = this;
		this.m_Color2BiomeMap = new Dictionary<uint, BiomeDefinition>();
		this.m_Id2BiomeArr = new BiomeDefinition[256];
		this.m_Name2BiomeMap = new CaseInsensitiveStringDictionary<BiomeDefinition>();
		this.m_PoiMap = new Dictionary<uint, PoiMapElement>();
		this.readXML(_genxml, _instantiateReferences);
	}

	public void Cleanup()
	{
	}

	public static void CleanupStatic()
	{
		if (WorldBiomes.Instance != null)
		{
			WorldBiomes.Instance.Cleanup();
		}
	}

	public int GetBiomeCount()
	{
		if (this.m_Color2BiomeMap == null)
		{
			return 0;
		}
		return this.m_Color2BiomeMap.Count;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Dictionary<uint, BiomeDefinition> GetBiomeMap()
	{
		return this.m_Color2BiomeMap;
	}

	public BiomeDefinition GetBiome(Color32 _color)
	{
		if (this.m_Color2BiomeMap.ContainsKey((uint)((int)_color.r << 16 | (int)_color.g << 8 | (int)_color.b)))
		{
			return this.m_Color2BiomeMap[(uint)((int)_color.r << 16 | (int)_color.g << 8 | (int)_color.b)];
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BiomeDefinition GetBiome(byte _id)
	{
		return this.m_Id2BiomeArr[(int)_id];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetBiome(byte _id, out BiomeDefinition _bd)
	{
		_bd = this.m_Id2BiomeArr[(int)_id];
		return _bd != null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BiomeDefinition GetBiome(string _name)
	{
		if (!this.m_Name2BiomeMap.ContainsKey(_name))
		{
			return null;
		}
		return this.m_Name2BiomeMap[_name];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readXML(XDocument _xml, bool _instantiateReferences)
	{
		Array.Clear(this.m_Id2BiomeArr, 0, this.m_Id2BiomeArr.Length);
		this.m_Color2BiomeMap.Clear();
		this.m_Name2BiomeMap.Clear();
		if (BiomeDefinition.nameToId == null)
		{
			BiomeDefinition.nameToId = new Dictionary<string, byte>();
		}
		foreach (XElement element in _xml.Descendants("biomemap"))
		{
			string attribute = element.GetAttribute("name");
			if (!BiomeDefinition.nameToId.ContainsKey(attribute))
			{
				BiomeDefinition.nameToId.Add(attribute, byte.Parse(element.GetAttribute("id")));
			}
		}
		foreach (XElement xelement in _xml.Descendants("biome"))
		{
			string attribute2 = xelement.GetAttribute("name");
			if (!BiomeDefinition.nameToId.ContainsKey(attribute2))
			{
				throw new Exception("Parsing biomes. Biome with name '" + attribute2 + "' also needs an entry in the biomemap");
			}
			byte id = BiomeDefinition.nameToId[attribute2];
			BiomeDefinition biomeDefinition = this.parseBiome(id, 0, attribute2, xelement, _instantiateReferences);
			this.m_Id2BiomeArr[(int)biomeDefinition.m_Id] = biomeDefinition;
			this.m_Color2BiomeMap.Add(biomeDefinition.m_uiColor, biomeDefinition);
			this.m_Name2BiomeMap.Add(biomeDefinition.m_sBiomeName, biomeDefinition);
		}
		BiomeParticleManager.RegistrationCompleted = true;
		foreach (XElement xelement2 in _xml.Descendants("pois"))
		{
			foreach (XElement xelement3 in xelement2.Descendants("poi"))
			{
				uint num = Convert.ToUInt32(xelement3.GetAttribute("poimapcolor").Substring(1), 16);
				int iSO = 0;
				int num2 = 0;
				if (xelement3.HasAttribute("surfaceoffset"))
				{
					iSO = Convert.ToInt32(xelement3.GetAttribute("surfaceoffset"));
				}
				if (xelement3.HasAttribute("smoothness"))
				{
					int num3 = Convert.ToInt32(xelement3.GetAttribute("smoothness"));
					int num4 = (num3 < 0) ? 0 : num3;
				}
				if (xelement3.HasAttribute("starttunnel"))
				{
					num2 = Convert.ToInt32(xelement3.GetAttribute("starttunnel"));
					num2 = ((num2 < 0) ? 0 : num2);
				}
				BlockValue blockValue = BlockValue.Air;
				if (xelement3.HasAttribute("blockname"))
				{
					blockValue = (_instantiateReferences ? this.getBlockValueForName(xelement3.GetAttribute("blockname")) : BlockValue.Air);
				}
				BlockValue blockBelow = BlockValue.Air;
				if (xelement3.HasAttribute("blockbelow"))
				{
					blockBelow = (_instantiateReferences ? this.getBlockValueForName(xelement3.GetAttribute("blockbelow")) : BlockValue.Air);
				}
				int ypos = -1;
				if (xelement3.HasAttribute("ypos"))
				{
					ypos = int.Parse(xelement3.GetAttribute("ypos"));
				}
				int yposFill = -1;
				if (xelement3.HasAttribute("yposfill"))
				{
					yposFill = int.Parse(xelement3.GetAttribute("yposfill"));
				}
				PoiMapElement poiMapElement = new PoiMapElement(num, xelement3.GetAttribute("prefab"), blockValue, blockBelow, iSO, ypos, yposFill, num2);
				this.m_PoiMap.Add(num, poiMapElement);
				foreach (XElement xelement4 in xelement3.Elements())
				{
					if (xelement4.Name == "decal")
					{
						int texIndex = Convert.ToInt32(xelement4.GetAttribute("texture"));
						BlockFace face = (BlockFace)Convert.ToInt32(xelement4.GetAttribute("face"));
						float prob = StringParsers.ParseFloat(xelement4.GetAttribute("prob"), 0, -1, NumberStyles.Any);
						poiMapElement.decals.Add(new PoiMapDecal(texIndex, face, prob));
					}
					if (xelement4.Name == "blockontop")
					{
						blockValue = (_instantiateReferences ? this.getBlockValueForName(xelement4.GetAttribute("blockname")) : BlockValue.Air);
						float prob2 = StringParsers.ParseFloat(xelement4.GetAttribute("prob"), 0, -1, NumberStyles.Any);
						int offset = xelement4.HasAttribute("offset") ? int.Parse(xelement4.GetAttribute("offset")) : 0;
						poiMapElement.blocksOnTop.Add(new PoiMapBlock(blockValue, prob2, offset));
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition parseBiome(byte id, byte subId, string name, XElement biomeElement, bool _instantiateReferences)
	{
		uint color = 0U;
		if (biomeElement.HasAttribute("biomemapcolor"))
		{
			color = Convert.ToUInt32(biomeElement.GetAttribute("biomemapcolor").Substring(1), 16);
		}
		int radiationLevel = 0;
		if (biomeElement.HasAttribute("radiationlevel"))
		{
			radiationLevel = int.Parse(biomeElement.GetAttribute("radiationlevel"));
		}
		string topSoilBlock = null;
		if (biomeElement.HasAttribute("topsoil_block"))
		{
			topSoilBlock = biomeElement.GetAttribute("topsoil_block");
		}
		string buff = null;
		if (biomeElement.HasAttribute("buff"))
		{
			buff = biomeElement.GetAttribute("buff");
		}
		BiomeDefinition biomeDefinition = new BiomeDefinition(id, subId, name, color, radiationLevel, topSoilBlock, buff);
		if (biomeElement.HasAttribute("prob"))
		{
			biomeDefinition.prob = StringParsers.ParseFloat(biomeElement.GetAttribute("prob"), 0, -1, NumberStyles.Any);
		}
		if (biomeElement.HasAttribute("yless"))
		{
			biomeDefinition.yLT = int.Parse(biomeElement.GetAttribute("yless"));
		}
		if (biomeElement.HasAttribute("ygreater"))
		{
			biomeDefinition.yGT = int.Parse(biomeElement.GetAttribute("ygreater"));
		}
		if (biomeElement.HasAttribute("freq"))
		{
			biomeDefinition.freq = StringParsers.ParseFloat(biomeElement.GetAttribute("freq"), 0, -1, NumberStyles.Any);
		}
		if (biomeElement.HasAttribute("gamestage_modifier"))
		{
			biomeDefinition.GameStageMod = StringParsers.ParseFloat(biomeElement.GetAttribute("gamestage_modifier"), 0, -1, NumberStyles.Any);
		}
		if (biomeElement.HasAttribute("gamestage_bonus"))
		{
			biomeDefinition.GameStageBonus = StringParsers.ParseFloat(biomeElement.GetAttribute("gamestage_bonus"), 0, -1, NumberStyles.Any);
		}
		if (biomeElement.HasAttribute("lootstage_modifier"))
		{
			biomeDefinition.LootStageMod = StringParsers.ParseFloat(biomeElement.GetAttribute("lootstage_modifier"), 0, -1, NumberStyles.Any);
		}
		if (biomeElement.HasAttribute("lootstage_bonus"))
		{
			biomeDefinition.LootStageBonus = StringParsers.ParseFloat(biomeElement.GetAttribute("lootstage_bonus"), 0, -1, NumberStyles.Any);
		}
		if (biomeElement.HasAttribute("difficulty"))
		{
			biomeDefinition.Difficulty = StringParsers.ParseSInt32(biomeElement.GetAttribute("difficulty"), 0, -1, NumberStyles.Integer);
		}
		foreach (XElement xelement in biomeElement.Elements())
		{
			if (xelement.Name == "subbiome")
			{
				subId += 1;
				BiomeDefinition biomeDefinition2 = this.parseBiome(id, subId, name, xelement, _instantiateReferences);
				biomeDefinition.addSubBiome(biomeDefinition2);
				if (biomeDefinition2.m_DecoBlocks.Count == 0 && biomeDefinition2.m_DecoPrefabs.Count == 0)
				{
					biomeDefinition2.m_DecoBlocks = biomeDefinition.m_DecoBlocks;
					biomeDefinition2.m_DistantDecoBlocks = biomeDefinition.m_DistantDecoBlocks;
					biomeDefinition2.m_DecoPrefabs = biomeDefinition.m_DecoPrefabs;
				}
			}
			else if (xelement.Name == "terrain")
			{
				if (!xelement.HasAttribute("class"))
				{
					throw new Exception("Attribute class missing on terrain in biome " + name);
				}
				string attribute = xelement.GetAttribute("class");
				Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("TGM", attribute);
				if (typeWithPrefix != null)
				{
					TGMAbstract tgmabstract = (TGMAbstract)Activator.CreateInstance(typeWithPrefix);
					if (tgmabstract == null)
					{
						throw new Exception("Class '" + attribute + "' not found!");
					}
					foreach (XElement propertyNode in xelement.Elements("property"))
					{
						tgmabstract.properties.Add(propertyNode, true);
					}
					tgmabstract.Init();
					biomeDefinition.m_Terrain = tgmabstract;
				}
			}
			else if (xelement.Name == "spectrum")
			{
				if (xelement.HasAttribute("name"))
				{
					string attribute2 = xelement.GetAttribute("name");
					biomeDefinition.m_SpectrumName = attribute2;
				}
			}
			else
			{
				if (xelement.Name == "weather")
				{
					string name2 = "?";
					if (xelement.HasAttribute("name"))
					{
						name2 = xelement.GetAttribute("name");
					}
					float prob = 1f;
					if (xelement.HasAttribute("prob"))
					{
						prob = StringParsers.ParseFloat(xelement.GetAttribute("prob"), 0, -1, NumberStyles.Any);
					}
					string buff2 = "";
					if (xelement.HasAttribute("buff"))
					{
						buff2 = xelement.GetAttribute("buff");
					}
					BiomeDefinition.WeatherGroup weatherGroup = biomeDefinition.AddWeatherGroup(name2, prob, buff2);
					using (IEnumerator<XElement> enumerator2 = xelement.Elements().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							XElement xelement2 = enumerator2.Current;
							string localName = xelement2.Name.LocalName;
							if (localName.EqualsCaseInsensitive("temperature"))
							{
								float min = (xelement2.GetAttribute("min").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("min"), 0, -1, NumberStyles.Any) : -50f;
								float max = (xelement2.GetAttribute("max").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("max"), 0, -1, NumberStyles.Any) : 150f;
								float probability = (xelement2.GetAttribute("prob").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("prob"), 0, -1, NumberStyles.Any) : 1f;
								weatherGroup.AddProbability(BiomeDefinition.Probabilities.ProbType.Temperature, min, max, probability);
							}
							else if (localName.EqualsCaseInsensitive("cloudthickness"))
							{
								float min2 = (xelement2.GetAttribute("min").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("min"), 0, -1, NumberStyles.Any) : 0f;
								float max2 = (xelement2.GetAttribute("max").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("max"), 0, -1, NumberStyles.Any) : 100f;
								float probability2 = (xelement2.GetAttribute("prob").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("prob"), 0, -1, NumberStyles.Any) : 1f;
								weatherGroup.AddProbability(BiomeDefinition.Probabilities.ProbType.CloudThickness, min2, max2, probability2);
							}
							else if (localName.EqualsCaseInsensitive("precipitation"))
							{
								float min3 = (xelement2.GetAttribute("min").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("min"), 0, -1, NumberStyles.Any) : 0f;
								float max3 = (xelement2.GetAttribute("max").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("max"), 0, -1, NumberStyles.Any) : 100f;
								float probability3 = (xelement2.GetAttribute("prob").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("prob"), 0, -1, NumberStyles.Any) : 1f;
								weatherGroup.AddProbability(BiomeDefinition.Probabilities.ProbType.Precipitation, min3, max3, probability3);
							}
							else if (localName.EqualsCaseInsensitive("fog"))
							{
								float min4 = (xelement2.GetAttribute("min").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("min"), 0, -1, NumberStyles.Any) : 0f;
								float max4 = (xelement2.GetAttribute("max").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("max"), 0, -1, NumberStyles.Any) : 100f;
								float probability4 = (xelement2.GetAttribute("prob").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("prob"), 0, -1, NumberStyles.Any) : 1f;
								weatherGroup.AddProbability(BiomeDefinition.Probabilities.ProbType.Fog, min4, max4, probability4);
							}
							else if (localName.EqualsCaseInsensitive("wind"))
							{
								float min5 = (xelement2.GetAttribute("min").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("min"), 0, -1, NumberStyles.Any) : 0f;
								float max5 = (xelement2.GetAttribute("max").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("max"), 0, -1, NumberStyles.Any) : 100f;
								float probability5 = (xelement2.GetAttribute("prob").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("prob"), 0, -1, NumberStyles.Any) : 1f;
								weatherGroup.AddProbability(BiomeDefinition.Probabilities.ProbType.Wind, min5, max5, probability5);
							}
							else if (localName.EqualsCaseInsensitive("particleeffect"))
							{
								string prefabName = (xelement2.GetAttribute("prefab").Length > 0) ? xelement2.GetAttribute("prefab") : "error";
								int num = (int)((xelement2.GetAttribute("ChunkMargin").Length > 0) ? StringParsers.ParseFloat(xelement2.GetAttribute("ChunkMargin"), 0, -1, NumberStyles.Any) : 8f);
								BiomeParticleManager.RegisterEffect(name, prefabName, (float)num);
							}
							else if (localName.EqualsCaseInsensitive("spectrum"))
							{
								string attribute3 = xelement2.GetAttribute("name");
								weatherGroup.spectrum = EnumUtils.Parse<SpectrumWeatherType>(attribute3, SpectrumWeatherType.Biome, true);
							}
						}
						continue;
					}
				}
				if (xelement.Name == "layers")
				{
					using (IEnumerator<XElement> enumerator2 = xelement.Elements("layer").GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							XElement xelement3 = enumerator2.Current;
							int depth = -1;
							if (xelement3.HasAttribute("depth") && !xelement3.GetAttribute("depth").Equals("*"))
							{
								depth = int.Parse(xelement3.GetAttribute("depth"));
							}
							int fillupto = 0;
							if (xelement3.HasAttribute("fillupto"))
							{
								fillupto = int.Parse(xelement3.GetAttribute("fillupto"));
								depth = 0;
							}
							int filluptorg = 0;
							if (xelement3.HasAttribute("filluptorg"))
							{
								filluptorg = int.Parse(xelement3.GetAttribute("filluptorg"));
								depth = 0;
							}
							string attribute4 = xelement3.GetAttribute("blockname");
							BiomeLayer biomeLayer = new BiomeLayer(depth, fillupto, filluptorg, new BiomeBlockDecoration(attribute4, 1f, 1f, _instantiateReferences ? this.getBlockValueForName(attribute4) : BlockValue.Air, 0, int.MaxValue));
							biomeDefinition.AddLayer(biomeLayer);
							foreach (XElement element in xelement3.Descendants("resource"))
							{
								float prob2 = StringParsers.ParseFloat(element.GetAttribute("prob"), 0, -1, NumberStyles.Any);
								string text = Convert.ToString(element.GetAttribute("blockname"));
								BiomeBlockDecoration res = new BiomeBlockDecoration(text, prob2, 0f, _instantiateReferences ? this.getBlockValueForName(text) : BlockValue.Air, 0, int.MaxValue);
								biomeLayer.AddResource(res);
							}
						}
						continue;
					}
				}
				if (xelement.Name == "decorations")
				{
					using (IEnumerator<XElement> enumerator2 = xelement.Elements("decoration").GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							XElement element2 = enumerator2.Current;
							string attribute5 = element2.GetAttribute("type");
							if (attribute5.Equals("block"))
							{
								float prob3 = StringParsers.ParseFloat(element2.GetAttribute("prob"), 0, -1, NumberStyles.Any);
								float clusprob = 0f;
								string attribute6 = element2.GetAttribute("blockname");
								int num2 = element2.HasAttribute("rotatemax") ? int.Parse(element2.GetAttribute("rotatemax")) : 0;
								int checkResource = element2.HasAttribute("checkresource") ? int.Parse(element2.GetAttribute("checkresource")) : int.MaxValue;
								BlockValue blockValue = _instantiateReferences ? this.getBlockValueForName(attribute6) : BlockValue.Air;
								if (!blockValue.isair && blockValue.Block.isMultiBlock && (blockValue.Block.multiBlockPos.dim.x > 1 || blockValue.Block.multiBlockPos.dim.z > 1) && num2 > 3)
								{
									Log.Error("Parsing biomes. Block with name '" + attribute6 + "' supports only rotations 0-3, setting it to 3");
									num2 = 3;
								}
								biomeDefinition.AddDecoBlock(new BiomeBlockDecoration(attribute6, prob3, clusprob, blockValue, num2, checkResource));
							}
							else if (attribute5.Equals("prefab"))
							{
								float prob4 = StringParsers.ParseFloat(element2.GetAttribute("prob"), 0, -1, NumberStyles.Any);
								string attribute7 = element2.GetAttribute("name");
								if (string.IsNullOrEmpty(attribute7))
								{
									throw new Exception("Parsing biomes. No model name specified on prefab in biome '" + name + "'");
								}
								int checkResource2 = element2.HasAttribute("checkresource") ? int.Parse(element2.GetAttribute("checkresource")) : 10000;
								bool isDecorateOnSlopes = element2.HasAttribute("onslopes") && StringParsers.ParseBool(element2.GetAttribute("onslopes"), 0, -1, true);
								biomeDefinition.AddDecoPrefab(new BiomePrefabDecoration(attribute7, prob4, isDecorateOnSlopes, checkResource2));
							}
							else
							{
								if (!attribute5.Equals("terrain"))
								{
									throw new Exception("Unknown decoration type " + attribute5);
								}
								float prob5 = StringParsers.ParseFloat(element2.GetAttribute("prob"), 0, -1, NumberStyles.Any);
								string attribute8 = element2.GetAttribute("name");
								if (string.IsNullOrEmpty(attribute8))
								{
									throw new Exception("Parsing biomes. No name specified on terrain in biome '" + name + "'");
								}
								if (_instantiateReferences && !SdFile.Exists(GameIO.GetGameDir("Data/Bluffs") + "/" + attribute8 + ".tga"))
								{
									throw new Exception("Parsing biomes. Prefab with name '" + attribute8 + ".tga' not found!");
								}
								float minScale = 1f;
								float maxScale = 1f;
								string text2 = element2.HasAttribute("scale") ? element2.GetAttribute("scale") : null;
								if (text2 != null && text2.IndexOf(',') > 0)
								{
									string[] array = text2.Split(',', StringSplitOptions.None);
									minScale = StringParsers.ParseFloat(array[0], 0, -1, NumberStyles.Any);
									maxScale = StringParsers.ParseFloat(array[1], 0, -1, NumberStyles.Any);
								}
								else if (text2 != null)
								{
									maxScale = (minScale = StringParsers.ParseFloat(text2, 0, -1, NumberStyles.Any));
								}
								biomeDefinition.AddBluff(new BiomeBluffDecoration(attribute8, prob5, minScale, maxScale));
							}
						}
						continue;
					}
				}
				if (xelement.Name == "replacements")
				{
					foreach (XElement element3 in xelement.Elements("replace"))
					{
						string attribute9 = element3.GetAttribute("source");
						string attribute10 = element3.GetAttribute("target");
						biomeDefinition.AddReplacement(Block.GetBlockValue(attribute9, false).type, Block.GetBlockValue(attribute10, false).type);
					}
				}
			}
		}
		biomeDefinition.SetupWeather();
		return biomeDefinition;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue getBlockValueForName(string blockname)
	{
		ItemValue item = ItemClass.GetItem(blockname, false);
		if (item.IsEmpty())
		{
			throw new Exception("Block with name '" + blockname + "' not found!");
		}
		return item.ToBlockValue();
	}

	public PoiMapElement getPoiForColor(uint uiColor)
	{
		PoiMapElement result;
		if (this.m_PoiMap.TryGetValue(uiColor, out result))
		{
			return result;
		}
		return null;
	}

	public void AddPoiMapElement(PoiMapElement _newElement)
	{
		if (!this.m_PoiMap.ContainsKey(_newElement.m_uColorId))
		{
			this.m_PoiMap.Add(_newElement.m_uColorId, _newElement);
		}
	}

	public int GetTotalBluffsCount()
	{
		int num = 0;
		for (int i = 0; i < this.m_Id2BiomeArr.Length; i++)
		{
			if (this.m_Id2BiomeArr[i] != null)
			{
				num += this.m_Id2BiomeArr[i].m_DecoBluffs.Count;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<uint, BiomeDefinition> m_Color2BiomeMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition[] m_Id2BiomeArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, BiomeDefinition> m_Name2BiomeMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<uint, PoiMapElement> m_PoiMap;

	public static WorldBiomes Instance;
}
