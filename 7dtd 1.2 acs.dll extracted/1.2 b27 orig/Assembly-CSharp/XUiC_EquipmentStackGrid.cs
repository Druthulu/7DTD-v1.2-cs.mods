using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EquipmentStackGrid : XUiController
{
	public virtual XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Equipment;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = base.GetChildrenByType<XUiC_EquipmentStack>(null);
		this.itemControllers = childrenByType;
		this.bAwakeCalled = true;
		this.IsDirty = false;
		XUiM_PlayerEquipment.HandleRefreshEquipment += this.XUiM_PlayerEquipment_HandleRefreshEquipment;
		base.xui.OnShutdown += this.HandleShutdown;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment(XUiM_PlayerEquipment _playerEquipment)
	{
		if (base.xui.PlayerEquipment == _playerEquipment)
		{
			this.IsDirty = true;
		}
	}

	public void HandleShutdown()
	{
		XUiM_PlayerEquipment.HandleRefreshEquipment -= this.XUiM_PlayerEquipment_HandleRefreshEquipment;
		base.xui.OnShutdown -= this.HandleShutdown;
	}

	public void SetEquipmentSlotForStack(EquipmentSlots equipSlot)
	{
		XUiC_EquipmentStack xuiC_EquipmentStack = (XUiC_EquipmentStack)this.itemControllers[(int)equipSlot];
		if (xuiC_EquipmentStack != null)
		{
			xuiC_EquipmentStack.EquipSlot = equipSlot;
			this.equipmentList.Add(xuiC_EquipmentStack);
		}
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		if (this.IsDirty)
		{
			if (!this.slotsSetup)
			{
				this.slotIndexList.Clear();
				this.equipmentList.Clear();
				int num = 0;
				while (num < 5 && num < this.itemControllers.Length)
				{
					XUiC_EquipmentStack xuiC_EquipmentStack = this.itemControllers[num] as XUiC_EquipmentStack;
					if (xuiC_EquipmentStack != null)
					{
						xuiC_EquipmentStack.EquipSlot = (EquipmentSlots)num;
						this.equipmentList.Add(xuiC_EquipmentStack);
					}
					num++;
				}
				if (this.ExtraSlot != null)
				{
					this.equipmentList.Add(this.ExtraSlot);
				}
				this.slotsSetup = true;
			}
			this.items = this.GetSlots();
			this.SetStacks(this.items);
			this.IsDirty = false;
		}
		base.Update(_dt);
	}

	public virtual ItemValue[] GetSlots()
	{
		Equipment equipment = base.xui.PlayerEquipment.Equipment;
		ItemValue[] array = new ItemValue[this.equipmentList.Count];
		for (int i = 0; i < this.equipmentList.Count; i++)
		{
			ItemValue itemValue = equipment.GetSlotItem(i);
			if (itemValue == null)
			{
				itemValue = ItemValue.None.Clone();
			}
			array[i] = itemValue;
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleBagContentsChanged(ItemValue[] _items)
	{
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		if (base.xui.playerUI.entityPlayer != null)
		{
			this.SetStacks(_items);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetStacks(ItemValue[] stackList)
	{
		if (stackList == null)
		{
			return;
		}
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		XUiC_CharacterFrameWindow childByType2 = base.xui.GetChildByType<XUiC_CharacterFrameWindow>();
		int num = 0;
		while (num < stackList.Length && this.equipmentList.Count > num && stackList.Length > num)
		{
			XUiC_EquipmentStack xuiC_EquipmentStack = this.equipmentList[num];
			xuiC_EquipmentStack.SlotChangedEvent -= this.HandleSlotChangedEvent;
			xuiC_EquipmentStack.ItemValue = stackList[num];
			xuiC_EquipmentStack.SlotChangedEvent += this.HandleSlotChangedEvent;
			xuiC_EquipmentStack.SlotNumber = num;
			xuiC_EquipmentStack.InfoWindow = childByType;
			xuiC_EquipmentStack.FrameWindow = childByType2;
			num++;
		}
	}

	public void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		if (stack.IsEmpty())
		{
			base.xui.PlayerEquipment.Equipment.SetSlotItem(slotNumber, null, true);
			base.xui.PlayerEquipment.RefreshEquipment();
			return;
		}
		this.items[slotNumber] = stack.itemValue.Clone();
		base.xui.PlayerEquipment.Equipment.SetSlotItem(slotNumber, stack.itemValue, true);
		base.xui.PlayerEquipment.RefreshEquipment();
		QuestEventManager.Current.WoreItem(stack.itemValue);
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
		this.IsDirty = true;
		this.IsDormant = false;
	}

	public override void OnClose()
	{
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			this.itemControllers[i].Hovered(false);
		}
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
		this.IsDormant = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController[] itemControllers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemValue[] items;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> slotIndexList = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_EquipmentStack> equipmentList = new List<XUiC_EquipmentStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool slotsSetup;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAwakeCalled;

	public XUiC_EquipmentStack ExtraSlot;

	public enum UIEquipmentSlots
	{
		Headgear,
		Eyewear,
		Face,
		Shirt,
		Jacket,
		ChestArmor,
		Gloves,
		Backpack,
		Pants,
		Footwear,
		LegArmor
	}
}
