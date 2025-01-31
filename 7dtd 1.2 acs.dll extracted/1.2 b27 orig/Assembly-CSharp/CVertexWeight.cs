﻿using System;
using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public class CVertexWeight
{
	public CVertexWeight(int i, Vector3 p, float w)
	{
		this.index = i;
		this.localPosition = p;
		this.weight = w;
	}

	public int index;

	public Vector3 localPosition;

	public float weight;
}
