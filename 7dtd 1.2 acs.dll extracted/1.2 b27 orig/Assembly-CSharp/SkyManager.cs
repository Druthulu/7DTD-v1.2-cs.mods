using System;
using PI.NGSS;
using UnityEngine;

public class SkyManager : MonoBehaviour
{
	public static Transform SunLightT
	{
		get
		{
			return SkyManager.sunLightT;
		}
	}

	public static bool IsBloodMoonVisible()
	{
		int num = (int)SkyManager.dayCount;
		int @int = GameStats.GetInt(EnumGameStats.BloodMoonDay);
		return (num == @int && SkyManager.TimeOfDay() >= 18f) || (num > 1 && num == @int + 1 && SkyManager.TimeOfDay() <= 6f);
	}

	public static float BloodMoonVisiblePercent()
	{
		int num = (int)SkyManager.dayCount;
		int @int = GameStats.GetInt(EnumGameStats.BloodMoonDay);
		float num2 = SkyManager.TimeOfDay();
		if (num != @int)
		{
			return (float)((num > 1 && num == @int + 1 && num2 <= 6f) ? 1 : 0);
		}
		float num3 = 22f - num2;
		if (num3 < 0f)
		{
			return 1f;
		}
		float num4 = 1f - num3 / 4f;
		if (num4 >= 0f)
		{
			return num4;
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		GameOptionsManager.ShadowDistanceChanged += this.OnShadowDistanceChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		GameOptionsManager.ShadowDistanceChanged -= this.OnShadowDistanceChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnShadowDistanceChanged(int optionsShadowDistance)
	{
		int shadowCustomResolution;
		switch (optionsShadowDistance)
		{
		case 0:
		case 1:
		case 2:
			shadowCustomResolution = 1024;
			goto IL_36;
		case 3:
			shadowCustomResolution = 2048;
			goto IL_36;
		}
		shadowCustomResolution = 4096;
		IL_36:
		SkyManager.sunLight.shadowCustomResolution = shadowCustomResolution;
		SkyManager.moonLight.shadowCustomResolution = shadowCustomResolution;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		SkyManager.Reset();
	}

	public static void Loaded(GameObject _obj)
	{
		_obj.SetActive(true);
		_obj.transform.parent = GameManager.Instance.transform;
		SkyManager.skyManager = _obj.GetComponent<SkyManager>();
		SkyManager.Reset();
	}

	public static void Cleanup()
	{
		if (SkyManager.skyManager != null)
		{
			UnityEngine.Object.DestroyImmediate(SkyManager.skyManager.gameObject);
		}
	}

	public static void SetSkyEnabled(bool _enabled)
	{
		if (_enabled)
		{
			SkyManager.SetFogDebug(-1f, float.MinValue, float.MinValue);
			SkyManager.mainCamera.backgroundColor = Color.black;
		}
		else
		{
			SkyManager.SetFogDebug(0f, float.MinValue, float.MinValue);
			SkyManager.mainCamera.backgroundColor = new Color(0.44f, 0.48f, 0.52f);
		}
		SkyManager.atmosphereSphere.gameObject.SetActive(_enabled);
		SkyManager.cloudsSphere.gameObject.SetActive(_enabled);
	}

	public static Color GetSkyColor()
	{
		return SkyManager.SkyColor;
	}

	public static void SetSkyColor(Color c)
	{
		SkyManager.SkyColor = c;
	}

	public static void SetGameTime(ulong _time)
	{
		SkyManager.dayCount = _time / 24000f + 1f;
		SkyManager.timeOfDay = _time;
	}

	public static float TimeOfDay()
	{
		return SkyManager.timeOfDay % 24000UL / 1000f;
	}

	public static float GetTimeOfDayAsMinutes()
	{
		return SkyManager.TimeOfDay() / 24f * (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
	}

	public static void SetCloudTextures(Texture _mainTex, Texture _blendTex)
	{
		SkyManager.cloudMainTex = _mainTex;
		SkyManager.cloudBlendTex = _blendTex;
	}

	public static void SetCloudTransition(float t)
	{
		SkyManager.cloudTransition = t;
	}

	public static Color GetFogColor()
	{
		return SkyManager.fogColor;
	}

	public static void SetFogColor(Color c)
	{
		if (SkyManager.fogDebugColor.a > 0f)
		{
			c = SkyManager.fogDebugColor;
		}
		SkyManager.fogColor = c;
	}

	public static float GetFogDensity()
	{
		return SkyManager.fogDensity;
	}

	public static void SetFogDensity(float density)
	{
		if (SkyManager.fogDebugDensity >= 0f)
		{
			density = SkyManager.fogDebugDensity;
		}
		SkyManager.fogDensity = density;
		float num = density - 0.65f;
		if (num < 0f)
		{
			num = 0f;
		}
		SkyManager.fogLightScale = 1f - num * 1.7f;
	}

	public static float GetFogStart()
	{
		return SkyManager.fogStart;
	}

	public static float GetFogEnd()
	{
		return SkyManager.fogEnd;
	}

	public static void SetFogFade(float start, float end)
	{
		float t = 1f;
		World world = GameManager.Instance.World;
		if (world != null)
		{
			t = (world.GetPrimaryPlayer().bPlayingSpawnIn ? 1f : 0.01f);
		}
		SkyManager.fogStart = Mathf.Lerp(SkyManager.fogStart, start, t);
		SkyManager.fogEnd = Mathf.Lerp(SkyManager.fogEnd, end, t);
		if (SkyManager.fogDebugDensity >= 0f)
		{
			if (SkyManager.fogDebugStart > -1000f)
			{
				SkyManager.fogStart = SkyManager.fogDebugStart;
			}
			if (SkyManager.fogDebugEnd > -1000f)
			{
				SkyManager.fogEnd = SkyManager.fogDebugEnd;
			}
		}
	}

	public static void SetFogDebug(float density = -1f, float start = -3.40282347E+38f, float end = -3.40282347E+38f)
	{
		SkyManager.fogDebugDensity = density;
		SkyManager.fogDebugStart = start;
		SkyManager.fogDebugEnd = end;
		SkyManager.SetFogDensity(0f);
	}

	public static void SetFogDebugColor(Color color = default(Color))
	{
		SkyManager.fogDebugColor = color;
		SkyManager.SetFogColor(Color.gray);
	}

	public static void Reset()
	{
		SkyManager.bNeedsReset = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearStatics()
	{
		SkyManager.random = null;
		SkyManager.cloudTransition = 1f;
		SkyManager.cloudMainTexOld = null;
		SkyManager.cloudBlendTex = null;
		SkyManager.parent = null;
		SkyManager.sunLightT = null;
		SkyManager.sunLight = null;
		SkyManager.moonLightT = null;
		SkyManager.moonLight = null;
		SkyManager.moonSpriteT = null;
		SkyManager.mainCamera = null;
		SkyManager.moonSpriteMat = null;
		SkyManager.cloudsSphere = null;
		SkyManager.cloudsSphereMtrl = null;
		SkyManager.atmosphereSphere = null;
		SkyManager.atmosphereMtrl = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		SkyManager.random = GameRandomManager.Instance.CreateGameRandom();
		RenderSettings.fog = true;
		RenderSettings.fogMode = FogMode.Exponential;
		this.sunAxis = Vector3.forward;
		this.sunStartV = Quaternion.AngleAxis(20f, Vector3.up) * Vector3.left;
		this.moonStartV = Quaternion.AngleAxis(35f, Vector3.up) * Vector3.right;
		this.starAxis = Vector3.forward;
		if (SkyManager.parent == null)
		{
			SkyManager.parent = GameObject.Find("SkySystem(Clone)");
			if (SkyManager.parent == null)
			{
				return;
			}
		}
		if (SkyManager.sunLightT == null)
		{
			Transform x = SkyManager.parent.transform.Find("SunLight");
			if (x != null)
			{
				SkyManager.sunLightT = x;
			}
		}
		if (SkyManager.sunLight == null && SkyManager.sunLightT != null)
		{
			SkyManager.sunLight = SkyManager.sunLightT.transform.GetComponent<Light>();
		}
		if (SkyManager.moonLightT == null)
		{
			SkyManager.moonLightT = SkyManager.parent.transform.Find("MoonLight");
			if (SkyManager.moonLightT)
			{
				SkyManager.moonLight = SkyManager.moonLightT.GetComponent<Light>();
			}
		}
		SkyManager.GetMaterialAndTransform("MoonSprite", ref SkyManager.moonSpriteT, ref SkyManager.moonSpriteMat);
		SkyManager.GetMaterialAndTransform("CloudsSphere", ref SkyManager.cloudsSphere, ref SkyManager.cloudsSphereMtrl);
		SkyManager.GetMaterialAndTransform("AtmosphereSphere", ref SkyManager.atmosphereSphere, ref SkyManager.atmosphereMtrl);
		MeshFilter component = SkyManager.moonSpriteT.GetComponent<MeshFilter>();
		if (component != null)
		{
			Mesh mesh = component.mesh;
			if (mesh != null)
			{
				mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000000f);
			}
		}
		if (SkyManager.cloudsSphereMtrl)
		{
			SkyManager.cloudsSphereMtrl.SetFloat("_CloudSpeed", this.cloudSpeed);
		}
		if (SkyManager.mainCamera == null)
		{
			this.GetMainCamera();
		}
		this.OnShadowDistanceChanged(GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GetMaterialAndTransform(string objectName, ref Transform trans, ref Material mtrl)
	{
		Transform transform = SkyManager.parent.transform.Find(objectName);
		if (trans == null)
		{
			trans = transform;
		}
		if (mtrl == null)
		{
			MeshRenderer component = transform.GetComponent<MeshRenderer>();
			mtrl = component.material;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetMainCamera()
	{
		SkyManager.mainCamera = Camera.main;
	}

	public static bool IsDark()
	{
		return SkyManager.TimeOfDay() < SkyManager.dawnTime || SkyManager.TimeOfDay() > SkyManager.duskTime;
	}

	public static float GetDawnTime()
	{
		return SkyManager.dawnTime;
	}

	public static float GetDawnTimeAsMinutes()
	{
		return SkyManager.dawnTime / 24f * (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
	}

	public static float GetDuskTime()
	{
		return SkyManager.duskTime;
	}

	public static float GetDuskTimeAsMinutes()
	{
		return SkyManager.duskTime / 24f * (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float CalcDayPercent()
	{
		float num;
		if (SkyManager.worldRotation < 0.5f)
		{
			num = Mathf.Pow(1f - Utils.FastAbs(0.25f - SkyManager.worldRotation) * 4f, 0.6f);
			num = num * 0.68f + 0.5f;
			if (num > 1f)
			{
				num = 1f;
			}
		}
		else
		{
			num = Mathf.Pow(1f - Utils.FastAbs(0.75f - SkyManager.worldRotation) * 4f, 0.6f);
			num = 0.5f - num * 0.68f;
			if (num < 0f)
			{
				num = 0f;
			}
		}
		return num;
	}

	public static void TriggerLightning(Vector3 _position)
	{
		if (!SkyManager.triggerLightning)
		{
			SkyManager.lightningFlashes = SkyManager.random.RandomRange(3, 5);
			float num = 0f;
			for (int i = 1; i < SkyManager.lightningFlashes; i++)
			{
				float num2 = SkyManager.random.RandomRange(0.07f, 0.2f);
				num += num2;
				SkyManager.lightningTimes[i] = num;
			}
			SkyManager.lightningIndex = 0;
			SkyManager.lightningStartTime = Time.time;
			SkyManager.lightningPosition = _position;
			SkyManager.lightningDir = SkyManager.random.RandomFloat * 360f;
			SkyManager.lightningFrameCount = 2;
			SkyManager.triggerLightning = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSunMoonAngles()
	{
		if (!SkyManager.sunLight)
		{
			return;
		}
		if (!SkyManager.moonLight)
		{
			return;
		}
		int @int = GameStats.GetInt(EnumGameStats.DayLightLength);
		SkyManager.duskTime = 22f;
		if (@int > 22)
		{
			SkyManager.duskTime = Mathf.Clamp((float)@int, 0f, 23.999f);
		}
		SkyManager.dawnTime = Mathf.Clamp(SkyManager.duskTime - (float)@int, 0f, 23.999f);
		if (Time.time - SkyManager.worldRotationTime >= 0.2f || SkyManager.bUpdateSunMoonNow)
		{
			SkyManager.worldRotationTime = Time.time;
			SkyManager.bUpdateSunMoonNow = false;
			float num = SkyManager.TimeOfDay();
			if (num >= SkyManager.dawnTime && num < SkyManager.duskTime)
			{
				SkyManager.worldRotationTarget = (num - SkyManager.dawnTime) / (SkyManager.duskTime - SkyManager.dawnTime);
			}
			else
			{
				float num2 = 24f - SkyManager.duskTime;
				float num3 = num2 + SkyManager.dawnTime;
				if (num < SkyManager.dawnTime)
				{
					SkyManager.worldRotationTarget = (num2 + num) / num3;
				}
				else
				{
					SkyManager.worldRotationTarget = (num - SkyManager.duskTime) / num3;
				}
				SkyManager.worldRotationTarget += 1f;
			}
			SkyManager.worldRotationTarget *= 0.5f;
			SkyManager.worldRotationTarget = Mathf.Clamp01(SkyManager.worldRotationTarget);
		}
		float num4 = SkyManager.worldRotationTarget - SkyManager.worldRotation;
		float num5 = SkyManager.worldRotationTarget;
		if (num4 < -0.5f)
		{
			num5 += 1f;
		}
		else if (num4 > 0.5f)
		{
			num5 -= 1f;
		}
		SkyManager.worldRotation = Mathf.Lerp(SkyManager.worldRotation, num5, 0.05f);
		if (SkyManager.worldRotation < 0f)
		{
			SkyManager.worldRotation += 1f;
		}
		else if (SkyManager.worldRotation >= 1f)
		{
			SkyManager.worldRotation -= 1f;
		}
		SkyManager.dayPercent = SkyManager.CalcDayPercent();
		float angle = SkyManager.worldRotation * 360f;
		SkyManager.sunDirV = Quaternion.AngleAxis(angle, this.sunAxis) * this.sunStartV;
		SkyManager.moonLightRot = Quaternion.LookRotation(Quaternion.AngleAxis(angle, this.sunAxis) * this.moonStartV);
		float num6 = SkyManager.worldRotation * 360f;
		if (SkyManager.sunIntensity >= 0.001f)
		{
			if (num6 < 14f)
			{
				num6 = 14f;
			}
			if (num6 > 166f)
			{
				num6 = 166f;
			}
			Vector3 eulerAngles = Quaternion.LookRotation(Quaternion.AngleAxis(num6, this.sunAxis) * this.sunStartV).eulerAngles;
			SkyManager.sunLightT.localEulerAngles = eulerAngles;
			SkyManager.sunLight.shadowStrength = 1f;
			SkyManager.sunLight.shadows = ((SkyManager.sunIntensity > 0f) ? LightShadows.Soft : LightShadows.None);
			SkyManager.moonLight.enabled = false;
		}
		else if (SkyManager.moonLightColor.grayscale > 0f)
		{
			if (num6 < 166f)
			{
				num6 = 166f;
			}
			if (num6 > 346f)
			{
				num6 = 346f;
			}
			Vector3 eulerAngles2 = Quaternion.LookRotation(Quaternion.AngleAxis(num6, this.sunAxis) * this.moonStartV).eulerAngles;
			SkyManager.moonLightT.localEulerAngles = eulerAngles2;
			float num7 = SkyManager.fogLightScale * SkyManager.moonBright;
			float t = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxBrightness) * 2f;
			num7 *= Utils.FastLerp(0.2f, 1f, t);
			SkyManager.moonLight.intensity = num7;
			SkyManager.moonLight.color = SkyManager.moonLightColor;
			SkyManager.moonLight.shadowStrength = 1f;
			SkyManager.moonLight.shadows = ((num7 > 0f) ? LightShadows.Soft : LightShadows.None);
			SkyManager.moonLight.enabled = true;
		}
		else
		{
			SkyManager.moonLight.enabled = false;
		}
		SkyManager.sunMoonDirV = SkyManager.sunDirV;
		if (SkyManager.sunIntensity < 0.001f)
		{
			SkyManager.sunMoonDirV = SkyManager.moonLightRot * Vector3.forward;
		}
		if (!GameManager.IsDedicatedServer && SkyManager.mainCamera)
		{
			Vector3 position = SkyManager.mainCamera.transform.position;
			if (SkyManager.moonSpriteT)
			{
				SkyManager.moonSpriteT.position = SkyManager.moonLightRot * Vector3.forward * -45000f;
				SkyManager.moonSpriteT.rotation = Quaternion.LookRotation(SkyManager.moonSpriteT.position, Vector3.up);
				SkyManager.moonSpriteT.position += position;
				float num8 = 6857.143f;
				if (SkyManager.IsBloodMoonVisible())
				{
					num8 *= 1.3f;
				}
				SkyManager.moonSpriteT.localScale = new Vector3(num8, num8, num8);
			}
			this.UpdateSunShaftSettings();
		}
		SkyManager.atmosphereSphere.Rotate(this.starAxis, SkyManager.worldRotation * 0.004f);
		if (this.bUpdateShaders && SkyManager.cloudsSphereMtrl)
		{
			SkyManager.cloudsSphereMtrl.SetVector("_SunDir", SkyManager.sunDirV);
			SkyManager.cloudsSphereMtrl.SetVector("_SunMoonDir", SkyManager.sunMoonDirV);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSunShaftSettings()
	{
		Vector3 vector = SkyManager.GetSunDirection();
		if (vector.y <= 0.09f)
		{
			SkyManager.sunShaftSettings.sunShaftIntensity = 0.1f + SkyManager.GetFogDensity() * 0.5f;
			SkyManager.sunShaftSettings.sunColor = SkyManager.GetSunLightColor();
			SkyManager.sunShaftSettings.sunThreshold = new Color(0.87f, 0.74f, 0.65f, 1f);
		}
		else
		{
			vector = SkyManager.GetMoonDirection();
			SkyManager.sunShaftSettings.sunShaftIntensity = 0.06f + SkyManager.GetFogDensity() * 0.85f;
			SkyManager.sunShaftSettings.sunColor = SkyManager.GetMoonLightColor();
			SkyManager.sunShaftSettings.sunThreshold = new Color(0.8f, 0.6f, 0.6f, 1f);
		}
		SkyManager.sunShaftSettings.sunPosition = vector * -100000f;
	}

	public static SunShaftsEffect.SunSettings GetSunShaftSettings()
	{
		return SkyManager.sunShaftSettings;
	}

	public static float GetLuma(Color color)
	{
		return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool VerifyCamera()
	{
		if (GameManager.IsDedicatedServer)
		{
			return true;
		}
		if (SkyManager.mainCamera == null)
		{
			this.GetMainCamera();
			if (SkyManager.mainCamera == null)
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool VerifyValidMaterials()
	{
		return !(SkyManager.atmosphereMtrl == null) && !(SkyManager.cloudsSphere == null) && !(SkyManager.atmosphereSphere == null) && !(SkyManager.moonLight == null) && !(SkyManager.sunLight == null) && !(SkyManager.moonSpriteT == null) && !(SkyManager.moonSpriteMat == null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetIfNeeded()
	{
		if (SkyManager.bNeedsReset)
		{
			this.ClearStatics();
			this.Init();
			SkyManager.bNeedsReset = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateFogShader()
	{
		this.fogParams.x = SkyManager.GetFogStart();
		this.fogParams.y = SkyManager.GetFogEnd();
		this.fogParams.z = 1f;
		this.fogParams.w = Mathf.Pow(SkyManager.GetFogDensity(), 2f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateShaderGlobals()
	{
		Color c = SkyManager.GetFogColor();
		float num = SkyManager.GetFogDensity();
		num *= num * num;
		RenderSettings.fogDensity = num;
		RenderSettings.fogColor = c;
		Shader.SetGlobalVector("_FogParams", new Vector4(num, this.fogParams.x, this.fogParams.y, 0f));
		Shader.SetGlobalColor("_FogColor", c.linear);
		Shader.SetGlobalVector("SunColor", SkyManager.sunLight.color);
		Shader.SetGlobalVector("FogColor", c);
		Shader.SetGlobalFloat("_HighResViewDistance", (float)GameUtils.GetViewDistance() * 16f);
		Shader.SetGlobalFloat("_DayPercent", SkyManager.dayPercent);
	}

	public void Update()
	{
		if (SkyManager.parent == null)
		{
			this.Init();
			if (SkyManager.parent == null)
			{
				return;
			}
		}
		SkyManager.sMaxSunIntensity = this.maxSunIntensity;
		if (SkyManager.mainCamera)
		{
			float num = SkyManager.fogDensity + 0.001f;
			num *= SkyManager.fogDensity * SkyManager.fogDensity;
			SkyManager.mainCamera.farClipPlane = Utils.FastClamp(6f / num + 400f, 200f, 2800f);
		}
		int num2 = Time.frameCount & 1;
		this.bUpdateShaders = (num2 == 0 || SkyManager.triggerLightning);
		if (!this.VerifyCamera())
		{
			return;
		}
		this.ResetIfNeeded();
		this.UpdateSunMoonAngles();
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		float magnitude = (SkyManager.parent.transform.position - SkyManager.mainCamera.transform.position).magnitude;
		if (magnitude < 0.001f)
		{
			this.bMovingSky = false;
			this.targetSkyPosition = SkyManager.mainCamera.transform.position;
		}
		else if (!this.bMovingSky && magnitude > 5f)
		{
			this.bMovingSky = true;
			this.targetSkyPosition = SkyManager.mainCamera.transform.position;
		}
		if (this.bMovingSky)
		{
			SkyManager.parent.transform.position = Vector3.Lerp(SkyManager.parent.transform.position, this.targetSkyPosition, Mathf.Clamp01(Time.deltaTime * 10f));
		}
		this.UpdateFogShader();
		if (!this.bUpdateShaders)
		{
			return;
		}
		if (!this.VerifyValidMaterials())
		{
			return;
		}
		SkyManager.sSunFadeHeight = this.sunFadeHeight;
		this.UpdateShaderGlobals();
		bool flag = SkyManager.IsBloodMoonVisible();
		int num3 = 0;
		SkyManager.moonSpriteColor = new Color(1f, 0.14f, 0.05f) * 1.5f;
		if (!flag)
		{
			num3 = (int)(SkyManager.dayCount + 5.5f) % 7;
			SkyManager.moonSpriteColor = Color.Lerp(Color.white, SkyManager.moonLightColor, 0.2f);
		}
		float num4 = SkyManager.moonPhases[num3];
		float f = num4 * 3.14159274f;
		Vector3 v = new Vector3(-Mathf.Sin(f), 0f, Mathf.Cos(f));
		SkyManager.moonSpriteMat.SetVector("_LightDir", v);
		SkyManager.moonSpriteMat.SetColor("_Color", SkyManager.moonSpriteColor);
		SkyManager.moonBright = 1f - num4;
		if (SkyManager.moonBright < 0f)
		{
			SkyManager.moonBright = -SkyManager.moonBright;
		}
		float b = ((Mathf.Pow(SkyManager.moonLightColor.grayscale, 0.45f) - 0.5f) * 0.5f + 0.5f) * SkyManager.moonBright;
		float value = Mathf.Max(SkyManager.sunIntensity, b);
		if (WeatherManager.currentWeather != null)
		{
			SkyManager.starIntensity = 1f - WeatherManager.currentWeather.CloudThickness() * 0.01f;
		}
		if (this.bUpdateShaders)
		{
			SkyManager.atmosphereMtrl.SetColor("_SkyColor", SkyManager.GetSkyColor());
			SkyManager.atmosphereMtrl.SetFloat("_Stars", SkyManager.starIntensity);
			SkyManager.atmosphereMtrl.SetVector("_SunDir", SkyManager.sunDirV);
			SkyManager.cloudsSphereMtrl.SetFloat("_CloudTransition", SkyManager.cloudTransition);
			SkyManager.cloudsSphereMtrl.SetFloat("_LightIntensity", value);
			SkyManager.cloudsSphereMtrl.SetColor("_SkyColor", SkyManager.GetSkyColor());
			SkyManager.cloudsSphereMtrl.SetColor("_SunColor", SkyManager.sunLight.color);
			SkyManager.cloudsSphereMtrl.SetVector("_SunDir", SkyManager.sunDirV);
			SkyManager.cloudsSphereMtrl.SetVector("_SunMoonDir", SkyManager.sunMoonDirV);
			SkyManager.cloudsSphereMtrl.SetColor("_MoonColor", SkyManager.moonSpriteColor);
		}
		if (SkyManager.cloudMainTex != SkyManager.cloudMainTexOld)
		{
			SkyManager.cloudsSphereMtrl.SetTexture("_CloudMainTex", SkyManager.cloudMainTex);
			SkyManager.cloudMainTexOld = SkyManager.cloudMainTex;
		}
		if (SkyManager.cloudBlendTex != SkyManager.cloudBlendTexOld)
		{
			SkyManager.cloudsSphereMtrl.SetTexture("_CloudBlendTex", SkyManager.cloudBlendTex);
			SkyManager.cloudBlendTexOld = SkyManager.cloudBlendTex;
		}
		if (SkyManager.triggerLightning)
		{
			if (--SkyManager.lightningFrameCount >= 0)
			{
				Light light = SkyManager.moonLight;
				Vector3 b2;
				b2.x = (float)SkyManager.random.RandomRange(-80, 80);
				b2.y = (float)SkyManager.random.RandomRange(-50, 50);
				b2.z = (float)SkyManager.random.RandomRange(-80, 80);
				b2.y += 300f;
				Transform transform = light.transform;
				transform.position = SkyManager.lightningPosition + b2;
				transform.LookAt(GameManager.Instance.World.GetPrimaryPlayer().transform);
				light.color = Color.white;
				light.intensity = 1.1f;
				light.shadows = LightShadows.Hard;
				light.shadowStrength = 1f;
				light.enabled = true;
				SkyManager.cloudsSphereMtrl.SetFloat("_Lightning", 1f);
				float f2 = (SkyManager.lightningDir + (float)SkyManager.random.RandomRange(-40, 40)) * 0.0174532924f;
				Vector3 v2;
				v2.x = Mathf.Cos(f2);
				v2.y = 0f;
				v2.z = Mathf.Sin(f2);
				SkyManager.cloudsSphereMtrl.SetVector("_LightningDir", v2);
			}
			else
			{
				SkyManager.cloudsSphereMtrl.SetFloat("_Lightning", 0f);
				if (SkyManager.lightningIndex >= SkyManager.lightningFlashes)
				{
					SkyManager.triggerLightning = false;
				}
				else if (Time.time >= SkyManager.lightningStartTime + SkyManager.lightningTimes[SkyManager.lightningIndex])
				{
					SkyManager.lightningIndex++;
					SkyManager.lightningFrameCount = 2;
				}
			}
		}
		if (SkyManager.frustumShadows == null)
		{
			SkyManager.frustumShadows = SkyManager.mainCamera.GetComponent<NGSS_FrustumShadows_7DTD>();
		}
		if (SkyManager.frustumShadows != null)
		{
			SkyManager.frustumShadows.mainShadowsLight = ((!SkyManager.IsDark()) ? SkyManager.sunLight : SkyManager.moonLight);
		}
	}

	public static float GetSunAngle()
	{
		return SkyManager.sunDirV.y;
	}

	public static Vector3 GetSunDirection()
	{
		return SkyManager.sunDirV;
	}

	public static float GetSunPercent()
	{
		return -SkyManager.sunDirV.y;
	}

	public static Vector3 GetSunLightDirection()
	{
		if (!(SkyManager.sunLight == null))
		{
			return SkyManager.sunLightT.forward;
		}
		return Vector3.down;
	}

	public static float GetSunIntensity()
	{
		return SkyManager.sunIntensity;
	}

	public static Color GetSunLightColor()
	{
		if (SkyManager.sunLight)
		{
			return SkyManager.sunLight.color;
		}
		return Color.black;
	}

	public static void SetSunColor(Color color)
	{
		if (SkyManager.sunLight != null)
		{
			SkyManager.sunLight.color = color;
		}
	}

	public static void SetSunIntensity(float i)
	{
		SkyManager.sunIntensity = i;
		float sunAngle = SkyManager.GetSunAngle();
		if (sunAngle >= -SkyManager.sSunFadeHeight)
		{
			SkyManager.sunIntensity = -sunAngle * 10f * SkyManager.sunIntensity * (float)((sunAngle < 0f) ? 1 : 0);
		}
		SkyManager.sunIntensity = Mathf.Clamp(SkyManager.sunIntensity, 0f, SkyManager.sMaxSunIntensity);
		SkyManager.sunIntensity *= 1.5f;
		if (SkyManager.sunLight != null)
		{
			SkyManager.sunLight.intensity = SkyManager.sunIntensity * SkyManager.fogLightScale;
		}
	}

	public static float GetMoonAmbientScale(float add, float mpy)
	{
		return Utils.FastLerp(add + SkyManager.moonBright * mpy, 1f, SkyManager.dayPercent * 3.030303f);
	}

	public static float GetMoonBrightness()
	{
		return SkyManager.moonLightColor.grayscale * SkyManager.moonBright;
	}

	public static Color GetMoonLightColor()
	{
		return SkyManager.moonLightColor;
	}

	public static Vector3 GetMoonDirection()
	{
		return SkyManager.moonLightRot * Vector3.forward;
	}

	public static void SetMoonLightColor(Color color)
	{
		SkyManager.moonLightColor = color;
	}

	public static float GetWorldRotation()
	{
		return SkyManager.worldRotation;
	}

	public static SkyManager skyManager;

	public static float dayCount;

	public static bool indoorFogOn = true;

	public static Material atmosphereMtrl;

	public static Transform atmosphereSphere;

	public static Material cloudsSphereMtrl;

	public static Transform cloudsSphere;

	public static bool bUpdateSunMoonNow = false;

	public static float sSunFadeHeight = 0.1f;

	public static float dayPercent;

	public static GameRandom random;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float sMaxSunIntensity = 0.7f;

	public float maxSunIntensity = 0.7f;

	public float sunHeight = 0.1f;

	public float moonHeight = 0.095f;

	public float sunFadeHeight = 0.1f;

	public float cloudSpeed = 0.05f;

	public float ShowFogDensity;

	public Color ShowSkyColor;

	public Color ShowFogColor;

	public float maxFarClippingPlane = 1500f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float dawnTime = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float duskTime = 22f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static ulong timeOfDay = 0UL;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float cloudTransition = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float fogDensity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float fogLightScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float fogStart = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float fogEnd = 80f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float fogDebugDensity = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float fogDebugStart = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float fogDebugEnd = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Color fogDebugColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float worldRotationTime = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float worldRotation = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float worldRotationTarget = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float sunIntensity = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float starIntensity = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool bNeedsReset = false;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Color SkyColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Color fogColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Transform sunLightT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Light sunLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector3 sunDirV;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector3 sunMoonDirV;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly float[] moonPhases = new float[]
	{
		0.05f,
		0.35f,
		0.55f,
		0.75f,
		1.4f,
		1.63f,
		1.82f
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Transform moonLightT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Light moonLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Transform moonSpriteT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Material moonSpriteMat;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float moonBright;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Color moonLightColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Color moonSpriteColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Quaternion moonLightRot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture cloudMainTex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture cloudMainTexOld;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture cloudBlendTex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture cloudBlendTexOld;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static GameObject parent;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Camera mainCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static NGSS_FrustumShadows_7DTD frustumShadows;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cWorldRotationUpdateFreq = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cStarRotationSpeed = 0.004f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bUpdateShaders;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector4 fogParams;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 sunAxis;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 sunStartV;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 moonStartV;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 starAxis;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool triggerLightning;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int lightningFlashes;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float[] lightningTimes = new float[]
	{
		0f,
		1f,
		2f,
		3f,
		4f
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int lightningIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float lightningStartTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector3 lightningPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float lightningDir;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int lightningFrameCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static SunShaftsEffect.SunSettings sunShaftSettings;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 targetSkyPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bMovingSky = true;
}
