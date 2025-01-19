using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

public class GameStagesFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		GameStageGroup.Clear();
		List<GameStagesFromXml.Group> list = new List<GameStagesFromXml.Group>();
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new XmlLoadException("gamestages.xml", root, "Missing root element!");
		}
		foreach (XElement xelement in root.Elements())
		{
			if (xelement.Name == "spawner")
			{
				GameStagesFromXml.ParseGameStageDef(xelement);
			}
			else if (xelement.Name == "group")
			{
				GameStagesFromXml.Group item = GameStagesFromXml.ParseGameStageGroup(xelement);
				list.Add(item);
			}
			else if (xelement.Name == "config")
			{
				if (xelement.HasAttribute("startingWeight"))
				{
					GameStageDefinition.StartingWeight = StringParsers.ParseFloat(xelement.GetAttribute("startingWeight"), 0, -1, NumberStyles.Any);
				}
				if (xelement.HasAttribute("difficultyBonus"))
				{
					GameStageDefinition.DifficultyBonus = StringParsers.ParseFloat(xelement.GetAttribute("difficultyBonus"), 0, -1, NumberStyles.Any);
				}
				if (xelement.HasAttribute("daysAliveChangeWhenKilled"))
				{
					GameStageDefinition.DaysAliveChangeWhenKilled = long.Parse(xelement.GetAttribute("daysAliveChangeWhenKilled"));
				}
				if (xelement.HasAttribute("diminishingReturns"))
				{
					GameStageDefinition.DiminishingReturns = StringParsers.ParseFloat(xelement.GetAttribute("diminishingReturns"), 0, -1, NumberStyles.Any);
				}
				if (xelement.HasAttribute("lootBonusEvery"))
				{
					GameStageDefinition.LootBonusEvery = int.Parse(xelement.GetAttribute("lootBonusEvery"));
				}
				if (xelement.HasAttribute("lootBonusMaxCount"))
				{
					GameStageDefinition.LootBonusMaxCount = int.Parse(xelement.GetAttribute("lootBonusMaxCount"));
				}
				if (xelement.HasAttribute("lootBonusScale"))
				{
					GameStageDefinition.LootBonusScale = StringParsers.ParseFloat(xelement.GetAttribute("lootBonusScale"), 0, -1, NumberStyles.Any);
				}
				string attribute;
				if ((attribute = xelement.GetAttribute("lootWanderingBonusEvery")).Length > 0)
				{
					GameStageDefinition.LootWanderingBonusEvery = int.Parse(attribute);
				}
				if ((attribute = xelement.GetAttribute("lootWanderingBonusScale")).Length > 0)
				{
					GameStageDefinition.LootWanderingBonusScale = StringParsers.ParseFloat(attribute, 0, -1, NumberStyles.Any);
				}
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			GameStagesFromXml.Group group = list[i];
			string text = group.spawnerName;
			if (string.IsNullOrEmpty(text))
			{
				text = "SleeperGSList";
			}
			GameStageDefinition spawner;
			if (!GameStageDefinition.TryGetGameStage(text, out spawner))
			{
				throw new XmlLoadException("gamestages.xml", group.element, string.Concat(new string[]
				{
					"Group '",
					group.name,
					"': Spawner '",
					text,
					"' not found!"
				}));
			}
			GameStageGroup group2 = new GameStageGroup(spawner);
			GameStageGroup.AddGameStageGroup(group.name, group2);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameStagesFromXml.Group ParseGameStageGroup(XElement root)
	{
		string attribute = root.GetAttribute("name");
		if (attribute.Length == 0)
		{
			throw new XmlLoadException("gamestages.xml", root, "<group> missing name!");
		}
		string attribute2 = root.GetAttribute("spawner");
		return new GameStagesFromXml.Group(attribute, attribute2, root);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseGameStageDef(XElement root)
	{
		string attribute = root.GetAttribute("name");
		if (attribute.Length == 0)
		{
			throw new XmlLoadException("gamestages.xml", root, "<spawner> missing name!");
		}
		GameStageDefinition gameStageDefinition = new GameStageDefinition(attribute);
		foreach (XElement root2 in root.Elements("gamestage"))
		{
			GameStagesFromXml.ParseStage(gameStageDefinition, root2);
		}
		GameStageDefinition.AddGameStage(gameStageDefinition);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseStage(GameStageDefinition gsd, XElement root)
	{
		string attribute = root.GetAttribute("stage");
		if (attribute.Length == 0)
		{
			throw new XmlLoadException("gamestages.xml", root, "GameStage " + gsd.name + " sub element is missing stage!");
		}
		GameStageDefinition.Stage stage = new GameStageDefinition.Stage(int.Parse(attribute));
		foreach (XElement root2 in root.Elements("spawn"))
		{
			GameStagesFromXml.ParseSpawn(gsd, stage, root2);
		}
		if (stage.Count > 0)
		{
			gsd.AddStage(stage);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseSpawn(GameStageDefinition gsd, GameStageDefinition.Stage stage, XElement root)
	{
		string attribute = root.GetAttribute("group");
		if (attribute.Length == 0)
		{
			throw new XmlLoadException("gamestages.xml", root, "<spawn> is missing group!");
		}
		if (!EntityGroups.list.ContainsKey(attribute))
		{
			throw new XmlLoadException("gamestages.xml", root, string.Format("Spawner '{0}', gamestage {1}: EntityGroup '{2}' unknown!", gsd.name, stage.stageNum, attribute));
		}
		int spawnCount = 1;
		root.ParseAttribute("num", ref spawnCount);
		int maxAlive = 1;
		root.ParseAttribute("maxAlive", ref maxAlive);
		float interval = 0f;
		root.ParseAttribute("interval", ref interval);
		ulong duration = 1UL;
		root.ParseAttribute("duration", ref duration);
		GameStageDefinition.SpawnGroup spawn = new GameStageDefinition.SpawnGroup(attribute, spawnCount, maxAlive, interval, duration);
		stage.AddSpawnGroup(spawn);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string XMLName = "gamestages.xml";

	[PublicizedFrom(EAccessModifier.Private)]
	public struct Group
	{
		public Group(string _name, string _spawnerName, XElement _element)
		{
			this.name = _name;
			this.spawnerName = _spawnerName;
			this.element = _element;
		}

		public readonly string name;

		public readonly string spawnerName;

		public readonly XElement element;
	}
}
