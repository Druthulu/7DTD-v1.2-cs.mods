using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public static class GameIO
{
	[PublicizedFrom(EAccessModifier.Private)]
	static GameIO()
	{
		GameIO.m_UnityDataPath = Application.dataPath;
		GameIO.m_UnityRuntimePlatform = Application.platform;
	}

	public static SdFileInfo[] GetDirectory(string _path, string _pattern)
	{
		if (!SdDirectory.Exists(_path))
		{
			return Array.Empty<SdFileInfo>();
		}
		return new SdDirectoryInfo(_path).GetFiles(_pattern);
	}

	public static long FileSize(string _filePath)
	{
		SdFileInfo sdFileInfo = new SdFileInfo(_filePath);
		if (!sdFileInfo.Exists)
		{
			return -1L;
		}
		return sdFileInfo.Length;
	}

	public static string GetNormalizedPath(string _path)
	{
		return Path.GetFullPath(_path).TrimEnd(GameIO.pathTrimCharacters);
	}

	public static bool PathsEquals(string _path1, string _path2, bool _ignoreCase)
	{
		return string.Equals(GameIO.GetNormalizedPath(_path1), GameIO.GetNormalizedPath(_path2), _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
	}

	public static string GetFileExtension(string _filename)
	{
		int startIndex;
		if (_filename.Length > 4 && (startIndex = _filename.LastIndexOf('.')) > 0)
		{
			return _filename.Substring(startIndex);
		}
		return _filename;
	}

	public static string RemoveFileExtension(string _filename)
	{
		int length;
		if (_filename.Length > 4 && (length = _filename.LastIndexOf('.')) > 0)
		{
			return _filename.Substring(0, length);
		}
		return _filename;
	}

	public static string RemoveExtension(string _filename, string _extension)
	{
		if (_filename.Length > _extension.Length && _filename.EndsWith(_extension, StringComparison.InvariantCultureIgnoreCase))
		{
			return _filename.Substring(0, _filename.Length - _extension.Length);
		}
		return _filename;
	}

	public static string GetFilenameFromPath(string _filepath)
	{
		int num = _filepath.LastIndexOfAny(GameIO.ResourcePathSeparators);
		if (num >= 0 && num < _filepath.Length)
		{
			_filepath = _filepath.Substring(num + 1);
		}
		return _filepath;
	}

	public static string GetFilenameFromPathWithoutExtension(string _filepath)
	{
		int num = _filepath.LastIndexOfAny(GameIO.ResourcePathSeparators);
		int num2 = _filepath.LastIndexOf('.');
		if (num >= 0 && num2 < num)
		{
			num2 = -1;
		}
		if (num >= 0 && num2 >= 0)
		{
			return _filepath.Substring(num + 1, num2 - num - 1);
		}
		if (num >= 0)
		{
			return _filepath.Substring(num + 1);
		}
		if (num2 >= 0)
		{
			return _filepath.Substring(0, num2);
		}
		return _filepath;
	}

	public static string GetDirectoryFromPath(string _filepath)
	{
		int num = _filepath.LastIndexOf('/');
		if (num > 0 && num < _filepath.Length)
		{
			_filepath = _filepath.Substring(0, num);
		}
		return _filepath;
	}

	public static long GetDirectorySize(string _filepath, bool recursive = true)
	{
		return GameIO.GetDirectorySize(new SdDirectoryInfo(_filepath), recursive);
	}

	public static long GetDirectorySize(SdDirectoryInfo directoryInfo, bool recursive = true)
	{
		long num = 0L;
		if (directoryInfo == null || !directoryInfo.Exists)
		{
			return num;
		}
		foreach (SdFileInfo sdFileInfo in directoryInfo.GetFiles())
		{
			num += sdFileInfo.Length;
		}
		if (recursive)
		{
			foreach (SdDirectoryInfo directoryInfo2 in directoryInfo.GetDirectories())
			{
				num += GameIO.GetDirectorySize(directoryInfo2, recursive);
			}
		}
		return num;
	}

	public static string GetGameDir(string _relDir)
	{
		return GameIO.GetApplicationPath() + "/" + _relDir;
	}

	public static string GetApplicationPath()
	{
		if (GameIO.m_ApplicationPath == null)
		{
			string text = GameIO.m_UnityDataPath;
			RuntimePlatform unityRuntimePlatform = GameIO.m_UnityRuntimePlatform;
			if (unityRuntimePlatform <= RuntimePlatform.PS4)
			{
				if (unityRuntimePlatform - RuntimePlatform.OSXPlayer > 1)
				{
					if (unityRuntimePlatform != RuntimePlatform.PS4)
					{
						goto IL_3F;
					}
					goto IL_4B;
				}
			}
			else
			{
				if (unityRuntimePlatform == RuntimePlatform.PS5)
				{
					goto IL_4B;
				}
				if (unityRuntimePlatform - RuntimePlatform.WindowsServer > 1)
				{
					goto IL_3F;
				}
			}
			text += "/..";
			goto IL_4B;
			IL_3F:
			text += "/..";
			IL_4B:
			GameIO.m_ApplicationPath = text;
		}
		return GameIO.m_ApplicationPath;
	}

	public static string GetGamePath()
	{
		if (GameIO.m_UnityRuntimePlatform != RuntimePlatform.OSXPlayer && GameIO.m_UnityRuntimePlatform != RuntimePlatform.OSXServer)
		{
			return GameIO.m_UnityDataPath + "/..";
		}
		return GameIO.m_UnityDataPath + "/../..";
	}

	public static string GetApplicationScratchPath()
	{
		if (GameIO.m_ApplicationScratchPath == null)
		{
			RuntimePlatform unityRuntimePlatform = GameIO.m_UnityRuntimePlatform;
			if (unityRuntimePlatform <= RuntimePlatform.XboxOne)
			{
				if (unityRuntimePlatform != RuntimePlatform.PS4)
				{
					if (unityRuntimePlatform != RuntimePlatform.XboxOne)
					{
						goto IL_50;
					}
					GameIO.m_ApplicationScratchPath = "D:";
					goto IL_5A;
				}
			}
			else
			{
				if (unityRuntimePlatform - RuntimePlatform.GameCoreXboxSeries <= 1)
				{
					GameIO.m_ApplicationScratchPath = "D:";
					goto IL_5A;
				}
				if (unityRuntimePlatform != RuntimePlatform.PS5)
				{
					goto IL_50;
				}
			}
			GameIO.m_ApplicationScratchPath = "/hostapp";
			goto IL_5A;
			IL_50:
			GameIO.m_ApplicationScratchPath = GameIO.GetApplicationPath();
		}
		IL_5A:
		return GameIO.m_ApplicationScratchPath;
	}

	public static string GetApplicationTempPath()
	{
		if (GameIO.m_ApplicationTempPath == null)
		{
			RuntimePlatform unityRuntimePlatform = GameIO.m_UnityRuntimePlatform;
			if (unityRuntimePlatform != RuntimePlatform.PS4)
			{
				if (unityRuntimePlatform == RuntimePlatform.XboxOne)
				{
					GameIO.m_ApplicationTempPath = "T:\\";
					goto IL_68;
				}
				switch (unityRuntimePlatform)
				{
				case RuntimePlatform.GameCoreXboxSeries:
					GameIO.m_ApplicationTempPath = "T:\\";
					goto IL_68;
				case RuntimePlatform.GameCoreXboxOne:
					GameIO.m_ApplicationTempPath = "T:\\";
					goto IL_68;
				case RuntimePlatform.PS5:
					break;
				default:
					GameIO.m_ApplicationTempPath = GameIO.GetApplicationPath();
					goto IL_68;
				}
			}
			GameIO.m_ApplicationTempPath = "/temp0";
		}
		IL_68:
		return GameIO.m_ApplicationTempPath;
	}

	public static IEnumerator PrecacheFile(string _path, int _doYieldEveryMs = -1, Action<float, long, long> _statusUpdateHandler = null)
	{
		if (!SdFile.Exists(_path))
		{
			Log.Error("File does not exist: " + _path);
			yield break;
		}
		Stream fs;
		try
		{
			fs = SdFile.OpenRead(_path);
		}
		catch (Exception e)
		{
			Log.Error("Precaching file failed");
			Log.Exception(e);
			yield break;
		}
		byte[] buf = new byte[16384];
		MicroStopwatch msw = new MicroStopwatch();
		int num;
		do
		{
			if (_doYieldEveryMs > 0 && msw.ElapsedMilliseconds >= (long)_doYieldEveryMs)
			{
				if (_statusUpdateHandler != null)
				{
					_statusUpdateHandler((float)fs.Position / (float)fs.Length, fs.Position, fs.Length);
				}
				yield return null;
				msw.ResetAndRestart();
			}
			try
			{
				num = fs.Read(buf, 0, buf.Length);
			}
			catch (Exception e2)
			{
				Log.Error("Precaching file failed");
				Log.Exception(e2);
				try
				{
					fs.Dispose();
				}
				catch (Exception e3)
				{
					Log.Error("Failed disposing filestream");
					Log.Exception(e3);
				}
				yield break;
			}
		}
		while (num > 0);
		fs.Dispose();
		yield break;
	}

	public static string GetDocumentPath()
	{
		RuntimePlatform unityRuntimePlatform = GameIO.m_UnityRuntimePlatform;
		if (unityRuntimePlatform <= RuntimePlatform.LinuxPlayer)
		{
			if (unityRuntimePlatform <= RuntimePlatform.WindowsPlayer)
			{
				if (unityRuntimePlatform <= RuntimePlatform.OSXPlayer)
				{
					goto IL_83;
				}
				if (unityRuntimePlatform != RuntimePlatform.WindowsPlayer)
				{
					goto IL_B3;
				}
				goto IL_7B;
			}
			else
			{
				if (unityRuntimePlatform == RuntimePlatform.WindowsEditor)
				{
					goto IL_7B;
				}
				if (unityRuntimePlatform != RuntimePlatform.LinuxPlayer)
				{
					goto IL_B3;
				}
			}
		}
		else if (unityRuntimePlatform <= RuntimePlatform.PS4)
		{
			if (unityRuntimePlatform != RuntimePlatform.LinuxEditor)
			{
				if (unityRuntimePlatform != RuntimePlatform.PS4)
				{
					goto IL_B3;
				}
				return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
		}
		else
		{
			if (unityRuntimePlatform == RuntimePlatform.XboxOne)
			{
				UnityEngine.Debug.LogWarning("XboxOne: Platform Document Path is currently not set");
				return null;
			}
			switch (unityRuntimePlatform)
			{
			case RuntimePlatform.GameCoreXboxSeries:
			case RuntimePlatform.GameCoreXboxOne:
				return Application.persistentDataPath;
			case RuntimePlatform.PS5:
				return "/download0";
			case RuntimePlatform.EmbeddedLinuxArm64:
			case RuntimePlatform.EmbeddedLinuxArm32:
			case RuntimePlatform.EmbeddedLinuxX64:
			case RuntimePlatform.EmbeddedLinuxX86:
				goto IL_B3;
			case RuntimePlatform.LinuxServer:
				break;
			case RuntimePlatform.WindowsServer:
				goto IL_7B;
			case RuntimePlatform.OSXServer:
				goto IL_83;
			default:
				goto IL_B3;
			}
		}
		return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		IL_7B:
		return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		IL_83:
		return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Library/Application Support";
		IL_B3:
		return null;
	}

	public static string GetDefaultUserGameDataDir()
	{
		return GameIO.GetDocumentPath() + "/" + "7 Days To Die".Replace(" ", "");
	}

	public static string GetUserGameDataDir()
	{
		return LaunchPrefs.UserDataFolder.Value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateUserDataFolderDependentPaths()
	{
		string userGameDataDir = GameIO.GetUserGameDataDir();
		if (GameIO.m_LastUserDataFolder == userGameDataDir)
		{
			return;
		}
		object cachedUserDataFolderDependentLock = GameIO.m_CachedUserDataFolderDependentLock;
		lock (cachedUserDataFolderDependentLock)
		{
			if (!(GameIO.m_LastUserDataFolder == userGameDataDir))
			{
				GameIO.m_CachedSaveGameRootDir = Path.Combine(userGameDataDir, "Saves");
				GameIO.m_CachedSaveGameLocalRootDir = Path.Combine(userGameDataDir, "SavesLocal");
				GameIO.m_LastUserDataFolder = userGameDataDir;
			}
		}
	}

	public static string GetSaveGameRootDir()
	{
		GameIO.UpdateUserDataFolderDependentPaths();
		return GameIO.m_CachedSaveGameRootDir;
	}

	public static string GetSaveGameDir(string _worldName)
	{
		return GameIO.GetSaveGameRootDir() + "/" + _worldName;
	}

	public static string GetSaveGameDir(string _worldName, string _gameName)
	{
		return GameIO.GetSaveGameDir(_worldName) + "/" + _gameName;
	}

	public static string GetSaveGameDir()
	{
		return GameIO.GetSaveGameDir(GamePrefs.GetString(EnumGamePrefs.GameWorld), GamePrefs.GetString(EnumGamePrefs.GameName));
	}

	public static string GetSaveGameLocalRootDir()
	{
		GameIO.UpdateUserDataFolderDependentPaths();
		return GameIO.m_CachedSaveGameLocalRootDir;
	}

	public static string GetSaveGameLocalDir()
	{
		string @string = GamePrefs.GetString(EnumGamePrefs.GameGuidClient);
		if (string.IsNullOrEmpty(@string))
		{
			throw new Exception("Accessing GetSaveGameLocalDir while GameGuidClient is not yet set!");
		}
		return GameIO.GetSaveGameLocalRootDir() + "/" + @string;
	}

	public static string GetPlayerDataDir()
	{
		return Path.Combine(GameIO.GetSaveGameDir(), "Player");
	}

	public static string GetPlayerDataLocalDir()
	{
		return Path.Combine(GameIO.GetSaveGameLocalDir(), "Player");
	}

	public static int GetPlayerSaves(GameIO.FoundSave _foundSave = null, bool includeArchived = false)
	{
		int num = 0;
		string saveGameRootDir = GameIO.GetSaveGameRootDir();
		if (!SdDirectory.Exists(saveGameRootDir))
		{
			return 0;
		}
		SdFileSystemInfo[] array = new SdDirectoryInfo(saveGameRootDir).GetDirectories();
		foreach (SdDirectoryInfo sdDirectoryInfo in array)
		{
			string fullName = sdDirectoryInfo.FullName;
			if (SdDirectory.Exists(fullName))
			{
				SdFileSystemInfo[] array2 = new SdDirectoryInfo(fullName).GetDirectories();
				foreach (SdDirectoryInfo sdDirectoryInfo2 in array2)
				{
					if (!sdDirectoryInfo2.Name.Contains("#"))
					{
						bool flag = SdFile.Exists(Path.Combine(sdDirectoryInfo2.FullName, "archived.flag"));
						if (includeArchived || !flag)
						{
							string text = sdDirectoryInfo2.FullName + "/main.ttw";
							if (SdFile.Exists(text))
							{
								try
								{
									WorldState worldState = new WorldState();
									worldState.Load(text, false, false, false);
									if (worldState.gameVersion != null)
									{
										if (_foundSave != null)
										{
											_foundSave(sdDirectoryInfo2.Name, sdDirectoryInfo.Name, SdFile.GetLastWriteTime(text), worldState, flag);
										}
										num++;
									}
								}
								catch (Exception ex)
								{
									Log.Warning("Error reading header of level '" + text + "'. Ignoring. Msg: " + ex.Message);
								}
							}
						}
					}
				}
			}
		}
		return num;
	}

	public static string GetSaveGameRegionDir()
	{
		return Path.Combine(GameIO.GetSaveGameDir(), "Region");
	}

	public static string GetSaveGameRegionDirDefault(string _levelName)
	{
		return PathAbstractions.WorldsSearchPaths.GetLocation(_levelName, null, null).FullPath + "/Region";
	}

	public static string GetSaveGameRegionDirDefault()
	{
		return GameIO.GetSaveGameRegionDirDefault(GamePrefs.GetString(EnumGamePrefs.GameWorld));
	}

	public static string GetWorldDir()
	{
		return PathAbstractions.WorldsSearchPaths.GetLocation(GamePrefs.GetString(EnumGamePrefs.GameWorld), null, null).FullPath;
	}

	public static string GetWorldDir(string _worldName)
	{
		PathAbstractions.AbstractedLocation location = PathAbstractions.WorldsSearchPaths.GetLocation(_worldName, null, null);
		if (location.Type != PathAbstractions.EAbstractedLocationType.None)
		{
			return location.FullPath;
		}
		return null;
	}

	public static bool DoesWorldExist(string _worldName)
	{
		return GameIO.GetWorldDir(_worldName) != null;
	}

	public static bool IsWorldGenerated(string _worldName)
	{
		return SdDirectory.Exists(Path.Combine(GameIO.GetUserGameDataDir(), "GeneratedWorlds", _worldName));
	}

	public static bool IsAbsolutePath(string _path)
	{
		RuntimePlatform unityRuntimePlatform = GameIO.m_UnityRuntimePlatform;
		switch (unityRuntimePlatform)
		{
		case RuntimePlatform.OSXEditor:
		case RuntimePlatform.OSXPlayer:
		case RuntimePlatform.IPhonePlayer:
			break;
		case RuntimePlatform.WindowsPlayer:
		case RuntimePlatform.WindowsEditor:
			goto IL_E8;
		case RuntimePlatform.OSXWebPlayer:
		case RuntimePlatform.OSXDashboardPlayer:
		case RuntimePlatform.WindowsWebPlayer:
		case (RuntimePlatform)6:
			goto IL_13F;
		default:
			switch (unityRuntimePlatform)
			{
			case RuntimePlatform.Android:
			case RuntimePlatform.LinuxPlayer:
			case RuntimePlatform.LinuxEditor:
				break;
			case RuntimePlatform.NaCl:
			case (RuntimePlatform)14:
			case RuntimePlatform.FlashPlayer:
			case RuntimePlatform.WebGLPlayer:
				goto IL_13F;
			default:
				switch (unityRuntimePlatform)
				{
				case RuntimePlatform.PS4:
				case RuntimePlatform.PS5:
				case RuntimePlatform.LinuxServer:
				case RuntimePlatform.OSXServer:
					break;
				case RuntimePlatform.PSM:
				case RuntimePlatform.SamsungTVPlayer:
				case (RuntimePlatform)29:
				case RuntimePlatform.WiiU:
				case RuntimePlatform.tvOS:
				case RuntimePlatform.Switch:
				case RuntimePlatform.Lumin:
				case RuntimePlatform.Stadia:
				case RuntimePlatform.CloudRendering:
				case RuntimePlatform.EmbeddedLinuxArm64:
				case RuntimePlatform.EmbeddedLinuxArm32:
				case RuntimePlatform.EmbeddedLinuxX64:
				case RuntimePlatform.EmbeddedLinuxX86:
					goto IL_13F;
				case RuntimePlatform.XboxOne:
				case RuntimePlatform.GameCoreXboxSeries:
				case RuntimePlatform.GameCoreXboxOne:
				case RuntimePlatform.WindowsServer:
					goto IL_E8;
				default:
					goto IL_13F;
				}
				break;
			}
			break;
		}
		return _path[0] == '/' || _path[0] == '\\' || _path.StartsWith("~/") || _path.StartsWith("~\\");
		IL_E8:
		return _path[1] == ':' && (_path[2] == '/' || _path[2] == '\\') && ((_path[0] >= 'A' && _path[0] <= 'Z') || (_path[0] >= 'a' && _path[0] <= 'z'));
		IL_13F:
		throw new ArgumentOutOfRangeException("_path", _path, "Unsupported platform");
	}

	public static string MakeAbsolutePath(string _path)
	{
		if (GameIO.IsAbsolutePath(_path))
		{
			return _path;
		}
		return GameIO.GetGamePath() + "/" + _path;
	}

	public static string GetOsStylePath(string _path)
	{
		if (GameIO.m_UnityRuntimePlatform != RuntimePlatform.WindowsPlayer && GameIO.m_UnityRuntimePlatform != RuntimePlatform.WindowsServer)
		{
			return _path.Replace("\\", "/");
		}
		return _path.Replace("/", "\\");
	}

	public static void CopyDirectory(string _sourceDirectory, string _targetDirectory)
	{
		SdDirectoryInfo source = new SdDirectoryInfo(_sourceDirectory);
		SdDirectoryInfo target = new SdDirectoryInfo(_targetDirectory);
		GameIO.CopyAll(source, target);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CopyAll(SdDirectoryInfo _source, SdDirectoryInfo _target)
	{
		SdDirectory.CreateDirectory(_target.FullName);
		foreach (SdFileInfo sdFileInfo in _source.GetFiles())
		{
			sdFileInfo.CopyTo(Path.Combine(_target.FullName, sdFileInfo.Name), true);
		}
		foreach (SdDirectoryInfo sdDirectoryInfo in _source.GetDirectories())
		{
			SdDirectoryInfo target = _target.CreateSubdirectory(sdDirectoryInfo.Name);
			GameIO.CopyAll(sdDirectoryInfo, target);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OpenExplorerOnLinux(string _path)
	{
		_path = _path.Replace("\\", "/");
		if (SdFile.Exists(_path))
		{
			_path = Path.GetDirectoryName(_path);
		}
		if (_path.IndexOf(' ') >= 0)
		{
			_path = "\"" + _path + "\"";
		}
		try
		{
			Process.Start("xdg-open", _path);
		}
		catch (Exception e)
		{
			Log.Error("Failed opening file browser:");
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OpenExplorerOnMac(string _path)
	{
		_path = _path.Replace("\\", "/");
		bool flag = SdDirectory.Exists(_path);
		if (!_path.StartsWith("\""))
		{
			_path = "\"" + _path;
		}
		if (!_path.EndsWith("\""))
		{
			_path += "\"";
		}
		try
		{
			Process.Start("open", (flag ? "" : "-R ") + _path);
		}
		catch (Exception e)
		{
			Log.Error("Failed opening Finder:");
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OpenExplorerOnWin(string _path)
	{
		_path = _path.Replace("/", "\\");
		bool flag = SdDirectory.Exists(_path);
		try
		{
			Process.Start("explorer.exe", (flag ? "/root,\"" : "/select,\"") + _path + "\"");
		}
		catch (Exception e)
		{
			Log.Error("Failed opening Explorer:");
			Log.Exception(e);
		}
	}

	public static void OpenExplorer(string _path)
	{
		RuntimePlatform platform = Application.platform;
		if (platform > RuntimePlatform.WindowsEditor)
		{
			if (platform != RuntimePlatform.LinuxPlayer && platform != RuntimePlatform.LinuxEditor)
			{
				switch (platform)
				{
				case RuntimePlatform.LinuxServer:
					break;
				case RuntimePlatform.WindowsServer:
					goto IL_39;
				case RuntimePlatform.OSXServer:
					goto IL_40;
				default:
					goto IL_4E;
				}
			}
			GameIO.OpenExplorerOnLinux(_path);
			return;
		}
		if (platform <= RuntimePlatform.OSXPlayer)
		{
			goto IL_40;
		}
		if (platform != RuntimePlatform.WindowsPlayer && platform != RuntimePlatform.WindowsEditor)
		{
			goto IL_4E;
		}
		IL_39:
		GameIO.OpenExplorerOnWin(_path);
		return;
		IL_40:
		GameIO.OpenExplorerOnMac(_path);
		return;
		IL_4E:
		Log.Error("Failed opening file browser: Unsupported OS");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_ApplicationScratchPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string XB1ScratchPath = "D:";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PS4ScratchPath = "/hostapp";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_ApplicationTempPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string XB1TempPath = "T:\\";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PS4TempPath = "/temp0";

	[PublicizedFrom(EAccessModifier.Private)]
	public static RuntimePlatform m_UnityRuntimePlatform;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_UnityDataPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_ApplicationPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_LastUserDataFolder;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object m_CachedUserDataFolderDependentLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_CachedSaveGameRootDir;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_CachedSaveGameLocalRootDir;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] pathTrimCharacters = new char[]
	{
		'/',
		'\\'
	};

	public static readonly char[] ResourcePathSeparators = new char[]
	{
		'/',
		'\\',
		'?'
	};

	public delegate void FoundSave(string saveName, string worldName, DateTime lastSaved, WorldState worldState, bool isArchived);
}
