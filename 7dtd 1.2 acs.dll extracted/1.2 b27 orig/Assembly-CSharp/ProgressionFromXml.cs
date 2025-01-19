using System;
using System.Collections;
using System.Globalization;
using System.Xml.Linq;

public class ProgressionFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <progression> found!");
		}
		if (Progression.ProgressionClasses != null)
		{
			Progression.ProgressionClasses.Clear();
		}
		else
		{
			Progression.ProgressionClasses = new CaseInsensitiveStringDictionary<ProgressionClass>();
		}
		foreach (XElement xelement in root.Elements())
		{
			string localName = xelement.Name.LocalName;
			if (localName == "level")
			{
				ProgressionFromXml.parseLevelNode(xelement);
			}
			else if (localName == "attributes" || localName == "skills" || localName == "perks" || localName == "book_groups" || localName == "crafting_skills")
			{
				ProgressionFromXml.parseProgressionItems(xelement);
			}
		}
		foreach (string text in Progression.ProgressionClasses.Keys)
		{
			ProgressionClass progressionClass = Progression.ProgressionClasses[text];
			if (progressionClass.ParentName != null && progressionClass.ParentName != string.Empty)
			{
				ProgressionClass progressionClass2;
				if (!Progression.ProgressionClasses.TryGetValue(progressionClass.ParentName, out progressionClass2))
				{
					Log.Error(string.Concat(new string[]
					{
						"Progression class '",
						text,
						"' has non-existing parent name '",
						progressionClass.ParentName,
						"'"
					}));
				}
				else
				{
					progressionClass2.Children.Add(progressionClass);
					if (progressionClass.IsBook || progressionClass.IsBookGroup)
					{
						Progression.ProgressionClasses[progressionClass.ParentName].DisplayType = ProgressionClass.DisplayTypes.Book;
					}
					else if (progressionClass.IsCrafting)
					{
						Progression.ProgressionClasses[progressionClass.ParentName].DisplayType = ProgressionClass.DisplayTypes.Crafting;
					}
				}
			}
		}
		ProgressionFromXml.clearProgressionValueLinks();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void clearProgressionValueLinks()
	{
		if (!GameManager.Instance)
		{
			return;
		}
		World world = GameManager.Instance.World;
		DictionaryList<int, Entity> dictionaryList = (world != null) ? world.Entities : null;
		if (dictionaryList == null || dictionaryList.Count == 0)
		{
			return;
		}
		foreach (Entity entity in dictionaryList.list)
		{
			EntityAlive entityAlive = entity as EntityAlive;
			if (entityAlive != null)
			{
				Progression progression = entityAlive.Progression;
				if (progression != null)
				{
					progression.ClearProgressionClassLinks();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseLevelNode(XElement element)
	{
		if (element.HasAttribute("exp_to_level"))
		{
			Progression.BaseExpToLevel = int.Parse(element.GetAttribute("exp_to_level"));
		}
		else
		{
			Progression.BaseExpToLevel = 500;
		}
		if (element.HasAttribute("clamp_exp_cost_at_level"))
		{
			Progression.ClampExpCostAtLevel = int.Parse(element.GetAttribute("clamp_exp_cost_at_level"));
		}
		else
		{
			Progression.ClampExpCostAtLevel = 300;
		}
		if (element.HasAttribute("experience_multiplier"))
		{
			Progression.ExpMultiplier = StringParsers.ParseFloat(element.GetAttribute("experience_multiplier"), 0, -1, NumberStyles.Any);
		}
		else
		{
			Progression.ExpMultiplier = 1.02f;
		}
		if (element.HasAttribute("skill_points_per_level"))
		{
			Progression.SkillPointsPerLevel = int.Parse(element.GetAttribute("skill_points_per_level"));
		}
		else
		{
			Progression.SkillPointsPerLevel = 1;
		}
		if (element.HasAttribute("skill_point_multiplier"))
		{
			Progression.SkillPointMultiplier = StringParsers.ParseFloat(element.GetAttribute("skill_point_multiplier"), 0, -1, NumberStyles.Any);
		}
		else
		{
			Progression.SkillPointMultiplier = 0f;
		}
		if (element.HasAttribute("max_level"))
		{
			Progression.MaxLevel = int.Parse(element.GetAttribute("max_level"));
			return;
		}
		Progression.MaxLevel = 200;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseProgressionItems(XElement element)
	{
		int max_level = 0;
		int min_level = 0;
		int base_cost = 1;
		float cost_multiplier_per_level = 1f;
		float max_level_ratio_to_parent = 1f;
		if (element.HasAttribute("min_level"))
		{
			min_level = int.Parse(element.GetAttribute("min_level"));
		}
		else if (element.Name == "attributes")
		{
			min_level = 1;
		}
		else if (element.Name == "skills")
		{
			min_level = 1;
		}
		else if (element.Name == "perks")
		{
			min_level = 0;
		}
		else if (element.Name == "crafting_skills")
		{
			min_level = 1;
		}
		if (element.HasAttribute("max_level"))
		{
			max_level = int.Parse(element.GetAttribute("max_level"));
		}
		else if (element.Name == "attributes")
		{
			max_level = 10;
		}
		else if (element.Name == "skills")
		{
			max_level = 100;
		}
		else if (element.Name == "perks")
		{
			max_level = 5;
		}
		else if (element.Name == "crafting_skills")
		{
			max_level = 100;
		}
		if (element.HasAttribute("max_level_ratio_to_parent"))
		{
			max_level_ratio_to_parent = StringParsers.ParseFloat(element.GetAttribute("max_level_ratio_to_parent"), 0, -1, NumberStyles.Any);
		}
		if (element.HasAttribute("base_skill_point_cost"))
		{
			base_cost = int.Parse(element.GetAttribute("base_skill_point_cost"));
		}
		else if (element.HasAttribute("base_exp_cost"))
		{
			base_cost = int.Parse(element.GetAttribute("base_exp_cost"));
		}
		if (element.HasAttribute("cost_multiplier_per_level"))
		{
			cost_multiplier_per_level = StringParsers.ParseFloat(element.GetAttribute("cost_multiplier_per_level"), 0, -1, NumberStyles.Any);
		}
		if (element.Name == "crafting_skills" && element.HasAttribute("complete_sound"))
		{
			ProgressionClass.DisplayData.CompletionSound = element.GetAttribute("complete_sound");
		}
		float num = 1f;
		foreach (XElement childElement in element.Elements())
		{
			ProgressionFromXml.parseProgressionItem(childElement, max_level, min_level, base_cost, cost_multiplier_per_level, max_level_ratio_to_parent, ref num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseProgressionItem(XElement childElement, int max_level, int min_level, int base_cost, float cost_multiplier_per_level, float max_level_ratio_to_parent, ref float order)
	{
		ProgressionType progressionType = ProgressionType.None;
		if (childElement.Name == "attribute")
		{
			progressionType = ProgressionType.Attribute;
		}
		else if (childElement.Name == "skill")
		{
			progressionType = ProgressionType.Skill;
		}
		else if (childElement.Name == "perk")
		{
			progressionType = ProgressionType.Perk;
		}
		else if (childElement.Name == "book")
		{
			progressionType = ProgressionType.Book;
		}
		else if (childElement.Name == "book_group")
		{
			progressionType = ProgressionType.BookGroup;
		}
		else if (childElement.Name == "crafting_skill")
		{
			progressionType = ProgressionType.Crafting;
		}
		if (progressionType == ProgressionType.None)
		{
			return;
		}
		if (childElement.HasAttribute("min_level"))
		{
			min_level = int.Parse(childElement.GetAttribute("min_level"));
		}
		if (childElement.HasAttribute("max_level"))
		{
			max_level = int.Parse(childElement.GetAttribute("max_level"));
		}
		if (childElement.HasAttribute("max_level_ratio_to_parent"))
		{
			max_level_ratio_to_parent = StringParsers.ParseFloat(childElement.GetAttribute("max_level_ratio_to_parent"), 0, -1, NumberStyles.Any);
		}
		if (childElement.HasAttribute("base_skill_point_cost"))
		{
			base_cost = int.Parse(childElement.GetAttribute("base_skill_point_cost"));
		}
		else if (childElement.HasAttribute("base_exp_cost"))
		{
			base_cost = int.Parse(childElement.GetAttribute("base_exp_cost"));
		}
		if (childElement.HasAttribute("cost_multiplier_per_level"))
		{
			cost_multiplier_per_level = StringParsers.ParseFloat(childElement.GetAttribute("cost_multiplier_per_level"), 0, -1, NumberStyles.Any);
		}
		if (childElement.HasAttribute("name"))
		{
			ProgressionClass progressionClass = new ProgressionClass(childElement.GetAttribute("name").ToLower())
			{
				MinLevel = min_level,
				MaxLevel = max_level,
				Type = progressionType,
				BaseCostToLevel = base_cost,
				CostMultiplier = ((cost_multiplier_per_level > 0f) ? cost_multiplier_per_level : 1f),
				ParentMaxLevelRatio = max_level_ratio_to_parent
			};
			progressionClass.ListSortOrder = order;
			order += 1f;
			if (childElement.HasAttribute("parent"))
			{
				progressionClass.ParentName = childElement.GetAttribute("parent").ToLower();
			}
			if (childElement.HasAttribute("name_key"))
			{
				progressionClass.NameKey = childElement.GetAttribute("name_key");
			}
			if (childElement.HasAttribute("max_level_ratio_to_parent"))
			{
				progressionClass.ParentMaxLevelRatio = StringParsers.ParseFloat(childElement.GetAttribute("max_level_ratio_to_parent"), 0, -1, NumberStyles.Any);
			}
			if (childElement.HasAttribute("desc_key"))
			{
				progressionClass.DescKey = childElement.GetAttribute("desc_key");
			}
			if (childElement.HasAttribute("long_desc_key"))
			{
				progressionClass.LongDescKey = childElement.GetAttribute("long_desc_key");
			}
			if (childElement.HasAttribute("icon"))
			{
				progressionClass.Icon = childElement.GetAttribute("icon");
			}
			foreach (XElement xelement in childElement.Elements())
			{
				if (xelement.Name == "level_requirements")
				{
					int level = 0;
					if (xelement.HasAttribute("level"))
					{
						level = StringParsers.ParseSInt32(xelement.GetAttribute("level"), 0, -1, NumberStyles.Integer);
					}
					LevelRequirement lr = new LevelRequirement(level);
					if (xelement.HasElements)
					{
						foreach (XElement element in xelement.Elements("requirement"))
						{
							IRequirement requirement = RequirementBase.ParseRequirement(element);
							if (requirement != null)
							{
								lr.AddRequirement(requirement);
							}
						}
					}
					progressionClass.AddLevelRequirement(lr);
				}
				else if (xelement.Name == "display_entry")
				{
					string[] array = xelement.GetAttribute("unlock_level").Split(',', StringSplitOptions.None);
					int[] array2 = new int[array.Length];
					string item = "";
					string[] customName = null;
					string[] array3 = null;
					string[] customIconTint = null;
					bool customHasQuality = false;
					for (int i = 0; i < array.Length; i++)
					{
						array2[i] = StringParsers.ParseSInt32(array[i], 0, -1, NumberStyles.Integer);
					}
					if (xelement.HasAttribute("item"))
					{
						item = xelement.GetAttribute("item");
					}
					if (xelement.HasAttribute("has_quality"))
					{
						customHasQuality = StringParsers.ParseBool(xelement.GetAttribute("has_quality"), 0, -1, true);
					}
					if (xelement.HasAttribute("icon"))
					{
						array3 = xelement.GetAttribute("icon").Split(',', StringSplitOptions.None);
					}
					if (array3 != null)
					{
						if (xelement.HasAttribute("icontint"))
						{
							customIconTint = xelement.GetAttribute("icontint").Split(',', StringSplitOptions.None);
						}
						else
						{
							customIconTint = new string[array3.Length];
						}
					}
					if (xelement.HasAttribute("name"))
					{
						customName = xelement.GetAttribute("name").Split(',', StringSplitOptions.None);
					}
					if (xelement.HasAttribute("name_key"))
					{
						string[] array4 = xelement.GetAttribute("name_key").Split(',', StringSplitOptions.None);
						for (int j = 0; j < array4.Length; j++)
						{
							array4[j] = Localization.Get(array4[j], false);
						}
						customName = array4;
					}
					ProgressionClass.DisplayData displayData = progressionClass.AddDisplayData(item, array2, array3, customIconTint, customName, customHasQuality);
					if (displayData.ItemName != "")
					{
						displayData.AddUnlockData(displayData.ItemName, 0, null);
					}
					if (xelement.HasElements)
					{
						foreach (XElement element2 in xelement.Elements("unlock_entry"))
						{
							int unlockTier = 0;
							if (element2.HasAttribute("unlock_tier"))
							{
								unlockTier = StringParsers.ParseSInt32(element2.GetAttribute("unlock_tier"), 0, -1, NumberStyles.Integer) - 1;
							}
							string[] recipeList = null;
							if (element2.HasAttribute("recipes"))
							{
								recipeList = element2.GetAttribute("recipes").Split(',', StringSplitOptions.None);
							}
							if (element2.HasAttribute("item"))
							{
								string[] array5 = element2.GetAttribute("item").Split(',', StringSplitOptions.None);
								for (int k = 0; k < array5.Length; k++)
								{
									displayData.AddUnlockData(array5[k], unlockTier, recipeList);
								}
							}
						}
					}
				}
			}
			progressionClass.Effects = MinEffectController.ParseXml(childElement, null, MinEffectController.SourceParentType.ProgressionClass, progressionClass.Name);
			Progression.ProgressionClasses.Add(progressionClass.Name, progressionClass);
		}
	}
}
