using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TileEntityLootContainer : TileEntity, IInventory, ITileEntityLootable, ITileEntity
{
	public string lootListName { get; set; }

	public virtual float LootStageMod
	{
		get
		{
			return (base.blockValue.Block as BlockLoot).LootStageMod;
		}
	}

	public virtual float LootStageBonus
	{
		get
		{
			return (base.blockValue.Block as BlockLoot).LootStageBonus;
		}
	}

	public ItemStack[] items
	{
		get
		{
			if (this.itemsArr == null)
			{
				this.itemsArr = ItemStack.CreateArray(this.containerSize.x * this.containerSize.y);
			}
			return this.itemsArr;
		}
		set
		{
			this.itemsArr = value;
		}
	}

	public bool bPlayerBackpack { get; set; }

	public bool bPlayerStorage { get; set; }

	public PreferenceTracker preferences { get; set; }

	public bool bTouched { get; set; }

	public ulong worldTimeTouched { get; set; }

	public bool bWasTouched { get; set; }

	public TileEntityLootContainer(Chunk _chunk) : base(_chunk)
	{
		this.containerSize = new Vector2i(3, 3);
		this.lootListName = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityLootContainer(TileEntityLootContainer _other) : base(null)
	{
		this.lootListName = _other.lootListName;
		this.containerSize = _other.containerSize;
		this.items = ItemStack.Clone(_other.items);
		this.bTouched = _other.bTouched;
		this.worldTimeTouched = _other.worldTimeTouched;
		this.bPlayerBackpack = _other.bPlayerBackpack;
		this.bPlayerStorage = _other.bPlayerStorage;
		this.bUserAccessing = _other.bUserAccessing;
	}

	public override TileEntity Clone()
	{
		return new TileEntityLootContainer(this);
	}

	public void CopyLootContainerDataFromOther(TileEntityLootContainer _other)
	{
		this.lootListName = _other.lootListName;
		this.containerSize = _other.containerSize;
		this.items = ItemStack.Clone(_other.items);
		this.bTouched = _other.bTouched;
		this.worldTimeTouched = _other.worldTimeTouched;
		this.bPlayerBackpack = _other.bPlayerBackpack;
		this.bPlayerStorage = _other.bPlayerStorage;
		this.bUserAccessing = _other.bUserAccessing;
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays) > 0 && !this.bPlayerStorage && this.bTouched && this.IsEmpty())
		{
			int num = GameUtils.WorldTimeToHours(this.worldTimeTouched);
			num += GameUtils.WorldTimeToDays(this.worldTimeTouched) * 24;
			if ((GameUtils.WorldTimeToHours(world.worldTime) + GameUtils.WorldTimeToDays(world.worldTime) * 24 - num) / 24 < GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays))
			{
				if (this.entList == null)
				{
					this.entList = new List<Entity>();
				}
				else
				{
					this.entList.Clear();
				}
				world.GetEntitiesInBounds(typeof(EntityPlayer), new Bounds(base.ToWorldPos().ToVector3(), Vector3.one * 16f), this.entList);
				if (this.entList.Count > 0)
				{
					this.worldTimeTouched = world.worldTime;
					this.setModified();
					return;
				}
				return;
			}
			else
			{
				this.bWasTouched = false;
				this.bTouched = false;
				this.setModified();
			}
		}
	}

	public Vector2i GetContainerSize()
	{
		return this.containerSize;
	}

	public void SetContainerSize(Vector2i _containerSize, bool clearItems = true)
	{
		this.containerSize = _containerSize;
		if (clearItems)
		{
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
	}

	public override void OnRemove(World world)
	{
		base.OnRemove(world);
		this.OnDestroy();
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency || this.readVersion > 8)
		{
			if (_br.ReadBoolean())
			{
				this.lootListName = _br.ReadString();
			}
			this.containerSize = default(Vector2i);
			this.containerSize.x = (int)_br.ReadUInt16();
			this.containerSize.y = (int)_br.ReadUInt16();
			this.bTouched = _br.ReadBoolean();
			this.worldTimeTouched = (ulong)_br.ReadUInt32();
			this.bPlayerBackpack = _br.ReadBoolean();
			int num = Math.Min((int)_br.ReadInt16(), this.containerSize.x * this.containerSize.y);
			if (this.bUserAccessing)
			{
				ItemStack itemStack = ItemStack.Empty.Clone();
				if (_eStreamMode == TileEntity.StreamModeRead.Persistency && this.readVersion < 3)
				{
					for (int i = 0; i < num; i++)
					{
						itemStack.ReadOld(_br);
					}
				}
				else
				{
					for (int j = 0; j < num; j++)
					{
						itemStack.Read(_br);
					}
				}
			}
			else
			{
				if (this.containerSize.x * this.containerSize.y != this.items.Length)
				{
					this.items = ItemStack.CreateArray(this.containerSize.x * this.containerSize.y);
				}
				if (_eStreamMode == TileEntity.StreamModeRead.Persistency && this.readVersion < 3)
				{
					for (int k = 0; k < num; k++)
					{
						this.items[k].Clear();
						this.items[k].ReadOld(_br);
					}
				}
				else
				{
					for (int l = 0; l < num; l++)
					{
						this.items[l].Clear();
						this.items[l].Read(_br);
					}
				}
			}
			this.bPlayerStorage = _br.ReadBoolean();
			if ((_eStreamMode != TileEntity.StreamModeRead.Persistency || this.readVersion > 9) && _br.ReadBoolean())
			{
				this.preferences = new PreferenceTracker(-1);
				this.preferences.Read(_br);
			}
			return;
		}
		throw new Exception("Outdated loot data");
	}

	public override void write(PooledBinaryWriter stream, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(stream, _eStreamMode);
		bool flag = !string.IsNullOrEmpty(this.lootListName);
		stream.Write(flag);
		if (flag)
		{
			stream.Write(this.lootListName);
		}
		stream.Write((ushort)this.containerSize.x);
		stream.Write((ushort)this.containerSize.y);
		stream.Write(this.bTouched);
		stream.Write((uint)this.worldTimeTouched);
		stream.Write(this.bPlayerBackpack);
		stream.Write((short)this.items.Length);
		ItemStack[] items = this.items;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Clone().Write(stream);
		}
		stream.Write(this.bPlayerStorage);
		bool flag2 = this.preferences != null;
		stream.Write(flag2);
		if (flag2)
		{
			this.preferences.Write(stream);
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Loot;
	}

	public ItemStack[] GetItems()
	{
		return this.items;
	}

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		this.OnDestroy();
		if (_other is TileEntityLootContainer)
		{
			TileEntityLootContainer tileEntityLootContainer = _other as TileEntityLootContainer;
			this.bTouched = tileEntityLootContainer.bTouched;
			this.worldTimeTouched = tileEntityLootContainer.worldTimeTouched;
			this.bPlayerBackpack = tileEntityLootContainer.bPlayerBackpack;
			this.bPlayerStorage = tileEntityLootContainer.bPlayerStorage;
			this.items = ItemStack.Clone(tileEntityLootContainer.items, 0, this.containerSize.x * this.containerSize.y);
			if (this.items.Length != this.containerSize.x * this.containerSize.y)
			{
				Log.Error("UpgradeDowngradeFrom: other.size={0}, other.length={1}, this.size={2}, this.length={3}", new object[]
				{
					tileEntityLootContainer.containerSize,
					tileEntityLootContainer.items.Length,
					this.containerSize,
					this.items.Length
				});
			}
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		ITileEntityLootable tileEntityLootable;
		if (!_teNew.TryGetSelfOrFeature(out tileEntityLootable))
		{
			GameManager.Instance.DropContentOfLootContainerServer(_bvOld, base.ToWorldPos(), base.EntityId, this);
		}
	}

	public void UpdateSlot(int _idx, ItemStack _item)
	{
		this.items[_idx] = _item.Clone();
		base.NotifyListeners();
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
		base.NotifyListeners();
		this.bTouched = true;
		this.setModified();
	}

	[return: TupleElementNames(new string[]
	{
		"anyMoved",
		"allMoved"
	})]
	public ValueTuple<bool, bool> TryStackItem(int startIndex, ItemStack _itemStack)
	{
		bool item = false;
		int count = _itemStack.count;
		int num = 0;
		for (int i = startIndex; i < this.items.Length; i++)
		{
			num = _itemStack.count;
			if (_itemStack.itemValue.type == this.items[i].itemValue.type && this.items[i].CanStackPartly(ref num))
			{
				this.items[i].count += num;
				_itemStack.count -= num;
				this.setModified();
				if (_itemStack.count == 0)
				{
					base.NotifyListeners();
					return new ValueTuple<bool, bool>(true, true);
				}
			}
		}
		if (_itemStack.count != count)
		{
			item = true;
			base.NotifyListeners();
		}
		return new ValueTuple<bool, bool>(item, false);
	}

	public bool AddItem(ItemStack _item)
	{
		for (int i = 0; i < this.items.Length; i++)
		{
			if (this.items[i].IsEmpty())
			{
				this.UpdateSlot(i, _item);
				base.SetModified();
				return true;
			}
		}
		return false;
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

	public override void Reset(FastTags<TagGroup.Global> questTags)
	{
		base.Reset(questTags);
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
		BlockLoot blockLoot = base.blockValue.Block as BlockLoot;
		if (blockLoot != null && blockLoot.AlternateLootList != null)
		{
			for (int j = 0; j < blockLoot.AlternateLootList.Count; j++)
			{
				if (questTags.Test_AnySet(blockLoot.AlternateLootList[j].tag))
				{
					this.lootListName = blockLoot.AlternateLootList[j].lootEntry;
					break;
				}
			}
		}
		this.setModified();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] itemsArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> entList;
}
