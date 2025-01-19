﻿using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

public class VersionInformation : IComparable<VersionInformation>
{
	public VersionInformation(VersionInformation.EGameReleaseType _releaseType, int _major, int _minor, int _build)
	{
		this.ReleaseType = _releaseType;
		this.Major = _major;
		this.Minor = _minor;
		this.Build = _build;
		this.NumericalRepresentation = (int)((this.Major < 1) ? ((VersionInformation.EGameReleaseType)(-1)) : (((this.ReleaseType * (VersionInformation.EGameReleaseType)100 + this.Major) * (VersionInformation.EGameReleaseType)100 + this.Minor) * (VersionInformation.EGameReleaseType)1000 + this.Build));
		this.ShortString = ((this.Major < 1) ? "Unk" : string.Format("{0}{1}.{2}", this.ReleaseType.ToStringCached<VersionInformation.EGameReleaseType>()[0], this.Major, this.Minor));
		this.LongStringNoBuild = ((this.Major < 1) ? "Unknown" : string.Format("{0} {1}.{2}", this.ReleaseType.ToStringCached<VersionInformation.EGameReleaseType>(), this.Major, this.Minor));
		this.LongString = ((this.Major < 1) ? "Unknown" : string.Format("{0} {1}.{2} (b{3})", new object[]
		{
			this.ReleaseType.ToStringCached<VersionInformation.EGameReleaseType>(),
			this.Major,
			this.Minor,
			this.Build
		}));
		this.SerializableString = string.Format("{0}.{1}.{2}.{3}", new object[]
		{
			this.ReleaseType.ToStringCached<VersionInformation.EGameReleaseType>(),
			this.Major,
			this.Minor,
			this.Build
		});
		this.Version = new Version((int)((this.ReleaseType >= VersionInformation.EGameReleaseType.Alpha) ? this.ReleaseType : VersionInformation.EGameReleaseType.Alpha), (this.Major >= 0) ? this.Major : 0, (this.Minor >= 0) ? this.Minor : 0, (this.Build >= 0) ? this.Build : 0);
		this.IsValid = (this.Major > 0);
	}

	public int CompareTo(VersionInformation _other)
	{
		int num = this.ReleaseType.CompareTo(_other.ReleaseType);
		if (num != 0)
		{
			return num;
		}
		int num2 = this.Major.CompareTo(_other.Major);
		if (num2 != 0)
		{
			return num2;
		}
		int num3 = this.Minor.CompareTo(_other.Minor);
		if (num3 != 0)
		{
			return num3;
		}
		return this.Build.CompareTo(_other.Build);
	}

	public bool EqualsMinor(VersionInformation _other)
	{
		return this.ReleaseType == _other.ReleaseType && this.Major == _other.Major && this.Minor == _other.Minor;
	}

	public bool EqualsMajor(VersionInformation _other)
	{
		return this.ReleaseType == _other.ReleaseType && this.Major == _other.Major;
	}

	public VersionInformation.EVersionComparisonResult CompareToRunningBuild()
	{
		VersionInformation cVersionInformation = Constants.cVersionInformation;
		if (this.ReleaseType != cVersionInformation.ReleaseType || this.Major != cVersionInformation.Major)
		{
			return VersionInformation.EVersionComparisonResult.DifferentMajor;
		}
		if (this.Minor < cVersionInformation.Minor)
		{
			return VersionInformation.EVersionComparisonResult.OlderMinor;
		}
		if (this.Minor > cVersionInformation.Minor)
		{
			return VersionInformation.EVersionComparisonResult.NewerMinor;
		}
		if (this.Build != cVersionInformation.Build)
		{
			return VersionInformation.EVersionComparisonResult.SameMinor;
		}
		return VersionInformation.EVersionComparisonResult.SameBuild;
	}

	public static bool TryParseSerializedString(string _serializedVersionInformation, out VersionInformation _result)
	{
		_result = null;
		string[] array = _serializedVersionInformation.Split('.', StringSplitOptions.None);
		if (array.Length != 4)
		{
			return false;
		}
		VersionInformation.EGameReleaseType releaseType;
		if (!EnumUtils.TryParse<VersionInformation.EGameReleaseType>(array[0], out releaseType, false))
		{
			return false;
		}
		int major;
		if (!StringParsers.TryParseSInt32(array[1], out major, 0, -1, NumberStyles.Integer))
		{
			return false;
		}
		int minor;
		if (!StringParsers.TryParseSInt32(array[2], out minor, 0, -1, NumberStyles.Integer))
		{
			return false;
		}
		int build;
		if (!StringParsers.TryParseSInt32(array[3], out build, 0, -1, NumberStyles.Integer))
		{
			return false;
		}
		_result = new VersionInformation(releaseType, major, minor, build);
		return true;
	}

	public static bool TryParseLegacyString(string _legacyVersionString, out VersionInformation _verInfo)
	{
		Match match = VersionInformation.legacyVersionStringMatcher.Match(_legacyVersionString);
		_verInfo = null;
		if (!match.Success)
		{
			return false;
		}
		int major;
		if (!StringParsers.TryParseSInt32(match.Groups[1].Value, out major, 0, -1, NumberStyles.Integer))
		{
			return false;
		}
		int minor;
		if (match.Groups[2].Success)
		{
			if (!StringParsers.TryParseSInt32(match.Groups[2].Value, out minor, 0, -1, NumberStyles.Integer))
			{
				return false;
			}
		}
		else
		{
			minor = 0;
		}
		int build;
		if (!StringParsers.TryParseSInt32(match.Groups[3].Value, out build, 0, -1, NumberStyles.Integer))
		{
			return false;
		}
		_verInfo = new VersionInformation(VersionInformation.EGameReleaseType.Alpha, major, minor, build);
		return true;
	}

	public void Write(BinaryWriter _writer)
	{
		_writer.Write((byte)this.ReleaseType);
		_writer.Write(this.Major);
		_writer.Write(this.Minor);
		_writer.Write(this.Build);
	}

	public static VersionInformation Read(BinaryReader _reader)
	{
		VersionInformation.EGameReleaseType releaseType = (VersionInformation.EGameReleaseType)_reader.ReadByte();
		int major = _reader.ReadInt32();
		int minor = _reader.ReadInt32();
		int build = _reader.ReadInt32();
		return new VersionInformation(releaseType, major, minor, build);
	}

	public readonly VersionInformation.EGameReleaseType ReleaseType;

	public readonly int Major;

	public readonly int Minor;

	public readonly int Build;

	public readonly bool IsValid;

	public readonly int NumericalRepresentation;

	public readonly string ShortString;

	public readonly string LongStringNoBuild;

	public readonly string LongString;

	public readonly string SerializableString;

	public readonly Version Version;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex legacyVersionStringMatcher = new Regex("^\\s*Alpha\\s*(\\d+)(?:\\.(\\d+))?\\s*(?:\\(b(\\d+)\\))?\\s*$");

	public enum EGameReleaseType
	{
		Alpha,
		V
	}

	public enum EVersionComparisonResult
	{
		SameBuild,
		SameMinor,
		NewerMinor,
		OlderMinor,
		DifferentMajor
	}
}
