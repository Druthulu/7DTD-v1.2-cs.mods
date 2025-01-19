using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Challenges;

public class XUiM_Recipes : XUiModel
{
	public event XUiEvent_TrackedQuestChanged OnTrackedRecipeChanged;

	public Recipe TrackedRecipe
	{
		get
		{
			return this.trackedRecipe;
		}
		set
		{
			if (this.trackedRecipe != null)
			{
				this.trackedRecipe.IsTracked = false;
			}
			this.trackedRecipe = value;
			if (this.trackedRecipe != null)
			{
				this.trackedRecipe.IsTracked = true;
			}
			if (this.OnTrackedRecipeChanged != null)
			{
				this.OnTrackedRecipeChanged();
			}
		}
	}

	public void SetPreviousTracked(EntityPlayerLocal player)
	{
		XUi xui = player.PlayerUI.xui;
		Quest trackedQuest = player.QuestJournal.TrackedQuest;
		XUiM_Recipes.sPreviouslyTrackedChallenge = xui.QuestTracker.TrackedChallenge;
		player.QuestJournal.TrackedQuest = null;
		xui.QuestTracker.TrackedChallenge = null;
		XUiM_Recipes.sPreviouslyTrackedQuest = trackedQuest;
	}

	public void ResetToPreviousTracked(EntityPlayerLocal player)
	{
		XUi xui = player.PlayerUI.xui;
		if (XUiM_Recipes.sPreviouslyTrackedQuest != null)
		{
			if (!player.QuestJournal.QuestIsActive(XUiM_Recipes.sPreviouslyTrackedQuest))
			{
				XUiM_Recipes.sPreviouslyTrackedQuest = player.QuestJournal.FindActiveQuest();
			}
		}
		else if (XUiM_Recipes.sPreviouslyTrackedChallenge != null && !XUiM_Recipes.sPreviouslyTrackedChallenge.IsActive)
		{
			XUiM_Recipes.sPreviouslyTrackedChallenge = XUiM_Recipes.sPreviouslyTrackedChallenge.Owner.GetNextChallenge(XUiM_Recipes.sPreviouslyTrackedChallenge);
		}
		player.QuestJournal.TrackedQuest = XUiM_Recipes.sPreviouslyTrackedQuest;
		xui.QuestTracker.TrackedChallenge = XUiM_Recipes.sPreviouslyTrackedChallenge;
	}

	public void RefreshTrackedRecipe()
	{
		if (this.OnTrackedRecipeChanged != null)
		{
			this.OnTrackedRecipeChanged();
		}
	}

	public static ReadOnlyCollection<Recipe> GetRecipes()
	{
		return CraftingManager.NonScrapableRecipes;
	}

	public static void FilterRecipesByCategory(string _category, ref List<Recipe> recipeList)
	{
		List<Recipe> list = new List<Recipe>();
		for (int i = 0; i < recipeList.Count; i++)
		{
			if (recipeList[i].isChallenge)
			{
				list.Add(recipeList[i]);
			}
			else if (recipeList[i].isQuest)
			{
				list.Add(recipeList[i]);
			}
			else if (recipeList[i].IsTracked)
			{
				list.Add(recipeList[i]);
			}
			else
			{
				ItemClass forId = ItemClass.GetForId(recipeList[i].itemValueType);
				if (forId != null)
				{
					string[] array;
					if (!forId.IsBlock())
					{
						array = forId.Groups;
					}
					else
					{
						array = Block.list[forId.Id].GroupNames;
					}
					if (array != null)
					{
						for (int j = 0; j < array.Length; j++)
						{
							if (array[j] != null && array[j].EqualsCaseInsensitive(_category))
							{
								list.Add(recipeList[i]);
								break;
							}
						}
					}
				}
			}
		}
		recipeList = list;
	}

	public static List<Recipe> FilterRecipesByWorkstation(string _workstation, IList<Recipe> recipeList)
	{
		if (_workstation == null)
		{
			_workstation = "";
		}
		List<Recipe> list = new List<Recipe>();
		for (int i = 0; i < recipeList.Count; i++)
		{
			if (recipeList[i] != null)
			{
				if (_workstation != "")
				{
					Block blockByName = Block.GetBlockByName(_workstation, false);
					if (blockByName != null && blockByName.Properties.Values.ContainsKey("Workstation.CraftingAreaRecipes"))
					{
						string text = blockByName.Properties.Values["Workstation.CraftingAreaRecipes"];
						string[] array = new string[]
						{
							text
						};
						if (text.Contains(","))
						{
							array = text.Replace(", ", ",").Replace(" ,", ",").Replace(" , ", ",").Split(',', StringSplitOptions.None);
						}
						bool flag = false;
						for (int j = 0; j < array.Length; j++)
						{
							if (recipeList[i].craftingArea != null && recipeList[i].craftingArea.EqualsCaseInsensitive(array[j]))
							{
								list.Add(recipeList[i]);
								flag = true;
								break;
							}
							if ((recipeList[i].craftingArea == null || recipeList[i].craftingArea == "") && array[j].EqualsCaseInsensitive("player"))
							{
								list.Add(recipeList[i]);
								flag = true;
								break;
							}
						}
						if (!flag && recipeList[i].craftingArea != null && recipeList[i].craftingArea.EqualsCaseInsensitive(_workstation))
						{
							list.Add(recipeList[i]);
						}
					}
					else if (recipeList[i].craftingArea != null && recipeList[i].craftingArea.EqualsCaseInsensitive(_workstation))
					{
						list.Add(recipeList[i]);
					}
				}
				else if (recipeList[i].craftingArea == null || recipeList[i].craftingArea == "")
				{
					list.Add(recipeList[i]);
				}
			}
		}
		return list;
	}

