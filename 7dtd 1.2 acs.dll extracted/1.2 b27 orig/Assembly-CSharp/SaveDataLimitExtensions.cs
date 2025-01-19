using System;

public static class SaveDataLimitExtensions
{
	public static bool IsSupported(this SaveDataLimitType saveDataLimitType)
	{
		return !PlatformOptimizations.LimitedSaveData || saveDataLimitType > SaveDataLimitType.Unlimited;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static long GetRegionSizeLimit(this SaveDataLimitType saveDataLimitType)
	{
		if (!saveDataLimitType.IsSupported())
		{
			throw new ArgumentException(string.Format("Unexpected usage of {0}.{1}.{2}() when not supported by the current device.", "SaveDataLimitType", saveDataLimitType, "GetRegionSizeLimit"), "saveDataLimitType");
		}
		long result;
		switch (saveDataLimitType)
		{
		case SaveDataLimitType.Unlimited:
			result = -1L;
			break;
		case SaveDataLimitType.Short:
			result = 33554432L;
			break;
		case SaveDataLimitType.Medium:
			result = 67108864L;
			break;
		case SaveDataLimitType.Long:
			result = 134217728L;
			break;
		case SaveDataLimitType.VeryLong:
			result = 268435456L;
			break;
		default:
			throw new ArgumentOutOfRangeException("saveDataLimitType", saveDataLimitType, null);
		}
		return result;
	}

	public static long CalculateTotalSize(this SaveDataLimitType saveDataLimitType, Vector2i worldSize)
	{
		if (!saveDataLimitType.IsSupported())
		{
			throw new ArgumentException(string.Format("Unexpected usage of {0}.{1}.{2}() when not supported by the current device.", "SaveDataLimitType", saveDataLimitType, "CalculateTotalSize"), "saveDataLimitType");
		}
		long regionSizeLimit = saveDataLimitType.GetRegionSizeLimit();
		if (regionSizeLimit <= 0L)
		{
			return -1L;
		}
		long num = SaveDataLimitUtils.CalculatePlayerMapSize(worldSize);
		return 104857600L + num + regionSizeLimit;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const long MB = 1048576L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long FLAT_OVERHEAD = 104857600L;
}
