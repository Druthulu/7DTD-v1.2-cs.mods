using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_IngredientEntry : XUiController
{
	public Recipe Recipe { get; set; }

	public ItemStack Ingredient
	{
		get
		{
			return this.ingredient;
		}
		set
		{
			this.ingredient = value;
			if (this.ingredient != null)
			{
				this.materialBased = ((XUiC_IngredientList)this.parent).Recipe.materialBasedRecipe;
				if (this.ingredient.itemValue.ItemClass != null)
				{
					this.material = this.ingredient.itemValue.ItemClass.MadeOfMaterial.ForgeCategory;
					if (this.material == null)
					{
						this.material = "";
					}
				}
			}
			this.isDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("icon");
		if (childById != null)
		{
			this.icoItem = (childById.ViewComponent as XUiV_Sprite);
		}
		XUiController childById2 = base.GetChildById("name");
		if (childById2 != null)
		{
			this.lblName = (childById2.ViewComponent as XUiV_Label);
		}
		childById2 = base.GetChildById("havecount");
		if (childById2 != null)
		{
			this.lblHaveCount = (childById2.ViewComponent as XUiV_Label);
		}
		childById2 = base.GetChildById("needcount");
		if (childById2 != null)
		{
			this.lblNeedCount = (childById2.ViewComponent as XUiV_Label);
		}
		this.craftCountControl = this.windowGroup.Controller.GetChildByType<XUiC_RecipeCraftCount>();
		this.craftCountControl.OnCountChanged += this.HandleOnCountChanged;
		this.isDirty = false;
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		bool flag = this.ingredient != null;
		if (bindingName == "itemname")
		{
			string text = "";
			if (flag && this.materialBased)
			{
				if (Localization.Exists("lbl" + this.material, false))
				{
					text = Localization.Get("lbl" + this.material, false);
				}
				else
				{
					text = XUi.UppercaseFirst(this.material);
				}
			}
			value = (flag ? (this.materialBased ? text : this.ingredient.itemValue.ItemClass.GetLocalizedItemName()) : "");
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
		if (bindingName == "havecount")
		{
			XUiC_WorkstationMaterialInputGrid childByType = this.windowGroup.Controller.GetChildByType<XUiC_WorkstationMaterialInputGrid>();
			if (childByType != null)
			{
				if (this.materialBased)
				{
					value = (flag ? this.havecountFormatter.Format(childByType.GetWeight(this.material)) : "");
				}
				else
				{
					value = (flag ? this.havecountFormatter.Format(base.xui.PlayerInventory.GetItemCount(this.ingredient.itemValue)) : "");
				}
			}
			else
			{
				XUiC_WorkstationInputGrid childByType2 = this.windowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
				if (childByType2 != null)
				{
					value = (flag ? this.havecountFormatter.Format(childByType2.GetItemCount(this.ingredient.itemValue)) : "");
				}
				else
				{
					value = (flag ? this.havecountFormatter.Format(base.xui.PlayerInventory.GetItemCount(this.ingredient.itemValue)) : "");
				}
			}
			return true;
		}
		if (bindingName == "needcount")
		{
			value = (flag ? this.needcountFormatter.Format(this.ingredient.count * this.craftCountControl.Count) : "");
			return true;
		}
		if (!(bindingName == "haveneedcount"))
		{
			return false;
		}
		string str = flag ? this.needcountFormatter.Format(this.ingredient.count * this.craftCountControl.Count) : "";
		XUiC_WorkstationMaterialInputGrid childByType3 = this.windowGroup.Controller.GetChildByType<XUiC_WorkstationMaterialInputGrid>();
		if (childByType3 != null)
		{
			if (this.materialBased)
			{
				value = (flag ? (this.havecountFormatter.Format(childByType3.GetWeight(this.material)) + "/" + str) : "");
			}
			else
			{
				value = (flag ? (this.havecountFormatter.Format(base.xui.PlayerInventory.GetItemCount(this.ingredient.itemValue)) + "/" + str) : "");
			}
		}
		else
		{
			XUiC_WorkstationInputGrid childByType4 = this.windowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
			if (childByType4 != null)
			{
				value = (flag ? (this.havecountFormatter.Format(childByType4.GetItemCount(this.ingredient.itemValue)) + "/" + str) : "");
			}
			else
			{
				value = (flag ? (this.havecountFormatter.Format(base.xui.PlayerInventory.GetItemCount(this.ingredient.itemValue)) + "/" + str) : "");
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
	{
		this.isDirty = true;
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
	public bool materialBased;

	[PublicizedFrom(EAccessModifier.Private)]
	public string material = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite icoItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblHaveCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblNeedCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeCraftCount craftCountControl;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt havecountFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt needcountFormatter = new CachedStringFormatterInt();
}
