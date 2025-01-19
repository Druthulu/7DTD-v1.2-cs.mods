using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_GameEventsList : XUiC_List<XUiC_GameEventsList.GameEventEntry>
{
	public string Category
	{
		get
		{
			return this.category;
		}
		set
		{
			this.category = value;
			this.RebuildList(false);
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (KeyValuePair<string, GameEventActionSequence> keyValuePair in GameEventManager.GameEventSequences)
		{
			if (keyValuePair.Value.AllowUserTrigger && (this.category == "" || (keyValuePair.Value.CategoryNames != null && keyValuePair.Value.CategoryNames.ContainsCaseInsensitive(this.category))))
			{
				this.allEntries.Add(new XUiC_GameEventsList.GameEventEntry(keyValuePair.Key));
			}
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.allEntries.Count == 0)
		{
			this.RebuildList(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string category = "";

	[Preserve]
	public class GameEventEntry : XUiListEntry
	{
		public GameEventEntry(string _name)
		{
			this.name = _name;
		}

		public override int CompareTo(object _otherEntry)
		{
			if (!(_otherEntry is XUiC_GameEventsList.GameEventEntry))
			{
				return 1;
			}
			return string.Compare(this.name, ((XUiC_GameEventsList.GameEventEntry)_otherEntry).name, StringComparison.OrdinalIgnoreCase);
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

		public readonly string camelCase;
	}
}
