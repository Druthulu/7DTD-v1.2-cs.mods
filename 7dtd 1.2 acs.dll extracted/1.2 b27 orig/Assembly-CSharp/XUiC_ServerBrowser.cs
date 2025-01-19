using System;
using System.Collections;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowser : XUiController
{
	public bool ShowingFilters
	{
		get
		{
			return this.showingFilters;
		}
		set
		{
			this.filtersTabSelector.isActiveTabSelector = value;
			this.browserTabSelector.isActiveTabSelector = !value;
			if (value != this.showingFilters)
			{
				this.showingFilters = value;
				if (!this.showingFilters)
				{
					this.filtersHiddenRefreshRequired = true;
				}
				this.IsDirty = true;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_ServerBrowser.ID = base.WindowGroup.ID;
		((XUiC_SimpleButton)base.GetChildById("btnBack")).OnPressed += this.BtnBack_OnPressed;
		using (IEnumerator<XUiC_ServersList.EnumServerLists> enumerator = EnumUtils.Values<XUiC_ServersList.EnumServerLists>().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XUiC_ServersList.EnumServerLists list = enumerator.Current;
				string id = "type" + list.ToStringCached<XUiC_ServersList.EnumServerLists>();
				XUiController childById = base.GetChildById(id);
				XUiController xuiController = (childById != null) ? childById.GetChildById("button") : null;
				if (xuiController != null)
				{
					xuiController.OnPress += delegate(XUiController _sender, int _args)
					{
						this.serversList.SetServerTypeFilter(list);
						this.IsDirty = true;
					};
				}
			}
		}
		this.btnConnect = (XUiC_SimpleButton)base.GetChildById("btnConnect");
		this.btnConnect.OnPressed += this.BtnConnect_OnPressed;
		this.RefreshConnectLabel();
		XUiController[] childrenById = base.GetChildrenById("btnDirectConnect", null);
		for (int i = 0; i < childrenById.Length; i++)
		{
			((XUiC_SimpleButton)childrenById[i]).OnPressed += this.BtnDirectConnect_OnPressed;
		}
		base.GetChildById("btnShowFilters").OnPress += this.ShowFiltersButton_OnPress;
		((XUiC_SimpleButton)base.GetChildById("pnlEacNeeded").GetChildById("btnOk")).OnPressed += this.BtnEacNeededOk_OnPressed;
		XUiController childById2 = base.GetChildById("pnlEacNeeded").GetChildById("outclick");
		childById2.ViewComponent.IsNavigatable = false;
		childById2.OnPress += this.BtnEacNeededOk_OnPressed;
		this.browserTabSelector = base.GetChildByType<XUiC_TabSelector>();
		this.serversList = (XUiC_ServersList)base.GetChildById("servers");
		this.serversList.OnEntryDoubleClicked += this.ServersList_OnEntryDoubleClicked;
		this.serversList.SelectionChanged += this.ServersList_OnSelectionChanged;
		this.serversList.CountsChanged += this.ServersList_OnCountsChanged;
		this.serverInfo = (XUiC_ServerInfo)base.GetChildById("serverinfo");
		this.serverFilters = (XUiC_ServerFilters)base.GetChildById("serverfilters");
		this.serverFilters.OnFilterChanged += this.OnFilterValueChanged;
		this.serverFilters.SetServersList(this.serversList);
		this.filtersTabSelector = this.serverFilters.GetChildByType<XUiC_TabSelector>();
		this.directConnect = (XUiC_ServerBrowserDirectConnect)base.GetChildById("pnlDirectConnect");
		this.searchErrorPanel = (XUiV_Panel)base.GetChildById("searchErrorPanel").ViewComponent;
		this.searchErrorLabel = (XUiV_Label)base.GetChildById("searchErrorPanel").GetChildById("lblErrorMessage").ViewComponent;
		((XUiC_SimpleButton)base.GetChildById("searchErrorPanel").GetChildById("btnOk")).OnPressed += this.BtnSearchErrorPanel_OnPressed;
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			this.SetServerListType(XUiC_ServersList.EnumServerLists.Peer);
		}
		else
		{
			this.SetServerListType(XUiC_ServersList.EnumServerLists.Dedicated);
		}
		base.RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshConnectLabel()
	{
		InControlExtensions.SetApplyButtonString(this.btnConnect, "xuiServerBrowserConnect");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		this.RefreshConnectLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetServerListType(XUiC_ServersList.EnumServerLists _type)
	{
		this.serversList.SetServerTypeFilter(_type);
		this.serverInfo.InitializeForListFilter(_type);
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDirectConnect_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.directConnect.Show(_sender);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConnect_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_ServerBrowser.<>c__DisplayClass24_0 CS$<>8__locals1 = new XUiC_ServerBrowser.<>c__DisplayClass24_0();
		CS$<>8__locals1.<>4__this = this;
		if (this.btnConnect.Enabled)
		{
			if (this.wdwMultiplayerPrivileges == null)
			{
				this.wdwMultiplayerPrivileges = XUiC_MultiplayerPrivilegeNotification.GetWindow();
			}
			XUiC_ServerBrowser.<>c__DisplayClass24_0 CS$<>8__locals2 = CS$<>8__locals1;
			XUiC_ServersList xuiC_ServersList = this.serversList;
			GameServerInfo gsi;
			if (xuiC_ServersList == null)
			{
				gsi = null;
			}
			else
			{
				XUiC_ListEntry<XUiC_ServersList.ListEntry> selectedEntry = xuiC_ServersList.SelectedEntry;
				if (selectedEntry == null)
				{
					gsi = null;
				}
				else
				{
					XUiC_ServersList.ListEntry entry = selectedEntry.GetEntry();
					gsi = ((entry != null) ? entry.gameServerInfo : null);
				}
			}
			CS$<>8__locals2.gsi = gsi;
			if (CS$<>8__locals1.gsi == null)
			{
				return;
			}
			EUserPerms permissionsWithPrompt = CS$<>8__locals1.gsi.AllowsCrossplay ? (EUserPerms.Multiplayer | EUserPerms.Crossplay) : EUserPerms.Multiplayer;
			XUiC_MultiplayerPrivilegeNotification xuiC_MultiplayerPrivilegeNotification = this.wdwMultiplayerPrivileges;
			if (xuiC_MultiplayerPrivilegeNotification == null)
			{
				return;
			}
			xuiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(permissionsWithPrompt, delegate(bool result)
			{
				if (!result)
				{
					return;
				}
				SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo = CS$<>8__locals1.gsi;
				IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
				if ((antiCheatClient == null || !antiCheatClient.ClientAntiCheatEnabled()) && CS$<>8__locals1.gsi.EACEnabled)
				{
					CS$<>8__locals1.<>4__this.showingEacNeeded = true;
					CS$<>8__locals1.<>4__this.IsDirty = true;
					return;
				}
				if (CS$<>8__locals1.gsi.GetValue(GameInfoBool.IsPasswordProtected))
				{
					XUiC_ServerPasswordWindow.OpenPasswordWindow(CS$<>8__locals1.<>4__this.xui, false, ServerInfoCache.Instance.GetPassword(CS$<>8__locals1.gsi), false, new Action<string>(CS$<>8__locals1.<>4__this.connectToServer), delegate
					{
					});
					return;
				}
				CS$<>8__locals1.<>4__this.connectToServer("");
			}, (EUserPerms)0, -1f, false, null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void connectToServer(string _password)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo != null)
		{
			ServerInfoCache.Instance.SavePassword(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo, _password);
		}
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnEacNeededOk_OnPressed(XUiController _xUiController, int _mouseButton)
	{
		this.showingEacNeeded = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		this.btnConnect.SelectCursorElement(true, false);
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSearchErrorPanel_OnPressed(XUiController _xUiController, int _mouseButton)
	{
		this.searchErrorPanel.IsVisible = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnFilterValueChanged(IServerBrowserFilterControl _sender)
	{
		this.serversList.SetFilter(_sender.GetFilter());
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool anyFilterSet()
	{
		return this.serversList != null && this.serversList.GetActiveFilterCount() > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowFiltersButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.showFilters();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showFilters()
	{
		if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			ServerListManager.Instance.StopSearch();
		}
		this.ShowingFilters = true;
		this.serverFilters.GetChildByType<XUiC_TabSelector>().SelectedTabIndex = 0;
		base.xui.playerUI.CursorController.SetNavigationLockView(this.serverFilters.ViewComponent, this.serverFilters.GetInitialSelectedElement());
	}

	public override void OnOpen()
	{
		this.IsDirty = true;
		base.OnOpen();
		this.windowGroup.isEscClosable = false;
		ServerListManager.Instance.RegisterGameServerFoundCallback(new GameServerFoundCallback(this.onGameServerFoundCallback), new MaxResultsReachedCallback(this.maxResultsCallback), new ServerSearchErrorCallback(this.serverSearchErrorCallback));
		this.btnConnect.Enabled = false;
		if (this.directConnect != null)
		{
			this.directConnect.ViewComponent.IsVisible = false;
		}
		this.searchErrorPanel.IsVisible = false;
		if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			this.showFilters();
		}
		else
		{
			ServerListManager.Instance.StartSearch(null);
			this.ShowingFilters = false;
			base.GetChildById("btnBack").SelectCursorElement(false, false);
		}
		this.showingEacNeeded = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_MultiplayerPrivilegeNotification.Close();
		ServerListManager.Instance.Disconnect();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!PermissionsManager.IsMultiplayerAllowed() && base.xui.playerUI.windowManager.IsWindowOpen(this.windowGroup.ID))
		{
			this.closeBrowser();
		}
		if (base.xui.playerUI.playerInput != null)
		{
			if (base.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed && !base.xui.playerUI.windowManager.IsInputActive())
			{
				this.handleBackOrEscape();
			}
			if (base.xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
			{
				if (this.ShowingFilters)
				{
					this.serverFilters.StartShortcutPressed();
				}
				else if (this.btnConnect.Enabled)
				{
					this.BtnConnect_OnPressed(null, 0);
				}
			}
		}
		if (this.IsDirty)
		{
			this.IsDirty = false;
			base.RefreshBindings(false);
			if (this.showingEacNeeded)
			{
				base.xui.playerUI.CursorController.SetNavigationLockView(base.GetChildById("pnlEacNeeded").ViewComponent, base.GetChildById("pnlEacNeeded").GetChildById("btnOk").ViewComponent);
			}
			if (this.filtersHiddenRefreshRequired)
			{
				this.filtersHiddenRefreshRequired = false;
				if (ServerListManager.Instance.IsPrefilteredSearch)
				{
					base.GetChildById("btnBack").SelectCursorElement(true, false);
					return;
				}
				base.GetChildById("btnShowFilters").SelectCursorElement(true, false);
			}
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 1836220566U)
		{
			if (num <= 1261704394U)
			{
				if (num <= 311487681U)
				{
					if (num != 85646752U)
					{
						if (num == 311487681U)
						{
							if (_bindingName == "filtersbuttonselected")
							{
								_value = this.anyFilterSet().ToString();
								return true;
							}
						}
					}
					else if (_bindingName == "typepeerselected")
					{
						_value = (this.serversList != null && this.serversList.CurrentServerTypeList == XUiC_ServersList.EnumServerLists.Peer).ToString();
						return true;
					}
				}
				else if (num != 977844537U)
				{
					if (num == 1261704394U)
					{
						if (_bindingName == "limitwarningvisible")
						{
							_value = this.maxSearchResultsReached.ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "typefriendscount")
				{
					_value = ((this.serversList == null) ? "" : this.serversList.GetServerCount(XUiC_ServersList.EnumServerLists.Friends).ToString());
					return true;
				}
			}
			else if (num <= 1454731843U)
			{
				if (num != 1309303374U)
				{
					if (num == 1454731843U)
					{
						if (_bindingName == "typededicatedselected")
						{
							_value = (this.serversList != null && this.serversList.CurrentServerTypeList == XUiC_ServersList.EnumServerLists.Dedicated).ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "typehistorycount")
				{
					_value = ((this.serversList == null) ? "" : this.serversList.GetServerCount(XUiC_ServersList.EnumServerLists.History).ToString());
					return true;
				}
			}
			else if (num != 1635420769U)
			{
				if (num == 1836220566U)
				{
					if (_bindingName == "typehistoryselected")
					{
						_value = (this.serversList != null && this.serversList.CurrentServerTypeList == XUiC_ServersList.EnumServerLists.History).ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "typelanselected")
			{
				_value = (this.serversList != null && this.serversList.CurrentServerTypeList == XUiC_ServersList.EnumServerLists.LAN).ToString();
				return true;
			}
		}
		else if (num <= 2310900206U)
		{
			if (num <= 1998907144U)
			{
				if (num != 1856335655U)
				{
					if (num == 1998907144U)
					{
						if (_bindingName == "typepeercount")
						{
							_value = ((this.serversList == null) ? "" : this.serversList.GetServerCount(XUiC_ServersList.EnumServerLists.Peer).ToString());
							return true;
						}
					}
				}
				else if (_bindingName == "typelancount")
				{
					_value = ((this.serversList == null) ? "" : this.serversList.GetServerCount(XUiC_ServersList.EnumServerLists.LAN).ToString());
					return true;
				}
			}
			else if (num != 2081112507U)
			{
				if (num == 2310900206U)
				{
					if (_bindingName == "filtersvisible")
					{
						_value = this.ShowingFilters.ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "typefriendsselected")
			{
				_value = (this.serversList != null && this.serversList.CurrentServerTypeList == XUiC_ServersList.EnumServerLists.Friends).ToString();
				return true;
			}
		}
		else if (num <= 3397682445U)
		{
			if (num != 3210372993U)
			{
				if (num == 3397682445U)
				{
					if (_bindingName == "isprefilteredsearch")
					{
						_value = ServerListManager.Instance.IsPrefilteredSearch.ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "eacneededvisible")
			{
				_value = this.showingEacNeeded.ToString();
				return true;
			}
		}
		else if (num != 3869733143U)
		{
			if (num == 3982131297U)
			{
				if (_bindingName == "typededicatedcount")
				{
					_value = ((this.serversList == null) ? "" : this.serversList.GetServerCount(XUiC_ServersList.EnumServerLists.Dedicated).ToString());
					return true;
				}
			}
		}
		else if (_bindingName == "resultlimit")
		{
			_value = this.searchResultLimit.ToString();
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onGameServerDetailsCallback(bool _success, string _message, GameServerInfo _info)
	{
		if (_success && this.serversList.SelectedEntryIndex >= 0 && _info.Equals(this.serversList.SelectedEntry.GetEntry().gameServerInfo))
		{
			this.serverInfo.SetServerInfo(_info);
			this.serversList.SelectedEntry.RefreshBindings(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onGameServerFoundCallback(IPlatform _owner, GameServerInfo _gameServerInfo, EServerRelationType _source)
	{
		if (!(_gameServerInfo.AllowsCrossplay ? PermissionsManager.IsCrossplayAllowed() : _gameServerInfo.PlayGroup.IsCurrent()))
		{
			return;
		}
		if (Submission.Enabled && !_gameServerInfo.IsCompatibleVersion)
		{
			return;
		}
		if (_gameServerInfo.IsDedicated)
		{
			this.serversList.AddGameServer(_gameServerInfo, _source);
			this.IsDirty = true;
			return;
		}
		string value = _gameServerInfo.GetValue(GameInfoString.CombinedPrimaryId);
		string value2 = _gameServerInfo.GetValue(GameInfoString.CombinedNativeId);
		if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(value2))
		{
			this.serversList.AddGameServer(_gameServerInfo, _source);
			this.IsDirty = true;
			return;
		}
		PlatformUserIdentifierAbs hostPrimaryId = PlatformUserIdentifierAbs.FromCombinedString(value, true);
		PlatformUserIdentifierAbs hostNativeId = PlatformUserIdentifierAbs.FromCombinedString(value2, true);
		ThreadManager.StartCoroutine(this.ResolveBlocksAndAddServerCoroutine(hostPrimaryId, hostNativeId, _gameServerInfo, _source));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ResolveBlocksAndAddServerCoroutine(PlatformUserIdentifierAbs _hostPrimaryId, PlatformUserIdentifierAbs _hostNativeId, GameServerInfo _gameServerInfo, EServerRelationType _source)
	{
		IPlatformUserData userData = PlatformUserManager.GetOrCreate(_hostPrimaryId);
		userData.NativeId = _hostNativeId;
		userData.MarkBlockedStateChanged();
		yield return PlatformUserManager.ResolveUserBlockedCoroutine(userData);
		if (userData.Blocked[EBlockType.Play].IsBlocked())
		{
			yield break;
		}
		this.serversList.AddGameServer(_gameServerInfo, _source);
		this.IsDirty = true;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void maxResultsCallback(IPlatform _sourcePlatform, bool _maxReached, int _limit)
	{
		this.maxSearchResultsReached = _maxReached;
		this.searchResultLimit = _limit;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void serverSearchErrorCallback(string _message)
	{
		this.searchErrorPanel.IsVisible = true;
		this.searchErrorLabel.Text = _message;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnEntryDoubleClicked(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && this.serversList.SelectedEntryIndex >= 0)
		{
			this.BtnConnect_OnPressed(_sender, _mouseButton);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnSelectionChanged(XUiC_ListEntry<XUiC_ServersList.ListEntry> _previousEntry, XUiC_ListEntry<XUiC_ServersList.ListEntry> _newEntry)
	{
		bool flag = _newEntry != null;
		GameServerInfo gameServerInfo = flag ? _newEntry.GetEntry().gameServerInfo : null;
		this.btnConnect.Enabled = (flag && gameServerInfo.IsCompatibleVersion);
		this.serverInfo.SetServerInfo(gameServerInfo);
		if (flag)
		{
			ServerInformationTcpClient.RequestRules(gameServerInfo, false, new ServerInformationTcpClient.RulesRequestDone(this.onGameServerDetailsCallback));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnCountsChanged()
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.handleBackOrEscape();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleBackOrEscape()
	{
		if (base.xui.playerUI.windowManager.IsWindowOpen(XUiC_ServerPasswordWindow.ID))
		{
			return;
		}
		if (this.directConnect != null && this.directConnect.ViewComponent.IsVisible)
		{
			this.directConnect.Hide();
			return;
		}
		if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			if (this.ShowingFilters)
			{
				this.closeBrowser();
				return;
			}
			this.maxSearchResultsReached = false;
			this.serversList.RebuildList(false);
			this.showFilters();
			this.btnConnect.Enabled = false;
			return;
		}
		else
		{
			if (this.ShowingFilters)
			{
				this.serverFilters.closeFilters();
				base.GetChildById("btnShowFilters").SelectCursorElement(true, false);
				return;
			}
			this.closeBrowser();
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeBrowser()
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true, false, true);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnConnect;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showingEacNeeded;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServersList serversList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerFilters serverFilters;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerInfo serverInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MultiplayerPrivilegeNotification wdwMultiplayerPrivileges;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserDirectConnect directConnect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel searchErrorPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label searchErrorLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool maxSearchResultsReached;

	[PublicizedFrom(EAccessModifier.Private)]
	public int searchResultLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showingFilters;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool filtersHiddenRefreshRequired;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector browserTabSelector;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector filtersTabSelector;
}
