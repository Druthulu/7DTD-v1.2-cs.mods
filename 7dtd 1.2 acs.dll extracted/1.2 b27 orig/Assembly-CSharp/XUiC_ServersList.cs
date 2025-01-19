using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServersList : XUiC_List<XUiC_ServersList.ListEntry>
{
	public event XUiEvent_OnPressEventHandler OnEntryDoubleClicked;

	public event Action<int> OnFilterResultsChanged;

	public event Action CountsChanged;

	public XUiC_ServersList.EnumServerLists CurrentServerTypeList
	{
		get
		{
			return this.currentServerTypeList;
		}
	}

	public override void Init()
	{
		base.Init();
		this.sortButtons = new XUiController[5];
		for (int i = 0; i < 5; i++)
		{
			this.sortButtons[i] = base.GetChildById("serverlistheader").GetChildById(((XUiC_ServersList.EnumColumns)i).ToStringCached<XUiC_ServersList.EnumColumns>());
			if (this.sortButtons[i] != null)
			{
				this.sortButtons[i].ViewComponent.Value = i.ToString();
				this.sortButtons[i].OnPress += this.SortButton_OnPress;
				this.sortButtons[i].OnHover += this.SortButton_OnHover;
			}
		}
		for (int j = 0; j < this.listEntryControllers.Length; j++)
		{
			XUiC_ListEntry<XUiC_ServersList.ListEntry> xuiC_ListEntry = this.listEntryControllers[j];
			xuiC_ListEntry.OnDoubleClick += this.EntryDoubleClicked;
			XUiV_Button xuiV_Button = (XUiV_Button)xuiC_ListEntry.GetChildById("favorite").ViewComponent;
			xuiV_Button.Value = j.ToString();
			xuiV_Button.Controller.OnPress += delegate(XUiController _sender, int _args)
			{
				int num = StringParsers.ParseSInt32(_sender.ViewComponent.Value, 0, -1, NumberStyles.Integer);
				XUiC_ListEntry<XUiC_ServersList.ListEntry> xuiC_ListEntry2 = this.listEntryControllers[num];
				GameServerInfo gameServerInfo = xuiC_ListEntry2.GetEntry().gameServerInfo;
				this.AddRemoveServerCount(gameServerInfo, false);
				ServerInfoCache.Instance.ToggleFavorite(gameServerInfo);
				this.AddRemoveServerCount(gameServerInfo, true);
				xuiC_ListEntry2.IsDirty = true;
			};
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList(false);
		this.stopListUpdateThread = false;
		ThreadManager.StartThread("ServerBrowserListUpdater", null, new ThreadManager.ThreadFunctionLoopDelegate(this.currentListUpdateThread), null, System.Threading.ThreadPriority.BelowNormal, null, null, false, false);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.stopListUpdateThread = true;
	}

	public override void Update(float _dt)
	{
		bool flag = false;
		if (this.pageUpdateRequired && (double)(Time.realtimeSinceStartup - this.lastPageUpdate) > 0.1)
		{
			flag = true;
			this.pageUpdateRequired = false;
			this.lastPageUpdate = Time.realtimeSinceStartup;
			this.IsDirty = true;
		}
		base.Update(_dt);
		if (flag)
		{
			Action<int> onFilterResultsChanged = this.OnFilterResultsChanged;
			if (onFilterResultsChanged == null)
			{
				return;
			}
			onFilterResultsChanged(base.EntryCount);
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		Dictionary<XUiC_ServersList.EnumServerLists, int> obj = this.serverCounts;
		lock (obj)
		{
			this.serverCounts.Clear();
		}
		base.RebuildList(_resetFilter);
	}

	public void RefreshBindingListEntry(XUiC_ServersList.ListEntry entry)
	{
		foreach (XUiC_ListEntry<XUiC_ServersList.ListEntry> xuiC_ListEntry in this.listEntryControllers)
		{
			if (xuiC_ListEntry.GetEntry() == entry)
			{
				xuiC_ListEntry.RefreshBindings(false);
				return;
			}
		}
	}

	public override void RefreshView(bool _resetFilter = false, bool _resetPage = true)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnSearchInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!string.IsNullOrEmpty(_text))
		{
			this.SetFilter(new XUiC_ServersList.UiServerFilter("servername", XUiC_ServersList.EnumServerLists.All, (XUiC_ServersList.ListEntry _entry) => _entry.MatchesSearch(_text), IServerListInterface.ServerFilter.EServerFilterType.Any, 0, 0, false, null));
		}
		else
		{
			this.SetFilter(new XUiC_ServersList.UiServerFilter("servername", XUiC_ServersList.EnumServerLists.All, null, IServerListInterface.ServerFilter.EServerFilterType.Any, 0, 0, false, null));
		}
		this.updateCurrentList = true;
		this.IsDirty = true;
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
		if (_name == "icon_enabled_color")
		{
			XUiC_ServersList.iconEnabledColor = _value;
			return true;
		}
		if (!(_name == "icon_disabled_color"))
		{
			return base.ParseAttribute(_name, _value, _parent);
		}
		XUiC_ServersList.iconDisabledColor = _value;
		return true;
	}

	public void SetServerTypeFilter(XUiC_ServersList.EnumServerLists _typelist)
	{
		Func<XUiC_ServersList.ListEntry, bool> func;
		switch (_typelist)
		{
		case XUiC_ServersList.EnumServerLists.Dedicated:
			func = ((XUiC_ServersList.ListEntry _line) => _line.gameServerInfo.IsDedicated);
			goto IL_D8;
		case XUiC_ServersList.EnumServerLists.Peer:
			func = ((XUiC_ServersList.ListEntry _line) => _line.gameServerInfo.IsPeerToPeer);
			goto IL_D8;
		case XUiC_ServersList.EnumServerLists.Regular:
			break;
		case XUiC_ServersList.EnumServerLists.Friends:
			func = ((XUiC_ServersList.ListEntry _line) => _line.gameServerInfo.IsFriends);
			goto IL_D8;
		default:
			if (_typelist == XUiC_ServersList.EnumServerLists.History)
			{
				func = ((XUiC_ServersList.ListEntry _line) => _line.gameServerInfo.IsFavoriteHistory);
				goto IL_D8;
			}
			if (_typelist == XUiC_ServersList.EnumServerLists.LAN)
			{
				func = ((XUiC_ServersList.ListEntry _line) => _line.gameServerInfo.IsLAN);
				goto IL_D8;
			}
			break;
		}
		func = null;
		IL_D8:
		Func<XUiC_ServersList.ListEntry, bool> func2 = func;
		this.currentServerTypeList = _typelist;
		this.SetFilter(new XUiC_ServersList.UiServerFilter("servertype", XUiC_ServersList.EnumServerLists.All, func2, IServerListInterface.ServerFilter.EServerFilterType.Any, 0, 0, false, null));
		base.ClearSelection();
	}

	public void SetFilter(XUiC_ServersList.UiServerFilter _filter)
	{
		Dictionary<string, XUiC_ServersList.UiServerFilter> obj = this.currentFilters;
		lock (obj)
		{
			if (_filter.Func == null)
			{
				this.currentFilters.Remove(_filter.Name);
			}
			else
			{
				this.currentFilters[_filter.Name] = _filter;
			}
		}
		this.updateCurrentList = true;
	}

	public int GetActiveFilterCount()
	{
		Dictionary<string, XUiC_ServersList.UiServerFilter> obj = this.currentFilters;
		int result;
		lock (obj)
		{
			result = (this.currentFilters.ContainsKey("servertype") ? (this.currentFilters.Count - 1) : this.currentFilters.Count);
		}
		return result;
	}

	public int GetServerCount(XUiC_ServersList.EnumServerLists _list)
	{
		Dictionary<XUiC_ServersList.EnumServerLists, int> obj = this.serverCounts;
		int result;
		lock (obj)
		{
			this.serverCounts.TryGetValue(_list, out result);
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateListFiltering(ref IEnumerable<XUiC_ServersList.ListEntry> _list)
	{
		Dictionary<string, XUiC_ServersList.UiServerFilter> obj = this.currentFilters;
		lock (obj)
		{
			foreach (KeyValuePair<string, XUiC_ServersList.UiServerFilter> keyValuePair in this.currentFilters)
			{
				if ((this.currentServerTypeList & keyValuePair.Value.ApplyingLists) != XUiC_ServersList.EnumServerLists.None)
				{
					_list = _list.Where(keyValuePair.Value.Func);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateListSorting(ref IEnumerable<XUiC_ServersList.ListEntry> _list)
	{
		if (this.currentServerTypeList == XUiC_ServersList.EnumServerLists.History && this.sortFuncInt == null && this.sortFuncString == null)
		{
			_list = _list.OrderByDescending(this.sortDefaultFavHistory);
		}
		if (this.sortFuncInt != null)
		{
			_list = (this.sortAscending ? _list.OrderBy(this.sortFuncInt) : _list.OrderByDescending(this.sortFuncInt));
		}
		if (this.sortFuncString != null)
		{
			_list = (this.sortAscending ? _list.OrderBy(this.sortFuncString) : _list.OrderByDescending(this.sortFuncString));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int indexOfGameServerInfo(GameServerInfo _gameServerInfo)
	{
		if (_gameServerInfo == null)
		{
			return -1;
		}
		int hashCode = this.uniqueIdComparer.GetHashCode(_gameServerInfo);
		int count = this.allEntries.Count;
		for (int i = 0; i < count; i++)
		{
			if (this.uniqueIdComparer.GetHashCode(this.allEntries[i].gameServerInfo) == hashCode && this.uniqueIdComparer.Equals(_gameServerInfo, this.allEntries[i].gameServerInfo))
			{
				return i;
			}
		}
		return -1;
	}

	public bool AddGameServer(GameServerInfo _gameServerInfo, EServerRelationType _source)
	{
		int num = this.indexOfGameServerInfo(_gameServerInfo);
		bool result;
		if (num >= 0)
		{
			GameServerInfo gameServerInfo = this.allEntries[num].gameServerInfo;
			this.AddRemoveServerCount(gameServerInfo, false);
			gameServerInfo.Merge(_gameServerInfo, _source);
			this.AddRemoveServerCount(gameServerInfo, true);
			XUiC_ListEntry<XUiC_ServersList.ListEntry> xuiC_ListEntry = base.IsVisible(this.allEntries[num]);
			if (xuiC_ListEntry != null)
			{
				xuiC_ListEntry.IsDirty = true;
			}
			result = false;
		}
		else
		{
			List<XUiC_ServersList.ListEntry> allEntries = this.allEntries;
			lock (allEntries)
			{
				this.allEntries.Add(new XUiC_ServersList.ListEntry(_gameServerInfo, this));
			}
			this.AddRemoveServerCount(_gameServerInfo, true);
			result = true;
		}
		this.updateCurrentList = true;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddRemoveServerCount(GameServerInfo _gsi, bool _add)
	{
		if (_gsi.IsDedicated)
		{
			this.addRemoveCountSingleListType(_gsi, _add, XUiC_ServersList.EnumServerLists.Dedicated);
		}
		if (_gsi.IsPeerToPeer)
		{
			this.addRemoveCountSingleListType(_gsi, _add, XUiC_ServersList.EnumServerLists.Peer);
		}
		if (_gsi.IsFriends)
		{
			this.addRemoveCountSingleListType(_gsi, _add, XUiC_ServersList.EnumServerLists.Friends);
		}
		if (_gsi.IsFavoriteHistory)
		{
			this.addRemoveCountSingleListType(_gsi, _add, XUiC_ServersList.EnumServerLists.History);
		}
		if (_gsi.IsLAN)
		{
			this.addRemoveCountSingleListType(_gsi, _add, XUiC_ServersList.EnumServerLists.LAN);
		}
		Action countsChanged = this.CountsChanged;
		if (countsChanged == null)
		{
			return;
		}
		countsChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addRemoveCountSingleListType(GameServerInfo _gsi, bool _add, XUiC_ServersList.EnumServerLists _list)
	{
		Dictionary<XUiC_ServersList.EnumServerLists, int> obj = this.serverCounts;
		lock (obj)
		{
			int num;
			if (this.serverCounts.TryGetValue(_list, out num))
			{
				this.serverCounts[_list] = num + (_add ? 1 : -1);
			}
			else if (_add)
			{
				this.serverCounts[_list] = 1;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentListUpdateThread(ThreadManager.ThreadInfo _tInfo)
	{
		if (!this.stopListUpdateThread && !_tInfo.TerminationRequested())
		{
			if (this.updateCurrentList)
			{
				this.updateCurrentList = false;
				List<XUiC_ServersList.ListEntry> allEntries = this.allEntries;
				lock (allEntries)
				{
					this.filteredEntriesTmp.AddRange(this.allEntries);
				}
				IEnumerable<XUiC_ServersList.ListEntry> source = this.filteredEntriesTmp;
				this.updateListFiltering(ref source);
				this.updateListSorting(ref source);
				this.filteredEntriesTmp = source.ToList<XUiC_ServersList.ListEntry>();
				lock (this)
				{
					allEntries = this.filteredEntriesTmp;
					List<XUiC_ServersList.ListEntry> filteredEntries = this.filteredEntries;
					this.filteredEntries = allEntries;
					this.filteredEntriesTmp = filteredEntries;
				}
				this.pageUpdateRequired = true;
				this.filteredEntriesTmp.Clear();
			}
			return 50;
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortButton_OnHover(XUiController _sender, bool _isOver)
	{
		for (int i = 0; i < this.sortButtons.Length; i++)
		{
			if (_sender == this.sortButtons[i])
			{
				Color color = (this.sortColumn == (XUiC_ServersList.EnumColumns)i) ? new Color32(222, 206, 163, byte.MaxValue) : (_isOver ? new Color32(250, byte.MaxValue, 163, byte.MaxValue) : Color.white);
				XUiView viewComponent = _sender.ViewComponent;
				XUiV_Label xuiV_Label = viewComponent as XUiV_Label;
				if (xuiV_Label == null)
				{
					XUiV_Sprite xuiV_Sprite = viewComponent as XUiV_Sprite;
					if (xuiV_Sprite != null)
					{
						xuiV_Sprite.Color = color;
					}
				}
				else
				{
					xuiV_Label.Color = color;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortButton_OnPress(XUiController _sender, int _mouseButton)
	{
		for (int i = 0; i < this.sortButtons.Length; i++)
		{
			if (_sender == this.sortButtons[i])
			{
				this.updateSortType((XUiC_ServersList.EnumColumns)i);
				for (int j = 0; j < this.sortButtons.Length; j++)
				{
					if (this.sortButtons[j] != null)
					{
						this.SortButton_OnHover(this.sortButtons[j], i == j);
					}
				}
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSortType(XUiC_ServersList.EnumColumns _sortType)
	{
		switch (_sortType)
		{
		case XUiC_ServersList.EnumColumns.ServerName:
			this.sortFuncString = ((XUiC_ServersList.ListEntry _line) => _line.gameServerInfo.GetValue(GameInfoString.GameHost));
			this.sortFuncInt = null;
			break;
		case XUiC_ServersList.EnumColumns.World:
			this.sortFuncString = ((XUiC_ServersList.ListEntry _line) => _line.gameServerInfo.GetValue(GameInfoString.LevelName));
			this.sortFuncInt = null;
			break;
		case XUiC_ServersList.EnumColumns.Difficulty:
			this.sortFuncString = null;
			this.sortFuncInt = ((XUiC_ServersList.ListEntry _line) => _line.gameServerInfo.GetValue(GameInfoInt.GameDifficulty));
			break;
		case XUiC_ServersList.EnumColumns.Players:
			this.sortFuncString = null;
			this.sortFuncInt = ((XUiC_ServersList.ListEntry _line) => _line.gameServerInfo.GetValue(GameInfoInt.CurrentPlayers));
			break;
		case XUiC_ServersList.EnumColumns.Ping:
			this.sortFuncString = null;
			this.sortFuncInt = ((XUiC_ServersList.ListEntry _line) => _line.gameServerInfo.GetValue(GameInfoInt.Ping));
			break;
		default:
			this.sortFuncString = null;
			this.sortFuncInt = null;
			break;
		}
		if (this.sortColumn == _sortType)
		{
			if (!this.sortAscending)
			{
				this.sortColumn = XUiC_ServersList.EnumColumns.Count;
				this.sortFuncString = null;
				this.sortFuncInt = null;
				return;
			}
			this.sortAscending = false;
		}
		else
		{
			this.sortAscending = true;
		}
		this.sortColumn = _sortType;
		this.updateCurrentList = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string serverTypeFilterName = "servertype";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string serverNameFilterName = "servername";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string textEnabledColor = "255,255,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string textDisabledColor = "160,160,160";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string iconEnabledColor = "222,206,163";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string iconDisabledColor = "2,2,2,2";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool stopListUpdateThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateCurrentList;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pageUpdateRequired;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastPageUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_ServersList.ListEntry> filteredEntriesTmp = new List<XUiC_ServersList.ListEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController[] sortButtons;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool sortAscending = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<XUiC_ServersList.ListEntry, string> sortFuncString;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<XUiC_ServersList.ListEntry, int> sortFuncInt;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Func<XUiC_ServersList.ListEntry, int> sortDefaultFavHistory = delegate(XUiC_ServersList.ListEntry _line)
	{
		if (!_line.gameServerInfo.IsFavorite)
		{
			return _line.gameServerInfo.LastPlayedLinux;
		}
		return int.MaxValue;
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServersList.EnumServerLists currentServerTypeList;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, XUiC_ServersList.UiServerFilter> currentFilters = new Dictionary<string, XUiC_ServersList.UiServerFilter>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly GameServerInfo.UniqueIdEqualityComparer uniqueIdComparer = GameServerInfo.UniqueIdEqualityComparer.Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<XUiC_ServersList.EnumServerLists, int> serverCounts = new EnumDictionary<XUiC_ServersList.EnumServerLists, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServersList.EnumColumns sortColumn = XUiC_ServersList.EnumColumns.Count;

	[Preserve]
	public class ListEntry : XUiListEntry
	{
		public ListEntry(GameServerInfo _serverInfo, XUiC_ServersList _serversList)
		{
			this.gameServerInfo = _serverInfo;
			this.serversList = _serversList;
		}

		public override int CompareTo(object _otherEntry)
		{
			XUiC_ServersList.ListEntry listEntry = _otherEntry as XUiC_ServersList.ListEntry;
			if (listEntry == null)
			{
				return 1;
			}
			return this.gameServerInfo.LastPlayedLinux.CompareTo(listEntry.gameServerInfo.LastPlayedLinux);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
			if (num <= 933488787U)
			{
				if (num <= 646081446U)
				{
					if (num != 164617456U)
					{
						if (num != 375255177U)
						{
							if (num == 646081446U)
							{
								if (_bindingName == "playersonline")
								{
									_value = this.gameServerInfo.GetValue(GameInfoInt.CurrentPlayers).ToString();
									return true;
								}
							}
						}
						else if (_bindingName == "ping")
						{
							int value = this.gameServerInfo.GetValue(GameInfoInt.Ping);
							_value = ((value >= 0) ? value.ToString() : "N/A");
							return true;
						}
					}
					else if (_bindingName == "difficulty")
					{
						_value = this.gameServerInfo.GetValue(GameInfoInt.GameDifficulty).ToString();
						return true;
					}
				}
				else if (num <= 774435485U)
				{
					if (num != 724407982U)
					{
						if (num == 774435485U)
						{
							if (_bindingName == "playersmax")
							{
								_value = this.gameServerInfo.GetValue(GameInfoInt.MaxPlayers).ToString();
								return true;
							}
						}
					}
					else if (_bindingName == "servericonatlas")
					{
						_value = "SymbolAtlas";
						return true;
					}
				}
				else if (num != 796832095U)
				{
					if (num == 933488787U)
					{
						if (_bindingName == "world")
						{
							_value = GeneratedTextManager.GetDisplayTextImmediately(this.gameServerInfo.ServerWorldName, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
							if (!GeneratedTextManager.IsFiltering(this.gameServerInfo.ServerWorldName) && !GeneratedTextManager.IsFiltered(this.gameServerInfo.ServerWorldName))
							{
								GeneratedTextManager.GetDisplayText(this.gameServerInfo.ServerWorldName, delegate(string _)
								{
									this.serversList.RefreshBindingListEntry(this);
								}, false, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
							}
							return true;
						}
					}
				}
				else if (_bindingName == "servericon")
				{
					_value = PlatformManager.NativePlatform.Utils.GetCrossplayPlayerIcon(this.gameServerInfo.PlayGroup, true, EPlatformIdentifier.None);
					return true;
				}
			}
			else if (num <= 1820482109U)
			{
				if (num <= 1346672274U)
				{
					if (num != 1244254369U)
					{
						if (num == 1346672274U)
						{
							if (_bindingName == "isdedicated")
							{
								_value = this.gameServerInfo.IsDedicated.ToString();
								return true;
							}
						}
					}
					else if (_bindingName == "textcolor")
					{
						_value = (this.gameServerInfo.IsCompatibleVersion ? XUiC_ServersList.textEnabledColor : XUiC_ServersList.textDisabledColor);
						return true;
					}
				}
				else if (num != 1566407741U)
				{
					if (num == 1820482109U)
					{
						if (_bindingName == "isfavorite")
						{
							_value = this.gameServerInfo.IsFavorite.ToString();
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
			else if (num <= 3175373231U)
			{
				if (num != 2675616140U)
				{
					if (num == 3175373231U)
					{
						if (_bindingName == "passwordcolor")
						{
							_value = (this.gameServerInfo.GetValue(GameInfoBool.IsPasswordProtected) ? XUiC_ServersList.iconEnabledColor : XUiC_ServersList.iconDisabledColor);
							return true;
						}
					}
				}
				else if (_bindingName == "pingcolor")
				{
					_value = ((this.gameServerInfo.IsCompatibleVersion && this.gameServerInfo.GetValue(GameInfoInt.Ping) >= 0) ? XUiC_ServersList.textEnabledColor : XUiC_ServersList.textDisabledColor);
					return true;
				}
			}
			else if (num != 3810676121U)
			{
				if (num == 4010301179U)
				{
					if (_bindingName == "anticheatcolor")
					{
						_value = (this.gameServerInfo.EACEnabled ? XUiC_ServersList.iconEnabledColor : XUiC_ServersList.iconDisabledColor);
						return true;
					}
				}
			}
			else if (_bindingName == "servername")
			{
				_value = GeneratedTextManager.GetDisplayTextImmediately(this.gameServerInfo.ServerDisplayName, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
				if (!GeneratedTextManager.IsFiltering(this.gameServerInfo.ServerDisplayName) && !GeneratedTextManager.IsFiltered(this.gameServerInfo.ServerDisplayName))
				{
					GeneratedTextManager.GetDisplayText(this.gameServerInfo.ServerDisplayName, delegate(string _)
					{
						this.serversList.RefreshBindingListEntry(this);
					}, false, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
				}
				return true;
			}
			return false;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.gameServerInfo.GetValue(GameInfoString.GameHost).ContainsCaseInsensitive(_searchString);
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
			if (num > 933488787U)
			{
				if (num <= 1820482109U)
				{
					if (num <= 1346672274U)
					{
						if (num != 1244254369U)
						{
							if (num != 1346672274U)
							{
								return false;
							}
							if (!(_bindingName == "isdedicated"))
							{
								return false;
							}
						}
						else
						{
							if (!(_bindingName == "textcolor"))
							{
								return false;
							}
							goto IL_213;
						}
					}
					else if (num != 1566407741U)
					{
						if (num != 1820482109U)
						{
							return false;
						}
						if (!(_bindingName == "isfavorite"))
						{
							return false;
						}
					}
					else if (!(_bindingName == "hasentry"))
					{
						return false;
					}
					_value = false.ToString();
					return true;
				}
				if (num <= 3175373231U)
				{
					if (num != 2675616140U)
					{
						if (num != 3175373231U)
						{
							return false;
						}
						if (!(_bindingName == "passwordcolor"))
						{
							return false;
						}
					}
					else if (!(_bindingName == "pingcolor"))
					{
						return false;
					}
				}
				else if (num != 3810676121U)
				{
					if (num != 4010301179U)
					{
						return false;
					}
					if (!(_bindingName == "anticheatcolor"))
					{
						return false;
					}
				}
				else
				{
					if (!(_bindingName == "servername"))
					{
						return false;
					}
					goto IL_20A;
				}
				IL_213:
				_value = "0,0,0";
				return true;
			}
			if (num <= 646081446U)
			{
				if (num != 164617456U)
				{
					if (num != 375255177U)
					{
						if (num != 646081446U)
						{
							return false;
						}
						if (!(_bindingName == "playersonline"))
						{
							return false;
						}
					}
					else if (!(_bindingName == "ping"))
					{
						return false;
					}
				}
				else if (!(_bindingName == "difficulty"))
				{
					return false;
				}
			}
			else if (num <= 774435485U)
			{
				if (num != 724407982U)
				{
					if (num != 774435485U)
					{
						return false;
					}
					if (!(_bindingName == "playersmax"))
					{
						return false;
					}
				}
				else if (!(_bindingName == "servericonatlas"))
				{
					return false;
				}
			}
			else if (num != 796832095U)
			{
				if (num != 933488787U)
				{
					return false;
				}
				if (!(_bindingName == "world"))
				{
					return false;
				}
			}
			else if (!(_bindingName == "servericon"))
			{
				return false;
			}
			IL_20A:
			_value = "";
			return true;
		}

		public readonly GameServerInfo gameServerInfo;

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_ServersList serversList;
	}

	public class UiServerFilter : IServerListInterface.ServerFilter
	{
		public UiServerFilter(string _name, XUiC_ServersList.EnumServerLists _applyingTo, Func<XUiC_ServersList.ListEntry, bool> _func = null, IServerListInterface.ServerFilter.EServerFilterType _type = IServerListInterface.ServerFilter.EServerFilterType.Any, int _intMinValue = 0, int _intMaxValue = 0, bool _boolValue = false, string _stringNeedle = null) : base(_name, _type, _intMinValue, _intMaxValue, _boolValue, _stringNeedle)
		{
			this.Func = _func;
			this.ApplyingLists = _applyingTo;
		}

		public readonly Func<XUiC_ServersList.ListEntry, bool> Func;

		public readonly XUiC_ServersList.EnumServerLists ApplyingLists;
	}

	[Flags]
	public enum EnumServerLists
	{
		None = 0,
		Dedicated = 1,
		Peer = 2,
		Friends = 4,
		History = 8,
		LAN = 16,
		Regular = 3,
		Special = 28,
		All = 31
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EnumColumns
	{
		ServerName,
		World,
		Difficulty,
		Players,
		Ping,
		Count
	}
}
