using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityTrader : EntityNPC
{
	public TraderData TraderData
	{
		get
		{
			if (this.TileEntityTrader != null)
			{
				return this.TileEntityTrader.TraderData;
			}
			return null;
		}
	}

	public TraderInfo TraderInfo
	{
		get
		{
			if (this.TileEntityTrader != null)
			{
				return this.TileEntityTrader.TraderData.TraderInfo;
			}
			return null;
		}
	}

	public override bool IsValidAimAssistSnapTarget
	{
		get
		{
			return false;
		}
	}

	public override void InitLocation(Vector3 _pos, Vector3 _rot)
	{
		_pos.y = Mathf.Floor(_pos.y);
		base.InitLocation(_pos, _rot);
		this.PhysicsTransform.gameObject.SetActive(false);
	}

	public override void PostInit()
	{
		base.PostInit();
		this.SetupStartingItems();
		if (base.NPCInfo != null && base.NPCInfo.TraderID > 0)
		{
			Chunk chunk = GameManager.Instance.World.GetChunkFromWorldPos(World.worldToBlockPos(this.position)) as Chunk;
			if (this.TileEntityTrader == null)
			{
				this.TileEntityTrader = new TileEntityTrader(chunk);
				this.TileEntityTrader.entityId = this.entityId;
				this.TileEntityTrader.TraderData.TraderID = base.NPCInfo.TraderID;
			}
			else
			{
				this.TileEntityTrader.SetChunk(chunk);
			}
			this.IsGodMode.Value = true;
		}
		this.inventory.SetHoldingItemIdx(0);
		this.emodel.avatarController.SetArchetypeStance(base.NPCInfo.CurrentStance);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetupStartingItems()
	{
		for (int i = 0; i < this.itemsOnEnterGame.Count; i++)
		{
			ItemStack itemStack = this.itemsOnEnterGame[i];
			ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
			if (forId.HasQuality)
			{
				itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6, false, null, 1f);
			}
			else
			{
				itemStack.count = forId.Stacknumber.Value;
			}
			this.inventory.SetItem(i, itemStack);
		}
	}

	public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
	{
		if (this.IsDead() || base.NPCInfo == null)
		{
			return new EntityActivationCommand[0];
		}
		return new EntityActivationCommand[]
		{
			new EntityActivationCommand("talk", "talk", true),
			new EntityActivationCommand("trade", "map_trader", true),
			new EntityActivationCommand("remove", "x", GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && !GameUtils.IsPlaytesting())
		};
	}

	public void ActivateTrader(bool traderIsOpen)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			this.waitingToActivate = false;
		}
		if (traderIsOpen)
		{
			int num = this.outstandingIndexInBlockActivationCommands;
			Vector3i blockPos = this.outstandingTePos;
			EntityAlive entityAlive = this.outstandingEntityFocusing;
			EntityPlayerLocal entityPlayerLocal = entityAlive as EntityPlayerLocal;
			LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
			QuestEventManager.Current.NPCInteracted(this);
			QuestEventManager.Current.NPCMet(this);
			Quest nextCompletedQuest = (entityAlive as EntityPlayerLocal).QuestJournal.GetNextCompletedQuest(null, this.entityId);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.activeQuests = QuestEventManager.Current.GetQuestList(GameManager.Instance.World, this.entityId, entityAlive.entityId);
				if (this.activeQuests == null)
				{
					this.SetupActiveQuestsForPlayer(entityPlayerLocal, -1);
				}
			}
			switch (num)
			{
			case 0:
				uiforPlayer.xui.Dialog.Respondent = this;
				if (nextCompletedQuest != null)
				{
					uiforPlayer.xui.Dialog.QuestTurnIn = nextCompletedQuest;
					uiforPlayer.windowManager.CloseAllOpenWindows(null, false);
					this.PlayVoiceSetEntry("quest_complete", entityPlayerLocal, true, true);
					uiforPlayer.windowManager.Open("questTurnIn", true, false, true);
					return;
				}
				if (!uiforPlayer.windowManager.IsWindowOpen("dialog"))
				{
					uiforPlayer.windowManager.CloseAllOpenWindows(null, false);
					uiforPlayer.windowManager.Open("dialog", true, false, true);
					QuestEventManager.Current.NPCInteracted(this);
					return;
				}
				break;
			case 1:
				uiforPlayer.xui.Trader.TraderEntity = this;
				if (nextCompletedQuest == null)
				{
					GameManager.Instance.TELockServer(0, blockPos, this.TileEntityTrader.entityId, entityAlive.entityId, null);
					this.PlayVoiceSetEntry("trade", entityPlayerLocal, true, true);
					return;
				}
				uiforPlayer.xui.Dialog.QuestTurnIn = nextCompletedQuest;
				uiforPlayer.windowManager.CloseAllOpenWindows(null, false);
				this.PlayVoiceSetEntry("quest_complete", entityPlayerLocal, true, true);
				uiforPlayer.windowManager.Open("questTurnIn", true, false, true);
				return;
			case 2:
			{
				Waypoint entityVehicleWaypoint = entityPlayerLocal.Waypoints.GetEntityVehicleWaypoint(this.entityId);
				if (entityVehicleWaypoint != null)
				{
					entityPlayerLocal.Waypoints.Collection.Remove(entityVehicleWaypoint);
					NavObjectManager.Instance.UnRegisterNavObjectByPosition(entityVehicleWaypoint.pos, "waypoint");
				}
				GameEventManager.Current.HandleAction("game_remove_entity", entityPlayerLocal, this, false, "", "", false, true, "", null);
				break;
			}
			default:
				return;
			}
		}
	}

	public void SetupActiveQuestsForPlayer(EntityPlayer player, int overrideFactionPoints = -1)
	{
		this.activeQuests = this.PopulateActiveQuests(player, -1, overrideFactionPoints);
		QuestEventManager.Current.SetupQuestList(this.entityId, player.entityId, this.activeQuests);
	}

	public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
	{
		this.outstandingIndexInBlockActivationCommands = _indexInBlockActivationCommands;
		this.outstandingTePos = _tePos;
		this.outstandingEntityFocusing = _entityFocusing;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.ActivateTrader(this.traderArea == null || !this.traderArea.IsClosed);
		}
		else
		{
			EntityPlayerLocal entityPlayerLocal = _entityFocusing as EntityPlayerLocal;
			if (entityPlayerLocal != null && !this.waitingToActivate && !entityPlayerLocal.PlayerUI.windowManager.IsModalWindowOpen())
			{
				this.waitingToActivate = true;
				NetPackageTraderStatus package = NetPackageManager.GetPackage<NetPackageTraderStatus>();
				package.Setup(this.entityId, false);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package, false);
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void StartTrading(EntityPlayer _player)
	{
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal);
		uiforPlayer.xui.Trader.TraderEntity = this;
		uiforPlayer.xui.Dialog.keepZoomOnClose = true;
		GameManager.Instance.TELockServer(0, base.GetBlockPosition(), this.TileEntityTrader.entityId, _player.entityId, null);
		QuestEventManager.Current.NPCInteracted(this);
		this.PlayVoiceSetEntry("trade", _player, true, true);
	}

	public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
	}

	public override void OnUpdateLive()
	{
		base.OnUpdateLive();
		if (this.questDictionary.Count == 0)
		{
			this.PopulateQuestList();
		}
		if (this.nativeCollider)
		{
			this.nativeCollider.enabled = true;
		}
		if (!GameManager.IsDedicatedServer)
		{
			EntityPlayerLocal primaryPlayer = this.world.GetPrimaryPlayer();
			this.emodel.SetLookAt(primaryPlayer.getHeadPosition());
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (this.traderArea == null)
			{
				this.traderArea = this.world.GetTraderAreaAt(new Vector3i(this.position));
			}
			if (this.traderArea != null)
			{
				if (this.updateTime <= 0f)
				{
					this.updateTime = Time.time + 3f;
				}
				if (Time.time > this.updateTime)
				{
					this.updateTime = Time.time + 1f;
					List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(this, new Bounds(this.position, Vector3.one * 10f));
					if (entitiesInBounds.Count > 0)
					{
						for (int i = 0; i < entitiesInBounds.Count; i++)
						{
							if (entitiesInBounds[i] is EntityNPC && entitiesInBounds[i].EntityClass == base.EntityClass)
							{
								if (entitiesInBounds[i].entityId < this.entityId)
								{
									this.IsDespawned = true;
									this.MarkToUnload();
								}
							}
							else if (entitiesInBounds[i] is EntityPlayer)
							{
								EntityPlayer entityPlayer = entitiesInBounds[i] as EntityPlayer;
								if (base.CanSee(entityPlayer))
								{
									if (this.GreetingDictionary.ContainsKey(entityPlayer))
									{
										if (Time.time < this.GreetingDictionary[entityPlayer])
										{
											this.GreetingDictionary[entityPlayer] = Time.time + EntityTrader.traderTalkDelayTime;
											goto IL_20D;
										}
										this.GreetingDictionary[entityPlayer] = Time.time + EntityTrader.traderTalkDelayTime;
									}
									else
									{
										this.GreetingDictionary.Add(entityPlayer, Time.time + EntityTrader.traderTalkDelayTime);
									}
									this.PlayVoiceSetEntry("greeting", entityPlayer, false, true);
									this.SendAnimReaction(1);
								}
							}
							IL_20D:;
						}
					}
					if (this.TraderInfo == null)
					{
						return;
					}
					if (!this.traderArea.IsClosed)
					{
						if (this.TraderInfo.IsWarningTime)
						{
							if (!this.warningPlayed)
							{
								this.warningPlayed = true;
								this.traderArea.HandleWarning(this.world, this);
							}
						}
						else
						{
							this.warningPlayed = false;
						}
					}
					bool flag = !this.TraderInfo.IsOpen;
					if (this.traderArea.IsClosed != flag || this.firstTime)
					{
						bool playSound;
						if (flag)
						{
							playSound = this.TraderInfo.ShouldPlayCloseSound;
						}
						else
						{
							playSound = this.TraderInfo.ShouldPlayOpenSound;
						}
						this.firstTime = !this.traderArea.SetClosed(this.world, flag, this, playSound);
					}
				}
			}
		}
	}

	public void PopulateQuestList()
	{
		if (base.NPCInfo == null || base.NPCInfo.Quests == null)
		{
			return;
		}
		this.specialQuestList = new List<QuestEntry>();
		this.questDictionary.Clear();
		for (int i = 0; i < base.NPCInfo.Quests.Count; i++)
		{
			string questID = base.NPCInfo.Quests[i].QuestID;
			if (QuestClass.GetQuest(questID).CheckCriteriaQuestGiver(this))
			{
				QuestEntry questEntry = base.NPCInfo.Quests[i];
				questEntry.QuestID = questID;
				if (questEntry.QuestClass.UniqueKey == "")
				{
					if (!this.questDictionary.ContainsKey((int)questEntry.QuestClass.DifficultyTier))
					{
						this.questDictionary.Add((int)questEntry.QuestClass.DifficultyTier, new List<QuestEntry>());
					}
					this.questDictionary[(int)questEntry.QuestClass.DifficultyTier].Add(questEntry);
				}
				else
				{
					this.specialQuestList.Add(questEntry);
				}
			}
		}
	}

	public bool UpdateLocations(int tier, List<Vector2> pois)
	{
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			List<PrefabInstance> prefabsForTrader = QuestEventManager.Current.GetPrefabsForTrader(this.traderArea, tier, i, this.world.GetGameRandom());
			num += ((prefabsForTrader != null) ? prefabsForTrader.Count : 0);
		}
		return pois.Count == num;
	}

	public void SetActiveQuests(EntityPlayer player, NetPackageNPCQuestList.QuestPacketEntry[] questList)
	{
		if (this.activeQuests == null)
		{
			this.activeQuests = new List<Quest>();
		}
		this.activeQuests.Clear();
		if (questList != null)
		{
			for (int i = 0; i < questList.Length; i++)
			{
				Quest quest = QuestClass.GetQuest(questList[i].QuestID).CreateQuest();
				quest.QuestGiverID = this.entityId;
				quest.QuestFaction = base.NPCInfo.QuestFaction;
				quest.SetPosition(this, questList[i].QuestLocation, questList[i].QuestSize);
				quest.SetPositionData(Quest.PositionDataTypes.QuestGiver, this.position);
				quest.SetPositionData(Quest.PositionDataTypes.TraderPosition, questList[i].TraderPos);
				quest.DataVariables.Add("POIName", Localization.Get(questList[i].POIName, false));
				this.activeQuests.Add(quest);
			}
		}
	}

	public void ClearActiveQuests(int playerID)
	{
		try
		{
			this.activeQuests = null;
		}
		catch
		{
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			QuestEventManager.Current.ClearQuestListForPlayer(this.entityId, playerID);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageNPCQuestList>().Setup(this.entityId, playerID), false);
	}

	public void HandleClientQuests(EntityPlayer player)
	{
		if (this.activeQuests == null && !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageNPCQuestList>().Setup(this.entityId, player.entityId, player.QuestJournal.GetCurrentFactionTier(base.NPCInfo.QuestFaction, 0, false)), false);
		}
	}

	public List<Quest> PopulateActiveQuests(EntityPlayer player, int currentTier = -1, int questFactionPoints = -1)
	{
		if (this.questDictionary.Count == 0)
		{
			this.PopulateQuestList();
			if (this.questDictionary.Count == 0)
			{
				return null;
			}
		}
		bool @bool = GameStats.GetBool(EnumGameStats.EnemySpawnMode);
		List<Quest> list = new List<Quest>();
		this.tempTopTierQuests.Clear();
		this.tempSpecialQuests.Clear();
		this.uniqueKeysUsed.Clear();
		Vector2 vector;
		if (this.traderArea != null)
		{
			vector = new Vector2((float)this.traderArea.Position.x, (float)this.traderArea.Position.z);
		}
		else
		{
			vector = new Vector2(this.position.x, this.position.z);
		}
		if (currentTier == -1)
		{
			currentTier = player.QuestJournal.GetCurrentFactionTier(base.NPCInfo.QuestFaction, 0, false);
		}
		if (questFactionPoints == -1)
		{
			questFactionPoints = player.QuestJournal.GlobalFactionPoints;
		}
		QuestTraderData traderData = player.QuestJournal.GetTraderData(vector);
		if (traderData != null)
		{
			traderData.CheckReset(player);
		}
		this.usedPOILocations.Clear();
		List<QuestEntry> list2 = new List<QuestEntry>();
		for (int i = 1; i <= currentTier; i++)
		{
			List<Vector2> list3 = player.QuestJournal.GetUsedPOIs(vector, i);
			if (this.UpdateLocations(i, this.usedPOILocations))
			{
				list3 = null;
				if (traderData != null)
				{
					traderData.ClearTier(i);
					if (!(player is EntityPlayerLocal))
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNPCQuestList>().SetupClear(player.entityId, vector, i), false, player.entityId, -1, -1, null, 192);
					}
				}
			}
			if (list3 != null)
			{
				this.usedPOILocations.AddRange(list3);
			}
			list2.Clear();
			for (int j = 0; j < this.questDictionary[i].Count; j++)
			{
				QuestEntry questEntry = this.questDictionary[i][j];
				if ((questEntry.StartStage == -1 || questEntry.StartStage <= questFactionPoints) && (questEntry.EndStage == -1 || questEntry.EndStage >= questFactionPoints))
				{
					list2.Add(questEntry);
				}
			}
			int num = 0;
			int num2 = 0;
			while (num2 < 100 && list2.Count != 0)
			{
				int index = this.rand.RandomRange(list2.Count);
				QuestEntry questEntry2 = list2[index];
				if (this.rand.RandomFloat < questEntry2.Prob)
				{
					QuestClass questClass = questEntry2.QuestClass;
					Quest quest = questClass.CreateQuest();
					quest.QuestGiverID = this.entityId;
					quest.QuestFaction = base.NPCInfo.QuestFaction;
					quest.SetPositionData(Quest.PositionDataTypes.QuestGiver, this.position);
					quest.SetPositionData(Quest.PositionDataTypes.TraderPosition, (this.traderArea != null) ? this.traderArea.Position : this.position);
					quest.SetupTags();
					if (@bool || !quest.QuestTags.Test_AnySet(QuestEventManager.clearTag))
					{
						if (quest.SetupPosition(this, player, this.usedPOILocations, player.entityId))
						{
							if (questClass.SingleQuest)
							{
								list2.RemoveAt(index);
							}
							list.Add(quest);
							num++;
						}
						if (quest.QuestTags.Test_AnySet(QuestEventManager.treasureTag) && GameSparksCollector.CollectGamePlayData)
						{
							GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.QuestOfferedDistance, ((int)Vector3.Distance(quest.Position, this.position) / 50 * 50).ToString(), 1, true, GameSparksCollector.GSDataCollection.SessionUpdates);
						}
						if (num == 5)
						{
							break;
						}
					}
				}
				num2++;
			}
		}
		for (int k = 0; k < this.specialQuestList.Count; k++)
		{
			QuestEntry questEntry3 = this.specialQuestList[k];
			if ((questEntry3.StartStage == -1 || questEntry3.StartStage <= questFactionPoints) && (questEntry3.EndStage == -1 || questEntry3.EndStage >= questFactionPoints))
			{
				list2.Add(questEntry3);
			}
			if (questEntry3.QuestClass.UniqueKey == "" || !this.uniqueKeysUsed.Contains(questEntry3.QuestClass.UniqueKey))
			{
				QuestClass questClass2 = questEntry3.QuestClass;
				if ((int)(questClass2.DifficultyTier - 1) <= currentTier && !player.QuestJournal.FindCompletedQuest(questClass2.ID, questClass2.Repeatable ? ((int)base.NPCInfo.QuestFaction) : -1))
				{
					int l = 0;
					while (l < 100)
					{
						Quest quest2 = questClass2.CreateQuest();
						quest2.QuestGiverID = this.entityId;
						quest2.QuestFaction = base.NPCInfo.QuestFaction;
						quest2.SetPositionData(Quest.PositionDataTypes.QuestGiver, this.position);
						quest2.SetPositionData(Quest.PositionDataTypes.TraderPosition, (this.traderArea != null) ? this.traderArea.Position : this.position);
						quest2.SetupTags();
						if (!quest2.NeedsNPCSetPosition || quest2.SetupPosition(this, player, this.usedPOILocations, player.entityId))
						{
							list.Add(quest2);
							if (questClass2.UniqueKey != "")
							{
								this.uniqueKeysUsed.Add(questClass2.UniqueKey);
							}
							if (GameSparksCollector.CollectGamePlayData)
							{
								GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.QuestTraderToTraderDistance, ((int)Vector3.Distance(quest2.Position, this.position) / 50 * 50).ToString(), 1, true, GameSparksCollector.GSDataCollection.SessionUpdates);
								break;
							}
							break;
						}
						else
						{
							l++;
						}
					}
				}
			}
		}
		return list;
	}

	public int GetQuestFactionPoints(EntityPlayer player)
	{
		return player.QuestJournal.GlobalFactionPoints;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEntityTargeted(EntityAlive target)
	{
		base.OnEntityTargeted(target);
		if (!this.isEntityRemote && base.GetSpawnerSource() != EnumSpawnerSource.Dynamic && target != null)
		{
			this.world.aiDirector.NotifyIntentToAttack(this, target);
		}
	}

	public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
	{
		if (base.NPCInfo != null && base.NPCInfo.TraderID > 0)
		{
			return;
		}
		base.SetAttackTarget((EntityAlive)GameManager.Instance.World.GetEntity(_dmResponse.Source.getEntityId()), 600);
		base.ProcessDamageResponseLocal(_dmResponse);
	}

	public override bool CanDamageEntity(int _sourceEntityId)
	{
		return false;
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
	{
		if (base.NPCInfo != null && base.NPCInfo.TraderID > 0)
		{
			return 0;
		}
		return base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
	}

	public override void AwardKill(EntityAlive killer)
	{
		if (base.NPCInfo != null && base.NPCInfo.TraderID > 0)
		{
			return;
		}
		base.AwardKill(killer);
	}

	public override Vector3 GetLookVector()
	{
		if (this.lookAtPosition.Equals(Vector3.zero))
		{
			return base.GetLookVector();
		}
		return Vector3.Normalize(this.lookAtPosition - this.getHeadPosition());
	}

	public override Ray GetLookRay()
	{
		return new Ray(this.position + new Vector3(0f, this.GetEyeHeight() * this.eyeHeightHackMod, 0f), this.GetLookVector());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
	{
	}

	public override void PlayVoiceSetEntry(string name, EntityPlayer player, bool ignoreTime = true, bool showReactionAnim = true)
	{
		if (this.lastVoiceTime - Time.time < 0f || ignoreTime)
		{
			string voiceSet = base.NPCInfo.VoiceSet;
			string a = base.NPCInfo.CurrentStance.ToStringCached<NPCInfo.StanceTypes>();
			if (voiceSet == "" || a == "")
			{
				return;
			}
			string text = (voiceSet + "_" + name).ToLower();
			Manager.StopAllSequencesOnEntity((player == null) ? this : player);
			if (this.lastSoundPlayed != "")
			{
				if (player == null)
				{
					base.StopOneShot(this.lastSoundPlayed);
				}
				else
				{
					player.StopOneShot(this.lastSoundPlayed);
				}
				this.lastSoundPlayed = "";
			}
			if (player == null)
			{
				this.PlayOneShot(text, false, false, false);
			}
			else if (player.isEntityRemote)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageAudioPlayInHead>().Setup(text, true), false, player.entityId, -1, -1, null, 192);
			}
			else
			{
				player.PlayOneShot(text, true, false, true);
			}
			this.lastSoundPlayed = text;
			if (showReactionAnim)
			{
				this.PlayAnimReaction(EntityTrader.AnimReaction.Neutral);
			}
			if (!ignoreTime)
			{
				this.lastVoiceTime = Time.time + 5f;
			}
		}
	}

	public void PlayAnimReaction(EntityTrader.AnimReaction reaction)
	{
		AvatarController avatarController = this.emodel.avatarController;
		if (avatarController)
		{
			avatarController.TriggerReaction((int)reaction);
		}
	}

	public void SendAnimReaction(int reaction)
	{
		List<AnimParamData> list = new List<AnimParamData>();
		list.Add(new AnimParamData(AvatarController.reactionTypeHash, AnimParamData.ValueTypes.Int, reaction));
		list.Add(new AnimParamData(AvatarController.reactionTriggerHash, AnimParamData.ValueTypes.Trigger, true));
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAnimationData>().Setup(this.entityId, list), false, -1, -1, this.entityId, null, 192);
	}

	public float eyeHeightHackMod = 1f;

	public bool ShowWornEquipment;

	public TileEntityTrader TileEntityTrader;

	public TraderArea traderArea;

	public Dictionary<EntityPlayer, float> GreetingDictionary = new Dictionary<EntityPlayer, float>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool firstTime = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool warningPlayed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float traderTalkDelayTime = 90f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool waitingToActivate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int outstandingIndexInBlockActivationCommands;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i outstandingTePos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive outstandingEntityFocusing;

	public List<QuestEntry> specialQuestList;

	public Dictionary<int, List<QuestEntry>> questDictionary = new Dictionary<int, List<QuestEntry>>();

	public List<Quest> activeQuests;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<int> tempTopTierQuests = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<int> tempSpecialQuests = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector2> usedPOILocations = new List<Vector2>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<string> uniqueKeysUsed = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastVoiceTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string lastSoundPlayed;

	public enum AnimReaction
	{
		Happy,
		Neutral,
		Angry
	}
}
