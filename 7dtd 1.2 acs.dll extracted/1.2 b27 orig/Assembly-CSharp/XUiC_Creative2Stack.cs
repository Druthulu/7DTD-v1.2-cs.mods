using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Creative2Stack : XUiC_ItemStack
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setItemStack(ItemStack _stack)
	{
		this.itemStack = _stack;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleDropOne()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleClickComplete()
	{
		this.lastClicked = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SwapItem()
	{
		base.xui.dragAndDrop.CurrentStack = this.itemStack.Clone();
		base.xui.dragAndDrop.PickUpType = base.StackLocation;
		base.PlayPickupSound(null);
		base.HandleSlotChangeEvent();
	}
}
