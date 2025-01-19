﻿using System;
using UnityEngine;

public class MapObjectHiddenCache : MapObject
{
	public MapObjectHiddenCache(Vector3 _position) : base(EnumMapObjectType.HiddenCache, _position, (long)(++MapObjectHiddenCache.newID), null, false)
	{
	}

	public override string GetMapIcon()
	{
		return "ui_game_symbol_fetch_loot";
	}

	public override string GetCompassIcon()
	{
		return "ui_game_symbol_fetch_loot";
	}

	public override string GetCompassDownIcon()
	{
		return "ui_game_symbol_fetch_loot_down";
	}

	public override string GetCompassUpIcon()
	{
		return "ui_game_symbol_fetch_loot_up";
	}

	public override bool UseUpDownCompassIcons()
	{
		return false;
	}

	public override bool IsOnCompass()
	{
		return true;
	}

	public override bool IsCompassIconClamped()
	{
		return true;
	}

	public override int GetLayerForMapIcon()
	{
		return 0;
	}

	public override bool IsMapIconEnabled()
	{
		return false;
	}

	public override Color GetMapIconColor()
	{
		if (this.IsSelected)
		{
			return new Color32(byte.MaxValue, 180, 0, byte.MaxValue);
		}
		return Color.white;
	}

	public bool IsSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int newID;
}
