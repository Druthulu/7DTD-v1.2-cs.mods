﻿using System;
using Platform;
using UnityEngine;

public static class GameOptionsPlatforms
{
	public static float GetStreamingMipmapBudget()
	{
		return (float)SystemInfo.graphicsMemorySize * 0.9f;
	}

	public static string GetItemIconFilterString()
	{
		string result = "mip0";
		if ((float)SystemInfo.graphicsMemorySize <= 3200f || SystemInfo.systemMemorySize < 6800)
		{
			result = "mip1";
		}
		return result;
	}

	public static int CalcTextureQualityMin()
	{
		int systemMemorySize = SystemInfo.systemMemorySize;
		if (SystemInfo.graphicsMemorySize < 2400 || systemMemorySize < 4900)
		{
			return 1;
		}
		return 0;
	}

	public static int CalcDefaultGfxPreset()
	{
		if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
		{
			int result = 2;
			float num = (float)SystemInfo.systemMemorySize;
			float num2 = (float)SystemInfo.graphicsMemorySize;
			if (!SystemInfo.operatingSystem.Contains(" Steam ") && (num2 < 2400f || num < 4800f))
			{
				result = 1;
			}
			if (num2 > 7500f && num > 5200f)
			{
				string text = SystemInfo.graphicsDeviceVendor.ToLower();
				if (text.Contains("nvidia"))
				{
					if (!GameOptionsPlatforms.FindGfxName(" 1070, 305"))
					{
						result = 3;
						if (GameOptionsPlatforms.FindGfxName(" 208, 307, 308, 309, 407, 408, 409, 507, 508, 509"))
						{
							result = 4;
						}
					}
				}
				else if ((text == "amd" || text == "ati") && !GameOptionsPlatforms.FindGfxName("RX 570,RX 580,RX 590,RX 5500,RX 65,RX 66"))
				{
					result = 3;
					if (GameOptionsPlatforms.FindGfxName(" 680, 690, 695, 770, 780, 790, 795, 870, 880, 890, 895"))
					{
						result = 4;
					}
				}
			}
			return result;
		}
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			return 6;
		}
		return 2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool FindGfxName(string names)
	{
		string text = SystemInfo.graphicsDeviceName.ToLower();
		if (text.Contains("laptop"))
		{
			return false;
		}
		string[] array = names.Split(',', StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			if (text.Contains(array[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static int ApplyTextureFilterLimit(int filter)
	{
		if ((float)SystemInfo.graphicsMemorySize < 3200f)
		{
			filter = 0;
		}
		return filter;
	}

	public static class GfxPreset
	{
		public const int Lowest = 0;

		public const int Low = 1;

		public const int Medium = 2;

		public const int High = 3;

		public const int Ultra = 4;

		public const int Custom = 5;

		public const int ConsolePerformance = 6;

		public const int ConsolePerformanceFSR = 7;

		public const int ConsoleQuality = 8;

		public const int ConsoleQualityFSR = 9;

		public const int Simplified = 100;
	}
}
