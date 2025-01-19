using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerFilters : XUiController
{
	public event Action<IServerBrowserFilterControl> OnFilterChanged;

	public override void Init()
	{
		base.Init();
		base.GetChildById("outclick").OnPress += this.CloseFiltersButton_OnPress;
		base.GetChildById("btnCloseFilters").OnPress += this.CloseFiltersButton_OnPress;
		((XUiC_SimpleButton)base.GetChildById("btnResetFilters")).OnPressed += this.ResetFiltersButton_OnPress;
		((XUiC_SimpleButton)base.GetChildById("btnBack")).OnPressed += this.BackButton_OnPress;
		this.btnStartSearch = (XUiC_SimpleButton)base.GetChildById("btnStartSearch");
		this.btnStartSearch.OnPressed += this.StartSearchButton_OnPress;
		this.btnStartSearch.Text = "[action:gui:GUI Apply] " + Localization.Get("xuiServerStartSearch", false).ToUpper();
		foreach (XUiC_ServerBrowserGamePrefSelectorCombo xuiC_ServerBrowserGamePrefSelectorCombo in base.GetChildrenByType<XUiC_ServerBrowserGamePrefSelectorCombo>(null))
		{
			GameInfoInt gameInfoInt = xuiC_ServerBrowserGamePrefSelectorCombo.GameInfoInt;
			if (gameInfoInt != GameInfoInt.CurrentServerTime)
			{
				if (gameInfoInt != GameInfoInt.AirDropFrequency)
				{
					if (gameInfoInt == GameInfoInt.WorldSize)
					{
						if (PlatformOptimizations.EnforceMaxWorldSizeClient)
						{
							xuiC_ServerBrowserGamePrefSelectorCombo.ValueRangeMin = new int?(0);
							xuiC_ServerBrowserGamePrefSelectorCombo.ValueRangeMax = new int?(PlatformOptimizations.MaxWorldSizeClient);
						}
					}
				}
				else
				{
					xuiC_ServerBrowserGamePrefSelectorCombo.ValuePreDisplayModifierFunc = ((int _n) => _n / 24);
				}
			}
			else
			{
				xuiC_ServerBrowserGamePrefSelectorCombo.CustomValuePreFilterModifierFunc = ((int _n) => (_n - 1) * 24000);
			}
			if (xuiC_ServerBrowserGamePrefSelectorCombo.GameInfoString == GameInfoString.Region)
			{
				this.regionFilter = xuiC_ServerBrowserGamePrefSelectorCombo;
			}
			GameInfoBool gameInfoBool = xuiC_ServerBrowserGamePrefSelectorCombo.GameInfoBool;
			switch (gameInfoBool)
			{
			case GameInfoBool.EACEnabled:
				this.eacFilter = xuiC_ServerBrowserGamePrefSelectorCombo;
				break;
			case GameInfoBool.SanctionsIgnored:
				this.ignoreSanctionsFilter = xuiC_ServerBrowserGamePrefSelectorCombo;
				break;
			case GameInfoBool.Architecture64:
			case GameInfoBool.StockSettings:
			case GameInfoBool.StockFiles:
				break;
			case GameInfoBool.ModdedConfig:
				this.moddedConfigsFilter = xuiC_ServerBrowserGamePrefSelectorCombo;
				break;
			case GameInfoBool.RequiresMod:
				this.requiresModsFilter = xuiC_ServerBrowserGamePrefSelectorCombo;
				break;
			default:
				if (gameInfoBool == GameInfoBool.AllowCrossplay)
				{
					this.crossplayFilter = xuiC_ServerBrowserGamePrefSelectorCombo;
				}
				break;
			}
			XUiC_ServerBrowserGamePrefSelectorCombo xuiC_ServerBrowserGamePrefSelectorCombo2 = xuiC_ServerBrowserGamePrefSelectorCombo;
			xuiC_ServerBrowserGamePrefSelectorCombo2.OnValueChanged = (Action<IServerBrowserFilterControl>)Delegate.Combine(xuiC_ServerBrowserGamePrefSelectorCombo2.OnValueChanged, new Action<IServerBrowserFilterControl>(this.OnFilterValueChanged));
			this.allFilterControls.Add(xuiC_ServerBrowserGamePrefSelectorCombo);
		}
		foreach (XUiC_ServerBrowserGamePrefString xuiC_ServerBrowserGamePrefString in base.GetChildrenByType<XUiC_ServerBrowserGamePrefString>(null))
		{
			if (xuiC_ServerBrowserGamePrefString.GameInfoString == GameInfoString.Language)
			{
				this.languageFilter = xuiC_ServerBrowserGamePrefString;
			}
			XUiC_ServerBrowserGamePrefString xuiC_ServerBrowserGamePrefString2 = xuiC_ServerBrowserGamePrefString;
			xuiC_ServerBrowserGamePrefString2.OnValueChanged = (Action<IServerBrowserFilterControl>)Delegate.Combine(xuiC_ServerBrowserGamePrefString2.OnValueChanged, new Action<IServerBrowserFilterControl>(this.OnFilterValueChanged));
			this.allFilterControls.Add(xuiC_ServerBrowserGamePrefString);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.IsDirty = false;
			base.RefreshBindings(false);
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "crossplayTooltip")
		{
			_value = (PermissionsManager.GetPermissionDenyReason(EUserPerms.Crossplay, PermissionsManager.PermissionSources.All) ?? Localization.Get("xuiOptionsGeneralCrossplayTooltip", false));
			return true;
		}
		if (!(_bindingName == "results"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = this.currentResults.ToString();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseFiltersButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.closeFilters();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetFiltersButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.ResetFilters();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BackButton_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartSearchButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.closeFilters();
		List<IServerListInterface.ServerFilter> list = new List<IServerListInterface.ServerFilter>();
		foreach (IServerBrowserFilterControl serverBrowserFilterControl in this.allFilterControls)
		{
			XUiC_ServersList.UiServerFilter filter = serverBrowserFilterControl.GetFilter();
			if (filter.Type != IServerListInterface.ServerFilter.EServerFilterType.Any)
			{
				list.Add(filter);
			}
		}
		ServerListManager.Instance.StartSearch(list);
	}

	public void StartShortcutPressed()
	{
		if (this.btnStartSearch.Enabled && this.btnStartSearch.IsVisible)
		{
			this.StartSearchButton_OnPress(null, 0);
			return;
		}
		this.CloseFiltersButton_OnPress(null, 0);
	}

	public void ResetFilters()
	{
		foreach (IServerBrowserFilterControl serverBrowserFilterControl in this.allFilterControls)
		{
			serverBrowserFilterControl.Reset();
		}
		if (this.regionFilter != null)
		{
			string stringValue = GamePrefs.GetString(EnumGamePrefs.Region) ?? "";
			this.regionFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(stringValue, null));
		}
		if (this.languageFilter != null)
		{
			string value = GamePrefs.GetString(EnumGamePrefs.LanguageBrowser) ?? "";
			this.languageFilter.SetValue(value);
		}
		this.ApplyForcedSettings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyForcedSettings()
	{
		bool flag = PermissionsManager.IsCrossplayAllowed();
		this.crossplayFilter.Enabled = flag;
		if (!flag)
		{
			this.crossplayFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(0, null));
		}
		if (!LaunchPrefs.AllowJoinConfigModded.Value)
		{
			this.moddedConfigsFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(0, null));
			this.moddedConfigsFilter.ViewComponent.UiTransform.gameObject.SetActive(false);
		}
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			this.requiresModsFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(0, null));
			this.requiresModsFilter.ViewComponent.UiTransform.gameObject.SetActive(false);
			if (Submission.Enabled)
			{
				this.eacFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(1, null));
				this.eacFilter.ViewComponent.UiTransform.gameObject.SetActive(false);
				this.ignoreSanctionsFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(0, null));
				this.ignoreSanctionsFilter.ViewComponent.UiTransform.gameObject.SetActive(false);
			}
		}
	}

	public void closeFilters()
	{
		this.windowGroup.Controller.GetChildByType<XUiC_ServerBrowser>().ShowingFilters = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
	}

	public void SetServersList(XUiC_ServersList _serversList)
	{
		_serversList.OnFilterResultsChanged += this.ServersList_OnFilterResultsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnFilterResultsChanged(int _count)
	{
		this.currentResults = _count;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnFilterValueChanged(IServerBrowserFilterControl _sender)
	{
		Action<IServerBrowserFilterControl> onFilterChanged = this.OnFilterChanged;
		if (onFilterChanged != null)
		{
			onFilterChanged(_sender);
		}
		this.IsDirty = true;
	}

	public override void OnVisibilityChanged(bool _isVisible)
	{
		base.OnVisibilityChanged(_isVisible);
		if (_isVisible && base.IsOpen)
		{
			this.SelectInitialElement();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!this.openedBefore)
		{
			this.openedBefore = true;
			this.ResetFilters();
		}
		else
		{
			this.ApplyForcedSettings();
		}
		base.RefreshBindings(false);
	}

	public void SelectInitialElement()
	{
		if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			base.GetChildById("btnStartSearch").SelectCursorElement(true, false);
			return;
		}
		base.GetChildById("btnResetFilters").SelectCursorElement(true, false);
	}

	public XUiView GetInitialSelectedElement()
	{
		if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			return base.GetChildById("btnStartSearch").ViewComponent;
		}
		return base.GetChildById("btnResetFilters").ViewComponent;
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.regionFilter != null)
		{
			XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue selection = this.regionFilter.GetSelection();
			if (selection.Type == XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.String)
			{
				GamePrefs.Set(EnumGamePrefs.Region, selection.StringValue);
			}
		}
		if (this.languageFilter != null)
		{
			string value = this.languageFilter.GetValue();
			GamePrefs.Set(EnumGamePrefs.LanguageBrowser, value);
		}
		GamePrefs.Set(EnumGamePrefs.IgnoreEOSSanctions, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<IServerBrowserFilterControl> allFilterControls = new List<IServerBrowserFilterControl>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo regionFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefString languageFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo crossplayFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo moddedConfigsFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo requiresModsFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo eacFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo ignoreSanctionsFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentResults;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openedBefore;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnStartSearch;
}
