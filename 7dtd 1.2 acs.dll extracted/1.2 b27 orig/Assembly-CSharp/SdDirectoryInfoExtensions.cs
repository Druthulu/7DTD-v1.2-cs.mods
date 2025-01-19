using System;
using System.Collections.Generic;
using System.IO;

public static class SdDirectoryInfoExtensions
{
	public static bool IsDirEmpty(this SdDirectoryInfo possiblyEmptyDir)
	{
		bool result;
		using (IEnumerator<SdFileSystemInfo> enumerator = possiblyEmptyDir.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).GetEnumerator())
		{
			result = !enumerator.MoveNext();
		}
		return result;
	}
}
