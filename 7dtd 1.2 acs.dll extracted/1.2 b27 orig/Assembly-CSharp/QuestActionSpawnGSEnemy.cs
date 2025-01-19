﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class QuestActionSpawnGSEnemy : BaseQuestAction
{
	public override void PerformAction(Quest ownerQuest)
	{
		if (!GameStats.GetBool(EnumGameStats.EnemySpawnMode))
		{
			return;
		}
		this.HandleSpawnEnemies(ownerQuest);
	}

	public void HandleSpawnEnemies(Quest ownerQuest)
	{
		if (this.Value != null && this.Value != "" && !int.TryParse(this.Value, out this.count) && this.Value.Contains("-"))
		{
			string[] array = this.Value.Split('-', StringSplitOptions.None);
			int min = Convert.ToInt32(array[0]);
			int maxExclusive = Convert.ToInt32(array[1]);
			World world = GameManager.Instance.World;
			this.count = world.GetGameRandom().RandomRange(min, maxExclusive);
		}
		GameManager.Instance.StartCoroutine(this.SpawnEnemies(ownerQuest));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SpawnEnemies(Quest ownerQuest)
	{
		EntityPlayerLocal player = ownerQuest.OwnerJournal.OwnerPlayer;
		int lastClassId = 0;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			World world = GameManager.Instance.World;
			GameStageDefinition def = GameStageDefinition.GetGameStage(this.ID);
			int num;
			for (int i = 0; i < this.count; i = num + 1)
			{
				yield return new WaitForSeconds(0.5f);
				int randomFromGroup = EntityGroups.GetRandomFromGroup(def.GetStage(player.PartyGameStage).GetSpawnGroup(0).groupName, ref lastClassId, null);
				if (randomFromGroup != 0)
				{
					QuestActionSpawnGSEnemy.SpawnQuestEntity(randomFromGroup, -1, player);
				}
				num = i;
			}
			def = null;
		}
		else
		{
			int num;
			for (int i = 0; i < this.count; i = num + 1)
			{
				yield return new WaitForSeconds(0.5f);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEntitySpawn>().Setup(this.ID, player.entityId), false);
				num = i;
			}
		}
		yield break;
	}

	public static void SpawnQuestEntity(int spawnedEntityID, int entityIDQuestHolder, EntityPlayer player = null)
	{
		World world = GameManager.Instance.World;
		if (player == null)
		{
			player = (world.GetEntity(entityIDQuestHolder) as EntityPlayer);
		}
		Vector3 a = new Vector3(world.GetGameRandom().RandomFloat * 2f + -1f, 0f, world.GetGameRandom().RandomFloat * 2f + -1f);
		a.Normalize();
		float d = world.GetGameRandom().RandomFloat * 12f + 12f;
		Vector3 vector = player.position + a * d;
		Vector3 rotation = new Vector3(0f, player.transform.eulerAngles.y + 180f, 0f);
		float num = (float)GameManager.Instance.World.GetHeight((int)vector.x, (int)vector.z);
		float num2 = (float)GameManager.Instance.World.GetTerrainHeight((int)vector.x, (int)vector.z);
		vector.y = (num + num2) / 2f + 1.5f;
		Entity entity = EntityFactory.CreateEntity(spawnedEntityID, vector, rotation);
		entity.SetSpawnerSource(EnumSpawnerSource.Dynamic);
		GameManager.Instance.World.SpawnEntityInWorld(entity);
		(entity as EntityAlive).SetAttackTarget(player, 200);
	}

	public override BaseQuestAction Clone()
	{
		QuestActionSpawnGSEnemy questActionSpawnGSEnemy = new QuestActionSpawnGSEnemy();
		base.CopyValues(questActionSpawnGSEnemy);
		return questActionSpawnGSEnemy;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(QuestActionSpawnGSEnemy.PropGameStageID))
		{
			this.ID = properties.Values[QuestActionSpawnGSEnemy.PropGameStageID];
		}
		if (properties.Values.ContainsKey(QuestActionSpawnGSEnemy.PropCount))
		{
			this.Value = properties.Values[QuestActionSpawnGSEnemy.PropCount];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int count = 1;

	public static string PropGameStageID = "gamestage_list";

	public static string PropCount = "count";
}
