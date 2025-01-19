using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerInfo : XUiController
{
	public override void Init()
	{
		base.Init();
		foreach (XUiC_ServerBrowserGamePrefInfo xuiC_ServerBrowserGamePrefInfo in base.GetChildrenByType<XUiC_ServerBrowserGamePrefInfo>(null))
		{
			this.infoFields.Add(xuiC_ServerBrowserGamePrefInfo);
			if (xuiC_ServerBrowserGamePrefInfo.ValueType == GamePrefs.EnumType.Int)
			{
				GameInfoInt gameInfoInt = xuiC_ServerBrowserGamePrefInfo.GameInfoInt;
				if (gameInfoInt != GameInfoInt.CurrentServerTime)
				{
					if (gameInfoInt == GameInfoInt.AirDropFrequency)
					{
						xuiC_ServerBrowserGamePrefInfo.CustomIntValueFormatter = delegate(GameServerInfo _info, int _value)
						{
							string str;
							if (_value % 24 == 0)
							{
								str = "goAirDropValue";
								_value /= 24;
							}
							else
							{
								str = "goAirDropValueHour";
							}
							return string.Format(Localization.Get(str + ((_value == 1) ? "" : "s"), false), _value);
						};
					}
				}
				else
				{
					xuiC_ServerBrowserGamePrefInfo.CustomIntValueFormatter = ((GameServerInfo _info, int _worldTime) => ValueDisplayFormatters.WorldTime((ulong)((long)_worldTime), Localization.Get("xuiDayTimeLong", false)));
				}
			}
			else if (xuiC_ServerBrowserGamePrefInfo.ValueType == GamePrefs.EnumType.String && xuiC_ServerBrowserGamePrefInfo.GameInfoString == GameInfoString.ServerVersion)
			{
				xuiC_ServerBrowserGamePrefInfo.CustomStringValueFormatter = delegate(GameServerInfo _info, string _s)
				{
					if (_info.Version.Build != 0)
					{
						return (_info.IsCompatibleVersion ? "" : "[ff0000]") + _info.Version.LongString;
					}
					return "[ff0000]" + _s;
				};
			}
		}
		XUiController childById = base.GetChildById("ServerDescription");
		this.lblServerDescription = (XUiV_Label)childById.ViewComponent;
		XUiController childById2 = base.GetChildById("ServerWebsiteURL");
		this.lblServerUrl = (XUiV_Label)childById2.ViewComponent;
		childById2.OnHover += this.UrlController_OnHover;
		childById2.OnPress += this.UrlController_OnPress;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.lblServerUrl.IsNavigatable = !string.IsNullOrEmpty(this.lblServerUrl.Text);
	}

	public void InitializeForListFilter(XUiC_ServersList.EnumServerLists mode)
	{
		if (mode == XUiC_ServersList.EnumServerLists.Peer || mode == XUiC_ServersList.EnumServerLists.Friends || mode == XUiC_ServersList.EnumServerLists.History)
		{
			this.SetDisplayMode(XUiC_ServerInfo.DisplayMode.Peer);
			return;
		}
		this.SetDisplayMode(XUiC_ServerInfo.DisplayMode.Dedicated);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDisplayMode(XUiC_ServerInfo.DisplayMode _mode)
	{
		if (this.displayMode != _mode)
		{
			this.displayMode = _mode;
			base.RefreshBindings(false);
		}
	}

	public void SetServerInfo(GameServerInfo _gameInfo)
	{
		foreach (XUiC_ServerBrowserGamePrefInfo xuiC_ServerBrowserGamePrefInfo in this.infoFields)
		{
			xuiC_ServerBrowserGamePrefInfo.SetCurrentValue(_gameInfo);
		}
		if (_gameInfo == null)
		{
			this.lblServerDescription.Text = "";
			this.lblServerUrl.Text = "";
			this.unfilteredUrlText = "";
			this.lblServerUrl.IsNavigatable = false;
			return;
		}
		AuthoredText authoredText = _gameInfo.ServerDescription;
		if (_gameInfo.IsDedicated)
		{
			this.SetDisplayMode(XUiC_ServerInfo.DisplayMode.Dedicated);
		}
		else
		{
			this.SetDisplayMode(XUiC_ServerInfo.DisplayMode.Peer);
			if (string.IsNullOrEmpty(authoredText.Text))
			{
				authoredText = _gameInfo.ServerDisplayName;
			}
		}
		this.unfilteredUrlText = _gameInfo.ServerURL.Text;
		GeneratedTextManager.GetDisplayText(authoredText, delegate(string desc)
		{
			this.lblServerDescription.Text = desc;
		}, true, false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.Supported);
		GeneratedTextManager.GetDisplayText(_gameInfo.ServerURL, delegate(string url)
		{
			this.lblServerUrl.Text = url;
		}, true, false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
		this.lblServerUrl.IsNavigatable = !string.IsNullOrEmpty(_gameInfo.ServerURL.Text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UrlController_OnHover(XUiController _sender, bool _isOver)
	{
		this.lblServerUrl.Color = (_isOver ? new Color32(250, byte.MaxValue, 163, byte.MaxValue) : Color.white);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UrlController_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_MessageBoxWindowGroup.ShowUrlConfirmationDialog(base.xui, this.unfilteredUrlText, false, null, null, null, this.lblServerUrl.Text);
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "showip")
		{
			XUiC_ServerInfo.DisplayMode displayMode = this.displayMode;
			if (displayMode != XUiC_ServerInfo.DisplayMode.Dedicated)
			{
				if (displayMode == XUiC_ServerInfo.DisplayMode.Peer)
				{
					_value = false.ToString();
				}
			}
			else
			{
				_value = true.ToString();
			}
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_ServerBrowserGamePrefInfo> infoFields = new List<XUiC_ServerBrowserGamePrefInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblServerDescription;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblServerUrl;

	[PublicizedFrom(EAccessModifier.Private)]
	public string unfilteredUrlText;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerInfo.DisplayMode displayMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum DisplayMode
	{
		Dedicated,
		Peer
	}
}
