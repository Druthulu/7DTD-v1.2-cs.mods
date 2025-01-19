using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ContainerStandardControls : XUiController
{
	public bool[] LockedSlots
	{
		get
		{
			if (!this.IsBackpack)
			{
				return null;
			}
			return base.xui.playerUI.entityPlayer.bag.LockedSlots;
		}
		set
		{
			if (this.IsBackpack)
			{
				base.xui.playerUI.entityPlayer.bag.LockedSlots = value;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("btnSort");
		if (childById != null)
		{
			childById.OnPress += delegate(XUiController _, int _)
			{
				this.Sort();
			};
		}
		childById = base.GetChildById("btnMoveAll");
		if (childById != null)
		{
			childById.OnPress += delegate(XUiController _, int _)
			{
				this.MoveAll();
			};
		}
		childById = base.GetChildById("btnMoveFillAndSmart");
		if (childById != null)
		{
			childById.OnPress += delegate(XUiController _, int _)
			{
				this.MoveFillAndSmart();
			};
		}
		childById = base.GetChildById("btnMoveFillStacks");
		if (childById != null)
		{
			childById.OnPress += delegate(XUiController _, int _)
			{
				this.MoveFillStacks();
			};
		}
		childById = base.GetChildById("btnMoveSmart");
		if (childById != null)
		{
			childById.OnPress += delegate(XUiController _, int _)
			{
				this.MoveSmart();
			};
		}
		childById = base.GetChildById("btnToggleLockMode");
		if (childById != null)
		{
			childById.OnPress += delegate(XUiController _, int _)
			{
				this.ToggleLockMode();
			};
			XUiV_Button xuiV_Button = childById.ViewComponent as XUiV_Button;
			if (xuiV_Button != null)
			{
				this.lockModeButtonColorTweener = xuiV_Button.UiTransform.gameObject.GetOrAddComponent<TweenColor>();
				this.lockModeButtonColorTweener.from = xuiV_Button.DefaultSpriteColor;
				this.lockModeButtonColorTweener.to = xuiV_Button.SelectedSpriteColor;
				this.lockModeButtonColorTweener.style = UITweener.Style.PingPong;
				this.lockModeButtonColorTweener.enabled = false;
				this.lockModeButtonColorTweener.duration = 0.4f;
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		Action<bool[]> applyLockedSlotStates = this.ApplyLockedSlotStates;
		if (applyLockedSlotStates == null)
		{
			return;
		}
		applyLockedSlotStates(this.LockedSlots);
	}

	public void Sort()
	{
		Action<XUiC_ContainerStandardControls> updateLockedSlotStates = this.UpdateLockedSlotStates;
		if (updateLockedSlotStates != null)
		{
			updateLockedSlotStates(this);
		}
		Action<bool[]> sortPressed = this.SortPressed;
		if (sortPressed == null)
		{
			return;
		}
		sortPressed(this.LockedSlots);
	}

	public void MoveAll()
	{
		Action<XUiC_ContainerStandardControls> updateLockedSlotStates = this.UpdateLockedSlotStates;
		if (updateLockedSlotStates != null)
		{
			updateLockedSlotStates(this);
		}
		XUiController srcWindow;
		XUiC_ItemStackGrid srcGrid;
		IInventory dstInventory;
		if (this.MoveAllowed(out srcWindow, out srcGrid, out dstInventory))
		{
			ValueTuple<bool, bool> valueTuple = XUiM_LootContainer.StashItems(srcWindow, srcGrid, dstInventory, 0, this.LockedSlots, XUiM_LootContainer.EItemMoveKind.All, this.MoveStartBottomRight);
			bool item = valueTuple.Item1;
			bool item2 = valueTuple.Item2;
			Action<bool, bool> moveAllDone = this.MoveAllDone;
			if (moveAllDone == null)
			{
				return;
			}
			moveAllDone(item, item2);
		}
	}

	public void MoveFillAndSmart()
	{
		Action<XUiC_ContainerStandardControls> updateLockedSlotStates = this.UpdateLockedSlotStates;
		if (updateLockedSlotStates != null)
		{
			updateLockedSlotStates(this);
		}
		XUiController srcWindow;
		XUiC_ItemStackGrid srcGrid;
		IInventory dstInventory;
		if (this.MoveAllowed(out srcWindow, out srcGrid, out dstInventory))
		{
			XUiM_LootContainer.StashItems(srcWindow, srcGrid, dstInventory, 0, this.LockedSlots, XUiM_LootContainer.EItemMoveKind.FillOnlyFirstCreateSecond, this.MoveStartBottomRight);
		}
	}

	public void MoveFillStacks()
	{
		Action<XUiC_ContainerStandardControls> updateLockedSlotStates = this.UpdateLockedSlotStates;
		if (updateLockedSlotStates != null)
		{
			updateLockedSlotStates(this);
		}
		XUiController srcWindow;
		XUiC_ItemStackGrid srcGrid;
		IInventory dstInventory;
		if (this.MoveAllowed(out srcWindow, out srcGrid, out dstInventory))
		{
			XUiM_LootContainer.StashItems(srcWindow, srcGrid, dstInventory, 0, this.LockedSlots, XUiM_LootContainer.EItemMoveKind.FillOnly, this.MoveStartBottomRight);
		}
	}

	public void MoveSmart()
	{
		Action<XUiC_ContainerStandardControls> updateLockedSlotStates = this.UpdateLockedSlotStates;
		if (updateLockedSlotStates != null)
		{
			updateLockedSlotStates(this);
		}
		XUiController srcWindow;
		XUiC_ItemStackGrid srcGrid;
		IInventory dstInventory;
		if (this.MoveAllowed(out srcWindow, out srcGrid, out dstInventory))
		{
			XUiM_LootContainer.StashItems(srcWindow, srcGrid, dstInventory, 0, this.LockedSlots, XUiM_LootContainer.EItemMoveKind.FillAndCreate, this.MoveStartBottomRight);
		}
	}

	public void ToggleLockMode()
	{
		Action<XUiC_ContainerStandardControls> updateLockedSlotStates = this.UpdateLockedSlotStates;
		if (updateLockedSlotStates != null)
		{
			updateLockedSlotStates(this);
		}
		Action lockModeToggled = this.LockModeToggled;
		if (lockModeToggled == null)
		{
			return;
		}
		lockModeToggled();
	}

	public void LockModeChanged(bool _state)
	{
		if (this.lockModeButtonColorTweener != null)
		{
			this.lockModeButtonColorTweener.enabled = _state;
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "move_start_bottom_left")
		{
			this.MoveStartBottomRight = StringParsers.ParseBool(_value, 0, -1, true);
			return true;
		}
		if (!(_name == "is_backpack"))
		{
			return base.ParseAttribute(_name, _value, _parent);
		}
		this.IsBackpack = StringParsers.ParseBool(_value, 0, -1, true);
		return true;
	}

	public Action<bool[]> ApplyLockedSlotStates;

	public Action<XUiC_ContainerStandardControls> UpdateLockedSlotStates;

	public Action<bool[]> SortPressed;

	public Action LockModeToggled;

	public XUiC_ContainerStandardControls.MoveAllowedDelegate MoveAllowed;

	public Action<bool, bool> MoveAllDone;

	public bool MoveStartBottomRight;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsBackpack;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool LockModeEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public TweenColor lockModeButtonColorTweener;

	public delegate bool MoveAllowedDelegate(out XUiController _parentWindow, out XUiC_ItemStackGrid _sourceGrid, out IInventory _destinationInventory);
}
