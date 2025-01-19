using System;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DeathBar : XUiController
{
	public EntityAlive Target { get; set; }

	public override void Init()
	{
		base.Init();
		this.IsDirty = true;
		this.viewComponent.IsVisible = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.LocalPlayer == null && base.xui != null && base.xui.playerUI != null && base.xui.playerUI.entityPlayer != null)
		{
			this.LocalPlayer = base.xui.playerUI.entityPlayer;
		}
		if (this.LocalPlayer == null)
		{
			return;
		}
		if (this.LocalPlayer.IsAlive())
		{
			this.viewComponent.IsVisible = false;
			return;
		}
		if (this.deathText != TwitchManager.DeathText)
		{
			this.deathText = TwitchManager.DeathText;
			base.RefreshBindings(true);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.RefreshBindings(true);
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (!(_bindingName == "death_text"))
		{
			if (!(_bindingName == "visible"))
			{
				return false;
			}
			if (this.LocalPlayer == null)
			{
				_value = "false";
				return true;
			}
			if (this.LocalPlayer.IsAlive())
			{
				_value = "false";
				return true;
			}
			if (TwitchManager.DeathText == "")
			{
				_value = "false";
				return true;
			}
			_value = "true";
			return true;
		}
		else
		{
			if (this.LocalPlayer == null)
			{
				_value = "";
				return true;
			}
			_value = this.deathText;
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string deathText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal LocalPlayer;
}
