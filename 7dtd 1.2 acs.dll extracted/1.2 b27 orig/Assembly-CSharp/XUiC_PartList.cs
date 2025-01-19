using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PartList : XUiC_ItemStackGrid
{
	public override void Init()
	{
		base.Init();
		XUiC_ItemStack[] itemControllers = this.itemControllers;
		for (int i = 0; i < itemControllers.Length; i++)
		{
			itemControllers[i].ViewComponent.IsNavigatable = false;
		}
	}

	public override ItemStack[] GetSlots()
	{
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
	}

	public void SetSlot(ItemValue part, int index)
	{
		if (part != null && !part.IsEmpty())
		{
			ItemStack itemStack = new ItemStack(part.Clone(), 1);
			this.itemControllers[index].ItemStack = itemStack;
			this.itemControllers[index].GreyedOut = false;
		}
		else
		{
			this.itemControllers[index].ItemStack = ItemStack.Empty.Clone();
			this.itemControllers[index].GreyedOut = false;
		}
		this.itemControllers[index].ViewComponent.EventOnPress = false;
		this.itemControllers[index].ViewComponent.EventOnHover = false;
	}

	public void SetSlots(ItemValue[] parts, int startIndex = 0)
	{
		for (int i = 0; i < this.itemControllers.Length - startIndex; i++)
		{
			int num = i + startIndex;
			if (parts.Length > i && parts[i] != null && !parts[i].IsEmpty())
			{
				ItemStack itemStack = new ItemStack(parts[i].Clone(), 1);
				this.itemControllers[num].ItemStack = itemStack;
				this.itemControllers[num].GreyedOut = false;
			}
			else
			{
				this.itemControllers[num].ItemStack = ItemStack.Empty.Clone();
				this.itemControllers[num].GreyedOut = false;
			}
			this.itemControllers[num].ViewComponent.EventOnPress = false;
			this.itemControllers[num].ViewComponent.EventOnHover = false;
		}
	}

	public void SetAmmoSlot(ItemValue ammo, int count)
	{
		int num = 5;
		int count2 = (count < 1) ? 1 : count;
		if (ammo != null && !ammo.IsEmpty())
		{
			ItemStack itemStack = new ItemStack(ammo, count2);
			this.itemControllers[num].ItemStack = itemStack;
			this.itemControllers[num].GreyedOut = (count == 0);
			return;
		}
		this.itemControllers[num].ItemStack = ItemStack.Empty.Clone();
		this.itemControllers[num].GreyedOut = false;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetMainItem(ItemStack itemStack)
	{
		this.itemClass = itemStack.itemValue.ItemClass;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;
}
