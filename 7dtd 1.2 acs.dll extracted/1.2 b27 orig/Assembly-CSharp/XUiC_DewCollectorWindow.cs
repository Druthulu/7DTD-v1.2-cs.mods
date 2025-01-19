﻿using System;
using System.Collections;
using System.Globalization;
using Audio;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DewCollectorWindow : XUiController
{
	public int ContainerSlotsCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.containerSlotsSize.x * this.containerSlotsSize.y;
		}
	}

	public override void Init()
	{
		base.Init();
		this.container = base.GetChildByType<XUiC_DewCollectorContainer>();
		this.controls = base.GetChildByType<XUiC_ContainerStandardControls>();
		if (this.controls != null)
		{
			this.controls.SortPressed = delegate(bool[] _ignoredSlots)
			{
				ItemStack[] array = StackSortUtil.CombineAndSortStacks(this.te.items, 0, _ignoredSlots);
				for (int i = 0; i < array.Length; i++)
				{
					this.te.UpdateSlot(i, array[i]);
				}
				this.te.SetModified();
			};
			this.controls.MoveAllowed = delegate(out XUiController _parentWindow, out XUiC_ItemStackGrid _grid, out IInventory _inventory)
			{
				_parentWindow = this;
				_grid = this.container;
				_inventory = base.xui.PlayerInventory;
				return true;
			};
			this.controls.MoveAllDone = delegate(bool _allMoved, bool _anyMoved)
			{
				if (_anyMoved)
				{
					Manager.BroadcastPlayByLocalPlayer(this.te.ToWorldPos().ToVector3() + Vector3.one * 0.5f, "UseActions/takeall1");
				}
				if (_allMoved)
				{
					ThreadManager.StartCoroutine(this.closeInventoryLater());
				}
			};
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
					base.xui.playerUI.windowManager.CloseAllOpenWindows(null, false);
				}
			}
		}
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		if (!this.isClosing && base.ViewComponent != null && base.ViewComponent.IsVisible && (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard || !base.xui.playerUI.windowManager.IsInputActive()) && (base.xui.playerUI.playerInput.GUIActions.LeftStick.WasPressed || base.xui.playerUI.playerInput.PermanentActions.Reload.WasPressed))
		{
			this.controls.MoveAll();
		}
	}

	public void SetTileEntity(TileEntityDewCollector _te)
	{
		this.te = _te;
		if (this.te != null)
		{
			this.containerSlotsSize = this.te.GetContainerSize();
			Vector2i vector2i = new Vector2i(this.containerSlotsSize.x * this.container.GridCellSize.x, this.containerSlotsSize.y * this.container.GridCellSize.y);
			this.windowWidth = vector2i.x + this.windowGridWidthDifference;
			this.te.HandleUpdate(GameManager.Instance.World);
			this.container.SetSlots(this.te, this.te.GetItems());
			base.RefreshBindings(true);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.isClosing = false;
	}

	public override void OnClose()
	{
		this.wasReleased = false;
		this.activeKeyDown = false;
		GameManager instance = GameManager.Instance;
		Vector3i blockPos = this.te.ToWorldPos();
		this.te.SetUserAccessing(false);
		this.te.SetModified();
		instance.TEUnlockServer(this.te.GetClrIdx(), blockPos, this.te.entityId, true);
		this.SetTileEntity(null);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.OnClose();
	}

	public void OpenContainer()
	{
		this.container.SetSlots(this.te, this.te.GetItems());
		base.OnOpen();
		this.te.SetUserAccessing(true);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftStickButton, "igcoLootAll", XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoInspect", XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot, 0f);
		base.RefreshBindings(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator closeInventoryLater()
	{
		this.isClosing = true;
		yield return null;
		base.xui.playerUI.windowManager.CloseIfOpen("dewcollector");
		this.isClosing = false;
		yield break;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "window_grid_width_difference")
		{
			this.windowGridWidthDifference = StringParsers.ParseSInt32(_value, 0, -1, NumberStyles.Integer);
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "windowWidth")
		{
			_value = this.windowWidth.ToString();
			return true;
		}
		if (_bindingName == "lootcontainer_name")
		{
			_value = Localization.Get("xuiDewCollector", false);
			return true;
		}
		if (_bindingName == "take_all_tooltip")
		{
			_value = string.Format(Localization.Get("xuiLootTakeAllTooltip", false), "[action:permanent:Reload:emptystring:KeyboardWithAngleBrackets]");
			return true;
		}
		if (_bindingName == "buttons_visible")
		{
			_value = (this.windowWidth >= 450).ToString();
			return true;
		}
		if (!(_bindingName == "container_slots"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = this.ContainerSlotsCount.ToString();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityDewCollector te;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DewCollectorContainer container;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ContainerStandardControls controls;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lootContainerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSlotsSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int windowWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int windowGridWidthDifference;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClosing;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;
}
