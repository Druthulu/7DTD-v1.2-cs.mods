﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Audio;
using GameEvent.GameEventHelpers;
using UnityEngine;

public class GameEventManager
{
	public static GameEventManager Current
	{
		get
		{
			if (GameEventManager.instance == null)
			{
				GameEventManager.instance = new GameEventManager();
			}
			return GameEventManager.instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public GameEventManager()
	{
		this.Random = GameRandomManager.Instance.CreateGameRandom();
	}

	public static bool HasInstance
	{
		get
		{
			return GameEventManager.instance != null;
		}
	}

	public void AddSequence(GameEventActionSequence action)
	{
		if (!GameEventManager.GameEventSequences.ContainsKey(action.Name))
		{
			GameEventManager.GameEventSequences.Add(action.Name, action);
		}
	}

	public void Cleanup()
	{
		this.ClearActions();
		this.GameEventFlags.Clear();
		this.BossGroups.Clear();
		this.CurrentBossGroup = null;
		this.HomerunManager.Cleanup();
	}

	public void ClearActions()
	{
		this.ActionSequenceUpdates.Clear();
		GameEventManager.GameEventSequences.Clear();
		this.CategoryList.Clear();
		this.spawnEntries.Clear();
		this.blockEntries.Clear();
	}

	public BossGroup CurrentBossGroup
	{
		get
		{
			return this.currentBossGroup;
		}
		set
		{
			if (this.currentBossGroup == value)
			{
				return;
			}
			if (this.currentBossGroup != null)
			{
				this.currentBossGroup.IsCurrent = false;
				this.currentBossGroup.RemoveNavObjects();
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
				{
					this.currentBossGroup.MinionEntities = null;
				}
			}
			this.currentBossGroup = value;
			if (this.currentBossGroup != null)
			{
				this.currentBossGroup.IsCurrent = true;
				this.currentBossGroup.RequestStatRefresh();
				this.currentBossGroup.AddNavObjects();
			}
		}
	}

	public int CurrentCount
	{
		get
		{
			return this.spawnEntries.Count + this.ReservedCount;
		}
	}

	public GameEventActionSequence.TargetTypes GetTargetType(string gameEventName)
	{
		if (GameEventManager.GameEventSequences.ContainsKey(gameEventName))
		{
			return GameEventManager.GameEventSequences[gameEventName].TargetType;
		}
		return GameEventActionSequence.TargetTypes.Entity;
	}

	public bool HandleAction(string name, EntityPlayer requester, Entity entity, bool twitchActivated, string extraData = "", string tag = "", bool crateShare = false, bool allowRefunds = true, string sequenceLink = "", GameEventActionSequence ownerSeq = null)
	{
		return this.HandleAction(name, requester, entity, twitchActivated, Vector3i.zero, extraData, tag, crateShare, allowRefunds, sequenceLink, ownerSeq);
	}

	public bool HandleAction(string name, EntityPlayer requester, Entity entity, bool twitchActivated, Vector3 targetPosition, string extraData = "", string tag = "", bool crateShare = false, bool allowRefunds = true, string sequenceLink = "", GameEventActionSequence ownerSeq = null)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			return this.HandleActionClient(name, entity, twitchActivated, targetPosition, extraData, tag, crateShare, allowRefunds, sequenceLink);
		}
		if (GameEventManager.GameEventSequences.ContainsKey(name))
		{
			GameEventActionSequence gameEventActionSequence = GameEventManager.GameEventSequences[name];
			if (gameEventActionSequence.CanPerform(entity))
			{
				if (gameEventActionSequence.SingleInstance)
				{
					for (int i = 0; i < this.ActionSequenceUpdates.Count; i++)
					{
						if (this.ActionSequenceUpdates[i].Name == name)
						{
							return false;
						}
					}
				}
				GameEventActionSequence gameEventActionSequence2 = gameEventActionSequence.Clone();
				gameEventActionSequence2.Target = entity;
				gameEventActionSequence2.TargetPosition = targetPosition;
				if (ownerSeq == null && sequenceLink != "" && requester != null)
				{
					ownerSeq = this.GetSequenceLink(requester, sequenceLink);
				}
				if (ownerSeq != null)
				{
					gameEventActionSequence2.Requester = ownerSeq.Requester;
					gameEventActionSequence2.ExtraData = ownerSeq.ExtraData;
					gameEventActionSequence2.CrateShare = ownerSeq.CrateShare;
					gameEventActionSequence2.Tag = ownerSeq.Tag;
					gameEventActionSequence2.AllowRefunds = ownerSeq.AllowRefunds;
					gameEventActionSequence2.TwitchActivated = ownerSeq.TwitchActivated;
				}
				else
				{
					gameEventActionSequence2.Requester = requester;
					gameEventActionSequence2.ExtraData = extraData;
					gameEventActionSequence2.CrateShare = crateShare;
					gameEventActionSequence2.Tag = tag;
					gameEventActionSequence2.AllowRefunds = allowRefunds;
					gameEventActionSequence2.TwitchActivated = twitchActivated;
				}
				gameEventActionSequence2.OwnerSequence = ownerSeq;
				if (gameEventActionSequence2.TargetType != GameEventActionSequence.TargetTypes.Entity)
				{
					gameEventActionSequence2.POIPosition = new Vector3i(targetPosition);
				}
				gameEventActionSequence2.SetupTarget();
				this.ActionSequenceUpdates.Add(gameEventActionSequence2);
				return true;
			}
		}
		return false;
	}

