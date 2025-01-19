using System;
using System.Globalization;
using UnityEngine;

public abstract class TGMAbstract
{
	public abstract void SetSeed(int _seed);

	public abstract float GetValue(float _x, float _z, float _biomeIntens);

	public virtual Vector3 GetNormal(float _x, float _z, float _biomeIntens)
	{
		return Vector3.up;
	}

	public virtual void Init()
	{
		this.baseHeight = (this.properties.Values.ContainsKey("BaseHeight") ? StringParsers.ParseFloat(this.properties.Values["BaseHeight"], 0, -1, NumberStyles.Any) : -15f);
	}

	public virtual float GetBaseHeight()
	{
		return this.baseHeight;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public TGMAbstract()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float baseHeight;

	public DynamicProperties properties = new DynamicProperties();

	public bool IsSeedSet;
}
