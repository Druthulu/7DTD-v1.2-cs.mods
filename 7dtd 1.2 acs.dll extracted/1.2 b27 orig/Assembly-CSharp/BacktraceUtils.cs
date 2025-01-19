using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Backtrace.Unity;
using Backtrace.Unity.Model;
using Backtrace.Unity.Types;
using Platform;
using UnityEngine;

public static class BacktraceUtils
{
	public static bool Initialized
	{
		get
		{
			return BacktraceUtils.s_Configuration != null;
		}
	}

	public static bool Enabled
	{
		get
		{
			return BacktraceUtils.s_BacktraceEnabled && BacktraceUtils.s_VersionEnabled;
		}
	}

	public static bool BugReportFeature
	{
		get
		{
			return BacktraceUtils.Enabled && BacktraceUtils.s_BugReportFeature;
		}
	}

	public static bool BugReportAttachSaveFeature
	{
		get
		{
			return BacktraceUtils.Enabled && BacktraceUtils.s_BugReportFeature && BacktraceUtils.s_BugReportAttachSaveFeature;
		}
	}

	public static bool BugReportAttachWholeWorldFeature
	{
		get
		{
			return BacktraceUtils.s_BugReportFeature && BacktraceUtils.s_BugReportAttachSaveFeature && BacktraceUtils.s_BugReportAttachWholeWorldFeature;
		}
	}

	public static void InitializeBacktrace()
	{
		BacktraceUtils.InitializeConfiguration();
		BacktraceUtils.InitializeBacktraceClient();
		Log.Out("Backtrace Initialized");
	}

