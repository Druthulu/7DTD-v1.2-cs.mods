using System;
using GUI_2;
using InControl;
using Platform;
using UnityEngine;

public class XUi_FallThrough : MonoBehaviour
{
	public void SetXUi(XUi _xui)
	{
		this.xui = _xui;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		UICamera.fallThrough = base.gameObject;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		bool flag = this.xui.dragAndDrop != null && !this.xui.dragAndDrop.CurrentStack.IsEmpty();
		bool flag2 = flag && UICamera.hoveredObject != null && UICamera.hoveredObject.name.EqualsCaseInsensitive("xui") && this.xui.dragAndDrop.CurrentStack.itemValue.ItemClassOrMissing.CanDrop(null);
		int num = (this.xui.dragAndDrop == null) ? 0 : this.xui.dragAndDrop.CurrentStack.count;
		bool flag3 = false;
		LocalPlayerUI playerUI = this.xui.playerUI;
		if (null != playerUI && null != playerUI.uiCamera && playerUI.playerInput != null && playerUI.playerInput.GUIActions != null)
		{
			PlayerActionsGUI guiactions = playerUI.playerInput.GUIActions;
			bool flag4 = false;
			bool flag5 = false;
			if (guiactions.LastInputType == BindingSourceType.DeviceBindingSource)
			{
				flag4 |= guiactions.Submit.WasReleased;
				flag5 |= guiactions.HalfStack.WasReleased;
			}
			else
			{
				flag4 |= playerUI.CursorController.GetMouseButtonDown(UICamera.MouseButton.LeftButton);
				flag5 |= playerUI.CursorController.GetMouseButtonDown(UICamera.MouseButton.RightButton);
			}
			if (flag2)
			{
				if (flag4 || (num == 1 && flag5))
				{
					this.xui.dragAndDrop.DropCurrentItem();
					flag3 = true;
				}
				else if (num > 1 && flag5)
				{
					this.xui.dragAndDrop.DropCurrentItem(1);
					num--;
					flag3 = true;
				}
			}
			if ((flag4 || flag5) && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && this.xui.currentPopupMenu != null && !this.xui.currentPopupMenu.IsOver)
			{
				this.xui.currentPopupMenu.ClearItems();
			}
		}
		if (flag2 != this.canDrop || flag3)
		{
			this.canDrop = flag2;
			this.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
			if (flag && this.canDrop)
			{
				if (num > 1)
				{
					this.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoDropAll", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
					this.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoDropOne", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
				}
				else
				{
					this.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoDrop", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
				}
				this.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
				this.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir, 0f);
				return;
			}
			this.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
			this.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem, 0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public XUi xui;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool canDrop;
}
