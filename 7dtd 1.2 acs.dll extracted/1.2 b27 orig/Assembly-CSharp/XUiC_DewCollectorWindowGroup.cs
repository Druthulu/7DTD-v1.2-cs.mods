using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DewCollectorWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		this.dewCatcherWindow = base.GetChildByType<XUiC_DewCollectorWindow>();
		this.dewCollectorModGrid = base.GetChildByType<XUiC_DewCollectorModGrid>();
		this.nonPagingHeaderWindow = base.GetChildByType<XUiC_WindowNonPagingHeader>();
	}

	public void SetTileEntity(TileEntityDewCollector _te)
	{
		this.te = _te;
		this.dewCatcherWindow.SetTileEntity(_te);
		this.dewCollectorModGrid.SetTileEntity(_te);
		this.lootingHeader = Localization.Get("xuiDewCollector", false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OpenContainer()
	{
		base.OnOpen();
		base.xui.playerUI.windowManager.OpenIfNotOpen("backpack", false, false, true);
		this.dewCatcherWindow.ViewComponent.UiTransform.gameObject.SetActive(true);
		this.dewCatcherWindow.OpenContainer();
		if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader(this.lootingHeader);
		}
		this.dewCatcherWindow.ViewComponent.IsVisible = true;
		if (this.windowGroup.UseStackPanelAlignment)
		{
			base.xui.RecenterWindowGroup(this.windowGroup, false);
		}
	}

	public override void OnOpen()
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader("LOOTING");
		}
		this.OpenContainer();
		BlockDewCollector blockDewCollector = this.te.blockValue.Block as BlockDewCollector;
		if (blockDewCollector != null)
		{
			string openSound = blockDewCollector.OpenSound;
			Manager.BroadcastPlayByLocalPlayer(this.te.ToWorldPos().ToVector3() + Vector3.one * 0.5f, openSound);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen("backpack");
		this.te.ToWorldPos();
		if (this.te.blockValue.Block != null)
		{
			BlockDewCollector blockDewCollector = this.te.blockValue.Block as BlockDewCollector;
			if (blockDewCollector != null)
			{
				string closeSound = blockDewCollector.CloseSound;
				Manager.BroadcastPlayByLocalPlayer(this.te.ToWorldPos().ToVector3() + Vector3.one * 0.5f, closeSound);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_DewCollectorWindowGroup GetInstance(XUi _xuiInstance = null)
	{
		if (_xuiInstance == null)
		{
			_xuiInstance = LocalPlayerUI.GetUIForPrimaryPlayer().xui;
		}
		return (XUiC_DewCollectorWindowGroup)_xuiInstance.FindWindowGroupByName(XUiC_DewCollectorWindowGroup.ID);
	}

	public static Vector3i GetTeBlockPos(XUi _xuiInstance = null)
	{
		TileEntityDewCollector tileEntityDewCollector = XUiC_DewCollectorWindowGroup.GetInstance(_xuiInstance).te;
		if (tileEntityDewCollector == null)
		{
			return Vector3i.zero;
		}
		return tileEntityDewCollector.ToWorldPos();
	}

	public static void CloseIfOpenAtPos(Vector3i _blockPos, XUi _xuiInstance = null)
	{
		GUIWindowManager windowManager = XUiC_DewCollectorWindowGroup.GetInstance(_xuiInstance).xui.playerUI.windowManager;
		if (windowManager.IsWindowOpen(XUiC_DewCollectorWindowGroup.ID) && XUiC_DewCollectorWindowGroup.GetTeBlockPos(null) == _blockPos)
		{
			windowManager.Close(XUiC_DewCollectorWindowGroup.ID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DewCollectorWindow dewCatcherWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DewCollectorModGrid dewCollectorModGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label headerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityDewCollector te;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lootingHeader;

	public static string ID = "dewcollector";

	[PublicizedFrom(EAccessModifier.Private)]
	public float totalOpenTime;
}
