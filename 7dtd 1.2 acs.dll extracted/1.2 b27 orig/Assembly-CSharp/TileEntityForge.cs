using System;
using UnityEngine;

public class TileEntityForge : TileEntity
{
	public TileEntityForge(Chunk _chunk) : base(_chunk)
	{
		this.fuel = ItemStack.CreateArray(3);
		this.input = ItemStack.CreateArray(1);
		this.mold = ItemStack.Empty.Clone();
		this.output = ItemStack.Empty.Clone();
		this.outputItem = ItemValue.None.Clone();
		this.burningItemValue = ItemValue.None.Clone();
		this.fuelInForgeInTicks = 0;
	}

	public bool CanOperate(ulong _worldTimeInTicks)
	{
		return this.GetFuelLeft(_worldTimeInTicks) + this.GetFuelInStorage() > 0 && this.outputWeight > 0 && this.metalInForge > 0;
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		this.recalcStats();
		int num = (this.lastTickTime != 0UL) ? ((int)(GameTimer.Instance.ticks - this.lastTickTime)) : 0;
		this.lastTickTime = GameTimer.Instance.ticks;
		this.lastTickTimeDataCalculated = this.lastTickTime;
		this.updateLightState(world);
		if (this.fuelInStorageInTicks + this.fuelInForgeInTicks == 0)
		{
			return;
		}
		base.emitHeatMapEvent(world, EnumAIDirectorChunkEvent.Forge);
		bool flag = false;
		num = Utils.FastMin(num, (int)((float)(this.fuelInStorageInTicks + this.fuelInForgeInTicks) / 1f));
		if (this.fuelInStorageInTicks + this.fuelInForgeInTicks > 0)
		{
			int num2 = (int)((float)num * 1f);
			this.fuelInForgeInTicks -= num2;
			while (this.fuelInForgeInTicks < 0)
			{
				flag |= this.moveDown(this.fuel);
				if (!this.fuel[this.fuel.Length - 1].IsEmpty())
				{
					this.burningItemValue = this.fuel[this.fuel.Length - 1].itemValue;
					this.fuelInForgeInTicks += ItemClass.GetFuelValue(this.fuel[this.fuel.Length - 1].itemValue) * 20;
					this.fuel[this.fuel.Length - 1].count--;
					if (this.fuel[this.fuel.Length - 1].count == 0)
					{
						this.fuel[this.fuel.Length - 1].Clear();
					}
				}
			}
			this.updateLightState(world);
			flag = true;
		}
		flag |= this.moveDown(this.fuel);
		if (this.outputWeight > 0)
		{
			int num3 = (int)((float)num * 0.1f);
			while (this.metalInForge < num3 && this.inputMetal >= num3 - this.metalInForge)
			{
				flag |= this.moveDown(this.input);
				if (!this.input[this.input.Length - 1].IsEmpty())
				{
					this.metalInForge += ItemClass.GetForId(this.input[this.input.Length - 1].itemValue.type).GetWeight();
					this.input[this.input.Length - 1].count--;
					if (this.input[this.input.Length - 1].count == 0)
					{
						this.input[this.input.Length - 1].Clear();
					}
					flag = true;
				}
				this.recalcStats();
			}
			if (this.metalInForge > 0)
			{
				num3 = Utils.FastMin(this.metalInForge, num3);
				this.metalInForge -= num3;
				this.moldedMetalSoFar += num3;
				flag = true;
			}
			bool flag2 = false;
			while (this.moldedMetalSoFar >= this.outputWeight)
			{
				this.moldedMetalSoFar -= this.outputWeight;
				this.output = new ItemStack(this.outputItem, this.output.count + 1);
				flag2 = true;
				flag = true;
			}
			if (flag2)
			{
				world.GetGameManager().PlaySoundAtPositionServer(base.ToWorldPos().ToVector3(), "Forge/forge_item_complete", AudioRolloffMode.Logarithmic, 100);
			}
		}
		if (flag)
		{
			this.setModified();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLightState(World world)
	{
		BlockValue block = world.GetBlock(base.ToWorldPos());
		if (this.fuelInStorageInTicks + this.fuelInForgeInTicks == 0 && block.meta != 0)
		{
			block.meta = 0;
			world.SetBlockRPC(base.ToWorldPos(), block);
			return;
		}
		if (this.fuelInStorageInTicks + this.fuelInForgeInTicks != 0 && block.meta == 0)
		{
			block.meta = 1;
			world.SetBlockRPC(base.ToWorldPos(), block);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool moveDown(ItemStack[] _items)
	{
		if (_items.Length < 2)
		{
			return false;
		}
		bool result = false;
		for (int i = _items.Length - 1; i > 0; i--)
		{
			if (_items[i].IsEmpty() && !_items[i - 1].IsEmpty())
			{
				_items[i] = _items[i - 1].Clone();
				_items[i - 1].Clear();
				result = true;
			}
		}
		return result;
	}

	public int GetFuelLeft(ulong _worldTimeInTicks)
	{
		if (_worldTimeInTicks == 0UL || this.lastTickTimeDataCalculated == 0UL)
		{
			return this.fuelInForgeInTicks / 20;
		}
		float num = (_worldTimeInTicks - this.lastTickTimeDataCalculated) * 1f;
		return (int)Math.Max((float)this.fuelInForgeInTicks - num, 0f) / 20;
	}

	public override bool IsActive(World world)
	{
		return world.GetBlock(base.ToWorldPos()).meta > 0;
	}

	public int GetInputWeight()
	{
		return this.inputMetal;
	}

	public int GetFuelInStorage()
	{
		return this.fuelInStorageInTicks / 20;
	}

	public int GetMetalForgedSoFar(ulong _currentTickTime)
	{
		if (_currentTickTime == 0UL || this.lastTickTimeDataCalculated == 0UL || !this.CanOperate(_currentTickTime))
		{
			return this.moldedMetalSoFar;
		}
		int num = (int)((_currentTickTime - this.lastTickTimeDataCalculated) * 0.1f);
		if (num < this.metalInForge)
		{
			return Math.Min(this.moldedMetalSoFar + num, this.moldedMetalSoFar + this.metalInForge);
		}
		return Math.Min(this.moldedMetalSoFar + num, this.outputWeight);
	}

	public int GetOutputWeight()
	{
		return this.outputWeight;
	}

	public int GetCurrentMetalInForge(ulong _currentTickTime)
	{
		if (_currentTickTime == 0UL || this.lastTickTimeDataCalculated == 0UL || !this.CanOperate(_currentTickTime))
		{
			return this.metalInForge;
		}
		int num = (int)((_currentTickTime - this.lastTickTimeDataCalculated) * 0.1f);
		return Math.Max(this.metalInForge - num, 0);
	}

	public float GetMoldTimeNeeded(ulong _currentTickTime)
	{
		return (float)(this.GetInputWeight() + this.GetCurrentMetalInForge(_currentTickTime)) / 2f;
	}

	public ItemStack[] GetFuel()
	{
		return this.fuel;
	}

	public void SetFuel(ItemStack[] _fuel)
	{
		this.fuel = ItemStack.Clone(_fuel);
		this.setModified();
	}

	public ItemStack[] GetInput()
	{
		return this.input;
	}

	public void SetInput(ItemStack[] _input, bool _bSetModified = true)
	{
		this.input = ItemStack.Clone(_input);
		if (_bSetModified)
		{
			this.setModified();
		}
	}

	public ItemStack GetMold()
	{
		return this.mold;
	}

	public void SetMold(ItemStack _mold)
	{
		this.mold = _mold;
		this.moldedMetalSoFar = 0;
		this.metalInForge = 0;
		this.setModified();
	}

	public ItemStack GetOutput()
	{
		return this.output;
	}

	public ItemValue GetBurningItemValue()
	{
		return this.burningItemValue;
	}

	public void SetOutput(ItemStack _output, bool _bSetModified = true)
	{
		this.output = _output;
		if (_bSetModified)
		{
			this.setModified();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setModified()
	{
		this.recalcStats();
		base.setModified();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void recalcStats()
	{
		this.outputWeight = 0;
		this.outputItem = ItemValue.None.Clone();
		if (!this.mold.IsEmpty())
		{
			this.outputItem = new ItemValue(ItemClass.GetForId(this.mold.itemValue.type).MoldTarget.Id, false);
			this.outputWeight = ItemClass.GetForId(this.outputItem.type).GetWeight();
		}
		this.fuelInStorageInTicks = 0;
		for (int i = 0; i < this.fuel.Length; i++)
		{
			if (!this.fuel[i].IsEmpty())
			{
				this.fuelInStorageInTicks += ItemClass.GetFuelValue(this.fuel[i].itemValue) * this.fuel[i].count * 20;
			}
		}
		this.inputMetal = 0;
		for (int j = 0; j < this.input.Length; j++)
		{
			ItemClass forId = ItemClass.GetForId(this.input[j].itemValue.type);
			if (forId != null)
			{
				this.inputMetal += forId.GetWeight() * this.input[j].count;
			}
		}
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency)
		{
			this.lastTickTime = _br.ReadUInt64();
			this.lastTickTimeDataCalculated = GameTimer.Instance.ticks;
			int num = (int)_br.ReadByte();
			if (this.fuel == null || this.fuel.Length != num)
			{
				this.fuel = ItemStack.CreateArray(num);
			}
			if (this.readVersion < 3)
			{
				for (int i = 0; i < num; i++)
				{
					this.fuel[i].ReadOld(_br);
				}
			}
			else
			{
				for (int j = 0; j < num; j++)
				{
					this.fuel[j].Read(_br);
				}
			}
			int num2 = (int)_br.ReadByte();
			if (this.input == null || this.input.Length != num2)
			{
				this.input = ItemStack.CreateArray(num2);
			}
			if (this.readVersion < 3)
			{
				for (int k = 0; k < num2; k++)
				{
					this.input[k].Read(_br);
				}
			}
			else
			{
				for (int l = 0; l < num2; l++)
				{
					this.input[l].Read(_br);
				}
			}
			if (this.readVersion < 3)
			{
				this.mold.ReadOld(_br);
				this.output.ReadOld(_br);
			}
			else
			{
				this.mold.Read(_br);
				this.output.Read(_br);
			}
			this.fuelInForgeInTicks = _br.ReadInt32();
			this.moldedMetalSoFar = (int)_br.ReadInt16();
			this.metalInForge = (int)_br.ReadInt16();
			this.burningItemValue.Read(_br);
		}
		else if (_eStreamMode == TileEntity.StreamModeRead.FromClient)
		{
			this.lastTickTimeDataCalculated = GameTimer.Instance.ticks;
			int num3 = (int)_br.ReadByte();
			if (this.fuel == null || this.fuel.Length != num3)
			{
				this.fuel = ItemStack.CreateArray(num3);
			}
			for (int m = 0; m < num3; m++)
			{
				this.fuel[m].ReadDelta(_br, this.fuel[m]);
			}
			int num4 = (int)_br.ReadByte();
			if (this.input == null || this.input.Length != num4)
			{
				this.input = ItemStack.CreateArray(num4);
			}
			for (int n = 0; n < num4; n++)
			{
				this.input[n].ReadDelta(_br, this.input[n]);
			}
			this.mold.ReadDelta(_br, this.mold);
			this.output.ReadDelta(_br, this.output);
			if (this.mold.itemValue.type == 0)
			{
				this.moldedMetalSoFar = 0;
				this.metalInForge = 0;
			}
		}
		else if (_eStreamMode == TileEntity.StreamModeRead.FromServer)
		{
			if (base.bWaitingForServerResponse)
			{
				Log.Warning("Throwing away server packet as we are waiting for status update!");
			}
			this.lastTickTimeDataCalculated = GameTimer.Instance.ticks;
			int num5 = (int)_br.ReadByte();
			if (this.fuel == null || this.fuel.Length != num5)
			{
				this.fuel = ItemStack.CreateArray(num5);
			}
			if (!base.bWaitingForServerResponse)
			{
				for (int num6 = 0; num6 < num5; num6++)
				{
					this.fuel[num6].Read(_br);
				}
				this.lastServerFuel = ItemStack.Clone(this.fuel);
			}
			else
			{
				ItemStack itemStack = ItemStack.Empty.Clone();
				for (int num7 = 0; num7 < num5; num7++)
				{
					itemStack.Read(_br);
				}
			}
			int num8 = (int)_br.ReadByte();
			if (this.input == null || this.input.Length != num8)
			{
				this.input = ItemStack.CreateArray(num8);
			}
			if (!base.bWaitingForServerResponse)
			{
				for (int num9 = 0; num9 < num8; num9++)
				{
					this.input[num9].Read(_br);
				}
				this.lastServerInput = ItemStack.Clone(this.input);
			}
			else
			{
				ItemStack itemStack2 = ItemStack.Empty.Clone();
				for (int num10 = 0; num10 < num8; num10++)
				{
					itemStack2.Read(_br);
				}
			}
			if (!base.bWaitingForServerResponse)
			{
				this.mold.Read(_br);
				this.lastServerMold = this.mold.Clone();
			}
			else
			{
				ItemStack.Empty.Clone().Read(_br);
			}
			if (!base.bWaitingForServerResponse)
			{
				this.output.Read(_br);
				this.lastServerOutput = this.output.Clone();
			}
			else
			{
				ItemStack.Empty.Clone().Read(_br);
			}
			this.fuelInForgeInTicks = _br.ReadInt32();
			this.moldedMetalSoFar = (int)_br.ReadInt16();
			this.metalInForge = (int)_br.ReadInt16();
			this.burningItemValue.Read(_br);
		}
		this.recalcStats();
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(this.lastTickTime);
			_bw.Write((byte)this.fuel.Length);
			for (int i = 0; i < this.fuel.Length; i++)
			{
				this.fuel[i].Write(_bw);
			}
			_bw.Write((byte)this.input.Length);
			for (int j = 0; j < this.input.Length; j++)
			{
				this.input[j].Write(_bw);
			}
			this.mold.Write(_bw);
			this.output.Write(_bw);
			_bw.Write(this.fuelInForgeInTicks);
			_bw.Write((short)this.moldedMetalSoFar);
			_bw.Write((short)this.metalInForge);
			this.burningItemValue.Write(_bw);
			return;
		}
		if (_eStreamMode == TileEntity.StreamModeWrite.ToServer)
		{
			_bw.Write((byte)this.fuel.Length);
			for (int k = 0; k < this.fuel.Length; k++)
			{
				this.fuel[k].WriteDelta(_bw, (this.lastServerFuel != null) ? this.lastServerFuel[k] : ItemStack.Empty.Clone());
			}
			_bw.Write((byte)this.input.Length);
			for (int l = 0; l < this.input.Length; l++)
			{
				this.input[l].WriteDelta(_bw, (this.lastServerInput != null) ? this.lastServerInput[l] : ItemStack.Empty.Clone());
			}
			this.mold.WriteDelta(_bw, this.lastServerMold);
			this.output.WriteDelta(_bw, this.lastServerOutput);
			return;
		}
		if (_eStreamMode == TileEntity.StreamModeWrite.ToClient)
		{
			_bw.Write((byte)this.fuel.Length);
			for (int m = 0; m < this.fuel.Length; m++)
			{
				this.fuel[m].Write(_bw);
			}
			_bw.Write((byte)this.input.Length);
			for (int n = 0; n < this.input.Length; n++)
			{
				this.input[n].Write(_bw);
			}
			this.mold.Write(_bw);
			this.output.Write(_bw);
			_bw.Write(this.fuelInForgeInTicks);
			_bw.Write((short)this.moldedMetalSoFar);
			_bw.Write((short)this.metalInForge);
			this.burningItemValue.Write(_bw);
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Forge;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cFuelBurnPerTick = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMoldPerTick = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] fuel;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] input;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack mold;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack output;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fuelInForgeInTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int moldedMetalSoFar;

	[PublicizedFrom(EAccessModifier.Private)]
	public int metalInForge;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue burningItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lastTickTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lastTickTimeDataCalculated;

	[PublicizedFrom(EAccessModifier.Private)]
	public int inputMetal;

	[PublicizedFrom(EAccessModifier.Private)]
	public int outputWeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fuelInStorageInTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue outputItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] lastServerFuel;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] lastServerInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack lastServerMold = ItemStack.Empty.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack lastServerOutput = ItemStack.Empty.Clone();
}
