using System;
using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Timer : XUiController
{
	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.playerUI.CursorController.SetCursorHidden(true);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.CancelButton, "igcoCancel", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu, 0f);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.UpdateTimer(0f, 1f);
		this.fullTime = 0f;
		base.xui.playerUI.CursorController.SetCursorHidden(false);
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		if (this.timeLeft > 0f)
		{
			this.eventData.timeLeft = this.timeLeft;
			this.eventData.HandleCloseEvent(this.timeLeft);
		}
		base.xui.playerUI.entityPlayer.SetControllable(true);
		CursorControllerAbs.SetCursor(CursorControllerAbs.ECursorType.Default);
	}

	public void UpdateTimer(float _timeLeft, float _fillAmount)
	{
		this.currentTimeLeft = _timeLeft;
		this.currentFillAmount = _fillAmount;
		base.RefreshBindings(false);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.fullTime > 0f)
		{
			if (this.eventData.CloseOnHit && base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.hasBeenAttackedTime > 0)
			{
				base.xui.playerUI.windowManager.Close("timer");
				return;
			}
			this.timeLeft -= _dt;
			float fillAmount = this.timeLeft / this.fullTime;
			this.UpdateTimer(this.timeLeft, fillAmount);
			if (this.eventData.alternateTime != -1f && this.fullTime - this.timeLeft > this.eventData.alternateTime)
			{
				this.eventData.timeLeft = this.timeLeft;
				this.timeLeft = 0f;
				this.fullTime = 0f;
				base.xui.playerUI.windowManager.Close("timer");
				GameManager.Instance.SetPauseWindowEffects(false);
				base.xui.dragAndDrop.InMenu = false;
				this.eventData.HandleAlternateEvent();
				return;
			}
			if (this.timeLeft <= 0f)
			{
				this.timeLeft = 0f;
				this.fullTime = 0f;
				base.xui.playerUI.windowManager.Close("timer");
				GameManager.Instance.SetPauseWindowEffects(false);
				base.xui.dragAndDrop.InMenu = false;
				this.eventData.HandleEvent();
			}
		}
	}

	public void SetTimer(float _fullTime, TimerEventData _eventData, float startTime = -1f, string _labelText = "")
	{
		this.currentOpenEventText = _labelText;
		this.fullTime = _fullTime;
		if (startTime == -1f)
		{
			this.timeLeft = this.fullTime;
		}
		else
		{
			this.timeLeft = startTime;
		}
		this.eventData = _eventData;
		this.UpdateTimer(this.timeLeft, this.timeLeft / this.fullTime);
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "timeleft")
		{
			_value = this.currentTimeLeft.ToCultureInvariantString("0.0");
			return true;
		}
		if (_bindingName == "percent")
		{
			_value = this.currentFillAmount.ToCultureInvariantString();
			return true;
		}
		if (!(_bindingName == "caption"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = (this.currentOpenEventText ?? "");
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float fullTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float timeLeft;

	public string currentOpenEventText;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentFillAmount;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentTimeLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public TimerEventData eventData;
}
