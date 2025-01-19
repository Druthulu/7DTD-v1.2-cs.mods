using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveClosestPOIGoto : ObjectiveGoto
{
	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveRallyPointHeadTo", false);
	}

	public override void SetupDisplay()
	{
		base.Description = this.keyword;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetupIcon()
	{
		this.icon = "ui_game_symbol_quest";
	}

	public override bool SetupPosition(EntityNPC ownerNPC = null, EntityPlayer player = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = 1)
	{
		return this.GetPosition(ownerNPC, player, usedPOILocations, -1) != Vector3.zero;
	}

	public override void SetPosition(Vector3 position, Vector3 size)
	{
		base.FinalizePoint(position, size);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 GetPosition(EntityNPC ownerNPC = null, EntityPlayer entityPlayer = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		int traderId = (ownerNPC == null) ? -1 : ownerNPC.entityId;
		int playerId = (entityPlayer == null) ? -1 : entityPlayer.entityId;
		if (base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.POIPosition))
		{
			base.OwnerQuest.Position = this.position;
			this.positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, this.NavObjectName, -1);
			base.CurrentValue = 2;
			return this.position;
		}
		EntityAlive entityAlive = (ownerNPC == null) ? base.OwnerQuest.OwnerJournal.OwnerPlayer : ownerNPC;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PrefabInstance closestPOIToWorldPos = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetClosestPOIToWorldPos(base.OwnerQuest.QuestTags, new Vector2(entityAlive.position.x, entityAlive.position.z), null, -1, false, BiomeFilterTypes.SameBiome, "");
			if (closestPOIToWorldPos == null)
			{
				return Vector3.zero;
			}
			Vector2 vector = new Vector2((float)closestPOIToWorldPos.boundingBoxPosition.x + (float)closestPOIToWorldPos.boundingBoxSize.x / 2f, (float)closestPOIToWorldPos.boundingBoxPosition.z + (float)closestPOIToWorldPos.boundingBoxSize.z / 2f);
			if (vector.x == -0.1f && vector.y == -0.1f)
			{
				Log.Error("ObjectiveClosestPOIGoto: No POI found.");
				return Vector3.zero;
			}
			int num = (int)vector.x;
			int num2 = (int)entityAlive.position.y;
			int num3 = (int)vector.y;
			if (GameManager.Instance.World.IsPositionInBounds(this.position))
			{
				base.FinalizePoint(new Vector3((float)closestPOIToWorldPos.boundingBoxPosition.x, (float)closestPOIToWorldPos.boundingBoxPosition.y, (float)closestPOIToWorldPos.boundingBoxPosition.z), new Vector3((float)closestPOIToWorldPos.boundingBoxSize.x, (float)closestPOIToWorldPos.boundingBoxSize.y, (float)closestPOIToWorldPos.boundingBoxSize.z));
				this.position = new Vector3((float)num, (float)num2, (float)num3);
				base.OwnerQuest.Position = this.position;
				return this.position;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(traderId, playerId, base.OwnerQuest.QuestTags, base.OwnerQuest.QuestCode, NetPackageQuestGotoPoint.QuestGotoTypes.Closest, base.OwnerQuest.QuestClass.DifficultyTier, 0, -1, 0f, 0f, 0f, -1f, BiomeFilterTypes.SameBiome, ""), false);
			base.CurrentValue = 1;
		}
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_WaitingForServer()
	{
		if (this.positionSet)
		{
			base.CurrentValue = 2;
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveClosestPOIGoto objectiveClosestPOIGoto = new ObjectiveClosestPOIGoto();
		this.CopyValues(objectiveClosestPOIGoto);
		return objectiveClosestPOIGoto;
	}
}
