﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class WorldEnvironment : MonoBehaviour
{
	public void Init(WorldCreationData _wcd, World _world)
	{
		this.world = _world;
		this.createPrefab(_wcd);
		this.createTransforms(_wcd);
		this.createWeatherEffects(_wcd);
		this.createAtmosphere(_wcd);
		this.createDistantTerrain();
	}

	public static void OnXMLChanged()
	{
		DynamicProperties properties = WorldEnvironment.Properties;
		if (properties == null)
		{
			return;
		}
		properties.ParseVec("ambientEquatorScale", ref WorldEnvironment.dataAmbientEquatorScale);
		properties.ParseVec("ambientGroundScale", ref WorldEnvironment.dataAmbientGroundScale);
		properties.ParseVec("ambientSkyScale", ref WorldEnvironment.dataAmbientSkyScale);
		properties.ParseVec("ambientSkyDesat", ref WorldEnvironment.dataAmbientSkyDesat);
		properties.ParseVec("ambientMoon", ref WorldEnvironment.dataAmbientMoon);
		properties.ParseFloat("ambientInsideSpeed", ref WorldEnvironment.dataAmbientInsideSpeed);
		properties.ParseFloat("ambientInsideThreshold", ref WorldEnvironment.dataAmbientInsideThreshold);
		properties.ParseVec("ambientInsideEquatorScale", ref WorldEnvironment.dataAmbientInsideEquatorScale);
		properties.ParseVec("ambientInsideGroundScale", ref WorldEnvironment.dataAmbientInsideGroundScale);
		properties.ParseVec("ambientInsideSkyScale", ref WorldEnvironment.dataAmbientInsideSkyScale);
		properties.ParseVec("fogPower", ref WorldEnvironment.dataFogPow);
		properties.ParseVec("fogWater", ref WorldEnvironment.dataFogWater);
		properties.ParseVec("fogWaterColor", ref WorldEnvironment.dataFogWaterColor);
		properties.ParseFloat("test", ref WorldEnvironment.dataTest);
	}

	public void CreateUnityTerrain()
	{
	}

	public static GameObject CreateUnityTerrainOld(string _levelName, int _sliceAtWidth, int _heightMapDataWidth, int _heightMapDataHeight, List<float[,]> _heightMapDataList, int _worldScale = 1, float _yOffsetTerrain = 1f, int _sliceAt = 2048, bool _bEditMode = true)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		MicroStopwatch microStopwatch2 = new MicroStopwatch();
		TextureAtlasTerrain textureAtlasTerrain = (TextureAtlasTerrain)MeshDescription.meshes[5].textureAtlas;
		Texture2D[] array = new Texture2D[]
		{
			textureAtlasTerrain.diffuse[6],
			textureAtlasTerrain.diffuse[1],
			textureAtlasTerrain.diffuse[195],
			textureAtlasTerrain.diffuse[195],
			textureAtlasTerrain.diffuse[185],
			textureAtlasTerrain.diffuse[184],
			textureAtlasTerrain.diffuse[438],
			textureAtlasTerrain.diffuse[288],
			textureAtlasTerrain.diffuse[10],
			textureAtlasTerrain.diffuse[11],
			textureAtlasTerrain.diffuse[403],
			textureAtlasTerrain.diffuse[34]
		};
		Texture2D[] array2 = new Texture2D[]
		{
			textureAtlasTerrain.normal[6],
			textureAtlasTerrain.normal[1],
			textureAtlasTerrain.normal[195],
			textureAtlasTerrain.normal[195],
			textureAtlasTerrain.normal[185],
			textureAtlasTerrain.normal[184],
			textureAtlasTerrain.normal[438],
			textureAtlasTerrain.normal[288],
			textureAtlasTerrain.normal[10],
			textureAtlasTerrain.normal[11],
			textureAtlasTerrain.normal[403],
			textureAtlasTerrain.normal[34]
		};
		string fullPath = PathAbstractions.WorldsSearchPaths.GetLocation(_levelName, null, null).FullPath;
		int num = 4;
		int num2;
		int num3;
		Color32[] array3 = TextureUtils.LoadTexturePixels(fullPath + "/splat1", out num2, out num3, null);
		float num4 = (float)num2 / (float)_heightMapDataWidth;
		string pathNoExtension = fullPath + "/splat2";
		float num5 = 0f;
		int num6;
		int num7;
		Color32[] array4 = TextureUtils.LoadTexturePixels(pathNoExtension, out num6, out num7, null);
		if (array4 != null)
		{
			num5 = (float)num6 / (float)_heightMapDataWidth;
			num += 4;
		}
		string pathNoExtension2 = fullPath + "/splat3";
		float num8 = 0f;
		int num9;
		int num10;
		Color32[] array5 = TextureUtils.LoadTexturePixels(pathNoExtension2, out num9, out num10, null);
		if (array5 != null)
		{
			num8 = (float)num9 / (float)_heightMapDataWidth;
			num += 4;
		}
		microStopwatch2.ResetAndRestart();
		for (int i = 0; i < array3.Length; i++)
		{
			Color32 color = array3[i];
			int num11 = (int)color.r;
			int num12 = (int)color.g;
			int num13 = (int)color.b;
			int num14 = (int)color.a;
			if (num > 4)
			{
				color = array4[i];
				num11 += (int)color.r;
				num12 += (int)color.g;
				num13 += (int)color.b;
				num14 += (int)color.a;
			}
			if (num > 8)
			{
				color = array5[i];
				num11 += (int)color.r;
				num12 += (int)color.g;
				num13 += (int)color.b;
				num14 += (int)color.a;
			}
			if (num11 + num12 + num13 + num14 < 255)
			{
				array3[i] = new Color32((byte)((int)array3[i].r + (255 - (num11 + num12 + num13 + num14))), array3[i].g, array3[i].b, array3[i].a);
			}
		}
		Log.Out("Splat1 color fix {0}ms", new object[]
		{
			microStopwatch2.ElapsedMilliseconds
		});
		microStopwatch2.ResetAndRestart();
		GameObject gameObject = new GameObject("Terrain");
		float num15 = (float)(_heightMapDataWidth / 2) + 0.5f;
		gameObject.transform.position = new Vector3(-1f * num15 * (float)_worldScale, 1f, -1f * num15 * (float)_worldScale);
		Origin.Add(gameObject.transform, 0);
		int num16 = 0;
		int num17 = _heightMapDataWidth / _sliceAtWidth;
		int num18 = _heightMapDataHeight / _sliceAtWidth;
		Terrain[,] array6 = new Terrain[num17, num18];
		float[,,] array7 = new float[_sliceAtWidth, _sliceAtWidth, num];
		for (int j = 0; j < num18; j++)
		{
			for (int k = 0; k < num17; k++)
			{
				microStopwatch2.ResetAndRestart();
				TerrainData terrainData = new TerrainData();
				terrainData.heightmapResolution = _sliceAtWidth;
				terrainData.size = new Vector3((float)(_sliceAtWidth * _worldScale), 256f, (float)(_sliceAtWidth * _worldScale));
				terrainData.SetHeights(0, 0, _heightMapDataList[num16++]);
				Log.Out("Setting heights to unity terrain took " + microStopwatch2.ElapsedMilliseconds.ToString() + "ms");
				microStopwatch2.ResetAndRestart();
				terrainData.alphamapResolution = _sliceAtWidth;
				TerrainLayer[] array8 = new TerrainLayer[array.Length];
				for (int l = 0; l < array.Length; l++)
				{
					array8[l] = new TerrainLayer
					{
						diffuseTexture = array[l],
						normalMapTexture = array2[l],
						tileSize = new Vector2(10f, 10f)
					};
				}
				terrainData.terrainLayers = array8;
				for (int m = 0; m < _sliceAtWidth; m++)
				{
					int num19 = j * _sliceAtWidth + m;
					int num20 = (int)((float)num19 * num4);
					for (int n = 0; n < _sliceAtWidth; n++)
					{
						int num21 = k * _sliceAtWidth + n;
						int num22 = (int)((float)num21 * num4 + (float)(num20 * num2)) % array3.Length;
						Color32 color2 = array3[num22];
						array7[m, n, 0] = (float)color2.r * 0.003921569f;
						array7[m, n, 1] = (float)color2.g * 0.003921569f;
						array7[m, n, 2] = (float)color2.b * 0.003921569f;
						array7[m, n, 3] = (float)color2.a * 0.003921569f;
						if (num > 4)
						{
							num22 = (int)((float)num21 * num5 + (float)((int)((float)num19 * num5) * num6)) % array4.Length;
							color2 = array4[num22];
							array7[m, n, 4] = (float)color2.r * 0.003921569f;
							array7[m, n, 5] = (float)color2.g * 0.003921569f;
							array7[m, n, 6] = (float)color2.b * 0.003921569f;
							array7[m, n, 7] = (float)color2.a * 0.003921569f;
						}
						if (num > 8)
						{
							num22 = (int)((float)num21 * num8 + (float)((int)((float)num19 * num8) * num9)) % array5.Length;
							color2 = array5[num22];
							array7[m, n, 8] = (float)color2.r * 0.003921569f;
							array7[m, n, 9] = (float)color2.g * 0.003921569f;
							array7[m, n, 10] = (float)color2.b * 0.003921569f;
							array7[m, n, 11] = (float)color2.a * 0.003921569f;
						}
					}
				}
				terrainData.SetAlphamaps(0, 0, array7);
				Log.Out("Splats took " + microStopwatch2.ElapsedMilliseconds.ToString() + "ms");
				microStopwatch2.ResetAndRestart();
				GameObject gameObject2 = Terrain.CreateTerrainGameObject(terrainData);
				Terrain component = gameObject2.GetComponent<Terrain>();
				if (_bEditMode)
				{
					component.heightmapPixelError = 5f;
					component.basemapDistance = 2000f;
					gameObject2.AddComponent<TerrainDetectChanges>();
				}
				else
				{
					component.heightmapPixelError = 20f;
				}
				array6[k, j] = component;
				gameObject2.layer = 16;
				gameObject2.tag = "T_Mesh";
				gameObject2.transform.parent = gameObject.transform;
				gameObject2.transform.localPosition = new Vector3((float)(k * _sliceAtWidth * _worldScale), _yOffsetTerrain, (float)(j * _sliceAtWidth * _worldScale));
			}
		}
		for (int num23 = 0; num23 < num18; num23++)
		{
			for (int num24 = 0; num24 < num17; num24++)
			{
				array6[num24, num23].SetNeighbors((num24 > 0) ? array6[num24 - 1, num23] : null, (num23 > 0) ? array6[num24, num23 - 1] : null, (num24 < num17 - 1) ? array6[num24 + 1, num23] : null, (num23 < num18 - 1) ? array6[num24, num23 + 1] : null);
			}
		}
		Log.Out("Creating unity terrain took " + microStopwatch.ElapsedMilliseconds.ToString() + "ms");
		return gameObject;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createDistantTerrain()
	{
		if (!GameManager.IsDedicatedServer && !this.world.ChunkClusters[0].IsFixedSize)
		{
			this.chunkClusterVisibleDelegate = new ChunkCluster.OnChunkVisibleDelegate(this.OnChunkDisplayed);
			this.world.ChunkClusters[0].OnChunkVisibleDelegates += this.chunkClusterVisibleDelegate;
		}
	}

	public void OnChunkDisplayed(long _key, bool _bDisplayed)
	{
		if (UnityDistantTerrainTest.Instance != null)
		{
			UnityDistantTerrainTest.Instance.OnChunkVisible(WorldChunkCache.extractX(_key), WorldChunkCache.extractZ(_key), _bDisplayed);
		}
	}

	public void Cleanup()
	{
		this.localPlayer = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createPrefab(WorldCreationData _wcd)
	{
		if (_wcd.Properties.Values.ContainsKey("WorldEnvironment.Prefab"))
		{
			UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>(_wcd.Properties.Values["WorldEnvironment.Prefab"])).transform.parent = base.transform;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void createWeatherEffects(WorldCreationData _wcd)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void createTransforms(WorldCreationData _wcd)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void createAtmosphere(WorldCreationData _wcd)
	{
	}

	public Color GetAmbientColor()
	{
		if (this.world == null || this.world.BiomeAtmosphereEffects == null)
		{
			return Color.black;
		}
		return this.world.BiomeAtmosphereEffects.GetSkyColorSpectrum(this.dayTimeScalar);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AmbientSpectrumFrameUpdate()
	{
		if (this.world == null || this.world.BiomeAtmosphereEffects == null)
		{
			return;
		}
		float target = 0f;
		if (this.localPlayer && this.localPlayer.Stats.LightInsidePer >= WorldEnvironment.dataAmbientInsideThreshold)
		{
			target = 1f;
		}
		this.insideCurrent = Mathf.MoveTowards(this.insideCurrent, target, WorldEnvironment.dataAmbientInsideSpeed * Time.deltaTime);
		float dayPercent = SkyManager.dayPercent;
		float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxBrightness);
		float num = (@float < 0.5f) ? Utils.FastLerpUnclamped(0.2f, 1f, @float * 2f) : (@float + 0.5f);
		float b = Mathf.LerpUnclamped(Utils.FastMax(1f, num), num, this.insideCurrent);
		num = Utils.FastLerp(num, b, dayPercent * 2f);
		this.brightnessInOutDayNight = num;
		float num2 = SkyManager.GetMoonAmbientScale(WorldEnvironment.dataAmbientMoon.x, WorldEnvironment.dataAmbientMoon.y);
		num2 = Mathf.LerpUnclamped(num2, 1f, this.insideCurrent);
		num *= num2;
		num += this.nightVisionBrightness;
		Color skyColor = SkyManager.GetSkyColor();
		Color b2 = new Color(0.7f, 0.7f, 0.7f, 1f);
		float t = Mathf.LerpUnclamped(WorldEnvironment.dataAmbientSkyDesat.y, WorldEnvironment.dataAmbientSkyDesat.x, dayPercent);
		Color color = Color.LerpUnclamped(skyColor, b2, t);
		float a = Mathf.LerpUnclamped(WorldEnvironment.dataAmbientSkyScale.y, WorldEnvironment.dataAmbientSkyScale.x, dayPercent);
		float b3 = Mathf.LerpUnclamped(WorldEnvironment.dataAmbientInsideSkyScale.y, WorldEnvironment.dataAmbientInsideSkyScale.x, dayPercent);
		float num3 = Mathf.LerpUnclamped(a, b3, this.insideCurrent);
		float num4 = SkyManager.GetLuma(color) * num3;
		num3 *= num;
		RenderSettings.ambientSkyColor = color * num3;
		float num5 = Mathf.LerpUnclamped(WorldEnvironment.dataAmbientEquatorScale.y, WorldEnvironment.dataAmbientEquatorScale.x, dayPercent);
		float b4 = Mathf.LerpUnclamped(WorldEnvironment.dataAmbientInsideEquatorScale.y, WorldEnvironment.dataAmbientInsideEquatorScale.x, dayPercent);
		num5 = Mathf.LerpUnclamped(num5, b4, this.insideCurrent);
		num5 *= num;
		RenderSettings.ambientEquatorColor = SkyManager.GetFogColor() * num5;
		Color sunLightColor = SkyManager.GetSunLightColor();
		float a2 = Mathf.LerpUnclamped(WorldEnvironment.dataAmbientGroundScale.y, WorldEnvironment.dataAmbientGroundScale.x, dayPercent);
		float b5 = Mathf.LerpUnclamped(WorldEnvironment.dataAmbientInsideGroundScale.y, WorldEnvironment.dataAmbientInsideGroundScale.x, dayPercent);
		float num6 = Mathf.LerpUnclamped(a2, b5, this.insideCurrent);
		num4 += SkyManager.GetLuma(sunLightColor) * num6;
		num6 *= num;
		RenderSettings.ambientGroundColor = sunLightColor * num6;
		WorldEnvironment.AmbientTotal = num4 * num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpectrumsFrameUpdate()
	{
		if (this.world == null || !this.localPlayer)
		{
			return;
		}
		BiomeAtmosphereEffects biomeAtmosphereEffects = this.world.BiomeAtmosphereEffects;
		if (biomeAtmosphereEffects == null)
		{
			return;
		}
		Color color = biomeAtmosphereEffects.GetSkyColorSpectrum(this.dayTimeScalar);
		color = WeatherManager.Instance.GetWeatherSpectrum(color, AtmosphereEffect.ESpecIdx.Sky, this.dayTimeScalar);
		SkyManager.SetSkyColor(color);
		color = biomeAtmosphereEffects.GetSunColorSpectrum(this.dayTimeScalar);
		color = WeatherManager.Instance.GetWeatherSpectrum(color, AtmosphereEffect.ESpecIdx.Sun, this.dayTimeScalar);
		SkyManager.SetSunColor(color);
		SkyManager.SetSunIntensity(color.a * 2f);
		color = biomeAtmosphereEffects.GetMoonColorSpectrum(this.dayTimeScalar);
		color = WeatherManager.Instance.GetWeatherSpectrum(color, AtmosphereEffect.ESpecIdx.Moon, this.dayTimeScalar);
		SkyManager.SetMoonLightColor(color);
		color = biomeAtmosphereEffects.GetFogColorSpectrum(this.dayTimeScalar);
		color = WeatherManager.Instance.GetWeatherSpectrum(color, AtmosphereEffect.ESpecIdx.Fog, this.dayTimeScalar);
		Color color2 = biomeAtmosphereEffects.GetFogFadeColorSpectrum(this.dayTimeScalar);
		color2 = WeatherManager.Instance.GetWeatherSpectrum(color2, AtmosphereEffect.ESpecIdx.FogFade, this.dayTimeScalar);
		SkyManager.SetFogFade(1f - color2.r - 0.5f, 1f - color2.a);
		Color color3 = new Color(color.r, color.g, color.b, 1f);
		color3 *= Utils.FastMin(this.brightnessInOutDayNight, 1f);
		float dayPercent = SkyManager.dayPercent;
		float num = Mathf.Pow(color.a, Utils.FastLerpUnclamped(WorldEnvironment.dataFogPow.y, WorldEnvironment.dataFogPow.x, dayPercent));
		num += WeatherManager.currentWeather.FogPercent();
		if (num > 1f)
		{
			num = 1f;
		}
		if (this.fogDensityOverride >= 0f)
		{
			num = this.fogDensityOverride;
			color3 = this.fogColorOverride;
		}
		float t = 0.01f;
		if (this.localPlayer.IsUnderwaterCamera)
		{
			this.isUnderWater = true;
			num = WorldEnvironment.dataFogWater.x;
			t = WorldEnvironment.dataFogWater.y;
			float num2 = 0.35f + SkyManager.dayPercent * 0.65f;
			color3 = new Color(WorldEnvironment.dataFogWaterColor.x * num2, WorldEnvironment.dataFogWaterColor.y * num2, WorldEnvironment.dataFogWaterColor.z * num2, 1f);
		}
		else if (this.isUnderWater)
		{
			this.isUnderWater = false;
			t = 1f;
		}
		if (this.localPlayer.bPlayingSpawnIn)
		{
			t = 1f;
		}
		if (this.localPlayer.InWorldLookPercent < 1f)
		{
			color3 = Color.LerpUnclamped(new Color(0.5f, 0.5f, 0.2f), color3, this.localPlayer.InWorldLookPercent);
			num = Utils.FastLerpUnclamped(0.6f, num, this.localPlayer.InWorldLookPercent);
		}
		SkyManager.SetFogColor(Color.Lerp(SkyManager.GetFogColor(), color3, t));
		SkyManager.SetFogDensity(Mathf.Lerp(SkyManager.GetFogDensity(), num, t));
	}

	public void WorldTimeChanged()
	{
		if (this.world == null)
		{
			return;
		}
		WeatherManager.worldTime = this.world.worldTime;
		SkyManager.SetGameTime(this.world.worldTime);
		if (this.world.BiomeAtmosphereEffects != null)
		{
			this.world.BiomeAtmosphereEffects.Update();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void enableAllDisplayedDistantChunks(bool _bEnable)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		this.dayTimeScalar = SkyManager.GetWorldRotation();
		if (!WeatherManager.Instance)
		{
			return;
		}
		WeatherManager.Instance.FrameUpdate();
		this.AmbientSpectrumFrameUpdate();
		if (this.localPlayer == null)
		{
			this.localPlayer = this.world.GetPrimaryPlayer();
			if (this.localPlayer == null)
			{
				return;
			}
		}
		if (!GameManager.IsDedicatedServer && UnityDistantTerrainTest.Instance != null)
		{
			UnityDistantTerrainTest.Instance.FrameUpdate(this.localPlayer);
		}
		this.SpectrumsFrameUpdate();
		IList<ChunkGameObject> displayedChunkGameObjects = this.world.m_ChunkManager.GetDisplayedChunkGameObjects();
		int count = displayedChunkGameObjects.Count;
		int num = count;
		if (num > 8)
		{
			num = 8;
		}
		for (int i = 0; i < num; i++)
		{
			int num2 = this.chunkLODIndex + 1;
			this.chunkLODIndex = num2;
			if (num2 >= count)
			{
				this.chunkLODIndex = 0;
			}
			displayedChunkGameObjects[this.chunkLODIndex].CheckLODs(-1);
		}
	}

	public void CreateLevelBorderBox(World _world)
	{
	}

	public void SetColliders(float _worldX, float _worldZ, float _worldXDim, float _worldZDim, float _waterSize, float _bDistance)
	{
	}

	public Color GetSunLightColor()
	{
		return this.world.BiomeAtmosphereEffects.GetSunColorSpectrum(this.dayTimeScalar);
	}

	public Color GetMoonLightColor()
	{
		return this.world.BiomeAtmosphereEffects.GetMoonColorSpectrum(this.dayTimeScalar);
	}

	public static float CalculateCelestialAngle(ulong _worldTime, float _t)
	{
		float num = ((float)((int)(_worldTime % 24000UL)) + _t) / 24000f - 0.25f;
		if (num < 0f)
		{
			num += 1f;
		}
		if (num > 1f)
		{
			num -= 1f;
		}
		float num2 = num;
		num = 1f - (float)((Math.Cos((double)num * 3.1415926535897931) + 1.0) / 2.0);
		return num2 + (num - num2) / 3f;
	}

	public BiomeAtmosphereEffects GetBiomeAtmosphereEffects()
	{
		return this.world.BiomeAtmosphereEffects;
	}

	public int DistantTerrain_GetBlockIdAt(int x, int y, int z)
	{
		int poiblockIdOverride = this.world.ChunkCache.ChunkProvider.GetPOIBlockIdOverride(x, z);
		if (poiblockIdOverride != 0)
		{
			return poiblockIdOverride;
		}
		BlockValue blockValue = this.world.ChunkCache.ChunkProvider.GetBiomeProvider().GetTopmostBlockValue(x, z);
		if (!blockValue.isair)
		{
			return blockValue.type;
		}
		float num;
		BiomeDefinition biomeAt = this.world.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt(x, z, out num);
		if (biomeAt == null)
		{
			return 1;
		}
		blockValue = biomeAt.m_Layers[0].m_Block.blockValue;
		if ((biomeAt.m_Layers[0].m_FillUpTo > 0 || biomeAt.m_Layers[0].m_FillUpToRg > 0) && biomeAt.m_Layers.Count > 1)
		{
			blockValue = biomeAt.m_Layers[1].m_Block.blockValue;
		}
		return blockValue.type;
	}

	public void SetFogOverride(Color _color = default(Color), float _density = -1f)
	{
		this.fogColorOverride = _color;
		this.fogDensityOverride = _density;
	}

	public void SetNightVision(float _brightness)
	{
		this.nightVisionBrightness = _brightness * 0.4f;
	}

	public static DynamicProperties Properties;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector2 dataAmbientEquatorScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector2 dataAmbientGroundScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector2 dataAmbientSkyScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector2 dataAmbientSkyDesat;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector2 dataAmbientMoon;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float dataAmbientInsideSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float dataAmbientInsideThreshold;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector2 dataAmbientInsideEquatorScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector2 dataAmbientInsideGroundScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector2 dataAmbientInsideSkyScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector2 dataFogPow;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector2 dataFogWater;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector3 dataFogWaterColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float dataTest;

	public const float cFogTransitionSpeed = 0.01f;

	public const float cBrightnessMin = 0.2f;

	public static float AmbientTotal;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float dayTimeScalar;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float insideCurrent;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Color fogColorOverride;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fogDensityOverride = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float brightnessInOutDayNight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float nightVisionBrightness;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isUnderWater;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ChunkCluster.OnChunkVisibleDelegate chunkClusterVisibleDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bTerrainActived;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int chunkLODIndex;
}
