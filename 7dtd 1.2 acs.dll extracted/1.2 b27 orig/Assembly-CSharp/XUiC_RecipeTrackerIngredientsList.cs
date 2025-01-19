using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RecipeTrackerIngredientsList : XUiController
{
	public Recipe Recipe
	{
		get
		{
			return this.recipe;
		}
		set
		{
			this.recipe = value;
			Recipe recipe = this.recipe;
			this.selectedCraftingTier = ((recipe != null) ? recipe.craftingTier : -1);
			this.isDirty = true;
			this.firstSetup = true;
		}
	}

	public int Count
	{
		get
		{
			return this.count;
		}
		set
		{
			this.count = value;
			this.isDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_RecipeTrackerIngredientEntry[] childrenByType = base.GetChildrenByType<XUiC_RecipeTrackerIngredientEntry>(null);
		for (int i = 0; i < childrenByType.Length; i++)
		{
			if (childrenByType[i] != null)
			{
				childrenByType[i].Owner = this;
				this.ingredientEntries.Add(childrenByType[i]);
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.PlayerInventory.OnBackpackItemsChanged += this.PlayerInventory_OnBackpackItemsChanged;
		base.xui.PlayerInventory.OnToolbeltItemsChanged += this.PlayerInventory_OnToolbeltItemsChanged;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.PlayerInventory.OnBackpackItemsChanged -= this.PlayerInventory_OnBackpackItemsChanged;
		base.xui.PlayerInventory.OnToolbeltItemsChanged -= this.PlayerInventory_OnToolbeltItemsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		this.isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		this.isDirty = true;
	}

	public override void Update(float _dt)
	{
		if (this.isDirty)
		{
			if (this.recipe != null)
			{
				bool flag = true;
				int num = this.ingredientEntries.Count;
				int num2 = this.recipe.ingredients.Count;
				int craftingTier = (this.selectedCraftingTier == -1) ? ((int)EffectManager.GetValue(PassiveEffects.CraftingTier, null, 1f, base.xui.playerUI.entityPlayer, this.recipe, this.recipe.tags, true, true, true, true, true, 1, true, false)) : this.selectedCraftingTier;
				for (int i = 0; i < num; i++)
				{
					XUiC_RecipeTrackerIngredientEntry xuiC_RecipeTrackerIngredientEntry = this.ingredientEntries[i];
					ItemStack itemStack = (i < num2) ? this.recipe.ingredients[i].Clone() : null;
					if (itemStack != null && this.recipe.UseIngredientModifier)
					{
						itemStack.count = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, (float)itemStack.count, base.xui.playerUI.entityPlayer, this.recipe, FastTags<TagGroup.Global>.Parse(itemStack.itemValue.ItemClass.GetItemName()), true, true, true, true, true, craftingTier, true, false);
					}
					if (itemStack == null || (itemStack != null && itemStack.count > 0))
					{
						xuiC_RecipeTrackerIngredientEntry.Ingredient = itemStack;
					}
					else
					{
						xuiC_RecipeTrackerIngredientEntry.Ingredient = null;
					}
					if (xuiC_RecipeTrackerIngredientEntry.Ingredient != null && !xuiC_RecipeTrackerIngredientEntry.IsComplete)
					{
						flag = false;
					}
				}
				if (this.firstSetup)
				{
					this.lastComplete = flag;
					this.firstSetup = false;
				}
				if (flag && !this.lastComplete)
				{
					GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "ttAllIngredientsFound", Localization.Get(this.recipe.GetName(), false), null, null, false);
					this.lastComplete = flag;
					this.isDirty = false;
					base.Update(_dt);
					return;
				}
				this.lastComplete = flag;
			}
			else
			{
				int num3 = this.ingredientEntries.Count;
				for (int j = 0; j < num3; j++)
				{
					this.ingredientEntries[j].Ingredient = null;
				}
			}
			this.isDirty = false;
		}
		base.Update(_dt);
	}

	public int GetActiveIngredientCount()
	{
		if (this.recipe == null)
		{
			return 0;
		}
		int craftingTier = (this.selectedCraftingTier == -1) ? ((int)EffectManager.GetValue(PassiveEffects.CraftingTier, null, 1f, base.xui.playerUI.entityPlayer, this.recipe, this.recipe.tags, true, true, true, true, true, 1, true, false)) : this.selectedCraftingTier;
		int num = 0;
		for (int i = 0; i < this.recipe.ingredients.Count; i++)
		{
			ItemStack itemStack = this.recipe.ingredients[i];
			if (this.recipe.UseIngredientModifier && (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, (float)itemStack.count, base.xui.playerUI.entityPlayer, this.recipe, FastTags<TagGroup.Global>.Parse(itemStack.itemValue.ItemClass.GetItemName()), true, true, true, true, true, craftingTier, true, false) > 0)
			{
				num++;
			}
		}
		return num;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		if (name == "complete_icon")
		{
			this.completeIconName = value;
			return true;
		}
		if (name == "incomplete_icon")
		{
			this.incompleteIconName = value;
			return true;
		}
		if (name == "complete_color")
		{
			Color32 color = StringParsers.ParseColor(value);
			this.completeColor = string.Format("{0},{1},{2},{3}", new object[]
			{
				color.r,
				color.g,
				color.b,
				color.a
			});
			this.completeHexColor = Utils.ColorToHex(color);
			return true;
		}
		if (!(name == "incomplete_color"))
		{
			return base.ParseAttribute(name, value, _parent);
		}
		Color32 color2 = StringParsers.ParseColor(value);
		this.incompleteColor = string.Format("{0},{1},{2},{3}", new object[]
		{
			color2.r,
			color2.g,
			color2.b,
			color2.a
		});
		this.incompleteHexColor = Utils.ColorToHex(color2);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedCraftingTier;

	[PublicizedFrom(EAccessModifier.Private)]
	public int count = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_RecipeTrackerIngredientEntry> ingredientEntries = new List<XUiC_RecipeTrackerIngredientEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstSetup;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastComplete;

	public string completeIconName = "";

	public string incompleteIconName = "";

	public string completeHexColor = "FF00FF00";

	public string incompleteHexColor = "FFB400";

	public string warningHexColor = "FFFF00FF";

	public string inactiveHexColor = "888888FF";

	public string activeHexColor = "FFFFFFFF";

	public string completeColor = "0,255,0,255";

	public string incompleteColor = "255, 180, 0, 255";

	public string warningColor = "255,255,0,255";
}
