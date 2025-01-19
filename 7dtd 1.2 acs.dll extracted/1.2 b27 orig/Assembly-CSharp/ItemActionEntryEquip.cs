﻿using System;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryEquip : BaseItemActionEntry
{
	public ItemActionEntryEquip(XUiController controller) : base(controller, "lblContextActionEquip", "ui_game_symbol_knife", BaseItemActionEntry.GamepadShortCut.DPadUp, "crafting/craft_click_craft", "ui/ui_denied")
	{
	}

	public override void RefreshEnabled()
	{
		XUi xui = base.ItemController.xui;
		EntityPlayerLocal entityPlayer = xui.playerUI.entityPlayer;
		Inventory toolbelt = xui.PlayerInventory.Toolbelt;
		base.Enabled = (!entityPlayer.AttachedToEntity && toolbelt.GetItem(toolbelt.DUMMY_SLOT_IDX).IsEmpty());
	}

	public override void OnActivated()
	{
		XUi xui = base.ItemController.xui;
		EntityPlayerLocal entityPlayer = xui.playerUI.entityPlayer;
		XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
		Inventory toolbelt = playerInventory.Toolbelt;
		int focusedItemIdx = playerInventory.Toolbelt.GetFocusedItemIdx();
		ItemStack itemStack = playerInventory.Toolbelt.GetItem(focusedItemIdx).Clone();
		ItemStack itemStack2 = ((XUiC_ItemStack)base.ItemController).ItemStack.Clone();
		if (xui.AssembleItem.CurrentItemStackController != null)
		{
			XUiC_ItemStack currentItemStackController = xui.AssembleItem.CurrentItemStackController;
			if (currentItemStackController.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt && currentItemStackController.SlotNumber == focusedItemIdx)
			{
				return;
			}
		}
		if (itemStack.Equals(itemStack2) || (!itemStack.IsEmpty() && itemStack.itemValue.ItemClass.IsQuestItem))
		{
			return;
		}
		toolbelt.SetItem(focusedItemIdx, new ItemStack(new ItemValue(ItemClass.GetItem("UselessThing", false).GetItemId(), false), 1));
		toolbelt.ClearPreferredItemInSlot(focusedItemIdx);
		toolbelt.OnUpdate();
		((XUiC_ItemStack)base.ItemController).ItemStack = ItemStack.Empty.Clone();
		if (!xui.PlayerInventory.AddItem(itemStack))
		{
			((XUiC_ItemStack)base.ItemController).ItemStack = itemStack;
		}
		toolbelt.SetItem(focusedItemIdx, itemStack2);
		toolbelt.OnUpdate();
		toolbelt.SetHoldingItemIdx(focusedItemIdx);
	}
}
