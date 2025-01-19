﻿using System;
using UnityEngine;

public class MapObjectRestorePower : MapObject
{
	public MapObjectRestorePower(Vector3 _position) : base(EnumMapObjectType.RestorePower, _position, (long)(++MapObjectRestorePower.newID), null, false)
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
		return true;
	}

	public override bool IsOnCompass()
	{
		return true;
	}

	public override bool IsCompassIconClamped()
	{
		return true;
	}

	public override bool NearbyCompassBlink()
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
		return Color.yellow;
	}

	public bool IsSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int newID;
}