	public static List<Recipe> FilterRecipesByName(string _name, IList<Recipe> recipeList)
	{
		List<Recipe> list = new List<Recipe>();
		for (int i = 0; i < recipeList.Count; i++)
		{
			Recipe recipe = recipeList[i];
			if (!recipe.craftingArea.EqualsCaseInsensitive("assembly"))
			{
				string a;
				if (Localization.TryGet(recipe.GetName(), out a) && a.ContainsCaseInsensitive(_name))
				{
					list.Add(recipe);
				}
				else
				{
					ItemClass forId = ItemClass.GetForId(recipe.itemValueType);
					if (forId != null)
					{
						if (!forId.IsBlock())
						{
							if (forId.GetItemName().ContainsCaseInsensitive(_name))
							{
								list.Add(recipe);
							}
							else if (forId.GetLocalizedItemName().ContainsCaseInsensitive(_name))
							{
								list.Add(recipe);
							}
						}
						else
						{
							Block block = Block.list[forId.Id];
							if (block != null)
							{
								if (block.GetBlockName().ContainsCaseInsensitive(_name))
								{
									list.Add(recipe);
								}
								else if (block.GetLocalizedBlockName().ContainsCaseInsensitive(_name))
								{
									list.Add(recipe);
								}
							}
						}
					}
				}
			}
		}
		return list;
	}

	public static List<Recipe> FilterRecipesByIngredient(ItemStack stack, IList<Recipe> recipeList)
	{
		List<Recipe> list = new List<Recipe>();
		ItemValue[] items = new ItemValue[]
		{
			stack.itemValue
		};
		for (int i = 0; i < recipeList.Count; i++)
		{
			if (recipeList[i].ContainsIngredients(items))
			{
				list.Add(recipeList[i]);
			}
		}
		return list;
	}

	public static List<Recipe> FilterRecipesByItem(List<int> itemIDs, IList<Recipe> recipeList)
	{
		List<Recipe> list = new List<Recipe>();
		for (int i = 0; i < recipeList.Count; i++)
		{
			if (itemIDs.Contains(recipeList[i].itemValueType))
			{
				list.Add(recipeList[i]);
			}
		}
		return list;
	}

	public static List<Recipe> FilterRecipesByID(int itemID, IList<Recipe> recipeList)
	{
		List<Recipe> list = new List<Recipe>();
		for (int i = 0; i < recipeList.Count; i++)
		{
			if (itemID == recipeList[i].itemValueType)
			{
				list.Add(recipeList[i]);
			}
		}
		return list;
	}

	public static int GetCount(XUi xui)
	{
		return CraftingManager.GetRecipes().Count;
	}

	public static string GetRecipeName(Recipe _recipe)
	{
		return _recipe.GetName();
	}

	public static string GetRecipeSpriteName(Recipe _recipe)
	{
		return _recipe.GetIcon();
	}

	public static bool GetRecipeIsUnlocked(XUi xui, Recipe _recipe)
	{
		return _recipe.IsUnlocked(xui.playerUI.entityPlayer);
	}

	public static bool GetRecipeIsUnlocked(XUi xui, string itemName)
	{
		Recipe recipe = CraftingManager.GetRecipe(itemName);
		return recipe != null && XUiM_Recipes.GetRecipeIsUnlocked(xui, recipe);
	}

	public static bool GetRecipeIsUnlockable(XUi xui, Recipe _recipe)
	{
		return _recipe.IsLearnable;
	}

	public static bool GetRecipeIsFavorite(XUi xui, Recipe _recipe)
	{
		return CraftingManager.RecipeIsFavorite(_recipe);
	}

	public static float GetRecipeCraftTime(XUi xui, Recipe _recipe)
	{
		return EffectManager.GetValue(PassiveEffects.CraftingTime, null, _recipe.craftingTime, xui.playerUI.entityPlayer, _recipe, _recipe.tags, true, true, true, true, true, 1, true, false);
	}

	public static int GetRecipeCraftOutputCount(XUi xui, Recipe _recipe)
	{
		return (int)EffectManager.GetValue(PassiveEffects.CraftingOutputCount, null, (float)_recipe.count, xui.playerUI.entityPlayer, _recipe, _recipe.tags, true, true, true, true, true, 1, true, false);
	}

	public static ItemStack GetRecipeOutput(Recipe _recipe)
	{
		return new ItemStack(new ItemValue(_recipe.itemValueType, false), _recipe.count);
	}

	public static List<ItemStack> GetRecipeIngredients(Recipe _recipe)
	{
		return _recipe.GetIngredientsSummedUp();
	}

	public static void GetCurrentSlots()
	{
	}

	public static bool HasIngredientsForRecipe(IList<ItemStack> allItems, Recipe _recipe, EntityAlive _ea = null)
	{
		return _recipe.CanCraftAny(allItems, _ea);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Quest sPreviouslyTrackedQuest;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Challenge sPreviouslyTrackedChallenge;

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe trackedRecipe;

	public int TrackedRecipeQuality = 1;

	public int TrackedRecipeCount = 1;
}
