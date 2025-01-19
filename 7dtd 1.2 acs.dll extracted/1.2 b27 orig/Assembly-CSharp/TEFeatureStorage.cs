using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureStorage : TEFeatureAbs, ITileEntityLootable, ITileEntity, IInventory
{
	public float LootStageMod { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public float LootStageBonus { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		this.lockFeature = base.Parent.GetFeature<ILockable>();
		this.lockpickFeature = base.Parent.GetFeature<ILockPickable>();
		DynamicProperties props = _featureData.Props;
		if (!props.Values.ContainsKey(BlockLoot.PropLootList))
		{
			Log.Error("Block with name " + base.Parent.TeData.Block.GetBlockName() + " does not have a loot list");
		}
		else
		{
			this.lootListName = props.Values[BlockLoot.PropLootList];
		}
		float lootStageMod = 0f;
		float lootStageBonus = 0f;
		props.ParseFloat(BlockLoot.PropLootStageMod, ref lootStageMod);
		props.ParseFloat(BlockLoot.PropLootStageBonus, ref lootStageBonus);
		this.LootStageMod = lootStageMod;
		this.LootStageBonus = lootStageBonus;
		for (int i = 1; i < 99; i++)
		{
			string text = BlockLoot.PropAlternateLootList + i.ToString();
			if (!props.Values.ContainsKey(text))
			{
				break;
			}
			string text2 = "";
			if (props.Params1.ContainsKey(text))
			{
				text2 = props.Params1[text];
			}
			if (!string.IsNullOrEmpty(text2))
			{
				FastTags<TagGroup.Global> tag = FastTags<TagGroup.Global>.Parse(text2);
				if (this.AlternateLootList == null)
				{
					this.AlternateLootList = new List<BlockLoot.AlternateLootEntry>();
				}
				this.AlternateLootList.Add(new BlockLoot.AlternateLootEntry
				{
					tag = tag,
					lootEntry = props.Values[text]
				});
			}
		}
		this.SetContainerSize(LootContainer.GetLootContainer(this.lootListName, true).size, true);
	}

	public override void CopyFrom(TileEntityComposite _other)
	{
		base.CopyFrom(_other);
		ITileEntityLootable tileEntityLootable;
		if (_other.TryGetSelfOrFeature(out tileEntityLootable))
		{
			this.lootListName = tileEntityLootable.lootListName;
			this.containerSize = tileEntityLootable.GetContainerSize();
			this.items = ItemStack.Clone(tileEntityLootable.items);
			this.bPlayerBackpack = tileEntityLootable.bPlayerBackpack;
			this.worldTimeTouched = tileEntityLootable.worldTimeTouched;
			this.bTouched = tileEntityLootable.bTouched;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (!GameManager.IsDedicatedServer)
		{
			XUiC_LootWindowGroup.CloseIfOpenAtPos(base.ToWorldPos(), null);
		}
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _placingEntity)
	{
		base.PlaceBlock(_world, _result, _placingEntity);
		if (_placingEntity != null && _placingEntity.entityType == EntityType.Player)
		{
			this.worldTimeTouched = _world.GetWorldTime();
			this.SetEmpty();
		}
	}

	public override void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
		base.UpgradeDowngradeFrom(_other);
		ITileEntityLootable feature = _other.GetFeature<ITileEntityLootable>();
		if (feature != null)
		{
			this.bTouched = feature.bTouched;
			this.worldTimeTouched = feature.worldTimeTouched;
			this.bPlayerBackpack = feature.bPlayerBackpack;
			this.migrateItemsFromOtherContainer(feature);
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		ITileEntityLootable tileEntityLootable;
		if (!_teNew.TryGetSelfOrFeature(out tileEntityLootable))
		{
			GameManager.Instance.DropContentOfLootContainerServer(_bvOld, base.ToWorldPos(), this.EntityId, this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void migrateItemsFromOtherContainer(ITileEntityLootable _other)
	{
		this.items = ItemStack.Clone(_other.items, 0, this.containerSize.x * this.containerSize.y);
		if (this.items.Length < _other.items.Length)
		{
			Log.Error(string.Format("{0}.UpgradeDowngradeFrom to smaller container, discarding overflow: other.size={1}, other.length={2}, this.size={3}, this.length={4}", new object[]
			{
				"TEFeatureStorage",
				_other.GetContainerSize(),
				_other.items.Length,
				this.containerSize,
				this.items.Length
			}));
		}
	}

	public override void Reset(FastTags<TagGroup.Global> _questTags)
	{
		base.Reset(_questTags);
		if (this.bPlayerStorage || this.bPlayerBackpack)
		{
			return;
		}
		this.bTouched = false;
		this.bWasTouched = false;
		for (int i = 0; i < this.items.Length; i++)
		{
			this.items[i].Clear();
		}
		if (this.AlternateLootList != null)
		{
			for (int j = 0; j < this.AlternateLootList.Count; j++)
			{
				if (_questTags.Test_AnySet(this.AlternateLootList[j].tag))
				{
					this.lootListName = this.AlternateLootList[j].lootEntry;
					break;
				}
			}
		}
		base.SetModified();
	}

	public override void UpdateTick(World _world)
	{
		base.UpdateTick(_world);
		if (base.Parent.PlayerPlaced)
		{
			return;
		}
		if (!this.bTouched || !this.IsEmpty() || GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays) <= 0)
		{
			return;
		}
		int num = GameUtils.WorldTimeToTotalHours(this.worldTimeTouched);
		if ((GameUtils.WorldTimeToTotalHours(_world.worldTime) - num) / 24 >= GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays))
		{
			this.bWasTouched = false;
			this.bTouched = false;
			base.SetModified();
			return;
		}
		if (this.entityTempList == null)
		{
			this.entityTempList = new List<Entity>();
		}
		else
		{
			this.entityTempList.Clear();
		}
		_world.GetEntitiesInBounds(typeof(EntityPlayer), new Bounds(base.ToWorldPos().ToVector3(), Vector3.one * 16f), this.entityTempList);
		if (this.entityTempList.Count > 0)
		{
			this.worldTimeTouched = _world.worldTime;
			base.SetModified();
			return;
		}
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		if (this.lockFeature == null)
		{
			if (!this.bTouched)
			{
				return string.Format(Localization.Get("lootTooltipNew", false), _activateHotkeyMarkup, _focusedTileEntityName);
			}
			if (this.IsEmpty())
			{
				return string.Format(Localization.Get("lootTooltipEmpty", false), _activateHotkeyMarkup, _focusedTileEntityName);
			}
			return string.Format(Localization.Get("lootTooltipTouched", false), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		else
		{
			if (!this.lockFeature.IsLocked())
			{
				return string.Format(Localization.Get("tooltipUnlocked", false), _activateHotkeyMarkup, _focusedTileEntityName);
			}
			if (this.lockFeature.LocalPlayerIsOwner() || this.lockpickFeature != null)
			{
				return string.Format(Localization.Get("tooltipLocked", false), _activateHotkeyMarkup, _focusedTileEntityName);
			}
			if (this.lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
			{
				return string.Format(Localization.Get("tooltipLocked", false), _activateHotkeyMarkup, _focusedTileEntityName);
			}
			return string.Format(Localization.Get("tooltipJammed", false), _activateHotkeyMarkup, _focusedTileEntityName);
		}
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("Search", "search", true, false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (!base.CommandIs(_commandName, "Search"))
		{
			return false;
		}
		if (this.lockFeature != null && this.lockFeature.IsLocked() && !this.lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = base.Parent.ToWorldPos();
		this.bWasTouched = this.bTouched;
		_world.GetGameManager().TELockServer(0, blockPos, base.Parent.EntityId, _player.entityId, "container");
		return true;
	}

	public string lootListName { get; set; }

	public bool bPlayerBackpack { get; set; }

	public bool bPlayerStorage
	{
		get
		{
			return base.Parent.Owner != null;
		}
		set
		{
		}
	}

	public PreferenceTracker preferences { get; set; }

	public ulong worldTimeTouched { get; set; }

	public bool bTouched
	{
		get
		{
			return this.bPlayerStorage || this.internalTouched;
		}
		set
		{
			this.internalTouched = value;
		}
	}

	public bool bWasTouched { get; set; }

	public ItemStack[] items
	{
		get
		{
			ItemStack[] result;
			if ((result = this.itemsArr) == null)
			{
				result = (this.itemsArr = ItemStack.CreateArray(this.containerSize.x * this.containerSize.y));
			}
			return result;
		}
		set
		{
			this.itemsArr = value;
		}
	}

	public virtual Vector2i GetContainerSize()
	{
		return this.containerSize;
	}

	public virtual void SetContainerSize(Vector2i _containerSize, bool _clearItems = true)
	{
		this.containerSize = _containerSize;
		if (!_clearItems)
		{
			return;
		}
		if (this.containerSize.x * this.containerSize.y != this.items.Length)
		{
			this.items = ItemStack.CreateArray(this.containerSize.x * this.containerSize.y);
			return;
		}
		for (int i = 0; i < this.items.Length; i++)
		{
			this.items[i] = ItemStack.Empty.Clone();
		}
	}

	public void UpdateSlot(int _idx, ItemStack _item)
	{
		this.items[_idx] = _item.Clone();
		base.Parent.NotifyListeners();
	}

	public void RemoveItem(ItemValue _item)
	{
		for (int i = 0; i < this.items.Length; i++)
		{
			if (this.items[i].itemValue.ItemClass == _item.ItemClass)
			{
				this.UpdateSlot(i, ItemStack.Empty.Clone());
			}
		}
	}

	public bool IsEmpty()
	{
		for (int i = 0; i < this.items.Length; i++)
		{
			if (!this.items[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public void SetEmpty()
	{
		for (int i = 0; i < this.items.Length; i++)
		{
			this.items[i].Clear();
		}
		base.Parent.NotifyListeners();
		this.bTouched = true;
		base.SetModified();
	}

	public bool AddItem(ItemStack _itemStack)
	{
		for (int i = 0; i < this.items.Length; i++)
		{
			if (this.items[i].IsEmpty())
			{
				this.UpdateSlot(i, _itemStack);
				base.SetModified();
				return true;
			}
		}
		return false;
	}

	[return: TupleElementNames(new string[]
	{
		"anyMoved",
		"allMoved"
	})]
	public ValueTuple<bool, bool> TryStackItem(int _startIndex, ItemStack _itemStack)
	{
		bool item = false;
		int count = _itemStack.count;
		for (int i = _startIndex; i < this.items.Length; i++)
		{
			int count2 = _itemStack.count;
			if (_itemStack.itemValue.type == this.items[i].itemValue.type && this.items[i].CanStackPartly(ref count2))
			{
				this.items[i].count += count2;
				_itemStack.count -= count2;
				if (_itemStack.count == 0)
				{
					break;
				}
			}
		}
		if (_itemStack.count != count)
		{
			item = true;
			base.SetModified();
			base.Parent.NotifyListeners();
		}
		return new ValueTuple<bool, bool>(item, _itemStack.count == 0);
	}

	public bool HasItem(ItemValue _item)
	{
		for (int i = 0; i < this.items.Length; i++)
		{
			if (this.items[i].itemValue.ItemClass == _item.ItemClass)
			{
				return true;
			}
		}
		return false;
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, int _readVersion)
	{
		base.Read(_br, _eStreamMode, _readVersion);
		if (_br.ReadBoolean())
		{
			this.lootListName = _br.ReadString();
		}
		this.containerSize = new Vector2i
		{
			x = (int)_br.ReadUInt16(),
			y = (int)_br.ReadUInt16()
		};
		this.bTouched = _br.ReadBoolean();
		this.worldTimeTouched = (ulong)_br.ReadUInt32();
		_br.ReadBoolean();
		int num = Math.Min((int)_br.ReadInt16(), this.containerSize.x * this.containerSize.y);
		if (base.Parent.IsUserAccessing())
		{
			ItemStack itemStack = ItemStack.Empty.Clone();
			for (int i = 0; i < num; i++)
			{
				itemStack.Read(_br);
			}
		}
		else
		{
			if (this.containerSize.x * this.containerSize.y != this.items.Length)
			{
				this.items = ItemStack.CreateArray(this.containerSize.x * this.containerSize.y);
			}
			for (int j = 0; j < num; j++)
			{
				this.items[j].Clear();
				this.items[j].Read(_br);
			}
		}
		if (_br.ReadBoolean())
		{
			this.preferences = new PreferenceTracker(-1);
			this.preferences.Read(_br);
		}
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		bool flag = !string.IsNullOrEmpty(this.lootListName);
		_bw.Write(flag);
		if (flag)
		{
			_bw.Write(this.lootListName);
		}
		_bw.Write((ushort)this.containerSize.x);
		_bw.Write((ushort)this.containerSize.y);
		_bw.Write(this.bTouched);
		_bw.Write((uint)this.worldTimeTouched);
		_bw.Write(false);
		_bw.Write((short)this.items.Length);
		ItemStack[] items = this.items;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Clone().Write(_bw);
		}
		bool flag2 = this.preferences != null;
		_bw.Write(flag2);
		if (flag2)
		{
			this.preferences.Write(_bw);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ILockable lockFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public ILockPickable lockpickFeature;

	public List<BlockLoot.AlternateLootEntry> AlternateLootList;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> entityTempList;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSize = Vector2i.one;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] itemsArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool internalTouched;
}
