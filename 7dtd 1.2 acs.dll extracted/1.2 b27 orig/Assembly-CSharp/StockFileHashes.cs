﻿using System;
using System.IO;
using System.Linq;

public class StockFileHashes
{
	public static bool HasStockXMLs()
	{
		string applicationPath = GameIO.GetApplicationPath();
		bool result = true;
		foreach (StockFileHashes.HashDef hashDef in StockFileHashes.hashDefinitions)
		{
			if (File.Exists(applicationPath + "/" + hashDef.filename) && !IOUtils.CalcHashSync(applicationPath + "/" + hashDef.filename, "MD5").SequenceEqual(hashDef.hash))
			{
				Log.Out("Wrong hash on " + hashDef.filename);
				result = false;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static StockFileHashes.HashDef[] hashDefinitions = new StockFileHashes.HashDef[]
	{
		new StockFileHashes.HashDef("Data/Config/archetypes.xml", new byte[]
		{
			25,
			51,
			133,
			174,
			196,
			148,
			4,
			198,
			233,
			159,
			173,
			66,
			98,
			179,
			201,
			183
		}),
		new StockFileHashes.HashDef("Data/Config/biomes.xml", new byte[]
		{
			233,
			168,
			34,
			134,
			171,
			175,
			132,
			152,
			133,
			102,
			65,
			203,
			183,
			92,
			201,
			56
		}),
		new StockFileHashes.HashDef("Data/Config/blockplaceholders.xml", new byte[]
		{
			229,
			97,
			168,
			78,
			76,
			85,
			152,
			24,
			103,
			84,
			12,
			47,
			163,
			159,
			217,
			35
		}),
		new StockFileHashes.HashDef("Data/Config/blocks.xml", new byte[]
		{
			57,
			2,
			171,
			29,
			86,
			238,
			86,
			193,
			192,
			22,
			16,
			245,
			117,
			216,
			166,
			96
		}),
		new StockFileHashes.HashDef("Data/Config/buffs.xml", new byte[]
		{
			62,
			219,
			122,
			112,
			236,
			173,
			35,
			60,
			150,
			202,
			196,
			114,
			164,
			146,
			73,
			147
		}),
		new StockFileHashes.HashDef("Data/Config/challenges.xml", new byte[]
		{
			130,
			121,
			237,
			108,
			135,
			187,
			112,
			53,
			142,
			55,
			55,
			123,
			227,
			151,
			197,
			79
		}),
		new StockFileHashes.HashDef("Data/Config/devicesgameprefs.xml", new byte[]
		{
			116,
			216,
			164,
			112,
			212,
			99,
			189,
			83,
			67,
			146,
			21,
			153,
			137,
			190,
			111,
			105
		}),
		new StockFileHashes.HashDef("Data/Config/dialogs.xml", new byte[]
		{
			35,
			201,
			149,
			2,
			241,
			201,
			196,
			179,
			84,
			72,
			235,
			78,
			7,
			182,
			157,
			9
		}),
		new StockFileHashes.HashDef("Data/Config/dmscontent.xml", new byte[]
		{
			227,
			228,
			249,
			130,
			165,
			104,
			103,
			226,
			40,
			122,
			227,
			135,
			209,
			230,
			224,
			253
		}),
		new StockFileHashes.HashDef("Data/Config/entityclasses.xml", new byte[]
		{
			253,
			189,
			63,
			16,
			150,
			6,
			151,
			104,
			156,
			181,
			111,
			81,
			235,
			141,
			217,
			112
		}),
		new StockFileHashes.HashDef("Data/Config/entitygroups.xml", new byte[]
		{
			197,
			68,
			83,
			231,
			172,
			193,
			106,
			221,
			3,
			63,
			62,
			154,
			9,
			41,
			25,
			138
		}),
		new StockFileHashes.HashDef("Data/Config/events.xml", new byte[]
		{
			182,
			79,
			153,
			68,
			89,
			55,
			110,
			44,
			144,
			175,
			7,
			100,
			227,
			247,
			172,
			249
		}),
		new StockFileHashes.HashDef("Data/Config/gameevents.xml", new byte[]
		{
			218,
			15,
			191,
			5,
			234,
			154,
			148,
			13,
			22,
			174,
			200,
			55,
			195,
			130,
			226,
			162
		}),
		new StockFileHashes.HashDef("Data/Config/gamestages.xml", new byte[]
		{
			24,
			18,
			11,
			95,
			92,
			12,
			211,
			82,
			184,
			146,
			6,
			163,
			124,
			88,
			86,
			69
		}),
		new StockFileHashes.HashDef("Data/Config/items.xml", new byte[]
		{
			160,
			2,
			8,
			41,
			139,
			196,
			30,
			127,
			56,
			151,
			214,
			199,
			111,
			228,
			39,
			140
		}),
		new StockFileHashes.HashDef("Data/Config/item_modifiers.xml", new byte[]
		{
			95,
			242,
			127,
			114,
			58,
			68,
			19,
			198,
			48,
			138,
			180,
			122,
			153,
			174,
			26,
			180
		}),
		new StockFileHashes.HashDef("Data/Config/loadingscreen.xml", new byte[]
		{
			152,
			75,
			81,
			54,
			100,
			18,
			199,
			72,
			63,
			121,
			192,
			76,
			70,
			38,
			222,
			87
		}),
		new StockFileHashes.HashDef("Data/Config/loot.xml", new byte[]
		{
			135,
			35,
			84,
			151,
			42,
			57,
			223,
			7,
			182,
			35,
			202,
			225,
			246,
			181,
			161,
			39
		}),
		new StockFileHashes.HashDef("Data/Config/materials.xml", new byte[]
		{
			131,
			72,
			242,
			29,
			26,
			222,
			221,
			51,
			147,
			71,
			102,
			55,
			140,
			17,
			153,
			234
		}),
		new StockFileHashes.HashDef("Data/Config/misc.xml", new byte[]
		{
			6,
			30,
			64,
			210,
			57,
			24,
			111,
			218,
			220,
			196,
			79,
			71,
			82,
			100,
			147,
			202
		}),
		new StockFileHashes.HashDef("Data/Config/music.xml", new byte[]
		{
			217,
			163,
			53,
			240,
			52,
			17,
			162,
			81,
			253,
			105,
			18,
			110,
			145,
			228,
			245,
			66
		}),
		new StockFileHashes.HashDef("Data/Config/nav_objects.xml", new byte[]
		{
			168,
			62,
			237,
			11,
			204,
			81,
			35,
			31,
			155,
			12,
			81,
			165,
			107,
			229,
			172,
			190
		}),
		new StockFileHashes.HashDef("Data/Config/npc.xml", new byte[]
		{
			230,
			87,
			31,
			138,
			202,
			206,
			185,
			148,
			242,
			155,
			157,
			160,
			248,
			162,
			39,
			216
		}),
		new StockFileHashes.HashDef("Data/Config/painting.xml", new byte[]
		{
			231,
			210,
			164,
			172,
			55,
			227,
			69,
			43,
			94,
			172,
			110,
			155,
			66,
			192,
			149,
			221
		}),
		new StockFileHashes.HashDef("Data/Config/physicsbodies.xml", new byte[]
		{
			177,
			133,
			161,
			23,
			111,
			175,
			24,
			168,
			56,
			192,
			212,
			201,
			206,
			240,
			191,
			49
		}),
		new StockFileHashes.HashDef("Data/Config/progression.xml", new byte[]
		{
			232,
			50,
			54,
			14,
			38,
			233,
			82,
			32,
			222,
			108,
			231,
			133,
			218,
			214,
			69,
			55
		}),
		new StockFileHashes.HashDef("Data/Config/qualityinfo.xml", new byte[]
		{
			245,
			42,
			38,
			133,
			161,
			89,
			249,
			251,
			119,
			10,
			19,
			138,
			15,
			146,
			239,
			133
		}),
		new StockFileHashes.HashDef("Data/Config/quests.xml", new byte[]
		{
			114,
			168,
			34,
			42,
			27,
			234,
			140,
			188,
			58,
			71,
			202,
			13,
			60,
			239,
			135,
			99
		}),
		new StockFileHashes.HashDef("Data/Config/recipes.xml", new byte[]
		{
			174,
			151,
			2,
			207,
			80,
			63,
			115,
			107,
			219,
			240,
			21,
			179,
			239,
			239,
			112,
			12
		}),
		new StockFileHashes.HashDef("Data/Config/rwgmixer.xml", new byte[]
		{
			6,
			128,
			38,
			68,
			91,
			157,
			142,
			186,
			114,
			107,
			201,
			205,
			140,
			34,
			30,
			232
		}),
		new StockFileHashes.HashDef("Data/Config/shapes.xml", new byte[]
		{
			175,
			69,
			48,
			250,
			11,
			156,
			189,
			114,
			42,
			237,
			113,
			56,
			95,
			190,
			128,
			252
		}),
		new StockFileHashes.HashDef("Data/Config/sounds.xml", new byte[]
		{
			115,
			38,
			33,
			128,
			194,
			32,
			1,
			231,
			92,
			108,
			62,
			217,
			232,
			64,
			75,
			212
		}),
		new StockFileHashes.HashDef("Data/Config/spawning.xml", new byte[]
		{
			129,
			80,
			132,
			43,
			189,
			243,
			188,
			49,
			128,
			55,
			157,
			55,
			73,
			52,
			156,
			121
		}),
		new StockFileHashes.HashDef("Data/Config/subtitles.xml", new byte[]
		{
			4,
			56,
			213,
			81,
			5,
			204,
			124,
			203,
			30,
			73,
			149,
			245,
			157,
			19,
			225,
			150
		}),
		new StockFileHashes.HashDef("Data/Config/traders.xml", new byte[]
		{
			182,
			177,
			47,
			46,
			15,
			192,
			20,
			109,
			130,
			168,
			53,
			68,
			61,
			131,
			3,
			253
		}),
		new StockFileHashes.HashDef("Data/Config/twitch.xml", new byte[]
		{
			130,
			193,
			122,
			121,
			191,
			88,
			49,
			134,
			80,
			226,
			218,
			102,
			173,
			105,
			79,
			180
		}),
		new StockFileHashes.HashDef("Data/Config/twitch_events.xml", new byte[]
		{
			237,
			183,
			148,
			133,
			226,
			153,
			210,
			134,
			94,
			227,
			141,
			252,
			133,
			67,
			69,
			181
		}),
		new StockFileHashes.HashDef("Data/Config/ui_display.xml", new byte[]
		{
			83,
			173,
			175,
			239,
			82,
			51,
			198,
			129,
			228,
			31,
			33,
			244,
			166,
			0,
			124,
			238
		}),
		new StockFileHashes.HashDef("Data/Config/utilityai.xml", new byte[]
		{
			148,
			66,
			30,
			155,
			79,
			240,
			189,
			89,
			140,
			199,
			156,
			38,
			31,
			46,
			220,
			226
		}),
		new StockFileHashes.HashDef("Data/Config/vehicles.xml", new byte[]
		{
			185,
			57,
			37,
			203,
			102,
			122,
			192,
			239,
			5,
			150,
			210,
			211,
			143,
			121,
			108,
			45
		}),
		new StockFileHashes.HashDef("Data/Config/videos.xml", new byte[]
		{
			178,
			87,
			79,
			101,
			15,
			121,
			124,
			227,
			47,
			212,
			249,
			178,
			188,
			149,
			13,
			90
		}),
		new StockFileHashes.HashDef("Data/Config/weathersurvival.xml", new byte[]
		{
			231,
			248,
			75,
			252,
			204,
			81,
			44,
			189,
			206,
			14,
			186,
			174,
			39,
			115,
			216,
			204
		}),
		new StockFileHashes.HashDef("Data/Config/worldglobal.xml", new byte[]
		{
			32,
			198,
			191,
			36,
			139,
			180,
			31,
			150,
			164,
			194,
			154,
			121,
			141,
			160,
			221,
			130
		}),
		new StockFileHashes.HashDef("Data/Config/XUi/controls.xml", new byte[]
		{
			207,
			55,
			195,
			130,
			86,
			136,
			123,
			109,
			167,
			245,
			171,
			72,
			210,
			253,
			110,
			51
		}),
		new StockFileHashes.HashDef("Data/Config/XUi/styles.xml", new byte[]
		{
			85,
			175,
			136,
			53,
			223,
			110,
			185,
			11,
			201,
			158,
			162,
			237,
			134,
			74,
			144,
			186
		}),
		new StockFileHashes.HashDef("Data/Config/XUi/windows.xml", new byte[]
		{
			170,
			193,
			39,
			244,
			248,
			149,
			254,
			189,
			53,
			143,
			237,
			244,
			251,
			160,
			173,
			196
		}),
		new StockFileHashes.HashDef("Data/Config/XUi/xui.xml", new byte[]
		{
			38,
			94,
			70,
			242,
			245,
			44,
			44,
			96,
			181,
			123,
			69,
			244,
			8,
			121,
			253,
			221
		}),
		new StockFileHashes.HashDef("Data/Config/XUi_Common/controls.xml", new byte[]
		{
			107,
			19,
			156,
			117,
			189,
			156,
			60,
			123,
			89,
			183,
			158,
			19,
			144,
			166,
			42,
			112
		}),
		new StockFileHashes.HashDef("Data/Config/XUi_Common/styles.xml", new byte[]
		{
			246,
			225,
			86,
			105,
			127,
			10,
			172,
			191,
			106,
			206,
			141,
			254,
			115,
			145,
			41,
			159
		})
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public struct HashDef
	{
		public HashDef(string _filename, byte[] _hash)
		{
			this.filename = _filename;
			this.hash = _hash;
		}

		public string filename;

		public byte[] hash;
	}
}
