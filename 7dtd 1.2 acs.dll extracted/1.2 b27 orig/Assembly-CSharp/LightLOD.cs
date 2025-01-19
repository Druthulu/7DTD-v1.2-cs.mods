using System;
using UnityEngine;

public class LightLOD : MonoBehaviour
{
	public Light GetLight()
	{
		if (!this.myLight)
		{
			if (!this.otherLight)
			{
				this.myLight = base.GetComponent<Light>();
			}
			else
			{
				this.myLight = this.otherLight;
			}
			if (this.myLight)
			{
				this.shadowStateMaster = this.myLight.shadows;
				this.shadowStrengthMaster = this.myLight.shadowStrength;
			}
		}
		return this.myLight;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		if (this.hasInitialized)
		{
			return;
		}
		this.hasInitialized = true;
		this.selfT = base.transform;
		Light light = this.GetLight();
		if (light)
		{
			if (GameManager.IsDedicatedServer && this.registeredRange == 0f)
			{
				this.SetRange(light.range);
			}
			this.lightIntensityMaster = light.intensity;
			this.lightRangeMaster = light.range;
			this.CalcViewDistance();
		}
		if (this.RefFlare)
		{
			this.lensFlare = this.RefFlare.GetComponent<LensFlare>();
		}
		else
		{
			this.lensFlare = base.GetComponent<LensFlare>();
		}
		if (this.RefIlluminatedMaterials != null)
		{
			this.maxLightPowerValues = new float[this.RefIlluminatedMaterials.Length];
			for (int i = 0; i < this.RefIlluminatedMaterials.Length; i++)
			{
				Transform transform = this.RefIlluminatedMaterials[i];
				if (transform)
				{
					Renderer component = transform.GetComponent<Renderer>();
					if (component != null)
					{
						Material material = component.material;
						if (material != null)
						{
							if (material.HasProperty("_LightPower"))
							{
								this.maxLightPowerValues[i] = Utils.FastMax(material.GetFloat("_LightPower"), 1f);
							}
							else
							{
								this.maxLightPowerValues[i] = 1f;
							}
						}
					}
				}
			}
		}
		if (this.lightStateStart != LightStateType.Static)
		{
			this.LightStateType = this.lightStateStart;
		}
		if (!GameManager.IsDedicatedServer)
		{
			this.audioSource = base.GetComponent<AudioSource>();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		this.Init();
		if (GameManager.Instance.World == null)
		{
			return;
		}
		this.lightRange = this.lightRangeMaster;
		this.lightIntensity = this.lightIntensityMaster;
		if (this.myLight)
		{
			this.myLight.enabled = false;
		}
		this.isInitialBlockDone = false;
		this.parentT = this.selfT.parent;
		LightManager.LightChanged(this.selfT.position + Origin.position);
		if (GameLightManager.Instance != null)
		{
			GameLightManager.Instance.AddLight(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckInitialBlock()
	{
		if (this.isInitialBlockDone || !this.parentT || !this.selfT)
		{
			return;
		}
		if (this.blockPos.x == -2147483648)
		{
			this.isInitialBlockDone = true;
			if (!this.isHeld && this.registeredRange == 0f && this.myLight)
			{
				this.SetRange(this.lightRange);
			}
			return;
		}
		World world = GameManager.Instance.World;
		BlockValue block = world.GetBlock(this.blockPos);
		Block block2 = block.Block;
		if (block2.isMultiBlock && block.ischild)
		{
			Vector3i parentPos = block2.multiBlockPos.GetParentPos(this.blockPos, block);
			block = world.GetBlock(parentPos);
		}
		if (this.bToggleable)
		{
			this.SetOnOff((block.meta & 2) > 0);
		}
		if (this.registeredRange == 0f && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.myLight)
		{
			this.SetRange(this.lightRange);
		}
		this.isInitialBlockDone = true;
	}

	public LightStateType LightStateType
	{
		get
		{
			return this.lightStateType;
		}
		set
		{
			if (this.lightStateType == value)
			{
				return;
			}
			if (GameManager.IsDedicatedServer)
			{
				return;
			}
			if (this.lightStateType != LightStateType.Static)
			{
				UnityEngine.Object.Destroy(this.lightState);
			}
			this.lightStateType = value;
			if (value == LightStateType.Static)
			{
				this.lightState = null;
				this.lightStateEnabled = false;
				return;
			}
			Type type = Type.GetType(this.lightStateType.ToStringCached<LightStateType>());
			this.lightState = (LightState)base.gameObject.GetComponent(type);
			if (!this.lightState)
			{
				this.lightState = (LightState)base.gameObject.AddComponent(type);
			}
			this.lightStateEnabled = true;
			if (!this.bSwitchedOn)
			{
				this.lightState.enabled = false;
			}
		}
	}

	public float MaxIntensity
	{
		get
		{
			return this.lightIntensity;
		}
		set
		{
			this.lightIntensity = value;
			this.lightIntensityMaster = value;
			LightLOD.MaxIntensityEvent maxIntensityChanged = this.MaxIntensityChanged;
			if (maxIntensityChanged == null)
			{
				return;
			}
			maxIntensityChanged();
		}
	}

	public float LightAngle
	{
		set
		{
			if (this.myLight.type == LightType.Spot)
			{
				if (value > 160f)
				{
					value = 160f;
				}
				this.myLight.spotAngle = value;
			}
		}
	}

	public void SetEmissiveColor(bool _on)
	{
		if (this.RefIlluminatedMaterials == null)
		{
			return;
		}
		Light light = this.GetLight();
		if (!light)
		{
			return;
		}
		Color color = light.color;
		for (int i = 0; i < this.RefIlluminatedMaterials.Length; i++)
		{
			Transform transform = this.RefIlluminatedMaterials[i];
			if (transform)
			{
				Renderer component = transform.GetComponent<Renderer>();
				if (component)
				{
					Material[] materials = component.materials;
					if (materials != null)
					{
						if (_on)
						{
							Color value = this.EmissiveFromLightColorOn ? color : this.EmissiveColor;
							foreach (Material material in materials)
							{
								if (material)
								{
									material.SetColor("_EmissionColor", value);
									material.EnableKeyword("_EMISSION");
								}
							}
						}
						else
						{
							foreach (Material material2 in materials)
							{
								if (material2)
								{
									material2.SetColor("_EmissionColor", LightLOD.EmissiveColorOff);
									material2.DisableKeyword("_EMISSION");
								}
							}
						}
					}
				}
			}
		}
	}

	public void SetEmissiveColorCurrent(Color _color)
	{
		if (this.RefIlluminatedMaterials == null)
		{
			return;
		}
		Light light = this.GetLight();
		if (!light)
		{
			return;
		}
		Color color = light.color;
		for (int i = 0; i < this.RefIlluminatedMaterials.Length; i++)
		{
			Transform transform = this.RefIlluminatedMaterials[i];
			if (transform)
			{
				Renderer component = transform.GetComponent<Renderer>();
				if (component)
				{
					Material[] materials = component.materials;
					if (materials != null)
					{
						if (!this.EmissiveFromLightColorOn)
						{
							Color emissiveColor = this.EmissiveColor;
						}
						foreach (Material material in materials)
						{
							if (material)
							{
								material.SetColor("_EmissionColor", _color);
							}
						}
					}
				}
			}
		}
	}

	public void SetRange(float _range)
	{
		Light light = this.GetLight();
		this.lightRange = _range;
		this.lightRangeMaster = _range;
		light.range = _range;
		this.CalcViewDistance();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && base.enabled && light.enabled)
		{
			this.UnregisterFromLightManager();
			this.registeredPos = light.transform.position + Origin.position;
			this.registeredRange = LightManager.RegisterLight(light);
		}
	}

	public void TestRegistration()
	{
		if (this.myLight)
		{
			this.SetRange(this.lightRange);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcViewDistance()
	{
		this.lightViewDistance = Utils.FastMax(this.MaxDistance, this.lightRangeMaster * 1.5f);
	}

	public void SwitchOnOff(bool _isOn, Vector3i _blockPos)
	{
		this.blockPos = _blockPos;
		if (!this.bToggleable)
		{
			return;
		}
		this.SetOnOff(_isOn);
	}

	public void SetOnOff(bool _isOn)
	{
		Light light = this.GetLight();
		this.bSwitchedOn = _isOn;
		if (light)
		{
			light.enabled = _isOn;
		}
		if (this.LitRootObject)
		{
			this.LitRootObject.SetActive(_isOn);
		}
		if (this.lensFlare)
		{
			this.lensFlare.enabled = _isOn;
		}
		this.SetEmissiveColor(this.bSwitchedOn);
		base.enabled = _isOn;
		if (this.lightState != null)
		{
			this.lightState.enabled = _isOn;
		}
		if (this.audioSource)
		{
			this.audioSource.enabled = _isOn;
		}
	}

	public void SwitchLightByState(bool _isOn)
	{
		Light light = this.GetLight();
		if (light)
		{
			light.enabled = _isOn;
		}
		this.SetEmissiveColor(_isOn);
		GameLightManager.Instance.MakeLightAPriority(this);
	}

	public void OnDisable()
	{
		if (GameManager.Instance.World != null)
		{
			LightManager.LightChanged(this.selfT.position + Origin.position);
			if (GameLightManager.Instance != null)
			{
				GameLightManager.Instance.RemoveLight(this);
			}
		}
		this.UnregisterFromLightManager();
		this.parentT = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UnregisterFromLightManager()
	{
		if (this.registeredRange > 0f)
		{
			LightManager.UnRegisterLight(this.registeredPos, this.registeredRange);
			this.registeredRange = 0f;
		}
	}

	public void FrameUpdate(Vector3 cameraPos)
	{
		this.priority = 0f;
		if (this.bRenderingOff || this.lightStateEnabled)
		{
			return;
		}
		this.CheckInitialBlock();
		if (!this.bSwitchedOn)
		{
			return;
		}
		Light light = this.myLight;
		if (light)
		{
			float num = (this.selfT.position - cameraPos).sqrMagnitude * this.DistanceScale;
			float num2 = Mathf.Sqrt(num) - this.lightRange;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			float num3 = this.lightViewDistance;
			if (this.bPlayerPlacedLight)
			{
				num3 *= 1.2f;
			}
			if (LightLOD.DebugViewDistance > 0f)
			{
				num3 = Utils.FastMax(LightLOD.DebugViewDistance, this.lightRange + 0.01f);
			}
			float num4 = num3 * num3;
			this.distSqRatio = num / num4;
			if (this.bToggleable)
			{
				this.LightStateCheck();
			}
			float num5 = num3 - this.lightRange;
			if (num2 < num5)
			{
				this.priority = 1f;
				if (this.bPlayerPlacedLight)
				{
					if (this.distSqRatio >= 0.640000045f)
					{
						light.shadows = LightShadows.None;
					}
					else if (this.distSqRatio >= 0.0625f)
					{
						if (this.shadowStateMaster == LightShadows.Soft)
						{
							light.shadows = LightShadows.Hard;
						}
						light.shadowStrength = (1f - Utils.FastClamp01((this.distSqRatio - 0.36f) / 0.280000031f)) * this.shadowStrengthMaster;
					}
					else
					{
						light.shadows = this.shadowStateMaster;
						light.shadowStrength = this.shadowStrengthMaster;
					}
				}
				float num6 = num2 / num5;
				float num7 = 1f - num6 * num6;
				light.intensity = this.lightIntensity * num7;
				light.range = this.lightRange * 0.5f + this.lightRange * 0.5f * num7;
				light.enabled = true;
			}
			else
			{
				light.enabled = false;
			}
			if (this.lensFlare != null)
			{
				if (num < 10f * num4)
				{
					float num8 = (1f - num / (num4 * 10f)) * this.lightIntensity * 0.33f * this.FlareBrightnessFactor;
					if (num8 > 1f)
					{
						num8 = 1f;
					}
					if (this.lightRange < 4f)
					{
						num8 *= this.lightRange * 0.25f;
					}
					this.lensFlare.brightness = num8;
					this.lensFlare.color = light.color;
					this.lensFlare.enabled = true;
					return;
				}
				this.lensFlare.enabled = false;
			}
		}
	}

	public void SetRenderingOn()
	{
		this.bRenderingOff = false;
	}

	public void SetRenderingOff()
	{
		this.bRenderingOff = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LightStateCheck()
	{
		if (this.lightState == null)
		{
			return;
		}
		if (this.distSqRatio < this.lightState.LODThreshold && !(this.lightState is Blinking) && !this.lightState.enabled)
		{
			this.lightState.enabled = true;
		}
	}

	public GameObject LitRootObject;

	public Transform[] RefIlluminatedMaterials;

	public Transform RefFlare;

	public float MaxDistance = 30f;

	public float DistanceScale = 1f;

	public float FlareBrightnessFactor = 1f;

	public bool bPlayerPlacedLight;

	public bool bSwitchedOn;

	public bool bToggleable = true;

	public bool isHeld;

	public Light otherLight;

	public bool EmissiveFromLightColorOn;

	public Color EmissiveColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Color EmissiveColorOff = Color.black;

	public float StateRate = 1f;

	public float FluxDelay = 1f;

	public static float DebugViewDistance;

	public LightLOD.MaxIntensityEvent MaxIntensityChanged;

	public LightStateType lightStateStart;

	public bool lightStateEnabled;

	public float priority;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform selfT;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform parentT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hasInitialized;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i blockPos = Vector3i.min;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isInitialBlockDone;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Light myLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lightIntensityMaster;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lightIntensity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lightRangeMaster;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lightRange;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lightViewDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float distSqRatio;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bRenderingOff;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LensFlare lensFlare;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float[] maxLightPowerValues;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 registeredPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float registeredRange;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LightStateType lightStateType;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LightState lightState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LightShadows shadowStateMaster;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float shadowStrengthMaster;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioSource audioSource;

	public delegate void MaxIntensityEvent();
}
