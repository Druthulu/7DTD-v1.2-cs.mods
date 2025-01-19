using System;
using System.Globalization;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChatOutput : XUiController
{
	public override void Init()
	{
		XUiC_ChatOutput.ID = this.windowGroup.ID;
		base.Init();
		this.txtOutput = (XUiV_TextList)base.GetChildById("txtOutput").ViewComponent;
		this.collider = this.txtOutput.UiTransform.GetComponent<BoxCollider>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddMessage(EnumGameMessages _messageType, EChatType _chatType, string _message)
	{
		if (this.txtOutput == null)
		{
			return;
		}
		if (!ThreadManager.IsMainThread())
		{
			ThreadManager.AddSingleTaskMainThread("AddTextListLine", delegate(object _)
			{
				this.AddMessage(_messageType, _chatType, _message);
			}, null);
			return;
		}
		if (_messageType == EnumGameMessages.Chat)
		{
			switch (_chatType)
			{
			case EChatType.Global:
				_message = "[ffffff]" + _message;
				break;
			case EChatType.Friends:
				_message = "[00bb00]" + _message;
				break;
			case EChatType.Party:
				_message = "[ffcc00]" + _message;
				break;
			case EChatType.Whisper:
				_message = "[d00000]" + _message;
				break;
			default:
				throw new ArgumentOutOfRangeException("_chatType", _chatType, null);
			}
		}
		_message = _message.Replace('\n', ' ');
		this.txtOutput.AddLine(_message);
		this.txtOutput.Label.alpha = 1f;
		this.currentWaitTime = this.fadeoutWaitTime + this.fadeoutDuration;
		this.txtOutput.TextList.scrollValue = 1f;
	}

	public static void AddMessage(XUi _xuiInstance, EnumGameMessages _messageType, EChatType _chatType, string _message, int _senderId, EMessageSender _messageSender, GeneratedTextManager.TextFilteringMode _filteringMode = GeneratedTextManager.TextFilteringMode.None)
	{
		if (_messageType == EnumGameMessages.Chat && !PermissionsManager.IsCommunicationAllowed())
		{
			return;
		}
		if (_senderId != -1)
		{
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_senderId);
			if (playerDataFromEntityID == null)
			{
				Log.Warning(string.Format("Could not find player name corresponding to entity id {0}, discarding message", _senderId));
				return;
			}
			if (playerDataFromEntityID != null && playerDataFromEntityID.PlatformData.Blocked[EBlockType.TextChat].IsBlocked())
			{
				return;
			}
			if (_messageSender == EMessageSender.SenderIdAsPlayer)
			{
				_message = Utils.CreateGameMessage((playerDataFromEntityID != null) ? playerDataFromEntityID.PlayerName.DisplayName : null, _message);
			}
		}
		if (_messageSender == EMessageSender.Server)
		{
			_message = Utils.CreateGameMessage(Localization.Get("xuiChatServer", false), _message);
		}
		GeneratedTextManager.BbCodeSupportMode bbSupportMode = GeneratedTextManager.BbCodeSupportMode.Supported;
		if (_messageType == EnumGameMessages.Chat && _messageSender == EMessageSender.SenderIdAsPlayer)
		{
			bbSupportMode = GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes;
		}
		GeneratedTextManager.GetDisplayText(_message, null, delegate(string _filteredMessage)
		{
			XUiController xuiController = _xuiInstance.FindWindowGroupByName(XUiC_ChatOutput.ID);
			if (xuiController != null)
			{
				_filteredMessage += "[ffffffff][/url][/b][/i][/u][/s][/sub][/sup]";
				xuiController.GetChildByType<XUiC_ChatOutput>().AddMessage(_messageType, _chatType, _filteredMessage);
			}
		}, false, _filteringMode, bbSupportMode);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		bool flag = base.xui.playerUI.windowManager.IsWindowOpen(XUiC_Chat.ID);
		this.collider.enabled = flag;
		if (flag)
		{
			this.currentWaitTime = this.fadeoutWaitTime + this.fadeoutDuration;
		}
		this.txtOutput.Label.alpha = Mathf.Lerp(0f, 1f, this.currentWaitTime / this.fadeoutDuration);
		this.currentWaitTime -= Time.deltaTime;
		if (GameManager.Instance == null || GameManager.Instance.World == null || base.xui.playerUI.entityPlayer == null || base.xui.playerUI.entityPlayer.IsDead())
		{
			this.txtOutput.Label.alpha = 0f;
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "fadeout_duration")
		{
			this.fadeoutDuration = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
			return true;
		}
		if (!(_name == "fadeout_wait_time"))
		{
			return base.ParseAttribute(_name, _value, _parent);
		}
		this.fadeoutWaitTime = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
		return true;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_TextList txtOutput;

	[PublicizedFrom(EAccessModifier.Private)]
	public BoxCollider collider;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadeoutWaitTime = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadeoutDuration = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentWaitTime;
}