	public static void BacktraceUserLoggedIn(IPlatform platform)
	{
		if (BacktraceUtils.s_BacktraceClient == null)
		{
			return;
		}
		Log.Out(string.Format("[BACKTRACE] Attempting to get User ID from platform: {0}", platform.PlatformIdentifier));
		if (platform.User.PlatformUserId == null)
		{
			Log.Out(string.Format("[BACKTRACE] {0} PlatformUserId missing at this time", platform.PlatformIdentifier));
			return;
		}
		string text = platform.User.PlatformUserId.PlatformIdentifierString + "-" + platform.User.PlatformUserId.ReadablePlatformUserIdentifier;
		BacktraceUtils.s_BacktraceClient.SetAttributes(new Dictionary<string, string>
		{
			{
				"gamestats.platformuserid",
				text
			}
		});
		Log.Out("[BACKTRACE] Platform ID set to: \"" + text + "\"");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Reset()
	{
		BacktraceUtils.s_BacktraceEnabled = false;
		BacktraceUtils.s_VersionEnabled = false;
		BacktraceUtils.s_Configuration.HandleUnhandledExceptions = false;
		BacktraceUtils.s_Configuration.Sampling = 0.01;
		BacktraceUtils.s_Configuration.CaptureNativeCrashes = true;
		BacktraceUtils.s_Configuration.MinidumpType = MiniDumpType.Normal;
		BacktraceUtils.s_Configuration.DeduplicationStrategy = DeduplicationStrategy.Default;
	}

	public static void SendBugReport(string message, string screenshotPath = null, SaveInfoProvider.SaveEntryInfo saveEntry = null, Action<BacktraceResult> callback = null)
	{
		if (!BacktraceUtils.BugReportFeature)
		{
			Log.Out("[Backtrace] Backtrace bug reporting disabled by platform");
			return;
		}
		if (BacktraceUtils.s_BacktraceClient == null)
		{
			BacktraceUtils.DebugEnableBacktrace();
		}
		List<string> list = new List<string>();
		if (screenshotPath != null)
		{
			list.Add(screenshotPath);
		}
		if (BacktraceUtils.BugReportAttachSaveFeature && saveEntry != null)
		{
			bool flag = false;
			string text;
			BacktraceUtils.SaveArchiveResult saveArchiveResult = BacktraceUtils.TryCreateSaveArchive(saveEntry, out text);
			if (saveArchiveResult == BacktraceUtils.SaveArchiveResult.Success)
			{
				Log.Out("[BACKTRACE] Save file path: " + text);
				list.Add(text);
			}
			else if (saveArchiveResult == BacktraceUtils.SaveArchiveResult.MissingRegions)
			{
				list.Add(text);
				List<string> list2;
				if (BacktraceUtils.TryCreateRegionArchives(saveEntry, out list2))
				{
					list.AddRange(list2);
					Log.Out("[BACKTRACE] region file paths: " + string.Join(", ", list2));
					saveArchiveResult = BacktraceUtils.SaveArchiveResult.Success;
					flag = true;
				}
			}
			string text2;
			if (saveArchiveResult != BacktraceUtils.SaveArchiveResult.FailureToArchive && BacktraceUtils.TryCreateWorldArchive(saveEntry.WorldEntry, out text2) && text2 != null)
			{
				list.Add(text2);
				Log.Out("[BACKTRACE] World file path: " + text2);
				saveArchiveResult = BacktraceUtils.SaveArchiveResult.Success;
				flag = true;
			}
			if (saveArchiveResult == BacktraceUtils.SaveArchiveResult.Success && flag)
			{
				list = BacktraceUtils.CondenseZipFiles(saveEntry, list);
			}
			Log.Out("[BACKTRACE] File Sizes:");
			foreach (string text3 in list)
			{
				Log.Out(string.Format("[BACKTRACE] {0}: {1:N3}MB", text3, (double)new SdFileInfo(text3).Length / 1024.0 / 1024.0));
			}
		}
		BacktraceClient backtraceClient = BacktraceUtils.s_BacktraceClient;
		if (backtraceClient == null)
		{
			return;
		}
		backtraceClient.Send(message, callback, list, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<string> CondenseZipFiles(SaveInfoProvider.SaveEntryInfo saveEntry, List<string> attachmentPaths)
	{
		string text = Path.Join(PlatformApplicationManager.Application.temporaryCachePath, string.Concat(new string[]
		{
			"/combined_",
			saveEntry.WorldEntry.Name,
			"_",
			saveEntry.Name,
			"/"
		}));
		string text2 = Path.Join(PlatformApplicationManager.Application.temporaryCachePath, string.Concat(new string[]
		{
			"/combined_",
			saveEntry.WorldEntry.Name,
			"_",
			saveEntry.Name,
			".zip"
		}));
		if (SdDirectory.Exists(text))
		{
			SdDirectory.Delete(text, true);
		}
		SdDirectory.CreateDirectory(text);
		List<string> list = new List<string>();
		List<string> list2 = new List<string>(attachmentPaths);
		foreach (string text3 in attachmentPaths)
		{
			if (Path.GetFileName(text3).ContainsCaseInsensitive("zip"))
			{
				string text4 = Path.Join(text, Path.GetFileNameWithoutExtension(text3)) + Path.AltDirectorySeparatorChar.ToString();
				SdDirectory.CreateDirectory(text4);
				ZipFile.ExtractToDirectory(text3, text4);
				list.Add(text3);
				list2.Remove(text3);
			}
		}
		using (Stream stream = SdFile.Open(text2, FileMode.Create, FileAccess.Write))
		{
			using (ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Create))
			{
				zipArchive.CreateFromDirectory(text);
			}
		}
		SdDirectory.Delete(text, true);
		if ((double)new SdFileInfo(text2).Length / 1024.0 / 1024.0 <= 30.0)
		{
			Log.Out("[BACKTRACE] Created combined archive: " + text2);
			Log.Out("[BACKTRACE] Remove the following: " + string.Join(", ", list));
			attachmentPaths = list2;
			attachmentPaths.Add(text2);
			using (List<string>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string path = enumerator.Current;
					SdFile.Delete(path);
				}
				return attachmentPaths;
			}
		}
		Log.Out("[BACKTRACE] Combined archive is too large to upload: " + text2);
		SdFile.Delete(text2);
		return attachmentPaths;
	}

	public static BacktraceUtils.SaveArchiveResult TryCreateSaveArchive(SaveInfoProvider.SaveEntryInfo saveEntry, out string archivePath)
	{
		BacktraceUtils.SaveArchiveResult result;
		try
		{
			string saveGameDir = GameIO.GetSaveGameDir(saveEntry.WorldEntry.Name, saveEntry.Name);
			archivePath = Path.Join(PlatformApplicationManager.Application.temporaryCachePath, string.Concat(new string[]
			{
				"Save_",
				saveEntry.WorldEntry.Name,
				"_",
				saveEntry.Name,
				".zip"
			}));
			if (SdFile.Exists(archivePath))
			{
				Log.Out("[BACKTRACE] Old save archive path: {0} exists, deleting...", new object[]
				{
					archivePath
				});
				SdFile.Delete(archivePath);
			}
			using (Stream stream = SdFile.OpenWrite(archivePath))
			{
				using (ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Create, false, null))
				{
					zipArchive.CreateFromDirectory(saveGameDir);
				}
			}
			double num = (double)new SdFileInfo(archivePath).Length / 1024.0 / 1024.0;
			if (SdFile.Exists(archivePath) && num <= 30.0)
			{
				if (new SdFileInfo(archivePath).Length != 0L)
				{
					Log.Out("[BACKTRACE] Save archive path: {0} exists, size is {1}MB, success!", new object[]
					{
						archivePath,
						num.ToString("N3")
					});
					return BacktraceUtils.SaveArchiveResult.Success;
				}
				Log.Warning("[BACKTRACE] Save archive exists: {0}, but is empty, retry", new object[]
				{
					archivePath
				});
				SdFile.Delete(archivePath);
			}
			else if (SdFile.Exists(archivePath))
			{
				Log.Out("[BACKTRACE] Save archive path: {0} exists, size is too big, {1}MB, deleting...", new object[]
				{
					archivePath,
					num.ToString("N3")
				});
				SdFile.Delete(archivePath);
			}
			using (ZipArchive zipArchive2 = ZipFile.Open(archivePath, ZipArchiveMode.Create))
			{
				SdDirectoryInfo directoryInfo = new SdDirectoryInfo(saveGameDir);
				zipArchive2.AddSearchPattern(directoryInfo, "*.sdf", SearchOption.AllDirectories);
				zipArchive2.AddSearchPattern(directoryInfo, "*.ttw", SearchOption.AllDirectories);
				zipArchive2.AddSearchPattern(directoryInfo, "*.xml", SearchOption.AllDirectories);
				zipArchive2.AddSearchPattern(directoryInfo, "*.7dt", SearchOption.AllDirectories);
				zipArchive2.AddSearchPattern(directoryInfo, "*.nim", SearchOption.AllDirectories);
				zipArchive2.AddSearchPattern(directoryInfo, "*.dat", SearchOption.AllDirectories);
				zipArchive2.AddSearchPattern(directoryInfo, "*.ttp", SearchOption.AllDirectories);
				zipArchive2.AddSearchPattern(directoryInfo, "*.ttp.meta", SearchOption.AllDirectories);
				zipArchive2.AddSearchPattern(directoryInfo, "*.7rm", SearchOption.AllDirectories);
			}
			if (SdFile.Exists(archivePath))
			{
				result = BacktraceUtils.SaveArchiveResult.MissingRegions;
			}
			else
			{
				result = BacktraceUtils.SaveArchiveResult.FailureToArchive;
			}
		}
		catch (Exception ex)
		{
			Log.Error("[BACKTRACE]  Exception: Could not create save archive: {0}", new object[]
			{
				ex.Message
			});
			archivePath = null;
			result = BacktraceUtils.SaveArchiveResult.FailureToArchive;
		}
		return result;
	}

