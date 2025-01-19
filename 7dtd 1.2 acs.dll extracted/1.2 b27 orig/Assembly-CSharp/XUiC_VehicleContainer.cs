using System;
using System.Collections;
using System.Globalization;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_VehicleContainer : XUiC_ItemStackGrid
{
	public int containerSlotsCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.containerSlotsSize.x * this.containerSlotsSize.y;
		}
	}

	public Vector2i GridCellSize { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Vehicle;
		}
	}

	public override ItemStack[] GetSlots()
	{
		return this.items;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetStacks(ItemStack[] stackList)
	{
	}

	public override void Init()
	{
		base.Init();
		this.window = (XUiV_Window)this.viewComponent;
		this.grid = (XUiV_Grid)base.GetChildById("queue").ViewComponent;
		this.GridCellSize = new Vector2i(this.grid.CellWidth, this.grid.CellHeight);
		this.controls = base.GetChildByType<XUiC_ContainerStandardControls>();
		if (this.controls != null)
		{
			this.controls.SortPressed = new Action<bool[]>(this.btnSort_OnPress);
			this.controls.MoveAllowed = delegate(out XUiController _parentWindow, out XUiC_ItemStackGrid _grid, out IInventory _inventory)
			{
				_parentWindow = this;
				_grid = this;
				_inventory = base.xui.PlayerInventory;
				return true;
			};
			this.controls.MoveAllDone = delegate(bool _allMoved, bool _anyMoved)
			{
				if (_anyMoved)
				{
					Manager.BroadcastPlayByLocalPlayer(this.currentVehicleEntity.position + Vector3.one * 0.5f, "UseActions/takeall1");
				}
				if (_allMoved)
				{
					ThreadManager.StartCoroutine(this.closeInventoryLater());
				}
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnSort_OnPress(bool[] _ignoredSlos)
	{
		if (base.xui.vehicle.GetVehicle() == null)
		{
			return;
		}
		ItemStack[] slots = StackSortUtil.CombineAndSortStacks(base.xui.vehicle.bag.GetSlots(), 0, _ignoredSlos);
		base.xui.vehicle.bag.SetSlots(slots);
	}

	public void SetSlots(ItemStack[] stackList)
	{
		if (stackList == null)
		{
			return;
		}
		if (base.xui.vehicle.GetVehicle() == null)
		{
			return;
		}
		this.currentVehicleEntity = base.xui.vehicle;
		this.containerSlotsSize = this.currentVehicleEntity.lootContainer.GetContainerSize();
		Vector2i vector2i = new Vector2i(this.containerSlotsSize.x * this.GridCellSize.x, this.containerSlotsSize.y * this.GridCellSize.y);
		this.windowWidth = vector2i.x + this.windowGridWidthDifference;
		base.xui.vehicle.bag.OnBackpackItemsChangedInternal += this.OnBagItemChangedInternal;
		this.items = stackList;
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		this.grid.Columns = this.containerSlotsSize.x;
		this.grid.Rows = this.containerSlotsSize.y;
		int num = stackList.Length;
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_ItemStack xuiC_ItemStack = this.itemControllers[i];
			xuiC_ItemStack.SlotNumber = i;
			xuiC_ItemStack.SlotChangedEvent -= this.HandleLootSlotChangedEvent;
			xuiC_ItemStack.InfoWindow = childByType;
			xuiC_ItemStack.StackLocation = XUiC_ItemStack.StackLocationTypes.LootContainer;
			xuiC_ItemStack.UnlockStack();
			if (i < num)
			{
				xuiC_ItemStack.ForceSetItemStack(this.items[i]);
				this.itemControllers[i].ViewComponent.IsVisible = true;
				xuiC_ItemStack.SlotChangedEvent += this.HandleLootSlotChangedEvent;
			}
			else
			{
				xuiC_ItemStack.ItemStack = ItemStack.Empty.Clone();
				this.itemControllers[i].ViewComponent.IsVisible = false;
			}
		}
		base.RefreshBindings(true);
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		if (this.IsDirty)
		{
			this.hasStorage = base.xui.vehicle.GetVehicle().HasStorage();
			base.ViewComponent.IsVisible = this.hasStorage;
			this.IsDirty = false;
		}
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
					this.OnClose();
					base.xui.playerUI.windowManager.CloseAllOpenWindows(null, false);
				}
			}
			if (!this.isClosing && base.ViewComponent != null && base.ViewComponent.IsVisible && this.items != null && (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard || !base.xui.playerUI.windowManager.IsInputActive()) && (base.xui.playerUI.playerInput.GUIActions.LeftStick.WasPressed || base.xui.playerUI.playerInput.PermanentActions.Reload.WasPressed))
			{
				this.controls.MoveAll();
			}
		}
	}

	public void HandleLootSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		if (base.xui.vehicle == null)
		{
			return;
		}
		base.xui.vehicle.bag.SetSlot(slotNumber, stack, true);
	}

	public void OnBagItemChangedInternal()
	{
		if (base.xui.vehicle == null)
		{
			return;
		}
		ItemStack[] slots = base.xui.vehicle.bag.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			this.SetItemInSlot(i, slots[i]);
		}
		base.xui.vehicle.SetBagModified();
	}

	public void SetItemInSlot(int i, ItemStack stack)
	{
		if (i >= this.itemControllers.Length)
		{
			return;
		}
		this.itemControllers[i].ItemStack = stack;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.window.TargetAlpha = 1f;
		base.ViewComponent.OnOpen();
		base.ViewComponent.IsVisible = true;
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.currentVehicleEntity = null;
		if (base.xui.vehicle == null)
		{
			return;
		}
		base.xui.vehicle.bag.OnBackpackItemsChangedInternal -= this.OnBagItemChangedInternal;
		this.window.TargetAlpha = 0f;
		base.xui.vehicle = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator closeInventoryLater()
	{
		yield return null;
		base.xui.playerUI.windowManager.CloseIfOpen("vehicleStorage");
		this.isClosing = false;
		yield break;
	}

	public bool AddItem(ItemStack itemStack)
	{
		base.xui.vehicle.bag.TryStackItem(0, itemStack);
		return itemStack.count > 0 && base.xui.vehicle.bag.AddItem(itemStack);
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
		_value = this.containerSlotsCount.ToString();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClosing;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasStorage;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Window window;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid grid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ContainerStandardControls controls;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSlotsSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityVehicle currentVehicleEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public int windowWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int windowGridWidthDifference;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;
}
