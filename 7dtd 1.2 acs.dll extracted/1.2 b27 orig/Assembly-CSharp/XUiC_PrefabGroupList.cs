using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabGroupList : XUiC_List<XUiC_PrefabGroupList.PrefabGroupEntry>
{
	public override void OnOpen()
	{
		base.OnOpen();
		bool flag = false;
		this.groupsResult.Clear();
		GameManager.Instance.prefabEditModeManager.GetAllGroups(this.groupsResult, null);
		foreach (string text in this.groupsResult)
		{
			bool flag2 = false;
			using (List<XUiC_PrefabGroupList.PrefabGroupEntry>.Enumerator enumerator2 = this.allEntries.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					if (enumerator2.Current.name == text)
					{
						flag2 = true;
						break;
					}
				}
			}
			if (!flag2)
			{
				this.allEntries.Add(new XUiC_PrefabGroupList.PrefabGroupEntry(text, text));
				flag = true;
			}
		}
		if (flag)
		{
			this.allEntries.Sort();
			this.RefreshView(false, true);
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		this.groupsResult.Clear();
		GameManager.Instance.prefabEditModeManager.GetAllGroups(this.groupsResult, null);
		foreach (string text in this.groupsResult)
		{
			this.allEntries.Add(new XUiC_PrefabGroupList.PrefabGroupEntry(text, text));
		}
		this.allEntries.Sort();
		this.allEntries.Insert(0, new XUiC_PrefabGroupList.PrefabGroupEntry("<All>", null));
		this.allEntries.Insert(1, new XUiC_PrefabGroupList.PrefabGroupEntry("<Ungrouped>", ""));
		base.RebuildList(_resetFilter);
	}

	public bool SelectByName(string _name)
	{
		if (!string.IsNullOrEmpty(_name))
		{
			for (int i = 0; i < this.filteredEntries.Count; i++)
			{
				if (this.filteredEntries[i].name.Equals(_name, StringComparison.OrdinalIgnoreCase))
				{
					base.SelectedEntryIndex = i;
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> groupsResult = new List<string>();

	[Preserve]
	public class PrefabGroupEntry : XUiListEntry
	{
		public PrefabGroupEntry(string _name, string _filterString)
		{
			this.name = _name;
			this.filterString = _filterString;
		}

		public override int CompareTo(object _otherEntry)
		{
			if (!(_otherEntry is XUiC_PrefabGroupList.PrefabGroupEntry))
			{
				return 1;
			}
			return string.Compare(this.name, ((XUiC_PrefabGroupList.PrefabGroupEntry)_otherEntry).name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = this.name;
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

		public readonly string filterString;
	}
}
