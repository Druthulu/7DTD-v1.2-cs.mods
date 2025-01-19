using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryShowPerk : BaseItemActionEntry
{
	public ItemActionEntryShowPerk(XUiController controller, RecipeUnlockData unlockData) : base(controller, "Perk", "ui_game_symbol_skill", BaseItemActionEntry.GamepadShortCut.DPadUp, "crafting/craft_click_craft", "ui/ui_denied")
	{
		this.UnlockData = unlockData;
		switch (unlockData.UnlockType)
		{
		case RecipeUnlockData.UnlockTypes.Perk:
			base.ActionName = Localization.Get("xuiPerk", false);
			base.IconName = "ui_game_symbol_skills";
			return;
		case RecipeUnlockData.UnlockTypes.Book:
			base.ActionName = Localization.Get("xuiBook", false);
			base.IconName = "ui_game_symbol_book";
			return;
		case RecipeUnlockData.UnlockTypes.Skill:
			base.ActionName = Localization.Get("RewardSkill_keyword", false);
			base.IconName = "ui_game_symbol_hammer";
			return;
		case RecipeUnlockData.UnlockTypes.Schematic:
			base.ActionName = Localization.Get("xuiItem", false);
			base.IconName = "ui_game_symbol_hammer";
			return;
		default:
			return;
		}
	}

	public override void RefreshEnabled()
	{
		base.Enabled = true;
	}

	public override void OnActivated()
	{
		XUi xui = base.ItemController.xui;
		xui.playerUI.windowManager.CloseIfOpen("looting");
		List<XUiC_SkillList> childrenByType = xui.GetChildrenByType<XUiC_SkillList>();
		XUiC_SkillList xuiC_SkillList = null;
		for (int i = 0; i < childrenByType.Count; i++)
		{
			if (childrenByType[i].WindowGroup != null && childrenByType[i].WindowGroup.isShowing)
			{
				xuiC_SkillList = childrenByType[i];
				break;
			}
		}
		if (xuiC_SkillList == null)
		{
			XUiC_WindowSelector.OpenSelectorAndWindow(xui.playerUI.entityPlayer, "skills");
			xuiC_SkillList = xui.GetChildByType<XUiC_SkillList>();
		}
		if (xuiC_SkillList != null)
		{
			xuiC_SkillList.SetSelectedByUnlockData(this.UnlockData);
		}
	}

	public RecipeUnlockData UnlockData;
}
