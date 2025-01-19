using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RequiredItemStack : XUiC_ItemStack
{
	public event XUiEvent_RequiredSlotFailedSwapEventHandler FailedSwap;

	public ItemClass RequiredItemClass
	{
		get
		{
			return this.requiredItemClass;
		}
		set
		{
			this.requiredItemClass = value;
			this.IsDirty = true;
		}
	}

	public override string ItemIcon
	{
		get
		{
			if (this.RequiredItemClass != null && this.itemStack.IsEmpty())
			{
				return this.RequiredItemClass.GetIconName();
			}
			return base.ItemIcon;
		}
	}

	public override string ItemIconColor
	{
		get
		{
			if (base.itemClass != null)
			{
				base.GreyedOut = false;
				return base.ItemIconColor;
			}
			if (this.requiredItemClass != null && !base.StackLock)
			{
				base.GreyedOut = true;
				return "255,255,255,255";
			}
			base.GreyedOut = false;
			return "255,255,255,0";
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanSwap(ItemStack stack)
	{
		if (this.TakeOnly && !stack.IsEmpty())
		{
			return false;
		}
		bool flag;
		if (this.RequiredType == XUiC_RequiredItemStack.RequiredTypes.ItemClass && this.RequiredItemClass != null && this.RequiredItemOnly)
		{
			flag = (stack.itemValue.ItemClass == this.RequiredItemClass);
		}
		else if (this.RequiredType == XUiC_RequiredItemStack.RequiredTypes.IsPart)
		{
			flag = (stack.itemValue.ItemClass.PartParentId != null);
		}
		else if (this.RequiredType == XUiC_RequiredItemStack.RequiredTypes.HasQuality)
		{
			flag = stack.itemValue.HasQuality;
		}
		else
		{
			flag = (this.RequiredType != XUiC_RequiredItemStack.RequiredTypes.HasQualityNoParts || (stack.itemValue.HasQuality && !stack.itemValue.ItemClass.HasSubItems));
		}
		if (!flag && this.FailedSwap != null)
		{
			this.FailedSwap(stack);
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleDropOne()
	{
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		if (!currentStack.IsEmpty())
		{
			int num = 1;
			if (this.itemStack.IsEmpty() && this.CanSwap(currentStack))
			{
				ItemStack itemStack = currentStack.Clone();
				itemStack.count = num;
				currentStack.count -= num;
				base.xui.dragAndDrop.CurrentStack = currentStack;
				base.xui.dragAndDrop.PickUpType = base.StackLocation;
				base.ItemStack = itemStack;
				base.PlayPlaceSound(null);
			}
		}
	}

	public XUiC_RequiredItemStack.RequiredTypes RequiredType;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass requiredItemClass;

	public bool RequiredItemOnly = true;

	public bool TakeOnly;

	public enum RequiredTypes
	{
		ItemClass,
		IsPart,
		HasQuality,
		HasQualityNoParts
	}
}
