using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;

public class TileEntityVendingMachine : TileEntityTrader, ILockable
{
	public TileEntityVendingMachine(Chunk _chunk) : base(_chunk)
	{
		this.allowedUserIds = new List<PlatformUserIdentifierAbs>();
		this.isLocked = true;
		this.ownerID = null;
		this.password = "";
		this.rentalEndTime = 0UL;
		this.rentalEndDay = 0;
		this.TraderData = new TraderData();
	}

	public TileEntityVendingMachine.RentResult CanRent()
	{
		if (this.ownerID != null && !this.ownerID.Equals(PlatformManager.InternalLocalUserIdentifier))
		{
			return TileEntityVendingMachine.RentResult.AlreadyRented;
		}
		if (GameManager.Instance.World.GetPrimaryPlayer().PlayerUI.xui.PlayerInventory.CurrencyAmount < this.TraderData.TraderInfo.RentCost)
		{
			return TileEntityVendingMachine.RentResult.NotEnoughMoney;
		}
		if (this.checkAlreadyRentingVM())
		{
			return TileEntityVendingMachine.RentResult.AlreadyRentingVM;
		}
		return TileEntityVendingMachine.RentResult.Allowed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkAlreadyRentingVM()
	{
		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		Vector3i rentedVMPosition = primaryPlayer.RentedVMPosition;
		return !(rentedVMPosition == base.ToWorldPos()) && !(rentedVMPosition == Vector3i.zero) && primaryPlayer.RentalEndDay > GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityVendingMachine(TileEntityVendingMachine _other) : base(null)
	{
		this.allowedUserIds.AddRange(_other.allowedUserIds);
		this.isLocked = _other.isLocked;
		this.ownerID = _other.ownerID;
		this.password = _other.password;
		this.bUserAccessing = _other.bUserAccessing;
		this.rentalEndTime = _other.rentalEndTime;
		this.rentalEndDay = _other.rentalEndDay;
		this.TraderData = new TraderData(_other.TraderData);
		this.nextAutoBuy = _other.nextAutoBuy;
	}

	public override TileEntity Clone()
	{
		return new TileEntityVendingMachine(this);
	}

	public bool IsLocked()
	{
		return this.isLocked;
	}

	public void SetLocked(bool _isLocked)
	{
		this.isLocked = _isLocked;
		this.setModified();
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		this.ownerID = _userIdentifier;
		this.setModified();
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		return (_userIdentifier != null && _userIdentifier.Equals(this.ownerID)) || this.allowedUserIds.Contains(_userIdentifier);
	}

	public bool LocalPlayerIsOwner()
	{
		return this.IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier != null && _userIdentifier.Equals(this.ownerID);
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return this.ownerID;
	}

	public bool HasPassword()
	{
		return !string.IsNullOrEmpty(this.password);
	}

	public string GetPassword()
	{
		return this.password;
	}

	public bool CheckPassword(string _password, PlatformUserIdentifierAbs _userIdentifier, out bool changed)
	{
		changed = false;
		if (_userIdentifier != null && _userIdentifier.Equals(this.ownerID))
		{
			if (Utils.HashString(_password) != this.password)
			{
				changed = true;
				this.password = Utils.HashString(_password);
				this.allowedUserIds.Clear();
				this.setModified();
			}
			return true;
		}
		if (Utils.HashString(_password) == this.password)
		{
			this.allowedUserIds.Add(_userIdentifier);
			this.setModified();
			return true;
		}
		return false;
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		int num = _br.ReadInt32();
		this.isLocked = _br.ReadBoolean();
		this.ownerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		this.password = _br.ReadString();
		this.allowedUserIds = new List<PlatformUserIdentifierAbs>();
		int num2 = _br.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			this.allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br, false, false));
		}
		if (num > 1)
		{
			this.rentalEndDay = _br.ReadInt32();
		}
		else
		{
			this.rentalEndTime = _br.ReadUInt64();
			this.rentalEndDay = GameUtils.WorldTimeToDays(this.rentalEndTime);
		}
		this.TraderData.Read(0, _br);
		if (this.TraderData.TraderInfo.Rentable)
		{
			this.nextAutoBuy = _br.ReadUInt64();
		}
		this.syncNeeded = false;
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(2);
		_bw.Write(this.isLocked);
		this.ownerID.ToStream(_bw, false);
		_bw.Write(this.password);
		_bw.Write(this.allowedUserIds.Count);
		for (int i = 0; i < this.allowedUserIds.Count; i++)
		{
			this.allowedUserIds[i].ToStream(_bw, false);
		}
		_bw.Write(this.rentalEndDay);
		this.TraderData.Write(_bw);
		if (this.TraderData.TraderInfo.Rentable)
		{
			_bw.Write(this.nextAutoBuy);
		}
	}

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		if (_other is ILockable)
		{
			ILockable lockable = _other as ILockable;
			base.EntityId = lockable.EntityId;
			this.SetLocked(lockable.IsLocked());
			this.SetOwner(lockable.GetOwner());
			this.allowedUserIds = new List<PlatformUserIdentifierAbs>(lockable.GetUsers());
			this.password = lockable.GetPassword();
			this.setModified();
		}
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return this.allowedUserIds;
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.VendingMachine;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void ClearOwner()
	{
		this.ownerID = null;
	}

	public bool IsRentable
	{
		get
		{
			return this.TraderData.TraderInfo.Rentable;
		}
	}

	public float RentTimeRemaining
	{
		get
		{
			return (float)(this.rentalEndDay - GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime));
		}
	}

	public int RentalEndDay
	{
		get
		{
			return this.rentalEndDay;
		}
	}

	public bool Rent()
	{
		if ((this.ownerID != null && !this.ownerID.Equals(PlatformManager.InternalLocalUserIdentifier)) || !this.TraderData.TraderInfo.Rentable)
		{
			return false;
		}
		XUi xui = GameManager.Instance.World.GetPrimaryPlayer().PlayerUI.xui;
		if (xui.PlayerInventory.CurrencyAmount >= this.TraderData.TraderInfo.RentCost)
		{
			ItemStack itemStack = new ItemStack(ItemClass.GetItem(TraderInfo.CurrencyItem, false), this.TraderData.TraderInfo.RentCost);
			xui.PlayerInventory.RemoveItem(itemStack);
			if (this.ownerID == null)
			{
				this.ownerID = PlatformManager.InternalLocalUserIdentifier;
				this.rentalEndDay = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime) + 30;
				this.SetAutoBuyTime(true);
			}
			else
			{
				this.rentalEndDay += 30;
			}
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			primaryPlayer.RentedVMPosition = base.ToWorldPos();
			primaryPlayer.RentalEndDay = this.rentalEndDay;
			this.setModified();
			return true;
		}
		return false;
	}

	public void ClearVendingMachine()
	{
		this.TraderData.AvailableMoney = 0;
		this.TraderData.PrimaryInventory.Clear();
		this.ownerID = null;
		this.allowedUserIds.Clear();
		this.rentalEndTime = 0UL;
		this.password = "";
		this.setModified();
	}

	public bool TryAutoBuy(bool isInitial = true)
	{
		if (this.nextAutoBuy == 0UL)
		{
			this.SetAutoBuyTime(true);
		}
		if (GameManager.Instance.World.worldTime > this.nextAutoBuy)
		{
			GameRandom random = GameManager.Instance.lootManager.Random;
			if (random.RandomFloat < this.autoBuyThreshold && this.TraderData.PrimaryInventory.Count > this.minimumAutoBuyCount)
			{
				int num = random.RandomRange(1, Mathf.Max(this.TraderData.PrimaryInventory.Count / 10, 1));
				Log.Warning("Items Purchased: " + num.ToString());
				for (int i = 0; i < num; i++)
				{
					int num2 = 0;
					for (int j = 0; j < this.TraderData.PrimaryInventory.Count; j++)
					{
						if (this.TraderData.GetMarkupByIndex(j) <= 0)
						{
							ItemStack itemStack = this.TraderData.PrimaryInventory[j];
							if ((itemStack.itemValue.ItemClass.IsBlock() ? Block.list[itemStack.itemValue.type].EconomicValue : itemStack.itemValue.ItemClass.EconomicValue) > 0f && itemStack.itemValue.ItemClass.SellableToTrader)
							{
								num2++;
							}
						}
					}
					if (num2 > 0)
					{
						int num3 = random.RandomRange(num2);
						num2 = 0;
						for (int k = 0; k < this.TraderData.PrimaryInventory.Count; k++)
						{
							if (this.TraderData.GetMarkupByIndex(k) <= 0)
							{
								ItemStack itemStack2 = this.TraderData.PrimaryInventory[k];
								if ((itemStack2.itemValue.ItemClass.IsBlock() ? Block.list[itemStack2.itemValue.type].EconomicValue : itemStack2.itemValue.ItemClass.EconomicValue) > 0f && itemStack2.itemValue.ItemClass.SellableToTrader)
								{
									if (num2 == num3)
									{
										int count = itemStack2.count;
										int buyPrice = XUiM_Trader.GetBuyPrice(LocalPlayerUI.GetUIForPrimaryPlayer().xui, itemStack2.itemValue, count, null, k);
										this.TraderData.PrimaryInventory.RemoveAt(k);
										this.TraderData.RemoveMarkup(k);
										this.TraderData.AvailableMoney += buyPrice;
										break;
									}
									num2++;
								}
							}
						}
					}
				}
				this.autoBuyThreshold = this.autoBuyThresholdStep;
			}
			else
			{
				this.autoBuyThreshold += this.autoBuyThresholdStep;
			}
			this.SetAutoBuyTime(false);
			return this.TryAutoBuy(false);
		}
		return !isInitial;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAutoBuyTime(bool isInitial)
	{
		uint num = 24000U;
		if (isInitial)
		{
			this.nextAutoBuy = GameManager.Instance.World.worldTime + (ulong)num;
			return;
		}
		this.nextAutoBuy += (ulong)num;
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (!this.TraderData.TraderInfo.PlayerOwned && this.TraderData.TraderInfo.Rentable && this.ownerID != null && this.rentalEndDay <= GameUtils.WorldTimeToDays(world.worldTime))
		{
			this.ClearVendingMachine();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new const int ver = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlatformUserIdentifierAbs> allowedUserIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public string password;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong rentalEndTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong nextAutoBuy;

	[PublicizedFrom(EAccessModifier.Private)]
	public int rentalEndDay;

	[PublicizedFrom(EAccessModifier.Private)]
	public float autoBuyThresholdStep = 0.333333343f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float autoBuyThreshold = 0.333333343f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int minimumAutoBuyCount = 5;

	public enum RentResult
	{
		Allowed,
		AlreadyRented,
		AlreadyRentingVM,
		NotEnoughMoney
	}
}
