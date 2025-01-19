using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class SdFile
{
	public static StreamReader OpenText(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedOpenText(path2);
		}
		return File.OpenText(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static StreamReader ManagedOpenText(SaveDataManagedPath path)
	{
		return new StreamReader(SdFile.ManagedOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read));
	}

	public static StreamWriter CreateText(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedCreateText(path2);
		}
		return File.CreateText(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static StreamWriter ManagedCreateText(SaveDataManagedPath path)
	{
		return new StreamWriter(SdFile.ManagedOpen(path, FileMode.Create, FileAccess.Write, FileShare.Read));
	}

	public static StreamWriter AppendText(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedAppendText(path2);
		}
		return File.AppendText(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static StreamWriter ManagedAppendText(SaveDataManagedPath path)
	{
		return new StreamWriter(SdFile.ManagedOpen(path, FileMode.Append, FileAccess.Write, FileShare.Read));
	}

	public static void Copy(string sourceFileName, string destFileName)
	{
		SaveDataManagedPath sourceFileName2;
		bool flag = SaveDataUtils.TryGetManagedPath(sourceFileName, out sourceFileName2);
		SaveDataManagedPath destFileName2;
		bool flag2 = SaveDataUtils.TryGetManagedPath(destFileName, out destFileName2);
		if (flag && flag2)
		{
			SdFile.ManagedToManagedCopy(sourceFileName2, destFileName2, false);
			return;
		}
		if (flag)
		{
			SdFile.ManagedToUnmanagedCopy(sourceFileName2, destFileName, false);
			return;
		}
		if (flag2)
		{
			SdFile.UnmanagedToManagedCopy(sourceFileName, destFileName2, false);
			return;
		}
		File.Copy(sourceFileName, destFileName);
	}

	public static void Copy(string sourceFileName, string destFileName, bool overwrite)
	{
		SaveDataManagedPath sourceFileName2;
		bool flag = SaveDataUtils.TryGetManagedPath(sourceFileName, out sourceFileName2);
		SaveDataManagedPath destFileName2;
		bool flag2 = SaveDataUtils.TryGetManagedPath(destFileName, out destFileName2);
		if (flag && flag2)
		{
			SdFile.ManagedToManagedCopy(sourceFileName2, destFileName2, overwrite);
			return;
		}
		if (flag)
		{
			SdFile.ManagedToUnmanagedCopy(sourceFileName2, destFileName, overwrite);
			return;
		}
		if (flag2)
		{
			SdFile.UnmanagedToManagedCopy(sourceFileName, destFileName2, overwrite);
			return;
		}
		File.Copy(sourceFileName, destFileName, overwrite);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ManagedToManagedCopy(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName, bool overwrite)
	{
		using (Stream stream = SdFile.ManagedOpen(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			using (Stream stream2 = SdFile.ManagedOpen(destFileName, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.Read))
			{
				StreamUtils.StreamCopy(stream, stream2, null, true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ManagedToUnmanagedCopy(SaveDataManagedPath sourceFileName, string destFileName, bool overwrite)
	{
		using (Stream stream = SdFile.ManagedOpen(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			using (FileStream fileStream = File.Open(destFileName, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.Read))
			{
				StreamUtils.StreamCopy(stream, fileStream, null, true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void UnmanagedToManagedCopy(string sourceFileName, SaveDataManagedPath destFileName, bool overwrite)
	{
		using (FileStream fileStream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			using (Stream stream = SdFile.ManagedOpen(destFileName, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.Read))
			{
				StreamUtils.StreamCopy(fileStream, stream, null, true);
			}
		}
	}

	public static Stream Create(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedCreate(path2);
		}
		return File.Create(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static Stream ManagedCreate(SaveDataManagedPath path)
	{
		return SdFile.ManagedOpen(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
	}

	public static void Delete(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			SdFile.ManagedDelete(path2);
			return;
		}
		File.Delete(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ManagedDelete(SaveDataManagedPath path)
	{
		SaveDataUtils.SaveDataManager.ManagedFileDelete(path);
	}

	public static bool Exists(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedExists(path2);
		}
		return File.Exists(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool ManagedExists(SaveDataManagedPath path)
	{
		return SaveDataUtils.SaveDataManager.ManagedFileExists(path);
	}

	public static Stream Open(string path, FileMode mode)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedOpen(path2, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);
		}
		return File.Open(path, mode);
	}

	public static Stream Open(string path, FileMode mode, FileAccess access)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedOpen(path2, mode, access, FileShare.None);
		}
		return File.Open(path, mode, access);
	}

	public static Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedOpen(path2, mode, access, share);
		}
		return File.Open(path, mode, access, share);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static Stream ManagedOpen(SaveDataManagedPath path, FileMode mode, FileAccess access, FileShare share)
	{
		return SaveDataUtils.SaveDataManager.ManagedFileOpen(path, mode, access, share);
	}

	public static DateTime GetLastWriteTime(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedGetLastWriteTime(path2);
		}
		return File.GetLastWriteTime(path);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime ManagedGetLastWriteTime(SaveDataManagedPath path)
	{
		return SdFile.ManagedGetLastWriteTimeUtc(path).ToLocalTime();
	}

	public static DateTime GetLastWriteTimeUtc(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedGetLastWriteTimeUtc(path2);
		}
		return File.GetLastWriteTimeUtc(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static DateTime ManagedGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return SaveDataUtils.SaveDataManager.ManagedFileGetLastWriteTimeUtc(path);
	}

	public static Stream OpenRead(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedOpen(path2, FileMode.Open, FileAccess.Read, FileShare.Read);
		}
		return File.OpenRead(path);
	}

	public static Stream OpenWrite(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedOpen(path2, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		}
		return File.OpenWrite(path);
	}

	public static string ReadAllText(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedReadAllText(path2, Encoding.UTF8);
		}
		return File.ReadAllText(path);
	}

	public static string ReadAllText(string path, Encoding encoding)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedReadAllText(path2, encoding);
		}
		return File.ReadAllText(path, encoding);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static string ManagedReadAllText(SaveDataManagedPath path, Encoding encoding)
	{
		string result;
		using (Stream stream = SdFile.ManagedOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			using (StreamReader streamReader = new StreamReader(stream, encoding))
			{
				result = streamReader.ReadToEnd();
			}
		}
		return result;
	}

	public static void WriteAllText(string path, string contents)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			SdFile.ManagedWriteAllText(path2, contents, SdFile.UTF8NoBom);
			return;
		}
		File.WriteAllText(path, contents);
	}

	public static void WriteAllText(string path, string contents, Encoding encoding)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			SdFile.ManagedWriteAllText(path2, contents, encoding);
			return;
		}
		File.WriteAllText(path, contents, encoding);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedWriteAllText(SaveDataManagedPath path, string contents, Encoding encoding)
	{
		using (Stream stream = SdFile.ManagedOpen(path, FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			using (StreamWriter streamWriter = new StreamWriter(stream, encoding))
			{
				streamWriter.Write(contents);
			}
		}
	}

	public static byte[] ReadAllBytes(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedReadAllBytes(path2);
		}
		return File.ReadAllBytes(path);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] ManagedReadAllBytes(SaveDataManagedPath path)
	{
		byte[] result;
		using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true))
		{
			using (Stream stream = SdFile.ManagedOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				stream.CopyTo(pooledExpandableMemoryStream);
				result = pooledExpandableMemoryStream.ToArray();
			}
		}
		return result;
	}

	public static void WriteAllBytes(string path, byte[] bytes)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			SdFile.ManagedWriteAllBytes(path2, bytes);
			return;
		}
		File.WriteAllBytes(path, bytes);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedWriteAllBytes(SaveDataManagedPath path, byte[] bytes)
	{
		using (Stream stream = SdFile.ManagedOpen(path, FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			stream.Write(bytes, 0, bytes.Length);
		}
	}

	public static string[] ReadAllLines(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedReadAllLines(path2, Encoding.UTF8);
		}
		return File.ReadAllLines(path);
	}

	public static string[] ReadAllLines(string path, Encoding encoding)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedReadAllLines(path2, encoding);
		}
		return File.ReadAllLines(path, encoding);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] ManagedReadAllLines(SaveDataManagedPath path, Encoding encoding)
	{
		List<string> list = new List<string>();
		string[] result;
		using (Stream stream = SdFile.ManagedOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			using (StreamReader streamReader = new StreamReader(stream, encoding))
			{
				for (;;)
				{
					string text = streamReader.ReadLine();
					if (text == null)
					{
						break;
					}
					list.Add(text);
				}
				result = list.ToArray();
			}
		}
		return result;
	}

	public static IEnumerable<string> ReadLines(string path)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedReadLines(path2, Encoding.UTF8);
		}
		return File.ReadLines(path);
	}

	public static IEnumerable<string> ReadLines(string path, Encoding encoding)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			return SdFile.ManagedReadLines(path2, encoding);
		}
		return File.ReadLines(path, encoding);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<string> ManagedReadLines(SaveDataManagedPath path, Encoding encoding)
	{
		SdFile.<ManagedReadLines>d__43 <ManagedReadLines>d__ = new SdFile.<ManagedReadLines>d__43(-2);
		<ManagedReadLines>d__.<>3__path = path;
		<ManagedReadLines>d__.<>3__encoding = encoding;
		return <ManagedReadLines>d__;
	}

	public static void WriteAllLines(string path, string[] contents)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			SdFile.ManagedWriteAllLines(path2, contents, SdFile.UTF8NoBom);
			return;
		}
		File.WriteAllLines(path, contents);
	}

	public static void WriteAllLines(string path, string[] contents, Encoding encoding)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			SdFile.ManagedWriteAllLines(path2, contents, encoding);
			return;
		}
		File.WriteAllLines(path, contents, encoding);
	}

	public static void WriteAllLines(string path, IEnumerable<string> contents)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			SdFile.ManagedWriteAllLines(path2, contents, SdFile.UTF8NoBom);
			return;
		}
		File.WriteAllLines(path, contents);
	}

	public static void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			SdFile.ManagedWriteAllLines(path2, contents, encoding);
			return;
		}
		File.WriteAllLines(path, contents, encoding);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedWriteAllLines(SaveDataManagedPath path, IEnumerable<string> contents, Encoding encoding)
	{
		using (Stream stream = SdFile.ManagedOpen(path, FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			using (StreamWriter streamWriter = new StreamWriter(stream, encoding))
			{
				SdFile.ManagedWriteAllLines(streamWriter, contents);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedWriteAllLines(TextWriter writer, IEnumerable<string> contents)
	{
		try
		{
			foreach (string value in contents)
			{
				writer.WriteLine(value);
			}
		}
		finally
		{
			if (writer != null)
			{
				((IDisposable)writer).Dispose();
			}
		}
	}

	public static void AppendAllText(string path, string contents)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			SdFile.ManagedAppendAllText(path2, contents, SdFile.UTF8NoBom);
			return;
		}
		File.AppendAllText(path, contents);
	}

	public static void AppendAllText(string path, string contents, Encoding encoding)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			SdFile.ManagedAppendAllText(path2, contents, encoding);
			return;
		}
		File.AppendAllText(path, contents, encoding);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedAppendAllText(SaveDataManagedPath path, string contents, Encoding encoding)
	{
		using (Stream stream = SdFile.ManagedOpen(path, FileMode.Append, FileAccess.Write, FileShare.Read))
		{
			using (StreamWriter streamWriter = new StreamWriter(stream, encoding))
			{
				streamWriter.Write(contents);
			}
		}
	}

	public static void AppendAllLines(string path, IEnumerable<string> contents)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			SdFile.ManagedAppendAllLines(path2, contents, SdFile.UTF8NoBom);
			return;
		}
		File.AppendAllLines(path, contents);
	}

	public static void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
	{
		SaveDataManagedPath path2;
		if (SaveDataUtils.TryGetManagedPath(path, out path2))
		{
			SdFile.ManagedAppendAllLines(path2, contents, encoding);
			return;
		}
		File.AppendAllLines(path, contents, encoding);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedAppendAllLines(SaveDataManagedPath path, IEnumerable<string> contents, Encoding encoding)
	{
		using (Stream stream = SdFile.ManagedOpen(path, FileMode.Append, FileAccess.Write, FileShare.Read))
		{
			using (StreamWriter streamWriter = new StreamWriter(stream, encoding))
			{
				SdFile.ManagedWriteAllLines(streamWriter, contents);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Encoding UTF8NoBom = new UTF8Encoding(false);
}
