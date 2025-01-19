﻿using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ToolbeltWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("btnClearInventory1");
		if (childById != null)
		{
			childById.OnPress += this.BtnClearInventory1_OnPress;
		}
		childById = base.GetChildById("btnClearInventory2");
		if (childById != null)
		{
			childById.OnPress += this.BtnClearInventory2_OnPress;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnClearInventory1_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.entityPlayer.EmptyToolbelt(0, 10);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnClearInventory2_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.entityPlayer.EmptyToolbelt(10, 20);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.deltaTime = _dt;
		if ((DateTime.Now - this.updateTime).TotalSeconds > 0.5)
		{
			this.updateTime = DateTime.Now;
		}
		base.RefreshBindings(false);
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		base.ViewComponent.IsVisible = ((!(this.localPlayer.AttachedToEntity != null) || !(this.localPlayer.AttachedToEntity is EntityVehicle)) && !this.localPlayer.IsDead() && (windowManager.IsHUDEnabled() || (base.xui.dragAndDrop.InMenu && windowManager.IsHUDPartialHidden())));
		if (this.CustomAttributes.ContainsKey("standard_xp_color"))
		{
			this.standardXPColor = this.CustomAttributes["standard_xp_color"];
		}
		else
		{
			this.standardXPColor = "128,4,128";
		}
		if (this.CustomAttributes.ContainsKey("updating_xp_color"))
		{
			this.updatingXPColor = this.CustomAttributes["updating_xp_color"];
		}
		else
		{
			this.updatingXPColor = "128,4,128";
		}
		if (this.CustomAttributes.ContainsKey("deficit_xp_color"))
		{
			this.expDeficitColor = this.CustomAttributes["deficit_xp_color"];
		}
		else
		{
			this.expDeficitColor = "222,20,20";
		}
		if (this.CustomAttributes.ContainsKey("xp_fill_speed"))
		{
			this.xpFillSpeed = StringParsers.ParseFloat(this.CustomAttributes["xp_fill_speed"], 0, -1, NumberStyles.Any);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.localPlayer == null)
		{
			this.localPlayer = base.xui.playerUI.entityPlayer;
		}
		this.currentValue = (this.lastValue = XUiM_Player.GetLevelPercent(this.localPlayer));
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "xp")
		{
			if (this.localPlayer != null)
			{
				if (this.localPlayer.Progression.ExpDeficit > 0)
				{
					float v = Math.Max(this.lastDeficitValue, 0f) * 1.01f;
					value = this.bindingXp.Format(v);
					this.currentValue = (float)this.localPlayer.Progression.ExpDeficit / (float)this.localPlayer.Progression.GetExpForNextLevel();
					if (this.currentValue != this.lastDeficitValue)
					{
						this.lastDeficitValue = Mathf.Lerp(this.lastDeficitValue, this.currentValue, Time.deltaTime * this.xpFillSpeed);
						if (Mathf.Abs(this.currentValue - this.lastDeficitValue) < 0.005f)
						{
							this.lastDeficitValue = this.currentValue;
						}
					}
				}
				else
				{
					float v2 = Math.Max(this.lastValue, 0f) * 1.01f;
					value = this.bindingXp.Format(v2);
					this.currentValue = XUiM_Player.GetLevelPercent(this.localPlayer);
					if (this.currentValue != this.lastValue)
					{
						this.lastValue = Mathf.Lerp(this.lastValue, this.currentValue, Time.deltaTime * this.xpFillSpeed);
						if (Mathf.Abs(this.currentValue - this.lastValue) < 0.005f)
						{
							this.lastValue = this.currentValue;
						}
					}
				}
			}
			return true;
		}
		if (bindingName == "xpcolor")
		{
			if (this.localPlayer != null)
			{
				if (this.localPlayer.Progression.ExpDeficit > 0)
				{
					value = this.expDeficitColor;
				}
				else
				{
					value = ((this.currentValue == this.lastValue) ? this.standardXPColor : this.updatingXPColor);
				}
			}
			else
			{
				value = "";
			}
			return true;
		}
		if (!(bindingName == "creativewindowopen"))
		{
			return false;
		}
		value = base.xui.playerUI.windowManager.IsWindowOpen("creative").ToString();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lmpPositionAdjustment = 0.05f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastDeficitValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float deltaTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public string standardXPColor = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string updatingXPColor = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string expDeficitColor = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public float xpFillSpeed = 2.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public CachedStringFormatterFloat bindingXp = new CachedStringFormatterFloat(null);
}
