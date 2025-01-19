using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ReportPlayer : XUiController
{
	public static void Open(PlayerData _reportedPlayerData, string _windowOnClose = "")
	{
		if (LocalPlayerUI.primaryUI.windowManager.IsWindowOpen(XUiC_ReportPlayer.ID))
		{
			return;
		}
		XUiC_ReportPlayer childByType = ((XUiWindowGroup)LocalPlayerUI.primaryUI.windowManager.GetWindow(XUiC_ReportPlayer.ID)).Controller.GetChildByType<XUiC_ReportPlayer>();
		childByType.reportedPlayerData = _reportedPlayerData;
		childByType.windowOnClose = _windowOnClose;
		XUiC_ReportPlayer.initialText = "";
		LocalPlayerUI.primaryUI.windowManager.Open(XUiC_ReportPlayer.ID, true, false, true);
	}

	public static void Open(PlayerData _reportedPlayerData, EnumReportCategory _initialCategory, string _intialText, string _windowOnClose = "")
	{
		XUiC_ReportPlayer.initialCategory = _initialCategory;
		XUiC_ReportPlayer.initialText = _intialText;
		XUiC_ReportPlayer.Open(_reportedPlayerData, _windowOnClose);
	}

	public override void Init()
	{
		base.Init();
		XUiC_ReportPlayer.ID = base.WindowGroup.ID;
		this.lblReportedPlayer = (XUiV_Label)base.GetChildById("lblReportedPlayer").ViewComponent;
		this.cbxCategory = (XUiC_ComboBoxList<IPlayerReporting.PlayerReportCategory>)base.GetChildById("cbxCategory");
		IPlayerReporting playerReporting = PlatformManager.MultiPlatform.PlayerReporting;
		IList<IPlayerReporting.PlayerReportCategory> list = (playerReporting != null) ? playerReporting.ReportCategories() : null;
		if (list != null)
		{
			foreach (IPlayerReporting.PlayerReportCategory item in list)
			{
				this.cbxCategory.Elements.Add(item);
			}
		}
		this.txtMessage = (XUiC_TextInput)base.GetChildById("txtMessage");
		this.txtMessage.OnInputErrorHandler += this.UpdateErrorMessage;
		((XUiC_SimpleButton)base.GetChildById("btnSend")).OnPressed += this.BtnSend_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnCancel")).OnPressed += this.BtnCancel_OnPressed;
		this.btnKickPlayer = (XUiC_SimpleButton)base.GetChildById("btnKick");
		this.btnKickPlayer.OnPressed += this.BtnKick_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnKick_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (this.reportedPlayerData != null)
		{
			PlatformUserIdentifierAbs platformUserIdentifierAbs;
			ClientInfo clientInfo;
			if (ConsoleHelper.ParseParamPartialNameOrId(this.reportedPlayerData.PlayerName.Text, out platformUserIdentifierAbs, out clientInfo, true) == 1)
			{
				DateTime maxValue = DateTime.MaxValue;
				string text = "";
				if (clientInfo != null)
				{
					ClientInfo cInfo = clientInfo;
					GameUtils.EKickReason kickReason = GameUtils.EKickReason.ManualKick;
					int apiResponseEnum = 0;
					string customReason = string.IsNullOrEmpty(text) ? "" : text;
					GameUtils.KickPlayerForClientInfo(cInfo, new GameUtils.KickPlayerData(kickReason, apiResponseEnum, maxValue, customReason));
				}
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[xui] Kick Succeeded");
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[xui] Failed to find player to kick");
		}
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSend_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (this.reportedPlayerData == null)
		{
			this.ReportSentMessageBox(false);
			return;
		}
		string text = this.txtMessage.Text;
		if (!string.IsNullOrEmpty(XUiC_ReportPlayer.initialText))
		{
			text = text + "\n" + string.Format(Localization.Get("xuiReportAutomatedMessage", false), XUiC_ReportPlayer.initialText, XUiC_ReportPlayer.initialCategory);
		}
		IPlayerReporting playerReporting = PlatformManager.MultiPlatform.PlayerReporting;
		if (playerReporting != null)
		{
			playerReporting.ReportPlayer(this.reportedPlayerData.PrimaryId, this.cbxCategory.Value, text, new Action<bool>(this.ReportSentMessageBox));
		}
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReportSentMessageBox(bool _success)
	{
		string text = _success ? Localization.Get("xuiReportPlayerSuccess", false) : Localization.Get("xuiReportPlayerFail", false);
		XUiC_MessageBoxWindowGroup.ShowMessageBox(base.xui, Localization.Get("xuiReportPlayerHeader", false), text, XUiC_MessageBoxWindowGroup.MessageBoxTypes.Ok, null, null, true, true);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.txtMessage.Text = XUiC_ReportPlayer.initialText;
		this.inputErrorMessage = null;
		XUiC_ComboBoxList<IPlayerReporting.PlayerReportCategory> xuiC_ComboBoxList = this.cbxCategory;
		List<IPlayerReporting.PlayerReportCategory> elements = this.cbxCategory.Elements;
		IPlayerReporting playerReporting = PlatformManager.MultiPlatform.PlayerReporting;
		xuiC_ComboBoxList.SelectedIndex = elements.IndexOf((playerReporting != null) ? playerReporting.GetPlayerReportCategoryMapping(XUiC_ReportPlayer.initialCategory) : null);
		if (this.cbxCategory.SelectedIndex == -1)
		{
			this.cbxCategory.SelectedIndex = 0;
		}
		if (this.reportedPlayerData != null)
		{
			this.lblReportedPlayer.Text = GeneratedTextManager.GetDisplayTextImmediately(this.reportedPlayerData.PlayerName, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
		}
		PersistentPlayerList persistentPlayers = GameManager.Instance.persistentPlayers;
		int? num;
		if (persistentPlayers == null)
		{
			num = null;
		}
		else
		{
			PersistentPlayerData playerData = persistentPlayers.GetPlayerData(this.reportedPlayerData.PrimaryId);
			num = ((playerData != null) ? new int?(playerData.EntityId) : null);
		}
		int num2 = num ?? -1;
		this.btnKickPlayer.IsVisible = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && num2 != -1);
		base.RefreshBindings(false);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.reportedPlayerData = null;
		if (!string.IsNullOrEmpty(this.windowOnClose))
		{
			LocalPlayerUI.primaryUI.windowManager.Open(this.windowOnClose, true, false, true);
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "inputWarning")
		{
			_value = this.inputErrorMessage;
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateErrorMessage(XUiController _sender, string _errorMessage)
	{
		this.inputErrorMessage = _errorMessage;
		base.RefreshBindings(false);
	}

	public static string ID;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string initialText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static EnumReportCategory initialCategory = EnumReportCategory.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblReportedPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<IPlayerReporting.PlayerReportCategory> cbxCategory;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtMessage;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnKickPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public string inputErrorMessage;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerData reportedPlayerData;

	[PublicizedFrom(EAccessModifier.Private)]
	public string windowOnClose;
}
