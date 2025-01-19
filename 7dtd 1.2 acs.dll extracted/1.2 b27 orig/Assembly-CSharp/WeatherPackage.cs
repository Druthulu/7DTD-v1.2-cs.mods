using System;
using UnityEngine;

public class WeatherPackage
{
	public WeatherPackage()
	{
		this.param = new float[5];
	}

	public void CopyFrom(WeatherPackage _package)
	{
		int num = Utils.FastMin(5, _package.param.Length);
		for (int i = 0; i < num; i++)
		{
			this.param[i] = _package.param[i];
		}
		this.particleRain = _package.particleRain;
		this.particleSnow = _package.particleSnow;
		this.surfaceWet = _package.surfaceWet;
		this.surfaceSnow = _package.surfaceSnow;
		this.biomeID = _package.biomeID;
		this.weatherSpectrum = _package.weatherSpectrum;
	}

	public void Normalize(BiomeDefinition biomeDefinition)
	{
		for (int i = 0; i < 5; i++)
		{
			this.param[i] = biomeDefinition.WeatherClampToPossibleValues(this.param[i], (BiomeDefinition.Probabilities.ProbType)i);
		}
		this.particleSnow = Mathf.Clamp01(this.particleSnow);
		this.particleRain = Mathf.Clamp01(this.particleRain);
		this.surfaceWet = Mathf.Clamp01(this.surfaceWet);
		this.surfaceSnow = Mathf.Clamp01(this.surfaceSnow);
	}

	public override string ToString()
	{
		string text = string.Format("id {0}, params ", this.biomeID);
		for (int i = 0; i < this.param.Length; i++)
		{
			text += string.Format("{0}, ", this.param[i]);
		}
		return text;
	}

	public float[] param;

	public float particleRain;

	public float particleSnow;

	public float surfaceWet;

	public float surfaceSnow;

	public byte biomeID;

	public short weatherSpectrum;
}
