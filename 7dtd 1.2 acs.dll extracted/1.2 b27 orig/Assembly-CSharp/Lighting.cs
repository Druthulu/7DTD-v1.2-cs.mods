using System;
using UnityEngine;

public struct Lighting
{
	public Lighting(byte _sun, byte _block, byte _stability)
	{
		this.sun = _sun;
		this.block = _block;
		this.stability = _stability;
	}

	public Color ToColor()
	{
		return new Color((float)this.sun * 0.06666667f, 0f, (float)this.stability * 0.06666667f, (float)this.block * 0.06666667f);
	}

	public static Color ToColor(int _sunLight, int _blockLight)
	{
		Color result;
		result.r = (float)_sunLight * 0.06666667f;
		result.g = 0f;
		result.b = 0f;
		result.a = (float)_blockLight * 0.06666667f;
		return result;
	}

	public static Color ToColor(int _sunLight, int _blockLight, float _sideFactor)
	{
		Color result;
		result.r = (float)_sunLight * 0.06666667f * _sideFactor;
		result.g = 0f;
		result.b = 0f;
		result.a = (float)_blockLight * 0.06666667f;
		return result;
	}

	public static Lighting one = new Lighting(15, 15, 0);

	public byte sun;

	public byte block;

	public byte stability;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cToPer = 0.06666667f;
}
