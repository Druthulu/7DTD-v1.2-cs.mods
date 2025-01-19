using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabFileList : XUiC_List<XUiC_PrefabFileList.PrefabFileEntry>
{
	public event XUiC_PrefabFileList.EntryDoubleClickedDelegate OnEntryDoubleClicked;

	public override void Init()
	{
		base.Init();
		XUiC_ListEntry<XUiC_PrefabFileList.PrefabFileEntry>[] listEntryControllers = this.listEntryControllers;
		for (int i = 0; i < listEntryControllers.Length; i++)
		{
			listEntryControllers[i].OnDoubleClick += this.EntryDoubleClicked;
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		this.prefabSearchList.Clear();
		GameManager.Instance.prefabEditModeManager.FindPrefabs(this.groupFilter, this.prefabSearchList);
		foreach (PathAbstractions.AbstractedLocation location in this.prefabSearchList)
		{
			this.allEntries.Add(new XUiC_PrefabFileList.PrefabFileEntry(location));
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public void SetGroupFilter(string _filter)
	{
		this.groupFilter = _filter;
		this.RebuildList(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntryDoubleClicked(XUiController _sender, int _mouseButton)
	{
		if (this.OnEntryDoubleClicked != null)
		{
			this.OnEntryDoubleClicked(((XUiC_ListEntry<XUiC_PrefabFileList.PrefabFileEntry>)_sender).GetEntry());
		}
	}

	public bool SelectByName(string _name)
	{
		if (!string.IsNullOrEmpty(_name))
		{
			for (int i = 0; i < this.filteredEntries.Count; i++)
			{
				if (this.filteredEntries[i].location.Name.EqualsCaseInsensitive(_name))
				{
					base.SelectedEntryIndex = i;
					return true;
				}
			}
		}
		return false;
	}

	public bool SelectByLocation(PathAbstractions.AbstractedLocation _location)
	{
		if (_location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			return false;
		}
		for (int i = 0; i < this.filteredEntries.Count; i++)
		{
			if (this.filteredEntries[i].location == _location)
			{
				base.SelectedEntryIndex = i;
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string groupFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PathAbstractions.AbstractedLocation> prefabSearchList = new List<PathAbstractions.AbstractedLocation>();

	public delegate void EntryDoubleClickedDelegate(XUiC_PrefabFileList.PrefabFileEntry _entry);

	public class PrefabFileEntry : XUiListEntry
	{
		public PrefabFileEntry(PathAbstractions.AbstractedLocation _location)
		{
			this.location = _location;
		}

		public override int CompareTo(object _otherEntry)
		{
			XUiC_PrefabFileList.PrefabFileEntry prefabFileEntry = _otherEntry as XUiC_PrefabFileList.PrefabFileEntry;
			if (prefabFileEntry == null)
			{
				return 1;
			}
			return this.location.CompareTo(prefabFileEntry.location);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				string str = "";
				if (this.location.Type == PathAbstractions.EAbstractedLocationType.Mods)
				{
					str = " (Mod: " + this.location.ContainingMod.Name + ")";
				}
				else if (this.location.Type != PathAbstractions.EAbstractedLocationType.GameData)
				{
					str = " (from " + this.location.Type.ToStringCached<PathAbstractions.EAbstractedLocationType>() + ")";
				}
				_value = this.location.Name + str;
				return true;
			}
			if (!(_bindingName == "localizedname"))
			{
				return false;
			}
			_value = Localization.Get(this.location.Name, false);
			return true;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.location.Name.ContainsCaseInsensitive(_searchString);
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = string.Empty;
				return true;
			}
			if (!(_bindingName == "localizedname"))
			{
				return false;
			}
			_value = string.Empty;
			return true;
		}

		public readonly PathAbstractions.AbstractedLocation location;
	}
}
