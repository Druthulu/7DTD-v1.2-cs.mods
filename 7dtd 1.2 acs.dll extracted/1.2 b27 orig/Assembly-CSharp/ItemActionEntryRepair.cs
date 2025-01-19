using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryRepair : BaseItemActionEntry
{
	public ItemActionEntryRepair(XUiController controller) : base(controller, "lblContextActionRepair", "ui_game_symbol_wrench", BaseItemActionEntry.GamepadShortCut.DPadLeft, "crafting/craft_click_craft", "ui/ui_denied")
	{
		this.lblReadBook = Localization.Get("xuiRepairMustReadBook", false);
		this.lblNeedMaterials = Localization.Get("xuiRepairMissingMats", false);
		controller.xui.PlayerInventory.OnBackpackItemsChanged += this.PlayerInventory_OnBackpackItemsChanged;
		controller.xui.PlayerInventory.OnToolbeltItemsChanged += this.PlayerInventory_OnToolbeltItemsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		this.RefreshEnabled();
		if (base.ParentItem != null)
		{
			base.ParentItem.MarkDirty();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		this.RefreshEnabled();
		if (base.ParentItem != null)
		{
			base.ParentItem.MarkDirty();
		}
	}

	public override void OnDisabledActivate()
	{
		ItemActionEntryRepair.StateTypes stateTypes = this.state;
		if (stateTypes == ItemActionEntryRepair.StateTypes.RecipeLocked)
		{
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, this.lblReadBook, false);
			return;
		}
		if (stateTypes != ItemActionEntryRepair.StateTypes.NotEnoughMaterials)
		{
			return;
		}
		GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, this.lblNeedMaterials, false);
		ItemClass forId = ItemClass.GetForId(((XUiC_ItemStack)base.ItemController).ItemStack.itemValue.type);
		if (forId.RepairTools != null && forId.RepairTools.Length > 0)
		{
			ItemClass itemClass = ItemClass.GetItemClass(forId.RepairTools[0].Value, false);
			if (itemClass != null)
			{
				ItemStack @is = new ItemStack(new ItemValue(itemClass.Id, false), 0);
				base.ItemController.xui.playerUI.entityPlayer.AddUIHarvestingItem(@is, true);
			}
		}
	}

	public override void RefreshEnabled()
	{
		base.RefreshEnabled();
		this.state = ItemActionEntryRepair.StateTypes.Normal;
		XUi xui = base.ItemController.xui;
		if (((XUiC_ItemStack)base.ItemController).ItemStack.IsEmpty() || ((XUiC_ItemStack)base.ItemController).StackLock)
		{
			return;
		}
		ItemClass forId = ItemClass.GetForId(((XUiC_ItemStack)base.ItemController).ItemStack.itemValue.type);
		base.Enabled = (this.state == ItemActionEntryRepair.StateTypes.Normal);
		if (!base.Enabled)
		{
			base.IconName = "ui_game_symbol_book";
			return;
		}
		ItemValue itemValue = ((XUiC_ItemStack)base.ItemController).ItemStack.itemValue;
		if (forId.RepairTools != null && forId.RepairTools.Length > 0)
		{
			ItemClass itemClass = ItemClass.GetItemClass(forId.RepairTools[0].Value, false);
			if (itemClass != null)
			{
				int b = Convert.ToInt32(Math.Ceiling((double)((float)Mathf.CeilToInt(itemValue.UseTimes) / (float)itemClass.RepairAmount.Value)));
				if (Mathf.Min(xui.PlayerInventory.GetItemCount(new ItemValue(itemClass.Id, false)), b) * itemClass.RepairAmount.Value <= 0)
				{
					this.state = ItemActionEntryRepair.StateTypes.NotEnoughMaterials;
					base.Enabled = (this.state == ItemActionEntryRepair.StateTypes.Normal);
				}
			}
		}
	}

	public override void OnActivated()
	{
		XUi xui = base.ItemController.xui;
		XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
		((XUiC_ItemStack)base.ItemController).TimeIntervalElapsedEvent += this.ItemActionEntryRepair_TimeIntervalElapsedEvent;
		XUiC_ItemStack xuiC_ItemStack = (XUiC_ItemStack)base.ItemController;
		ItemValue itemValue = xuiC_ItemStack.ItemStack.itemValue;
		ItemClass forId = ItemClass.GetForId(itemValue.type);
		int sourceToolbeltSlot = (xuiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt) ? xuiC_ItemStack.SlotNumber : -1;
		if (forId.RepairTools != null && forId.RepairTools.Length > 0)
		{
			ItemClass itemClass = ItemClass.GetItemClass(forId.RepairTools[0].Value, false);
			if (itemClass == null)
			{
				return;
			}
			int b = Convert.ToInt32(Math.Ceiling((double)((float)Mathf.CeilToInt(itemValue.UseTimes) / (float)itemClass.RepairAmount.Value)));
			int num = Mathf.Min(playerInventory.GetItemCount(new ItemValue(itemClass.Id, false)), b);
			int num2 = num * itemClass.RepairAmount.Value;
			XUiC_CraftingWindowGroup childByType = xui.FindWindowGroupByName("crafting").GetChildByType<XUiC_CraftingWindowGroup>();
			if (childByType != null && num2 > 0)
			{
				Recipe recipe = new Recipe();
				recipe.count = 1;
				recipe.craftExpGain = Mathf.CeilToInt(forId.RepairExpMultiplier * (float)num);
				recipe.ingredients.Add(new ItemStack(new ItemValue(itemClass.Id, false), num));
				recipe.itemValueType = itemValue.type;
				recipe.craftingTime = itemClass.RepairTime.Value * (float)num;
				num2 = (int)EffectManager.GetValue(PassiveEffects.RepairAmount, null, (float)num2, xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(recipe.GetName()), true, true, true, true, true, 1, true, false);
				recipe.craftingTime = (float)((int)EffectManager.GetValue(PassiveEffects.CraftingTime, null, recipe.craftingTime, xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(recipe.GetName()), true, true, true, true, true, 1, true, false));
				ItemClass.GetForId(recipe.itemValueType);
				if (!childByType.AddRepairItemToQueue(recipe.craftingTime, itemValue.Clone(), num2, sourceToolbeltSlot))
				{
					this.WarnQueueFull();
					return;
				}
				((XUiC_ItemStack)base.ItemController).ItemStack = ItemStack.Empty.Clone();
				playerInventory.RemoveItems(recipe.ingredients, 1, null);
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ItemActionEntryRepair_TimeIntervalElapsedEvent(float timeLeft, XUiC_ItemStack _uiItemStack)
	{
		if (timeLeft <= 0f)
		{
			ItemStack itemStack = _uiItemStack.ItemStack.Clone();
			itemStack.itemValue.UseTimes = Mathf.Max(0f, itemStack.itemValue.UseTimes - (float)_uiItemStack.RepairAmount);
			_uiItemStack.ItemStack = itemStack;
			_uiItemStack.TimeIntervalElapsedEvent -= this.ItemActionEntryRepair_TimeIntervalElapsedEvent;
			_uiItemStack.UnlockStack();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WarnQueueFull()
	{
		string text = "No room in queue!";
		if (Localization.Exists("wrnQueueFull", false))
		{
			text = Localization.Get("wrnQueueFull", false);
		}
		GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, text, false);
		Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
	}

	public override void DisableEvents()
	{
		base.ItemController.xui.PlayerInventory.OnBackpackItemsChanged -= this.PlayerInventory_OnBackpackItemsChanged;
		base.ItemController.xui.PlayerInventory.OnToolbeltItemsChanged -= this.PlayerInventory_OnToolbeltItemsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionEntryRepair.StateTypes state;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblReadBook;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblNeedMaterials;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum StateTypes
	{
		Normal,
		RecipeLocked,
		NotEnoughMaterials
	}
}
