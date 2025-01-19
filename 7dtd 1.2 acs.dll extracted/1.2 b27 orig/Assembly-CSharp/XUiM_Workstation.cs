using System;

public class XUiM_Workstation : XUiModel
{
	public TileEntityWorkstation TileEntity
	{
		get
		{
			return this.tileEntity;
		}
	}

	public XUiM_Workstation(TileEntityWorkstation _te)
	{
		this.tileEntity = _te;
	}

	public bool GetIsBurning()
	{
		return this.tileEntity.IsBurning;
	}

	public bool GetIsBesideWater()
	{
		return this.tileEntity.IsBesideWater;
	}

	public void SetIsBurning(bool _isBurning)
	{
		this.tileEntity.IsBurning = _isBurning;
		this.tileEntity.ResetTickTime();
	}

	public ItemStack[] GetInputStacks()
	{
		return this.tileEntity.Input;
	}

	public void SetInputStacks(ItemStack[] itemStacks)
	{
		this.tileEntity.Input = itemStacks;
	}

	public void SetInputInSlot(int idx, ItemStack itemStack)
	{
		this.tileEntity.Input[idx] = itemStack.Clone();
	}

	public ItemStack[] GetOutputStacks()
	{
		return this.tileEntity.Output;
	}

	public void SetOutputStacks(ItemStack[] itemStacks)
	{
		this.tileEntity.Output = itemStacks;
	}

	public void SetOutputInSlot(int idx, ItemStack itemStack)
	{
		this.tileEntity.Output[idx] = itemStack.Clone();
	}

	public ItemStack[] GetToolStacks()
	{
		return this.tileEntity.Tools;
	}

	public void SetToolStacks(ItemStack[] itemStacks)
	{
		this.tileEntity.Tools = itemStacks;
	}

	public void SetToolInSlot(int idx, ItemStack itemStack)
	{
		this.tileEntity.Tools[idx] = itemStack.Clone();
	}

	public ItemStack[] GetFuelStacks()
	{
		return this.tileEntity.Fuel;
	}

	public void SetFuelStacks(ItemStack[] itemStacks)
	{
		this.tileEntity.Fuel = itemStacks;
	}

	public void SetFuelInSlot(int idx, ItemStack itemStack)
	{
		this.tileEntity.Fuel[idx] = itemStack.Clone();
	}

	public float GetBurnTimeLeft()
	{
		if (this.tileEntity.BurnTimeLeft == 0f)
		{
			return 0f;
		}
		return this.tileEntity.BurnTimeLeft + 0.5f;
	}

	public float GetTotalBurnTimeLeft()
	{
		if (this.tileEntity.BurnTotalTimeLeft == 0f)
		{
			return 0f;
		}
		return this.tileEntity.BurnTotalTimeLeft + 0.5f;
	}

	public RecipeQueueItem[] GetRecipeQueueItems()
	{
		return this.tileEntity.Queue;
	}

	public void SetRecipeQueueItems(RecipeQueueItem[] queueStacks)
	{
		this.tileEntity.Queue = queueStacks;
	}

	public void SetQueueInSlot(int idx, RecipeQueueItem queueStack)
	{
		this.tileEntity.Queue[idx] = queueStack;
	}

	public void SetUserAccessing(bool isUserAccessing)
	{
		this.tileEntity.SetUserAccessing(isUserAccessing);
	}

	public string[] GetMaterialNames()
	{
		return this.tileEntity.MaterialNames;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityWorkstation tileEntity;
}
