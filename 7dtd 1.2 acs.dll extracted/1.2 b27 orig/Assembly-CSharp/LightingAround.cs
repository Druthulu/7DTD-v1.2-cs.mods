﻿using System;

public class LightingAround
{
	public LightingAround(byte _sun, byte _block, byte _stabilityMiddle)
	{
		Lighting lighting = new Lighting(_sun, _block, _stabilityMiddle);
		for (int i = 0; i < 9; i++)
		{
			this.lights[i] = lighting;
		}
	}

	public void SetStab(byte _stab)
	{
		for (int i = 0; i < 9; i++)
		{
			this.lights[i].stability = _stab;
		}
	}

	public Lighting this[LightingAround.Pos _pos]
	{
		get
		{
			return this.lights[(int)_pos];
		}
		set
		{
			this.lights[(int)_pos] = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCount = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public Lighting[] lights = new Lighting[9];

	public enum Pos
	{
		Middle,
		X0Y0Z0,
		X1Y0Z0,
		X1Y0Z1,
		X0Y0Z1,
		X0Y1Z0,
		X1Y1Z0,
		X1Y1Z1,
		X0Y1Z1
	}
}
