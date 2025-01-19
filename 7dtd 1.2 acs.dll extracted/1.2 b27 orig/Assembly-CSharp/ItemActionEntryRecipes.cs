using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryRecipes : BaseItemActionEntry
{
	public ItemActionEntryRecipes(XUiController controller) : base(controller, "lblContextActionRecipes", "ui_game_symbol_hammer", BaseItemActionEntry.GamepadShortCut.DPadLeft, "crafting/craft_click_craft", "ui/ui_denied")
	{
	}

	public override void RefreshEnabled()
	{
		base.Enabled = true;
	}

	public override void OnActivated()
	{
		XUi xui = base.ItemController.xui;
		xui.playerUI.windowManager.CloseIfOpen("looting");
		List<XUiC_RecipeList> childrenByType = xui.GetChildrenByType<XUiC_RecipeList>();
		XUiC_RecipeList xuiC_RecipeList = null;
		for (int i = 0; i < childrenByType.Count; i++)
		{
			if (childrenByType[i].WindowGroup != null && childrenByType[i].WindowGroup.isShowing)
			{
				xuiC_RecipeList = childrenByType[i];
				break;
			}
		}
		if (xuiC_RecipeList == null)
		{
			XUiC_WindowSelector.OpenSelectorAndWindow(xui.playerUI.entityPlayer, "crafting");
			xuiC_RecipeList = xui.GetChildByType<XUiC_RecipeList>();
		}
		ItemStack recipeDataByIngredientStack = ItemStack.Empty.Clone();
		XUiC_ItemStack xuiC_ItemStack = base.ItemController as XUiC_ItemStack;
		if (xuiC_ItemStack != null)
		{
			recipeDataByIngredientStack = xuiC_ItemStack.ItemStack;
		}
		if (xuiC_RecipeList != null)
		{
			xuiC_RecipeList.SetRecipeDataByIngredientStack(recipeDataByIngredientStack);
		}
	}
}
