﻿using System;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class EntityNPC : EntityAlive
{
	public NPCInfo NPCInfo
	{
		get
		{
			if (this.npcID != "")
			{
				return NPCInfo.npcInfoList[this.npcID];
			}
			return null;
		}
	}

	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
		EntityClass entityClass = EntityClass.list[this.entityClass];
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropNPCID))
		{
			this.npcID = entityClass.Properties.Values[EntityClass.PropNPCID];
		}
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		this.bag.SetSlots(GameUtils.ReadItemStack(_br));
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		GameUtils.WriteItemStack(_bw, this.bag.GetSlots());
	}

	public override bool IsSavedToFile()
	{
		return (base.GetSpawnerSource() != EnumSpawnerSource.Dynamic || this.IsDead()) && base.IsSavedToFile();
	}

	public override float GetSeeDistance()
	{
		return 80f;
	}

	public override void VisiblityCheck(float _distanceSqr, bool _masterIsZooming)
	{
		bool bVisible = _distanceSqr < (float)(_masterIsZooming ? 14400 : 8100);
		this.emodel.SetVisible(bVisible, false);
	}

	public override bool CanBePushed()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool canDespawn()
	{
		return this.world.GetPlayers().Count == 0 && base.canDespawn();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isRadiationSensitive()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDetailedHeadBodyColliders()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isGameMessageOnDeath()
	{
		return false;
	}

	public virtual void PlayVoiceSetEntry(string name, EntityPlayer player, bool ignoreTime = true, bool showReactionAnim = true)
	{
	}

	public string npcID = "";
}
