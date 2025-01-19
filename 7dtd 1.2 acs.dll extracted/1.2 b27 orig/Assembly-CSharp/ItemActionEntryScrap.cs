using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryScrap : BaseItemActionEntry
{
	public ItemActionEntryScrap(XUiController controller) : base(controller, "lblContextActionScrap", "ui_game_symbol_scrap", BaseItemActionEntry.GamepadShortCut.DPadRight, "crafting/craft_click_craft", "ui/ui_denied")
	{
		this.lblQueueFull = Localization.Get("xuiCraftQueueFull", false);
	}

	public override void OnActivated()
	{
		XUi xui = base.ItemController.xui;
		XUiC_ItemStack xuiC_ItemStack = (XUiC_ItemStack)base.ItemController;
		xuiC_ItemStack.LockChangedEvent += this.ItemStackController_LockChangedEvent;
		ItemStack itemStack = xuiC_ItemStack.ItemStack.Clone();
		Recipe scrapableRecipe = CraftingManager.GetScrapableRecipe(itemStack.itemValue, itemStack.count);
		if (scrapableRecipe == null)
		{
			return;
		}
		this.recipe = scrapableRecipe;
		XUiController xuiController = base.ItemController.xui.FindWindowGroupByName("workstation_workbench");
		if (xuiController == null || !xuiController.WindowGroup.isShowing)
		{
			xuiController = xui.FindWindowGroupByName("crafting");
		}
		XUiC_CraftingWindowGroup childByType = xuiController.GetChildByType<XUiC_CraftingWindowGroup>();
		if (childByType == null)
		{
			return;
		}
		ItemClass forId = ItemClass.GetForId(this.recipe.itemValueType);
		ItemClass forId2 = ItemClass.GetForId(itemStack.itemValue.type);
		this.craftComponentTime = (float)((int)forId.CraftComponentTime);
		int num = forId2.GetWeight() * itemStack.count;
		int weight = forId.GetWeight();
		int num2 = num / weight;
		if (num2 == 0)
		{
			return;
		}
		int num3 = (int)((float)(num2 * weight) / (float)forId2.GetWeight() + 0.5f);
		if (childByType != null && num2 > 0)
		{
			Recipe recipe = new Recipe();
			num2 = (int)((float)num2 * 0.75f);
			if (num2 <= 0)
			{
				num2 = 1;
			}
			recipe.count = num2;
			recipe.craftExpGain = this.recipe.craftExpGain;
			recipe.ingredients.Add(new ItemStack(itemStack.itemValue, num3));
			recipe.itemValueType = forId.Id;
			recipe.craftingTime = ((forId2.ScrapTimeOverride > 0f) ? forId2.ScrapTimeOverride : EffectManager.GetValue(PassiveEffects.ScrappingTime, null, forId.CraftComponentTime * (float)num2, xui.playerUI.entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
			recipe.scrapable = true;
			recipe.IsScrap = true;
			if (!childByType.AddItemToQueue(recipe, 1))
			{
				this.WarnQueueFull();
				return;
			}
			itemStack.count -= num3;
			itemStack = this.HandleRemoveAmmo(itemStack);
			((XUiC_ItemStack)base.ItemController).ItemStack = ((itemStack.count <= 0) ? ItemStack.Empty.Clone() : itemStack.Clone());
			((XUiC_ItemStack)base.ItemController).WindowGroup.Controller.SetAllChildrenDirty(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WarnQueueFull()
	{
		GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, this.lblQueueFull, false);
		Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ItemStackController_TimeIntervalElapsedEvent(float timeLeft, XUiC_ItemStack _uiItemStack)
	{
		EntityPlayerLocal entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		XUiM_PlayerInventory playerInventory = base.ItemController.xui.PlayerInventory;
		XUiC_ItemStack xuiC_ItemStack = (XUiC_ItemStack)base.ItemController;
		ItemStack itemStack = xuiC_ItemStack.ItemStack.Clone();
		itemStack.count -= (int)this.numberOfCurrentItemsNeededFor1StackOfOutputItem;
		xuiC_ItemStack.ItemStack = itemStack;
		ItemStack itemStack2 = new ItemStack(new ItemValue(this.recipe.itemValueType, false), (int)this.scrapItemCount);
		if (!playerInventory.AddItem(itemStack2))
		{
			GameManager.Instance.ItemDropServer(itemStack2, entityPlayer.GetPosition(), new Vector3(0.5f, 0f, 0.5f), -1, 60f, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ItemStackController_LockChangedEvent(XUiC_ItemStack.LockTypes lockType, XUiC_ItemStack _uiItemStack)
	{
		XUiC_ItemStack xuiC_ItemStack = (XUiC_ItemStack)base.ItemController;
		if (xuiC_ItemStack.ItemStack.count == 0)
		{
			xuiC_ItemStack.ItemStack = ItemStack.Empty.Clone();
		}
		xuiC_ItemStack.TimeIntervalElapsedEvent -= this.ItemStackController_TimeIntervalElapsedEvent;
		xuiC_ItemStack.UnlockStack();
		xuiC_ItemStack.LockChangedEvent -= this.ItemStackController_LockChangedEvent;
	}

	public override void RefreshEnabled()
	{
		ItemStack itemStack = ((XUiC_ItemStack)base.ItemController).ItemStack;
		if (itemStack.itemValue.Modifications.Length != 0)
		{
			for (int i = 0; i < itemStack.itemValue.Modifications.Length; i++)
			{
				ItemValue itemValue = itemStack.itemValue.Modifications[i];
				if (itemValue != null && !itemValue.IsEmpty() && (itemValue.ItemClass as ItemClassModifier).Type == ItemClassModifier.ModifierTypes.Attachment)
				{
					base.Enabled = false;
					return;
				}
			}
		}
		base.Enabled = true;
	}

	public override void OnDisabledActivate()
	{
		GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("ttCannotScrapWithAttachments", false), false);
	}

	public ItemStack HandleRemoveAmmo(ItemStack stack)
	{
		if (stack.itemValue.Meta > 0)
		{
			ItemClass forId = ItemClass.GetForId(stack.itemValue.type);
			for (int i = 0; i < forId.Actions.Length; i++)
			{
				ItemActionRanged itemActionRanged = forId.Actions[i] as ItemActionRanged;
				if (itemActionRanged != null && !(forId.Actions[i] is ItemActionTextureBlock) && itemActionRanged.MagazineItemNames != null && (int)stack.itemValue.SelectedAmmoTypeIndex < itemActionRanged.MagazineItemNames.Length)
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
		return stack;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblQueueFull;

	[PublicizedFrom(EAccessModifier.Private)]
	public float scrapItemCount = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float craftComponentTime = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float numberOfCurrentItemsNeededFor1StackOfOutputItem = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentInterval;
}
