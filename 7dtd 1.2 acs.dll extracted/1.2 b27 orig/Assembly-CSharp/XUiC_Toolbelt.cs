using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Toolbelt : XUiC_ItemStackGrid
{
	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.ToolBelt;
		}
	}

	public bool HasSecondRow
	{
		get
		{
			return this.itemControllers != null && this.backendSlotCount > this.itemControllers.Length / 2;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_Toolbelt.ID = base.WindowGroup.ID;
	}

	public override ItemStack[] GetSlots()
	{
		return base.xui.PlayerInventory.GetToolbeltItemStacks();
	}

	public override void Update(float _dt)
	{
		if (!XUi.IsGameRunning())
		{
			return;
		}
		if (this.openLater)
		{
			this.OnOpen();
			this.openLater = false;
		}
		Inventory toolbelt = base.xui.PlayerInventory.Toolbelt;
		if (this.currentHoldingIndex != toolbelt.GetFocusedItemIdx())
		{
			if (this.currentHoldingIndex != toolbelt.DUMMY_SLOT_IDX)
			{
				this.itemControllers[this.currentHoldingIndex].IsHolding = false;
			}
			this.currentHoldingIndex = toolbelt.GetFocusedItemIdx();
			if (this.currentHoldingIndex != toolbelt.DUMMY_SLOT_IDX)
			{
				this.itemControllers[this.currentHoldingIndex].IsHolding = true;
			}
		}
		if (Time.time > this.updateTime)
		{
			this.updateTime = Time.time + 0.5f;
			bool flag = toolbelt.IsHoldingItemActionRunning();
			if (this.lastActionRunning != flag)
			{
				this.currentHoldingIndex = toolbelt.holdingItemIdx;
				if (this.currentHoldingIndex != toolbelt.DUMMY_SLOT_IDX)
				{
					if (flag)
					{
						this.lastActionSlot = toolbelt.holdingItemIdx;
						this.itemControllers[toolbelt.holdingItemIdx].HiddenLock = true;
					}
					else
					{
						this.itemControllers[this.lastActionSlot].HiddenLock = flag;
					}
				}
				this.lastActionRunning = flag;
			}
			if (!GameManager.Instance.bCursorVisible)
			{
				this.ClearHoveredItems();
			}
		}
		if (this.IsDirty)
		{
			this.UpdateQuickSwap();
			base.RefreshBindings(false);
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.xui.PlayerInventory.SetToolbeltItemStacks(stackList);
	}

	public override bool AlwaysUpdate()
	{
		return this.openLater;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateQuickSwap()
	{
		int quickSwapSlot = base.xui.PlayerInventory.QuickSwapSlot;
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			this.itemControllers[i].isQuickSwap = (i == quickSwapSlot);
		}
	}

	public override void OnOpen()
	{
		if (!XUi.IsGameRunning())
		{
			this.openLater = true;
			return;
		}
		base.OnOpen();
		base.xui.PlayerInventory.OnToolbeltItemsChanged += this.PlayerInventory_OnToolbeltItemsChanged;
		this.PlayerInventory_OnToolbeltItemsChanged();
		this.currentHoldingIndex = base.xui.PlayerInventory.Toolbelt.holdingItemIdx;
		if (this.currentHoldingIndex != base.xui.PlayerInventory.Toolbelt.DUMMY_SLOT_IDX)
		{
			this.itemControllers[this.currentHoldingIndex].IsHolding = true;
		}
		base.xui.playerUI.windowManager.OpenIfNotOpen("dragAndDrop", false, false, true);
		if (this.backendSlotCount < 0)
		{
			this.backendSlotCount = base.xui.PlayerInventory.Toolbelt.PUBLIC_SLOTS;
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		this.SetStacks(this.GetSlots());
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.PlayerInventory.OnToolbeltItemsChanged -= this.PlayerInventory_OnToolbeltItemsChanged;
		base.xui.playerUI.windowManager.CloseIfOpen("dragAndDrop");
		if (this.currentHoldingIndex != base.xui.PlayerInventory.Toolbelt.DUMMY_SLOT_IDX)
		{
			this.itemControllers[this.currentHoldingIndex].IsHolding = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public XUiC_ItemStack GetSlotControl(int slotIdx)
	{
		return this.itemControllers[slotIdx];
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "secondrow")
		{
			_value = this.HasSecondRow.ToString();
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentHoldingIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastActionSlot;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastActionRunning;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int backendSlotCount = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openLater;
}
