using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemSpawnRateLimiter : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
		foreach (ParticleSystem particleSystem in base.GetComponentsInChildren<ParticleSystem>())
		{
			this.originalSpawnRates[particleSystem] = particleSystem.emission.rateOverTime.constant;
			ParticleSystem.EmissionModule emission = particleSystem.emission;
			ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
			rateOverTime.constant = this.originalSpawnRates[particleSystem] * @float;
			emission.rateOverTime = rateOverTime;
		}
		GameOptionsManager.OnGameOptionsApplied += this.OnGameOptionsApplied;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		GameOptionsManager.OnGameOptionsApplied -= this.OnGameOptionsApplied;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGameOptionsApplied()
	{
		float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
		foreach (ParticleSystem particleSystem in base.GetComponentsInChildren<ParticleSystem>())
		{
			if (this.originalSpawnRates.ContainsKey(particleSystem))
			{
				ParticleSystem.EmissionModule emission = particleSystem.emission;
				ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
				rateOverTime.constant = this.originalSpawnRates[particleSystem] * @float;
				emission.rateOverTime = rateOverTime;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<ParticleSystem, float> originalSpawnRates = new Dictionary<ParticleSystem, float>();
}
