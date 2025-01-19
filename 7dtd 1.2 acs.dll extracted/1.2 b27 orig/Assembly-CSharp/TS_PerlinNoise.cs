using System;
using UnityEngine;

public class TS_PerlinNoise
{
	public TS_PerlinNoise(int seed)
	{
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(seed);
		this._noisePermutations = new int[512];
		int[] array = new int[256];
		for (int i = 0; i < 256; i++)
		{
			array[i] = i;
		}
		for (int j = 0; j < 256; j++)
		{
			int num = gameRandom.RandomRange(255);
			num = ((num < 0) ? (-num) : num);
			int num2 = array[j];
			array[j] = array[num];
			array[num] = num2;
		}
		for (int k = 0; k < 256; k++)
		{
			this._noisePermutations[k] = (this._noisePermutations[k + 256] = array[k]);
		}
	}

	public float noise(float x, float y, float z)
	{
		int num = (int)TeraMath.fastFloor(x) & 255;
		int num2 = (int)TeraMath.fastFloor(y) & 255;
		int num3 = (int)TeraMath.fastFloor(z) & 255;
		x -= TeraMath.fastFloor(x);
		y -= TeraMath.fastFloor(y);
		z -= TeraMath.fastFloor(z);
		float t = TS_PerlinNoise.fade(x);
		float t2 = TS_PerlinNoise.fade(y);
		float t3 = TS_PerlinNoise.fade(z);
		int num4 = this._noisePermutations[num] + num2;
		int num5 = this._noisePermutations[num4] + num3;
		int num6 = this._noisePermutations[num4 + 1] + num3;
		int num7 = this._noisePermutations[num + 1] + num2;
		int num8 = this._noisePermutations[num7] + num3;
		int num9 = this._noisePermutations[num7 + 1] + num3;
		return TS_PerlinNoise.lerp(t3, TS_PerlinNoise.lerp(t2, TS_PerlinNoise.lerp(t, TS_PerlinNoise.grad(this._noisePermutations[num5], x, y, z), TS_PerlinNoise.grad(this._noisePermutations[num8], x - 1f, y, z)), TS_PerlinNoise.lerp(t, TS_PerlinNoise.grad(this._noisePermutations[num6], x, y - 1f, z), TS_PerlinNoise.grad(this._noisePermutations[num9], x - 1f, y - 1f, z))), TS_PerlinNoise.lerp(t2, TS_PerlinNoise.lerp(t, TS_PerlinNoise.grad(this._noisePermutations[num5 + 1], x, y, z - 1f), TS_PerlinNoise.grad(this._noisePermutations[num8 + 1], x - 1f, y, z - 1f)), TS_PerlinNoise.lerp(t, TS_PerlinNoise.grad(this._noisePermutations[num6 + 1], x, y - 1f, z - 1f), TS_PerlinNoise.grad(this._noisePermutations[num9 + 1], x - 1f, y - 1f, z - 1f))));
	}

	public float fBm(float x, float y, float z)
	{
		float num = 0f;
		if (this._recomputeSpectralWeights)
		{
			this._spectralWeights = new float[this._octaves];
			for (int i = 0; i < this._octaves; i++)
			{
				this._spectralWeights[i] = Mathf.Pow(2.13792014f, -0.836281f * (float)i);
			}
			this._recomputeSpectralWeights = false;
		}
		for (int j = 0; j < this._octaves; j++)
		{
			num += this.noise(x, y, z) * this._spectralWeights[j];
			x *= 2.13792014f;
			y *= 2.13792014f;
			z *= 2.13792014f;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float fade(float t)
	{
		return t * t * t * (t * (t * 6f - 15f) + 10f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float lerp(float t, float a, float b)
	{
		return a + t * (b - a);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float grad(int hash, float x, float y, float z)
	{
		int num = hash & 15;
		float num2 = (num < 8) ? x : y;
		float num3 = (num < 4) ? y : ((num == 12 || num == 14) ? x : z);
		return (((num & 1) == 0) ? num2 : (-num2)) + (((num & 2) == 0) ? num3 : (-num3));
	}

	public void setOctaves(int octaves)
	{
		this._octaves = octaves;
		this._recomputeSpectralWeights = true;
	}

	public int getOctaves()
	{
		return this._octaves;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float LACUNARITY = 2.13792014f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float H = 0.836281f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] _spectralWeights;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] _noisePermutations;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _recomputeSpectralWeights = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int _octaves = 9;
}
