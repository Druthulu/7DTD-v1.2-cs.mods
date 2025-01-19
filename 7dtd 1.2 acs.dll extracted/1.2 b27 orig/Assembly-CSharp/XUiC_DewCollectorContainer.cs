using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DewCollectorContainer : XUiC_ItemStackGrid, ITileEntityChangedListener
{
	public Vector2i GridCellSize { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public override ItemStack[] GetSlots()
	{
		return this.localTileEntity.items;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetStacks(ItemStack[] stackList)
	{
	}

	public void SetSlots(TileEntityDewCollector lootContainer, ItemStack[] stackList)
	{
		if (stackList == null)
		{
			return;
		}
		this.localTileEntity = lootContainer;
		this.items = this.localTileEntity.items;
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		XUiV_Grid xuiV_Grid = (XUiV_Grid)this.viewComponent;
		xuiV_Grid.Columns = lootContainer.GetContainerSize().x;
		xuiV_Grid.Rows = lootContainer.GetContainerSize().y;
		int num = stackList.Length;
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_ItemStack xuiC_ItemStack = this.itemControllers[i];
			xuiC_ItemStack.InfoWindow = childByType;
			xuiC_ItemStack.SlotNumber = i;
			xuiC_ItemStack.SlotChangedEvent -= this.HandleLootSlotChangedEvent;
			xuiC_ItemStack.InfoWindow = childByType;
			xuiC_ItemStack.OverrideStackCount = 1;
			xuiC_ItemStack.StackLocation = XUiC_ItemStack.StackLocationTypes.LootContainer;
			if (i < num)
			{
				this.SetItemInSlot(i, this.localTileEntity.items[i], false);
				this.itemControllers[i].ViewComponent.IsVisible = true;
				xuiC_ItemStack.SlotChangedEvent += this.HandleLootSlotChangedEvent;
			}
			else
			{
				xuiC_ItemStack.ItemStack = ItemStack.Empty.Clone();
				this.itemControllers[i].ViewComponent.IsVisible = false;
			}
		}
		this.windowGroup.Controller.SetAllChildrenDirty(false);
	}

	public override void Init()
	{
		base.Init();
		XUiV_Grid xuiV_Grid = (XUiV_Grid)this.viewComponent;
		this.GridCellSize = new Vector2i(xuiV_Grid.CellWidth, xuiV_Grid.CellHeight);
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_DewCollectorStack xuiC_DewCollectorStack = (XUiC_DewCollectorStack)this.itemControllers[i];
			xuiC_DewCollectorStack.RequiredItemClass = ItemClass.GetItemClass(this.requiredItem, false);
			xuiC_DewCollectorStack.RequiredItemOnly = true;
			xuiC_DewCollectorStack.TakeOnly = true;
		}
	}

	public void HandleLootSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		this.localTileEntity.UpdateSlot(slotNumber, stack);
		this.localTileEntity.SetModified();
	}

	public void OnTileEntityChanged(ITileEntity _te)
	{
		ItemStack[] slots = this.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			XUiC_ItemStack xuiC_ItemStack = this.itemControllers[i];
			xuiC_ItemStack.SlotChangedEvent -= this.HandleLootSlotChangedEvent;
			this.SetItemInSlot(i, slots[i], true);
			xuiC_ItemStack.SlotChangedEvent += this.HandleLootSlotChangedEvent;
		}
	}

	public void SetItemInSlot(int i, ItemStack stack, bool onTEChanged)
	{
		if (onTEChanged && this.itemControllers[i].ItemStack.IsEmpty() && !stack.IsEmpty())
		{
			string convertSound = ((BlockDewCollector)this.localTileEntity.blockValue.Block).ConvertSound;
			Manager.BroadcastPlayByLocalPlayer(this.localTileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f, convertSound);
		}
		this.itemControllers[i].ItemStack = stack.Clone();
		XUiC_DewCollectorStack xuiC_DewCollectorStack = (XUiC_DewCollectorStack)this.itemControllers[i];
		xuiC_DewCollectorStack.FillAmount = this.localTileEntity.fillValues[i];
		xuiC_DewCollectorStack.MaxFill = this.localTileEntity.CurrentConvertTime;
		xuiC_DewCollectorStack.IsCurrentStack = (this.localTileEntity.CurrentIndex == i);
		xuiC_DewCollectorStack.IsBlocked = this.localTileEntity.IsBlocked;
		xuiC_DewCollectorStack.IsModded = this.localTileEntity.IsModdedConvertItem;
		xuiC_DewCollectorStack.RefreshBindings(false);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!this.localTileEntity.listeners.Contains(this))
		{
			this.localTileEntity.listeners.Add(this);
		}
		this.localTileEntity.Destroyed += this.LocalTileEntity_Destroyed;
		this.blockValue = GameManager.Instance.World.GetBlock(this.localTileEntity.ToWorldPos());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LocalTileEntity_Destroyed(ITileEntity te)
	{
		if (GameManager.Instance != null)
		{
			if (te == this.localTileEntity)
			{
				XUiC_DewCollectorWindowGroup.CloseIfOpenAtPos(te.ToWorldPos(), null);
				return;
			}
			te.Destroyed -= this.LocalTileEntity_Destroyed;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.lootContainer = null;
		if (this.localTileEntity != null)
		{
			this.localTileEntity.Destroyed -= this.LocalTileEntity_Destroyed;
			if (this.localTileEntity.listeners.Contains(this))
			{
				this.localTileEntity.listeners.Remove(this);
			}
			this.localTileEntity = null;
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "required_item")
		{
			this.requiredItem = _value;
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityDewCollector localTileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public string requiredItem = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValue;
}
