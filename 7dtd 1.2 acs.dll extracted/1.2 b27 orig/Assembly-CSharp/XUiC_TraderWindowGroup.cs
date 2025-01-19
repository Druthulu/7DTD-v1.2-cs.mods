using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TraderWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiController childByType = base.GetChildByType<XUiC_WindowNonPagingHeader>();
		if (childByType != null)
		{
			this.nonPagingHeader = (XUiC_WindowNonPagingHeader)childByType;
		}
		childByType = base.GetChildByType<XUiC_TraderWindow>();
		if (childByType != null)
		{
			this.TraderWindow = (XUiC_TraderWindow)childByType;
		}
		childByType = base.GetChildByType<XUiC_ServiceInfoWindow>();
		if (childByType != null)
		{
			this.ServiceInfoWindow = (XUiC_ServiceInfoWindow)childByType;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.windowGroup.isShowing)
		{
			if (!base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
			{
				this.wasReleased = true;
			}
			if (this.wasReleased)
			{
				if (base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
				{
					this.activeKeyDown = true;
				}
				if (base.xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && this.activeKeyDown)
				{
					this.activeKeyDown = false;
					if (!base.xui.playerUI.windowManager.IsInputActive())
					{
						base.xui.playerUI.windowManager.CloseAllOpenWindows(null, false);
					}
				}
			}
		}
	}

	public override bool AlwaysUpdate()
	{
		return false;
	}

	public override void OnOpen()
	{
		base.xui.Trader.TraderWindowGroup = this;
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		if (this.nonPagingHeader != null)
		{
			this.nonPagingHeader.SetHeader((base.xui.Trader.TraderTileEntity.entityId == -1) ? Localization.Get("xuiVending", false) : Localization.Get("xuiTrader", false));
		}
		base.xui.RecenterWindowGroup(this.windowGroup, false);
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].OnOpen();
		}
		if (this.ServiceInfoWindow != null)
		{
			this.ServiceInfoWindow.ViewComponent.IsVisible = false;
		}
		if (base.xui.Trader.TraderTileEntity.entityId != -1 && base.xui.playerUI.entityPlayer.OverrideFOV != 30f)
		{
			base.xui.playerUI.entityPlayer.OverrideFOV = 30f;
			base.xui.playerUI.entityPlayer.OverrideLookAt = base.xui.Trader.TraderEntity.getHeadPosition();
		}
		base.xui.Dialog.keepZoomOnClose = false;
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.wasReleased = false;
		this.activeKeyDown = false;
		base.xui.Trader.TraderWindowGroup = null;
		base.xui.playerUI.entityPlayer.OverrideFOV = -1f;
	}

	public void RefreshTraderItems()
	{
		this.TraderWindow.CompletedTransaction = true;
		this.TraderWindow.RefreshTraderItems();
	}

	public void RefreshTraderWindow()
	{
		this.TraderWindow.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeader;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TraderWindow TraderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServiceInfoWindow ServiceInfoWindow;

	public static string ID = "trader";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;
}
