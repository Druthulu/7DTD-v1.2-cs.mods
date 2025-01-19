using System;
using UnityEngine;

public class RecipeUnlockData
{
	public RecipeUnlockData.UnlockTypes UnlockType
	{
		get
		{
			return this.unlockType;
		}
		set
		{
			this.unlockType = value;
		}
	}

	public ItemClass Item
	{
		get
		{
			return this.item;
		}
		set
		{
			this.item = value;
			this.unlockType = RecipeUnlockData.UnlockTypes.Schematic;
		}
	}

	public ProgressionClass Perk
	{
		get
		{
			return this.perk;
		}
		set
		{
			this.perk = value;
			if (this.perk.IsBook)
			{
				this.unlockType = RecipeUnlockData.UnlockTypes.Book;
				return;
			}
			if (this.perk.IsCrafting)
			{
				this.unlockType = RecipeUnlockData.UnlockTypes.Skill;
				return;
			}
			this.unlockType = RecipeUnlockData.UnlockTypes.Perk;
		}
	}

	public RecipeUnlockData(string unlock)
	{
		if (Progression.ProgressionClasses.ContainsKey(unlock))
		{
			this.Perk = Progression.ProgressionClasses[unlock];
			return;
		}
		ItemClass itemClass = ItemClass.GetItemClass(unlock, true);
		if (itemClass != null)
		{
			this.Item = itemClass;
			return;
		}
		this.UnlockType = RecipeUnlockData.UnlockTypes.None;
	}

	public string GetName()
	{
		if (this.unlockType == RecipeUnlockData.UnlockTypes.Schematic)
		{
			return this.item.GetLocalizedItemName();
		}
		return Localization.Get(this.perk.NameKey, false);
	}

	public string GetIcon()
	{
		if (this.unlockType == RecipeUnlockData.UnlockTypes.Schematic)
		{
			return "ui_game_symbol_book";
		}
		if (this.unlockType == RecipeUnlockData.UnlockTypes.Perk)
		{
			return "ui_game_symbol_skills";
		}
		if (this.unlockType == RecipeUnlockData.UnlockTypes.Skill)
		{
			return "ui_game_symbol_hammer";
		}
		return "ui_game_symbol_book";
	}

	public string GetLevel(EntityPlayerLocal player, string recipeName)
	{
		if (this.unlockType == RecipeUnlockData.UnlockTypes.Skill)
		{
			for (int i = 0; i < this.perk.DisplayDataList.Count; i++)
			{
				ProgressionClass.DisplayData displayData = this.perk.DisplayDataList[i];
				for (int j = 0; j < displayData.UnlockDataList.Count; j++)
				{
					ProgressionClass.DisplayData.UnlockData unlockData = displayData.UnlockDataList[j];
					if (unlockData.ItemName == recipeName || (unlockData.RecipeList != null && unlockData.RecipeList.ContainsCaseInsensitive(recipeName)))
					{
						int unlockTier = unlockData.UnlockTier;
						ProgressionValue progressionValue = player.Progression.GetProgressionValue(this.perk.Name);
						return string.Format("{0}/{1}", progressionValue.Level, displayData.QualityStarts[unlockTier].ToString());
					}
				}
			}
			return "--";
		}
		return "--";
	}

	public string GetIconAtlas()
	{
		return "UIAtlas";
	}

	public Color GetItemTint()
	{
		if (this.unlockType == RecipeUnlockData.UnlockTypes.Schematic && this.item != null)
		{
			return this.item.GetIconTint(null);
		}
		return Color.white;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RecipeUnlockData.UnlockTypes unlockType;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass item;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionClass perk;

	public enum UnlockTypes
	{
		None,
		Perk,
		Book,
		Skill,
		Schematic
	}
}
