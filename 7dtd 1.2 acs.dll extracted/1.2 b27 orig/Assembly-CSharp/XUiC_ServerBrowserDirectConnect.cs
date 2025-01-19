using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserDirectConnect : XUiController
{
	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		return false;
	}

	public override void Init()
	{
		base.Init();
		this.txtIp = (XUiC_TextInput)base.GetChildById("txtIp");
		this.txtPort = (XUiC_TextInput)base.GetChildById("txtPort");
		this.txtIp.OnClipboardHandler += this.TxtIp_OnClipboardHandler;
		this.txtIp.OnSubmitHandler += this.Txt_OnSubmitHandler;
		this.txtPort.OnSubmitHandler += this.Txt_OnSubmitHandler;
		this.txtIp.OnChangeHandler += this.validateIpPort;
		this.txtPort.OnChangeHandler += this.validateIpPort;
		this.txtIp.SelectOnTab = this.txtPort;
		this.txtPort.SelectOnTab = this.txtIp;
		this.btnCancel = (XUiC_SimpleButton)base.GetChildById("btnCancel");
		this.btnCancel.OnPressed += this.BtnCancel_OnPressed;
		this.btnDirectConnectConnect = (XUiC_SimpleButton)base.GetChildById("btnDirectConnectConnect");
		this.btnDirectConnectConnect.OnPressed += this.BtnConnect_OnPressed;
		this.btnDirectConnectConnect.Enabled = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_MultiplayerPrivilegeNotification.Close();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtIp_OnClipboardHandler(UIInput.ClipboardAction _actiontype, string _oldtext, int _selstart, int _selend, string _actionresulttext)
	{
		if (_actiontype != UIInput.ClipboardAction.Paste)
		{
			return;
		}
		if (_selend - _selstart != _oldtext.Length)
		{
			return;
		}
		Match match = XUiC_ServerBrowserDirectConnect.ipPortMatcher.Match(_actionresulttext);
		if (!match.Success)
		{
			return;
		}
		string value = match.Groups[1].Value;
		int num;
		if (!StringParsers.TryParseSInt32(match.Groups[2].Value, out num, 0, -1, NumberStyles.Integer))
		{
			return;
		}
		this.txtIp.Text = value;
		this.txtPort.Text = num.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Txt_OnSubmitHandler(XUiController _sender, string _text)
	{
		this.BtnConnect_OnPressed(_sender, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConnect_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.ViewComponent.IsVisible = false;
		this.saveIpPort();
		if (this.wdwMultiplayerPrivileges == null)
		{
			this.wdwMultiplayerPrivileges = XUiC_MultiplayerPrivilegeNotification.GetWindow();
		}
		XUiC_MultiplayerPrivilegeNotification xuiC_MultiplayerPrivilegeNotification = this.wdwMultiplayerPrivileges;
		if (xuiC_MultiplayerPrivilegeNotification == null)
		{
			return;
		}
		xuiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(EUserPerms.Multiplayer, delegate(bool result)
		{
			if (!result)
			{
				return;
			}
			string text = this.txtIp.Text.Trim();
			long num;
			if (!long.TryParse(text.Replace(".", ""), out num))
			{
				try
				{
					IPHostEntry hostEntry = Dns.GetHostEntry(text);
					if (hostEntry.AddressList.Length == 0)
					{
						Log.Out("No valid IP for server found");
						return;
					}
					text = hostEntry.AddressList[0].ToString();
					if (hostEntry.AddressList[0].AddressFamily != AddressFamily.InterNetwork)
					{
						for (int i = 1; i < hostEntry.AddressList.Length; i++)
						{
							if (hostEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
							{
								text = hostEntry.AddressList[i].ToString();
								break;
							}
						}
					}
				}
				catch (SocketException ex)
				{
					string str = "No such hostname: \"";
					string str2 = text;
					string str3 = "\": ";
					SocketException ex2 = ex;
					Log.Out(str + str2 + str3 + ((ex2 != null) ? ex2.ToString() : null));
					return;
				}
			}
			Log.Out("Connect by IP");
			GameServerInfo gameServerInfo = new GameServerInfo();
			gameServerInfo.SetValue(GameInfoString.IP, text);
			gameServerInfo.SetValue(GameInfoInt.Port, int.Parse(this.txtPort.Text));
			GameManager.Instance.showOpenerMovieOnLoad = false;
			SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo = gameServerInfo;
			base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
			SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
		}, (EUserPerms)0, -1f, false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.Hide();
	}

	public void Show(XUiController _cancelTarget)
	{
		this.viewComponent.IsVisible = true;
		if (this.txtIp != null)
		{
			this.txtIp.Text = GamePrefs.GetString(EnumGamePrefs.ConnectToServerIP);
			this.txtPort.Text = GamePrefs.GetInt(EnumGamePrefs.ConnectToServerPort).ToString();
			this.txtIp.SetSelected(true, false);
			this.validateIpPort(null, null, true);
		}
		this.cancelTarget = _cancelTarget;
		base.xui.playerUI.CursorController.SetNavigationLockView(this.viewComponent, null);
		if (this.btnCancel != null)
		{
			this.btnCancel.SelectCursorElement(true, false);
		}
	}

	public void Hide()
	{
		this.viewComponent.IsVisible = false;
		this.saveIpPort();
		base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		if (this.cancelTarget != null)
		{
			this.cancelTarget.SelectCursorElement(true, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void validateIpPort(XUiController _sender, string _newText, bool _changeFromCode)
	{
		int num;
		bool flag = int.TryParse(this.txtPort.Text, out num);
		flag = (flag && this.txtIp.Text.Length > 0 && num > 0 && num < 65533);
		this.btnDirectConnectConnect.Enabled = flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveIpPort()
	{
		if (this.txtIp != null && this.btnDirectConnectConnect.Enabled)
		{
			GamePrefs.Set(EnumGamePrefs.ConnectToServerIP, this.txtIp.Text);
			GamePrefs.Set(EnumGamePrefs.ConnectToServerPort, StringParsers.ParseSInt32(this.txtPort.Text, 0, -1, NumberStyles.Integer));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MultiplayerPrivilegeNotification wdwMultiplayerPrivileges;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtIp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtPort;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDirectConnectConnect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCancel;

	public XUiController cancelTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex ipPortMatcher = new Regex("^(.*):(\\d+)$");
}
