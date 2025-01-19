using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BackpackWindow : XUiController
{
	public bool UserLockMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.userLockMode;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value == this.userLockMode)
			{
				return;
			}
			if (this.userLockMode)
			{
				this.UpdateLockedSlots(this.standardControls);
			}
			XUiC_ContainerStandardControls xuiC_ContainerStandardControls = this.standardControls;
			if (xuiC_ContainerStandardControls != null)
			{
				xuiC_ContainerStandardControls.LockModeChanged(value);
			}
			this.userLockMode = value;
			base.WindowGroup.isEscClosable = !this.userLockMode;
			base.xui.playerUI.windowManager.GetModalWindow().isEscClosable = !this.userLockMode;
			base.RefreshBindings(false);
		}
	}

	public override void Init()
	{
		base.Init();
		this.backpackGrid = base.GetChildByType<XUiC_Backpack>();
		this.standardControls = base.GetChildByType<XUiC_ContainerStandardControls>();
		if (this.standardControls != null)
		{
			this.standardControls.ApplyLockedSlotStates = new Action<bool[]>(this.ApplyLockedSlotStates);
			this.standardControls.UpdateLockedSlotStates = new Action<XUiC_ContainerStandardControls>(this.UpdateLockedSlots);
			this.standardControls.SortPressed = new Action<bool[]>(this.BtnSort_OnPress);
			this.standardControls.MoveAllowed = delegate(out XUiController _parentWindow, out XUiC_ItemStackGrid _grid, out IInventory _inventory)
			{
				_parentWindow = this;
				_grid = this.backpackGrid;
				return this.TryGetMoveDestinationInventory(out _inventory);
			};
			this.standardControls.LockModeToggled = delegate()
			{
				this.UserLockMode = !this.UserLockMode;
			};
		}
		XUiController childById = base.GetChildById("btnClearInventory");
		if (childById != null)
		{
			childById.OnPress += this.BtnClearInventory_OnPress;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetMoveDestinationInventory(out IInventory _dstInventory)
	{
		_dstInventory = null;
		XUiM_AssembleItem assembleItem = base.xui.AssembleItem;
		if (((assembleItem != null) ? assembleItem.CurrentItem : null) != null)
		{
			return false;
		}
		bool flag = base.xui.vehicle != null && base.xui.vehicle.GetVehicle().HasStorage();
		bool flag2 = base.xui.lootContainer != null && base.xui.lootContainer.EntityId == -1;
		bool flag3 = base.xui.lootContainer != null && GameManager.Instance.World.GetEntity(base.xui.lootContainer.EntityId) is EntityDrone;
		if (!flag && !flag2 && !flag3)
		{
			return false;
		}
		if (flag && base.xui.FindWindowGroupByName(XUiC_VehicleStorageWindowGroup.ID).GetChildByType<XUiC_VehicleContainer>() == null)
		{
			return false;
		}
		if (flag3)
		{
			_dstInventory = base.xui.lootContainer;
		}
		else
		{
			IInventory inventory2;
			if (!flag2)
			{
				IInventory inventory = base.xui.vehicle.bag;
				inventory2 = inventory;
			}
			else
			{
				IInventory inventory = base.xui.lootContainer;
				inventory2 = inventory;
			}
			_dstInventory = inventory2;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnClearInventory_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.entityPlayer.EmptyBackpack();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSort_OnPress(bool[] _ignoredSlots)
	{
		ItemStack itemStack = null;
		if (base.xui.AssembleItem.CurrentItemStackController != null)
		{
			itemStack = base.xui.AssembleItem.CurrentItemStackController.ItemStack;
		}
		base.xui.PlayerInventory.SortStacks(0, _ignoredSlots);
		if (itemStack != null)
		{
			base.GetChildByType<XUiC_ItemStackGrid>().AssembleLockSingleStack(itemStack);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyLockedSlotStates(bool[] _lockedSlots)
	{
		XUiC_ItemStack[] itemStackControllers = this.backpackGrid.GetItemStackControllers();
		for (int i = 0; i < itemStackControllers.Length; i++)
		{
			itemStackControllers[i].UserLockedSlot = (_lockedSlots != null && i < _lockedSlots.Length && _lockedSlots[i]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLockedSlots(XUiC_ContainerStandardControls _csc)
	{
		if (_csc == null)
		{
			return;
		}
		int slotCount = base.xui.PlayerInventory.Backpack.SlotCount;
		bool[] array = _csc.LockedSlots ?? new bool[slotCount];
		if (array.Length < slotCount)
		{
			bool[] array2 = new bool[slotCount];
			Array.Copy(array, array2, array.Length);
			array = array2;
		}
		XUiC_ItemStack[] itemStackControllers = this.backpackGrid.GetItemStackControllers();
		int num = 0;
		while (num < itemStackControllers.Length && num < array.Length)
		{
			array[num] = itemStackControllers[num].UserLockedSlot;
			num++;
		}
		_csc.LockedSlots = array;
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "currencyamount")
		{
			value = "0";
			if (XUi.IsGameRunning() && base.xui != null && base.xui.PlayerInventory != null)
			{
				value = this.currencyFormatter.Format(base.xui.PlayerInventory.CurrencyAmount);
			}
			return true;
		}
		if (bindingName == "currencyicon")
		{
			value = TraderInfo.CurrencyItem;
			return true;
		}
		if (bindingName == "lootingorvehiclestorage")
		{
			bool flag = base.xui.vehicle != null && base.xui.vehicle.GetVehicle().HasStorage();
			bool flag2 = base.xui.lootContainer != null && base.xui.lootContainer.EntityId == -1;
			bool flag3 = base.xui.lootContainer != null && GameManager.Instance.World.GetEntity(base.xui.lootContainer.EntityId) is EntityDrone;
			value = (flag || flag2 || flag3).ToString();
			return true;
		}
		if (bindingName == "creativewindowopen")
		{
			value = base.xui.playerUI.windowManager.IsWindowOpen("creative").ToString();
			return true;
		}
		if (!(bindingName == "userlockmode"))
		{
			return false;
		}
		value = this.UserLockMode.ToString();
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.localPlayer == null)
		{
			this.localPlayer = base.xui.playerUI.entityPlayer;
		}
		base.xui.PlayerInventory.RefreshCurrency();
		base.xui.PlayerInventory.OnCurrencyChanged += this.PlayerInventory_OnCurrencyChanged;
		base.RefreshBindings(false);
		if (!string.IsNullOrEmpty(XUiC_BackpackWindow.defaultSelectedElement))
		{
			base.GetChildById(XUiC_BackpackWindow.defaultSelectedElement).SelectCursorElement(true, false);
			XUiC_BackpackWindow.defaultSelectedElement = "";
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.UserLockMode = false;
		if (base.xui != null && base.xui.PlayerInventory != null)
		{
			base.xui.PlayerInventory.OnCurrencyChanged -= this.PlayerInventory_OnCurrencyChanged;
		}
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		PlayerActionsLocal playerInput = base.xui.playerUI.playerInput;
		if (this.UserLockMode && (playerInput.GUIActions.Cancel.WasPressed || playerInput.PermanentActions.Cancel.WasPressed))
		{
			this.UserLockMode = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnCurrencyChanged()
	{
		base.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Backpack backpackGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHidden;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool userLockMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ContainerStandardControls standardControls;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt currencyFormatter = new CachedStringFormatterInt();

	public static string defaultSelectedElement;
}
