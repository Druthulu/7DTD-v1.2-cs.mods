﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class Color32EqualityComparer : IEqualityComparer<Color32>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Color32EqualityComparer()
	{
	}

	public bool Equals(Color32 _a, Color32 _b)
	{
		return _a.r == _b.r && _a.g == _b.g && _a.b == _b.b && _a.a == _b.a;
	}

	public int GetHashCode(Color32 _a)
	{
		return ((_a.r.GetHashCode() * 397 ^ _a.g.GetHashCode()) * 397 ^ _a.b.GetHashCode()) * 397 ^ _a.a.GetHashCode();
	}

	public static readonly Color32EqualityComparer Instance = new Color32EqualityComparer();
}
