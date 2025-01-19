using System;
using UnityEngine;

public class MapObjectSleepingBag : MapObject
{
	public MapObjectSleepingBag(Vector3 _position, Entity _entity) : base(EnumMapObjectType.SleepingBag, _position, (long)_entity.entityId, _entity, false)
	{
	}

	public override string GetMapIcon()
	{
		return "ui_game_symbol_map_bed";
	}

	public override string GetCompassIcon()
	{
		return "ui_game_symbol_map_bed";
	}

	public override bool IsOnCompass()
	{
		return this.IsMapIconEnabled() && this.entity is EntityPlayerLocal;
	}

	public override int GetLayerForMapIcon()
	{
		return 0;
	}

	public override bool IsMapIconEnabled()
	{
		return !(this.entity != null) || !(this.entity is EntityPlayer) || this.entity is EntityPlayerLocal || ((EntityPlayer)this.entity).IsFriendOfLocalPlayer;
	}

	public override Color GetMapIconColor()
	{
		if (this.entity != null && this.entity is EntityPlayer && !(this.entity is EntityPlayerLocal))
		{
			bool isFriendOfLocalPlayer = ((EntityPlayer)this.entity).IsFriendOfLocalPlayer;
			return Color.green * 0.75f;
		}
		return Color.white;
	}
}
