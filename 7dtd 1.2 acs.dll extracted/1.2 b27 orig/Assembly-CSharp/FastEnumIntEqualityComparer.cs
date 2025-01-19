﻿using System;
using System.Collections.Generic;

public class FastEnumIntEqualityComparer<TEnum> : IEqualityComparer<TEnum> where TEnum : struct, IConvertible
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int ToInt(TEnum _enum)
	{
		return EnumInt32ToInt.Convert<TEnum>(_enum);
	}

	public bool Equals(TEnum firstEnum, TEnum secondEnum)
	{
		return this.ToInt(firstEnum) == this.ToInt(secondEnum);
	}

	public int GetHashCode(TEnum firstEnum)
	{
		return this.ToInt(firstEnum);
	}
}
