using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
	public static void Init(World _world, GameObject _obj)
	{
		WeatherManager.Cleanup();
		WeatherManager.Instance = _obj.GetComponent<WeatherManager>();
		WeatherManager.Instance.Init(_world);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init(World _world)
	{
		this.world = _world;
		string @string = GamePrefs.GetString(EnumGamePrefs.GameMode);
		this.isGameModeNormal = (!GameModeEditWorld.TypeName.Equals(@string) && !GameModeCreative.TypeName.Equals(@string));
		this.InitBiomeWeather();
		this.InitWeatherPackages();
		WeatherManager.currentWeather = new WeatherManager.BiomeWeather(null);
		WeatherManager.currentWeather.biomeDefinition = this.world.Biomes.GetBiome(3);
		WeatherManager.currentWeather.parameters[2].name = "CloudThickness";
		WeatherManager.currentWeather.parameters[4].name = "Fog";
		WeatherManager.currentWeather.parameters[1].name = "Precipitation";
		WeatherManager.currentWeather.parameters[0].name = "Temperature";
		WeatherManager.currentWeather.parameters[3].name = "Wind";
		WeatherManager.currentWeather.rainParam.name = "Rain";
		WeatherManager.currentWeather.wetParam.name = "Wet";
		WeatherManager.currentWeather.snowCoverParam.name = "SnowCover";
		WeatherManager.currentWeather.snowFallParam.name = "SnowFall";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitBiomeWeather()
	{
		this.biomeWeather = new List<WeatherManager.BiomeWeather>();
		foreach (KeyValuePair<uint, BiomeDefinition> keyValuePair in this.world.Biomes.GetBiomeMap())
		{
			WeatherManager.BiomeWeather item = new WeatherManager.BiomeWeather(keyValuePair.Value);
			this.biomeWeather.Add(item);
		}
		for (int i = 0; i < this.biomeWeather.Count; i++)
		{
			this.biomeWeather[i].Reset();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitWeatherPackages()
	{
		int count = this.world.Biomes.GetBiomeMap().Count;
		Log.Out("WeatherManager: Init {0} weather packages", new object[]
		{
			count
		});
		this.weatherPackages = new WeatherPackage[count];
		for (int i = 0; i < count; i++)
		{
			this.weatherPackages[i] = new WeatherPackage();
		}
	}

	public static void Cleanup()
	{
		if (WeatherManager.Instance != null)
		{
			UnityEngine.Object.DestroyImmediate(WeatherManager.Instance.gameObject);
			WeatherManager.forceClouds = -1f;
			WeatherManager.forceRain = -1f;
			WeatherManager.forceWet = -1f;
			WeatherManager.forceSnow = -1f;
			WeatherManager.forceSnowfall = -1f;
			WeatherManager.forceTemperature = -100f;
			WeatherManager.forceWind = -1f;
		}
		WeatherManager.players.Clear();
	}

	public static void ClearTemperatureOffSetHeights()
	{
		WeatherManager.temperatureOffsetHeights.Clear();
	}

	public static void AddTemperatureOffSetHeight(float height, float degreesOffset)
	{
		WeatherManager.temperatureOffsetHeights.Add(new Vector2(height, degreesOffset));
		IComparer<Vector2> comparer = new WeatherManager.temperatureOffsetHeightsComparer();
		WeatherManager.temperatureOffsetHeights.Sort(comparer);
	}

	public static float SeaLevel()
	{
		if (!WeatherManager.hasCreatedSeaLevel)
		{
			if (WeatherManager.temperatureOffsetHeights.Count < 1)
			{
				return 0f;
			}
			WeatherManager.hasCreatedSeaLevel = true;
			int num = 0;
			for (int i = 1; i < WeatherManager.temperatureOffsetHeights.Count; i++)
			{
				if (Mathf.Abs(WeatherManager.temperatureOffsetHeights[i].y) < Mathf.Abs(WeatherManager.temperatureOffsetHeights[num].y))
				{
					num = i;
				}
			}
			if (WeatherManager.temperatureOffsetHeights[num].y < 0f && num < WeatherManager.temperatureOffsetHeights.Count - 1)
			{
				float num2 = Mathf.Abs(WeatherManager.temperatureOffsetHeights[num].y) / (WeatherManager.temperatureOffsetHeights[num + 1].y + Mathf.Abs(WeatherManager.temperatureOffsetHeights[num].y));
				WeatherManager.seaLevel = WeatherManager.temperatureOffsetHeights[num].x * (1f - num2) + WeatherManager.temperatureOffsetHeights[num + 1].x * num2;
			}
			else if (WeatherManager.temperatureOffsetHeights[num].y < 0f)
			{
				WeatherManager.seaLevel = WeatherManager.temperatureOffsetHeights[num].x + Mathf.Abs(WeatherManager.temperatureOffsetHeights[num].y);
			}
			else if (WeatherManager.temperatureOffsetHeights[num].y >= 0f && num > 0)
			{
				float num3 = Mathf.Abs(WeatherManager.temperatureOffsetHeights[num - 1].y) / (Mathf.Abs(WeatherManager.temperatureOffsetHeights[num - 1].y) + WeatherManager.temperatureOffsetHeights[num].y);
				WeatherManager.seaLevel = WeatherManager.temperatureOffsetHeights[num - 1].x * (1f - num3) + WeatherManager.temperatureOffsetHeights[num].x * num3;
			}
			else if (WeatherManager.temperatureOffsetHeights[num].y >= 0f)
			{
				WeatherManager.seaLevel = WeatherManager.temperatureOffsetHeights[num].x - WeatherManager.temperatureOffsetHeights[num].y;
			}
		}
		return WeatherManager.seaLevel;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcGlobalPrecipCloudsTemperature(ref float _precipitation, ref float _cloudThickness, ref float _temperature)
	{
		_precipitation += WeatherManager.globalRainPercent * 50f;
		_cloudThickness += WeatherManager.globalRainPercent * 50f;
		_precipitation = Mathf.Clamp(_precipitation, 0f, 100f);
		_cloudThickness = Mathf.Clamp(_cloudThickness, 0f, 100f);
		_temperature += WeatherManager.globalTemperatureOffset;
		_temperature -= _precipitation * 0.1f;
		float num = SkyManager.GetSunPercent();
		if (num > 0f)
		{
			num *= 1f - _cloudThickness * 0.01f;
		}
		_temperature += num * 20f;
	}

	public static float GetCloudThickness()
	{
		if (WeatherManager.forceClouds >= 0f)
		{
			return WeatherManager.forceClouds * 100f;
		}
		if (WeatherManager.currentWeather == null)
		{
			return 0f;
		}
		return WeatherManager.currentWeather.parameters[2].value;
	}

	public static float GetTemperature()
	{
		if (WeatherManager.forceTemperature > -100f)
		{
			return WeatherManager.forceTemperature;
		}
		if (WeatherManager.currentWeather == null)
		{
			return 0f;
		}
		return WeatherManager.currentWeather.parameters[0].value;
	}

	public static float GetWindSpeed()
	{
		if (WeatherManager.forceWind >= 0f)
		{
			return WeatherManager.forceWind;
		}
		if (WeatherManager.currentWeather == null)
		{
			return 0f;
		}
		return WeatherManager.currentWeather.parameters[3].value;
	}

	public static void EntityAddedToWorld(Entity entity)
	{
		if (entity != null)
		{
			WeatherManager.players.Add(entity);
		}
	}

	public static void EntityRemovedFromWorld(Entity entity)
	{
		if (entity != null)
		{
			WeatherManager.players.Remove(entity);
		}
	}

	public float GetCurrentSnowfallValue()
	{
		if (WeatherManager.forceSnowfall < 0f && WeatherManager.currentWeather != null)
		{
			return WeatherManager.currentWeather.snowFallParam.value;
		}
		return WeatherManager.forceSnowfall;
	}

	public static float GetCurrentSnowValue()
	{
		if (WeatherManager.forceSnow < 0f && WeatherManager.currentWeather != null)
		{
			return WeatherManager.currentWeather.snowCoverParam.value;
		}
		return WeatherManager.forceSnow;
	}

	public float GetCurrentRainfallValue()
	{
		if (WeatherManager.forceRain < 0f && WeatherManager.currentWeather != null)
		{
			return WeatherManager.currentWeather.rainParam.value;
		}
		return WeatherManager.forceRain;
	}

	public float GetCurrentWetSurfaceValue()
	{
		if (WeatherManager.forceWet < 0f && WeatherManager.currentWeather != null)
		{
			return WeatherManager.currentWeather.wetParam.value;
		}
		return WeatherManager.forceWet;
	}

	public float GetCurrentCloudThicknessPercent()
	{
		return WeatherManager.GetCloudThickness() * 0.01f;
	}

	public float GetCurrentTemperatureValue()
	{
		return WeatherManager.GetTemperature();
	}

	public static void SetSimRandom(float _random)
	{
		WeatherManager.forceSimRandom = _random;
		if (WeatherManager.Instance)
		{
			WeatherManager.Instance.lastWorldTimeWeatherChanged = 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadSpectrums()
	{
		if (WeatherManager.atmosphereSpectrum == null)
		{
			WeatherManager.atmosphereSpectrum = new AtmosphereEffect[Enum.GetNames(typeof(SpectrumWeatherType)).Length - 1];
			WeatherManager.ReloadSpectrums();
		}
	}

	public static void ReloadSpectrums()
	{
		WeatherManager.atmosphereSpectrum[1] = AtmosphereEffect.Load("Snowy", null);
		WeatherManager.atmosphereSpectrum[2] = AtmosphereEffect.Load("Stormy", null);
		WeatherManager.atmosphereSpectrum[3] = AtmosphereEffect.Load("Rainy", null);
		WeatherManager.atmosphereSpectrum[4] = AtmosphereEffect.Load("Foggy", null);
		WeatherManager.atmosphereSpectrum[5] = AtmosphereEffect.Load("BloodMoon", null);
	}

	public void Start()
	{
		this.windZoneObj = GameObject.Find("WindZone");
		if (this.windZoneObj)
		{
			this.windZone = this.windZoneObj.GetComponent<WindZone>();
		}
		WeatherManager.LoadSpectrums();
		this.raycastMask = (LayerMask.GetMask(new string[]
		{
			"Water",
			"NoShadow",
			"Items",
			"CC Physics",
			"TerrainCollision",
			"CC Physics Dead",
			"CC Local Physics"
		}) | 1);
		this.noiseTexture = Resources.Load<Texture>("Textures/Graphics/StipplingNoise");
		this.snowTexture = Resources.Load<Texture>("Textures/Graphics/Snow_n");
		string str = "Textures/Environment/Spectrums/Default/";
		for (int i = 0; i < WeatherManager.strCloudTypes.Length; i++)
		{
			Texture texture = Resources.Load<Texture>(str + WeatherManager.strCloudTypes[i] + "Clouds");
			WeatherManager.clouds[i] = texture;
		}
	}

	public void PushTransitions()
	{
		this.spectrumBlend = 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CurrentWeatherFromNearBiomesFrameUpdate()
	{
		if (this.world.BiomeAtmosphereEffects != null)
		{
			BiomeDefinition[] nearBiomes = this.world.BiomeAtmosphereEffects.nearBiomes;
			for (int i = 0; i < WeatherManager.currentWeather.parameters.Length; i++)
			{
				WeatherManager.currentWeather.parameters[i].target = 0f;
			}
			WeatherManager.currentWeather.rainParam.target = 0f;
			WeatherManager.currentWeather.snowFallParam.target = 0f;
			WeatherManager.currentWeather.wetParam.target = 0f;
			WeatherManager.currentWeather.snowCoverParam.target = 0f;
			float num = 0f;
			for (int j = 0; j < nearBiomes.Length; j++)
			{
				BiomeDefinition biomeDefinition = nearBiomes[j];
				WeatherManager.CurrentBiome currentBiome = this.editorNearBiomes[j];
				if (biomeDefinition != null)
				{
					num += biomeDefinition.currentPlayerIntensity;
					currentBiome.name = biomeDefinition.m_sBiomeName;
					currentBiome.intensity = biomeDefinition.currentPlayerIntensity;
				}
				else
				{
					currentBiome.name = "null";
					currentBiome.intensity = 0f;
				}
			}
			WeatherManager.inWeatherGracePeriod = ((WeatherManager.worldTime < 30000UL || !this.isGameModeNormal) && this.CustomWeatherTime == -1f);
			if (WeatherManager.inWeatherGracePeriod)
			{
				WeatherManager.currentWeather.rainParam.Set(0f);
				WeatherManager.currentWeather.snowFallParam.Set(0f);
				WeatherManager.currentWeather.wetParam.Set(0f);
				WeatherManager.currentWeather.snowCoverParam.Set(0f);
				int num2 = 0;
				float num3 = 0f;
				foreach (BiomeDefinition biomeDefinition2 in nearBiomes)
				{
					if (biomeDefinition2 != null && biomeDefinition2.currentPlayerIntensity > num3)
					{
						num3 = biomeDefinition2.currentPlayerIntensity;
						num2 = (int)biomeDefinition2.m_Id;
					}
				}
				int num4 = 70;
				if (num2 != 1)
				{
					if (num2 - 2 <= 1)
					{
						num4 = 60;
					}
				}
				else
				{
					num4 = 45;
				}
				WeatherManager.currentWeather.parameters[0].Set((float)num4);
				WeatherManager.currentWeather.parameters[3].Set(8f);
				return;
			}
			WeatherManager.currentWeather.biomeDefinition = nearBiomes[0];
			foreach (BiomeDefinition biomeDefinition3 in nearBiomes)
			{
				if (biomeDefinition3 != null)
				{
					BiomeDefinition biomeDefinition4;
					if (!WorldBiomes.Instance.TryGetBiome(biomeDefinition3.m_Id, out biomeDefinition4))
					{
						biomeDefinition4 = biomeDefinition3;
					}
					float num5 = Mathf.Clamp01(biomeDefinition3.currentPlayerIntensity / num);
					for (int m = 0; m < WeatherManager.currentWeather.parameters.Length; m++)
					{
						WeatherManager.Param param = WeatherManager.currentWeather.parameters[m];
						param.target += biomeDefinition4.weatherPackage.param[m] * num5;
						if (!this.isCurrentWeatherUpdatedFirstTime)
						{
							param.value = param.target;
						}
					}
					WeatherManager.currentWeather.rainParam.target += biomeDefinition4.weatherPackage.particleRain * num5;
					WeatherManager.currentWeather.wetParam.target += biomeDefinition4.weatherPackage.surfaceWet * num5;
					WeatherManager.currentWeather.snowCoverParam.target += biomeDefinition4.weatherPackage.surfaceSnow * num5;
					WeatherManager.currentWeather.snowFallParam.target += biomeDefinition4.weatherPackage.particleSnow * num5;
					if (!this.isCurrentWeatherUpdatedFirstTime)
					{
						WeatherManager.currentWeather.rainParam.value = Mathf.Clamp01(WeatherManager.currentWeather.rainParam.target);
						WeatherManager.currentWeather.wetParam.value = Mathf.Clamp01(WeatherManager.currentWeather.wetParam.target);
						WeatherManager.currentWeather.snowCoverParam.value = Mathf.Clamp01(WeatherManager.currentWeather.snowCoverParam.target);
						WeatherManager.currentWeather.snowFallParam.value = Mathf.Clamp01(WeatherManager.currentWeather.snowFallParam.target);
					}
					WeatherManager.currentWeather.Normalize();
				}
			}
			this.isCurrentWeatherUpdatedFirstTime = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplySavedPackages()
	{
		Log.Out("WeatherManager: ApplySavedPackages");
		foreach (WeatherPackage weatherPackage in WeatherManager.savedWeather)
		{
			foreach (WeatherManager.BiomeWeather biomeWeather in this.biomeWeather)
			{
				if (weatherPackage.biomeID == biomeWeather.biomeDefinition.m_Id)
				{
					for (int i = 0; i < 5; i++)
					{
						biomeWeather.biomeDefinition.WeatherSetValue((BiomeDefinition.Probabilities.ProbType)i, weatherPackage.param[i]);
					}
					for (int j = 0; j < weatherPackage.param.Length; j++)
					{
						biomeWeather.parameters[j].Set(weatherPackage.param[j]);
					}
					biomeWeather.rainParam.Set(weatherPackage.particleRain);
					biomeWeather.snowFallParam.Set(weatherPackage.particleSnow);
					biomeWeather.wetParam.Set(weatherPackage.surfaceWet);
					biomeWeather.snowCoverParam.Set(weatherPackage.surfaceSnow);
					BiomeDefinition biomeDefinition;
					if (WorldBiomes.Instance.TryGetBiome(weatherPackage.biomeID, out biomeDefinition))
					{
						biomeDefinition.weatherPackage = weatherPackage;
						break;
					}
					break;
				}
			}
		}
		WeatherManager.savedWeather.Clear();
	}

	public static void SetSnowAccumulationSpeed(float _newSpeed)
	{
		WeatherManager.sSnowAccumulationSpeed = _newSpeed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateWeatherServerFrameUpdate()
	{
		if (WeatherManager.inWeatherGracePeriod)
		{
			WeatherManager.GeneralReset();
		}
		else
		{
			float num = (float)(WeatherManager.worldTime - this.temperatureStartWorldTime) / 5000f;
			WeatherManager.globalTemperatureOffset = Mathf.Lerp(this.temperatureStart, this.temperatureTarget, num);
			if (num < 0f || num > 1f)
			{
				this.temperatureStartWorldTime = WeatherManager.worldTime;
				this.temperatureStart = WeatherManager.globalTemperatureOffset;
				this.temperatureTarget = this.world.RandomRange(-5f, 5f);
			}
			if (num < 0f)
			{
				WeatherManager.VersionReset();
			}
			float num2 = SkyManager.dayCount - WeatherManager.globalRainDayStart;
			if (num2 >= 0f)
			{
				WeatherManager.globalRainPercent = num2 / (WeatherManager.globalRainDayPeak - WeatherManager.globalRainDayStart);
				float num3 = SkyManager.dayCount - WeatherManager.globalRainDayPeak;
				if (num3 >= 0f)
				{
					WeatherManager.globalRainPercent = 1f - num3 / 0.0291666668f;
					if (WeatherManager.globalRainPercent <= 0f)
					{
						WeatherManager.globalRainPercent = 0f;
						WeatherManager.globalRainDayStart = SkyManager.dayCount + this.world.RandomRange(0.375f, 0.6666667f);
						WeatherManager.globalRainDayPeak = WeatherManager.globalRainDayStart + this.world.RandomRange(0.020833334f, 0.09583333f);
					}
				}
			}
			WeatherManager.isThunderWeather = SkyManager.IsBloodMoonVisible();
			if (this.CustomWeatherTime > 0f)
			{
				this.CustomWeatherTime -= Time.deltaTime;
				if (this.CustomWeatherTime <= 0f)
				{
					this.CustomWeatherName = "";
					this.CustomWeatherTime = -1f;
					WeatherManager.GeneralReset();
					for (int i = 0; i < this.biomeWeather.Count; i++)
					{
						this.biomeWeather[i].ForceWeather("default");
					}
					this.lastWorldTimeWeatherChanged = WeatherManager.worldTime;
				}
			}
			else if (Utils.FastAbs(WeatherManager.worldTime - this.lastWorldTimeWeatherChanged) >= 1500f)
			{
				this.lastWorldTimeWeatherChanged = WeatherManager.worldTime;
				for (int j = 0; j < this.biomeWeather.Count; j++)
				{
					this.biomeWeather[j].Randomize();
				}
			}
		}
		for (int k = 0; k < this.biomeWeather.Count; k++)
		{
			this.biomeWeather[k].ServerFrameUpdate();
		}
	}

	public void HandleBiomeChanging(EntityPlayer _player, BiomeDefinition _oldBiome, BiomeDefinition _newBiome)
	{
		if (_player == null)
		{
			return;
		}
		string text = (_oldBiome != null && _oldBiome.currentWeather != null) ? _oldBiome.currentWeather.buffName : "";
		string text2 = (_newBiome != null && _newBiome.currentWeather != null) ? _newBiome.currentWeather.buffName : "";
		if (text != "" && text2 != "")
		{
			if (text != text2)
			{
				_player.Buffs.RemoveBuff(text, true);
				_player.Buffs.AddBuff(text2, -1, true, false, -1f);
				return;
			}
		}
		else
		{
			if (text != "")
			{
				_player.Buffs.RemoveBuff(text, true);
				return;
			}
			if (text2 != "")
			{
				_player.Buffs.AddBuff(text2, -1, true, false, -1f);
			}
		}
	}

	public void ForceWeather(string _weatherName, float _duration)
	{
		WeatherManager.GeneralReset();
		this.CustomWeatherName = _weatherName;
		this.CustomWeatherTime = _duration;
		for (int i = 0; i < this.biomeWeather.Count; i++)
		{
			this.biomeWeather[i].ForceWeather(_weatherName);
		}
	}

	public static void VersionReset()
	{
		WeatherManager.GeneralReset();
		WeatherManager.savedWeather.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GeneralReset()
	{
		WeatherManager.globalTemperatureOffset = 0f;
		WeatherManager.globalRainDayStart = -1f;
		WeatherManager.globalRainDayPeak = 0f;
		WeatherManager.globalRainPercent = 0f;
		WeatherManager.isThunderWeather = false;
		WeatherManager.Instance.thunderLastTime = Time.time;
	}

	public void FrameUpdate()
	{
		if (GameManager.Instance == null || SkyManager.random == null)
		{
			return;
		}
		if (this.world == null)
		{
			return;
		}
		float time = Time.time;
		if (time > this.checkPlayerMoveTime + 1f)
		{
			this.checkPlayerMoveTime = time;
			EntityPlayerLocal primaryPlayer = this.world.GetPrimaryPlayer();
			if (primaryPlayer != null)
			{
				Vector3 vector = primaryPlayer.position - this.playerPosition;
				if (vector.x * vector.x + vector.y * vector.y + vector.z * vector.z > 400f)
				{
					this.spectrumBlend = 1f;
				}
				this.playerPosition = primaryPlayer.position;
			}
		}
		bool flag = GameManager.IsDedicatedServer || SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		if (flag && WeatherManager.savedWeather.Count > 0 && this.biomeWeather != null)
		{
			this.ApplySavedPackages();
		}
		int num = Time.frameCount;
		if (this.frameCount == num)
		{
			return;
		}
		this.frameCount = num;
		this.ParticlesFrameUpdate();
		if (flag)
		{
			this.GenerateWeatherServerFrameUpdate();
		}
		if (WeatherManager.isThunderWeather && flag && time >= this.thunderLastTime + this.thunderDelay)
		{
			this.thunderLastTime = time;
			Vector3 a = Vector3.zero;
			int num2 = 0;
			for (int i = 0; i < WeatherManager.players.Count; i++)
			{
				Entity entity = WeatherManager.players[i];
				if (entity != null)
				{
					a += entity.GetPosition();
					num2++;
				}
			}
			if (num2 > 0)
			{
				a.x /= (float)num2;
				a.y /= (float)num2;
				a.z /= (float)num2;
				float num3 = this.world.GetWorldTime();
				float num4 = num3 % 4f;
				float num5 = num3 % 3f;
				float num6 = num3 % 5f;
				Vector3 b;
				b.x = ((num4 == 2f) ? 0.5f : (num4 - 2f + (float)((num4 > 2f) ? 1 : -1))) * 200f;
				b.y = num5 * 10f + 200f;
				b.z = ((num6 == 2f) ? 0.5f : (num6 - 2f + (float)((num6 > 2f) ? 1 : -1))) * 200f;
				a -= b;
				WeatherManager.sLightningPos = a;
				WeatherManager.sLightningWorldTime = this.world.GetWorldTime() + 40UL;
				this.isPlayThunder = true;
				this.thunderDelay = SkyManager.random.RandomFloat;
				this.thunderDelay = Mathf.LerpUnclamped(this.thunderFreq.x, this.thunderFreq.y, this.thunderDelay);
			}
		}
		this.CurrentWeatherFromNearBiomesFrameUpdate();
		WeatherManager.currentWeather.ParamsFrameUpdate();
		if (this.isPlayThunder)
		{
			this.isPlayThunder = false;
			if (((((WeatherManager.forceRain >= 0f) ? WeatherManager.forceRain : WeatherManager.currentWeather.rainParam.value) > 0.5f && ((WeatherManager.forceClouds >= 0f) ? (WeatherManager.forceClouds * 100f) : WeatherManager.currentWeather.CloudThickness()) >= 70f) || SkyManager.IsBloodMoonVisible()) && EnvironmentAudioManager.Instance != null)
			{
				EnvironmentAudioManager.Instance.TriggerThunder(WeatherManager.sLightningWorldTime, WeatherManager.sLightningPos);
			}
		}
		if (flag)
		{
			this.WeatherPackagesServerFrameUpdate();
		}
		if (WeatherManager.currentWeather != null)
		{
			WeatherManager.currentWeather.Normalize();
		}
		if (WeatherManager.needToReUpdateWeatherSpectrums)
		{
			WeatherManager.needToReUpdateWeatherSpectrums = false;
			this.spectrumBlend = 1f;
		}
		this.SpectrumsFrameUpdate();
		this.CloudsFrameUpdate();
		this.WindFrameUpdate();
		TriggerEffectManager.UpdateDualSenseLightFromWeather(WeatherManager.currentWeather);
	}

	public void CloudsFrameUpdateNow()
	{
		this.CloudsFrameUpdate();
		this.cloudThickness = this.cloudThicknessTarget;
	}

	public void CloudsFrameUpdate()
	{
		if (WeatherManager.currentWeather == null)
		{
			return;
		}
		float num = WeatherManager.currentWeather.CloudThickness();
		if (WeatherManager.forceClouds >= 0f)
		{
			num = WeatherManager.forceClouds * 100f;
		}
		this.cloudThicknessTarget = num;
		if (num < 20f)
		{
			this.cloudThicknessTarget = 0f;
		}
		this.cloudThickness = Mathf.MoveTowards(this.cloudThickness, this.cloudThicknessTarget, 0.05f);
		Texture mainTex;
		Texture blendTex;
		float num2;
		if (this.cloudThickness <= 40f)
		{
			mainTex = WeatherManager.clouds[0];
			blendTex = WeatherManager.clouds[1];
			num2 = this.cloudThickness / 40f;
		}
		else
		{
			mainTex = WeatherManager.clouds[2];
			blendTex = WeatherManager.clouds[1];
			num2 = (this.cloudThickness - 40f) / 50f;
			if (num2 >= 1f)
			{
				num2 = 1f;
			}
			num2 = 1f - num2;
		}
		SkyManager.SetCloudTextures(mainTex, blendTex);
		SkyManager.SetCloudTransition(num2);
	}

	public void WindFrameUpdate()
	{
		float deltaTime = Time.deltaTime;
		float num = WeatherManager.GetWindSpeed();
		float num2 = num * 0.01f;
		this.windGust += this.windGustStep * deltaTime;
		if (this.windGust <= 0f)
		{
			this.windGust = 0f;
			this.windGustTime -= deltaTime;
			if (this.windGustTime <= 0f)
			{
				GameRandom gameRandom = this.world.GetGameRandom();
				this.windGustTarget = (3f + num * 0.33f) * gameRandom.RandomFloat + 5f;
				this.windGustStep = 0.35f * this.windGustTarget;
				this.windGustTime = (1f + 5f * gameRandom.RandomFloat) * (1f - num2) + 0.5f;
			}
		}
		if (this.windGust > this.windGustTarget)
		{
			this.windGust = this.windGustTarget;
			this.windGustStep = -this.windGustStep;
		}
		num += this.windGust;
		num *= 0.01f;
		this.windZone.windMain = num * 1.5f;
		this.windSpeedPrevious = this.windSpeed;
		this.windSpeed = num;
		this.windTimePrevious = this.windTime;
		this.windTime += num * deltaTime;
		Shader.SetGlobalVector("_Wind", new Vector4(this.windSpeed, this.windTime, this.windSpeedPrevious, this.windTimePrevious));
	}

	public void InitParticles()
	{
		this.GetParticleParts("Rain", out this.rainParticleObj, out this.rainParticleMat, out this.rainParticleSys);
		this.rainEmissionMaxRate = this.rainParticleSys.emission.rateOverTime.constant;
		this.GetParticleParts("Snow", out this.snowParticleObj, out this.snowParticleMat, out this.snowParticleSys);
		this.snowParticleT = this.snowParticleObj.transform;
		this.snowEmissionMaxRate = this.snowParticleSys.emission.rateOverTime.constant;
		Transform transform = this.snowParticleT.Find("Near");
		this.snowNearParticleSys = transform.GetComponent<ParticleSystem>();
		this.snowNearEmissionMaxRate = this.snowNearParticleSys.emission.rateOverTime.constant;
		Transform transform2 = this.snowParticleT.Find("Top");
		this.snowTopParticleSys = transform2.GetComponent<ParticleSystem>();
		this.snowTopEmissionMaxRate = this.snowTopParticleSys.emission.rateOverTime.constant;
		Transform transform3 = this.snowParticleT.Find("Far");
		this.snowFarParticleSys = transform3.GetComponent<ParticleSystem>();
		this.snowFarBaseColor = this.snowFarParticleSys.main.startColor.color;
		this.snowPlayerForceT = this.snowParticleT.Find("PlayerForce");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetParticleParts(string name, out GameObject obj, out Material mat, out ParticleSystem ps)
	{
		Transform transform = SkyManager.skyManager.transform.Find(name);
		obj = transform.gameObject;
		ps = obj.GetComponent<ParticleSystem>();
		Renderer component = ps.GetComponent<Renderer>();
		mat = component.material;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParticlesFrameUpdate()
	{
		if (this.mainCamera == null)
		{
			this.mainCamera = Camera.main;
			if (this.mainCamera == null)
			{
				return;
			}
		}
		EntityPlayerLocal primaryPlayer = this.world.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			return;
		}
		Vector4 value = new Vector4((WeatherManager.forceWet >= 0f) ? WeatherManager.forceWet : WeatherManager.currentWeather.wetParam.value, (WeatherManager.forceSnow >= 0f) ? WeatherManager.forceSnow : WeatherManager.currentWeather.snowCoverParam.value, 0f, (WeatherManager.forceRain >= 0f) ? WeatherManager.forceRain : Mathf.Clamp01(WeatherManager.currentWeather.rainParam.value * 40f));
		Shader.SetGlobalVector("_WeatherParams0", value);
		if (this.noiseTexture)
		{
			Shader.SetGlobalTexture("_NoiseSampler", this.noiseTexture);
		}
		Shader.SetGlobalTexture("_SnowSampler", this.snowTexture);
		if (Time.time > this.particleFallHitTime + 0.05f)
		{
			this.particleFallHitTime = Time.time;
			Vector3 position = this.mainCamera.transform.position;
			position.y += 250f;
			RaycastHit raycastHit;
			if (Physics.SphereCast(new Ray(position, Vector3.down), 9f, out raycastHit, float.PositiveInfinity, this.raycastMask))
			{
				this.particleFallLastPos = raycastHit.point;
				Vector3 velocityPerSecond = primaryPlayer.GetVelocityPerSecond();
				this.particleFallLastPos.x = this.particleFallLastPos.x + velocityPerSecond.x * 2f;
				this.particleFallLastPos.z = this.particleFallLastPos.z + velocityPerSecond.z * 2f;
				if (velocityPerSecond.y < -5f)
				{
					velocityPerSecond.y = -5f;
				}
				this.particleFallLastPos.y = this.particleFallLastPos.y + velocityPerSecond.y;
			}
			this.particleFallPos = this.particleFallLastPos;
			this.particleFallPos.y = this.particleFallPos.y + 12f;
			this.rainParticleObj.transform.position = this.particleFallPos;
			this.snowParticleT.position = this.particleFallPos;
		}
		float num = (WeatherManager.forceRain >= 0f) ? WeatherManager.forceRain : WeatherManager.currentWeather.rainParam.value;
		if (this.rainParticleSys != null)
		{
			this.rainParticleSys.emission.rateOverTime = this.rainEmissionMaxRate * (num * 0.995f + 0.005f);
		}
		float num2 = 1f;
		this.SetParticleIntensity(this.rainParticleObj, this.rainParticleMat, num * num2);
		if (this.snowParticleObj)
		{
			float num3 = (WeatherManager.forceSnowfall >= 0f) ? WeatherManager.forceSnowfall : Mathf.Clamp01(WeatherManager.currentWeather.snowFallParam.value);
			this.snowParticleObj.SetActive(num3 > 0f);
			if (num3 > 0f)
			{
				float num4 = num3 * 0.995f + 0.005f;
				this.snowParticleSys.emission.rateOverTime = this.snowEmissionMaxRate * num4;
				this.snowNearParticleSys.emission.rateOverTime = this.snowNearEmissionMaxRate * num4;
				this.snowTopParticleSys.emission.rateOverTime = this.snowTopEmissionMaxRate * num4;
				Color color = this.snowFarBaseColor;
				color.a *= num3 * 0.95f + 0.05f;
				ParticleSystem.MainModule main = this.snowFarParticleSys.main;
				ParticleSystem.MinMaxGradient startColor = main.startColor;
				startColor.color = color;
				main.startColor = startColor;
				Vector3 position2 = primaryPlayer.position - Origin.position;
				position2.y += 1f;
				this.snowPlayerForceT.position = position2;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetParticleIntensity(GameObject go, Material mtrl, float intensity)
	{
		if (go != null)
		{
			go.SetActive(intensity > 0f);
			if (mtrl != null && intensity > 0f)
			{
				mtrl.SetFloat("_Intensity", intensity);
			}
		}
	}

	public string GetSpectrumInfo()
	{
		float value = 1f - this.spectrumBlend;
		float value2 = this.spectrumBlend;
		string text = this.spectrumSourceType.ToString();
		string text2 = this.spectrumTargetType.ToString();
		return string.Format("source {0} {1}, target {2} {3}", new object[]
		{
			text,
			value.ToCultureInvariantString(),
			text2,
			value2.ToCultureInvariantString()
		});
	}

	public static void SetForceSpectrum(SpectrumWeatherType type)
	{
		WeatherManager.forcedSpectrum = type;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpectrumsFrameUpdate()
	{
		WeatherManager.LoadSpectrums();
		float num = SkyManager.BloodMoonVisiblePercent();
		if (num > 0f && this.spectrumTargetType == SpectrumWeatherType.BloodMoon)
		{
			this.spectrumBlend = num;
		}
		else if (this.spectrumBlend < 1f)
		{
			this.spectrumBlend += Time.deltaTime / 10f;
			if (this.spectrumBlend > 1f)
			{
				this.spectrumBlend = 1f;
			}
		}
		if (this.spectrumSourceType == this.spectrumTargetType)
		{
			this.spectrumBlend = 1f;
		}
		if (this.spectrumBlend >= 1f)
		{
			this.spectrumSourceType = this.spectrumTargetType;
			this.spectrumTargetType = SpectrumWeatherType.Biome;
			if (WeatherManager.currentWeather.biomeDefinition != null)
			{
				this.spectrumTargetType = WeatherManager.currentWeather.biomeDefinition.weatherSpectrum;
			}
			if (num > 0f)
			{
				this.spectrumTargetType = SpectrumWeatherType.BloodMoon;
			}
			if (this.spectrumSourceType != this.spectrumTargetType)
			{
				this.spectrumBlend = 0f;
			}
		}
	}

	public Color GetWeatherSpectrum(Color regularSpectrum, AtmosphereEffect.ESpecIdx type, float dayTimeScalar)
	{
		if (WeatherManager.forcedSpectrum == SpectrumWeatherType.None)
		{
			Color a = regularSpectrum;
			Color a2 = regularSpectrum;
			if (this.isGameModeNormal)
			{
				if (this.spectrumSourceType != SpectrumWeatherType.Biome)
				{
					ColorSpectrum colorSpectrum = WeatherManager.atmosphereSpectrum[(int)this.spectrumSourceType].spectrums[(int)type];
					if (colorSpectrum != null)
					{
						a = colorSpectrum.GetValue(dayTimeScalar);
					}
				}
				if (this.spectrumTargetType != SpectrumWeatherType.Biome)
				{
					ColorSpectrum colorSpectrum2 = WeatherManager.atmosphereSpectrum[(int)this.spectrumTargetType].spectrums[(int)type];
					if (colorSpectrum2 != null)
					{
						a2 = colorSpectrum2.GetValue(dayTimeScalar);
					}
				}
			}
			return a * (1f - this.spectrumBlend) + a2 * this.spectrumBlend;
		}
		int num = (int)WeatherManager.forcedSpectrum;
		AtmosphereEffect atmosphereEffect = WeatherManager.atmosphereSpectrum[num];
		if (atmosphereEffect == null)
		{
			return regularSpectrum;
		}
		ColorSpectrum colorSpectrum3 = atmosphereEffect.spectrums[(int)type];
		if (colorSpectrum3 == null)
		{
			return regularSpectrum;
		}
		return colorSpectrum3.GetValue(dayTimeScalar);
	}

	public void TriggerThunder(ulong _playWorldTime, Vector3 _pos)
	{
		WeatherManager.sLightningWorldTime = _playWorldTime;
		WeatherManager.sLightningPos = _pos;
		this.isPlayThunder = true;
	}

	public void ClientProcessPackages(WeatherPackage[] _packages)
	{
		int num = Time.frameCount;
		if (this.processingPackageFrame == num)
		{
			return;
		}
		this.processingPackageFrame = num;
		foreach (WeatherPackage weatherPackage in _packages)
		{
			BiomeDefinition biomeDefinition;
			if (WorldBiomes.Instance.TryGetBiome(weatherPackage.biomeID, out biomeDefinition))
			{
				biomeDefinition.weatherPackage.CopyFrom(weatherPackage);
				biomeDefinition.weatherSpectrum = (SpectrumWeatherType)weatherPackage.weatherSpectrum;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WeatherPackagesServerFrameUpdate()
	{
		int num = Utils.FastMin(this.biomeWeather.Count, this.weatherPackages.Length);
		for (int i = 0; i < num; i++)
		{
			WeatherManager.BiomeWeather biomeWeather = this.biomeWeather[i];
			WeatherPackage weatherPackage = this.weatherPackages[i];
			int num2 = 0;
			while (num2 < biomeWeather.parameters.Length && num2 < weatherPackage.param.Length)
			{
				weatherPackage.param[num2] = biomeWeather.parameterFinals[num2];
				num2++;
			}
			weatherPackage.particleRain = Mathf.Clamp01(biomeWeather.rainParam.value);
			weatherPackage.particleSnow = Mathf.Clamp01(biomeWeather.snowFallParam.value);
			weatherPackage.surfaceWet = Mathf.Clamp01(biomeWeather.wetParam.value);
			weatherPackage.surfaceSnow = Mathf.Clamp01(biomeWeather.snowCoverParam.value);
			weatherPackage.biomeID = biomeWeather.biomeDefinition.m_Id;
			weatherPackage.weatherSpectrum = (short)biomeWeather.biomeDefinition.weatherSpectrum;
			if (WeatherManager.forceRain >= 0f)
			{
				weatherPackage.particleRain = WeatherManager.forceRain;
			}
			if (WeatherManager.forceWet >= 0f)
			{
				weatherPackage.surfaceWet = WeatherManager.forceWet;
			}
			if (WeatherManager.forceSnow >= 0f)
			{
				weatherPackage.surfaceSnow = WeatherManager.forceSnow;
			}
			if (WeatherManager.forceSnowfall >= 0f)
			{
				weatherPackage.particleSnow = WeatherManager.forceSnowfall;
			}
			BiomeDefinition biomeDefinition = biomeWeather.biomeDefinition;
			BiomeDefinition biomeDefinition2;
			if (WorldBiomes.Instance.TryGetBiome(biomeWeather.biomeDefinition.m_Id, out biomeDefinition2))
			{
				biomeDefinition = biomeDefinition2;
			}
			biomeDefinition.weatherPackage = weatherPackage;
		}
	}

	public void SendPackages()
	{
		NetPackageWeather package = NetPackageManager.GetPackage<NetPackageWeather>();
		package.Setup(this.weatherPackages, WeatherManager.sLightningWorldTime, WeatherManager.sLightningPos);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, true, -1, -1, -1, null, 192);
		WeatherManager.sLightningWorldTime = 0UL;
	}

	[Conditional("DEBUG_WEATHERNET")]
	public void LogNet(string _format = "", params object[] _args)
	{
		_format = string.Format("{0} WeatherManager net {1}", GameManager.frameCount, _format);
		Log.Warning(_format, _args);
	}

	public static WeatherManager Instance;

	public const int BaseTemperature = 70;

	public List<WeatherManager.BiomeWeather> biomeWeather;

	public static List<WeatherPackage> savedWeather = new List<WeatherPackage>();

	public static ulong worldTime;

	public static float forceClouds = -1f;

	public static float forceRain = -1f;

	public static float forceWet = -1f;

	public static float forceSnow = -1f;

	public static float forceSnowfall = -1f;

	public const float cForceTempDefault = -100f;

	public static float forceTemperature = -100f;

	public static float forceWind = -1f;

	public static bool needToReUpdateWeatherSpectrums;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float forceSimRandom = -1f;

	public static float globalTemperatureOffset;

	public static float globalRainDayStart = -1f;

	public static float globalRainDayPeak;

	public static float globalRainPercent;

	public static float globalRainOLD1;

	public static float globalRainOLD2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<Vector2> temperatureOffsetHeights = new List<Vector2>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool hasCreatedSeaLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float seaLevel = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isGameModeNormal;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public WeatherPackage[] weatherPackages;

	public static bool inWeatherGracePeriod = true;

	public static WeatherManager.BiomeWeather currentWeather;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 thunderFreq = new Vector2(30f, 60f);

	public static ulong sLightningWorldTime;

	public static Vector3 sLightningPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isPlayThunder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float thunderLastTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float thunderDelay = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cWeatherTransitionSeconds = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float sSnowAccumulationSpeed = 150f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cWeatherChangeFrequency = 1500f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int frameCount = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<Entity> players = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static string[] strCloudTypes = new string[]
	{
		"Whispy",
		"Fluffy",
		"ThickOvercast"
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture[] clouds = new Texture[WeatherManager.strCloudTypes.Length];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isCurrentWeatherUpdatedFirstTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cWindMax = 100f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cWindScale = 0.01f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject windZoneObj;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public WindZone windZone;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windSpeedPrevious;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windTimePrevious;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windGust;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windGustStep;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windGustTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windGustTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Camera mainCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture noiseTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture snowTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int raycastMask;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject rainParticleObj;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ParticleSystem rainParticleSys;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material rainParticleMat;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float rainEmissionMaxRate = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject snowParticleObj;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform snowParticleT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material snowParticleMat;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ParticleSystem snowParticleSys;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float snowEmissionMaxRate = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ParticleSystem snowNearParticleSys;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float snowNearEmissionMaxRate = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ParticleSystem snowTopParticleSys;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float snowTopEmissionMaxRate = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ParticleSystem snowFarParticleSys;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Color snowFarBaseColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform snowPlayerForceT;

	public float spectrumBlend = 1f;

	public static SpectrumWeatherType forcedSpectrum = SpectrumWeatherType.None;

	public SpectrumWeatherType spectrumSourceType;

	public SpectrumWeatherType spectrumTargetType;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static AtmosphereEffect[] atmosphereSpectrum;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 playerPosition = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float checkPlayerMoveTime;

	public WeatherManager.CurrentBiome[] editorNearBiomes = new WeatherManager.CurrentBiome[4];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cTimeScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cTemperatureChangeDuration = 5000f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cRainStartMin = 0.375f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cRainStartMax = 0.6666667f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cRainPeakMin = 0.020833334f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cRainPeakMax = 0.09583333f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cRainFade = 0.0291666668f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastWorldTimeWeatherChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float temperatureStart = WeatherManager.globalTemperatureOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float temperatureTarget = WeatherManager.globalTemperatureOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ulong temperatureStartWorldTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool isThunderWeather;

	public string CustomWeatherName = "";

	public float CustomWeatherTime = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cloudThickness;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cloudThicknessTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float particleFallHitTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 particleFallLastPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 particleFallPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int processingPackageFrame = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum CloudTypes
	{
		Whispy,
		Fluffy,
		ThickOvercast
	}

	public class temperatureOffsetHeightsComparer : IComparer<Vector2>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public int Compare(Vector2 x, Vector2 y)
		{
			if (y.x < x.x)
			{
				return 1;
			}
			if (y.x <= x.x)
			{
				return 0;
			}
			return -1;
		}
	}

	[Serializable]
	public class Param
	{
		public void Clamp()
		{
			this.value = Mathf.Clamp01(this.value);
			this.target = Mathf.Clamp01(this.target);
		}

		public Param(float _value, float _step1Time = 0.25f)
		{
			this.name = "Param";
			this.value = _value;
			this.target = _value;
			this.step1Time = _step1Time;
		}

		public void FrameUpdate()
		{
			float time = Time.time;
			if (this.value == this.target)
			{
				this.lastTime = time;
				return;
			}
			float num = time - this.lastTime;
			if (num < 0f)
			{
				this.lastTime = time;
			}
			if (num >= 0.01f)
			{
				if (num > 1f)
				{
					num = 1f;
				}
				float num2 = num / this.step1Time;
				if (this.value > this.target)
				{
					this.value -= num2;
					if (this.value < this.target)
					{
						this.value = this.target;
					}
				}
				else
				{
					this.value += num2;
					if (this.value > this.target)
					{
						this.value = this.target;
					}
				}
				this.lastTime = time;
			}
		}

		public void Reset()
		{
			this.Set(0f);
			this.lastTime = Time.time;
		}

		public void Set(float _value)
		{
			this.value = _value;
			this.target = _value;
		}

		public void SetTarget(float _target)
		{
			if (this.target == _target)
			{
				return;
			}
			this.target = _target;
			this.lastTime = Time.time;
		}

		public string name;

		public float value;

		public float target;

		public float step1Time;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public float lastTime;
	}

	[Serializable]
	public class BiomeWeather
	{
		public BiomeWeather(BiomeDefinition _definition = null)
		{
			this.biomeDefinition = _definition;
			for (int i = 0; i < 5; i++)
			{
				this.parameters[i] = new WeatherManager.Param(0f, 0.3f);
			}
			this.parameters[0].step1Time = 0.15f;
		}

		public float CloudThickness()
		{
			return this.parameters[2].value;
		}

		public float FogPercent()
		{
			return this.parameters[4].value * 0.01f;
		}

		public float Wind()
		{
			return this.parameters[3].value;
		}

		public void Normalize()
		{
			this.rainParam.Clamp();
			this.wetParam.Clamp();
			this.snowCoverParam.Clamp();
			this.snowFallParam.Clamp();
		}

		public void ServerFrameUpdate()
		{
			WeatherManager.Param[] array = this.parameters;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].FrameUpdate();
			}
			float num = this.parameters[1].value;
			float num2 = this.parameters[2].value;
			float num3 = this.parameters[0].value;
			WeatherManager.Instance.CalcGlobalPrecipCloudsTemperature(ref num, ref num2, ref num3);
			num = this.biomeDefinition.WeatherClampToPossibleValues(num, BiomeDefinition.Probabilities.ProbType.Precipitation);
			num2 = this.biomeDefinition.WeatherClampToPossibleValues(num2, BiomeDefinition.Probabilities.ProbType.CloudThickness);
			num3 = this.biomeDefinition.WeatherClampToPossibleValues(num3, WeatherManager.globalTemperatureOffset, BiomeDefinition.Probabilities.ProbType.Temperature);
			if (WeatherManager.forceClouds >= 0f)
			{
				num2 = WeatherManager.forceClouds * 100f;
			}
			if (WeatherManager.forceTemperature > -100f)
			{
				num3 = WeatherManager.forceTemperature;
			}
			this.parameterFinals[1] = num;
			this.parameterFinals[2] = num2;
			this.parameterFinals[0] = num3;
			this.parameterFinals[3] = this.parameters[3].value;
			this.parameterFinals[4] = this.parameters[4].value;
			float num4 = (num * 0.01f - 0.3f) / 0.7f;
			this.rainParam.SetTarget((num4 > 0f && num3 > 32f) ? num4 : 0f);
			float num5 = this.rainParam.value;
			if (WeatherManager.forceRain >= 0f)
			{
				num5 = WeatherManager.forceRain;
			}
			if (num5 > 0.5f && num2 >= 70f)
			{
				WeatherManager.isThunderWeather = true;
			}
			float sunPercent = SkyManager.GetSunPercent();
			float num6 = Mathf.Clamp01(sunPercent);
			this.wetParam.SetTarget((float)((num5 > 0f) ? 1 : 0));
			float num7 = 10f;
			this.wetParam.step1Time = num7;
			this.wetParam.step1Time -= num7 * 0.5f * num6 * (float)((this.wetParam.target < this.wetParam.value) ? 1 : 0);
			this.snowFallParam.SetTarget((num4 > 0f && num3 <= 32f) ? num4 : 0f);
			float num8 = this.snowFallParam.value;
			if (WeatherManager.forceSnowfall >= 0f)
			{
				num8 = WeatherManager.forceSnowfall;
			}
			float num9 = ((num3 <= 32f) ? ((32f - num3) / 32f) : 0f) * 0.15f;
			num9 *= 1f - Mathf.Clamp01(sunPercent * 8f);
			this.snowCoverParam.SetTarget((num8 > 0f) ? 1f : num9);
			float sSnowAccumulationSpeed = WeatherManager.sSnowAccumulationSpeed;
			this.snowCoverParam.step1Time = sSnowAccumulationSpeed * 0.5f * num5;
			this.snowCoverParam.step1Time -= sSnowAccumulationSpeed * 0.25f * num6 * (float)((this.snowCoverParam.target < this.snowCoverParam.value) ? 1 : 0);
			this.rainParam.FrameUpdate();
			this.wetParam.FrameUpdate();
			this.snowCoverParam.FrameUpdate();
			this.snowFallParam.FrameUpdate();
		}

		public void ParamsFrameUpdate()
		{
			WeatherManager.Param[] array = this.parameters;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].FrameUpdate();
			}
			this.rainParam.FrameUpdate();
			this.wetParam.FrameUpdate();
			this.snowCoverParam.FrameUpdate();
			this.snowFallParam.FrameUpdate();
		}

		public void Reset()
		{
			if (this.biomeDefinition == null)
			{
				return;
			}
			this.biomeDefinition.WeatherRandomize(0f);
			for (int i = 0; i < 5; i++)
			{
				this.parameters[i].Set(this.biomeDefinition.WeatherGetValue((BiomeDefinition.Probabilities.ProbType)i));
			}
			this.rainParam.Set(0f);
			this.wetParam.Set(0f);
			this.snowCoverParam.Set(0f);
			this.snowFallParam.Set(0f);
		}

		public void Randomize()
		{
			if (this.biomeDefinition == null)
			{
				return;
			}
			float rand = GameManager.Instance.World.GetGameRandom().RandomFloat;
			if (WeatherManager.forceSimRandom >= 0f)
			{
				rand = WeatherManager.forceSimRandom;
			}
			this.biomeDefinition.WeatherRandomize(rand);
			for (int i = 0; i < 5; i++)
			{
				BiomeDefinition.Probabilities.ProbType type = (BiomeDefinition.Probabilities.ProbType)i;
				this.parameters[i].target = this.biomeDefinition.WeatherGetValue(type);
			}
		}

		public void ForceWeather(string name)
		{
			if (this.biomeDefinition == null)
			{
				return;
			}
			WeatherManager.GeneralReset();
			this.biomeDefinition.WeatherRandomize(name);
			for (int i = 0; i < 5; i++)
			{
				BiomeDefinition.Probabilities.ProbType type = (BiomeDefinition.Probabilities.ProbType)i;
				this.parameters[i].target = this.biomeDefinition.WeatherGetValue(type);
			}
		}

		public override string ToString()
		{
			string str = string.Format("{0}: {1}, ", this.biomeDefinition.m_sBiomeName, this.biomeDefinition.weatherName);
			for (int i = 0; i < 5; i++)
			{
				BiomeDefinition.Probabilities.ProbType probType = (BiomeDefinition.Probabilities.ProbType)i;
				str += string.Format("{0} {1}, ", probType, this.biomeDefinition.WeatherGetValue(probType));
			}
			str += string.Format("rain {0}, ", this.rainParam.value);
			str += string.Format("wet {0}, ", this.wetParam.value);
			str += string.Format("snowCover {0}, ", this.snowCoverParam.value);
			return str + string.Format("snowFall {0}", this.snowFallParam.value);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public const float cStep1Time = 0.15f;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public const float cPrecipitationPercentVisibleMin = 0.3f;

		public BiomeDefinition biomeDefinition;

		public WeatherManager.Param[] parameters = new WeatherManager.Param[5];

		public float[] parameterFinals = new float[5];

		public WeatherManager.Param rainParam = new WeatherManager.Param(0f, 0.25f);

		public WeatherManager.Param wetParam = new WeatherManager.Param(0f, 0.25f);

		public WeatherManager.Param snowCoverParam = new WeatherManager.Param(0f, 0.25f);

		public WeatherManager.Param snowFallParam = new WeatherManager.Param(0f, 0.25f);
	}

	[Serializable]
	public class CurrentBiome
	{
		public string name;

		public float intensity;
	}
}
