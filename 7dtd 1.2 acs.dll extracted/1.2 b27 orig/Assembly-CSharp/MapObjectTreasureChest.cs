﻿using System;
using UnityEngine;

public class MapObjectTreasureChest : MapObject
{
	public MapObjectTreasureChest(Vector3 _position, int _questCode, int _defaultRadius) : base(EnumMapObjectType.TreasureChest, _position, (long)_questCode, null, false)
	{
		this.DefaultRadius = _defaultRadius;
	}

	public override string GetMapIcon()
	{
		return "ui_game_symbol_treasure";
	}

	public override string GetCompassIcon()
	{
		return "ui_game_symbol_treasure";
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
		return true;
	}

	public override Color GetMapIconColor()
	{
		if (this.IsSelected)
		{
			return new Color32(222, 206, 163, byte.MaxValue);
		}
		return Color.white;
	}

	public bool IsSelected;

	public int DefaultRadius = 1;
}
