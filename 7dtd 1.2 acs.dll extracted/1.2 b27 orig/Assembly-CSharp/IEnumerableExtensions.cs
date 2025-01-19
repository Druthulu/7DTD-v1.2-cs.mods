﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[PublicizedFrom(EAccessModifier.Internal)]
public static class IEnumerableExtensions
{
	[Obsolete("Causes allocations, use Length or Count==0")]
	public static bool IsEmpty<a>(this IEnumerable<a> A_0)
	{
		return !A_0.Any<a>();
	}

	public static string Join<T>(this IEnumerable<T> A_0)
	{
		return string.Join(", ", A_0.Select(new Func<T, string>(IEnumerableExtensions.Join<T>)).ToArray<string>());
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public static string Join<T>(T A_0)
	{
		return A_0.ToString();
	}
}
