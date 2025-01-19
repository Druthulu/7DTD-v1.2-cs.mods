using System;
using System.Collections;
using System.Globalization;
using Audio;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LootWindow : XUiController
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
		this.lootContainer = base.GetChildByType<XUiC_LootContainer>();
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
				_grid = this.lootContainer;
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
		base.RegisterForInputStyleChanges();
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
		if (this.te != null)
		{
			Vector3 vector = this.te.ToWorldCenterPos();
			if (vector != Vector3.zero)
			{
				float num = Constants.cCollectItemDistance + 30f;
				float sqrMagnitude = (base.xui.playerUI.entityPlayer.position - vector).sqrMagnitude;
				if (sqrMagnitude > num * num)
				{
					Log.Out("Loot Window closed at distance {0}", new object[]
					{
						Mathf.Sqrt(sqrMagnitude)
					});
					base.xui.playerUI.windowManager.CloseAllOpenWindows(null, false);
					this.CloseContainer(false);
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

	public void SetTileEntityChest(string _lootContainerName, ITileEntityLootable _te)
	{
		this.te = _te;
		this.lootContainerName = _lootContainerName;
		if (this.te != null)
		{
			this.containerSlotsSize = this.te.GetContainerSize();
			Vector2i vector2i = new Vector2i(this.containerSlotsSize.x * this.lootContainer.GridCellSize.x, this.containerSlotsSize.y * this.lootContainer.GridCellSize.y);
			this.windowWidth = vector2i.x + this.windowGridWidthDifference;
			this.lootContainer.SetSlots(this.te, this.te.items);
			ITileEntitySignable selfOrFeature = _te.GetSelfOrFeature<ITileEntitySignable>();
			if (selfOrFeature != null)
			{
				GeneratedTextManager.GetDisplayText(selfOrFeature.GetAuthoredText(), delegate(string containerName)
				{
					if (!string.IsNullOrEmpty(containerName))
					{
						this.lootContainerName = containerName;
					}
					base.RefreshBindings(true);
				}, true, true, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
				return;
			}
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
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		entityPlayer.MinEventContext.TileEntity = this.te;
		entityPlayer.FireEvent(MinEventTypes.onSelfCloseLootContainer, true);
	}

	public void OpenContainer()
	{
		this.lootContainer.SetSlots(this.te, this.te.items);
		base.OnOpen();
		this.te.SetUserAccessing(true);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftStickButton, "igcoLootAll", XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoInspect", XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot, 0f);
		base.RefreshBindings(true);
		this.lootContainer.SelectCursorElement(true, false);
	}

	public void CloseContainer(bool ignoreCloseSound)
	{
		if (this.te == null)
		{
			return;
		}
		GameManager instance = GameManager.Instance;
		if (!ignoreCloseSound)
		{
			LootContainer lootContainer = LootContainer.GetLootContainer(this.te.lootListName, true);
			if (lootContainer != null && lootContainer.soundClose != null)
			{
				Vector3 position = this.te.ToWorldPos().ToVector3() + Vector3.one * 0.5f;
				if (this.te.EntityId != -1 && GameManager.Instance.World != null)
				{
					Entity entity = GameManager.Instance.World.GetEntity(this.te.EntityId);
					if (entity != null)
					{
						position = entity.GetPosition();
					}
				}
				Manager.BroadcastPlayByLocalPlayer(position, lootContainer.soundClose);
			}
		}
		Vector3i blockPos = this.te.ToWorldPos();
		if (GameManager.Instance.World.GetTileEntity(this.te.GetClrIdx(), blockPos).GetSelfOrFeature<ITileEntityLootable>() == this.te)
		{
			this.te.SetModified();
		}
		this.te.SetUserAccessing(false);
		instance.TEUnlockServer(this.te.GetClrIdx(), blockPos, this.te.EntityId, true);
		this.SetTileEntityChest("", null);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.OnClose();
	}

	public PreferenceTracker GetPreferenceTrackerFromTileEntity()
	{
		ITileEntityLootable tileEntityLootable = this.te;
		if (tileEntityLootable == null)
		{
			return null;
		}
		return tileEntityLootable.preferences;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator closeInventoryLater()
	{
		this.isClosing = true;
		yield return null;
		base.xui.playerUI.windowManager.CloseIfOpen("looting");
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
			_value = ((!string.IsNullOrEmpty(this.lootContainerName)) ? this.lootContainerName : Localization.Get("xuiLoot", false));
			return true;
		}
		if (_bindingName == "take_all_tooltip")
		{
			if (base.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				_value = string.Format(Localization.Get("xuiLootTakeAllTooltip", false), "[action:permanent:Reload:emptystring:KeyboardWithAngleBrackets]");
			}
			else
			{
				_value = string.Format(Localization.Get("xuiLootTakeAllTooltip", false), base.xui.playerUI.playerInput.GUIActions.LeftStick.GetBindingString(true, PlatformManager.NativePlatform.Input.CurrentControllerInputStyle, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.Plain, false, null));
			}
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
	public ITileEntityLootable te;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_LootContainer lootContainer;

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
