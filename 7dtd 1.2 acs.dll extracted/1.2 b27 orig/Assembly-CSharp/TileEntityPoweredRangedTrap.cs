using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class TileEntityPoweredRangedTrap : TileEntityPoweredBlock
{
	public ItemClass AmmoItem
	{
		get
		{
			if (this.ammoItem == null)
			{
				Block block = this.chunk.GetBlock(base.localChunkPos).Block;
				BlockLauncher blockLauncher = block as BlockLauncher;
				if (blockLauncher != null)
				{
					this.ammoItem = ItemClass.GetItemClass(blockLauncher.AmmoItemName, false);
				}
				else
				{
					BlockRanged blockRanged = block as BlockRanged;
					if (blockRanged != null)
					{
						this.ammoItem = ItemClass.GetItemClass(blockRanged.AmmoItemName, false);
					}
				}
			}
			return this.ammoItem;
		}
		set
		{
			this.ammoItem = value;
		}
	}

	public TileEntityPoweredRangedTrap(Chunk _chunk) : base(_chunk)
	{
	}

	public int OwnerEntityID
	{
		get
		{
			if (this.ownerEntityID == -1)
			{
				this.SetOwnerEntityID();
			}
			return this.ownerEntityID;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.ownerEntityID = value;
		}
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier != null && _userIdentifier.Equals(this.ownerID);
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return this.ownerID;
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		this.ownerID = _userIdentifier;
		this.SetOwnerEntityID();
		this.setModified();
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetSendSlots()
	{
		this.ClientData.SendSlots = true;
	}

	public override void OnSetLocalChunkPosition()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetOwnerEntityID()
	{
		if (GameManager.Instance == null)
		{
			return;
		}
		PersistentPlayerList persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
		PersistentPlayerData persistentPlayerData = (persistentPlayerList != null) ? persistentPlayerList.GetPlayerData(this.ownerID) : null;
		if (persistentPlayerData != null)
		{
			this.ownerEntityID = persistentPlayerData.EntityId;
			return;
		}
		this.ownerEntityID = -1;
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		this.ownerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		this.SetOwnerEntityID();
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency)
		{
			if (_eStreamMode == TileEntity.StreamModeRead.FromClient)
			{
				this.bUserAccessing = _br.ReadBoolean();
				if (this.PowerItem == null)
				{
					this.PowerItem = base.CreatePowerItemForTileEntity((ushort)this.chunk.GetBlock(base.localChunkPos).type);
				}
				(this.PowerItem as PowerRangedTrap).IsLocked = _br.ReadBoolean();
				if (_br.ReadBoolean())
				{
					this.ClientData.ItemSlots = GameUtils.ReadItemStack(_br);
					(this.PowerItem as PowerRangedTrap).SetSlots(this.ClientData.ItemSlots);
				}
				this.TargetType = _br.ReadInt32();
				return;
			}
			bool flag = _br.ReadBoolean();
			bool flag2 = !this.bUserAccessing || (this.bUserAccessing && this.ClientData.IsLocked);
			if (flag)
			{
				this.ClientData.IsLocked = _br.ReadBoolean();
				ItemStack[] itemSlots = GameUtils.ReadItemStack(_br);
				if (flag2)
				{
					this.ClientData.ItemSlots = itemSlots;
				}
			}
			int targetType = _br.ReadInt32();
			if (!this.bUserAccessing)
			{
				this.TargetType = targetType;
			}
		}
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		this.ownerID.ToStream(_bw, false);
		if (_eStreamMode != TileEntity.StreamModeWrite.Persistency)
		{
			if (_eStreamMode == TileEntity.StreamModeWrite.ToServer)
			{
				_bw.Write(this.bUserAccessing);
				_bw.Write(this.IsLocked);
				_bw.Write(this.ClientData.SendSlots);
				if (this.ClientData.SendSlots)
				{
					GameUtils.WriteItemStack(_bw, this.ClientData.ItemSlots);
					this.ClientData.SendSlots = false;
				}
				_bw.Write(this.TargetType);
				return;
			}
			PowerRangedTrap powerRangedTrap = this.PowerItem as PowerRangedTrap;
			_bw.Write(powerRangedTrap != null);
			if (powerRangedTrap != null)
			{
				_bw.Write(powerRangedTrap.IsLocked);
				GameUtils.WriteItemStack(_bw, powerRangedTrap.Stacks);
			}
			_bw.Write(this.TargetType);
		}
	}

	public bool IsLocked
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (this.PowerItem as PowerRangedTrap).IsLocked;
			}
			return this.ClientData.IsLocked;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				(this.PowerItem as PowerRangedTrap).IsLocked = value;
				return;
			}
			this.ClientData.IsLocked = value;
			base.SetModified();
		}
	}

	public ItemStack[] ItemSlots
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (this.PowerItem as PowerRangedTrap).Stacks;
			}
			return this.ClientData.ItemSlots;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				(this.PowerItem as PowerRangedTrap).SetSlots(value);
				return;
			}
			this.ClientData.ItemSlots = value;
			base.SetModified();
		}
	}

	public int TargetType
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (int)(this.PowerItem as PowerRangedTrap).TargetType;
			}
			return this.ClientData.TargetType;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				(this.PowerItem as PowerRangedTrap).TargetType = (PowerRangedTrap.TargetTypes)value;
				return;
			}
			this.ClientData.TargetType = value;
		}
	}

	public bool TargetSelf
	{
		get
		{
			return (this.TargetType & 1) == 1;
		}
	}

	public bool TargetAllies
	{
		get
		{
			return (this.TargetType & 2) == 2;
		}
	}

	public bool TargetStrangers
	{
		get
		{
			return (this.TargetType & 4) == 4;
		}
	}

	public bool TargetZombies
	{
		get
		{
			return (this.TargetType & 8) == 8;
		}
	}

	public bool TryStackItem(ItemStack itemStack)
	{
		if (this.IsLocked)
		{
			return false;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return (this.PowerItem as PowerRangedTrap).TryStackItem(itemStack);
		}
		for (int i = 0; i < this.ClientData.ItemSlots.Length; i++)
		{
			int count = itemStack.count;
			if (this.ClientData.ItemSlots[i].IsEmpty())
			{
				this.ClientData.ItemSlots[i] = itemStack.Clone();
				this.ClientData.SendSlots = true;
				base.SetModified();
				itemStack.count = 0;
				return true;
			}
			if (this.ClientData.ItemSlots[i].itemValue.type == itemStack.itemValue.type && this.ClientData.ItemSlots[i].CanStackPartly(ref count))
			{
				this.ClientData.ItemSlots[i].count += count;
				itemStack.count -= count;
				this.ClientData.SendSlots = true;
				base.SetModified();
				if (itemStack.count == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetValuesFromBlock(ushort blockID)
	{
		base.SetValuesFromBlock(blockID);
		if (Block.list[(int)blockID].Properties.Values.ContainsKey("BurstFireRate"))
		{
			this.DelayTime = StringParsers.ParseFloat(Block.list[(int)blockID].Properties.Values["BurstFireRate"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.DelayTime = 0.5f;
		}
		if (Block.list[(int)blockID].Properties.Values.ContainsKey("ShowTargeting"))
		{
			this.ShowTargeting = StringParsers.ParseBool(Block.list[(int)blockID].Properties.Values["ShowTargeting"], 0, -1, true);
			return;
		}
		this.ShowTargeting = true;
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.PowerRangeTrap;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool DecrementAmmo()
	{
		for (int i = 0; i < this.ItemSlots.Length; i++)
		{
			if (this.ItemSlots[i].count > 0)
			{
				this.ItemSlots[i].count--;
				if (this.ItemSlots[i].count == 0)
				{
					this.ItemSlots[i] = ItemStack.Empty.Clone();
				}
				base.SetModified();
				return true;
			}
		}
		return false;
	}

	public bool AddItem(ItemStack itemStack)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return (this.PowerItem as PowerRangedTrap).AddItem(itemStack);
		}
		for (int i = 0; i < this.ItemSlots.Length; i++)
		{
			if (this.ItemSlots[i].IsEmpty())
			{
				this.ItemSlots[i] = itemStack;
				return true;
			}
		}
		return false;
	}

	public override void ClientUpdate()
	{
		if (Time.time > this.updateTime)
		{
			this.updateTime = Time.time + this.DelayTime;
			World world = GameManager.Instance.World;
			BlockValue block = this.chunk.GetBlock(base.localChunkPos);
			Block block2 = block.Block;
			BlockLauncher blockLauncher = block2 as BlockLauncher;
			if (blockLauncher != null)
			{
				blockLauncher.InstantiateProjectile(world, base.GetClrIdx(), base.ToWorldPos());
				return;
			}
			if (block2 is BlockRanged)
			{
				block2.ActivateBlock(world, base.GetClrIdx(), base.ToWorldPos(), block, base.IsPowered, base.IsPowered);
			}
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap;
		if (_teNew.TryGetSelfOrFeature(out tileEntityPoweredRangedTrap))
		{
			return;
		}
		List<ItemStack> list = new List<ItemStack>();
		list.AddRange(this.ItemSlots);
		Vector3 pos = base.ToWorldCenterPos();
		pos.y += 0.9f;
		GameManager.Instance.DropContentInLootContainerServer(-1, "DroppedLootContainer", pos, list.ToArray(), true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass ammoItem;

	public readonly TileEntityPoweredRangedTrap.ClientAmmoData ClientData = new TileEntityPoweredRangedTrap.ClientAmmoData();

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int ownerEntityID = -1;

	public bool ShowTargeting = true;

	public class ClientAmmoData
	{
		public ClientAmmoData()
		{
			for (int i = 0; i < this.ItemSlots.Length; i++)
			{
				this.ItemSlots[i] = ItemStack.Empty.Clone();
			}
		}

		public bool IsLocked;

		public bool SendSlots;

		public ItemStack[] ItemSlots = new ItemStack[3];

		public int TargetType = 12;
	}
}
