﻿using System;
using System.Globalization;

public static class ILaunchPrefExtensions
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string CreateCommandLineArgument(ILaunchPref pref, string value)
	{
		return "-" + pref.Name + "=" + value;
	}

	public static string ToCommandLine(this ILaunchPref<string> pref, string value)
	{
		return ILaunchPrefExtensions.CreateCommandLineArgument(pref, value);
	}

	public static string ToCommandLine(this ILaunchPref<long> pref, long value)
	{
		return ILaunchPrefExtensions.CreateCommandLineArgument(pref, value.ToString(CultureInfo.InvariantCulture));
	}

	public static string ToCommandLine(this ILaunchPref<bool> pref, bool value)
	{
		return ILaunchPrefExtensions.CreateCommandLineArgument(pref, value.ToString(CultureInfo.InvariantCulture));
	}
}
