using System;
using System.Globalization;
using UnityEngine;

public class NavObjectSettings
{
	public virtual void Init()
	{
		if (this.Properties.Values.ContainsKey("sprite_name"))
		{
			this.SpriteName = this.Properties.Values["sprite_name"];
		}
		if (this.Properties.Values.ContainsKey("min_distance"))
		{
			this.MinDistance = StringParsers.ParseFloat(this.Properties.Values["min_distance"], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey("max_distance"))
		{
			this.MaxDistance = StringParsers.ParseFloat(this.Properties.Values["max_distance"], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey("offset"))
		{
			this.Offset = StringParsers.ParseVector3(this.Properties.Values["offset"], 0, -1);
		}
		if (this.Properties.Values.ContainsKey("color"))
		{
			this.Color = StringParsers.ParseColor32(this.Properties.Values["color"]);
		}
		if (this.Properties.Values.ContainsKey("has_pulse"))
		{
			this.HasPulse = StringParsers.ParseBool(this.Properties.Values["has_pulse"], 0, -1, true);
		}
	}

	public DynamicProperties Properties;

	public string SpriteName = "";

	public float MinDistance;

	public float MaxDistance = -1f;

	public Vector3 Offset = Vector3.zero;

	public Color Color = Color.white;

	public bool HasPulse;
}
