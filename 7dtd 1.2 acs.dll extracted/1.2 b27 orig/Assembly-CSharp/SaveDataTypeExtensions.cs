using System;

public static class SaveDataTypeExtensions
{
	public static bool IsRoot(this SaveDataType saveDataType)
	{
		return saveDataType == SaveDataType.User;
	}

	public static int GetSlotPathDepth(this SaveDataType saveDataType)
	{
		switch (saveDataType)
		{
		case SaveDataType.User:
			return 0;
		case SaveDataType.Saves:
			return 2;
		case SaveDataType.SavesLocal:
			return 1;
		case SaveDataType.GeneratedWorlds:
			return 1;
		default:
			Log.Error(string.Format("{0}.{1} does not have a slot path length, defaulting to '0'.", "SaveDataType", saveDataType));
			return 0;
		}
	}

	public static string GetPathRaw(this SaveDataType saveDataType)
	{
		string result;
		switch (saveDataType)
		{
		case SaveDataType.User:
			result = string.Empty;
			break;
		case SaveDataType.Saves:
			result = "Saves";
			break;
		case SaveDataType.SavesLocal:
			result = "SavesLocal";
			break;
		case SaveDataType.GeneratedWorlds:
			result = "GeneratedWorlds";
			break;
		default:
			throw new ArgumentOutOfRangeException("saveDataType", saveDataType, string.Format("No path specified for {0}.", saveDataType));
		}
		return result;
	}

	public static SaveDataManagedPath GetPath(this SaveDataType saveDataType)
	{
		SaveDataManagedPath result;
		switch (saveDataType)
		{
		case SaveDataType.User:
			result = SaveDataTypeExtensions.s_rootPathUser;
			break;
		case SaveDataType.Saves:
			result = SaveDataTypeExtensions.s_rootPathSaves;
			break;
		case SaveDataType.SavesLocal:
			result = SaveDataTypeExtensions.s_rootPathSavesLocal;
			break;
		case SaveDataType.GeneratedWorlds:
			result = SaveDataTypeExtensions.s_rootPathGeneratedWorlds;
			break;
		default:
			throw new ArgumentOutOfRangeException("saveDataType", saveDataType, string.Format("No relative path specified for {0}.", saveDataType));
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SaveDataManagedPath s_rootPathUser = new SaveDataManagedPath(SaveDataType.User.GetPathRaw());

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SaveDataManagedPath s_rootPathSaves = new SaveDataManagedPath(SaveDataType.Saves.GetPathRaw());

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SaveDataManagedPath s_rootPathSavesLocal = new SaveDataManagedPath(SaveDataType.SavesLocal.GetPathRaw());

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SaveDataManagedPath s_rootPathGeneratedWorlds = new SaveDataManagedPath(SaveDataType.GeneratedWorlds.GetPathRaw());
}
