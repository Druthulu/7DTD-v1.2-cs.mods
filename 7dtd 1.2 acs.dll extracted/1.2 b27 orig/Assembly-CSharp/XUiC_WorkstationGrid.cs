using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationGrid : XUiC_ItemStackGrid
{
	public XUiM_Workstation WorkstationData
	{
		get
		{
			return this.workstationData;
		}
	}

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Workstation;
		}
	}

	public virtual void SetSlots(ItemStack[] stacks)
	{
		base.SetStacks(stacks);
	}

	public virtual bool HasRequirement(Recipe recipe)
	{
		return true;
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		this.workstationData = ((XUiC_WorkstationWindowGroup)this.windowGroup.Controller).WorkstationData;
		this.IsDirty = true;
		this.IsDormant = false;
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnClose();
			base.ViewComponent.IsVisible = false;
		}
		this.IsDirty = true;
		this.IsDormant = true;
	}

	public int AddToItemStackArray(ItemStack _itemStack)
	{
		ItemStack[] slots = this.GetSlots();
		int num = -1;
		int num2 = 0;
		while (num == -1 && num2 < slots.Length)
		{
			if (slots[num2].CanStackWith(_itemStack, false))
			{
				slots[num2].count += _itemStack.count;
				_itemStack.count = 0;
				num = num2;
			}
			num2++;
		}
		int num3 = 0;
		while (num == -1 && num3 < slots.Length)
		{
			if (slots[num3].IsEmpty())
			{
				slots[num3] = _itemStack;
				num = num3;
			}
			num3++;
		}
		if (num != -1)
		{
			this.SetSlots(slots);
			this.UpdateBackend(slots);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiM_Workstation workstationData;
}
