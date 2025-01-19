using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_IngredientList : XUiController
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
			this.isDirty = true;
		}
	}

	public int CraftingTier
	{
		get
		{
			return this.craftingTier;
		}
		set
		{
			this.craftingTier = value;
			this.isDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = base.GetChildrenByType<XUiC_IngredientEntry>(null);
		XUiController[] array = childrenByType;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				this.ingredientEntries.Add(array[i]);
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
				int count = this.ingredientEntries.Count;
				int count2 = this.recipe.ingredients.Count;
				for (int i = 0; i < count; i++)
				{
					if (this.ingredientEntries[i] is XUiC_IngredientEntry)
					{
						ItemStack itemStack = (i < count2) ? this.recipe.ingredients[i].Clone() : null;
						if (itemStack != null && this.recipe.UseIngredientModifier)
						{
							itemStack.count = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, (float)itemStack.count, base.xui.playerUI.entityPlayer, this.recipe, FastTags<TagGroup.Global>.Parse(itemStack.itemValue.ItemClass.GetItemName()), true, true, true, true, true, this.craftingTier, true, false);
						}
						if (itemStack == null || (itemStack != null && itemStack.count > 0))
						{
							((XUiC_IngredientEntry)this.ingredientEntries[i]).Ingredient = itemStack;
						}
						else
						{
							((XUiC_IngredientEntry)this.ingredientEntries[i]).Ingredient = null;
						}
					}
				}
			}
			else
			{
				int count3 = this.ingredientEntries.Count;
				for (int j = 0; j < count3; j++)
				{
					if (this.ingredientEntries[j] is XUiC_IngredientEntry)
					{
						((XUiC_IngredientEntry)this.ingredientEntries[j]).Ingredient = null;
					}
				}
			}
			this.isDirty = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiController> ingredientEntries = new List<XUiController>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public int craftingTier = -1;
}