	public bool HandleActionClient(string name, Entity entity, bool twitchActivated, string extraData = "", string tag = "", bool crateShare = false, bool allowRefunds = true, string sequenceLink = "")
	{
		return this.HandleActionClient(name, entity, twitchActivated, Vector3.zero, extraData, tag, crateShare, allowRefunds, sequenceLink);
	}

	public bool HandleActionClient(string name, Entity entity, bool twitchActivated, Vector3 targetPosition, string extraData = "", string tag = "", bool crateShare = false, bool allowRefunds = true, string sequenceLink = "")
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageGameEventRequest>().Setup(name, entity ? entity.entityId : -1, twitchActivated, targetPosition, extraData, tag, crateShare, allowRefunds, sequenceLink), false);
		return true;
	}

	public void Update(float deltaTime)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && GameManager.Instance.World != null)
		{
			this.HandleSpawnUpdates(deltaTime);
			this.HandleActionUpdates();
			this.HandleBlockUpdates(deltaTime);
			this.HandleEventFlagUpdates(deltaTime);
			this.HandleBossGroupUpdates(deltaTime);
			this.HomerunManager.Update(deltaTime);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleSpawnUpdates(float deltaTime)
	{
		bool flag = false;
		if (this.spawnEntries.Count > 0)
		{
			this.attackTimerUpdate -= deltaTime;
			if (this.attackTimerUpdate <= 0f)
			{
				flag = true;
				this.attackTimerUpdate = 2f;
			}
		}
		for (int i = this.spawnEntries.Count - 1; i >= 0; i--)
		{
			GameEventManager.SpawnEntry spawnEntry = this.spawnEntries[i];
			if (spawnEntry.SpawnedEntity.IsDespawned)
			{
				spawnEntry.GameEvent.HasDespawn = true;
				this.spawnEntries.RemoveAt(i);
				if (spawnEntry.Requester != null)
				{
					if (spawnEntry.Requester is EntityPlayerLocal)
					{
						GameEventManager.Current.HandleGameEntityDespawned(spawnEntry.SpawnedEntity.entityId);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(NetPackageGameEventResponse.ResponseTypes.EntityDespawned, spawnEntry.SpawnedEntity.entityId, -1, "", false), false, spawnEntry.Requester.entityId, -1, -1, null, 192);
					}
				}
			}
			else if (!spawnEntry.SpawnedEntity.IsAlive() || spawnEntry.SpawnedEntity.emodel == null)
			{
				this.spawnEntries.RemoveAt(i);
				if (spawnEntry.Requester != null)
				{
					if (spawnEntry.Requester is EntityPlayerLocal)
					{
						GameEventManager.Current.HandleGameEntityKilled(spawnEntry.SpawnedEntity.entityId);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(NetPackageGameEventResponse.ResponseTypes.EntityKilled, spawnEntry.SpawnedEntity.entityId, -1, "", false), false, spawnEntry.Requester.entityId, -1, -1, null, 192);
					}
				}
			}
			else if (flag)
			{
				spawnEntry.HandleUpdate();
			}
		}
	}

	public void RemoveSpawnedEntry(Entity spawnedEntity)
	{
		for (int i = this.spawnEntries.Count - 1; i >= 0; i--)
		{
			if (this.spawnEntries[i].SpawnedEntity == spawnedEntity)
			{
				GameEventManager.SpawnEntry spawnEntry = this.spawnEntries[i];
				spawnEntry.GameEvent.HasDespawn = true;
				this.spawnEntries.RemoveAt(i);
				if (spawnEntry.Requester != null)
				{
					if (spawnEntry.Requester is EntityPlayerLocal)
					{
						GameEventManager.Current.HandleGameEntityDespawned(spawnEntry.SpawnedEntity.entityId);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(NetPackageGameEventResponse.ResponseTypes.EntityDespawned, spawnEntry.SpawnedEntity.entityId, -1, "", false), false, spawnEntry.Requester.entityId, -1, -1, null, 192);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleActionUpdates()
	{
		for (int i = 0; i < this.ActionSequenceUpdates.Count; i++)
		{
			GameEventActionSequence gameEventActionSequence = this.ActionSequenceUpdates[i];
			if (gameEventActionSequence.StartTime <= 0f)
			{
				gameEventActionSequence.StartSequence(this);
			}
			gameEventActionSequence.Update();
		}
		for (int j = this.ActionSequenceUpdates.Count - 1; j >= 0; j--)
		{
			GameEventActionSequence gameEventActionSequence2 = this.ActionSequenceUpdates[j];
			if (!gameEventActionSequence2.HasTarget() && gameEventActionSequence2.AllowRefunds)
			{
				gameEventActionSequence2.IsComplete = true;
			}
			if (gameEventActionSequence2.IsComplete)
			{
				this.ReservedCount -= gameEventActionSequence2.ReservedSpawnCount;
				this.ActionSequenceUpdates.RemoveAt(j);
			}
		}
	}

	public void RegisterSpawnedEntity(Entity spawned, Entity target, EntityPlayer requester, GameEventActionSequence gameEvent, bool isAggressive = true)
	{
		this.spawnEntries.Add(new GameEventManager.SpawnEntry
		{
			SpawnedEntity = (spawned as EntityAlive),
			Target = (target as EntityAlive),
			Requester = requester,
			GameEvent = gameEvent
		});
	}

	public GameEventManager.SpawnedBlocksEntry RegisterSpawnedBlocks(List<Vector3i> blockList, Entity target, EntityPlayer requester, GameEventActionSequence gameEvent, float timeAlive, string removeSound, Vector3 center, bool refundOnRemove)
	{
		GameEventManager.SpawnedBlocksEntry spawnedBlocksEntry = new GameEventManager.SpawnedBlocksEntry
		{
			BlockList = blockList,
			Target = target,
			Requester = requester,
			GameEvent = gameEvent,
			TimeAlive = timeAlive,
			RemoveSound = removeSound,
			Center = center,
			RefundOnRemove = refundOnRemove
		};
		this.blockEntries.Add(spawnedBlocksEntry);
		return spawnedBlocksEntry;
	}

	public event OnGameEventAccessApproved GameEventAccessApproved;

	public void HandleGameEventAccessApproved()
	{
		if (this.GameEventAccessApproved != null)
		{
			this.GameEventAccessApproved();
		}
	}

	public event OnGameEntityAdded GameEntitySpawned;

	public void HandleGameEntitySpawned(string gameEventID, int entityID, string tag)
	{
		if (this.GameEntitySpawned != null)
		{
			this.GameEntitySpawned(gameEventID, entityID, tag);
		}
	}

	public event OnGameEntityChanged GameEntityDespawned;

	public void HandleGameEntityDespawned(int entityID)
	{
		if (this.GameEntityDespawned != null)
		{
			this.GameEntityDespawned(entityID);
		}
	}

	public event OnGameEntityChanged GameEntityKilled;

	public void HandleGameEntityKilled(int entityID)
	{
		if (this.GameEntityKilled != null)
		{
			this.GameEntityKilled(entityID);
		}
	}

	public event OnGameBlocksAdded GameBlocksAdded;

	public void HandleGameBlocksAdded(string gameEventID, int blockGroupID, List<Vector3i> blockList, string tag)
	{
		if (this.GameBlocksAdded != null)
		{
			this.GameBlocksAdded(gameEventID, blockGroupID, blockList, tag);
		}
	}

	public event OnGameBlockRemoved GameBlockRemoved;

	public void BlockRemoved(Vector3i blockPos)
	{
		for (int i = 0; i < this.blockEntries.Count; i++)
		{
			if (this.blockEntries[i].RemoveBlock(blockPos))
			{
				if (this.blockEntries[i].Requester is EntityPlayerLocal)
				{
					GameEventManager.Current.HandleGameBlockRemoved(blockPos);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(NetPackageGameEventResponse.ResponseTypes.BlockRemoved, blockPos), false, this.blockEntries[i].Requester.entityId, -1, -1, null, 192);
				}
				if (this.blockEntries[i].BlockList.Count == 0)
				{
					this.blockEntries.RemoveAt(i);
				}
				return;
			}
		}
	}

	public void HandleGameBlockRemoved(Vector3i blockPos)
	{
		if (this.GameBlockRemoved != null)
		{
			this.GameBlockRemoved(blockPos);
		}
	}

	public event OnGameBlocksRemoved GameBlocksRemoved;

	public void HandleGameBlocksRemoved(int blockGroupID, bool isDespawn)
	{
		if (this.GameBlocksRemoved != null)
		{
			this.GameBlocksRemoved(blockGroupID, isDespawn);
		}
	}

	public event OnGameEventStatus GameEventApproved;

	public void HandleGameEventApproved(string gameEventID, int targetEntityID, string extraData, string tag)
	{
		if (this.GameEventApproved != null)
		{
			this.GameEventApproved(gameEventID, targetEntityID, extraData, tag);
		}
	}

	public event OnGameEventStatus GameEventDenied;

	public void HandleGameEventDenied(string gameEventID, int targetEntityID, string extraData, string tag)
	{
		if (this.GameEventDenied != null)
		{
			this.GameEventDenied(gameEventID, targetEntityID, extraData, tag);
		}
	}

	public event OnGameEventStatus TwitchPartyGameEventApproved;

	public void HandleTwitchPartyGameEventApproved(string gameEventID, int targetEntityID, string extraData, string tag)
	{
		if (this.TwitchPartyGameEventApproved != null)
		{
			this.TwitchPartyGameEventApproved(gameEventID, targetEntityID, extraData, tag);
		}
	}

	public event OnGameEventStatus TwitchRefundNeeded;

	public void HandleTwitchRefundNeeded(string gameEventID, int targetEntityID, string extraData, string tag)
	{
		if (this.TwitchRefundNeeded != null)
		{
			this.TwitchRefundNeeded(gameEventID, targetEntityID, extraData, tag);
		}
	}

	public event OnGameEventStatus GameEventCompleted;

	public void HandleGameEventCompleted(string gameEventID, int targetEntityID, string extraData, string tag)
	{
		if (this.GameEventCompleted != null)
		{
			this.GameEventCompleted(gameEventID, targetEntityID, extraData, tag);
		}
	}

	public void HandleGameEventSequenceItemForClient(string gameEventID, int index)
	{
		EntityPlayer player = XUiM_Player.GetPlayer();
		GameEventManager.GameEventSequences[gameEventID].HandleClientPerform(player, index);
	}

	public void HandleTwitchSetOwner(int targetEntityID, int entitySpawnedID, string extraData)
	{
		EntityAlive entityAlive = GameManager.Instance.World.GetEntity(entitySpawnedID) as EntityAlive;
		if (entityAlive != null)
		{
			entityAlive.SetSpawnByData(targetEntityID, extraData);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleBlockUpdates(float deltaTime)
	{
		for (int i = this.blockEntries.Count - 1; i >= 0; i--)
		{
			GameEventManager.SpawnedBlocksEntry spawnedBlocksEntry = this.blockEntries[i];
			if (spawnedBlocksEntry.TimeAlive > 0f)
			{
				spawnedBlocksEntry.TimeAlive -= deltaTime;
			}
			else if (spawnedBlocksEntry.TimeAlive != -1f)
			{
				if (spawnedBlocksEntry.TryRemoveBlocks())
				{
					this.blockEntries.RemoveAt(i);
				}
				else
				{
					spawnedBlocksEntry.TimeAlive = 5f;
				}
			}
			if (spawnedBlocksEntry.IsRefunded)
			{
				this.blockEntries.RemoveAt(i);
			}
		}
	}

	public void RefundSpawnedBlock(Vector3i blockPos)
	{
		for (int i = 0; i < this.blockEntries.Count; i++)
		{
			GameEventManager.SpawnedBlocksEntry spawnedBlocksEntry = this.blockEntries[i];
			if (spawnedBlocksEntry.BlockList.Contains(blockPos) && !spawnedBlocksEntry.IsRefunded)
			{
				spawnedBlocksEntry.GameEvent.SetRefundNeeded();
				spawnedBlocksEntry.IsRefunded = true;
			}
		}
	}

	public void SendBlockDamageUpdate(Vector3i blockPos)
	{
		for (int i = 0; i < this.blockEntries.Count; i++)
		{
			GameEventManager.SpawnedBlocksEntry spawnedBlocksEntry = this.blockEntries[i];
			if (spawnedBlocksEntry.BlockList.Contains(blockPos))
			{
				spawnedBlocksEntry.GameEvent.EventVariables.ModifyEventVariable("Damaged", GameEventVariables.OperationTypes.Add, 1, int.MinValue, int.MaxValue);
			}
		}
	}

	public void SetGameEventFlag(GameEventManager.GameEventFlagTypes flag, bool value, float duration)
	{
		if (value)
		{
			for (int i = 0; i < this.GameEventFlags.Count; i++)
			{
				if (this.GameEventFlags[i].FlagType == flag)
				{
					this.GameEventFlags[i].Duration = duration;
					return;
				}
			}
			this.GameEventFlags.Add(new GameEventManager.GameEventFlag
			{
				FlagType = flag,
				Duration = duration
			});
			this.HandleFlagChanged(flag, false, true);
			return;
		}
		for (int j = 0; j < this.GameEventFlags.Count; j++)
		{
			if (this.GameEventFlags[j].FlagType == flag)
			{
				this.GameEventFlags.RemoveAt(j);
				this.HandleFlagChanged(flag, true, false);
				return;
			}
		}
	}

	public bool CheckGameEventFlag(GameEventManager.GameEventFlagTypes flag)
	{
		for (int i = 0; i < this.GameEventFlags.Count; i++)
		{
			if (this.GameEventFlags[i].FlagType == flag)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleEventFlagUpdates(float deltaTime)
	{
		for (int i = this.GameEventFlags.Count - 1; i >= 0; i--)
		{
			GameEventManager.GameEventFlag gameEventFlag = this.GameEventFlags[i];
			if (gameEventFlag.Duration > 0f)
			{
				gameEventFlag.Duration -= deltaTime;
				this.HandleFlagBuffUpdates(gameEventFlag.FlagType, deltaTime);
				if (gameEventFlag.Duration <= 0f)
				{
					this.GameEventFlags.RemoveAt(i);
					this.HandleFlagChanged(gameEventFlag.FlagType, true, false);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleFlagBuffUpdates(GameEventManager.GameEventFlagTypes flag, float deltaTime)
	{
		this.gameFlagCheckTime -= deltaTime;
		if (this.gameFlagCheckTime <= 0f)
		{
			string name = "";
			switch (flag)
			{
			case GameEventManager.GameEventFlagTypes.BigHead:
				name = "twitch_buffBigHead";
				break;
			case GameEventManager.GameEventFlagTypes.Dancing:
				name = "twitch_buffDance";
				break;
			case GameEventManager.GameEventFlagTypes.BucketHead:
				name = "twitch_buffBucketHead";
				break;
			case GameEventManager.GameEventFlagTypes.TinyZombies:
				name = "twitch_buffTinyZombies";
				break;
			}
			foreach (EntityPlayer entityPlayer in GameManager.Instance.World.Players.dict.Values)
			{
				if (!entityPlayer.Buffs.HasBuff(name))
				{
					entityPlayer.Buffs.AddBuff(name, -1, true, false, -1f);
				}
			}
			this.gameFlagCheckTime = 1f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleFlagChanged(GameEventManager.GameEventFlagTypes flag, bool oldValue, bool newValue)
	{
		switch (flag)
		{
		case GameEventManager.GameEventFlagTypes.BigHead:
			foreach (Entity entity in GameManager.Instance.World.Entities.dict.Values)
			{
				EntityAlive entityAlive = entity as EntityAlive;
				if (entityAlive != null && !(entityAlive is EntityPlayer))
				{
					if (newValue)
					{
						entityAlive.Buffs.AddBuff("twitch_bighead", -1, true, false, -1f);
					}
					else
					{
						entityAlive.Buffs.RemoveBuff("twitch_bighead", true);
					}
				}
			}
			using (Dictionary<int, EntityPlayer>.ValueCollection.Enumerator enumerator2 = GameManager.Instance.World.Players.dict.Values.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					EntityPlayer entityPlayer = enumerator2.Current;
					if (newValue)
					{
						entityPlayer.Buffs.AddBuff("twitch_buffBigHead", -1, true, false, -1f);
					}
					else
					{
						entityPlayer.Buffs.RemoveBuff("twitch_buffBigHead", true);
					}
				}
				return;
			}
			break;
		case GameEventManager.GameEventFlagTypes.Dancing:
			break;
		case GameEventManager.GameEventFlagTypes.BucketHead:
			goto IL_215;
		case GameEventManager.GameEventFlagTypes.TinyZombies:
			goto IL_321;
		default:
			return;
		}
		foreach (Entity entity2 in GameManager.Instance.World.Entities.dict.Values)
		{
			EntityAlive entityAlive2 = entity2 as EntityAlive;
			if (entityAlive2 != null && !(entityAlive2 is EntityPlayer))
			{
				if (newValue)
				{
					entityAlive2.Buffs.AddBuff("twitch_dance", -1, true, false, -1f);
				}
				else
				{
					entityAlive2.Buffs.RemoveBuff("twitch_dance", true);
				}
			}
		}
		using (Dictionary<int, EntityPlayer>.ValueCollection.Enumerator enumerator2 = GameManager.Instance.World.Players.dict.Values.GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				EntityPlayer entityPlayer2 = enumerator2.Current;
				if (newValue)
				{
					entityPlayer2.Buffs.AddBuff("twitch_buffDance", -1, true, false, -1f);
				}
				else
				{
					entityPlayer2.Buffs.RemoveBuff("twitch_buffDance", true);
				}
			}
			return;
		}
		IL_215:
		foreach (Entity entity3 in GameManager.Instance.World.Entities.dict.Values)
		{
			EntityAlive entityAlive3 = entity3 as EntityAlive;
			if (entityAlive3 != null && !(entityAlive3 is EntityPlayer) && !(entityAlive3 is EntityVehicle))
			{
				if (newValue)
				{
					entityAlive3.Buffs.AddBuff("twitch_buckethead", -1, true, false, -1f);
				}
				else
				{
					entityAlive3.Buffs.RemoveBuff("twitch_buckethead", true);
				}
			}
		}
		using (Dictionary<int, EntityPlayer>.ValueCollection.Enumerator enumerator2 = GameManager.Instance.World.Players.dict.Values.GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				EntityPlayer entityPlayer3 = enumerator2.Current;
				if (newValue)
				{
					entityPlayer3.Buffs.AddBuff("twitch_buffBucketHead", -1, true, false, -1f);
				}
				else
				{
					entityPlayer3.Buffs.RemoveBuff("twitch_buffBucketHead", true);
				}
			}
			return;
		}
		IL_321:
		foreach (Entity entity4 in GameManager.Instance.World.Entities.dict.Values)
		{
			EntityAlive entityAlive4 = entity4 as EntityAlive;
			if (entityAlive4 != null && entityAlive4 is EntityZombie)
			{
				if (newValue)
				{
					entityAlive4.Buffs.AddBuff("twitch_tiny", -1, true, false, -1f);
				}
				else
				{
					entityAlive4.Buffs.RemoveBuff("twitch_tiny", true);
				}
			}
		}
		foreach (EntityPlayer entityPlayer4 in GameManager.Instance.World.Players.dict.Values)
		{
			if (newValue)
			{
				entityPlayer4.Buffs.AddBuff("twitch_buffTinyZombies", -1, true, false, -1f);
			}
			else
			{
				entityPlayer4.Buffs.RemoveBuff("twitch_buffTinyZombies", true);
			}
		}
	}

	public void HandleSpawnModifier(EntityAlive alive)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			return;
		}
		for (int i = 0; i < this.GameEventFlags.Count; i++)
		{
			switch (this.GameEventFlags[i].FlagType)
			{
			case GameEventManager.GameEventFlagTypes.BigHead:
				if (alive != null && !(alive is EntityPlayer))
				{
					alive.Buffs.AddBuff("twitch_bighead", -1, true, false, -1f);
				}
				break;
			case GameEventManager.GameEventFlagTypes.Dancing:
				if (alive != null && !(alive is EntityPlayer))
				{
					alive.Buffs.AddBuff("twitch_dance", -1, true, false, -1f);
				}
				break;
			case GameEventManager.GameEventFlagTypes.BucketHead:
				if (alive != null && !(alive is EntityPlayer) && !(alive is EntityVehicle))
				{
					alive.Buffs.AddBuff("twitch_buckethead", -1, true, false, -1f);
				}
				break;
			case GameEventManager.GameEventFlagTypes.TinyZombies:
				if (alive != null && alive is EntityZombie)
				{
					alive.Buffs.AddBuff("twitch_tiny", -1, true, false, -1f);
				}
				break;
			}
		}
	}

	public void HandleForceBossDespawn(EntityPlayer player)
	{
		for (int i = 0; i < this.BossGroups.Count; i++)
		{
			if (this.BossGroups[i].TargetPlayer == player)
			{
				this.BossGroups[i].RemoveNavObjects();
				this.BossGroups[i].DespawnAll();
			}
		}
	}

	public int SetupBossGroup(EntityPlayer target, EntityAlive boss, List<EntityAlive> minions, BossGroup.BossGroupTypes bossGroupType, string bossIcon)
	{
		for (int i = 0; i < this.BossGroups.Count; i++)
		{
			if (this.BossGroups[i].BossEntity == boss)
			{
				return this.BossGroups[i].BossGroupID;
			}
		}
		BossGroup bossGroup = new BossGroup(target, boss, minions, bossGroupType, bossIcon);
		this.BossGroups.Add(bossGroup);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageBossEvent>().Setup(NetPackageBossEvent.BossEventTypes.AddGroup, bossGroup.BossGroupID, bossGroup.CurrentGroupType, bossGroup.BossEntityID, bossGroup.MinionEntityIDs, bossGroup.BossIcon), false, -1, -1, -1, null, 192);
		return bossGroup.BossGroupID;
	}

	public void UpdateBossGroupType(int bossGroupID, BossGroup.BossGroupTypes bossGroupType)
	{
		for (int i = 0; i < this.BossGroups.Count; i++)
		{
			if (bossGroupID == this.BossGroups[i].BossGroupID)
			{
				BossGroup bossGroup = this.BossGroups[i];
				bossGroup.CurrentGroupType = bossGroupType;
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageBossEvent>().Setup(NetPackageBossEvent.BossEventTypes.UpdateGroupType, bossGroupID, bossGroupType), false, -1, -1, -1, null, 192);
				}
				if (bossGroup.IsCurrent)
				{
					bossGroup.RemoveNavObjects();
					bossGroup.AddNavObjects();
				}
			}
		}
	}

	public void SetupClientBossGroup(int bossGroupID, BossGroup.BossGroupTypes bossGroupType, int bossID, List<int> minionIDs, string bossIcon1)
	{
		for (int i = 0; i < this.BossGroups.Count; i++)
		{
			if (bossGroupID == this.BossGroups[i].BossGroupID)
			{
				this.BossGroups[i].CurrentGroupType = bossGroupType;
				return;
			}
		}
		this.BossGroups.Add(new BossGroup(bossGroupID, bossGroupType, bossID, minionIDs, bossIcon1));
	}

	public void RemoveClientBossGroup(int bossGroupID)
	{
		for (int i = 0; i < this.BossGroups.Count; i++)
		{
			if (bossGroupID == this.BossGroups[i].BossGroupID)
			{
				this.BossGroups[i].RemoveNavObjects();
				this.BossGroups.RemoveAt(i);
				return;
			}
		}
	}

	public void RemoveEntityFromBossGroup(int bossGroupID, int entityID)
	{
		for (int i = 0; i < this.BossGroups.Count; i++)
		{
			if (bossGroupID == this.BossGroups[i].BossGroupID)
			{
				this.BossGroups[i].RemoveMinion(entityID);
			}
		}
	}

	public void SendBossGroups(int entityID)
	{
		for (int i = 0; i < this.BossGroups.Count; i++)
		{
			BossGroup bossGroup = this.BossGroups[i];
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageBossEvent>().Setup(NetPackageBossEvent.BossEventTypes.AddGroup, bossGroup.BossGroupID, bossGroup.CurrentGroupType, bossGroup.BossEntityID, bossGroup.MinionEntityIDs, bossGroup.BossIcon), false, entityID, -1, -1, null, 192);
		}
	}

	public void RequestBossGroupStatRefresh(int bossGroupID, int playerID)
	{
		if (this.BossGroups.Count > 0)
		{
			this.BossGroups[0].RefreshStats(playerID);
		}
	}

	public void HandleBossGroupUpdates(float deltaTime)
	{
		if (GameManager.Instance.World == null)
		{
			return;
		}
		this.bossCheckTime -= deltaTime;
		for (int i = this.BossGroups.Count - 1; i >= 0; i--)
		{
			this.BossGroups[i].HandleAutoPull();
			this.BossGroups[i].HandleLiveHandling();
			if (this.bossCheckTime <= 0f && this.BossGroups[i].ServerUpdate())
			{
				if (this.CurrentBossGroup == this.BossGroups[i])
				{
					this.CurrentBossGroup = null;
				}
				this.BossGroups.RemoveAt(i);
			}
		}
		if (this.bossCheckTime <= 0f)
		{
			this.bossCheckTime = 1f;
		}
	}

	public void UpdateCurrentBossGroup(EntityPlayerLocal player)
	{
		this.serverBossGroupCheckTime -= Time.deltaTime;
		if (!this.BossGroupInitialized)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageBossEvent>().Setup(NetPackageBossEvent.BossEventTypes.RequestGroups, -1), false);
			}
			this.BossGroupInitialized = true;
			return;
		}
		if (this.serverBossGroupCheckTime <= 0f)
		{
			if (this.CurrentBossGroup != null)
			{
				this.CurrentBossGroup.Update(player);
				if (this.CurrentBossGroup.ReadyForRemove || !this.CurrentBossGroup.IsPlayerWithinRange(player))
				{
					this.CurrentBossGroup = null;
				}
			}
			else
			{
				for (int i = 0; i < this.BossGroups.Count; i++)
				{
					this.BossGroups[i].Update(player);
					if (!this.BossGroups[i].ReadyForRemove && this.BossGroups[i].IsPlayerWithinRange(player))
					{
						this.CurrentBossGroup = this.BossGroups[i];
						this.serverBossGroupCheckTime = 1f;
						return;
					}
				}
				this.CurrentBossGroup = null;
			}
			this.serverBossGroupCheckTime = 1f;
		}
	}

	public void RegisterLink(EntityPlayer player, GameEventActionSequence seq, string tag)
	{
		for (int i = 0; i < this.SequenceLinks.Count; i++)
		{
			if (this.SequenceLinks[i].CheckLink(player, tag))
			{
				return;
			}
		}
		this.SequenceLinks.Add(new GameEventManager.SequenceLink
		{
			Owner = player,
			OwnerSeq = seq,
			Tag = tag
		});
	}

	public bool HasSequenceLink(GameEventActionSequence seq)
	{
		for (int i = 0; i < this.SequenceLinks.Count; i++)
		{
			if (this.SequenceLinks[i].OwnerSeq == seq)
			{
				return true;
			}
		}
		return false;
	}

	public GameEventActionSequence GetSequenceLink(EntityPlayer player, string tag)
	{
		if (player == null || tag == "")
		{
			return null;
		}
		for (int i = 0; i < this.SequenceLinks.Count; i++)
		{
			if (this.SequenceLinks[i].CheckLink(player, tag))
			{
				return this.SequenceLinks[i].OwnerSeq;
			}
		}
		return null;
	}

	public void UnRegisterLink(EntityPlayer player, string tag)
	{
		for (int i = 0; i < this.SequenceLinks.Count; i++)
		{
			if (this.SequenceLinks[i].CheckLink(player, tag))
			{
				this.SequenceLinks.RemoveAt(i);
			}
		}
	}

	public static int GetIntValue(EntityAlive alive, string value, int defaultValue = 0)
	{
		if (string.IsNullOrEmpty(value))
		{
			return defaultValue;
		}
		if (value.StartsWith("@"))
		{
			if (alive != null)
			{
				return (int)alive.Buffs.GetCustomVar(value.Substring(1), 0f);
			}
			return defaultValue;
		}
		else
		{
			if (value.Contains("-"))
			{
				string[] array = value.Split('-', StringSplitOptions.None);
				int min = StringParsers.ParseSInt32(array[0], 0, -1, NumberStyles.Integer);
				int maxExclusive = StringParsers.ParseSInt32(array[1], 0, -1, NumberStyles.Integer) + 1;
				return GameEventManager.instance.Random.RandomRange(min, maxExclusive);
			}
			int result = 0;
			StringParsers.TryParseSInt32(value, out result, 0, -1, NumberStyles.Integer);
			return result;
		}
	}

	public static float GetFloatValue(EntityAlive alive, string value, float defaultValue = 0f)
	{
		if (string.IsNullOrEmpty(value))
		{
			return defaultValue;
		}
		if (value.StartsWith("@"))
		{
			if (alive != null)
			{
				return alive.Buffs.GetCustomVar(value.Substring(1), 0f);
			}
			return defaultValue;
		}
		else
		{
			if (value.Contains("-"))
			{
				string[] array = value.Split('-', StringSplitOptions.None);
				float min = (float)StringParsers.ParseSInt32(array[0], 0, -1, NumberStyles.Integer);
				float maxExclusive = (float)(StringParsers.ParseSInt32(array[1], 0, -1, NumberStyles.Integer) + 1);
				return GameEventManager.instance.Random.RandomRange(min, maxExclusive);
			}
			float result = 0f;
			StringParsers.TryParseFloat(value, out result, 0, -1, NumberStyles.Any);
			return result;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameEventManager instance = null;

	public static Dictionary<string, GameEventActionSequence> GameEventSequences = new Dictionary<string, GameEventActionSequence>();

	public GameRandom Random;

	public List<string> ActiveRecipients = new List<string>();

	public List<GameEventActionSequence> ActionSequenceUpdates = new List<GameEventActionSequence>();

	public List<GameEventManager.SpawnEntry> spawnEntries = new List<GameEventManager.SpawnEntry>();

	public List<GameEventManager.SpawnedBlocksEntry> blockEntries = new List<GameEventManager.SpawnedBlocksEntry>();

	public List<GameEventManager.Category> CategoryList = new List<GameEventManager.Category>();

	public HomerunManager HomerunManager = new HomerunManager();

	public int MaxSpawnCount = 20;

	public int ReservedCount;

	public const int AttackTime = 12000;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameEventManager.GameEventFlag> GameEventFlags = new List<GameEventManager.GameEventFlag>();

	public bool BossGroupInitialized;

	public List<BossGroup> BossGroups = new List<BossGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public BossGroup currentBossGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public float serverBossGroupCheckTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float bossCheckTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float gameFlagCheckTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float attackTimerUpdate = 2f;

	public List<GameEventManager.SequenceLink> SequenceLinks = new List<GameEventManager.SequenceLink>();

	public class Category
	{
		public string Name;

		public string DisplayName;

		public string Icon;
	}

	public enum GameEventFlagTypes
	{
		Invalid = -1,
		BigHead,
		Dancing,
		BucketHead,
		TinyZombies
	}

	public class SpawnEntry
	{
		public void HandleUpdate()
		{
			if (!this.IsAggressive)
			{
				return;
			}
			EntityPlayer entityPlayer = this.SpawnedEntity.GetAttackTarget() as EntityPlayer;
			if (entityPlayer == null)
			{
				this.SpawnedEntity.SetAttackTarget(this.SpawnedEntity.world.GetClosestPlayer(this.SpawnedEntity, 500f, false), 1000);
				return;
			}
			this.SpawnedEntity.SetAttackTarget(entityPlayer, 1000);
		}

		public EntityAlive SpawnedEntity;

		public EntityAlive Target;

		public EntityPlayer Requester;

		public GameEventActionSequence GameEvent;

		public bool IsAggressive;
	}

	public class SpawnedBlocksEntry
	{
		public SpawnedBlocksEntry()
		{
			this.BlockGroupID = ++GameEventManager.SpawnedBlocksEntry.newID;
		}

		public bool RemoveBlock(Vector3i blockPos)
		{
			bool result = false;
			for (int i = this.BlockList.Count - 1; i >= 0; i--)
			{
				if (this.BlockList[i] == blockPos)
				{
					this.BlockList.RemoveAt(i);
					result = true;
				}
			}
			return result;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public bool TryRemoveBlocks()
		{
			List<BlockChangeInfo> list = null;
			World world = GameManager.Instance.World;
			IChunk chunk = null;
			for (int i = this.BlockList.Count - 1; i >= 0; i--)
			{
				if (world.GetChunkFromWorldPos(this.BlockList[i], ref chunk))
				{
					if (list == null)
					{
						list = new List<BlockChangeInfo>();
					}
					list.Add(new BlockChangeInfo(0, this.BlockList[i], BlockValue.Air, false));
					this.BlockList.RemoveAt(i);
				}
			}
			if (list != null)
			{
				GameManager.Instance.World.SetBlocksRPC(list);
			}
			if (this.BlockList.Count == 0)
			{
				if (!string.IsNullOrEmpty(this.RemoveSound))
				{
					Manager.BroadcastPlayByLocalPlayer(this.Center, this.RemoveSound);
				}
				if (this.RefundOnRemove)
				{
					this.GameEvent.SetRefundNeeded();
				}
				if (this.Requester is EntityPlayerLocal)
				{
					GameEventManager.Current.HandleGameBlocksRemoved(this.BlockGroupID, this.IsDespawn);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(NetPackageGameEventResponse.ResponseTypes.BlocksRemoved, -1, this.BlockGroupID, "", this.IsDespawn), false, this.Requester.entityId, -1, -1, null, 192);
				}
			}
			return this.BlockList.Count == 0;
		}

		public int BlockGroupID;

		[PublicizedFrom(EAccessModifier.Private)]
		public static int newID;

		public List<Vector3i> BlockList = new List<Vector3i>();

		public Vector3 Center;

		public Entity Target;

		public EntityPlayer Requester;

		public GameEventActionSequence GameEvent;

		public float TimeAlive = -1f;

		public string RemoveSound;

		public bool RefundOnRemove;

		public bool IsDespawn;

		public bool IsRefunded;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class GameEventFlag
	{
		public GameEventManager.GameEventFlagTypes FlagType;

		public float Duration = -1f;
	}

	public class SequenceLink
	{
		public bool CheckLink(EntityPlayer player, string tag)
		{
			return this.Owner == player && this.Tag == tag;
		}

		public EntityPlayer Owner;

		public GameEventActionSequence OwnerSeq;

		public string Tag = "";
	}
}
