using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerCameraWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiController childByType = base.GetChildByType<XUiC_WindowNonPagingHeader>();
		if (childByType != null)
		{
			this.nonPagingHeader = (XUiC_WindowNonPagingHeader)childByType;
		}
		childByType = base.GetChildByType<XUiC_CameraWindow>();
		if (childByType != null)
		{
			this.cameraWindow = (XUiC_CameraWindow)childByType;
		}
	}

	public TileEntityPowered TileEntity
	{
		get
		{
			return this.tileEntity;
		}
		set
		{
			this.tileEntity = value;
			this.cameraWindow.TileEntity = this.tileEntity;
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
		base.OnOpen();
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		if (this.nonPagingHeader != null)
		{
			World world = GameManager.Instance.World;
			string localizedBlockName = this.tileEntity.GetChunk().GetBlock(this.tileEntity.localChunkPos).Block.GetLocalizedBlockName();
			this.nonPagingHeader.SetHeader(localizedBlockName);
		}
		base.xui.RecenterWindowGroup(this.windowGroup, false);
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].OnOpen();
		}
		this.IsDirty = true;
		base.xui.playerUI.CursorController.Locked = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		bool flag = true;
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (entityPlayer.IsDead() || Vector3.Distance(entityPlayer.position, this.TileEntity.ToWorldPos().ToVector3()) > Constants.cDigAndBuildDistance * Constants.cDigAndBuildDistance)
		{
			flag = false;
		}
		this.wasReleased = false;
		this.activeKeyDown = false;
		if (flag && base.xui.playerUI.windowManager.HasWindow(XUiC_CameraWindow.lastWindowGroup))
		{
			if (XUiC_CameraWindow.lastWindowGroup == "powerrangedtrap")
			{
				((XUiC_PowerRangedTrapWindowGroup)((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow(XUiC_CameraWindow.lastWindowGroup)).Controller).TileEntity = (TileEntityPoweredRangedTrap)this.TileEntity;
			}
			else if (XUiC_CameraWindow.lastWindowGroup == "powertrigger")
			{
				((XUiC_PowerTriggerWindowGroup)((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow(XUiC_CameraWindow.lastWindowGroup)).Controller).TileEntity = (TileEntityPoweredTrigger)this.TileEntity;
			}
			else
			{
				((XUiC_PoweredGenericWindowGroup)((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow(XUiC_CameraWindow.lastWindowGroup)).Controller).TileEntity = this.TileEntity;
			}
			base.xui.playerUI.windowManager.Open(XUiC_CameraWindow.lastWindowGroup, true, false, true);
		}
		base.xui.playerUI.CursorController.Locked = false;
	}

	public bool UseEdgeDetection = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeader;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CameraWindow cameraWindow;

	public static string ID = "powercamera";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowered tileEntity;
}
