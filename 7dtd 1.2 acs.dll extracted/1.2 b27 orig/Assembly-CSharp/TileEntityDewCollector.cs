using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TileEntityDewCollector : TileEntity, IInventory
{
	public bool IsBlocked
	{
		get
		{
			return this.isBlocked;
		}
	}

	public float[] fillValues
	{
		get
		{
			if (this.fillValuesArr == null)
			{
				this.fillValuesArr = new float[this.containerSize.x * this.containerSize.y];
			}
			return this.fillValuesArr;
		}
		set
		{
			this.fillValuesArr = value;
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

	public ItemStack[] ModSlots
	{
		get
		{
			return this.modSlots;
		}
		set
		{
			if (!this.IsModsSame(value))
			{
				this.modSlots = ItemStack.Clone(value);
				this.visibleChanged = true;
				this.modsChanged = true;
				this.HandleModChanged();
				this.UpdateVisible();
				this.setModified();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsModsSame(ItemStack[] _modSlots)
	{
		if (_modSlots == null || _modSlots.Length != this.modSlots.Length)
		{
			return false;
		}
		for (int i = 0; i < _modSlots.Length; i++)
		{
			if (!_modSlots[i].Equals(this.modSlots[i]))
			{
				return false;
			}
		}
		return true;
	}

	public TileEntityDewCollector(Chunk _chunk) : base(_chunk)
	{
		this.containerSize = new Vector2i(3, 1);
		this.modSlots = ItemStack.CreateArray(3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityDewCollector(TileEntityDewCollector _other) : base(null)
	{
		this.containerSize = _other.containerSize;
		this.items = ItemStack.Clone(_other.items);
		this.modSlots = ItemStack.Clone(_other.modSlots);
		this.worldTimeTouched = _other.worldTimeTouched;
		this.bUserAccessing = _other.bUserAccessing;
		this.ConvertToItem = _other.ConvertToItem;
		this.CurrentIndex = _other.CurrentIndex;
		this.CurrentConvertTime = _other.CurrentConvertTime;
		this.leftoverTime = _other.leftoverTime;
	}

	public override TileEntity Clone()
	{
		return new TileEntityDewCollector(this);
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		this.HandleUpdate(world);
	}

	public void HandleUpdate(World world)
	{
		if (this.ConvertToItem == null)
		{
			BlockDewCollector blockDewCollector = (BlockDewCollector)base.blockValue.Block;
			this.ConvertToItem = ItemClass.GetItemClass(blockDewCollector.ConvertToItem, false);
			this.ModdedConvertToItem = ItemClass.GetItemClass(blockDewCollector.ModdedConvertToItem, false);
			this.modsChanged = true;
		}
		if (base.blockValue.Block.IsUnderwater(GameManager.Instance.World, base.ToWorldPos(), base.blockValue))
		{
			return;
		}
		if (this.countdownBlockedCheck.HasPassed())
		{
			this.isBlocked = this.HandleSkyCheck();
			this.countdownBlockedCheck.ResetAndRestart();
		}
		if (this.isBlocked)
		{
			return;
		}
		bool flag = this.HandleLeftOverTime();
		this.worldTimeTouched = world.worldTime;
		float num = (this.lastWorldTime != 0UL) ? GameUtils.WorldTimeToTotalSeconds(world.worldTime - this.lastWorldTime) : 0f;
		this.lastWorldTime = world.worldTime;
		if (num <= 0f)
		{
			return;
		}
		this.HandleModChanged();
		if (this.CurrentIndex == -1)
		{
			for (int i = 0; i < this.items.Length; i++)
			{
				if (this.items[i].IsEmpty())
				{
					this.CurrentIndex = i;
					break;
				}
				if (this.fillValues[i] != -1f)
				{
					this.fillValues[i] = -1f;
					flag = true;
				}
			}
			if (this.CurrentIndex == -1)
			{
				this.leftoverTime = 0f;
			}
		}
		for (int j = 0; j < this.items.Length; j++)
		{
			if (this.CurrentIndex == j)
			{
				if (this.items[j].IsEmpty())
				{
					if (this.fillValues[j] == -1f)
					{
						BlockDewCollector blockDewCollector2 = (BlockDewCollector)base.blockValue.Block;
						this.CurrentConvertTime = GameManager.Instance.World.GetGameRandom().RandomRange(blockDewCollector2.MinConvertTime, blockDewCollector2.MaxConvertTime);
						this.fillValues[j] = this.leftoverTime;
						this.leftoverTime = 0f;
					}
					else
					{
						this.fillValues[j] += num * this.CurrentConvertSpeed;
						if (this.fillValues[j] >= this.CurrentConvertTime)
						{
							this.leftoverTime = this.fillValues[j] - this.CurrentConvertTime;
							this.items[j] = new ItemStack(new ItemValue(this.IsModdedConvertItem ? this.ModdedConvertToItem.Id : this.ConvertToItem.Id, false), this.CurrentConvertCount);
							this.fillValues[j] = -1f;
							this.CurrentConvertTime = -1f;
							this.CurrentIndex = -1;
						}
					}
					flag = true;
				}
				else
				{
					if (this.fillValues[j] != -1f)
					{
						this.fillValues[j] = -1f;
					}
					this.CurrentIndex = -1;
					flag = true;
				}
			}
			else if (this.fillValues[j] != -1f)
			{
				this.fillValues[j] = -1f;
				flag = true;
			}
		}
		if (flag)
		{
			base.NotifyListeners();
			base.emitHeatMapEvent(world, EnumAIDirectorChunkEvent.Campfire);
			this.setModified();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleModChanged()
	{
		if (this.modsChanged)
		{
			this.modsChanged = false;
			BlockDewCollector blockDewCollector = (BlockDewCollector)base.blockValue.Block;
			this.IsModdedConvertItem = false;
			this.CurrentConvertCount = 1;
			this.CurrentConvertSpeed = 1f;
			for (int i = 0; i < this.modSlots.Length; i++)
			{
				if (!this.modSlots[i].IsEmpty())
				{
					switch (blockDewCollector.ModTypes[i])
					{
					case BlockDewCollector.ModEffectTypes.Type:
						this.IsModdedConvertItem = true;
						break;
					case BlockDewCollector.ModEffectTypes.Speed:
						this.CurrentConvertSpeed = blockDewCollector.ModdedConvertSpeed;
						break;
					case BlockDewCollector.ModEffectTypes.Count:
						this.CurrentConvertCount = blockDewCollector.ModdedConvertCount;
						break;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateVisible()
	{
		if (this.visibleChanged)
		{
			this.visibleChanged = false;
			BlockDewCollector blockDewCollector = GameManager.Instance.World.GetBlock(base.ToWorldPos()).Block as BlockDewCollector;
			if (blockDewCollector != null)
			{
				blockDewCollector.UpdateVisible(this);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HandleSkyCheck()
	{
		Vector3i localChunkPos = base.localChunkPos;
		for (int i = 0; i < 7; i++)
		{
			localChunkPos.y++;
			if (localChunkPos.y >= 256)
			{
				break;
			}
			BlockValue block = this.chunk.GetBlock(localChunkPos);
			if (block.Block != base.blockValue.Block && block.Block.IsCollideArrows)
			{
				return true;
			}
		}
		return false;
	}

	public bool HandleLeftOverTime()
	{
		if (this.CurrentConvertTime == -1f)
		{
			this.SetupCurrentConvertTime();
		}
		if (this.leftoverTime == 0f)
		{
			return false;
		}
		bool result = false;
		if (this.CurrentIndex != -1)
		{
			if (this.items[this.CurrentIndex].IsEmpty())
			{
				if (this.fillValues[this.CurrentIndex] == -1f)
				{
					this.fillValues[this.CurrentIndex] = 0f;
				}
				if (this.leftoverTime > this.CurrentConvertTime)
				{
					this.items[this.CurrentIndex] = new ItemStack(new ItemValue(this.IsModdedConvertItem ? this.ModdedConvertToItem.Id : this.ConvertToItem.Id, false), this.CurrentConvertCount);
					this.leftoverTime -= this.CurrentConvertTime;
					this.fillValues[this.CurrentIndex] = -1f;
					this.CurrentIndex = -1;
				}
				else
				{
					this.fillValues[this.CurrentIndex] = this.leftoverTime;
					this.leftoverTime = 0f;
				}
				result = true;
			}
			else
			{
				this.CurrentIndex = -1;
			}
		}
		if (this.leftoverTime == 0f)
		{
			return result;
		}
		for (int i = 0; i < this.items.Length; i++)
		{
			if (this.items[i].IsEmpty())
			{
				if (this.leftoverTime <= this.CurrentConvertTime)
				{
					this.fillValues[i] = this.leftoverTime;
					this.leftoverTime = 0f;
					this.CurrentIndex = i;
					return true;
				}
				this.items[i] = new ItemStack(new ItemValue(this.IsModdedConvertItem ? this.ModdedConvertToItem.Id : this.ConvertToItem.Id, false), this.CurrentConvertCount);
				this.leftoverTime -= this.CurrentConvertTime;
				this.fillValues[i] = -1f;
				this.CurrentIndex = -1;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupCurrentConvertTime()
	{
		BlockDewCollector blockDewCollector = (BlockDewCollector)base.blockValue.Block;
		this.CurrentConvertTime = GameManager.Instance.World.GetGameRandom().RandomRange(blockDewCollector.MinConvertTime, blockDewCollector.MaxConvertTime);
	}

	public void SetWorldTime()
	{
		this.lastWorldTime = GameManager.Instance.World.worldTime;
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
		this.containerSize = default(Vector2i);
		this.containerSize.x = (int)_br.ReadUInt16();
		this.containerSize.y = (int)_br.ReadUInt16();
		this.lastWorldTime = _br.ReadUInt64();
		this.CurrentConvertTime = _br.ReadSingle();
		this.CurrentIndex = (int)_br.ReadInt16();
		this.leftoverTime = _br.ReadSingle();
		int num = Math.Min((int)_br.ReadInt16(), this.containerSize.x * this.containerSize.y);
		if (this.containerSize.x * this.containerSize.y != this.items.Length)
		{
			this.items = ItemStack.CreateArray(this.containerSize.x * this.containerSize.y);
		}
		if (this.containerSize.x * this.containerSize.y != this.fillValues.Length)
		{
			this.fillValues = new float[this.containerSize.x * this.containerSize.y];
		}
		for (int i = 0; i < num; i++)
		{
			this.items[i].Clear();
			this.items[i].Read(_br);
		}
		for (int j = 0; j < num; j++)
		{
			this.fillValues[j] = _br.ReadSingle();
		}
		if (this.readVersion >= 11 || _eStreamMode != TileEntity.StreamModeRead.Persistency)
		{
			int num2 = (int)_br.ReadInt16();
			for (int k = 0; k < num2; k++)
			{
				this.modSlots[k].Clear();
				this.modSlots[k].Read(_br);
			}
			this.modsChanged = true;
			this.HandleModChanged();
		}
	}

	public override void write(PooledBinaryWriter stream, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(stream, _eStreamMode);
		stream.Write((ushort)this.containerSize.x);
		stream.Write((ushort)this.containerSize.y);
		stream.Write(this.lastWorldTime);
		stream.Write(this.CurrentConvertTime);
		stream.Write((short)this.CurrentIndex);
		stream.Write(this.leftoverTime);
		stream.Write((short)this.items.Length);
		ItemStack[] items = this.items;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Clone().Write(stream);
		}
		for (int j = 0; j < this.fillValues.Length; j++)
		{
			stream.Write(this.fillValues[j]);
		}
		stream.Write((short)this.modSlots.Length);
		items = this.modSlots;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Clone().Write(stream);
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.DewCollector;
	}

	public ItemStack[] GetItems()
	{
		return this.items;
	}

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		this.OnDestroy();
		if (_other is TileEntityDewCollector)
		{
			TileEntityDewCollector tileEntityDewCollector = _other as TileEntityDewCollector;
			this.worldTimeTouched = tileEntityDewCollector.worldTimeTouched;
			this.items = ItemStack.Clone(tileEntityDewCollector.items, 0, this.containerSize.x * this.containerSize.y);
			if (this.items.Length != this.containerSize.x * this.containerSize.y)
			{
				Log.Error("UpgradeDowngradeFrom: other.size={0}, other.length={1}, this.size={2}, this.length={3}", new object[]
				{
					tileEntityDewCollector.containerSize,
					tileEntityDewCollector.items.Length,
					this.containerSize,
					this.items.Length
				});
			}
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		TileEntityDewCollector tileEntityDewCollector;
		if (!_teNew.TryGetSelfOrFeature(out tileEntityDewCollector))
		{
			List<ItemStack> list = new List<ItemStack>();
			if (this.itemsArr != null)
			{
				list.AddRange(this.itemsArr);
			}
			if (this.modSlots != null)
			{
				list.AddRange(this.modSlots);
			}
			Vector3 pos = base.ToWorldCenterPos();
			pos.y += 0.9f;
			GameManager.Instance.DropContentInLootContainerServer(-1, "DroppedLootContainer", pos, list.ToArray(), true);
		}
	}

	public void UpdateSlot(int _idx, ItemStack _item)
	{
		this.items[_idx] = _item.Clone();
		base.NotifyListeners();
	}

	public bool IsWaterEmpty()
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

	public bool IsEmpty()
	{
		for (int i = 0; i < this.items.Length; i++)
		{
			if (!this.items[i].IsEmpty())
			{
				return false;
			}
		}
		for (int j = 0; j < this.modSlots.Length; j++)
		{
			if (!this.modSlots[j].IsEmpty())
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
		for (int j = 0; j < this.fillValues.Length; j++)
		{
			this.fillValues[j] = -1f;
		}
		base.NotifyListeners();
		this.setModified();
	}

	[return: TupleElementNames(new string[]
	{
		"anyMoved",
		"allMoved"
	})]
	public ValueTuple<bool, bool> TryStackItem(int startIndex, ItemStack _itemStack)
	{
		int count = _itemStack.count;
		int num = 0;
		bool item = false;
		for (int i = startIndex; i < this.items.Length; i++)
		{
			num = _itemStack.count;
			if (_itemStack.itemValue.type == this.items[i].itemValue.type && this.items[i].CanStackPartly(ref num))
			{
				this.items[i].count += num;
				_itemStack.count -= num;
				this.setModified();
				item = true;
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
				base.NotifyListeners();
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
		bool flag = false;
		for (int i = 0; i < this.items.Length; i++)
		{
			if (this.items[i].itemValue.ItemClass == _item.ItemClass)
			{
				this.UpdateSlot(i, ItemStack.Empty.Clone());
				flag = true;
			}
		}
		if (flag)
		{
			base.NotifyListeners();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSize;

	public ItemClass ConvertToItem;

	public ItemClass ModdedConvertToItem;

	public float CurrentConvertTime = -1f;

	public float CurrentConvertSpeed = 1f;

	public int CurrentConvertCount = 1;

	public float leftoverTime;

	public int CurrentIndex = -1;

	public bool IsModdedConvertItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public static System.Random r = new System.Random();

	[PublicizedFrom(EAccessModifier.Private)]
	public CountdownTimer countdownBlockedCheck = new CountdownTimer(5f + (float)TileEntityDewCollector.r.NextDouble(), true);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBlocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool visibleChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool modsChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lastWorldTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] fillValuesArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] itemsArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] modSlots;

	public ulong worldTimeTouched;
}
