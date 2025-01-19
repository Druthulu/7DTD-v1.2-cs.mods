using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PoweredGenericWindowGroup : XUiController
{
	public TileEntityPowered TileEntity
	{
		get
		{
			return this.tileEntity;
		}
		set
		{
			this.tileEntity = value;
			this.setupWindowTileEntities();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void setupWindowTileEntities()
	{
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
		base.xui.RecenterWindowGroup(this.windowGroup, false);
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].OnOpen();
		}
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.wasReleased = false;
		this.activeKeyDown = false;
		if (this.tileEntity != null && !XUiC_CameraWindow.hackyIsOpeningMaximizedWindow)
		{
			GameManager.Instance.TEUnlockServer(this.tileEntity.GetClrIdx(), this.tileEntity.ToWorldPos(), this.tileEntity.entityId, true);
			this.tileEntity = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public TileEntityPowered tileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;
}
