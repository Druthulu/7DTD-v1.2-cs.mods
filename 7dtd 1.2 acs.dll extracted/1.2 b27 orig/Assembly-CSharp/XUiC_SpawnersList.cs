using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnersList : XUiC_List<XUiC_SpawnersList.SpawnerEntry>
{
	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (KeyValuePair<string, GameStageGroup> keyValuePair in GameStageGroup.Groups)
		{
			string key = keyValuePair.Key;
			this.allEntries.Add(new XUiC_SpawnersList.SpawnerEntry(key));
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	[Preserve]
	public class SpawnerEntry : XUiListEntry
	{
		public SpawnerEntry(string _name)
		{
			this.name = _name;
			this.displayName = GameStageGroup.MakeDisplayName(_name);
		}

		public override int CompareTo(object _otherEntry)
		{
			if (!(_otherEntry is XUiC_SpawnersList.SpawnerEntry))
			{
				return 1;
			}
			return string.Compare(this.name, ((XUiC_SpawnersList.SpawnerEntry)_otherEntry).name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = this.displayName;
				return true;
			}
			return false;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = string.Empty;
				return true;
			}
			return false;
		}

		public readonly string name;

		public readonly string displayName;
	}
}
