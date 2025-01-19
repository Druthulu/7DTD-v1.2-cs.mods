using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class SdDirectory
{
	public static SdDirectoryInfo CreateDirectory(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdDirectory.ManagedCreateDirectory(path2);
		}
		return new SdDirectoryInfo(Directory.CreateDirectory(path));
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static SdDirectoryInfo ManagedCreateDirectory(SaveDataManagedPath path)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryCreateDirectory(path);
	}

	public static DateTime GetLastWriteTime(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdDirectory.ManagedGetLastWriteTime(path2);
		}
		return Directory.GetLastWriteTime(path);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime ManagedGetLastWriteTime(SaveDataManagedPath path)
	{
		return SdDirectory.ManagedGetLastWriteTimeUtc(path).ToLocalTime();
	}

	public static DateTime GetLastWriteTimeUtc(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdDirectory.ManagedGetLastWriteTimeUtc(path2);
		}
		return Directory.GetLastWriteTimeUtc(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static DateTime ManagedGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryGetLastWriteTimeUtc(path);
	}

	public static bool Exists(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdDirectory.ManagedExists(path2);
		}
		return Directory.Exists(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool ManagedExists(SaveDataManagedPath path)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryExists(path);
	}

	public static string[] GetFiles(string path)
	{
		return SdDirectory.EnumerateFiles(path).ToArray<string>();
	}

	public static string[] GetFiles(string path, string searchPattern)
	{
		return SdDirectory.EnumerateFiles(path, searchPattern).ToArray<string>();
	}

	public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
	{
		return SdDirectory.EnumerateFiles(path, searchPattern, searchOption).ToArray<string>();
	}

	public static string[] GetDirectories(string path)
	{
		return SdDirectory.EnumerateDirectories(path).ToArray<string>();
	}

	public static string[] GetDirectories(string path, string searchPattern)
	{
		return SdDirectory.EnumerateDirectories(path, searchPattern).ToArray<string>();
	}

	public static string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
	{
		return SdDirectory.EnumerateDirectories(path, searchPattern, searchOption).ToArray<string>();
	}

	public static string[] GetFileSystemEntries(string path)
	{
		return SdDirectory.EnumerateFileSystemEntries(path).ToArray<string>();
	}

	public static string[] GetFileSystemEntries(string path, string searchPattern)
	{
		return SdDirectory.EnumerateFileSystemEntries(path, searchPattern).ToArray<string>();
	}

	public static string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
	{
		return SdDirectory.EnumerateFileSystemEntries(path, searchPattern, searchOption).ToArray<string>();
	}

	public static IEnumerable<string> EnumerateDirectories(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return from x in SdDirectory.ManagedEnumerateDirectories(path2, "*", SearchOption.TopDirectoryOnly)
			select x.GetOriginalPath();
		}
		return Directory.EnumerateDirectories(path);
	}

	public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return from x in SdDirectory.ManagedEnumerateDirectories(path2, searchPattern, SearchOption.TopDirectoryOnly)
			select x.GetOriginalPath();
		}
		return Directory.EnumerateDirectories(path, searchPattern);
	}

	public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return from x in SdDirectory.ManagedEnumerateDirectories(path2, searchPattern, searchOption)
			select x.GetOriginalPath();
		}
		return Directory.EnumerateDirectories(path, searchPattern, searchOption);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<SaveDataManagedPath> ManagedEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryEnumerateDirectories(path, searchPattern, searchOption);
	}

	public static IEnumerable<string> EnumerateFiles(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return from x in SdDirectory.ManagedEnumerateFiles(path2, "*", SearchOption.TopDirectoryOnly)
			select x.GetOriginalPath();
		}
		return Directory.EnumerateFiles(path);
	}

	public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return from x in SdDirectory.ManagedEnumerateFiles(path2, searchPattern, SearchOption.TopDirectoryOnly)
			select x.GetOriginalPath();
		}
		return Directory.EnumerateFiles(path, searchPattern);
	}

	public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return from x in SdDirectory.ManagedEnumerateFiles(path2, searchPattern, searchOption)
			select x.GetOriginalPath();
		}
		return Directory.EnumerateFiles(path, searchPattern, searchOption);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<SaveDataManagedPath> ManagedEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryEnumerateFiles(path, searchPattern, searchOption);
	}

	public static IEnumerable<string> EnumerateFileSystemEntries(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return from x in SdDirectory.ManagedEnumerateFileSystemEntries(path2, "*", SearchOption.TopDirectoryOnly)
			select x.GetOriginalPath();
		}
		return Directory.EnumerateFileSystemEntries(path);
	}

	public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return from x in SdDirectory.ManagedEnumerateFileSystemEntries(path2, searchPattern, SearchOption.TopDirectoryOnly)
			select x.GetOriginalPath();
		}
		return Directory.EnumerateFileSystemEntries(path, searchPattern);
	}

	public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return from x in SdDirectory.ManagedEnumerateFileSystemEntries(path2, searchPattern, searchOption)
			select x.GetOriginalPath();
		}
		return Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<SaveDataManagedPath> ManagedEnumerateFileSystemEntries(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return SaveDataUtils.SaveDataManager.ManagedDirectoryEnumerateFileSystemEntries(path, searchPattern, searchOption);
	}

	public static string[] GetLogicalDrives()
	{
		return Directory.GetLogicalDrives();
	}

	public static string GetDirectoryRoot(string path)
	{
		return Directory.GetDirectoryRoot(path);
	}

	public static string GetCurrentDirectory()
	{
		return Directory.GetCurrentDirectory();
	}

	public static void SetCurrentDirectory(string path)
	{
		Directory.SetCurrentDirectory(path);
	}

	public static void Delete(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			SdDirectory.ManagedDelete(path2, false);
			return;
		}
		Directory.Delete(path);
	}

	public static void Delete(string path, bool recursive)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			SdDirectory.ManagedDelete(path2, recursive);
			return;
		}
		Directory.Delete(path, recursive);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ManagedDelete(SaveDataManagedPath path, bool recursive)
	{
		SaveDataUtils.SaveDataManager.ManagedDirectoryDelete(path, recursive);
	}
}
