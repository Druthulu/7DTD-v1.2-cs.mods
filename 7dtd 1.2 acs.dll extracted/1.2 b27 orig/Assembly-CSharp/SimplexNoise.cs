﻿using System;

public class SimplexNoise
{
	public static float noise(float x, float y, float z)
	{
		SimplexNoise.s = (x + y + z) * 0.333333343f;
		SimplexNoise.i = SimplexNoise.fastfloor(x + SimplexNoise.s);
		SimplexNoise.j = SimplexNoise.fastfloor(y + SimplexNoise.s);
		SimplexNoise.k = SimplexNoise.fastfloor(z + SimplexNoise.s);
		SimplexNoise.s = (float)(SimplexNoise.i + SimplexNoise.j + SimplexNoise.k) * 0.166666672f;
		SimplexNoise.u = x - (float)SimplexNoise.i + SimplexNoise.s;
		SimplexNoise.v = y - (float)SimplexNoise.j + SimplexNoise.s;
		SimplexNoise.w = z - (float)SimplexNoise.k + SimplexNoise.s;
		SimplexNoise.A[0] = (SimplexNoise.A[1] = (SimplexNoise.A[2] = 0));
		int num = (SimplexNoise.u >= SimplexNoise.w) ? ((SimplexNoise.u >= SimplexNoise.v) ? 0 : 1) : ((SimplexNoise.v >= SimplexNoise.w) ? 1 : 2);
		int num2 = (SimplexNoise.u < SimplexNoise.w) ? ((SimplexNoise.u < SimplexNoise.v) ? 0 : 1) : ((SimplexNoise.v < SimplexNoise.w) ? 1 : 2);
		return SimplexNoise.K(num) + SimplexNoise.K(3 - num - num2) + SimplexNoise.K(num2) + SimplexNoise.K(0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int fastfloor(float n)
	{
		if (n <= 0f)
		{
			return (int)n - 1;
		}
		return (int)n;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float K(int a)
	{
		SimplexNoise.s = (float)(SimplexNoise.A[0] + SimplexNoise.A[1] + SimplexNoise.A[2]) * 0.166666672f;
		float num = SimplexNoise.u - (float)SimplexNoise.A[0] + SimplexNoise.s;
		float num2 = SimplexNoise.v - (float)SimplexNoise.A[1] + SimplexNoise.s;
		float num3 = SimplexNoise.w - (float)SimplexNoise.A[2] + SimplexNoise.s;
		float num4 = 0.6f - num * num - num2 * num2 - num3 * num3;
		int num5 = SimplexNoise.shuffle(SimplexNoise.i + SimplexNoise.A[0], SimplexNoise.j + SimplexNoise.A[1], SimplexNoise.k + SimplexNoise.A[2]);
		SimplexNoise.A[a]++;
		if (num4 < 0f)
		{
			return 0f;
		}
		int num6 = num5 >> 5 & 1;
		int num7 = num5 >> 4 & 1;
		int num8 = num5 >> 3 & 1;
		int num9 = num5 >> 2 & 1;
		int num10 = num5 & 3;
		float num11 = (num10 == 1) ? num : ((num10 == 2) ? num2 : num3);
		float num12 = (num10 == 1) ? num2 : ((num10 == 2) ? num3 : num);
		float num13 = (num10 == 1) ? num3 : ((num10 == 2) ? num : num2);
		num11 = ((num6 == num8) ? (-num11) : num11);
		num12 = ((num6 == num7) ? (-num12) : num12);
		num13 = ((num6 != (num7 ^ num8)) ? (-num13) : num13);
		num4 *= num4;
		return 8f * num4 * num4 * (num11 + ((num10 == 0) ? (num12 + num13) : ((num9 == 0) ? num12 : num13)));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int shuffle(int i, int j, int k)
	{
		return SimplexNoise.b(i, j, k, 0) + SimplexNoise.b(j, k, i, 1) + SimplexNoise.b(k, i, j, 2) + SimplexNoise.b(i, j, k, 3) + SimplexNoise.b(j, k, i, 4) + SimplexNoise.b(k, i, j, 5) + SimplexNoise.b(i, j, k, 6) + SimplexNoise.b(j, k, i, 7);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int b(int i, int j, int k, int B)
	{
		return SimplexNoise.T[SimplexNoise.b(i, B) << 2 | SimplexNoise.b(j, B) << 1 | SimplexNoise.b(k, B)];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int b(int N, int B)
	{
		return N >> B & 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int i;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int j;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int k;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] A = new int[3];

	[PublicizedFrom(EAccessModifier.Private)]
	public static float u;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float v;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float w;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float s;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float onethird = 0.333333343f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float onesixth = 0.166666672f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] T = new int[]
	{
		21,
		56,
		50,
		44,
		13,
		19,
		7,
		42
	};
}
