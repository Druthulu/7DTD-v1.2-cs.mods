using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SavegamesList : XUiC_List<XUiC_SavegamesList.ListEntry>
{
	public event XUiEvent_OnPressEventHandler OnEntryDoubleClicked;

	public override void Init()
	{
		base.Init();
		XUiC_ListEntry<XUiC_SavegamesList.ListEntry>[] listEntryControllers = this.listEntryControllers;
		for (int i = 0; i < listEntryControllers.Length; i++)
		{
			XUiC_ListEntry<XUiC_SavegamesList.ListEntry> xuiC_ListEntry = listEntryControllers[i];
			xuiC_ListEntry.OnDoubleClick += delegate(XUiController _sender, int _mouseButton)
			{
				this.EntryDoubleClicked(_sender, _mouseButton, _sender.ViewComponent);
			};
			XUiC_ListEntry<XUiC_SavegamesList.ListEntry> closure = xuiC_ListEntry;
			XUiEvent_OnHoverEventHandler value = delegate(XUiController _controller, bool _isOver)
			{
				closure.ForceHovered = _isOver;
			};
			xuiC_ListEntry.GetChildById("Version").OnScroll += base.HandleOnScroll;
			xuiC_ListEntry.GetChildById("Version").OnPress += xuiC_ListEntry.XUiC_ListEntry_OnPress;
			xuiC_ListEntry.GetChildById("Version").OnDoubleClick += delegate(XUiController _sender, int _args)
			{
				this.EntryDoubleClicked(_sender, _args, _sender.Parent.ViewComponent);
			};
			xuiC_ListEntry.GetChildById("Version").OnHover += value;
			xuiC_ListEntry.GetChildById("Version").ViewComponent.IsSnappable = false;
			xuiC_ListEntry.GetChildById("World").OnScroll += base.HandleOnScroll;
			xuiC_ListEntry.GetChildById("World").OnPress += xuiC_ListEntry.XUiC_ListEntry_OnPress;
			xuiC_ListEntry.GetChildById("World").OnDoubleClick += delegate(XUiController _sender, int _args)
			{
				this.EntryDoubleClicked(_sender, _args, _sender.Parent.ViewComponent);
			};
			xuiC_ListEntry.GetChildById("World").OnHover += value;
			xuiC_ListEntry.GetChildById("World").ViewComponent.IsSnappable = false;
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
		GameIO.GetPlayerSaves(new GameIO.FoundSave(this.AddSaveToEntries), false);
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
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

	public void SetWorldFilter(string _worldName)
	{
		this.worldFilter = _worldName;
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
			if (this.filteredEntries[i].worldName != this.worldFilter)
			{
				this.filteredEntries.RemoveAt(i);
				i--;
			}
		}
	}

	public IEnumerable<XUiC_SavegamesList.ListEntry> GetSavesInWorld(string _worldName)
	{
		if (string.IsNullOrEmpty(_worldName))
		{
			yield break;
		}
		int num;
		for (int i = 0; i < this.allEntries.Count; i = num + 1)
		{
			if (this.allEntries[i].worldName == _worldName)
			{
				yield return this.allEntries[i];
			}
			num = i;
		}
		yield break;
	}

	public void SelectEntry(string worldName, string saveName)
	{
		if (this.filteredEntries == null)
		{
			Log.Error("filteredEntries is null");
			return;
		}
		for (int i = 0; i < this.filteredEntries.Count; i++)
		{
			XUiC_SavegamesList.ListEntry listEntry = this.filteredEntries[i];
			if (listEntry.worldName.EqualsCaseInsensitive(worldName) && listEntry.saveName.EqualsCaseInsensitive(saveName))
			{
				base.SelectedEntryIndex = i;
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddSaveToEntries(string saveName, string worldName, DateTime lastSaved, WorldState worldState, bool isArchived)
	{
		this.allEntries.Add(new XUiC_SavegamesList.ListEntry(saveName, worldName, lastSaved, worldState, this.matchingVersionColor, this.compatibleVersionColor, this.incompatibleVersionColor));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntryDoubleClicked(XUiController _sender, int _mouseButton, XUiView _listEntryView)
	{
		if (!_listEntryView.Enabled)
		{
			return;
		}
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
	public string matchingVersionColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string compatibleVersionColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string incompatibleVersionColor;

	public string worldFilter;

	[Preserve]
	public class ListEntry : XUiListEntry
	{
		public GameMode gameMode
		{
			get
			{
				return GameMode.GetGameModeForId(this.worldState.activeGameMode);
			}
		}

		public ListEntry(string _saveName, string _worldName, DateTime _lastSaved, WorldState _worldState, string matchingColor = "255,255,255", string compatibleColor = "255,255,255", string incompatibleColor = "255,255,255")
		{
			this.saveName = _saveName;
			this.worldName = _worldName;
			this.lastSaved = _lastSaved;
			this.worldState = _worldState;
			this.AbstractedLocation = PathAbstractions.WorldsSearchPaths.GetLocation(this.worldName, _worldName, _saveName);
			this.versionComparison = this.worldState.gameVersion.CompareToRunningBuild();
			this.matchingColor = matchingColor;
			this.compatibleColor = compatibleColor;
			this.incompatibleColor = incompatibleColor;
		}

		public override int CompareTo(object _otherEntry)
		{
			if (!(_otherEntry is XUiC_SavegamesList.ListEntry))
			{
				return 1;
			}
			return -1 * this.lastSaved.CompareTo(((XUiC_SavegamesList.ListEntry)_otherEntry).lastSaved);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
			if (num <= 1355330520U)
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
				else if (num != 719397874U)
				{
					if (num != 1181855383U)
					{
						if (num != 1355330520U)
						{
							return false;
						}
						if (!(_bindingName == "worldname"))
						{
							return false;
						}
						_value = this.worldName;
						return true;
					}
					else
					{
						if (!(_bindingName == "version"))
						{
							return false;
						}
						if (this.worldState.gameVersion.Major >= 0)
						{
							_value = this.worldState.gameVersion.ShortString;
						}
						else
						{
							_value = this.worldState.gameVersionString;
						}
						return true;
					}
				}
				else
				{
					if (!(_bindingName == "worldcolor"))
					{
						return false;
					}
					_value = ((this.AbstractedLocation.Type != PathAbstractions.EAbstractedLocationType.None) ? "255,255,255,128" : "255,0,0");
					return true;
				}
			}
			else if (num <= 1871248802U)
			{
				if (num != 1566407741U)
				{
					if (num != 1800901934U)
					{
						if (num != 1871248802U)
						{
							return false;
						}
						if (!(_bindingName == "worldtooltip"))
						{
							return false;
						}
						_value = ((this.AbstractedLocation.Type != PathAbstractions.EAbstractedLocationType.None) ? "" : Localization.Get("xuiSavegameWorldNotFound", false));
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
				else
				{
					if (!(_bindingName == "hasentry"))
					{
						return false;
					}
					_value = true.ToString();
					return true;
				}
			}
			else if (num != 2049496678U)
			{
				if (num != 3966689298U)
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
					if (!(_bindingName == "mode"))
					{
						return false;
					}
					GameMode gameMode = this.gameMode;
					if (gameMode == null)
					{
						_value = "-Unknown-";
					}
					else
					{
						string name = gameMode.GetName();
						_value = Localization.Get(name, false);
					}
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
			if (num <= 1355330520U)
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
					}
					else
					{
						if (!(_bindingName == "entrycolor"))
						{
							return false;
						}
						goto IL_160;
					}
				}
				else if (num != 719397874U)
				{
					if (num != 1181855383U)
					{
						if (num != 1355330520U)
						{
							return false;
						}
						if (!(_bindingName == "worldname"))
						{
							return false;
						}
					}
					else if (!(_bindingName == "version"))
					{
						return false;
					}
				}
				else
				{
					if (!(_bindingName == "worldcolor"))
					{
						return false;
					}
					goto IL_160;
				}
			}
			else if (num <= 1871248802U)
			{
				if (num != 1566407741U)
				{
					if (num != 1800901934U)
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
					else if (!(_bindingName == "lastplayed"))
					{
						return false;
					}
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
			else if (num != 2049496678U)
			{
				if (num != 3966689298U)
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
				else if (!(_bindingName == "mode"))
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
				goto IL_160;
			}
			_value = "";
			return true;
			IL_160:
			_value = "0,0,0";
			return true;
		}

		public readonly string saveName;

		public readonly string worldName;

		public readonly DateTime lastSaved;

		public readonly WorldState worldState;

		public readonly PathAbstractions.AbstractedLocation AbstractedLocation;

		public readonly VersionInformation.EVersionComparisonResult versionComparison;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string matchingColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string compatibleColor;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string incompatibleColor;
	}
}
