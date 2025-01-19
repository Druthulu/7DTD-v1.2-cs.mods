using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RecipeTrackerIngredientEntry : XUiController
{
	public XUiC_RecipeTrackerIngredientsList Owner { get; set; }

	public ItemStack Ingredient
	{
		get
		{
			return this.ingredient;
		}
		set
		{
			this.ingredient = value;
			if (this.ingredient != null && this.Owner.Recipe.materialBasedRecipe)
			{
				this.ingredient = null;
			}
			if (this.ingredient != null)
			{
				this.currentCount = base.xui.PlayerInventory.GetItemCount(this.ingredient.itemValue);
				this.IsComplete = (this.currentCount >= this.ingredient.count * this.Owner.Count);
			}
			else
			{
				this.currentCount = 0;
				this.IsComplete = false;
			}
			this.isDirty = true;
		}
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		bool flag = this.ingredient != null;
		if (bindingName == "hasingredient")
		{
			value = flag.ToString();
			return true;
		}
		if (bindingName == "itemname")
		{
			value = (flag ? this.ingredient.itemValue.ItemClass.GetLocalizedItemName() : "");
			return true;
		}
		if (bindingName == "itemicon")
		{
			value = (flag ? this.ingredient.itemValue.ItemClass.GetIconName() : "");
			return true;
		}
		if (bindingName == "itemicontint")
		{
			Color32 v = Color.white;
			if (flag)
			{
				ItemClass itemClass = this.ingredient.itemValue.ItemClass;
				if (itemClass != null)
				{
					v = itemClass.GetIconTint(this.ingredient.itemValue);
				}
			}
			value = this.itemicontintcolorFormatter.Format(v);
			return true;
		}
		if (bindingName == "itemcount")
		{
			if (flag)
			{
				int v2 = this.ingredient.count * this.Owner.Count;
				value = this.countFormatter.Format(this.currentCount, v2);
			}
			return true;
		}
		if (!(bindingName == "ingredientcompletehexcolor"))
		{
			return false;
		}
		if (flag)
		{
			value = ((this.currentCount >= this.ingredient.count * this.Owner.Count) ? this.Owner.completeHexColor : this.Owner.incompleteHexColor);
		}
		else
		{
			value = "FFFFFF";
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
	{
		base.RefreshBindings(true);
	}

	public override void Update(float _dt)
	{
		if (this.isDirty)
		{
			base.RefreshBindings(false);
			base.ViewComponent.IsVisible = true;
			this.isDirty = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack ingredient;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentCount;

	public bool IsComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> countFormatter = new CachedStringFormatter<int, int>((int _i1, int _i2) => _i1.ToString() + "/" + _i2.ToString());
}
