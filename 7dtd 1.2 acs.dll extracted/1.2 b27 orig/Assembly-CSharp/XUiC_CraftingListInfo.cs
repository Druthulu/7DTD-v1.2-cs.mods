using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CraftingListInfo : XUiController
{
	public string CategoryName
	{
		get
		{
			return this.categoryName;
		}
		set
		{
			this.isDirty |= (this.categoryName != value);
			this.categoryName = value;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("windowicon");
		if (childById != null)
		{
			this.icoCategoryIcon = (childById.ViewComponent as XUiV_Sprite);
		}
		XUiController childById2 = base.GetChildById("windowname");
		if (childById2 != null)
		{
			this.lblName = (childById2.ViewComponent as XUiV_Label);
		}
		childById2 = base.GetChildById("unlockedcount");
		if (childById2 != null)
		{
			this.lblUnlockedCount = (childById2.ViewComponent as XUiV_Label);
		}
		XUiController childById3 = base.GetChildById("categories");
		if (childById3 != null)
		{
			this.categoryList = (XUiC_CategoryList)childById3;
			this.categoryList.CategoryChanged += this.HandleCategoryChanged;
		}
		this.isDirty = true;
		this.IsDormant = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleCategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		this.CategoryName = _categoryEntry.CategoryDisplayName;
		this.spriteName = _categoryEntry.SpriteName;
	}

	public override void Update(float _dt)
	{
		if (!this.windowGroup.isShowing)
		{
			return;
		}
		if (this.isDirty)
		{
			if (this.lblName != null)
			{
				this.lblName.Text = this.categoryName;
			}
			if (this.icoCategoryIcon != null)
			{
				this.icoCategoryIcon.SpriteName = this.spriteName;
			}
			if (this.lblUnlockedCount != null)
			{
				this.lblUnlockedCount.Text = string.Format("{0}/{1}", CraftingManager.GetUnlockedRecipeCount(), CraftingManager.GetLockedRecipeCount());
			}
			this.isDirty = false;
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.isDirty = true;
		this.IsDormant = false;
		CraftingManager.RecipeUnlocked += this.CraftingManager_RecipeUnlocked;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.IsDormant = true;
		CraftingManager.RecipeUnlocked -= this.CraftingManager_RecipeUnlocked;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CraftingManager_RecipeUnlocked(string recipeName)
	{
		this.isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryName = "Basics";

	[PublicizedFrom(EAccessModifier.Private)]
	public string spriteName = "Craft_Icon_Basics";

	[PublicizedFrom(EAccessModifier.Private)]
	public string unlockedCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite icoCategoryIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblUnlockedCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;
}
