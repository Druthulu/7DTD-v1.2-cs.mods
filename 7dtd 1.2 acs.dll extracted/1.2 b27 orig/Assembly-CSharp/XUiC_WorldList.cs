using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorldList : XUiC_List<XUiC_WorldList.WorldListEntry>
{
	public event XUiEvent_OnPressEventHandler OnEntryClicked;

	public event XUiEvent_OnPressEventHandler OnEntryDoubleClicked;

	public override void Init()
	{
		base.Init();
		foreach (XUiC_ListEntry<XUiC_WorldList.WorldListEntry> xuiC_ListEntry in this.listEntryControllers)
		{
			xuiC_ListEntry.OnPress += this.EntryClicked;
			xuiC_ListEntry.OnDoubleClick += this.EntryDoubleClicked;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList(false);
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (PathAbstractions.AbstractedLocation abstractedLocation in PathAbstractions.WorldsSearchPaths.GetAvailablePathsList(null, null, null, false))
		{
			if (!XUiC_WorldList.forbiddenWorlds.ContainsWithComparer(abstractedLocation.Name, StringComparer.OrdinalIgnoreCase))
			{
				GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(abstractedLocation);
				if (worldInfo != null)
				{
					this.allEntries.Add(new XUiC_WorldList.WorldListEntry(abstractedLocation, GameIO.IsWorldGenerated(abstractedLocation.Name), worldInfo.GameVersionCreated, this.matchingVersionColor, this.compatibleVersionColor, this.incompatibleVersionColor));
				}
			}
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public bool SelectByName(string _name)
	{
		if (string.IsNullOrEmpty(_name))
		{
			return false;
		}
		for (int i = 0; i < this.filteredEntries.Count; i++)
		{
			if (this.filteredEntries[i].Location.Name.Equals(_name, StringComparison.OrdinalIgnoreCase))
			{
				base.SelectedEntryIndex = i;
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntryClicked(XUiController _sender, int _mouseButton)
	{
		XUiEvent_OnPressEventHandler onEntryClicked = this.OnEntryClicked;
		if (onEntryClicked == null)
		{
			return;
		}
		onEntryClicked(_sender, _mouseButton);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntryDoubleClicked(XUiController _sender, int _mouseButton)
	{
		XUiEvent_OnPressEventHandler onEntryDoubleClicked = this.OnEntryDoubleClicked;
		if (onEntryDoubleClicked == null)
		{
			return;
		}
		onEntryDoubleClicked(_sender, _mouseButton);
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
	public static readonly List<string> forbiddenWorlds = new List<string>
	{
		"Empty",
		"Playtesting"
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public string matchingVersionColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string compatibleVersionColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string incompatibleVersionColor;

	[Preserve]
	public class WorldListEntry : XUiListEntry
	{
		public WorldListEntry(PathAbstractions.AbstractedLocation _location, bool _generatedWorld, VersionInformation _version, string matchingColor = "255,255,255", string compatibleColor = "255,255,255", string incompatibleColor = "255,255,255")
		{
			this.Location = _location;
			this.GeneratedWorld = _generatedWorld;
			this.Version = _version;
			this.versionComparison = this.Version.CompareToRunningBuild();
			this.matchingColor = matchingColor;
			this.compatibleColor = compatibleColor;
			this.incompatibleColor = incompatibleColor;
		}

		public override int CompareTo(object _otherEntry)
		{
			XUiC_WorldList.WorldListEntry worldListEntry = _otherEntry as XUiC_WorldList.WorldListEntry;
			if (worldListEntry == null)
			{
				return 1;
			}
			return string.Compare(this.Location.Name, worldListEntry.Location.Name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				string str = "";
				if (this.Location.Type == PathAbstractions.EAbstractedLocationType.Mods)
				{
					str = " (Mod: " + this.Location.ContainingMod.Name + ")";
				}
				else if (this.Location.Type != PathAbstractions.EAbstractedLocationType.GameData)
				{
					str = " (from " + this.Location.Type.ToStringCached<PathAbstractions.EAbstractedLocationType>() + ")";
				}
				_value = this.Location.Name + str;
				return true;
			}
			if (!(_bindingName == "entrycolor"))
			{
				return false;
			}
			if (!this.GeneratedWorld)
			{
				_value = this.matchingColor;
				return true;
			}
			_value = ((this.versionComparison == VersionInformation.EVersionComparisonResult.SameBuild || this.versionComparison == VersionInformation.EVersionComparisonResult.SameMinor) ? this.matchingColor : ((this.versionComparison == VersionInformation.EVersionComparisonResult.NewerMinor || this.versionComparison == VersionInformation.EVersionComparisonResult.OlderMinor) ? this.compatibleColor : this.incompatibleColor));
			return true;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.Location.Name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = string.Empty;
				return true;
			}
			if (!(_bindingName == "entrycolor"))
			{
				return false;
			}
			_value = "0,0,0";
			return true;
		}

		public readonly PathAbstractions.AbstractedLocation Location;

		public readonly bool GeneratedWorld;

		public readonly VersionInformation Version;

		public readonly VersionInformation.EVersionComparisonResult versionComparison;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string matchingColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string compatibleColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string incompatibleColor;
	}
}
