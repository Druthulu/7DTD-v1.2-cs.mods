using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryDrop : BaseItemActionEntry
{
	public ItemActionEntryDrop(XUiController controller) : base(controller, "lblContextActionDrop", "ui_game_symbol_drop", BaseItemActionEntry.GamepadShortCut.DPadDown, "crafting/craft_click_craft", "ui/ui_denied")
	{
	}

	public override void OnActivated()
	{
		GameManager instance = GameManager.Instance;
		if (instance)
		{
			LocalPlayerUI playerUI = base.ItemController.xui.playerUI;
			NGUIWindowManager nguiWindowManager = playerUI.nguiWindowManager;
			XUiC_ItemStack xuiC_ItemStack = (XUiC_ItemStack)base.ItemController;
			base.ItemController.xui.CollectedItemList.RemoveItemStack(xuiC_ItemStack.ItemStack);
			instance.ItemDropServer(xuiC_ItemStack.ItemStack, playerUI.entityPlayer.GetDropPosition(), Vector3.zero, playerUI.entityPlayer.entityId, 60f, false);
			playerUI.entityPlayer.PlayOneShot("itemdropped", false, false, false);
			xuiC_ItemStack.ItemStack = ItemStack.Empty.Clone();
		}
	}

	public override void RefreshEnabled()
	{
		XUiC_ItemStack xuiC_ItemStack = (XUiC_ItemStack)base.ItemController;
		base.Enabled = (!xuiC_ItemStack.ItemStack.IsEmpty() && xuiC_ItemStack.ItemStack.itemValue.ItemClass.CanDrop(null) && !xuiC_ItemStack.StackLock);
	}

	public override void OnDisabledActivate()
	{
		GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, "This item cannot be dropped.", false);
	}
}
