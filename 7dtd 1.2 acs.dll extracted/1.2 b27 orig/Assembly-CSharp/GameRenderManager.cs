using System;
using System.Collections.Generic;
using FidelityFX.FSR3;
using HorizonBasedAmbientOcclusion;
using PI.NGSS;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class GameRenderManager
{
	public static GameRenderManager Create(EntityPlayerLocal player)
	{
		GameRenderManager gameRenderManager = new GameRenderManager();
		gameRenderManager.player = player;
		gameRenderManager.Init();
		return gameRenderManager;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		this.graphManager = GameGraphManager.Create(this.player);
		this.lightManager = GameLightManager.Create(this.player);
		this.reflectionManager = ReflectionManager.Create(this.player);
		this.PostProcessInit();
		this.DynamicResolutionInit();
	}

	public void Destroy()
	{
		this.lightManager.Destroy();
		this.lightManager = null;
		this.reflectionManager.Destroy();
		this.reflectionManager = null;
		this.DynamicResolutionDestroyRT();
	}

	public void FrameUpdate()
	{
		this.lightManager.FrameUpdate();
		this.reflectionManager.FrameUpdate();
		this.DynamicResolutionUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PostProcessInit()
	{
		Camera playerCamera = this.player.playerCamera;
	}

	public static int TextureMipmapLimit
	{
		get
		{
			return QualitySettings.globalTextureMipmapLimit;
		}
		set
		{
			QualitySettings.globalTextureMipmapLimit = value;
		}
	}

	public static void ApplyCameraOptions(EntityPlayerLocal player)
	{
		if (GameManager.Instance.World == null)
		{
			return;
		}
		if (player)
		{
			player.renderManager.ApplyCameraOptions();
			return;
		}
		List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
		for (int i = 0; i < localPlayers.Count; i++)
		{
			localPlayers[i].renderManager.ApplyCameraOptions();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyCameraOptions()
	{
		Camera playerCamera = this.player.playerCamera;
		PostProcessLayer component = playerCamera.GetComponent<PostProcessLayer>();
		playerCamera.depthTextureMode = (DepthTextureMode.Depth | DepthTextureMode.MotionVectors);
		NGSS_FrustumShadows_7DTD component2 = playerCamera.GetComponent<NGSS_FrustumShadows_7DTD>();
		switch (GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowQuality))
		{
		case 0:
		case 1:
		case 2:
			component2.enabled = false;
			break;
		case 3:
			component2.enabled = true;
			component2.m_shadowsBlurIterations = 1;
			component2.m_raySamples = 32;
			break;
		case 4:
			component2.enabled = true;
			component2.m_shadowsBlurIterations = 2;
			component2.m_raySamples = 48;
			break;
		case 5:
			component2.enabled = true;
			component2.m_shadowsBlurIterations = 4;
			component2.m_raySamples = 64;
			break;
		}
		int aaQuality = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxAA);
		float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxAASharpness);
		bool @bool = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxBloom);
		int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxSSReflections);
		bool bool2 = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxSSAO);
		bool bool3 = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxSunShafts);
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxMotionBlur);
		if (!GamePrefs.GetBool(EnumGamePrefs.OptionsGfxMotionBlurEnabled))
		{
			num = 0;
		}
		PostProcessVolume component3 = playerCamera.GetComponent<PostProcessVolume>();
		if (component3)
		{
			PostProcessProfile profile = component3.profile;
			if (profile)
			{
				component3.enabled = false;
				ScreenSpaceReflections setting = profile.GetSetting<ScreenSpaceReflections>();
				if (setting)
				{
					switch (@int)
					{
					case 1:
						setting.maximumIterationCount.Override(200);
						setting.resolution.Override(ScreenSpaceReflectionResolution.Downsampled);
						break;
					case 2:
						setting.maximumIterationCount.Override(120);
						setting.resolution.Override(ScreenSpaceReflectionResolution.FullSize);
						break;
					case 3:
						setting.maximumIterationCount.Override(250);
						setting.resolution.Override(ScreenSpaceReflectionResolution.FullSize);
						break;
					}
					setting.enabled.Override(@int > 0);
				}
				MotionBlur setting2 = profile.GetSetting<MotionBlur>();
				setting2.enabled.Override(num != 0);
				if (num != 1)
				{
					if (num == 2)
					{
						setting2.shutterAngle.Override(270f);
						setting2.sampleCount.Override(10);
					}
				}
				else
				{
					setting2.shutterAngle.Override(135f);
					setting2.sampleCount.Override(5);
				}
				profile.GetSetting<Bloom>().enabled.Override(@bool);
				ColorGrading setting3 = profile.GetSetting<ColorGrading>();
				float num2 = 0.5f - GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxBrightness);
				if (num2 < 0f)
				{
					num2 *= 0.4f;
				}
				else
				{
					num2 = 0f;
				}
				setting3.toneCurveGamma.Override(1f + num2);
				SunShaftsEffect sunShaftsEffect;
				if (profile.TryGetSettings<SunShaftsEffect>(out sunShaftsEffect))
				{
					sunShaftsEffect.enabled.Override(bool3);
				}
				component3.enabled = true;
			}
		}
		HBAO component4 = playerCamera.GetComponent<HBAO>();
		if (component4)
		{
			switch (GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset))
			{
			case 0:
			case 1:
			case 2:
				component4.SetQuality(HBAO.Quality.Low);
				break;
			case 3:
				component4.SetQuality(HBAO.Quality.Medium);
				break;
			case 4:
				component4.SetQuality(HBAO.Quality.High);
				break;
			}
			component4.enabled = bool2;
		}
		int int2 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxDynamicMode);
		int int3 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxDynamicMinFPS);
		float scale = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxDynamicScale);
		if (int2 == 0)
		{
			scale = -1f;
		}
		if (int2 == 1)
		{
			scale = 0f;
		}
		this.SetDynamicResolution(scale, (float)int3, -1f);
		if (this.dlssEnabled)
		{
			aaQuality = 0;
		}
		this.FSRInit(component.superResolution);
		if (component)
		{
			this.SetAntialiasing(aaQuality, @float, component);
			Rect rect = playerCamera.rect;
			rect.x = ((component.antialiasingMode == PostProcessLayer.Antialiasing.SuperResolution) ? 1E-07f : 0f);
			playerCamera.rect = rect;
		}
		this.reflectionManager.ApplyCameraOptions(playerCamera);
	}

	public void SetAntialiasing(int aaQuality, float sharpness, PostProcessLayer mainLayer)
	{
		if (aaQuality == 0)
		{
			mainLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
			this.FSRSetQuality(-1);
			return;
		}
		if (aaQuality <= 3)
		{
			if (aaQuality == 1)
			{
				mainLayer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
				mainLayer.fastApproximateAntialiasing.fastMode = false;
			}
			else if (aaQuality == 2)
			{
				mainLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
				mainLayer.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.Medium;
			}
			else
			{
				mainLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
				mainLayer.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.High;
			}
		}
		else if (aaQuality == 4 || !mainLayer.superResolution.IsSupported())
		{
			mainLayer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
			mainLayer.temporalAntialiasing.jitterSpread = 0.35f;
			mainLayer.temporalAntialiasing.stationaryBlending = 0.8f;
			mainLayer.temporalAntialiasing.motionBlending = 0.75f;
			mainLayer.temporalAntialiasing.sharpness = sharpness * 0.2f;
		}
		else
		{
			mainLayer.antialiasingMode = PostProcessLayer.Antialiasing.SuperResolution;
			mainLayer.superResolution.sharpness = sharpness;
		}
		this.FSRSetQuality(aaQuality - 5);
	}

	public void DynamicResolutionInit()
	{
		this.DynamicResolutionDestroyRT();
		Camera playerCamera = this.player.playerCamera;
		Camera finalCamera = this.player.finalCamera;
		bool flag = finalCamera != playerCamera;
		if (!GameRenderManager.dynamicIsEnabled)
		{
			if (flag)
			{
				UnityEngine.Object.Destroy(finalCamera.gameObject);
			}
			this.player.finalCamera = playerCamera;
		}
		else
		{
			if (!flag)
			{
				this.AddFinalCameraToPlayer();
			}
			this.DynamicResolutionAllocRTs();
		}
		this.DLSSInit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddFinalCameraToPlayer()
	{
		GameObject gameObject = new GameObject("FinalCamera");
		gameObject.transform.SetParent(this.player.cameraTransform, false);
		Camera camera = gameObject.AddComponent<Camera>();
		this.player.finalCamera = camera;
		camera.clearFlags = CameraClearFlags.Nothing;
		camera.cullingMask = 0;
		camera.depth = -0.1f;
		gameObject.AddComponent<LocalPlayerFinalCamera>().entityPlayerLocal = this.player;
	}

	public void DynamicResolutionUpdate()
	{
		if (GameManager.Instance.World == null)
		{
			return;
		}
		if (!GameRenderManager.dynamicIsEnabled)
		{
			return;
		}
		if (Screen.width != this.dynamicScreenW)
		{
			this.DynamicResolutionAllocRTs();
			return;
		}
		if (this.dynamicScaleOverride > 0f)
		{
			return;
		}
		if (this.dynamicUpdateDelay > 0f)
		{
			this.dynamicUpdateDelay -= Time.deltaTime;
			return;
		}
		float num = Time.deltaTime + 0.001f;
		this.dynamicFPS = this.dynamicFPS * 0.5f + 0.5f / num;
		float num2 = 0.1f * num;
		if (this.dynamicFPS < this.dynamicFPSTargetMin)
		{
			this.dynamicScaleTarget -= num2;
			if (this.dynamicScaleTarget < 0.4f)
			{
				this.dynamicScaleTarget = 0.4f;
			}
		}
		else
		{
			this.dynamicScaleTarget += num2 * 0.2f;
			if (this.dynamicFPS > this.dynamicFPSTargetMax)
			{
				this.dynamicScaleTarget += num2;
			}
			if (this.dynamicScaleTarget > 1f)
			{
				this.dynamicScaleTarget = 1f;
			}
		}
		if (this.dynamicScaleTarget < 1f || this.dynamicScale >= 1f)
		{
			float num3 = this.dynamicScaleTarget - this.dynamicScale;
			if (num3 > -0.049f && num3 < 0.049f)
			{
				return;
			}
		}
		this.dynamicScale = this.dynamicScaleTarget;
		RenderTexture y = null;
		for (int i = 0; i < this.dynamicRTs.Length; i++)
		{
			y = this.dynamicRTs[i];
			float num4 = (this.dynamicScales[i] + this.dynamicScales[i + 1]) * 0.5f;
			if (this.dynamicScale >= num4)
			{
				break;
			}
		}
		if (this.dynamicRT == y)
		{
			return;
		}
		this.dynamicRT = y;
	}

	public bool DynamicResolutionUpdateGraph(ref float value)
	{
		if (this.dynamicRT != null)
		{
			float num = (float)this.dynamicRT.width / (float)Screen.width;
			if (num != value)
			{
				value = num;
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DynamicResolutionAllocRTs()
	{
		this.DynamicResolutionDestroyRT();
		this.dynamicScreenW = Screen.width;
		int num = (this.dynamicScaleOverride > 0f) ? 1 : 4;
		this.dynamicRTs = new RenderTexture[num];
		for (int i = 0; i < num; i++)
		{
			float scale = this.dynamicScales[i];
			if (this.dynamicScaleOverride > 0f)
			{
				scale = this.dynamicScaleOverride;
			}
			RenderTexture renderTexture = this.DynamicResolutionAllocRT(scale);
			this.dynamicRTs[i] = renderTexture;
		}
		this.dynamicRT = this.dynamicRTs[0];
		this.dynamicScale = 1f;
		this.dynamicScaleTarget = 1f;
		this.dynamicUpdateDelay = 18f;
	}

	public RenderTexture DynamicResolutionAllocRT(float scale)
	{
		int num = (int)((float)Screen.width * scale);
		int num2 = (int)((float)Screen.height * scale);
		RenderTexture renderTexture = new RenderTexture(num, num2, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		renderTexture.name = string.Format("DynRT{0}x{1}", num, num2);
		Log.Out("DynamicResolutionAllocRT scale {0}, Tex {1}x{2}", new object[]
		{
			scale,
			num,
			num2
		});
		return renderTexture;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DynamicResolutionDestroyRT()
	{
		List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
		for (int i = 0; i < localPlayers.Count; i++)
		{
			localPlayers[i].playerCamera.targetTexture = null;
		}
		if (this.dynamicRTs != null)
		{
			for (int j = 0; j < this.dynamicRTs.Length; j++)
			{
				this.dynamicRTs[j].Release();
				UnityEngine.Object.Destroy(this.dynamicRTs[j]);
			}
			this.dynamicRTs = null;
		}
		this.dynamicRT = null;
	}

	public void SetDynamicResolution(float scale, float fpsMin, float fpsMax)
	{
		GameRenderManager.dynamicIsEnabled = (scale >= 0f);
		int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxVsync);
		this.dynamicFPSTargetMin = fpsMin;
		if (fpsMin < 0f)
		{
			this.dynamicFPSTargetMin = 30f;
		}
		if (@int > 0)
		{
			this.dynamicFPSTargetMin = Utils.FastMin(30f, this.dynamicFPSTargetMin);
		}
		if (@int > 1)
		{
			this.dynamicFPSTargetMin = Utils.FastMin(18f, this.dynamicFPSTargetMin);
		}
		this.dynamicFPSTargetMax = fpsMax;
		if (fpsMax < 0f)
		{
			this.dynamicFPSTargetMax = 64f;
			if (@int > 0)
			{
				this.dynamicFPSTargetMax = 55f;
			}
			if (@int > 1)
			{
				this.dynamicFPSTargetMax = 25f;
			}
		}
		this.dynamicScaleOverride = scale;
		if (this.dynamicScaleOverride > 0f)
		{
			this.dynamicScaleOverride = Mathf.Clamp(this.dynamicScaleOverride, 0.1f, 1f);
			this.dynamicScale = this.dynamicScaleOverride;
		}
		this.DynamicResolutionInit();
	}

	public RenderTexture GetDynamicRenderTexture()
	{
		return this.dynamicRT;
	}

	public void DynamicResolutionRender()
	{
		if (!this.dlssEnabled)
		{
			Graphics.Blit(this.GetDynamicRenderTexture(), null);
			return;
		}
		this.DLSSRender();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FSRInit(SuperResolution _sr)
	{
		this.fsr = _sr;
		this.fsr.callbacksFactory = ((PostProcessRenderContext context) => new GameRenderManager.FSRCallback());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FSRSetQuality(int _quality)
	{
		if (_quality < 0)
		{
			if (this.fsrEnabled)
			{
				this.fsrEnabled = false;
				this.SetMipmapBias(0f);
			}
			return;
		}
		this.fsrEnabled = true;
		this.mipmapTextureMem = 0UL;
		SuperResolution superResolution = this.fsr;
		Fsr3Upscaler.QualityMode qualityMode;
		switch (_quality)
		{
		case 0:
			qualityMode = Fsr3Upscaler.QualityMode.UltraPerformance;
			break;
		case 1:
			qualityMode = Fsr3Upscaler.QualityMode.Performance;
			break;
		case 2:
			qualityMode = Fsr3Upscaler.QualityMode.Balanced;
			break;
		case 3:
			qualityMode = Fsr3Upscaler.QualityMode.Quality;
			break;
		case 4:
			qualityMode = Fsr3Upscaler.QualityMode.UltraQuality;
			break;
		default:
			qualityMode = Fsr3Upscaler.QualityMode.NativeAA;
			break;
		}
		superResolution.qualityMode = qualityMode;
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore && SystemInfo.graphicsDeviceVendor.ToLower().Contains("nvidia"))
		{
			this.fsr.exposureSource = SuperResolution.ExposureSource.Default;
		}
	}

	public void FSRPreCull()
	{
		if (!this.fsrEnabled)
		{
			return;
		}
		this.UpdateMipmaps((float)this.fsr.renderSize.x / (float)Screen.width);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DLSSInit()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsDLSSSupported()
	{
		return false;
	}

	public void DLSSPreCull()
	{
	}

	public void DLSSSetupCommandBuffer()
	{
	}

	public void DLSSRender()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateMipmaps(float _renderToScreenRatio)
	{
		this.mipmapDelay -= Time.deltaTime;
		if (this.mipmapDelay <= 0f)
		{
			this.mipmapDelay = 2f;
			ulong currentTextureMemory = Texture.currentTextureMemory;
			if (this.mipmapTextureMem != currentTextureMemory)
			{
				this.mipmapTextureMem = currentTextureMemory;
				float num = 1f;
				num *= Mathf.Log(_renderToScreenRatio, 2f) - 1f;
				this.SetMipmapBias(num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMipmapBias(float _bias)
	{
		Texture2D[] array = Resources.FindObjectsOfTypeAll(typeof(Texture2D)) as Texture2D[];
		for (int i = 0; i < array.Length; i++)
		{
			array[i].mipMapBias = _bias;
		}
		Texture2DArray[] array2 = Resources.FindObjectsOfTypeAll(typeof(Texture2DArray)) as Texture2DArray[];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].mipMapBias = _bias;
		}
	}

	public void OnGUI()
	{
		this.graphManager.Draw();
	}

	public bool FPSUpdateGraph(ref float value)
	{
		value = 1f / (Time.deltaTime + 0.001f);
		return true;
	}

	public bool SPFUpdateGraph(ref float value)
	{
		value = (Time.deltaTime + 0.0001f) * 1000f;
		return true;
	}

	public GameGraphManager graphManager;

	public GameLightManager lightManager;

	public ReflectionManager reflectionManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicUpdateDelay = 18f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicChangeSeconds = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicFPSMin = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicFPSMax = 64f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicFPSVSyncMin = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicFPSVSyncMax = 55f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicScaleMin = 0.4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicScaleThreshold = 0.049f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDynamicRTCount = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly float[] dynamicScales = new float[]
	{
		1f,
		0.75f,
		0.62f,
		0.5f,
		0f
	};

	public static bool dynamicIsEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dynamicUpdateDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dynamicFPSTargetMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dynamicFPSTargetMax = 64f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dynamicFPS = 60f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dynamicScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dynamicScaleTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dynamicScaleOverride;

	[PublicizedFrom(EAccessModifier.Private)]
	public int dynamicScreenW;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture dynamicRT;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture[] dynamicRTs;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fsrEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public SuperResolution fsr;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dlssEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public float mipmapDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong mipmapTextureMem;

	public class FSRCallback : IFsr3UpscalerCallbacks
	{
		public virtual void ApplyMipmapBias(float biasOffset)
		{
		}

		public virtual void UndoMipmapBias()
		{
		}
	}
}
