using System;
using System.Globalization;
using UnityEngine;

public class NavObjectCompassSettings : NavObjectSettings
{
	public bool ShowVerticalCompassIcons
	{
		get
		{
			return this.UpSpriteName != "" || this.DownSpriteName != "";
		}
	}

	public override void Init()
	{
		base.Init();
		if (this.Properties.Values.ContainsKey("icon_clamped"))
		{
			this.IconClamped = StringParsers.ParseBool(this.Properties.Values["icon_clamped"], 0, -1, true);
		}
		if (this.Properties.Values.ContainsKey("min_icon_scale"))
		{
			this.MinCompassIconScale = StringParsers.ParseFloat(this.Properties.Values["min_icon_scale"], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey("max_icon_scale"))
		{
			this.MaxCompassIconScale = StringParsers.ParseFloat(this.Properties.Values["max_icon_scale"], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey("min_fade_percent"))
		{
			this.MinFadePercent = StringParsers.ParseFloat(this.Properties.Values["min_fade_percent"], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey("max_scale_distance"))
		{
			this.MaxScaleDistance = StringParsers.ParseFloat(this.Properties.Values["max_scale_distance"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.MaxScaleDistance = this.MaxDistance;
		}
		if (this.Properties.Values.ContainsKey("up_sprite_name"))
		{
			this.UpSpriteName = this.Properties.Values["up_sprite_name"];
		}
		if (this.Properties.Values.ContainsKey("down_sprite_name"))
		{
			this.DownSpriteName = this.Properties.Values["down_sprite_name"];
		}
		if (this.Properties.Values.ContainsKey("show_up_offset"))
		{
			this.ShowUpOffset = StringParsers.ParseFloat(this.Properties.Values["show_up_offset"], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey("show_down_offset"))
		{
			this.ShowDownOffset = StringParsers.ParseFloat(this.Properties.Values["show_down_offset"], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey("hot_zone_type"))
		{
			NavObjectCompassSettings.HotZoneSettings.HotZoneTypes hotZoneTypes = NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.None;
			if (!Enum.TryParse<NavObjectCompassSettings.HotZoneSettings.HotZoneTypes>(this.Properties.Values["hot_zone_type"], out hotZoneTypes))
			{
				hotZoneTypes = NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.None;
			}
			if (hotZoneTypes != NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.None)
			{
				this.HotZone = new NavObjectCompassSettings.HotZoneSettings();
				this.HotZone.HotZoneType = hotZoneTypes;
				if (this.Properties.Values.ContainsKey("hot_zone_sprite"))
				{
					this.HotZone.SpriteName = this.Properties.Values["hot_zone_sprite"];
				}
				if (this.Properties.Values.ContainsKey("hot_zone_color"))
				{
					this.HotZone.Color = StringParsers.ParseColor32(this.Properties.Values["hot_zone_color"]);
				}
				if (this.Properties.Values.ContainsKey("hot_zone_distance"))
				{
					this.HotZone.CustomDistance = StringParsers.ParseFloat(this.Properties.Values["hot_zone_distance"], 0, -1, NumberStyles.Any);
				}
			}
		}
		if (this.Properties.Values.ContainsKey("depth_offset"))
		{
			this.DepthOffset = StringParsers.ParseSInt32(this.Properties.Values["depth_offset"], 0, -1, NumberStyles.Integer);
		}
	}

	public bool IconClamped;

	public float MinCompassIconScale = 0.5f;

	public float MaxCompassIconScale = 1.25f;

	public float MaxScaleDistance = -1f;

	public float MinFadePercent = -1f;

	public string UpSpriteName = "";

	public string DownSpriteName = "";

	public float ShowUpOffset = 3f;

	public float ShowDownOffset = -2f;

	public int DepthOffset;

	public NavObjectCompassSettings.HotZoneSettings HotZone;

	public class HotZoneSettings
	{
		public NavObjectCompassSettings.HotZoneSettings.HotZoneTypes HotZoneType;

		public string SpriteName = "";

		public Color Color = Color.white;

		public float CustomDistance = -1f;

		public enum HotZoneTypes
		{
			None,
			Treasure,
			Custom
		}
	}
}
