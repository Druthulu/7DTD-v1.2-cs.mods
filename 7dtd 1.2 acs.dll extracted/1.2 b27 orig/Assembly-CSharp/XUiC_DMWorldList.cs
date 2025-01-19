using System;
using System.Collections.ObjectModel;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DMWorldList : XUiC_DMBaseList<XUiC_DMWorldList.ListEntry>
{
	public override void RebuildList(bool _resetFilter = false)
	{
	}

	public void RebuildList(ReadOnlyCollection<SaveInfoProvider.WorldEntryInfo> worldEntryInfos, bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (SaveInfoProvider.WorldEntryInfo worldEntryInfo in worldEntryInfos)
		{
			this.allEntries.Add(new XUiC_DMWorldList.ListEntry(worldEntryInfo, this.matchingVersionColor, this.compatibleVersionColor, this.incompatibleVersionColor));
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public void ClearList()
	{
		this.allEntries.Clear();
		this.RebuildList(true);
	}

	public bool SelectByKey(string _key)
	{
		if (string.IsNullOrEmpty(_key))
		{
			return false;
		}
		for (int i = 0; i < this.filteredEntries.Count; i++)
		{
			if (this.filteredEntries[i].Key.Equals(_key, StringComparison.OrdinalIgnoreCase))
			{
				base.SelectedEntryIndex = i;
				return true;
			}
		}
		return false;
	}

	public void UpdateHiddenEntryVisibility()
	{
		this.filteredEntries.Clear();
		this.FilterResults(this.previousMatch);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FilterResults(string _textMatch)
	{
		base.FilterResults(_textMatch);
		for (int i = 0; i < this.filteredEntries.Count; i++)
		{
			XUiC_DMWorldList.ListEntry listEntry = this.filteredEntries[i];
			if (listEntry.HideIfEmpty && listEntry.SaveDataSize == 0L)
			{
				this.filteredEntries.RemoveAt(i);
				i--;
			}
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "matching_version_color")
		{
			this.matchingVersionColor = _value;
			return true;
		}
		if (_name == "compatible_version_color")
		{
			this.compatibleVersionColor = _value;
			return true;
		}
		if (!(_name == "incompatible_version_color"))
		{
			return base.ParseAttribute(_name, _value, _parent);
		}
		this.incompatibleVersionColor = _value;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string matchingVersionColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string compatibleVersionColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string incompatibleVersionColor;

	[Preserve]
	public class ListEntry : XUiListEntry
	{
		public ListEntry(SaveInfoProvider.WorldEntryInfo worldEntryInfo, string matchingColor, string compatibleColor, string incompatibleColor)
		{
			this.WorldEntryInfo = worldEntryInfo;
			this.Key = worldEntryInfo.WorldKey;
			this.Name = worldEntryInfo.Name;
			this.Type = worldEntryInfo.Type;
			this.Location = worldEntryInfo.Location;
			this.Deletable = worldEntryInfo.Deletable;
			this.WorldDataSize = worldEntryInfo.WorldDataSize;
			this.Version = worldEntryInfo.Version;
			VersionInformation version = this.Version;
			this.versionComparison = ((version != null) ? version.CompareToRunningBuild() : VersionInformation.EVersionComparisonResult.SameBuild);
			this.SaveDataSize = worldEntryInfo.SaveDataSize;
			this.SaveDataCount = worldEntryInfo.SaveDataCount;
			this.matchingColor = matchingColor;
			this.compatibleColor = compatibleColor;
			this.incompatibleColor = incompatibleColor;
			this.HideIfEmpty = worldEntryInfo.HideIfEmpty;
		}

		public override int CompareTo(object _otherEntry)
		{
			XUiC_DMWorldList.ListEntry listEntry = _otherEntry as XUiC_DMWorldList.ListEntry;
			if (listEntry != null)
			{
				return this.WorldEntryInfo.CompareTo(listEntry.WorldEntryInfo);
			}
			return 1;
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
			if (num <= 1361572173U)
			{
				if (num <= 941220257U)
				{
					if (num != 572742324U)
					{
						if (num == 941220257U)
						{
							if (_bindingName == "saveDataSize")
							{
								_value = XUiC_DataManagement.FormatMemoryString(this.SaveDataSize);
								return true;
							}
						}
					}
					else if (_bindingName == "totalDataSize")
					{
						long bytes = this.Deletable ? (this.WorldDataSize + this.SaveDataSize) : this.SaveDataSize;
						_value = XUiC_DataManagement.FormatMemoryString(bytes);
						return true;
					}
				}
				else if (num != 1181855383U)
				{
					if (num != 1204961018U)
					{
						if (num == 1361572173U)
						{
							if (_bindingName == "type")
							{
								_value = this.Type;
								return true;
							}
						}
					}
					else if (_bindingName == "worldDataSize")
					{
						_value = (this.Deletable ? XUiC_DataManagement.FormatMemoryString(this.WorldDataSize) : "-");
						return true;
					}
				}
				else if (_bindingName == "version")
				{
					_value = ((this.Version == null) ? string.Empty : ((this.Version.Major >= 0) ? this.Version.LongStringNoBuild : Constants.cVersionInformation.LongStringNoBuild));
					return true;
				}
			}
			else if (num <= 2049496678U)
			{
				if (num != 1566407741U)
				{
					if (num == 2049496678U)
					{
						if (_bindingName == "versioncolor")
						{
							_value = ((this.Version == null) ? this.incompatibleColor : ((this.Version.Major < 0 || this.versionComparison == VersionInformation.EVersionComparisonResult.SameBuild || this.versionComparison == VersionInformation.EVersionComparisonResult.SameMinor) ? this.matchingColor : ((this.versionComparison == VersionInformation.EVersionComparisonResult.NewerMinor || this.versionComparison == VersionInformation.EVersionComparisonResult.OlderMinor) ? this.compatibleColor : this.incompatibleColor)));
							return true;
						}
					}
				}
				else if (_bindingName == "hasentry")
				{
					_value = true.ToString();
					return true;
				}
			}
			else if (num != 2369371622U)
			{
				if (num != 2497009599U)
				{
					if (num == 4086844294U)
					{
						if (_bindingName == "versiontooltip")
						{
							_value = ((this.Version == null) ? string.Empty : ((this.Version.Major < 0 || this.versionComparison == VersionInformation.EVersionComparisonResult.SameBuild || this.versionComparison == VersionInformation.EVersionComparisonResult.SameMinor) ? "" : ((this.versionComparison == VersionInformation.EVersionComparisonResult.NewerMinor) ? Localization.Get("xuiSavegameNewerMinor", false) : ((this.versionComparison == VersionInformation.EVersionComparisonResult.OlderMinor) ? Localization.Get("xuiSavegameOlderMinor", false) : Localization.Get("xuiSavegameDifferentMajor", false)))));
							return true;
						}
					}
				}
				else if (_bindingName == "saveDataCount")
				{
					_value = string.Format("{0} {1}", this.SaveDataCount, Localization.Get("xuiDmSaves", false));
					return true;
				}
			}
			else if (_bindingName == "name")
			{
				_value = this.Name;
				return true;
			}
			return false;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.Name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
			if (num <= 1361572173U)
			{
				if (num <= 941220257U)
				{
					if (num != 572742324U)
					{
						if (num != 941220257U)
						{
							return false;
						}
						if (!(_bindingName == "saveDataSize"))
						{
							return false;
						}
					}
					else if (!(_bindingName == "totalDataSize"))
					{
						return false;
					}
				}
				else if (num != 1181855383U)
				{
					if (num != 1204961018U)
					{
						if (num != 1361572173U)
						{
							return false;
						}
						if (!(_bindingName == "type"))
						{
							return false;
						}
					}
					else if (!(_bindingName == "worldDataSize"))
					{
						return false;
					}
				}
				else if (!(_bindingName == "version"))
				{
					return false;
				}
			}
			else if (num <= 2049496678U)
			{
				if (num != 1566407741U)
				{
					if (num != 2049496678U)
					{
						return false;
					}
					if (!(_bindingName == "versioncolor"))
					{
						return false;
					}
					_value = "0,0,0";
					return true;
				}
				else
				{
					if (!(_bindingName == "hasentry"))
					{
						return false;
					}
					_value = false.ToString();
					return true;
				}
			}
			else if (num != 2369371622U)
			{
				if (num != 2497009599U)
				{
					if (num != 4086844294U)
					{
						return false;
					}
					if (!(_bindingName == "versiontooltip"))
					{
						return false;
					}
				}
				else if (!(_bindingName == "saveDataCount"))
				{
					return false;
				}
			}
			else if (!(_bindingName == "name"))
			{
				return false;
			}
			_value = string.Empty;
			return true;
		}

		public readonly string Key;

		public readonly string Name;

		public readonly string Type;

		public readonly PathAbstractions.AbstractedLocation Location;

		public readonly bool Deletable;

		public readonly long WorldDataSize;

		public readonly VersionInformation Version;

		public readonly VersionInformation.EVersionComparisonResult versionComparison;

		public readonly long SaveDataSize;

		public readonly int SaveDataCount;

		public readonly bool HideIfEmpty;

		public readonly SaveInfoProvider.WorldEntryInfo WorldEntryInfo;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string matchingColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string compatibleColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string incompatibleColor;
	}
}
