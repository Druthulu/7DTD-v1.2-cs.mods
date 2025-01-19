using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DMSavegamesList : XUiC_DMBaseList<XUiC_DMSavegamesList.ListEntry>
{
	public override void RebuildList(bool _resetFilter = false)
	{
	}

	public void RebuildList(ReadOnlyCollection<SaveInfoProvider.SaveEntryInfo> saveEntryInfos, bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (SaveInfoProvider.SaveEntryInfo saveEntryInfo in saveEntryInfos)
		{
			this.allEntries.Add(new XUiC_DMSavegamesList.ListEntry(saveEntryInfo, this.matchingVersionColor, this.compatibleVersionColor, this.incompatibleVersionColor));
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public void ClearList()
	{
		this.allEntries.Clear();
		this.RebuildList(true);
	}

	public bool SelectByName(string _name)
	{
		if (!string.IsNullOrEmpty(_name))
		{
			for (int i = 0; i < this.filteredEntries.Count; i++)
			{
				if (this.filteredEntries[i].saveName.Equals(_name, StringComparison.OrdinalIgnoreCase))
				{
					base.SelectedEntryIndex = i;
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnSearchInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		base.OnSearchInputChanged(_sender, _text, _changeFromCode);
	}

	public void SetWorldFilter(string _worldKey)
	{
		this.worldFilter = _worldKey;
		this.filteredEntries.Clear();
		this.FilterResults(this.previousMatch);
		this.RefreshView(false, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FilterResults(string _textMatch)
	{
		base.FilterResults(_textMatch);
		if (this.worldFilter == null)
		{
			return;
		}
		for (int i = 0; i < this.filteredEntries.Count; i++)
		{
			if (this.filteredEntries[i].worldKey != this.worldFilter)
			{
				this.filteredEntries.RemoveAt(i);
				i--;
			}
		}
	}

	public IEnumerable<XUiC_DMSavegamesList.ListEntry> GetSavesInWorld(string _worldKey)
	{
		if (string.IsNullOrEmpty(_worldKey))
		{
			yield break;
		}
		int num;
		for (int i = 0; i < this.allEntries.Count; i = num + 1)
		{
			if (this.allEntries[i].worldKey == _worldKey)
			{
				yield return this.allEntries[i];
			}
			num = i;
		}
		yield break;
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

	public string worldFilter;

	[Preserve]
	public class ListEntry : XUiListEntry
	{
		public ListEntry(SaveInfoProvider.SaveEntryInfo saveEntryInfo, string matchingColor = "255,255,255", string compatibleColor = "255,255,255", string incompatibleColor = "255,255,255")
		{
			this.saveEntryInfo = saveEntryInfo;
			this.saveName = saveEntryInfo.Name;
			this.worldKey = saveEntryInfo.WorldEntry.WorldKey;
			this.saveDirectory = saveEntryInfo.SaveDir;
			this.lastSaved = saveEntryInfo.LastSaved;
			this.version = saveEntryInfo.Version;
			VersionInformation versionInformation = this.version;
			this.versionComparison = ((versionInformation != null) ? versionInformation.CompareToRunningBuild() : VersionInformation.EVersionComparisonResult.SameBuild);
			this.matchingColor = matchingColor;
			this.compatibleColor = compatibleColor;
			this.incompatibleColor = incompatibleColor;
		}

		public override int CompareTo(object _otherEntry)
		{
			XUiC_DMSavegamesList.ListEntry listEntry = _otherEntry as XUiC_DMSavegamesList.ListEntry;
			if (listEntry != null)
			{
				return this.saveEntryInfo.CompareTo(listEntry.saveEntryInfo);
			}
			return 1;
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
			if (num <= 1566407741U)
			{
				if (num <= 205488363U)
				{
					if (num != 8204530U)
					{
						if (num != 205488363U)
						{
							return false;
						}
						if (!(_bindingName == "savename"))
						{
							return false;
						}
						_value = this.saveName;
						return true;
					}
					else if (!(_bindingName == "entrycolor"))
					{
						return false;
					}
				}
				else if (num != 1181855383U)
				{
					if (num != 1355330520U)
					{
						if (num != 1566407741U)
						{
							return false;
						}
						if (!(_bindingName == "hasentry"))
						{
							return false;
						}
						_value = true.ToString();
						return true;
					}
					else
					{
						if (!(_bindingName == "worldname"))
						{
							return false;
						}
						_value = this.worldKey;
						return true;
					}
				}
				else
				{
					if (!(_bindingName == "version"))
					{
						return false;
					}
					_value = ((this.version == null) ? string.Empty : ((this.version.Major >= 0) ? this.version.LongStringNoBuild : Constants.cVersionInformation.LongStringNoBuild));
					return true;
				}
			}
			else if (num <= 1823525230U)
			{
				if (num != 1800901934U)
				{
					if (num != 1823525230U)
					{
						return false;
					}
					if (!(_bindingName == "lastplayedinfo"))
					{
						return false;
					}
					if (this.saveEntryInfo.SizeInfo.IsArchived)
					{
						_value = "[fabc02ff]" + Localization.Get("xuiDmArchivedLabel", false) + "[-]";
					}
					else
					{
						int num2 = (int)(DateTime.Now - this.lastSaved).TotalDays;
						_value = string.Format("[ffffff88]{0} {1}[-]", num2, Localization.Get("xuiDmDaysAgo", false));
					}
					return true;
				}
				else
				{
					if (!(_bindingName == "lastplayed"))
					{
						return false;
					}
					_value = this.lastSaved.ToString("yyyy-MM-dd HH:mm");
					return true;
				}
			}
			else if (num != 2049496678U)
			{
				if (num != 2595142937U)
				{
					if (num != 4086844294U)
					{
						return false;
					}
					if (!(_bindingName == "versiontooltip"))
					{
						return false;
					}
					_value = ((this.versionComparison == VersionInformation.EVersionComparisonResult.SameBuild || this.versionComparison == VersionInformation.EVersionComparisonResult.SameMinor) ? "" : ((this.versionComparison == VersionInformation.EVersionComparisonResult.NewerMinor) ? Localization.Get("xuiSavegameNewerMinor", false) : ((this.versionComparison == VersionInformation.EVersionComparisonResult.OlderMinor) ? Localization.Get("xuiSavegameOlderMinor", false) : Localization.Get("xuiSavegameDifferentMajor", false))));
					return true;
				}
				else
				{
					if (!(_bindingName == "savesize"))
					{
						return false;
					}
					string text = this.saveEntryInfo.SizeInfo.IsArchived ? "fabc02ff" : "ffffffbb";
					_value = string.Concat(new string[]
					{
						"[",
						text,
						"]",
						XUiC_DataManagement.FormatMemoryString(this.saveEntryInfo.SizeInfo.ReportedSize),
						"[-]"
					});
					return true;
				}
			}
			else if (!(_bindingName == "versioncolor"))
			{
				return false;
			}
			_value = ((this.versionComparison == VersionInformation.EVersionComparisonResult.SameBuild || this.versionComparison == VersionInformation.EVersionComparisonResult.SameMinor) ? this.matchingColor : ((this.versionComparison == VersionInformation.EVersionComparisonResult.NewerMinor || this.versionComparison == VersionInformation.EVersionComparisonResult.OlderMinor) ? this.compatibleColor : this.incompatibleColor));
			return true;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.saveName.ContainsCaseInsensitive(_searchString);
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
			if (num <= 1566407741U)
			{
				if (num <= 719397874U)
				{
					if (num != 8204530U)
					{
						if (num != 205488363U)
						{
							if (num != 719397874U)
							{
								return false;
							}
							if (!(_bindingName == "worldcolor"))
							{
								return false;
							}
							goto IL_186;
						}
						else if (!(_bindingName == "savename"))
						{
							return false;
						}
					}
					else
					{
						if (!(_bindingName == "entrycolor"))
						{
							return false;
						}
						goto IL_186;
					}
				}
				else if (num != 1181855383U)
				{
					if (num != 1355330520U)
					{
						if (num != 1566407741U)
						{
							return false;
						}
						if (!(_bindingName == "hasentry"))
						{
							return false;
						}
						_value = false.ToString();
						return true;
					}
					else if (!(_bindingName == "worldname"))
					{
						return false;
					}
				}
				else if (!(_bindingName == "version"))
				{
					return false;
				}
			}
			else if (num <= 1871248802U)
			{
				if (num != 1800901934U)
				{
					if (num != 1823525230U)
					{
						if (num != 1871248802U)
						{
							return false;
						}
						if (!(_bindingName == "worldtooltip"))
						{
							return false;
						}
					}
					else if (!(_bindingName == "lastplayedinfo"))
					{
						return false;
					}
				}
				else if (!(_bindingName == "lastplayed"))
				{
					return false;
				}
			}
			else if (num != 2049496678U)
			{
				if (num != 2595142937U)
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
				else if (!(_bindingName == "savesize"))
				{
					return false;
				}
			}
			else
			{
				if (!(_bindingName == "versioncolor"))
				{
					return false;
				}
				goto IL_186;
			}
			_value = "";
			return true;
			IL_186:
			_value = "0,0,0";
			return true;
		}

		public readonly string saveName;

		public readonly string worldKey;

		public readonly string saveDirectory;

		public readonly DateTime lastSaved;

		public readonly VersionInformation version;

		public readonly VersionInformation.EVersionComparisonResult versionComparison;

		public readonly SaveInfoProvider.SaveEntryInfo saveEntryInfo;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string matchingColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string compatibleColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string incompatibleColor;
	}
}