	public static bool TryCreateRegionArchives(SaveInfoProvider.SaveEntryInfo saveEntry, out List<string> archivePaths)
	{
		bool result;
		try
		{
			string saveGameDir = GameIO.GetSaveGameDir(saveEntry.WorldEntry.Name, saveEntry.Name);
			archivePaths = new List<string>();
			foreach (SdFileSystemInfo sdFileSystemInfo in from info in new SdDirectoryInfo(PlatformApplicationManager.Application.temporaryCachePath).EnumerateFileSystemInfos()
			where info.Name.EndsWith("_Region.zip", StringComparison.InvariantCulture)
			select info)
			{
				try
				{
					SdFile.Delete(sdFileSystemInfo.FullName);
				}
				catch (Exception arg)
				{
					Log.Warning(string.Format("[BACKTRACE] Failed To Delete {0}, Reason {1}", sdFileSystemInfo.FullName, arg));
				}
			}
			int num = 0;
			string text = Path.Join(PlatformApplicationManager.Application.temporaryCachePath, string.Format("Save_{0}_{1}_{2}_Region.zip", saveEntry.WorldEntry.Name, saveEntry.Name, num));
			Log.Out("[BACKTRACE] World archive path: {0}", new object[]
			{
				text
			});
			SdDirectoryInfo sdDirectoryInfo = new SdDirectoryInfo(saveGameDir);
			IEnumerable<SdFileSystemInfo> enumerable = (from fileSystemInfo in sdDirectoryInfo.EnumerateFileSystemInfos("*.7rg", SearchOption.AllDirectories)
			orderby fileSystemInfo.LastWriteTime.ToFileTimeUtc()
			select fileSystemInfo).Reverse<SdFileSystemInfo>();
			if (!(from fileSystemInfo in sdDirectoryInfo.EnumerateFileSystemInfos("*.7rg", SearchOption.AllDirectories)
			orderby fileSystemInfo.LastWriteTime.ToFileTimeUtc()
			select fileSystemInfo).Reverse<SdFileSystemInfo>().Any<SdFileSystemInfo>())
			{
				enumerable = sdDirectoryInfo.EnumerateFileSystemInfos("*.7rg", SearchOption.AllDirectories);
			}
			Queue<SdFileSystemInfo> queue = new Queue<SdFileSystemInfo>(enumerable);
			while (queue.Any<SdFileSystemInfo>())
			{
				enumerable = queue;
				foreach (SdFileSystemInfo sdFileSystemInfo2 in enumerable)
				{
					SdFileInfo sdFileInfo = new SdFileInfo(sdFileSystemInfo2.FullName);
					Log.Out(string.Format("{0} File size: {1} MiB", sdFileInfo.FullName, (float)sdFileInfo.Length / 1024f * 1024f));
					if (sdFileInfo.Length > 15728640L)
					{
						Log.Warning("File too big! " + text);
					}
					else
					{
						using (Stream stream = SdFile.Open(text, FileMode.OpenOrCreate, FileAccess.ReadWrite))
						{
							using (ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Update, false, null))
							{
								zipArchive.CreateEntryFromFile(sdFileSystemInfo2, sdFileSystemInfo2.Name, System.IO.Compression.CompressionLevel.Optimal);
							}
						}
						if (new SdFileInfo(text).Length > 31457280L)
						{
							Log.Warning("Archive too big! " + text);
							archivePaths.Add(text);
							break;
						}
						queue.Dequeue();
					}
				}
				archivePaths.Add(text);
				num++;
				text = Path.Join(PlatformApplicationManager.Application.temporaryCachePath, string.Format("Save_{0}_{1}_{2}_Region.zip", saveEntry.WorldEntry.Name, saveEntry.Name, num));
			}
			archivePaths = new List<string>(archivePaths.Distinct<string>());
			result = true;
		}
		catch (Exception ex)
		{
			Log.Error("[BACKTRACE] Exception: Could not create save region archive: {0}", new object[]
			{
				ex.Message
			});
			archivePaths = new List<string>();
			result = false;
		}
		return result;
	}

	public static bool TryCreateWorldArchive(SaveInfoProvider.WorldEntryInfo worldEntry, out string archivePath)
	{
		bool result;
		try
		{
			string fullPath = Path.GetFullPath(GameIO.GetWorldDir(worldEntry.Name));
			if (PathAbstractions.WorldsSearchPaths.GetLocation(fullPath, null, null).Type == PathAbstractions.EAbstractedLocationType.GameData)
			{
				Log.Out("[BACKTRACE] World {0} was found in the GameData, do not archive.", new object[]
				{
					worldEntry.Name
				});
				archivePath = string.Empty;
				result = false;
			}
			else
			{
				string fullPath2 = Path.GetFullPath(GameIO.GetUserGameDataDir());
				if (!fullPath.Contains(fullPath2))
				{
					Log.Out("[BACKTRACE] Not creating archive for world {0} not in the User Game Data Directory", new object[]
					{
						worldEntry.Name
					});
					archivePath = null;
					result = false;
				}
				else
				{
					Log.Out("[BACKTRACE] Creating archive for GeneratedWorld {0}", new object[]
					{
						worldEntry.Name
					});
					archivePath = Path.Join(PlatformApplicationManager.Application.temporaryCachePath, "/world_" + worldEntry.Name + ".zip");
					if (SdFile.Exists(archivePath))
					{
						Log.Out("[BACKTRACE] Old World archive path: {0} exists, deleting...", new object[]
						{
							archivePath
						});
					}
					Log.Out("[BACKTRACE] World archive path: {0}. Creating archive...", new object[]
					{
						archivePath
					});
					if (BacktraceUtils.BugReportAttachWholeWorldFeature)
					{
						using (Stream stream = SdFile.Open(archivePath, FileMode.Create, FileAccess.ReadWrite))
						{
							using (ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Create, false, null))
							{
								zipArchive.CreateFromDirectory(fullPath);
							}
						}
						double num = (double)new SdFileInfo(archivePath).Length / 1024.0 / 1024.0;
						if (SdFile.Exists(archivePath) && (BacktraceUtils.BugReportAttachWholeWorldFeature || num <= 30.0))
						{
							if (new SdFileInfo(archivePath).Length != 0L)
							{
								Log.Out("[BACKTRACE] World archive path: {0} exists, size is {1}MB, success!", new object[]
								{
									archivePath,
									num.ToString("N3")
								});
								return true;
							}
							Log.Warning("[BACKTRACE] World archive exists: {0}, but is empty, retry", new object[]
							{
								archivePath
							});
							SdFile.Delete(archivePath);
						}
						else if (SdFile.Exists(archivePath))
						{
							Log.Out("[BACKTRACE] World archive path: {0} exists, size is too big, {1}MB, deleting...", new object[]
							{
								archivePath,
								num.ToString("N3")
							});
							SdFile.Delete(archivePath);
						}
					}
					Log.Out("[BACKTRACE] Attempting to archive only required elements...", new object[]
					{
						archivePath
					});
					using (Stream stream2 = SdFile.Open(archivePath, FileMode.Create, FileAccess.ReadWrite))
					{
						using (ZipArchive zipArchive2 = new ZipArchive(stream2, ZipArchiveMode.Update, false, null))
						{
							SdDirectoryInfo directoryInfo = new SdDirectoryInfo(fullPath);
							zipArchive2.AddSearchPattern(directoryInfo, "*.ttw", SearchOption.TopDirectoryOnly);
							zipArchive2.AddSearchPattern(directoryInfo, "*.xml", SearchOption.TopDirectoryOnly);
						}
					}
					if (SdFile.Exists(archivePath))
					{
						double num2 = (double)new SdFileInfo(archivePath).Length / 1024.0 / 1024.0;
						Log.Out("[BACKTRACE] World archive path: {0}", new object[]
						{
							archivePath
						});
						Log.Out("[BACKTRACE] World archive File size: {0}MB", new object[]
						{
							num2.ToString("N3")
						});
						if (!BacktraceUtils.BugReportAttachWholeWorldFeature && num2 > 30.0)
						{
							SdFile.Delete(archivePath);
							archivePath = null;
							Log.Error("[BACKTRACE] Exception: World archive too big, deleted");
							return false;
						}
					}
					result = true;
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("[BACKTRACE] Exception: Could not create world archive: {0}", new object[]
			{
				ex.Message
			});
			archivePath = null;
			result = false;
		}
		return result;
	}

	public static void DebugEnableBacktrace()
	{
		BacktraceUtils.s_BacktraceEnabled = true;
		BacktraceUtils.s_VersionEnabled = true;
		BacktraceUtils.InitializeBacktraceClient();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void InitializeBacktraceClient()
	{
		if (BacktraceUtils.Enabled)
		{
			if (BacktraceUtils.s_Configuration == null)
			{
				BacktraceUtils.InitializeConfiguration();
			}
			BacktraceUtils.s_BacktraceClient = BacktraceClient.Initialize(BacktraceUtils.s_Configuration, null, "BacktraceClient");
			BacktraceUtils.SetAttributes(new Dictionary<string, string>
			{
				{
					"svn.commit",
					BacktraceUtils.s_svncommit
				},
				{
					"game.version",
					Constants.cVersionInformation.SerializableString
				}
			});
			BacktraceClient backtraceClient = BacktraceUtils.s_BacktraceClient;
			backtraceClient.OnServerError = (Action<Exception>)Delegate.Combine(backtraceClient.OnServerError, new Action<Exception>(delegate(Exception e)
			{
				Log.Error("[BACKTRACE] Error response: " + e.Message);
			}));
			BacktraceClient backtraceClient2 = BacktraceUtils.s_BacktraceClient;
			backtraceClient2.OnClientReportLimitReached = (Action<BacktraceReport>)Delegate.Combine(backtraceClient2.OnClientReportLimitReached, new Action<BacktraceReport>(delegate(BacktraceReport e)
			{
				Log.Error("[BACKTRACE] Report Limit Reached Error: " + e.Message);
			}));
			return;
		}
		BacktraceUtils.s_BacktraceClient = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void InitializeConfiguration()
	{
		BacktraceUtils.s_Configuration = ScriptableObject.CreateInstance<BacktraceConfiguration>();
		BacktraceUtils.s_Configuration.ServerUrl = "https://thefunpimps.sp.backtrace.io:6098/post?format=json&token=4deafd275ace1a865cc35c882f48a2d1f848c59fabea40227c1bfd84d9c794d9";
		BacktraceUtils.s_Configuration.Enabled = true;
		BacktraceUtils.s_platformString = DeviceFlags.Current.ToString().ToUpper();
		BacktraceUtils.Reset();
		object @lock = BacktraceUtils._lock;
		lock (@lock)
		{
			BacktraceUtils.s_PlayerLogAttachmentPath = Path.Join(Application.temporaryCachePath, "Player.log");
			Log.LogCallbacks += BacktraceUtils.Backtrace_LogCallbacks;
			BacktraceUtils.s_Configuration.AttachmentPaths = new string[]
			{
				BacktraceUtils.s_PlayerLogAttachmentPath
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Backtrace_LogCallbacks(string _msg, string _trace, LogType _type)
	{
		try
		{
			object @lock = BacktraceUtils._lock;
			lock (@lock)
			{
				using (StreamWriter streamWriter = new StreamWriter(BacktraceUtils.s_PlayerLogAttachmentPath, true))
				{
					streamWriter.WriteLine(_msg);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public static void StartStatisticsUpdate()
	{
		DebugGameStats.StartStatisticsUpdate(new DebugGameStats.StatisticsUpdatedCallback(BacktraceUtils.SetAttributes));
	}

	public static void SetAttributes(Dictionary<string, string> _attributesDictionary)
	{
		BacktraceClient backtraceClient = BacktraceUtils.s_BacktraceClient;
		if (backtraceClient == null)
		{
			return;
		}
		backtraceClient.SetAttributes(_attributesDictionary);
	}

	public static void SetAttribute(string _attributeName, string _attributeValue)
	{
		BacktraceClient backtraceClient = BacktraceUtils.s_BacktraceClient;
		if (backtraceClient == null)
		{
			return;
		}
		backtraceClient.SetAttributes(new Dictionary<string, string>
		{
			{
				_attributeName,
				_attributeValue
			}
		});
	}

	public static void UpdateConfig(XmlFile _xmlFile)
	{
		if (BacktraceUtils.s_Configuration == null)
		{
			return;
		}
		BacktraceUtils.Reset();
		XElement root = _xmlFile.XmlDoc.Root;
		if (root == null)
		{
			Log.Out("Could not load Backtrace Config from file " + _xmlFile.Filename + ".");
			return;
		}
		foreach (XElement element in root.Elements("platform"))
		{
			BacktraceUtils.ParsePlatform(element);
		}
		BacktraceUtils.InitializeBacktraceClient();
		Log.Out(string.Format("Backtrace Configuration refreshed from XML: Enabled {0}", BacktraceUtils.Enabled));
		Log.Out("[BACKTRACE] Bug reporting: " + (BacktraceUtils.s_BugReportFeature ? "Enabled" : "Disabled"));
		Log.Out("[BACKTRACE] Bug reporting attach save feature: " + (BacktraceUtils.s_BugReportAttachSaveFeature ? "Enabled" : "Disabled"));
		Log.Out("[BACKTRACE] Bug reporting attach entire world feature: " + (BacktraceUtils.s_BugReportAttachWholeWorldFeature ? "Enabled" : "Disabled"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParsePlatform(XElement _element)
	{
		string text;
		if (!_element.TryGetAttribute("name", out text))
		{
			throw new XmlLoadException("BacktraceConfig", _element, "Platform node attribute 'name' missing");
		}
		if (text.ToUpper() == "DEFAULT" || text.ToUpper() == BacktraceUtils.s_platformString)
		{
			foreach (XElement element in _element.Elements())
			{
				BacktraceUtils.ParsePlatformElement(element);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParsePlatformElement(XElement _element)
	{
		if (_element.Name == BacktraceUtils.enabled)
		{
			string text;
			BacktraceUtils.s_BacktraceEnabled = (_element.TryGetAttribute("value", out text) && text.Equals("true"));
			return;
		}
		if (_element.Name == BacktraceUtils.enabledversions)
		{
			string a;
			_element.TryGetAttribute("value", out a);
			BacktraceUtils.s_VersionEnabled = a.ContainsCaseInsensitive(Constants.cVersionInformation.SerializableString);
			return;
		}
		if (_element.Name == BacktraceUtils.sampling)
		{
			string s;
			_element.TryGetAttribute("value", out s);
			double num;
			if (double.TryParse(s, out num))
			{
				BacktraceUtils.s_Configuration.Sampling = num;
				return;
			}
		}
		else if (_element.Name == BacktraceUtils.deduplicationStrategy)
		{
			string value;
			_element.TryGetAttribute("value", out value);
			DeduplicationStrategy deduplicationStrategy;
			if (Enum.TryParse<DeduplicationStrategy>(value, out deduplicationStrategy))
			{
				BacktraceUtils.s_Configuration.DeduplicationStrategy = deduplicationStrategy;
				return;
			}
		}
		else if (_element.Name == BacktraceUtils.minidumptype)
		{
			string value2;
			_element.TryGetAttribute("value", out value2);
			MiniDumpType minidumpType;
			if (Enum.TryParse<MiniDumpType>(value2, out minidumpType))
			{
				BacktraceUtils.s_Configuration.MinidumpType = minidumpType;
				return;
			}
		}
		else
		{
			if (_element.Name == BacktraceUtils.enablemetricssupport)
			{
				string text2;
				BacktraceUtils.s_Configuration.EnableMetricsSupport = (_element.TryGetAttribute("value", out text2) && text2.Equals("true"));
				return;
			}
			if (_element.Name == BacktraceUtils.bugreporting)
			{
				string text3;
				BacktraceUtils.s_BugReportFeature = (_element.TryGetAttribute("value", out text3) && text3.Equals("true"));
				Log.Out("[BACKTRACE] Bug reporting " + (BacktraceUtils.s_BugReportFeature ? "Enabled" : "Disabled") + " with save uploading: " + (BacktraceUtils.s_BugReportAttachSaveFeature ? "Enabled" : "Disabled"));
				return;
			}
			if (_element.Name == BacktraceUtils.attachsaves)
			{
				string text4;
				BacktraceUtils.s_BugReportAttachSaveFeature = (_element.TryGetAttribute("value", out text4) && text4.Equals("true"));
				Log.Out("[BACKTRACE] Save Attaching Feature: " + (BacktraceUtils.s_BugReportAttachSaveFeature ? "Enabled" : "Disabled"));
				return;
			}
			if (_element.Name == BacktraceUtils.attachentireworld)
			{
				string text5;
				BacktraceUtils.s_BugReportAttachWholeWorldFeature = (_element.TryGetAttribute("value", out text5) && text5.Equals("true"));
				Log.Out("[BACKTRACE] Entire World Attaching Feature: " + (BacktraceUtils.s_BugReportAttachWholeWorldFeature ? "Enabled" : "Disabled"));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const long BacktraceFileSizeLimitMebibyte = 30L;

	[PublicizedFrom(EAccessModifier.Private)]
	public static BacktraceClient s_BacktraceClient;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object _lock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static string s_PlayerLogAttachmentPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string s_svncommit = "73248";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string s_platformString;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName enabled = "enabled";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName enabledversions = "enabledversions";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName sampling = "sampling";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName deduplicationStrategy = "deduplicationstrategy";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName minidumptype = "minidumptype";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName enablemetricssupport = "enablemetricssupport";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName bugreporting = "bugreportfeature";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName attachsaves = "bugreportattachsaves";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XName attachentireworld = "bugreportattachentireworld";

	[PublicizedFrom(EAccessModifier.Private)]
	public static BacktraceConfiguration s_Configuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_BacktraceEnabled = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_VersionEnabled = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_BugReportFeature = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_BugReportAttachSaveFeature = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_BugReportAttachWholeWorldFeature = false;

	public enum SaveArchiveResult
	{
		FailureToArchive,
		Success,
		MissingRegions
	}
}
