using System;
using System.Collections.ObjectModel;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BugReportSavesList : XUiC_List<XUiC_BugReportSavesList.ListEntry>
{
	public override void RebuildList(bool _resetFilter = false)
	{
	}

	public void RebuildList(ReadOnlyCollection<SaveInfoProvider.SaveEntryInfo> saveEntryInfos, bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (SaveInfoProvider.SaveEntryInfo saveEntryInfo in saveEntryInfos)
		{
			this.allEntries.Add(new XUiC_BugReportSavesList.ListEntry(saveEntryInfo));
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	[Preserve]
	public class ListEntry : XUiListEntry
	{
		public ListEntry(SaveInfoProvider.SaveEntryInfo saveEntryInfo)
		{
			this.saveEntryInfo = saveEntryInfo;
			this.saveName = saveEntryInfo.Name;
			this.worldKey = saveEntryInfo.WorldEntry.WorldKey;
			this.saveDirectory = saveEntryInfo.SaveDir;
			this.lastSaved = saveEntryInfo.LastSaved;
		}

		public override int CompareTo(object _otherEntry)
		{
			XUiC_BugReportSavesList.ListEntry listEntry = _otherEntry as XUiC_BugReportSavesList.ListEntry;
			if (listEntry != null)
			{
				return this.saveEntryInfo.CompareTo(listEntry.saveEntryInfo);
			}
			return 1;
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (_bindingName == "savename")
			{
				_value = this.saveName;
				return true;
			}
			if (_bindingName == "worldname")
			{
				_value = this.saveEntryInfo.WorldEntry.Name;
				return true;
			}
			if (_bindingName == "lastplayed")
			{
				_value = this.lastSaved.ToString("yyyy-MM-dd HH:mm");
				return true;
			}
			if (!(_bindingName == "hasentry"))
			{
				return false;
			}
			_value = true.ToString();
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

		public readonly SaveInfoProvider.SaveEntryInfo saveEntryInfo;
	}
}
