using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PopupMenu : XUiController
{
	public int MenuWidth { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public void SetupItems(List<MenuItemEntry> newMenuItems, Vector2i offsetPosition, XUiView _originView)
	{
		this.menuItems.Clear();
		this.MenuWidth = 0;
		this.IsOver = false;
		this.menuItems = newMenuItems;
		this.xuiPosition = base.xui.GetMouseXUIPosition();
		this.offset = offsetPosition;
		this.originView = _originView;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPosition()
	{
		if (this.gridView == null)
		{
			return;
		}
		Vector2i size = this.gridView.Size;
		Vector2i vector2i = base.xui.GetXUiScreenSize() / 2;
		Vector2i vector2i2 = new Vector2i((int)((double)vector2i.x * 0.97), (int)((double)vector2i.y * 0.97));
		Vector2i vector2i3 = this.xuiPosition + this.offset;
		Vector2i vector2i4 = vector2i3;
		Vector2i vector2i5 = vector2i4 + size;
		if (vector2i5.x >= vector2i2.x)
		{
			vector2i3.x = vector2i2.x - size.x;
		}
		else if (vector2i4.x <= -vector2i2.x)
		{
			vector2i3.x = -vector2i2.x;
		}
		if (vector2i5.y >= vector2i2.y)
		{
			vector2i3.y = vector2i2.y - size.y;
		}
		else if (vector2i4.y <= -vector2i2.y)
		{
			vector2i3.y = -vector2i2.y;
		}
		base.ViewComponent.Position = vector2i3;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetWidth(int newWidth)
	{
		if (newWidth > this.MenuWidth)
		{
			this.MenuWidth = newWidth;
		}
	}

	public void ClearItems()
	{
		this.menuItems.Clear();
		this.IsDirty = true;
	}

	public override void Init()
	{
		base.Init();
		base.xui.currentPopupMenu = this;
		this.grid = base.GetChildById("list");
		this.sprBackgroundBorder = base.GetChildById("sprBackgroundBorder");
		this.sprBackground = base.GetChildById("sprBackground");
		base.ViewComponent.IsVisible = false;
		this.gridView = (this.grid.ViewComponent as XUiV_Grid);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.setVisiblePending && Time.time - this.setVisibleTime > 0.1f)
		{
			this.setVisiblePending = false;
			base.ViewComponent.IsVisible = true;
			base.xui.playerUI.CursorController.SetNavigationLockView(this.viewComponent, this.children[0].ViewComponent);
		}
		if (this.menuItems.Count > 0 && !this.IsDirty && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && (base.xui.playerUI.CursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton) | base.xui.playerUI.playerInput.GUIActions.RightStick.WasReleased) && !this.IsOver)
		{
			this.ClearItems();
		}
		if (this.IsDirty)
		{
			if (this.menuItems.Count == 0)
			{
				this.IsDirty = false;
				base.ViewComponent.IsVisible = false;
				if (base.xui.playerUI.CursorController.navigationTarget != null && base.xui.playerUI.CursorController.navigationTarget.Controller.IsChildOf(this))
				{
					base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
					base.xui.playerUI.CursorController.SetNavigationTarget(this.originView);
				}
				return;
			}
			XUiC_PopupMenuItem[] childrenByType = this.grid.GetChildrenByType<XUiC_PopupMenuItem>(null);
			for (int i = 0; i < childrenByType.Length; i++)
			{
				XUiC_PopupMenuItem xuiC_PopupMenuItem = childrenByType[i];
				if (i < this.menuItems.Count)
				{
					xuiC_PopupMenuItem.ItemEntry = this.menuItems[i];
					xuiC_PopupMenuItem.ViewComponent.IsVisible = true;
				}
				else
				{
					xuiC_PopupMenuItem.ItemEntry = null;
					xuiC_PopupMenuItem.ViewComponent.IsVisible = false;
				}
			}
			for (int j = 0; j < childrenByType.Length; j++)
			{
				childrenByType[j].SetWidth(this.MenuWidth + 60);
			}
			XUiView viewComponent = this.sprBackground.ViewComponent;
			XUiView xuiView = this.gridView;
			Vector2i size = new Vector2i(this.MenuWidth + 60, this.menuItems.Count * 43);
			xuiView.Size = size;
			viewComponent.Size = size;
			XUiView viewComponent2 = this.sprBackgroundBorder.ViewComponent;
			XUiView viewComponent3 = base.ViewComponent;
			size = new Vector2i(this.MenuWidth + 60 + 6, this.menuItems.Count * 43 + 6);
			viewComponent3.Size = size;
			viewComponent2.Size = size;
			this.SetPosition();
			this.IsDirty = false;
			base.ViewComponent.IsVisible = true;
			base.xui.playerUI.CursorController.SetNavigationLockView(this.viewComponent, childrenByType[0].ViewComponent);
		}
	}

	public override void OnVisibilityChanged(bool _isVisible)
	{
		base.OnVisibilityChanged(_isVisible);
		if (_isVisible)
		{
			base.ViewComponent.TryUpdatePosition();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<MenuItemEntry> menuItems = new List<MenuItemEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i xuiPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i offset;

	public bool IsOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController grid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController sprBackgroundBorder;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController sprBackground;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid gridView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView originView;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setVisiblePending;

	[PublicizedFrom(EAccessModifier.Private)]
	public float setVisibleTime;
}
