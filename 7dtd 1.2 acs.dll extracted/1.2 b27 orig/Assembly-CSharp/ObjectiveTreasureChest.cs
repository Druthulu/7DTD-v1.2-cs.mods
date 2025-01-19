using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveTreasureChest : BaseObjective
{
	public override void SetupQuestTag()
	{
		base.OwnerQuest.AddQuestTag(QuestEventManager.craftingTag);
	}

	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			if (base.ObjectiveState != BaseObjective.ObjectiveStates.Complete)
			{
				return BaseObjective.ObjectiveValueTypes.Distance;
			}
			return BaseObjective.ObjectiveValueTypes.Boolean;
		}
	}

	public override bool UpdateUI
	{
		get
		{
			return base.ObjectiveState != BaseObjective.ObjectiveStates.Failed;
		}
	}

	public override bool NeedsNPCSetPosition
	{
		get
		{
			return !this.useNearby;
		}
	}

	public override void SetupObjective()
	{
		this.keyword = ((this.containerType == ObjectiveTreasureChest.ContainerTypes.TreasureChest) ? Localization.Get("ObjectiveTreasureChest_keyword", false) : Localization.Get("ObjectiveLocateSupplies_keyword", false));
		this.expectedBlockValue = Block.GetBlockValue(this.ID, false);
		if (this.expectedBlockValue.isair)
		{
			Log.Error("ObjectiveTreasureChest: Invalid treasure container name.");
		}
		if (this.altBlockName != "")
		{
			this.altExpectedBlockValue = Block.GetBlockValue(this.altBlockName, false);
			return;
		}
		this.altExpectedBlockValue = BlockValue.Air;
	}

	public override void SetupDisplay()
	{
		base.Description = this.keyword;
		this.StatusText = "";
	}

	public override string StatusText
	{
		get
		{
			if (base.OwnerQuest.CurrentState == Quest.QuestState.InProgress)
			{
				if (base.CurrentValue == 1)
				{
					return "";
				}
				if (this.currentDistance > 10f)
				{
					return ValueDisplayFormatters.Distance(this.currentDistance);
				}
				return Localization.Get("ObjectiveNearby_keyword", false);
			}
			else
			{
				if (base.OwnerQuest.CurrentState == Quest.QuestState.NotStarted)
				{
					return "";
				}
				if (base.ObjectiveState == BaseObjective.ObjectiveStates.Failed)
				{
					return Localization.Get("failed", false);
				}
				return Localization.Get("completed", false);
			}
		}
	}

	public override void AddHooks()
	{
		QuestEventManager questEventManager = QuestEventManager.Current;
		questEventManager.AddObjectiveToBeUpdated(this);
		questEventManager.ContainerOpened += this.Current_ContainerOpened;
		questEventManager.BlockDestroy += this.Current_BlockDestroy;
		questEventManager.ExplosionDetected += this.Current_ExplosionDetected;
	}

	public override void RemoveHooks()
	{
		QuestEventManager questEventManager = QuestEventManager.Current;
		questEventManager.RemoveObjectiveToBeUpdated(this);
		questEventManager.ContainerOpened -= this.Current_ContainerOpened;
		questEventManager.BlockDestroy -= this.Current_BlockDestroy;
		questEventManager.ExplosionDetected -= this.Current_ExplosionDetected;
		if (this.projector != null)
		{
			UnityEngine.Object.Destroy(this.projector.gameObject);
			this.projector = null;
		}
	}

	public override bool SetLocation(Vector3 pos, Vector3 size)
	{
		this.FinalizePoint((int)pos.x, (int)pos.y, (int)pos.z);
		return true;
	}

	public override void SetPosition(Vector3 position, Vector3 size)
	{
		this.FinalizePoint((int)position.x, (int)position.y, (int)position.z);
	}

	public override bool SetupPosition(EntityNPC ownerNPC = null, EntityPlayer player = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		return !this.useNearby && this.GetPosition(ownerNPC);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GetPosition(EntityNPC ownerNPC = null)
	{
		if (base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.TreasurePoint) && base.OwnerQuest.GetPositionData(out this.offset, Quest.PositionDataTypes.TreasureOffset))
		{
			float num = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, (float)this.DefaultTreasureRadius, base.OwnerQuest.OwnerJournal.OwnerPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			num = Mathf.Clamp(num, 0f, (float)this.DefaultTreasureRadius);
			World world = GameManager.Instance.World;
			Vector3 vector = this.position + this.offset * num;
			this.CurrentBlocksPerReduction = (int)EffectManager.GetValue(PassiveEffects.TreasureBlocksPerReduction, null, (float)this.blocksPerReduction, base.OwnerQuest.OwnerJournal.OwnerPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			base.OwnerQuest.Position = vector;
			this.positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.TreasurePoint, this.NavObjectName, this.CurrentRadius);
			base.CurrentValue = 2;
			if (this.useNearby && base.OwnerQuest.RallyMarkerActivated)
			{
				base.OwnerQuest.CloseQuest(Quest.QuestState.Failed, null);
			}
			return true;
		}
		EntityAlive entityAlive = (ownerNPC == null) ? base.OwnerQuest.OwnerJournal.OwnerPlayer : ownerNPC;
		if (this.Value != null && this.Value != "" && !StringParsers.TryParseFloat(this.Value, out this.distance, 0, -1, NumberStyles.Any) && this.Value.Contains("-"))
		{
			string[] array = this.Value.Split('-', StringSplitOptions.None);
			float min = StringParsers.ParseFloat(array[0], 0, -1, NumberStyles.Any);
			float maxExclusive = StringParsers.ParseFloat(array[1], 0, -1, NumberStyles.Any);
			this.distance = GameManager.Instance.World.GetGameRandom().RandomRange(min, maxExclusive);
		}
		if (this.useNearby)
		{
			float num2 = Mathf.Clamp(EffectManager.GetValue(PassiveEffects.TreasureRadius, null, (float)this.DefaultTreasureRadius, base.OwnerQuest.OwnerJournal.OwnerPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false), 0f, (float)this.DefaultTreasureRadius) * 0.5f - 0.5f;
			this.distance = GameManager.Instance.World.GetGameRandom().RandomRange(num2 * -1f, num2);
		}
		Vector3i zero = Vector3i.zero;
		this.CurrentBlocksPerReduction = (int)EffectManager.GetValue(PassiveEffects.TreasureBlocksPerReduction, null, (float)this.blocksPerReduction, base.OwnerQuest.OwnerJournal.OwnerPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		if (base.OwnerQuest.Position == Vector3.zero)
		{
			base.OwnerQuest.Position = entityAlive.position;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (QuestEventManager.Current.GetTreasureContainerPosition(base.OwnerQuest.QuestCode, this.distance, this.DefaultTreasureRadius + 2, (float)this.DefaultTreasureRadius, base.OwnerQuest.Position, entityAlive.entityId, this.useNearby, this.CurrentBlocksPerReduction, out this.CurrentBlocksPerReduction, out zero, out this.offset))
			{
				base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasureOffset, this.offset);
				this.FinalizePoint(zero.x, zero.y, zero.z);
				return true;
			}
		}
		else
		{
			QuestEventManager.Current.GetTreasureContainerPosition(base.OwnerQuest.QuestCode, this.distance, this.DefaultTreasureRadius + 2, (float)this.DefaultTreasureRadius, base.OwnerQuest.Position, entityAlive.entityId, this.useNearby, this.CurrentBlocksPerReduction, out this.CurrentBlocksPerReduction, out zero, out this.offset);
			base.CurrentValue = 1;
		}
		return false;
	}

	public static bool CalculateTreasurePoint(Vector3 startPosition, float distance, int offset, float treasureRadius, bool useNearby, out Vector3i position, out Vector3 treasureOffset)
	{
		World world = GameManager.Instance.World;
		treasureOffset = Vector3.zero;
		Vector3 treasureOffset2 = ObjectiveTreasureChest.GetTreasureOffset(world);
		treasureOffset2.Normalize();
		Vector3 vector = startPosition + treasureOffset2 * (distance - 1f);
		position = new Vector3i(vector.x, (float)((int)GameManager.Instance.World.GetHeightAt(vector.x, vector.z) - 3), vector.z);
		Vector3 b = position.ToVector3();
		if (!world.IsPositionInBounds(b) || !world.IsEmptyPosition(position) || world.IsPositionWithinPOI(b, offset) || world.IsPositionRadiated(b))
		{
			return false;
		}
		if (!GameManager.Instance.World.CheckForLevelNearbyHeights(vector.x, vector.z, 5))
		{
			return false;
		}
		if (GameManager.Instance.World.GetWaterAt(vector.x, vector.z))
		{
			return false;
		}
		if (vector.y <= 10f)
		{
			return false;
		}
		if (useNearby)
		{
			treasureOffset = (startPosition - b) / treasureRadius;
			treasureOffset.Normalize();
		}
		else
		{
			treasureOffset = ObjectiveTreasureChest.GetTreasureOffset(world);
			treasureOffset *= 0.9f;
		}
		return true;
	}

	public static Vector3 GetTreasureOffset(World world)
	{
		Vector2 randomInsideUnitCircle = world.GetGameRandom().RandomInsideUnitCircle;
		return new Vector3(randomInsideUnitCircle.x, 0f, randomInsideUnitCircle.y);
	}

	public void FinalizePoint(int x, int y, int z)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && base.CurrentValue != 1)
		{
			if (!base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.TreasurePoint))
			{
				this.position = new Vector3((float)x, (float)y, (float)z);
				this.neededContainerLocation = new Vector3i(x, y, z);
				base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasurePoint, this.position);
				if (base.OwnerQuest.DataVariables.ContainsKey("treasurecontainer"))
				{
					base.OwnerQuest.DataVariables["treasurecontainer"] = string.Format("{0},{1},{2}", x, y, z);
				}
				else
				{
					base.OwnerQuest.DataVariables.Add("treasurecontainer", string.Format("{0},{1},{2}", x, y, z));
				}
				if (base.OwnerQuest.OwnerJournal != null && base.OwnerQuest.OwnerJournal.OwnerPlayer != null)
				{
					float num = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, (float)this.DefaultTreasureRadius, base.OwnerQuest.OwnerJournal.OwnerPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
					num = Mathf.Clamp(num, 0f, (float)this.DefaultTreasureRadius);
					World world = GameManager.Instance.World;
					if (!base.OwnerQuest.GetPositionData(out this.offset, Quest.PositionDataTypes.TreasureOffset))
					{
						this.offset = ObjectiveTreasureChest.GetTreasureOffset(world);
						base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasureOffset, this.offset);
					}
					Vector3 vector = this.position + this.offset * num;
					base.OwnerQuest.Position = vector;
					this.positionSet = true;
					base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.TreasurePoint, this.NavObjectName, this.CurrentRadius);
					base.CurrentValue = 2;
					return;
				}
				base.OwnerQuest.Position = this.position;
			}
			return;
		}
		this.position = new Vector3((float)x, (float)y, (float)z);
		this.neededContainerLocation = new Vector3i(x, y, z);
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasurePoint, this.position);
		if (base.OwnerQuest.DataVariables.ContainsKey("treasurecontainer"))
		{
			base.OwnerQuest.DataVariables["treasurecontainer"] = string.Format("{0},{1},{2}", x, y, z);
		}
		else
		{
			base.OwnerQuest.DataVariables.Add("treasurecontainer", string.Format("{0},{1},{2}", x, y, z));
		}
		if (base.OwnerQuest.OwnerJournal != null && base.OwnerQuest.OwnerJournal.OwnerPlayer != null)
		{
			float num2 = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, (float)this.DefaultTreasureRadius, base.OwnerQuest.OwnerJournal.OwnerPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			num2 = Mathf.Clamp(num2, 0f, (float)this.DefaultTreasureRadius);
			World world2 = GameManager.Instance.World;
			if (!base.OwnerQuest.GetPositionData(out this.offset, Quest.PositionDataTypes.TreasureOffset))
			{
				if (this.useNearby)
				{
					this.offset = (base.OwnerQuest.Position - this.position) / num2;
					this.offset.Normalize();
				}
				else
				{
					this.offset = ObjectiveTreasureChest.GetTreasureOffset(world2);
				}
				base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasureOffset, this.offset);
			}
			Vector3 vector2 = this.position + this.offset * (num2 - 1f);
			base.OwnerQuest.Position = vector2;
			this.positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.TreasurePoint, this.NavObjectName, this.CurrentRadius);
			base.CurrentValue = 2;
			return;
		}
		base.OwnerQuest.Position = this.position;
	}

	public void FinalizePointFromServer(int _blocksPerReduction, Vector3i _chestPos, Vector3 _treasureOffset)
	{
		this.CurrentBlocksPerReduction = _blocksPerReduction;
		this.position = _chestPos.ToVector3();
		this.neededContainerLocation = _chestPos;
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasurePoint, this.position);
		if (base.OwnerQuest.DataVariables.ContainsKey("treasurecontainer"))
		{
			base.OwnerQuest.DataVariables["treasurecontainer"] = string.Format("{0},{1},{2}", _chestPos.x, _chestPos.y, _chestPos.z);
		}
		else
		{
			base.OwnerQuest.DataVariables.Add("treasurecontainer", string.Format("{0},{1},{2}", _chestPos.x, _chestPos.y, _chestPos.z));
		}
		if (base.OwnerQuest.OwnerJournal != null && base.OwnerQuest.OwnerJournal.OwnerPlayer != null)
		{
			float num = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, (float)this.DefaultTreasureRadius, base.OwnerQuest.OwnerJournal.OwnerPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			num = Mathf.Clamp(num, 0f, (float)this.DefaultTreasureRadius);
			World world = GameManager.Instance.World;
			this.offset = _treasureOffset;
			base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasureOffset, this.offset);
			Vector3 vector = this.position + this.offset * num;
			base.OwnerQuest.Position = vector;
			this.positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.TreasurePoint, this.NavObjectName, this.CurrentRadius);
			base.CurrentValue = 2;
			return;
		}
		base.OwnerQuest.Position = this.position;
		this.offset = _treasureOffset;
	}

	public override void Update(float updateTime)
	{
		if (this.projector == null)
		{
			this.projector = this.CreateProjector().GetComponent<BoundaryProjectorTreasure>();
		}
		if (base.OwnerQuest.Active && base.OwnerQuest.MapObject == null && base.OwnerQuest.NavObject == null)
		{
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.TreasurePoint, this.NavObjectName, this.CurrentRadius);
		}
		switch (base.CurrentValue)
		{
		case 0:
			this.GetPosition(null);
			return;
		case 1:
			break;
		case 2:
		{
			EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
			this.position = new Vector3(this.position.x, ownerPlayer.position.y, this.position.z);
			this.lastDistance = this.currentDistance;
			this.currentDistance = Vector3.Distance(ownerPlayer.position, this.position + this.offset * (float)this.CurrentRadius);
			if (this.currentDistance <= 20f)
			{
				if (!this.positionAdjusted)
				{
					int num = (int)this.position.x;
					int num2 = (int)this.position.z;
					int num3 = (int)(GameManager.Instance.World.GetTerrainHeight(num, num2) - 3);
					Vector3i blockPos = new Vector3i(num, num3, num2);
					if (GameManager.Instance.World.GetChunkFromWorldPos(blockPos) != null)
					{
						this.positionAdjusted = true;
						this.position = new Vector3((float)num, (float)num3, (float)num2);
						this.neededContainerLocation = blockPos;
						base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasurePoint, this.position);
					}
				}
				if (base.OwnerQuest.SharedOwnerID == -1)
				{
					if (GameManager.Instance.World.GetChunkFromWorldPos(this.neededContainerLocation) is Chunk)
					{
						BlockValue block = GameManager.Instance.World.GetBlock(this.neededContainerLocation);
						if (block.type != this.expectedBlockValue.type && (this.altExpectedBlockValue.isair || this.altExpectedBlockValue.type != block.type))
						{
							GameManager.Instance.World.SetBlockRPC(this.neededContainerLocation, this.expectedBlockValue, sbyte.MaxValue);
						}
						if (this.useNearby)
						{
							if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
							{
								QuestEventManager.Current.SetTreasureContainerPosition(base.OwnerQuest.QuestCode, this.neededContainerLocation);
							}
							else
							{
								SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestTreasurePoint>().Setup(base.OwnerQuest.QuestCode, this.neededContainerLocation), false);
							}
						}
						base.CurrentValue = 4;
						base.OwnerQuest.RallyMarkerActivated = true;
						return;
					}
				}
				else if (GameManager.Instance.World.GetBlock(this.neededContainerLocation).type == this.expectedBlockValue.type)
				{
					base.CurrentValue = 4;
					base.OwnerQuest.RallyMarkerActivated = true;
					return;
				}
			}
			break;
		}
		case 3:
		{
			if (GameManager.Instance.World.GetBlock(this.neededContainerLocation).type != this.expectedBlockValue.type)
			{
				base.CurrentValue = 2;
				return;
			}
			TileEntityLootContainer tileEntityLootContainer = (TileEntityLootContainer)GameManager.Instance.World.GetTileEntity(0, this.neededContainerLocation);
			tileEntityLootContainer.bPlayerBackpack = true;
			tileEntityLootContainer.SetModified();
			base.CurrentValue = 4;
			return;
		}
		case 4:
			if (this.positionSet)
			{
				EntityPlayer ownerPlayer2 = base.OwnerQuest.OwnerJournal.OwnerPlayer;
				this.lastDistance = this.currentDistance;
				this.currentDistance = Vector2.Distance(new Vector2(ownerPlayer2.position.x, ownerPlayer2.position.z), new Vector2(this.position.x + this.offset.x * (float)this.CurrentRadius, this.position.z + this.offset.z * (float)this.CurrentRadius));
				if (this.currentDistance < 30f)
				{
					this.HandleNavObjects(ownerPlayer2);
					if (!this.projector.IsInitialized)
					{
						this.RadiusBoundsChanged();
						this.projector.IsInitialized = true;
						this.projector.gameObject.SetActive(true);
					}
					else if (this.lastDistance >= 30f)
					{
						this.RadiusBoundsChanged();
						this.projector.gameObject.SetActive(true);
					}
					if (this.originPos != Origin.position)
					{
						this.ResetProjectorPosition((float)this.CurrentRadius);
					}
					this.projector.WithinRadius = (this.currentDistance <= this.projector.CurrentRadius);
					this.projector.transform.position = new Vector3(this.projector.transform.position.x, ownerPlayer2.transform.position.y, this.projector.transform.position.z);
					Chunk chunk = GameManager.Instance.World.GetChunkFromWorldPos(this.neededContainerLocation) as Chunk;
					if (chunk != null && chunk.IsDisplayed)
					{
						BlockValue block2 = GameManager.Instance.World.GetBlock(this.neededContainerLocation);
						if (block2.type != this.expectedBlockValue.type && (this.altExpectedBlockValue.isair || this.altExpectedBlockValue.type != block2.type))
						{
							base.OwnerQuest.CloseQuest(Quest.QuestState.Failed, null);
							return;
						}
					}
				}
				else if (this.currentDistance > 30f)
				{
					this.projector.gameObject.SetActive(false);
					return;
				}
			}
			break;
		case 5:
			QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
			if (this.directNavObject != null)
			{
				NavObjectManager.Instance.UnRegisterNavObject(this.directNavObject);
				this.directNavObject = null;
			}
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleNavObjects(EntityPlayer player)
	{
		float num = (float)this.CurrentRadius;
		num = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, num, player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		num = Mathf.Clamp(num, 0f, num);
		if (this.directNavObject == null)
		{
			this.directNavObject = NavObjectManager.Instance.RegisterNavObject(this.DirectNavObjectName, this.neededContainerLocation.ToVector3() + new Vector3(0.5f, 0f, 0.5f), "", false, null);
			if (base.OwnerQuest != null)
			{
				QuestClass questClass = base.OwnerQuest.QuestClass;
				this.directNavObject.name = questClass.Name;
			}
		}
		else
		{
			if (this.directNavObject.DisplayName == "" && base.OwnerQuest != null)
			{
				QuestClass questClass2 = base.OwnerQuest.QuestClass;
				this.directNavObject.name = questClass2.Name;
			}
			this.directNavObject.ExtraData = num;
		}
		if (num == 0f)
		{
			this.directNavObject.ForceDisabled = false;
			base.OwnerQuest.NavObject.ForceDisabled = true;
			return;
		}
		this.directNavObject.ForceDisabled = true;
		base.OwnerQuest.NavObject.ForceDisabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RadiusBoundsChanged()
	{
		if (this.projector == null)
		{
			return;
		}
		EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		float num = (float)this.CurrentRadius;
		num = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, num, ownerPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		num = Mathf.Clamp(num, 0f, num);
		this.projector.SetRadius(0, num);
		this.ResetProjectorPosition(num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetProjectorPosition(float radius)
	{
		this.projector.transform.position = this.position - Origin.position + this.offset * radius + Vector3.up * 4f;
		this.originPos = Origin.position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ContainerOpened(int entityId, Vector3i containerLocation, ITileEntityLootable lootTE)
	{
		if (base.Complete)
		{
			return;
		}
		if (entityId != -1)
		{
			return;
		}
		if (this.neededContainerLocation.x == containerLocation.x && this.neededContainerLocation.y == containerLocation.y && this.neededContainerLocation.z == containerLocation.z && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue = 5;
			if (this.projector != null)
			{
				UnityEngine.Object.Destroy(this.projector.gameObject);
				this.projector = null;
			}
			NavObjectManager.Instance.UnRegisterNavObject(this.directNavObject);
			this.Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockDestroy(Block block, Vector3i blockPos)
	{
		if (this.blocksPerReduction <= 0)
		{
			return;
		}
		if (base.Complete)
		{
			return;
		}
		if (!(block.shape is BlockShapeTerrain))
		{
			return;
		}
		Vector3 vector = this.position + this.offset * (float)this.CurrentRadius;
		if (Vector3.Distance(new Vector3((float)blockPos.x, 0f, (float)blockPos.z), new Vector3(vector.x, 0f, vector.z)) < (float)this.CurrentRadius + 0.5f && this.AddToDestroyCount())
		{
			this.HandleParty();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ExplosionDetected(Vector3 pos, int entityID, float blockDamage)
	{
		if (this.explosionEvent == "" || base.OwnerQuest.SharedOwnerID != -1)
		{
			return;
		}
		if (this.lastExplosionTime > Time.time)
		{
			return;
		}
		if (Vector3.Distance(this.position, pos) <= (float)(this.DefaultTreasureRadius * 2) && blockDamage > 1f && GameManager.Instance.World.GetEntity(entityID) is EntityPlayer)
		{
			GameEventManager.Current.HandleAction(this.explosionEvent, null, base.OwnerQuest.OwnerJournal.OwnerPlayer, false, this.neededContainerLocation, "", "", false, true, "", null);
			this.lastExplosionTime = Time.time + this.explosionEventDelay;
		}
	}

	public bool AddToDestroyCount()
	{
		if (this.CurrentRadius != 0)
		{
			this.destroyCount++;
			int num = this.CurrentBlocksPerReduction;
			int num2 = (int)EffectManager.GetValue(PassiveEffects.TreasureBlocksPerReduction, null, (float)this.blocksPerReduction, base.OwnerQuest.OwnerJournal.OwnerPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			if (num2 != num)
			{
				num = (this.CurrentBlocksPerReduction = num2);
				QuestEventManager.Current.UpdateTreasureBlocksPerReduction(base.OwnerQuest.QuestCode, num);
			}
			if (this.destroyCount >= num)
			{
				this.CurrentRadius--;
				this.destroyCount -= num;
			}
			if (this.CurrentRadius < 0)
			{
				this.CurrentRadius = 0;
			}
			if (this.CurrentRadius != this.lastRadius)
			{
				World world = GameManager.Instance.World;
				base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.TreasureOffset, this.offset);
				base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.TreasurePoint, this.NavObjectName, this.CurrentRadius);
				this.RadiusBoundsChanged();
				base.OwnerQuest.HandleQuestEvent(base.OwnerQuest, "TreasureRadiusReduction");
				GameManager.ShowTooltip(base.OwnerQuest.OwnerJournal.OwnerPlayer, Localization.Get("ttTreasureRadiusReduced", false), false);
				if (this.radiusReductionSound != "" && Vector3.Distance(base.OwnerQuest.OwnerJournal.OwnerPlayer.position, this.position + this.offset * (float)this.CurrentRadius) <= (float)(this.DefaultTreasureRadius * 2))
				{
					Manager.PlayInsidePlayerHead(this.radiusReductionSound, -1, 0f, false, false);
				}
				this.lastRadius = this.CurrentRadius;
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleParty()
	{
		EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		if (ownerPlayer.Party == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestObjectiveUpdate>().Setup(NetPackageQuestObjectiveUpdate.QuestObjectiveEventTypes.TreasureRadiusBreak, ownerPlayer.entityId, base.OwnerQuest.QuestCode), false, -1, -1, -1, null, 192);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestObjectiveUpdate>().Setup(NetPackageQuestObjectiveUpdate.QuestObjectiveEventTypes.TreasureRadiusBreak, ownerPlayer.entityId, base.OwnerQuest.QuestCode), false);
	}

	public override void Refresh()
	{
		bool complete = base.CurrentValue == 5;
		base.Complete = complete;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveTreasureChest objectiveTreasureChest = (ObjectiveTreasureChest)objective;
		objectiveTreasureChest.containerType = this.containerType;
		objectiveTreasureChest.altBlockName = this.altBlockName;
		objectiveTreasureChest.altExpectedBlockValue = this.altExpectedBlockValue;
		objectiveTreasureChest.distance = this.distance;
		objectiveTreasureChest.DefaultTreasureRadius = this.DefaultTreasureRadius;
		objectiveTreasureChest.CurrentRadius = this.CurrentRadius;
		objectiveTreasureChest.DirectNavObjectName = this.DirectNavObjectName;
		objectiveTreasureChest.destroyCount = this.destroyCount;
		objectiveTreasureChest.lastRadius = this.lastRadius;
		objectiveTreasureChest.blocksPerReduction = this.blocksPerReduction;
		objectiveTreasureChest.radiusReductionSound = this.radiusReductionSound;
		objectiveTreasureChest.originPos = this.originPos;
		objectiveTreasureChest.useNearby = this.useNearby;
		objectiveTreasureChest.explosionEventDelay = this.explosionEventDelay;
		objectiveTreasureChest.explosionEvent = this.explosionEvent;
	}

	public override BaseObjective Clone()
	{
		ObjectiveTreasureChest objectiveTreasureChest = new ObjectiveTreasureChest();
		this.CopyValues(objectiveTreasureChest);
		return objectiveTreasureChest;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(ObjectiveTreasureChest.PropBlock))
		{
			this.ID = properties.Values[ObjectiveTreasureChest.PropBlock];
		}
		if (properties.Values.ContainsKey(ObjectiveTreasureChest.PropAltBlock))
		{
			this.altBlockName = properties.Values[ObjectiveTreasureChest.PropAltBlock];
		}
		if (properties.Values.ContainsKey(ObjectiveTreasureChest.PropDistance))
		{
			this.Value = properties.Values[ObjectiveTreasureChest.PropDistance];
		}
		if (properties.Values.ContainsKey(ObjectiveTreasureChest.PropContainerType))
		{
			this.containerType = EnumUtils.Parse<ObjectiveTreasureChest.ContainerTypes>(properties.Values[ObjectiveTreasureChest.PropContainerType], false);
		}
		if (properties.Values.ContainsKey(ObjectiveTreasureChest.PropDefaultRadius))
		{
			this.DefaultTreasureRadius = StringParsers.ParseSInt32(properties.Values[ObjectiveTreasureChest.PropDefaultRadius], 0, -1, NumberStyles.Integer);
		}
		this.CurrentRadius = this.DefaultTreasureRadius;
		this.lastRadius = this.CurrentRadius;
		if (properties.Values.ContainsKey(ObjectiveTreasureChest.PropDirectNavObject))
		{
			this.DirectNavObjectName = properties.Values[ObjectiveTreasureChest.PropDirectNavObject];
		}
		if (properties.Values.ContainsKey(ObjectiveTreasureChest.PropBlocksPerReduction))
		{
			this.blocksPerReduction = StringParsers.ParseSInt32(properties.Values[ObjectiveTreasureChest.PropBlocksPerReduction], 0, -1, NumberStyles.Integer);
		}
		if (properties.Values.ContainsKey(ObjectiveTreasureChest.PropRadiusReductionSound))
		{
			this.radiusReductionSound = properties.Values[ObjectiveTreasureChest.PropRadiusReductionSound];
		}
		if (properties.Values.ContainsKey(ObjectiveTreasureChest.PropUseNearby))
		{
			this.useNearby = StringParsers.ParseBool(properties.Values[ObjectiveTreasureChest.PropUseNearby], 0, -1, true);
		}
		properties.ParseFloat(ObjectiveTreasureChest.PropExplosionEventDelay, ref this.explosionEventDelay);
		properties.ParseString(ObjectiveTreasureChest.PropExplosionEvent, ref this.explosionEvent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject CreateProjector()
	{
		if (ObjectiveTreasureChest.prefabProjector == null)
		{
			ObjectiveTreasureChest.prefabProjector = Resources.Load<GameObject>("Prefabs/prefabBoundaryProjectorTreasure");
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(ObjectiveTreasureChest.prefabProjector);
		gameObject.name = "Projector";
		gameObject.transform.position = new Vector3(-999f, -999f, -999f);
		return gameObject;
	}

	public override void HandleCompleted()
	{
		base.HandleCompleted();
		if (this.projector != null)
		{
			UnityEngine.Object.Destroy(this.projector.gameObject);
			this.projector = null;
		}
		if (this.directNavObject != null)
		{
			NavObjectManager.Instance.UnRegisterNavObject(this.directNavObject);
			this.directNavObject = null;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			QuestEventManager.Current.FinishTreasureQuest(base.OwnerQuest.QuestCode, base.OwnerQuest.OwnerJournal.OwnerPlayer);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestObjectiveUpdate>().Setup(NetPackageQuestObjectiveUpdate.QuestObjectiveEventTypes.TreasureComplete, base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, base.OwnerQuest.QuestCode), false);
	}

	public override void HandleFailed()
	{
		base.HandleFailed();
		if (this.projector != null)
		{
			UnityEngine.Object.Destroy(this.projector.gameObject);
			this.projector = null;
		}
		if (this.directNavObject != null)
		{
			NavObjectManager.Instance.UnRegisterNavObject(this.directNavObject);
			this.directNavObject = null;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			QuestEventManager.Current.FinishTreasureQuest(base.OwnerQuest.QuestCode, base.OwnerQuest.OwnerJournal.OwnerPlayer);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestObjectiveUpdate>().Setup(NetPackageQuestObjectiveUpdate.QuestObjectiveEventTypes.TreasureComplete, base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, base.OwnerQuest.QuestCode), false);
	}

	public override void Read(BinaryReader _br)
	{
		this.destroyCount = _br.ReadInt32();
		this.CurrentRadius = _br.ReadInt32();
		this.lastRadius = this.CurrentRadius;
	}

	public override void Write(BinaryWriter _bw)
	{
		_bw.Write(this.destroyCount);
		_bw.Write(this.CurrentRadius);
	}

	public override string ParseBinding(string bindingName)
	{
		string id = this.ID;
		string value = this.Value;
		if (!(bindingName == "distance"))
		{
			if (bindingName == "direction")
			{
				if (base.OwnerQuest.QuestGiverID != -1)
				{
					EntityNPC entityNPC = GameManager.Instance.World.GetEntity(base.OwnerQuest.QuestGiverID) as EntityNPC;
					if (entityNPC != null)
					{
						if (this.useNearby)
						{
							Vector3 vector = base.OwnerQuest.Position;
							vector.y = 0f;
							Vector3 vector2 = entityNPC.position;
							vector2.y = 0f;
							return ValueDisplayFormatters.Direction(GameUtils.GetDirByNormal(new Vector2(vector.x - vector2.x, vector.z - vector2.z)), false);
						}
						this.position.y = 0f;
						Vector3 vector3 = entityNPC.position;
						vector3.y = 0f;
						return ValueDisplayFormatters.Direction(GameUtils.GetDirByNormal(new Vector2(this.position.x - vector3.x, this.position.z - vector3.z)), false);
					}
				}
			}
		}
		else if (base.OwnerQuest.QuestGiverID != -1)
		{
			EntityNPC entityNPC2 = GameManager.Instance.World.GetEntity(base.OwnerQuest.QuestGiverID) as EntityNPC;
			if (entityNPC2 != null)
			{
				if (this.useNearby)
				{
					Vector3 a = base.OwnerQuest.Position;
					a.y = 0f;
					a.y = 0f;
					Vector3 a2 = entityNPC2.position;
					a2.y = 0f;
					this.currentDistance = Vector3.Distance(a2, a + this.offset);
					return ValueDisplayFormatters.Distance(this.currentDistance);
				}
				this.position.y = 0f;
				Vector3 a3 = entityNPC2.position;
				a3.y = 0f;
				this.currentDistance = Vector3.Distance(a3, this.position + this.offset);
				return ValueDisplayFormatters.Distance(this.currentDistance);
			}
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ObjectiveTreasureChest.ContainerTypes containerType;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool positionSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue expectedBlockValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue altExpectedBlockValue;

	public static int TreasureRadiusInitial = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastDistance = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float distance = 50f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float currentDistance;

	public int DefaultTreasureRadius = 9;

	public int CurrentRadius = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public string DirectNavObjectName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string altBlockName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string radiusReductionSound = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int blocksPerReduction = 1;

	public int CurrentBlocksPerReduction = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int destroyCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastExplosionTime = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float explosionEventDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public string explosionEvent = "";

	public static string PropBlock = "block";

	public static string PropAltBlock = "alt_block";

	public static string PropDistance = "distance";

	public static string PropContainerType = "container_type";

	public static string PropDefaultRadius = "default_radius";

	public static string PropDirectNavObject = "direct_nav_object";

	public static string PropBlocksPerReduction = "blocks_per_reduction";

	public static string PropRadiusReductionSound = "radius_reduction_sound";

	public static string PropUseNearby = "use_nearby";

	public static string PropExplosionEventDelay = "explosion_event_delay";

	public static string PropExplosionEvent = "explosion_event";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useNearby;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i neededContainerLocation = new Vector3i(-5000, -5000, -5000);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 originPos = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool positionAdjusted;

	[PublicizedFrom(EAccessModifier.Private)]
	public NavObject directNavObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public BoundaryProjectorTreasure projector;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 offset;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static GameObject prefabProjector;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum ContainerTypes
	{
		TreasureChest,
		Supplies
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum TreasureChestStates
	{
		NoPosition,
		WaitingForPoint,
		TryCreate,
		ValidateCreation,
		Created,
		Completed
	}
}
