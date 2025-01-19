using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RecipeTrackerWindow : XUiController
{
	public Recipe CurrentRecipe
	{
		get
		{
			return this.currentRecipe;
		}
		set
		{
			this.currentRecipe = value;
			this.IsDirty = true;
			base.RefreshBindings(true);
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_RecipeTrackerWindow.ID = base.WindowGroup.ID;
		this.ingredientList = base.GetChildByType<XUiC_RecipeTrackerIngredientsList>();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.localPlayer == null)
		{
			this.localPlayer = base.xui.playerUI.entityPlayer;
		}
		if (base.ViewComponent.IsVisible && this.localPlayer.IsDead())
		{
			this.IsDirty = true;
		}
		if (this.IsDirty)
		{
			this.ingredientList.Count = this.Count;
			if (this.currentRecipe != null)
			{
				int craftingTier = this.currentRecipe.GetCraftingTier(base.xui.playerUI.entityPlayer);
				if (this.currentRecipe.GetOutputItemClass().HasQuality)
				{
					this.currentRecipe.craftingTier = base.xui.Recipes.TrackedRecipeQuality;
				}
				else
				{
					this.currentRecipe.craftingTier = craftingTier;
				}
			}
			this.ingredientList.Recipe = this.currentRecipe;
			base.RefreshBindings(true);
			this.IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.CurrentRecipe = base.xui.Recipes.TrackedRecipe;
		base.xui.Recipes.OnTrackedRecipeChanged += this.RecipeTracker_OnTrackedRecipeChanged;
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		if (XUi.IsGameRunning())
		{
			base.xui.Recipes.OnTrackedRecipeChanged -= this.RecipeTracker_OnTrackedRecipeChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RecipeTracker_OnTrackedRecipeChanged()
	{
		this.CurrentRecipe = base.xui.Recipes.TrackedRecipe;
		this.Count = base.xui.Recipes.TrackedRecipeCount;
		this.IsDirty = true;
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "recipename")
		{
			value = ((this.currentRecipe != null) ? Localization.Get(this.currentRecipe.GetName(), false) : "");
			return true;
		}
		if (bindingName == "recipetitle")
		{
			value = ((this.currentRecipe != null) ? (Localization.Get(this.currentRecipe.GetName(), false) + ((this.Count > 1) ? string.Format(" (x{0})", this.Count) : "")) : "");
			return true;
		}
		if (bindingName == "recipeicon")
		{
			value = ((this.currentRecipe != null) ? this.currentRecipe.GetIcon() : "");
			return true;
		}
		if (bindingName == "showrecipe")
		{
			value = (this.currentRecipe != null && XUi.IsGameRunning() && this.localPlayer != null && !this.localPlayer.IsDead()).ToString();
			return true;
		}
		if (bindingName == "showempty")
		{
			value = (this.currentRecipe == null).ToString();
			return true;
		}
		if (!(bindingName == "trackerheight"))
		{
			return false;
		}
		if (this.currentRecipe == null)
		{
			value = "0";
		}
		else
		{
			value = this.trackerheightFormatter.Format(this.ingredientList.GetActiveIngredientCount() * 35);
		}
		return true;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeTrackerIngredientsList ingredientList;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	public int Count = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe currentRecipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt trackerheightFormatter = new CachedStringFormatterInt();
}
