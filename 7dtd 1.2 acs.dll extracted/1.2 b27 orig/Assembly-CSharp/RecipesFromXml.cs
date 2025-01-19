using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

public class RecipesFromXml
{
	public static bool SaveRecipes(string _filename)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement node = xmlDocument.AddXmlElement("recipes");
		foreach (Recipe recipe in CraftingManager.GetAllRecipes())
		{
			XmlElement xmlElement = node.AddXmlElement("recipe").SetAttrib("name", recipe.GetName()).SetAttrib("count", recipe.count.ToString()).SetAttrib("scrapable", recipe.scrapable.ToString());
			if (recipe.tooltip != null)
			{
				xmlElement.SetAttrib("tooltip", recipe.tooltip);
			}
			if (!string.IsNullOrEmpty(recipe.craftingArea))
			{
				xmlElement.SetAttrib("craft_area", recipe.craftingArea);
			}
			if (recipe.craftingToolType != 0)
			{
				xmlElement.SetAttrib("craft_tool", ItemClass.GetForId(recipe.craftingToolType).GetItemName());
			}
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemStack itemStack = recipe.ingredients[i];
				if (((itemStack != null) ? itemStack.itemValue : null) != null && ItemClass.GetForId(itemStack.itemValue.type) != null)
				{
					xmlElement.AddXmlElement("ingredient").SetAttrib("name", ItemClass.GetForId(itemStack.itemValue.type).GetItemName()).SetAttrib("count", itemStack.count.ToString());
				}
			}
			if (recipe.wildcardForgeCategory)
			{
				xmlElement.AddXmlElement("wildcard_forge_category");
			}
		}
		xmlDocument.SdSave(_filename);
		return true;
	}

	public static IEnumerator LoadRecipies(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <recipes> found!");
		}
		MicroStopwatch msw = new MicroStopwatch(true);
		foreach (XElement xelement in root.Elements("recipe"))
		{
			Recipe recipe = new Recipe();
			if (!xelement.HasAttribute("name"))
			{
				throw new Exception("Attribute 'name' missing on recipe");
			}
			string attribute = xelement.GetAttribute("name");
			recipe.itemValueType = ItemClass.GetItem(attribute, false).type;
			if (recipe.itemValueType == 0)
			{
				throw new Exception("No item/block with name '" + attribute + "' existing");
			}
			recipe.count = 1;
			if (xelement.HasAttribute("count"))
			{
				recipe.count = int.Parse(xelement.GetAttribute("count"));
			}
			recipe.scrapable = false;
			if (xelement.HasAttribute("scrapable"))
			{
				recipe.scrapable = StringParsers.ParseBool(xelement.GetAttribute("scrapable"), 0, -1, true);
			}
			recipe.materialBasedRecipe = false;
			if (xelement.HasAttribute("material_based"))
			{
				recipe.materialBasedRecipe = StringParsers.ParseBool(xelement.GetAttribute("material_based"), 0, -1, true);
			}
			if (xelement.HasAttribute("tags"))
			{
				recipe.tags = FastTags<TagGroup.Global>.Parse(xelement.GetAttribute("tags") + "," + attribute);
			}
			else if (xelement.HasAttribute("tag"))
			{
				recipe.tags = FastTags<TagGroup.Global>.Parse(xelement.GetAttribute("tag") + "," + attribute);
			}
			if (xelement.HasAttribute("tooltip"))
			{
				recipe.tooltip = xelement.GetAttribute("tooltip");
			}
			if (xelement.HasAttribute("craft_area"))
			{
				string attribute2 = xelement.GetAttribute("craft_area");
				recipe.craftingArea = attribute2;
			}
			else
			{
				recipe.craftingArea = "";
			}
			if (xelement.HasAttribute("craft_tool"))
			{
				recipe.craftingToolType = ItemClass.GetItem(xelement.GetAttribute("craft_tool"), false).type;
				ItemClass.list[ItemClass.GetItem(xelement.GetAttribute("craft_tool"), false).type].bCraftingTool = true;
			}
			else
			{
				recipe.craftingToolType = 0;
			}
			if (xelement.HasAttribute("craft_time"))
			{
				float craftingTime = 0f;
				StringParsers.TryParseFloat(xelement.GetAttribute("craft_time"), out craftingTime, 0, -1, NumberStyles.Any);
				recipe.craftingTime = craftingTime;
			}
			else
			{
				recipe.craftingTime = -1f;
			}
			if (xelement.HasAttribute("learn_exp_gain"))
			{
				float num = 0f;
				if (StringParsers.TryParseFloat(xelement.GetAttribute("learn_exp_gain"), out num, 0, -1, NumberStyles.Any))
				{
					recipe.unlockExpGain = (int)num;
				}
				else
				{
					recipe.unlockExpGain = 20;
				}
			}
			else
			{
				recipe.unlockExpGain = -1;
			}
			if (xelement.HasAttribute("craft_exp_gain"))
			{
				float num2 = 0f;
				if (StringParsers.TryParseFloat(xelement.GetAttribute("craft_exp_gain"), out num2, 0, -1, NumberStyles.Any))
				{
					recipe.craftExpGain = (int)num2;
				}
				else
				{
					recipe.craftExpGain = 1;
				}
			}
			else
			{
				recipe.craftExpGain = -1;
			}
			if (xelement.HasAttribute("is_trackable"))
			{
				recipe.IsTrackable = StringParsers.ParseBool(xelement.GetAttribute("is_trackable"), 0, -1, true);
			}
			else
			{
				recipe.IsTrackable = true;
			}
			recipe.UseIngredientModifier = true;
			if (xelement.HasAttribute("use_ingredient_modifier"))
			{
				recipe.UseIngredientModifier = StringParsers.ParseBool(xelement.GetAttribute("use_ingredient_modifier"), 0, -1, true);
			}
			recipe.Effects = MinEffectController.ParseXml(xelement, null, MinEffectController.SourceParentType.None, null);
			foreach (XElement xelement2 in xelement.Elements())
			{
				if (xelement2.Name == "ingredient")
				{
					XElement element = xelement2;
					if (!element.HasAttribute("name"))
					{
						throw new Exception("Attribute 'name' missing on ingredient in recipe '" + attribute + "'");
					}
					string attribute3 = element.GetAttribute("name");
					ItemValue item = ItemClass.GetItem(attribute3, false);
					if (item.IsEmpty())
					{
						throw new Exception("No item/block/material with name '" + attribute3 + "' existing");
					}
					int count = 1;
					if (element.HasAttribute("count"))
					{
						count = int.Parse(element.GetAttribute("count"));
					}
					recipe.AddIngredient(item, count);
				}
				else if (xelement2.Name == "wildcard_forge_category")
				{
					recipe.wildcardForgeCategory = true;
				}
			}
			recipe.Init();
			CraftingManager.AddRecipe(recipe);
			if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		IEnumerator<XElement> enumerator = null;
		CraftingManager.PostInit();
		yield break;
		yield break;
	}
}
