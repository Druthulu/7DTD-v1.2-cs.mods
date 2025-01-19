using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Platform;
using UnityEngine;

public class SaveInfoProvider
{
	public static SaveInfoProvider Instance
	{
		get
		{
			if (SaveInfoProvider.instance == null)
			{
				SaveInfoProvider.instance = new SaveInfoProvider();
			}
			return SaveInfoProvider.instance;
		}
	}

	public static bool DataLimitEnabled
	{
		get
		{
			return SaveDataUtils.SaveDataManager.ShouldLimitSize();
		}
	}

	public static string GetWorldEntryKey(string worldName, string worldType)
	{
		return (worldName + worldType).ToLowerInvariant();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetSaveEntryKey(string worldKey, string saveName)
	{
		return (worldKey + "/" + saveName).ToLowerInvariant();
	}

	public ReadOnlyCollection<SaveInfoProvider.WorldEntryInfo> WorldEntryInfos
	{
		get
		{
			this.RefreshIfDirty();
			return this.worldEntryInfos.AsReadOnly();
		}
	}

	public ReadOnlyCollection<SaveInfoProvider.SaveEntryInfo> SaveEntryInfos
	{
		get
		{
			this.RefreshIfDirty();
			return this.saveEntryInfos.AsReadOnly();
		}
	}

	public ReadOnlyCollection<SaveInfoProvider.PlayerEntryInfo> PlayerEntryInfos
	{
		get
		{
			this.RefreshIfDirty();
			return this.playerEntryInfos.AsReadOnly();
		}
	}

	public long TotalUsedBytes
	{
		get
		{
			this.RefreshIfDirty();
			return this.totalUsedBytes;
		}
	}

	public long TotalAllowanceBytes
	{
		get
		{
			this.RefreshIfDirty();
			return this.totalAllowanceBytes;
		}
	}

	public long TotalAvailableBytes
	{
		get
		{
			this.RefreshIfDirty();
			return this.totalAllowanceBytes - this.totalUsedBytes;
		}
	}

	public void SetDirty()
	{
		this.isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static long GetPlatformReservedSizeBytes(SaveDataSizes sizes)
	{
		return 5242880L;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshIfDirty()
	{
		if (!this.isDirty)
		{
			return;
		}
		this.isDirty = false;
		this.localSavesSum = 0L;
		this.remoteSavesSum = 0L;
		this.worldsSum = 0L;
		this.worldEntryInfosByWorldKey.Clear();
		this.saveEntryInfosBySaveKey.Clear();
		this.remoteSaveEntryInfosByGuid.Clear();
		this.worldEntryInfos.Clear();
		this.saveEntryInfos.Clear();
		this.playerEntryInfos.Clear();
		this.ProcessLocalWorlds();
		this.ProcessLocalWorldSaves();
		this.ProcessRemoteWorldSaves();
		this.worldEntryInfos.AddRange(this.worldEntryInfosByWorldKey.Values);
		this.totalUsedBytes = this.localSavesSum + this.remoteSavesSum + this.worldsSum;
		long num = 0L;
		this.worldEntryInfos.Sort();
		foreach (SaveInfoProvider.WorldEntryInfo worldEntryInfo in this.worldEntryInfos)
		{
			worldEntryInfo.BarStartOffset = num;
			if (worldEntryInfo.Deletable)
			{
				num += worldEntryInfo.WorldDataSize;
			}
			worldEntryInfo.SaveEntryInfos.Sort();
			foreach (SaveInfoProvider.SaveEntryInfo saveEntryInfo in worldEntryInfo.SaveEntryInfos)
			{
				saveEntryInfo.BarStartOffset = num;
				num += saveEntryInfo.SizeInfo.ReportedSize;
				long num2 = num;
				saveEntryInfo.PlayerEntryInfos.Sort();
				for (int i = saveEntryInfo.PlayerEntryInfos.Count - 1; i >= 0; i--)
				{
					SaveInfoProvider.PlayerEntryInfo playerEntryInfo = saveEntryInfo.PlayerEntryInfos[i];
					num2 -= playerEntryInfo.Size;
					playerEntryInfo.BarStartOffset = num2;
				}
			}
		}
		if (SaveDataUtils.SaveDataManager.ShouldLimitSize())
		{
			SaveDataUtils.SaveDataManager.UpdateSizes();
			SaveDataSizes sizes = SaveDataUtils.SaveDataManager.GetSizes();
			long platformReservedSizeBytes = SaveInfoProvider.GetPlatformReservedSizeBytes(sizes);
			this.totalAllowanceBytes = sizes.Total - platformReservedSizeBytes;
			return;
		}
		this.totalAllowanceBytes = -1L;
	}

	public void ClearResources()
	{
		this.worldEntryInfosByWorldKey.Clear();
		this.saveEntryInfosBySaveKey.Clear();
		this.remoteSaveEntryInfosByGuid.Clear();
		this.worldEntryInfos.Clear();
		this.saveEntryInfos.Clear();
		this.playerEntryInfos.Clear();
		this.protectedDirectories.Clear();
		this.isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessLocalWorlds()
	{
		foreach (PathAbstractions.AbstractedLocation abstractedLocation in PathAbstractions.WorldsSearchPaths.GetAvailablePathsList(null, null, null, false))
		{
			PathAbstractions.EAbstractedLocationType type = abstractedLocation.Type;
			string text;
			if (type != PathAbstractions.EAbstractedLocationType.Mods)
			{
				if (type != PathAbstractions.EAbstractedLocationType.GameData)
				{
					text = Localization.Get("xuiDmGenerated", false);
				}
				else
				{
					text = Localization.Get("xuiDmBuiltIn", false);
				}
			}
			else
			{
				text = Localization.Get("xuiDmMod", false) + ": " + abstractedLocation.ContainingMod.Name;
			}
			string type2 = text;
			SaveInfoProvider.WorldEntryInfo worldEntryInfo = new SaveInfoProvider.WorldEntryInfo
			{
				WorldKey = SaveInfoProvider.GetWorldEntryKey(abstractedLocation.Name, "Local"),
				Name = abstractedLocation.Name,
				Type = type2,
				Location = abstractedLocation,
				Deletable = GameIO.IsWorldGenerated(abstractedLocation.Name),
				WorldDataSize = GameIO.GetDirectorySize(abstractedLocation.FullPath, true),
				Version = null,
				HideIfEmpty = SaveInfoProvider.HideableWorlds.ContainsCaseInsensitive(abstractedLocation.Name)
			};
			if (worldEntryInfo.Deletable)
			{
				this.worldsSum += worldEntryInfo.WorldDataSize;
			}
			GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(abstractedLocation);
			if (worldInfo != null)
			{
				worldEntryInfo.Version = worldInfo.GameVersionCreated;
			}
			this.worldEntryInfosByWorldKey[worldEntryInfo.WorldKey] = worldEntryInfo;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessLocalWorldSaves()
	{
		string saveGameRootDir = GameIO.GetSaveGameRootDir();
		if (SdDirectory.Exists(saveGameRootDir))
		{
			SdFileSystemInfo[] array = new SdDirectoryInfo(saveGameRootDir).GetDirectories();
			foreach (SdDirectoryInfo sdDirectoryInfo in array)
			{
				if (SdDirectory.Exists(sdDirectoryInfo.FullName))
				{
					SdDirectoryInfo sdDirectoryInfo2 = new SdDirectoryInfo(sdDirectoryInfo.FullName);
					SdFileSystemInfo[] array2 = sdDirectoryInfo2.GetDirectories();
					SdFileSystemInfo[] array3 = array2;
					if (array3.Length == 0 && sdDirectoryInfo2.GetFiles().Length == 0)
					{
						SdDirectory.Delete(sdDirectoryInfo.FullName);
					}
					else
					{
						SaveInfoProvider.WorldEntryInfo worldEntryInfo;
						if (!this.worldEntryInfosByWorldKey.TryGetValue(SaveInfoProvider.GetWorldEntryKey(sdDirectoryInfo.Name, "Local"), out worldEntryInfo))
						{
							worldEntryInfo = new SaveInfoProvider.WorldEntryInfo
							{
								WorldKey = SaveInfoProvider.GetWorldEntryKey(sdDirectoryInfo.Name, SaveInfoProvider.DeletedWorldsType),
								Name = sdDirectoryInfo.Name,
								Type = SaveInfoProvider.DeletedWorldsType,
								Location = PathAbstractions.AbstractedLocation.None,
								Deletable = false,
								WorldDataSize = 0L,
								Version = null,
								HideIfEmpty = true
							};
							this.worldEntryInfosByWorldKey[worldEntryInfo.WorldKey] = worldEntryInfo;
						}
						foreach (SdDirectoryInfo curSaveFolder in array3)
						{
							this.ProcessSaveEntry(worldEntryInfo, curSaveFolder);
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessSaveEntry(SaveInfoProvider.WorldEntryInfo worldEntryInfo, SdDirectoryInfo curSaveFolder)
	{
		string text = curSaveFolder.FullName + "/main.ttw";
		SaveInfoProvider.SaveEntryInfo saveEntryInfo = new SaveInfoProvider.SaveEntryInfo
		{
			Name = curSaveFolder.Name,
			WorldEntry = worldEntryInfo,
			SaveDir = curSaveFolder.FullName,
			Version = null
		};
		saveEntryInfo.SizeInfo.BytesOnDisk = GameIO.GetDirectorySize(curSaveFolder, true);
		saveEntryInfo.SizeInfo.IsArchived = (SaveInfoProvider.DataLimitEnabled && SdFile.Exists(Path.Combine(curSaveFolder.FullName, "archived.flag")));
		if (SdFile.Exists(text))
		{
			saveEntryInfo.LastSaved = SdFile.GetLastWriteTime(text);
			try
			{
				WorldState worldState = new WorldState();
				worldState.Load(text, false, false, false);
				saveEntryInfo.Version = worldState.gameVersion;
				if (SaveInfoProvider.DataLimitEnabled && worldState.saveDataLimit != -1L)
				{
					saveEntryInfo.SizeInfo.BytesReserved = worldState.saveDataLimit;
					if (saveEntryInfo.SizeInfo.BytesOnDisk > saveEntryInfo.SizeInfo.BytesReserved)
					{
						Debug.LogError(string.Format("Directory size of save \"{0}\" exceeds serialized save data limit of {1}.", curSaveFolder.FullName, worldState.saveDataLimit));
					}
				}
				else
				{
					saveEntryInfo.SizeInfo.BytesReserved = -1L;
				}
				goto IL_178;
			}
			catch (Exception ex)
			{
				Log.Warning("Error reading header of level '" + text + "'. Msg: " + ex.Message);
				goto IL_178;
			}
		}
		if (curSaveFolder.Name != "WorldEditor" && curSaveFolder.Name != "PrefabEditor")
		{
			Log.Warning(string.Format("Could not find main ttw file for save in directory: {0}", curSaveFolder));
		}
		saveEntryInfo.LastSaved = curSaveFolder.LastWriteTime;
		IL_178:
		this.saveEntryInfos.Add(saveEntryInfo);
		string saveEntryKey = SaveInfoProvider.GetSaveEntryKey(saveEntryInfo.WorldEntry.WorldKey, saveEntryInfo.Name);
		this.saveEntryInfosBySaveKey[saveEntryKey] = saveEntryInfo;
		worldEntryInfo.SaveEntryInfos.Add(saveEntryInfo);
		worldEntryInfo.SaveDataCount++;
		worldEntryInfo.SaveDataSize += saveEntryInfo.SizeInfo.ReportedSize;
		this.localSavesSum += saveEntryInfo.SizeInfo.ReportedSize;
		this.ProcessPlayerEntries(saveEntryInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessPlayerEntries(SaveInfoProvider.SaveEntryInfo saveEntryInfo)
	{
		string text = saveEntryInfo.SaveDir + "/Player";
		if (!SdDirectory.Exists(text))
		{
			return;
		}
		this.fileNameKeysToPlayerInfos.Clear();
		foreach (SdFileInfo sdFileInfo in new SdDirectoryInfo(text).GetFiles())
		{
			int length;
			string text2;
			string a;
			if ((length = sdFileInfo.Name.IndexOf('.')) != -1)
			{
				text2 = sdFileInfo.Name.Substring(0, length);
				int num = sdFileInfo.Name.LastIndexOf('.') + 1;
				a = sdFileInfo.Name.Substring(num, sdFileInfo.Name.Length - num);
			}
			else
			{
				Debug.LogError("Encountered player save file with no extension.");
				text2 = sdFileInfo.Name;
				a = string.Empty;
			}
			DateTime lastWriteTime = SdFile.GetLastWriteTime(sdFileInfo.FullName);
			SaveInfoProvider.PlayerEntryInfo playerEntryInfo;
			if (!this.fileNameKeysToPlayerInfos.TryGetValue(text2, out playerEntryInfo))
			{
				playerEntryInfo = new SaveInfoProvider.PlayerEntryInfo
				{
					Id = text2,
					LastPlayed = lastWriteTime,
					SaveEntry = saveEntryInfo
				};
				PlatformUserIdentifierAbs platformUserIdentifierAbs;
				if (PlatformUserIdentifierAbs.TryFromCombinedString(text2, out platformUserIdentifierAbs))
				{
					playerEntryInfo.PrimaryUserId = platformUserIdentifierAbs;
					playerEntryInfo.CachedName = platformUserIdentifierAbs.ReadablePlatformUserIdentifier;
				}
				else
				{
					Log.Error("Could not associate player save file \"" + sdFileInfo.FullName + "\" with a player id. Combined id string: " + text2);
					playerEntryInfo.CachedName = text2;
				}
				saveEntryInfo.PlayerEntryInfos.Add(playerEntryInfo);
			}
			else if (lastWriteTime > playerEntryInfo.LastPlayed)
			{
				playerEntryInfo.LastPlayed = lastWriteTime;
			}
			playerEntryInfo.Size += sdFileInfo.Length;
			PlayerMetaInfo playerMetaInfo;
			if (string.Equals(a, "meta", StringComparison.InvariantCultureIgnoreCase) && PlayerMetaInfo.TryRead(sdFileInfo.FullName, out playerMetaInfo))
			{
				if (playerMetaInfo.nativeId != null)
				{
					playerEntryInfo.NativeUserId = playerMetaInfo.nativeId;
				}
				if (playerMetaInfo.name != null)
				{
					playerEntryInfo.CachedName = playerMetaInfo.name;
				}
				playerEntryInfo.PlayerLevel = playerMetaInfo.level;
				playerEntryInfo.DistanceWalked = playerMetaInfo.distanceWalked;
			}
			this.fileNameKeysToPlayerInfos[text2] = playerEntryInfo;
		}
		foreach (KeyValuePair<string, SaveInfoProvider.PlayerEntryInfo> keyValuePair in this.fileNameKeysToPlayerInfos)
		{
			long directorySize = GameIO.GetDirectorySize(new SdDirectoryInfo(Path.Combine(text, keyValuePair.Key)), true);
			keyValuePair.Value.Size += directorySize;
			this.playerEntryInfos.Add(keyValuePair.Value);
		}
		this.fileNameKeysToPlayerInfos.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessRemoteWorldSaves()
	{
		string saveGameLocalRootDir = GameIO.GetSaveGameLocalRootDir();
		if (!SdDirectory.Exists(saveGameLocalRootDir))
		{
			return;
		}
		SdFileSystemInfo[] array = new SdDirectoryInfo(saveGameLocalRootDir).GetDirectories();
		foreach (SdDirectoryInfo sdDirectoryInfo in array)
		{
			string text = sdDirectoryInfo.FullName + "/RemoteWorldInfo.xml";
			SaveInfoProvider.SaveEntryInfo saveEntryInfo = new SaveInfoProvider.SaveEntryInfo
			{
				SaveDir = sdDirectoryInfo.FullName
			};
			saveEntryInfo.SizeInfo.BytesOnDisk = GameIO.GetDirectorySize(sdDirectoryInfo, true);
			saveEntryInfo.SizeInfo.IsArchived = (SaveInfoProvider.DataLimitEnabled && SdFile.Exists(Path.Combine(sdDirectoryInfo.FullName, "archived.flag")));
			RemoteWorldInfo remoteWorldInfo;
			SaveInfoProvider.WorldEntryInfo worldEntryInfo;
			if (RemoteWorldInfo.TryRead(text, out remoteWorldInfo))
			{
				string worldEntryKey = SaveInfoProvider.GetWorldEntryKey(remoteWorldInfo.worldName, SaveInfoProvider.RemoteWorldsType);
				if (!this.worldEntryInfosByWorldKey.TryGetValue(worldEntryKey, out worldEntryInfo))
				{
					worldEntryInfo = new SaveInfoProvider.WorldEntryInfo
					{
						WorldKey = worldEntryKey,
						Name = remoteWorldInfo.worldName,
						Type = SaveInfoProvider.RemoteWorldsType,
						Location = PathAbstractions.AbstractedLocation.None,
						Deletable = false,
						WorldDataSize = 0L,
						Version = remoteWorldInfo.gameVersion
					};
					this.worldEntryInfosByWorldKey[worldEntryKey] = worldEntryInfo;
				}
				if (SaveInfoProvider.DataLimitEnabled && remoteWorldInfo.saveSize != -1L)
				{
					saveEntryInfo.SizeInfo.BytesReserved = remoteWorldInfo.saveSize;
					if (saveEntryInfo.SizeInfo.BytesOnDisk > saveEntryInfo.SizeInfo.BytesReserved)
					{
						Debug.LogError(string.Format("Directory size of save \"{0}\" exceeds serialized save data size of {1}.", sdDirectoryInfo.FullName, remoteWorldInfo.saveSize));
					}
				}
				else
				{
					saveEntryInfo.SizeInfo.BytesReserved = -1L;
				}
				saveEntryInfo.Name = remoteWorldInfo.gameName;
				saveEntryInfo.WorldEntry = worldEntryInfo;
				saveEntryInfo.LastSaved = SdFile.GetLastWriteTime(text);
				saveEntryInfo.Version = remoteWorldInfo.gameVersion;
			}
			else
			{
				string worldEntryKey2 = SaveInfoProvider.GetWorldEntryKey(SaveInfoProvider.RemoteWorldsLabel, SaveInfoProvider.RemoteWorldsType);
				if (!this.worldEntryInfosByWorldKey.TryGetValue(worldEntryKey2, out worldEntryInfo))
				{
					worldEntryInfo = new SaveInfoProvider.WorldEntryInfo
					{
						WorldKey = worldEntryKey2,
						Name = SaveInfoProvider.RemoteWorldsLabel,
						Type = SaveInfoProvider.RemoteWorldsType,
						Location = PathAbstractions.AbstractedLocation.None,
						Deletable = false,
						WorldDataSize = 0L,
						Version = null
					};
					this.worldEntryInfosByWorldKey[worldEntryKey2] = worldEntryInfo;
				}
				saveEntryInfo.Name = sdDirectoryInfo.Name;
				saveEntryInfo.WorldEntry = worldEntryInfo;
				saveEntryInfo.LastSaved = sdDirectoryInfo.LastWriteTime;
				saveEntryInfo.Version = null;
			}
			this.saveEntryInfos.Add(saveEntryInfo);
			string saveEntryKey = SaveInfoProvider.GetSaveEntryKey(saveEntryInfo.WorldEntry.WorldKey, saveEntryInfo.Name);
			this.saveEntryInfosBySaveKey[saveEntryKey] = saveEntryInfo;
			this.remoteSaveEntryInfosByGuid[sdDirectoryInfo.Name] = saveEntryInfo;
			worldEntryInfo.SaveEntryInfos.Add(saveEntryInfo);
			worldEntryInfo.SaveDataCount++;
			worldEntryInfo.SaveDataSize += saveEntryInfo.SizeInfo.ReportedSize;
			this.remoteSavesSum += saveEntryInfo.SizeInfo.ReportedSize;
		}
	}

	public bool TryGetLocalSaveEntry(string worldName, string saveName, out SaveInfoProvider.SaveEntryInfo saveEntryInfo)
	{
		this.RefreshIfDirty();
		string saveEntryKey = SaveInfoProvider.GetSaveEntryKey(SaveInfoProvider.GetWorldEntryKey(worldName, "Local"), saveName);
		return this.saveEntryInfosBySaveKey.TryGetValue(saveEntryKey, out saveEntryInfo);
	}

	public bool TryGetRemoteSaveEntry(string guid, out SaveInfoProvider.SaveEntryInfo saveEntryInfo)
	{
		this.RefreshIfDirty();
		return this.remoteSaveEntryInfosByGuid.TryGetValue(guid, out saveEntryInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string NormalizePath(string path)
	{
		return Path.GetFullPath(path);
	}

	public void SetDirectoryProtected(string path, bool isProtected)
	{
		if (isProtected)
		{
			this.protectedDirectories.Add(this.NormalizePath(path));
			return;
		}
		this.protectedDirectories.Remove(this.NormalizePath(path));
	}

	public bool IsDirectoryProtected(string path)
	{
		return this.protectedDirectories.Contains(this.NormalizePath(path));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const long BytesPerMB = 1048576L;

	public const string cLocalWorldsKey = "Local";

	public static readonly string RemoteWorldsLabel = "[" + Localization.Get("xuiDmRemoteWorlds", false) + "] ";

	public static readonly string RemoteWorldsType = Localization.Get("xuiDmRemote", false);

	public static readonly string DeletedWorldsType = Localization.Get("xuiDmDeleted", false);

	public static readonly List<string> HideableWorlds = new List<string>
	{
		"Empty",
		"Playtesting"
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static SaveInfoProvider instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, SaveInfoProvider.WorldEntryInfo> worldEntryInfosByWorldKey = new Dictionary<string, SaveInfoProvider.WorldEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, SaveInfoProvider.SaveEntryInfo> saveEntryInfosBySaveKey = new Dictionary<string, SaveInfoProvider.SaveEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, SaveInfoProvider.SaveEntryInfo> remoteSaveEntryInfosByGuid = new Dictionary<string, SaveInfoProvider.SaveEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SaveInfoProvider.WorldEntryInfo> worldEntryInfos = new List<SaveInfoProvider.WorldEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SaveInfoProvider.SaveEntryInfo> saveEntryInfos = new List<SaveInfoProvider.SaveEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SaveInfoProvider.PlayerEntryInfo> playerEntryInfos = new List<SaveInfoProvider.PlayerEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<string> protectedDirectories = new HashSet<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public long localSavesSum;

	[PublicizedFrom(EAccessModifier.Private)]
	public long remoteSavesSum;

	[PublicizedFrom(EAccessModifier.Private)]
	public long worldsSum;

	[PublicizedFrom(EAccessModifier.Private)]
	public long totalUsedBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public long totalAllowanceBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, SaveInfoProvider.PlayerEntryInfo> fileNameKeysToPlayerInfos = new Dictionary<string, SaveInfoProvider.PlayerEntryInfo>();

	public struct SaveSizeInfo
	{
		public long ReportedSize
		{
			get
			{
				if (!this.IsArchived)
				{
					return this.MaxSize;
				}
				return this.BytesOnDisk;
			}
		}

		public long MaxSize
		{
			get
			{
				return Math.Max(this.BytesOnDisk, this.BytesReserved);
			}
		}

		public bool Archivable
		{
			get
			{
				return this.BytesReserved >= this.BytesOnDisk;
			}
		}

		public long BytesOnDisk;

		public long BytesReserved;

		public bool IsArchived;
	}

	public class WorldEntryInfo : IComparable
	{
		public int CompareTo(object obj)
		{
			SaveInfoProvider.WorldEntryInfo worldEntryInfo = obj as SaveInfoProvider.WorldEntryInfo;
			if (worldEntryInfo != null)
			{
				return string.Compare(this.WorldKey, worldEntryInfo.WorldKey, StringComparison.OrdinalIgnoreCase);
			}
			return 1;
		}

		public string WorldKey;

		public string Name;

		public string Type;

		public PathAbstractions.AbstractedLocation Location;

		public bool Deletable;

		public long WorldDataSize;

		public VersionInformation Version;

		public long SaveDataSize;

		public int SaveDataCount;

		public long BarStartOffset;

		public bool HideIfEmpty;

		public readonly List<SaveInfoProvider.SaveEntryInfo> SaveEntryInfos = new List<SaveInfoProvider.SaveEntryInfo>();
	}

	public class SaveEntryInfo : IComparable
	{
		public int CompareTo(object obj)
		{
			SaveInfoProvider.SaveEntryInfo saveEntryInfo = obj as SaveInfoProvider.SaveEntryInfo;
			if (saveEntryInfo == null)
			{
				return 1;
			}
			int num = saveEntryInfo.LastSaved.CompareTo(this.LastSaved);
			if (num == 0)
			{
				return saveEntryInfo.Name.CompareTo(this.Name);
			}
			return num;
		}

		public string Name;

		public string SaveDir;

		public long Size;

		public SaveInfoProvider.SaveSizeInfo SizeInfo;

		public long BarStartOffset;

		public SaveInfoProvider.WorldEntryInfo WorldEntry;

		public DateTime LastSaved;

		public VersionInformation Version;

		public readonly List<SaveInfoProvider.PlayerEntryInfo> PlayerEntryInfos = new List<SaveInfoProvider.PlayerEntryInfo>();
	}

	public class PlayerEntryInfo : IComparable
	{
		public string PlatformName
		{
			get
			{
				PlatformUserIdentifierAbs platformUserIdentifierAbs = this.NativeUserId ?? this.PrimaryUserId;
				return ((platformUserIdentifierAbs != null) ? platformUserIdentifierAbs.PlatformIdentifierString : null) ?? "-";
			}
		}

		public IPlatformUserData PlatformUserData
		{
			get
			{
				if (this.platformUserData == null && this.PrimaryUserId != null)
				{
					IPlatformUserData orCreate = PlatformUserManager.GetOrCreate(this.PrimaryUserId);
					if (this.NativeUserId != null)
					{
						orCreate.NativeId = this.NativeUserId;
					}
					this.platformUserData = orCreate;
				}
				return this.platformUserData;
			}
		}

		public int CompareTo(object obj)
		{
			SaveInfoProvider.PlayerEntryInfo playerEntryInfo = obj as SaveInfoProvider.PlayerEntryInfo;
			if (playerEntryInfo == null)
			{
				return 1;
			}
			int num = playerEntryInfo.LastPlayed.CompareTo(this.LastPlayed);
			if (num == 0)
			{
				return playerEntryInfo.CachedName.CompareTo(this.CachedName);
			}
			return num;
		}

		public string Id;

		public string CachedName;

		public PlatformUserIdentifierAbs PrimaryUserId;

		public PlatformUserIdentifierAbs NativeUserId;

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatformUserData platformUserData;

		public long Size;

		public long BarStartOffset;

		public SaveInfoProvider.SaveEntryInfo SaveEntry;

		public DateTime LastPlayed;

		public int PlayerLevel;

		public float DistanceWalked;
	}

	public class PlayerEntryInfoPlatformDataResolver
	{
		public bool IsComplete { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		[PublicizedFrom(EAccessModifier.Private)]
		public PlayerEntryInfoPlatformDataResolver(List<SaveInfoProvider.PlayerEntryInfo> playerEntries)
		{
			this.pendingPlayerEntries = playerEntries;
		}

		public static SaveInfoProvider.PlayerEntryInfoPlatformDataResolver StartNew(IEnumerable<SaveInfoProvider.PlayerEntryInfo> playerEntries)
		{
			List<SaveInfoProvider.PlayerEntryInfo> list = new List<SaveInfoProvider.PlayerEntryInfo>();
			List<IPlatformUserData> list2 = null;
			foreach (SaveInfoProvider.PlayerEntryInfo playerEntryInfo in playerEntries)
			{
				list.Add(playerEntryInfo);
				IPlatformUserData platformUserData = playerEntryInfo.PlatformUserData;
				if (platformUserData != null)
				{
					if (list2 == null)
					{
						list2 = new List<IPlatformUserData>();
					}
					list2.Add(platformUserData);
				}
			}
			SaveInfoProvider.PlayerEntryInfoPlatformDataResolver playerEntryInfoPlatformDataResolver = new SaveInfoProvider.PlayerEntryInfoPlatformDataResolver(list);
			if (list2 == null)
			{
				playerEntryInfoPlatformDataResolver.IsComplete = true;
				return playerEntryInfoPlatformDataResolver;
			}
			if (!PlatformUserManager.AreUsersPendingResolve(list2))
			{
				playerEntryInfoPlatformDataResolver.IsComplete = true;
				return playerEntryInfoPlatformDataResolver;
			}
			ThreadManager.StartCoroutine(playerEntryInfoPlatformDataResolver.ResolveUserData(list2));
			return playerEntryInfoPlatformDataResolver;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator ResolveUserData(List<IPlatformUserData> resolvingPlatformData)
		{
			yield return PlatformUserManager.ResolveUsersDetailsCoroutine(resolvingPlatformData);
			yield return PlatformUserManager.ResolveUserBlocksCoroutine(resolvingPlatformData);
			this.IsComplete = true;
			yield break;
		}

		public readonly List<SaveInfoProvider.PlayerEntryInfo> pendingPlayerEntries;
	}
}
