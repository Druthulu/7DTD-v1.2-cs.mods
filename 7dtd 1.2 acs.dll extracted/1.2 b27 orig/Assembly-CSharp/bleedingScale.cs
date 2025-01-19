using System;
using UnityEngine;

public class bleedingScale : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		float x = this.parentObject.transform.lossyScale.x;
		base.gameObject.GetComponent<ParticleSystem>().main.startSize = new ParticleSystem.MinMaxCurve(this.minParticleScale * x, this.maxParticleScale * x)
		{
			mode = ParticleSystemCurveMode.TwoConstants
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
	}

	public GameObject parentObject;

	public float minParticleScale;

	public float maxParticleScale;
}
