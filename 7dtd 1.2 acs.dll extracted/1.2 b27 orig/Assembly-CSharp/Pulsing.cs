using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class Pulsing : LightState
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.LODThreshold = 0.75f;
		this.lightComp = this.lightLOD.GetLight();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (GameManager.Instance.IsPaused())
		{
			return;
		}
		if (!this.lightComp.enabled)
		{
			this.lightLOD.SwitchLightByState(true);
		}
		float distSqrRatio = base.GetDistSqrRatio();
		if (distSqrRatio >= this.LODThreshold || !this.lightLOD.bSwitchedOn)
		{
			base.enabled = false;
		}
		float num = (Mathf.Sin(6.28318548f * this.lightLOD.StateRate * Time.time) + 1f) / 2f;
		this.lightComp.intensity = Utils.FastClamp((1f - distSqrRatio) * this.lightLOD.MaxIntensity, 0f, this.lightLOD.MaxIntensity) * num;
		base.UpdateEmissive(num, this.lightLOD.EmissiveFromLightColorOn);
	}

	public override void Kill()
	{
		base.Kill();
		this.lightLOD.lightStateEnabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.lightLOD.lightStateEnabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		this.lightLOD.lightStateEnabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Light lightComp;
}
