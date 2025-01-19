using System;
using UnityEngine;

public class pulsingLightEmissive : LightState
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.lightP = this.lightLOD.GetLight();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (GameManager.Instance.IsPaused())
		{
			return;
		}
		float num = (Mathf.Sin(6.28318548f * this.lightLOD.StateRate * Time.time) + 1f) / 2f;
		this.lightP.intensity = Utils.FastClamp(this.lightLOD.MaxIntensity, 0f, this.lightLOD.MaxIntensity) * num;
		base.UpdateEmissive(num, this.lightLOD.EmissiveFromLightColorOn);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Light lightP;
}
