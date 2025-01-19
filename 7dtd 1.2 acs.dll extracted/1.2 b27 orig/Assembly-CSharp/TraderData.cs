using System;
using System.Collections.Generic;
using System.IO;

public class TraderData
{
	public TraderData()
	{
		this.world = GameManager.Instance.World;
	}

	public TraderData(TraderData other)
	{
		this.lastInventoryUpdate = other.lastInventoryUpdate;
		this.TraderID = other.TraderID;
		this.AvailableMoney = other.AvailableMoney;
		this.PrimaryInventory.AddRange(ItemStack.Clone(other.PrimaryInventory));
		this.priceMarkupList.AddRange(other.priceMarkupList);
		for (int i = 0; i < other.TierItemGroups.Count; i++)
		{
			this.TierItemGroups.Add(ItemStack.Clone(other.TierItemGroups[i]));
		}
	}

	public TraderInfo TraderInfo
	{
		get
		{
			if (this.TraderID != -1)
			{
				return TraderInfo.traderInfoList[this.TraderID];
			}
			return null;
		}
	}

	public float FullTime
	{
		get
		{
			return (float)((this.TraderInfo != null) ? this.TraderInfo.ResetIntervalInTicks : 0);
		}
	}

	public float CurrentTime
	{
		get
		{
			if (this.lastInventoryUpdate == 0UL)
			{
				return 0f;
			}
			return (this.FullTime - (float)((int)(this.world.GetWorldTime() - this.lastInventoryUpdate))) / 10f;
		}
	}

	public ulong NextResetTime
	{
		get
		{
			if (this.TraderInfo == null)
			{
				return 0UL;
			}
			return this.lastInventoryUpdate + (ulong)((long)this.TraderInfo.ResetIntervalInTicks);
		}
	}

	public void AddToPrimaryInventory(ItemStack stack, bool addMarkup)
	{
		for (int i = 0; i < this.PrimaryInventory.Count; i++)
		{
			if (stack.itemValue.type == this.PrimaryInventory[i].itemValue.type)
			{
				ItemClass forId = ItemClass.GetForId(stack.itemValue.type);
				if (forId.CanStack())
				{
					int num = Math.Min(stack.count, forId.Stacknumber.Value - this.PrimaryInventory[i].count);
					stack.count -= num;
					this.PrimaryInventory[i].count += num;
					if (stack.count == 0)
					{
						return;
					}
				}
			}
		}
		if (stack.count > 0)
		{
			this.PrimaryInventory.Add(stack.Clone());
			if (addMarkup)
			{
				this.priceMarkupList.Add(0);
			}
		}
	}

	public int GetPrimaryItemCount(ItemValue itemValue)
	{
		int num = 0;
		for (int i = 0; i < this.PrimaryInventory.Count; i++)
		{
			if (itemValue.type == this.PrimaryInventory[i].itemValue.type)
			{
				num += this.PrimaryInventory[i].count;
			}
		}
		return num;
	}

	public int GetMarkupByIndex(int index)
	{
		if (this.priceMarkupList.Count <= index || index == -1)
		{
			return 0;
		}
		return (int)this.priceMarkupList[index];
	}

	public void IncreaseMarkup(int index)
	{
		if (this.priceMarkupList.Count > index && this.priceMarkupList[index] < 100)
		{
			List<sbyte> list = this.priceMarkupList;
			sbyte b = list[index];
			list[index] = b + 1;
		}
	}

	public void DecreaseMarkup(int index)
	{
		if (this.priceMarkupList.Count > index && this.priceMarkupList[index] > -4)
		{
			List<sbyte> list = this.priceMarkupList;
			sbyte b = list[index];
			list[index] = b - 1;
		}
	}

	public void ResetMarkup(int index)
	{
		if (this.priceMarkupList.Count > index)
		{
			this.priceMarkupList[index] = 0;
		}
	}

	public void RemoveMarkup(int index)
	{
		if (this.priceMarkupList.Count > index)
		{
			this.priceMarkupList.RemoveAt(index);
		}
	}

	public void ClearMarkupList()
	{
		this.priceMarkupList.Clear();
	}

	public void Read(byte _version, BinaryReader _br)
	{
		this.TraderID = _br.ReadInt32();
		this.lastInventoryUpdate = _br.ReadUInt64();
		_br.ReadByte();
		this.ReadInventoryData(_br);
	}

	public void ReadInventoryData(BinaryReader _br)
	{
		this.PrimaryInventory.Clear();
		this.PrimaryInventory.AddRange(GameUtils.ReadItemStack(_br));
		this.TierItemGroups.Clear();
		int num = (int)_br.ReadByte();
		for (int i = 0; i < num; i++)
		{
			this.TierItemGroups.Add(GameUtils.ReadItemStack(_br));
		}
		this.AvailableMoney = _br.ReadInt32();
		this.priceMarkupList.Clear();
		int num2 = _br.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			this.priceMarkupList.Add(_br.ReadSByte());
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(this.TraderID);
		_bw.Write(this.lastInventoryUpdate);
		_bw.Write(TraderData.FileVersion);
		this.WriteInventoryData(_bw);
	}

	public void WriteInventoryData(BinaryWriter _bw)
	{
		GameUtils.WriteItemStack(_bw, this.PrimaryInventory);
		_bw.Write((byte)this.TierItemGroups.Count);
		for (int i = 0; i < this.TierItemGroups.Count; i++)
		{
			GameUtils.WriteItemStack(_bw, this.TierItemGroups[i]);
		}
		_bw.Write(this.AvailableMoney);
		_bw.Write(this.priceMarkupList.Count);
		for (int j = 0; j < this.priceMarkupList.Count; j++)
		{
			_bw.Write(this.priceMarkupList[j]);
		}
	}

	public List<ItemStack> PrimaryInventory = new List<ItemStack>();

	public List<ItemStack[]> TierItemGroups = new List<ItemStack[]>();

	public ulong lastInventoryUpdate;

	public int TraderID = -1;

	public int AvailableMoney;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<sbyte> priceMarkupList = new List<sbyte>();

	public static byte FileVersion = 1;
}
