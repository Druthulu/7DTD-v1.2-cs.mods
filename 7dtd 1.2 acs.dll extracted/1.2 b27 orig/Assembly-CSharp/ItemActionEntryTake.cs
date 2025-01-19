using System;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryTake : BaseItemActionEntry
{
	public ItemActionEntryTake(XUiController controller) : base(controller, "lblContextActionTake", "ui_game_symbol_hand", BaseItemActionEntry.GamepadShortCut.DPadUp, "crafting/craft_click_craft", "ui/ui_denied")
	{
	}

	public override void RefreshEnabled()
	{
		ItemStack itemStack = this.getItemStack();
		if (itemStack == null || itemStack.itemValue.IsEmpty())
		{
			base.Enabled = false;
			return;
		}
		int num = base.ItemController.xui.PlayerInventory.CountAvailabileSpaceForItem(itemStack.itemValue);
		base.Enabled = (num >= 1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack getItemStack()
	{
		XUiController itemController = base.ItemController;
		XUiC_ItemStack xuiC_ItemStack = itemController as XUiC_ItemStack;
		ItemStack result;
		if (xuiC_ItemStack == null)
		{
			XUiC_EquipmentStack xuiC_EquipmentStack = itemController as XUiC_EquipmentStack;
			if (xuiC_EquipmentStack == null)
			{
				XUiC_BasePartStack xuiC_BasePartStack = itemController as XUiC_BasePartStack;
				if (xuiC_BasePartStack == null)
				{
					result = null;
				}
				else
				{
					result = xuiC_BasePartStack.ItemStack;
				}
			}
			else
			{
				result = xuiC_EquipmentStack.ItemStack;
			}
		}
		else
		{
			result = xuiC_ItemStack.ItemStack;
		}
		return result;
	}

	public override void OnActivated()
	{
		XUiC_ItemStack xuiC_ItemStack = base.ItemController as XUiC_ItemStack;
		if (xuiC_ItemStack != null)
		{
			xuiC_ItemStack.HandleMoveToPreferredLocation();
			return;
		}
		XUiC_EquipmentStack xuiC_EquipmentStack = base.ItemController as XUiC_EquipmentStack;
		if (xuiC_EquipmentStack != null)
		{
			xuiC_EquipmentStack.HandleMoveToPreferredLocation();
			return;
		}
		XUiC_BasePartStack xuiC_BasePartStack = base.ItemController as XUiC_BasePartStack;
		if (xuiC_BasePartStack != null)
		{
			xuiC_BasePartStack.HandleMoveToPreferredLocation();
		}
	}
}
