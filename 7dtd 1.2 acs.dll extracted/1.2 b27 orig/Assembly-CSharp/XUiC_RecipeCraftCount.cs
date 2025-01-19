using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RecipeCraftCount : XUiC_Counter
{
	public override void Init()
	{
		base.Init();
		XUiC_RecipeList childByType = this.windowGroup.Controller.GetChildByType<XUiC_RecipeList>();
		if (childByType != null)
		{
			childByType.CraftCount = this;
			childByType.RecipeChanged += this.HandleRecipeChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleRecipeChanged(Recipe _recipe, XUiC_RecipeEntry recipeEntry)
	{
		if (this.recipe != _recipe)
		{
			this.recipe = _recipe;
			base.Count = 1;
			this.CalculateMaxCount();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleMaxCountOnPress(XUiController _sender, int _mouseButton)
	{
		base.Count = this.calcMaxCraftable();
		base.HandleCountChangedEvent();
	}

	public void CalculateMaxCount()
	{
		this.MaxCount = this.calcMaxCraftable();
		if (base.Count > this.MaxCount)
		{
			base.Count = this.MaxCount;
		}
		base.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int calcMaxCraftable()
	{
		if (this.recipe == null)
		{
			return 1;
		}
		XUiC_WorkstationInputGrid childByType = this.windowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
		ItemStack[] array;
		if (childByType != null)
		{
			array = childByType.GetSlots();
		}
		else
		{
			array = base.xui.PlayerInventory.GetAllItemStacks().ToArray();
		}
		for (int i = 0; i < this.recipe.ingredients.Count; i++)
		{
			ItemStack itemStack = this.recipe.ingredients[i];
			if (itemStack != null && itemStack.itemValue.HasQuality)
			{
				return 1;
			}
		}
		int num = int.MaxValue;
		int craftingTier = (this.recipe.craftingTier == -1) ? ((int)EffectManager.GetValue(PassiveEffects.CraftingTier, null, 1f, base.xui.playerUI.entityPlayer, this.recipe, this.recipe.tags, true, true, true, true, true, 1, true, false)) : this.recipe.craftingTier;
		for (int j = 0; j < this.recipe.ingredients.Count; j++)
		{
			ItemStack itemStack2 = this.recipe.ingredients[j];
			if (itemStack2 != null && itemStack2.itemValue.type != 0)
			{
				int count = itemStack2.count;
				float num2;
				if (this.recipe.UseIngredientModifier)
				{
					num2 = (float)((int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, (float)count, base.xui.playerUI.entityPlayer, this.recipe, FastTags<TagGroup.Global>.Parse(itemStack2.itemValue.ItemClass.GetItemName()), true, true, true, true, true, craftingTier, true, false));
				}
				else
				{
					num2 = (float)count;
				}
				if (num2 >= 1f)
				{
					int num3 = 0;
					for (int k = 0; k < array.Length; k++)
					{
						if (array[k] != null && array[k].itemValue.type != 0 && itemStack2.itemValue.type == array[k].itemValue.type)
						{
							num3 += array[k].count;
						}
					}
					int num4 = Mathf.CeilToInt((float)num3 / num2);
					if (Mathf.FloorToInt(num2 * (float)num4) > num3)
					{
						num4--;
					}
					num = Mathf.Min(num4, num);
					if (num == 0)
					{
						break;
					}
				}
			}
		}
		return Mathf.Clamp(num, 1, 10000);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.CalculateMaxCount();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (base.xui.PlayerInventory != null)
		{
			base.xui.PlayerInventory.OnBackpackItemsChanged += this.PlayerInventory_OnBackpackItemsChanged;
			base.xui.PlayerInventory.OnToolbeltItemsChanged += this.PlayerInventory_OnToolbeltItemsChanged;
			this.CalculateMaxCount();
		}
		((XUiV_Label)this.counter.ViewComponent).Text = (this.textInput.Text = base.Count.ToString());
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
		this.CalculateMaxCount();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		this.CalculateMaxCount();
	}

	public void RefreshCounts()
	{
		this.CalculateMaxCount();
		this.IsDirty = true;
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "enablecountdown")
		{
			value = (base.Count > 1).ToString();
			return true;
		}
		if (!(bindingName == "enablecountup"))
		{
			return false;
		}
		value = (base.Count < this.MaxCount).ToString();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxAllowed = 10000;

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;
}
