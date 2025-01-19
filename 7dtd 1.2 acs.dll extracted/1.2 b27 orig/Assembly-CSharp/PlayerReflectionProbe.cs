using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(ReflectionProbe))]
public class PlayerReflectionProbe : MonoBehaviour
{
	public static void UpdateCamera(Camera _mainCamera)
	{
		PlayerReflectionProbe.mainCamera = _mainCamera;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		this.allReflectionProbes = base.transform.parent.GetComponentsInChildren<ReflectionProbe>();
		this.reflectionProbe = base.GetComponent<ReflectionProbe>();
		PlayerReflectionProbe.playerReflectionProbe = this;
		PlayerReflectionProbe.SetReflectionSettings(GamePrefs.GetInt(EnumGamePrefs.OptionsGfxReflectQuality), GamePrefs.GetBool(EnumGamePrefs.OptionsGfxReflectShadows));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Destroy()
	{
		PlayerReflectionProbe.playerReflectionProbe = null;
		this.reflectionProbe = null;
		this.allReflectionProbes = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (!PlayerReflectionProbe.hasReflections || this.reflectionProbe == null || PlayerReflectionProbe.mainCamera == null)
		{
			return;
		}
		if (Time.time > this.updateEyeNearWaterTimer + this.eyeNearWaterUpdateFreq)
		{
			this.bEyeNearWaterSurface = false;
			RaycastHit raycastHit;
			if (Physics.SphereCast(new Ray(PlayerReflectionProbe.mainCamera.transform.position + Vector3.up, Vector3.down), 0.5f, out raycastHit, this.fWaterDisCheck, 16))
			{
				this.bEyeNearWaterSurface = (raycastHit.distance < float.PositiveInfinity);
			}
		}
		bool flag = !this.bEyeNearWaterSurface || Time.time > this.lastTimeUpdatedProbe + this.updateProbeWhileEyeNearWaterFreq;
		flag &= (Time.realtimeSinceStartup > this.nextUpdate);
		flag &= this.reflectionProbe.enabled;
		if (this.lastRenderID == -1 || this.catchUpFrameCount < 14)
		{
			for (int i = 0; i < this.allReflectionProbes.Length; i++)
			{
				this.allReflectionProbes[i].refreshMode = ReflectionProbeRefreshMode.EveryFrame;
				this.allReflectionProbes[i].timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
			}
			flag = true;
		}
		else if (this.catchUpFrameCount == 14)
		{
			for (int j = 0; j < this.allReflectionProbes.Length; j++)
			{
				this.allReflectionProbes[j].refreshMode = ReflectionProbeRefreshMode.ViaScripting;
				this.allReflectionProbes[j].timeSlicingMode = this.timeSlicingMode;
			}
		}
		this.catchUpFrameCount++;
		if (flag)
		{
			for (int k = 0; k < this.allReflectionProbes.Length; k++)
			{
				this.lastRenderID = this.allReflectionProbes[k].RenderProbe();
			}
			this.lastTimeUpdatedProbe = Time.time;
			this.nextUpdate = Time.realtimeSinceStartup + 1000f / (float)this.RefreshRate * 0.001f;
		}
	}

	public static void SetReflectionSettings(int qualityLevel, bool bReflectedShadows)
	{
		if (!PlayerReflectionProbe.playerReflectionProbe)
		{
			return;
		}
		PlayerReflectionProbe.playerReflectionProbe.setReflectionSettings(qualityLevel, bReflectedShadows);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setReflectionSettings(int qualityLevel, bool bReflectedShadows)
	{
		if (this.reflectionProbe == null)
		{
			return;
		}
		this.lastRenderID = -1;
		this.catchUpFrameCount = 0;
		RenderSettings.reflectionBounces = 1;
		PlayerReflectionProbe.hasReflections = (qualityLevel > 0);
		for (int i = 0; i < this.allReflectionProbes.Length; i++)
		{
			this.allReflectionProbes[i].enabled = PlayerReflectionProbe.hasReflections;
		}
		Shader.SetGlobalFloat("_ReflectionsOn", (float)(PlayerReflectionProbe.hasReflections ? 1 : 0));
		if (!PlayerReflectionProbe.hasReflections)
		{
			Shader.EnableKeyword("GAME_NOREFLECTION");
			return;
		}
		this.reflectionProbe.nearClipPlane = 0.1f;
		this.reflectionProbe.intensity = 1f;
		Shader.DisableKeyword("GAME_NOREFLECTION");
		switch (qualityLevel)
		{
		case 1:
		{
			this.RefreshRate = 3;
			this.reflectionProbe.farClipPlane = 30f;
			this.reflectionProbe.shadowDistance = (float)(bReflectedShadows ? 10 : 0);
			this.reflectionProbe.resolution = 128;
			this.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
			this.reflectionProbe.intensity = 0.5f;
			string[] options = new string[]
			{
				"ReflectionsOnly",
				"Terrain"
			};
			this.SetOptions(options);
			return;
		}
		case 2:
		{
			this.RefreshRate = 5;
			this.reflectionProbe.farClipPlane = 100f;
			this.reflectionProbe.shadowDistance = (float)(bReflectedShadows ? 20 : 0);
			this.reflectionProbe.resolution = 256;
			this.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
			this.reflectionProbe.intensity = 0.55f;
			string[] options2 = new string[]
			{
				"Trees",
				"ReflectionsOnly",
				"Terrain"
			};
			this.SetOptions(options2);
			return;
		}
		case 3:
		{
			this.RefreshRate = 15;
			this.reflectionProbe.farClipPlane = 200f;
			this.reflectionProbe.shadowDistance = (float)(bReflectedShadows ? 50 : 0);
			this.reflectionProbe.resolution = 256;
			this.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
			this.reflectionProbe.intensity = 0.7f;
			string[] options3 = new string[]
			{
				"Trees",
				"ReflectionsOnly",
				"Terrain"
			};
			this.SetOptions(options3);
			return;
		}
		case 4:
		{
			this.RefreshRate = 18;
			this.reflectionProbe.farClipPlane = 280f;
			this.reflectionProbe.shadowDistance = (float)(bReflectedShadows ? 70 : 0);
			this.reflectionProbe.resolution = 256;
			this.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
			this.reflectionProbe.intensity = 0.85f;
			string[] options4 = new string[]
			{
				"Trees",
				"ReflectionsOnly",
				"Terrain"
			};
			this.SetOptions(options4);
			return;
		}
		case 5:
		{
			this.RefreshRate = 24;
			this.reflectionProbe.farClipPlane = 425f;
			this.reflectionProbe.shadowDistance = (float)(bReflectedShadows ? 100 : 0);
			this.reflectionProbe.resolution = 512;
			this.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
			string[] options5 = new string[]
			{
				"Trees",
				"ReflectionsOnly",
				"Terrain"
			};
			this.SetOptions(options5);
			return;
		}
		}
		this.RefreshRate = 30;
		this.reflectionProbe.farClipPlane = 600f;
		this.reflectionProbe.shadowDistance = (float)(bReflectedShadows ? 150 : 0);
		this.reflectionProbe.resolution = 512;
		this.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
		string[] options6 = new string[]
		{
			"Trees",
			"ReflectionsOnly",
			"Terrain"
		};
		this.SetOptions(options6);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetOptions(string[] cullingMasks)
	{
		this.reflectionProbe.cullingMask = LayerMask.GetMask(cullingMasks);
	}

	public int RefreshRate = 30;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static PlayerReflectionProbe playerReflectionProbe = null;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ReflectionProbe[] allReflectionProbes;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ReflectionProbe reflectionProbe;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool hasReflections = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int lastRenderID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float nextUpdate;

	public float fWaterDisCheck = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ReflectionProbeTimeSlicingMode timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bEyeNearWaterSurface;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int raycastMask = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float updateEyeNearWaterTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float eyeNearWaterUpdateFreq = 1.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastTimeUpdatedProbe;

	public float updateProbeWhileEyeNearWaterFreq = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int[] viewDistantToWaterTransparencyDistance = new int[]
	{
		32,
		32,
		32,
		32,
		32,
		32,
		36,
		40,
		44,
		48,
		52,
		56,
		60
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Camera mainCamera = null;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int catchUpFrameCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cMaxFramesToUpdate = 14;
}
