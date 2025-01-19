using System;
using System.Globalization;
using UnityEngine;

public class NavObjectMapSettings : NavObjectSettings
{
	public override void Init()
	{
		base.Init();
		if (this.Properties.Values.ContainsKey("layer"))
		{
			this.Layer = StringParsers.ParseSInt32(this.Properties.Values["layer"], 0, -1, NumberStyles.Integer);
		}
		if (this.Properties.Values.ContainsKey("icon_scale"))
		{
			this.IconScale = StringParsers.ParseFloat(this.Properties.Values["icon_scale"], 0, -1, NumberStyles.Any);
			this.IconScaleVector = new Vector3(this.IconScale, this.IconScale, this.IconScale);
		}
		if (this.Properties.Values.ContainsKey("adjust_center"))
		{
			this.AdjustCenter = StringParsers.ParseBool(this.Properties.Values["adjust_center"], 0, -1, true);
		}
		if (this.Properties.Values.ContainsKey("use_rotation"))
		{
			this.UseRotation = StringParsers.ParseBool(this.Properties.Values["use_rotation"], 0, -1, true);
		}
	}

	public int Layer;

	public float IconScale = 1f;

	public Vector3 IconScaleVector = Vector3.one;

	public bool UseRotation;

	public bool AdjustCenter;
}
