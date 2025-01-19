using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BiomeDefinition
{
	public static string LocalizedBiomeName(BiomeDefinition.BiomeType _biomeType)
	{
		return Localization.Get("biome_" + _biomeType.ToStringCached<BiomeDefinition.BiomeType>(), false);
	}

	public event BiomeDefinition.OnWeatherChanged WeatherChanged;

	public BiomeDefinition(byte _id, byte _subId, string _name, uint _color, int _radiationLevel, string _topSoilBlock, string _buff)
	{
		this.m_Id = _id;
		this.m_BiomeType = (BiomeDefinition.BiomeType)(Enum.IsDefined(typeof(BiomeDefinition.BiomeType), (int)this.m_Id) ? this.m_Id : 0);
		this.subId = _subId;
		this.m_sBiomeName = _name;
		this.LocalizedName = Localization.Get("biome_" + _name, false);
		this.m_SpectrumName = this.m_sBiomeName;
		this.m_uiColor = _color;
		this.m_RadiationLevel = _radiationLevel;
		this.m_Terrain = null;
		this.m_Layers = new List<BiomeLayer>();
		this.m_DecoBlocks = new List<BiomeBlockDecoration>();
		this.m_DistantDecoBlocks = new List<BiomeBlockDecoration>();
		this.m_DecoPrefabs = new List<BiomePrefabDecoration>();
		this.m_DecoBluffs = new List<BiomeBluffDecoration>();
		this.m_TopSoilBlock = _topSoilBlock;
		this.Buff = _buff;
		this.InitWeather();
	}

	public void AddLayer(BiomeLayer _layer)
	{
		this.m_Layers.Add(_layer);
		this.TotalLayerDepth += _layer.m_Depth;
	}

	public void AddDecoBlock(BiomeBlockDecoration _deco)
	{
		if (Block.BlocksLoaded && _deco.blockValue.Block != null && _deco.blockValue.Block.IsDistantDecoration)
		{
			this.m_DistantDecoBlocks.Add(_deco);
		}
		this.m_DecoBlocks.Add(_deco);
	}

	public void AddDecoPrefab(BiomePrefabDecoration _deco)
	{
		this.m_DecoPrefabs.Add(_deco);
	}

	public void AddBluff(BiomeBluffDecoration _deco)
	{
		this.m_DecoBluffs.Add(_deco);
	}

	public void AddReplacement(int _sourceId, int _targetId)
	{
		this.Replacements[_sourceId] = _targetId;
	}

	public void addSubBiome(BiomeDefinition _subbiome)
	{
		this.subbiomes.Add(_subbiome);
	}

	public override bool Equals(object obj)
	{
		return obj is BiomeDefinition && ((BiomeDefinition)obj).m_Id == this.m_Id;
	}

	public override int GetHashCode()
	{
		return (int)this.m_Id;
	}

	public override string ToString()
	{
		return this.m_sBiomeName;
	}

	public static uint GetBiomeColor(BiomeDefinition.BiomeType _type)
	{
		return BiomeDefinition.BiomeColors[(int)_type];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitWeather()
	{
		for (int i = 0; i < 5; i++)
		{
			this.weatherGroupsMinPossible[i] = float.MaxValue;
			this.weatherGroupsMaxPossible[i] = float.MinValue;
		}
	}

	public BiomeDefinition.WeatherGroup AddWeatherGroup(string _name, float _prob, string _buff)
	{
		BiomeDefinition.WeatherGroup weatherGroup = new BiomeDefinition.WeatherGroup();
		weatherGroup.name = _name;
		weatherGroup.prob = _prob;
		weatherGroup.buffName = _buff;
		this.weatherGroups.Add(weatherGroup);
		return weatherGroup;
	}

	public void SetupWeather()
	{
		float num = 0f;
		for (int i = 0; i < this.weatherGroups.Count; i++)
		{
			BiomeDefinition.WeatherGroup weatherGroup = this.weatherGroups[i];
			num += weatherGroup.prob;
			weatherGroup.probabilities.Normalize();
			for (int j = 0; j < 5; j++)
			{
				Vector2 vector = weatherGroup.probabilities.CalcMinMaxPossibleValue((BiomeDefinition.Probabilities.ProbType)j);
				if (vector.x < this.weatherGroupsMinPossible[j])
				{
					this.weatherGroupsMinPossible[j] = vector.x;
				}
				if (vector.y > this.weatherGroupsMaxPossible[j])
				{
					this.weatherGroupsMaxPossible[j] = vector.y;
				}
			}
		}
		num += 1E-06f;
		for (int k = 0; k < this.weatherGroups.Count; k++)
		{
			this.weatherGroups[k].prob /= num;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float WeatherGetMinPossibleValue(BiomeDefinition.Probabilities.ProbType _type)
	{
		return this.weatherGroupsMinPossible[(int)_type];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float WeatherGetMaxPossibleValue(BiomeDefinition.Probabilities.ProbType _type)
	{
		return this.weatherGroupsMaxPossible[(int)_type];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float WeatherClampToPossibleValues(float _value, BiomeDefinition.Probabilities.ProbType _type)
	{
		return Utils.FastClamp(_value, this.weatherGroupsMinPossible[(int)_type], this.weatherGroupsMaxPossible[(int)_type]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float WeatherClampToPossibleValues(float _value, float _offset, BiomeDefinition.Probabilities.ProbType _type)
	{
		return Utils.FastClamp(_value, _offset + this.weatherGroupsMinPossible[(int)_type], _offset + this.weatherGroupsMaxPossible[(int)_type]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float WeatherGetValue(BiomeDefinition.Probabilities.ProbType _type)
	{
		return this.weatherValues[(int)_type];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WeatherSetValue(BiomeDefinition.Probabilities.ProbType _type, float _value)
	{
		this.weatherValues[(int)_type] = _value;
	}

	public void WeatherRandomize(float _rand)
	{
		if (SkyManager.IsBloodMoonVisible())
		{
			BiomeDefinition.WeatherGroup weatherGroup = this.FindWeatherGroup("bloodMoon");
			if (weatherGroup != null)
			{
				this.SelectWeatherGroup(weatherGroup);
				return;
			}
		}
		float num = 0f;
		for (int i = 0; i < this.weatherGroups.Count; i++)
		{
			BiomeDefinition.WeatherGroup weatherGroup2 = this.weatherGroups[i];
			num += weatherGroup2.prob;
			if (_rand < num)
			{
				this.SelectWeatherGroup(weatherGroup2);
				return;
			}
		}
	}

	public void WeatherRandomize(string weatherGroup)
	{
		BiomeDefinition.WeatherGroup weatherGroup2 = this.FindWeatherGroup(weatherGroup);
		if (weatherGroup2 != null)
		{
			this.SelectWeatherGroup(weatherGroup2);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition.WeatherGroup FindWeatherGroup(string _name)
	{
		for (int i = 0; i < this.weatherGroups.Count; i++)
		{
			BiomeDefinition.WeatherGroup weatherGroup = this.weatherGroups[i];
			if (weatherGroup.name == _name)
			{
				return weatherGroup;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SelectWeatherGroup(BiomeDefinition.WeatherGroup _wg)
	{
		this.weatherName = _wg.name;
		this.weatherSpectrum = _wg.spectrum;
		if (this.WeatherChanged != null)
		{
			this.WeatherChanged(this.currentWeather, _wg);
		}
		this.currentWeather = _wg;
		for (int i = 0; i < 5; i++)
		{
			float randomValue = _wg.probabilities.GetRandomValue((BiomeDefinition.Probabilities.ProbType)i);
			this.weatherValues[i] = randomValue;
		}
	}

	public WeatherPackage weatherPackage = new WeatherPackage();

	public float currentPlayerIntensity;

	public const string BiomeNameLocalizationPrefix = "biome_";

	public static string[] BiomeNames = new string[]
	{
		"any",
		"snow",
		"forest",
		"pine_forest",
		"plains",
		"desert",
		"water",
		"radiated",
		"wasteland",
		"burnt_forest",
		"city",
		"city_wasteland",
		"wasteland_hub",
		"caveFloor",
		"caveCeiling"
	};

	public static uint[] BiomeColors = new uint[]
	{
		0U,
		16777215U,
		0U,
		16384U,
		0U,
		16770167U,
		25599U,
		0U,
		16754688U,
		12189951U,
		8421504U,
		12632256U,
		10526880U,
		0U,
		0U
	};

	public readonly byte m_Id;

	public readonly BiomeDefinition.BiomeType m_BiomeType;

	public byte subId;

	public readonly string m_sBiomeName;

	public readonly string LocalizedName;

	public uint m_uiColor;

	public int m_RadiationLevel;

	public string m_SpectrumName;

	public static Dictionary<string, byte> nameToId;

	public List<BiomeLayer> m_Layers;

	public List<BiomeBlockDecoration> m_DecoBlocks;

	public List<BiomeBlockDecoration> m_DistantDecoBlocks;

	public List<BiomePrefabDecoration> m_DecoPrefabs;

	public List<BiomeBluffDecoration> m_DecoBluffs;

	public List<BiomeDefinition.WeatherGroup> weatherGroups = new List<BiomeDefinition.WeatherGroup>();

	public string weatherName;

	public SpectrumWeatherType weatherSpectrum;

	public BiomeDefinition.WeatherGroup currentWeather;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] weatherGroupsMinPossible = new float[5];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] weatherGroupsMaxPossible = new float[5];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] weatherValues = new float[5];

	public TGMAbstract m_Terrain;

	public int TotalLayerDepth;

	public List<BiomeDefinition> subbiomes = new List<BiomeDefinition>();

	public float freq = 0.03f;

	public float prob;

	public int yLT;

	public int yGT;

	public Dictionary<int, int> Replacements = new Dictionary<int, int>();

	public string m_TopSoilBlock;

	public float GameStageMod;

	public float GameStageBonus;

	public float LootStageMod;

	public float LootStageBonus;

	public string Buff;

	public int Difficulty = 1;

	public delegate void OnWeatherChanged(BiomeDefinition.WeatherGroup _oldWeather, BiomeDefinition.WeatherGroup _newWeather);

	public enum BiomeType
	{
		Any,
		Snow,
		Forest,
		PineForest,
		Plains,
		Desert,
		Water,
		Radiated,
		Wasteland,
		burnt_forest,
		city,
		city_wasteland,
		wasteland_hub,
		caveFloor,
		caveCeiling
	}

	public class Probabilities
	{
		public Probabilities()
		{
			this.probabilities = new List<Vector3>[5];
			for (int i = 0; i < 5; i++)
			{
				this.probabilities[i] = new List<Vector3>();
			}
		}

		public void AddProbability(BiomeDefinition.Probabilities.ProbType _type, float _min, float _max, float _probability)
		{
			this.probabilities[(int)_type].Add(new Vector3(_min, _max, _probability));
		}

		public Vector2 CalcMinMaxPossibleValue(BiomeDefinition.Probabilities.ProbType type)
		{
			Vector2 vector = new Vector2(float.MaxValue, float.MinValue);
			List<Vector3> list = this.probabilities[(int)type];
			for (int i = 0; i < list.Count; i++)
			{
				Vector3 vector2 = list[i];
				if (vector2.x < vector.x)
				{
					vector.x = vector2.x;
				}
				if (vector2.y > vector.y)
				{
					vector.y = vector2.y;
				}
			}
			return vector;
		}

		public float GetRandomValue(BiomeDefinition.Probabilities.ProbType _type)
		{
			GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
			float randomFloat = gameRandom.RandomFloat;
			float num = 0f;
			List<Vector3> list = this.probabilities[(int)_type];
			for (int i = 0; i < list.Count; i++)
			{
				Vector3 vector = list[i];
				num += vector.z;
				if (randomFloat < num)
				{
					float randomFloat2 = gameRandom.RandomFloat;
					return vector.x * randomFloat2 + vector.y * (1f - randomFloat2);
				}
			}
			return 0f;
		}

		public void Normalize()
		{
			for (int i = 0; i < 5; i++)
			{
				List<Vector3> list = this.probabilities[i];
				float num = 0f;
				for (int j = 0; j < list.Count; j++)
				{
					num += list[j].z;
				}
				for (int k = 0; k < list.Count; k++)
				{
					Vector3 value = list[k];
					value.z /= num;
					list[k] = value;
				}
			}
		}

		public const int ProbTypeCount = 5;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Vector3>[] probabilities;

		public enum ProbType
		{
			Temperature,
			Precipitation,
			CloudThickness,
			Wind,
			Fog,
			Count
		}
	}

	public class WeatherGroup
	{
		public void AddProbability(BiomeDefinition.Probabilities.ProbType _type, float _min, float _max, float _probability)
		{
			this.probabilities.AddProbability(_type, _min, _max, _probability);
		}

		public string name;

		public float prob;

		public string buffName;

		public SpectrumWeatherType spectrum;

		public BiomeDefinition.Probabilities probabilities = new BiomeDefinition.Probabilities();
	}
}
