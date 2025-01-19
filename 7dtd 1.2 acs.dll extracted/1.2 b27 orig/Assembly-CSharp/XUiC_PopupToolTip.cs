using System;
using System.Collections.Generic;
using System.Globalization;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PopupToolTip : XUiController
{
	public float TextAlphaCurrent
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.textAlphaCurrent = value;
			this.IsDirty = true;
		}
	}

	public string TooltipText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.tooltipText = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_PopupToolTip.ID = base.WindowGroup.ID;
		this.toolbelt = base.xui.GetChildByType<XUiC_Toolbelt>();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (GameStats.GetInt(EnumGameStats.GameState) == 1)
		{
			if ((base.xui.playerUI != null && base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.IsDead()) || !base.xui.playerUI.windowManager.IsHUDEnabled())
			{
				this.ClearTooltipsInternal();
			}
			if (!this.pauseToolTips)
			{
				this.TextAlphaCurrent = Mathf.Lerp(this.textAlphaCurrent, this.textAlphaTarget, _dt * 3f);
				if (this.countdownTooltip.HasPassed() && base.xui.isReady && !XUiC_SubtitlesDisplay.IsDisplaying)
				{
					this.DisplayTooltipText();
				}
			}
		}
		if (this.IsDirty)
		{
			base.RefreshBindings(true);
			this.IsDirty = false;
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "text")
		{
			_value = this.tooltipText;
			return true;
		}
		if (_bindingName == "textalpha")
		{
			_value = this.textalphaFormatter.Format((int)(255f * this.textAlphaCurrent));
			return true;
		}
		if (!(_bindingName == "yoffset_secondrow"))
		{
			return false;
		}
		_value = this.yoffsetFormatter.Format((this.toolbelt != null && this.toolbelt.HasSecondRow) ? this.yOffsetSecondRow : 0);
		return true;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "yoffset_second_row")
		{
			this.yOffsetSecondRow = StringParsers.ParseSInt32(_value, 0, -1, NumberStyles.Integer);
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearTooltipsInternal()
	{
		this.tooltipQueue.Clear();
		this.TextAlphaCurrent = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QueueTooltipInternal(string _text, string[] _args, string _alertSound, ToolTipEvent _eventHandler, bool _showImmediately)
	{
		if (string.IsNullOrEmpty(_text) && string.IsNullOrEmpty(_alertSound) && _eventHandler == null)
		{
			return;
		}
		_text = Localization.Get(_text, false);
		if (_args != null && _args.Length != 0)
		{
			_text = string.Format(_text, _args);
		}
		XUiC_PopupToolTip.Tooltip item = new XUiC_PopupToolTip.Tooltip(_text, _alertSound, _eventHandler);
		if (_showImmediately)
		{
			this.immediateTip = item;
			this.DisplayTooltipText();
			return;
		}
		if (!this.tooltipQueue.Contains(item))
		{
			this.tooltipQueue.Enqueue(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DisplayTooltipText()
	{
		if (this.tooltipQueue.Count == 0 && this.immediateTip == null)
		{
			this.TextAlphaCurrent = 0f;
			this.TooltipText = string.Empty;
			return;
		}
		XUiC_PopupToolTip.Tooltip tooltip;
		if (this.immediateTip != null)
		{
			tooltip = this.immediateTip;
			this.immediateTip = null;
		}
		else
		{
			tooltip = this.tooltipQueue.Dequeue();
		}
		if (!string.IsNullOrEmpty(tooltip.AlertSoundName))
		{
			Manager.PlayInsidePlayerHead(tooltip.AlertSoundName, -1, 0f, false, false);
		}
		ToolTipEvent @event = tooltip.Event;
		if (@event != null)
		{
			@event.HandleEvent();
		}
		this.TextAlphaCurrent = 0f;
		if (!string.IsNullOrEmpty(tooltip.Text))
		{
			this.textAlphaTarget = 1f;
			this.TooltipText = tooltip.Text;
		}
		else
		{
			this.textAlphaTarget = 0f;
			this.TooltipText = "";
		}
		this.countdownTooltip.ResetAndRestart();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetToolTipPauseInternal(bool _isPaused)
	{
		this.pauseToolTips = _isPaused;
		if (_isPaused)
		{
			this.TooltipText = string.Empty;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_PopupToolTip GetInstance(XUi _xui)
	{
		XUiWindowGroup xuiWindowGroup = (XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(XUiC_PopupToolTip.ID);
		if (xuiWindowGroup == null)
		{
			return null;
		}
		XUiController controller = xuiWindowGroup.Controller;
		if (controller == null)
		{
			return null;
		}
		return controller.GetChildByType<XUiC_PopupToolTip>();
	}

	public static void ClearTooltips(XUi _xui)
	{
		XUiC_PopupToolTip instance = XUiC_PopupToolTip.GetInstance(_xui);
		if (instance == null)
		{
			return;
		}
		instance.ClearTooltipsInternal();
	}

	public static void QueueTooltip(XUi _xui, string _text, string[] _args, string _alertSound, ToolTipEvent _eventHandler, bool _showImmediately)
	{
		XUiC_PopupToolTip instance = XUiC_PopupToolTip.GetInstance(_xui);
		if (instance == null)
		{
			return;
		}
		instance.QueueTooltipInternal(_text, _args, _alertSound, _eventHandler, _showImmediately);
	}

	public static void SetToolTipPause(XUi _xui, bool _isPaused)
	{
		XUiC_PopupToolTip instance = XUiC_PopupToolTip.GetInstance(_xui);
		if (instance == null)
		{
			return;
		}
		instance.SetToolTipPauseInternal(_isPaused);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int yOffsetSecondRow = 75;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Toolbelt toolbelt;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tooltipText = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public float textAlphaTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public float textAlphaCurrent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CountdownTimer countdownTooltip = new CountdownTimer(5f, true);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pauseToolTips;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Queue<XUiC_PopupToolTip.Tooltip> tooltipQueue = new Queue<XUiC_PopupToolTip.Tooltip>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PopupToolTip.Tooltip immediateTip;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt textalphaFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt yoffsetFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public class Tooltip : IEquatable<XUiC_PopupToolTip.Tooltip>
	{
		public Tooltip(string _text, string _alertSoundName, ToolTipEvent _event)
		{
			this.Text = _text;
			this.AlertSoundName = _alertSoundName;
			this.Event = _event;
		}

		public bool Equals(XUiC_PopupToolTip.Tooltip _other)
		{
			return _other != null && (this == _other || this.Text == _other.Text);
		}

		public override bool Equals(object _obj)
		{
			return _obj != null && (this == _obj || (!(_obj.GetType() != base.GetType()) && this.Equals((XUiC_PopupToolTip.Tooltip)_obj)));
		}

		public override int GetHashCode()
		{
			if (this.Text == null)
			{
				return 0;
			}
			return this.Text.GetHashCode();
		}

		public readonly string Text;

		public readonly string AlertSoundName;

		public readonly ToolTipEvent Event;
	}
}
