using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LootContainer : XUiC_ItemStackGrid, ITileEntityChangedListener
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

	public void SetSlots(ITileEntityLootable lootContainer, ItemStack[] stackList)
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
			xuiC_ItemStack.StackLocation = XUiC_ItemStack.StackLocationTypes.LootContainer;
			if (i < num)
			{
				xuiC_ItemStack.ForceSetItemStack(this.localTileEntity.items[i].Clone());
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
			xuiC_ItemStack.ItemStack = slots[i];
			xuiC_ItemStack.SlotChangedEvent += this.HandleLootSlotChangedEvent;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!this.localTileEntity.listeners.Contains(this))
		{
			this.localTileEntity.listeners.Add(this);
		}
		base.xui.lootContainer = this.localTileEntity;
		this.localTileEntity.Destroyed += this.LocalTileEntity_Destroyed;
		QuestEventManager.Current.OpenedContainer(this.localTileEntity.EntityId, this.localTileEntity.ToWorldPos(), this.localTileEntity);
		this.blockValue = GameManager.Instance.World.GetBlock(this.localTileEntity.ToWorldPos());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LocalTileEntity_Destroyed(ITileEntity te)
	{
		if (GameManager.Instance != null)
		{
			if (te == this.localTileEntity)
			{
				base.xui.playerUI.windowManager.Close("looting");
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
			QuestEventManager.Current.ClosedContainer(this.localTileEntity.EntityId, this.localTileEntity.ToWorldPos(), this.localTileEntity);
			this.localTileEntity = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntityLootable localTileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValue;
}
