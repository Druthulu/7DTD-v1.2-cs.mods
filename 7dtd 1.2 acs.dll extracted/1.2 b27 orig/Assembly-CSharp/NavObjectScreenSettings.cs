using System;
using System.Globalization;
using UnityEngine;

public class NavObjectScreenSettings : NavObjectSettings
{
	public override void Init()
	{
		base.Init();
		if (this.Properties.Values.ContainsKey("text_type") && !Enum.TryParse<NavObjectScreenSettings.ShowTextTypes>(this.Properties.Values["text_type"], out this.ShowTextType))
		{
			this.ShowTextType = NavObjectScreenSettings.ShowTextTypes.None;
		}
		if (this.Properties.Values.ContainsKey("sprite_size"))
		{
			this.SpriteSize = StringParsers.ParseFloat(this.Properties.Values["sprite_size"], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey("fade_percent"))
		{
			this.FadePercent = StringParsers.ParseFloat(this.Properties.Values["fade_percent"], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey("show_offscreen"))
		{
			this.ShowOffScreen = StringParsers.ParseBool(this.Properties.Values["show_offscreen"], 0, -1, true);
		}
		if (this.Properties.Values.ContainsKey("use_head_offset"))
		{
			this.UseHeadOffset = StringParsers.ParseBool(this.Properties.Values["use_head_offset"], 0, -1, true);
		}
		if (this.Properties.Values.ContainsKey("sprite_fill_color"))
		{
			this.SpriteFillColor = StringParsers.ParseColor32(this.Properties.Values["sprite_fill_color"]);
		}
		this.Properties.ParseEnum<NavObjectScreenSettings.SpriteFillTypes>("sprite_fill_type", ref this.SpriteFillType);
		this.Properties.ParseString("sprite_fill_name", ref this.SpriteFillName);
		this.Properties.ParseString("sub_sprite_name", ref this.SubSpriteName);
		this.Properties.ParseVec("sub_sprite_offset", ref this.SubSpriteOffset);
		this.Properties.ParseFloat("sub_sprite_size", ref this.SubSpriteSize);
		this.FadeEndDistance = (this.MaxDistance - this.MinDistance) * this.FadePercent;
		this.Properties.ParseInt("font_size", ref this.FontSize);
		if (this.Properties.Values.ContainsKey("font_color"))
		{
			this.FontColor = StringParsers.ParseColor32(this.Properties.Values["font_color"]);
		}
	}

	public NavObjectScreenSettings.ShowTextTypes ShowTextType;

	public int FontSize = 24;

	public Color FontColor = Color.white;

	public float SpriteSize = 32f;

	public float FadePercent = 0.9f;

	public float FadeEndDistance;

	public bool ShowOffScreen;

	public bool UseHeadOffset;

	public NavObjectScreenSettings.SpriteFillTypes SpriteFillType;

	public string SpriteFillName = "";

	public Color SpriteFillColor = Color.white;

	public string SubSpriteName = "";

	public Vector2 SubSpriteOffset = Vector2.zero;

	public float SubSpriteSize = 16f;

	public enum ShowTextTypes
	{
		None,
		Distance,
		Name,
		SpawnName
	}

	public enum SpriteFillTypes
	{
		None,
		Health
	}
}
