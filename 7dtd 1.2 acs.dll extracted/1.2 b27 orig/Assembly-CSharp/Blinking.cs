using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class Blinking : LightState
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.LODThreshold = 0.75f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator blink()
	{
		for (;;)
		{
			this.switchedOn = !this.switchedOn;
			this.lightLOD.SwitchLightByState(this.switchedOn);
			base.UpdateEmissive(this.switchedOn ? 1f : 0f, this.lightLOD.EmissiveFromLightColorOn);
			yield return new WaitForSeconds(1f / this.lightLOD.StateRate);
		}
		yield break;
	}

	public override void Kill()
	{
		base.Kill();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		base.StartCoroutine(this.blink());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		base.StopAllCoroutines();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool switchedOn = true;
}
