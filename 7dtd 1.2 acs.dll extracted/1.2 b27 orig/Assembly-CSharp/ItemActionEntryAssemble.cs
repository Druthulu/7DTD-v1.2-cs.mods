using System;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryAssemble : BaseItemActionEntry
{
	public ItemActionEntryAssemble(XUiController controller) : base(controller, "lblContextActionModify", "ui_game_symbol_assemble", BaseItemActionEntry.GamepadShortCut.None, "crafting/craft_click_craft", "ui/ui_denied")
	{
		XUiC_ItemStack xuiC_ItemStack = base.ItemController as XUiC_ItemStack;
		this.lblMustReadBook = Localization.Get("xuiAssembleMustReadBook", false);
		if (xuiC_ItemStack != null)
		{
			if (xuiC_ItemStack.AssembleLock)
			{
				base.ActionName = Localization.Get("lblContextActionComplete", false);
				return;
			}
		}
		else if (base.ItemController is XUiC_EquipmentStack && base.ItemController.xui.AssembleItem.CurrentEquipmentStackController == base.ItemController)
		{
			base.ActionName = Localization.Get("lblContextActionComplete", false);
		}
	}

	public override void OnDisabledActivate()
	{
		this.setWindowsDirty();
		if (this.recipeUnknown)
		{
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, this.lblMustReadBook, false);
		}
	}

	public override void OnActivated()
	{
		this.setWindowsDirty();
		GUIWindowManager windowManager = base.ItemController.xui.playerUI.windowManager;
		XUiC_ItemStack xuiC_ItemStack = base.ItemController as XUiC_ItemStack;
		if (xuiC_ItemStack != null)
		{
			if (xuiC_ItemStack.AssembleLock)
			{
				base.ItemController.xui.playerUI.windowManager.CloseAllOpenWindows(null, false);
				return;
			}
			ItemStack itemStack = xuiC_ItemStack.ItemStack.Clone();
			itemStack = this.HandleRemoveAmmo(itemStack);
			xuiC_ItemStack.ForceSetItemStack(itemStack);
			base.ItemController.xui.AssembleItem.CurrentItem = itemStack;
			base.ItemController.xui.AssembleItem.CurrentItemStackController = xuiC_ItemStack;
			base.ItemController.xui.AssembleItem.CurrentEquipmentStackController = null;
			if (!windowManager.IsWindowOpen(XUiC_AssembleWindowGroup.ID))
			{
				windowManager.Open(XUiC_AssembleWindowGroup.ID, true, false, true);
				xuiC_ItemStack.InfoWindow.SetItemStack(xuiC_ItemStack, true);
				return;
			}
			XUiC_AssembleWindowGroup.GetWindowGroup(base.ItemController.xui).ItemStack = itemStack;
			xuiC_ItemStack.InfoWindow.SetItemStack(xuiC_ItemStack, true);
			return;
		}
		else
		{
			XUiC_EquipmentStack xuiC_EquipmentStack = base.ItemController as XUiC_EquipmentStack;
			if (xuiC_EquipmentStack == null)
			{
				Log.Error("Modify, neither ItemStack nor EquipmentStack");
				return;
			}
			ItemStack itemStack = xuiC_EquipmentStack.ItemStack.Clone();
			base.ItemController.xui.AssembleItem.CurrentItem = itemStack;
			base.ItemController.xui.AssembleItem.CurrentItemStackController = null;
			base.ItemController.xui.AssembleItem.CurrentEquipmentStackController = xuiC_EquipmentStack;
			if (!windowManager.IsWindowOpen(XUiC_AssembleWindowGroup.ID))
			{
				windowManager.Open(XUiC_AssembleWindowGroup.ID, true, false, true);
				xuiC_EquipmentStack.InfoWindow.SetItemStack(xuiC_EquipmentStack, true);
				return;
			}
			XUiC_AssembleWindowGroup.GetWindowGroup(base.ItemController.xui).ItemStack = itemStack;
			xuiC_EquipmentStack.InfoWindow.SetItemStack(xuiC_EquipmentStack, true);
			windowManager.Close(XUiC_AssembleWindowGroup.ID);
			return;
		}
	}

	public ItemStack HandleRemoveAmmo(ItemStack stack)
	{
		if (stack.itemValue.Meta > 0)
		{
			ItemClass forId = ItemClass.GetForId(stack.itemValue.type);
			for (int i = 0; i < forId.Actions.Length; i++)
			{
				if (forId.Actions[i] is ItemActionRanged)
				{
					ItemActionRanged itemActionRanged = (ItemActionRanged)forId.Actions[i];
					if (!itemActionRanged.InfiniteAmmo && (int)stack.itemValue.SelectedAmmoTypeIndex < itemActionRanged.MagazineItemNames.Length)
					{
						ItemStack itemStack = new ItemStack(ItemClass.GetItem(itemActionRanged.MagazineItemNames[(int)stack.itemValue.SelectedAmmoTypeIndex], false), stack.itemValue.Meta);
						if (!base.ItemController.xui.PlayerInventory.AddItem(itemStack))
						{
							base.ItemController.xui.PlayerInventory.DropItem(itemStack);
						}
						stack.itemValue.Meta = 0;
					}
				}
			}
		}
		return stack;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setWindowsDirty()
	{
		GUIWindowManager windowManager = base.ItemController.xui.playerUI.windowManager;
		((XUiWindowGroup)windowManager.GetWindow("toolbelt")).Controller.GetChildByType<XUiC_Toolbelt>().SetAllChildrenDirty(false);
		((XUiWindowGroup)windowManager.GetWindow("backpack")).Controller.GetChildByType<XUiC_Backpack>().SetAllChildrenDirty(false);
	}

	public override void RefreshEnabled()
	{
		this.setWindowsDirty();
		XUiC_ItemStack xuiC_ItemStack = base.ItemController as XUiC_ItemStack;
		ItemStack itemStack;
		if (xuiC_ItemStack != null)
		{
			itemStack = xuiC_ItemStack.ItemStack;
		}
		else
		{
			itemStack = (base.ItemController as XUiC_EquipmentStack).ItemStack;
		}
		if (itemStack.IsEmpty())
		{
			return;
		}
		ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
		if (forId.HasSubItems)
		{
			if (!XUiM_Recipes.GetRecipeIsUnlocked(base.ItemController.xui, forId.Name))
			{
				this.recipeUnknown = true;
			}
		}
		else if (forId.PartParentId != null)
		{
			for (int i = 0; i < forId.PartParentId.Count; i++)
			{
				if (XUiM_Recipes.GetRecipeIsUnlocked(base.ItemController.xui, ItemClass.GetForId(forId.PartParentId[i]).Name))
				{
					this.recipeUnknown = false;
					break;
				}
				this.recipeUnknown = true;
			}
		}
		EntityPlayerLocal entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		base.IconName = "ui_game_symbol_assemble";
		base.Enabled = entityPlayer.IsAimingGunPossible();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblMustReadBook;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool recipeUnknown;
}
